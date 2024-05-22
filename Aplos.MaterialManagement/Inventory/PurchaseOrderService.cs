using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Inventory;
using Library.Model.Taxations;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using Library.ViewModel.Inventory;
using Library.ViewModel.Materials;
using Library.ViewModel.Setup;
using OTSBD;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace Library.MaterialManagement.Inventory
{
    public class PurchaseOrderService : Service<PurchaseOrder>, IPurchaseOrderService

    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<NotificationSetting> _notificationSetting;
        private readonly IRepositoryAsync<PurchaseOrder> _inventoryReceiveRepository;
        private readonly IRepositoryAsync<ServicePOMaster> _ServicePOMaster;
        private readonly IRepositoryAsync<ServicePODetail> _ServicePODetail;
        private readonly IRepositoryAsync<ServicePOTax> _ServicePOTax;
        private readonly IRepositoryAsync<ServiceAcknowledgementMaster> _ServiceAcknowledgementMaster;
        private readonly IRepositoryAsync<ServiceAcknowledgementDetail> _ServiceAcknowledgementDetail;

        private readonly IRepositoryAsync<ServiceAcknowledgementDetail> _ServiceAcknowledgementDetailRepository;
        private readonly IRepositoryAsync<ServivePOAcknowledgementMap> _ServivePOAcknowledgementMapRepository;
        private readonly IRepositoryAsync<ServiceAcknowledgementMaster> _ServiceAcknowledgementMasterRepository;
        private readonly IRepositoryAsync<PODocumentMap> _PODocumentMapRepository;
        private readonly IRepositoryAsync<ServicePOAckTax> _servicePOAckTaxRepository;
        private readonly IRepositoryAsync<ServiceAcknowledgementCharge> _ChargeServiceRepository;



        private readonly IUnitOfWork _unitOfWork;
        private double total;

        public PurchaseOrderService(
            IRepositoryAsync<PurchaseOrder> inventoryReceiveRepository
              , IRepositoryAsync<NotificationSetting> notificationSetting
             , IRepositoryAsync<ServicePOMaster> ServicePOMaster
             , IRepositoryAsync<ServicePODetail> ServicePODetail
             , IRepositoryAsync<ServicePOTax> ServicePOTax
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IRepositoryAsync<ServiceAcknowledgementMaster> ServiceAcknowledgementMaster
            , IRepositoryAsync<ServiceAcknowledgementDetail> ServiceAcknowledgementDetail
            , IRepositoryAsync<ServiceAcknowledgementDetail> ServiceAcknowledgementDetailRepository
            , IRepositoryAsync<ServivePOAcknowledgementMap> ServivePOAcknowledgementMapRepository
            , IRepositoryAsync<ServiceAcknowledgementMaster> ServiceAcknowledgementMasterRepository
            , IRepositoryAsync<PODocumentMap> PODocumentMapRepository
            , IRepositoryAsync<ServicePOAckTax> servicePOAckTaxRepository
            , IRepositoryAsync<ServiceAcknowledgementCharge> ChargeServiceRepository
            ) : base(inventoryReceiveRepository, unitOfWork, pkGeneratorService)
        {
            _inventoryReceiveRepository = inventoryReceiveRepository;
            _notificationSetting = notificationSetting;
            _sqlRepository = sqlRepository;
            _ServicePOMaster = ServicePOMaster;
            _ServicePODetail = ServicePODetail;
            _ChargeServiceRepository = ChargeServiceRepository;
            _ServicePOTax = ServicePOTax;
            _unitOfWork = unitOfWork;
            _ServiceAcknowledgementMaster = ServiceAcknowledgementMaster;
            _ServiceAcknowledgementDetail = ServiceAcknowledgementDetail;
            _ServiceAcknowledgementDetailRepository = ServiceAcknowledgementDetailRepository;
            _ServivePOAcknowledgementMapRepository = ServivePOAcknowledgementMapRepository;
            _ServiceAcknowledgementMasterRepository = ServiceAcknowledgementMasterRepository;
            _PODocumentMapRepository = PODocumentMapRepository;
            _servicePOAckTaxRepository = servicePOAckTaxRepository;
        }

        #endregion Constructor
        bplib.clsGenID objGenID = new bplib.clsGenID();
        #region InventoryReceive
        private void PurchaseOrderCheck(PurchaseOrder entity)
        {
            CheckUniqueColumn(UniqueColumnName.DocRefNo, entity.DocRefNo, r => r.Id != entity.Id && r.PartyId == entity.PartyId && r.DocRefNo == entity.DocRefNo);
        }
        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(PurchaseOrder), out sID);
            return sID;
        }
        private string GetPKServiveAck()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(ServiceAcknowledgementMaster), out sID);
            return sID;
        }
        private string GetPKSerAckMap()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(ServivePOAcknowledgementMap), out sID);
            return sID;
        }
        private string GetPK3()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(ServicePOAckTax), out sID);
            return sID;
        }

        private string PODocumentMap()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(PODocumentMap), out sID);
            return sID;
        }
        private string ServicePODocumentMap()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(ServicePODocumentMap), out sID);
            return sID;
        }
        private string ServicePOAckDocumentMap()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(ServicePOAckDocumentMap), out sID);
            return sID;
        }
        public void SaveTermsData(string TitleId, string POId)
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

                TnCDeleteDetail(POId);

                //    }
                //}
                con.OpenDataSetThroughAdapter("SELECT * FROM TermsAndConditionsPOChild WHERE 1=2", out dsToSalesOrder, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM TermsAndConditionsPODetails WHERE 1=2", out dsToFirstCharacteristics, false, "1");

                DataTable dtFromMaster = _sqlRepository.GetDataTable("SELECT * FROM  TermsAndConditionsChild WHERE TermsAndConditionsMasterId='" + TitleId + "'");
                DataTable dtFromFirstCharacteristics = _sqlRepository.GetDataTable("SELECT * FROM TermsAndConditionsDetails Where TermsAndConditionsChildId IN(Select Id from TermsAndConditionsChild Where TermsAndConditionsMasterId='" + TitleId + "')");

                int SCount = 0;
                objGenID.GenerateIDAuto("dbo.TermsAndConditionsPOChild", out Id);

                for (int m = 0; m < dtFromMaster.Rows.Count; m++)
                {
                    SCount++;
                    DataRow drSalesOrder = dsToSalesOrder.Tables[0].NewRow();
                    CopyRow(dtFromMaster.Rows[m], ref drSalesOrder);
                    drSalesOrder["Id"] = TitleId + Convert.ToInt32(Id) + SCount;
                    NewSoId = drSalesOrder["Id"].ToString();
                    // drSalesOrder["TermsAndConditionsMasterId"] = TitleId;
                    drSalesOrder["POId"] = POId;
                    dsToSalesOrder.Tables[0].Rows.Add(drSalesOrder);

                    dtFromFirstCharacteristics.DefaultView.RowFilter = "TermsAndConditionsChildId='" + dtFromMaster.Rows[m]["Id"].ToString() + "'";
                    for (int i = 0; i < dtFromFirstCharacteristics.DefaultView.Count; i++)
                    {
                        DataRow drFirstCharacteristics = dsToFirstCharacteristics.Tables[0].NewRow();
                        CopyRow(dtFromFirstCharacteristics.DefaultView[i].Row, ref drFirstCharacteristics);
                        drFirstCharacteristics["Id"] = NewSoId + (i + 1);
                        drFirstCharacteristics["TermsAndConditionsPOChildId"] = NewSoId;

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
        public override void Insert(PurchaseOrder entity)
        {
            try
            {
                ResetCurrencyRate(entity);
                entity.Id = GetPK();
                base.Insert(entity);
                SaveTermsData(entity.TermsAndConditionsId, entity.Id);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public void InsertPOBOQMaster(PurchaseOrder entity)
        {
            try
            {
                PurchaseOrderCheck(entity);
                ResetCurrencyRate(entity);
                entity.Id = GetPK();
                base.Insert(entity);
                //TODO:


            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public void TnCDeleteDetail(string POId)
        {
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            string strSQLDetail = "DELETE FROM TermsAndConditionsPODetails Where TermsAndConditionsPOChildId IN(SELECT ID FROM TermsAndConditionsPOChild WHERE POId='" + POId + "')";
            string strSQLChild = "DELETE FROM TermsAndConditionsPOChild WHERE POId='" + POId + "'";
            con = new ConnectionManager.DAL.ConManager("1");
            con.OpenConnection("1");
            con.BeginTransaction();
            con.ExecuteNonQueryWrapper(strSQLDetail, true, "1");
            con.ExecuteNonQueryWrapper(strSQLChild, true, "1");
            con.CommitTransaction();
        }
        public override void Update(PurchaseOrder entity)
        {
            try
            {

                ResetCurrencyRate(entity);
                base.Update(entity);
                SaveTermsData(entity.TermsAndConditionsId, entity.Id);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        private static void ResetCurrencyRate(PurchaseOrder entity)
        {
            if (string.IsNullOrEmpty(entity.ToCurrencyRate.ToString()))
            {
                if (entity.BaseCurrencyId != entity.CurrencyId)
                    throw new CustomException("Please input currency rate.");
                else
                    entity.ToCurrencyRate = 1;
            }
            else if (entity.ToCurrencyRate == 0)
            {
                if (entity.BaseCurrencyId != entity.CurrencyId)
                    throw new CustomException("Please input currency rate.");
            }
            else
            {
                if (entity.BaseCurrencyId == entity.CurrencyId)
                    entity.ToCurrencyRate = 1;
            }
        }

        public GridModel Query(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
                        FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
		                            JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting'  AND IR.EmployeeId IS NULL";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> QueryForCharges(string MasterId)
        {
            try
            {
                //var sql = @"SELECT A.Id, A.InventoryReceiveId, A.ServiceMasterId, B.UserName AS ServiceMasterName, A.Amount, A.TotalTaxAmount
                //            FROM [TRN].[InventoryService] AS A JOIN [HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id WHERE A.InventoryReceiveId='" + receiveId + "'";
                var sql = @"SELECT A.Id
                        , A.ServiceAcknowledgementMasterId
                        , A.ServiceMasterId
                        , B.UserName AS ServiceMasterName
                         ,A.Amount
						,IRT.TaxAmount TotalTaxAmount
                        FROM [TRN].ServiceAcknowledgementCharge AS A 
                        JOIN [HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id 
                        left join ( Select ServiceAcknowledgementChargeId, sum(TaxAmount) TaxAmount from  trn.ServicePOAckTax group by ServiceAcknowledgementChargeId) IRT On IRT.ServiceAcknowledgementChargeId=A.Id
                        WHERE A.ServiceAcknowledgementMasterId='" + MasterId + @"'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public GridModel GetPostingList(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText = @"DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                        SELECT IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
			                        , IR.EmployeeId, EI.EmployeeCode, EI.EmployeeName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, IR.IsTaxApplicable
                                    , COUNT(*) OVER () AS TotalRows
                        FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId=@plantId GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId=@plantId GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        WHERE IR.PlantId=@plantId AND IR.[Status]='Posting' AND IR.IsPaymentHold=0 AND CP.PlantId=@plantId AND IR.FixedAssetOrInventory='Inventory'";
                return _sqlRepository.GetDifferentGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetListForHold(string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
                                    --,IR.PODate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,pgl.CtnId
                                    ,IR.AddedBy
                                    ,IR.CheckedByStatus AS CheckedByStatus
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus
                        FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.EmployeeId=IR.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.EmployeeId=IR.AuthorizedBy
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
		                            JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                        WHERE IR.POType='PO' AND IR.PlantId='" + plantId + "'  AND ISNULL(IR.[Status],'')<>'Posting'  AND IR.EmployeeId IS NULL AND IR.IsApproved=0 AND isnull(IR.IsClosed,0)=0 Order by IR.PODate DESC, IR.ID DESC";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }

        }
        public IEnumerable<object> POCheckedRollBack(string plantId, string POTypeStatus)
        {
            if (POTypeStatus == "")
            {
                POTypeStatus = "Pending";
            }
            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var Sql = "";
                if (POTypeStatus == "Checked")
                {
                    Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate1
                                    --,IR.PODate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,isnull(pgl.CtnId,0) CtnId
                                    ,isnull(IR.AddedBy,'') AddedBy
                                   ,isnull(PLC.LCRef,'') PurchaseLC
									,isnull(Cn.ContractNo,'') ContructNumber
									--,isnull(Par1.UserName,'') Customer
									,isnull(IR.CheckedByStatus,'') AS CheckedByStatus
									,isnull(IR.AuthorizedByStatus,'') AS AuthorizedByStatus
                                    ,isnull(eI.EmployeeName,'') CheckedBy
									,isnull(eI1.EmployeeName,'') ApprovedBy
									,isnull(IR.ContractId,'') ContractId
									,isnull(IR.OrderSpecific,'') OrderSpecific,isnull(foruser.EmployeeName,'') PreparedBy
									,isnull(IR.PurchaseLCId,'') PurchaseLCId,isnull(Par.UserName,'') CustomerName,PT.PaymentMode,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                                    ,isnull(GRNId,'')GRNId
                        FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                          LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy

                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        left join SEC.[User] ei2 on ei2.UserId = IR.AddedBy
						LEFT JOIN dbo.EmployeeInformation foruser ON foruser.SystemId=ei2.EmployeeId
                        LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                        LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
		                            JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                        LEFT JOIN(
									SELECT distinct PDAMAP.PoId
									,GRNId=STUFF((select distinct ','+xPDAMAP.GRNId from
									trn.PurchaseOrder xpo
									INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.PoId
									where xPDAMAP.PoId=PDAMAP.PoId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									from trn.POGGRNMap PDAMAP
									LEFT JOIN [TRN].PurchaseOrder IR ON IR.Id = PDAMAP.PoId
									group by PDAMAP.PoId
							)PDA ON PDA.PoId=IR.Id
                        WHERE IR.PlantId='" + identity.PlantId + @"' 
                         AND IR.CheckedBy IS NOT NULL 
                         AND IR.AuthorizedBy IS NOt NULL  
                         AND IR.CheckedByStatus='Checked' 
                         AND IR.AuthorizedByStatus='For Approval'  
                         Order by IR.PODate DESC";//IR.AddedBy='" + identity.Name + "' " +


                }
                return _sqlRepository.GetDataCollection(Sql);
            }

            catch (Exception ex)
            {
                throw ex;

            }
        }
        public IEnumerable<object> GetPOTypeList(string plantId, string POTypeStatus, string poType)
        {
            if (POTypeStatus == "")
            {
                POTypeStatus = "Pending";
            }
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var Sql = "";
                if (POTypeStatus == "Pending")
                {
                    Sql = @"
						select * from(
							SELECT  ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
									, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate1
                                    , REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
									--,IR.PODate
									, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
									, CP.UserName AS PartyAccountGroupName
									, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
									--, IR.GateEntryNo
									--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
									, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
									, IR.FixedAssetOrInventory, IR.PODepended
									--, IR.AlongwithInvoice
									--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
									, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount
									,ROUND(IRD.BaseAmount, 2)+ROUND(PS.Amount,2) BaseAmount, IR.ToCurrencyRate
									, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
									,isnull(pgl.CtnId,0) CtnId
									,isnull(IR.AddedBy,'') AddedBy
                                   ,isnull(PLC.LCRef,'') PurchaseLC
									,isnull(Cn.ContractNo,'') ContructNumber
									,isnull(Par1.UserName,'') Customer
									,isnull(IR.CheckedByStatus,'') AS CheckedByStatus
									,isnull(IR.AuthorizedByStatus,'') AS AuthorizedByStatus
                                    ,isnull(eI.EmployeeName,'') CheckedBy
									,isnull(eI1.EmployeeName,'') ApprovedBy
									,isnull(IR.ContractId,'') ContractId
									,isnull(IR.OrderSpecific,'') OrderSpecific
									,isnull(IR.PurchaseLCId,'') PurchaseLCId
									,isnull(Par.UserName,'') CustomerName 
                                    ,PT.PaymentMode,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END,IR.AddedDate,IR.Tolerance,IR.TermsAndConditionsId
                                    ,IR.IsTradingPO
						FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
						LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
									ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                         LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						LEFT JOIN [HKP].[Party] Par1 ON Par1.Id= Ctc.CustomerId
						LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy

						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
						LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
						LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
						LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
						LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
						LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
						LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
						LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
						LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
						LEFT JOIN [HKP].[Party] AS Par ON Cn.CustomerId=Par.Id 
						LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
						LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
									JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
									WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
						LEFT JOIN (SELECT PS.InventoryReceiveId,SUM(ISNULL(PS.Amount,0)) Amount FROM TRN.POService PS GROUP BY PS.InventoryReceiveId) PS ON PS.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
						WHERE  IR.PlantId='" + identity.PlantId + @"' AND IR.POType='" + poType + @"'  --IR.AddedBy='" + identity.Name + @"' And
                        AND IR.CheckedBy IS NOT NULL 
						AND IR.CheckedByStatus='Pending' 
						AND isnull(IR.IsClosed,0)=0 
						--Order by IR.PODate DESC

						UNION All

						--DECLARE @plantId VARCHAR(10)='20171';
							SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
									, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate1
									--,IR.PODate
									, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
									, CP.UserName AS PartyAccountGroupName
									, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
									--, IR.GateEntryNo
									--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
									, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
									, IR.FixedAssetOrInventory, IR.PODepended
									--, IR.AlongwithInvoice
									--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
									, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount
									,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
									, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
									,isnull(pgl.CtnId,0) CtnId
									,isnull(IR.AddedBy,'') AddedBy
                                   ,isnull(PLC.LCRef,'') PurchaseLC
									,isnull(Cn.ContractNo,'') ContructNumber
									,isnull(Par1.UserName,'') Customer
									,isnull(IR.CheckedByStatus,'') AS CheckedByStatus
									,isnull(IR.AuthorizedByStatus,'') AS AuthorizedByStatus
                                    ,isnull(eI.EmployeeName,'') CheckedBy
									,isnull(eI1.EmployeeName,'') ApprovedBy
									,isnull(IR.ContractId,'') ContractId
									,isnull(IR.OrderSpecific,'') OrderSpecific
									,isnull(IR.PurchaseLCId,'') PurchaseLCId
									,isnull(Par.UserName,'') CustomerName ,PT.PaymentMode,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END,IR.AddedDate,IR.Tolerance,IR.TermsAndConditionsId
                                    ,IR.IsTradingPO
						FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
						LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
									ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                        LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						LEFT JOIN [HKP].[Party] Par1 ON Par1.Id= Ctc.CustomerId
						LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy

						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
						LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
						LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
						LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
						LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
						LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
						LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
						LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
						LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
						LEFT JOIN [HKP].[Party] AS Par ON Cn.CustomerId=Par.Id 
						LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
						LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
									JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
									WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
						Where IR.Id not in(Select distinct POId from trn.InventoryReceiveDetail where POId is not null)--and RequisitionId='110232'
						AND IR.CheckedByStatus IS NULL 
						AND IR.AuthorizedByStatus IS NULL						
						 And IR.PlantId='" + identity.PlantId + @"' AND IR.POType='" + poType + @"'--AND IR.AddedBy='" + identity.Name + @"'

                        AND isnull(IR.IsClosed,0)=0 
						--Order by IR.PODate DESC

						UNION All

						--DECLARE @plantId VARCHAR(10)='20171';
							SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
									, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate1
									--,IR.PODate
									, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
									, CP.UserName AS PartyAccountGroupName
									, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
									--, IR.GateEntryNo
									--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
									, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
									, IR.FixedAssetOrInventory, IR.PODepended
									--, IR.AlongwithInvoice
									--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
									, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount
									,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
									, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
									,isnull(pgl.CtnId,0) CtnId
									,isnull(IR.AddedBy,'') AddedBy
                                   ,isnull(PLC.LCRef,'') PurchaseLC
									,isnull(Cn.ContractNo,'') ContructNumber
									,isnull(Par1.UserName,'') Customer
									,isnull(IR.CheckedByStatus,'') AS CheckedByStatus
									,isnull(IR.AuthorizedByStatus,'') AS AuthorizedByStatus
                                    ,isnull(eI.EmployeeName,'') CheckedBy
									,isnull(eI1.EmployeeName,'') ApprovedBy
									,isnull(IR.ContractId,'') ContractId
									,isnull(IR.OrderSpecific,'') OrderSpecific
									,isnull(IR.PurchaseLCId,'') PurchaseLCId
									,isnull(Par.UserName,'') CustomerName ,PT.PaymentMode,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END,IR.AddedDate,IR.Tolerance,IR.TermsAndConditionsId
                                    ,IR.IsTradingPO
						FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
						LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
									ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                         LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						LEFT JOIN [HKP].[Party] Par1 ON Par1.Id= Ctc.CustomerId
						LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
						LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
						LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
						LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
						LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
						LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
						LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
						LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
						LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
						LEFT JOIN [HKP].[Party] AS Par ON Cn.CustomerId=Par.Id 
						LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
						LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
									JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
									WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
						Where IR.CheckedByStatus is null				
						AND IR.AuthorizedByStatus='For Approval'						
						And IR.PlantId='" + identity.PlantId + @"' AND IR.POType='" + poType + @"'	--AND IR.AddedBy='" + identity.Name + @"'	
                        AND isnull(IR.IsClosed,0)=0 
						) x
						Order by x.AddedDate DESC";
                }
                else if (POTypeStatus == "CheckedHoldRej")
                {
                    Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate1
                                    --,IR.PODate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,isnull(pgl.CtnId,0) CtnId
                                  ,isnull(IR.AddedBy,'') AddedBy
                                   ,isnull(PLC.LCRef,'') PurchaseLC
									,isnull(Cn.ContractNo,'') ContructNumber
									,isnull(Par1.UserName,'') Customer
									,isnull(IR.CheckedByStatus,'') AS CheckedByStatus
									,isnull(IR.AuthorizedByStatus,'') AS AuthorizedByStatus
                                    ,isnull(eI.EmployeeName,'') CheckedBy
									,isnull(eI1.EmployeeName,'') ApprovedBy
									,isnull(IR.ContractId,'') ContractId
									,isnull(IR.OrderSpecific,'') OrderSpecific
									,isnull(IR.PurchaseLCId,'') PurchaseLCId,isnull(Par.UserName,'') CustomerName,PT.PaymentMode,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                                    ,IR.TermsAndConditionsId,IR.IsTradingPO
                        FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                          LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy

                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[Party] Par1 ON Par1.Id= Ctc.CustomerId
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                        LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
		                            JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                        WHERE  IR.PlantId='" + identity.PlantId + @"' AND IR.CheckedBy IS NOT NULL AND IR.AuthorizedBy IS NOT NULL AND IR.CheckedByStatus='Hold' OR IR.CheckedByStatus='Reject' AND IR.POType='" + poType + @"' AND IR.PlantId='" + plantId + "'   AND isnull(IR.IsClosed,0)=0 Order by IR.PODate DESC";//IR.AddedBy='" + identity.Name + "' And

                }
                else if (POTypeStatus == "Checked")
                {
                    Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate1
                                    --,IR.PODate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,isnull(pgl.CtnId,0) CtnId
                                    ,isnull(IR.AddedBy,'') AddedBy
                                   ,isnull(PLC.LCRef,'') PurchaseLC
									,isnull(Cn.ContractNo,'') ContructNumber
									,isnull(Par1.UserName,'') Customer
									,isnull(IR.CheckedByStatus,'') AS CheckedByStatus
									,isnull(IR.AuthorizedByStatus,'') AS AuthorizedByStatus
                                    ,isnull(eI.EmployeeName,'') CheckedBy
									,isnull(eI1.EmployeeName,'') ApprovedBy
									,isnull(IR.ContractId,'') ContractId
									,isnull(IR.OrderSpecific,'') OrderSpecific
									,isnull(IR.PurchaseLCId,'') PurchaseLCId,isnull(Par.UserName,'') CustomerName,PT.PaymentMode,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                                    ,IR.TermsAndConditionsId,IR.IsTradingPO
                        FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                          LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						LEFT JOIN [HKP].[Party] Par1 ON Par1.Id= Ctc.CustomerId
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy

                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                        LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
                        LEFT JOIN [HKP].[Party] AS Par ON Cn.CustomerId=Par.Id 
                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
		                            JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                        WHERE IR.PlantId='" + identity.PlantId + @"' 
                         AND IR.CheckedBy IS NOT NULL 
                         AND IR.AuthorizedBy IS NOt NULL  
                         AND IR.CheckedByStatus='Checked' 
                         AND IR.AuthorizedByStatus='For Approval'  
                         AND IR.POType='" + poType + @"' 		
                         AND isnull(IR.IsClosed,0)=0 Order by IR.PODate DESC";//IR.AddedBy='" + identity.Name + "' " +


                }
                return _sqlRepository.GetDataCollection(Sql);
            }

            catch (Exception ex)
            {
                throw ex;

            }
        }
        public IEnumerable<object> GetIndependentPOListByStatus(string plantId, string ApproveRejectHold)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {

                var Sql = "";
                if (ApproveRejectHold == "Approved")
                {
                    Sql = @"select * from
											(
											SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
													, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
													--,IR.PODate
													, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
													, CP.UserName AS PartyAccountGroupName
													, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
													--, IR.GateEntryNo
													--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
													, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
													, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
													, IR.FixedAssetOrInventory, IR.PODepended
													--, IR.AlongwithInvoice
													--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
													, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
													, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
													, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
													, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
													,pgl.CtnId
													,isnull(IR.AddedBy,'') AddedBy
                                                    ,isnull(PLC.LCRef,'') PurchaseLC
									                ,isnull(Cn.ContractNo,'') ContructNumber
									                ,isnull(Par.UserName,'') Customer
													,isnull(IR.CheckedByStatus,'') AS CheckedByStatus,isnull(Par.UserName,'') CustomerName
													,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                                          
											FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
											LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
													ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                                            LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						                    LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						                    LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId
											LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
											LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy

											JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
											JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
											LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
											LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
											LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
											LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
											LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
											LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
											LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                                            LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
											LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
											LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
											LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
													JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"'  GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
											LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
													WHERE B.PlantId='" + plantId + @"'  GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
											LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
											LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
											WHERE  IR.POType='PO' AND IR.PlantId='" + plantId + @"' 
											AND IR.Id in(Select distinct POId from trn.InventoryReceive where POId is not null)--and RequisitionId='110232'
											AND IR.CheckedByStatus IS NULL
											AND IR.AuthorizedByStatus IS NULL
											AND isnull(IR.IsClosed,0)=0 
											--Order by IR.PODate ASC

											UNION ALL
											SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
													, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
													--,IR.PODate
													, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
													, CP.UserName AS PartyAccountGroupName
													, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
													--, IR.GateEntryNo
													--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
													, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
													, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
													, IR.FixedAssetOrInventory, IR.PODepended
													--, IR.AlongwithInvoice
													--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
													, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
													, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
													, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
													, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
													,pgl.CtnId
													,isnull(IR.AddedBy,'') AddedBy
                                                    ,isnull(PLC.LCRef,'') PurchaseLC
									                ,isnull(Cn.ContractNo,'') ContructNumber
									                ,isnull(Par.UserName,'') Customer
													,isnull(IR.CheckedByStatus,'') AS CheckedByStatus,isnull(Par.UserName,'') CustomerName
													,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                                               
											FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
											LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
													ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                                           LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						                    LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						                    LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId
											LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
											LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
											JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
											JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
											LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
											LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
											LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
											LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
											LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
											LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
											LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                                            LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
											LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
											LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
											LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
													JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"'  GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
											LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
													WHERE B.PlantId='" + plantId + @"'  GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
											LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
											LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
											WHERE  IR.POType='PO' AND IR.PlantId='" + plantId + @"' 
											AND IR.CheckedByStatus  Is null
											AND IR.AuthorizedByStatus='Approved'
											AND isnull(IR.IsClosed,0)=0 
											--Order by IR.PODate ASCr

                                             UNION ALL
											SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
													, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
													--,IR.PODate
													, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
													, CP.UserName AS PartyAccountGroupName
													, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
													--, IR.GateEntryNo
													--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
													, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
													, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
													, IR.FixedAssetOrInventory, IR.PODepended
													--, IR.AlongwithInvoice
													--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
													, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
													, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
													, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
													, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
													,pgl.CtnId
													,isnull(IR.AddedBy,'') AddedBy
                                                    ,isnull(PLC.LCRef,'') PurchaseLC
									                ,isnull(Cn.ContractNo,'') ContructNumber
									                ,isnull(Par.UserName,'') Customer
													,isnull(IR.CheckedByStatus,'') AS CheckedByStatus,isnull(Par.UserName,'') CustomerName
													,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                                            
											FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
											LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
													ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                                           LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						                    LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						                    LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId
											LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
											LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
											JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
											JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
											LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
											LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
											LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
											LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
											LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
											LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
											LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                                            LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
											LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
											LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
											LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
													JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"'  GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
											LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
													WHERE B.PlantId='" + plantId + @"'  GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
											LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
											LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
											WHERE  IR.POType='PO' AND IR.PlantId='" + plantId + @"' 
											AND IR.CheckedByStatus  Is null
											AND IR.AuthorizedByStatus Is null
											AND isnull(IR.IsClosed,0)=0 

											UNION ALL
											SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
													, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
													--,IR.PODate
													, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
													, CP.UserName AS PartyAccountGroupName
													, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
													--, IR.GateEntryNo
													--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
													, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
													, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
													, IR.FixedAssetOrInventory, IR.PODepended
													--, IR.AlongwithInvoice
													--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
													, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
													, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
													, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
													, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
													,pgl.CtnId
													,isnull(IR.AddedBy,'') AddedBy
                                                    ,isnull(PLC.LCRef,'') PurchaseLC
									                ,isnull(Cn.ContractNo,'') ContructNumber
									                ,isnull(Par.UserName,'') Customer
													,isnull(IR.CheckedByStatus,'') AS CheckedByStatus,isnull(Par.UserName,'') CustomerName
													,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                                                
											FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
											LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
													ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                                            LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						                    LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						                    LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId
											LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
											LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
											JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
											JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
											LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
											LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
											LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
											LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
											LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
											LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
											LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                                            LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
											LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
											LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
											LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
													JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"'  GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
											LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
													WHERE B.PlantId='" + plantId + @"'  GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
											LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
											LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
											WHERE  IR.POType='PO' AND IR.PlantId='" + plantId + @"' 
											AND IR.CheckedByStatus='Checked'
											AND IR.AuthorizedByStatus='Approved'
											AND isnull(IR.IsClosed,0)=0 
											)x Order by PODate ASC";
                }
                else
                {
                    Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
                                    --,IR.PODate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount ,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,pgl.CtnId
                                    ,isnull(IR.AddedBy,'') AddedBy
                                      ,isnull(PLC.LCRef,'') PurchaseLC
									    ,isnull(Cn.ContractNo,'') ContructNumber
									    ,isnull(Par.UserName,'') Customer
										,isnull(IR.CheckedByStatus,'') AS CheckedByStatus,isnull(Par.UserName,'') CustomerName
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                                    ,IRD.TransactionQty,IRD.TransactionAmount,IRD.BaseAmount
                        FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                                            LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						                    LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						                    LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                        LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
		                            JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                        WHERE  IR.POType='PO' AND IR.PlantId='" + plantId + "'  AND IR.CheckedBy IS NOT NULL " +
                        "AND IR.CheckedByStatus='Checked' " +
                        "AND IR.AuthorizedBy IS NOT NULL " +
                         " AND IR.AuthorizedByStatus<>'Approved' " +
                        " AND IR.AuthorizedByStatus <> 'For Approval'  " +
      " AND isnull(IR.IsClosed,0)=0 Order by IR.PODate ASC";

                }
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetListForHold11BOQ(string plantId, string ApproveRejectHold, string poType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {

                var Sql = "";
                if (ApproveRejectHold == "Approved")
                {
                    Sql = @"select * from
											(
											SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
													, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
													--,IR.PODate
													, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
													, CP.UserName AS PartyAccountGroupName
													, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
													--, IR.GateEntryNo
													--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
													, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
													, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
													, IR.FixedAssetOrInventory, IR.PODepended
													--, IR.AlongwithInvoice
													--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
													, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
													, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
													, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
													, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
													,pgl.CtnId
													,isnull(IR.AddedBy,'') AddedBy
                                                    ,isnull(PLC.LCRef,'') PurchaseLC
									                ,isnull(Cn.ContractNo,'') ContructNumber
									                ,isnull(Par.UserName,'') Customer
													,isnull(IR.CheckedByStatus,'') AS CheckedByStatus,isnull(Par.UserName,'') CustomerName
													,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                                          
											FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
											LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
													ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                                            LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						                    LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						                    LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId
											LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
											LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy

											JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
											JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
											LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
											LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
											LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
											LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
											LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
											LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
											LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                                            LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
											LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
											LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
											LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
													JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"'  GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
											LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
													WHERE B.PlantId='" + plantId + @"'  GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
											LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
											LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
											WHERE  IR.POType='" + poType + @"' AND IR.PlantId='" + plantId + @"' 
											AND IR.Id in(Select distinct POId from trn.InventoryReceive where POId is not null)--and RequisitionId='110232'
											AND IR.CheckedByStatus IS NULL
											AND IR.AuthorizedByStatus IS NULL
											AND isnull(IR.IsClosed,0)=0 
											--Order by IR.PODate ASC

											UNION ALL
											SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
													, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
													--,IR.PODate
													, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
													, CP.UserName AS PartyAccountGroupName
													, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
													--, IR.GateEntryNo
													--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
													, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
													, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
													, IR.FixedAssetOrInventory, IR.PODepended
													--, IR.AlongwithInvoice
													--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
													, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
													, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
													, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
													, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
													,pgl.CtnId
													,isnull(IR.AddedBy,'') AddedBy
                                                    ,isnull(PLC.LCRef,'') PurchaseLC
									                ,isnull(Cn.ContractNo,'') ContructNumber
									                ,isnull(Par.UserName,'') Customer
													,isnull(IR.CheckedByStatus,'') AS CheckedByStatus,isnull(Par.UserName,'') CustomerName
													,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                                               
											FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
											LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
													ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                                           LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						                    LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						                    LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId
											LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
											LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
											JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
											JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
											LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
											LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
											LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
											LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
											LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
											LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
											LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                                            LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
											LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
											LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
											LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
													JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"'  GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
											LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
													WHERE B.PlantId='" + plantId + @"'  GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
											LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
											LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
											WHERE  IR.POType='" + poType + @"' AND IR.PlantId='" + plantId + @"' 
											AND IR.CheckedByStatus  Is null
											AND IR.AuthorizedByStatus='Approved'
											AND isnull(IR.IsClosed,0)=0 
											--Order by IR.PODate ASCr

                                             UNION ALL
											SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
													, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
													--,IR.PODate
													, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
													, CP.UserName AS PartyAccountGroupName
													, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
													--, IR.GateEntryNo
													--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
													, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
													, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
													, IR.FixedAssetOrInventory, IR.PODepended
													--, IR.AlongwithInvoice
													--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
													, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
													, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
													, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
													, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
													,pgl.CtnId
													,isnull(IR.AddedBy,'') AddedBy
                                                    ,isnull(PLC.LCRef,'') PurchaseLC
									                ,isnull(Cn.ContractNo,'') ContructNumber
									                ,isnull(Par.UserName,'') Customer
													,isnull(IR.CheckedByStatus,'') AS CheckedByStatus,isnull(Par.UserName,'') CustomerName
													,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                                            
											FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
											LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
													ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                                           LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						                    LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						                    LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId
											LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
											LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
											JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
											JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
											LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
											LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
											LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
											LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
											LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
											LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
											LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                                            LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
											LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
											LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
											LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
													JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"'  GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
											LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
													WHERE B.PlantId='" + plantId + @"'  GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
											LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
											LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
											WHERE  IR.POType='" + poType + @"' AND IR.PlantId='" + plantId + @"' 
											AND IR.CheckedByStatus  Is null
											AND IR.AuthorizedByStatus Is null
											AND isnull(IR.IsClosed,0)=0 

											UNION ALL
											SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
													, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
													--,IR.PODate
													, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
													, CP.UserName AS PartyAccountGroupName
													, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
													--, IR.GateEntryNo
													--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
													, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
													, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
													, IR.FixedAssetOrInventory, IR.PODepended
													--, IR.AlongwithInvoice
													--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
													, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
													, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
													, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
													, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
													,pgl.CtnId
													,isnull(IR.AddedBy,'') AddedBy
                                                    ,isnull(PLC.LCRef,'') PurchaseLC
									                ,isnull(Cn.ContractNo,'') ContructNumber
									                ,isnull(Par.UserName,'') Customer
													,isnull(IR.CheckedByStatus,'') AS CheckedByStatus,isnull(Par.UserName,'') CustomerName
													,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                                                
											FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
											LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
													ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                                            LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						                    LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						                    LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId
											LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
											LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
											JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
											JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
											LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
											LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
											LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
											LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
											LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
											LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
											LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                                            LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
											LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
											LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
											LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
													JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"'  GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
											LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
													WHERE B.PlantId='" + plantId + @"'  GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
											LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
											LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
											WHERE  IR.POType='" + poType + @"' AND IR.PlantId='" + plantId + @"' 
											AND IR.CheckedByStatus='Checked'
											AND IR.AuthorizedByStatus='Approved'
											AND isnull(IR.IsClosed,0)=0 
											)x Order by PODate ASC";
                }
                else
                {
                    Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
                                    --,IR.PODate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount ,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,pgl.CtnId
                                    ,isnull(IR.AddedBy,'') AddedBy
                                      ,isnull(PLC.LCRef,'') PurchaseLC
									    ,isnull(Cn.ContractNo,'') ContructNumber
									    ,isnull(Par.UserName,'') Customer
										,isnull(IR.CheckedByStatus,'') AS CheckedByStatus,isnull(Par.UserName,'') CustomerName
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                                    ,IRD.TransactionQty,IRD.TransactionAmount,IRD.BaseAmount
                        FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                                            LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						                    LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						                    LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                        LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
		                            JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                        WHERE  IR.POType='" + poType + @"' AND IR.PlantId='" + plantId + "'  AND IR.CheckedBy IS NOT NULL " +
                        "AND IR.CheckedByStatus='Checked' " +
                        "AND IR.AuthorizedBy IS NOT NULL " +
                         " AND IR.AuthorizedByStatus<>'Approved' " +
                        " AND IR.AuthorizedByStatus <> 'For Approval'  " +
      " AND isnull(IR.IsClosed,0)=0 Order by IR.PODate ASC";

                }
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> PORollBackApproved(string plantId, string ApproveRejectHold)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {

                var Sql = "";
                if (ApproveRejectHold == "Approved")
                {
                    Sql = @"select * from
											(
											SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
													, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
													--,IR.PODate
													, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
													, CP.UserName AS PartyAccountGroupName
													, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
													--, IR.GateEntryNo
													--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
													, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
													, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
													, IR.FixedAssetOrInventory, IR.PODepended
													--, IR.AlongwithInvoice
													--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
													, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
													, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
													, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
													, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
													,pgl.CtnId
													,isnull(IR.AddedBy,'') AddedBy
                                                    ,isnull(PLC.LCRef,'') PurchaseLC
									                ,isnull(Cn.ContractNo,'') ContructNumber
									                ,isnull(Par.UserName,'') Customer
													,isnull(IR.CheckedByStatus,'') AS CheckedByStatus,isnull(Par.UserName,'') CustomerName
													,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                                          ,isnull(GRNId,'')GRNId
											FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
											LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
													ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                                            LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						                    LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						                    LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId
											LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
											LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
											JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
											JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
											LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
											LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
											LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
											LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
											LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
											LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
											LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                                            LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
											LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
											LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
											LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
													JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"'  GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
											LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
													WHERE B.PlantId='" + plantId + @"'  GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
											LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
											LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                                                                                LEFT JOIN(
									SELECT distinct PDAMAP.PoId
									,GRNId=STUFF((select distinct ','+xPDAMAP.GRNId from
									trn.PurchaseOrder xpo
									INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.PoId
									where xPDAMAP.PoId=PDAMAP.PoId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									from trn.POGGRNMap PDAMAP
									LEFT JOIN [TRN].PurchaseOrder IR ON IR.Id = PDAMAP.PoId
									group by PDAMAP.PoId
							)PDA ON PDA.PoId=IR.Id		
											WHERE  IR.PlantId='" + plantId + @"' 
											AND IR.Id in(Select distinct POId from trn.InventoryReceive where POId is not null)--and RequisitionId='110232'
											AND IR.CheckedByStatus IS NULL
											AND IR.AuthorizedByStatus IS NULL
											AND isnull(IR.IsClosed,0)=0 
											--Order by IR.PODate ASC

											UNION ALL
											SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
													, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
													--,IR.PODate
													, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
													, CP.UserName AS PartyAccountGroupName
													, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
													--, IR.GateEntryNo
													--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
													, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
													, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
													, IR.FixedAssetOrInventory, IR.PODepended
													--, IR.AlongwithInvoice
													--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
													, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
													, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
													, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
													, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
													,pgl.CtnId
													,isnull(IR.AddedBy,'') AddedBy
                                                    ,isnull(PLC.LCRef,'') PurchaseLC
									                ,isnull(Cn.ContractNo,'') ContructNumber
									                ,isnull(Par.UserName,'') Customer
													,isnull(IR.CheckedByStatus,'') AS CheckedByStatus,isnull(Par.UserName,'') CustomerName
													,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                                               ,isnull(GRNId,'')GRNId
											FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
											LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
													ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                                           LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						                    LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						                    LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId
											LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
											LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
											JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
											JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
											LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
											LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
											LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
											LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
											LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
											LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
											LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                                            LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
											LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
											LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
											LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
													JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"'  GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
											LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
													WHERE B.PlantId='" + plantId + @"'  GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
											LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
											LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                                    LEFT JOIN(
									SELECT distinct PDAMAP.PoId
									,GRNId=STUFF((select distinct ','+xPDAMAP.GRNId from
									trn.PurchaseOrder xpo
									INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.PoId
									where xPDAMAP.PoId=PDAMAP.PoId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									from trn.POGGRNMap PDAMAP
									LEFT JOIN [TRN].PurchaseOrder IR ON IR.Id = PDAMAP.PoId
									group by PDAMAP.PoId
							)PDA ON PDA.PoId=IR.Id		
											WHERE  IR.PlantId='" + plantId + @"' 
											AND IR.CheckedByStatus  Is null
											AND IR.AuthorizedByStatus='Approved'
											AND isnull(IR.IsClosed,0)=0 
											--Order by IR.PODate ASCr

                                             UNION ALL
											SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
													, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
													--,IR.PODate
													, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
													, CP.UserName AS PartyAccountGroupName
													, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
													--, IR.GateEntryNo
													--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
													, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
													, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
													, IR.FixedAssetOrInventory, IR.PODepended
													--, IR.AlongwithInvoice
													--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
													, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
													, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
													, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
													, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
													,pgl.CtnId
													,isnull(IR.AddedBy,'') AddedBy
                                                    ,isnull(PLC.LCRef,'') PurchaseLC
									                ,isnull(Cn.ContractNo,'') ContructNumber
									                ,isnull(Par.UserName,'') Customer
													,isnull(IR.CheckedByStatus,'') AS CheckedByStatus,isnull(Par.UserName,'') CustomerName
													,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                                            ,isnull(GRNId,'')GRNId
											FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
											LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
													ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                                           LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						                    LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						                    LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId
											LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
											LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
											JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
											JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
											LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
											LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
											LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
											LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
											LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
											LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
											LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                                            LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
											LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
											LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
											LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
													JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"'  GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
											LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
													WHERE B.PlantId='" + plantId + @"'  GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
											LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
											LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                                    LEFT JOIN(
									SELECT distinct PDAMAP.PoId
									,GRNId=STUFF((select distinct ','+xPDAMAP.GRNId from
									trn.PurchaseOrder xpo
									INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.PoId
									where xPDAMAP.PoId=PDAMAP.PoId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									from trn.POGGRNMap PDAMAP
									LEFT JOIN [TRN].PurchaseOrder IR ON IR.Id = PDAMAP.PoId
									group by PDAMAP.PoId
							)PDA ON PDA.PoId=IR.Id		
											WHERE  IR.PlantId='" + plantId + @"' 
											AND IR.CheckedByStatus  Is null
											AND IR.AuthorizedByStatus Is null
											AND isnull(IR.IsClosed,0)=0 

											UNION ALL
											SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
													, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
													--,IR.PODate
													, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
													, CP.UserName AS PartyAccountGroupName
													, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
													--, IR.GateEntryNo
													--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
													, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
													, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
													, IR.FixedAssetOrInventory, IR.PODepended
													--, IR.AlongwithInvoice
													--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
													, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
													, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
													, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
													, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
													,pgl.CtnId
													,isnull(IR.AddedBy,'') AddedBy
                                                    ,isnull(PLC.LCRef,'') PurchaseLC
									                ,isnull(Cn.ContractNo,'') ContructNumber
									                ,isnull(Par.UserName,'') Customer
													,isnull(IR.CheckedByStatus,'') AS CheckedByStatus,isnull(Par.UserName,'') CustomerName
													,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                                                ,isnull(GRNId,'')GRNId
											FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
											LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
													ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                                            LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						                    LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						                    LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId
											LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
											LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
											JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
											JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
											LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
											LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
											LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
											LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
											LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
											LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
											LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                                            LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
											LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
											LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
											LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
													JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"'  GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
											LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
													WHERE B.PlantId='" + plantId + @"'  GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
											LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
											LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                                    LEFT JOIN(
									SELECT distinct PDAMAP.PoId
									,GRNId=STUFF((select distinct ','+xPDAMAP.GRNId from
									trn.PurchaseOrder xpo
									INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.PoId
									where xPDAMAP.PoId=PDAMAP.PoId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									from trn.POGGRNMap PDAMAP
									LEFT JOIN [TRN].PurchaseOrder IR ON IR.Id = PDAMAP.PoId
									group by PDAMAP.PoId
							)PDA ON PDA.PoId=IR.Id											
                                            WHERE  IR.PlantId='" + plantId + @"' 
											--AND IR.CheckedByStatus='Checked'
											AND IR.AuthorizedByStatus='Approved'
											--AND isnull(IR.IsClosed,0)=0 
											)x Order by PODate ASC";
                }
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }


        private string GRNApprovalLogTblId()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "GRNApprovalLogTbl", out sID);
            return sID;
        }
        public IEnumerable<object> GetListForHold1(string plantId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                var Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,IR.AddedBy
		                            ,PLC.LCANo PurchaseLC
									,Ctc.ContractNo ContructNumber
									,Par.UserName Customer
                                    ,eI.EmployeeName CheckedBy
                                    ,IR.CheckedByStatus AS CheckedByStatus
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus
                                    ,eI1.EmployeeName AuthorizedBy,isnull(PO.RequisitionId,'')  RequisitionId

                        FROM [TRN].[PurchaseOrder] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                                  LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId 
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
                        left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
		                            JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
                        WHERE IR.PlantId='" + plantId + @"' and IR.CheckedBy='" + identity.EmployeeId + @"' AND CheckedbyStatus = 'Checked' AND ISNULL(IR.[Status],'')<>'Posting'  Order by IR.ID DESC";//AND IR.IsApproved=0
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        //sk
        //  public IEnumerable<object> getCheckedList(string plantId)
        //  {

        //      var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

        //      try
        //      {
        //          var Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
        //                   SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
        //                     , CP.UserName AS PartyAccountGroupName
        //                           , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
        //                           --, IR.GateEntryNo
        //                              --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
        //                              , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
        //                           , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
        //                           , IR.FixedAssetOrInventory, IR.PODepended
        //                              --, IR.AlongwithInvoice
        //                              --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
        //                           , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
        //                           , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
        //                              , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
        //			, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
        //                              ,IR.AddedBy
        //                        ,PLC.LCANo PurchaseLC
        //			,Ctc.ContractNo ContructNumber
        //			,Par.UserName Customer
        //                              ,eI.EmployeeName CheckedBy
        //                              ,IR.CheckedByStatus AS CheckedByStatus
        //                     ,IR.AuthorizedByStatus AS AuthorizedByStatus
        //                              ,eI1.EmployeeName AuthorizedBy,isnull(PO.RequisitionId,'')  RequisitionId

        //                  FROM [TRN].[PurchaseOrder] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
        //                  LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
        //                     ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
        //                            LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
        //LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
        //LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId 
        //                  LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
        //                  LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
        //                  left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
        //                  left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
        //                  LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
        //                  LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
        //                  LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
        //                  LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
        //                  LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
        //                  LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
        //                  LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
        //                  LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
        //LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
        //                  LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
        //                        JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id  GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
        //                  LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
        //                         GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
        //                  LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
        //                  LEFT JOIN(
        //					select PDAMAP.InventoryReceiveId
        //					,RequisitionId=STUFF((select distinct ','+xpo.Id from
        //					trn.MaterialRequsitionMaster xpo
        //					INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
        //					where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

        //					from  TRN.PurchaseOrderDetail PDAMAP 
        //					LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
        //					group by  PDAMAP.InventoryReceiveId		
        //		)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
        //                  WHERE IR.PlantId='" + plantId + @"' and IR.CheckedBy='" + identity.EmployeeId + @"' AND CheckedbyStatus = 'Checked'  Order by IR.ID DESC";//AND IR.IsApproved=0
        //          return _sqlRepository.GetDataCollection(Sql);
        //      }
        //      catch (Exception ex)
        //      {
        //          throw new CustomException(ex.Message, ex,
        //              Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //              ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
        //      }
        //  }
        public IEnumerable<object> GetListForPOApproval1UnApproved(string plantId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                var Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         Select* from(
									SELECT ROW_NUMBER() OVER (ORDER BY  IR.Id) AS SiNo, IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106), ' ', '-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
							, CP.UserName AS PartyAccountGroupName
							, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106), ' ', '-') AS DocDate
							--, IR.GateEntryNo
							--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106), ' ', '-') AS EntryDate
							  , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
							, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
								, IR.FixedAssetOrInventory, IR.PODepended
								--, IR.AlongwithInvoice
								--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate
								  , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
							, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
							, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
							, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
							,IR.AddedBy
	                        ,PLC.LCANo PurchaseLC
							,Ctc.ContractNo ContructNumber
							,Par.UserName Customer
							,eI.EmployeeName CheckedBy
							, IR.CheckedByStatus AS CheckedByStatus
							,IR.AuthorizedByStatus AS AuthorizedByStatus
							,eI1.EmployeeName AuthorizedBy,isnull(PO.RequisitionId,'')  RequisitionId

					FROM[TRN].[PurchaseOrder] AS IR left JOIN[HKP].[Party] AS P ON IR.PartyId = P.Id
					LEFT JOIN(SELECT C.PartyId, C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
							ON PAG.Id= C.PartyAccountGroupId WHERE C.PartyType= 'Vendor') AS CP ON CP.PartyId = IR.PartyId AND CP.PlantId = IR.PlantId

                    LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
					LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
					LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId 
					LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId = IR.CheckedBy
					LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId = IR.AuthorizedBy
					left JOIN[SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id
					left JOIN[MST].[PaymentTerm] AS PT ON IR.PaymentTermId = PT.Id
					LEFT JOIN[HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
					LEFT JOIN[MST].[AddressMaster] AS AM ON IPP.AddressMasterId= AM.Id
					LEFT JOIN [SCS].[State] AS S1 ON AM.StateId= S1.Id
					LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId= DPP.Id
					LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId= AM2.Id
					LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId= S2.Id
					LEFT JOIN [ORG].Plant PL ON PL.Id= IR.PlantId
					LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id= PL.AddressMasterId
					LEFT JOIN [SCS].[State] AS SP ON SP.Id= AMP.StateId
					LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A

						   JOIN[TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId= B.Id WHERE B.PlantId= '" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
					LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN[TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId= B.Id
							WHERE B.PlantId= '" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
					 LEFT JOIN[SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId= UoM.Id
                    LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
					 WHERE IR.AuthorizedBy= '" + identity.EmployeeId + @"'
					 AND IR.CheckedByStatus= 'Checked'
					 AND IR.AuthorizedbyStatus = 'Approved'
 

					 UNION ALL
					 SELECT ROW_NUMBER() OVER (ORDER BY  IR.Id) AS SiNo, IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
							, CP.UserName AS PartyAccountGroupName
							, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
							--, IR.GateEntryNo
							--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
							, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
							, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
							  , IR.FixedAssetOrInventory, IR.PODepended
							--, IR.AlongwithInvoice
							--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
							, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
							, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
							, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
							, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
							,IR.AddedBy
	                                       ,PLC.LCANo PurchaseLC
									       ,Ctc.ContractNo ContructNumber
									       ,Par.UserName Customer
							,eI.EmployeeName CheckedBy
							, IR.CheckedByStatus AS CheckedByStatus
							,IR.AuthorizedByStatus AS AuthorizedByStatus
							,eI1.EmployeeName AuthorizedBy,isnull(PO.RequisitionId,'')  RequisitionId

					FROM[TRN].[PurchaseOrder] AS IR left JOIN[HKP].[Party] AS P ON IR.PartyId=P.Id
					LEFT JOIN (SELECT C.PartyId, C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG

						  ON PAG.Id= C.PartyAccountGroupId WHERE C.PartyType= 'Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId= IR.PlantId

		            LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
				    LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
				    LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId 
					LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId= IR.CheckedBy
					LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId= IR.AuthorizedBy
					left JOIN[SCS].[Currency] AS CU ON IR.CurrencyId= CU.Id
					left JOIN[MST].[PaymentTerm] AS PT ON IR.PaymentTermId= PT.Id
					LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId= IPP.Id
					LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId= AM.Id
					LEFT JOIN [SCS].[State] AS S1 ON AM.StateId= S1.Id
					LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId= DPP.Id
					LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId= AM2.Id
					LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId= S2.Id
					LEFT JOIN [ORG].Plant PL ON PL.Id= IR.PlantId
					LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id= PL.AddressMasterId
					LEFT JOIN [SCS].[State] AS SP ON SP.Id= AMP.StateId
					LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
							JOIN[TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId= B.Id WHERE B.PlantId= '" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
					 LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN[TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId= B.Id
 
							 WHERE B.PlantId= '" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
					  LEFT JOIN[SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId= UoM.Id
                      LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
					  WHERE IR.AuthorizedBy= '" + identity.EmployeeId + @"'
					  AND IR.CheckedByStatus is null
					AND IR.AuthorizedbyStatus = 'Approved'
					  )x
					  Order by PODate ASC";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetListForPOApproval1Auth(string plantId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                var Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                	
				Select* from(
									SELECT ROW_NUMBER() OVER (ORDER BY  IR.Id) AS SiNo, IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106), ' ', '-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
							, CP.UserName AS PartyAccountGroupName
							, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106), ' ', '-') AS DocDate
							--, IR.GateEntryNo
							--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106), ' ', '-') AS EntryDate
							  , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
							, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
								, IR.FixedAssetOrInventory, IR.PODepended
								--, IR.AlongwithInvoice
								--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate
								  , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
							, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
							, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
							, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
							,IR.AddedBy
	                        ,PLC.LCANo PurchaseLC
							,Ctc.ContractNo ContructNumber
							,Par.UserName Customer
							,eI.EmployeeName CheckedBy
							, IR.CheckedByStatus AS CheckedByStatus
							,IR.AuthorizedByStatus AS AuthorizedByStatus
							,eI1.EmployeeName AuthorizedBy,isnull(PO.RequisitionId,'')  RequisitionId

					FROM[TRN].[PurchaseOrder] AS IR left JOIN[HKP].[Party] AS P ON IR.PartyId = P.Id
					LEFT JOIN(SELECT C.PartyId, C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
							ON PAG.Id= C.PartyAccountGroupId WHERE C.PartyType= 'Vendor') AS CP ON CP.PartyId = IR.PartyId AND CP.PlantId = IR.PlantId

                    LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
					LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
					LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId 
					LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId = IR.CheckedBy
					LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId = IR.AuthorizedBy
					left JOIN[SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id
					left JOIN[MST].[PaymentTerm] AS PT ON IR.PaymentTermId = PT.Id
					LEFT JOIN[HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
					LEFT JOIN[MST].[AddressMaster] AS AM ON IPP.AddressMasterId= AM.Id
					LEFT JOIN [SCS].[State] AS S1 ON AM.StateId= S1.Id
					LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId= DPP.Id
					LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId= AM2.Id
					LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId= S2.Id
					LEFT JOIN [ORG].Plant PL ON PL.Id= IR.PlantId
					LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id= PL.AddressMasterId
					LEFT JOIN [SCS].[State] AS SP ON SP.Id= AMP.StateId
					LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A

						   JOIN[TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId= B.Id WHERE B.PlantId= '" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
					LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN[TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId= B.Id
							WHERE B.PlantId= '" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
					 LEFT JOIN[SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId= UoM.Id
                    LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
					 WHERE IR.AuthorizedBy= '" + identity.EmployeeId + @"'
					 AND IR.CheckedByStatus= 'Checked'
					 AND IR.AuthorizedbyStatus = 'Approved'
 

					 UNION ALL
					 SELECT ROW_NUMBER() OVER (ORDER BY  IR.Id) AS SiNo, IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
							, CP.UserName AS PartyAccountGroupName
							, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
							--, IR.GateEntryNo
							--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
							, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
							, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
							  , IR.FixedAssetOrInventory, IR.PODepended
							--, IR.AlongwithInvoice
							--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
							, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
							, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
							, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
							, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
							,IR.AddedBy
	                                       ,PLC.LCANo PurchaseLC
									       ,Ctc.ContractNo ContructNumber
									       ,Par.UserName Customer
							,eI.EmployeeName CheckedBy
							, IR.CheckedByStatus AS CheckedByStatus
							,IR.AuthorizedByStatus AS AuthorizedByStatus
							,eI1.EmployeeName AuthorizedBy,isnull(PO.RequisitionId,'')  RequisitionId

					FROM[TRN].[PurchaseOrder] AS IR left JOIN[HKP].[Party] AS P ON IR.PartyId=P.Id
					LEFT JOIN (SELECT C.PartyId, C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG

						  ON PAG.Id= C.PartyAccountGroupId WHERE C.PartyType= 'Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId= IR.PlantId

		            LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
				    LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
				    LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId 
					LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId= IR.CheckedBy
					LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId= IR.AuthorizedBy
					left JOIN[SCS].[Currency] AS CU ON IR.CurrencyId= CU.Id
					left JOIN[MST].[PaymentTerm] AS PT ON IR.PaymentTermId= PT.Id
					LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId= IPP.Id
					LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId= AM.Id
					LEFT JOIN [SCS].[State] AS S1 ON AM.StateId= S1.Id
					LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId= DPP.Id
					LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId= AM2.Id
					LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId= S2.Id
					LEFT JOIN [ORG].Plant PL ON PL.Id= IR.PlantId
					LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id= PL.AddressMasterId
					LEFT JOIN [SCS].[State] AS SP ON SP.Id= AMP.StateId
					LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
							JOIN[TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId= B.Id WHERE B.PlantId= '" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
					 LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN[TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId= B.Id
 
							 WHERE B.PlantId= '" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
					  LEFT JOIN[SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId= UoM.Id
                      LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
					  WHERE IR.AuthorizedBy= '" + identity.EmployeeId + @"'
					  AND IR.CheckedByStatus is null
					AND IR.AuthorizedbyStatus = 'Approved'
					  )x
					  Order by PODate ASC";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> getApprovedHoldReject(string plantId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                var Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                	
				Select top(600) * from(
									SELECT ROW_NUMBER() OVER (ORDER BY  IR.Id) AS SiNo, IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106), ' ', '-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
							, CP.UserName AS PartyAccountGroupName
							, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106), ' ', '-') AS DocDate
							--, IR.GateEntryNo
							--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106), ' ', '-') AS EntryDate
							  , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
							, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
								, IR.FixedAssetOrInventory, IR.PODepended
								--, IR.AlongwithInvoice
								--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate
								  , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
							, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
							, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
							, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
							,IR.AddedBy
	                        ,PLC.LCANo PurchaseLC
							,Ctc.ContractNo ContructNumber
							,Par.UserName Customer
							,eI.EmployeeName CheckedBy
							, IR.CheckedByStatus AS CheckedByStatus
							,IR.AuthorizedByStatus AS AuthorizedByStatus
							,eI1.EmployeeName AuthorizedBy,isnull(PO.RequisitionId,'')  RequisitionId

					FROM[TRN].[PurchaseOrder] AS IR left JOIN[HKP].[Party] AS P ON IR.PartyId = P.Id
					LEFT JOIN(SELECT C.PartyId, C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
							ON PAG.Id= C.PartyAccountGroupId WHERE C.PartyType= 'Vendor') AS CP ON CP.PartyId = IR.PartyId AND CP.PlantId = IR.PlantId

                    LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
					LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
					LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId 
					LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId = IR.CheckedBy
					LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId = IR.AuthorizedBy
					left JOIN[SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id
					left JOIN[MST].[PaymentTerm] AS PT ON IR.PaymentTermId = PT.Id
					LEFT JOIN[HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
					LEFT JOIN[MST].[AddressMaster] AS AM ON IPP.AddressMasterId= AM.Id
					LEFT JOIN [SCS].[State] AS S1 ON AM.StateId= S1.Id
					LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId= DPP.Id
					LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId= AM2.Id
					LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId= S2.Id
					LEFT JOIN [ORG].Plant PL ON PL.Id= IR.PlantId
					LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id= PL.AddressMasterId
					LEFT JOIN [SCS].[State] AS SP ON SP.Id= AMP.StateId
					LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A

						   JOIN[TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId= B.Id  GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
					LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN[TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId= B.Id
							 GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
					 LEFT JOIN[SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId= UoM.Id
                    LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
					 WHERE IR.AuthorizedBy= '" + identity.EmployeeId + @"'
					 AND IR.CheckedByStatus= 'Checked'
					 AND IR.AuthorizedbyStatus = 'Approved' 
 

					 UNION ALL
					 SELECT ROW_NUMBER() OVER (ORDER BY  IR.Id) AS SiNo, IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
							, CP.UserName AS PartyAccountGroupName
							, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
							--, IR.GateEntryNo
							--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
							, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
							, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
							  , IR.FixedAssetOrInventory, IR.PODepended
							--, IR.AlongwithInvoice
							--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
							, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
							, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
							, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
							, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
							,IR.AddedBy
	                                       ,PLC.LCANo PurchaseLC
									       ,Ctc.ContractNo ContructNumber
									       ,Par.UserName Customer
							,eI.EmployeeName CheckedBy
							, IR.CheckedByStatus AS CheckedByStatus
							,IR.AuthorizedByStatus AS AuthorizedByStatus
							,eI1.EmployeeName AuthorizedBy,isnull(PO.RequisitionId,'')  RequisitionId

					FROM[TRN].[PurchaseOrder] AS IR left JOIN[HKP].[Party] AS P ON IR.PartyId=P.Id
					LEFT JOIN (SELECT C.PartyId, C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG

						  ON PAG.Id= C.PartyAccountGroupId WHERE C.PartyType= 'Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId= IR.PlantId

		            LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
				    LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
				    LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId 
					LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId= IR.CheckedBy
					LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId= IR.AuthorizedBy
					left JOIN[SCS].[Currency] AS CU ON IR.CurrencyId= CU.Id
					left JOIN[MST].[PaymentTerm] AS PT ON IR.PaymentTermId= PT.Id
					LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId= IPP.Id
					LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId= AM.Id
					LEFT JOIN [SCS].[State] AS S1 ON AM.StateId= S1.Id
					LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId= DPP.Id
					LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId= AM2.Id
					LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId= S2.Id
					LEFT JOIN [ORG].Plant PL ON PL.Id= IR.PlantId
					LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id= PL.AddressMasterId
					LEFT JOIN [SCS].[State] AS SP ON SP.Id= AMP.StateId
					LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
							JOIN[TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId= B.Id  GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
					 LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN[TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId= B.Id
 
							  GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
					  LEFT JOIN[SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId= UoM.Id
                      LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
					  WHERE IR.AuthorizedBy= '" + identity.EmployeeId + @"'
					  AND IR.CheckedByStatus is null
					AND IR.AuthorizedbyStatus = 'Approved'
					  )x
					  Order by CONVERT(datetime,PODate) desc";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetListForPOApproval(string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                var Sql = @"--DECLARE @plantId VARCHAR(10)='201816';
                           SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
                                    --,IR.PODate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,pgl.CtnId
                                    ,IR.AddedBy
		                            ,PLC.LCANo PurchaseLC
									,Ctc.ContractNo ContructNumber
									,Par.UserName Customer
                                    ,IR.CheckedByStatus AS CheckedByStatus
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy,isnull(PO.RequisitionId,'') RequisitionId
                        FROM [TRN].[PurchaseOrder] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                        LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId 
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy

                        left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
		                            JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"'  GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId
						,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                        LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
                WHERE IR.CheckedBy='" + identity.EmployeeId + "' AND CheckedbyStatus ='pending' Order by IR.PODate ASC  "; //IR.PlantId = '" + plantId + "' and
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        //sk1
        //  public IEnumerable<object> getPendingList(string plantId)
        //  {
        //      var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //      try
        //      {
        //          var Sql = @"--DECLARE @plantId VARCHAR(10)='201816';
        //                     SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
        //                              , REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
        //                              --,IR.PODate
        //                              , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
        //                     , CP.UserName AS PartyAccountGroupName
        //                           , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
        //                           --, IR.GateEntryNo
        //                              --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
        //                              , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
        //                           , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
        //                           , IR.FixedAssetOrInventory, IR.PODepended
        //                              --, IR.AlongwithInvoice
        //                              --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
        //                           , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
        //                           , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
        //                              , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
        //			, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
        //                              ,pgl.CtnId
        //                              ,IR.AddedBy
        //                        ,PLC.LCANo PurchaseLC
        //			,Ctc.ContractNo ContructNumber
        //			,Par.UserName Customer
        //                              ,IR.CheckedByStatus AS CheckedByStatus
        //                     ,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy,isnull(PO.RequisitionId,'') RequisitionId
        //                  FROM [TRN].[PurchaseOrder] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
        //                  LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
        //                     ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId

        //                  LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
        //LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
        //LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId 
        //                  LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
        //                  LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy

        //                  left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
        //                  left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
        //                  LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
        //                  LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
        //                  LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
        //                  LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
        //                  LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
        //                  LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
        //                  LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
        //                  LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
        //LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
        //                  LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
        //                        JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id  GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
        //                  LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
        //                          GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
        //                  LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
        //                  LEFT JOIN (Select count(Id) as CtnId
        //,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
        //                  LEFT JOIN(
        //					select PDAMAP.InventoryReceiveId
        //					,RequisitionId=STUFF((select distinct ','+xpo.Id from
        //					trn.MaterialRequsitionMaster xpo
        //					INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
        //					where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

        //					from  TRN.PurchaseOrderDetail PDAMAP 
        //					LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
        //					group by  PDAMAP.InventoryReceiveId		
        //		)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
        //          WHERE IR.CheckedBy='" + identity.EmployeeId + "' AND CheckedbyStatus ='pending' Order by IR.PODate ASC  "; //IR.PlantId = '" + plantId + "' and
        //          return _sqlRepository.GetDataCollection(Sql);
        //      }
        //      catch (Exception ex)
        //      {
        //          throw new CustomException(ex.Message, ex,
        //              Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //              ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
        //      }
        //  }
        public IEnumerable<object> GetListForPOHoldandReject(string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                var Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,IR.AddedBy
			                        ,PLC.LCANo PurchaseLC
									,Ctc.ContractNo ContructNumber
									,Par.UserName Customer
                                    ,eI.EmployeeName CheckedBy
                                    ,IR.CheckedByStatus AS CheckedByStatus
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus
                                    ,eI1.EmployeeName AuthorizedBy
	                                ,IR.CheckedHoldRejectReason

                        FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                           LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId 
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
		                            JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        WHERE  IR.CheckedBy='" + identity.EmployeeId + @"' AND CheckedbyStatus ='Hold' OR CheckedbyStatus ='Reject' Order by IR.POdate ASC";// IR.PlantId = '" + plantId + @"' and
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> getCheckedHoldReject(string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,IR.AddedBy
			                        ,PLC.LCANo PurchaseLC
									,Ctc.ContractNo ContructNumber
									,Par.UserName Customer
                                    ,eI.EmployeeName CheckedBy
                                    ,IR.CheckedByStatus AS CheckedByStatus
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus
                                    ,eI1.EmployeeName AuthorizedBy
	                                ,IR.CheckedHoldRejectReason CheckedRejectReason

                        FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                           LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId 
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
		                            JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        WHERE  IR.CheckedBy='" + identity.EmployeeId + @"' AND CheckedbyStatus ='Hold' OR CheckedbyStatus ='Reject' Order by IR.POdate ASC";// IR.PlantId = '" + plantId + @"' and
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetListForPOApprovalAuthorized(string plantId, string POTypeApprovalStatus)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var Sql = "";
            try
            {
                if (POTypeApprovalStatus == "For Approval")
                {


                    Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
					select * from(SELECT ROW_NUMBER() OVER (ORDER BY  IR.Id) AS SiNo, IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106), ' ', '-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
											, CP.UserName AS PartyAccountGroupName
											, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106), ' ', '-') AS DocDate

											--, IR.GateEntryNo
											--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106), ' ', '-') AS EntryDate
											  , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
											, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
												, IR.FixedAssetOrInventory, IR.PODepended
												--, IR.AlongwithInvoice
												--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate
												  , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
											, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount,IR.ToCurrencyRate
											, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
											, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
											,IR.AddedBy
	                                       ,PLC.LCANo PurchaseLC
									       ,Ctc.ContractNo ContructNumber
									       ,Par.UserName Customer
											,eI.EmployeeName CheckedBy
											, IR.CheckedByStatus AS CheckedByStatus
											,IR.AuthorizedByStatus AS AuthorizedByStatus
											,eI1.EmployeeName AuthorizedBy,isnull(PO.RequisitionId,'')  RequisitionId
									FROM[TRN].[PurchaseOrder] AS IR left JOIN[HKP].[Party] AS P ON IR.PartyId = P.Id
									LEFT JOIN(SELECT C.PartyId, C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
											ON PAG.Id= C.PartyAccountGroupId WHERE C.PartyType= 'Vendor') AS CP ON CP.PartyId = IR.PartyId AND CP.PlantId = IR.PlantId
									LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						            LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						            LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId 
                                    LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId = IR.CheckedBy
									LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId = IR.AuthorizedBy
									left JOIN[SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id
									left JOIN[MST].[PaymentTerm] AS PT ON IR.PaymentTermId = PT.Id
									LEFT JOIN[HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
									LEFT JOIN[MST].[AddressMaster] AS AM ON IPP.AddressMasterId= AM.Id
									LEFT JOIN [SCS].[State] AS S1 ON AM.StateId= S1.Id
									LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId= DPP.Id
									LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId= AM2.Id
									LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId= S2.Id
									LEFT JOIN [ORG].Plant PL ON PL.Id= IR.PlantId
									LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id= PL.AddressMasterId
									LEFT JOIN [SCS].[State] AS SP ON SP.Id= AMP.StateId
									LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A

										   JOIN[TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId= B.Id WHERE B.PlantId= '" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
									LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN[TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId= B.Id
											WHERE B.PlantId= '" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
									 LEFT JOIN[SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId= UoM.Id
                                     LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
									 WHERE IR.AuthorizedBy ='" + identity.EmployeeId + @"'
									 AND IR.CheckedByStatus = 'Checked'
									 AND IR.AuthorizedByStatus = 'For Approval'
									 UNION ALL
									 SELECT ROW_NUMBER() OVER (ORDER BY  IR.Id) AS SiNo, IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
											, CP.UserName AS PartyAccountGroupName
											, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
											--, IR.GateEntryNo
											--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
											, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
											, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
											  , IR.FixedAssetOrInventory, IR.PODepended
											--, IR.AlongwithInvoice
											--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
											, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
											, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount,IR.ToCurrencyRate
											, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
											, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
											,IR.AddedBy
	                                       ,PLC.LCANo PurchaseLC
									       ,Ctc.ContractNo ContructNumber
									       ,Par.UserName Customer
											,eI.EmployeeName CheckedBy
											, IR.CheckedByStatus AS CheckedByStatus
											,IR.AuthorizedByStatus AS AuthorizedByStatus
											,eI1.EmployeeName AuthorizedBy,isnull(PO.RequisitionId,'')  RequisitionId
									FROM[TRN].[PurchaseOrder] AS IR left JOIN[HKP].[Party] AS P ON IR.PartyId=P.Id
									LEFT JOIN (SELECT C.PartyId, C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
								    ON PAG.Id= C.PartyAccountGroupId WHERE C.PartyType= 'Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId= IR.PlantId
									 LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						            LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						            LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId 
                                    LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId= IR.CheckedBy
									LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId= IR.AuthorizedBy
									left JOIN[SCS].[Currency] AS CU ON IR.CurrencyId= CU.Id
									left JOIN[MST].[PaymentTerm] AS PT ON IR.PaymentTermId= PT.Id
									LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId= IPP.Id
									LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId= AM.Id
									LEFT JOIN [SCS].[State] AS S1 ON AM.StateId= S1.Id
									LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId= DPP.Id
									LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId= AM2.Id
									LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId= S2.Id
									LEFT JOIN [ORG].Plant PL ON PL.Id= IR.PlantId
									LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id= PL.AddressMasterId
									LEFT JOIN [SCS].[State] AS SP ON SP.Id= AMP.StateId
									LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
											JOIN[TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId= B.Id WHERE B.PlantId= '" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
									 LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN[TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId= B.Id
 
											 WHERE B.PlantId= '" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
									  LEFT JOIN[SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId= UoM.Id
                                      LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
									  WHERE IR.AuthorizedBy ='" + identity.EmployeeId + @"'
									  AND IR.CheckedByStatus ='Checked' 
									AND IR.AuthorizedByStatus = 'For Approval'
									  )x
									  Order by PODate ASC";
                }
                else
                {
                    Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,IR.AddedBy
	                                ,PLC.LCANo PurchaseLC
									,Ctc.ContractNo ContructNumber
									,Par.UserName Customer
                                    ,eI.EmployeeName CheckedBy
                                    ,IR.CheckedByStatus AS CheckedByStatus
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus
                                    ,eI1.EmployeeName AuthorizedBy
                                    ,IR.ApprovedHoldRejectReason,isnull(PO.RequisitionId,'')  RequisitionId

                        FROM [TRN].[PurchaseOrder] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                         LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId 
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
                        left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
		                            JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
                        WHERE  IR.AuthorizedBy ='" + identity.EmployeeId + @"' AND CheckedbyStatus = 'Checked' AND AuthorizedbyStatus = 'Hold' OR AuthorizedbyStatus = 'Reject' Order by IR.PODate ASC";//IR.PlantId='" + plantId + @"' and
                }

                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> getUNApprovalList(string plantId, string POTypeApprovalStatus)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var Sql = "";
            try
            {
                if (POTypeApprovalStatus == "ForApproval")
                {
                    Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
					select * from(SELECT ROW_NUMBER() OVER (ORDER BY  IR.Id) AS SiNo, IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106), ' ', '-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
											, CP.UserName AS PartyAccountGroupName
											, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106), ' ', '-') AS DocDate

											--, IR.GateEntryNo
											--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106), ' ', '-') AS EntryDate
											  , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
											, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
												, IR.FixedAssetOrInventory, IR.PODepended
												--, IR.AlongwithInvoice
												--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate
												  , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
											, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount,IR.ToCurrencyRate
											, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
											, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
											,IR.AddedBy
	                                       ,PLC.LCANo PurchaseLC
									       ,Ctc.ContractNo ContructNumber
									       ,Par.UserName Customer
											,eI.EmployeeName CheckedBy
											, IR.CheckedByStatus AS CheckedByStatus
											,IR.AuthorizedByStatus AS AuthorizedByStatus
											,eI1.EmployeeName AuthorizedBy,isnull(PO.RequisitionId,'')  RequisitionId
									FROM[TRN].[PurchaseOrder] AS IR left JOIN[HKP].[Party] AS P ON IR.PartyId = P.Id
									LEFT JOIN(SELECT C.PartyId, C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
											ON PAG.Id= C.PartyAccountGroupId WHERE C.PartyType= 'Vendor') AS CP ON CP.PartyId = IR.PartyId AND CP.PlantId = IR.PlantId
									LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						            LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						            LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId 
                                    LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId = IR.CheckedBy
									LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId = IR.AuthorizedBy
									left JOIN[SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id
									left JOIN[MST].[PaymentTerm] AS PT ON IR.PaymentTermId = PT.Id
									LEFT JOIN[HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
									LEFT JOIN[MST].[AddressMaster] AS AM ON IPP.AddressMasterId= AM.Id
									LEFT JOIN [SCS].[State] AS S1 ON AM.StateId= S1.Id
									LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId= DPP.Id
									LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId= AM2.Id
									LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId= S2.Id
									LEFT JOIN [ORG].Plant PL ON PL.Id= IR.PlantId
									LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id= PL.AddressMasterId
									LEFT JOIN [SCS].[State] AS SP ON SP.Id= AMP.StateId
									LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A

										   JOIN[TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId= B.Id  GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
									LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN[TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId= B.Id
											 GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
									 LEFT JOIN[SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId= UoM.Id
                                     LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
									 WHERE IR.AuthorizedBy ='" + identity.EmployeeId + @"'
									 AND IR.CheckedByStatus = 'Checked'
									 AND IR.AuthorizedByStatus = 'For Approval' --And IRD.TransactionQty > 0
									 UNION ALL
									 SELECT ROW_NUMBER() OVER (ORDER BY  IR.Id) AS SiNo, IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
											, CP.UserName AS PartyAccountGroupName
											, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
											--, IR.GateEntryNo
											--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
											, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
											, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
											  , IR.FixedAssetOrInventory, IR.PODepended
											--, IR.AlongwithInvoice
											--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
											, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
											, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount,IR.ToCurrencyRate
											, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
											, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
											,IR.AddedBy
	                                       ,PLC.LCANo PurchaseLC
									       ,Ctc.ContractNo ContructNumber
									       ,Par.UserName Customer
											,eI.EmployeeName CheckedBy
											, IR.CheckedByStatus AS CheckedByStatus
											,IR.AuthorizedByStatus AS AuthorizedByStatus
											,eI1.EmployeeName AuthorizedBy,isnull(PO.RequisitionId,'')  RequisitionId
									FROM[TRN].[PurchaseOrder] AS IR left JOIN[HKP].[Party] AS P ON IR.PartyId=P.Id
									LEFT JOIN (SELECT C.PartyId, C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
								    ON PAG.Id= C.PartyAccountGroupId WHERE C.PartyType= 'Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId= IR.PlantId
									 LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						            LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						            LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId 
                                    LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId= IR.CheckedBy
									LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId= IR.AuthorizedBy
									left JOIN[SCS].[Currency] AS CU ON IR.CurrencyId= CU.Id
									left JOIN[MST].[PaymentTerm] AS PT ON IR.PaymentTermId= PT.Id
									LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId= IPP.Id
									LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId= AM.Id
									LEFT JOIN [SCS].[State] AS S1 ON AM.StateId= S1.Id
									LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId= DPP.Id
									LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId= AM2.Id
									LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId= S2.Id
									LEFT JOIN [ORG].Plant PL ON PL.Id= IR.PlantId
									LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id= PL.AddressMasterId
									LEFT JOIN [SCS].[State] AS SP ON SP.Id= AMP.StateId
									LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
											JOIN[TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId= B.Id  GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
									 LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN[TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId= B.Id
 
											  GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
									  LEFT JOIN[SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId= UoM.Id
                                      LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
									  WHERE IR.AuthorizedBy ='" + identity.EmployeeId + @"'
									  AND IR.CheckedByStatus Is Null 
									AND IR.AuthorizedByStatus = 'For Approval' --And IRD.TransactionQty > 0
									  )x
									  Order by PODate ASC";
                }
                else if (POTypeApprovalStatus == "ApproveHoldReject")
                {
                    Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,IR.AddedBy
	                                ,PLC.LCANo PurchaseLC
									,Ctc.ContractNo ContructNumber
									,Par.UserName Customer
                                    ,eI.EmployeeName CheckedBy
                                    ,IR.CheckedByStatus AS CheckedByStatus
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus
                                    ,eI1.EmployeeName AuthorizedBy
                                    ,IR.ApprovedHoldRejectReason ApproveRejectReason
                                    ,isnull(PO.RequisitionId,'')  RequisitionId

                        FROM [TRN].[PurchaseOrder] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                         LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId 
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
                        left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
		                            JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id  GROUP BY A.InventoryReceiveId
                                   ) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
                        WHERE  IR.AuthorizedBy ='" + identity.EmployeeId + @"' AND CheckedbyStatus = 'Checked' AND AuthorizedbyStatus = 'Hold' OR AuthorizedbyStatus = 'Reject' Order by IR.PODate ASC";//IR.PlantId='" + plantId + @"' and
                }

                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetPOMasterById(string plantId, string id)
        {
            try
            {
                var Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
                        FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
		                            JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting'  AND IR.EmployeeId IS NULL AND IR.Id='" + id + "'";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public GridModel GetEmployeePurchaseList(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName, IR.EmployeeId, EI.EmployeeName, EI.EmployeeCode
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
                        FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting'  AND IR.EmployeeId<>''";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public Dictionary<string, object> GetExpenseBookingFile(string id)
        {
            try
            {
                var sql = @"Select Id, FileName From [TRN].[InventoryReceive]  Where Id='" + id + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public GridModel GetListForInvPayable(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, IR.InvoicingPartyPlantId AS PartyPlantId, P.Code AS PartyCode, P.UserName AS PartyName
			                    , CP.UserName AS PartyAccountGroupName
			                    , IR.EmployeeId, EI.EmployeeCode, EI.EmployeeName
	                            , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                            , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                            , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                            , IR.FixedAssetOrInventory, IR.PODepended
                                --, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                            , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                            , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount
                                , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, IR.IsTaxApplicable, IR.ToCurrencyRate
                    FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                    ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                    LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
                    JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                    JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                    LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                    LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                    LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                    LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                    LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                        JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                        WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                    WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.IsPaymentHold=0 AND CP.PlantId='" + plantId + @"' AND IR.FixedAssetOrInventory='Inventory' ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public decimal GetToCurrencyRate(string currencyId, string baseCurrencyId, DateTime docDate, string companyId)
        {
            try
            {
                decimal toCurrencyRate = 0;
                if (currencyId != baseCurrencyId)
                {
                    var sql = @"SELECT ISNULL((SELECT TOP(1) ISNULL(A.ToCurrencyBankSelling,0) FROM SCS.ExchangeRate AS A WHERE
                                            FromCurrencyCode='" + currencyId + "' AND ToCurrencyCode='" + baseCurrencyId + "' AND A.FromDate<='" + docDate + @"' AND A.CompanyId='" + companyId + "' ORDER BY CAST(FromDate AS DATE) DESC), 0)";
                    toCurrencyRate = _inventoryReceiveRepository.SqlQuery<decimal>(sql).First();
                }
                return toCurrencyRate;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                 ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public void Delete(string id)
        {
            try
            {
                var detail = Convert.ToBoolean(_inventoryReceiveRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM TRN.InventoryReceiveDetail WHERE InventoryReceiveId='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                var service = Convert.ToBoolean(_inventoryReceiveRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM TRN.InventoryService WHERE InventoryReceiveId='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                if (!detail && !service)
                {
                    var data = base.Find(id);
                    if (data.IsNull()) throw new CustomException(ServiceResources.RecordNoLonger);
                    base.Delete(data);
                }
                else throw new CustomException("Please delete first line item.");
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public void DeleteMaterialTax(string id)
        {
            try
            {
                string _sql = "delete from TRN.PurchaseOrderTax where id='" + id + "'";
                _sqlRepository.ExecuteSqlCommand(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        #endregion InventoryReceive

        #region Tax

        public IEnumerable<object> GetTaxCategoryList(string companyGroupId, string receiveId, string plantId, string hsnCodeId, string PODate)
        {
            try
            {
                var sql = @"DECLARE @receiveId varchar(10)='" + receiveId + @"'
                                  , @partyState varchar(30)
                                  , @partyCountry varchar(10)
                                  , @plantState varchar(30)
                                  , @plantCountry varchar(10)
                                  , @plantId varchar(30)='" + plantId + @"'
                                  , @hsnCodeId varchar(30)='" + hsnCodeId + @"'
                    SET @partyCountry =(SELECT AM.CountryId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id
                                                    JOIN TRN.PurchaseOrder AS IR ON IR.InvoicingPartyPlantId=PP.Id WHERE IR.Id=@receiveId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @partyState =(SELECT AM.StateId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id
                                    JOIN TRN.PurchaseOrder AS IR ON IR.InvoicingPartyPlantId=PP.Id WHERE IR.Id=@receiveId)-- AND AD.Active=1 AND AD.Archive=0)

                    SET @plantState =(SELECT AD.StateId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @plantCountry =(SELECT AD.CountryId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SELECT TVD.Id, TVD.TaxCategoryId, HP.HSNCodeId, HN.Code AS HSNCode, TC.UserName, HP.[Percentage] AS [Percentage], NULL TotalAmount
                    FROM [MST].[TaxVariantDetail] AS TVD
                    JOIN [MST].[TaxVariant] AS TV ON TVD.TaxVariantId=TV.Id
                    JOIN [MST].[TaxCategory] AS TC ON TVD.TaxCategoryId=TC.Id
                    --LEFT JOIN (SELECT * FROM [MST].[HSNTaxPercentage] WHERE HSNCodeId=@hsnCodeId) AS HP ON HP.TaxCategoryId=TC.Id
					LEFT JOIN (SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY TaxCategoryId, HSNCodeId ORDER BY EffectiveDate DESC) AS RN
								FROM [MST].[HSNTaxPercentage] WHERE CountryId=@plantCountry AND HSNCodeId=@hsnCodeId AND convert(DATE, EffectiveDate)<='" + PODate + @"') AS TBL WHERE RN=1) AS HP ON HP.TaxCategoryId=TC.Id

                    LEFT JOIN [HKP].[HSNCode] AS HN ON HP.HSNCodeId=HN.Id
                    WHERE TV.CompanyGroupId='" + companyGroupId + @"' AND TV.CountryId=@plantCountry --AND HP.HSNCodeId=@hsnCodeId
                    AND TV.TaxFor=CASE WHEN @partyCountry=@plantCountry THEN '" + TaxFor.DomesticPurchase + @"'
				                        WHEN @partyCountry<>@plantCountry THEN '" + TaxFor.OverseasPurchase + @"' END
                    AND (TV.Different=CASE WHEN @partyCountry=@plantCountry AND @partyState=@plantState AND TV.DifferentIn='State' THEN 'Same'
					                       WHEN @partyCountry=@plantCountry AND @partyState<>@plantState AND TV.DifferentIn='State' THEN 'Different' END
	                    OR TV.Different IS NULL)
                    ORDER BY TC.[Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> getserviceTaxByTaxCategoryList(string companyGroupId, string receiveId, string plantId, string hsnCodeId, string PODate)
        {
            try
            {
                var sql = @"DECLARE @receiveId varchar(10)='" + receiveId + @"'
                                  , @partyState varchar(30)
                                  , @partyCountry varchar(10)
                                  , @plantState varchar(30)
                                  , @plantCountry varchar(10)
                                  , @plantId varchar(30)='" + plantId + @"'
                                  , @hsnCodeId varchar(30)='" + hsnCodeId + @"'
                    SET @partyCountry =(SELECT AM.CountryId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id
                                                    JOIN TRN.ServicePOMaster AS IR ON IR.InvoicingPartyPlantId=PP.Id WHERE IR.Id=@receiveId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @partyState =(SELECT AM.StateId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id
                                    JOIN TRN.ServicePOMaster AS IR ON IR.InvoicingPartyPlantId=PP.Id WHERE IR.Id=@receiveId)-- AND AD.Active=1 AND AD.Archive=0)

                    SET @plantState =(SELECT AD.StateId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @plantCountry =(SELECT AD.CountryId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SELECT TVD.Id, TVD.TaxCategoryId, HP.HSNCodeId, HN.Code AS HSNCode, TC.UserName, HP.[Percentage] AS [Percentage], NULL TotalAmount,'' ServiceMasterId
                    FROM [MST].[TaxVariantDetail] AS TVD
                    JOIN [MST].[TaxVariant] AS TV ON TVD.TaxVariantId=TV.Id
                    JOIN [MST].[TaxCategory] AS TC ON TVD.TaxCategoryId=TC.Id
                    --LEFT JOIN (SELECT * FROM [MST].[HSNTaxPercentage] WHERE HSNCodeId=@hsnCodeId) AS HP ON HP.TaxCategoryId=TC.Id
					LEFT JOIN (SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY TaxCategoryId, HSNCodeId ORDER BY EffectiveDate DESC) AS RN
								FROM [MST].[HSNTaxPercentage] WHERE CountryId=@plantCountry AND HSNCodeId=@hsnCodeId AND convert(DATE, EffectiveDate)<='" + PODate + @"') AS TBL WHERE RN=1) AS HP ON HP.TaxCategoryId=TC.Id

                    LEFT JOIN [HKP].[HSNCode] AS HN ON HP.HSNCodeId=HN.Id
                    WHERE TV.CompanyGroupId='" + companyGroupId + @"' AND TV.CountryId=@plantCountry --AND HP.HSNCodeId=@hsnCodeId
                    AND TV.TaxFor=CASE WHEN @partyCountry=@plantCountry THEN '" + TaxFor.DomesticPurchase + @"'
				                        WHEN @partyCountry<>@plantCountry THEN '" + TaxFor.OverseasPurchase + @"' END
                    AND (TV.Different=CASE WHEN @partyCountry=@plantCountry AND @partyState=@plantState AND TV.DifferentIn='State' THEN 'Same'
					                       WHEN @partyCountry=@plantCountry AND @partyState<>@plantState AND TV.DifferentIn='State' THEN 'Different' END
	                    OR TV.Different IS NULL)
                    ORDER BY TC.[Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetJWServiceTaxCategoryList(string companyGroupId, string receiveId, string plantId, string hsnCodeId, string PODate)
        {
            try
            {
                var sql = @"DECLARE @receiveId varchar(10)='" + receiveId + @"'
                                  , @partyState varchar(30)
                                  , @partyCountry varchar(10)
                                  , @plantState varchar(30)
                                  , @plantCountry varchar(10)
                                  , @plantId varchar(30)='" + plantId + @"'
                                  , @hsnCodeId varchar(30)='" + hsnCodeId + @"'
                    SET @partyCountry =(SELECT AM.CountryId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id
                                                    JOIN OSTransformationPO AS IR ON IR.InvoicingPartyPlantId=PP.Id WHERE IR.Id=@receiveId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @partyState =(SELECT AM.StateId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id
                                    JOIN OSTransformationPO AS IR ON IR.InvoicingPartyPlantId=PP.Id WHERE IR.Id=@receiveId)-- AND AD.Active=1 AND AD.Archive=0)

                    SET @plantState =(SELECT AD.StateId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @plantCountry =(SELECT AD.CountryId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SELECT TVD.Id, TVD.TaxCategoryId, HP.HSNCodeId, HN.Code AS HSNCode, TC.UserName, HP.[Percentage] AS [Percentage], NULL TotalAmount
                    FROM [MST].[TaxVariantDetail] AS TVD
                    JOIN [MST].[TaxVariant] AS TV ON TVD.TaxVariantId=TV.Id
                    JOIN [MST].[TaxCategory] AS TC ON TVD.TaxCategoryId=TC.Id
                    --LEFT JOIN (SELECT * FROM [MST].[HSNTaxPercentage] WHERE HSNCodeId=@hsnCodeId) AS HP ON HP.TaxCategoryId=TC.Id
					LEFT JOIN (SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY TaxCategoryId, HSNCodeId ORDER BY EffectiveDate DESC) AS RN
								FROM [MST].[HSNTaxPercentage] WHERE CountryId=@plantCountry AND HSNCodeId=@hsnCodeId AND convert(DATE, EffectiveDate)<='" + PODate + @"') AS TBL WHERE RN=1) AS HP ON HP.TaxCategoryId=TC.Id

                    LEFT JOIN [HKP].[HSNCode] AS HN ON HP.HSNCodeId=HN.Id
                    WHERE TV.CompanyGroupId='" + companyGroupId + @"' AND TV.CountryId=@plantCountry --AND HP.HSNCodeId=@hsnCodeId
                    AND TV.TaxFor=CASE WHEN @partyCountry=@plantCountry THEN '" + TaxFor.DomesticPurchase + @"'
				                        WHEN @partyCountry<>@plantCountry THEN '" + TaxFor.OverseasPurchase + @"' END
                    AND (TV.Different=CASE WHEN @partyCountry=@plantCountry AND @partyState=@plantState AND TV.DifferentIn='State' THEN 'Same'
					                       WHEN @partyCountry=@plantCountry AND @partyState<>@plantState AND TV.DifferentIn='State' THEN 'Different' END
	                    OR TV.Different IS NULL)
                    ORDER BY TC.[Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetTaxCategoryListForSalesService(string companyGroupId, string receiveId, string plantId, string hsnCodeId, string InventorySalesDate)
        {
            try
            {
                var sql = @"DECLARE @receiveId varchar(10)='" + receiveId + @"'
                                  , @partyState varchar(30)
                                  , @partyCountry varchar(10)
                                  , @plantState varchar(30)
                                  , @plantCountry varchar(10)
                                  , @plantId varchar(30)='" + plantId + @"'
                                  , @hsnCodeId varchar(30)='" + hsnCodeId + @"'
                    SET @partyCountry =(SELECT AM.CountryId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id
                                                    JOIN TRN.InventorySales AS IR ON IR.InvoicingPartyPlantId=PP.Id WHERE IR.Id=@receiveId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @partyState =(SELECT AM.StateId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id
                                    JOIN TRN.InventorySales AS IR ON IR.InvoicingPartyPlantId=PP.Id WHERE IR.Id=@receiveId)-- AND AD.Active=1 AND AD.Archive=0)

                    SET @plantState =(SELECT AD.StateId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @plantCountry =(SELECT AD.CountryId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SELECT TVD.Id, TVD.TaxCategoryId, HP.HSNCodeId, HN.Code AS HSNCode, TC.UserName, ISNULL(HP.[Percentage], 0) AS [Percentage], NULL TotalAmount
                    FROM [MST].[TaxVariantDetail] AS TVD
                    JOIN [MST].[TaxVariant] AS TV ON TVD.TaxVariantId=TV.Id
                    JOIN [MST].[TaxCategory] AS TC ON TVD.TaxCategoryId=TC.Id
                    --LEFT JOIN (SELECT * FROM [MST].[HSNTaxPercentage] WHERE HSNCodeId=@hsnCodeId) AS HP ON HP.TaxCategoryId=TC.Id
					LEFT JOIN (SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY TaxCategoryId, HSNCodeId ORDER BY EffectiveDate DESC) AS RN
								FROM [MST].[HSNTaxPercentage] WHERE CountryId=@plantCountry AND HSNCodeId=@hsnCodeId) AS TBL WHERE RN=1 AND EffectiveDate<='" + InventorySalesDate + @"') AS HP ON HP.TaxCategoryId=TC.Id

                    LEFT JOIN [HKP].[HSNCode] AS HN ON HP.HSNCodeId=HN.Id
                    WHERE TV.CompanyGroupId='" + companyGroupId + @"' AND TV.CountryId=@plantCountry --AND HP.HSNCodeId=@hsnCodeId
                    AND TV.TaxFor=CASE WHEN @partyCountry=@plantCountry THEN '" + TaxFor.DomesticPurchase + @"'
				                        WHEN @partyCountry<>@plantCountry THEN '" + TaxFor.OverseasPurchase + @"' END
                    AND (TV.Different=CASE WHEN @partyCountry=@plantCountry AND @partyState=@plantState AND TV.DifferentIn='State' THEN 'Same'
					                       WHEN @partyCountry=@plantCountry AND @partyState<>@plantState AND TV.DifferentIn='State' THEN 'Different' END
	                    OR TV.Different IS NULL)
                    ORDER BY TC.[Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }






        public IEnumerable<PurchaseOrderTax> GetTaxCategoryList1(string companyGroupId, string receiveId, string plantId, string hsnCodeId)
        {
            try
            {
                var sql = @"DECLARE @receiveId varchar(10)='" + receiveId + @"'
                                  , @partyState varchar(30)
                                  , @partyCountry varchar(10)
                                  , @plantState varchar(30)
                                  , @plantCountry varchar(10)
                                  , @plantId varchar(30)='" + plantId + @"'
                                  , @hsnCodeId varchar(30)='" + hsnCodeId + @"'
                    SET @partyCountry =(SELECT AM.CountryId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id
                                                    JOIN TRN.PurchaseOrder AS IR ON IR.InvoicingPartyPlantId=PP.Id WHERE IR.Id=@receiveId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @partyState =(SELECT AM.StateId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id
                                    JOIN TRN.PurchaseOrder AS IR ON IR.InvoicingPartyPlantId=PP.Id WHERE IR.Id=@receiveId)-- AND AD.Active=1 AND AD.Archive=0)

                    SET @plantState =(SELECT AD.StateId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @plantCountry =(SELECT AD.CountryId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SELECT TVD.Id, TVD.TaxCategoryId, HP.HSNCodeId, HN.Code AS HSNCode, TC.UserName, ISNULL(HP.[Percentage], 0) AS [Percentage], NULL TotalAmount
                    FROM [MST].[TaxVariantDetail] AS TVD
                    JOIN [MST].[TaxVariant] AS TV ON TVD.TaxVariantId=TV.Id
                    JOIN [MST].[TaxCategory] AS TC ON TVD.TaxCategoryId=TC.Id
                    --LEFT JOIN (SELECT * FROM [MST].[HSNTaxPercentage] WHERE HSNCodeId=@hsnCodeId) AS HP ON HP.TaxCategoryId=TC.Id
					LEFT JOIN (SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY TaxCategoryId, HSNCodeId ORDER BY EffectiveDate DESC) AS RN
								FROM [MST].[HSNTaxPercentage] WHERE CountryId=@plantCountry AND HSNCodeId=@hsnCodeId) AS TBL WHERE RN=1) AS HP ON HP.TaxCategoryId=TC.Id

                    LEFT JOIN [HKP].[HSNCode] AS HN ON HP.HSNCodeId=HN.Id
                    WHERE TV.CompanyGroupId='" + companyGroupId + @"' AND TV.CountryId=@plantCountry --AND HP.HSNCodeId=@hsnCodeId
                    AND TV.TaxFor=CASE WHEN @partyCountry=@plantCountry THEN '" + TaxFor.DomesticPurchase + @"'
				                        WHEN @partyCountry<>@plantCountry THEN '" + TaxFor.OverseasPurchase + @"' END
                    AND (TV.Different=CASE WHEN @partyCountry=@plantCountry AND @partyState=@plantState AND TV.DifferentIn='State' THEN 'Same'
					                       WHEN @partyCountry=@plantCountry AND @partyState<>@plantState AND TV.DifferentIn='State' THEN 'Different' END
	                    OR TV.Different IS NULL)
                    ORDER BY TC.[Sequence]";
                return _sqlRepository.GetModelCollection<PurchaseOrderTax>(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetTaxCategoryListServiceAcknowledgement(string companyGroupId, string serviceId, string plantId, string hsnCodeId)
        {
            try
            {
                var sql = @"DECLARE @receiveId varchar(10)='" + serviceId + @"'
                                  , @partyState varchar(30)
                                  , @partyCountry varchar(10)
                                  , @plantState varchar(30)
                                  , @plantCountry varchar(10)
                                  , @plantId varchar(30)='" + plantId + @"'
                                  , @hsnCodeId varchar(30)='" + hsnCodeId + @"'
                    SET @partyCountry =(SELECT AM.CountryId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id
                                                    JOIN [TRN].[ServiceAcknowledgementMaster] AS IR ON IR.InvoicingPartyPlantId=PP.Id WHERE IR.Id=@receiveId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @partyState =(SELECT AM.StateId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id
                                    JOIN [TRN].[ServiceAcknowledgementMaster] AS IR ON IR.InvoicingPartyPlantId=PP.Id WHERE IR.Id=@receiveId)-- AND AD.Active=1 AND AD.Archive=0)

                    SET @plantState =(SELECT AD.StateId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @plantCountry =(SELECT AD.CountryId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SELECT TVD.Id, TVD.TaxCategoryId, HP.HSNCodeId, HN.Code AS HSNCode, TC.UserName, ISNULL(HP.[Percentage], 0) AS [Percentage], NULL TotalAmount
                    FROM [MST].[TaxVariantDetail] AS TVD
                    JOIN [MST].[TaxVariant] AS TV ON TVD.TaxVariantId=TV.Id
                    JOIN [MST].[TaxCategory] AS TC ON TVD.TaxCategoryId=TC.Id
                    --LEFT JOIN (SELECT * FROM [MST].[HSNTaxPercentage] WHERE HSNCodeId=@hsnCodeId) AS HP ON HP.TaxCategoryId=TC.Id
					LEFT JOIN (SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY TaxCategoryId, HSNCodeId ORDER BY EffectiveDate DESC) AS RN
								FROM [MST].[HSNTaxPercentage] WHERE CountryId=@plantCountry AND HSNCodeId=@hsnCodeId) AS TBL WHERE RN=1) AS HP ON HP.TaxCategoryId=TC.Id

                    LEFT JOIN [HKP].[HSNCode] AS HN ON HP.HSNCodeId=HN.Id
                    WHERE TV.CompanyGroupId='" + companyGroupId + @"' AND TV.CountryId=@plantCountry --AND HP.HSNCodeId=@hsnCodeId
                    AND TV.TaxFor=CASE WHEN @partyCountry=@plantCountry THEN '" + TaxFor.DomesticPurchase + @"'
				                        WHEN @partyCountry<>@plantCountry THEN '" + TaxFor.OverseasPurchase + @"' END
                    AND (TV.Different=CASE WHEN @partyCountry=@plantCountry AND @partyState=@plantState AND TV.DifferentIn='State' THEN 'Same'
					                       WHEN @partyCountry=@plantCountry AND @partyState<>@plantState AND TV.DifferentIn='State' THEN 'Different' END
	                    OR TV.Different IS NULL)
                    ORDER BY TC.[Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetReceiveTaxList(string receiveDetailId)
        {
            try
            {
                var sql = @"SELECT A.Id,A.InventoryReceiveId, A.TaxCategoryId, TC.UserName AS TaxCategory, A.HSNCodeId, HN.Code AS HSNCode, A.[Percentage], A.TaxAmount,d.id As PODetailId
                            FROM [TRN].[PurchaseOrderTax] AS A JOIN [MST].[TaxCategory] AS TC ON A.TaxCategoryId=TC.Id
                            LEFT JOIN [HKP].[HSNCode] AS HN ON A.HSNCodeId=HN.Id
							left join TRN.PurchaseOrderDetail d on d.id= A.InventoryReceiveDetailId
                            WHERE A.InventoryReceiveId='" + receiveDetailId + "' AND A.InventoryServiceId IS NULL ORDER BY TC.[Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetTotalReceiveTaxList(string receiveId)
        {
            try
            {
                var sql = @"SELECT A.Id,A.InventoryReceiveId, A.TaxCategoryId, TC.UserName AS TaxCategory, A.HSNCodeId, HN.Code AS HSNCode, A.[Percentage], A.TaxAmount,d.id As PODetailId
                            FROM [TRN].[PurchaseOrderTax] AS A JOIN [MST].[TaxCategory] AS TC ON A.TaxCategoryId=TC.Id
                            LEFT JOIN [HKP].[HSNCode] AS HN ON A.HSNCodeId=HN.Id
							left join TRN.PurchaseOrderDetail d on d.id= A.InventoryReceiveDetailId
                            WHERE A.InventoryReceiveId=201979 AND A.InventoryServiceId IS NULL ORDER BY TC.[Sequence]
                            WHERE A.InventoryReceiveId='" + receiveId + "' AND A.InventoryServiceId IS NULL GROUP BY A.TaxCategoryId, TC.UserName, TC.[Sequence] ORDER BY TC.[Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetServiceTaxList(string serviceId)
        {
            try
            {
                var sql = @"SELECT A.Id,A.InventoryServiceId, A.TaxCategoryId, TC.UserName AS TaxCategory, A.HSNCodeId, HN.Code AS HSNCode, A.[Percentage], A.TaxAmount
                            FROM [TRN].[PurchaseOrderTax] AS A JOIN [MST].[TaxCategory] AS TC ON A.TaxCategoryId=TC.Id
                            LEFT JOIN [HKP].[HSNCode] AS HN ON A.HSNCodeId=HN.Id
                            WHERE A.InventoryReceiveId='" + serviceId + "' AND A.InventoryReceiveDetailId IS NULL ORDER BY TC.[Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetServiceTaxListForTax(string serviceId)
        {
            try
            {
                var sql = @"SELECT A.Id,A.ServiceAcknowledgementChargeId, A.TaxCategoryId, TC.UserName, A.HSNCodeId, HN.Code AS HSNCode, A.[Percentage], A.TaxAmount
                            FROM [TRN].ServicePOAckTax AS A 
                            JOIN [MST].[TaxCategory] AS TC ON A.TaxCategoryId=TC.Id
                            LEFT JOIN [HKP].[HSNCode] AS HN ON A.HSNCodeId=HN.Id
                            WHERE A.ServiceAcknowledgementChargeId='" + serviceId + @"' AND A.ServiceAcknowledgementDetailId IS NULL ORDER BY TC.[Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        #endregion Tax

        #region PurchaseOrder Service

        #endregion
        public decimal GetChargesRatio(string receiveId, string detailId, decimal detailTotalAmnt, string serviceId, decimal svcTotalAmnt, bool isNonCreditable)
        {
            try
            {
                decimal svcAmount = 0;
                if (isNonCreditable)
                    svcAmount = _inventoryReceiveRepository.SqlQuery<decimal>("SELECT ISNULL(SUM(Amount), 0)+ISNULL(SUM(TotalTaxAmount), 0) FROM TRN.POService WHERE InventoryReceiveId='" + receiveId + "' AND ISNULL(Id, '')<>'" + serviceId + "'").First();
                else
                    svcAmount = _inventoryReceiveRepository.SqlQuery<decimal>("SELECT ISNULL(SUM(Amount), 0) FROM TRN.POService WHERE InventoryReceiveId='" + receiveId + "' AND ISNULL(Id, '')<>'" + serviceId + "'").First();
                if (svcTotalAmnt > 0) svcAmount += svcTotalAmnt;
                else svcAmount -= svcTotalAmnt;

                var detailAmount = _inventoryReceiveRepository.SqlQuery<decimal>("SELECT ISNULL(SUM(TransactionAmount), 1) FROM TRN.PurchaseOrderDetail WHERE InventoryReceiveId='" + receiveId + "' AND ISNULL(Id, '')<>'" + detailId + "'").First();
                if (detailTotalAmnt > 0) detailAmount += detailTotalAmnt;
                else detailAmount -= detailTotalAmnt;
                if (detailAmount == 0) detailAmount = 1;
                return svcAmount == 0 && detailAmount == 0 ? 0 : svcAmount / detailAmount;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                 ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public void GRNApproved(IEnumerable<PurchaseOrder> entities)
        {
            var flag = false;
            try
            {
                if (entities.IsNull()) throw new CustomException("Select GRN");
                var ids = entities.Select(t => t.Id).ToArray();
                var dbList = base.Query(t => ids.Contains(t.Id)).Select().ToList();
                _unitOfWork.BeginTransaction();
                flag = true;
                foreach (var entity in entities)
                {
                    foreach (var item in dbList)
                    {
                        if (entity.Id == item.Id)
                        {
                            item.IsApproved = entity.IsApproved;
                            base.UpdateGraph(item);
                        }
                    }
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }
        public void PaymentHold(IEnumerable<PurchaseOrder> entities)
        {
            var flag = false;
            try
            {
                if (entities.IsNull()) throw new CustomException("Select GRN");
                var ids = entities.Select(t => t.Id).ToArray();
                var dbList = base.Query(t => ids.Contains(t.Id)).Select().ToList();
                _unitOfWork.BeginTransaction();
                flag = true;
                foreach (var entity in entities)
                {
                    foreach (var item in dbList)
                    {
                        if (entity.Id == item.Id)
                        {
                            item.IsPaymentHold = entity.IsPaymentHold;
                            base.UpdateGraph(item);
                        }
                    }
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }
        public IEnumerable<object> GetListByParty(string CompanyId, string AccountType)
        {
            try
            {
                var sql = @"SELECT P.Id AS PartyId, P.Id, P.Code
                        ,P.UserName,P.Code +' '+ P.UserName as  PUserName
                        , P.PartyType, PAG.UserName AS PartyAccountGroup
                        , CO.UserName AS Country, S.UserName AS States,C.Code AS Currency,PP.UserName AS PartyPlant
                        , NULL AS InvoicingPartyPlantId, NULL AS DeliveryPartyPlantId, NULL AS GSTIN
                        FROM [HKP].[Party] AS P
                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.PartyId=P.Id
                        LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=P.Id
                        LEFT JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=CP.PartyAccountGroupId
                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=CP.CurrencyId
                        LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=P.AddressMasterId
                        LEFT JOIN [SCS].[Country] AS CO ON CO.Id=AM.CountryId
                        LEFT JOIN [SCS].[State] AS S ON S.Id=AM.StateId
                        where CP.CompanyId='" + CompanyId + "' and PAG.AccountType='" + AccountType + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetPartyPlantCbo(string partyId, string Id)
        {
            try
            {
                var sql = @"SELECT PP.Id AS [Value], PP.UserName AS [Text], PP.Id AS PartyPlantId, PP.UserName AS PartyPlantName, PP.PartyId, PP.IsDefault, PP.AddressMasterId, PP.GSTIN
                        --, ISNULL(P.InvoicingByAddress, AM.Address1) AS Address1
                        --,ISNULL(P.DeliveryByAddress, AM.Address2) AS Address2, AM.CountryId
                        ,isnull(P.InvoicingByAddress,'')+ (isnull(AM.Address1,'') + isnull(AM.Address2,'') + Isnull(AM.Address3,'')) AS Address1
	                   ,ISNULL(P.DeliveryByAddress,'')+ (isnull(AM.Address1,'')+isnull(AM.Address2,'')+Isnull(AM.Address3,'')) AS Address2	
                       ,CO.Code AS CountryCode, CO.UserName AS CountryName, AM.StateId, S.Code AS StateCode, S.UserName AS StateName, AM.CityId, C.Code AS CityCode, C.UserName AS CityName
					   --,AM.Address1
                        FROM [HKP].[PartyPlant] AS PP
                        LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PP.AddressMasterId
                        LEFT JOIN [SCS].[Country] AS CO ON CO.Id=AM.CountryId
                        LEFT JOIN [SCS].[State] AS S ON S.Id=AM.StateId
                        LEFT JOIN [SCS].[City] AS C ON C.Id=AM.CityId
                        LEFT JOIN [TRN].PurchaseOrder p ON p.PartyId=PP.PartyId AND P.Id='" + Id + @"'
                        WHERE PP.PartyId='" + partyId + "' ORDER BY 2";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetStateByInvoicingPartyPlantId(string InvoicingPartyPlantId)
        {
            try
            {
                var sql = @"select distinct d.StandardName from trn.PurchaseOrder a
                            INNER join [HKP].[PartyPlant]  b on a.InvoicingPartyPlantId=b.Id 

                            INNER JOIn [MST].[AddressMaster] c on c.id=b.AddressMasterId
                            INNER join [SCS].[State] d on c.StateId=d.id
                            where a.InvoicingPartyPlantId='" + InvoicingPartyPlantId + "'";


                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        #region Taufik PurchaseOrderReport 
        public void GePurchaseOrderReport(string companyGroupId, string companyId, string plantId, string userId, string purchaseOrderId)
        {
            ReportUtility ru = new ReportUtility();
            var fileName = "";
            var strPath = "";
            var File = "";
            fileName = "PurchaseOrder" + plantId + ".docx";
            strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }

            ////A opens input document.
            WordDocument document = new WordDocument(File, FormatType.Docx);

            //Gets the paragraph at index 1
            try
            {
                string invoicePartyAddress = "";
                string vendorPartyAddress = "";
                WSection section = document.Sections[0];
                //var DiscountAmount = "";

                DataTable dsOrderMaster, dsServiceItems, dsTermsAndCondition;
                dsOrderMaster = loadOrderMaster(purchaseOrderId);//sql
                dsTermsAndCondition = TermsAndConditionSQL(purchaseOrderId);

                Dictionary<string, string> columns = new Dictionary<string, string>();
                var poApprovedStatus = "";
                invoicePartyAddress = ru.GetAddress(dsOrderMaster.Rows[0]["InvoicePartyAddressMasterId"].ToString(), dsOrderMaster.Rows[0]["InvoicingByAddress"].ToString());
                document.Replace("{InvoicingPartyAddress}", invoicePartyAddress, false, false);
                vendorPartyAddress = ru.GetAddress(dsOrderMaster.Rows[0]["VendorAddressMasterId"].ToString(), "");
                document.Replace("{VendorAddress}", vendorPartyAddress, false, false);
                document.Replace("{DeliveryInstruction}", dsOrderMaster.Rows[0]["DeliveryInstruction"].ToString(), false, false);
                document.Replace("{SpecialInstruction}", dsOrderMaster.Rows[0]["SpecialInstruction"].ToString(), false, false);
                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);


                dsServiceItems = loadServicerMasterItems(purchaseOrderId);
                var materialTotal = makeMaterialDetailsTable(document, dsOrderMaster, purchaseOrderId);//Material Details 
                var TermsAndCondition = makeTermsAndCondition(purchaseOrderId, document, dsTermsAndCondition);//Terms And Conditions


                var serviceTotal = 0.00;
                if (dsServiceItems.Rows.Count > 0)
                {
                    //{ServiceItems}
                    serviceTotal = makeServiceDetailsTable(document, dsServiceItems, purchaseOrderId);//Service Details 
                    document.Replace("{ServiceDetails}", "Service Details", true, true);
                }
                var DiscountAmount = "";
                DiscountAmount = dsOrderMaster.Rows[0]["DiscountAmount"].ToString();
                document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{DiscountAmount}", (DiscountAmount).ToString() + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{AfterDiscountTotal}", ((clsStaticInfo.dbl(materialTotal.ToString()) + clsStaticInfo.dbl(serviceTotal.ToString())) - clsStaticInfo.dbl(DiscountAmount.ToString())).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{TotalInWords}", ru.InWord(((clsStaticInfo.dbl(materialTotal.ToString()) + clsStaticInfo.dbl(serviceTotal.ToString())) - clsStaticInfo.dbl(DiscountAmount.ToString())), dsOrderMaster.Rows[0]["CurrencyId"].ToString()), true, true);

                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();
                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                //creating secondary array to prevent memory leak and accidental over-writing (Tarek Talukder-26-May-2019)
                List<string> strReplace = new List<string>();
                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());
                StringCollection strColDistinct = new StringCollection();
                for (int i = 0; i < strReplace.Count; i++)
                {
                    if (strColDistinct.Contains(strReplace[i].ToUpper()))
                        continue;

                    strColDistinct.Add(strReplace[i].ToUpper());

                    string text = strReplace[i].ToUpper();
                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        ReplaceInfo[text] = document.Replace(text, dsOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }

                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);
                //removing any unused place holder
                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "", false, false);

                }
                DocToPDFConverter converter = new DocToPDFConverter();
                //Converts Word document into PDF document
                //Syncfusion.Pdf.PdfDocument pdfDocument = converter.ConvertToPDF(document);
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                pdfDocument.PageSettings.Width = 1200;
                pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();
                //Closes the instance of document objects
                document.Close();
                string Prefix = "PurchaseOrder" + purchaseOrderId;
                //Saves the PDF file 
                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);
                document.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //Closes the instance of document objects
            document.Close();
        }
        public DataTable loadOrderMasterItems(string OrderMasterID)
        {
            string strSQL;
            //clsConnection objCon;
            try
            {
                strSQL = @"select so.MasterOrderItemId,so.Id AS SOID,CONCAT( mm.[Description],' ',a.StandardName) AS MaterialDesc,
                                so.Qty,uom.UserName AS UOM,SO.Rate,so.Qty*so.Rate AS Amount,isnull(SO.Discount,0) AS Discount
                                  from [TRN].[MasterOrderItem] T
                                INNER JOIN [TRN].[MasterOrder] O ON o.Id=t.MasterOrderId
                                INNER JOIN [TRN].[SalesOrder]  SO ON so.MasterOrderItemId=t.Id
                                LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=t.MaterialMasterId
                                LEFT OUTER JOIN mst.MaterialGroupMaster AS mgm ON mgm.Id=mm.MaterialGroupMasterId
                                LEFT OUTER JOIN [MST].[MaterialMasterArticle] A ON a.Id=t.ArticleId
                                LEFT OUTER JOIN [SCS].[UnitOfMeasurement] UOM ON uom.Id=o.TotalQtyUOMId
                                where MasterOrderId='" + OrderMasterID + "'";
                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        public DataTable loadMaterialTax(string purchaseOrderId)
        {
            string strSQL;
            try
            {
                strSQL = @"select InventoryServiceId,PO.Id PurchaseOrderId,POD.Id PurchaseOrderDetailId,tg.Code AS TaxCode,PODT.Percentage, PODT.TaxAmount from TRN.PurchaseOrder PO
                            INNER JOIN TRN.PurchaseOrderDetail POD ON POD.InventoryReceiveId = PO.Id
                            Inner join TRN.PurchaseOrderTax PODT ON PODT.InventoryReceiveId = PO.Id and PODT.InventoryReceiveDetailId = POD.Id
                            LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=PODT.TaxCategoryId
                            WHERE PO.Id='" + purchaseOrderId + @"' 
							and InventoryReceiveDetailId  is not null and  InventoryServiceId is null AND PODT.Percentage > 0 
							ORDER BY tg.[Sequence] ";
                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }
        public double makeMaterialDetailsTable(WordDocument document, DataTable dsOrderMaster, string purchaseOrderId)
        {
            string replaceString = "{materialItems}";
            ReportUtility ru = new ReportUtility();
            DataTable dsOrderItems, dsTax;
            //clsDataContext data = new clsDataContext();
            dsTax = loadMaterialTax(purchaseOrderId);
            int LasColumnIndex = 14;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));
            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    LasColumnIndex++;
                }
            }
            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);
            WTableRow TemplateRow = wTable.Rows[0].Clone();
            #region column headers
            document.EnsureMinimal();
            //wTable.Title = "Material Details";
            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SL");
            range.ApplyCharacterFormat(FontBold);
            int colRo = COL; COL++;
            wTable.Rows[ROW].Cells[colRo].Width = 30;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Material");
            range.ApplyCharacterFormat(FontBold);
            int colMaterialGroup = COL; COL++;
            wTable.Rows[ROW].Cells[colMaterialGroup].Width = 80;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            wTable.Rows[ROW].Cells[colArticle].Width = 75;



            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU1");
            range.ApplyCharacterFormat(FontBold);
            int colChar1 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar1].Width = 35;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU2");
            range.ApplyCharacterFormat(FontBold);
            int colChar2 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar2].Width = 35;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU3");
            range.ApplyCharacterFormat(FontBold);
            int colChar3 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar3].Width = 35;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("HSN No");
            //range.ApplyCharacterFormat(FontBold);
            //int colHSNCode = COL; COL++;
            //wTable.Rows[ROW].Cells[colChar3].Width = 40;



            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Material Description");
            range.ApplyCharacterFormat(FontBold);
            int colMatDescription = COL; COL++;
            wTable.Rows[ROW].Cells[colMatDescription].Width = 55;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Description");
            range.ApplyCharacterFormat(FontBold);
            int colDescription = COL; COL++;
            wTable.Rows[ROW].Cells[colDescription].Width = 55;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Reff No");
            range.ApplyCharacterFormat(FontBold);
            int colRefferenceNo = COL; COL++;
            //wTable.Rows[ROW].Cells[colRefferenceNo].Width = 30;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Delivery Date");
            range.ApplyCharacterFormat(FontBold);
            int colDeliveryDate = COL; COL++;
            //wTable.Rows[ROW].Cells[colDeliveryDate].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Origin");//TRN.PurchaseOrderDetail ->CountryId
            range.ApplyCharacterFormat(FontBold);
            int colOriginCountry = COL; COL++;
            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UOM");
            range.ApplyCharacterFormat(FontBold);
            int colUOM = COL++;
            //wTable.Rows[ROW].Cells[colUOM].Width = 30;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate (" + dsOrderMaster.Rows[0]["CurrencyName"].ToString() + ")");
            range.ApplyCharacterFormat(FontBold);
            int colRate = COL;
            wTable.Rows[ROW].Cells[colRate].Width = 60;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
                wTable.Rows[ROW].Cells[colTotalTaxableAmount].Width = 60;
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        //two columns required for tax
                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                        range.ApplyCharacterFormat(FontBold);
                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
                range.ApplyCharacterFormat(FontBold);
            }


            if (dv.Count > 0)
            {
                wTable.Rows.Add(TemplateRow);
                ROW++;
                WTableRow TROW = wTable.LastRow;
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
                        range.ApplyCharacterFormat(FontBold);
                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }
            #endregion column headers
            if (dv.Count > 0)
            {
                wTable.Rows.Add(TemplateRow);

                WTableRow TROW = wTable.LastRow;
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                for (int i = 0; i < dv.Count; i++)
                {

                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
                    range.ApplyCharacterFormat(FontBold);
                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                    range.ApplyCharacterFormat(FontBold);
                }
                ROW++;
            }
            else
            {
                ROW++;
                wTable.AddRow();

            }
            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                ROW++;
                sl++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                TROW.Cells[colRo].AddParagraph().AppendText(sl.ToString());
                TROW.Cells[colMaterialGroup].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialMaster"].ToString());
                TROW.Cells[colArticle].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Article"].ToString());
                TROW.Cells[colChar1].AddParagraph().AppendText(dsOrderMaster.Rows[i]["FirstCharacteristicsValue"].ToString());
                TROW.Cells[colChar2].AddParagraph().AppendText(dsOrderMaster.Rows[i]["SecondCharacteristicsValue"].ToString());
                TROW.Cells[colChar3].AddParagraph().AppendText(dsOrderMaster.Rows[i]["ThirdCharacteristicsValue"].ToString());
                //TROW.Cells[colHSNCode].AddParagraph().AppendText(dsOrderMaster.Rows[i]["HSNCode"].ToString());
                TROW.Cells[colMatDescription].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialDetail"].ToString());
                TROW.Cells[colDescription].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Description"].ToString());
                TROW.Cells[colRefferenceNo].AddParagraph().AppendText(dsOrderMaster.Rows[i]["RefferenceNo"].ToString());
                TROW.Cells[colDeliveryDate].AddParagraph().AppendText(dsOrderMaster.Rows[i]["DeliveryDate"].ToString());
                TROW.Cells[colOriginCountry].AddParagraph().AppendText(dsOrderMaster.Rows[i]["CountryOfOrigin"].ToString());
                TROW.Cells[colQty].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["POTransactionQty"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colRate].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["TransactionRate"].ToString()).ToString("#,##0.0000"));
                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colUOM].AddParagraph().AppendText(dsOrderMaster.Rows[i]["TransactionUoM"].ToString());
                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString()).ToString("#,##0.00"));
                totalValue += clsStdLib.dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString());
                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(totalValue.ToString("F2"));
                if (dv.Count > 0)
                {
                    //dsTax.Tables[0].DefaultView.RowFilter = "MasterOrderItemId='" + dsOrderItems.Tables[0].Rows[i]["MasterOrderItemId"].ToString() + "'";
                    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
                    //double totalTax = 0;
                    for (int T = 0; T < dv.Count; T++)
                    {
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND PurchaseOrderDetailId='" + dsOrderMaster.Rows[i]["PurchaseOrderDetailId"].ToString() + "'";
                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("#,##0.00"));
                        }
                    }
                }
            }

            ROW++;
            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);



            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (C == colMaterialGroup || C == colRate || C == colArticle || C == colChar1 || C == colChar2 || C == colChar3 || C == colUOM || C == colMatDescription || C == colRefferenceNo || C == colDescription || C == colDeliveryDate || C == colOriginCountry || dicTaxes.ContainsValue(C))
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);

            }

            #endregion Total
            ROW++;
            #region Sub Total
            double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(TrnAmount)", "").ToString())
                //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                + clsStdLib.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());
            #endregion Total
            ROW++;
            #region Total Payable


            #endregion Total Payable
            ROW++;
            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            //myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                // TROW.Cells[0].Width = 120;
                //if (dv.Count < 3)
                //    TROW.Cells[0].Width = 120 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle");
                    }
                }
            }


            IWParagraphStyle myStyleRightAlign = document.AddParagraphStyle("MyStyleRightAlign");
            //Sets the formatting of the style
            myStyleRightAlign.CharacterFormat.FontSize = 8f;
            myStyleRightAlign.CharacterFormat.TextColor = Color.Black;
            myStyleRightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;



            for (int R = 1; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];



                foreach (WParagraph item in TROW.Cells[colQty].Paragraphs)
                {
                    item.ApplyStyle("MyStyleRightAlign");
                }


                foreach (WParagraph item in TROW.Cells[colRate].Paragraphs)
                {
                    item.ApplyStyle("MyStyleRightAlign");
                }


                foreach (WParagraph item in TROW.Cells[colTotalTaxableAmount].Paragraphs)
                {
                    item.ApplyStyle("MyStyleRightAlign");
                }


            }

            #endregion paragrpath formats
            #region merging section

            //tax codes merging (horizontal)
            ROW = 0;
            for (int i = 0; i < dv.Count; i++)
                wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            WTableRow TROWe = wTable.LastRow;
            for (int i = 0; i <= colTotalTaxableAmount; i++)
            {
                TROWe.Cells[i].Width = wTable.Rows[0].Cells[i].Width;
                wTable.ApplyVerticalMerge(i, ROW - 1, ROW);
            }
            //wTable.ApplyVerticalMerge(i, ROW - 1, ROW);




            IWParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
            style.CharacterFormat.Bold = true;
            style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section

            #endregion merging section
            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);
            return total;
        }

        public double makeTermsAndCondition(string purchaseOrderId, WordDocument document, DataTable dsTermsAndCondition)
        {
            string replaceString = "{TermsAndCondition}";

            WCharacterFormat FontBoldUnderline = new WCharacterFormat(document);
            FontBoldUnderline.Bold = true;
            FontBoldUnderline.UnderlineStyle = UnderlineStyle.Single;

            WCharacterFormat FontBold2 = new WCharacterFormat(document);
            FontBold2.Bold = true;

            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 8f;
            rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;
            int LasColumnIndex = 2;
            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex);
            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();
            int colTermsAndCondition = COL;
            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;
            string CmpTitile = "";
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            int colHeader = 0;
            int colDescription = 0;

            for (int i = 0; i < dsTermsAndCondition.Rows.Count; i++)
            {
                if (dsTermsAndCondition.Rows[i]["TermsAndConditionPOChildId"].ToString() != CmpTitile)
                {
                    COL = 0;
                    IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dsTermsAndCondition.Rows[i]["Title"].ToString() + ".");
                    range.ApplyCharacterFormat(FontBoldUnderline);

                    //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Header");
                    //range.ApplyCharacterFormat(FontBold);
                    colHeader = COL; COL++;
                    wTable.Rows[ROW].Cells[colHeader].Width = 150;


                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                    //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Description");
                    //range.ApplyCharacterFormat(FontBold);
                    colDescription = COL; COL++;
                    wTable.Rows[ROW].Cells[colDescription].Width = 700;


                    // wTable.Rows[ROW].Cells[colTermsAndCondition].Width = 500;
                    sl = 0;
                }
                #endregion column headers
                ROW++;
                sl++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                IWTextRange A = TROW.Cells[colHeader].AddParagraph().AppendText(sl + "." + dsTermsAndCondition.Rows[i]["HeaderCaption"].ToString() + ".");
                A.ApplyCharacterFormat(FontBold2);
                TROW.Cells[colDescription].AddParagraph().AppendText(sl + "." + dsTermsAndCondition.Rows[i]["DESCRIPTION"].ToString() + ".");
                CmpTitile = dsTermsAndCondition.Rows[i]["TermsAndConditionPOChildId"].ToString();
            }
            ROW++;


            #region Total
            //int TotalRow = ROW;
            //wTable.AddRow();
            //WTableRow _TROW = wTable.LastRow;

            //range.ApplyCharacterFormat(FontBold);
            #endregion Total
            ROW++;
            #region paragrpath formats

            IWParagraphStyle myStyle = document.AddParagraphStyle("ServiceStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;


            #endregion paragrpath formats

            #region merging section

            //tax codes merging (horizontal)
            ROW = 0;
            ROW++;
            #endregion merging section


            wTable.TableFormat.Borders.BorderType = BorderStyle.None;

            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);

            return 0;
        }

        public double makeServiceDetailsTable(WordDocument document, DataTable dsServiceItems, string purchaseOrderId)
        {
            string replaceString = "{ServiceItems}";
            ReportUtility ru = new ReportUtility();
            DataTable dsTax;
            //clsDataContext data = new clsDataContext();
            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign1");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 8f;
            rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;
            dsTax = loadServiceMasterTax(purchaseOrderId);
            int LasColumnIndex = 2;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));
            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    LasColumnIndex++;
                }
            }
            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);
            WTableRow TemplateRow = wTable.Rows[0].Clone();
            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Services");
            int colServiceName = COL; COL++;
            range.ApplyCharacterFormat(FontBold);



            // range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Description");
            // range.ApplyCharacterFormat(FontBold);
            //var colDescription = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Description");
            int colDescription = COL; //COL++;           
            range.ApplyCharacterFormat(FontBold);




            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
                range.ApplyCharacterFormat(FontBold);

                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    //two columns required for tax
                    COL++;
                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                    range.ApplyCharacterFormat(FontBold);

                    COL++;
                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                    range.ApplyCharacterFormat(FontBold);

                }
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
                range.ApplyCharacterFormat(FontBold);

            }


            wTable.Rows.Add(TemplateRow);
            ROW++;

            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {

                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
                    range.ApplyCharacterFormat(FontBold);
                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                    range.ApplyCharacterFormat(FontBold);

                }
            }
            #endregion column headers
            double totalValue = 0;
            int startRow = ROW + 1;
            for (int i = 0; i < dsServiceItems.Rows.Count; i++)
            {
                ROW++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                }
                IParagraphItem p = TROW.Cells[colServiceName].AddParagraph().AppendText(dsServiceItems.Rows[i]["Service"].ToString());
                TROW.Cells[colDescription].AddParagraph().AppendText(dsServiceItems.Rows[i]["Description"].ToString());
                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsServiceItems.Rows[i]["Amount"].ToString()).ToString("#,##0.00"));
                totalValue += clsStdLib.dbl(dsServiceItems.Rows[i]["Amount"].ToString());
                if (dv.Count > 0)
                {
                    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
                    //double totalTax = 0;
                    for (int T = 0; T < dv.Count; T++)
                    {
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND InventoryServiceId='" + dsServiceItems.Rows[i]["ServiceId"] + "'";
                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("#,##0.00"));
                        }
                    }
                }
            }

            ROW++;
            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);
            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (C == colDescription || dicTaxes.ContainsValue(C))
                    continue;
                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {
                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);
            }
            #endregion Total
            ROW++;
            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStdLib.dbl(dsServiceItems.Compute("SUM(Amount)", "").ToString())
                //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                + clsStdLib.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());

            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2") + " (" + ru.InWord(total, dsOrderMaster.Rows[0]["CurrencyId"].ToString()) + ")");

            #endregion Total
            ROW++;
            #region Total Payable
            //int TotalPayableRow = ROW;
            //int TotalPayableColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[TotalPayableColumn].AddParagraph().AppendText("Total Amount Payable");
            //_TROW.Cells[TotalPayableColumn + 1].AddParagraph().AppendText("Need To Discuss");

            #endregion Total Payable
            ROW++;
            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle2 = document.AddParagraphStyle("MyStyle2");
            //Sets the formatting of the style
            myStyle2.CharacterFormat.FontSize = 8f;
            myStyle2.CharacterFormat.TextColor = Color.Black;
            myStyle2.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                // TROW.Cells[0].Width = 120;
                //if (dv.Count < 3)
                //    TROW.Cells[0].Width = 120 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle2");
                    }
                }
            }


            #endregion paragrpath formats
            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            for (int i = 0; i < dv.Count; i++)
                wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            for (int i = 0; i <= colTotalTaxableAmount; i++)
                wTable.ApplyVerticalMerge(i, ROW - 1, ROW);




            IWParagraphStyle style2 = document.AddParagraphStyle("SubTotalStyle2");
            style2.CharacterFormat.Bold = true;
            style2.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section


            //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
            //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
            //        PARA.ApplyStyle("SubTotalStyle2");

            //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
            #endregion merging section
            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);
            return total;
        }
        class clsStdLib
        {
            public static string passWord = "prodDisplay";
            public clsStdLib()
            {

            }
            public enum mType
            {
                Error,
                Success,
                Information
            }
            public static bool passwordGet = true;
            public static string[] sMonth = new string[] { "<Unselect>", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

            public static string DataRankNames(int dayNo)
            {

                if (dayNo <= 0)
                    return "";

                if (dayNo.ToString().Length > 1)
                {
                    string Right = dayNo.ToString().Substring(dayNo.ToString().Length - 2, 2);
                    if (clsStdLib.dbl(Right) >= 10 && clsStdLib.dbl(Right) <= 20)
                        return dayNo + "th";
                }

                string RightString = dayNo.ToString().Substring(dayNo.ToString().Length - 1, 1);
                switch (RightString)
                {
                    case "1":
                        return dayNo + "st";
                    case "2":
                        return dayNo + "nd";
                    case "3":
                        return dayNo + "rd";
                    default:
                        return dayNo + "th";
                }
            }
            #region date related
            public static readonly string dateFormat = "dd-MMM-yyyy";
            public static readonly string sqliteDateFormat = "yyyy-MM-dd";
            public static readonly string AppToDBdateFormat = "yyyy-MM-dd hh:mm:ss";
            public static bool IsDateOK(string strdate)
            {
                try
                {
                    if (strdate.Length != 11)
                    {
                        return false;
                    }
                    if (strdate.Substring(2, 1) != "-" && strdate.Substring(6, 1) != "-")
                    {
                        return false;
                    }
                    System.DateTime myDt = System.Convert.ToDateTime(strdate);
                    return true;
                }
                catch (System.Exception ex)
                {
                    return false;
                }
                finally
                {
                    //
                }
            }// end function
            private static bool DateOkCheck(string strdate)
            {
                try
                {
                    System.DateTime myDt = System.Convert.ToDateTime(strdate);
                    return true;
                }
                catch (System.Exception ex)
                {
                    return false;
                }
                finally
                {
                    //
                }
            }// end function
            public static object chk_NullDateData(object dateValue)
            {
                if (DateOkCheck("" + dateValue.ToString()) == false)
                {
                    dateValue = "";
                }

                if (("" + dateValue.ToString()) == "")
                {
                    System.DateTime dt = new System.DateTime(1901, 1, 1);
                    dateValue = (object)dt;
                }
                return (object)dateValue;
            }
            public static System.DateTime AppDateConvert(object dateValue, string input_date_format, string output_date_format)
            {
                string strDate = null;
                dateValue = chk_NullDateData(dateValue);
                strDate = dateValue.ToString();
                if (strDate != "")
                {
                    if (input_date_format.Trim() != "")
                    {
                        if (output_date_format.Trim() != "")
                        {
                            System.Globalization.DateTimeFormatInfo InputFormat = new System.Globalization.DateTimeFormatInfo();
                            InputFormat.ShortDatePattern = input_date_format;
                            System.DateTime myDt = System.Convert.ToDateTime(strDate, InputFormat);
                            strDate = myDt.ToString(output_date_format);
                        }
                    }
                }
                return System.Convert.ToDateTime(strDate);
            }// End of function
            public static Object DateData_AppToDB(object dateValue, string DB_Level_date_format)
            {
                if (string.IsNullOrEmpty((string)dateValue))
                    return DBNull.Value;

                string strDate = null;
                strDate = dateValue.ToString();
                if (DB_Level_date_format != "")
                {
                    // Collecting the user terminal set format 
                    System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
                    strDate = AppDateConvert(strDate, USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString(), DB_Level_date_format).ToString();
                }

                string m = System.Convert.ToDateTime(strDate).ToString(AppToDBdateFormat);
                return System.Convert.ToDateTime(strDate).ToString(AppToDBdateFormat);


            }// End of function
            public static System.DateTime DateData_DBToApp(object dateValue)
            {
                string strDate = null;
                strDate = dateValue.ToString();

                System.Globalization.DateTimeFormatInfo myDBDateFormat = new System.Globalization.CultureInfo("en-US", false).DateTimeFormat;
                strDate = DateData_DBToApp(dateValue, myDBDateFormat.ShortDatePattern.ToString()).ToString();
                return System.Convert.ToDateTime(strDate);
            }// End function
            public static System.DateTime DateData_DBToApp(object dateValue, string DB_Level_date_format)
            {
                string strDate = null;
                strDate = dateValue.ToString();
                if (DB_Level_date_format != "")
                {
                    // Collecting the user terminal set format 
                    System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
                    strDate = AppDateConvert(strDate, DB_Level_date_format, USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString()).ToString();
                }
                return System.Convert.ToDateTime(strDate);
            }// End of function
            public static String makeBaseBlank(object dateValue)
            {
                System.DateTime dt;
                dt = System.Convert.ToDateTime(dateValue.ToString());
                if (dt.Year == 1901)
                {
                    return "";
                }
                else
                {
                    return dateValue.ToString();
                }
            }// End of function
            ///<summary>
            ///return day difference in integer. 
            ///    Example 1: firstDate[Less Than]lastDate returns positive value
            ///    Example 2: firstDate>lastDate returns negative value
            ///    Example 3: firstDate=lastDate returns 0 [zero]**/
            /// </summary>
            public static int dateDiff(string firstDate, string lastDate)
            {

                int difference = 0;
                try
                {
                    firstDate = Convert.ToDateTime(firstDate).ToString("dd-MMM-yyyy");
                    lastDate = Convert.ToDateTime(lastDate).ToString("dd-MMM-yyyy");

                    if (IsDateOK(firstDate) == false)
                    {
                        Exception ex = new Exception("Invalid [First Date]");
                        throw (ex);
                    }
                    if (IsDateOK(lastDate) == false)
                    {
                        Exception ex = new Exception("Invalid [Last Date]");
                        throw (ex);
                    }
                    DateTime dateFirstDate = Convert.ToDateTime(firstDate);
                    DateTime dateLastDate = Convert.ToDateTime(lastDate);
                    TimeSpan TimeSpan = dateLastDate.Subtract(dateFirstDate);


                    difference = TimeSpan.Days;
                }
                catch (Exception ex)
                {
                    throw (ex);
                }

                return difference;
            }



            public static string getSqliteDate(string standardDate)
            {
                return (Convert.ToDateTime(standardDate).ToString(sqliteDateFormat));
            }
            public static string getStandardDateFromSqliteDate(string SqliteDate)
            {
                if (SqliteDate.Length != 10)
                    return "";
                if (SqliteDate.Split('-').Length != 3)
                    return "";
                //many things to validate 
                //but i have less time :)
                string month = ValidLength(sMonth[Convert.ToInt32(SqliteDate.Split('-')[1])], 3).ToString();


                return SqliteDate.Split('-')[2] + "-" + month + "-" + SqliteDate.Split('-')[0];
            }
            #endregion date related
            #region numeric
            public static bool IsNumeric(string strNumber)
            {
                Double d;
                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
                if (strNumber.Length == 0)
                {
                    return false;
                }
                return Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d);
            } // End Function
            public static string GetNumericData(string strNumber)
            {
                double d;
                strNumber = strNumber.Replace(",", "");
                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
                if (strNumber.Trim() == "")
                { return "0"; }
                else if (System.Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
                {
                    return strNumber;
                }
                else
                {
                    return "0";
                }
            }// end function
            public static string GetNumericDataInDecimalFormat(string strNumber, int precision)
            {
                if (precision < 1)
                    return strNumber;

                string s_precision = new String('0', precision);

                double d;
                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
                if (strNumber.Trim() == "")
                { return "0." + s_precision; }
                else if (System.Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
                {
                    return string.Format("{0:0." + s_precision + "}", d);
                }
                else
                {
                    return "0." + s_precision;
                }
            }// end function
            public static double dbl(string d)
            {
                return Convert.ToDouble(GetNumericData(d));

            }
            public static int Percentage(int total, double percentage)
            {
                return (int)(total * (percentage / 100));

            }
            //validation
            public static void numericValidation(string value, bool isMandatory, bool isInteger, bool negativeAllowed, string fieldName)
            {

                try
                {



                    if (isMandatory == true)
                    {
                        if (value.Trim() == "")
                        {
                            Exception ex = new Exception("please insert [" + fieldName + "]");
                            throw (ex);
                        }
                        if (Convert.ToDouble(GetNumericData(value.Trim())) == 0)
                        {
                            Exception ex = new Exception("please insert [" + fieldName + "]");
                            throw (ex);
                        }

                        if (value.Trim() != "")
                        {
                            if (IsNumeric(value.Trim()) == false)
                            {
                                Exception ex = new Exception("Invalid numeric value [" + value + "] for the field [" + fieldName + "]");
                                throw (ex);
                            }
                        }
                    }

                    if (value.Trim() != "")
                    {
                        if (IsNumeric(value.Trim()) == false)
                        {
                            Exception ex = new Exception("Invalid numeric value [" + value + "] for the field [" + fieldName + "]");
                            throw (ex);
                        }
                        if (isInteger == true)
                        {

                            if (isInt(value.Trim()) == false)
                            {
                                Exception ex = new Exception("Number must be integer for the field [" + fieldName + "]");
                                throw (ex);
                            }

                        }
                        if (negativeAllowed == false)
                        {
                            if (Convert.ToDouble(GetNumericData(value.Trim())) < 0)
                            {
                                Exception ex = new Exception("Negative values are not allowed for the field [" + fieldName + "]");
                                throw (ex);
                            }
                        }
                    }



                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {

                }


            }

            ///<summary>
            ///check whether a value is integer or not returns true if integer, 
            ///false if floating or string containing alpahnumeric
            ///</summary>
            public static bool isInt(string num)
            {

                bool isInt;
                int number;
                try
                {
                    isInt = System.Int32.TryParse(num, out number);
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {

                }
                return isInt;
            }


            #endregion numeric
            #region string

            public static readonly string excelNegativePOsitiveSign = @"+#,##0.00;-#,##0.00;* ??;@";
            public static readonly string NegativePOsitiveSign = @"+#,##0.00;-#,##0.00;0";
            public static readonly string NumberFormatString = "#,##0.000;(#,##0.000);* ??;@";
            public static readonly string NumberFormatStringFourDecimal = "#,##0.0000;(#,##0.0000);* ??;@";
            public static readonly string NumberFormatStringFiveDecimal = "#,##0.00000;(#,##0.00000);* ??;@";
            public static readonly string NumberFormatStringTwoDecimal = "#,##0.00;(#,##0.00);* ??;@";
            public static readonly string NumberFormatStringTwoDecimalWithZero = "#,##0.00;(#,##0.00)";
            public static readonly string NumberFormatStringInteger = "#,##0;(#,##0);* ??;@";
            public static readonly string NumberFormatStringIntegerWithZero = "#,##0;(#,##0)";
            public static readonly string NumberFormatStringText = "@"; //format cell data as text


            public static object ValidLength(string str)
            {

                string removechar = "";
                if (str.Trim() == "")
                {
                    return (object)Convert.DBNull;
                }
                removechar = str.Trim();
                removechar = removechar.Replace("'", " ");

                return (object)removechar.Trim();

            }
            public static object ValidLength(string str, int length)
            {

                string removechar = "";
                if (str.Trim() == "")
                {
                    return (object)Convert.DBNull;
                }
                removechar = str.Trim();
                removechar = removechar.Replace("'", " ");


                int strLen = removechar.Length;
                if (strLen > length)
                    removechar = removechar.Substring(0, length);

                return (object)removechar.Trim();

            }
            public static string FileNameLegalChar(string fileName)
            {
                string illegalChar = @"~`!@#$%^&*=/\|>,<";
                foreach (char c in illegalChar)
                {
                    fileName = fileName.Replace(c.ToString(), " ");
                }

                return fileName;
            }
            private StringCollection getTableColumns(ref DataSet dsLocal)
            {
                StringCollection strcol = new StringCollection();
                for (int COL = 0; COL < dsLocal.Tables[0].Columns.Count; COL++)
                {
                    strcol.Add(dsLocal.Tables[0].Columns[COL].ColumnName.ToUpper());
                }

                return strcol;

            }
            public static string emptyString(string str)
            {
                //this function returns an empty string(not a null) from null or empty or '&nbsp;' from the page
                if (str == "&nbsp;")
                    str = "";
                if (string.IsNullOrEmpty(str) == true)
                    str = "";


                return str;
            }//this function returns an empty string(not a null) from null or empty '&nbsp;' from the page
            #endregion string

            #region others
            //public void copyDataset(DataSet source, ref DataSet destination)
            //{
            //    //StringCollection strColDestinationColumns = getTableColumns(ref destination);//upper case
            //    DataRow drLocal = null;
            //    for (int ROW = 0; ROW < source.Tables[0].Rows.Count; ROW++)
            //    {
            //        drLocal = destination.Tables[0].NewRow();
            //        for (int COL = 0; COL < source.Tables[0].Columns.Count; COL++)
            //        {
            //            if (strColDestinationColumns.Contains(source.Tables[0].Columns[COL].ToString().ToUpper()))
            //            {
            //                drLocal[source.Tables[0].Columns[COL].ToString()] = ValidLength(source.Tables[0].Rows[ROW][source.Tables[0].Columns[COL].ToString()].ToString());
            //            }
            //        }
            //        destination.Tables[0].Rows.Add(drLocal);
            //    }


            //}
            public static string GetxlsCol(int intCol)
            {
                //returns excel columns based on column number. tested 1 to 256 column numbers
                try
                {
                    if (intCol < 1 || intCol > 256)
                    {
                        System.Exception ex = new Exception("Invalid Column Value");
                        throw (ex);
                    }
                    intCol = intCol - 1;
                    int intFirstLetter = ((intCol) / 512) + 64;
                    int intSecondLetter = ((intCol % 512) / 26) + 64;
                    int intThirdLetter = (intCol % 26) + 65;
                    char FirstLetter;
                    char SecondLetter;
                    if (intFirstLetter > 64)
                        FirstLetter = (char)intFirstLetter;
                    else
                        FirstLetter = ' ';

                    if (intSecondLetter > 64)
                        SecondLetter = (char)intSecondLetter;
                    else
                        SecondLetter = ' ';

                    char ThirdLetter = (char)intThirdLetter;
                    return string.Concat(FirstLetter, SecondLetter, ThirdLetter).Trim();
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {

                }
            }//returns excel columns based on column number. tested 1 to 256 column numbers
            #endregion others
            public static object RetValidLen(string Data)
            {
                if (string.IsNullOrEmpty(Data))
                    return DBNull.Value;
                return Data;
            }
            public static double sum(string columnName, DataTable dtLocal, string criteria)
            {
                double total = 0;
                DataRow[] dr = dtLocal.Select(criteria);
                foreach (DataRow d in dr)
                {
                    total += dbl(d[columnName].ToString());
                }
                return total;
            }
        }
        #region  Taufik

        public IEnumerable<object> GetMaterialDetails(string Id)
        {
            try
            {
                var sql = @"Select  Id	
                        ,InventoryReceiveId	
                        ,InventoryMaterialId	
                        ,MaterialStorageId	
                        ,TransactionQty	
                        ,TransactionUoMId	
                        ,BaseQty	
                        ,BaseUOMId	
                        ,BaseUoMFactor	
                        ,TransactionRate	
                        ,TransactionAmount	
                        ,IssueQty	
                        ,AddedBy	
                        ,AddedDate	
                        ,AddedFromIP	
                        ,UpdatedBy	
                        ,UpdatedDate	
                        ,UpdatedFromIP	
                        ,TotalTaxAmount	
                        ,BaseAmount	
                        ,ChargesAmount	
                        ,WithInvoiceRate	
                        ,AfterInvoiceRate	
                        ,CountryId	
                        ,InventoryMaterialId	
                        ,MaterialStorageId	
                        ,BaseUOMId	
                        ,TransactionQty	
                        ,TransactionRate	
                        ,TransactionAmount
                        from 
                        TRN.purchaseorderdetail pd
                        where id='" + Id + "' ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }


        public DataTable loadOrderMaster(string purchaseOrderId)
        {
            string strSQL;
            try
            {
                //           strSQL = @"SELECT PO.Id PONumber
                //               ,HSNC.Code HSNCode
                //             ,CNO.ContractNo
                //             ,CNO.Id ContractId
                //               ,mo.BuyerReferenceNo 
                //,PLC.LCRef LCNumber 
                //               ,PLC.BenificiaryBank BeneficiaryBank
                //               ,PLC.BenificiaryBank OpeningBank
                //--,B.UserName BeneficiaryBank
                //--,B.UserName OpeningBank
                //               ,PO.CompanyGroupId
                //               ,PO.CompanyId
                //               ,Plant.GSTIN
                //            ,REPLACE(Convert(VARCHAR(11), PLC.LCDate, 106), ' ', '-') AS LCODate
                //               ,REPLACE(Convert(VARCHAR(11), PO.PODate, 106), ' ', '-') AS PODate
                //               ,POType=CASE WHEN PO.POType='PO' then 'PO Without Requisition' ELSE 'PO With Requisition' END
                //               ,REPLACE(Convert(VARCHAR(11), PO.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
                //               ,REPLACE(Convert(VARCHAR(11), PO.MatureDate, 106), ' ', '-') AS MatureDate
                //               ,PO.InvoicingPartyPlantId
                //               ,INVPARTYPL.UserName InvoicingPartyName
                //               ,INVPARTYPL.AddressMasterId InvoicePartyAddressMasterId
                //               ,INVPARTYPL.GSTIN InvoicingPartyGSTIN
                //               ,ISNULL(PO.InvoicingByAddress,'') InvoicingByAddress
                //               ,PO.DeliveryByAddress
                //               ,DPARTYPL.UserName DeliveryParty
                //               ,PO.DeliveryPartyPlantId
                //               ,POD.InventoryMaterialId MaterialMasterId
                //               ,PO.DocRefNo
                //               ,REPLACE(Convert(VARCHAR(11), PO.DocDate, 106), ' ', '-') AS DocDate
                //               ,CheckedBy=CASE WHEN PO.CheckedByStatus='Checked' Then eI.EmployeeName else '' END
                //               ,AuthorizedBy=CASE When PO.AuthorizedByStatus='Approved'then eI1.EmployeeName else '' END
                //               ,AddedBy=CASE When PO.CheckedByStatus='pending' OR PO.CheckedByStatus='Hold' OR PO.CheckedByStatus='Reject' OR PO.CheckedByStatus='Checked'then eI3.EmployeeName else PO.AddedBy  END 
                //               ,PO.AddedDate
                //               ,PO.UpdatedBy
                //               ,PO.UpdatedDate
                //               ,PO.IsApproved
                //               ,PO.PartyType
                //               ,PO.PartyId
                //               ,POD.RefferenceNo
                //               ,isnull(PO.DiscountAmount,0) DiscountAmount
                //               ,ISNULL(PO.DeliveryInstruction,'') DeliveryInstruction
                //               ,ISNULL(PO.SpecialInstruction,'') SpecialInstruction
                //               ,Party.UserName VendorName
                //               ,Party.AddressMasterId VendorAddressMasterId
                //               ,Party.TINNO VendorGSTIN
                //               ,Case When PO.IsNonCreditable = 1 then 'NonCreditable' when Po.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
                //               ,PO.CurrencyId
                //               ,CRNC.Code AS CurrencyName
                //               ,PO.ToCurrencyRate
                //               ,BASECRNC.Code AS BaseCurrencyName
                //               ,PayTerm.UserName PaymentTerm
                //               ,MM.UserName MaterialMaster
                //               ,MM.MaterialGroupMasterId
                //               ,MGM.UserName MaterialGroupMaster
                //               ,POD.ArticleId
                //               ,MMA.StandardName Article
                //               ,FC.Id FirstCharId
                //               ,FC.UserName FirstChar
                //               ,POD.FirstCharacteristicsValueId
                //               ,FCV.UserName AS FirstCharacteristicsValue
                //               ,POD.SecondCharacteristicsValueId
                //               ,SCV.UserName AS SecondCharacteristicsValue
                //               ,POD.ThirdCharacteristicsValueId
                //               ,TCV.UserName AS ThirdCharacteristicsValue
                //               ,SC.Id SecondCharId
                //               ,SC.UserName SecondChar
                //               ,TC.Id ThirdCharId
                //               ,TC.UserName ThirdChar
                //               ,ROUND(POD.TransactionQty, 2) POTransactionQty
                //               ,ROUND(POD.TransactionRate, 4) TransactionRate
                //               ,ROUND((POD.TransactionQty * POD.TransactionRate), 2) AS TrnAmount
                //               ,POD.BaseAmount
                //               ,POD.TotalTaxAmount AS BaseTaxAmount
                //               ,REPLACE(Convert(VARCHAR(11), POD.DeliveryDate, 106), ' ', '-') AS DeliveryDate
                //               ,TaxAmount = (
                //               SELECT SUM(TaxAmount)
                //               FROM [TRN].[PurchaseOrderTax]
                //               WHERE InventoryReceiveDetailId = POD.Id
                //               )
                //               ,ServiceTaxAmount = (
                //               SELECT SUM(TotalTaxAmount)
                //               FROM [TRN].[POService]
                //               WHERE InventoryReceiveId = POD.InventoryReceiveId
                //               )
                //               ,POD.Description
                //               ,POD.ChargesAmount
                //               ,POD.CountryId
                //               ,POCountry.UserName CountryOfOrigin
                //               ,POD.Id PurchaseOrderDetailId
                //               ,POD.TransactionUoMId
                //                ,TUoM.ShortName AS TransactionUoM
                //               ,MRMD.MaterialDetail MaterialDetail
                //               ,CheckStatus= CASE when PO.CheckedByStatus='pending' Then 'To be checked'
                //               when PO.CheckedByStatus='Hold' Then 'Hold'
                //               when PO.CheckedByStatus='Reject' Then 'Reject'
                //               when PO.CheckedByStatus='Checked' Then 'Checked'
                //               else ''
                //               END
                //               ,ApproveStatus= CASE
                //               when PO.AuthorizedByStatus='Reject' Then 'Reject For Approved'
                //               when PO.AuthorizedByStatus='Hold' Then 'Hold For Approved'
                //               when PO.AuthorizedByStatus='For Approval' Then 'To be Approval'
                //               when PO.AuthorizedByStatus='Approved' Then 'Approved'
                //               else ''
                //               END
                //               FROM TRN.PurchaseOrder PO
                //               LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = PO.CompanyGroupId
                //               LEFT JOIN ORG.Company Cmp ON Cmp.Id = PO.CompanyId
                //               LEFT JOIN ORG.Plant Plant ON Plant.Id = PO.PlantId
                //               LEFT JOIN SCS.Currency CRNC ON CRNC.Id = PO.CurrencyId
                //               LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = PO.BaseCurrencyId
                //               LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = PO.PaymentTermId
                //               LEFT JOIN HKP.PartyPlant INVPARTYPL ON INVPARTYPL.Id = PO.InvoicingPartyPlantId
                //               LEFT JOIN HKP.PartyPlant DPARTYPL ON DPARTYPL.Id = PO.DeliveryPartyPlantId
                //               LEFT JOIN TRN.PurchaseOrderDetail POD ON PO.Id = POD.InventoryReceiveId
                //LEFT JOIN [dbo].[Contract] CNO ON CNO.Id = PO.ContractId
                //               LEFT JOIN trn.MasterOrderItem AS mo ON mo.ContractId=cno.Id
                //LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id = PO.PurchaseLCId
                //           -- LEFT JOIN [HKP].[Bank] B ON B.Id = PLC.BenificiaryBankId
                //               LEFT JOIN SCS.Country POCountry ON POD.CountryId = POCountry.Id
                //               LEFT JOIN HKP.Party Party ON Party.Id = PO.PartyId
                //               LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = POD.InventoryMaterialId
                //            LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
                //               LEFT JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                //               LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = POD.ArticleId
                //               LEFT JOIN HKP.Characteristics AS FC ON POD.FirstCharacteristicsId = FC.Id
                //               LEFT JOIN HKP.Characteristics AS SC ON POD.SecondCharacteristicsId = SC.Id
                //               LEFT JOIN HKP.Characteristics AS TC ON POD.ThirdCharacteristicsId = TC.Id
                //               LEFT JOIN HKP.CharacteristicsValue AS FCV ON POD.FirstCharacteristicsValueId = FCV.Id
                //               LEFT JOIN HKP.CharacteristicsValue AS SCV ON POD.SecondCharacteristicsValueId = SCV.Id
                //               LEFT JOIN HKP.CharacteristicsValue AS TCV ON POD.ThirdCharacteristicsValueId = TCV.Id
                //               LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON POD.TransactionUoMId = TUoM.Id
                //               LEFT JOIN TRN.MaterialRequsitionDetails AS MRMD ON MRMD.Id=POD.RequisitionDetailId
                //               LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=PO.CheckedBy
                //               LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=PO.AuthorizedBy
                //               left join [SEC].[User] U on U.UserId=PO.AddedBy
                //               LEFT JOIN dbo.EmployeeInformation eI3 ON eI3.SystemId=U.EmployeeId

                strSQL = @"SELECT PO.Id PONumber
                    , HSNC.Code HSNCode
                      , CNO.ContractNo
 	                ,CNO.Id ContractId
                    --,BuyerReferenceNo = STUFF((SELECT DISTINCT ',' + moi.BuyerReferenceNo from
                    --   BOQ boq
                    --   INNER JOin trn.POBOQMAP xboqMap on boq.Id = xboqMap.BOQDetailId
                    --   INNER JOIN trn.PurchaseOrderDetail xpod on xpod.Id = xboqMap.PODetailId
                    --   LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id = boq.MasterOrderItemId
                    --           WHERE xpod.Id = pod.Id for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
					,PLC.LCRef LCNumber
                    , PLC.BenificiaryBank BeneficiaryBank
                     , PLC.BenificiaryBank OpeningBank

                    --,B.UserName BeneficiaryBank

                    --,B.UserName OpeningBank
                    , PO.CompanyGroupId
                    ,PO.CompanyId,PO.BaseNoOfDays
                    --,Plant.GSTIN
	                ,REPLACE(Convert(VARCHAR(11), PLC.LCDate, 106), ' ', '-') AS LCODate
                    , REPLACE(Convert(VARCHAR(11), PO.PODate, 106), ' ', '-') AS PODate
                  --, POType = CASE WHEN PO.POType = 'PO' then 'PO Without Requisition' when PO.POType = 'POBOQ' then 'PO BOQ' ELSE 'PO With Requisition' END
                    ,POType=CASE WHEN PO.POType='PO' then 'PO Without Requisition' ELSE 'PO With Requisition' END
                    ,REPLACE(Convert(VARCHAR(11), PO.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
                    , REPLACE(Convert(VARCHAR(11), PO.MatureDate, 106), ' ', '-') AS MatureDate
                     , PO.InvoicingPartyPlantId
                    ,INVPARTYPL.UserName InvoicingPartyName
                    , INVPARTYPL.AddressMasterId InvoicePartyAddressMasterId
                     , INVPARTYPL.GSTIN InvoicingPartyGSTIN
                      , ISNULL(PO.InvoicingByAddress, '') InvoicingByAddress
                    ,PO.DeliveryByAddress
                    ,DPARTYPL.UserName DeliveryParty
                    , PO.DeliveryPartyPlantId
                    ,POD.InventoryMaterialId MaterialMasterId
                    , PO.DocRefNo
                    ,REPLACE(Convert(VARCHAR(11), PO.DocDate, 106), ' ', '-') AS DocDate
                    , CheckedBy = CASE WHEN PO.CheckedByStatus = 'Checked' Then eI.EmployeeName else '' END
                    ,AuthorizedBy = CASE When PO.AuthorizedByStatus = 'Approved'then eI1.EmployeeName else '' END
                    ,AddedBy = CASE When PO.CheckedByStatus = 'pending' OR PO.CheckedByStatus = 'Hold' OR PO.CheckedByStatus = 'Reject' OR PO.CheckedByStatus = 'Checked'then eI3.EmployeeName else PO.AddedBy END
                              , PO.AddedDate
                    ,PO.UpdatedBy
                    ,PO.UpdatedDate
                    ,PO.IsApproved
                    ,PO.PartyType
                    ,PO.PartyId
                    ,BuyerReferenceNo = STUFF((SELECT DISTINCT ',' + moi.BuyerReferenceNo from
                                            trn.MasterOrderItem moi
											LEFT JOIN TRN.SalesOrder SO ON SO.MasterOrderItemId=moi.Id
                                        WHERE CNO.Id = SO.ContractId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,POD.RefferenceNo
                    , isnull(PO.DiscountAmount, 0) DiscountAmount
                    ,ISNULL(PO.DeliveryInstruction, '') DeliveryInstruction
                    ,ISNULL(PO.SpecialInstruction, '') SpecialInstruction
                    ,Party.UserName VendorName
                    , Party.AddressMasterId VendorAddressMasterId
                     , Party.TINNO GSTIN
                      , Case When PO.IsNonCreditable = 1 then 'NonCreditable' when Po.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
                       , PO.CurrencyId
                    ,CRNC.Code AS CurrencyName
                    ,PO.ToCurrencyRate
                    ,BASECRNC.Code AS BaseCurrencyName
                    ,PayTerm.UserName PaymentTerm
                    , MM.UserName MaterialMaster
                     , MM.MaterialGroupMasterId
                    ,MGM.UserName MaterialGroupMaster
                    , POD.ArticleId
                    ,MMA.StandardName Article
                    , FC.Id FirstCharId
                     , FC.UserName FirstChar
                      , POD.FirstCharacteristicsValueId
                    ,FCV.UserName AS FirstCharacteristicsValue
                    ,POD.SecondCharacteristicsValueId
                    ,SCV.UserName AS SecondCharacteristicsValue
                    ,POD.ThirdCharacteristicsValueId
                    ,TCV.UserName AS ThirdCharacteristicsValue
                    ,SC.Id SecondCharId
                    , SC.UserName SecondChar
                     , TC.Id ThirdCharId
                      , TC.UserName ThirdChar
                       , ROUND(POD.TransactionQty, 2) POTransactionQty
                    ,ROUND(POD.TransactionRate, 4) TransactionRate
                    ,ROUND((POD.TransactionQty * POD.TransactionRate), 2) AS TrnAmount
                    , POD.BaseAmount
                    ,POD.TotalTaxAmount AS BaseTaxAmount
                    ,REPLACE(Convert(VARCHAR(11), POD.DeliveryDate, 106), ' ', '-') AS DeliveryDate
                    , TaxAmount = (
                    SELECT SUM(TaxAmount)
                    FROM[TRN].[PurchaseOrderTax]
                    WHERE InventoryReceiveDetailId = POD.Id
                    )
                    ,ServiceTaxAmount = (
                    SELECT SUM(TotalTaxAmount)
                    FROM[TRN].[POService]
                    WHERE InventoryReceiveId = POD.InventoryReceiveId
                    )
                    ,POD.Description
                    ,POD.ChargesAmount
                    ,POD.CountryId
                    ,POCountry.UserName CountryOfOrigin
                    , POD.Id PurchaseOrderDetailId
                     , POD.TransactionUoMId
                     ,TUoM.ShortName AS TransactionUoM
                    --,MRMD.MaterialDetail MaterialDetail
                     , MaterialDetail = STUFF((SELECT DISTINCT ',' + boq.RMDescription from
                                             BOQ boq
     
                                             INNER JOin trn.POBOQMAP xboqMap on boq.Id = xboqMap.BOQDetailId

                                        INNER JOIN trn.PurchaseOrderDetail xpod on xpod.Id = xboqMap.PODetailId

                                        LEFT JOIN CostingBOQItems xboqI on xboqI.CostingItemId = boq.CostingItemId

                                        WHERE xpod.Id = pod.Id for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,BuyerPONumber = STUFF((SELECT DISTINCT ',' + PO.PONumber from
                                            BOQ boq
    
                                            INNER JOin trn.POBOQMAP xboqMap on boq.Id = xboqMap.BOQDetailId

                                        INNER JOIN trn.PurchaseOrderDetail xpod on xpod.Id = xboqMap.PODetailId

                                        LEFT  JOIN[TRN].[SalesOrder] AS so ON so.MasterOrderItemId = boq.MasterOrderItemId

                                        LEFT  JOIN[TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id

                                        WHERE xpod.Id = pod.Id for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,CheckStatus = CASE when PO.CheckedByStatus = 'pending' Then 'To be checked'
                    when PO.CheckedByStatus = 'Hold' Then 'Hold'
                    when PO.CheckedByStatus = 'Reject' Then 'Reject'
                    when PO.CheckedByStatus = 'Checked' Then 'Checked'
                    else ''
                    END
                    ,ApproveStatus = CASE
                    when PO.AuthorizedByStatus = 'Reject' Then 'Reject For Approved'
                    when PO.AuthorizedByStatus = 'Hold' Then 'Hold For Approved'
                    when PO.AuthorizedByStatus = 'For Approval' Then 'To be Approval'
                    when PO.AuthorizedByStatus = 'Approved' Then 'Approved'
                    else ''
                    END
                    FROM TRN.PurchaseOrder PO
                    LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = PO.CompanyGroupId
                    LEFT JOIN ORG.Company Cmp ON Cmp.Id = PO.CompanyId
                    LEFT JOIN ORG.Plant Plant ON Plant.Id = PO.PlantId
                    LEFT JOIN SCS.Currency CRNC ON CRNC.Id = PO.CurrencyId
                    LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = PO.BaseCurrencyId
                    LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = PO.PaymentTermId
                    LEFT JOIN HKP.PartyPlant INVPARTYPL ON INVPARTYPL.Id = PO.InvoicingPartyPlantId
                    LEFT JOIN HKP.PartyPlant DPARTYPL ON DPARTYPL.Id = PO.DeliveryPartyPlantId
                    LEFT JOIN TRN.PurchaseOrderDetail POD ON PO.Id = POD.InventoryReceiveId

                    LEFT JOIN[dbo].[Contract] CNO ON CNO.Id = PO.ContractId

                    LEFT JOIN[dbo].[PurchaseLC] PLC ON PLC.Id = PO.PurchaseLCId
                  -- LEFT JOIN[HKP].[Bank] B ON B.Id = PLC.BenificiaryBankId
                    LEFT JOIN SCS.Country POCountry ON POD.CountryId = POCountry.Id
                    LEFT JOIN HKP.Party Party ON Party.Id = PO.PartyId
                    LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = POD.InventoryMaterialId

                    LEFT JOIN[HKP].[HSNCode] AS HSNC ON HSNC.ID = MM.HSNCodeId
                    LEFT JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                    LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = POD.ArticleId
                    LEFT JOIN HKP.Characteristics AS FC ON POD.FirstCharacteristicsId = FC.Id
                    LEFT JOIN HKP.Characteristics AS SC ON POD.SecondCharacteristicsId = SC.Id
                    LEFT JOIN HKP.Characteristics AS TC ON POD.ThirdCharacteristicsId = TC.Id
                    LEFT JOIN HKP.CharacteristicsValue AS FCV ON POD.FirstCharacteristicsValueId = FCV.Id
                    LEFT JOIN HKP.CharacteristicsValue AS SCV ON POD.SecondCharacteristicsValueId = SCV.Id
                    LEFT JOIN HKP.CharacteristicsValue AS TCV ON POD.ThirdCharacteristicsValueId = TCV.Id
                    LEFT JOIN[SCS].[UnitOfMeasurement] AS TUoM ON POD.TransactionUoMId = TUoM.Id
                    LEFT JOIN TRN.MaterialRequsitionDetails AS MRMD ON MRMD.Id = POD.RequisitionDetailId
                    LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId = PO.CheckedBy
                    LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId = PO.AuthorizedBy
                    left join[SEC].[User] U on U.UserId = PO.AddedBy
                    LEFT JOIN dbo.EmployeeInformation eI3 ON eI3.SystemId = U.EmployeeId

                WHERE PO.Id = '" + purchaseOrderId + @"' order by MM.UserName";
                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        public DataTable TermsAndConditionSQL(string purchaseOrderId)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT  ROW_NUMBER() OVER(ORDER BY tac.Sequence) RoWNo, PO.Id POId
,tac.Id TermsAndConditionMasterId,tacc.Id TermsAndConditionPOChildId,tacd.id TermsAndConditionPODetailId,
tacc.Title,tacd.HeaderCaption,tacd.DESCRIPTION
FROM TRN.PurchaseOrder AS PO
LEFT OUTER JOIN HKP.TermsAndConditions AS tac ON PO.TermsAndConditionsId=tac.Id
LEFT OUTER JOIN TermsAndConditionsPOChild AS tacc ON tacc.POId=PO.Id
LEFT OUTER JOIN TermsAndConditionsPODetails AS tacd ON tacd.TermsAndConditionsPOChildId=tacc.Id
WHERE PO.id='" + purchaseOrderId + @"' Order By tac.Sequence,tacc.Id ";

                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }



        public DataTable loadServicerMasterItems(string purchaseOrderId)
        {
            string strSQL;

            try
            {
                strSQL = @"SELECT POS.Id ServiceId,SM.UserName  Service , POS.Description, POS.Amount,POS.TotalTaxAmount,Pos.AddedBy,pos.AddedDate,pos.UpdatedBy,pos.UpdatedDate FROM TRN.PurchaseOrder PO
                            INNER join TRN.POService POS ON POS.InventoryReceiveId = PO.Id
                            INNER JOIN HKP.ServiceMaster SM ON POS.ServiceMasterId = SM.Id 
                            where PO.Id = '" + purchaseOrderId + @"'";


                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }
        public DataTable loadServiceMasterTax(string purchaseOrderId)
        {
            string strSQL;

            try
            {
                strSQL = @"SELECT InventoryServiceId,PO.Id PurchaseOrderId,tg.Code AS TaxCode,PODT.Percentage, PODT.TaxAmount from TRN.PurchaseOrder PO
                            INNER JOIN TRN.POService POS ON POS.InventoryReceiveId = PO.Id
                            INNER JOIN TRN.PurchaseOrderTax PODT ON PODT.InventoryReceiveId = PO.Id and PODT.InventoryServiceId = POS.Id
                              LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=PODT.TaxCategoryId
                                WHERE PO.Id='" + purchaseOrderId + @"' 
								AND InventoryServiceId   IS NOT NULL AND  InventoryReceiveDetailId IS NULL 
								 ORDER BY tg.[Sequence] ";


                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }
        #endregion
        public DataTable GetPurchaseOrderSqlData(string purchaseOrderId)
        {
            try
            {
                string sqlTxt = @"SELECT PO.Id PONumber
                                                ,PO.CompanyGroupId
                                                ,PO.CompanyId
                                                ,REPLACE(Convert(VARCHAR(11), PO.PODate, 106), ' ', '-') AS PODate
	                                            ,PO.POType
                                                ,REPLACE(Convert(VARCHAR(11), PO.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
                                                ,REPLACE(Convert(VARCHAR(11), PO.MatureDate, 106), ' ', '-') AS MatureDate
		                                        ,PO.InvoicingPartyPlantId
		                                        ,INVPARTYPL.UserName InvoiceParty
		                                        ,PO.InvoicingByAddress
		                                        ,PO.DeliveryByAddress
		                                        ,DPARTYPL.UserName DeliveryParty
		                                        ,PO.DeliveryPartyPlantId		
		                                        ,POM.MaterialMasterId
		                                        ,PO.DocRefNo
		                                        ,PO.DocDate
		                                        ,PO.AddedBy
		                                        ,PO.AddedDate
		                                        ,PO.UpdatedBy
		                                        ,PO.UpdatedDate
		                                        ,PO.IsApproved
		                                        ,PO.PartyType
		                                        ,PO.IsNonCreditable
		                                        ,PO.CurrencyId
	                                            ,CRNC.Code AS CurrencyName
	                                            ,PO.ToCurrencyRate
		                                        ,BASECRNC.Code AS BaseCurrencyName
		                                        ,PayTerm.UserName PaymentTerm
	                                          ,MM.UserName MaterialMaster
	                                          ,MM.MaterialGroupMasterId
	                                          ,MGM.UserName MaterialGroupMaster
	                                          ,POM.ArticleId
	                                          ,MMA.StandardName Article
	                                          ,FC.Id FirstCharId
	                                          ,FC.UserName FirstChar
                                              ,POM.FirstCharacteristicsValueId
	                                          ,FCV.UserName AS FirstCharacteristicsValue
                                              ,POM.SecondCharacteristicsValueId
	                                          ,SCV.UserName AS SecondCharacteristicsValue
	                                          ,POM.ThirdCharacteristicsValueId
	                                          ,TCV.UserName AS ThirdCharacteristicsValue
	                                          ,SC.Id SecondCharId
	                                          ,SC.UserName SecondChar
	                                          ,TC.Id ThirdCharId
	                                          ,TC.UserName ThirdChar
	                                          ,ROUND(POD.TransactionQty, 2) POTransactionQty
	                                          ,ROUND(POD.TransactionRate, 2) TransactionRate
	                                          ,ROUND((POD.TransactionQty * POD.TransactionRate), 2) AS TrnAmount
	                                          ,POD.BaseAmount
	                                          ,POD.TotalTaxAmount AS BaseTaxAmount
	                                          ,TaxAmount = (
		                                            SELECT SUM(TaxAmount)
		                                            FROM [TRN].[PurchaseOrderTax]
		                                            WHERE InventoryReceiveDetailId = POD.Id
		                                            )
	                                          ,ServiceTaxAmount = (
		                                            SELECT SUM(TotalTaxAmount)
		                                            FROM [TRN].[POService]
		                                            WHERE InventoryReceiveId = POM.Id
		                                            )
	                                          ,POD.ChargesAmount
	                                          ,POD.CountryId

	                                          ,POD.TransactionUoMId
	                                          ,TUoM.UserName AS TransactionUoM
                                              FROM TRN.PurchaseOrder PO
                                         LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = PO.CompanyGroupId
                                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = PO.CompanyId
                                         LEFT JOIN ORG.Plant Plant ON Plant.Id = PO.PlantId
                                         LEFT JOIN SCS.Currency CRNC ON CRNC.Id = PO.CurrencyId
                                         LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = PO.BaseCurrencyId
                                         LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = PO.PaymentTermId
                                         LEFT JOIN HKP.PartyPlant  INVPARTYPL ON INVPARTYPL.Id = PO.InvoicingPartyPlantId
                                         LEFT JOIN HKP.PartyPlant  DPARTYPL ON DPARTYPL.Id = PO.DeliveryPartyPlantId 
                                         LEFT JOIN TRN.PurchaseOrderDetail POD ON PO.Id = POD.InventoryReceiveId

                                         LEFT JOIN TRN.POMaterial AS POM ON POD.InventoryMaterialId = POM.Id
                                         INNER JOIN MST.MaterialMaster AS MM ON MM.Id = POM.MaterialMasterId
                                         INNER JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                                         INNER JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = POM.ArticleId
                                         LEFT JOIN HKP.Characteristics AS FC ON POM.FirstCharacteristicsId = FC.Id
                                         LEFT JOIN HKP.Characteristics AS SC ON POM.SecondCharacteristicsId = SC.Id
                                         LEFT JOIN HKP.Characteristics AS TC ON POM.ThirdCharacteristicsId = TC.Id
                                         LEFT JOIN HKP.CharacteristicsValue AS FCV ON POM.FirstCharacteristicsValueId = FCV.Id
                                         LEFT JOIN HKP.CharacteristicsValue AS SCV ON POM.SecondCharacteristicsValueId = SCV.Id
                                         LEFT JOIN HKP.CharacteristicsValue AS TCV ON POM.ThirdCharacteristicsValueId = TCV.Id
                                         JOIN [SCS].[UnitOfMeasurement] AS TUoM ON POD.TransactionUoMId = TUoM.Id
                                         WHERE PO.Id = '" + purchaseOrderId + @"' and PO.IsApproved = 0";

                return _sqlRepository.GetDataTable(sqlTxt);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region PO Approval for post
        public void PoApproved(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy, string CheckedRejectReason)
        {
            try
            {

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var AuthorizedById = "";
                var AuthorizedByStatus = "";
                PoValue = "0";
                var Id = GetPK();
                if (CheckedStataus == "Checked")
                {
                    if (AuthorizedBy == null || AuthorizedBy == "")
                    {
                        throw new CustomException("Select Approved By");
                    }
                    else if (identity.EmployeeId == AuthorizedBy)
                    {
                        throw new CustomException("You can't select same user");
                    }
                    AuthorizedById = AuthorizedBy;
                    AuthorizedByStatus = "For Approval";
                }
                else
                {
                    AuthorizedById = null;

                }
                var Status = CheckedStataus;
                var UpdatedBy = "";

                var ip = identity.IPAddress;
                var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var AddedBy = identity.Name;
                var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var CompanyGroupId = identity.CompanyGroupId;
                var CompanyId = identity.CompanyId;
                var PlantId = identity.PlantId;

                con.BeginTransaction();
                con.executeQuery("Update TRN.PurchaseOrder set IsApproved='0',CheckedByStatus='" + Status + "',AuthorizedBy='" + AuthorizedById + "',AuthorizedByStatus='" + AuthorizedByStatus + "',CheckedHoldRejectReason='" + CheckedRejectReason + "' where id='" + PoId + "'");
                con.executeQuery("Insert into TRN.PurchaseOrderApprovalLog(Id," +
                "CompanyGroupId," +
                "CompanyId," +
                "PlantId," +
                "ApprovedBy," +
                "Date," +
                "POValue," +
                "Status," +
                "AddedBy," +
                "AddedDate," +
                "AddedFromIp," +
                "UpdatedBy," +
                "UpdatedDate," +
                "UpdatedFromIp,POID) " +
                "values ('" + Id + "'," +
                "'" + CompanyGroupId + "'," +
                "'" + CompanyId + "'," +
                "'" + PlantId + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + PoValue + "'," +
                "'" + Status + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + ip + "'," +
                "'" + UpdatedBy + "'," +
                "'" + updatedDate + "', " +
                "'" + ip + "','" + PoId + "')");
                con.CommitTransaction();

                //string _sql = "Update TRN.PurchaseOrder set IsApproved='0',CheckedByStatus='" + Status + "',AuthorizedBy='" + AuthorizedById + "',AuthorizedByStatus='" + AuthorizedByStatus + "',CheckedHoldRejectReason='" + CheckedRejectReason + "' where id='" + PoId + "'";
                //_sqlRepository.ExecuteSqlCommand(_sql);
                //string _sql1 = "Insert into TRN.PurchaseOrderApprovalLog(Id," +
                //"CompanyGroupId," +
                //"CompanyId," +
                //"PlantId," +
                //"ApprovedBy," +
                //"Date," +
                //"POValue," +
                //"Status," +
                //"AddedBy," +
                //"AddedDate," +
                //"AddedFromIp," +
                //"UpdatedBy," +
                //"UpdatedDate," +
                //"UpdatedFromIp,POID) " +
                //"values ('" + Id + "'," +
                //"'" + CompanyGroupId + "'," +
                //"'" + CompanyId + "'," +
                //"'" + PlantId + "'," +
                //"'" + AddedBy + "'," +
                //"'" + AddedDate + "'," +
                //"'" + PoValue + "'," +
                //"'" + Status + "'," +
                //"'" + AddedBy + "'," +
                //"'" + AddedDate + "'," +
                //"'" + ip + "'," +
                //"'" + UpdatedBy + "'," +
                //"'" + updatedDate + "', " +
                //"'" + ip + "','" + PoId + "')";
                //_sqlRepository.ExecuteSqlCommand(_sql1);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public void PoUnApproved(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy)
        {
            try
            {
                //var AuthorizedById = "";
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                PoValue = "0";
                var Id = GetPK();
                //if (CheckedStataus == "Checked")
                //{
                //    if (AuthorizedBy == null || AuthorizedBy == "")
                //    {
                //        throw new CustomException("Select Approved By");
                //    }
                //    AuthorizedById = AuthorizedBy;

                //}
                //else
                //{
                //    AuthorizedById = null;

                //}
                var Status = "UnApproved";
                var UpdatedBy = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var ip = identity.IPAddress;
                var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var AddedBy = identity.Name;
                var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var CompanyGroupId = identity.CompanyGroupId;
                var CompanyId = identity.CompanyId;
                var PlantId = identity.PlantId;
                con.BeginTransaction();
                con.executeQuery("Update TRN.PurchaseOrder set IsApproved='0',CheckedByStatus='For Checked',AuthorizedBy=null,AuthorizedByStatus=null where id='" + PoId + "'");
                //_sqlRepository.ExecuteSqlCommand(_sql);
                con.executeQuery("Insert into TRN.PurchaseOrderApprovalLog(Id," +
                "CompanyGroupId," +
                "CompanyId," +
                "PlantId," +
                "ApprovedBy," +
                "Date," +
                "POValue," +
                "Status," +
                "AddedBy," +
                "AddedDate," +
                "AddedFromIp," +
                "UpdatedBy," +
                "UpdatedDate," +
                "UpdatedFromIp,POID) " +
                "values ('" + Id + "'," +
                "'" + CompanyGroupId + "'," +
                "'" + CompanyId + "'," +
                "'" + PlantId + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + PoValue + "'," +
                "'" + Status + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + ip + "'," +
                "'" + UpdatedBy + "'," +
                "'" + updatedDate + "', " +
                "'" + ip + "','" + PoId + "')");
                con.CommitTransaction();
                //_sqlRepository.ExecuteSqlCommand(_sql1);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }



        public void PoApprovedAuth(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy, string ApproveRejectReason)
        {
            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                var IsApproved = 0;

                PoValue = "0";
                var Id = GetPK();
                if (CheckedStataus == "Approved")
                {
                    IsApproved = 1;

                }
                else
                {
                    IsApproved = 0;

                }
                var Status = CheckedStataus;
                var UpdatedBy = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var ip = identity.IPAddress;
                var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var AddedBy = identity.Name;
                var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var CompanyGroupId = identity.CompanyGroupId;
                var CompanyId = identity.CompanyId;
                var PlantId = identity.PlantId;
                con.BeginTransaction();
                con.executeQuery("Update TRN.PurchaseOrder set AuthorizedByStatus='" + Status + "',IsApproved='" + IsApproved + "', AuthorizedBy='" + identity.EmployeeId + "',ApprovedHoldRejectReason='" + ApproveRejectReason + "' where id='" + PoId + "'");
                //_sqlRepository.ExecuteSqlCommand(_sql);
                con.executeQuery("Insert into TRN.PurchaseOrderApprovalLog(Id," +
                "CompanyGroupId," +
                "CompanyId," +
                "PlantId," +
                "ApprovedBy," +
                "Date," +
                "POValue," +
                "Status," +
                "AddedBy," +
                "AddedDate," +
                "AddedFromIp," +
                "UpdatedBy," +
                "UpdatedDate," +
                "UpdatedFromIp,POID) " +
                "values ('" + Id + "'," +
                "'" + CompanyGroupId + "'," +
                "'" + CompanyId + "'," +
                "'" + PlantId + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + PoValue + "'," +
                "'" + Status + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + ip + "'," +
                "'" + UpdatedBy + "'," +
                "'" + updatedDate + "', " +
                "'" + ip + "','" + PoId + "')");
                con.CommitTransaction();
                //_sqlRepository.ExecuteSqlCommand(_sql1);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }


        #endregion
        #region un Approval for post
        public void PoApproved1(string PoId, string PoValue)
        {
            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                PoValue = "0";
                var Id = GetPK();

                var Status = "UnApproved";
                var UpdatedBy = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var ip = identity.IPAddress;
                var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var AddedBy = identity.Name;
                var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var CompanyGroupId = identity.CompanyGroupId;
                var CompanyId = identity.CompanyId;
                var PlantId = identity.PlantId;
                // var res =_inventoryReceiveRepository.SqlQuery<Int32>($"select distinct statusflag=Case when B.POId is not null then 1 else 0 end	from trn.PurchaseOrder A	Left JOIN trn.InventoryReceiveDetail B on B.POID=A.Id where A.Id='"+PoId+"'").First();               

                //if(Convert.ToBoolean(res))
                // {
                //     throw new CustomException("You can not un Approved the PO? GRN already Received");
                // }
                // else
                // {
                con.BeginTransaction();
                con.executeQuery("Update TRN.PurchaseOrder set IsApproved='0' where id='" + PoId + "'");
                //_sqlRepository.ExecuteSqlCommand(_sql);
                con.executeQuery("Insert into TRN.PurchaseOrderApprovalLog(Id," +
                "CompanyGroupId," +
                "CompanyId," +
                "PlantId," +
                "ApprovedBy," +
                "Date," +
                "POValue," +
                "Status," +
                "AddedBy," +
                "AddedDate," +
                "AddedFromIp," +
                "UpdatedBy," +
                "UpdatedDate," +
                "UpdatedFromIp,POID) " +
                "values ('" + Id + "'," +
                "'" + CompanyGroupId + "'," +
                "'" + CompanyId + "'," +
                "'" + PlantId + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + PoValue + "'," +
                "'" + Status + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + ip + "'," +
                "'" + UpdatedBy + "'," +
                "'" + updatedDate + "', " +
                "'" + ip + "','" + PoId + "')");
                con.CommitTransaction();
                //_sqlRepository.ExecuteSqlCommand(_sql1);
                // }

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public void PoApproved1Auth(string PoId, string PoValue)
        {
            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                PoValue = "0";
                var Id = GetPK();

                var Status = "UnApproved";
                var UpdatedBy = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var ip = identity.IPAddress;
                var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var AddedBy = identity.Name;
                var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var CompanyGroupId = identity.CompanyGroupId;
                var CompanyId = identity.CompanyId;
                var PlantId = identity.PlantId;
                // var res =_inventoryReceiveRepository.SqlQuery<Int32>($"select distinct statusflag=Case when B.POId is not null then 1 else 0 end	from trn.PurchaseOrder A	Left JOIN trn.InventoryReceiveDetail B on B.POID=A.Id where A.Id='"+PoId+"'").First();               

                //if(Convert.ToBoolean(res))
                // {
                //     throw new CustomException("You can not un Approved the PO? GRN already Received");
                // }
                // else
                // {
                con.BeginTransaction();
                con.executeQuery("Update TRN.PurchaseOrder set IsApproved='0' where id='" + PoId + "'");
                //_sqlRepository.ExecuteSqlCommand(_sql);
                con.executeQuery("Insert into TRN.PurchaseOrderApprovalLog(Id," +
                "CompanyGroupId," +
                "CompanyId," +
                "PlantId," +
                "ApprovedBy," +
                "Date," +
                "POValue," +
                "Status," +
                "AddedBy," +
                "AddedDate," +
                "AddedFromIp," +
                "UpdatedBy," +
                "UpdatedDate," +
                "UpdatedFromIp,POID) " +
                "values ('" + Id + "'," +
                "'" + CompanyGroupId + "'," +
                "'" + CompanyId + "'," +
                "'" + PlantId + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + PoValue + "'," +
                "'" + Status + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + ip + "'," +
                "'" + UpdatedBy + "'," +
                "'" + updatedDate + "', " +
                "'" + ip + "','" + PoId + "')");
                con.CommitTransaction();
                //_sqlRepository.ExecuteSqlCommand(_sql1);
                // }

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }




        #region POClose Taufik
        public IEnumerable<object> GetListForPOClose(string plantId)
        {
            try
            {


                var Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                                SELECT ROW_NUMBER() OVER(
                                        ORDER BY IR.Id
                                        ) AS SiNo
                                    , IR.Id
	                                ,REPLACE(CONVERT(CHAR(11), IR.PODate, 106), ' ', '-') AS PODate
                                    , IR.CompanyGroupId
	                                ,IR.CompanyId
	                                ,IR.PlantId
	                                ,IR.PartyId
	                                ,P.Code AS PartyCode
	                                ,P.UserName AS PartyName
	                                ,CP.UserName AS PartyAccountGroupName
	                                ,IR.MaterialStorageId
	                                ,IR.DocRefNo
	                                ,REPLACE(CONVERT(CHAR(11), IR.DocDate, 106), ' ', '-') AS DocDate

                                    --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106), ' ', '-') AS EntryDate
                                      , IR.CurrencyId
	                                ,CU.Code AS CurrencyCode
	                                ,IR.BaseCurrencyId
	                                ,IR.PaymentTermId
	                                ,IR.BaseNoOfDays
	                                ,REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
                                    , REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
                                     , IR.FixedAssetOrInventory
	                                ,IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate
                                      , IR.InvoicingPartyPlantId
	                                ,IPP.UserName AS InvoicingBy
	                                ,IR.InvoicingByAddress
	                                ,IR.DeliveryPartyPlantId
	                                ,DPP.UserName AS DeliveryBy
	                                ,IR.DeliveryByAddress
	                                ,IR.IsNonCreditable
	                                ,IRD.TransactionQty
	                                ,TU.TransactionUoMId
	                                ,UoM.UserName AS TransactionUoM
	                                ,IRD.TransactionAmount
	                                ,IRD.BaseAmount
	                                ,IR.ToCurrencyRate
	                                ,S1.UserName AS InvoicingState
	                                ,S1.Id AS InvoicingStateId
	                                ,S2.UserName AS DeliveryState
	                                ,PT.UserName AS PaymentTermName
	                                ,CP.TaxApplicable
	                                ,CP.IsTaxApplicableChangeable
	                                ,IR.IsTaxApplicable
	                                ,IR.IsApproved
	                                ,IR.IsPaymentHold
	                                ,SP.Id AS PlantStateId
	                                ,pgl.CtnId
                                FROM[TRN].[PurchaseOrder]
                                        AS IR
                                        JOIN[HKP].[Party] AS P ON IR.PartyId = P.Id


                                        LEFT JOIN (
					                                        SELECT count(Id) AS CtnId
						                                        ,POID
					                                        FROM TRN.PurchaseOrderClosedLog
					                                        WHERE STATUS = 'locked'
					                                        GROUP BY POID
					                                        ) AS pgl ON pgl.POID = IR.Id


                                        LEFT JOIN (
                                           SELECT C.PartyId
                                               , C.PaymentTermId
                                               , C.PlantId
                                               , PAG.UserName
                                               , C.TaxApplicable
                                               , C.IsTaxApplicableChangeable
                                           FROM [HKP].[CompanyParty] AS C
                                           LEFT JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id = C.PartyAccountGroupId
                                           WHERE C.PartyType = 'Vendor'
                                           ) AS CP ON CP.PartyId = IR.PartyId
                                           AND CP.PlantId = IR.PlantId
                                        JOIN[SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id
                                        JOIN[MST].[PaymentTerm] AS PT ON IR.PaymentTermId = PT.Id
                                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId = IPP.Id
                                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId = AM.Id
                                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId = S1.Id
                                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId = DPP.Id
                                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId = AM2.Id
                                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId = S2.Id
                                        LEFT JOIN [ORG].Plant PL ON PL.Id = IR.PlantId
                                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id = PL.AddressMasterId
                                        LEFT JOIN [SCS].[State] AS SP ON SP.Id = AMP.StateId
                                        LEFT JOIN (
                                           SELECT A.InventoryReceiveId
                                               , SUM(A.TransactionQty) AS TransactionQty
                                               , SUM(A.TransactionAmount) AS TransactionAmount
                                               , SUM(A.BaseAmount) AS BaseAmount
                                           FROM [TRN].[PurchaseOrderDetail] AS A
                                           JOIN[TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId = B.Id
                                           WHERE B.PlantId='" + plantId + @"'
                                           GROUP BY A.InventoryReceiveId
                                           ) AS IRD ON IRD.InventoryReceiveId = IR.Id
                                        LEFT JOIN (
                                           SELECT A.InventoryReceiveId
                                               , A.TransactionUoMId
                                           FROM [TRN].[PurchaseOrderDetail] AS A
                                           JOIN[TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId = B.Id
                                           WHERE B.PlantId='" + plantId + @"'
                                           GROUP BY A.InventoryReceiveId
                                               , A.TransactionUoMId
                                           HAVING COUNT(A.InventoryReceiveId) > COUNT(A.TransactionUoMId)
                                           ) AS TU ON TU.InventoryReceiveId = IR.Id
                                        LEFT JOIN[SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId = UoM.Id
                                        WHERE IR.PlantId='" + plantId + @"'

                                           AND ISNULL(IR.[Status], '') <> 'Posting'
	                                        AND IR.id IN (
                                                SELECT pod.InventoryReceiveId AS POMasterID
                                                FROM [TRN].[PurchaseOrderDetail] POD
                                                INNER JOIN trn.PurchaseOrder PO ON po.id = pod.InventoryReceiveId

                                                LEFT OUTER JOIN (
                                                    SELECT poDetailsID
                                                        , sum(BaseQty) AS ReceivedQty
                                                    FROM TRN.InventoryReceiveDetail GRND
                                                    GROUP BY poDetailsID
                                                    ) AS RCV ON rcv.PODetailsId = pod.Id
                                                WHERE isnull(rcv.ReceivedQty, 0) >= pod.BaseQty
                                                    AND po.IsClosed=1
			                                        AND po.id NOT IN (
                                                        SELECT pod.InventoryReceiveId
                                                        FROM [TRN].[PurchaseOrderDetail] POD
                                                        INNER JOIN trn.PurchaseOrder PO ON po.id = pod.InventoryReceiveId

                                                        LEFT OUTER JOIN (
                                                            SELECT poDetailsID
                                                                , sum(BaseQty) AS ReceivedQty
                                                            FROM TRN.InventoryReceiveDetail GRND

                                                            GROUP BY poDetailsID

                                                            ) AS RCV ON rcv.PODetailsId = pod.Id
                                                        WHERE pod.BaseQty > isnull(rcv.ReceivedQty, 0)

                                                            AND po.IsClosed = 1
				                                        )
		                                        )
	                                        AND IR.EmployeeId IS NULL

                                            AND IR.IsApproved = 1

                                            AND ir.IsClosed = 1 AND pgl.CtnId is not null
                                        ORDER BY IR.ID DESC";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }


        #endregion


        #region POUnClose Taufik
        public IEnumerable<object> GetListForPOUnClose(string plantId)
        {
            try
            {


                var Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';

                SELECT ROW_NUMBER() OVER(
        ORDER BY IR.Id
        ) AS SiNo
    , IR.Id
	,REPLACE(CONVERT(CHAR(11), IR.PODate, 106), ' ', '-') AS PODate
    , IR.CompanyGroupId
	,IR.CompanyId
	,IR.PlantId
	,IR.PartyId
	,P.Code AS PartyCode
	,P.UserName AS PartyName
	,CP.UserName AS PartyAccountGroupName
	,IR.MaterialStorageId
	,IR.DocRefNo
	,REPLACE(CONVERT(CHAR(11), IR.DocDate, 106), ' ', '-') AS DocDate

    --, IR.GateEntryNo
    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106), ' ', '-') AS EntryDate
      , IR.CurrencyId
	,CU.Code AS CurrencyCode
	,IR.BaseCurrencyId
	,IR.PaymentTermId
	,IR.BaseNoOfDays
	,REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
    , REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
     , IR.FixedAssetOrInventory
	,IR.PODepended
    --, IR.AlongwithInvoice
    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate
      , IR.InvoicingPartyPlantId
	,IPP.UserName AS InvoicingBy
	,IR.InvoicingByAddress
	,IR.DeliveryPartyPlantId
	,DPP.UserName AS DeliveryBy
	,IR.DeliveryByAddress
	,IR.IsNonCreditable
	,IRD.TransactionQty
	,TU.TransactionUoMId
	,UoM.UserName AS TransactionUoM
	,IRD.TransactionAmount
	,IRD.BaseAmount
	,IR.ToCurrencyRate
	,S1.UserName AS InvoicingState
	,S1.Id AS InvoicingStateId
	,S2.UserName AS DeliveryState
	,PT.UserName AS PaymentTermName
	,CP.TaxApplicable
	,CP.IsTaxApplicableChangeable
	,IR.IsTaxApplicable
	,IR.IsApproved
	,IR.IsPaymentHold
	,SP.Id AS PlantStateId
    ,pgl.CtnId

FROM[TRN].[PurchaseOrder]
        AS IR
JOIN[HKP].[Party] AS P ON IR.PartyId = P.Id


LEFT JOIN (
					SELECT count(Id) AS CtnId
						,POID
					FROM TRN.PurchaseOrderClosedLog
					WHERE STATUS = 'locked'
					GROUP BY POID
					) AS pgl ON pgl.POID = IR.Id




LEFT JOIN (
   SELECT C.PartyId

       , C.PaymentTermId

       , C.PlantId

       , PAG.UserName

       , C.TaxApplicable

       , C.IsTaxApplicableChangeable
   FROM [HKP].[CompanyParty] AS C

   LEFT JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id = C.PartyAccountGroupId

   WHERE C.PartyType = 'Vendor'
   ) AS CP ON CP.PartyId = IR.PartyId
   AND CP.PlantId = IR.PlantId
JOIN[SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id
JOIN[MST].[PaymentTerm] AS PT ON IR.PaymentTermId = PT.Id
LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId = IPP.Id
LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId = AM.Id
LEFT JOIN [SCS].[State] AS S1 ON AM.StateId = S1.Id
LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId = DPP.Id
LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId = AM2.Id
LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId = S2.Id
LEFT JOIN [ORG].Plant PL ON PL.Id = IR.PlantId
LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id = PL.AddressMasterId
LEFT JOIN [SCS].[State] AS SP ON SP.Id = AMP.StateId

LEFT JOIN (
   SELECT A.InventoryReceiveId

       , SUM(A.TransactionQty) AS TransactionQty

       , SUM(A.TransactionAmount) AS TransactionAmount

       , SUM(A.BaseAmount) AS BaseAmount
   FROM [TRN].[PurchaseOrderDetail] AS A

   JOIN[TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId = B.Id

   WHERE B.PlantId='" + plantId + @"'

   GROUP BY A.InventoryReceiveId

   ) AS IRD ON IRD.InventoryReceiveId = IR.Id
LEFT JOIN (
   SELECT A.InventoryReceiveId

       , A.TransactionUoMId
   FROM [TRN].[PurchaseOrderDetail] AS A

   JOIN[TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId = B.Id

   WHERE B.PlantId='" + plantId + @"'

   GROUP BY A.InventoryReceiveId

       , A.TransactionUoMId
   HAVING COUNT(A.InventoryReceiveId) > COUNT(A.TransactionUoMId)

   ) AS TU ON TU.InventoryReceiveId = IR.Id
LEFT JOIN[SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId = UoM.Id
WHERE IR.PlantId='" + plantId + @"'

   AND ISNULL(IR.[Status], '') <> 'Posting'
	AND IR.id IN (
        SELECT pod.InventoryReceiveId AS POMasterID
        FROM [TRN].[PurchaseOrderDetail] POD
        INNER JOIN trn.PurchaseOrder PO ON po.id = pod.InventoryReceiveId

        LEFT OUTER JOIN (
            SELECT poDetailsID
                , sum(BaseQty) AS ReceivedQty
            FROM TRN.InventoryReceiveDetail GRND

            GROUP BY poDetailsID

            ) AS RCV ON rcv.PODetailsId = pod.Id
        WHERE isnull(rcv.ReceivedQty, 0) >= pod.BaseQty
            AND po.IsClosed = 0
			AND po.id NOT IN (
                SELECT pod.InventoryReceiveId
                FROM [TRN].[PurchaseOrderDetail] POD
                INNER JOIN trn.PurchaseOrder PO ON po.id = pod.InventoryReceiveId

                LEFT OUTER JOIN (
                    SELECT poDetailsID
                        , sum(BaseQty) AS ReceivedQty
                    FROM TRN.InventoryReceiveDetail GRND

                    GROUP BY poDetailsID

                    ) AS RCV ON rcv.PODetailsId = pod.Id
                WHERE pod.BaseQty > isnull(rcv.ReceivedQty, 0)

                    AND po.IsClosed = 0
				)
		)
	AND IR.EmployeeId IS NULL

    AND IR.IsApproved = 1

    AND ir.IsClosed = 0
ORDER BY IR.ID DESC";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }


        #endregion
        public void POClose(string PoId, string PoValue)
        {
            try
            {
                PoValue = "0";
                var Id = GetPK();

                var Status = "locked";
                var UpdatedBy = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var ip = identity.IPAddress;
                var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var AddedBy = identity.Name;
                var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var CompanyGroupId = identity.CompanyGroupId;
                var CompanyId = identity.CompanyId;
                var PlantId = identity.PlantId;
                string _sql = "Update TRN.PurchaseOrder set IsClosed='1' where id='" + PoId + "'";
                _sqlRepository.ExecuteSqlCommand(_sql);
                string _sql1 = "Insert into TRN.PurchaseOrderClosedLog(Id" +
                ", CompanyGroupId" +
                ", CompanyId" +
                ", PlantId" +
                ", Closedby" +
                ", Date" +
                ", POValue" +
                ", Status" +
                ", AddedBy" +
                ", AddedDate" +
                ", AddedFromIp" +
                ", UpdatedBy" +
                ", UpdatedDate" +
                ", UpdatedFromIp,POID) " +
                "values ('" + Id + "'" +
                ",'" + CompanyGroupId + "'" +
                ",'" + CompanyId + "'" +
                ",'" + PlantId + "'" +
                ",'" + AddedBy + "'" +
                ",'" + AddedDate + "'" +
                ",'" + PoValue + "'" +
                ",'" + Status + "'" +
                ",'" + AddedBy + "'" +
                ",'" + AddedDate + "'" +
                ",'" + ip + "'" +
                ",'" + UpdatedBy + "'" +
                ",'" + updatedDate + "'" +
                ", '" + ip + "','" + PoId + "')";
                _sqlRepository.ExecuteSqlCommand(_sql1);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }



        public void POUnClose(string PoId, string PoValue)
        {
            try
            {
                PoValue = "0";
                var Id = GetPK();

                var Status = "Unlocked";
                var UpdatedBy = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var ip = identity.IPAddress;
                var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var AddedBy = identity.Name;
                var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var CompanyGroupId = identity.CompanyGroupId;
                var CompanyId = identity.CompanyId;
                var PlantId = identity.PlantId;
                string _sql = "Update TRN.PurchaseOrder set IsClosed='0' where id='" + PoId + "'";
                _sqlRepository.ExecuteSqlCommand(_sql);
                string _sql1 = "Insert into TRN.PurchaseOrderClosedLog(Id" +
                ", CompanyGroupId" +
                ", CompanyId" +
                ", PlantId" +
                ", Closedby" +
                ", Date" +
                ", POValue" +
                ", Status" +
                ", AddedBy" +
                ", AddedDate" +
                ", AddedFromIp" +
                ", UpdatedBy" +
                ", UpdatedDate" +
                ", UpdatedFromIp,POID) " +
                "values ('" + Id + "'" +
                ",'" + CompanyGroupId + "'" +
                ",'" + CompanyId + "'" +
                ",'" + PlantId + "'" +
                ",'" + AddedBy + "'" +
                ",'" + AddedDate + "'" +
                ",'" + PoValue + "'" +
                ",'" + Status + "'" +
                ",'" + AddedBy + "'" +
                ",'" + AddedDate + "'" +
                ",'" + ip + "'" +
                ",'" + UpdatedBy + "'" +
                ",'" + updatedDate + "'" +
                ", '" + ip + "','" + PoId + "')";
                _sqlRepository.ExecuteSqlCommand(_sql1);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        #endregion
        #region Taufik po approve List of po closed

        public IEnumerable<object> GetListForAllPOList(string plantId)
        {
            try
            {
                var Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
                                    --,IR.PODate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,pgl.CtnId
                        FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id


                    LEFT JOIN (
					SELECT count(Id) AS CtnId
						,POID
					FROM TRN.PurchaseOrderClosedLog
					WHERE STATUS = 'locked'
					GROUP BY POID
					) AS pgl ON pgl.POID = IR.Id

                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
		                            JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                 
                        WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting'  AND IR.EmployeeId IS NULL AND IR.IsApproved=0 AND IR.IsClosed=0 Order by IR.PODate DESC, IR.ID DESC";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }




        #endregion
        #region FGForMasterOrder 22-Jun-2019

        public IEnumerable<object> GetListForMasterOrder(string CompanyId)
        {
            try
            {
                var sql = @"SELECT A.Id, A.CompanyId, A.CommitmentId, A.PlantId, A.EntityId
                                    , A.OrderType, A.PartyId, P.UserName AS CustomerName, A.BuyerId	
                                    , A.BuyerBrandId, A.BuyerDivisionId, A.TestingStandardId, A.MasterOrderNo, A.OrderStatusId	
                                    , A.OrderCategoryId, A.SeasonId, A.OrderYear, A.CurrencyId, A.TotalQty	
                                    , A.NoOfLineItem, A.ResponsiblePersonId, EI.EmployeeName AS ResponsiblePersonName
                                    , A.InvoicingPartyPlantId, InvPP.UserName AS InvoicingPartyPlant, A.InvoicingByAddress
		                            , A.DeliveryPartyPlantId, DeliPP.UserName AS DeliveryPartyPlant, A.DeliveryByAddress
		                            , PartyAccountGroupId=(SELECT DISTINCT PartyAccountGroupId FROM [HKP].[CompanyParty] WHERE CompanyId=A.CompanyId
								                            AND PartyId=A.PartyId AND PartyType='Customer' )
								    ,A.OrderWastagePercentage
								    ,A.ExtraOrderPercentage
								    ,A.TotalQtyUOMId,PL.UserName,A.IsReplacement,A.Type,C.Code Currency,A.SpecialTaxId,A.AddedDate as EntryDate
                            FROM [TRN].[MasterOrder] AS A
                            JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN [HKP].[PartyPlant] AS InvPP ON A.InvoicingPartyPlantId=InvPP.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DeliPP ON A.DeliveryPartyPlantId=DeliPP.Id
                            LEFT JOIN EmployeeInformation AS EI ON A.ResponsiblePersonId=EI.SystemId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            WHERE A.CompanyId='" + CompanyId + "' Order by A.AddedDate Desc";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }
        public IEnumerable<object> getTaxCategoryListForFGService(string companyGroupId, string plantId, string hsnCodeId, string partyPlantId)
        {
            try
            {
                var sql = @"DECLARE @partyState varchar(30)
                                  , @partyCountry varchar(10)
                                  , @plantState varchar(30)
                                  , @plantCountry varchar(10)
                                  , @plantId varchar(30)='" + plantId + @"'
                                  , @hsnCodeId varchar(30)='" + hsnCodeId + @"'
                    --SET @partyCountry =(SELECT AM.CountryId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id
                                                   -- JOIN TRN.PurchaseOrder AS IR ON IR.InvoicingPartyPlantId=PP.Id )-- AND AD.Active=1 AND AD.Archive=0)
                    --SET @partyState =(SELECT AM.StateId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id
                                    --JOIN TRN.PurchaseOrder AS IR ON IR.InvoicingPartyPlantId=PP.Id )-- AND AD.Active=1 AND AD.Archive=0)
                    SET @partyCountry =(SELECT AM.CountryId FROM HKP.PartyPlant AS PP
                    LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id WHERE PP.Id='" + partyPlantId + @"')
                    SET @partyState =(SELECT AM.StateId FROM HKP.PartyPlant AS PP
                    LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id WHERE PP.Id='" + partyPlantId + @"')
                    SET @plantState =(SELECT AD.StateId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @plantCountry =(SELECT AD.CountryId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SELECT TVD.Id, TVD.TaxCategoryId, HP.HSNCodeId, HN.Code AS HSNCode, TC.UserName, ISNULL(HP.[Percentage], 0) AS [Percentage], NULL TotalAmount
                    FROM [MST].[TaxVariantDetail] AS TVD
                    JOIN [MST].[TaxVariant] AS TV ON TVD.TaxVariantId=TV.Id
                    JOIN [MST].[TaxCategory] AS TC ON TVD.TaxCategoryId=TC.Id
                    --LEFT JOIN (SELECT * FROM [MST].[HSNTaxPercentage] WHERE HSNCodeId=@hsnCodeId) AS HP ON HP.TaxCategoryId=TC.Id
					LEFT JOIN (SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY TaxCategoryId, HSNCodeId ORDER BY EffectiveDate DESC) AS RN
								FROM [MST].[HSNTaxPercentage] WHERE CountryId=@plantCountry AND HSNCodeId=@hsnCodeId) AS TBL WHERE RN=1) AS HP ON HP.TaxCategoryId=TC.Id

                    LEFT JOIN [HKP].[HSNCode] AS HN ON HP.HSNCodeId=HN.Id
                    WHERE TV.CompanyGroupId='" + companyGroupId + @"' AND TV.CountryId=@plantCountry --AND HP.HSNCodeId=@hsnCodeId
                    AND TV.TaxFor=CASE WHEN @partyCountry=@plantCountry THEN '" + TaxFor.DomesticPurchase + @"'
				                        WHEN @partyCountry<>@plantCountry THEN '" + TaxFor.OverseasPurchase + @"' END
                    AND (TV.Different=CASE WHEN @partyCountry=@plantCountry AND @partyState=@plantState AND TV.DifferentIn='State' THEN 'Same'
					                       WHEN @partyCountry=@plantCountry AND @partyState<>@plantState AND TV.DifferentIn='State' THEN 'Different' END
	                    OR TV.Different IS NULL)
                    ORDER BY TC.[Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetMasterItemList(string masterOrderId)
        {
            try
            {
                //          var sql = @"SELECT MOI.Id, MOI.MasterOrderId, MOI.InquiryItemId, MOI.SampleItemId, MOI.TestingStandardId
                //                    , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName
                //                    , MOI.ArticleId, ART.StandardName AS ArticleName
                //                    , MOI.Code, MOI.BuyerReferenceNo, MOI.OwnReferenceNo, MOI.TotalQty
                //                    , MOI.OrderWastagePercentage, MOI.ExtraOrderPercentage, MM.HSNCodeId
                //	 , ISNULL(HART.HasAttribute,CAST(0 AS BIT)) AS HasAttribute
                //                       , ISNULL((select sum(SO.Qty) from TRN.SalesOrder SO where So.MasterOrderItemId = MOI.Id),0) as SOQty,MOI.Type
                //                  FROM TRN.MasterOrderItem AS MOI
                //                  JOIN MST.MaterialMaster AS MM ON MOI.MaterialMasterId=MM.Id
                //                  LEFT JOIN MST.MaterialMasterArticle AS ART ON MOI.ArticleId=ART.Id
                //LEFT JOIN (SELECT AttributeSetLength=CASE WHEN COUNT(MaterialMasterId)>0THEN COUNT(MaterialMasterId) ELSE 0 END
                //                                          , HasAttribute=CASE WHEN COUNT(MaterialMasterId)>0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END, MaterialMasterId
                //                                      FROM MST.MaterialMasterAttribute GROUP BY MaterialMasterId) AS HART ON HART.MaterialMasterId=MM.Id
                //                  WHERE MOI.MasterOrderId='" + masterOrderId + "'";
                //var sql = @"SELECT MOI.Id
                //            , MOI.MasterOrderId
                //            , MOI.InquiryItemId
                //            , MOI.SampleItemId
                //            , MOI.TestingStandardId
                //            , MGM.UserName AS MaterialGroupMasterName
                //            , MOI.MaterialMasterId
                //            , MM.UserName AS UserName	

                //            , MOI.ArticleId
                //            , ART.StandardName 
                //            --, MOI.Code
                //            , MOI.BuyerReferenceNo
                //            , MOI.OwnReferenceNo
                //            , MOI.TotalQty AS TransactionQty
                //            , MOI.OrderWastagePercentage
                //            , MOI.ExtraOrderPercentage
                //            , MM.HSNCodeId
                //            , ISNULL(HART.HasAttribute,CAST(0 AS BIT)) AS HasAttribute
                //            , ISNULL((select sum(SO.Qty) from TRN.SalesOrder SO where So.MasterOrderItemId = MOI.Id),0) as SOQty,MOI.Type
                //            FROM TRN.MasterOrderItem AS MOI
                //            JOIN MST.MaterialMaster AS MM ON MOI.MaterialMasterId=MM.Id
                //            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                //            LEFT JOIN MST.MaterialMasterArticle AS ART ON MOI.ArticleId=ART.Id
                //            LEFT JOIN (SELECT AttributeSetLength=CASE WHEN COUNT(MaterialMasterId)>0THEN COUNT(MaterialMasterId) ELSE 0 END
                //             , HasAttribute=CASE WHEN COUNT(MaterialMasterId)>0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END, MaterialMasterId
                //            FROM MST.MaterialMasterAttribute GROUP BY MaterialMasterId) AS HART ON HART.MaterialMasterId=MM.Id
                //WHERE MOI.MasterOrderId='" + masterOrderId + "'";

                var sql = @"SELECT    MOI.Id
                                    , MOI.MasterOrderId
                                    , MOI.InquiryItemId
                                    , MOI.SampleItemId
                                    , MOI.TestingStandardId
                                    , MGM.UserName AS MaterialGroupMasterName
                                    , MOI.MaterialMasterId
                                    , MM.UserName AS UserName	

                                    , MOI.ArticleId
                                    , ART.StandardName 

	                                 , FC.CharacteristicsId As FirstCharacteristicsId, C.UserName AS FirstCharacteristics
                                     , Cv1.CharacteristicsId As FirstCharacteristicsValueId, Cv1.UserName AS FirstCharacteristicsValue

                                     , SC.CharacteristicsId AS SecondCharacteristicsId, C2.UserName AS SecondCharacteristics
                                     , Cv2.CharacteristicsId SecondCharacteristicsValueId, Cv2.UserName AS SecondCharacteristicsValue

                                     , TC.CharacteristicsId ThirdCharacteristicsId, C3.UserName AS ThirdCharacteristics
                                     , Cv3.CharacteristicsId AS ThirdCharacteristicsValueId, Cv3.UserName AS ThirdCharacteristicsValue
	                                 ,MOI.TotalQty AS OrderQty
	                                ,0 PORaised
	                                ,0 AS TransactionQty
	                                ,0 BalanceQty	 
	                                 , MM.SalesOrderUOMId AS TransactionUoMId
	                                  ,MM.BaseUOMId AS TransactionUoM
                                     --, ROUND(IRD.TransactionQty,2) TransactionQty, IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM, ROUND(IRD.TransactionRate,2) TransactionRate , CU.Code AS CurrencyName, IR.ToCurrencyRate

                                    --, MOI.Code
                                    , MOI.BuyerReferenceNo
                                    , MOI.OwnReferenceNo
    
                                    , MOI.OrderWastagePercentage
                                    , MOI.ExtraOrderPercentage
                                    , MM.HSNCodeId
                                    , ISNULL(HART.HasAttribute,CAST(0 AS BIT)) AS HasAttribute
                                    , ISNULL((select sum(SO.Qty) from TRN.SalesOrder SO where So.MasterOrderItemId = MOI.Id),0) as SOQty,MOI.Type
                                    FROM TRN.MasterOrderItem AS MOI
                                    JOIN MST.MaterialMaster AS MM ON MOI.MaterialMasterId=MM.Id
                                    LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                                    LEFT JOIN MST.MaterialMasterArticle AS ART ON MOI.ArticleId=ART.Id
                                    LEFT JOIN (SELECT AttributeSetLength=CASE WHEN COUNT(MaterialMasterId)>0THEN COUNT(MaterialMasterId) ELSE 0 END
                                        , HasAttribute=CASE WHEN COUNT(MaterialMasterId)>0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END, MaterialMasterId
   
                                    FROM MST.MaterialMasterAttribute GROUP BY MaterialMasterId) AS HART ON HART.MaterialMasterId=MM.Id
	                                LEFT Join trn.SalesOrder SO on so.MasterOrderItemId=MOI.Id

	                                Left JOin [TRN].[FirstCharacteristics] FC on FC.SalesOrderId=So.Id
	                                LEFT JOIN HKP.Characteristics AS C ON FC.CharacteristicsId=C.Id
	                                LEFT JOIN HKP.CharacteristicsValue AS Cv1 ON FC.CharacteristicsValueId=Cv1.Id

	                                Left JOin [TRN].[SecondCharacteristics] SC on SC.SalesOrderId=So.Id
	                                LEFT JOIN HKP.Characteristics AS C2 ON SC.CharacteristicsId=C2.Id
	                                LEFT JOIN HKP.CharacteristicsValue AS Cv2 ON SC.CharacteristicsValueId=Cv2.Id

	                                Left JOin [TRN].[ThirdCharacteristics] TC on TC.SalesOrderId=So.Id
	                                LEFT JOIN HKP.Characteristics AS C3 ON TC.CharacteristicsId=C3.Id
	                                LEFT JOIN HKP.CharacteristicsValue AS Cv3 ON TC.CharacteristicsValueId=Cv3.Id	
                                WHERE MOI.MasterOrderId='" + masterOrderId + "'";


                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }
        #endregion
        #region new Function by taufik



        public IEnumerable<object> GetSupervisorCbo()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select E.SystemId As Value, (E.Employeecode+'-'+ E.EmployeeName) As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='PurchaseOrderCheckedBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetIssueSlipCheckByCbo()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select E.SystemId As Value, (E.Employeecode+'-'+ E.EmployeeName) As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='IssueSlipCheckedBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetSupervisorCboApproved()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select E.SystemId As Value, (E.EmployeeCode+'-'+E.EmployeeName) As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='PurchaseOrderApproveBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }



        public IEnumerable<object> GetSupervisorCboApproved1()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select E.SystemId As Value, (E.EmployeeCode+'-'+E.EmployeeName )As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='RequisitionApproveBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        #endregion
        #region PO By Requisition

        public IEnumerable<object> GetListForPOBYReq(string plantId, string POTypeStatus)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var Sql = "";
            if (string.IsNullOrEmpty(POTypeStatus) == true)
            {
                POTypeStatus = "Pending";
            }
            if (POTypeStatus == "Pending")
            {

                //Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                //                     SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
                //                                , REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
                //                                --,IR.PODate
                //                                , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
                //                       , CP.UserName AS PartyAccountGroupName
                //                             , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
                //                             --, IR.GateEntryNo
                //                                --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                //                                , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
                //                             , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
                //                             , IR.FixedAssetOrInventory, IR.PODepended
                //                                --, IR.AlongwithInvoice
                //                                --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
                //                             , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
                //                             , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                //                                , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
                //					, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                //                                ,pgl.CtnId
                //                                ,IR.AddedBy
                //                                ,IR.CheckedByStatus AS CheckedByStatus,PT.PaymentMode
                //                       ,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy
                //                    FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                //                    LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                //                       ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId


                //                    LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                //                    LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy

                //                    JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                //                    JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                //                    LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                //                    LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                //                    LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                //                    LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                //                    LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                //                    LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                //                    LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                //                    LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
                //		LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                //                    LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
                //                          JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                //                    LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
                //                          WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                //                    LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                //                    LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                //                    WHERE  IR.PlantId='" + identity.PlantId + @"' AND IR.POType='POByReq' AND IR.PlantId='" + plantId + "' AND IR.CheckedByStatus='Pending' AND isnull(IR.IsClosed,0)=0 Order by IR.PODate DESC";//IR.AddedBy='" + identity.Name + "' And
                //																																																	//return _sqlRepository.GetDataCollection(Sql);

                Sql = @"--DECLARE @plantId VARCHAR(10)='20171';
								Select * from(
								SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
										, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
										--,IR.PODate
										, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
										, CP.UserName AS PartyAccountGroupName
										, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
										--, IR.GateEntryNo
										--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
										, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
										, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
										, IR.FixedAssetOrInventory, IR.PODepended
										--, IR.AlongwithInvoice
										--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
										, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
										, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
										, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
										, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
										,pgl.CtnId
										,IR.AddedBy,IR.AddedDate
										,IR.CheckedByStatus AS CheckedByStatus,PT.PaymentMode
										,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById,PO.RequisitionId
                                        ,DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END,isnull(IR.Tolerance,0) Tolerance
								FROM [TRN].[PurchaseOrder] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
								LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
										ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                         
								LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
								LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy

								left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
								left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
								LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
								LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
								LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
								LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
								LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
								LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
								LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
								LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
								LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
								LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
										JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
								LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
										WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
								LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
								LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                                LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
								WHERE  IR.PlantId='" + identity.PlantId + @"' AND IR.POType='POByReq' 					
								AND IR.CheckedBy IS NOT NULL 
								AND IR.CheckedByStatus='Pending' 
								AND isnull(IR.IsClosed,0)=0 

								UNION ALL
								SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
										, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
										--,IR.PODate
										, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
										, CP.UserName AS PartyAccountGroupName
										, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
										--, IR.GateEntryNo
										--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
										, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
										, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
										, IR.FixedAssetOrInventory, IR.PODepended
										--, IR.AlongwithInvoice
										--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
										, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
										, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
										, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
										, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
										,pgl.CtnId
										,IR.AddedBy,IR.AddedDate
										,IR.CheckedByStatus AS CheckedByStatus,PT.PaymentMode
										,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById,PO.RequisitionId
                                         ,DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END,isnull(IR.Tolerance,0) Tolerance
								FROM [TRN].[PurchaseOrder] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
								LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
										ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                         
								LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
								LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy

								left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
								left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
								LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
								LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
								LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
								LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
								LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
								LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
								LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
								LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
								LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
								LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
										JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
								LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
										WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
								LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
								LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                                LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
								WHERE  IR.PlantId='" + identity.PlantId + @"' AND IR.POType='POByReq' 					
								AND IR.Id not in(Select distinct POId from trn.InventoryReceiveDetail where POId is not null)--and RequisitionId='110232'
								AND IR.CheckedByStatus IS NULL 
								AND IR.AuthorizedByStatus IS NULL		
								AND isnull(IR.IsClosed,0)=0 

								UNION ALL
								SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
										, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
										--,IR.PODate
										, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
										, CP.UserName AS PartyAccountGroupName
										, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
										--, IR.GateEntryNo
										--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
										, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
										, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
										, IR.FixedAssetOrInventory, IR.PODepended
										--, IR.AlongwithInvoice
										--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
										, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
										, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
										, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
										, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
										,pgl.CtnId
										,IR.AddedBy,IR.AddedDate
										,IR.CheckedByStatus AS CheckedByStatus,PT.PaymentMode
										,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById,PO.RequisitionId
                                         ,DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END,isnull(IR.Tolerance,0) Tolerance
								FROM [TRN].[PurchaseOrder] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
								LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
										ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                         
								LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
								LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy

								left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
								left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
								LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
								LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
								LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
								LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
								LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
								LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
								LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
								LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
								LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
								LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
										JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
								LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
										WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
								LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
								LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                                LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
								WHERE  IR.PlantId='" + identity.PlantId + @"' AND IR.POType='POByReq' 					
								AND IR.CheckedByStatus Is null					
								AND IR.AuthorizedByStatus='For Approval'
								AND isnull(IR.IsClosed,0)=0 
								)x
								Order by PODate ASC";
            }
            else if (POTypeStatus == "CheckedHoldRej")
            {



                Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
                                    --,IR.PODate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,pgl.CtnId
                                    ,IR.AddedBy,IR.AddedDate
                                    ,IR.CheckedByStatus AS CheckedByStatus,PT.PaymentMode
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy,PO.RequisitionId
                                     ,DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                        FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                          
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy

                        left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
		                            JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                        LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
                        WHERE  IR.PlantId='" + identity.PlantId + @"' AND IR.CheckedByStatus='Hold' OR IR.CheckedByStatus='Reject' AND IR.POType='POByReq' AND IR.PlantId='" + plantId + "'   AND isnull(IR.IsClosed,0)=0 Order by IR.PODate DESC";//IR.AddedBy='" + identity.Name + "' And
                                                                                                                                                                                                                                                   //return _sqlRepository.GetDataCollection(Sql);

            }
            else if (POTypeStatus == "Checked")
            {



                Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
                                    --,IR.PODate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,pgl.CtnId
                                    ,IR.AddedBy,IR.AddedDate
                                    ,IR.CheckedByStatus AS CheckedByStatus,PT.PaymentMode
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy,PO.RequisitionId
                                     ,DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END
                        FROM [TRN].[PurchaseOrder] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                          
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy

                        left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
		                            JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                        LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
                        WHERE  IR.PlantId='" + identity.PlantId + @"'  
						AND IR.CheckedBy IS NOT NULL 
						AND IR.CheckedByStatus='Checked' 
						AND IR.AuthorizedBy IS NOT NULL 
						AND IR.AuthorizedByStatus='For Approval'  
						AND IR.POType='POByReq' 
						AND IR.PlantId='" + plantId + "'   " +
                        "AND isnull(IR.IsClosed,0)=0 " +
                        "Order by IR.PODate DESC";


            }
            return _sqlRepository.GetDataCollection(Sql);
        }
        public IEnumerable<object> GetListForPOBYReq1(string plantId, string ApproveRejectHold)
        {
            var Sql = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (ApproveRejectHold == "Approval")
            {



                Sql = @"--DECLARE @plantId VARCHAR(10)='20171';
				Select top(600) * from (
				SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
						, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
						--,IR.PODate
						, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
						, CP.UserName AS PartyAccountGroupName
						, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
						--, IR.GateEntryNo
						--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
						, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
						, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
						, IR.FixedAssetOrInventory, IR.PODepended
						--, IR.AlongwithInvoice
						--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
						, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
						, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
						, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
						, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
						,pgl.CtnId
						--,IR.AddedBy
						,IR.CheckedByStatus AS CheckedByStatus,PT.PaymentMode
						,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy,eI2.EmployeeName As Addedby,PO.RequisitionId
				FROM [TRN].[PurchaseOrder] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
				LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
						ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId   
				LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
				LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
				LEFT JOIN dbo.EmployeeInformation eI2 ON eI2.SystemId=IR.AddedBy
				LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
				LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
				LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
				LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
				LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
				LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
				LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
				LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
				LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
				LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
				LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
				LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
						JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
				LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
						WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
				LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
				LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
				WHERE  IR.POType='POByReq' 
				AND IR.PlantId='" + plantId + @"' 
				AND IR.CheckedBy IS NOT NULL 
				AND IR.CheckedByStatus='Checked' 
				AND IR.AuthorizedBy IS NOT NULL 
				AND IR.AuthorizedByStatus='Approved' 
				AND isnull(IR.IsClosed,0)=0 

				UNION ALL

				SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
						, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
						--,IR.PODate
						, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
						, CP.UserName AS PartyAccountGroupName
						, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
						--, IR.GateEntryNo
						--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
						, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
						, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
						, IR.FixedAssetOrInventory, IR.PODepended
						--, IR.AlongwithInvoice
						--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
						, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
						, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
						, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
						, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
						,pgl.CtnId
						--,IR.AddedBy
						,IR.CheckedByStatus AS CheckedByStatus,PT.PaymentMode
						,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy,eI2.EmployeeName As Addedby,PO.RequisitionId
				FROM [TRN].[PurchaseOrder] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
				LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
						ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId   
				LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
				LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
				LEFT JOIN dbo.EmployeeInformation eI2 ON eI2.SystemId=IR.AddedBy
				LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
				LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
				LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
				LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
				LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
				LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
				LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
				LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
				LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
				LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
				LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
				LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
						JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
				LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
						WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
				LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
				LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
				WHERE  IR.POType='POByReq' 
				AND IR.PlantId='" + plantId + @"'
				AND IR.CheckedByStatus  Is null
				AND IR.AuthorizedByStatus='Approved'
				AND isnull(IR.IsClosed,0)=0 

				UNION ALL

				SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
						, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
						--,IR.PODate
						, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
						, CP.UserName AS PartyAccountGroupName
						, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
						--, IR.GateEntryNo
						--, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
						, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
						, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
						, IR.FixedAssetOrInventory, IR.PODepended
						--, IR.AlongwithInvoice
						--, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
						, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
						, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
						, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
						, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
						,pgl.CtnId
						--,IR.AddedBy
						,IR.CheckedByStatus AS CheckedByStatus,PT.PaymentMode
						,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy,eI2.EmployeeName As Addedby,PO.RequisitionId
				FROM [TRN].[PurchaseOrder] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
				LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
						ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId   
				LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
				LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
				LEFT JOIN dbo.EmployeeInformation eI2 ON eI2.SystemId=IR.AddedBy
				LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
				LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
				LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
				LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
				LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
				LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
				LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
				LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
				LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
				LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
				LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
				LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
						JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
				LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
						WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
				LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
				LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
				WHERE  IR.POType='POByReq' 
				AND IR.PlantId='" + plantId + @"'
				AND IR.Id in(Select distinct POId from trn.InventoryReceive where POId is not null)--and RequisitionId='110232'
				AND IR.CheckedByStatus IS NULL
				AND IR.AuthorizedByStatus IS NULL
				AND isnull(IR.IsClosed,0)=0 
				)x
				Order by  CONVERT(datetime,X.PODate) desc";

            }
            else
            {
                Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
                                    --,IR.PODate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,pgl.CtnId
                                    ,IR.AddedBy
                                    ,IR.CheckedByStatus AS CheckedByStatus,PT.PaymentMode
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy,eI2.EmployeeName As Addedby,PO.RequisitionId
                        FROM [TRN].[PurchaseOrder] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                          
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
					    LEFT JOIN dbo.EmployeeInformation eI2 ON eI2.SystemId=IR.AddedBy

                        left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
		                            JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                        LEFT JOIN(
											select PDAMAP.InventoryReceiveId
											,RequisitionId=STUFF((select distinct ','+xpo.Id from
											trn.MaterialRequsitionMaster xpo
											INNER JOin TRN.PurchaseOrderDetail xPDAMAP on xpo.Id=xPDAMAP.RequisitionId
											where xPDAMAP.InventoryReceiveId=PDAMAP.InventoryReceiveId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
											from  TRN.PurchaseOrderDetail PDAMAP 
											LEFT JOIN [TRN].MaterialRequsitionMaster IR ON IR.Id = PDAMAP.RequisitionId
											group by  PDAMAP.InventoryReceiveId		
								)PO ON PO.InventoryReceiveId = IRD.InventoryReceiveId
                        WHERE  IR.POType='POByReq' AND IR.PlantId='" + plantId + "'  AND IR.CheckedBy IS NOT NULL  AND IR.CheckedByStatus = 'Checked'	AND IR.AuthorizedBy IS NOT NULL And IR.AuthorizedByStatus <> 'Approved'	And IR.AuthorizedByStatus <> 'For Approval' AND isnull(IR.IsClosed,0)=0   Order by IR.PODate DESC";//IR.AddedBy='" + identity.Name + "' AND 


            }
            return _sqlRepository.GetDataCollection(Sql);
        }



        public IEnumerable<object> GetRequisitionList(string RequisitionId)
        {
            try
            {
                var sql = @"SELECT    MOI.Id
                    , MOI.MasterOrderId
                    , MOI.InquiryItemId
                    , MOI.SampleItemId
                    , MOI.TestingStandardId
                    , MGM.UserName AS MaterialGroupMasterName
                    , MOI.MaterialMasterId
                    , MM.UserName AS UserName	

                    , MOI.ArticleId
                    , ART.StandardName 

	                    , FC.CharacteristicsId As FirstCharacteristicsId, C.UserName AS FirstCharacteristics
                        , Cv1.CharacteristicsId As FirstCharacteristicsValueId, Cv1.UserName AS FirstCharacteristicsValue

                        , SC.CharacteristicsId AS SecondCharacteristicsId, C2.UserName AS SecondCharacteristics
                        , Cv2.CharacteristicsId SecondCharacteristicsValueId, Cv2.UserName AS SecondCharacteristicsValue

                        , TC.CharacteristicsId ThirdCharacteristicsId, C3.UserName AS ThirdCharacteristics
                        , Cv3.CharacteristicsId AS ThirdCharacteristicsValueId, Cv3.UserName AS ThirdCharacteristicsValue
	                    ,MOI.TotalQty AS OrderQty
	                ,0 PORaised
	                ,0 AS TransactionQty
	                ,0 BalanceQty	 
	                    , MM.SalesOrderUOMId AS TransactionUoMId
	                    ,MM.BaseUOMId AS TransactionUoM
                        --, ROUND(IRD.TransactionQty,2) TransactionQty, IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM, ROUND(IRD.TransactionRate,2) TransactionRate , CU.Code AS CurrencyName, IR.ToCurrencyRate

                    --, MOI.Code
                    , MOI.BuyerReferenceNo
                    , MOI.OwnReferenceNo
    
                    , MOI.OrderWastagePercentage
                    , MOI.ExtraOrderPercentage
                    , MM.HSNCodeId
                    , ISNULL(HART.HasAttribute,CAST(0 AS BIT)) AS HasAttribute
                    , ISNULL((select sum(SO.Qty) from TRN.SalesOrder SO where So.MasterOrderItemId = MOI.Id),0) as SOQty,MOI.Type
                    FROM TRN.MasterOrderItem AS MOI
                    JOIN MST.MaterialMaster AS MM ON MOI.MaterialMasterId=MM.Id
                    LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                    LEFT JOIN MST.MaterialMasterArticle AS ART ON MOI.ArticleId=ART.Id
                    LEFT JOIN (SELECT AttributeSetLength=CASE WHEN COUNT(MaterialMasterId)>0THEN COUNT(MaterialMasterId) ELSE 0 END
                        , HasAttribute=CASE WHEN COUNT(MaterialMasterId)>0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END, MaterialMasterId
   
                    FROM MST.MaterialMasterAttribute GROUP BY MaterialMasterId) AS HART ON HART.MaterialMasterId=MM.Id
	                LEFT Join trn.SalesOrder SO on so.MasterOrderItemId=MOI.Id

	                Left JOin [TRN].[FirstCharacteristics] FC on FC.SalesOrderId=So.Id
	                LEFT JOIN HKP.Characteristics AS C ON FC.CharacteristicsId=C.Id
	                LEFT JOIN HKP.CharacteristicsValue AS Cv1 ON FC.CharacteristicsValueId=Cv1.Id

	                Left JOin [TRN].[SecondCharacteristics] SC on SC.SalesOrderId=So.Id
	                LEFT JOIN HKP.Characteristics AS C2 ON SC.CharacteristicsId=C2.Id
	                LEFT JOIN HKP.CharacteristicsValue AS Cv2 ON SC.CharacteristicsValueId=Cv2.Id

	                Left JOin [TRN].[ThirdCharacteristics] TC on TC.SalesOrderId=So.Id
	                LEFT JOIN HKP.Characteristics AS C3 ON TC.CharacteristicsId=C3.Id
	                LEFT JOIN HKP.CharacteristicsValue AS Cv3 ON TC.CharacteristicsValueId=Cv3.Id	
                WHERE MOI.MasterOrderId='" + RequisitionId + "'";


                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }
        public DataTable GetListForRequisition(string CompanyId)
        {
            try
            {

                var sql = @"select    * from  (SELECT  IR.Id As RequisitionId 
								, IM.Id  AS RequisitionDetailId  
								, ISNULL(MGM.UserName,'') AS MaterialGroupMasterName
								, IM.MaterialMasterId, MM.UserName
								, IM.ArticleId, ART.StandardName
								, IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
								, IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
								, IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
								, IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
								, IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
								, IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
								, ROUND(IM.TransactionQty,2) ReqQty
								,ISNULL(PORaisedQty,0) AS PORaisedQty
								,(ROUND(IM.TransactionQty,2)-ISNULL(PORaisedQty,0)) TransactionQty
                          
								, IM.TransactionUoMId
								, TUoM.UserName AS TransactionUoM
								, '' TransactionRate 
								, CU.Code AS CurrencyName

								, ROUND((IM.TransactionQty * IM.EstimatedRate),2) AS TrnAmount   
								,IM.MaterialDetail
								,Replace(CONVERT(VARCHAR(11), IM.DeliveryDate, 106), ' ', '-') DeliveryDate

								,Act.Id As Activity
								,Act.UserName As ActivityName
								,IM.BudgetType
								,IM.Reason
								,IM.Remarks
								,IM.FutureReqApp
								--,BudgetMasterId
								--,GLGeneralInfoId
								,null CheckedStatus   
								,null TaxList
							--,(ISNULL(IM.TransactionQty + ISNULL(PORaisedQty,0),0)-ROUND(IM.TransactionQty,2)) AS BalanceQty
								,(ROUND(IM.TransactionQty,2)-ISNULL(PORaisedQty,0)) AS BalanceQty
							,MM.HSNCodeId	
							--,IM.DeliveryDate
							,EI.EmployeeName PreparedBy
							,IM.POQtyStatus
							,convert(bit,0) WantToClose
							,IR.InActive
							,IR.CheckedByStatus
							,IR.AuthorizedByStatus,MM.IsOriginApplicable
							FROM TRN.MaterialRequsitionDetails AS IM
							left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
							LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
							LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
							LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
							LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
							LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
							LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id

							JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IM.TransactionUoMId=TUoM.Id
							JOIN [TRN].[MaterialRequsitionMaster] AS IR ON IM.MaterialReqqusitionMasterId=IR.Id
							JOIN [SCS].[Currency] AS CU ON IM.CurrencyId=CU.Id 
							JOIN [HKP].[Activity] As Act On ACT.Id=IM.ActivityId
							LEFT JOIN (select PORD.RequisitionDetailId,  Sum(PORD.TransactionQty) as PORaisedQty 
										FROM trn.PurchaseOrderDetail POD
										INNER JOIN TRN.PoRequisitionDetail PORD ON PORD.PoDetailId=POD.Id
										Group By PORD.RequisitionDetailId) AS Pre on pre.RequisitionDetailId=IM.id
								LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.ReqEmpId
							WHERE  Isnull(IR.InActive,0)=0 AND Isnull(IM.POQtyStatus,0)=0 
							AND IR.CheckedByStatus is null AND IR.AuthorizedByStatus is null							
							AND IM.CompanyGroupId='" + CompanyId + @"' and IM.MaterialMasterId is not null 

							UNION ALL

							SELECT IR.Id As RequisitionId 
								, IM.Id  AS RequisitionDetailId  
									, ISNULL(MGM.UserName,'') AS MaterialGroupMasterName
								, IM.MaterialMasterId, MM.UserName
								, IM.ArticleId, ART.StandardName
								, IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
								, IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
								, IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
								, IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
								, IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
								, IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
								, ROUND(IM.TransactionQty,2) ReqQty
								,ISNULL(PORaisedQty,0) AS PORaisedQty
								,(ROUND(IM.TransactionQty,2)-ISNULL(PORaisedQty,0)) TransactionQty
                          
								, IM.TransactionUoMId
								, TUoM.UserName AS TransactionUoM
								, '' TransactionRate 
								, CU.Code AS CurrencyName

								, ROUND((IM.TransactionQty * IM.EstimatedRate),2) AS TrnAmount   
								,IM.MaterialDetail
								,Replace(CONVERT(VARCHAR(11), IM.DeliveryDate, 106), ' ', '-') DeliveryDate

								,Act.Id As Activity
								,Act.UserName As ActivityName
								,IM.BudgetType
								,IM.Reason
								,IM.Remarks
								,IM.FutureReqApp
								--,BudgetMasterId
								--,GLGeneralInfoId
								,null CheckedStatus   
								,null TaxList
							--,(ISNULL(IM.TransactionQty + ISNULL(PORaisedQty,0),0)-ROUND(IM.TransactionQty,2)) AS BalanceQty
								,(ROUND(IM.TransactionQty,2)-ISNULL(PORaisedQty,0)) AS BalanceQty
							,MM.HSNCodeId	
							--,IM.DeliveryDate
							,EI.EmployeeName PreparedBy
							,IM.POQtyStatus
							,convert(bit,0) WantToClose
							,IR.InActive
							,IR.CheckedByStatus
							,IR.AuthorizedByStatus,MM.IsOriginApplicable
							FROM TRN.MaterialRequsitionDetails AS IM
							left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
							LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
							LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
							LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
							LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
							LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
							LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id

							LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IM.TransactionUoMId=TUoM.Id
							LEFT JOIN [TRN].[MaterialRequsitionMaster] AS IR ON IM.MaterialReqqusitionMasterId=IR.Id
							LEFT JOIN [SCS].[Currency] AS CU ON IM.CurrencyId=CU.Id 
							JOIN [HKP].[Activity] As Act On ACT.Id=IM.ActivityId
							LEFT JOIN (select PORD.RequisitionDetailId,  Sum(PORD.TransactionQty) as PORaisedQty 
										FROM trn.PurchaseOrderDetail POD
										INNER JOIN TRN.PoRequisitionDetail PORD ON PORD.PoDetailId=POD.Id
										Group By PORD.RequisitionDetailId) AS Pre on pre.RequisitionDetailId=IM.id
								LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.ReqEmpId
							WHERE  Isnull(IR.InActive,0)=0 AND Isnull(IM.POQtyStatus,0)=0 
							AND IR.CheckedByStatus is null AND  IR.AuthorizedByStatus='Approved'
							AND IM.CompanyGroupId='" + CompanyId + @"' and IM.MaterialMasterId is not null 

							UNION ALL

							SELECT IR.Id As RequisitionId 
								, IM.Id  AS RequisitionDetailId  
									, ISNULL(MGM.UserName,'') AS MaterialGroupMasterName
								, IM.MaterialMasterId, MM.UserName
								, IM.ArticleId, ART.StandardName
								, IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
								, IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
								, IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
								, IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
								, IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
								, IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
								, ROUND(IM.TransactionQty,2) ReqQty
								,ISNULL(PORaisedQty,0) AS PORaisedQty
								,(ROUND(IM.TransactionQty,2)-ISNULL(PORaisedQty,0)) TransactionQty
                          
								, IM.TransactionUoMId
								, TUoM.UserName AS TransactionUoM
								, '' TransactionRate 
								, CU.Code AS CurrencyName

								, ROUND((IM.TransactionQty * IM.EstimatedRate),2) AS TrnAmount   
								,IM.MaterialDetail
								,Replace(CONVERT(VARCHAR(11), IM.DeliveryDate, 106), ' ', '-') DeliveryDate

								,Act.Id As Activity
								,Act.UserName As ActivityName
								,IM.BudgetType
								,IM.Reason
								,IM.Remarks
								,IM.FutureReqApp
								--,BudgetMasterId
								--,GLGeneralInfoId
								,null CheckedStatus   
								,null TaxList
							--,(ISNULL(IM.TransactionQty + ISNULL(PORaisedQty,0),0)-ROUND(IM.TransactionQty,2)) AS BalanceQty
								,(ROUND(IM.TransactionQty,2)-ISNULL(PORaisedQty,0)) AS BalanceQty
							,MM.HSNCodeId	
							--,IM.DeliveryDate
							,EI.EmployeeName PreparedBy
							,IM.POQtyStatus
							,convert(bit,0) WantToClose
							,IR.InActive
							,IR.CheckedByStatus
							,IR.AuthorizedByStatus,MM.IsOriginApplicable
							FROM TRN.MaterialRequsitionDetails AS IM
							left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
							LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
							LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
							LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
							LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
							LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
							LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id

							JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IM.TransactionUoMId=TUoM.Id
							JOIN [TRN].[MaterialRequsitionMaster] AS IR ON IM.MaterialReqqusitionMasterId=IR.Id
							JOIN [SCS].[Currency] AS CU ON IM.CurrencyId=CU.Id 
							JOIN [HKP].[Activity] As Act On ACT.Id=IM.ActivityId
							LEFT JOIN (select PORD.RequisitionDetailId,  Sum(PORD.TransactionQty) as PORaisedQty 
										FROM trn.PurchaseOrderDetail POD
										INNER JOIN TRN.PoRequisitionDetail PORD ON PORD.PoDetailId=POD.Id
										Group By PORD.RequisitionDetailId) AS Pre on pre.RequisitionDetailId=IM.id
								LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.ReqEmpId
							WHERE  Isnull(IR.InActive,0)=0 AND Isnull(IM.POQtyStatus,0)=0 
							AND IR.CheckedByStatus='Checked' AND  IR.AuthorizedByStatus='Approved'	
							AND IM.CompanyGroupId='" + CompanyId + @"' and IM.MaterialMasterId is not null 
							)x
							Order By x.StandardName DESC
							";
                return _sqlRepository.GetDataTable(sql);
            }//Order by MGM.UserName , IM.MaterialMasterId, MM.UserName, IM.ArticleId, ART.StandardName, IM.FirstCharacteristicsId, FC.UserName , IM.FirstCharacteristicsValueId, FCV.UserName , IM.SecondCharacteristicsId, SC.UserName , IM.SecondCharacteristicsValueId, SCV.UserName ,IM.ThirdCharacteristicsId, TC.UserName,IM.ThirdCharacteristicsValueId, TCV.UserName DESC
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public IEnumerable<object> GetListForRequisition1(string CompanyId)
        {
            try
            {
                var sql = @"SELECT     
                            PurchaseOrderGroupName=Case when pog.UserName <> '' then pog.UserName else 'N/A' end,MGM.UserName AS MaterialGroupMasterName
                            , MM.UserName
                            , ART.StandardName  
                            ,IM.MaterialDetail,IM.Id RequisitionDetailId,EI.EmployeeName AddedBy,REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS AddedDate
                            FROM TRN.MaterialRequsitionDetails AS IM
                            JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                            LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                            left JOIN [TRN].[MaterialRequsitionMaster] AS IR ON IM.MaterialReqqusitionMasterId=IR.Id
                            Left JOIN [TRN].[PurchaseOrderGroupDetails] pogd ON pogd.MaterialMasterId=MM.Id AND pogd.ArticleId=Art.Id 
	                        LEFT JOIN [TRN].[PurchaseOrderGroup] pog ON pog.Id=pogd.PurchaseOrderGroupId
                           Left JOIN employeeInformation EI On EI.SystemId=IR.ReqEmpId
                            WHERE IM.POQtyStatus=0 AND IR.AuthorizedByStatus='Approved'
                            AND IM.CompanyGroupId='" + CompanyId + "' " +
                            "group by MGM.UserName,MM.UserName,ART.StandardName,IM.MaterialDetail, pog.UserName,IM.Id ,EI.EmployeeName,IR.AddedDate  " +
                            "Union ALL" +
                            " SELECT   '' PurchaseOrderGroupName,'' AS MaterialGroupMasterName, '' As UserName, '' As StandardName,IM.MaterialDetail,IM.Id RequisitionDetailId,EI.EmployeeName AddedBy,REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS AddedDate FROM TRN.MaterialRequsitionDetails AS IM  JOIN [TRN].[MaterialRequsitionMaster] AS IR ON IM.MaterialReqqusitionMasterId=IR.Id Left JOIN employeeInformation EI On EI.SystemId=IR.ReqEmpId WHERE IM.POQtyStatus=0 AND IR.AuthorizedByStatus='Approved' AND IM.MaterialMasterId is null ANd IM.ArticleId Is null AND IM.CompanyGroupId='" + CompanyId + "'  group by IM.MaterialDetail,IM.Id,EI.EmployeeName,IR.AddedDate Order By AddedDate";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }


        public IEnumerable<object> GetTaxCategoryListPOBYReq(string receiveDetailId)
        {
            try
            {
                var sql = @"SELECT A.Id,A.Id AS TaxId,A.InventoryReceiveId, A.TaxCategoryId, TC.UserName AS TaxCategory, A.HSNCodeId, HN.Code AS HSNCode, A.[Percentage], A.TaxAmount,d.id As PODetailId
                            FROM [TRN].[PurchaseOrderTax] AS A JOIN [MST].[TaxCategory] AS TC ON A.TaxCategoryId=TC.Id
                            LEFT JOIN [HKP].[HSNCode] AS HN ON A.HSNCodeId=HN.Id
							left join TRN.PurchaseOrderDetail d on d.id= A.InventoryReceiveDetailId
                            WHERE A.InventoryReceiveId='" + receiveDetailId + "' AND A.InventoryServiceId IS NULL ORDER BY TC.[Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        #endregion
        public void GePurchaseOrderReportByReq(string companyGroupId, string companyId, string plantId, string userId, string purchaseOrderId)
        {
            ReportUtility ru = new ReportUtility();
            var fileName = "";
            var strPath = "";
            var File = "";
            fileName = "PurchaseOrder" + plantId + ".docx";
            strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }
            ////A opens input document.
            WordDocument document = new WordDocument(File, FormatType.Docx);
            //Gets the paragraph at index 1
            try
            {
                string invoicePartyAddress = "";
                string vendorPartyAddress = "";
                WSection section = document.Sections[0];
                DataTable dsOrderMaster, dsServiceItems;
                dsOrderMaster = loadOrderMasterReq(purchaseOrderId);//sql
                Dictionary<string, string> columns = new Dictionary<string, string>();
                var poApprovedStatus = "";
                if (string.IsNullOrEmpty(dsOrderMaster.Rows[0]["IsApproved"].ToString()) == false)//IsApproved
                {
                    if (Convert.ToBoolean(dsOrderMaster.Rows[0]["IsApproved"]) == false)
                    {
                        poApprovedStatus = "Unapproved";
                        document.Replace("{PurOrApprovedStatus}", poApprovedStatus, true, true);
                    }
                    else
                    {
                        var poApprovedDT = _sqlRepository.GetDataTable(@"select Count(*) ApproveNumber from trn.PurchaseOrderApprovalLog where POID = '" + purchaseOrderId + @"' and [Status] = 'Approved'");
                        poApprovedStatus = "Approved(" + poApprovedDT.Rows[0]["ApproveNumber"] + ")";
                        document.Replace("{PurOrApprovedStatus}", poApprovedStatus, true, true);
                    }
                }
                invoicePartyAddress = ru.GetAddress(dsOrderMaster.Rows[0]["InvoicePartyAddressMasterId"].ToString(), dsOrderMaster.Rows[0]["InvoicingByAddress"].ToString());
                document.Replace("{InvoicingPartyAddress}", invoicePartyAddress, false, false);
                vendorPartyAddress = ru.GetAddress(dsOrderMaster.Rows[0]["VendorAddressMasterId"].ToString(), "");
                document.Replace("{VendorAddress}", vendorPartyAddress, false, false);
                document.Replace("{DeliveryInstruction}", dsOrderMaster.Rows[0]["DeliveryInstruction"].ToString(), false, false);
                document.Replace("{SpecialInstruction}", dsOrderMaster.Rows[0]["SpecialInstruction"].ToString(), false, false);
                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);
                dsServiceItems = loadServicerMasterItemsReq(purchaseOrderId);
                var materialTotal = makeMaterialDetailsTable(document, dsOrderMaster, purchaseOrderId);//Material Details 
                var serviceTotal = 0.00;
                if (dsServiceItems.Rows.Count > 0)
                {
                    //{ServiceItems}
                    serviceTotal = makeServiceDetailsTable(document, dsServiceItems, purchaseOrderId);//Service Details 
                    document.Replace("{ServiceDetails}", "Service Details", true, true);
                }
                var DiscountAmount = "";
                DiscountAmount = dsOrderMaster.Rows[0]["DiscountAmount"].ToString();
                document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{DiscountAmount}", (DiscountAmount).ToString() + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{AfterDiscountTotal}", ((clsStaticInfo.dbl(materialTotal.ToString()) + clsStaticInfo.dbl(serviceTotal.ToString())) - clsStaticInfo.dbl(DiscountAmount.ToString())).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{TotalInWords}", ru.InWord(((clsStaticInfo.dbl(materialTotal.ToString()) + clsStaticInfo.dbl(serviceTotal.ToString())) - clsStaticInfo.dbl(DiscountAmount.ToString())), dsOrderMaster.Rows[0]["CurrencyId"].ToString()), true, true);


                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();
                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                //creating secondary array to prevent memory leak and accidental over-writing (Tarek Talukder-26-May-2019)
                List<string> strReplace = new List<string>();
                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());
                StringCollection strColDistinct = new StringCollection();
                for (int i = 0; i < strReplace.Count; i++)
                {
                    if (strColDistinct.Contains(strReplace[i].ToUpper()))
                        continue;
                    strColDistinct.Add(strReplace[i].ToUpper());
                    string text = strReplace[i].ToUpper();
                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        ReplaceInfo[text] = document.Replace(text, dsOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);
                //removing any unused place holder
                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "", false, false);
                }
                //Region that is for Pdf.Document
                DocToPDFConverter converter = new DocToPDFConverter();
                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();
                //Closes the instance of document objects
                document.Close();
                string Prefix = "PurchaseOrder" + purchaseOrderId;
                //Saves the PDF file 
                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);
                document.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //Closes the instance of document objects
            document.Close();
        }
        public DataTable loadOrderMasterReq(string purchaseOrderId)
        {
            string strSQL;
            try
            {
                //           strSQL = @"SELECT PO.Id PONumber
                //,PO.ContractId ContractNO 
                //,PO.PurchaseLCId LCNumber
                //,B.UserName BeneficiaryBank
                //,B.UserName OpeningBank
                //,PDAC.Id AcceptanceNo
                //                                       ,PO.CompanyGroupId
                //                                       ,PO.CompanyId
                //                                       ,Plant.GSTIN
                //						,REPLACE(Convert(VARCHAR(11), PDAC.AcceptanceDate, 106), ' ', '-') AS AcceptanceDate
                //                                       ,REPLACE(Convert(VARCHAR(11), PO.PODate, 106), ' ', '-') AS PODate
                //                                       ,PO.POType
                //                                       ,REPLACE(Convert(VARCHAR(11), PO.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
                //                                       ,REPLACE(Convert(VARCHAR(11), PO.MatureDate, 106), ' ', '-') AS MatureDate
                //                                    ,PO.InvoicingPartyPlantId
                //                                    ,INVPARTYPL.UserName InvoicingPartyName
                //                                       ,INVPARTYPL.AddressMasterId InvoicePartyAddressMasterId
                //                                       ,INVPARTYPL.GSTIN InvoicingPartyGSTIN
                //                                       ,ISNULL(PO.InvoicingByAddress,'') InvoicingByAddress
                //                                    ,PO.DeliveryByAddress
                //                                    ,DPARTYPL.UserName DeliveryParty
                //                                    ,PO.DeliveryPartyPlantId		
                //                                    ,POD.InventoryMaterialId As MaterialMasterId
                //                                    ,PO.DocRefNo
                //                                       ,REPLACE(Convert(VARCHAR(11), PO.DocDate, 106), ' ', '-') AS DocDate
                //						,eI.EmployeeName CheckedBy
                //						,eI1.EmployeeName AuthorizedBy
                //						,eI3.EmployeeName AddedBy
                //                                    ,PO.AddedDate
                //                                    ,PO.UpdatedBy
                //                                    ,PO.UpdatedDate
                //                                    ,PO.IsApproved 
                //                                    ,PO.PartyType
                //                                    ,PO.PartyId
                //                                       ,PO.RefferenceNo
                //                                       ,ISNULL(PO.DeliveryInstruction,'') DeliveryInstruction
                //                                    ,ISNULL(PO.SpecialInstruction,'') SpecialInstruction
                //                                    ,Party.UserName VendorName
                //                                       ,Party.AddressMasterId VendorAddressMasterId
                //                                       ,Party.TINNO VendorGSTIN
                //                                    ,Case When PO.IsNonCreditable = 1 then 'NonCreditable' when Po.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
                //                                    ,PO.CurrencyId
                //                                    ,CRNC.Code AS CurrencyName
                //                                    ,PO.ToCurrencyRate
                //                                    ,BASECRNC.Code AS BaseCurrencyName
                //                                    ,PayTerm.UserName PaymentTerm
                //                                    ,MM.UserName MaterialMaster
                //                                    ,MM.MaterialGroupMasterId
                //                                    ,MGM.UserName MaterialGroupMaster
                //                                    ,POD.ArticleId
                //                                    ,MMA.StandardName Article
                //                                    ,FC.Id FirstCharId
                //                                    ,FC.UserName FirstChar
                //                                       ,POD.FirstCharacteristicsValueId
                //                                    ,FCV.UserName AS FirstCharacteristicsValue
                //                                       ,POD.SecondCharacteristicsValueId
                //                                    ,SCV.UserName AS SecondCharacteristicsValue
                //                                    ,POD.ThirdCharacteristicsValueId
                //                                    ,TCV.UserName AS ThirdCharacteristicsValue
                //                                    ,SC.Id SecondCharId
                //                                    ,SC.UserName SecondChar
                //                                    ,TC.Id ThirdCharId
                //                                    ,TC.UserName ThirdChar
                //                                    ,ROUND(POD.TransactionQty, 2) POTransactionQty
                //                                    ,ROUND(POD.TransactionRate, 2) TransactionRate
                //                                    ,ROUND((POD.TransactionQty * POD.TransactionRate), 2) AS TrnAmount
                //                                    ,POD.BaseAmount
                //                                    ,POD.TotalTaxAmount AS BaseTaxAmount
                //                                    ,TaxAmount = (
                //                                     SELECT SUM(TaxAmount)
                //                                     FROM [TRN].[PurchaseOrderTax]
                //                                     WHERE InventoryReceiveDetailId = POD.Id
                //                                     )
                //                                    ,ServiceTaxAmount = (
                //                                     SELECT SUM(TotalTaxAmount)
                //                                     FROM [TRN].[POService]
                //                                     WHERE InventoryReceiveId = POD.InventoryReceiveId
                //                                     )
                //                                       ,POD.Description
                //                                    ,POD.ChargesAmount
                //                                    ,POD.CountryId
                //                                    ,POCountry.UserName CountryOfOrigin
                //                                       ,POD.Id PurchaseOrderDetailId
                //                                    ,POD.TransactionUoMId
                //                                    ,TUoM.UserName AS TransactionUoM
                //                                       ,MRMD.MaterialDetail,REPLACE(Convert(VARCHAR(11), POD.DeliveryDate, 106), ' ', '-') AS DeliveryDate
                //                                       ,CheckStatus= CASE when PO.CheckedByStatus='pending' Then 'To be checked'
                //                                   when PO.CheckedByStatus='Hold' Then 'Hold'
                //                                   when PO.CheckedByStatus='Reject' Then 'Reject'
                //                                   when PO.CheckedByStatus='Checked' Then 'Checked'
                //                                   else ''
                //                                   END
                //                                   ,ApproveStatus= CASE
                //                                   when PO.AuthorizedByStatus='Reject' Then 'Reject For Approved'
                //                                   when PO.AuthorizedByStatus='Hold' Then 'Hold For Approved'
                //                                   when PO.AuthorizedByStatus='For Approval' Then 'To be Approval'
                //                                   when PO.AuthorizedByStatus='Approved' Then 'Approved'
                //                                   else ''
                //                                   END
                //                                   ,isnull(PO.DiscountAmount,0) DiscountAmount
                //                                   FROM TRN.PurchaseOrder PO
                //                                   LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = PO.CompanyGroupId
                //                                   LEFT JOIN ORG.Company Cmp ON Cmp.Id = PO.CompanyId
                //                                   LEFT JOIN ORG.Plant Plant ON Plant.Id = PO.PlantId
                //                                   LEFT JOIN SCS.Currency CRNC ON CRNC.Id = PO.CurrencyId
                //                                   LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = PO.BaseCurrencyId
                //                                   LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = PO.PaymentTermId
                //                                   LEFT JOIN HKP.PartyPlant  INVPARTYPL ON INVPARTYPL.Id = PO.InvoicingPartyPlantId
                //                                   LEFT JOIN HKP.PartyPlant  DPARTYPL ON DPARTYPL.Id = PO.DeliveryPartyPlantId                                          
                //                                   LEFT JOIN TRN.PurchaseOrderDetail POD ON PO.Id = POD.InventoryReceiveId
                //					 LEFT JOIN TRN.PurchaseDocAcceptance PDAC ON PDAC.Id = PO.Id
                //					LEFT JOIN [dbo].[Contract] CNO ON CNO.Id = PO.ContractId
                //                   LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id = PO.PurchaseLCId
                //                               LEFT JOIN [HKP].[Bank] B ON B.Id = PLC.BenificiaryBankId
                //                                   LEFT JOIN SCS.Country POCountry ON POD.CountryId = POCountry.Id
                //                                   LEFT JOIN HKP.Party Party ON Party.Id = PO.PartyId                                        
                //                                   --LEFT JOIN TRN.POMaterial AS POM ON POD.InventoryMaterialId = POM.Id
                //                                   INNER JOIN MST.MaterialMaster AS MM ON MM.Id = POD.InventoryMaterialId
                //                                   INNER JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                //                                   LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = POD.ArticleId
                //                                   LEFT JOIN HKP.Characteristics AS FC ON POD.FirstCharacteristicsId = FC.Id
                //                                   LEFT JOIN HKP.Characteristics AS SC ON POD.SecondCharacteristicsId = SC.Id
                //                                   LEFT JOIN HKP.Characteristics AS TC ON POD.ThirdCharacteristicsId = TC.Id
                //                                   LEFT JOIN HKP.CharacteristicsValue AS FCV ON POD.FirstCharacteristicsValueId = FCV.Id
                //                                   LEFT JOIN HKP.CharacteristicsValue AS SCV ON POD.SecondCharacteristicsValueId = SCV.Id
                //                                   LEFT JOIN HKP.CharacteristicsValue AS TCV ON POD.ThirdCharacteristicsValueId = TCV.Id
                //                                   LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON POD.TransactionUoMId = TUoM.Id
                //                                   LEFT JOIN TRN.MaterialRequsitionDetails AS MRMD ON MRMD.Id=POD.RequisitionDetailId
                //					LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=PO.CheckedBy
                //                                   LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=PO.AuthorizedBy
                //		            LEFT JOIN [SEC].[User] U On U.UserId=PO.AddedBy
                //		            LEFT JOIN dbo.EmployeeInformation eI3 ON eI3.SystemId=U.EmployeeId
                strSQL = @"SELECT PO.Id PONumber
                , HSNC.Code HSNCode
                  , PO.ContractId ContractNO
                  , PLC.LCRef LCNumber
                  ,PLC.BenificiaryBank BeneficiaryBank
                  ,PLC.BenificiaryBank OpeningBank
                     , PO.CompanyGroupId
                    ,PO.CompanyId,PO.BaseNoOfDays
                    ,Plant.GSTIN
	                ,REPLACE(Convert(VARCHAR(11), PLC.LCDate, 106), ' ', '-') AS LCODate
                    , REPLACE(Convert(VARCHAR(11), PO.PODate, 106), ' ', '-') AS PODate
                     , POType = CASE WHEN PO.POType = 'PO' then 'PO Without Requisition' ELSE 'PO With Requisition' END
                    ,REPLACE(Convert(VARCHAR(11), PO.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
                    , REPLACE(Convert(VARCHAR(11), PO.MatureDate, 106), ' ', '-') AS MatureDate
                     , PO.InvoicingPartyPlantId
                    ,INVPARTYPL.UserName InvoicingPartyName
                    , INVPARTYPL.AddressMasterId InvoicePartyAddressMasterId
                     , INVPARTYPL.GSTIN InvoicingPartyGSTIN
                      , ISNULL(PO.InvoicingByAddress, '') InvoicingByAddress
                    ,PO.DeliveryByAddress
                    ,DPARTYPL.UserName DeliveryParty
                    , PO.DeliveryPartyPlantId
                    ,POD.InventoryMaterialId MaterialMasterId
                    , PO.DocRefNo
                    ,REPLACE(Convert(VARCHAR(11), PO.DocDate, 106), ' ', '-') AS DocDate
                    , CheckedBy = CASE WHEN PO.CheckedByStatus = 'Checked' Then eI.EmployeeName else '' END
                    ,AuthorizedBy = CASE When PO.AuthorizedByStatus = 'Approved'then eI1.EmployeeName else '' END
                    ,AddedBy = CASE When PO.CheckedByStatus = 'pending' OR PO.CheckedByStatus = 'Hold' OR PO.CheckedByStatus = 'Reject' OR PO.CheckedByStatus = 'Checked'then eI3.EmployeeName else PO.AddedBy END
                              , PO.AddedDate
                    ,PO.UpdatedBy
                    ,PO.UpdatedDate
                    ,PO.IsApproved
                    ,PO.PartyType
                    ,PO.PartyId
                    ,ISNULL(POD.RefferenceNo,'') RefferenceNo,ISNULL(POD.RefferenceNo,'') BuyerRefferenceNo
                    ,isnull(PO.DiscountAmount, 0) DiscountAmount
                    ,ISNULL(PO.DeliveryInstruction, '') DeliveryInstruction
                    ,ISNULL(PO.SpecialInstruction, '') SpecialInstruction
                    ,Party.UserName VendorName
                    , Party.AddressMasterId VendorAddressMasterId
                     , Party.TINNO VendorGSTIN
                      , Case When PO.IsNonCreditable = 1 then 'NonCreditable' when Po.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
                       , PO.CurrencyId
                    ,CRNC.Code AS CurrencyName
                    ,PO.ToCurrencyRate
                    ,BASECRNC.Code AS BaseCurrencyName
                    ,PayTerm.UserName PaymentTerm
                    , MM.UserName MaterialMaster
                     , MM.MaterialGroupMasterId
                    ,MGM.UserName MaterialGroupMaster
                    , POD.ArticleId
                    ,MMA.StandardName Article
                    , FC.Id FirstCharId
                     , FC.UserName FirstChar
                      , POD.FirstCharacteristicsValueId
                    ,FCV.UserName AS FirstCharacteristicsValue
                    ,POD.SecondCharacteristicsValueId
                    ,SCV.UserName AS SecondCharacteristicsValue
                    ,POD.ThirdCharacteristicsValueId
                    ,TCV.UserName AS ThirdCharacteristicsValue
                    ,SC.Id SecondCharId
                    , SC.UserName SecondChar
                     , TC.Id ThirdCharId
                      , TC.UserName ThirdChar
                       , ROUND(POD.TransactionQty, 2) POTransactionQty
                    ,ROUND(POD.TransactionRate, 4) TransactionRate
                    ,ROUND((POD.TransactionQty * POD.TransactionRate), 2) AS TrnAmount
                    , POD.BaseAmount
                    ,POD.TotalTaxAmount AS BaseTaxAmount
                    ,REPLACE(Convert(VARCHAR(11), POD.DeliveryDate, 106), ' ', '-') AS DeliveryDate
                    , TaxAmount = (
                    SELECT SUM(TaxAmount)
                    FROM[TRN].[PurchaseOrderTax]
                    WHERE InventoryReceiveDetailId = POD.Id
                    )
                    ,ServiceTaxAmount = (
                    SELECT SUM(TotalTaxAmount)
                    FROM[TRN].[POService]
                    WHERE InventoryReceiveId = POD.InventoryReceiveId
                    )
                    ,POD.Description
                    ,POD.ChargesAmount
                    ,POD.CountryId
                    ,POCountry.UserName CountryOfOrigin
                    , POD.Id PurchaseOrderDetailId
                     , POD.TransactionUoMId
                     ,TUoM.ShortName AS TransactionUoM
                    ,MRMD.MaterialDetail MaterialDetail
                    , CheckStatus = CASE when PO.CheckedByStatus = 'pending' Then 'To be checked'
                    when PO.CheckedByStatus = 'Hold' Then 'Hold'
                    when PO.CheckedByStatus = 'Reject' Then 'Reject'
                    when PO.CheckedByStatus = 'Checked' Then 'Checked'
                    else ''
                    END
                    ,ApproveStatus = CASE
                    when PO.AuthorizedByStatus = 'Reject' Then 'Reject For Approved'
                    when PO.AuthorizedByStatus = 'Hold' Then 'Hold For Approved'
                    when PO.AuthorizedByStatus = 'For Approval' Then 'To be Approval'
                    when PO.AuthorizedByStatus = 'Approved' Then 'Approved'
                    else ''
                    END
                    FROM TRN.PurchaseOrder PO
                    LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = PO.CompanyGroupId
                    LEFT JOIN ORG.Company Cmp ON Cmp.Id = PO.CompanyId
                    LEFT JOIN ORG.Plant Plant ON Plant.Id = PO.PlantId
                    LEFT JOIN SCS.Currency CRNC ON CRNC.Id = PO.CurrencyId
                    LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = PO.BaseCurrencyId
                    LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = PO.PaymentTermId
                    LEFT JOIN HKP.PartyPlant INVPARTYPL ON INVPARTYPL.Id = PO.InvoicingPartyPlantId
                    LEFT JOIN HKP.PartyPlant DPARTYPL ON DPARTYPL.Id = PO.DeliveryPartyPlantId
                    LEFT JOIN TRN.PurchaseOrderDetail POD ON PO.Id = POD.InventoryReceiveId

                    LEFT JOIN[dbo].[Contract] CNO ON CNO.Id = PO.ContractId

                    LEFT JOIN[dbo].[PurchaseLC] PLC ON PLC.Id = PO.PurchaseLCId

                    --LEFT JOIN[HKP].[Bank] B ON B.Id = PLC.BenificiaryBankId
                    LEFT JOIN SCS.Country POCountry ON POD.CountryId = POCountry.Id
                    LEFT JOIN HKP.Party Party ON Party.Id = PO.PartyId
                    LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = POD.InventoryMaterialId

                    LEFT JOIN[HKP].[HSNCode] AS HSNC ON HSNC.ID = MM.HSNCodeId
                    LEFT JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                    LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = POD.ArticleId
                    LEFT JOIN HKP.Characteristics AS FC ON POD.FirstCharacteristicsId = FC.Id
                    LEFT JOIN HKP.Characteristics AS SC ON POD.SecondCharacteristicsId = SC.Id
                    LEFT JOIN HKP.Characteristics AS TC ON POD.ThirdCharacteristicsId = TC.Id
                    LEFT JOIN HKP.CharacteristicsValue AS FCV ON POD.FirstCharacteristicsValueId = FCV.Id
                    LEFT JOIN HKP.CharacteristicsValue AS SCV ON POD.SecondCharacteristicsValueId = SCV.Id
                    LEFT JOIN HKP.CharacteristicsValue AS TCV ON POD.ThirdCharacteristicsValueId = TCV.Id
                    LEFT JOIN[SCS].[UnitOfMeasurement] AS TUoM ON POD.TransactionUoMId = TUoM.Id
                    LEFT JOIN TRN.MaterialRequsitionDetails AS MRMD ON MRMD.Id = POD.RequisitionDetailId
                    LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId = PO.CheckedBy
                    LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId = PO.AuthorizedBy
                    left join[SEC].[User] U on U.UserId = PO.AddedBy
                    LEFT JOIN dbo.EmployeeInformation eI3 ON eI3.SystemId = U.EmployeeId
                                        WHERE PO.Id = '" + purchaseOrderId + @"' and POD.InventoryMaterialId is not null";
                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }
        public DataTable loadServicerMasterItemsReq(string purchaseOrderId)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT POS.Id ServiceId,SM.UserName  Service , POS.Description, POS.Amount,POS.TotalTaxAmount,Pos.AddedBy,pos.AddedDate,pos.UpdatedBy,pos.UpdatedDate FROM TRN.PurchaseOrder PO
                            INNER join TRN.POService POS ON POS.InventoryReceiveId = PO.Id
                            INNER JOIN HKP.ServiceMaster SM ON POS.ServiceMasterId = SM.Id 
                            where PO.Id = '" + purchaseOrderId + @"'";
                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }
        public IEnumerable<object> GetLCContractList(bool isProcurementOnBom, string plantId)
        {
            try
            {
                if (isProcurementOnBom)
                {

                    var sql = @"SELECT C.Id ContractId
                            , c.CustomerId
							,c.IsLC
							,c.AddedBy
							,c.AddedDate
							,c.AddedFromIP
							,C.UpdatedBy
							,C.UpdatedDate
							,C.UpdatedFromIP
							, P.UserName AS CustomerName
							, MLC.Id MasterLCNo
                            , MLC.LCRef
							,C.ContractNo
							,[Buyer]= STUFF((select distinct ',' + B.UserName from
                                       trn.MasterOrder XMOI
   
                                       LEFT JOIN[HKP].[Buyer] AS B ON B.Id = XMOI.BuyerId

                                    LEFT JOIN trn.MasterOrderItem AS I ON I.MasterOrderId = XMOI.Id

                                    where I.ContractId = C.Id for xml path('') ), 1, 1, ''
									)
                            ,C.UDNo,MLC.OpeningBank
                            FROM[dbo].[Contract] C
                           JOIN[HKP].[Party] AS P ON C.CustomerId = P.Id

                            LEFT JOIN[dbo].[MasterLC] MLC ON MLC.Id = C.MasterLCId--MLC ON MLC.ContractId = C.Id

                            where  (C.PlantId='" + plantId + @"' OR isnull(C.Id,'') IN(


                            select distinct isnull(so.ContractId,'') AS ContractId from BOQ
                            join trn.MasterOrderItem MOI on moi.id= BOQ.MasterOrderItemId
                            LEFT JOIN TRN.SalesOrder so on moi.Id=so.MasterOrderItemId
                            join hkp.PartyPlant P on p.PartyId= boq.VendorId

                            where P.PlantId= '" + plantId + @"'

                            union

						   select isnull(MOI.ContractId,'') from trn.MasterOrderItem MOI
						   join org.Entity E on e.id=isnull(moi.EntityIdWithinCompany,moi.EntityIdWithinGroup)
						   where Type='OutSource' and isnull(consignment,0)=0 and E.plantId='" + plantId + @"'
                            )) AND isnull(C.Id,'') NOT IN (
                                select isnull(MOI.ContractId,'') from trn.MasterOrderItem MOI
                                join trn.MasterOrder MO ON MO.Id=moi.MasterOrderId 
                                WHERE MOI.Type='OutSource' and isnull(MOI.consignment,0)=0 AND MO.plantId='" + plantId + @"'
                            )

                            ORDER BY C.CustomerId";
                    return _sqlRepository.GetDataCollection(sql);

                }
                else
                {
                    var sql = @"SELECT C.Id ContractId
							,c.CustomerId
							,c.IsLC
							,c.AddedBy
							,c.AddedDate
							,c.AddedFromIP
							,C.UpdatedBy
							,C.UpdatedDate
							,C.UpdatedFromIP
							, P.UserName AS CustomerName
							, MLC.Id MasterLCNo
							,MLC.LCRef
							,C.ContractNo
							,[Buyer]=STUFF((select distinct ','+B.UserName from
									trn.MasterOrder XMOI
									LEFT JOIN [HKP].[Buyer] AS B ON B.Id=XMOI.BuyerId
									LEFT JOIN trn.MasterOrderItem AS I ON I.MasterOrderId=XMOI.Id
                                    LEFT JOIN TRN.SalesOrder so on I.Id=so.MasterOrderItemId
									where so.ContractId=C.Id for xml path('') ), 1, 1, ''
									)
                            ,C.UDNo,MLC.OpeningBank
							FROM [dbo].[Contract] C
							JOIN [HKP].[Party] AS P ON C.CustomerId=P.Id
							LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=C.MasterLCId--MLC ON MLC.ContractId=C.Id
							 where  (C.PlantId='" + plantId + @"' OR isnull(C.Id,'') IN(
						    select isnull(so.ContractId,'') from trn.MasterOrderItem MOI
						   LEFT JOIN TRN.SalesOrder so on MOI.Id=so.MasterOrderItemId
						   join org.Entity E on e.id=isnull(moi.EntityIdWithinCompany,moi.EntityIdWithinGroup)
						   where Type='OutSource' and isnull(consignment,0)=0 and E.plantId='" + plantId + @"'
                            )) AND isnull(C.Id,'') NOT IN (
                                 select isnull(so.ContractId,'') from trn.MasterOrderItem MOI
						   LEFT JOIN TRN.SalesOrder so on MOI.Id=so.MasterOrderItemId
                                join trn.MasterOrder MO ON MO.Id=moi.MasterOrderId 
                                WHERE MOI.Type='OutSource' and isnull(MOI.consignment,0)=0 AND MO.plantId='" + plantId + @"'
                            )

                            ORDER BY C.CustomerId";
                    return _sqlRepository.GetDataCollection(sql);
                }

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetalldataPOWithLCMap(string plantId)
        {
            try
            {
                // Edit by Mizan
                var Sql = @"SELECT 
                                distinct PO.Id,REPLACE(CONVERT(CHAR(11), PO.PODate, 106),' ','-') AS PODate,PO.PartyId,
                                InvPP.StandardName ,ISNULL(PO.OrderSpecific,'') OrderSpec,ISNULL(PLC.ContractId, PO.ContractId) ContractId,PO.PurchaseLCId, CN.Code Currency,PO.CurrencyId
                                ,CONVERT(NUMERIC(10,2),POD.TransactionAmount) TransactionAmount, 0 AS [check],Flag='MaterialPO',PLC.LCRef,ISNULL(C.ContractNo,'')ContractNo,ISNULL(PO.DocRefNo,'')DocRefNo
                                FROM TRN.PurchaseOrder PO
                                INNER JOIN (SELECT SUM(TransactionAmount) TransactionAmount, InventoryReceiveId FROM [TRN].[PurchaseOrderDetail] GROUP BY InventoryReceiveId) POD ON POD.InventoryReceiveId=PO.Id
                                LEFT JOIN [HKP].[Party] AS InvPP ON PO.PartyId=InvPP.Id
                                LEFT JOIN [MST].[PaymentTerm] PT ON PT.id=PO.PaymentTermId 
                                LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=PO.PurchaseLCId 
                                LEFT JOIN SCS.Currency CN ON CN.Id=PO.CurrencyId 
	                            LEFT JOIN [dbo].[Contract] C ON C.Id=PO.ContractId
                                WHERE PO.PlantId='" + plantId + @"' AND PT.PaymentMode = 'LC' AND ISNULL(PO.PurchaseLCId,'')<>'' AND PO.IsClosed=0
                            UNION
                            SELECT 
                                distinct PO.Id,REPLACE(CONVERT(CHAR(11), PO.PODate, 106),' ','-') AS PODate,PO.PartyId,
                                InvPP.StandardName,ISNULL(PO.OrderSpecific,'') OrderSpec,ISNULL(PLC.ContractId, PO.ContractId) ContractId,PO.PurchaseLCId, CN.Code Currency,PO.CurrencyId
                                ,CONVERT(NUMERIC(10,2),POD.TransactionAmount) TransactionAmount, 0 AS [check],Flag='ServicePO',PLC.LCRef,ISNULL(C.ContractNo,'')ContractNo,ISNULL(PO.DocRefNo,'')DocRefNo
                                FROM TRN.[ServicePOMaster] PO
                                INNER JOIN (SELECT SUM(Amount) TransactionAmount, ServicePOMasterId FROM [TRN].[ServicePODetail] GROUP BY ServicePOMasterId) POD ON POD.ServicePOMasterId=PO.Id
                                LEFT JOIN [HKP].[Party] AS InvPP ON PO.PartyId=InvPP.Id
                                LEFT JOIN [MST].[PaymentTerm] PT ON PT.id=PO.PaymentTermId 
                                LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=PO.PurchaseLCId 
                                LEFT JOIN SCS.Currency CN ON CN.Id=PO.CurrencyId 
	                            LEFT JOIN [dbo].[Contract] C ON C.Id=PO.ContractId
                                WHERE PO.PlantId='" + plantId + @"' AND PT.PaymentMode = 'LC' AND ISNULL(PO.PurchaseLCId,'')<>'' AND PO.IsClosed=0

                            UNION
                            SELECT distinct PO.Id,REPLACE(CONVERT(CHAR(11), PO.PODate, 106),' ','-') AS PODate,PO.PartyId,
                                InvPP.StandardName,ISNULL(PO.OrderSpecific,'') OrderSpec,ISNULL(PLC.ContractId, PO.ContractId) ContractId,PO.PurchaseLCId, CN.Code Currency,PO.CurrencyId
                                ,CONVERT(NUMERIC(10,2),POD.TransactionAmount) TransactionAmount, 0 AS [check],Flag='OutSourcePO',PLC.LCRef,ISNULL(C.ContractNo,'')ContractNo,ISNULL(PO.DocRefNo,'')DocRefNo
                                FROM [dbo].[OSTransformationPO] PO
                                INNER JOIN (SELECT SUM(ISNULL(TransactionAmount,0)) TransactionAmount, OSTransformationPOId FROM [dbo].[OSTransformationPODetail] GROUP BY OSTransformationPOId) POD ON POD.OSTransformationPOId=PO.Id
                                LEFT JOIN [HKP].[Party] AS InvPP ON PO.PartyId=InvPP.Id
                                LEFT JOIN [MST].[PaymentTerm] PT ON PT.id=PO.PaymentTermId 
                                LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=PO.PurchaseLCId 
                                LEFT JOIN SCS.Currency CN ON CN.Id=PO.CurrencyId 
	                            LEFT JOIN [dbo].[Contract] C ON C.Id=PO.ContractId
                                WHERE PO.PlantId='" + plantId + @"' AND PT.PaymentMode = 'LC' AND ISNULL(PO.PurchaseLCId,'')<>'' AND ISNULL(PO.IsClosed,0)=0";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetalldataPOWithoutLCMap(string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var Sql = @"
                            SELECT [check]=CAST (CASE WHEN PO.PurchaseLCId IS NULL THEN 0 ELSE 1 END AS bit),
                                    PO.Id,REPLACE(CONVERT(CHAR(11), PO.PODate, 106),' ','-') AS PODate,PO.PartyId,
                                    InvPP.StandardName ,ISNULL(PO.OrderSpecific,'')OS,PO.ContractId,PO.PurchaseLCId, CN.Code Currency,PO.CurrencyId
                                    ,CONVERT(NUMERIC(10,2),POD.TransactionAmount) TransactionAmount,ISNULL(C.ContractNo,'')ContractNo ,Flag='MaterialPO',ISNULL(PO.DocRefNo,'')DocRefNo
                                    ,IsFirst=case when GRN.GRNId>0 then 0 else 1 end
                                    FROM TRN.PurchaseOrder PO
                                    INNER JOIN (SELECT SUM(TransactionAmount) TransactionAmount, InventoryReceiveId 
							        FROM [TRN].[PurchaseOrderDetail] GROUP BY InventoryReceiveId) POD ON POD.InventoryReceiveId=PO.Id
                                    LEFT JOIN [HKP].[Party] AS InvPP ON PO.PartyId=InvPP.Id
                                    LEFT JOIN [MST].[PaymentTerm] PT ON PT.id=PO.PaymentTermId 
                                    LEFT JOIN [dbo].[Contract] C ON C.Id=PO.ContractId
                                    LEFT JOIN SCS.Currency CN ON CN.Id=PO.CurrencyId 
                                    LEFT JOIN (Select PoId,COUNT(GRNId) GRNId from tRN.POGGRNMap GROUP BY PoId) GRN ON GRN.PoId=PO.Id
                                    WHERE PO.PlantId='" + plantId + @"' AND PT.PaymentMode = 'LC' AND ISNULL(PO.PurchaseLCId,'')='' AND PO.IsClosed=0  AND AuthorizedByStatus='Approved'
                            UNION 
                            SELECT [check]=CAST (CASE WHEN PO.PurchaseLCId IS NULL THEN 0 ELSE 1 END AS bit),
                                    PO.Id,REPLACE(CONVERT(CHAR(11), PO.PODate, 106),' ','-') AS PODate,PO.PartyId,
                                    InvPP.StandardName ,ISNULL(PO.OrderSpecific,'')OS,PO.ContractId,PO.PurchaseLCId, CN.Code Currency,PO.CurrencyId
                                    ,CONVERT(NUMERIC(10,2),POD.TransactionAmount) TransactionAmount,ISNULL(C.ContractNo,'')ContractNo,Flag='ServicePO',ISNULL(PO.DocRefNo,'')DocRefNo
                                    ,IsFirst=case when GRN.GRNId>0 then 0 else 1 end
                                    FROM [TRN].[ServicePOMaster] PO
                                    INNER JOIN (SELECT SUM(Amount) TransactionAmount, ServicePOMasterId 
							        FROM [TRN].[ServicePODetail] GROUP BY ServicePOMasterId) POD ON POD.ServicePOMasterId=PO.Id
                                    LEFT JOIN [HKP].[Party] AS InvPP ON PO.PartyId=InvPP.Id
                                    LEFT JOIN [MST].[PaymentTerm] PT ON PT.id=PO.PaymentTermId 
                                    LEFT JOIN [dbo].[Contract] C ON C.Id=PO.ContractId
                                    LEFT JOIN SCS.Currency CN ON CN.Id=PO.CurrencyId 
                                    LEFT JOIN (Select ServicePoId,COUNT(ServiceAckId) GRNId from tRN.ServivePOAcknowledgementMap GROUP BY ServicePoId) GRN ON GRN.ServicePoId=PO.Id
                                    WHERE PO.PlantId='" + plantId + @"' AND PT.PaymentMode = 'LC' AND ISNULL(PO.PurchaseLCId,'')='' AND PO.IsClosed=0  AND ApprovedByStatus='Approved'
                        UNION 
                        SELECT [check]=CAST (CASE WHEN PO.PurchaseLCId IS NULL THEN 0 ELSE 1 END AS bit),
                            PO.Id,REPLACE(CONVERT(CHAR(11), PO.PODate, 106),' ','-') AS PODate,PO.PartyId,
                            InvPP.StandardName ,ISNULL(PO.OrderSpecific,'')OS,PO.ContractId,PO.PurchaseLCId, CN.Code Currency,PO.CurrencyId
                            ,CONVERT(NUMERIC(10,2),POD.TransactionAmount) TransactionAmount,ISNULL(C.ContractNo,'')ContractNo,Flag='OutSourcePO',ISNULL(PO.DocRefNo,'')DocRefNo
                            --,IsFirst=case when GRN.GRNId>0 then 0 else 1 end
	                        ,0 IsFirst
                            FROM [dbo].[OSTransformationPO] PO
                            INNER JOIN (SELECT SUM(ISNULL(TransactionAmount,0)) TransactionAmount, OSTransformationPOId 
	                        FROM [dbo].[OSTransformationPODetail] GROUP BY OSTransformationPOId) POD ON POD.OSTransformationPOId=PO.Id
                            LEFT JOIN [HKP].[Party] AS InvPP ON PO.PartyId=InvPP.Id
                            LEFT JOIN [MST].[PaymentTerm] PT ON PT.id=PO.PaymentTermId 
                            LEFT JOIN [dbo].[Contract] C ON C.Id=PO.ContractId
                            LEFT JOIN SCS.Currency CN ON CN.Id=PO.CurrencyId 
                            --LEFT JOIN (Select ServicePoId,COUNT(ServiceAckId) GRNId from TRN.ServivePOAcknowledgementMap GROUP BY ServicePoId) GRN ON GRN.ServicePoId=PO.Id
                            WHERE PO.PlantId='" + plantId + @"' AND PT.PaymentMode = 'LC' AND ISNULL(PO.PurchaseLCId,'')='' AND ISNULL(PO.IsClosed,0)=0 AND PO.IsApproved=1";

                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetLCListByContract(string ContractId, string VendorId, string CurrencyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                //var wc = "";
                //if (ContractId != "null" && !string.IsNullOrEmpty(ContractId))
                //{
                //    // ContractId = "";
                //    wc = "AND ISNULL(PLC.ContractId,'')='" + ContractId + "'";
                //}
                //else
                //{
                //    wc = "";
                //}
                var Sql = "";
                Sql = @"SELECT PLC.Id Text,PLC.Id Value
	                ,PLC.ContractId
	                ,C.ContractNo
	                ,PLC.BenificiaryBank
	                ,PLC.OpeningBankMasterId
	                ,BM.AccountTitle
	                ,PLC.LeinBank
	                ,PLC.OrderSpecific
	                ,FORMAT(PLC.LCDate,'dd-MMM-yyyy') LCOpeningDate
	                ,FORMAT(PLC.ExpiryDate,'dd-MMM-yyyy') LCExpiryDate
	                ,PLC.Amount
	                ,EI.EmployeeName AS PreparedBy
	                ,PLC.FinalDestination
	                ,PLc.PortOfLandingId
	                ,prt.UserName PortOfLanding
	                ,PLC.Type
	                ,PLC.Tenure,PLC.VendorId
	                ,PLC.CurrencyId,U.UserId,PLC.AddedBy,EI.SystemId, U.EmployeeId, CN.Code Currency,PLC.LCRef,C.UDNo, MLC.LCRef MasterLCRef,CP.UserName CustomerName,P.UserName PartyName,IsAccepptanceFirst = CASE WHEN PLC.IsAccepptanceFirst= 1 THEN 1 ELSE 0 END
                FROM [dbo].[PurchaseLC] PLC
                LEFt JOIN [dbo].[Contract] C ON C.Id=PLC.ContractId
				LEFT JOIN [HKP].[Party] CP On CP.Id=C.CustomerId
				LEFt JOIN [dbo].MasterLC MLC ON MLC.Id=C.MasterLCId
                LEFT JOIN [HKP].[Party] P On P.Id=PLC.VendorId
                LEFT JOIN [MST].[BankMaster] BM On BM.Id=PLC.OpeningBankMasterId ANd BM.AccountType='HouseBank'
                LEFT JOIN [MST].[Port] Prt ON prt.id=PLC.PortOfLandingId
                LEFT JOIN [SEC].[User] U ON U.UserId=PLC.AddedBy
                LEFT JOIN EmployeeInformation EI ON EI.SystemId= U.EmployeeId
                LEFT JOIN SCS.Currency CN ON CN.Id=PLC.CurrencyId
                WHERE PLC.[Status]='Active' And PLC.VendorId='" + VendorId + @"' AND PLC.CurrencyId='" + CurrencyId + "'";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> UpdatePOforLC(string POId, string PurchaseLCId, string flag)
        {
            try
            {
                if (flag == "MaterialPO")
                {
                    var Sql = @"Update trn.purchaseOrder set PurchaseLCId='" + PurchaseLCId + "'  WHERE Id='" + POId + "'";
                    return _sqlRepository.GetDataCollection(Sql);
                }
                else if (flag == "ServicePO")
                {
                    var Sql = @"Update trn.ServicePOMaster set PurchaseLCId='" + PurchaseLCId + "'  WHERE Id='" + POId + "'";
                    return _sqlRepository.GetDataCollection(Sql);
                }
                else
                {
                    var Sql = @"Update [dbo].[OSTransformationPO] set PurchaseLCId='" + PurchaseLCId + "'  WHERE Id='" + POId + "'";
                    return _sqlRepository.GetDataCollection(Sql);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        #region Taufik PurchaseDocumentAcceptanceReport 
        public void GetPurchaseAcceptanceReport(string CompanyGroupId, string plantId, string PDACId)
        {
            ReportUtility ru = new ReportUtility();

            var fileName = "";
            var strPath = "";

            var File = "";

            fileName = "PurchaseDocAcceptance" + plantId + ".docx";
            strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }

            ////A opens input document.
            WordDocument document = new WordDocument(File, FormatType.Docx);
            //Gets the paragraph at index 1
            try
            {
                string invoicePartyAddress = "";
                string vendorPartyAddress = "";
                WSection section = document.Sections[0];
                DataTable dsOrderMaster, dsServiceItems;
                dsOrderMaster = PurchaseDocAcceptanceMasterTable(PDACId);//sql
                Dictionary<string, string> columns = new Dictionary<string, string>();
                var poApprovedStatus = "";
                //document.Replace("{Remarks}", dsOrderMaster.Rows[0]["Remarks"].ToString(), false, false);
                //document.Replace("{AddedBy}", dsOrderMaster.Rows[0]["AddedBy"].ToString(), false, false);

                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);
                dsServiceItems = PurchaseDocAcceptanceServiceTable(PDACId);
                var materialSTotal = PurchaseDocAcceptanceServiceTableHeader(document, dsServiceItems, PDACId);
                DataTable dsMaterialItems = PurchaseDocAcceptanceDetailTable(PDACId);
                var materialTotal = PurchaseDocAcceptanceDetailTableHeader(document, dsMaterialItems, PDACId);//Material Details 

                document.Replace("{GrandTotal}", (materialTotal).ToString("#,##0.00") + " " + dsMaterialItems.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{TotalInWords}", ru.InWord((materialTotal), dsMaterialItems.Rows[0]["CurrencyId"].ToString()), true, true);

                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();
                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                //creating secondary array to prevent memory leak and accidental over-writing (Tarek Talukder-26-May-2019)
                List<string> strReplace = new List<string>();
                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());

                StringCollection strColDistinct = new StringCollection();
                for (int i = 0; i < strReplace.Count; i++)
                {
                    if (strColDistinct.Contains(strReplace[i].ToUpper()))
                        continue;

                    strColDistinct.Add(strReplace[i].ToUpper());

                    string text = strReplace[i].ToUpper();
                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        ReplaceInfo[text] = document.Replace(text, dsOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }

                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);


                //removing any unused place holder
                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "", false, false);

                }

                //Region that is for Pdf.Document
                DocToPDFConverter converter = new DocToPDFConverter();

                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);

                //Releases all resources used by DocToPDFConverter
                converter.Dispose();

                //Closes the instance of document objects
                document.Close();
                string Prefix = "PurchaseDocAcceptance" + PDACId;
                //Saves the PDF file 
                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);

                document.Close();

            }
            catch (Exception ex)
            {
                throw ex;

            }
            document.Close();

        }


        public DataTable PurchaseDocAcceptanceServiceTable(string PDACId)
        {
            string strSQL;

            try
            {
                strSQL = @"SELECT PDASer.Id
                  ,LCCT.UserName Service
                  ,PDASer.PurchaseDocAcceptanceId
                  ,PDASer.ServiceMasterId
                  ,PDASer.Amount
                  ,PDASer.TotalTaxAmount
                  ,PDASer.AddedBy
	              , REPLACE(CONVERT(CHAR(11),PDASer.AddedDate, 106),' ','-') AS AddedDate
                  ,PDASer.AddedFromIP
                  ,PDASer.UpdatedBy
                  ,PDASer.UpdatedDate
                  ,PDASer.UpdatedFromIP
              FROM [TRN].[PurchaseDocAcceptanceService] PDASer
            LEFT JOIN [TRN].[PurchaseDocAcceptance] PDAcc ON PDASer.PurchaseDocAcceptanceId=PDAcc.Id
            LEFT JOIN [HKP].[OverHeadType] LCCT ON PDASer.ServiceMasterId=LCCT.Id
            Where  PDASer.PurchaseDocAcceptanceId= '" + PDACId + @"'";


                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }


        public double PurchaseDocAcceptanceServiceTableHeader(WordDocument document, DataTable dsServiceItems, string PDACId)
        {
            string replaceString = "{ServiceItems}";
            ReportUtility ru = new ReportUtility();

            DataTable dsTax;
            //clsDataContext data = new clsDataContext();

            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 8f;
            rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;


            dsTax = loadServiceMasterTax(PDACId);

            int LasColumnIndex = 1;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));

            //LasColumnIndex++;
            //dicTaxes.Add("totaltax", LasColumnIndex);
            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    LasColumnIndex++;
                }
            }


            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Services");
            int colServiceName = COL; //COL++;           
            range.ApplyCharacterFormat(FontBold);
            //wTable.Rows[ROW].Cells[colServiceName].Width = 100;



            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
                range.ApplyCharacterFormat(FontBold);

                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    //two columns required for tax
                    COL++;
                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                    range.ApplyCharacterFormat(FontBold);

                    COL++;
                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                    range.ApplyCharacterFormat(FontBold);

                }
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
                range.ApplyCharacterFormat(FontBold);
                //wTable.Rows[ROW].Cells[colTotalTaxableAmount].Width = 100;
            }


            wTable.Rows.Add(TemplateRow);
            ROW++;

            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {

                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate");
                    range.ApplyCharacterFormat(FontBold);
                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                    range.ApplyCharacterFormat(FontBold);

                }
            }


            if (dv.Count > 0)
            {
                WTableRow TROW = wTable.LastRow;
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                for (int i = 0; i < dv.Count; i++)
                {

                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
                    range.ApplyCharacterFormat(FontBold);
                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount)");
                    range.ApplyCharacterFormat(FontBold);

                }


            }
            #endregion column headers
            double totalValue = 0;
            int startRow = ROW + 1;
            for (int i = 0; i < dsServiceItems.Rows.Count; i++)
            {
                ROW++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                }
                IParagraphItem p = TROW.Cells[colServiceName].AddParagraph().AppendText(dsServiceItems.Rows[i]["Service"].ToString());

                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsServiceItems.Rows[i]["Amount"].ToString()).ToString("#,##0.00"));
                totalValue += clsStdLib.dbl(dsServiceItems.Rows[i]["Amount"].ToString());
                if (dv.Count > 0)
                {
                    //dsTax.Tables[0].DefaultView.RowFilter = "MasterOrderItemId='" + dsOrderItems.Tables[0].Rows[i]["MasterOrderItemId"].ToString() + "'";
                    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
                    //double totalTax = 0;

                    for (int T = 0; T < dv.Count; T++)
                    {
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND InventoryServiceId='" + dsServiceItems.Rows[i]["ServiceMasterId"] + "'";
                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("#,##0.00"));
                        }
                    }
                }
            }

            ROW++;
            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);

            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (dicTaxes.ContainsValue(C))
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);

            }
            #endregion Total


            ROW++;
            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStdLib.dbl(dsServiceItems.Compute("SUM(Amount)", "").ToString())
                //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                + clsStdLib.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());

            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2") + " (" + ru.InWord(total, dsOrderMaster.Rows[0]["CurrencyId"].ToString()) + ")");

            #endregion Total


            ROW++;
            #region Total Payable
            //int TotalPayableRow = ROW;
            //int TotalPayableColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[TotalPayableColumn].AddParagraph().AppendText("Total Amount Payable");
            //_TROW.Cells[TotalPayableColumn + 1].AddParagraph().AppendText("Need To Discuss");

            #endregion Total Payable


            ROW++;


            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle2 = document.AddParagraphStyle("MyStyle2");
            //Sets the formatting of the style
            myStyle2.CharacterFormat.FontSize = 8f;
            myStyle2.CharacterFormat.TextColor = Color.Black;
            myStyle2.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                //TROW.Cells[0].Width = 20;
                //if (dv.Count < 3)
                //    TROW.Cells[0].Width = 120 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle2");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            for (int i = 0; i < dv.Count; i++)
                wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            for (int i = 0; i <= colTotalTaxableAmount; i++)
                wTable.ApplyVerticalMerge(i, ROW - 1, ROW);




            IWParagraphStyle style2 = document.AddParagraphStyle("SubTotalStyle2");
            style2.CharacterFormat.Bold = true;
            style2.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section


            //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
            //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
            //        PARA.ApplyStyle("SubTotalStyle2");

            //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
            #endregion merging section



            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);
            return total;
        }


        public DataTable PurchaseDocAcceptanceMasterTable(string PDACId)
        {
            string strSQL;
            //clsConnection objCon;
            try
            {
                strSQL = @"SELECT               
                            PDAcc.Id AcceptanceID
						   ,PO1.Id PONumber
						  ,PLC.LCRef LCNumber
                          ,PDAcc.CompanyGroupId
                          ,PDAcc.CompanyId
                          ,PDAcc.PlantId
                          ,PO.POType
	                      ,PO.DocRefNo
						  ,PO.ContractId 
						  ,Cnt.ContractNo
						 ,PLC.BenificiaryBank
						 ,B.UserName OpeningBank
						 ,PDAcc.AcceptanceNo 
						 ,Par.UserName VendorName
                          , REPLACE(CONVERT(CHAR(11),PLC.LCDate, 106),' ','-') AS LCDate
						   , REPLACE(CONVERT(CHAR(11),PDAcc.AcceptanceDate, 106),' ','-') AS AcceptanceDate
						  , REPLACE(CONVERT(CHAR(11),PO.DocDate, 106),' ','-') AS DocDate
                          , REPLACE(CONVERT(CHAR(11),PDAcc.EntryDate, 106),' ','-') AS EntryDate
                          ,AddedBy=CASE When U.EmployeeId is null OR U.EmployeeId is not Null  then eI3.EmployeeName else ''  END   
                         ,REPLACE(CONVERT(CHAR(11),PDAcc.AddedDate, 106),' ','-') AS AddedDate
                          ,PDAcc.AddedFromIP
                          ,PDAcc.UpdatedBy
                          ,PDAcc.UpdatedDate
                          ,PDAcc.UpdatedFromIP
                          ,PDAcc.POId
                          ,PDAcc.CheckedBy
                          ,PDAcc.CheckedByStatus
                          ,PDAcc.AuthorizedBy
                          ,PDAcc.AuthorizedByStatus
                          ,PDAcc.Remarks Remarks
                      FROM [TRN].[PurchaseDocAcceptance] PDAcc
                      LEFT JOIN [TRN].[PurchaseDocAcceptanceDetail]  PDACD ON PDAcc.Id=PDACD.PurchaseDocAcceptanceId
					  LEFT JOIN [TRN].[PurchaseOrder] PO1 ON PO1.Id= PDACD.POId
					  LEFT JOIN [HKP].[Party] Par ON Par.Id= PO1.PartyId
                      LEFT JOIN ORG.Company Cmp ON Cmp.Id = PDAcc.CompanyId
                      LEFT JOIN ORG.Plant Plant ON Plant.Id = PDAcc.PlantId
                      LEFT JOIN [TRN].[PurchaseOrder] PO ON PO.PurchaseLCId =PDAcc.PurchaseLCId
					  LEFT JOIN DBO.PurchaseLC PLC ON PLC.Id =PDAcc.PurchaseLCId
					  LEFT JOIN [dbo].[Contract] Cnt ON cnt.Id =PO.ContractId
	                  LEFT JOIN [MST].[BankMaster] BM ON BM.Id = PLC.OpeningBankMasterId
					  LEFT JOIN [HKP].[Bank] B ON B.Id = BM.BankId
					  left join [SEC].[User] U on U.UserId=PDAcc.AddedBy
					  LEFT JOIN dbo.EmployeeInformation eI3 ON eI3.SystemId=U.EmployeeId
                      Where PDAcc.Id='" + PDACId + @"'";
                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }


        private DataTable PurchaseDocAcceptanceDetailTable(string PDACDId)
        {

            //string sqlText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + requistionId + @"'

            string strSQL;
            //clsConnection objCon;
            try
            {
                strSQL = @"SELECT						
								PDACD.Id AS PDAcDetailId
							   ,MM.UserName MaterialName
								,ART.StandardName Article
                       	    , REPLACE(CONVERT(CHAR(11),PDAcc.AcceptanceDate, 106),' ','-') AS AcceptaceDate
                            , PDACD.MaterialMasterId 
                            , PDACD.ArticleId, 
                             PDACD.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                            , PDACD.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                            , PDACD.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                            , PDACD.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                            , PDACD.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                            , PDACD.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
                            , ROUND(PDACD.TransactionQty,2) TransactionQty
							, ROUND(PDACD.MaterialTranRate,2) MaterialTranRate 
							, ROUND(PDACD.MaterialTranAmount,2) TrnAmount
	                        , PDACD.TransactionUoMId
	                        ,TUoM.ShortName AS TransactionUoM
	                        , CU.Code AS CurrencyName
		                    , CU.Id AS CurrencyId
                       FROM TRN.PurchaseDocAcceptanceDetail AS PDACD
						LEFT JOIN [TRN].[PurchaseDocAcceptance] PDAcc ON PDACD.PurchaseDocAcceptanceId=PDAcc.Id
						LEFT JOIN MST.MaterialMaster AS MM ON PDACD.MaterialMasterId=MM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON PDACD.ArticleId=ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON PDACD.FirstCharacteristicsId=FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON PDACD.SecondCharacteristicsId=SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON PDACD.ThirdCharacteristicsId=TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON PDACD.FirstCharacteristicsValueId=FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON PDACD.SecondCharacteristicsValueId=SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON PDACD.ThirdCharacteristicsValueId=TCV.Id
                        LEft JOIN [SCS].[UnitOfMeasurement] AS TUoM ON PDACD.TransactionUoMId=TUoM.Id
					    LEFT JOIN [TRN].[PurchaseOrder] PO ON PO.Id = PDACD.POId
						LEFT JOIN [TRN].PurchaseOrderDetail POD ON PO.Id = PDACD.PODetailId
                        LEFT JOIN [SCS].[Currency] AS CU ON PO.CurrencyId=CU.Id 
                      Where PDAcc.Id='" + PDACDId + @"'";


                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }
        public double PurchaseDocAcceptanceDetailTableHeader(WordDocument document, DataTable dsMaterialItems, string PDACId)
        {
            string replaceString = "{materialPurchaseDocDetail}";
            ReportUtility ru = new ReportUtility();
            DataTable dsTax;

            int LasColumnIndex = 11;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();

            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();
            //wTable.Title = "Material Details";
            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;



            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SL");
            range.ApplyCharacterFormat(FontBold);
            int colRo = COL; COL++;
            wTable.Rows[ROW].Cells[colRo].Width = 30;

            //         wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("PD Acc Detail Id");
            //         range.ApplyCharacterFormat(FontBold);
            //         int colRowId = COL; COL++;
            //wTable.Rows[ROW].Cells[colRowId].Width = 65;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("PD Acc Detail Id");
            range.ApplyCharacterFormat(FontBold);
            int colRowId = COL; COL++;
            wTable.Rows[ROW].Cells[colRowId].Width = 65;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Materials");
            range.ApplyCharacterFormat(FontBold);
            int colMaterialGroup = COL; COL++;
            wTable.Rows[ROW].Cells[colMaterialGroup].Width = 100;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article ");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            wTable.Rows[ROW].Cells[colArticle].Width = 100;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU1");
            range.ApplyCharacterFormat(FontBold);
            int colChar1 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar1].Width = 60;
            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU2");
            range.ApplyCharacterFormat(FontBold);
            int colChar2 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar2].Width = 60;
            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU3");
            range.ApplyCharacterFormat(FontBold);
            int colChar3 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar3].Width = 60;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Acceptance Date");
            range.ApplyCharacterFormat(FontBold);
            int colRequiredDate = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Material TranRate");
            range.ApplyCharacterFormat(FontBold);
            int colMaterialTranAmount = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UOM");
            range.ApplyCharacterFormat(FontBold);
            int colUOM = COL; COL++;
            wTable.Rows[ROW].Cells[colUOM].Width = 40;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
            range.ApplyCharacterFormat(FontBold);
            int colTotalTaxableAmount = COL; COL++;


            #endregion column headers
            var NormalOverBudgetNew = "";
            var normalBudgetType = "";
            var overBudgetType = "";
            var newBudgetType = "";
            var MaterialDetail = "";
            var RequiredDate = "";
            var Remarks = "";
            var Reason = "";
            var OwnStock = "";
            var OtherStock = " ";


            double totalValue = 0;
            int startRow = ROW;
            int sl = 0;
            for (int i = 0; i < dsMaterialItems.Rows.Count; i++)
            {
                sl++;
                //ROW++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    //TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }


                TROW.Cells[colRo].AddParagraph().AppendText(sl.ToString());

                TROW.Cells[colRowId].AddParagraph().AppendText(dsMaterialItems.Rows[i]["PDAcDetailId"].ToString());
                TROW.Cells[colMaterialGroup].AddParagraph().AppendText(dsMaterialItems.Rows[i]["MaterialName"].ToString());
                TROW.Cells[colArticle].AddParagraph().AppendText(dsMaterialItems.Rows[i]["Article"].ToString());
                TROW.Cells[colChar1].AddParagraph().AppendText(dsMaterialItems.Rows[i]["FirstCharacteristicsValue"].ToString());
                TROW.Cells[colChar2].AddParagraph().AppendText(dsMaterialItems.Rows[i]["SecondCharacteristicsValue"].ToString());
                TROW.Cells[colChar3].AddParagraph().AppendText(dsMaterialItems.Rows[i]["ThirdCharacteristicsValue"].ToString());
                TROW.Cells[colRequiredDate].AddParagraph().AppendText(dsMaterialItems.Rows[i]["AcceptaceDate"].ToString());
                TROW.Cells[colQty].AddParagraph().AppendText(clsStdLib.dbl(dsMaterialItems.Rows[i]["TransactionQty"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colMaterialTranAmount].AddParagraph().AppendText(clsStdLib.dbl(dsMaterialItems.Rows[i]["MaterialTranRate"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsMaterialItems.Rows[i]["TrnAmount"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colUOM].AddParagraph().AppendText(dsMaterialItems.Rows[i]["TransactionUoM"].ToString());

                ROW++;
            }


            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);


            double value = 0;
            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                value = 0;
                if (C == colRowId || C == colArticle || C == colChar1 || C == colChar2 || C == colChar3 || C == colQty || C == colUOM || C == colMaterialTranAmount || C == colRequiredDate || C == colMaterialGroup)
                    continue;


                for (int i = startRow; i <= TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);

            }
            #endregion Total


            ROW++;
            #region Sub Total

            double total = clsStdLib.dbl(dsMaterialItems.Compute("SUM(TrnAmount)", "").ToString());


            #endregion Total


            ROW++;
            #region Total Payable

            #endregion Total Payable


            ROW++;


            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            //myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            //for (int i = 0; i < dv.Count; i++)
            //    wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            //for (int i = 0; i <= colTotalTaxableAmount; i++)
            //    wTable.ApplyVerticalMerge(i, ROW - 1, ROW);




            IWParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
            style.CharacterFormat.Bold = true;
            style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section

            #endregion merging section

            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);
            return total;
        }


        class clsStdLib1
        {
            public static string passWord = "prodDisplay";
            public clsStdLib1()
            {

            }
            public enum mType
            {
                Error,
                Success,
                Information
            }
            public static bool passwordGet = true;
            public static string[] sMonth = new string[] { "<Unselect>", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

            public static string DataRankNamesPDAC(int dayNo)
            {

                if (dayNo <= 0)
                    return "";

                if (dayNo.ToString().Length > 1)
                {
                    string Right = dayNo.ToString().Substring(dayNo.ToString().Length - 2, 2);
                    if (clsStdLib.dbl(Right) >= 10 && clsStdLib.dbl(Right) <= 20)
                        return dayNo + "th";
                }

                string RightString = dayNo.ToString().Substring(dayNo.ToString().Length - 1, 1);
                switch (RightString)
                {
                    case "1":
                        return dayNo + "st";
                    case "2":
                        return dayNo + "nd";
                    case "3":
                        return dayNo + "rd";
                    default:
                        return dayNo + "th";

                }



            }

            #region date related
            public static readonly string dateFormat = "dd-MMM-yyyy";
            public static readonly string sqliteDateFormat = "yyyy-MM-dd";
            public static readonly string AppToDBdateFormat = "yyyy-MM-dd hh:mm:ss";
            public static bool IsDateOKPDAcc(string strdate)
            {
                try
                {
                    if (strdate.Length != 11)
                    {
                        return false;
                    }
                    if (strdate.Substring(2, 1) != "-" && strdate.Substring(6, 1) != "-")
                    {
                        return false;
                    }
                    System.DateTime myDt = System.Convert.ToDateTime(strdate);
                    return true;
                }
                catch (System.Exception ex)
                {
                    return false;
                }
                finally
                {
                    //
                }
            }// end function
            private static bool DateOkCheck(string strdate)
            {
                try
                {
                    System.DateTime myDt = System.Convert.ToDateTime(strdate);
                    return true;
                }
                catch (System.Exception ex)
                {
                    return false;
                }
                finally
                {
                    //
                }
            }// end function
            public static object chk_NullDateData(object dateValue)
            {
                if (DateOkCheck("" + dateValue.ToString()) == false)
                {
                    dateValue = "";
                }

                if (("" + dateValue.ToString()) == "")
                {
                    System.DateTime dt = new System.DateTime(1901, 1, 1);
                    dateValue = (object)dt;
                }
                return (object)dateValue;
            }
            public static System.DateTime AppDateConvert(object dateValue, string input_date_format, string output_date_format)
            {
                string strDate = null;
                dateValue = chk_NullDateData(dateValue);
                strDate = dateValue.ToString();
                if (strDate != "")
                {
                    if (input_date_format.Trim() != "")
                    {
                        if (output_date_format.Trim() != "")
                        {
                            System.Globalization.DateTimeFormatInfo InputFormat = new System.Globalization.DateTimeFormatInfo();
                            InputFormat.ShortDatePattern = input_date_format;
                            System.DateTime myDt = System.Convert.ToDateTime(strDate, InputFormat);
                            strDate = myDt.ToString(output_date_format);
                        }
                    }
                }
                return System.Convert.ToDateTime(strDate);
            }// End of function
            public static Object DateData_AppToDB(object dateValue, string DB_Level_date_format)
            {
                if (string.IsNullOrEmpty((string)dateValue))
                    return DBNull.Value;

                string strDate = null;
                strDate = dateValue.ToString();
                if (DB_Level_date_format != "")
                {
                    // Collecting the user terminal set format 
                    System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
                    strDate = AppDateConvert(strDate, USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString(), DB_Level_date_format).ToString();
                }

                string m = System.Convert.ToDateTime(strDate).ToString(AppToDBdateFormat);
                return System.Convert.ToDateTime(strDate).ToString(AppToDBdateFormat);


            }// End of function
            public static System.DateTime DateData_DBToApp(object dateValue)
            {
                string strDate = null;
                strDate = dateValue.ToString();

                System.Globalization.DateTimeFormatInfo myDBDateFormat = new System.Globalization.CultureInfo("en-US", false).DateTimeFormat;
                strDate = DateData_DBToApp(dateValue, myDBDateFormat.ShortDatePattern.ToString()).ToString();
                return System.Convert.ToDateTime(strDate);
            }// End function
            public static System.DateTime DateData_DBToApp(object dateValue, string DB_Level_date_format)
            {
                string strDate = null;
                strDate = dateValue.ToString();
                if (DB_Level_date_format != "")
                {
                    // Collecting the user terminal set format 
                    System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
                    strDate = AppDateConvert(strDate, DB_Level_date_format, USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString()).ToString();
                }
                return System.Convert.ToDateTime(strDate);
            }// End of function
            public static String makeBaseBlank(object dateValue)
            {
                System.DateTime dt;
                dt = System.Convert.ToDateTime(dateValue.ToString());
                if (dt.Year == 1901)
                {
                    return "";
                }
                else
                {
                    return dateValue.ToString();
                }
            }// End of function
            ///<summary>
            ///return day difference in integer. 
            ///    Example 1: firstDate[Less Than]lastDate returns positive value
            ///    Example 2: firstDate>lastDate returns negative value
            ///    Example 3: firstDate=lastDate returns 0 [zero]**/
            /// </summary>
            public static int dateDiff(string firstDate, string lastDate)
            {

                int difference = 0;
                try
                {
                    firstDate = Convert.ToDateTime(firstDate).ToString("dd-MMM-yyyy");
                    lastDate = Convert.ToDateTime(lastDate).ToString("dd-MMM-yyyy");

                    if (IsDateOKPDAcc(firstDate) == false)
                    {
                        Exception ex = new Exception("Invalid [First Date]");
                        throw (ex);
                    }
                    if (IsDateOKPDAcc(lastDate) == false)
                    {
                        Exception ex = new Exception("Invalid [Last Date]");
                        throw (ex);
                    }
                    DateTime dateFirstDate = Convert.ToDateTime(firstDate);
                    DateTime dateLastDate = Convert.ToDateTime(lastDate);
                    TimeSpan TimeSpan = dateLastDate.Subtract(dateFirstDate);


                    difference = TimeSpan.Days;
                }
                catch (Exception ex)
                {
                    throw (ex);
                }

                return difference;
            }



            public static string getSqliteDate(string standardDate)
            {
                return (Convert.ToDateTime(standardDate).ToString(sqliteDateFormat));
            }
            public static string getStandardDateFromSqliteDate(string SqliteDate)
            {
                if (SqliteDate.Length != 10)
                    return "";
                if (SqliteDate.Split('-').Length != 3)
                    return "";
                //many things to validate 
                //but i have less time :)
                string month = ValidLength(sMonth[Convert.ToInt32(SqliteDate.Split('-')[1])], 3).ToString();


                return SqliteDate.Split('-')[2] + "-" + month + "-" + SqliteDate.Split('-')[0];
            }
            #endregion date related

            #region numeric
            public static bool IsNumeric(string strNumber)
            {
                Double d;
                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
                if (strNumber.Length == 0)
                {
                    return false;
                }
                return Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d);
            } // End Function
            public static string GetNumericData(string strNumber)
            {
                double d;
                strNumber = strNumber.Replace(",", "");
                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
                if (strNumber.Trim() == "")
                { return "0"; }
                else if (System.Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
                {
                    return strNumber;
                }
                else
                {
                    return "0";
                }
            }// end function
            public static string GetNumericDataInDecimalFormat(string strNumber, int precision)
            {
                if (precision < 1)
                    return strNumber;

                string s_precision = new String('0', precision);

                double d;
                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
                if (strNumber.Trim() == "")
                { return "0." + s_precision; }
                else if (System.Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
                {
                    return string.Format("{0:0." + s_precision + "}", d);
                }
                else
                {
                    return "0." + s_precision;
                }
            }// end function
            public static double dbl1(string d)
            {
                return Convert.ToDouble(GetNumericData(d));

            }
            public static int Percentage(int total, double percentage)
            {
                return (int)(total * (percentage / 100));

            }
            //validation
            public static void numericValidation(string value, bool isMandatory, bool isInteger, bool negativeAllowed, string fieldName)
            {

                try
                {



                    if (isMandatory == true)
                    {
                        if (value.Trim() == "")
                        {
                            Exception ex = new Exception("please insert [" + fieldName + "]");
                            throw (ex);
                        }
                        if (Convert.ToDouble(GetNumericData(value.Trim())) == 0)
                        {
                            Exception ex = new Exception("please insert [" + fieldName + "]");
                            throw (ex);
                        }

                        if (value.Trim() != "")
                        {
                            if (IsNumeric(value.Trim()) == false)
                            {
                                Exception ex = new Exception("Invalid numeric value [" + value + "] for the field [" + fieldName + "]");
                                throw (ex);
                            }
                        }
                    }

                    if (value.Trim() != "")
                    {
                        if (IsNumeric(value.Trim()) == false)
                        {
                            Exception ex = new Exception("Invalid numeric value [" + value + "] for the field [" + fieldName + "]");
                            throw (ex);
                        }
                        if (isInteger == true)
                        {

                            if (isInt(value.Trim()) == false)
                            {
                                Exception ex = new Exception("Number must be integer for the field [" + fieldName + "]");
                                throw (ex);
                            }

                        }
                        if (negativeAllowed == false)
                        {
                            if (Convert.ToDouble(GetNumericData(value.Trim())) < 0)
                            {
                                Exception ex = new Exception("Negative values are not allowed for the field [" + fieldName + "]");
                                throw (ex);
                            }
                        }
                    }



                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {

                }


            }

            ///<summary>
            ///check whether a value is integer or not returns true if integer, 
            ///false if floating or string containing alpahnumeric
            ///</summary>
            public static bool isInt(string num)
            {

                bool isInt;
                int number;
                try
                {
                    isInt = System.Int32.TryParse(num, out number);
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {

                }
                return isInt;
            }


            #endregion numeric

            #region string

            public static readonly string excelNegativePOsitiveSign = @"+#,##0.00;-#,##0.00;* ??;@";
            public static readonly string NegativePOsitiveSign = @"+#,##0.00;-#,##0.00;0";
            public static readonly string NumberFormatString = "#,##0.000;(#,##0.000);* ??;@";
            public static readonly string NumberFormatStringFourDecimal = "#,##0.0000;(#,##0.0000);* ??;@";
            public static readonly string NumberFormatStringFiveDecimal = "#,##0.00000;(#,##0.00000);* ??;@";
            public static readonly string NumberFormatStringTwoDecimal = "#,##0.00;(#,##0.00);* ??;@";
            public static readonly string NumberFormatStringTwoDecimalWithZero = "#,##0.00;(#,##0.00)";
            public static readonly string NumberFormatStringInteger = "#,##0;(#,##0);* ??;@";
            public static readonly string NumberFormatStringIntegerWithZero = "#,##0;(#,##0)";
            public static readonly string NumberFormatStringText = "@"; //format cell data as text


            public static object ValidLength(string str)
            {

                string removechar = "";
                if (str.Trim() == "")
                {
                    return (object)Convert.DBNull;
                }
                removechar = str.Trim();
                removechar = removechar.Replace("'", " ");

                return (object)removechar.Trim();

            }
            public static object ValidLength(string str, int length)
            {

                string removechar = "";
                if (str.Trim() == "")
                {
                    return (object)Convert.DBNull;
                }
                removechar = str.Trim();
                removechar = removechar.Replace("'", " ");


                int strLen = removechar.Length;
                if (strLen > length)
                    removechar = removechar.Substring(0, length);

                return (object)removechar.Trim();

            }
            public static string FileNameLegalChar(string fileName)
            {
                string illegalChar = @"~`!@#$%^&*=/\|>,<";
                foreach (char c in illegalChar)
                {
                    fileName = fileName.Replace(c.ToString(), " ");
                }

                return fileName;
            }
            private StringCollection getTableColumns(ref DataSet dsLocal)
            {
                StringCollection strcol = new StringCollection();
                for (int COL = 0; COL < dsLocal.Tables[0].Columns.Count; COL++)
                {
                    strcol.Add(dsLocal.Tables[0].Columns[COL].ColumnName.ToUpper());
                }

                return strcol;

            }
            public static string emptyString(string str)
            {
                //this function returns an empty string(not a null) from null or empty or '&nbsp;' from the page
                if (str == "&nbsp;")
                    str = "";
                if (string.IsNullOrEmpty(str) == true)
                    str = "";


                return str;
            }//this function returns an empty string(not a null) from null or empty '&nbsp;' from the page
            #endregion string


            #region others
            //public void copyDataset(DataSet source, ref DataSet destination)
            //{
            //    //StringCollection strColDestinationColumns = getTableColumns(ref destination);//upper case
            //    DataRow drLocal = null;
            //    for (int ROW = 0; ROW < source.Tables[0].Rows.Count; ROW++)
            //    {
            //        drLocal = destination.Tables[0].NewRow();
            //        for (int COL = 0; COL < source.Tables[0].Columns.Count; COL++)
            //        {
            //            if (strColDestinationColumns.Contains(source.Tables[0].Columns[COL].ToString().ToUpper()))
            //            {
            //                drLocal[source.Tables[0].Columns[COL].ToString()] = ValidLength(source.Tables[0].Rows[ROW][source.Tables[0].Columns[COL].ToString()].ToString());
            //            }
            //        }
            //        destination.Tables[0].Rows.Add(drLocal);
            //    }


            //}
            public static string GetxlsCol(int intCol)
            {
                //returns excel columns based on column number. tested 1 to 256 column numbers
                try
                {
                    if (intCol < 1 || intCol > 256)
                    {
                        System.Exception ex = new Exception("Invalid Column Value");
                        throw (ex);
                    }
                    intCol = intCol - 1;
                    int intFirstLetter = ((intCol) / 512) + 64;
                    int intSecondLetter = ((intCol % 512) / 26) + 64;
                    int intThirdLetter = (intCol % 26) + 65;
                    char FirstLetter;
                    char SecondLetter;
                    if (intFirstLetter > 64)
                        FirstLetter = (char)intFirstLetter;
                    else
                        FirstLetter = ' ';

                    if (intSecondLetter > 64)
                        SecondLetter = (char)intSecondLetter;
                    else
                        SecondLetter = ' ';

                    char ThirdLetter = (char)intThirdLetter;
                    return string.Concat(FirstLetter, SecondLetter, ThirdLetter).Trim();
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {

                }
            }//returns excel columns based on column number. tested 1 to 256 column numbers
            #endregion others

            public static object RetValidLen(string Data)
            {
                if (string.IsNullOrEmpty(Data))
                    return DBNull.Value;

                return Data;
            }
            public static double sum(string columnName, DataTable dtLocal, string criteria)
            {
                double total = 0;
                DataRow[] dr = dtLocal.Select(criteria);
                foreach (DataRow d in dr)
                {
                    total += dbl(d[columnName].ToString());
                }


                return total;
            }

            private static double dbl(string v)
            {
                throw new NotImplementedException();
            }
        }



        #region  Taufik

        public IEnumerable<object> GetMaterialDetailsPDAC(string Id)
        {
            try
            {
                var sql = @"Select  Id	
                        ,InventoryReceiveId	
                        ,InventoryMaterialId	
                        ,MaterialStorageId	
                        ,TransactionQty	
                        ,TransactionUoMId	
                        ,BaseQty	
                        ,BaseUOMId	
                        ,BaseUoMFactor	
                        ,TransactionRate	
                        ,TransactionAmount	
                        ,IssueQty	
                        ,AddedBy	
                        ,AddedDate	
                        ,AddedFromIP	
                        ,UpdatedBy	
                        ,UpdatedDate	
                        ,UpdatedFromIP	
                        ,TotalTaxAmount	
                        ,BaseAmount	
                        ,ChargesAmount	
                        ,WithInvoiceRate	
                        ,AfterInvoiceRate	
                        ,CountryId	
                        ,InventoryMaterialId	
                        ,MaterialStorageId	
                        ,BaseUOMId	
                        ,TransactionQty	
                        ,TransactionRate	
                        ,TransactionAmount
                        from 
                        TRN.purchaseorderdetail pd
                        where id='" + Id + @"'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }


        public DataTable loadOrderMasterPDAC(string purchaseOrderId)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT PO.Id PONumber
                                                ,PO.CompanyGroupId
                                                ,PO.CompanyId
                                                ,Plant.GSTIN
                                                ,REPLACE(Convert(VARCHAR(11), PO.PODate, 106), ' ', '-') AS PODate
                                                ,REPLACE(Convert(VARCHAR(11), PO.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
                                                ,REPLACE(Convert(VARCHAR(11), PO.MatureDate, 106), ' ', '-') AS MatureDate
		                                        ,PO.InvoicingPartyPlantId
		                                        ,INVPARTYPL.UserName InvoicingPartyName
                                                ,INVPARTYPL.AddressMasterId InvoicePartyAddressMasterId
                                                ,INVPARTYPL.GSTIN InvoicingPartyGSTIN
                                                ,ISNULL(PO.InvoicingByAddress,'') InvoicingByAddress
												,PO.DeliveryByAddress
		                                        ,DPARTYPL.UserName DeliveryParty
		                                        ,PO.DeliveryPartyPlantId		
		                                        ,POM.MaterialMasterId
		                                        ,PO.DocRefNo
                                                ,REPLACE(Convert(VARCHAR(11), PO.DocDate, 106), ' ', '-') AS DocDate
		                                        ,PO.AddedBy
		                                        ,PO.AddedDate
		                                        ,PO.UpdatedBy
		                                        ,PO.UpdatedDate
		                                        ,PO.IsApproved 
		                                        ,PO.PartyType
												,PO.PartyId
                                                ,ISNULL(PO.DeliveryInstruction,'') DeliveryInstruction
												,ISNULL(PO.SpecialInstruction,'') SpecialInstruction
												,Party.UserName VendorName
                                                ,Party.AddressMasterId VendorAddressMasterId
                                                ,Party.TINNO VendorGSTIN
		                                        ,Case When PO.IsNonCreditable = 1 then 'NonCreditable' when Po.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
		                                        ,PO.CurrencyId
	                                            ,CRNC.Code AS CurrencyName
	                                            ,PO.ToCurrencyRate
		                                        ,BASECRNC.Code AS BaseCurrencyName
		                                        ,PayTerm.UserName PaymentTerm
	                                          ,MM.UserName MaterialMaster
	                                          ,MM.MaterialGroupMasterId
	                                          ,MGM.UserName MaterialGroupMaster
	                                          ,POM.ArticleId
	                                          ,MMA.StandardName Article
	                                          ,FC.Id FirstCharId
	                                          ,FC.UserName FirstChar
                                              ,POM.FirstCharacteristicsValueId
	                                          ,FCV.UserName AS FirstCharacteristicsValue
                                              ,POM.SecondCharacteristicsValueId
	                                          ,SCV.UserName AS SecondCharacteristicsValue
	                                          ,POM.ThirdCharacteristicsValueId
	                                          ,TCV.UserName AS ThirdCharacteristicsValue
	                                          ,SC.Id SecondCharId
	                                          ,SC.UserName SecondChar
	                                          ,TC.Id ThirdCharId
	                                          ,TC.UserName ThirdChar
	                                          ,ROUND(POD.TransactionQty, 2) POTransactionQty
	                                          ,ROUND(POD.TransactionRate, 2) TransactionRate
	                                          ,ROUND((POD.TransactionQty * POD.TransactionRate), 2) AS TrnAmount
	                                          ,POD.BaseAmount
	                                          ,POD.TotalTaxAmount AS BaseTaxAmount
	                                          ,TaxAmount = (
		                                            SELECT SUM(TaxAmount)
		                                            FROM [TRN].[PurchaseOrderTax]
		                                            WHERE InventoryReceiveDetailId = POD.Id
		                                            )
	                                          ,ServiceTaxAmount = (
		                                            SELECT SUM(TotalTaxAmount)
		                                            FROM [TRN].[POService]
		                                            WHERE InventoryReceiveId = POM.Id
		                                            )
	                                          ,POD.ChargesAmount
	                                          ,POD.CountryId
	                                          ,POCountry.UserName CountryOfOrigin
                                                ,POD.Id PurchaseOrderDetailId
	                                          ,POD.TransactionUoMId
	                                          ,TUoM.UserName AS TransactionUoM
                                              FROM TRN.PurchaseOrder PO
                                         LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = PO.CompanyGroupId
                                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = PO.CompanyId
                                         LEFT JOIN ORG.Plant Plant ON Plant.Id = PO.PlantId
                                         LEFT JOIN SCS.Currency CRNC ON CRNC.Id = PO.CurrencyId
                                         LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = PO.BaseCurrencyId
                                         LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = PO.PaymentTermId
                                         LEFT JOIN HKP.PartyPlant  INVPARTYPL ON INVPARTYPL.Id = PO.InvoicingPartyPlantId
                                         LEFT JOIN HKP.PartyPlant  DPARTYPL ON DPARTYPL.Id = PO.DeliveryPartyPlantId                                          
                                         LEFT JOIN TRN.PurchaseOrderDetail POD ON PO.Id = POD.InventoryReceiveId
                                         LEFT JOIN SCS.Country POCountry ON POD.CountryId = POCountry.Id
										 LEFT JOIN HKP.Party Party ON Party.Id = PO.PartyId                                        
                                         LEFT JOIN TRN.POMaterial AS POM ON POD.InventoryMaterialId = POM.Id
                                         INNER JOIN MST.MaterialMaster AS MM ON MM.Id = POM.MaterialMasterId
                                         INNER JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                                         LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = POM.ArticleId
                                         LEFT JOIN HKP.Characteristics AS FC ON POM.FirstCharacteristicsId = FC.Id
                                         LEFT JOIN HKP.Characteristics AS SC ON POM.SecondCharacteristicsId = SC.Id
                                         LEFT JOIN HKP.Characteristics AS TC ON POM.ThirdCharacteristicsId = TC.Id
                                         LEFT JOIN HKP.CharacteristicsValue AS FCV ON POM.FirstCharacteristicsValueId = FCV.Id
                                         LEFT JOIN HKP.CharacteristicsValue AS SCV ON POM.SecondCharacteristicsValueId = SCV.Id
                                         LEFT JOIN HKP.CharacteristicsValue AS TCV ON POM.ThirdCharacteristicsValueId = TCV.Id
                                         LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON POD.TransactionUoMId = TUoM.Id
                                         WHERE PO.Id = '" + purchaseOrderId + @"' ";

                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }







        public DataTable PurchaseDocAcceptanceMaster(string PDACId)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT IM.Id
             , IM.Id AS MaterialReqqusitionMasterId
            , MGM.UserName AS MaterialGroupMasterName
            , IM.MaterialMasterId, MM.UserName
            , IM.ArticleId, ART.StandardName
            , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
            , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
            , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
            , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
            , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
            , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
            , ROUND(IM.TransactionQty,2) TransactionQty
	        , IM.TransactionUoMId
	        , TUoM.UserName AS TransactionUoM
	        , ROUND(IM.EstimatedRate,2) TransactionRate 
	        , CU.Code AS CurrencyName

            , ROUND((IM.TransactionQty*IM.EstimatedRate),2) AS TrnAmount   
            ,IM.MaterialDetail
            ,Replace(CONVERT(VARCHAR(11), IM.DeliveryDate, 106), ' ', '-') DeliveryDate

	        ,Act.Id As Activity
	        ,Act.UserName As ActivityName
	        ,IM.BudgetType
	        ,IM.Reason
	        ,IM.Remarks
	        ,IM.FutureReqApp
	        --,BudgetMasterId
	        --,GLGeneralInfoId
        FROM TRN.MaterialRequsitionDetails AS IM
        JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id

        JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IM.TransactionUoMId=TUoM.Id
        JOIN [TRN].[MaterialRequsitionMaster] AS IR ON IM.MaterialReqqusitionMasterId=IR.Id
        JOIN [SCS].[Currency] AS CU ON IM.CurrencyId=CU.Id 
        JOIN [HKP].[Activity] As Act On ACT.Id=IM.ActivityId
        --JOIN [HKP].Budget
        --JOIN [HKP].Gl
        WHERE IM.MaterialReqqusitionMasterId=inventoryReceiveId ";

                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }
        public DataTable loadServicerMasterItemsPDAC(string PDACId)
        {
            string strSQL;

            try
            {
                strSQL = @"SELECT PDASer.Id
                  ,LCCT.UserName AcceptanceServiceNname
                  ,PDASer.PurchaseDocAcceptanceId
                  ,PDASer.AcceptanceServiceId
                  ,PDASer.Amount
                  ,PDASer.TotalTaxAmount
                  ,PDASer.AddedBy
	              , REPLACE(CONVERT(CHAR(11),PDASer.AddedDate, 106),' ','-') AS AddedDate
                  ,PDASer.AddedFromIP
                  ,PDASer.UpdatedBy
                  ,PDASer.UpdatedDate
                  ,PDASer.UpdatedFromIP
              FROM [TRN].[PurchaseDocAcceptanceService] PDASer
            LEFT JOIN [TRN].[PurchaseDocAcceptance] PDAcc ON PDASer.PurchaseDocAcceptanceId=PDAcc.Id
            LEFT JOIN [HKP].[OverHeadType] LCCT ON PDASer.AcceptanceServiceId=LCCT.Id
            Where  PDASer.Id = '" + PDACId + @"'";


                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }
        public DataTable loadServiceMasterTaxPODAC(string purchaseOrderId)
        {
            string strSQL;

            try
            {
                strSQL = @"SELECT InventoryServiceId,PO.Id PurchaseOrderId,tg.Code AS TaxCode,PODT.Percentage, PODT.TaxAmount from TRN.PurchaseOrder PO
                            INNER JOIN TRN.POService POS ON POS.InventoryReceiveId = PO.Id
                            INNER JOIN TRN.PurchaseOrderTax PODT ON PODT.InventoryReceiveId = PO.Id and PODT.InventoryServiceId = POS.Id
                              LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=PODT.TaxCategoryId
                                WHERE PO.Id='" + purchaseOrderId + @"' 
								AND InventoryServiceId   IS NOT NULL AND  InventoryReceiveDetailId IS NULL 
								 ORDER BY tg.[Sequence] ";


                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }
        #endregion



        public DataTable GetPurchaseDocAcceptanceSqlData(string purchaseOrderId)
        {
            try
            {
                string sqlTxt = @"SELECT PO.Id PONumber
                                                ,PO.CompanyGroupId
                                                ,PO.CompanyId
                                                ,REPLACE(Convert(VARCHAR(11), PO.PODate, 106), ' ', '-') AS PODate
                                                ,REPLACE(Convert(VARCHAR(11), PO.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
                                                ,REPLACE(Convert(VARCHAR(11), PO.MatureDate, 106), ' ', '-') AS MatureDate
		                                        ,PO.InvoicingPartyPlantId
		                                        ,INVPARTYPL.UserName InvoiceParty
		                                        ,PO.InvoicingByAddress
		                                        ,PO.DeliveryByAddress
		                                        ,DPARTYPL.UserName DeliveryParty
		                                        ,PO.DeliveryPartyPlantId		
		                                        ,POM.MaterialMasterId
		                                        ,PO.DocRefNo
		                                        ,PO.DocDate
		                                        ,PO.AddedBy
		                                        ,PO.AddedDate
		                                        ,PO.UpdatedBy
		                                        ,PO.UpdatedDate
		                                        ,PO.IsApproved
		                                        ,PO.PartyType
		                                        ,PO.IsNonCreditable
		                                        ,PO.CurrencyId
	                                            ,CRNC.Code AS CurrencyName
	                                            ,PO.ToCurrencyRate
		                                        ,BASECRNC.Code AS BaseCurrencyName
		                                        ,PayTerm.UserName PaymentTerm
	                                          ,MM.UserName MaterialMaster
	                                          ,MM.MaterialGroupMasterId
	                                          ,MGM.UserName MaterialGroupMaster
	                                          ,POM.ArticleId
	                                          ,MMA.StandardName Article
	                                          ,FC.Id FirstCharId
	                                          ,FC.UserName FirstChar
                                              ,POM.FirstCharacteristicsValueId
	                                          ,FCV.UserName AS FirstCharacteristicsValue
                                              ,POM.SecondCharacteristicsValueId
	                                          ,SCV.UserName AS SecondCharacteristicsValue
	                                          ,POM.ThirdCharacteristicsValueId
	                                          ,TCV.UserName AS ThirdCharacteristicsValue
	                                          ,SC.Id SecondCharId
	                                          ,SC.UserName SecondChar
	                                          ,TC.Id ThirdCharId
	                                          ,TC.UserName ThirdChar
	                                          ,ROUND(POD.TransactionQty, 2) POTransactionQty
	                                          ,ROUND(POD.TransactionRate, 2) TransactionRate
	                                          ,ROUND((POD.TransactionQty * POD.TransactionRate), 2) AS TrnAmount
	                                          ,POD.BaseAmount
	                                          ,POD.TotalTaxAmount AS BaseTaxAmount
	                                          ,TaxAmount = (
		                                            SELECT SUM(TaxAmount)
		                                            FROM [TRN].[PurchaseOrderTax]
		                                            WHERE InventoryReceiveDetailId = POD.Id
		                                            )
	                                          ,ServiceTaxAmount = (
		                                            SELECT SUM(TotalTaxAmount)
		                                            FROM [TRN].[POService]
		                                            WHERE InventoryReceiveId = POM.Id
		                                            )
	                                          ,POD.ChargesAmount
	                                          ,POD.CountryId

	                                          ,POD.TransactionUoMId
	                                          ,TUoM.UserName AS TransactionUoM
                                              FROM TRN.PurchaseOrder PO
                                         LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = PO.CompanyGroupId
                                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = PO.CompanyId
                                         LEFT JOIN ORG.Plant Plant ON Plant.Id = PO.PlantId
                                         LEFT JOIN SCS.Currency CRNC ON CRNC.Id = PO.CurrencyId
                                         LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = PO.BaseCurrencyId
                                         LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = PO.PaymentTermId
                                         LEFT JOIN HKP.PartyPlant  INVPARTYPL ON INVPARTYPL.Id = PO.InvoicingPartyPlantId
                                         LEFT JOIN HKP.PartyPlant  DPARTYPL ON DPARTYPL.Id = PO.DeliveryPartyPlantId 
                                         LEFT JOIN TRN.PurchaseOrderDetail POD ON PO.Id = POD.InventoryReceiveId

                                         LEFT JOIN TRN.POMaterial AS POM ON POD.InventoryMaterialId = POM.Id
                                         INNER JOIN MST.MaterialMaster AS MM ON MM.Id = POM.MaterialMasterId
                                         INNER JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                                         INNER JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = POM.ArticleId
                                         LEFT JOIN HKP.Characteristics AS FC ON POM.FirstCharacteristicsId = FC.Id
                                         LEFT JOIN HKP.Characteristics AS SC ON POM.SecondCharacteristicsId = SC.Id
                                         LEFT JOIN HKP.Characteristics AS TC ON POM.ThirdCharacteristicsId = TC.Id
                                         LEFT JOIN HKP.CharacteristicsValue AS FCV ON POM.FirstCharacteristicsValueId = FCV.Id
                                         LEFT JOIN HKP.CharacteristicsValue AS SCV ON POM.SecondCharacteristicsValueId = SCV.Id
                                         LEFT JOIN HKP.CharacteristicsValue AS TCV ON POM.ThirdCharacteristicsValueId = TCV.Id
                                         JOIN [SCS].[UnitOfMeasurement] AS TUoM ON POD.TransactionUoMId = TUoM.Id
                                         WHERE PO.Id = '" + purchaseOrderId + @"' and PO.IsApproved = 0";

                return _sqlRepository.GetDataTable(sqlTxt);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
        #region Service PO By Requisition

        private string GetPKServicePOMaster()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(ServicePOMaster), out sID);
            return sID;
        }
        public void InsertServicePOByReq(ServicePOMaster entity)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var plantId = _inventoryReceiveRepository.SqlQuery<string>($"SELECT FilePrefix from org.plant WHERE Id ='{identity.PlantId}'").FirstOrDefault();
                if (plantId == null)
                {
                    throw new CustomException("No Prefix Available for this Plant");
                }
                var year1 = DateTime.Now.ToShortDateString().ToString();
                var yr = year1.Substring(7);
                // var id = GetPKServicePOMaster();
                //var resId = id.Substring(2);
                //entity.Id = plantId + yr + resId;
                entity.Id = GetPKServicePOMaster();
                if (string.IsNullOrEmpty(entity.EmployeeId) || string.IsNullOrWhiteSpace(entity.EmployeeId))
                    entity.EmployeeId = null;
                AuditService.AddedLog(entity);
                entity.ModelState = ModelState.Added;
                _ServicePOMaster.Insert(entity);
                _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public void Update(ServicePOMaster entity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //var leastDate = base.Query(t => t.Id != entity.Id && t.PlantId == entity.PlantId).Select(t => t.GRNDate).OrderByDescending(t => t.Year).ThenByDescending(t => t.Month).ThenByDescending(t => t.Date).FirstOrDefault();
                //if (Convert.ToDateTime(entity.GRNDate) < leastDate) throw new CustomException("GRN date can't less then " + leastDate.ToString("dd/MMM/yyyy"));
                //ResetCurrencyRate(entity);
                entity.CompanyGroupId = identity.CompanyGroupId;
                //base.Update(entity);
                AuditService.UpdatedLog(entity);
                entity.ModelState = ModelState.Modified;
                _ServicePOMaster.Update(entity);
                _unitOfWork.SaveChanges();

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public void DeleteServicePOReq(string id)
        {
            try
            {
                var detail = Convert.ToBoolean(_ServicePOMaster.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id  FROM [TRN].[ServicePODetail] WHERE ServicePOMasterId='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());

                if (!detail)

                {
                    var data = _ServicePOMaster.Find(id);
                    if (data.IsNull())
                        throw new CustomException(ServiceResources.RecordNoLonger);
                    _ServicePOMaster.Delete(data);
                    _unitOfWork.SaveChanges();

                }

                else throw new CustomException("Please delete first line item.");
            }



            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetListForServicePOBYReq(string plantId, string POTypeStatus, string POType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var Sql = "";
            if (string.IsNullOrEmpty(POTypeStatus) == true)
            {
                POTypeStatus = "ForChecked";
            }
            if (POTypeStatus == "ForChecked")
            {
                Sql = @"--DECLARE @plantId VARCHAR(10)='20171';
                SELECT * FROM    (  SELECT ROW_NUMBER()  OVER (ORDER BY  SPOM.Id) AS SiNo,SPOM.Id
		                            , REPLACE(CONVERT(CHAR(11), SPOM.PODate, 106),' ','-') AS PODate
		                            , SPOM.CompanyGroupId, SPOM.CompanyId, SPOM.PlantId, SPOM.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
		                            , CP.UserName AS PartyAccountGroupName
		                            , SPOM.MaterialStorageId, SPOM.DocRefNo, REPLACE(CONVERT(CHAR(11), SPOM.DocDate, 106),' ','-') AS DocDate
		                            , SPOM.CurrencyId, CU.Code AS CurrencyCode, SPOM.BaseCurrencyId, SPOM.PaymentTermId, SPOM.BaseNoOfDays
		                            , REPLACE(CONVERT(CHAR(11), SPOM.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), SPOM.MatureDate, 106),' ','-') AS MatureDate
		                            , SPOM.FixedAssetOrInventory, SPOM.PODepended
		                            , SPOM.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, SPOM.InvoicingByAddress, SPOM.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, SPOM.DeliveryByAddress, SPOM.IsNonCreditable
		                            ,SPOM.ToCurrencyRate
		                            , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, SPOM.IsTaxApplicable
		                            , SPOM.IsApproved, SPOM.IsPaymentHold, SP.Id AS PlantStateId
		                            ,pgl.CtnId,SPOM.DiscountAmount
		                            ,SPOM.AddedBy
		                            ,SPOM.CheckedByStatus AS CheckedByStatus
		                            ,SPOM.ApprovedByStatus AS ApprovedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy,PT.PaymentMode
		                            ,IRD.Amount
		                            ,IRD.TotalTaxAmount,SPOM.OrderSpecific,SPOM.POType,SPOM.ContractId,eI.SystemId AS CheckedById,eI1.SystemId AS ApprovedById
                            FROM [TRN].[ServicePOMaster] AS SPOM JOIN [HKP].[Party] AS P ON SPOM.PartyId=P.Id
                            LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable 
                            FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                            ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor'
                            ) AS CP ON CP.PartyId=SPOM.PartyId AND CP.PlantId=SPOM.PlantId
                            LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=SPOM.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=SPOM.ApprovedBy
                            left JOIN [SCS].[Currency] AS CU ON SPOM.CurrencyId=CU.Id
                            left JOIN [MST].[PaymentTerm] AS PT ON SPOM.PaymentTermId=PT.Id
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON SPOM.InvoicingPartyPlantId=IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON SPOM.DeliveryPartyPlantId=DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                            LEFT JOIN [ORG].Plant PL ON PL.Id=SPOM.PlantId
                            LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
                            LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                            LEFT JOIN (SELECT A.ServicePOMasterId,sum(A.Amount) Amount, sum(A.TotalTaxAmount) TotalTaxAmount FROM [TRN].[ServicePODetail] AS A
                            left JOIN  [TRN].[ServicePOMaster] AS B ON A.ServicePOMasterId=B.Id WHERE B.PlantId='" + plantId + @"'  
                            GROUP BY A.ServicePOMasterId
                            ) AS IRD ON IRD.ServicePOMasterId=SPOM.Id
                            LEFT JOIN (Select count(Id) as CtnId,POID from TRN.ServicePurchaseOrderApprovalLog where Status='Approval' group by POID) as pgl  on pgl.POID=SPOM.Id
                            WHERE  SPOM.PlantId='" + plantId + @"' 
                            AND SPOM.POType='" + POType + @"' 
                            AND SPOM.CheckedByStatus='For Checking'
							AND SPOM.ApprovedByStatus IS NULL

                            AND isnull(SPOM.IsClosed,0)=0 
							UNION all

							   SELECT ROW_NUMBER()  OVER (ORDER BY  SPOM.Id) AS SiNo,SPOM.Id
		                            , REPLACE(CONVERT(CHAR(11), SPOM.PODate, 106),' ','-') AS PODate
		                            , SPOM.CompanyGroupId, SPOM.CompanyId, SPOM.PlantId, SPOM.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
		                            , CP.UserName AS PartyAccountGroupName
		                            , SPOM.MaterialStorageId, SPOM.DocRefNo, REPLACE(CONVERT(CHAR(11), SPOM.DocDate, 106),' ','-') AS DocDate
		                            , SPOM.CurrencyId, CU.Code AS CurrencyCode, SPOM.BaseCurrencyId, SPOM.PaymentTermId, SPOM.BaseNoOfDays
		                            , REPLACE(CONVERT(CHAR(11), SPOM.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), SPOM.MatureDate, 106),' ','-') AS MatureDate
		                            , SPOM.FixedAssetOrInventory, SPOM.PODepended
		                            , SPOM.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, SPOM.InvoicingByAddress, SPOM.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, SPOM.DeliveryByAddress, SPOM.IsNonCreditable
		                            ,SPOM.ToCurrencyRate
		                            , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, SPOM.IsTaxApplicable
		                            , SPOM.IsApproved, SPOM.IsPaymentHold, SP.Id AS PlantStateId
		                            ,pgl.CtnId,SPOM.DiscountAmount
		                            ,SPOM.AddedBy
		                            ,SPOM.CheckedByStatus AS CheckedByStatus
		                            ,SPOM.ApprovedByStatus AS ApprovedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy,PT.PaymentMode
		                            ,IRD.Amount
		                            ,IRD.TotalTaxAmount,SPOM.OrderSpecific,SPOM.POType,SPOM.ContractId,eI.SystemId AS CheckedById,eI1.SystemId AS ApprovedById
                            FROM [TRN].[ServicePOMaster] AS SPOM JOIN [HKP].[Party] AS P ON SPOM.PartyId=P.Id
                            LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable 
                            FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                            ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor'
                            ) AS CP ON CP.PartyId=SPOM.PartyId AND CP.PlantId=SPOM.PlantId
                            LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=SPOM.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=SPOM.ApprovedBy
                            left JOIN [SCS].[Currency] AS CU ON SPOM.CurrencyId=CU.Id
                            left JOIN [MST].[PaymentTerm] AS PT ON SPOM.PaymentTermId=PT.Id
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON SPOM.InvoicingPartyPlantId=IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON SPOM.DeliveryPartyPlantId=DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                            LEFT JOIN [ORG].Plant PL ON PL.Id=SPOM.PlantId
                            LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
                            LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                            LEFT JOIN (SELECT A.ServicePOMasterId,sum(A.Amount) Amount, sum(A.TotalTaxAmount) TotalTaxAmount FROM [TRN].[ServicePODetail] AS A
                            left JOIN  [TRN].[ServicePOMaster] AS B ON A.ServicePOMasterId=B.Id WHERE B.PlantId='" + plantId + @"'  
                            GROUP BY A.ServicePOMasterId
                            ) AS IRD ON IRD.ServicePOMasterId=SPOM.Id
                            LEFT JOIN (Select count(Id) as CtnId,POID from TRN.ServicePurchaseOrderApprovalLog where Status='Approval' group by POID) as pgl  on pgl.POID=SPOM.Id
                            WHERE  SPOM.PlantId='" + plantId + @"' 
                            AND SPOM.POType='" + POType + @"' 
                            AND SPOM.CheckedByStatus IS NULL
							AND SPOM.ApprovedByStatus ='For Approval'
                            AND isnull(SPOM.IsClosed,0)=0 

							UNION ALL
							   SELECT ROW_NUMBER()  OVER (ORDER BY  SPOM.Id) AS SiNo,SPOM.Id
		                            , REPLACE(CONVERT(CHAR(11), SPOM.PODate, 106),' ','-') AS PODate
		                            , SPOM.CompanyGroupId, SPOM.CompanyId, SPOM.PlantId, SPOM.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
		                            , CP.UserName AS PartyAccountGroupName
		                            , SPOM.MaterialStorageId, SPOM.DocRefNo, REPLACE(CONVERT(CHAR(11), SPOM.DocDate, 106),' ','-') AS DocDate
		                            , SPOM.CurrencyId, CU.Code AS CurrencyCode, SPOM.BaseCurrencyId, SPOM.PaymentTermId, SPOM.BaseNoOfDays
		                            , REPLACE(CONVERT(CHAR(11), SPOM.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), SPOM.MatureDate, 106),' ','-') AS MatureDate
		                            , SPOM.FixedAssetOrInventory, SPOM.PODepended
		                            , SPOM.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, SPOM.InvoicingByAddress, SPOM.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, SPOM.DeliveryByAddress, SPOM.IsNonCreditable
		                            ,SPOM.ToCurrencyRate
		                            , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, SPOM.IsTaxApplicable
		                            , SPOM.IsApproved, SPOM.IsPaymentHold, SP.Id AS PlantStateId
		                            ,pgl.CtnId,SPOM.DiscountAmount
		                            ,SPOM.AddedBy
		                            ,SPOM.CheckedByStatus AS CheckedByStatus
		                            ,SPOM.ApprovedByStatus AS ApprovedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy,PT.PaymentMode
		                            ,IRD.Amount
		                            ,IRD.TotalTaxAmount,SPOM.OrderSpecific,SPOM.POType,SPOM.ContractId,eI.SystemId AS CheckedById,eI1.SystemId AS ApprovedById
                            FROM [TRN].[ServicePOMaster] AS SPOM JOIN [HKP].[Party] AS P ON SPOM.PartyId=P.Id
                            LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable 
                            FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                            ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor'
                            ) AS CP ON CP.PartyId=SPOM.PartyId AND CP.PlantId=SPOM.PlantId
                            LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=SPOM.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=SPOM.ApprovedBy
                            left JOIN [SCS].[Currency] AS CU ON SPOM.CurrencyId=CU.Id
                            left JOIN [MST].[PaymentTerm] AS PT ON SPOM.PaymentTermId=PT.Id
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON SPOM.InvoicingPartyPlantId=IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON SPOM.DeliveryPartyPlantId=DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                            LEFT JOIN [ORG].Plant PL ON PL.Id=SPOM.PlantId
                            LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
                            LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                            LEFT JOIN (SELECT A.ServicePOMasterId,sum(A.Amount) Amount, sum(A.TotalTaxAmount) TotalTaxAmount FROM [TRN].[ServicePODetail] AS A
                            left JOIN  [TRN].[ServicePOMaster] AS B ON A.ServicePOMasterId=B.Id WHERE B.PlantId='" + plantId + @"' 
                            GROUP BY A.ServicePOMasterId
                            ) AS IRD ON IRD.ServicePOMasterId=SPOM.Id
                            LEFT JOIN (Select count(Id) as CtnId,POID from TRN.ServicePurchaseOrderApprovalLog where Status='Approval' group by POID) as pgl  on pgl.POID=SPOM.Id
                            WHERE  SPOM.PlantId='" + plantId + @"'  
                            AND SPOM.POType='" + POType + @"' 
                            AND SPOM.CheckedByStatus IS NULL
						AND	SPOM.ApprovedByStatus IS NULL
                            AND isnull(SPOM.IsClosed,0)=0 
							AND SPOM.Id not in( Select ServicePOMasterId from trn.ServicePODetail where ServicePOMasterId IS NOT NULL)
							)X
							
							Order by Id DESC
";



            }
            else if (POTypeStatus == "CheckedHoldRej")
            {



                Sql = @"--DECLARE @plantId VARCHAR(10)='20171';
                                                   SELECT ROW_NUMBER()  OVER (ORDER BY  SPOM.Id) AS SiNo,SPOM.Id
		                            , REPLACE(CONVERT(CHAR(11), SPOM.PODate, 106),' ','-') AS PODate
		                            , SPOM.CompanyGroupId, SPOM.CompanyId, SPOM.PlantId, SPOM.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
		                            , CP.UserName AS PartyAccountGroupName
		                            , SPOM.MaterialStorageId, SPOM.DocRefNo, REPLACE(CONVERT(CHAR(11), SPOM.DocDate, 106),' ','-') AS DocDate
		                            , SPOM.CurrencyId, CU.Code AS CurrencyCode, SPOM.BaseCurrencyId, SPOM.PaymentTermId, SPOM.BaseNoOfDays
		                            , REPLACE(CONVERT(CHAR(11), SPOM.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), SPOM.MatureDate, 106),' ','-') AS MatureDate
		                            , SPOM.FixedAssetOrInventory, SPOM.PODepended
		                            , SPOM.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, SPOM.InvoicingByAddress, SPOM.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, SPOM.DeliveryByAddress, SPOM.IsNonCreditable
		                            ,SPOM.ToCurrencyRate
		                            , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, SPOM.IsTaxApplicable
		                            , SPOM.IsApproved, SPOM.IsPaymentHold, SP.Id AS PlantStateId
		                            ,pgl.CtnId
		                            ,SPOM.AddedBy
		                            ,SPOM.CheckedByStatus AS CheckedByStatus
		                            ,SPOM.ApprovedByStatus AS ApprovedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy,PT.PaymentMode
		                            ,IRD.Amount
		                            ,IRD.TotalTaxAmount,SPOM.POType,SPOM.ContractId
                            FROM [TRN].[ServicePOMaster] AS SPOM JOIN [HKP].[Party] AS P ON SPOM.PartyId=P.Id
                            LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable 
                            FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                            ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor'
                            ) AS CP ON CP.PartyId=SPOM.PartyId AND CP.PlantId=SPOM.PlantId
                            LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=SPOM.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=SPOM.ApprovedBy
                            left JOIN [SCS].[Currency] AS CU ON SPOM.CurrencyId=CU.Id
                            left JOIN [MST].[PaymentTerm] AS PT ON SPOM.PaymentTermId=PT.Id
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON SPOM.InvoicingPartyPlantId=IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON SPOM.DeliveryPartyPlantId=DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                            LEFT JOIN [ORG].Plant PL ON PL.Id=SPOM.PlantId
                            LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
                            LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                            LEFT JOIN (SELECT A.ServicePOMasterId,sum(A.Amount) Amount, sum(A.TotalTaxAmount) TotalTaxAmount FROM [TRN].[ServicePODetail] AS A
                            left JOIN  [TRN].[ServicePOMaster] AS B ON A.ServicePOMasterId=B.Id WHERE B.PlantId='" + plantId + @"'  
                            GROUP BY A.ServicePOMasterId
                            ) AS IRD ON IRD.ServicePOMasterId=SPOM.Id
                            LEFT JOIN (Select count(Id) as CtnId,POID from TRN.ServicePurchaseOrderApprovalLog where Status='Approval' group by POID) as pgl  on pgl.POID=SPOM.Id
                        WHERE  SPOM.PlantId='" + identity.PlantId + @"'
						AND SPOM.POType='" + POType + @"' 
						AND (SPOM.CheckedByStatus='Hold' OR SPOM.CheckedByStatus='Reject' )
                        AND (Isnull(SPOM.ApprovedByStatus,'')='' OR SPOM.ApprovedByStatus is NULL)
						AND isnull(SPOM.IsClosed,0)=0 Order by PODate,Id DESC";




            }
            else if (POTypeStatus == "Checked")
            {



                Sql = @"--DECLARE @plantId VARCHAR(10)='20171';
                                                   SELECT ROW_NUMBER()  OVER (ORDER BY  SPOM.Id) AS SiNo,SPOM.Id
		                            , REPLACE(CONVERT(CHAR(11), SPOM.PODate, 106),' ','-') AS PODate
		                            , SPOM.CompanyGroupId, SPOM.CompanyId, SPOM.PlantId, SPOM.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
		                            , CP.UserName AS PartyAccountGroupName
		                            , SPOM.MaterialStorageId, SPOM.DocRefNo, REPLACE(CONVERT(CHAR(11), SPOM.DocDate, 106),' ','-') AS DocDate
		                            , SPOM.CurrencyId, CU.Code AS CurrencyCode, SPOM.BaseCurrencyId, SPOM.PaymentTermId, SPOM.BaseNoOfDays
		                            , REPLACE(CONVERT(CHAR(11), SPOM.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), SPOM.MatureDate, 106),' ','-') AS MatureDate
		                            , SPOM.FixedAssetOrInventory, SPOM.PODepended
		                            , SPOM.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, SPOM.InvoicingByAddress, SPOM.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, SPOM.DeliveryByAddress, SPOM.IsNonCreditable
		                            ,SPOM.ToCurrencyRate
		                            , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, SPOM.IsTaxApplicable
		                            , SPOM.IsApproved, SPOM.IsPaymentHold, SP.Id AS PlantStateId
		                            ,pgl.CtnId
		                            ,SPOM.AddedBy
		                            ,SPOM.CheckedByStatus AS CheckedByStatus
		                            ,SPOM.ApprovedByStatus AS ApprovedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy,PT.PaymentMode
		                            ,IRD.Amount
		                            ,IRD.TotalTaxAmount,SPOM.POType,SPOM.ContractId
                            FROM [TRN].[ServicePOMaster] AS SPOM JOIN [HKP].[Party] AS P ON SPOM.PartyId=P.Id
                            LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable 
                            FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                            ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor'
                            ) AS CP ON CP.PartyId=SPOM.PartyId AND CP.PlantId=SPOM.PlantId
                            LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=SPOM.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=SPOM.ApprovedBy
                            left JOIN [SCS].[Currency] AS CU ON SPOM.CurrencyId=CU.Id
                            left JOIN [MST].[PaymentTerm] AS PT ON SPOM.PaymentTermId=PT.Id
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON SPOM.InvoicingPartyPlantId=IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON SPOM.DeliveryPartyPlantId=DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                            LEFT JOIN [ORG].Plant PL ON PL.Id=SPOM.PlantId
                            LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
                            LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                            LEFT JOIN (SELECT A.ServicePOMasterId,sum(A.Amount) Amount, sum(A.TotalTaxAmount) TotalTaxAmount FROM [TRN].[ServicePODetail] AS A
                            left JOIN  [TRN].[ServicePOMaster] AS B ON A.ServicePOMasterId=B.Id WHERE B.PlantId='" + plantId + @"' 
                            GROUP BY A.ServicePOMasterId
                            ) AS IRD ON IRD.ServicePOMasterId=SPOM.Id
                            LEFT JOIN (Select count(Id) as CtnId,POID from TRN.ServicePurchaseOrderApprovalLog where Status='Approval' group by POID) as pgl  on pgl.POID=SPOM.Id
                      WHERE  SPOM.PlantId='" + identity.PlantId + @"'  AND SPOM.CheckedBy IS NOT NULL 
                        AND SPOM.CheckedByStatus='Checked' 
                        AND SPOM.ApprovedByStatus = 'For Approval'
                        AND SPOM.POType='" + POType + @"' 
                        AND SPOM.PlantId='" + plantId + "'   " + "AND isnull(SPOM.IsClosed,0)=0 Order by PODate,Id DESC";
                //IR.AddedBy='" + identity.Name + "' And

            }
            return _sqlRepository.GetDataCollection(Sql);
        }
        public IEnumerable<object> GetListForServicePOBYReqHR(string plantId, string ApproveRejectHold, string POType)
        {
            var Sql = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (ApproveRejectHold == "Approval")
            {
                Sql = @"--DECLARE @plantId VARCHAR(10)='20171';
                                 Select * from       (           SELECT ROW_NUMBER()  OVER (ORDER BY  SPOM.Id) AS SiNo,SPOM.Id
		                            , REPLACE(CONVERT(CHAR(11), SPOM.PODate, 106),' ','-') AS PODate
		                            , SPOM.CompanyGroupId, SPOM.CompanyId, SPOM.PlantId, SPOM.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
		                            , CP.UserName AS PartyAccountGroupName
		                            , SPOM.MaterialStorageId, SPOM.DocRefNo, REPLACE(CONVERT(CHAR(11), SPOM.DocDate, 106),' ','-') AS DocDate
		                            , SPOM.CurrencyId, CU.Code AS CurrencyCode, SPOM.BaseCurrencyId, SPOM.PaymentTermId, SPOM.BaseNoOfDays
		                            , REPLACE(CONVERT(CHAR(11), SPOM.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), SPOM.MatureDate, 106),' ','-') AS MatureDate
		                            , SPOM.FixedAssetOrInventory, SPOM.PODepended
		                            , SPOM.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, SPOM.InvoicingByAddress, SPOM.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, SPOM.DeliveryByAddress, SPOM.IsNonCreditable
		                            ,SPOM.ToCurrencyRate
		                            , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, SPOM.IsTaxApplicable
		                            , SPOM.IsApproved, SPOM.IsPaymentHold, SP.Id AS PlantStateId
		                            ,pgl.CtnId
		                            ,SPOM.AddedBy
		                            ,SPOM.CheckedByStatus AS CheckedByStatus
		                            ,SPOM.ApprovedByStatus AS ApprovedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy,PT.PaymentMode
		                            ,IRD.Amount
		                            ,IRD.TotalTaxAmount,SPOM.POType
                            FROM [TRN].[ServicePOMaster] AS SPOM JOIN [HKP].[Party] AS P ON SPOM.PartyId=P.Id
                            LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable 
                            FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                            ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor'
                            ) AS CP ON CP.PartyId=SPOM.PartyId AND CP.PlantId=SPOM.PlantId
                            LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=SPOM.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=SPOM.ApprovedBy
                            left JOIN [SCS].[Currency] AS CU ON SPOM.CurrencyId=CU.Id
                            left JOIN [MST].[PaymentTerm] AS PT ON SPOM.PaymentTermId=PT.Id
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON SPOM.InvoicingPartyPlantId=IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON SPOM.DeliveryPartyPlantId=DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                            LEFT JOIN [ORG].Plant PL ON PL.Id=SPOM.PlantId
                            LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
                            LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                            LEFT JOIN (SELECT A.ServicePOMasterId,sum(A.Amount) Amount, sum(A.TotalTaxAmount) TotalTaxAmount FROM [TRN].[ServicePODetail] AS A
                            left JOIN  [TRN].[ServicePOMaster] AS B ON A.ServicePOMasterId=B.Id WHERE B.PlantId='" + plantId + @"'  
                            GROUP BY A.ServicePOMasterId
                            ) AS IRD ON IRD.ServicePOMasterId=SPOM.Id
                            LEFT JOIN (Select count(Id) as CtnId,POID from TRN.ServicePurchaseOrderApprovalLog where Status='Approval' group by POID) as pgl  on pgl.POID=SPOM.Id
                        WHERE  SPOM.POType='" + POType + @"'
                        AND SPOM.PlantId='" + plantId + @"' 
						AND SPOM.CheckedByStatus='Checked' 
						AND  SPOM.ApprovedByStatus ='Approved'
						 AND isnull(SPOM.IsClosed,0)=0 
						

						UNION ALL

						  SELECT ROW_NUMBER()  OVER (ORDER BY  SPOM.Id) AS SiNo,SPOM.Id
		                            , REPLACE(CONVERT(CHAR(11), SPOM.PODate, 106),' ','-') AS PODate
		                            , SPOM.CompanyGroupId, SPOM.CompanyId, SPOM.PlantId, SPOM.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
		                            , CP.UserName AS PartyAccountGroupName
		                            , SPOM.MaterialStorageId, SPOM.DocRefNo, REPLACE(CONVERT(CHAR(11), SPOM.DocDate, 106),' ','-') AS DocDate
		                            , SPOM.CurrencyId, CU.Code AS CurrencyCode, SPOM.BaseCurrencyId, SPOM.PaymentTermId, SPOM.BaseNoOfDays
		                            , REPLACE(CONVERT(CHAR(11), SPOM.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), SPOM.MatureDate, 106),' ','-') AS MatureDate
		                            , SPOM.FixedAssetOrInventory, SPOM.PODepended
		                            , SPOM.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, SPOM.InvoicingByAddress, SPOM.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, SPOM.DeliveryByAddress, SPOM.IsNonCreditable
		                            ,SPOM.ToCurrencyRate
		                            , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, SPOM.IsTaxApplicable
		                            , SPOM.IsApproved, SPOM.IsPaymentHold, SP.Id AS PlantStateId
		                            ,pgl.CtnId
		                            ,SPOM.AddedBy
		                            ,SPOM.CheckedByStatus AS CheckedByStatus
		                            ,SPOM.ApprovedByStatus AS ApprovedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy,PT.PaymentMode
		                            ,IRD.Amount
		                            ,IRD.TotalTaxAmount,SPOM.POType
                            FROM [TRN].[ServicePOMaster] AS SPOM JOIN [HKP].[Party] AS P ON SPOM.PartyId=P.Id
                            LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable 
                            FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                            ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor'
                            ) AS CP ON CP.PartyId=SPOM.PartyId AND CP.PlantId=SPOM.PlantId
                            LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=SPOM.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=SPOM.ApprovedBy
                            left JOIN [SCS].[Currency] AS CU ON SPOM.CurrencyId=CU.Id
                            left JOIN [MST].[PaymentTerm] AS PT ON SPOM.PaymentTermId=PT.Id
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON SPOM.InvoicingPartyPlantId=IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON SPOM.DeliveryPartyPlantId=DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                            LEFT JOIN [ORG].Plant PL ON PL.Id=SPOM.PlantId
                            LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
                            LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                            LEFT JOIN (SELECT A.ServicePOMasterId,sum(A.Amount) Amount, sum(A.TotalTaxAmount) TotalTaxAmount FROM [TRN].[ServicePODetail] AS A
                            left JOIN  [TRN].[ServicePOMaster] AS B ON A.ServicePOMasterId=B.Id WHERE B.PlantId='" + plantId + @"'   
                            GROUP BY A.ServicePOMasterId
                            ) AS IRD ON IRD.ServicePOMasterId=SPOM.Id
                            LEFT JOIN (Select count(Id) as CtnId,POID from TRN.ServicePurchaseOrderApprovalLog where Status='Approval' group by POID) as pgl  on pgl.POID=SPOM.Id
                        WHERE  SPOM.POType='" + POType + @"'
                        AND SPOM.PlantId='" + plantId + @"' 
						AND (SPOM.CheckedByStatus IS NULL OR ISNULL(SPOM.CheckedByStatus,'')='')
						AND SPOM.ApprovedByStatus='Approved' 
						AND isnull(SPOM.IsClosed,0)=0 
						
						UNION ALL

						  SELECT ROW_NUMBER()  OVER (ORDER BY  SPOM.Id) AS SiNo,SPOM.Id
		                            , REPLACE(CONVERT(CHAR(11), SPOM.PODate, 106),' ','-') AS PODate
		                            , SPOM.CompanyGroupId, SPOM.CompanyId, SPOM.PlantId, SPOM.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
		                            , CP.UserName AS PartyAccountGroupName
		                            , SPOM.MaterialStorageId, SPOM.DocRefNo, REPLACE(CONVERT(CHAR(11), SPOM.DocDate, 106),' ','-') AS DocDate
		                            , SPOM.CurrencyId, CU.Code AS CurrencyCode, SPOM.BaseCurrencyId, SPOM.PaymentTermId, SPOM.BaseNoOfDays
		                            , REPLACE(CONVERT(CHAR(11), SPOM.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), SPOM.MatureDate, 106),' ','-') AS MatureDate
		                            , SPOM.FixedAssetOrInventory, SPOM.PODepended
		                            , SPOM.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, SPOM.InvoicingByAddress, SPOM.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, SPOM.DeliveryByAddress, SPOM.IsNonCreditable
		                            ,SPOM.ToCurrencyRate
		                            , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, SPOM.IsTaxApplicable
		                            , SPOM.IsApproved, SPOM.IsPaymentHold, SP.Id AS PlantStateId
		                            ,pgl.CtnId
		                            ,SPOM.AddedBy
		                            ,SPOM.CheckedByStatus AS CheckedByStatus
		                            ,SPOM.ApprovedByStatus AS ApprovedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy,PT.PaymentMode
		                            ,IRD.Amount
		                            ,IRD.TotalTaxAmount,SPOM.POType
                            FROM [TRN].[ServicePOMaster] AS SPOM JOIN [HKP].[Party] AS P ON SPOM.PartyId=P.Id
                            LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable 
                            FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                            ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor'
                            ) AS CP ON CP.PartyId=SPOM.PartyId AND CP.PlantId=SPOM.PlantId
                            LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=SPOM.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=SPOM.ApprovedBy
                            left JOIN [SCS].[Currency] AS CU ON SPOM.CurrencyId=CU.Id
                            left JOIN [MST].[PaymentTerm] AS PT ON SPOM.PaymentTermId=PT.Id
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON SPOM.InvoicingPartyPlantId=IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON SPOM.DeliveryPartyPlantId=DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                            LEFT JOIN [ORG].Plant PL ON PL.Id=SPOM.PlantId
                            LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
                            LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                            LEFT JOIN (SELECT A.ServicePOMasterId,sum(A.Amount) Amount, sum(A.TotalTaxAmount) TotalTaxAmount FROM [TRN].[ServicePODetail] AS A
                            left JOIN  [TRN].[ServicePOMaster] AS B ON A.ServicePOMasterId=B.Id WHERE B.PlantId='" + plantId + @"'   
                            GROUP BY A.ServicePOMasterId
                            ) AS IRD ON IRD.ServicePOMasterId=SPOM.Id
                            LEFT JOIN (Select count(Id) as CtnId,POID from TRN.ServicePurchaseOrderApprovalLog where Status='Approval' group by POID) as pgl  on pgl.POID=SPOM.Id
                        WHERE  SPOM.POType='" + POType + @"'
                        AND SPOM.PlantId='" + plantId + @"' 
						AND (SPOM.CheckedByStatus IS NULL OR ISNULL(SPOM.CheckedByStatus,'')='')
						AND (SPOM.ApprovedByStatus IS NULL OR ISNULL(SPOM.ApprovedByStatus,'')='') 
						AND isnull(SPOM.IsClosed,0)=0 
						AND SPOM.Id  in( Select ServicePOMasterId from trn.ServicePODetail where ServicePOMasterId IS NOT NULL)
						)X
						Order by PODate,Id DESC";


            }
            else
            {
                Sql = @"--DECLARE @plantId VARCHAR(10)='20171';
                                                   SELECT ROW_NUMBER()  OVER (ORDER BY  SPOM.Id) AS SiNo,SPOM.Id
		                            , REPLACE(CONVERT(CHAR(11), SPOM.PODate, 106),' ','-') AS PODate
		                            , SPOM.CompanyGroupId, SPOM.CompanyId, SPOM.PlantId, SPOM.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
		                            , CP.UserName AS PartyAccountGroupName
		                            , SPOM.MaterialStorageId, SPOM.DocRefNo, REPLACE(CONVERT(CHAR(11), SPOM.DocDate, 106),' ','-') AS DocDate
		                            , SPOM.CurrencyId, CU.Code AS CurrencyCode, SPOM.BaseCurrencyId, SPOM.PaymentTermId, SPOM.BaseNoOfDays
		                            , REPLACE(CONVERT(CHAR(11), SPOM.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), SPOM.MatureDate, 106),' ','-') AS MatureDate
		                            , SPOM.FixedAssetOrInventory, SPOM.PODepended
		                            , SPOM.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, SPOM.InvoicingByAddress, SPOM.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, SPOM.DeliveryByAddress, SPOM.IsNonCreditable
		                            ,SPOM.ToCurrencyRate
		                            , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, SPOM.IsTaxApplicable
		                            , SPOM.IsApproved, SPOM.IsPaymentHold, SP.Id AS PlantStateId
		                            ,pgl.CtnId
		                            ,SPOM.AddedBy
		                            ,SPOM.CheckedByStatus AS CheckedByStatus
		                            ,SPOM.ApprovedByStatus AS ApprovedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy,PT.PaymentMode
		                            ,IRD.Amount
		                            ,IRD.TotalTaxAmount,SPOM.POType
                            FROM [TRN].[ServicePOMaster] AS SPOM JOIN [HKP].[Party] AS P ON SPOM.PartyId=P.Id
                            LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable 
                            FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                            ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor'
                            ) AS CP ON CP.PartyId=SPOM.PartyId AND CP.PlantId=SPOM.PlantId
                            LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=SPOM.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=SPOM.ApprovedBy
                            left JOIN [SCS].[Currency] AS CU ON SPOM.CurrencyId=CU.Id
                            left JOIN [MST].[PaymentTerm] AS PT ON SPOM.PaymentTermId=PT.Id
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON SPOM.InvoicingPartyPlantId=IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON SPOM.DeliveryPartyPlantId=DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                            LEFT JOIN [ORG].Plant PL ON PL.Id=SPOM.PlantId
                            LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
                            LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                            LEFT JOIN (SELECT A.ServicePOMasterId,sum(A.Amount) Amount, sum(A.TotalTaxAmount) TotalTaxAmount FROM [TRN].[ServicePODetail] AS A
                            left JOIN  [TRN].[ServicePOMaster] AS B ON A.ServicePOMasterId=B.Id WHERE B.PlantId='" + plantId + @"'  
                            GROUP BY A.ServicePOMasterId
                            ) AS IRD ON IRD.ServicePOMasterId=SPOM.Id
                            LEFT JOIN (Select count(Id) as CtnId,POID from TRN.ServicePurchaseOrderApprovalLog where Status='Approval' group by POID) as pgl  on pgl.POID=SPOM.Id
                        WHERE  SPOM.POType='" + POType + @"'
                        AND SPOM.PlantId='" + plantId + "' " +
                       "AND SPOM.CheckedByStatus='Checked' " +
                       "AND (SPOM.ApprovedByStatus = 'HOld' OR SPOM.ApprovedByStatus = 'Reject') " +
                       "AND isnull(SPOM.IsClosed,0)=0 Order by SPOM.PODate ASC";//IR.AddedBy='" + identity.Name + "' AND

            }
            return _sqlRepository.GetDataCollection(Sql);
        }

        //public IEnumerable<object> GetListForServiceRequisition(string Id)
        //{
        //    try
        //    {

        //        var sql = @"SELECT
        //                   SRD.Id ServiceRequsitionDetailId 
        //                  ,SRD.ServiceRequisitionMasterID ServiceReqMasterId
        //                  ,SRD.CurrencyId
        //                  ,SRD.Rate
        //                  --,SM.Description
        //                  ,SRD.ServiceMasterId
        //                  ,SRD.TotalServiceTranAmount
        //                  ,SRD.TotalServiceBooksCurrencyAmount Amount 
        //                  ,SRD.AddedBy
        //                  ,REPLACE(CONVERT(CHAR(11), SRM.RequisitionDate, 106),' ','-') AS RequisitionDate 
        //                  ,SRD.AddedDate
        //                  ,SRD.AddedFromIP
        //                  ,SRD.UpdatedBy
        //                  ,SRD.UpdatedDate
        //                  ,SRD.UpdatedFromIP
        //                  ,SRD.Remarks,SRD.RefferenceNo
        //                  ,SM.StandardName ServiceMasterName
        //,SM.ID ServiceMasterId
        //                  ,CR.Code CurrencyName
        //                  ,0  Active 
        //                  ,SRD.Id ServiceRequsitionDetailId
        //               ,SRD.Description,SM.HSNCodeId
        //                  ,ISNULL(SRD.Qty,0) Qty
        //,ISNULL(SRD.TransactionRate,0) TransactionRate
        //                  ,UOM.UserName UoM
        //                  ,SRD.TransactionUoMId
        //          FROM TRN.ServiceRequsitionDetail SRD
        //          left JOIN[TRN].[ServiceRequsitionMaster] AS SRM ON SRM.Id=SRD.ServiceRequisitionMasterID
        //          left JOIN[HKP].[ServiceMaster]   AS SM ON SM.Id= SRD.ServiceMasterId
        //          left JOIN [SCS].[Currency] AS CR ON CR .Id= SRD.CurrencyId
        //          left JOIN SCS.UnitOfMeasurement UOM ON UOM.Id=SRD.TransactionUoMId
        //          LEFT JOIN (SELECT SPD.ServicePOMasterId,ServiceRequsitionDetailId,SUM(Qty) Qty from trn.ServicePODetail SPD
        //LEFT JOIN trn.servicePOMaster SPM ON SPD.ServicePOMasterId=SPM.Id
        //--where SPD.ServicePOMasterId!='"+ Id + @"'
        //GROUP BY ServiceRequsitionDetailId,SPD.ServicePOMasterId
        //)PODetail ON PODetail.ServiceRequsitionDetailId=SRD.Id				 
        //          WHERE SRM.AuthorizedByStatus='Approved'

        //        --SRD.Id not in(select ServiceRequsitionDetailId from trn.ServicePODetail where ServiceRequsitionDetailId is not null)";
        //        //Where SRM.ComCompanyGroupId= '" + CompanyId + "'";
        //        return _sqlRepository.GetDataCollection(sql);
        //    }//Order by MGM.UserName , IM.MaterialMasterId, MM.UserName, IM.ArticleId, ART.StandardName, IM.FirstCharacteristicsId, FC.UserName , IM.FirstCharacteristicsValueId, FCV.UserName , IM.SecondCharacteristicsId, SC.UserName , IM.SecondCharacteristicsValueId, SCV.UserName ,IM.ThirdCharacteristicsId, TC.UserName,IM.ThirdCharacteristicsValueId, TCV.UserName DESC
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
        //    }
        //}


        public IEnumerable<object> GetServicePOByReqSupervisorCbo()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select E.SystemId As Value, (E.Employeecode+'-'+ E.EmployeeName) As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='ServicePOCheckedBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #region Taufik Service PO Reportby 
        public void ServicePurchaseOrderReport(string CompanyGroupId, string plantId, string purchaseOrderId)
        {
            ReportUtility ru = new ReportUtility();

            var fileName = "";
            var strPath = "";

            var File = "";

            fileName = "ServicePurchaseOrder" + plantId + ".docx";
            strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }
            WordDocument document = new WordDocument(File, FormatType.Docx);

            try
            {
                string invoicePartyAddress = "";
                string vendorPartyAddress = "";
                WSection section = document.Sections[0];
                DataTable dsOrderMaster, dsServiceItems;
                dsOrderMaster = loadOrderServicePOMaster(purchaseOrderId);//sql
                Dictionary<string, string> columns = new Dictionary<string, string>();
                var poApprovedStatus = "";
                document.Replace("{AddedBy}", dsOrderMaster.Rows[0]["AddedBy"].ToString(), false, false);
                var serviceTotal = 0.00;
                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);
                DataTable dsMaterialItems = ServicePODetail(purchaseOrderId);
                var materialTotal = MakeMaterialDetailsTable(document, dsMaterialItems, purchaseOrderId);//Material Details 
                //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                //document.Replace("{TotalInWords}", ru.InWord(clsStaticInfo.dbl(materialTotal + serviceTotal), dsMaterialItems.Rows[0]["CurrencyId"].ToString()), true, true);


                var DiscountAmount = "";
                DiscountAmount = dsOrderMaster.Rows[0]["DiscountAmount"].ToString();
                document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{DiscountAmount}", (DiscountAmount).ToString() + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{AfterDiscountTotal}", ((clsStaticInfo.dbl(materialTotal.ToString()) + clsStaticInfo.dbl(serviceTotal.ToString())) - clsStaticInfo.dbl(DiscountAmount.ToString())).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{TotalInWords}", ru.InWord(((clsStaticInfo.dbl(materialTotal.ToString()) + clsStaticInfo.dbl(serviceTotal.ToString())) - clsStaticInfo.dbl(DiscountAmount.ToString())), dsOrderMaster.Rows[0]["CurrencyId"].ToString()), true, true);


                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();
                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                List<string> strReplace = new List<string>();
                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());

                StringCollection strColDistinct = new StringCollection();
                for (int i = 0; i < strReplace.Count; i++)
                {
                    if (strColDistinct.Contains(strReplace[i].ToUpper()))
                        continue;

                    strColDistinct.Add(strReplace[i].ToUpper());

                    string text = strReplace[i].ToUpper();
                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        ReplaceInfo[text] = document.Replace(text, dsOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }
                }
                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);

                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "", false, false);

                }

                //Region that is for Pdf.Document
                DocToPDFConverter converter = new DocToPDFConverter();

                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                converter.Dispose();
                document.Close();
                string Prefix = "ServicePO" + purchaseOrderId;
                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                pdfDocument.Close(true);
                document.Close();

            }
            catch (Exception ex)
            {
                throw ex;

            }
            document.Close();
        }




        public DataTable loadOrderServicePOMaster(string purchaseOrderId)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT SPO.Id PONumber
                                                ,SPO.ContractId
                                                ,PurchaseLCId
                                                ,SPO.CompanyGroupId
                                                ,SPO.CompanyId
                                                ,Plant.GSTIN
                                                ,REPLACE(Convert(VARCHAR(11), SPO.PODate, 106), ' ', '-') AS PODate
                                                ,SPO.POType
                                                ,REPLACE(Convert(VARCHAR(11), SPO.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
                                                ,REPLACE(Convert(VARCHAR(11), SPO.MatureDate, 106), ' ', '-') AS MatureDate
		                                        ,SPO.InvoicingPartyPlantId
		                                        ,INVPARTYPL.UserName InvoicingPartyName
                                                ,INVPARTYPL.AddressMasterId InvoicePartyAddressMasterId
                                                ,INVPARTYPL.GSTIN InvoicingPartyGSTIN
                                                ,ISNULL(SPO.InvoicingByAddress,'') InvoicingByAddress
												,SPO.DeliveryByAddress
		                                        ,DPARTYPL.UserName DeliveryParty
		                                        ,SPO.DeliveryPartyPlantId		
		                                        ,POD.InventoryMaterialId MaterialMasterId
		                                        ,SPO.DocRefNo
                                                ,REPLACE(Convert(VARCHAR(11), SPO.DocDate, 106), ' ', '-') AS DocDate
		                                         ,Convert(decimal(18,2), ISNULL(SPO.DiscountAmount, 0)) DiscountAmount
		                                        ,SPO.AddedDate
		                                        ,SPO.UpdatedBy
		                                        ,SPO.UpdatedDate
		                                        ,SPO.IsApproved 
		                                        ,SPO.PartyType
												,SPO.PartyId
                                                ,ISNULL(SPO.DeliveryInstruction,'') DeliveryInstruction
												,ISNULL(SPO.SpecialInstruction,'') SpecialInstruction
												,Party.UserName VendorName
                                                ,Party.AddressMasterId VendorAddressMasterId
                                                ,Party.TINNO VendorGSTIN
		                                        ,Case When SPO.IsNonCreditable = 1 then 'NonCreditable' when SPO.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
		                                        ,SPO.CurrencyId
	                                            ,CRNC.Code AS CurrencyName
	                                            ,SPO.ToCurrencyRate
		                                        ,BASECRNC.Code AS BaseCurrencyName
		                                        ,PayTerm.UserName PaymentTerm
	                                          ,MM.UserName MaterialMaster
	                                          ,MM.MaterialGroupMasterId
	                                          ,MGM.UserName MaterialGroupMaster
	                                          ,POD.ArticleId
	                                          ,MMA.StandardName Article
	                                          ,FC.Id FirstCharId
	                                          ,FC.UserName FirstChar
                                              ,POD.FirstCharacteristicsValueId
	                                          ,FCV.UserName AS FirstCharacteristicsValue
                                              ,POD.SecondCharacteristicsValueId
	                                          ,SCV.UserName AS SecondCharacteristicsValue
	                                          ,POD.ThirdCharacteristicsValueId
	                                          ,TCV.UserName AS ThirdCharacteristicsValue
	                                          ,SC.Id SecondCharId
	                                          ,SC.UserName SecondChar
	                                          ,TC.Id ThirdCharId
	                                          ,TC.UserName ThirdChar
	                                          ,ROUND(POD.TransactionQty, 2) POTransactionQty
	                                          ,ROUND(POD.TransactionRate, 2) TransactionRate
	                                          ,ROUND((POD.TransactionQty * POD.TransactionRate), 2) AS TrnAmount
	                                          ,POD.BaseAmount
	                                          ,POD.TotalTaxAmount AS BaseTaxAmount
	                                          ,TaxAmount = (
		                                            SELECT SUM(TaxAmount)
		                                            FROM [TRN].[PurchaseOrderTax]
		                                            WHERE InventoryReceiveDetailId = POD.Id
		                                            )
	                                          ,ServiceTaxAmount = (
		                                            SELECT SUM(TotalTaxAmount)
		                                            FROM [TRN].[POService]
		                                            WHERE InventoryReceiveId = POD.InventoryReceiveId
		                                            )
                                              ,POD.Description
	                                          ,POD.ChargesAmount
	                                          ,POD.CountryId
	                                          ,POCountry.UserName CountryOfOrigin
                                                ,POD.Id PurchaseOrderDetailId
	                                          ,POD.TransactionUoMId
	                                          ,TUoM.UserName AS TransactionUoM
                                               ,MRMD.MaterialDetail MaterialDetail

											   ,CheckedBy=eI.EmployeeName 
                                                ,AuthorizedBy=CASE When SPO.ApprovedByStatus='Approved'then eI1.EmployeeName else '' END
                                                ,AddedBy=CASE When SPO.CheckedByStatus='For Checking' OR SPO.CheckedByStatus='Hold' OR SPO.CheckedByStatus='Reject' OR SPO.CheckedByStatus='Checked'then eI3.EmployeeName else ''  END 
                                             	,PurOrCheckedStatus= CASE when SPO.CheckedByStatus='For Checking' Then 'To be Checked'
                                           when SPO.CheckedByStatus='Hold' Then 'Hold'
						                   when SPO.CheckedByStatus='Reject' Then 'Reject'
						                   when SPO.CheckedByStatus='Checked' Then 'Checked'
						                  else ''
						   
							                END
			                           ,PurOrApprovedStatus= CASE 
						                   when SPO.ApprovedByStatus='Reject' Then 'Reject For Approved'
						                   when SPO.ApprovedByStatus='Hold' Then 'Hold For Approved'
						                   when SPO.ApprovedByStatus='For Approval' Then 'To be Approval'
						                   when SPO.ApprovedByStatus='Approval' Then 'Approved'
						                   else ''
							                END
											 
											  FROM [TRN].[ServicePOMaster] SPO
                                         LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = SPO.CompanyGroupId
                                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = SPO.CompanyId
                                         LEFT JOIN ORG.Plant Plant ON Plant.Id = SPO.PlantId
										  LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=SPO.CheckedBy
										 LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=SPO.ApprovedBy
										 left join [SEC].[User] U on U.UserId=SPO.AddedBy
                                         LEFT JOIN dbo.EmployeeInformation eI3 ON eI3.SystemId=U.EmployeeId
                                         LEFT JOIN SCS.Currency CRNC ON CRNC.Id = SPO.CurrencyId
                                         LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = SPO.BaseCurrencyId
                                         LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = SPO.PaymentTermId
                                         LEFT JOIN HKP.PartyPlant  INVPARTYPL ON INVPARTYPL.Id = SPO.InvoicingPartyPlantId
                                         LEFT JOIN HKP.PartyPlant  DPARTYPL ON DPARTYPL.Id = SPO.DeliveryPartyPlantId                                          
                                         LEFT JOIN TRN.PurchaseOrderDetail POD ON SPO.Id = POD.InventoryReceiveId
                                         LEFT JOIN SCS.Country POCountry ON POD.CountryId = POCountry.Id
										 LEFT JOIN HKP.Party Party ON Party.Id = SPO.PartyId                                        
                                         LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = POD.InventoryMaterialId
                                         LEFT JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                                         LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = POD.ArticleId
                                         LEFT JOIN HKP.Characteristics AS FC ON POD.FirstCharacteristicsId = FC.Id
                                         LEFT JOIN HKP.Characteristics AS SC ON POD.SecondCharacteristicsId = SC.Id
                                         LEFT JOIN HKP.Characteristics AS TC ON POD.ThirdCharacteristicsId = TC.Id
                                         LEFT JOIN HKP.CharacteristicsValue AS FCV ON POD.FirstCharacteristicsValueId = FCV.Id
                                         LEFT JOIN HKP.CharacteristicsValue AS SCV ON POD.SecondCharacteristicsValueId = SCV.Id
                                         LEFT JOIN HKP.CharacteristicsValue AS TCV ON POD.ThirdCharacteristicsValueId = TCV.Id
                                         LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON POD.TransactionUoMId = TUoM.Id
			                            LEFT JOIN TRN.MaterialRequsitionDetails AS MRMD ON MRMD.Id=POD.RequisitionDetailId
	                            WHERE SPO.id='" + purchaseOrderId + @"'";
                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        public double MakeMaterialDetailsTable(WordDocument document, DataTable dsOrderMaster, string purchaseOrderId)
        {
            string replaceString = "{ServicePODetail}";
            ReportUtility ru = new ReportUtility();
            DataTable dsOrderItems, dsTax;
            dsTax = loadServicePOTax(purchaseOrderId);
            int LasColumnIndex = 7;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));
            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    LasColumnIndex++;
                }
            }
            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);
            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            //wTable.Title = "Material Details";
            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;
            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("PO Detail Id");
            range.ApplyCharacterFormat(FontBold);
            int colRowId = COL; COL++;
            wTable.Rows[ROW].Cells[colRowId].Width = 40;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Service Name");
            range.ApplyCharacterFormat(FontBold);
            int colSN = COL; COL++;
            wTable.Rows[ROW].Cells[colSN].Width = 80;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Description");
            range.ApplyCharacterFormat(FontBold);
            int colDescription = COL; COL++;
            wTable.Rows[ROW].Cells[colDescription].Width = 200;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;
            wTable.Rows[ROW].Cells[colQty].Width = 60;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UoM");
            range.ApplyCharacterFormat(FontBold);
            int colUoM = COL; COL++;
            wTable.Rows[ROW].Cells[colUoM].Width = 40;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate");
            range.ApplyCharacterFormat(FontBold);
            int colRate = COL; COL++;
            wTable.Rows[ROW].Cells[colRate].Width = 60;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Currency");
            range.ApplyCharacterFormat(FontBold);
            int colCurrency = COL; /*COL++;*/
            wTable.Rows[ROW].Cells[colCurrency].Width = 45;


            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Amount(TRN)");
            //range.ApplyCharacterFormat(FontBold);
            //int colATRN = COL; //COL++;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
                wTable.Rows[ROW].Cells[colTotalTaxableAmount].Width = 60;
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        //two columns required for tax
                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                        range.ApplyCharacterFormat(FontBold);
                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
                range.ApplyCharacterFormat(FontBold);
                //int colTotalAmount = COL;
            }


            if (dv.Count > 0)
            {
                wTable.Rows.Add(TemplateRow);
                ROW++;
                WTableRow TROW = wTable.LastRow;
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
                        range.ApplyCharacterFormat(FontBold);
                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }
            }
            #endregion column headers
            if (dv.Count > 0)
            {
                wTable.Rows.Add(TemplateRow);

                WTableRow TROW = wTable.LastRow;
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                for (int i = 0; i < dv.Count; i++)
                {

                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
                    range.ApplyCharacterFormat(FontBold);
                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                    range.ApplyCharacterFormat(FontBold);
                }
                ROW++;
            }
            #endregion column headers
            double totalValue = 0;
            int startRow = ROW;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                ROW++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                }

                TROW.Cells[colRowId].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Id"].ToString());
                TROW.Cells[colSN].AddParagraph().AppendText(dsOrderMaster.Rows[i]["ServiceMasterName"].ToString());
                TROW.Cells[colDescription].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Description"].ToString());
                TROW.Cells[colQty].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["Qty"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colUoM].AddParagraph().AppendText(dsOrderMaster.Rows[i]["UoM"].ToString());
                TROW.Cells[colRate].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["Rate"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colCurrency].AddParagraph().AppendText(dsOrderMaster.Rows[i]["CurrencyName"].ToString());
                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["TotalServicAmount"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["TotalServicAmount"].ToString()).ToString("#,##0.00"));
                if (dv.Count > 0)
                {
                    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
                    for (int T = 0; T < dv.Count; T++)
                    {
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND ServicePODetailId='" + dsOrderMaster.Rows[i]["Id"].ToString() + "'";
                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("#,##0.00"));
                        }
                    }
                }
            }

            ROW++;
            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);

            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (C == colRowId || C == colSN || C == colDescription || C == colQty || C == colRate || C == colUoM || C == colCurrency || dicTaxes.ContainsValue(C))
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);

            }
            #endregion Total


            ROW++;
            #region Sub Total
            double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(TotalServicAmount)", "").ToString())
                //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                + clsStdLib.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString())
                ;

            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2") + " (" + ru.InWord(total, dsOrderMaster.Rows[0]["CurrencyId"].ToString()) + ")");

            #endregion Total

            ROW++;
            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            //myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle");
                    }
                }
            }

            IWParagraphStyle myStyleRightAlign = document.AddParagraphStyle("MyStyleRightAlign");
            //Sets the formatting of the style
            myStyleRightAlign.CharacterFormat.FontSize = 8f;
            myStyleRightAlign.CharacterFormat.TextColor = Color.Black;
            myStyleRightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;



            for (int R = 1; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];



                //foreach (WParagraph item in TROW.Cells[colATRN].Paragraphs)
                //{
                //	item.ApplyStyle("MyStyleRightAlign");
                //}


                foreach (WParagraph item in TROW.Cells[colTotalTaxableAmount].Paragraphs)
                {
                    item.ApplyStyle("MyStyleRightAlign");
                }

            }



            #endregion paragrpath formats

            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            for (int i = 0; i < dv.Count; i++)
                wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            for (int i = 0; i <= colTotalTaxableAmount; i++)
                wTable.ApplyVerticalMerge(i, ROW - 1, ROW);

            IWParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
            style.CharacterFormat.Bold = true;
            style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section

            #endregion merging section

            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);
            return total;
        }
        public DataTable loadServicePOTax(string purchaseOrderId)
        {
            string strSQL;
            try
            {
                strSQL = @"select ServicePODetailId,SPO.Id ServicePOMasterId,SPOD.Id ServicePODetailId1,tg.Code AS TaxCode,SPOTx.Percentage, SPOTx.TaxAmount 
							from [TRN].[ServicePOMaster] SPO
                            Left JOIN 	[TRN].[ServicePODetail] SPOD ON SPOD.ServicePOMasterId = SPO.Id
                            Left join [TRN].[ServicePOTax] SPOTx ON  SPOTx.ServicePODetailId = SPOD.Id
                            LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=SPOTx.TaxCategoryId
                           WHERE SPO.Id='" + purchaseOrderId + @"' 
							and ServicePODetailId  is not null --and  ServiceMasterId is null 
							ORDER BY tg.[Sequence]";
                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }
        private DataTable ServicePODetail(string purchaseOrderId)
        {
            try
            {
                string sqlText = @"--DECLARE @inventoryReceiveId VARCHAR(10)='" + purchaseOrderId + @"'
	           
                      SELECT SPOD. [Id] 
                      ,SPOD.[ServicePOMasterId]
                      ,SPOD.[ServiceMasterId]
	                  ,SM.StandardName ServiceMasterName
                      ,SPOD.Qty 
	                  ,UOM.ShortName UoM
	                  ,SPOD.Rate
                      ,SPOD.[Amount] 
                    ,ROUND(SPOD.[Amount] , 2) TotalServicAmount
                    --,ROUND(SPTax.TaxAmount , 2) TotalTaxAmount
					,ROUND(SPOD.TotalTaxAmount  , 2) TotalTaxAmount
                      ,SPOD.[AddedBy]
                      ,SPOD.[AddedDate]
                      ,SPOD.[AddedFromIP]
                      ,SPOD.[UpdatedBy]
                      ,SPOD.[UpdatedDate]
                      ,SPOD.[UpdatedFromIP]
                      ,SPOD.[GRNServiceAmount]
                      ,SPOD.[AmountStatus]
                      ,SPOD.[Description]
	                  ,CR.Code CurrencyName
                      ,SPOM.CurrencyId
                  FROM [TRN].[ServicePODetail] SPOD
                  left JOIN [TRN].[ServicePOMaster]  AS SPOM ON SPOM.Id=SPOD.ServicePOMasterId
                  left JOIN [SCS].[Currency] AS CR ON CR .Id=SPOM.CurrencyId
                  left JOIN [HKP].[ServiceMaster]  AS SM ON SM.Id=SPOD.ServiceMasterId
                  left join(select ServicePODetailId, sum(TaxAmount) TaxAmount from  TRN.ServicePOTax group by ServicePODetailId) AS SPTax ON SPOD.Id=SPTax.ServicePODetailId
                   Left JOin [SCS].[UnitOfMeasurement] UOM ON UOM.Id=SPOD.TransactionUoMId
                  Where SPOM.Id='" + purchaseOrderId + @"'";

                return _sqlRepository.GetDataTable(sqlText);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        #endregion


        #region Taufik Service Acknowledgement Report 
        public void ServiceAcknowledgementReport(string CompanyGroupId, string plantId, string SurviceAckId)
        {
            ReportUtility ru = new ReportUtility();

            var fileName = "";
            var strPath = "";

            var File = "";

            fileName = "ServiceAcknowledgement" + plantId + ".docx";
            strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }
            WordDocument document = new WordDocument(File, FormatType.Docx);

            try
            {
                string invoicePartyAddress = "";
                string vendorPartyAddress = "";
                WSection section = document.Sections[0];
                DataTable dsOrderMaster, dsServiceItems;
                dsOrderMaster = loadOrderServiceAcknowledgementMaster(SurviceAckId);//sql
                Dictionary<string, string> columns = new Dictionary<string, string>();
                var poApprovedStatus = "";
                document.Replace("{AddedBy}", dsOrderMaster.Rows[0]["AddedBy"].ToString(), false, false);
                var serviceTotal = 0.00;
                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);
                DataTable dsMaterialItems = ServiceAcknowledgementDetail(SurviceAckId);
                var dsInventoryReceiveAdditionalTax = loadInventoryReceiveAdditionalTax(SurviceAckId);
                var dsAdditionalServiceItems = loadAdditionalServiceAckMaster(SurviceAckId);
                //var InventoryReceiveAdditionalTax = 0.00;
                //if (dsInventoryReceiveAdditionalTax.Rows.Count > 0)

                //{
                //    InventoryReceiveAdditionalTax = makeInventoryReceiveAdditionalTaxTable(document, dsInventoryReceiveAdditionalTax, SurviceAckId);//Service Details 
                //}
                var materialTotal = MakeServiceAckDetailsTable(document, dsMaterialItems, SurviceAckId);//Material Details 
                                                                                                        //document.Replace("{GrandTotal}", (materialTotal).ToString("#,##0.00") + " " + dsMaterialItems.Rows[0]["CurrencyName"].ToString(), true, true);
                                                                                                        //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                var additionalserviceTotal = 0.00;
                if (dsAdditionalServiceItems.Rows.Count > 0)

                {
                    additionalserviceTotal = additionalServiceAckTable(document, dsAdditionalServiceItems, SurviceAckId);//Service Details 
                    //document.Replace("{ServiceAdditionalTax}", "Service Details", true, true);

                    //{TotalInWords}
                }
                document.Replace("{GrandTotal}", ((materialTotal + serviceTotal) + additionalserviceTotal).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{TotalInWords}", ru.InWord(((materialTotal + serviceTotal) + additionalserviceTotal), dsOrderMaster.Rows[0]["CurrencyId"].ToString()), true, true);
                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();
                //document.Replace("{TotalInWords}", ru.InWord((clsStaticInfo.dbl(materialTotal + serviceTotal)), dsMaterialItems.Rows[0]["CurrencyId"].ToString()), true, true);
                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                List<string> strReplace = new List<string>();
                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());

                StringCollection strColDistinct = new StringCollection();
                for (int i = 0; i < strReplace.Count; i++)
                {
                    if (strColDistinct.Contains(strReplace[i].ToUpper()))
                        continue;

                    strColDistinct.Add(strReplace[i].ToUpper());

                    string text = strReplace[i].ToUpper();
                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        ReplaceInfo[text] = document.Replace(text, dsOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }
                }
                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);

                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "", false, false);

                }

                //Region that is for Pdf.Document
                DocToPDFConverter converter = new DocToPDFConverter();

                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                converter.Dispose();
                document.Close();
                string Prefix = "Service Ack" + SurviceAckId;
                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                pdfDocument.Close(true);
                document.Close();

            }
            catch (Exception ex)
            {
                throw ex;

            }
            document.Close();
        }

        public DataTable loadOrderServiceAcknowledgementMaster(string SurviceAckId)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT                SACKM.Id ACKNumber
												 ,PO1.POId PONumber
                                                ,SACKM.CompanyGroupId
                                                ,SACKM.CompanyId
												,SACKM.BaseNoOfDays
                                                ,Plant.GSTIN
												,eI.EmployeeName
                                               ,REPLACE(Convert(VARCHAR(11), SACKM.AcknowledgementDate, 106), ' ', '-') AS AcknowledgementDate
                                                ,REPLACE(Convert(VARCHAR(11), SACKM.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
                                                ,REPLACE(Convert(VARCHAR(11), SACKM.MatureDate, 106), ' ', '-') AS MatureDate
												,REPLACE(Convert(VARCHAR(11), PO1.PODate, 106), ' ', '-') AS PODate
		                                        ,SACKM.InvoicingPartyPlantId
												,ACKStatus= CASE when SACKM.CheckedByStatus='ForChecked' Then 'To be checked'
																				when SACKM.CheckedByStatus='Hold' Then 'Hold'
																				when SACKM.CheckedByStatus='Reject' Then 'Reject'
																				when SACKM.CheckedByStatus='Checked' AND SACKM.ApprovedByStatus='To be Approval' Then 'Checked' 
																				when SACKM.CheckedByStatus='Checked'  AND SACKM.ApprovedByStatus='Reject' Then 'Reject For Approved'
																				when SACKM.CheckedByStatus='Checked' AND SACKM.ApprovedByStatus='Hold' Then 'Hold For Approved'
																				when SACKM.CheckedByStatus='Checked' and SACKM.ApprovedByStatus='For Approval' Then 'To be Approval'
																				when SACKM.CheckedByStatus='Checked' and SACKM.ApprovedByStatus='Approved' Then 'Approved'
																				when SACKM.CheckedByStatus Is null and SACKM.ApprovedByStatus Is null Then 'Approved'
																			else ''
																			END
		                                        ,INVPARTYPL.UserName InvoicingPartyName
                                                ,INVPARTYPL.AddressMasterId InvoicePartyAddressMasterId
                                                ,INVPARTYPL.GSTIN InvoicingPartyGSTIN
                                                ,ISNULL(SACKM.InvoicingByAddress,'') InvoicingByAddress
												,SACKM.DeliveryByAddress
		                                        ,DPARTYPL.UserName DeliveryParty
		                                        ,SACKM.DeliveryPartyPlantId		
		                                        ,POD.InventoryMaterialId MaterialMasterId
		                                        ,SACKM.DocRefNo
												 ,SACKM.PODepended
                                                ,REPLACE(Convert(VARCHAR(11), SACKM.DocDate, 106), ' ', '-') AS DocDate
		                                        --,SACKM.AddedBy
		                                        ,SACKM.AddedDate
		                                        ,SACKM.UpdatedBy
		                                        ,SACKM.UpdatedDate
		                                        ,SACKM.IsApproved 
												 ,SACKM.ServicePOId 
												 ,SACKM.IsPaymentHold 
		                                        ,SACKM.PartyType
												 ,SACKM.PreparedBy
												,SACKM.PartyId
												,SACKM.Status
												,Party.UserName VendorName
                                                ,Party.AddressMasterId VendorAddressMasterId
                                                ,Party.TINNO VendorGSTIN
		                                        ,Case When SACKM.IsNonCreditable = 1 then 'NonCreditable' when SACKM.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
		                                        ,SACKM.CurrencyId
	                                            ,CRNC.Code AS CurrencyName
	                                            ,SACKM.ToCurrencyRate
												 ,SACKM.IsTaxApplicable
		                                        ,BASECRNC.Code AS BaseCurrencyName
		                                        ,PayTerm.UserName PaymentTerm
	                                          ,MM.UserName MaterialMaster
	                                          ,MM.MaterialGroupMasterId
	                                          ,MGM.UserName MaterialGroupMaster
	                                          ,POD.ArticleId
	                                          ,MMA.StandardName Article
	                                          ,FC.Id FirstCharId
	                                          ,FC.UserName FirstChar
                                              ,POD.FirstCharacteristicsValueId
	                                          ,FCV.UserName AS FirstCharacteristicsValue
                                              ,POD.SecondCharacteristicsValueId
	                                          ,SCV.UserName AS SecondCharacteristicsValue
	                                          ,POD.ThirdCharacteristicsValueId
	                                          ,TCV.UserName AS ThirdCharacteristicsValue
	                                          ,SC.Id SecondCharId
	                                          ,SC.UserName SecondChar
	                                          ,TC.Id ThirdCharId
	                                          ,TC.UserName ThirdChar
	                                          ,ROUND(POD.TransactionQty, 2) POTransactionQty
	                                          ,ROUND(POD.TransactionRate, 2) TransactionRate
	                                          ,ROUND((POD.TransactionQty * POD.TransactionRate), 2) AS TrnAmount
	                                          ,POD.BaseAmount
	                                          ,POD.TotalTaxAmount AS BaseTaxAmount
	                                          ,TaxAmount = (
		                                            SELECT SUM(TaxAmount)
		                                            FROM [TRN].[PurchaseOrderTax]
		                                            WHERE InventoryReceiveDetailId = POD.Id
		                                            )
	                                          ,ServiceTaxAmount = (
		                                            SELECT SUM(TotalTaxAmount)
		                                            FROM [TRN].[POService]
		                                            WHERE InventoryReceiveId = POD.InventoryReceiveId
		                                            )
                                              ,POD.Description
	                                          ,POD.ChargesAmount
	                                          ,POD.CountryId
	                                          ,POCountry.UserName CountryOfOrigin
                                                ,POD.Id PurchaseOrderDetailId
	                                          ,POD.TransactionUoMId
	                                          ,TUoM.UserName AS TransactionUoM
                                               ,MRMD.MaterialDetail MaterialDetail

											   ,CheckedByName=CASE WHEN SACKM.CheckedByStatus='Checked' Then eI.EmployeeName else '' END 
                                                ,AuthorizedBy=CASE When SACKM.ApprovedByStatus='Approval'then eI1.EmployeeName else '' END
                                            
														,AddedBy=CASE When eI3.EmployeeName  Is null														   
																      Then U.UserId 
																      ELSE eI3.EmployeeName 													
																END 
                                   
									,PurOrCheckedStatus= CASE when SACKM.CheckedByStatus='For Checking' Then 'To be checked'
                                           when SACKM.CheckedByStatus='Hold' Then 'Hold'
						                   when SACKM.CheckedByStatus='Reject' Then 'Reject'
						                   when SACKM.CheckedByStatus='Checked' Then 'Checked'
						                  else ''
						   
							                END
			                           ,PurOrApprovedStatus= CASE 
						                   when SACKM.ApprovedByStatus='Reject' Then 'Reject For Approved'
						                   when SACKM.ApprovedByStatus='Hold' Then 'Hold For Approved'
						                   when SACKM.ApprovedByStatus='For Approval' Then 'To be Approval'
						                   when SACKM.ApprovedByStatus='Approval' Then 'Approved'
						                   else ''
							                END
                                              FROM [TRN].[ServiceAcknowledgementMaster] SACKM
											  LEFT JOIN [TRN].ServiceAcknowledgementDetail SACKMD ON SACKMD.ServiceAcknowledgementMasterId =SACKM.Id
                                         LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = SACKM.CompanyGroupId
                                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = SACKM.CompanyId
                                         LEFT JOIN ORG.Plant Plant ON Plant.Id = SACKM.PlantId
                                         LEFT JOIN SCS.Currency CRNC ON CRNC.Id = SACKM.CurrencyId
                                         LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = SACKM.BaseCurrencyId
                                         LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = SACKM.PaymentTermId
                                         LEFT JOIN HKP.PartyPlant  INVPARTYPL ON INVPARTYPL.Id = SACKM.InvoicingPartyPlantId
                                         LEFT JOIN HKP.PartyPlant  DPARTYPL ON DPARTYPL.Id = SACKM.DeliveryPartyPlantId                                          
                                         LEFT JOIN TRN.PurchaseOrderDetail POD ON SACKM.Id = POD.InventoryReceiveId
                                         LEFT JOIN SCS.Country POCountry ON POD.CountryId = POCountry.Id
										 LEFT JOIN HKP.Party Party ON Party.Id = SACKM.PartyId                                        
                                         LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = POD.InventoryMaterialId
                                         LEFT JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                                         LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = POD.ArticleId
                                         LEFT JOIN HKP.Characteristics AS FC ON POD.FirstCharacteristicsId = FC.Id
                                         LEFT JOIN HKP.Characteristics AS SC ON POD.SecondCharacteristicsId = SC.Id
                                         LEFT JOIN HKP.Characteristics AS TC ON POD.ThirdCharacteristicsId = TC.Id
                                         LEFT JOIN HKP.CharacteristicsValue AS FCV ON POD.FirstCharacteristicsValueId = FCV.Id
                                         LEFT JOIN HKP.CharacteristicsValue AS SCV ON POD.SecondCharacteristicsValueId = SCV.Id
                                         LEFT JOIN HKP.CharacteristicsValue AS TCV ON POD.ThirdCharacteristicsValueId = TCV.Id
                                         LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON POD.TransactionUoMId = TUoM.Id
			                             LEFT JOIN TRN.MaterialRequsitionDetails AS MRMD ON MRMD.Id=POD.RequisitionDetailId
										 LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=SACKM.CheckedBy
										 LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=SACKM.ApprovedBy
										 left join [SEC].[User] U on U.UserId=SACKM.AddedBy
                                         LEFT JOIN dbo.EmployeeInformation eI3 ON eI3.SystemId=U.EmployeeId
										 LEFT JOIN(
									select PDAMAP.ServiceAckId, REPLACE(Convert(VARCHAR(11), IR.PODate, 106), ' ', '-') AS PODate 
									,PoId=STUFF((select distinct ','+xpo.Id from
									[TRN].[ServicePOMaster] xpo
									INNER JOin [TRN].[ServivePOAcknowledgementMap] xPDAMAP on xpo.Id=xPDAMAP.ServicePoId
									where xPDAMAP.ServiceAckId=PDAMAP.ServiceAckId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

									from [TRN].[ServivePOAcknowledgementMap] PDAMAP
									LEFT JOIN [TRN].[ServicePOMaster] IR ON IR.Id = PDAMAP.ServicePoId
									--where PDAMAP.GRNId='2020463'
									group by PDAMAP.ServiceAckId, IR.podate

									)PO1 ON PO1.ServiceAckId = SACKMD.ServiceAcknowledgementMasterId
	                            WHERE SACKM.id='" + SurviceAckId + @"'";
                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        public DataTable loadInventoryReceiveAdditionalTax(string SurviceAckId)
        {
            string strSQL;

            try
            {
                strSQL = @"Select TxC.UserName Taxname  ,IRAT.ID ,IRAT.TaxCodeId TaxCode,IRAT.TaxAmount,IRAT.Percentage   
					    FROM [TRN].ServiceAcknowledgementAdditionalTax IRAT
						LEFT JOIN [TRN].[ServiceAcknowledgementMaster] IR ON IR.ID= IRAT.ServicePOAckMasterId
						LEFT JOIN [MST].[TaxCode] TxC ON TxC.Id= IRAT.TaxCodeId
                        where IR.Id = '" + SurviceAckId + "'";

                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }
        public DataTable loadAdditionalServiceAckMaster(string ServAckMasterId)
        {
            string strSQL;

            try
            {
                strSQL = @"SELECT IOS.Id ServiceId, SM.UserName  Service ,IOS.Amount,IOS.TotalTaxAmount,IOS.AddedBy,IOS.AddedDate,IOS.UpdatedBy,IOS.UpdatedDate 
                               FROM TRN.ServiceAcknowledgementMaster   IR
                            INNER join trn.ServiceAcknowledgementCharge IOS ON IOS.ServiceAcknowledgementMasterId = IR.Id
                            INNER JOIN HKP.ServiceMaster SM ON IOS.ServiceMasterId = SM.Id 
                            where IR.Id = '" + ServAckMasterId + "'";

                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }
        public double MakeServiceAckDetailsTable(WordDocument document, DataTable dsOrderMaster, string purchaseOrderId)
        {
            string replaceString = "{ServicePODetail}";
            ReportUtility ru = new ReportUtility();
            DataTable dsOrderItems, dsTax;
            dsTax = loadServiceAckTax(purchaseOrderId);
            int LasColumnIndex = 7;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));
            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    LasColumnIndex++;
                }
            }
            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);
            WTableRow TemplateRow = wTable.Rows[0].Clone();
            #region column headers
            document.EnsureMinimal();
            //wTable.Title = "Material Details";
            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;
            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("RowId");
            int colRowId = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Service Name");
            range.ApplyCharacterFormat(FontBold);
            int colSN = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("HSN No");
            range.ApplyCharacterFormat(FontBold);
            int colHSNNo = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;
            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UoM");
            range.ApplyCharacterFormat(FontBold);
            int colUoM = COL; COL++;
            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate");
            range.ApplyCharacterFormat(FontBold);
            int colRate = COL; COL++;





            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Currency");
            range.ApplyCharacterFormat(FontBold);
            int colCurrency = COL; /*COL++;*/

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Amount(TRN)");
            //range.ApplyCharacterFormat(FontBold);
            //int colATRN = COL;/* COL++;*/


            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
            //range.ApplyCharacterFormat(FontBold);
            //int colTotalAmount = COL;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
                wTable.Rows[ROW].Cells[colTotalTaxableAmount].Width = 60;
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        //two columns required for tax
                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                        range.ApplyCharacterFormat(FontBold);
                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
                range.ApplyCharacterFormat(FontBold);
                //int colTotalAmount = COL;
            }


            if (dv.Count > 0)
            {
                wTable.Rows.Add(TemplateRow);
                ROW++;
                WTableRow TROW = wTable.LastRow;
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
                        range.ApplyCharacterFormat(FontBold);
                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }
            #endregion column headers
            if (dv.Count > 0)
            {
                wTable.Rows.Add(TemplateRow);

                WTableRow TROW = wTable.LastRow;
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                for (int i = 0; i < dv.Count; i++)
                {

                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
                    range.ApplyCharacterFormat(FontBold);
                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                    range.ApplyCharacterFormat(FontBold);
                }
                //ROW++;
            }
            #endregion column headers
            double totalValue = 0;
            int startRow = ROW;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                ROW++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                }

                TROW.Cells[colRowId].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Id"].ToString());
                TROW.Cells[colSN].AddParagraph().AppendText(dsOrderMaster.Rows[i]["ServiceMasterName"].ToString());
                TROW.Cells[colHSNNo].AddParagraph().AppendText(dsOrderMaster.Rows[i]["HSNNo"].ToString());
                TROW.Cells[colQty].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["Qty"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colUoM].AddParagraph().AppendText(dsOrderMaster.Rows[i]["UoM"].ToString());
                TROW.Cells[colRate].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["Rate"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colCurrency].AddParagraph().AppendText(dsOrderMaster.Rows[i]["CurrencyName"].ToString());
                // TROW.Cells[colATRN].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["TotalServicAmount"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["TotalAmount"].ToString()).ToString("#,##0.00"));

                if (dv.Count > 0)
                {
                    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
                    for (int T = 0; T < dv.Count; T++)
                    {
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND PurchaseOrderDetailId='" + dsOrderMaster.Rows[i]["Id"].ToString() + "'";
                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("#,##0.00"));
                        }
                    }
                }
            }

            ROW++;
            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);

            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (C == colRowId || C == colSN || C == colHSNNo || C == colCurrency || C == colQty || C == colRate || C == colUoM || dicTaxes.ContainsValue(C))
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);

            }
            #endregion Total


            ROW++;
            #region Sub Total
            double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(TotalAmount)", "").ToString())
                //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                + clsStdLib.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString())
                ;

            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2") + " (" + ru.InWord(total, dsOrderMaster.Rows[0]["CurrencyId"].ToString()) + ")");

            #endregion Total

            ROW++;
            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            //myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle");
                    }
                }
            }

            IWParagraphStyle myStyleRightAlign = document.AddParagraphStyle("MyStyleRightAlign");
            //Sets the formatting of the style
            myStyleRightAlign.CharacterFormat.FontSize = 8f;
            myStyleRightAlign.CharacterFormat.TextColor = Color.Black;
            myStyleRightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;



            for (int R = 1; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];



                //foreach (WParagraph item in TROW.Cells[colATRN].Paragraphs)
                //{
                //	item.ApplyStyle("MyStyleRightAlign");
                //}


                foreach (WParagraph item in TROW.Cells[colTotalTaxableAmount].Paragraphs)
                {
                    item.ApplyStyle("MyStyleRightAlign");
                }

            }

            #endregion paragrpath formats

            #region Column marge 


            //tax codes merging (horizontal)
            ROW = 0;
            for (int i = 0; i < dv.Count; i++)
                wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            WTableRow TROWe = wTable.LastRow;
            for (int i = 0; i <= colTotalTaxableAmount; i++)
            {
                TROWe.Cells[i].Width = wTable.Rows[0].Cells[i].Width;
                wTable.ApplyVerticalMerge(i, ROW - 1, ROW);
            }

            #endregion Column marge 
            //primary cells merging (veritcal)
            ROW++;
            //for (int i = 0; i <= colTotalTaxableAmount; i++)
            //	wTable.ApplyVerticalMerge(i, ROW - 1, ROW);

            IWParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
            style.CharacterFormat.Bold = true;
            style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section



            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);
            return total;
        }
        public DataTable loadAdditionalServiceMasterTax(string serviceAckId)
        {
            string strSQL;
            try
            {
                strSQL = @"select ISER.Id InventoryServiceId,IR.Id PurchaseOrderId,tg.Code AS TaxCode,IRT.Percentage, IRT.TaxAmount
							from TRN.ServiceAcknowledgementMaster IR
                              INNER JOIN trn.ServiceAcknowledgementCharge ISER ON ISER.ServiceAcknowledgementMasterId = IR.Id
                              INNER join trn.ServicePOAckTax IRT ON IRT.ServiceAcknowledgementMasterId = IR.Id and IRT.ServiceAcknowledgementChargeId = ISER.Id
                               LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=IRT.TaxCategoryId
                                WHERE IR.Id='" + serviceAckId + @"'
								and ServiceAcknowledgementChargeId  is not null and ServiceAcknowledgementDetailId is null
								 ORDER BY tg.[Sequence]";
                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }
        public double additionalServiceAckTable(WordDocument document, DataTable dsOrderMaster, string grnId)
        {
            string replaceString = "{additionalserviceTotal}";

            ReportUtility ru = new ReportUtility();

            DataTable dsTax;
            //clsDataContext data = new clsDataContext();

            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 8f;
            rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;


            dsTax = loadAdditionalServiceMasterTax(grnId);

            int LasColumnIndex = 1;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));

            //LasColumnIndex++;
            //dicTaxes.Add("totaltax", LasColumnIndex);
            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    LasColumnIndex++;
                }
            }

            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();


            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;
            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Service Name");
            range.ApplyCharacterFormat(FontBold);
            //wTable.Rows[ROW].Cells[COL].Width = 100;
            int colServiceName = COL; //COL++;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    //two columns required for tax
                    COL++;
                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                    range.ApplyCharacterFormat(FontBold);
                    COL++;
                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                }
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
            }

            wTable.Rows.Add(TemplateRow);
            ROW++;

            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {

                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
                    range.ApplyCharacterFormat(FontBold);
                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                    range.ApplyCharacterFormat(FontBold);

                }
            }

            #endregion column headers
            double totalValue = 0;
            int startRow = ROW + 1;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                ROW++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                }
                IParagraphItem p = TROW.Cells[colServiceName].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Service"].ToString());

                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["Amount"].ToString()).ToString("#,##0.00"));

                totalValue += clsStdLib.dbl(dsOrderMaster.Rows[i]["Amount"].ToString());

                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(totalValue.ToString("F2"));

                if (dv.Count > 0)
                {
                    //dsTax.Tables[0].DefaultView.RowFilter = "MasterOrderItemId='" + dsOrderItems.Tables[0].Rows[i]["MasterOrderItemId"].ToString() + "'";
                    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
                    //double totalTax = 0;

                    for (int T = 0; T < dv.Count; T++)
                    {
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND InventoryServiceId='" + dsOrderMaster.Rows[i]["ServiceId"] + "'";
                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("F2"));
                        }
                    }
                }
            }

            ROW++;

            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            //wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;

            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);

            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (dicTaxes.ContainsValue(C))
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("F2"));
            }


            #endregion Total


            ROW++;


            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(Amount)", "").ToString())
//- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
+ clsStdLib.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());



            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2") + " (" + ru.InWord(total, dsOrderMaster.Rows[0]["CurrencyId"].ToString()) + ")");

            #endregion Total


            ROW++;

            #region Total Payable
            //int TotalPayableRow = ROW;
            //int TotalPayableColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[TotalPayableColumn].AddParagraph().AppendText("Total Amount Payable");
            //_TROW.Cells[TotalPayableColumn + 1].AddParagraph().AppendText("Need To Discuss");

            #endregion Total Payable


            ROW++;

            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle2 = document.AddParagraphStyle("MyStyle2");
            //Sets the formatting of the style
            myStyle2.CharacterFormat.FontSize = 8f;
            myStyle2.CharacterFormat.TextColor = Color.Black;
            myStyle2.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 35;
                if (dv.Count < 3)
                    TROW.Cells[0].Width = +((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle2");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            for (int i = 0; i < dv.Count; i++)
                wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            for (int i = 0; i <= colTotalTaxableAmount; i++)
                wTable.ApplyVerticalMerge(i, ROW - 1, ROW);

            IWParagraphStyle style2 = document.AddParagraphStyle("SubTotalStyle2");
            style2.CharacterFormat.Bold = true;
            style2.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;


            //Adds new paragraph to the section


            //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
            //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
            //        PARA.ApplyStyle("SubTotalStyle2");

            //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
            #endregion merging section



            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);
            return total;
        }

        public double makeInventoryReceiveAdditionalTaxTable(WordDocument document, DataTable dsOrderMaster, string SurviceAckId)
        {
            string replaceString = "{InventoryReceiveAdditionalTax}";

            ReportUtility ru = new ReportUtility();

            DataTable dsTax;
            //clsDataContext data = new clsDataContext();

            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign1");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 8f;
            rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;


            dsTax = loadInventoryReceiveAdditionalTax(SurviceAckId);

            int LasColumnIndex = 1;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));

            //LasColumnIndex++;
            //dicTaxes.Add("totaltax", LasColumnIndex);
            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    //LasColumnIndex++;
                }
            }

            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();


            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;
            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxname");
            range.ApplyCharacterFormat(FontBold);
            int colTaxname = COL; COL++;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Percentage");
            range.ApplyCharacterFormat(FontBold);
            int colPercentage = COL;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Tax Amount");
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                //for (int i = 0; i < dv.Count; i++)
                //{
                //	//two columns required for tax
                //	COL++;
                //	range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                //	range.ApplyCharacterFormat(FontBold);
                //	COL++;
                //	range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                //}
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
            }

            wTable.Rows.Add(TemplateRow);
            ROW++;

            //if (dv.Count > 0)
            //{
            //	for (int i = 0; i < dv.Count; i++)
            //	{

            //		range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
            //		range.ApplyCharacterFormat(FontBold);
            //		range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
            //		range.ApplyCharacterFormat(FontBold);

            //	}
            //}

            #endregion column headers
            double totalValue = 0;
            int startRow = ROW + 1;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                //ROW++;
                //wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                //for (int CE = 0; CE < TROW.Cells.Count; CE++)
                //{
                //	foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                //	{
                //		item.Text = "";
                //	}
                //}
                IParagraphItem p = TROW.Cells[colTaxname].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Taxname"].ToString());
                TROW.Cells[colPercentage].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["Percentage"].ToString()).ToString("#,##0.0000"));

                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["TaxAmount"].ToString()).ToString("#,##0.00"));
            }

            #region Sub Total


            double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(TaxAmount)", "").ToString());

            #endregion Total


            //ROW++;

            #region Total Payable
            #endregion Total Payable

            //ROW++;

            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle3 = document.AddParagraphStyle("MyStyle3");
            //Sets the formatting of the style
            myStyle3.CharacterFormat.FontSize = 8f;
            myStyle3.CharacterFormat.TextColor = Color.Black;
            myStyle3.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 35;
                if (dv.Count < 3)
                    TROW.Cells[0].Width = +((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle3");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            #endregion merging section

            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            int k = document.Replace(replaceString, textBodyPart, false, false);
            return total;
        }
        public DataTable loadServiceAckTax(string purchaseOrderId)
        {
            string strSQL;
            try
            {
                strSQL = @"	select PO.Id PurchaseOrderId,POD.Id PurchaseOrderDetailId,tg.Code AS TaxCode,SPAT.Percentage, SPAT.TaxAmount 
							from TRN.ServiceAcknowledgementMaster PO
                            INNER JOIN TRN.ServiceAcknowledgementDetail POD ON POD.ServiceAcknowledgementMasterId = PO.Id
                            Inner join TRN.ServicePOAckTax SPAT ON SPAT.ServiceAcknowledgementMasterId = PO.Id and SPAT.ServiceAcknowledgementDetailId = POD.Id
                            LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=SPAT.TaxCategoryId
                            WHERE PO.Id='" + purchaseOrderId + @"' 
							and ServiceAcknowledgementDetailId  is not null  
							ORDER BY tg.[Sequence] ";
                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }
        private DataTable ServiceAcknowledgementDetail(string SurviceAckId)
        {
            try
            {
                string sqlText = @"--DECLARE @inventoryReceiveId VARCHAR(10)='" + SurviceAckId + @"'
	           
                      SELECT SACKD. [Id] 
                      ,SPOD.[ServicePOMasterId]
                      ,SPOD.[ServiceMasterId]
					 ,HSNC.Code HSNNo
	                  ,SM.StandardName ServiceMasterName
                      ,SPOD.[Amount] 
                    ,ROUND(SACKD.[Amount] , 2) TotalServicAmount
                    ,ROUND(SPTax.TaxAmount , 2) TotalTaxAmount
                      ,SPOD.[AddedBy]
                      ,SPOD.[AddedDate]
                      ,SPOD.[AddedFromIP]
                      ,SPOD.[UpdatedBy]
                      ,SPOD.[UpdatedDate]
                      ,SPOD.[UpdatedFromIP]
	                  ,CR.Code CurrencyName
                      ,SACKM.CurrencyId
                      ,SACKD.TotalAmount,SACKD.Qty,SACKD.Rate,UOM.UserName UoM
                  FROM [TRN].ServiceAcknowledgementDetail SACKD
                  left JOIN [TRN].ServiceAcknowledgementMaster  AS SACKM ON SACKM.Id=SACKD.ServiceAcknowledgementMasterId
                  left JOIN [SCS].[Currency] AS CR ON CR .Id=SACKM.CurrencyId
                  left JOIN [HKP].[ServiceMaster]  AS SM ON SM.Id=SACKD.ServiceMasterId
				  LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=SM.HSNCodeId
				  left JOIN [TRN].[ServicePODetail]  AS SPOD ON SM.Id=SACKD.ServicePODetailId
                  left join TRN.ServicePOTax AS SPTax ON SACKM.Id=SPTax.ServicePOMasterId
                   left JOIN [SCS].[UnitOfMeasurement] UOM ON UOM.Id=SACKD.TransactionUoMId
                  Where SACKM.Id='" + SurviceAckId + @"'";

                return _sqlRepository.GetDataTable(sqlText);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        //#endregion
        #region service ack
        public void InsertServiceAck(ServiceAcknowledgementMaster entity, IEnumerable<ServiceAcknowledgementViewModel> DetailList, IEnumerable<ServicePOAckTax> ServicePOAndAckTax)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var plantId = _ServicePOMaster.SqlQuery<string>($"SELECT FilePrefix from org.plant WHERE Id ='{identity.PlantId}'").FirstOrDefault();
                if (plantId == null)
                {
                    throw new CustomException("No Prefix Available for this Plant");
                }

                if (DetailList != null)
                {
                    foreach (var item in DetailList)
                    {
                        if (!item.check)
                        {
                            throw new CustomException("Please Select a Line Item !");
                        }
                        else if (item.check.ToString() == "0")
                        {
                            throw new CustomException("Please Check a check box !");
                        }

                    }
                }
                else
                {
                    throw new CustomException("Please Select atlest one Line item !");
                }
                if (string.IsNullOrEmpty(entity.Id))
                {
                    var year1 = DateTime.Now.Year.ToString();
                    var id = GetPKServiveAck();
                    entity.Id = plantId + id;
                    AuditService.AddedLog(entity);
                    entity.ModelState = ModelState.Added;
                    _ServiceAcknowledgementMaster.Insert(entity);
                    if (DetailList.IsNotNull())
                    {
                        var currentId = _ServiceAcknowledgementDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[ServiceAcknowledgementDetail] WHERE ServiceAcknowledgementMasterId='{entity.Id}'").First();

                        // Insert in receive detail
                        if (!string.IsNullOrEmpty(entity.Id))
                        {

                            foreach (var itemDetail in DetailList)
                            {
                                var grndId = "";
                                var NewId = entity.Id + "-";
                                currentId++;
                                var receiveDetail = new ServiceAcknowledgementDetail
                                {
                                    Id = NewId + currentId,
                                    ServiceAcknowledgementMasterId = entity.Id,
                                    ServiceMasterId = itemDetail.ServiceMasterId,
                                    ServicePOMasterId = itemDetail.ServicePOMasterId,
                                    ServicePODetailId = itemDetail.ServicePODetailId,
                                    Amount = Math.Round(itemDetail.Amount, 2),
                                    TotalTaxAmount = Math.Round(itemDetail.TotalTaxAmount, 2),
                                    TotalAmount = Math.Round(itemDetail.TotalAmount, 2),
                                    Qty = itemDetail.CurrentQty,
                                    Rate = Math.Round(itemDetail.Rate, 4),
                                    TransactionUoMId = itemDetail.TransactionUoMId
                                };
                                AuditService.AddedLog(receiveDetail);
                                //InsertGraph(receiveDetail);
                                //UpdateInventoryDetail(receiveDetail, ratio, Convert.ToDecimal(entity.ToCurrencyRate), entity.IsNonCreditable);
                                _ServiceAcknowledgementDetail.Insert(receiveDetail);
                                var receiveDetail1 = new ServivePOAcknowledgementMap
                                {

                                    Id = plantId + GetPKSerAckMap(),
                                    CompanyGroupId = identity.CompanyGroupId,
                                    CompanyId = identity.CompanyId,
                                    PlantId = identity.PlantId,
                                    ServiceAckId = entity.Id,
                                    ServicePoId = itemDetail.ServicePOMasterId,
                                    ServicePoDetailId = itemDetail.ServicePODetailId,
                                    Qty = itemDetail.CurrentQty
                                };

                                AuditService.AddedLog(receiveDetail1);
                                _ServivePOAcknowledgementMapRepository.Insert(receiveDetail1);

                                if (ServicePOAndAckTax.IsNotNull())
                                {
                                    var currentIdTax = _servicePOAckTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[ServicePOAckTax] WHERE ServiceAcknowledgementDetailId='{receiveDetail.Id}'").First();
                                    foreach (var item in ServicePOAndAckTax.Where(r => r.ServicePoDetailId == receiveDetail1.ServicePoDetailId))
                                    {
                                        var potax = new ServicePOAckTax();
                                        currentIdTax++;
                                        potax.Id = MakePK(receiveDetail.Id, currentIdTax, 2);
                                        //potax.Id = GetPK3();
                                        potax.ServiceAcknowledgementMasterId = entity.Id;
                                        potax.ServiceAcknowledgementDetailId = receiveDetail.Id;
                                        potax.TaxCategoryId = item.TaxCategoryId;
                                        potax.HSNCodeId = item.HSNCodeId;
                                        potax.Percentage = item.Percentage;
                                        potax.TaxAmount = Math.Round(item.TaxAmount, 2);
                                        potax.ModelState = ModelState.Added;
                                        AuditService.AddedLog(potax);
                                        _servicePOAckTaxRepository.Insert(potax);
                                    }
                                }
                                receiveDetail.TotalTaxAmount = ServicePOAndAckTax.Where(r => r.ServicePoDetailId == receiveDetail.ServicePODetailId).Sum(r => r.TaxAmount);
                            }
                        }
                    }
                }
                else//Update
                {
                    AuditService.UpdatedLog(entity);
                    _ServiceAcknowledgementMaster.Update(entity);
                    if (DetailList.IsNotNull())
                    {
                        var currentId = _ServiceAcknowledgementDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[ServiceAcknowledgementDetail] WHERE ServiceAcknowledgementMasterId='{entity.Id}'").First();

                        // Insert in receive detail
                        if (!string.IsNullOrEmpty(entity.Id))
                        {

                            foreach (var itemDetail in DetailList)
                            {
                                var grndId = "";
                                var NewId = entity.Id + "-";
                                currentId++;
                                var receiveDetail = new ServiceAcknowledgementDetail
                                {
                                    Id = itemDetail.ServicePODetailId,//NewId + currentId,
                                    ServiceAcknowledgementMasterId = entity.Id,
                                    ServiceMasterId = itemDetail.ServiceMasterId,
                                    ServicePOMasterId = itemDetail.ServicePOMasterId,
                                    ServicePODetailId = itemDetail.ServicePoDelId,
                                    Amount = Math.Round(itemDetail.Amount, 2),
                                    TotalTaxAmount = Math.Round(itemDetail.TotalTaxAmount, 2),
                                    TotalAmount = Math.Round(itemDetail.TotalAmount, 2),
                                    Qty = itemDetail.CurrentQty,
                                    Rate = Math.Round(itemDetail.Rate, 4),
                                    TransactionUoMId = itemDetail.TransactionUoMId
                                };
                                AuditService.UpdatedLog(receiveDetail);
                                //InsertGraph(receiveDetail);
                                //UpdateInventoryDetail(receiveDetail, ratio, Convert.ToDecimal(entity.ToCurrencyRate), entity.IsNonCreditable);
                                _ServiceAcknowledgementDetail.Update(receiveDetail);
                                //var ServivePOAcknowledgementMap
                                var receiveDetail1 = new ServivePOAcknowledgementMap
                                {

                                    Id = itemDetail.MapId, //GetPK2(),
                                    CompanyGroupId = identity.CompanyGroupId,
                                    CompanyId = identity.CompanyId,
                                    PlantId = identity.PlantId,
                                    ServiceAckId = entity.Id,
                                    ServicePoId = itemDetail.ServicePOMasterId,
                                    ServicePoDetailId = itemDetail.ServicePoDelId,
                                    Qty = itemDetail.CurrentQty


                                };

                                AuditService.UpdatedLog(receiveDetail1);
                                _ServivePOAcknowledgementMapRepository.Update(receiveDetail1);

                                if (ServicePOAndAckTax.IsNotNull())
                                {
                                    var currentIdTax = _servicePOAckTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[ServicePOAckTax] WHERE ServiceAcknowledgementDetailId='{receiveDetail.Id}'").First();
                                    foreach (var item in ServicePOAndAckTax.Where(r => r.ServiceAcknowledgementDetailId == receiveDetail.Id))
                                    {
                                        var potax = new ServicePOAckTax();
                                        potax.Id = item.Id; //MakePK(receiveDetail.Id, currentIdTax, 2);
                                        //potax.Id = GetPK3();
                                        potax.ServiceAcknowledgementMasterId = item.ServiceAcknowledgementMasterId;
                                        potax.ServiceAcknowledgementDetailId = item.ServiceAcknowledgementDetailId;
                                        potax.TaxCategoryId = item.TaxCategoryId;
                                        potax.HSNCodeId = item.HSNCodeId;
                                        potax.Percentage = item.Percentage;
                                        potax.TaxAmount = Math.Round(item.TaxAmount, 2);
                                        potax.ModelState = ModelState.Added;
                                        AuditService.UpdatedLog(potax);
                                        _servicePOAckTaxRepository.Update(potax);
                                    }
                                }
                                receiveDetail.TotalTaxAmount = ServicePOAndAckTax.Where(r => r.ServiceAcknowledgementDetailId == receiveDetail.Id).Sum(r => r.TaxAmount);
                            }
                        }
                    }
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public void InsertIndependentServiceAck(string ServiceAckId, ServiceAcknowledgementViewModel ackDetailModel, IEnumerable<ServicePOAckTax> ServicePOAndAckTax)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                var currentId = _ServiceAcknowledgementDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[ServiceAcknowledgementDetail] WHERE ServiceAcknowledgementMasterId='{ServiceAckId}'").First();

                var NewId = ServiceAckId + "-";
                currentId++;
                var receiveDetail = new ServiceAcknowledgementDetail
                {
                    Id = NewId + currentId,
                    ServiceAcknowledgementMasterId = ServiceAckId,
                    ServiceMasterId = ackDetailModel.ServiceMasterId,
                    ServicePOMasterId = ackDetailModel.ServicePOMasterId,
                    ServicePODetailId = ackDetailModel.ServicePODetailId,
                    Amount = Math.Round(ackDetailModel.Amount, 2),
                    TotalTaxAmount = Math.Round(ackDetailModel.TotalTaxAmount, 2),
                    TotalAmount = Math.Round(ackDetailModel.TotalAmount, 2),
                    Qty = ackDetailModel.Qty,
                    Rate = Math.Round(ackDetailModel.Rate, 4),
                    TransactionUoMId = ackDetailModel.TransactionUoMId
                };
                AuditService.AddedLog(receiveDetail);
                _ServiceAcknowledgementDetail.Insert(receiveDetail);

                if (ServicePOAndAckTax.IsNotNull())
                {
                    var currentIdTax = _servicePOAckTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[ServicePOAckTax] WHERE ServiceAcknowledgementDetailId='{receiveDetail.Id}'").First();
                    foreach (var item in ServicePOAndAckTax)
                    {
                        var potax = new ServicePOAckTax();
                        currentIdTax++;
                        potax.Id = MakePK(receiveDetail.Id, currentIdTax, 2);
                        //potax.Id = GetPK3();
                        potax.ServiceAcknowledgementMasterId = ServiceAckId;
                        potax.ServiceAcknowledgementDetailId = receiveDetail.Id;
                        potax.TaxCategoryId = item.TaxCategoryId;
                        potax.HSNCodeId = item.HSNCodeId;
                        potax.Percentage = item.Percentage;
                        potax.TaxAmount = Math.Round(item.TaxAmount, 2);
                        potax.ModelState = ModelState.Added;
                        AuditService.AddedLog(potax);
                        _servicePOAckTaxRepository.Insert(potax);
                    }
                }
                receiveDetail.TotalTaxAmount = ServicePOAndAckTax.Where(r => r.ServicePoDetailId == receiveDetail.ServicePODetailId).Sum(r => r.TaxAmount);

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public void InsertIndependentServiceAck(ServiceAcknowledgementMaster entity)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var plantId = _ServicePOMaster.SqlQuery<string>($"SELECT FilePrefix from org.plant WHERE Id ='{identity.PlantId}'").FirstOrDefault();
                if (plantId == null)
                {
                    throw new CustomException("No Prefix Available for this Plant");
                }


                if (string.IsNullOrEmpty(entity.Id))
                {
                    var year1 = DateTime.Now.Year.ToString();
                    var id = GetPKServiveAck();
                    entity.Id = plantId + id;
                    entity.ServiceType = GRNType.ServiceACK.ToString();
                    AuditService.AddedLog(entity);
                    entity.ModelState = ModelState.Added;
                    _ServiceAcknowledgementMaster.Insert(entity);

                }
                else//Update
                {
                    AuditService.UpdatedLog(entity);
                    _ServiceAcknowledgementMaster.Update(entity);
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public void InsertGraphCharge(InventoryMaterialViewModel entity, IEnumerable<ServicePOAckTax> taxCategoryList)
        {
            if (Convert.ToBoolean(_ChargeServiceRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT * FROM TRN.ServiceAcknowledgementCharge WHERE ServiceAcknowledgementMasterId='" + entity.ServiceAcknowledgementMasterId + "' AND ServiceMasterId='" + entity.ServiceMasterId + "') AS A) SELECT 1 ELSE SELECT 0 RETURN").First()))
                throw new CustomException("This service already taken."); ;
            //int currentId=0;
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;

                if (entity.IsNotNull())
                {
                    entity.ToCurrencyRate = entity.ToCurrencyRate == 0 ? 1 : entity.ToCurrencyRate;
                    var currentId = _ChargeServiceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[ServiceAcknowledgementCharge] WHERE ServiceAcknowledgementMasterId='{entity.ServiceAcknowledgementMasterId}'").First();
                    currentId++;
                    var service = new ServiceAcknowledgementCharge
                    {
                        Id = MakePK(entity.InventoryReceiveId + 2, currentId, 2),
                        ServiceAcknowledgementMasterId = entity.ServiceAcknowledgementMasterId,
                        ServiceMasterId = entity.ServiceMasterId,
                        Amount = Convert.ToDecimal(entity.TransactionAmount),
                        TotalTaxAmount = Convert.ToDecimal(entity.TotalTaxAmount)
                    };
                    AuditService.AddedLog(service);
                    _ChargeServiceRepository.Insert(service);
                    if (taxCategoryList.IsNotNull())
                    {
                        var crrId = _ChargeServiceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[ServicePOAckTax] WHERE ServiceAcknowledgementChargeId='{service.Id}'").First();
                        foreach (var item in taxCategoryList)
                        {
                            crrId++;
                            item.Id = MakePK(service.Id, crrId, 2);
                            item.ServiceAcknowledgementMasterId = entity.ServiceAcknowledgementMasterId;
                            item.ServiceAcknowledgementDetailId = null;
                            item.ServiceAcknowledgementChargeId = service.Id;
                            AuditService.AddedLog(item);
                            _servicePOAckTaxRepository.Insert(item);
                        }
                    }
                    //if (entity.CurrencyId != entity.BaseCurrencyId)
                    //    UpdateInventoryDetail(service, ratioServiceTax, ratio, Convert.ToDecimal(entity.ToCurrencyRate), entity.IsNonCreditable);
                    //else if (entity.CurrencyId == entity.BaseCurrencyId)
                    //    UpdateInventoryDetail(service, ratioServiceTax, ratio, 1, entity.IsNonCreditable);
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                 ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }
        public void UpdateGraphCharge(InventoryMaterialViewModel entity, List<ServicePOAckTax> taxCategoryList)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster, dsChild;

                string DetailsId = string.Empty; string Id = string.Empty;
                string sql = "SELECT * FROM [TRN].[ServiceAcknowledgementCharge] WHERE ServiceAcknowledgementMasterId='" + entity.ServiceAcknowledgementMasterId + "' ";
                string sql1 = "SELECT * FROM [TRN].[ServiceAcknowledgementCharge] WHERE ServiceAcknowledgementMasterId='" + entity.ServiceAcknowledgementMasterId + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                objCon.OpenDataSetThroughAdapter(sql1, out dsChild, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 1)
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    Id = dr["Id"].ToString();

                    dr["Amount"] = entity.TransactionAmount;
                    dr["TotalTaxAmount"] = entity.TotalTaxAmount;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }

                for (int i = 0; i < taxCategoryList.Count; i++)
                {
                    DataRow dr = dsChild.Tables[0].NewRow();

                    dsChild.Tables[0].Rows.Add(dr);
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsChild);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public void DeleteServiceAck(string id)
        {
            try
            {
                //var detail = Convert.ToBoolean(_inventoryReceiveRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM TRN.InventoryReceiveDetail WHERE InventoryReceiveId='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                var service = Convert.ToBoolean(_ServiceAcknowledgementDetailRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM TRN.ServiceAcknowledgementDetail WHERE ServiceAcknowledgementMasterId='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                if (!service)
                {
                    var data = _ServiceAcknowledgementMasterRepository.Find(id);
                    if (data.IsNull())
                        throw new CustomException(ServiceResources.RecordNoLonger);
                    //_ServiceAcknowledgementMasterRepository.Delete(data);
                    var sql = @"delete from trn.ServivePOAcknowledgementMap where ServiceAckId='" + id + @"'";
                    _sqlRepository.GetDataCollection(sql);
                    var sql1 = @"delete from trn.ServiceAcknowledgementMaster where id='" + id + @"'";
                    _sqlRepository.GetDataCollection(sql1);
                }
                else throw new CustomException("Please delete first line item.");
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        #endregion
        public IEnumerable<object> GetCheckedByAndApprovedBY(string CheckedBy, string ApprovedBy)
        {
            var sql = "";
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (CheckedBy == "true" && ApprovedBy == "true")
                {
                    sql = @"select E.SystemId As Value, (E.Employeecode+'-'+ E.EmployeeName) As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='PurchaseOrderCheckedBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                }
                else if (CheckedBy == "false" && ApprovedBy == "true")
                {
                    sql = @"select E.SystemId As Value, (E.Employeecode+'-'+ E.EmployeeName) As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='PurchaseOrderApproveBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                }
                else if (CheckedBy == "false" && ApprovedBy == "false")
                {

                }
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetCheckedByAndApprovedBYOutSource(string CheckedBy, string ApprovedBy)
        {
            var sql = "";
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (CheckedBy == "true" && ApprovedBy == "true")
                {
                    sql = @"select E.SystemId As Value, (E.Employeecode+'-'+ E.EmployeeName) As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='OutSourceCheckedBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                    return _sqlRepository.GetDataCollection(sql);
                }
                else if (CheckedBy == "false" && ApprovedBy == "true")
                {
                    sql = @"select E.SystemId As Value, (E.Employeecode+'-'+ E.EmployeeName) As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          --where  A.ActionStatus='OutSourceApprovedBy'
                            where  A.ActionStatus='OutSourceApproveBy' AND E.EmployeeStatus='Active' ";//A.PlantId='" + identity.PlantId + "' AND
                    return _sqlRepository.GetDataCollection(sql);
                }
                else
                    return null;

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        #region Notification Seting code for Service PO Requisition

        public IEnumerable<object> GetCheckedByAndApprovedBYServicePORequisition(string CheckedBy, string ApprovedBy)
        {

            var sql = "";
            try
            {
                //var DailySendMailRequisition = _notificationSetting.SqlQuery<bool>(@"Select NotificationAfterCreation  from NotificationSetting Where BusinessFlow = 'MaterialRequistion'").FirstOrDefault();
                //if (DailySendMailRequisition == true)
                //{
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (CheckedBy == "true" && ApprovedBy == "true")
                {
                    sql = @"select E.SystemId As Value, (E.Employeecode+'-'+ E.EmployeeName) As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='ServicePOCheckedBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                }
                else if (CheckedBy == "false" && ApprovedBy == "true")
                {
                    sql = @"select E.SystemId As Value, (E.Employeecode+'-'+ E.EmployeeName) As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='ServicePOApproveBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                }
                else if (CheckedBy == "false" && ApprovedBy == "false")
                {

                }
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        #endregion Notification Seting code for Service Requisitione
        #region Notification Seting code for service po acknowledgement

        public IEnumerable<object> GetCheckedByAndApprovedBYServicePOAcknowledgement(string CheckedBy, string ApprovedBy)
        {

            var sql = "";
            try
            {
                //var DailySendMailRequisition = _notificationSetting.SqlQuery<bool>(@"Select NotificationAfterCreation  from NotificationSetting Where BusinessFlow = 'MaterialRequistion'").FirstOrDefault();
                //if (DailySendMailRequisition == true)
                //{
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (CheckedBy == "true" && ApprovedBy == "true")
                {
                    sql = @"select E.SystemId As Value, (E.Employeecode+'-'+ E.EmployeeName) As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='ServiceAcknowledgementCheckedBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                }
                else if (CheckedBy == "false" && ApprovedBy == "true")
                {
                    sql = @"select E.SystemId As Value, (E.Employeecode+'-'+ E.EmployeeName) As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='ServiceAcknowledgementApproveBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                }
                else if (CheckedBy == "false" && ApprovedBy == "false")
                {

                }
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        #endregion Notification Seting code for Service Requisitione

        public Dictionary<string, object> GetPOFile(string id)
        {
            try
            {
                var sql = @"Select Id, FileName From [TRN].[ExpenseBooking]  Where Id='" + id + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

       

        public void InsertPODocMap(PODocumentMap entity, string POId, out string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string sql = "SELECT * FROM [TRN].[PODocumentMap] WHERE Id='" + entity.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = entity.POId + "-" + PODocumentMap();
                    var createdId = dr["Id"];
                    dr["POId"] = entity.POId;

                    dr["CompanyGroupId"] = entity.CompanyGroupId;

                    dr["UserFilename"] = entity.UserFilename;
                    dr["SystemFileName"] = createdId + Path.GetExtension(entity.UserFilename);
                    dr["Description"] = entity.Description;
                    dr["Remarks"] = entity.Remarks;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;


                    dsMaster.Tables[0].Rows.Add(dr);
                }
               

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();
            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }
        public void InsertServicePODocMap(ServicePODocumentMap entity, string POId, out string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string sql = "SELECT * FROM [TRN].ServicePODocumentMap WHERE Id='" + entity.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = entity.POId + "-" + ServicePODocumentMap();
                    var createdId = dr["Id"];
                    dr["ServicePOMasterId"] = entity.POId;

                    dr["CompanyGroupId"] = entity.CompanyGroupId;

                    dr["UserFilename"] = entity.UserFilename;
                    dr["SystemFileName"] = createdId + Path.GetExtension(entity.UserFilename);
                    dr["Description"] = entity.Description;
                    dr["Remarks"] = entity.Remarks;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;


                    dsMaster.Tables[0].Rows.Add(dr);
                }
              

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();
            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }

        public void InsertServicePOAckDocMap(ServicePOAckDocumentMap entity, string POId, out string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string sql = "SELECT * FROM [TRN].ServicePOAckDocumentMap WHERE Id='" + entity.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = entity.POId + "-" + ServicePOAckDocumentMap();
                    var createdId = dr["Id"];
                    dr["ServiceAcknowledgementMasterId"] = entity.POId;

                    dr["CompanyGroupId"] = entity.CompanyGroupId;

                    dr["UserFilename"] = entity.UserFilename;
                    dr["SystemFileName"] = createdId + Path.GetExtension(entity.UserFilename);
                    dr["Description"] = entity.Description;
                    dr["Remarks"] = entity.Remarks;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;


                    dsMaster.Tables[0].Rows.Add(dr);
                }
              

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();
            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }

        #region PO Parameter Change
        public IEnumerable<object> GetAllPOList(string column, string value, string plantId)
        {
            try
            {
                var Sql = "";
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                Sql = @"SELECT top(300)* FROM(SELECT  ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id
									, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate1
                                    , REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
									, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
									, CP.UserName AS PartyAccountGroupName
									, IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
									, REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
									, IR.FixedAssetOrInventory, IR.PODepended
									, IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount
									,ROUND(IRD.BaseAmount, 2) BaseAmount, IR.ToCurrencyRate
									, S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
									,isnull(IR.AddedBy,'') AddedBy
                                   ,isnull(PLC.LCRef,'') PurchaseLC
									,isnull(Cn.ContractNo,'') ContractNo
									,isnull(Par1.UserName,'') Customer
									,isnull(IR.CheckedByStatus,'') AS CheckedByStatus
									,isnull(IR.AuthorizedByStatus,'') AS AuthorizedByStatus
                                    ,isnull(eI.EmployeeName,'') CheckedBy
									,isnull(eI1.EmployeeName,'') ApprovedBy
									,isnull(IR.ContractId,'') ContractId
									,isnull(IR.OrderSpecific,'') OrderSpecific
									,isnull(IR.PurchaseLCId,'') PurchaseLCId
									,isnull(Par.UserName,'') CustomerName 
                                    ,PT.PaymentMode,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById, DiscountAmount=CASE WHEN IR.DiscountAmount IS NULL THEN 0 ELSE IR.DiscountAmount END,IR.AddedDate,IR.Tolerance,IR.POType
						FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
						LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
									ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
                         LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						LEFT JOIN [HKP].[Party] Par1 ON Par1.Id= Ctc.CustomerId
						LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy

						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
						LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
						LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
						LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
						LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
						LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
						LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
						LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
						LEFT JOIN [dbo].[Contract] AS Cn ON IR.ContractId=Cn.Id
						LEFT JOIN [HKP].[Party] AS Par ON Cn.CustomerId=Par.Id 
						LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
						LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
						LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
									JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
									WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id						
						WHERE  IR.PlantId='" + plantId + @"') AS TEMP WHERE " + strkey + " order by TEMP.AddedDate desc";
                return _sqlRepository.GetDataCollection(Sql);
            }

            catch (Exception ex)
            {
                throw ex;

            }
        }

        public IEnumerable<object> GetLCList(string masterId)
        {
            try
            {
                var sql = @"SELECT LC.LCRef FROM TRN.PurchaseOrder PO JOIN dbo.PurchaseLC LC ON LC.Id=PO.PurchaseLCId where PO.Id='" + masterId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetGRNList(string masterId)
        {
            try
            {
                var sql = @"SELECT SUM(IRD.TotalMaterialTranAmount) TotalAmount FROM 
                            TRN.InventoryReceiveDetail IRD
                            JOIN TRN.PurchaseOrder PO ON PO.Id=IRD.POId
                            WHERE PO.Id='" + masterId + "' GROUP BY PO.Id";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetAcceptanceList(string masterId)
        {
            try
            {
                var sql = @"SELECT SUM(IRD.TotalMaterialTranAmount) TotalAmount FROM 
                            TRN.PurchaseDocAcceptanceDetail IRD
                            JOIN TRN.PurchaseOrder PO ON PO.Id=IRD.POId
                            WHERE PO.Id='" + masterId + "' GROUP BY PO.Id";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

    }
}