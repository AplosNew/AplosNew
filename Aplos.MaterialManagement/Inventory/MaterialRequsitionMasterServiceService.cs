using Library.Core;
using Library.Crosscutting;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Model.Inventory;
using Library.Model.Logs;
using Library.Model.Organizations;
using Library.Model.Setups;
using Library.Model.Taxations;
using Library.Service.Addresses;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using Library.ViewModel.OrderManagements;
using Library.ViewModel.Setup;
using Library.ViewModel.Setups;
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
    public class MaterialRequsitionMasterServiceService : Service<MaterialRequsitionMaster>, IMaterialRequsitionMasterServiceService

    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<MaterialRequsitionMaster> _materialRequsitionMaster;
        private readonly IRepositoryAsync<NotificationSetting> _notificationSetting;
        private readonly IRepositoryAsync<MaterialRequsitionDetails> _materialRequsitionDetailsRepository;
        private readonly IRepositoryAsync<MaterialRequsitionMaster> _materialRequsitionRepository;
        private readonly IRepositoryAsync<CompanyGroup> _companyGroupRepository;
        private readonly IRepositoryAsync<Company> _companyRepository;
        private readonly ISMTPConfigurationService _smtpConfigurationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<MailLog> _mailLogRepository;
        private readonly IRepositoryAsync<MailReceiverDetail> _mailReceiverDetailRepository;
        private readonly IRepositoryAsync<EmployeeInformation> _employeeInformationRepository;

        public MaterialRequsitionMasterServiceService(
            IRepositoryAsync<MaterialRequsitionMaster> materialRequsitionMaster
             ,IRepositoryAsync<NotificationSetting> notificationSetting
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IRepositoryAsync<MaterialRequsitionDetails> materialRequsitionDetailsRepository
            , IRepositoryAsync<MaterialRequsitionMaster> materialRequsitionMasterRepository
             , IRepositoryAsync<CompanyGroup> companyGroupRepository
            , IRepositoryAsync<Company> companyRepository
            , ISMTPConfigurationService smtpConfigurationService
            , IRepositoryAsync<MailLog> mailLogRepository
            ,IRepositoryAsync<MailReceiverDetail> mailReceiverDetailRepository
            , IRepositoryAsync<EmployeeInformation> employeeInformationRepository

            ) : base(materialRequsitionMaster, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
            _materialRequsitionMaster = materialRequsitionMaster;
            _notificationSetting = notificationSetting;
            _materialRequsitionDetailsRepository = materialRequsitionDetailsRepository;
            _materialRequsitionDetailsRepository = materialRequsitionDetailsRepository;
            _materialRequsitionRepository = materialRequsitionMasterRepository;
            _companyGroupRepository = companyGroupRepository;
            _companyRepository = companyRepository;
            _smtpConfigurationService = smtpConfigurationService;
            _mailLogRepository = mailLogRepository;
            _mailReceiverDetailRepository = mailReceiverDetailRepository;
            _employeeInformationRepository = employeeInformationRepository;


        }

        #endregion Constructor

        #region InventoryReceive

        //private string GetPK()
        //{
        //    return GetAutoNumber(nameof(MaterialRequsitionMaster), PKGeneratorEnum.Yearly, null, DateTime.Now);
        //}
        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(MaterialRequsitionMaster), out sID);
            return sID;
        }
        
        public void InsertRequsition(MaterialRequsitionMaster entity)
        {
            try
            {
                
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var plantId = _materialRequsitionRepository.SqlQuery<string>($"SELECT FilePrefix from org.plant WHERE Id ='{identity.PlantId}'").FirstOrDefault();
                if (plantId == null)
                {
                    throw new CustomException("No Prefix Available for this Plant");
                }
                
                
                var id = GetPK();
                entity.Id = plantId + id;
         
                AuditService.AddedLog(entity);
                entity.ModelState = ModelState.Added;
                _materialRequsitionMaster.Insert(entity);
                var DailySendMailRequisition = _notificationSetting.SqlQuery<bool>(@"Select NotificationAfterCreation  from NotificationSetting Where BusinessFlow = 'MaterialRequistion'").FirstOrDefault();
                if (DailySendMailRequisition == true)
                {
                   
                    DailySendMailRequisitionCheck(entity.CheckedBy, entity.AddedFromIP, "", entity.Id, entity.RequirmentType, entity.RequisitionType, entity.AddedBy);
                }

                else
                {

                }
                
                _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public override void Update(MaterialRequsitionMaster entity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                
                entity.CompanyGroupId = identity.CompanyGroupId;
                base.Update(entity);
                DailySendMailRequisitionCheck(entity.CheckedBy, entity.AddedFromIP, "", entity.Id, entity.RequirmentType, entity.RequisitionType, entity.ReqEmpId);
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
                        WHERE IR.PlantId='" + plantId + "'  AND ISNULL(IR.[Status],'')<>'Posting'  AND IR.EmployeeId IS NULL AND IR.IsApproved=0 AND isnull(IR.IsClosed,0)=0 Order by IR.PODate DESC, IR.ID DESC";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
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
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,IR.AddedBy
                                    ,eI.EmployeeName CheckedBy
                                    ,IR.CheckedByStatus AS CheckedByStatus
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus
                                    ,eI1.EmployeeName AuthorizedBy

                        FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
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
                        WHERE IR.PlantId='" + plantId + @"' and IR.CheckedBy='" + identity.EmployeeId + @"' AND CheckedbyStatus = 'Checked' AND ISNULL(IR.[Status],'')<>'Posting'  AND IR.EmployeeId IS NULL AND IR.IsApproved=0 Order by IR.ID DESC";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetListForPOApproval1UnApproved(string plantId)
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
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,IR.AddedBy
                                    ,eI.EmployeeName CheckedBy
                                    ,IR.CheckedByStatus AS CheckedByStatus
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus
                                    ,eI1.EmployeeName AuthorizedBy

                        FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
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
                        WHERE IR.PlantId='" + plantId + @"'  AND ISNULL(IR.[Status],'')<>'Posting'  AND IR.EmployeeId IS NULL AND IR.IsApproved=1 Order by IR.ID DESC";
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
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,IR.AddedBy
                                    ,eI.EmployeeName CheckedBy
                                    ,IR.CheckedByStatus AS CheckedByStatus
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus
                                    ,eI1.EmployeeName AuthorizedBy

                        FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
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
                        WHERE IR.PlantId='" + plantId + @"' and IR.AuthorizedBy='" + identity.EmployeeId + @"' AND AuthorizedbyStatus = 'Approved' AND ISNULL(IR.[Status],'')<>'Posting'  AND IR.EmployeeId IS NULL AND IR.IsApproved=1 Order by IR.ID DESC";
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
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,IR.AddedBy
                                    ,eI.EmployeeName CheckedBy
                                    ,IR.CheckedByStatus AS CheckedByStatus
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus
                                    ,eI1.EmployeeName AuthorizedBy

                        FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
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
                        WHERE IR.PlantId='" + plantId + @"' and IR.CheckedBy='" + identity.EmployeeId + @"' AND CheckedbyStatus <> 'Checked' AND ISNULL(IR.[Status],'')<>'Posting'  AND IR.EmployeeId IS NULL AND IR.IsApproved=0 Order by IR.ID DESC";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetListForPOApprovalAuthorized(string plantId)
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
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,IR.AddedBy
                                    ,eI.EmployeeName CheckedBy
                                    ,IR.CheckedByStatus AS CheckedByStatus
			                        ,IR.AuthorizedByStatus AS AuthorizedByStatus
                                    ,eI1.EmployeeName AuthorizedBy

                        FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        
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
                        WHERE IR.PlantId='" + plantId + @"' and IR.AuthorizedBy is not null AND CheckedbyStatus = 'Checked' AND Authorizedbystatus <> 'Approved' AND ISNULL(IR.[Status],'')<>'Posting'  AND IR.EmployeeId IS NULL AND IR.IsApproved=0 Order by IR.ID DESC";
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
                                            FromCurrencyCode='" + currencyId + "'   AND A.CompanyId='" + companyId + "' ORDER BY CAST(FromDate AS DATE) DESC), 0)";
                    toCurrencyRate = _materialRequsitionDetailsRepository.SqlQuery<decimal>(sql).First();
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

        public IEnumerable<object> GetTaxCategoryList(string companyGroupId, string receiveId, string plantId, string hsnCodeId)
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

        #endregion Tax

        #region PurchaseOrder Service

        #endregion
        public decimal GetChargesRatio(string receiveId, string detailId, decimal detailTotalAmnt, string serviceId, decimal svcTotalAmnt, bool isNonCreditable)
        {
            try
            {
                decimal svcAmount = 0;
                if (isNonCreditable)
                    svcAmount = _materialRequsitionDetailsRepository.SqlQuery<decimal>("SELECT ISNULL(SUM(Amount), 0)+ISNULL(SUM(TotalTaxAmount), 0) FROM TRN.POService WHERE InventoryReceiveId='" + receiveId + "' AND ISNULL(Id, '')<>'" + serviceId + "'").First();
                else
                    svcAmount = _materialRequsitionDetailsRepository.SqlQuery<decimal>("SELECT ISNULL(SUM(Amount), 0) FROM TRN.POService WHERE InventoryReceiveId='" + receiveId + "' AND ISNULL(Id, '')<>'" + serviceId + "'").First();
                if (svcTotalAmnt > 0) svcAmount += svcTotalAmnt;
                else svcAmount -= svcTotalAmnt;

                var detailAmount = _materialRequsitionDetailsRepository.SqlQuery<decimal>("SELECT ISNULL(SUM(TransactionAmount), 1) FROM TRN.PurchaseOrderDetail WHERE InventoryReceiveId='" + receiveId + "' AND ISNULL(Id, '')<>'" + detailId + "'").First();
                if (detailTotalAmnt > 0) detailAmount += detailTotalAmnt;
                else detailAmount -= detailTotalAmnt;

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

        
        public void PaymentHold(IEnumerable<PurchaseOrder> entities)
        {
            
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
                       ,ISNULL(P.InvoicingByAddress, (AM.Address1+AM.Address2 +Isnull(AM.Address3,''))) AS Address1
	                   ,ISNULL(P.DeliveryByAddress, (AM.Address1+AM.Address2+Isnull(AM.Address3,''))) AS Address2	
                        , CO.Code AS CountryCode, CO.UserName AS CountryName, AM.StateId, S.Code AS StateCode, S.UserName AS StateName, AM.CityId, C.Code AS CityCode, C.UserName AS CityName
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






        #region Taufik RequisitionReport 
        public void RequisitionReportby(string CompanyGroupId, string PlantId, string RequisitionId, string startDate, string endDate,string empId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (PlantId == null || PlantId == "undefined" || PlantId =="")
                PlantId = identity.PlantId;
            ReportUtility ru = new ReportUtility();

            var fileName = "";
            var strPath = "";

            var File = "";

          

            fileName = "Requisition" + PlantId + ".docx";
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

                DataTable dsOrderMaster, dsServiceItems, dsTotalEmpWise;
                dsOrderMaster = loadOrderReqqusitionMaster(RequisitionId);//sql
                dsTotalEmpWise = LoadRequisitionMasterTotalEmpWise(RequisitionId, startDate,  endDate,  empId);
                Dictionary<string, string> columns = new Dictionary<string, string>();
                var poApprovedStatus = "";


                document.Replace("{Remarks}", dsOrderMaster.Rows[0]["Remarks"].ToString(), false, false);
                document.Replace("{AddedBy}", dsOrderMaster.Rows[0]["AddedBy"].ToString(), false, false);
                document.Replace("{CheckedByName}", dsOrderMaster.Rows[0]["CheckedByName"].ToString(), false, false);
                document.Replace("{AuthorizedByName}", dsOrderMaster.Rows[0]["AuthorizedByName"].ToString(), false, false);
                

                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);
                //dsServiceItems = loadServicerMasterItems(RequisitionId);
                DataTable dsMaterialItems = requistionMaterialDetail(RequisitionId);
                DataTable dsRequisitionForMonth = requistionForCurrentMonth(RequisitionId,startDate,endDate,empId);       

                var materialTotal = makeMaterialDetailsTable(document, dsMaterialItems, RequisitionId);//Material Details 
                var materialTotal1 = makeRequisitionTotalTable(document, dsTotalEmpWise, RequisitionId);//Material Details 
                var RequisitionForMonth = makeMonthlyRequisitionTable(document, dsRequisitionForMonth, RequisitionId);//Material Details 

              
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
                string Prefix = "Req" + RequisitionId;
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

		private object AppendText(double v)
		{
			throw new NotImplementedException();
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
                                where MasterOrderId='20194'";

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

        public DataTable loadOrderReqqusitionMaster(string MaterialMasterId)
        {
            string strSQL;
            //clsConnection objCon;
            try
            {
                strSQL = @"Select 
	                            MRM.Id RQNumber                            
								,REPLACE(CONVERT(VARCHAR(11),MRM.RequisitionDate, 113), ' ', '-') RequisitionDate
	                            ,MRM.RequisitionType
	                            ,MRM.RequirmentType
	                            ,MRM.QualityApprovalResponsiblePersonId
	                            ,MRM.NeedSpecialAppId
	                            ,E.UserName EntityName
	                            ,MRM.EntityId
	                            ,MRM.ReasonWhyItIsNotPlanEarlier requirementReason
	                            --,MRM.AddedBy
                                ,MRM.AddedDate
                                ,MRM.OrderRefNo
	                            ,MRM.AddedFromIP
	                            ,MRM.UpdatedBy
	                            ,MRM.UpdatedDate
	                            ,MRM.UpdatedFromIP
	                            ,MRM.Remarks
	                            
								,CheckedByName=CASE WHEN MRM.CheckedByStatus='Checked' Then eI.EmployeeName else '' END 
                                                ,AuthorizedByName=CASE When MRM.AuthorizedByStatus='Approved'then eI1.EmployeeName else '' END
                                                ,AddedBy=CASE When MRM.CheckedByStatus='For Checking' OR MRM.CheckedByStatus='Hold' OR MRM.CheckedByStatus='Reject' OR MRM.CheckedByStatus='Checked'then empPreparedby.EmployeeName else ''  END 
	                            ,MRM.AuthorizedByStatus
	                            ,MRM.IsApproved
	                            ,A.UserName ActivityName
	                            ,MM.UserName MaterialName
	                            ,MRD.TransactionQty
	                            ,MRD.EstimatedRate
	                            ,MRD.TotalAmount

								,PurOrCheckedStatus= CASE when MRM.CheckedByStatus='For Checking' Then 'To be checked'
                           when MRM.CheckedByStatus='Hold' Then 'Hold'
						   when MRM.CheckedByStatus='Reject' Then 'Reject'
						   when MRM.CheckedByStatus='Checked' Then 'Checked'
						  else ''
						   
							END
			  ,PurOrApprovedStatus= CASE 
						   when MRM.AuthorizedByStatus='Reject' Then 'Reject For Approved'
						   when MRM.AuthorizedByStatus='Hold' Then 'Hold For Approved'
						   when MRM.AuthorizedByStatus='For Approval' Then 'To be Approval'
						   when MRM.AuthorizedByStatus='Approved' Then 'Approved'
						   else ''
							END
                            ,REPLACE(CONVERT(VARCHAR(11),x.UpdatedDate, 113), ' ', '-') CheckedDate
                            ,REPLACE(CONVERT(VARCHAR(11),y.UpdatedDate, 113), ' ', '-') ApprovedDate
                                --, empPreparedby.EmployeeName AddedBy --- EEICHK.EmployeeCode +'-'+
	                            FROM [TRN].[MaterialRequsitionMaster] MRM
	                            Left Join [TRN].[MaterialRequsitionDetails] MRD On MRD.MaterialReqqusitionMasterId=MRM.Id
	                            Left Join org.Entity E on E.Id=MRM.EntityId
	                                LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=MRM.CheckedBy
                                         LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=MRM.AuthorizedBy
                                Left Join EmployeeInformation empPreparedby on empPreparedby.SystemId=MRM.ReqEmpId
                                --LEFT JOIN [SEC].[User] U ON U.UserId=MRM.AddedBy

	                            LEFT JOin HKp.Activity A On A.Id=MRD.ActivityId
	                            Left Join MST.MaterialMaster MM on MM.Id=MRD.MaterialMasterId 
                                Left join (select top(1) ReqId,UpdatedDate from trn.RequisitionApprovalLog where ReqId='" + MaterialMasterId + @"' and Status='Checked' order by AddedDate desc)x on x.ReqId=MRM.Id
								Left join (select top(1) ReqId,UpdatedDate  from trn.RequisitionApprovalLog where ReqId='" + MaterialMasterId + @"' and Status='Approved' order by AddedDate desc) y on y.ReqId=MRM.Id
	                            WHERE MRM.Id='" + MaterialMasterId + @"'";

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

        public DataTable LoadRequisitionMasterTotalEmpWise(string MaterialMasterId,string startDate, string endDate,string empId) 
        {
            
            string strSQL;
            //clsConnection objCon;
            try
            {
                strSQL = @"select --sum(x.RequisitionId) RequisitionId,x.ReqEmpId,x.EmployeeName,sum(Round(x.ReqTotalAmount,2)) ReqTotalAmount,sum(Round(x.POTotalAmount,2)) POTotalAmount,sum(Round(x.GRNTOtalAmount,2)) GRNTOtalAmount 
									x.Code,sum(x.RequisitionId) RequisitionId,x.ReqEmpId,x.EmployeeName,convert(varchar, convert(money, sum(x.ReqTotalAmount)), 1) ReqTotalAmount,convert(varchar, convert(money, sum(x.POTotalAmount)), 1) POTotalAmount,sum(Round(x.GRNTOtalAmount,2)) GRNTOtalAmount 
                                        from (
                                        Select  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName, Count(MR.Id) RequisitionId, 0 ReqTotalAmount,0 POTotalAmount,0 GRNTOtalAmount 
                                        from trn.MaterialRequsitionMaster MR
										 LEFT JOIN(select Id,MaterialReqqusitionMasterId,CurrencyId ,Sum(TotalAmount) TotalAmount 
		                                        FROM trn.MaterialRequsitionDetails group by Id,MaterialReqqusitionMasterId,CurrencyId) MRD ON MRD.MaterialReqqusitionMasterId=MR.Id
                                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=MR.ReqEmpId
											left join scs.Currency C On MRD.CurrencyId=C.Id
                                        where MR.ReqEmpId='" + empId + "' and  MR.RequisitionDate between '" + startDate + "' AND '" + endDate + @"'
                                        Group by  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName

                                        UNION All
                                        Select  MRD.CurrencyId,C.Code, MR.ReqEmpId,EI.EmployeeName, 0 RequisitionId,Sum(ISNULL(MRD.TotalAmount,0)) ReqTotalAmount,0 POTotalAmount,0 GRNTOtalAmount 
                                        from trn.MaterialRequsitionMaster MR
                                        LEFT JOIN(select Id,MaterialReqqusitionMasterId,CurrencyId ,Sum(isnull(TotalAmount,0)) TotalAmount 
		                                        FROM trn.MaterialRequsitionDetails group by Id,MaterialReqqusitionMasterId,CurrencyId) MRD ON MRD.MaterialReqqusitionMasterId=MR.Id
                                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=MR.ReqEmpId
										left join scs.Currency C On MRD.CurrencyId=C.Id
                                        where MR.ReqEmpId='" + empId + "' and  MR.RequisitionDate between '" + startDate + "' AND '" + endDate + @"'
                                        Group by  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName

                                        UNION All
                                        Select  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName, 0 RequisitionId,0 ReqTotalAmount,sum(ISNULL(BaseAmount,0)) POTotalAmount,0 GRNTOtalAmount 
                                        from trn.MaterialRequsitionMaster MR
                                        LEFT JOIN(select Id,MaterialReqqusitionMasterId,CurrencyId ,Sum(ISNULL(TotalAmount,0)) TotalAmount 
                                        FROM trn.MaterialRequsitionDetails group by Id,MaterialReqqusitionMasterId,CurrencyId) MRD ON MRD.MaterialReqqusitionMasterId=MR.Id
                                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=MR.ReqEmpId                            
                                        left JOIN(select Id, RequisitionDetailId ,Sum(BaseAmount) BaseAmount from trn.PurchaseOrderDetail where RequisitionDetailId is not NULL  group by RequisitionDetailId,Id)PO ON PO.RequisitionDetailId=MRD.Id
                                        	left join scs.Currency C On MRD.CurrencyId=C.Id
                                        where MR.ReqEmpId='" + empId + "' and  MR.RequisitionDate between '" + startDate + "' AND '" + endDate + @"'
                                        Group by  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName
                                        )x
                                        Group By x.Code,x.EmployeeName,x.ReqEmpId";


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
							and InventoryReceiveDetailId  is not null and  InventoryServiceId is null 
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

        public double makeMaterialDetailsTable(WordDocument document, DataTable dsMaterialItems, string requisitionId)
        {
            string replaceString = "{materialRequsitionDetail}";
            ReportUtility ru = new ReportUtility();
            DataTable dsTax;
            //clsDataContext data = new clsDataContext();


            //dsTax = loadMaterialTax(purchaseOrderId);

            int LasColumnIndex = 17;
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
            int colRo = COL; COL++;
            wTable.Rows[ROW].Cells[colRo].Width = 25;

            wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("RowId");
            int colRowId = COL; COL++;
            wTable.Rows[ROW].Cells[colRowId].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Materials");
            range.ApplyCharacterFormat(FontBold);
            int colMaterialGroup = COL; COL++;
            wTable.Rows[ROW].Cells[colMaterialGroup].Width = 85;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article ");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            wTable.Rows[ROW].Cells[colArticle].Width = 85;
            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU1");
            range.ApplyCharacterFormat(FontBold);
            int colChar1 = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU2");
            range.ApplyCharacterFormat(FontBold);
            int colChar2 = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU3");
            range.ApplyCharacterFormat(FontBold);
            int colChar3 = COL; COL++;
            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Origin");//TRN.PurchaseOrderDetail ->CountryId
            //range.ApplyCharacterFormat(FontBold);
            //int colOriginCountry = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Material Detail");
            range.ApplyCharacterFormat(FontBold);
            int colMaterialDetail = COL; COL++;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Required Date");
            range.ApplyCharacterFormat(FontBold);
            int colRequiredDate = COL; COL++;
            wTable.Rows[ROW].Cells[colRequiredDate].Width = 60;



            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate (" + dsMaterialItems.Rows[0]["CurrencyName"].ToString() + ")");
            range.ApplyCharacterFormat(FontBold);
            int colRate = COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UOM");
            range.ApplyCharacterFormat(FontBold);
            int colUOM = COL; COL++;
            wTable.Rows[ROW].Cells[colUOM].Width = 25;





            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
            range.ApplyCharacterFormat(FontBold);
            int colTotalTaxableAmount = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Normal/Over Budget/New");
            range.ApplyCharacterFormat(FontBold);
            int colNormal = COL; COL++;
            wTable.Rows[ROW].Cells[colNormal].Width = 58;
            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Normal/OverBudget/New");
            //range.ApplyCharacterFormat(FontBold);
            //int colOB = COL; COL++;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Normal/OverBudget/New");
            //range.ApplyCharacterFormat(FontBold);
            //int colNew = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Own Stock");
            range.ApplyCharacterFormat(FontBold);
            int colOnStock = COL; COL++;
            wTable.Rows[ROW].Cells[colUOM].Width = 40;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Other Stock");
            range.ApplyCharacterFormat(FontBold);
            int colOtherStock = COL; COL++;
            wTable.Rows[ROW].Cells[colOtherStock].Width = 40;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Reason");
            range.ApplyCharacterFormat(FontBold);
            int colReason = COL; COL++;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Remarks");
            range.ApplyCharacterFormat(FontBold);
            int colRemarks = COL;

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
                }
                if (dsMaterialItems.Rows[i]["BudgetType"].ToString().ToUpper() == "NORMAL")
                {
                    NormalOverBudgetNew = "Normal";
                }
                if (dsMaterialItems.Rows[i]["BudgetType"].ToString().ToUpper() == "OVER BUDGET")
                {
                    NormalOverBudgetNew = "Over budget";
                }
                if (dsMaterialItems.Rows[i]["BudgetType"].ToString().ToUpper() != "NORMAL" && dsMaterialItems.Rows[i]["BudgetType"].ToString().ToUpper() != "OVER BUDGET")
                {
                    NormalOverBudgetNew = "New";
                }

                TROW.Cells[colRo].AddParagraph().AppendText(sl.ToString());

                TROW.Cells[colRowId].AddParagraph().AppendText(dsMaterialItems.Rows[i]["MDetailId"].ToString());
                //TROW.Cells[colRowId].AddParagraph().AppendText(Convert.ToDouble(dsMaterialItems.Rows[i]["MDetailId"]).ToString());
                //TROW.Cells[colRowId].Width = 70;
                TROW.Cells[colMaterialGroup].AddParagraph().AppendText(dsMaterialItems.Rows[i]["MaterialName"].ToString());
                TROW.Cells[colArticle].AddParagraph().AppendText(dsMaterialItems.Rows[i]["Article"].ToString());
                TROW.Cells[colChar1].AddParagraph().AppendText(dsMaterialItems.Rows[i]["FirstCharacteristicsValue"].ToString());
                TROW.Cells[colChar2].AddParagraph().AppendText(dsMaterialItems.Rows[i]["SecondCharacteristicsValue"].ToString());
                TROW.Cells[colChar3].AddParagraph().AppendText(dsMaterialItems.Rows[i]["ThirdCharacteristicsValue"].ToString());
                TROW.Cells[colQty].AddParagraph().AppendText(clsStdLib.dbl(dsMaterialItems.Rows[i]["TransactionQty"].ToString()).ToString("F2"));
                TROW.Cells[colRate].AddParagraph().AppendText(clsStdLib.dbl(dsMaterialItems.Rows[i]["TransactionRate"].ToString()).ToString("F2"));
                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsMaterialItems.Rows[i]["TrnAmount"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colUOM].AddParagraph().AppendText(dsMaterialItems.Rows[i]["TransactionUoM"].ToString());
                TROW.Cells[colNormal].AddParagraph().AppendText(NormalOverBudgetNew);
                //TROW.Cells[colOB].AddParagraph().AppendText(NormalOverBudgetNew);
                //TROW.Cells[colNew].AddParagraph().AppendText(NormalOverBudgetNew);
                TROW.Cells[colMaterialDetail].AddParagraph().AppendText(dsMaterialItems.Rows[i]["MaterialDetail"].ToString());
                TROW.Cells[colRequiredDate].AddParagraph().AppendText(dsMaterialItems.Rows[i]["RequiredDate"].ToString());

                TROW.Cells[colOtherStock].AddParagraph().AppendText(OtherStock);

                TROW.Cells[colReason].AddParagraph().AppendText(dsMaterialItems.Rows[i]["Reason"].ToString());
                TROW.Cells[colRemarks].AddParagraph().AppendText(dsMaterialItems.Rows[i]["Remarks"].ToString());
                //TROW.Cells[colReason].AddParagraph().AppendText(Reason);
                //TROW.Cells[colRemarks].AddParagraph().AppendText(Remarks);

                totalValue += clsStdLib.dbl(dsMaterialItems.Rows[i]["TrnAmount"].ToString());


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
                if (C == colRate || C == colRowId || C == colArticle || C == colChar1 || C == colChar2 || C == colChar3 || C == colQty || C == colUOM || C == colNormal || C == colMaterialDetail || C == colRequiredDate || C == colReason || C == colRemarks || C == colMaterialGroup)
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
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStdLib.dbl(dsMaterialItems.Compute("SUM(TrnAmount)", "").ToString());
            //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
            //+ clsStdLib.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());

            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2") + " (" + ru.InWord(total, dsOrderMaster.Rows[0]["CurrencyId"].ToString()) + ")");

            #endregion Total


            ROW++;


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
                //TROW.Cells[0].Width = 20;
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


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
          
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
        public double makeRequisitionTotalTable(WordDocument document, DataTable dsTotalEmpWise, string requisitionId)
        {
            string replaceString = "{materialRequsitionTotalDetail}";
            ReportUtility ru = new ReportUtility();
            DataTable dsTax;      
            int LasColumnIndex = 4;
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
            int colRo = COL; COL++;
            wTable.Rows[ROW].Cells[colRo].Width = 35;

            

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Requisition");
            range.ApplyCharacterFormat(FontBold);
            int colTotalRequisition = COL; COL++;
            wTable.Rows[ROW].Cells[colTotalRequisition].Width = 85;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Currency ");
            range.ApplyCharacterFormat(FontBold);
            int colCurrency = COL; COL++;
            wTable.Rows[ROW].Cells[colCurrency].Width = 85;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Requisition Amount");
            range.ApplyCharacterFormat(FontBold);
            int colTotalRequisitionAmount = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total PO Amount");
            range.ApplyCharacterFormat(FontBold);
            int colTotalPOAmount = COL; 
            #endregion column headers
            var NormalOverBudgetNew = "";    
            double totalValue = 0;
            int startRow = ROW;
            int sl = 0;
            for (int i = 0; i < dsTotalEmpWise.Rows.Count; i++)
            {
                sl++;            
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;      
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                }
                

                TROW.Cells[colRo].AddParagraph().AppendText(sl.ToString());

                TROW.Cells[colTotalRequisition].AddParagraph().AppendText(dsTotalEmpWise.Rows[i]["RequisitionId"].ToString());                
                TROW.Cells[colCurrency].AddParagraph().AppendText(dsTotalEmpWise.Rows[i]["Code"].ToString());
                TROW.Cells[colTotalRequisitionAmount].AddParagraph().AppendText(dsTotalEmpWise.Rows[i]["ReqTotalAmount"].ToString());
                TROW.Cells[colTotalPOAmount].AddParagraph().AppendText(dsTotalEmpWise.Rows[i]["POTotalAmount"].ToString());           


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
                if (C == colTotalRequisition || C == colCurrency)
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

            double total = 0;//clsStdLib.dbl(dsTotalEmpWise.Compute("SUM(1)", "").ToString());
            
            #endregion Total


            ROW++;
            #region Total Payable
            
            #endregion Total Payable


            ROW++;


            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle1 = document.AddParagraphStyle("MyStyle1");
            //Sets the formatting of the style
            myStyle1.CharacterFormat.FontSize = 8f;
            //myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle1.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle1");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            
            //primary cells merging (veritcal)
            ROW++;
            
            IWParagraphStyle style3 = document.AddParagraphStyle("SubTotalStyle3");
            style3.CharacterFormat.Bold = true;
            style3.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section

            #endregion merging section

            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);
            return total;
        }

        public double makeMonthlyRequisitionTable(WordDocument document, DataTable dsRequisitionForMonth, string requisitionId)
        {
            string replaceString = "{materialRequsitionForMonth}";
            ReportUtility ru = new ReportUtility();
            DataTable dsTax;
            int LasColumnIndex = 4;
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
            int colRo = COL; COL++;
            wTable.Rows[ROW].Cells[colRo].Width = 35;



            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Requisition");
            range.ApplyCharacterFormat(FontBold);
            int colTotalRequisition = COL; COL++;
            wTable.Rows[ROW].Cells[colTotalRequisition].Width = 85;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Currency ");
            range.ApplyCharacterFormat(FontBold);
            int colCurrency = COL; COL++;
            wTable.Rows[ROW].Cells[colCurrency].Width = 85;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Requisition Amount");
            range.ApplyCharacterFormat(FontBold);
            int colTotalRequisitionAmount = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total PO Amount");
            range.ApplyCharacterFormat(FontBold);
            int colTotalPOAmount = COL;
            #endregion column headers
            var NormalOverBudgetNew = "";
            double totalValue = 0;
            int startRow = ROW;
            int sl = 0;
            for (int i = 0; i < dsRequisitionForMonth.Rows.Count; i++)
            {
                sl++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                }


                TROW.Cells[colRo].AddParagraph().AppendText(sl.ToString());

                TROW.Cells[colTotalRequisition].AddParagraph().AppendText(dsRequisitionForMonth.Rows[i]["RequisitionId"].ToString());
                TROW.Cells[colCurrency].AddParagraph().AppendText(dsRequisitionForMonth.Rows[i]["Code"].ToString());
                TROW.Cells[colTotalRequisitionAmount].AddParagraph().AppendText(dsRequisitionForMonth.Rows[i]["ReqTotalAmount"].ToString());
                TROW.Cells[colTotalPOAmount].AddParagraph().AppendText(dsRequisitionForMonth.Rows[i]["POTotalAmount"].ToString());


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
                if (C == colTotalRequisition || C == colCurrency)
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

            double total = 0;//clsStdLib.dbl(dsTotalEmpWise.Compute("SUM(1)", "").ToString());

            #endregion Total


            ROW++;
            #region Total Payable

            #endregion Total Payable


            ROW++;


            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyleRequisitionForMonth");
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
                        item.ApplyStyle("MyStyleRequisitionForMonth");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;

            //primary cells merging (veritcal)
            ROW++;

            IWParagraphStyle style3 = document.AddParagraphStyle("SubTotalStyleForMonth3");
            style3.CharacterFormat.Bold = true;
            style3.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section

            #endregion merging section

            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);
            return total;
        }


        public double makeServiceDetailsTable(WordDocument document, DataTable dsServiceItems, string purchaseOrderId)
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


            dsTax = loadServiceMasterTax(purchaseOrderId);

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

                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate");
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
               
                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsServiceItems.Rows[i]["Amount"].ToString()).ToString("#,##0.00"));

                totalValue += clsStdLib.dbl(dsServiceItems.Rows[i]["Amount"].ToString());

                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(totalValue.ToString("F2"));

                if (dv.Count > 0)
                {
                    //dsTax.Tables[0].DefaultView.RowFilter = "MasterOrderItemId='" + dsOrderItems.Tables[0].Rows[i]["MasterOrderItemId"].ToString() + "'";
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

        private DataTable requistionMaterialDetail(string requistionId)
        {
            try
            {
                string sqlText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + requistionId + @"'
	           
                           SELECT IM.Id AS MDetailId
                             , IM.MaterialReqqusitionMasterId 
                       	     , REPLACE(CONVERT(CHAR(11),IM.DeliveryDate, 106),' ','-') AS RequiredDate
                            , MGM.UserName AS MaterialGroupMasterName
                            , IM.MaterialMasterId, MM.UserName MaterialName
                            , IM.ArticleId, ART.StandardName Article
                            , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                            , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                            , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                            , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                            , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                            , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
                            , ROUND(IM.TransactionQty,2) TransactionQty
	                        , IM.TransactionUoMId
                             ,IM.BudgetType 
	                        , TUoM.UserName AS TransactionUoM
	                        , ROUND(IM.EstimatedRate,2) TransactionRate 
	                        , CU.Code AS CurrencyName
		                        , CU.Id AS CurrencyId
                            , ROUND((IM.TransactionQty*IM.EstimatedRate),2) AS TrnAmount   
                            ,IM.MaterialDetail
                            ,Replace(CONVERT(VARCHAR(11), IM.DeliveryDate, 106), ' ', '-') DeliveryDate

	                        ,Act.Id As Activity
	                        ,Act.UserName As ActivityName
	                        ,IM.OwnStock
                            ,IM.OtherStock 
	                        ,IM.Reason
	                        ,IM.Remarks
	                        ,IM.FutureReqApp
	                        --,BudgetMasterId
	                        --,GLGeneralInfoId
                        FROM TRN.MaterialRequsitionDetails AS IM
                        LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id

                        LEft JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IM.TransactionUoMId=TUoM.Id
                        LEFT JOIN [TRN].[MaterialRequsitionMaster] AS IR ON IM.MaterialReqqusitionMasterId=IR.Id
                        LEFT JOIN [SCS].[Currency] AS CU ON IM.CurrencyId=CU.Id 
                        LEFT JOIN [HKP].[Activity] As Act On ACT.Id=IM.ActivityId
                        --JOIN [HKP].Budget
                        --JOIN [HKP].Gl
                        WHERE IM.MaterialReqqusitionMasterId=@inventoryReceiveId";

                return _sqlRepository.GetDataTable(sqlText);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private DataTable requistionForCurrentMonth(string RequisitionId, string startDate, string endDate, string empId)
        {
            string strSQL;
            var myDate = Convert.ToDateTime(DateTime.Now);
            var startOfMonth = new DateTime(myDate.Year, myDate.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            startDate = Convert.ToDateTime(startOfMonth).ToString("dd-MMM-yyyy");
            endDate = Convert.ToDateTime(endOfMonth).ToString("dd-MMM-yyyy");
            try
            {
                strSQL = @"select --sum(x.RequisitionId) RequisitionId,x.ReqEmpId,x.EmployeeName,sum(Round(x.ReqTotalAmount,2)) ReqTotalAmount,sum(Round(x.POTotalAmount,2)) POTotalAmount,sum(Round(x.GRNTOtalAmount,2)) GRNTOtalAmount 
        x.Code,sum(x.RequisitionId) RequisitionId,x.ReqEmpId,x.EmployeeName,convert(varchar, convert(money, sum(x.ReqTotalAmount)), 1) ReqTotalAmount,convert(varchar, convert(money, sum(x.POTotalAmount)), 1) POTotalAmount,sum(Round(x.GRNTOtalAmount,2)) GRNTOtalAmount 
                                       from (
                                       Select  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName, Count(MR.Id) RequisitionId, 0 ReqTotalAmount,0 POTotalAmount,0 GRNTOtalAmount 
                                       from trn.MaterialRequsitionMaster MR
        	 LEFT JOIN(select Id,MaterialReqqusitionMasterId,CurrencyId ,Sum(TotalAmount) TotalAmount 
                                         FROM trn.MaterialRequsitionDetails group by Id,MaterialReqqusitionMasterId,CurrencyId) MRD ON MRD.MaterialReqqusitionMasterId=MR.Id
                                       LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=MR.ReqEmpId
        		left join scs.Currency C On MRD.CurrencyId=C.Id
                                       where MR.ReqEmpId='" + empId + "' and  MR.RequisitionDate between '" + startDate + "' AND '" + endDate + @"'
                                       Group by  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName

                                       UNION All
                                       Select  MRD.CurrencyId,C.Code, MR.ReqEmpId,EI.EmployeeName, 0 RequisitionId,Sum(ISNULL(MRD.TotalAmount,0)) ReqTotalAmount,0 POTotalAmount,0 GRNTOtalAmount 
                                       from trn.MaterialRequsitionMaster MR
                                       LEFT JOIN(select Id,MaterialReqqusitionMasterId,CurrencyId ,Sum(TotalAmount) TotalAmount 
                                         FROM trn.MaterialRequsitionDetails group by Id,MaterialReqqusitionMasterId,CurrencyId) MRD ON MRD.MaterialReqqusitionMasterId=MR.Id
                                       LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=MR.ReqEmpId
        	left join scs.Currency C On MRD.CurrencyId=C.Id
                                       where MR.ReqEmpId='" + empId + "' and  MR.RequisitionDate between '" + startDate + "' AND '" + endDate + @"'
                                       Group by  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName

                                       UNION All
                                       Select  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName, 0 RequisitionId,0 ReqTotalAmount,sum(ISNULL(BaseAmount,0)) POTotalAmount,0 GRNTOtalAmount 
                                       from trn.MaterialRequsitionMaster MR
                                       LEFT JOIN(select Id,MaterialReqqusitionMasterId,CurrencyId ,Sum(TotalAmount) TotalAmount 
                                       FROM trn.MaterialRequsitionDetails group by Id,MaterialReqqusitionMasterId,CurrencyId) MRD ON MRD.MaterialReqqusitionMasterId=MR.Id
                                       LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=MR.ReqEmpId                               
                                       left JOIN(select Id, RequisitionDetailId ,Sum(BaseAmount) BaseAmount from trn.PurchaseOrderDetail where RequisitionDetailId is not NULL  group by RequisitionDetailId,Id)PO ON PO.RequisitionDetailId=MRD.Id
                                       	left join scs.Currency C On MRD.CurrencyId=C.Id
                                       where MR.ReqEmpId='" + empId + "' and  MR.RequisitionDate between '" + startDate + "' AND '" + endDate + @"'
                                       Group by  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName
                                       )x
                                       Group By x.Code,x.EmployeeName,x.ReqEmpId";

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
                        where id=201925101";
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







        public DataTable RequsitionMaster(string RequisitionId)
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
        public DataTable loadServicerMasterItems(string purchaseOrderId)
        {
            string strSQL;

            try
            {
                strSQL = @"SELECT POS.Id ServiceId,SM.UserName  Service ,POS.Amount,POS.TotalTaxAmount,Pos.AddedBy,pos.AddedDate,pos.UpdatedBy,pos.UpdatedDate FROM TRN.PurchaseOrder PO
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




        #region PO Approval for post
        public void PoApproved(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy)
        {
            try
            {
                var AuthorizedById = "";

                PoValue = "0";
                var Id = GetPK();
                if (CheckedStataus == "Checked")
                {
                    if (AuthorizedBy == null || AuthorizedBy == "")
                    {
                        throw new CustomException("Select Approved By");
                    }
                    AuthorizedById = AuthorizedBy;

                }
                else
                {
                    AuthorizedById = null;

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
                string _sql = "Update TRN.PurchaseOrder set IsApproved='0',CheckedByStatus='" + Status + "',AuthorizedBy='" + AuthorizedById + "' where id='" + PoId + "'";
                _sqlRepository.ExecuteSqlCommand(_sql);
                string _sql1 = "Insert into TRN.PurchaseOrderApprovalLog(Id," +
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
                "'" + ip + "','" + PoId + "')";
                _sqlRepository.ExecuteSqlCommand(_sql1);
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
                string _sql = "Update TRN.PurchaseOrder set IsApproved='0',CheckedBy=null,CheckedByStatus=null,AuthorizedBy=null,AuthorizedByStatus=null where id='" + PoId + "'";
                _sqlRepository.ExecuteSqlCommand(_sql);
                string _sql1 = "Insert into TRN.PurchaseOrderApprovalLog(Id," +
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
                "'" + ip + "','" + PoId + "')";
                _sqlRepository.ExecuteSqlCommand(_sql1);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }



        public void PoApprovedAuth(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy)
        {
            try
            {
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
                string _sql = "Update TRN.PurchaseOrder set AuthorizedByStatus='" + Status + "',IsApproved='" + IsApproved + "', AuthorizedBy='" + identity.EmployeeId + "' where id='" + PoId + "'";
                _sqlRepository.ExecuteSqlCommand(_sql);
                string _sql1 = "Insert into TRN.PurchaseOrderApprovalLog(Id," +
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
                "'" + ip + "','" + PoId + "')";
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

        #region un Approval for post
        public void PoApproved1(string PoId, string PoValue)
        {
            try
            {
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
              
                string _sql = "Update TRN.PurchaseOrder set IsApproved='0' where id='" + PoId + "'";
                _sqlRepository.ExecuteSqlCommand(_sql);
                string _sql1 = "Insert into TRN.PurchaseOrderApprovalLog(Id," +
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
                "'" + ip + "','" + PoId + "')";
                _sqlRepository.ExecuteSqlCommand(_sql1);
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
               
                string _sql = "Update TRN.PurchaseOrder set IsApproved='0' where id='" + PoId + "'";
                _sqlRepository.ExecuteSqlCommand(_sql);
                string _sql1 = "Insert into TRN.PurchaseOrderApprovalLog(Id," +
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
                "'" + ip + "','" + PoId + "')";
                _sqlRepository.ExecuteSqlCommand(_sql1);
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
                var sql = @"select E.SystemId As Value, (E.EmployeeCode+'-'+E.EmployeeName) As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='RequisitionCheckedBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        public IEnumerable<object> GetCheckedByAndApprovedBY(string CheckedBy, string ApprovedBy) 
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
                    sql = @"select E.SystemId As Value, (E.EmployeeCode+'-'+E.EmployeeName) As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='RequisitionCheckedBy' and E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                }
                else if (CheckedBy == "false" && ApprovedBy == "true")
                {
                    sql = @"select E.SystemId As Value, (E.EmployeeCode+'-'+E.EmployeeName) As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='RequisitionApproveBy' and E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
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


        public IEnumerable<object> GetSupervisorCboApproved()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select E.SystemId As Value, E.SystemId+'-'+E.EmployeeName As Text from dbo.SupervisorSetUp A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where A.PlantId='" + identity.PlantId + "' AND A.ActionStatus='AuthorizedBy'";
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> GetEntity()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select Id as Value, UserName As Text from ORG.Entity";
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> GetEmployee()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select SystemId as Value, EmployeeName As Text from [dbo].[EmployeeInformation]";
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetAllReqdata(string ReqStatus)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = "";
                if (ReqStatus == "ForChecked")
                {
                    sql = @"select x.Id
	                                ,REPLACE(CONVERT(CHAR(11), x.RequisitionDate, 106),' ','-') AS RequisitionDate ,REPLACE(CONVERT(CHAR(11), x.RequisitionDate, 106),' ','-') AS RequisitionDate1 
	                                ,x.RequisitionType
	                                ,x.RequirmentType
	                                ,x.EntityName
                                    ,x.Reason
	                                --,A.UserName ActivityName
	                                --,MM.UserName MaterialName
	                                ,SUM(x.TransactionQty) TransactionQty
	                                ,SUM(x.EstimatedRate) EstimatedRate
	                                ,SUM(x.TotalAmount) TotalAmount
                                    ,x.CheckedBy
                                    ,x.CheckedByStatus
	                                ,x.AuthorizedBy,x.OrderRefNo ,x.AuthorizedByStatus, x.ReqEmpId ,x.CheckedById,x.ApprovedById from 
                            (
                            Select 
	                                MRM.Id
	                                ,REPLACE(CONVERT(CHAR(11), MRM.RequisitionDate, 106),' ','-') AS RequisitionDate 
	                                ,MRM.RequisitionType
	                                ,MRM.RequirmentType
	                                ,E.UserName EntityName
                                    ,MRM.ReasonWhyItIsNotPlanEarlier Reason
	                                --,A.UserName ActivityName
	                                --,MM.UserName MaterialName
	                                ,SUM(MRD.TransactionQty) TransactionQty
	                                ,SUM(MRD.EstimatedRate) EstimatedRate
	                                ,SUM(MRD.TotalAmount) TotalAmount
                                    ,ei.EmployeeName AS CheckedBy,ei.SystemId CheckedById,ei1.SystemId ApprovedById
                                    ,MRM.CheckedByStatus
	                                ,ei1.EmployeeName AS AuthorizedBy,MRM.OrderRefNo ,MRM.AuthorizedByStatus, MRM.ReqEmpId
		                            --,ei2.EmployeeName As AddedBy
                                FROM [TRN].[MaterialRequsitionMaster] MRM 
                                Left Join  [TRN].[MaterialRequsitionDetails] MRD  On MRM.Id=MRD.MaterialReqqusitionMasterId
                                Left Join org.Entity E on E.Id=MRM.EntityId
                                --Left JOin HKp.Activity A On A.Id=MRD.ActivityId
                                --Left Join MST.MaterialMaster MM on MM.Id=MRD.MaterialMasterId 
                                LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=mrm.CheckedBy
                                LEFT JOIN EmployeeInformation AS ei1 ON ei1.SystemId=mrm.AuthorizedBy
	                            --LEFT JOIN EmployeeInformation AS ei2 ON ei2.SystemId=mrm.AddedBy
                                Where MRM.Id not in(Select distinct RequisitionId from trn.purchaseOrderDetail where RequisitionId is not null)--and RequisitionId='110232'
	                            AND MRM.CheckedByStatus IS NULL --OR MRM.CheckedByStatus='For Checking' 
                                AND MRM.AuthorizedByStatus IS NULL --OR MRM.AuthorizedBy='For Approval' 
                                AND MRM.ReqEmpId='" + identity.EmployeeId + @"'

                                group by MRM.Id,MRM.RequisitionDate,MRM.RequisitionType,MRM.RequirmentType
	                            ,E.UserName,MRM.ReasonWhyItIsNotPlanEarlier ,ei.EmployeeName,ei1.EmployeeName
	                            ,MRM.CheckedByStatus,MRM.OrderRefNo ,MRM.AuthorizedByStatus, MRM.ReqEmpId,ei.SystemId ,ei1.SystemId  
	                            union ALL
	                            Select 
	                                MRM.Id
	                                ,REPLACE(CONVERT(CHAR(11), MRM.RequisitionDate, 106),' ','-') AS RequisitionDate 
	                                ,MRM.RequisitionType
	                                ,MRM.RequirmentType
	                                ,E.UserName EntityName
                                    ,MRM.ReasonWhyItIsNotPlanEarlier Reason
	                                --,A.UserName ActivityName
	                                --,MM.UserName MaterialName
	                                ,SUM(MRD.TransactionQty) TransactionQty
	                                ,SUM(MRD.EstimatedRate) EstimatedRate
	                                ,SUM(MRD.TotalAmount) TotalAmount
                                    ,ei.EmployeeName AS CheckedBy,ei.SystemId CheckedById,ei1.SystemId ApprovedById
                                    ,MRM.CheckedByStatus
	                                ,ei1.EmployeeName AS AuthorizedBy,MRM.OrderRefNo ,MRM.AuthorizedByStatus, MRM.ReqEmpId
		                            --,ei2.EmployeeName As AddedBy
                                FROM [TRN].[MaterialRequsitionMaster] MRM 
                                Left Join  [TRN].[MaterialRequsitionDetails] MRD  On MRM.Id=MRD.MaterialReqqusitionMasterId
                                Left Join org.Entity E on E.Id=MRM.EntityId
                                --Left JOin HKp.Activity A On A.Id=MRD.ActivityId
                                --Left Join MST.MaterialMaster MM on MM.Id=MRD.MaterialMasterId 
                                LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=mrm.CheckedBy
                                LEFT JOIN EmployeeInformation AS ei1 ON ei1.SystemId=mrm.AuthorizedBy
	                            --LEFT JOIN EmployeeInformation AS ei2 ON ei2.SystemId=mrm.AddedBy
                                Where  MRM.ReqEmpId='" + identity.EmployeeId + @"' 	AND  MRM.CheckedByStatus='For Checking' 
                       
                        
	                            group by MRM.Id,MRM.RequisitionDate,MRM.RequisitionType,MRM.RequirmentType
	                            ,E.UserName,MRM.ReasonWhyItIsNotPlanEarlier ,ei.EmployeeName,ei1.EmployeeName
	                            ,MRM.CheckedByStatus,MRM.OrderRefNo ,MRM.AuthorizedByStatus, MRM.ReqEmpId,ei.SystemId ,ei1.SystemId 

	                            Union All

	                            Select 
	                                MRM.Id
	                                ,REPLACE(CONVERT(CHAR(11), MRM.RequisitionDate, 106),' ','-') AS RequisitionDate 
	                                ,MRM.RequisitionType
	                                ,MRM.RequirmentType
	                                ,E.UserName EntityName
                                    ,MRM.ReasonWhyItIsNotPlanEarlier Reason
	                                --,A.UserName ActivityName
	                                --,MM.UserName MaterialName
	                                ,SUM(MRD.TransactionQty) TransactionQty
	                                ,SUM(MRD.EstimatedRate) EstimatedRate
	                                ,SUM(MRD.TotalAmount) TotalAmount
                                    ,ei.EmployeeName AS CheckedBy,ei.SystemId CheckedById,ei1.SystemId ApprovedById
                                    ,MRM.CheckedByStatus
	                                ,ei1.EmployeeName AS AuthorizedBy,MRM.OrderRefNo ,MRM.AuthorizedByStatus, MRM.ReqEmpId
		                            --,ei2.EmployeeName As AddedBy
                                FROM [TRN].[MaterialRequsitionMaster] MRM 
                                Left Join  [TRN].[MaterialRequsitionDetails] MRD  On MRM.Id=MRD.MaterialReqqusitionMasterId
                                Left Join org.Entity E on E.Id=MRM.EntityId
                                --Left JOin HKp.Activity A On A.Id=MRD.ActivityId
                                --Left Join MST.MaterialMaster MM on MM.Id=MRD.MaterialMasterId 
                                LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=mrm.CheckedBy
                                LEFT JOIN EmployeeInformation AS ei1 ON ei1.SystemId=mrm.AuthorizedBy
	                            --LEFT JOIN EmployeeInformation AS ei2 ON ei2.SystemId=mrm.AddedBy
                                Where  MRM.ReqEmpId='" + identity.EmployeeId + @"' AND MRM.CheckedByStatus is null	AND  MRM.AuthorizedByStatus='For Approval'                       
                        
	                            group by MRM.Id,MRM.RequisitionDate,MRM.RequisitionType,MRM.RequirmentType
	                            ,E.UserName,MRM.ReasonWhyItIsNotPlanEarlier ,ei.EmployeeName,ei1.EmployeeName
	                            ,MRM.CheckedByStatus,MRM.OrderRefNo ,MRM.AuthorizedByStatus, MRM.ReqEmpId ,ei.SystemId ,ei1.SystemId 
	
	                            )x
	                            group by x.Id
	                                ,x.RequisitionDate                                
                                    ,x.RequisitionType
	                                ,x.RequirmentType
	                                ,x.EntityName
                                    ,x.Reason 
	  
                                    ,x.CheckedBy
                                    ,x.CheckedByStatus
	                                ,x.AuthorizedBy,x.OrderRefNo ,x.AuthorizedByStatus, x.ReqEmpId,x.CheckedById,x.ApprovedById
                            Order By x.RequisitionDate ASC";
                }
                else if (ReqStatus == "HoldReject")
                {
                    sql = @"Select 
	                        MRM.Id
	                        ,REPLACE(CONVERT(CHAR(11), MRM.RequisitionDate, 106),' ','-') AS RequisitionDate ,REPLACE(CONVERT(CHAR(11), MRM.RequisitionDate, 106),' ','-') AS RequisitionDate1 
	                        ,MRM.RequisitionType
	                        ,MRM.RequirmentType
	                        ,E.UserName EntityName
                            ,MRM.ReasonWhyItIsNotPlanEarlier Reason
	                        ,SUM(MRD.TransactionQty) TransactionQty
	                        ,SUM(MRD.EstimatedRate) EstimatedRate
	                        ,SUM(MRD.TotalAmount) TotalAmount
                             ,ei.EmployeeName AS CheckedBy
							 ,MRM.CheckedByStatus,
	                        --,ei1.EmployeeName AS AuthorizedBy,
							MRM.CheckedHoldRejectReason,MRM.OrderRefNo ,MRM.AuthorizedByStatus
                        FROM [TRN].[MaterialRequsitionMaster] MRM 
                        Left Join  [TRN].[MaterialRequsitionDetails] MRD  On MRM.Id=MRD.MaterialReqqusitionMasterId
                        Left Join org.Entity E on E.Id=MRM.EntityId
                        LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=mrm.CheckedBy
                        LEFT JOIN EmployeeInformation AS ei1 ON ei1.SystemId=mrm.AuthorizedBy
                        --Where MRM.CheckedBy IS NOT NULL AND MRM.CheckedByStatus<>'Checked' AND MRM.AuthorizedByStatus IS NULL AND MRM.AuthorizedBy IS not null
                        Where MRM.CheckedBy IS NOT NULL 
						AND MRM.CheckedByStatus<>'Checked' 
						AND MRM.AuthorizedByStatus ='For Approval'
						AND MRM.AuthorizedBy IS not null
                        AND MRM.ReqEmpId='" + identity.EmployeeId + "' " +
                        "group by MRM.Id,MRM.RequisitionDate,MRM.RequisitionType,MRM.RequirmentType,E.UserName,MRM.ReasonWhyItIsNotPlanEarlier" +
                        ",ei.EmployeeName ,MRM.CheckedHoldRejectReason, MRM.CheckedByStatus,MRM.OrderRefNo ,MRM.AuthorizedByStatus" +
                        " Order By MRM.RequisitionDate DESC";
                }
                else
                {
                    sql = @"Select 
	                        MRM.Id
	                        ,REPLACE(CONVERT(CHAR(11), MRM.RequisitionDate, 106),' ','-') AS RequisitionDate ,REPLACE(CONVERT(CHAR(11), MRM.RequisitionDate, 106),' ','-') AS RequisitionDate1 
	                        ,MRM.RequisitionType
	                        ,MRM.RequirmentType
	                        ,E.UserName EntityName
                            ,MRM.ReasonWhyItIsNotPlanEarlier Reason
	                        --,A.UserName ActivityName
	                        --,MM.UserName MaterialName
	                        ,SUM(MRD.TransactionQty) TransactionQty
	                        ,SUM(MRD.EstimatedRate) EstimatedRate
	                        ,SUM(MRD.TotalAmount) TotalAmount
                            ,ei.EmployeeName AS CheckedBy
	                        ,ei1.EmployeeName AS AuthorizedBy
                            ,MRM.CheckedByStatus,MRM.OrderRefNo ,MRM.AuthorizedByStatus
                        FROM [TRN].[MaterialRequsitionMaster] MRM 
                        Left Join  [TRN].[MaterialRequsitionDetails] MRD  On MRM.Id=MRD.MaterialReqqusitionMasterId
                        Left Join org.Entity E on E.Id=MRM.EntityId
                        --Left JOin HKp.Activity A On A.Id=MRD.ActivityId
                        --Left Join MST.MaterialMaster MM on MM.Id=MRD.MaterialMasterId 
                        LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=mrm.CheckedBy
                        LEFT JOIN EmployeeInformation AS ei1 ON ei1.SystemId=mrm.AuthorizedBy
                        --Where MRM.CheckedBy IS NOT NULL AND MRM.CheckedByStatus='Checked'  AND MRM.AuthorizedByStatus IS NULL AND MRM.AuthorizedBy IS not null
                        Where MRM.CheckedBy IS NOT NULL  
                           AND MRM.CheckedByStatus='Checked'  
                           AND MRM.AuthorizedByStatus='For Approval'
                           AND MRM.AuthorizedBy IS not null                           
                           AND MRM.ReqEmpId='" + identity.EmployeeId + "' " +
                        "group by MRM.Id,MRM.RequisitionDate,MRM.RequisitionType,MRM.RequirmentType,E.UserName,MRM.ReasonWhyItIsNotPlanEarlier  " +
                        ",ei.EmployeeName ,ei1.EmployeeName ,MRM.CheckedByStatus,MRM.OrderRefNo ,MRM.AuthorizedByStatus " +
                        "Order By MRM.RequisitionDate DESC";

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

        public IEnumerable<object> GetAllReqdataDetails()//string ReqDetailId
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var _sql = @"SELECT IM.Id
                        --,IM.Id AS MaterialReqqusitionMasterId
                         ,IM.MaterialReqqusitionMasterId AS Id
                         ,IR.Id MaterialReqqusitionMasterId
                        , MGM.UserName AS MaterialGroupName
                        , IM.MaterialMasterId, MM.UserName AS MaterialName
                        , IM.ArticleId, ART.StandardName AS ArticleName
                        , IM.FirstCharacteristicsId, FC.UserName 
                        , IM.FirstCharacteristicsValueId , FCV.UserName AS SKU1
                        , IM.SecondCharacteristicsId, SC.UserName 
                        , IM.SecondCharacteristicsValueId, SCV.UserName AS SKU2
                        , IM.ThirdCharacteristicsId, TC.UserName 
                        , IM.ThirdCharacteristicsValueId , TCV.UserName AS SKU3
                        , ROUND(IM.TransactionQty,2) TransactionQty
                        , IM.TransactionUoMId
                        , TUoM.UserName AS TransactionUoM
                        , ROUND(IM.EstimatedRate,2) EstimatedRate 
                        , CU.Code AS CurrencyName
                        , ROUND((IM.TransactionQty * IM.EstimatedRate),2) AS TotalAmount   
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
                        ,IM.MaterialDetail
                        ,IM.PORcvQty PORaisedQty
						,Balance=(ROUND(IM.TransactionQty,2)-IM.PORcvQty)
						,RequisitionStatus=CASE WHEN IM.POQtyStatus=1 THEN 'Closed' ELSE 'Not Closed' END
                        --,isnull(pod.GRNRcvQty,0) GRNRcvQty
						--,pod.Id POdetailId
                          ,IR.OrderRefNo
                        FROM TRN.MaterialRequsitionDetails AS IM
                        LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
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
                        LEFT JOIN [HKP].[Activity] As Act On ACT.Id=IM.ActivityId
                        --Left join trn.PurchaseOrderDetail pod on pod.RequisitionDetailId=im.Id
                       --WHERE IM.MaterialReqqusitionMasterId
                       ";
                return _sqlRepository.GetDataCollection(_sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        public IEnumerable<object> GetAllReqdataDetailsById(string Id)//string ReqDetailId
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var _sql = @"SELECT ROW_NUMBER()  OVER (ORDER BY  IM.Id) as RowId
                         ,IM.Id AS Id
                         --,IR.MaterialReqqusitionMasterId AS Id
                         ,IR.Id MaterialReqqusitionMasterId
                        , MGM.UserName AS MaterialGroupName
                        , IM.MaterialMasterId, MM.UserName AS MaterialName
                        , IM.ArticleId, ART.StandardName AS ArticleName
                        , IM.FirstCharacteristicsId, FC.UserName 
                        , IM.FirstCharacteristicsValueId , FCV.UserName AS SKU1
                        , IM.SecondCharacteristicsId, SC.UserName 
                        , IM.SecondCharacteristicsValueId, SCV.UserName AS SKU2
                        , IM.ThirdCharacteristicsId, TC.UserName 
                        , IM.ThirdCharacteristicsValueId , TCV.UserName AS SKU3
                        , ROUND(IM.TransactionQty,2) TransactionQty
                        , IM.TransactionUoMId
                        , TUoM.UserName AS TransactionUoM
                        , ROUND(IM.EstimatedRate,2) EstimatedRate 
                        , CU.Code AS CurrencyName
                        , ROUND((IM.TransactionQty * IM.EstimatedRate),2) AS TotalAmount   
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
                        ,IM.MaterialDetail--,IM.ApprovedQty
                        FROM TRN.MaterialRequsitionDetails AS IM
                        LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
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
                        LEFT JOIN [HKP].[Activity] As Act On ACT.Id=IM.ActivityId
                       WHERE IM.MaterialReqqusitionMasterId ='" + Id + @"'
                       ";
                return _sqlRepository.GetDataCollection(_sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        public IEnumerable<object> GetAllReqdata1(string ReqStatusApproval)
        {
            try
            {
                var sql = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (ReqStatusApproval == "HoldReject")
                {
                    sql = @"Select 
	                        MRM.Id
	                        ,REPLACE(CONVERT(CHAR(11), MRM.RequisitionDate, 106),' ','-') AS RequisitionDate ,REPLACE(CONVERT(CHAR(11), MRM.RequisitionDate, 106),' ','-') AS RequisitionDate1 
	                        ,MRM.RequisitionType
	                        ,MRM.RequirmentType
	                        ,E.UserName EntityName,MRM.ReasonWhyItIsNotPlanEarlier Reason
	                        --,A.UserName ActivityName
	                        --,MM.UserName MaterialName
	                        ,SUM(MRD.TransactionQty) TransactionQty
	                        ,SUM(MRD.EstimatedRate) EstimatedRate
	                        ,SUM(MRD.TotalAmount) TotalAmount
                            ,ei.EmployeeName AS CheckedBy
	                         ,ei1.EmployeeName AS AuthorizedBy
							,MRM.CheckedByStatus
							,MRM.AuthorizedByStatus
							,AuthorizedBy=CASE WHEN MRM.AuthorizedByStatus='Hold' THEN '' 
								  WHEN MRM.AuthorizedByStatus='Reject' THEN ''
								  ELSE ei1.EmployeeName END
					    ,MRM.ApprovedHoldRejectReason,MRM.OrderRefNo
                        FROM [TRN].[MaterialRequsitionMaster] MRM 
                        Left Join  [TRN].[MaterialRequsitionDetails] MRD  On MRM.Id=MRD.MaterialReqqusitionMasterId
                        Left Join org.Entity E on E.Id=MRM.EntityId
                        --Left JOin HKp.Activity A On A.Id=MRD.ActivityId
                        --Left Join MST.MaterialMaster MM on MM.Id=MRD.MaterialMasterId 
                        LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=mrm.CheckedBy
                        LEFT JOIN EmployeeInformation AS ei1 ON ei1.SystemId=mrm.AuthorizedBy
                        --Where MRM.CheckedByStatus='Checked' And MRM.AuthorizedByStatus<>'Approval'  
                        Where MRM.CheckedByStatus='Checked' 
                        And MRM.AuthorizedByStatus<>'Approved' 
                        And MRM.AuthorizedByStatus<>'For Approval' 
                        AND MRM.ReqEmpId='" + identity.EmployeeId + @"'
                        group by MRM.Id
	                        ,MRM.RequisitionDate
	                        ,MRM.RequisitionType
	                        ,MRM.RequirmentType
                            ,MRM.CheckedByStatus
	                        ,E.UserName ,MRM.ReasonWhyItIsNotPlanEarlier
	                        --,A.UserName ActivityName
	                        --,MM.UserName 
                            ,ei.EmployeeName
	                        ,ei1.EmployeeName,MRM.AuthorizedByStatus,MRM.ApprovedHoldRejectReason,MRM.OrderRefNo
                        Order By MRM.Id Desc";

                }
                else
                {
                    
                    sql = @"select x.Id
	                                ,REPLACE(CONVERT(CHAR(11), x.RequisitionDate, 106),' ','-') AS RequisitionDate ,REPLACE(CONVERT(CHAR(11), x.RequisitionDate, 106),' ','-') AS RequisitionDate1 
	                                ,x.RequisitionType
	                                ,x.RequirmentType
	                                ,x.EntityName
                                    ,x.Reason
	                                --,A.UserName ActivityName
	                                --,MM.UserName MaterialName
	                                ,SUM(x.TransactionQty) TransactionQty
	                                ,SUM(x.EstimatedRate) EstimatedRate
	                                ,SUM(x.TotalAmount) TotalAmount
                                    ,x.CheckedBy
                                    ,x.CheckedByStatus
	                                ,x.AuthorizedBy,x.OrderRefNo ,x.AuthorizedByStatus, x.ReqEmpId from 
                            (


                            Select 
	                                MRM.Id
	                                ,REPLACE(CONVERT(CHAR(11), MRM.RequisitionDate, 106),' ','-') AS RequisitionDate 
	                                ,MRM.RequisitionType
	                                ,MRM.RequirmentType
	                                ,E.UserName EntityName,MRM.ReasonWhyItIsNotPlanEarlier Reason
	                                --,A.UserName ActivityName
	                                --,MM.UserName MaterialName
	                                ,SUM(MRD.TransactionQty) TransactionQty
	                                ,SUM(MRD.EstimatedRate) EstimatedRate
	                                ,SUM(MRD.TotalAmount) TotalAmount
                                    ,SUM(pod.TransactionQty) POQty
	                                ,SUM(ird.TransactionQty) GRNQty
	                                ,SUM(ga.TransactionQty) allowcationQty
                                    ,ei.EmployeeName AS CheckedBy
	                                    ,ei1.EmployeeName AS AuthorizedBy
		                            ,MRM.CheckedByStatus
		                            ,MRM.AuthorizedByStatus
		                            ,MRM.OrderRefNo,MRM.ReqEmpId
                                FROM [TRN].[MaterialRequsitionMaster] MRM 
                                Left Join  [TRN].[MaterialRequsitionDetails] MRD  On MRM.Id=MRD.MaterialReqqusitionMasterId
                                LEFT JOIN trn.PurchaseOrderDetail AS pod ON pod.RequisitionDetailId=mrd.Id AND pod.RequisitionId=Mrm.Id
                                LEFT JOIN trn.InventoryReceiveDetail AS ird ON ird.RequisitionId=mrm.id AND ird.RequisitionDetailId=mrd.Id AND ird.POId=pod.Id
                                LEFT JOIN trn.GRNPORequisitionAllocation AS ga ON ga.InventoryReceiveDetailId=ird.InventoryReceiveId AND ga.POReqDetailsID=pod.Id
                                Left Join org.Entity E on E.Id=MRM.EntityId
                                --Left JOin HKp.Activity A On A.Id=MRD.ActivityId
                                --Left Join MST.MaterialMaster MM on MM.Id=MRD.MaterialMasterId 
                                    LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=mrm.CheckedBy
                                LEFT JOIN EmployeeInformation AS ei1 ON ei1.SystemId=mrm.AuthorizedBy
                                    Where MRM.Id in(Select distinct RequisitionId from trn.purchaseOrderDetail where RequisitionId is not null)--and RequisitionId='110232'
	                             AND MRM.CheckedByStatus IS NULL
                                AND MRM.AuthorizedByStatus IS NULL
                                AND MRM.ReqEmpId='" + identity.EmployeeId + @"'
                                group by MRM.Id
	                                ,MRM.RequisitionDate
	                                ,MRM.RequisitionType
	                                ,MRM.RequirmentType
                                    ,MRM.CheckedByStatus
	                                ,E.UserName ,MRM.ReasonWhyItIsNotPlanEarlier
	                                --,A.UserName ActivityName
	                                --,MM.UserName
                                        ,ei.EmployeeName
	                                ,ei1.EmployeeName,MRM.AuthorizedByStatus,MRM.OrderRefNo,MRM.ReqEmpId
                                -- Order By MRM.Id Desc
                                UNION ALL

                                Select 
	                                MRM.Id
	                                ,REPLACE(CONVERT(CHAR(11), MRM.RequisitionDate, 106),' ','-') AS RequisitionDate 
	                                ,MRM.RequisitionType
	                                ,MRM.RequirmentType
	                                ,E.UserName EntityName,MRM.ReasonWhyItIsNotPlanEarlier Reason
	                                --,A.UserName ActivityName
	                                --,MM.UserName MaterialName
	                                ,SUM(MRD.TransactionQty) TransactionQty
	                                ,SUM(MRD.EstimatedRate) EstimatedRate
	                                ,SUM(MRD.TotalAmount) TotalAmount
                                    ,SUM(pod.TransactionQty) POQty
	                                ,SUM(ird.TransactionQty) GRNQty
	                                ,SUM(ga.TransactionQty) allowcationQty
                                    ,ei.EmployeeName AS CheckedBy
	                                ,ei1.EmployeeName AS AuthorizedBy
		                            ,MRM.CheckedByStatus
		                            ,MRM.AuthorizedByStatus		
		                            ,MRM.OrderRefNo,MRM.ReqEmpId
                                FROM [TRN].[MaterialRequsitionMaster] MRM 
                                Left Join  [TRN].[MaterialRequsitionDetails] MRD  On MRM.Id=MRD.MaterialReqqusitionMasterId
                                LEFT JOIN trn.PurchaseOrderDetail AS pod ON pod.RequisitionDetailId=mrd.Id AND pod.RequisitionId=Mrm.Id
                                LEFT JOIN trn.InventoryReceiveDetail AS ird ON ird.RequisitionId=mrm.id AND ird.RequisitionDetailId=mrd.Id AND ird.POId=pod.Id
                                LEFT JOIN trn.GRNPORequisitionAllocation AS ga ON ga.InventoryReceiveDetailId=ird.InventoryReceiveId AND ga.POReqDetailsID=pod.Id
                                Left Join org.Entity E on E.Id=MRM.EntityId
                                --Left JOin HKp.Activity A On A.Id=MRD.ActivityId
                                --Left Join MST.MaterialMaster MM on MM.Id=MRD.MaterialMasterId 
                                    LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=mrm.CheckedBy
                                LEFT JOIN EmployeeInformation AS ei1 ON ei1.SystemId=mrm.AuthorizedBy
                                    Where --MRM.Id in(Select distinct RequisitionId from trn.purchaseOrderDetail where RequisitionId is not null)--and RequisitionId='110232'
	                             MRM.CheckedByStatus  Is null
                                AND MRM.AuthorizedByStatus='Approved'
                                AND MRM.ReqEmpId='" + identity.EmployeeId + @"'
                                group by MRM.Id
	                                ,MRM.RequisitionDate
	                                ,MRM.RequisitionType
	                                ,MRM.RequirmentType
                                    ,MRM.CheckedByStatus
	                                ,E.UserName ,MRM.ReasonWhyItIsNotPlanEarlier
	                                --,A.UserName ActivityName
	                                --,MM.UserName
                                        ,ei.EmployeeName
	                                ,ei1.EmployeeName,MRM.AuthorizedByStatus,MRM.OrderRefNo,MRM.ReqEmpId
                                -- Order By MRM.Id Desc

  
                                UNION ALL

                                Select 
	                                MRM.Id
	                                ,REPLACE(CONVERT(CHAR(11), MRM.RequisitionDate, 106),' ','-') AS RequisitionDate 
	                                ,MRM.RequisitionType
	                                ,MRM.RequirmentType
	                                ,E.UserName EntityName,MRM.ReasonWhyItIsNotPlanEarlier Reason
	                                --,A.UserName ActivityName
	                                --,MM.UserName MaterialName
	                                ,SUM(MRD.TransactionQty) TransactionQty
	                                ,SUM(MRD.EstimatedRate) EstimatedRate
	                                ,SUM(MRD.TotalAmount) TotalAmount
                                    ,SUM(pod.TransactionQty) POQty
	                                ,SUM(ird.TransactionQty) GRNQty
	                                ,SUM(ga.TransactionQty) allowcationQty
                                    ,ei.EmployeeName AS CheckedBy
	                                    ,ei1.EmployeeName AS AuthorizedBy
		                            ,MRM.CheckedByStatus
		                            ,MRM.AuthorizedByStatus
		                            ,MRM.OrderRefNo,MRM.ReqEmpId
                                FROM [TRN].[MaterialRequsitionMaster] MRM 
                                Left Join  [TRN].[MaterialRequsitionDetails] MRD  On MRM.Id=MRD.MaterialReqqusitionMasterId
                                LEFT JOIN trn.PurchaseOrderDetail AS pod ON pod.RequisitionDetailId=mrd.Id AND pod.RequisitionId=Mrm.Id
                                LEFT JOIN trn.InventoryReceiveDetail AS ird ON ird.RequisitionId=mrm.id AND ird.RequisitionDetailId=mrd.Id AND ird.POId=pod.Id
                                LEFT JOIN trn.GRNPORequisitionAllocation AS ga ON ga.InventoryReceiveDetailId=ird.InventoryReceiveId AND ga.POReqDetailsID=pod.Id
                                Left Join org.Entity E on E.Id=MRM.EntityId
                                --Left JOin HKp.Activity A On A.Id=MRD.ActivityId
                                --Left Join MST.MaterialMaster MM on MM.Id=MRD.MaterialMasterId 
                                    LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=mrm.CheckedBy
                                LEFT JOIN EmployeeInformation AS ei1 ON ei1.SystemId=mrm.AuthorizedBy
                                    Where --MRM.Id in(Select distinct RequisitionId from trn.purchaseOrderDetail where RequisitionId is not null)--and RequisitionId='110232'
	                             MRM.CheckedByStatus='Checked'
                                AND MRM.AuthorizedByStatus='Approved'
                                AND MRM.ReqEmpId='" + identity.EmployeeId + @"'
                                group by MRM.Id
	                                ,MRM.RequisitionDate
	                                ,MRM.RequisitionType
	                                ,MRM.RequirmentType
                                    ,MRM.CheckedByStatus
	                                ,E.UserName ,MRM.ReasonWhyItIsNotPlanEarlier
	                                --,A.UserName ActivityName
	                                --,MM.UserName
                                        ,ei.EmployeeName
	                                ,ei1.EmployeeName,MRM.AuthorizedByStatus,MRM.OrderRefNo,MRM.ReqEmpId
                                -- Order By MRM.Id Desc
                                )x
	                            group by x.Id
	                                ,x.RequisitionDate
	                                ,x.RequisitionType
	                                ,x.RequirmentType
	                                ,x.EntityName
                                    ,x.Reason 
                                    ,x.CheckedBy
                                    ,x.CheckedByStatus
	                                ,x.AuthorizedBy,x.OrderRefNo ,x.AuthorizedByStatus, x.ReqEmpId
                            Order By x.RequisitionDate DESC";

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
        public IEnumerable<object> GetReqMaster(string Id)
        {
            try
            {
                var sql = @"Select 
                            MRM.Id
                            ,MRM.RequisitionDate
                            ,MRM.RequisitionType
                            ,MRM.RequirmentType
                            ,MRM.QualityApprovalResponsiblePersonId
                            ,EI1.EmployeeName AS ResponsiblePersonName
                            ,MRM.NeedSpecialAppId
                            ,EI.EmployeeName AS EmployeeName
                            ,E.UserName EntityName
                            ,MRM.EntityId
                            ,MRM.ReasonWhyItIsNotPlanEarlier
                            ,MRM.AddedBy
                            ,MRM.AddedDate
                            ,MRM.AddedFromIP
                            ,MRM.UpdatedBy
                            ,MRM.UpdatedDate
                            ,MRM.UpdatedFromIP
                            ,MRM.RequisitionDate
                            ,MRM.Remarks
                            ,MRM.CheckedBy
                            ,MRM.CheckedByStatus
                            ,MRM.AuthorizedBy
                            ,MRM.AuthorizedByStatus
                            ,MRM.IsApproved
                            ,A.UserName ActivityName
                            ,MM.UserName MaterialName
                            ,MRD.TransactionQty
                            ,MRD.EstimatedRate
                            ,MRD.TotalAmount
                            FROM [TRN].[MaterialRequsitionMaster] MRM
                            Left Join [TRN].[MaterialRequsitionDetails] MRD On MRD.MaterialReqqusitionMasterId=MRM.Id
                            Left Join org.Entity E on E.Id=MRM.EntityId
                            LEFT JOin HKp.Activity A On A.Id=MRD.ActivityId
                            Left Join MST.MaterialMaster MM on MM.Id=MRD.MaterialMasterId 
                            LEFT JOIN dbo.EmployeeInformation EI On EI.SystemId=MRM.NeedSpecialAppId
                            LEFT JOIN dbo.EmployeeInformation EI1 On EI1.SystemId=MRM.QualityApprovalResponsiblePersonId
                                                where MRM.Id='" + Id + @"'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public GridModel QueryForPurchaseOrderDetail(GridParameter parameters, string inveReveiveId)
        {
            try
            {
                parameters.CmdText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"'	                                                 
                          SELECT IM.Id
                        ,IM.Id AS MaterialReqqusitionMasterId
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
                        , ROUND(IM.EstimatedRate,2) EstimatedRate 
                        , CU.Code AS CurrencyName
                        , ROUND((IM.TransactionQty * IM.EstimatedRate),2) AS TotalAmount   
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
                        LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
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
                        LEFT JOIN [HKP].[Activity] As Act On ACT.Id=IM.ActivityId
                        --JOIN [HKP].Budget
                        --JOIN [HKP].Gl
                        WHERE IM.MaterialReqqusitionMasterId=@inventoryReceiveId";
                return _sqlRepository.GetDifferentGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetInventoryMaterialListById(string inveReveiveId)
        {
            try
            {
                var _sql = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"'	                                                 
                          SELECT IM.Id
                        ,IR.Id AS MaterialReqqusitionMasterId
                        , MGM.UserName AS MaterialGroupMasterName
                        , IM.MaterialMasterId, MM.UserName AS MaterialMasterName
                        , IM.ArticleId, ART.StandardName AS ArticleName
                        , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                        , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                        , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                        , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                        , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                        , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
                        , ROUND(IM.TransactionQty,2) TransactionQty
                        , IM.TransactionUoMId
                        , TUoM.UserName AS TransactionUoM
                        , ROUND(IM.EstimatedRate,2) EstimatedRate 
                        , CU.Code AS CurrencyName
                        ,CU.Id AS CurrencyId
                        , ROUND((IM.TransactionQty * IM.EstimatedRate),2) AS TotalAmount   
                        ,IM.MaterialDetail
                        ,Replace(CONVERT(VARCHAR(11), IM.DeliveryDate, 106), ' ', '-') DeliveryDate
                        ,Act.Id As ActivityId
                        ,Act.UserName As ActivityName
                        ,IM.BudgetType
                        ,IM.Reason
                        ,IM.Remarks
                        ,IM.FutureReqApp
                        --,BudgetMasterId
                        --,GLGeneralInfoId
                         ,IM.QualityApprovalResponsiblePersonId
						,IM.NeedSpecialAppId
                        FROM TRN.MaterialRequsitionDetails AS IM
                        LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
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
                        LEFT JOIN [HKP].[Activity] As Act On ACT.Id=IM.ActivityId
                        --JOIN [HKP].Budget
                        --JOIN [HKP].Gl
                        WHERE IM.Id=@inventoryReceiveId";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public void UpdateMaterial(IEnumerable<MaterialRequisitionDetailViewModel> entity, IEnumerable<PurchaseOrderTax> receiveTaxList)
        {
            try
            {
                

                if (entity.IsNotNull())
                {
                    // var currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId='{entity.InventoryReceiveDetailId}'").First();
                    foreach (var item1 in entity)
                    {
                        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                        var ip = identity.IPAddress;
                        var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                        var UpdatedBy = identity.Name;
                        var ReqDetailId = item1.Id;
                        
                        var _sql = "UPDATE [TRN].[MaterialRequsitionDetails]   SET [MaterialDetail] = '" + item1.MaterialDetail + "',[TransactionQty] =  '" + Convert.ToDecimal(item1.TransactionQty) + "',[EstimatedRate] = '" + Convert.ToDecimal(item1.EstimatedRate) + "',[TotalAmount] = '" + Convert.ToDecimal(item1.TotalAmount) + "',[UpdatedBy] = '" + identity.UserId + "',[UpdatedDate] = '" + Convert.ToDateTime(DateTime.Now) + "',[UpdatedFromIP] = '" + identity.IPAddress + "' where id = '" + ReqDetailId + "'";
                        _sqlRepository.ExecuteSqlCommand(_sql);
                    }
                }
                
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }


        public void DeleteReqDetails(string id)
        {
            try
            {
               
                var data = _materialRequsitionDetailsRepository.Find(id);
                if (data.IsNull()) throw new CustomException(ServiceResources.RecordNoLonger);
                _materialRequsitionDetailsRepository.Delete(data.Id);
                _unitOfWork.SaveChanges();
                
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public void DeleteReq(string id)
        {
            try
            {
                var detail = Convert.ToBoolean(_materialRequsitionDetailsRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM TRN.MaterialRequsitionDetails WHERE MaterialReqqusitionMasterId='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                if (!detail)
                {
                    var data = base.Find(id);
                    if (data.IsNull()) throw new CustomException(ServiceResources.RecordNoLonger);
                    base.Delete(data);
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

        #endregion

        #region MailSend Function
        public void DailySendMailRequisitionCheck(string CheckedBy, string ip, string appVersion,string requisitionNo,string RequirmentType,string RequisitionType, string AddedBy) 
        {
            var log = new MailLog
            {
                AddedBy = "",
                AddedDate = DateTime.Now,
                AddedFromIP = "",
                AppVersion = "",
                CompanyGroupId = "",
                ModelState = ModelState.Added,
                RecordTime = DateTime.Now,
                ServiceName = "Requisition",
                UserId = null,
                AttachmentName = "N/A",
                IsSuccess = false,
                MailGenerator = MailGenerator.Scheduler.ToString(),
                Remarks = "Requisition To be Checked",
                Subject = "Requisition To be Checked"
            };

            try
            {
                var emailList = GetAdministrativeMailList();
                var companyGroupList = _companyGroupRepository.Query(r => r.Active && !r.Archive).Select().ToList();
                var companyList = _companyRepository.Query(r => r.Active && !r.Archive).Select().ToList();
                foreach (var companyGroup in companyGroupList)
                {
                    log.CompanyGroupId = companyGroup.Id;
                    var MyAppLogin = _materialRequsitionDetailsRepository.SqlQuery<string>("SELECT MyAppURL from ORG.CompanyGroup where id='" + companyGroup.Id + "'").First();
                    var smtpConfigurationCG = _smtpConfigurationService.Query(r => r.CompanyGroupId == companyGroup.Id).Select().FirstOrDefault();
                    var email = new EmailSender(smtpConfigurationCG.Host, smtpConfigurationCG.Port, smtpConfigurationCG.MailingUserName, smtpConfigurationCG.Password, true);
                    var ccList = string.Join(";", emailList.Where(r => r.Active && r.MailType == "Cc" && r.Email != string.Empty).Select(r => r.FullName + "<" + r.Email + ">"));
                    log.CcList = ccList;
                    var bccList = string.Join(";", emailList.Where(r => r.Active && r.MailType == "Bcc" && r.Email != string.Empty).Select(r => r.FullName + "<" + r.Email + ">"));
                    log.BccList = bccList;
                    var inActiveList = string.Join(";", emailList.Where(r => !r.Active).Select(r => r.MailType + ":" + r.FullName));
                    foreach (var company in companyList.Where(r => r.CompanyGroupId == companyGroup.Id))
                    {
                        var employeeList = _employeeInformationRepository.Query(r => r.GroupID == company.CompanyGroupId && r.CompanyId == company.Id && r.EmployeeStatus == "Active" && r.SystemId == CheckedBy).Select().ToList();
                        var AddedByName = _employeeInformationRepository.Query(r => r.GroupID == company.CompanyGroupId && r.CompanyId == company.Id && r.EmployeeStatus == "Active" && r.SystemId == AddedBy).Select().ToList();


                        foreach (var employee in employeeList)
                        {
                            var birthDate = Convert.ToDateTime(employee.BirthdayCelebrationDate);
                            var bdate = birthDate.Day.ToString();
                            var bmonth = birthDate.Month.ToString();
                            var byear = birthDate.Year.ToString();

                            var toDay = DateTime.Now;
                            var bToday = Convert.ToDateTime(toDay.ToString());

                            var tdate = bToday.Day.ToString();
                            var tmonth = bToday.Month.ToString();
                            var tyear = bToday.Year.ToString();

                            if (!string.IsNullOrEmpty(CheckedBy))
                            {
                                log.SenderEmail = smtpConfigurationCG.SenderSystemEmail;
                                log.SenderName = smtpConfigurationCG.SenderSystemName;

                                var birthDayList = employee.EmployeeName;
                                if (!string.IsNullOrEmpty(employee.EmailId))
                                {
                                    try
                                    {
                                        var message = email.PrepareMessage(log.SenderName + "<" + log.SenderEmail + ">",
                                            employee.EmployeeName + "<" + employee.EmailId + ">", ccList, bccList, "Requisition To be Checked",
                                             //"<b>Dear Sir, " + employee.EmployeeName + ".</b><br><br> Wishing you a wonderful year of<br> good health, happiness and success!<br><br> From <br>" + companyGroup.UserName + " Family");
                                             "<b>Dear Sir,</b><br><br>" +
                                             "You have a Checked request of purchase requisition from Mr. " + AddedByName[0].EmployeeName.ToString() + ".Please go to the portal for checking. Please see the following requisition information.<br><br> Requisition No:" + requisitionNo +
                                             "<br> Requisition Type:" + RequisitionType  +
                                             "<br> Requirment Type:" + RequirmentType+
                                              "<br><br> Thanking you,<br><br> " + smtpConfigurationCG.SenderSystemName + ".<br><br><br><a href=" + MyAppLogin + ">Please Login Here</a>");

                                        //"<b>Please Checked This Requisition No:" + requisitionNo);
                                        //,string RequirmentType,string RequisitionType,string Requisitiondate
                                        if (message != null)
                                        {
                                            email.Send(message);
                                            log.ToList = employee.EmailId;
                                            log.CcList = ccList;
                                            log.BccList = bccList;
                                            log.IsSuccess = true;
                                            log.HasAttachment = false;
                                            log.Remarks = "Requisition To be Checked" + employee.EmployeeName + " has been send Successfully";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        log.IsSuccess = false;
                                        log.HasAttachment = false;
                                        log.Remarks = ex.Message;
                                        continue;
                                    }
                                }
                                else
                                {
                                    log.IsSuccess = false;
                                    log.HasAttachment = false;
                                    log.Remarks = "EMail Id not Found";
                                }
                            }
                        }
                    }
                }
                _mailLogRepository.Insert(log);
                _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                log.Remarks = ex.Message;
                _mailLogRepository.Insert(log);
                _unitOfWork.SaveChanges();
                throw;
            }
        }

        public void DailySendMailRequisitionApproved(string addedBy, string ip, string appVersion, string RequisitionType, string RequirmentType, string CheckedBy, string ApprovedBy, string PoId,string PreparedBY)
        {
            var log = new MailLog
            {
                AddedBy = "",
                AddedDate = DateTime.Now,
                AddedFromIP = "",
                AppVersion = "",
                CompanyGroupId = "",
                ModelState = ModelState.Added,
                RecordTime = DateTime.Now,
                ServiceName = "Requisition",
                UserId = null,
                AttachmentName = "N/A",
                IsSuccess = false,
                MailGenerator = MailGenerator.Scheduler.ToString(),
                Remarks = "Requisition To be Approved",
                Subject = "Requisition To be Approved"
            };

            try
            {
                var emailList = GetAdministrativeMailList();
                var companyGroupList = _companyGroupRepository.Query(r => r.Active && !r.Archive).Select().ToList();
                var companyList = _companyRepository.Query(r => r.Active && !r.Archive).Select().ToList();
                foreach (var companyGroup in companyGroupList)
                {
                    log.CompanyGroupId = companyGroup.Id;
                    var MyAppLogin = _materialRequsitionDetailsRepository.SqlQuery<string>("SELECT MyAppURL from ORG.CompanyGroup where id='"+ companyGroup.Id + "'").First();

                    var smtpConfigurationCG = _smtpConfigurationService.Query(r => r.CompanyGroupId == companyGroup.Id).Select().FirstOrDefault();
                    var email = new EmailSender(smtpConfigurationCG.Host, smtpConfigurationCG.Port, smtpConfigurationCG.MailingUserName, smtpConfigurationCG.Password, true);
                    var ccList = string.Join(";", emailList.Where(r => r.Active && r.MailType == "Cc" && r.Email != string.Empty).Select(r => r.FullName + "<" + r.Email + ">"));
                    log.CcList = ccList;
                    var bccList = string.Join(";", emailList.Where(r => r.Active && r.MailType == "Bcc" && r.Email != string.Empty).Select(r => r.FullName + "<" + r.Email + ">"));
                    log.BccList = bccList;
                    var inActiveList = string.Join(";", emailList.Where(r => !r.Active).Select(r => r.MailType + ":" + r.FullName));
                    foreach (var company in companyList.Where(r => r.CompanyGroupId == companyGroup.Id))
                    {
                        var employeeList = _employeeInformationRepository.Query(r => r.GroupID == company.CompanyGroupId && r.CompanyId == company.Id && r.EmployeeStatus == "Active" && r.SystemId == ApprovedBy).Select().ToList();
                        var AddedByName = _employeeInformationRepository.Query(r => r.GroupID == company.CompanyGroupId && r.CompanyId == company.Id && r.EmployeeStatus == "Active" && r.SystemId == CheckedBy).Select().ToList();


                        foreach (var employee in employeeList)
                        {
                            var birthDate = Convert.ToDateTime(employee.BirthdayCelebrationDate);
                            var bdate = birthDate.Day.ToString();
                            var bmonth = birthDate.Month.ToString();
                            var byear = birthDate.Year.ToString();

                            var toDay = DateTime.Now;
                            var bToday = Convert.ToDateTime(toDay.ToString());

                            var tdate = bToday.Day.ToString();
                            var tmonth = bToday.Month.ToString();
                            var tyear = bToday.Year.ToString();

                            if (!string.IsNullOrEmpty(CheckedBy))
                            {
                                log.SenderEmail = smtpConfigurationCG.SenderSystemEmail;
                                log.SenderName = smtpConfigurationCG.SenderSystemName;

                                var birthDayList = employee.EmployeeName;
                                if (!string.IsNullOrEmpty(employee.EmailId))
                                {
                                    try
                                    {
                                        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;                                      
                                        var message = email.PrepareMessage(log.SenderName + "<" + log.SenderEmail + ">",
                                        employee.EmployeeName + "<" + employee.EmailId + ">", ccList, bccList, "Requisition To be Approved",
                                            //"<b>Dear Sir, " + employee.EmployeeName + ".</b><br><br> Wishing you a wonderful year of<br> good health, happiness and success!<br><br> From <br>" + companyGroup.UserName + " Family");
                                            "<b>Dear Sir,</b><br><br>" +
                                            "You have an Approval request of purchase requisition from Mr. " + PreparedBY + " Which is Checked By Mr. " + AddedByName[0].EmployeeName.ToString() + ".Please go to the portal for Approving. Please see the following requisition information.<br><br> Requisition No:" + PoId +
                                            "<br> Requisition Type:" + RequisitionType +
                                            "<br> Requirment Type:" + RequirmentType +
                                            "<br><br> Thanking you,<br><br> "+ smtpConfigurationCG.SenderSystemName + ".<br><br><br><a href="+MyAppLogin+">Please Login Here</a>");

                                        //"<b>Please Checked This Requisition No:" + requisitionNo);
                                        //,string RequirmentType,string RequisitionType,string Requisitiondate
                                        if (message != null)
                                        {
                                            email.Send(message);
                                            log.ToList = employee.EmailId;
                                            log.CcList = ccList;
                                            log.BccList = bccList;
                                            log.IsSuccess = true;
                                            log.HasAttachment = false;
                                            log.Remarks = "Requisition To be Approved" + employee.EmployeeName + " has been send Successfully";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        log.IsSuccess = false;
                                        log.HasAttachment = false;
                                        log.Remarks = ex.Message;
                                        continue;
                                    }
                                }
                                else
                                {
                                    log.IsSuccess = false;
                                    log.HasAttachment = false;
                                    log.Remarks = "EMail Id not Found";
                                }
                            }
                        }
                    }
                }
                _mailLogRepository.Insert(log);
                _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                log.Remarks = ex.Message;
                _mailLogRepository.Insert(log);
                _unitOfWork.SaveChanges();
                throw;
            }
        }


        public void DailySendMailRequisitionCreator(string addedBy, string ip, string appVersion, string RequisitionType, string RequirmentType, string CheckedBy, string ApprovedBy, string PoId, string PreparedBY, string PreparedBYId)
        {
            var log = new MailLog
            {
                AddedBy = "",
                AddedDate = DateTime.Now,
                AddedFromIP = "",
                AppVersion = "",
                CompanyGroupId = "",
                ModelState = ModelState.Added,
                RecordTime = DateTime.Now,
                ServiceName = "Requisition",
                UserId = null,
                AttachmentName = "N/A",
                IsSuccess = false,
                MailGenerator = MailGenerator.Scheduler.ToString(),
                Remarks = "Requisition has been  approved",
                Subject = "Requisition has been  approved"
            };

            try
            {
                var emailList = GetAdministrativeMailList();
                var companyGroupList = _companyGroupRepository.Query(r => r.Active && !r.Archive).Select().ToList();
                var companyList = _companyRepository.Query(r => r.Active && !r.Archive).Select().ToList();
                foreach (var companyGroup in companyGroupList)
                {
                    log.CompanyGroupId = companyGroup.Id;
                    var MyAppLogin = _materialRequsitionDetailsRepository.SqlQuery<string>("SELECT MyAppURL from ORG.CompanyGroup where id='" + companyGroup.Id + "'").First();

                    var smtpConfigurationCG = _smtpConfigurationService.Query(r => r.CompanyGroupId == companyGroup.Id).Select().FirstOrDefault();
                    var email = new EmailSender(smtpConfigurationCG.Host, smtpConfigurationCG.Port, smtpConfigurationCG.MailingUserName, smtpConfigurationCG.Password, true);
                    var ccList = string.Join(";", emailList.Where(r => r.Active && r.MailType == "Cc" && r.Email != string.Empty).Select(r => r.FullName + "<" + r.Email + ">"));
                    log.CcList = ccList;
                    var bccList = string.Join(";", emailList.Where(r => r.Active && r.MailType == "Bcc" && r.Email != string.Empty).Select(r => r.FullName + "<" + r.Email + ">"));
                    log.BccList = bccList;
                    var inActiveList = string.Join(";", emailList.Where(r => !r.Active).Select(r => r.MailType + ":" + r.FullName));
                    foreach (var company in companyList.Where(r => r.CompanyGroupId == companyGroup.Id))
                    {
                        var employeeList = _employeeInformationRepository.Query(r => r.GroupID == company.CompanyGroupId && r.CompanyId == company.Id && r.EmployeeStatus == "Active" && r.SystemId == PreparedBYId).Select().ToList();
                        var AddedByName = _employeeInformationRepository.Query(r => r.GroupID == company.CompanyGroupId && r.CompanyId == company.Id && r.EmployeeStatus == "Active" && r.SystemId == CheckedBy).Select().ToList();


                        foreach (var employee in employeeList)
                        {
                            var birthDate = Convert.ToDateTime(employee.BirthdayCelebrationDate);
                            var bdate = birthDate.Day.ToString();
                            var bmonth = birthDate.Month.ToString();
                            var byear = birthDate.Year.ToString();

                            var toDay = DateTime.Now;
                            var bToday = Convert.ToDateTime(toDay.ToString());

                            var tdate = bToday.Day.ToString();
                            var tmonth = bToday.Month.ToString();
                            var tyear = bToday.Year.ToString();

                            if (!string.IsNullOrEmpty(PreparedBYId))
                            {
                                log.SenderEmail = smtpConfigurationCG.SenderSystemEmail;
                                log.SenderName = smtpConfigurationCG.SenderSystemName;

                                var birthDayList = employee.EmployeeName;
                                if (!string.IsNullOrEmpty(employee.EmailId))
                                {
                                    try
                                    {
                                        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                                        var message = email.PrepareMessage(log.SenderName + "<" + log.SenderEmail + ">",
                                        employee.EmployeeName + "<" + employee.EmailId + ">", ccList, bccList, "Requisition has been approved",
                                            //"<b>Dear Sir, " + employee.EmployeeName + ".</b><br><br> Wishing you a wonderful year of<br> good health, happiness and success!<br><br> From <br>" + companyGroup.UserName + " Family");
                                            "<b>Dear Sir,</b><br><br>" +
                                            //"You have an Approval request of purchase requisition from Mr. " + PreparedBY + " Which is Checked By Mr. " + AddedByName[0].EmployeeName.ToString() + ".Please go to the portal for Approving. Please see the following requisition information.<br><br> Requisition No:" + PoId +
                                            "Your requisition has been  approved" +                                          
                                            "<br> Requisition No:" + PoId +
                                            "<br> Requisition Type:" + RequisitionType +
                                            "<br> Requirment Type:" + RequirmentType +
                                            "<br><br> Thanking you,<br><br> " + smtpConfigurationCG.SenderSystemName + ".<br><br><br><a href=" + MyAppLogin + ">Please Login Here</a>");

                                        //"<b>Please Checked This Requisition No:" + requisitionNo);
                                        //,string RequirmentType,string RequisitionType,string Requisitiondate
                                        if (message != null)
                                        {
                                            email.Send(message);
                                            log.ToList = employee.EmailId;
                                            log.CcList = ccList;
                                            log.BccList = bccList;
                                            log.IsSuccess = true;
                                            log.HasAttachment = false;
                                            log.Remarks = "Your requisition has been  approved";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        log.IsSuccess = false;
                                        log.HasAttachment = false;
                                        log.Remarks = ex.Message;
                                        continue;
                                    }
                                }
                                else
                                {
                                    log.IsSuccess = false;
                                    log.HasAttachment = false;
                                    log.Remarks = "EMail Id not Found";
                                }
                            }
                        }
                    }
                }
                _mailLogRepository.Insert(log);
                _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                log.Remarks = ex.Message;
                _mailLogRepository.Insert(log);
                _unitOfWork.SaveChanges();
                throw;
            }
        }
        private List<MailViewModel> GetAdministrativeMailList()
        {
            var sql = @"SELECT MRD.Id, MRD.UserId, MRD.MailType, ISNULL(U.FullName,MRD.FullName) AS FullName, MRD.Email AS Email, ISNULL(U.Active, CONVERT(BIT, 1)) AS Active  FROM [SCS].[MailReceiverDetail] AS MRD
						LEFT JOIN [SEC].[User] AS U ON U.Id=MRD.UserId
						JOIN [SCS].[MailReceiver] AS MR ON MR.Id = MRD.MailReceiverId
                        WHERE MR.Active = 1 AND MR.MailReceipientType = 'Admin'";
            return _mailReceiverDetailRepository.SqlQuery<MailViewModel>(sql).ToList();
        }





        #endregion
    }
}