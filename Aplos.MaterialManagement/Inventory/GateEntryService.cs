using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Inventory;
using Library.Model.Products;
using Library.Model.Taxations;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using Library.ViewModel.Inventory;
using Library.ViewModel.OrderManagements;
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
    public class GateEntryService : Service<GateEntry>, IGateEntryService

    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<GateEntry> _gateEntryRepository;
        private readonly IRepositoryAsync<MaterialRequsitionDetails> _materialRequsitionDetailsRepository; 
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<GatePassMaster> _gatePassMasterRepository;
        private readonly IRepositoryAsync<GatePassDetails> _gatePassMasterDetailsRepository; 

        public GateEntryService( 
             IRepositoryAsync<GateEntry> gateEntryRepository
            , IRepositoryAsync<GatePassMaster> gatePassMasterRepository 
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IRepositoryAsync<MaterialRequsitionDetails> materialRequsitionDetailsRepository 
            , IRepositoryAsync<GatePassDetails> gatePassMasterDetailsRepository
            ) : base(gateEntryRepository, unitOfWork, pkGeneratorService)
        {
            _gateEntryRepository = gateEntryRepository;
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
            _materialRequsitionDetailsRepository = materialRequsitionDetailsRepository;
            _gatePassMasterRepository = gatePassMasterRepository;
            _gatePassMasterDetailsRepository = gatePassMasterDetailsRepository;
        }

        #endregion Constructor

        #region InventoryReceive

        private string GetPK()
        {
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			var plantId = _gateEntryRepository.SqlQuery<string>($"SELECT FilePrefix from org.plant WHERE Id ='{identity.PlantId}'").FirstOrDefault();
			//var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			//string sID = string.Empty;
			//bplib.clsGenID objGenID = new bplib.clsGenID();
			//objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(GateEntry), out sID);
			//return sID;
			return GetAutoNumber(plantId + nameof(GateEntry), PKGeneratorEnum.Yearly, null, DateTime.Now);
		}
        private string GetPKForGatePass()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var plantId = _gateEntryRepository.SqlQuery<string>($"SELECT FilePrefix from org.plant WHERE Id ='{identity.PlantId}'").FirstOrDefault();
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //string sID = string.Empty;
            //bplib.clsGenID objGenID = new bplib.clsGenID();
            //objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(GateEntry), out sID);
            //return sID;
            return GetAutoNumber(plantId + nameof(GatePassMaster), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }
        public void Insert(GateEntry entity, string PlantWiseGateId)
        {
            try
            {
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //var plantId = _gateEntryRepository.SqlQuery<string>($"SELECT FilePrefix from org.plant WHERE Id ='{identity.PlantId}'").FirstOrDefault();
                var plantId = _gateEntryRepository.SqlQuery<string>($"SELECT PreFix from dbo.PlantWiseGate WHERE Id ='{PlantWiseGateId}'").FirstOrDefault();
				if (plantId==null)
				{
					throw new CustomException("No Prefix Available for this Gate");
				}
				//var currentId = _gateEntryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[Gateentry]  WHERE PlantId ='{identity.PlantId}'").First();
				var currentId = _gateEntryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 1) AS INT)), 0) Id FROM [TRN].[Gateentry] WHERE PlantId='{identity.PlantId}'").First();
				currentId++;
                //var year1 = DateTime.Now.ToShortDateString().ToString();
                var year1 = DateTime.Now.Year;
                //var yr= year1.Substring(2);
                var yr = year1;
                //var leastDate = base.Query(t => t.Id != entity.Id && t.PlantId == entity.PlantId).Select(t => t.GateEntryTime).OrderByDescending(t => t.Year).ThenByDescending(t => t.Month).ThenByDescending(t => t.Date).FirstOrDefault();
                //var leastDate = base.Query(t.PlantId == entity.PlantId).Select(t => t.GateEntryTime).OrderByDescending(t => t.Year).ThenByDescending(t => t.Month).ThenByDescending(t => t.Date).FirstOrDefault();
                //var hiestDate = _gateEntryRepository.SqlQuery<DateTime>($"SELECT cast(isnull(MAX(GateEntryTime),'') as DateTime) GateEntryTime FROM [TRN].[Gateentry] WHERE PlantId='{identity.PlantId}'").First();
                var hiestDate = _gateEntryRepository.SqlQuery<DateTime>($"SELECT cast(isnull(MAX(GateEntryTime),'') as DateTime) GateEntryTime FROM [TRN].[Gateentry] WHERE PlantWiseGateId='{entity.PlantWiseGateId}'").First();

				var hiestDate1 = hiestDate.ToString("dd-MMM-yyyy hh:mm:ss tt");
                //string hiestDate2 = " " + hiestDate1.ToString("hh:mm:ss tt"); 
                var hiestDate2= hiestDate1.Substring(12);
				var id = GetPK();
				var resId = id.Substring(4);
				if (Convert.ToDateTime(entity.GateEntryTime) < Convert.ToDateTime(hiestDate1))
				{
					throw new CustomException("Gate Entry date can't less then " + hiestDate.ToString("dd-MMM-yyyy ")+ hiestDate2);
				}
				//ResetCurrencyRate(entity);
				//entity.Id = plantId + yr + currentId;
				entity.Id = plantId + yr + resId;
				AuditService.AddedLog(entity);
				base.Insert(entity);
            }
            catch (Exception ex)
            {
				//throw ex;
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
        }
        public void InsertGatePass(GatePassMaster entity, string PlantWiseGateId)
        {
            var flag = false;
            try
            {
                
                _unitOfWork.BeginTransaction();
                 flag = true;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var plantId = _gatePassMasterRepository.SqlQuery<string>($"SELECT FilePrefix from org.plant WHERE Id ='{identity.PlantId}'").FirstOrDefault();
                //var plantId = _gateEntryRepository.SqlQuery<string>($"SELECT PreFix from dbo.PlantWiseGate WHERE Id ='{PlantWiseGateId}'").FirstOrDefault();
                //if (plantId == null)
                //{
                //    throw new CustomException("No Prefix Available for this Gate");
                //}
                ////var currentId = _gateEntryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[Gateentry]  WHERE PlantId ='{identity.PlantId}'").First();
                //var currentId = _gateEntryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 1) AS INT)), 0) Id FROM [TRN].[Gateentry] WHERE PlantId='{identity.PlantId}'").First();
                //currentId++;
                var year1 = DateTime.Now.Year;
                //var yr = year1.Substring(5);
                ////var leastDate = base.Query(t => t.Id != entity.Id && t.PlantId == entity.PlantId).Select(t => t.GateEntryTime).OrderByDescending(t => t.Year).ThenByDescending(t => t.Month).ThenByDescending(t => t.Date).FirstOrDefault();
                ////var leastDate = base.Query(t.PlantId == entity.PlantId).Select(t => t.GateEntryTime).OrderByDescending(t => t.Year).ThenByDescending(t => t.Month).ThenByDescending(t => t.Date).FirstOrDefault();
                ////var hiestDate = _gateEntryRepository.SqlQuery<DateTime>($"SELECT cast(isnull(MAX(GateEntryTime),'') as DateTime) GateEntryTime FROM [TRN].[Gateentry] WHERE PlantId='{identity.PlantId}'").First();
                //var hiestDate = _gateEntryRepository.SqlQuery<DateTime>($"SELECT cast(isnull(MAX(GateEntryTime),'') as DateTime) GateEntryTime FROM [TRN].[Gateentry] WHERE PlantWiseGateId='{entity.PlantWiseGateId}'").First();

                //var hiestDate1 = hiestDate.ToString("dd-MMM-yyyy HH:mm:ss");
                //var hiestDate2 = hiestDate1.Substring(10);
                var id = GetPKForGatePass();
                var resId = id.Substring(4);
                //if (Convert.ToDateTime(entity.GateEntryTime) < Convert.ToDateTime(hiestDate1))
                //{
                //    throw new CustomException("Gate Entry date can't less then " + hiestDate.ToString("dd-MMM-yyyy ") + hiestDate2);
                //}
                //ResetCurrencyRate(entity);
                //entity.Id = plantId + yr + currentId;
                entity.Id = plantId + year1 + resId;
                AuditService.AddedLog(entity);
                _gatePassMasterRepository.Insert(entity);
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
        public override void Update(GateEntry entity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //var leastDate = base.Query(t => t.Id != entity.Id && t.PlantId == entity.PlantId).Select(t => t.GRNDate).OrderByDescending(t => t.Year).ThenByDescending(t => t.Month).ThenByDescending(t => t.Date).FirstOrDefault();
                //if (Convert.ToDateTime(entity.GRNDate) < leastDate) throw new CustomException("GRN date can't less then " + leastDate.ToString("dd/MMM/yyyy"));
                //ResetCurrencyRate(entity);
                entity.CompanyGroupId = identity.CompanyGroupId;
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public void UpdateGatePass(GatePassMaster entity)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //var leastDate = base.Query(t => t.Id != entity.Id && t.PlantId == entity.PlantId).Select(t => t.GRNDate).OrderByDescending(t => t.Year).ThenByDescending(t => t.Month).ThenByDescending(t => t.Date).FirstOrDefault();
                //if (Convert.ToDateTime(entity.GRNDate) < leastDate) throw new CustomException("GRN date can't less then " + leastDate.ToString("dd/MMM/yyyy"));
                //ResetCurrencyRate(entity);
                entity.CompanyGroupId = identity.CompanyGroupId;
                _gatePassMasterRepository.Update(entity);
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
      //                           , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
      //                              , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
      //			, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
      //                              ,pgl.CtnId
      //                              ,IR.AddedBy
      //                  FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
      //                  LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
      //                     ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
      //                  JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
      //                  JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
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
      //                        JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
      //                  LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
      //                        WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
      //                  LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
      //                  LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
      //                  WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting'  AND IR.EmployeeId IS NULL AND IR.IsApproved=1  AND IR.IsClosed=0 AND pgl.CtnId is not null Order by IR.PODate DESC, IR.ID DESC";
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
            //                           , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
            //                              , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
            //			, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
            //                              ,pgl.CtnId
            //                              ,IR.AddedBy
            //                  FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
            //                  LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
            //                     ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
            //                  JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
            //                  JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
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
            //                        JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
            //                  LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
            //                        WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
            //                  LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
            //                  LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
            //                  WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting'  AND IR.EmployeeId IS NULL AND IR.IsApproved=1  AND IR.IsClosed=0 AND pgl.CtnId is not null Order by IR.PODate DESC, IR.ID DESC";
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
            //                           , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
            //                              , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
            //			, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
            //                              ,pgl.CtnId
            //                              ,IR.AddedBy
            //                  FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
            //                  LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
            //                     ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
            //                  JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
            //                  JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
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
            //                        JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
            //                  LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
            //                        WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
            //                  LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
            //                  LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
            //                  WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting'  AND IR.EmployeeId IS NULL AND IR.IsApproved=1  AND IR.IsClosed=0 AND pgl.CtnId is not null Order by IR.PODate DESC, IR.ID DESC";
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
                    toCurrencyRate = _gateEntryRepository.SqlQuery<decimal>(sql).First();
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
                    svcAmount = _gateEntryRepository.SqlQuery<decimal>("SELECT ISNULL(SUM(Amount), 0)+ISNULL(SUM(TotalTaxAmount), 0) FROM TRN.POService WHERE InventoryReceiveId='" + receiveId + "' AND ISNULL(Id, '')<>'" + serviceId + "'").First();
                else
                    svcAmount = _gateEntryRepository.SqlQuery<decimal>("SELECT ISNULL(SUM(Amount), 0) FROM TRN.POService WHERE InventoryReceiveId='" + receiveId + "' AND ISNULL(Id, '')<>'" + serviceId + "'").First();
                if (svcTotalAmnt > 0) svcAmount += svcTotalAmnt;
                else svcAmount -= svcTotalAmnt;

                var detailAmount = _gateEntryRepository.SqlQuery<decimal>("SELECT ISNULL(SUM(TransactionAmount), 1) FROM TRN.PurchaseOrderDetail WHERE InventoryReceiveId='" + receiveId + "' AND ISNULL(Id, '')<>'" + detailId + "'").First();
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

        //public void GRNApproved(IEnumerable<PurchaseOrder> entities)
        //{
        //    //var flag = false;
        //    //try
        //    //{
        //    //    if (entities.IsNull()) throw new CustomException("Select GRN");
        //    //    var ids = entities.Select(t => t.Id).ToArray();
        //    //    var dbList = base.Query(t => ids.Contains(t.Id)).Select().ToList();
        //    //    _unitOfWork.BeginTransaction();
        //    //    flag = true;
        //    //    foreach (var entity in entities)
        //    //    {
        //    //        foreach (var item in dbList)
        //    //        {
        //    //            if (entity.Id == item.Id)
        //    //            {
        //    //                item.IsApproved = entity.IsApproved;
        //    //                base.UpdateGraph(item);
        //    //            }
        //    //        }
        //    //    }

        //    //    _unitOfWork.SaveChanges();
        //    //    flag = false;
        //    //    _unitOfWork.Commit();
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    throw new CustomException(ex.Message, ex,
        //    //        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //    //        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
        //    //}
        //    //finally
        //    //{
        //    //    if (flag)
        //    //    {
        //    //        _unitOfWork.Rollback();
        //    //    }
        //    //}
        //}

        public void PaymentHold(IEnumerable<PurchaseOrder> entities)
        {
            //var flag = false;
            //try
            //{
            //    if (entities.IsNull()) throw new CustomException("Select GRN");
            //    var ids = entities.Select(t => t.Id).ToArray();
            //    var dbList = base.Query(t => ids.Contains(t.Id)).Select().ToList();
            //    _unitOfWork.BeginTransaction();
            //    flag = true;
            //    foreach (var entity in entities)
            //    {
            //        foreach (var item in dbList)
            //        {
            //            if (entity.Id == item.Id)
            //            {
            //                item.IsPaymentHold = entity.IsPaymentHold;
            //                base.UpdateGraph(item);
            //            }
            //        }
            //    }

            //    _unitOfWork.SaveChanges();
            //    flag = false;
            //    _unitOfWork.Commit();
            //}
            //catch (Exception ex)
            //{
            //    throw new CustomException(ex.Message, ex,
            //        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
            //        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            //}
            //finally
            //{
            //    if (flag)
            //    {
            //        _unitOfWork.Rollback();
            //    }
            //}
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

     




//        #region Taufik RequisitionReport 
//        public void RequisitionReportby(string CompanyGroupId,string plantId,  string RequisitionId)
//        {
//            ReportUtility ru = new ReportUtility();

//            var fileName = "";
//            var strPath = "";

//            var File = "";

//            fileName = "Requisition" + plantId + ".docx";
//            strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
//            File = strPath;
//            if (!System.IO.File.Exists(strPath))
//            {
//                throw new CustomException("File <" + fileName + "> Not Found.");
//            }

//            ////A opens input document.
//            WordDocument document = new WordDocument(File, FormatType.Docx);
//            //Gets the paragraph at index 1
//            try
//            {
//                string invoicePartyAddress = "";
//                string vendorPartyAddress = "";
//                WSection section = document.Sections[0];

//                DataTable dsOrderMaster, dsServiceItems;
//                dsOrderMaster = loadOrderReqqusitionMaster(RequisitionId);//sql
//                Dictionary<string, string> columns = new Dictionary<string, string>();
//                var poApprovedStatus = "";
//                //if (string.IsNullOrEmpty(dsOrderMaster.Rows[0]["IsApproved"].ToString()) == false)
//                //{
//                //    if (Convert.ToBoolean(dsOrderMaster.Rows[0]["IsApproved"]) == false)
//                //    {
//                //        poApprovedStatus = "Unapproved";
//                //        document.Replace("{PurOrApprovedStatus}", poApprovedStatus, true, true);
//                //    }
//                //    else
//                //    {
//                //        var poApprovedDT = _sqlRepository.GetDataTable(@"select Count(*) ApproveNumber from trn.PurchaseOrderApprovalLog where POID = '" + RequisitionId + @"' and [Status] = 'Approved'");

//                //        poApprovedStatus = "Approved(" + poApprovedDT.Rows[0]["ApproveNumber"] + ")";
//                //        document.Replace("{PurOrApprovedStatus}", poApprovedStatus, true, true);
//                //    }
//                //}

//                //invoicePartyAddress = ru.GetAddress(dsOrderMaster.Rows[0]["InvoicePartyAddressMasterId"].ToString(), dsOrderMaster.Rows[0]["InvoicingByAddress"].ToString());
//                //document.Replace("{InvoicingPartyAddress}", invoicePartyAddress, false, false);

//                //vendorPartyAddress = ru.GetAddress(dsOrderMaster.Rows[0]["VendorAddressMasterId"].ToString(), "");
//                //document.Replace("{VendorAddress}", vendorPartyAddress, false, false);


//                //document.Replace("{DeliveryInstruction}", dsOrderMaster.Rows[0]["DeliveryInstruction"].ToString(), false, false);

//                document.Replace("{Remarks}", dsOrderMaster.Rows[0]["Remarks"].ToString(), false, false);
//                document.Replace("{AddedBy}", dsOrderMaster.Rows[0]["AddedBy"].ToString(), false, false);

//                foreach (DataColumn item in dsOrderMaster.Columns)
//                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);
//                //dsServiceItems = loadServicerMasterItems(RequisitionId);
//                DataTable dsMaterialItems = requistionMaterialDetail(RequisitionId);
//                var materialTotal = makeMaterialDetailsTable(document, dsMaterialItems, RequisitionId);//Material Details 
//                //var serviceTotal = 0.00;
//                //if (dsServiceItems.Rows.Count > 0)
//                //{
//                //    //{ServiceItems}
//                //    serviceTotal = makeServiceDetailsTable(document, dsServiceItems, RequisitionId);//Service Details 
//                //    document.Replace("{ServiceDetails}", "Service Details", true, true);
//                //}
//                document.Replace("{GrandTotal}", (materialTotal ).ToString("#,##0.00") + " " + dsMaterialItems.Rows[0]["CurrencyName"].ToString(), true, true);
//                document.Replace("{TotalInWords}", ru.InWord((materialTotal), dsMaterialItems.Rows[0]["CurrencyId"].ToString()), true, true);

//                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();

//                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));

//                //creating secondary array to prevent memory leak and accidental over-writing (Tarek Talukder-26-May-2019)
//                List<string> strReplace = new List<string>();
//                for (int i = 0; i < allresult.Length; i++)
//                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());

//                StringCollection strColDistinct = new StringCollection();
//                for (int i = 0; i < strReplace.Count; i++)
//                {
//                    if (strColDistinct.Contains(strReplace[i].ToUpper()))
//                        continue;

//                    strColDistinct.Add(strReplace[i].ToUpper());

//                    string text = strReplace[i].ToUpper();
//                    ReplaceInfo.Add(text, 0);
//                    if (columns.ContainsKey(text.ToUpper()))
//                    {
//                        ReplaceInfo[text] = document.Replace(text, dsOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
//                    }

//                }

//                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);


//                //removing any unused place holder
//                foreach (var item in ReplaceInfo.Keys)
//                {
//                    if (ReplaceInfo[item.ToString()] == 0)
//                        document.Replace(item.ToString(), "", false, false);

//                }

//                document.Save(fileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
//                document.Close();

//            }
//            catch (Exception ex)
//            {
//                throw ex;

//            }



//            //Closes the instance of document objects


//            document.Close();


//            //document.Save("file.docx", Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
//            //document.Close();

//        }

//        public DataTable loadOrderMasterItems(string OrderMasterID)
//        {
//            string strSQL;
//            //clsConnection objCon;
//            try
//            {
//                strSQL = @"select so.MasterOrderItemId,so.Id AS SOID,CONCAT( mm.[Description],' ',a.StandardName) AS MaterialDesc,
//                                so.Qty,uom.UserName AS UOM,SO.Rate,so.Qty*so.Rate AS Amount,isnull(SO.Discount,0) AS Discount
//                                  from [TRN].[MasterOrderItem] T
//                                INNER JOIN [TRN].[MasterOrder] O ON o.Id=t.MasterOrderId
//                                INNER JOIN [TRN].[SalesOrder]  SO ON so.MasterOrderItemId=t.Id
//                                LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=t.MaterialMasterId
//                                LEFT OUTER JOIN mst.MaterialGroupMaster AS mgm ON mgm.Id=mm.MaterialGroupMasterId
//                                LEFT OUTER JOIN [MST].[MaterialMasterArticle] A ON a.Id=t.ArticleId
//                                LEFT OUTER JOIN [SCS].[UnitOfMeasurement] UOM ON uom.Id=o.TotalQtyUOMId
//                                where MasterOrderId='20194'";

//                return _sqlRepository.GetDataTable(strSQL);

//            }
//            catch (System.Exception ex)
//            {
//                throw (ex);
//            }
//            finally
//            {

//            }
//        }





//        public DataTable loadOrderReqqusitionMaster(string MaterialMasterId)
//        {
//            string strSQL;
//            //clsConnection objCon;
//            try
//            {
//                strSQL = @"Select 
//	                             MRM.Id
//	                            ,MRM.RequisitionDate
//	                            ,MRM.RequisitionType
//	                            ,MRM.RequirmentType
//	                            ,MRM.QualityApprovalResponsiblePersonId
//	                            ,MRM.NeedSpecialAppId
//	                            ,E.UserName EntityName
//	                            ,MRM.EntityId
//	                            ,MRM.ReasonWhyItIsNotPlanEarlier requirementReason
//	                            ,MRM.AddedBy
//                                ,MRM.AddedDate

//	                            ,MRM.AddedFromIP
//	                            ,MRM.UpdatedBy
//	                            ,MRM.UpdatedDate
//	                            ,MRM.UpdatedFromIP
//	                            ,MRM.Remarks
//	                            ,MRM.CheckedBy
//	                            ,MRM.CheckedByStatus
//	                            ,MRM.AuthorizedBy
//	                            ,MRM.AuthorizedByStatus
//	                            ,MRM.IsApproved
//	                            ,A.UserName ActivityName
//	                            ,MM.UserName MaterialName
//	                            ,MRD.TransactionQty
//	                            ,MRD.EstimatedRate
//	                            ,MRD.TotalAmount
//	                            FROM [TRN].[MaterialRequsitionMaster] MRM
//	                            Left Join [TRN].[MaterialRequsitionDetails] MRD On MRD.MaterialReqqusitionMasterId=MRM.Id
//	                            Left Join org.Entity E on E.Id=MRM.EntityId
//	                            LEFT JOin HKp.Activity A On A.Id=MRD.ActivityId
//	                            Left Join MST.MaterialMaster MM on MM.Id=MRD.MaterialMasterId 
//	                            WHERE MRM.Id='" + MaterialMasterId +@"'";

//                return _sqlRepository.GetDataTable(strSQL);

//            }
//            catch (System.Exception ex)
//            {
//                throw (ex);
//            }
//            finally
//            {

//            }
//        }




//        public DataTable loadMaterialTax(string purchaseOrderId)
//        {
//            string strSQL;

//            try
//            {
//                strSQL = @"select InventoryServiceId,PO.Id PurchaseOrderId,POD.Id PurchaseOrderDetailId,tg.Code AS TaxCode,PODT.Percentage, PODT.TaxAmount from TRN.PurchaseOrder PO
//                            INNER JOIN TRN.PurchaseOrderDetail POD ON POD.InventoryReceiveId = PO.Id
//                            Inner join TRN.PurchaseOrderTax PODT ON PODT.InventoryReceiveId = PO.Id and PODT.InventoryReceiveDetailId = POD.Id
//                            LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=PODT.TaxCategoryId
//                            WHERE PO.Id='" + purchaseOrderId + @"' 
//							and InventoryReceiveDetailId  is not null and  InventoryServiceId is null 
//							ORDER BY tg.[Sequence] ";


//                return _sqlRepository.GetDataTable(strSQL);

//            }
//            catch (Exception ex)
//            {
//                throw (ex);
//            }
//            finally
//            {

//            }
//        }

//        public double makeMaterialDetailsTable(WordDocument document, DataTable dsMaterialItems, string requisitionId)
//        {
//            string replaceString = "{materialRequsitionDetail}";
//            ReportUtility ru = new ReportUtility();
//            DataTable  dsTax;
//            //clsDataContext data = new clsDataContext();
          

//            //dsTax = loadMaterialTax(purchaseOrderId);

//            int LasColumnIndex = 9;
//            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
//            //DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));

//            //if (dv.Count > 0)
//            //{
//            //    for (int i = 0; i < dv.Count; i++)
//            //    {
//            //        LasColumnIndex++;
//            //        dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
//            //        LasColumnIndex++;
//            //    }
//            //}


//            WTable wTable = new WTable(document);
//            int ROW = 0; int COL = 0;
//            wTable.ResetCells(1, LasColumnIndex + 1);

//            WTableRow TemplateRow = wTable.Rows[0].Clone();

//            #region column headers
//            document.EnsureMinimal();
//            //wTable.Title = "Material Details";
//            WCharacterFormat FontBold = new WCharacterFormat(document);
//            FontBold.Bold = true;


//            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Materials");
//            range.ApplyCharacterFormat(FontBold);
//            int colMaterialGroup = COL; COL++;

//            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article ");
//            range.ApplyCharacterFormat(FontBold);
//            int colArticle = COL; COL++;

//            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SQ1");
//            range.ApplyCharacterFormat(FontBold);
//            int colChar1 = COL; COL++;

//            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SQ2");
//            range.ApplyCharacterFormat(FontBold);
//            int colChar2 = COL; COL++;

//            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SQ3");
//            range.ApplyCharacterFormat(FontBold);
//            int colChar3 = COL; COL++;
//            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Origin");//TRN.PurchaseOrderDetail ->CountryId
//            //range.ApplyCharacterFormat(FontBold);
//            //int colOriginCountry = COL; COL++;
//            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
//            range.ApplyCharacterFormat(FontBold);
//            int colQty = COL; COL++;

//            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate (" + dsMaterialItems.Rows[0]["CurrencyName"].ToString() + ")");
//            range.ApplyCharacterFormat(FontBold);
//            int colRate = COL++;

//            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UOM");
//            range.ApplyCharacterFormat(FontBold);
//            int colUOM = COL; 

//            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Normal");
//            //range.ApplyCharacterFormat(FontBold);
//            //int colNormal = COL; COL++;

//            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Over Budget");
//            //range.ApplyCharacterFormat(FontBold);
//            //int colOB = COL; COL++;

//            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("New");
//            //range.ApplyCharacterFormat(FontBold);
//            //int colNew = COL; 

//            int colTotalTaxableAmount = COL;
          
//            //if (dv.Count > 0)
//            //{
//            //    COL++;
//            //    colTotalTaxableAmount = COL;
//            //    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
//            //    range.ApplyCharacterFormat(FontBold);
//            //    //COL++;
//            //    for (int i = 0; i < dv.Count; i++)
//            //    {
//            //        //two columns required for tax
//            //        COL++;
//            //        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
//            //        range.ApplyCharacterFormat(FontBold);
//            //        COL++;
//            //        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
//            //        range.ApplyCharacterFormat(FontBold);

//            //    }
//            //}
//            //else
//            //{
//                COL++;
//                colTotalTaxableAmount = COL;
//                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
//                range.ApplyCharacterFormat(FontBold);

//            //}


//            wTable.Rows.Add(TemplateRow);
//            ROW++;

//            //if (dv.Count > 0)
//            //{
//            //    for (int i = 0; i < dv.Count; i++)
//            //    {

//            //        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate");
//            //        range.ApplyCharacterFormat(FontBold);
//            //        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
//            //        range.ApplyCharacterFormat(FontBold);

//            //    }
//            //}
//            #endregion column headers
//            double totalValue = 0;
//            int startRow = ROW ;
//            for (int i = 0; i < dsMaterialItems.Rows.Count; i++)
//            {
//                ROW++;
//                wTable.AddRow();
//                WTableRow TROW = wTable.LastRow;

//                // WTableRow TROW = wTable.Rows[1].Clone();
//                for (int CE = 0; CE < TROW.Cells.Count; CE++)
//                {
//                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
//                    {
//                        item.Text = "";
//                    }
//                }
//                TROW.Cells[colMaterialGroup].AddParagraph().AppendText(dsMaterialItems.Rows[i]["MaterialGroupMasterName"].ToString());
//                TROW.Cells[colArticle].AddParagraph().AppendText(dsMaterialItems.Rows[i]["Article"].ToString());
//                TROW.Cells[colChar1].AddParagraph().AppendText(dsMaterialItems.Rows[i]["FirstCharacteristicsValue"].ToString());
//                TROW.Cells[colChar2].AddParagraph().AppendText(dsMaterialItems.Rows[i]["SecondCharacteristicsValue"].ToString());
//                TROW.Cells[colChar3].AddParagraph().AppendText(dsMaterialItems.Rows[i]["ThirdCharacteristicsValue"].ToString());
//                TROW.Cells[colQty].AddParagraph().AppendText(clsStdLib.dbl(dsMaterialItems.Rows[i]["TransactionQty"].ToString()).ToString("F2"));
//                TROW.Cells[colRate].AddParagraph().AppendText(clsStdLib.dbl(dsMaterialItems.Rows[i]["TransactionRate"].ToString()).ToString("F2"));
//                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsMaterialItems.Rows[i]["TrnAmount"].ToString()).ToString("#,##0.00"));
//                TROW.Cells[colUOM].AddParagraph().AppendText(dsMaterialItems.Rows[i]["TransactionUoM"].ToString());
//                //TROW.Cells[colNormal].AddParagraph().AppendText(dsMaterialItems.Rows[i]["TransactionUoM"].ToString());
//                //TROW.Cells[colUOM].AddParagraph().AppendText(dsMaterialItems.Rows[i]["TransactionUoM"].ToString());
//                //TROW.Cells[colUOM].AddParagraph().AppendText(dsMaterialItems.Rows[i]["TransactionUoM"].ToString());

//                //totalValue += clsStdLib.dbl(dsMaterialItems.Rows[i]["TrnAmount"].ToString());

//                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(totalValue.ToString("F2"));

//                //if (dv.Count > 0)
//                //{
//                //    //dsTax.Tables[0].DefaultView.RowFilter = "MasterOrderItemId='" + dsOrderItems.Tables[0].Rows[i]["MasterOrderItemId"].ToString() + "'";
//                //    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
//                //    //double totalTax = 0;

//                //    for (int T = 0; T < dv.Count; T++)
//                //    {
//                //        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND PurchaseOrderDetailId='" + dsOrderMaster.Rows[i]["PurchaseOrderDetailId"].ToString() + "'";
//                //        if (dvtax.Count > 0)
//                //        {
//                //            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));
//                //            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("#,##0.00"));
//                //        }
//                //    }
//                //}


//            }

//            ROW++;
//            #region Total
//            int TotalRow = ROW;
//            wTable.AddRow();
//            WTableRow _TROW = wTable.LastRow;
//            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);



//            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
//            {
//                if (C == colRate || C == colArticle || C == colChar1 || C == colChar2 || C == colChar3 || C == colQty || C == colUOM)
//                    continue;

//                double value = 0;
//                for (int i = startRow; i < TotalRow; i++)
//                {

//                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
//                    {
//                        value += clsStdLib.dbl(item.Text);
//                    }
//                }
//                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);

//            }
//            #endregion Total


//            ROW++;
//            #region Sub Total
//            //int SubTotalRow = ROW;
//            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
//            //wTable.AddRow();
//            //_TROW = wTable.LastRow;

//            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

//            double total = clsStdLib.dbl(dsMaterialItems.Compute("SUM(TrnAmount)", "").ToString());
//                //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
//                //+ clsStdLib.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());

//            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2") + " (" + ru.InWord(total, dsOrderMaster.Rows[0]["CurrencyId"].ToString()) + ")");

//            #endregion Total


//            ROW++;
//            #region Total Payable
//            //int TotalPayableRow = ROW;
//            //int TotalPayableColumn = 0;//_TROW.Cells.Count - 5;
//            //wTable.AddRow();
//            //_TROW = wTable.LastRow;

//            //_TROW.Cells[TotalPayableColumn].AddParagraph().AppendText("Total Amount Payable");
//            //_TROW.Cells[TotalPayableColumn + 1].AddParagraph().AppendText("Need To Discuss");

//            #endregion Total Payable


//            ROW++;


//            #region paragrpath formats
//            //Adds a new paragraph style named "MyStyle"
//            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
//            //Sets the formatting of the style
//            myStyle.CharacterFormat.FontSize = 8f;
//            //myStyle.CharacterFormat.TextColor = Color.Black;
//            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

//            for (int R = 0; R < wTable.Rows.Count; R++)
//            {
//                WTableRow TROW = wTable.Rows[R];
//                TROW.Cells[0].Width = 120;
//                //if (dv.Count < 3)
//                //    TROW.Cells[0].Width = 120 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

//                for (int CE = 0; CE < TROW.Cells.Count; CE++)
//                {
//                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
//                    {
//                        item.ApplyStyle("MyStyle");
//                    }
//                }
//            }


//            #endregion paragrpath formats


//            #region merging section


//            //tax codes merging (horizontal)
//            ROW = 0;
//            //for (int i = 0; i < dv.Count; i++)
//            //    wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

//            //primary cells merging (veritcal)
//            ROW++;
//            //for (int i = 0; i <= colTotalTaxableAmount; i++)
//            //    wTable.ApplyVerticalMerge(i, ROW - 1, ROW);




//            IWParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
//            style.CharacterFormat.Bold = true;
//            style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
//            //Adds new paragraph to the section

//            #endregion merging section

//            TextBodyPart textBodyPart = new TextBodyPart(document);
//            textBodyPart.BodyItems.Add(wTable);
//            document.Replace(replaceString, textBodyPart, true, true);
//            return total;
//        }



//        public double makeServiceDetailsTable(WordDocument document, DataTable dsServiceItems, string purchaseOrderId)
//        {
//            string replaceString = "{ServiceItems}";
//            ReportUtility ru = new ReportUtility();

//            DataTable dsTax;
//            //clsDataContext data = new clsDataContext();

//            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign");
//            //Sets the formatting of the style
//            rightAlign.CharacterFormat.FontSize = 8f;
//            rightAlign.CharacterFormat.TextColor = Color.Black;
//            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;


//            dsTax = loadServiceMasterTax(purchaseOrderId);

//            int LasColumnIndex = 1;
//            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
//            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));

//            //LasColumnIndex++;
//            //dicTaxes.Add("totaltax", LasColumnIndex);
//            if (dv.Count > 0)
//            {
//                for (int i = 0; i < dv.Count; i++)
//                {
//                    LasColumnIndex++;
//                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
//                    LasColumnIndex++;
//                }
//            }


//            WTable wTable = new WTable(document);
//            int ROW = 0; int COL = 0;
//            wTable.ResetCells(1, LasColumnIndex + 1);

//            WTableRow TemplateRow = wTable.Rows[0].Clone();

//            #region column headers
//            document.EnsureMinimal();

//            WCharacterFormat FontBold = new WCharacterFormat(document);
//            FontBold.Bold = true;

//            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Services");
//            int colServiceName = COL; //COL++;           
//            range.ApplyCharacterFormat(FontBold);




//            int colTotalTaxableAmount = COL;
//            if (dv.Count > 0)
//            {
//                COL++;
//                colTotalTaxableAmount = COL;
//                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
//                range.ApplyCharacterFormat(FontBold);

//                //COL++;
//                for (int i = 0; i < dv.Count; i++)
//                {
//                    //two columns required for tax
//                    COL++;
//                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
//                    range.ApplyCharacterFormat(FontBold);

//                    COL++;
//                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
//                    range.ApplyCharacterFormat(FontBold);

//                }
//            }
//            else
//            {
//                COL++;
//                colTotalTaxableAmount = COL;
//                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
//                range.ApplyCharacterFormat(FontBold);

//            }


//            wTable.Rows.Add(TemplateRow);
//            ROW++;

//            if (dv.Count > 0)
//            {
//                for (int i = 0; i < dv.Count; i++)
//                {

//                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate");
//                    range.ApplyCharacterFormat(FontBold);
//                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
//                    range.ApplyCharacterFormat(FontBold);

//                }
//            }
//            #endregion column headers
//            double totalValue = 0;
//            int startRow = ROW + 1;
//            for (int i = 0; i < dsServiceItems.Rows.Count; i++)
//            {
//                ROW++;
//                wTable.AddRow();
//                WTableRow TROW = wTable.LastRow;

//                // WTableRow TROW = wTable.Rows[1].Clone();
//                for (int CE = 0; CE < TROW.Cells.Count; CE++)
//                {
//                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
//                    {
//                        item.Text = "";
//                    }
//                }
//                IParagraphItem p = TROW.Cells[colServiceName].AddParagraph().AppendText(dsServiceItems.Rows[i]["Service"].ToString());
//                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(dsServiceItems.Rows[i]["Amount"].ToString());
//                //TROW.Cells[colChar1].AddParagraph().AppendText(dsOrderMaster.Tables[0].Rows[i]["FirstCharacteristicsValue"].ToString());
//                //TROW.Cells[colChar2].AddParagraph().AppendText(dsOrderMaster.Tables[0].Rows[i]["SecondCharacteristicsValue"].ToString());
//                //TROW.Cells[colChar3].AddParagraph().AppendText(dsOrderMaster.Tables[0].Rows[i]["ThirdCharacteristicsValue"].ToString());
//                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsServiceItems.Rows[i]["Amount"].ToString()).ToString("#,##0.00"));

//                //TROW.Cells[colRate].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Tables[0].Rows[i]["TransactionRate"].ToString()).ToString("F2"));
//                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Tables[0].Rows[i]["TrnAmount"].ToString()).ToString("F2"));

//                totalValue += clsStdLib.dbl(dsServiceItems.Rows[i]["Amount"].ToString());

//                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(totalValue.ToString("F2"));

//                if (dv.Count > 0)
//                {
//                    //dsTax.Tables[0].DefaultView.RowFilter = "MasterOrderItemId='" + dsOrderItems.Tables[0].Rows[i]["MasterOrderItemId"].ToString() + "'";
//                    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
//                    //double totalTax = 0;

//                    for (int T = 0; T < dv.Count; T++)
//                    {
//                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND InventoryServiceId='" + dsServiceItems.Rows[i]["ServiceId"] + "'";
//                        if (dvtax.Count > 0)
//                        {
//                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));
//                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("#,##0.00"));
//                        }
//                    }
//                }
//            }

//            ROW++;
//            #region Total
//            int TotalRow = ROW;
//            wTable.AddRow();
//            WTableRow _TROW = wTable.LastRow;
//            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);

//            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
//            {
//                if (dicTaxes.ContainsValue(C))
//                    continue;

//                double value = 0;
//                for (int i = startRow; i < TotalRow; i++)
//                {

//                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
//                    {
//                        value += clsStdLib.dbl(item.Text);
//                    }
//                }
//                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);

//            }
//            #endregion Total


//            ROW++;
//            #region Sub Total
//            //int SubTotalRow = ROW;
//            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
//            //wTable.AddRow();
//            //_TROW = wTable.LastRow;

//            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

//            double total = clsStdLib.dbl(dsServiceItems.Compute("SUM(Amount)", "").ToString())
//                //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
//                + clsStdLib.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());

//            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2") + " (" + ru.InWord(total, dsOrderMaster.Rows[0]["CurrencyId"].ToString()) + ")");

//            #endregion Total


//            ROW++;
//            #region Total Payable
//            //int TotalPayableRow = ROW;
//            //int TotalPayableColumn = 0;//_TROW.Cells.Count - 5;
//            //wTable.AddRow();
//            //_TROW = wTable.LastRow;

//            //_TROW.Cells[TotalPayableColumn].AddParagraph().AppendText("Total Amount Payable");
//            //_TROW.Cells[TotalPayableColumn + 1].AddParagraph().AppendText("Need To Discuss");

//            #endregion Total Payable


//            ROW++;


//            #region paragrpath formats
//            //Adds a new paragraph style named "MyStyle"
//            IWParagraphStyle myStyle2 = document.AddParagraphStyle("MyStyle2");
//            //Sets the formatting of the style
//            myStyle2.CharacterFormat.FontSize = 8f;
//            myStyle2.CharacterFormat.TextColor = Color.Black;
//            myStyle2.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

//            for (int R = 0; R < wTable.Rows.Count; R++)
//            {
//                WTableRow TROW = wTable.Rows[R];
//                TROW.Cells[0].Width = 120;
//                if (dv.Count < 3)
//                    TROW.Cells[0].Width = 120 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

//                for (int CE = 0; CE < TROW.Cells.Count; CE++)
//                {
//                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
//                    {
//                        item.ApplyStyle("MyStyle2");
//                    }
//                }
//            }


//            #endregion paragrpath formats


//            #region merging section


//            //tax codes merging (horizontal)
//            ROW = 0;
//            for (int i = 0; i < dv.Count; i++)
//                wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

//            //primary cells merging (veritcal)
//            ROW++;
//            for (int i = 0; i <= colTotalTaxableAmount; i++)
//                wTable.ApplyVerticalMerge(i, ROW - 1, ROW);




//            IWParagraphStyle style2 = document.AddParagraphStyle("SubTotalStyle2");
//            style2.CharacterFormat.Bold = true;
//            style2.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
//            //Adds new paragraph to the section


//            //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
//            //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
//            //        PARA.ApplyStyle("SubTotalStyle2");

//            //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
//            #endregion merging section



//            TextBodyPart textBodyPart = new TextBodyPart(document);
//            textBodyPart.BodyItems.Add(wTable);
//            document.Replace(replaceString, textBodyPart, true, true);
//            return total;
//        }

//        private DataTable requistionMaterialDetail(string requistionId)
//        {
//            try
//            {
//                string sqlText = @"DECLARE @inventoryReceiveId VARCHAR(10)='"+ requistionId + @"'
	           
//SELECT IM.Id
//     , IM.Id AS MaterialReqqusitionMasterId
//    , MGM.UserName AS MaterialGroupMasterName
//    , IM.MaterialMasterId, MM.UserName MaterialName
//    , IM.ArticleId, ART.StandardName Article
//    , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
//    , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
//    , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
//    , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
//    , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
//    , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
//    , ROUND(IM.TransactionQty,2) TransactionQty
//	, IM.TransactionUoMId
//     ,IM.BudgetType 
//	, TUoM.UserName AS TransactionUoM
//	, ROUND(IM.EstimatedRate,2) TransactionRate 
//	, CU.Code AS CurrencyName
//		, CU.Id AS CurrencyId
//    , ROUND((IM.TransactionQty*IM.EstimatedRate),2) AS TrnAmount   
//    ,IM.MaterialDetail
//    ,Replace(CONVERT(VARCHAR(11), IM.DeliveryDate, 106), ' ', '-') DeliveryDate

//	,Act.Id As Activity
//	,Act.UserName As ActivityName
	
//	,IM.Reason
//	,IM.Remarks
//	,IM.FutureReqApp
//	--,BudgetMasterId
//	--,GLGeneralInfoId
//FROM TRN.MaterialRequsitionDetails AS IM
//JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
//LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
//LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
//LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
//LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
//LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
//LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
//LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
//LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id

//JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IM.TransactionUoMId=TUoM.Id
//JOIN [TRN].[MaterialRequsitionMaster] AS IR ON IM.MaterialReqqusitionMasterId=IR.Id
//JOIN [SCS].[Currency] AS CU ON IM.CurrencyId=CU.Id 
//JOIN [HKP].[Activity] As Act On ACT.Id=IM.ActivityId
//--JOIN [HKP].Budget
//--JOIN [HKP].Gl
//WHERE IM.MaterialReqqusitionMasterId=@inventoryReceiveId";

//                return _sqlRepository.GetDataTable(sqlText);
//            }
//            catch (Exception ex)
//            {

//                throw ex;
//            }
//        }

//        class clsStdLib
//        {
//            public static string passWord = "prodDisplay";
//            public clsStdLib()
//            {

//            }
//            public enum mType
//            {
//                Error,
//                Success,
//                Information
//            }
//            public static bool passwordGet = true;
//            public static string[] sMonth = new string[] { "<Unselect>", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

//            public static string DataRankNames(int dayNo)
//            {

//                if (dayNo <= 0)
//                    return "";

//                if (dayNo.ToString().Length > 1)
//                {
//                    string Right = dayNo.ToString().Substring(dayNo.ToString().Length - 2, 2);
//                    if (clsStdLib.dbl(Right) >= 10 && clsStdLib.dbl(Right) <= 20)
//                        return dayNo + "th";
//                }

//                string RightString = dayNo.ToString().Substring(dayNo.ToString().Length - 1, 1);
//                switch (RightString)
//                {
//                    case "1":
//                        return dayNo + "st";
//                    case "2":
//                        return dayNo + "nd";
//                    case "3":
//                        return dayNo + "rd";
//                    default:
//                        return dayNo + "th";

//                }




//            }

//            #region date related
//            public static readonly string dateFormat = "dd-MMM-yyyy";
//            public static readonly string sqliteDateFormat = "yyyy-MM-dd";
//            public static readonly string AppToDBdateFormat = "yyyy-MM-dd hh:mm:ss";
//            public static bool IsDateOK(string strdate)
//            {
//                try
//                {
//                    if (strdate.Length != 11)
//                    {
//                        return false;
//                    }
//                    if (strdate.Substring(2, 1) != "-" && strdate.Substring(6, 1) != "-")
//                    {
//                        return false;
//                    }
//                    System.DateTime myDt = System.Convert.ToDateTime(strdate);
//                    return true;
//                }
//                catch (System.Exception ex)
//                {
//                    return false;
//                }
//                finally
//                {
//                    //
//                }
//            }// end function
//            private static bool DateOkCheck(string strdate)
//            {
//                try
//                {
//                    System.DateTime myDt = System.Convert.ToDateTime(strdate);
//                    return true;
//                }
//                catch (System.Exception ex)
//                {
//                    return false;
//                }
//                finally
//                {
//                    //
//                }
//            }// end function
//            public static object chk_NullDateData(object dateValue)
//            {
//                if (DateOkCheck("" + dateValue.ToString()) == false)
//                {
//                    dateValue = "";
//                }

//                if (("" + dateValue.ToString()) == "")
//                {
//                    System.DateTime dt = new System.DateTime(1901, 1, 1);
//                    dateValue = (object)dt;
//                }
//                return (object)dateValue;
//            }
//            public static System.DateTime AppDateConvert(object dateValue, string input_date_format, string output_date_format)
//            {
//                string strDate = null;
//                dateValue = chk_NullDateData(dateValue);
//                strDate = dateValue.ToString();
//                if (strDate != "")
//                {
//                    if (input_date_format.Trim() != "")
//                    {
//                        if (output_date_format.Trim() != "")
//                        {
//                            System.Globalization.DateTimeFormatInfo InputFormat = new System.Globalization.DateTimeFormatInfo();
//                            InputFormat.ShortDatePattern = input_date_format;
//                            System.DateTime myDt = System.Convert.ToDateTime(strDate, InputFormat);
//                            strDate = myDt.ToString(output_date_format);
//                        }
//                    }
//                }
//                return System.Convert.ToDateTime(strDate);
//            }// End of function
//            public static Object DateData_AppToDB(object dateValue, string DB_Level_date_format)
//            {
//                if (string.IsNullOrEmpty((string)dateValue))
//                    return DBNull.Value;

//                string strDate = null;
//                strDate = dateValue.ToString();
//                if (DB_Level_date_format != "")
//                {
//                    // Collecting the user terminal set format 
//                    System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
//                    strDate = AppDateConvert(strDate, USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString(), DB_Level_date_format).ToString();
//                }

//                string m = System.Convert.ToDateTime(strDate).ToString(AppToDBdateFormat);
//                return System.Convert.ToDateTime(strDate).ToString(AppToDBdateFormat);


//            }// End of function
//            public static System.DateTime DateData_DBToApp(object dateValue)
//            {
//                string strDate = null;
//                strDate = dateValue.ToString();

//                System.Globalization.DateTimeFormatInfo myDBDateFormat = new System.Globalization.CultureInfo("en-US", false).DateTimeFormat;
//                strDate = DateData_DBToApp(dateValue, myDBDateFormat.ShortDatePattern.ToString()).ToString();
//                return System.Convert.ToDateTime(strDate);
//            }// End function
//            public static System.DateTime DateData_DBToApp(object dateValue, string DB_Level_date_format)
//            {
//                string strDate = null;
//                strDate = dateValue.ToString();
//                if (DB_Level_date_format != "")
//                {
//                    // Collecting the user terminal set format 
//                    System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
//                    strDate = AppDateConvert(strDate, DB_Level_date_format, USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString()).ToString();
//                }
//                return System.Convert.ToDateTime(strDate);
//            }// End of function
//            public static String makeBaseBlank(object dateValue)
//            {
//                System.DateTime dt;
//                dt = System.Convert.ToDateTime(dateValue.ToString());
//                if (dt.Year == 1901)
//                {
//                    return "";
//                }
//                else
//                {
//                    return dateValue.ToString();
//                }
//            }// End of function
//             ///<summary>
//             ///return day difference in integer. 
//             ///    Example 1: firstDate[Less Than]lastDate returns positive value
//             ///    Example 2: firstDate>lastDate returns negative value
//             ///    Example 3: firstDate=lastDate returns 0 [zero]**/
//             /// </summary>
//            public static int dateDiff(string firstDate, string lastDate)
//            {

//                int difference = 0;
//                try
//                {
//                    firstDate = Convert.ToDateTime(firstDate).ToString("dd-MMM-yyyy");
//                    lastDate = Convert.ToDateTime(lastDate).ToString("dd-MMM-yyyy");

//                    if (IsDateOK(firstDate) == false)
//                    {
//                        Exception ex = new Exception("Invalid [First Date]");
//                        throw (ex);
//                    }
//                    if (IsDateOK(lastDate) == false)
//                    {
//                        Exception ex = new Exception("Invalid [Last Date]");
//                        throw (ex);
//                    }
//                    DateTime dateFirstDate = Convert.ToDateTime(firstDate);
//                    DateTime dateLastDate = Convert.ToDateTime(lastDate);
//                    TimeSpan TimeSpan = dateLastDate.Subtract(dateFirstDate);


//                    difference = TimeSpan.Days;
//                }
//                catch (Exception ex)
//                {
//                    throw (ex);
//                }

//                return difference;
//            }



//            public static string getSqliteDate(string standardDate)
//            {
//                return (Convert.ToDateTime(standardDate).ToString(sqliteDateFormat));
//            }
//            public static string getStandardDateFromSqliteDate(string SqliteDate)
//            {
//                if (SqliteDate.Length != 10)
//                    return "";
//                if (SqliteDate.Split('-').Length != 3)
//                    return "";
//                //many things to validate 
//                //but i have less time :)
//                string month = ValidLength(sMonth[Convert.ToInt32(SqliteDate.Split('-')[1])], 3).ToString();


//                return SqliteDate.Split('-')[2] + "-" + month + "-" + SqliteDate.Split('-')[0];
//            }
//            #endregion date related

//            #region numeric
//            public static bool IsNumeric(string strNumber)
//            {
//                Double d;
//                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
//                if (strNumber.Length == 0)
//                {
//                    return false;
//                }
//                return Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d);
//            } // End Function
//            public static string GetNumericData(string strNumber)
//            {
//                double d;
//                strNumber = strNumber.Replace(",", "");
//                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
//                if (strNumber.Trim() == "")
//                { return "0"; }
//                else if (System.Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
//                {
//                    return strNumber;
//                }
//                else
//                {
//                    return "0";
//                }
//            }// end function
//            public static string GetNumericDataInDecimalFormat(string strNumber, int precision)
//            {
//                if (precision < 1)
//                    return strNumber;

//                string s_precision = new String('0', precision);

//                double d;
//                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
//                if (strNumber.Trim() == "")
//                { return "0." + s_precision; }
//                else if (System.Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
//                {
//                    return string.Format("{0:0." + s_precision + "}", d);
//                }
//                else
//                {
//                    return "0." + s_precision;
//                }
//            }// end function
//            public static double dbl(string d)
//            {
//                return Convert.ToDouble(GetNumericData(d));

//            }
//            public static int Percentage(int total, double percentage)
//            {
//                return (int)(total * (percentage / 100));

//            }
//            //validation
//            public static void numericValidation(string value, bool isMandatory, bool isInteger, bool negativeAllowed, string fieldName)
//            {

//                try
//                {



//                    if (isMandatory == true)
//                    {
//                        if (value.Trim() == "")
//                        {
//                            Exception ex = new Exception("please insert [" + fieldName + "]");
//                            throw (ex);
//                        }
//                        if (Convert.ToDouble(GetNumericData(value.Trim())) == 0)
//                        {
//                            Exception ex = new Exception("please insert [" + fieldName + "]");
//                            throw (ex);
//                        }

//                        if (value.Trim() != "")
//                        {
//                            if (IsNumeric(value.Trim()) == false)
//                            {
//                                Exception ex = new Exception("Invalid numeric value [" + value + "] for the field [" + fieldName + "]");
//                                throw (ex);
//                            }
//                        }
//                    }

//                    if (value.Trim() != "")
//                    {
//                        if (IsNumeric(value.Trim()) == false)
//                        {
//                            Exception ex = new Exception("Invalid numeric value [" + value + "] for the field [" + fieldName + "]");
//                            throw (ex);
//                        }
//                        if (isInteger == true)
//                        {

//                            if (isInt(value.Trim()) == false)
//                            {
//                                Exception ex = new Exception("Number must be integer for the field [" + fieldName + "]");
//                                throw (ex);
//                            }

//                        }
//                        if (negativeAllowed == false)
//                        {
//                            if (Convert.ToDouble(GetNumericData(value.Trim())) < 0)
//                            {
//                                Exception ex = new Exception("Negative values are not allowed for the field [" + fieldName + "]");
//                                throw (ex);
//                            }
//                        }
//                    }



//                }
//                catch (Exception ex)
//                {
//                    throw (ex);
//                }
//                finally
//                {

//                }


//            }

//            ///<summary>
//            ///check whether a value is integer or not returns true if integer, 
//            ///false if floating or string containing alpahnumeric
//            ///</summary>
//            public static bool isInt(string num)
//            {

//                bool isInt;
//                int number;
//                try
//                {
//                    isInt = System.Int32.TryParse(num, out number);
//                }
//                catch (Exception ex)
//                {
//                    throw (ex);
//                }
//                finally
//                {

//                }
//                return isInt;
//            }


//            #endregion numeric

//            #region string

//            public static readonly string excelNegativePOsitiveSign = @"+#,##0.00;-#,##0.00;* ??;@";
//            public static readonly string NegativePOsitiveSign = @"+#,##0.00;-#,##0.00;0";
//            public static readonly string NumberFormatString = "#,##0.000;(#,##0.000);* ??;@";
//            public static readonly string NumberFormatStringFourDecimal = "#,##0.0000;(#,##0.0000);* ??;@";
//            public static readonly string NumberFormatStringFiveDecimal = "#,##0.00000;(#,##0.00000);* ??;@";
//            public static readonly string NumberFormatStringTwoDecimal = "#,##0.00;(#,##0.00);* ??;@";
//            public static readonly string NumberFormatStringTwoDecimalWithZero = "#,##0.00;(#,##0.00)";
//            public static readonly string NumberFormatStringInteger = "#,##0;(#,##0);* ??;@";
//            public static readonly string NumberFormatStringIntegerWithZero = "#,##0;(#,##0)";
//            public static readonly string NumberFormatStringText = "@"; //format cell data as text


//            public static object ValidLength(string str)
//            {

//                string removechar = "";
//                if (str.Trim() == "")
//                {
//                    return (object)Convert.DBNull;
//                }
//                removechar = str.Trim();
//                removechar = removechar.Replace("'", " ");

//                return (object)removechar.Trim();

//            }
//            public static object ValidLength(string str, int length)
//            {

//                string removechar = "";
//                if (str.Trim() == "")
//                {
//                    return (object)Convert.DBNull;
//                }
//                removechar = str.Trim();
//                removechar = removechar.Replace("'", " ");


//                int strLen = removechar.Length;
//                if (strLen > length)
//                    removechar = removechar.Substring(0, length);

//                return (object)removechar.Trim();

//            }
//            public static string FileNameLegalChar(string fileName)
//            {
//                string illegalChar = @"~`!@#$%^&*=/\|>,<";
//                foreach (char c in illegalChar)
//                {
//                    fileName = fileName.Replace(c.ToString(), " ");
//                }

//                return fileName;
//            }
//            private StringCollection getTableColumns(ref DataSet dsLocal)
//            {
//                StringCollection strcol = new StringCollection();
//                for (int COL = 0; COL < dsLocal.Tables[0].Columns.Count; COL++)
//                {
//                    strcol.Add(dsLocal.Tables[0].Columns[COL].ColumnName.ToUpper());
//                }

//                return strcol;

//            }
//            public static string emptyString(string str)
//            {
//                //this function returns an empty string(not a null) from null or empty or '&nbsp;' from the page
//                if (str == "&nbsp;")
//                    str = "";
//                if (string.IsNullOrEmpty(str) == true)
//                    str = "";


//                return str;
//            }//this function returns an empty string(not a null) from null or empty '&nbsp;' from the page
//            #endregion string


//            #region others
//            //public void copyDataset(DataSet source, ref DataSet destination)
//            //{
//            //    //StringCollection strColDestinationColumns = getTableColumns(ref destination);//upper case
//            //    DataRow drLocal = null;
//            //    for (int ROW = 0; ROW < source.Tables[0].Rows.Count; ROW++)
//            //    {
//            //        drLocal = destination.Tables[0].NewRow();
//            //        for (int COL = 0; COL < source.Tables[0].Columns.Count; COL++)
//            //        {
//            //            if (strColDestinationColumns.Contains(source.Tables[0].Columns[COL].ToString().ToUpper()))
//            //            {
//            //                drLocal[source.Tables[0].Columns[COL].ToString()] = ValidLength(source.Tables[0].Rows[ROW][source.Tables[0].Columns[COL].ToString()].ToString());
//            //            }
//            //        }
//            //        destination.Tables[0].Rows.Add(drLocal);
//            //    }


//            //}
//            public static string GetxlsCol(int intCol)
//            {
//                //returns excel columns based on column number. tested 1 to 256 column numbers
//                try
//                {
//                    if (intCol < 1 || intCol > 256)
//                    {
//                        System.Exception ex = new Exception("Invalid Column Value");
//                        throw (ex);
//                    }
//                    intCol = intCol - 1;
//                    int intFirstLetter = ((intCol) / 512) + 64;
//                    int intSecondLetter = ((intCol % 512) / 26) + 64;
//                    int intThirdLetter = (intCol % 26) + 65;
//                    char FirstLetter;
//                    char SecondLetter;
//                    if (intFirstLetter > 64)
//                        FirstLetter = (char)intFirstLetter;
//                    else
//                        FirstLetter = ' ';

//                    if (intSecondLetter > 64)
//                        SecondLetter = (char)intSecondLetter;
//                    else
//                        SecondLetter = ' ';

//                    char ThirdLetter = (char)intThirdLetter;
//                    return string.Concat(FirstLetter, SecondLetter, ThirdLetter).Trim();
//                }
//                catch (Exception ex)
//                {
//                    throw (ex);
//                }
//                finally
//                {

//                }
//            }//returns excel columns based on column number. tested 1 to 256 column numbers
//            #endregion others

//            public static object RetValidLen(string Data)
//            {
//                if (string.IsNullOrEmpty(Data))
//                    return DBNull.Value;

//                return Data;
//            }
//            public static double sum(string columnName, DataTable dtLocal, string criteria)
//            {
//                double total = 0;
//                DataRow[] dr = dtLocal.Select(criteria);
//                foreach (DataRow d in dr)
//                {
//                    total += dbl(d[columnName].ToString());
//                }


//                return total;
//            }
//        }



//        #region  Taufik

//        public IEnumerable<object> GetMaterialDetails(string Id)
//        {
//            try
//            {
//                var sql = @"Select  Id	
//                        ,InventoryReceiveId	
//                        ,InventoryMaterialId	
//                        ,MaterialStorageId	
//                        ,TransactionQty	
//                        ,TransactionUoMId	
//                        ,BaseQty	
//                        ,BaseUOMId	
//                        ,BaseUoMFactor	
//                        ,TransactionRate	
//                        ,TransactionAmount	
//                        ,IssueQty	
//                        ,AddedBy	
//                        ,AddedDate	
//                        ,AddedFromIP	
//                        ,UpdatedBy	
//                        ,UpdatedDate	
//                        ,UpdatedFromIP	
//                        ,TotalTaxAmount	
//                        ,BaseAmount	
//                        ,ChargesAmount	
//                        ,WithInvoiceRate	
//                        ,AfterInvoiceRate	
//                        ,CountryId	
//                        ,InventoryMaterialId	
//                        ,MaterialStorageId	
//                        ,BaseUOMId	
//                        ,TransactionQty	
//                        ,TransactionRate	
//                        ,TransactionAmount
//                        from 
//                        TRN.purchaseorderdetail pd
//                        where id=201925101";
//                return _sqlRepository.GetDataCollection(sql);
//            }
//            catch (Exception ex)
//            {
//                throw new CustomException(ex.Message, ex,
//                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
//                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
//            }
//        }


//        public DataTable loadOrderMaster(string purchaseOrderId)
//        {
//            string strSQL;
//            try
//            {
//                strSQL = @"SELECT PO.Id PONumber
//                                                ,PO.CompanyGroupId
//                                                ,PO.CompanyId
//                                                ,Plant.GSTIN
//                                                ,REPLACE(Convert(VARCHAR(11), PO.PODate, 106), ' ', '-') AS PODate
//                                                ,REPLACE(Convert(VARCHAR(11), PO.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
//                                                ,REPLACE(Convert(VARCHAR(11), PO.MatureDate, 106), ' ', '-') AS MatureDate
//		                                        ,PO.InvoicingPartyPlantId
//		                                        ,INVPARTYPL.UserName InvoicingPartyName
//                                                ,INVPARTYPL.AddressMasterId InvoicePartyAddressMasterId
//                                                ,INVPARTYPL.GSTIN InvoicingPartyGSTIN
//                                                ,ISNULL(PO.InvoicingByAddress,'') InvoicingByAddress
//												,PO.DeliveryByAddress
//		                                        ,DPARTYPL.UserName DeliveryParty
//		                                        ,PO.DeliveryPartyPlantId		
//		                                        ,POM.MaterialMasterId
//		                                        ,PO.DocRefNo
//                                                ,REPLACE(Convert(VARCHAR(11), PO.DocDate, 106), ' ', '-') AS DocDate
//		                                        ,PO.AddedBy
//		                                        ,PO.AddedDate
//		                                        ,PO.UpdatedBy
//		                                        ,PO.UpdatedDate
//		                                        ,PO.IsApproved 
//		                                        ,PO.PartyType
//												,PO.PartyId
//                                                ,ISNULL(PO.DeliveryInstruction,'') DeliveryInstruction
//												,ISNULL(PO.SpecialInstruction,'') SpecialInstruction
//												,Party.UserName VendorName
//                                                ,Party.AddressMasterId VendorAddressMasterId
//                                                ,Party.TINNO VendorGSTIN
//		                                        ,Case When PO.IsNonCreditable = 1 then 'NonCreditable' when Po.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
//		                                        ,PO.CurrencyId
//	                                            ,CRNC.Code AS CurrencyName
//	                                            ,PO.ToCurrencyRate
//		                                        ,BASECRNC.Code AS BaseCurrencyName
//		                                        ,PayTerm.UserName PaymentTerm
//	                                          ,MM.UserName MaterialMaster
//	                                          ,MM.MaterialGroupMasterId
//	                                          ,MGM.UserName MaterialGroupMaster
//	                                          ,POM.ArticleId
//	                                          ,MMA.StandardName Article
//	                                          ,FC.Id FirstCharId
//	                                          ,FC.UserName FirstChar
//                                              ,POM.FirstCharacteristicsValueId
//	                                          ,FCV.UserName AS FirstCharacteristicsValue
//                                              ,POM.SecondCharacteristicsValueId
//	                                          ,SCV.UserName AS SecondCharacteristicsValue
//	                                          ,POM.ThirdCharacteristicsValueId
//	                                          ,TCV.UserName AS ThirdCharacteristicsValue
//	                                          ,SC.Id SecondCharId
//	                                          ,SC.UserName SecondChar
//	                                          ,TC.Id ThirdCharId
//	                                          ,TC.UserName ThirdChar
//	                                          ,ROUND(POD.TransactionQty, 2) POTransactionQty
//	                                          ,ROUND(POD.TransactionRate, 2) TransactionRate
//	                                          ,ROUND((POD.TransactionQty * POD.TransactionRate), 2) AS TrnAmount
//	                                          ,POD.BaseAmount
//	                                          ,POD.TotalTaxAmount AS BaseTaxAmount
//	                                          ,TaxAmount = (
//		                                            SELECT SUM(TaxAmount)
//		                                            FROM [TRN].[PurchaseOrderTax]
//		                                            WHERE InventoryReceiveDetailId = POD.Id
//		                                            )
//	                                          ,ServiceTaxAmount = (
//		                                            SELECT SUM(TotalTaxAmount)
//		                                            FROM [TRN].[POService]
//		                                            WHERE InventoryReceiveId = POM.Id
//		                                            )
//	                                          ,POD.ChargesAmount
//	                                          ,POD.CountryId
//	                                          ,POCountry.UserName CountryOfOrigin
//                                                ,POD.Id PurchaseOrderDetailId
//	                                          ,POD.TransactionUoMId
//	                                          ,TUoM.UserName AS TransactionUoM
//                                              FROM TRN.PurchaseOrder PO
//                                         LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = PO.CompanyGroupId
//                                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = PO.CompanyId
//                                         LEFT JOIN ORG.Plant Plant ON Plant.Id = PO.PlantId
//                                         LEFT JOIN SCS.Currency CRNC ON CRNC.Id = PO.CurrencyId
//                                         LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = PO.BaseCurrencyId
//                                         LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = PO.PaymentTermId
//                                         LEFT JOIN HKP.PartyPlant  INVPARTYPL ON INVPARTYPL.Id = PO.InvoicingPartyPlantId
//                                         LEFT JOIN HKP.PartyPlant  DPARTYPL ON DPARTYPL.Id = PO.DeliveryPartyPlantId                                          
//                                         LEFT JOIN TRN.PurchaseOrderDetail POD ON PO.Id = POD.InventoryReceiveId
//                                         LEFT JOIN SCS.Country POCountry ON POD.CountryId = POCountry.Id
//										 LEFT JOIN HKP.Party Party ON Party.Id = PO.PartyId                                        
//                                         LEFT JOIN TRN.POMaterial AS POM ON POD.InventoryMaterialId = POM.Id
//                                         INNER JOIN MST.MaterialMaster AS MM ON MM.Id = POM.MaterialMasterId
//                                         INNER JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
//                                         LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = POM.ArticleId
//                                         LEFT JOIN HKP.Characteristics AS FC ON POM.FirstCharacteristicsId = FC.Id
//                                         LEFT JOIN HKP.Characteristics AS SC ON POM.SecondCharacteristicsId = SC.Id
//                                         LEFT JOIN HKP.Characteristics AS TC ON POM.ThirdCharacteristicsId = TC.Id
//                                         LEFT JOIN HKP.CharacteristicsValue AS FCV ON POM.FirstCharacteristicsValueId = FCV.Id
//                                         LEFT JOIN HKP.CharacteristicsValue AS SCV ON POM.SecondCharacteristicsValueId = SCV.Id
//                                         LEFT JOIN HKP.CharacteristicsValue AS TCV ON POM.ThirdCharacteristicsValueId = TCV.Id
//                                         LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON POD.TransactionUoMId = TUoM.Id
//                                         WHERE PO.Id = '" + purchaseOrderId + @"' ";

//                return _sqlRepository.GetDataTable(strSQL);

//            }
//            catch (System.Exception ex)
//            {
//                throw (ex);
//            }
//            finally
//            {

//            }
//        }







//        public DataTable RequsitionMaster(string purchaseOrderId)
//        {
//            string strSQL;
//            try
//            {
//                strSQL = @"SELECT IM.Id
//     , IM.Id AS MaterialReqqusitionMasterId
//    , MGM.UserName AS MaterialGroupMasterName
//    , IM.MaterialMasterId, MM.UserName
//    , IM.ArticleId, ART.StandardName
//    , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
//    , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
//    , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
//    , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
//    , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
//    , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
//    , ROUND(IM.TransactionQty,2) TransactionQty
//	, IM.TransactionUoMId
//	, TUoM.UserName AS TransactionUoM
//	, ROUND(IM.EstimatedRate,2) TransactionRate 
//	, CU.Code AS CurrencyName

//    , ROUND((IM.TransactionQty*IM.EstimatedRate),2) AS TrnAmount   
//    ,IM.MaterialDetail
//    ,Replace(CONVERT(VARCHAR(11), IM.DeliveryDate, 106), ' ', '-') DeliveryDate

//	,Act.Id As Activity
//	,Act.UserName As ActivityName
//	,IM.BudgetType
//	,IM.Reason
//	,IM.Remarks
//	,IM.FutureReqApp
//	--,BudgetMasterId
//	--,GLGeneralInfoId
//FROM TRN.MaterialRequsitionDetails AS IM
//JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
//LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
//LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
//LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
//LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
//LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
//LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
//LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
//LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id

//JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IM.TransactionUoMId=TUoM.Id
//JOIN [TRN].[MaterialRequsitionMaster] AS IR ON IM.MaterialReqqusitionMasterId=IR.Id
//JOIN [SCS].[Currency] AS CU ON IM.CurrencyId=CU.Id 
//JOIN [HKP].[Activity] As Act On ACT.Id=IM.ActivityId
//--JOIN [HKP].Budget
//--JOIN [HKP].Gl
//WHERE IM.MaterialReqqusitionMasterId=inventoryReceiveId ";

//                return _sqlRepository.GetDataTable(strSQL);

//            }
//            catch (System.Exception ex)
//            {
//                throw (ex);
//            }
//            finally
//            {

//            }
//        }
//        public DataTable loadServicerMasterItems(string purchaseOrderId)
//        {
//            string strSQL;

//            try
//            {
//                strSQL = @"SELECT POS.Id ServiceId,SM.UserName  Service ,POS.Amount,POS.TotalTaxAmount,Pos.AddedBy,pos.AddedDate,pos.UpdatedBy,pos.UpdatedDate FROM TRN.PurchaseOrder PO
//                            INNER join TRN.POService POS ON POS.InventoryReceiveId = PO.Id
//                            INNER JOIN HKP.ServiceMaster SM ON POS.ServiceMasterId = SM.Id 
//                            where PO.Id = '" + purchaseOrderId + @"'";


//                return _sqlRepository.GetDataTable(strSQL);

//            }
//            catch (System.Exception ex)
//            {
//                throw (ex);
//            }
//            finally
//            {

//            }
//        }
//        public DataTable loadServiceMasterTax(string purchaseOrderId)
//        {
//            string strSQL;

//            try
//            {
//                strSQL = @"SELECT InventoryServiceId,PO.Id PurchaseOrderId,tg.Code AS TaxCode,PODT.Percentage, PODT.TaxAmount from TRN.PurchaseOrder PO
//                            INNER JOIN TRN.POService POS ON POS.InventoryReceiveId = PO.Id
//                            INNER JOIN TRN.PurchaseOrderTax PODT ON PODT.InventoryReceiveId = PO.Id and PODT.InventoryServiceId = POS.Id
//                              LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=PODT.TaxCategoryId
//                                WHERE PO.Id='" + purchaseOrderId + @"' 
//								AND InventoryServiceId   IS NOT NULL AND  InventoryReceiveDetailId IS NULL 
//								 ORDER BY tg.[Sequence] ";


//                return _sqlRepository.GetDataTable(strSQL);
//            }
//            catch (System.Exception ex)
//            {
//                throw (ex);
//            }
//            finally
//            {

//            }
//        }
//        #endregion



//        public DataTable GetPurchaseOrderSqlData(string purchaseOrderId)
//        {
//            try
//            {
//                string sqlTxt = @"SELECT PO.Id PONumber
//                                                ,PO.CompanyGroupId
//                                                ,PO.CompanyId
//                                                ,REPLACE(Convert(VARCHAR(11), PO.PODate, 106), ' ', '-') AS PODate
//                                                ,REPLACE(Convert(VARCHAR(11), PO.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
//                                                ,REPLACE(Convert(VARCHAR(11), PO.MatureDate, 106), ' ', '-') AS MatureDate
//		                                        ,PO.InvoicingPartyPlantId
//		                                        ,INVPARTYPL.UserName InvoiceParty
//		                                        ,PO.InvoicingByAddress
//		                                        ,PO.DeliveryByAddress
//		                                        ,DPARTYPL.UserName DeliveryParty
//		                                        ,PO.DeliveryPartyPlantId		
//		                                        ,POM.MaterialMasterId
//		                                        ,PO.DocRefNo
//		                                        ,PO.DocDate
//		                                        ,PO.AddedBy
//		                                        ,PO.AddedDate
//		                                        ,PO.UpdatedBy
//		                                        ,PO.UpdatedDate
//		                                        ,PO.IsApproved
//		                                        ,PO.PartyType
//		                                        ,PO.IsNonCreditable
//		                                        ,PO.CurrencyId
//	                                            ,CRNC.Code AS CurrencyName
//	                                            ,PO.ToCurrencyRate
//		                                        ,BASECRNC.Code AS BaseCurrencyName
//		                                        ,PayTerm.UserName PaymentTerm
//	                                          ,MM.UserName MaterialMaster
//	                                          ,MM.MaterialGroupMasterId
//	                                          ,MGM.UserName MaterialGroupMaster
//	                                          ,POM.ArticleId
//	                                          ,MMA.StandardName Article
//	                                          ,FC.Id FirstCharId
//	                                          ,FC.UserName FirstChar
//                                              ,POM.FirstCharacteristicsValueId
//	                                          ,FCV.UserName AS FirstCharacteristicsValue
//                                              ,POM.SecondCharacteristicsValueId
//	                                          ,SCV.UserName AS SecondCharacteristicsValue
//	                                          ,POM.ThirdCharacteristicsValueId
//	                                          ,TCV.UserName AS ThirdCharacteristicsValue
//	                                          ,SC.Id SecondCharId
//	                                          ,SC.UserName SecondChar
//	                                          ,TC.Id ThirdCharId
//	                                          ,TC.UserName ThirdChar
//	                                          ,ROUND(POD.TransactionQty, 2) POTransactionQty
//	                                          ,ROUND(POD.TransactionRate, 2) TransactionRate
//	                                          ,ROUND((POD.TransactionQty * POD.TransactionRate), 2) AS TrnAmount
//	                                          ,POD.BaseAmount
//	                                          ,POD.TotalTaxAmount AS BaseTaxAmount
//	                                          ,TaxAmount = (
//		                                            SELECT SUM(TaxAmount)
//		                                            FROM [TRN].[PurchaseOrderTax]
//		                                            WHERE InventoryReceiveDetailId = POD.Id
//		                                            )
//	                                          ,ServiceTaxAmount = (
//		                                            SELECT SUM(TotalTaxAmount)
//		                                            FROM [TRN].[POService]
//		                                            WHERE InventoryReceiveId = POM.Id
//		                                            )
//	                                          ,POD.ChargesAmount
//	                                          ,POD.CountryId

//	                                          ,POD.TransactionUoMId
//	                                          ,TUoM.UserName AS TransactionUoM
//                                              FROM TRN.PurchaseOrder PO
//                                         LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = PO.CompanyGroupId
//                                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = PO.CompanyId
//                                         LEFT JOIN ORG.Plant Plant ON Plant.Id = PO.PlantId
//                                         LEFT JOIN SCS.Currency CRNC ON CRNC.Id = PO.CurrencyId
//                                         LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = PO.BaseCurrencyId
//                                         LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = PO.PaymentTermId
//                                         LEFT JOIN HKP.PartyPlant  INVPARTYPL ON INVPARTYPL.Id = PO.InvoicingPartyPlantId
//                                         LEFT JOIN HKP.PartyPlant  DPARTYPL ON DPARTYPL.Id = PO.DeliveryPartyPlantId 
//                                         LEFT JOIN TRN.PurchaseOrderDetail POD ON PO.Id = POD.InventoryReceiveId

//                                         LEFT JOIN TRN.POMaterial AS POM ON POD.InventoryMaterialId = POM.Id
//                                         INNER JOIN MST.MaterialMaster AS MM ON MM.Id = POM.MaterialMasterId
//                                         INNER JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
//                                         INNER JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = POM.ArticleId
//                                         LEFT JOIN HKP.Characteristics AS FC ON POM.FirstCharacteristicsId = FC.Id
//                                         LEFT JOIN HKP.Characteristics AS SC ON POM.SecondCharacteristicsId = SC.Id
//                                         LEFT JOIN HKP.Characteristics AS TC ON POM.ThirdCharacteristicsId = TC.Id
//                                         LEFT JOIN HKP.CharacteristicsValue AS FCV ON POM.FirstCharacteristicsValueId = FCV.Id
//                                         LEFT JOIN HKP.CharacteristicsValue AS SCV ON POM.SecondCharacteristicsValueId = SCV.Id
//                                         LEFT JOIN HKP.CharacteristicsValue AS TCV ON POM.ThirdCharacteristicsValueId = TCV.Id
//                                         JOIN [SCS].[UnitOfMeasurement] AS TUoM ON POD.TransactionUoMId = TUoM.Id
//                                         WHERE PO.Id = '" + purchaseOrderId + @"' and PO.IsApproved = 0";

//                return _sqlRepository.GetDataTable(sqlTxt);
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//        }

//        #endregion




        #region Taufik GateEntryReport 
        public void GateEntryReport(string CompanyGroupId, string plantId, string GateEntryId)
        {
            ReportUtility ru = new ReportUtility();

            var fileName = "";
            var strPath = "";

            var File = "";

            fileName = "GateEntry" + plantId + ".docx";
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
                dsOrderMaster = loadOrderGeteEntry(GateEntryId);//sql
                Dictionary<string, string> columns = new Dictionary<string, string>();
                var poApprovedStatus = "";

        //document.Replace("{DeliveryInstruction}", dsOrderMaster.Rows[0]["DeliveryInstruction"].ToString(), false, false);
        document.Replace("{Id}", dsOrderMaster.Rows[0]["Id"].ToString(), false, false);
                //document.Replace("{CompanyGroupId}", dsOrderMaster.Rows[0]["CompanyGroupId"].ToString(), false, false);
                document.Replace("{EntryDate}", dsOrderMaster.Rows[0]["EntryDate"].ToString(), false, false);
                document.Replace("{PartyCode}", dsOrderMaster.Rows[0]["PartyCode"].ToString(), false, false);
                document.Replace("{ModeofTransport}", dsOrderMaster.Rows[0]["ModeofTransport"].ToString(), false, false);
                document.Replace("{Description}", dsOrderMaster.Rows[0]["Description"].ToString(), false, false);
                //document.Replace("{AddedBy}", dsOrderMaster.Rows[0]["AddedBy"].ToString(), false, false);
                document.Replace("{Remarks}", dsOrderMaster.Rows[0]["Remarks"].ToString(), false, false);
				document.Replace("{MaterialReceivedBy}", dsOrderMaster.Rows[0]["MaterialReceivedBy"].ToString(), false, false);
				//document.Replace("{EntryDate}", dsOrderMaster.Rows[0]["EntryDate"].ToString(), false, false);
				document.Replace("{EntryTime}", dsOrderMaster.Rows[0]["EntryTime"].ToString(), false, false);
				foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);
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

                document.Save(fileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();

            }
            catch (Exception ex)
            {
                throw ex;

            }
            //Closes the instance of document objects
            document.Close();
        }
        public DataTable loadOrderGeteEntry(string GateEntryId)
        {
            string strSQL;
            //clsConnection objCon;
            try
            {
                strSQL = @"SELECT
                                GTE.Id
                                ,GTE.CompanyGroupId
                                --,GTE.EntryDate
                                ,P.Code PartyCode
                                ,GTE.Description
                                ,GTE.PackageQty
                                ,GTE.ModeofTransport
                                ,GTE.Bill
                                ,GTE.PersonName
                                ,GTE.MobileNo
                                ,GTE.Remarks
                                ,GTE.AddedBy
                                ,GTE.AddedDate
                                ,GTE.AddedFromIP
                                ,GTE.UpdatedBy
                                ,GTE.UpdatedDate
                                ,GTE.UpdatedFromIp
                                ,E2.EmployeeName GateNumforEmployee
	                            ,P.UserName As PartyName,EI.FirstName As MaterialReceivedBy
                                 	 ,Replace(CONVERT(VARCHAR(11),GTE.EntryDate, 106), ' ', '-') EntryDate 
								,convert(varchar, GTE.GateEntryTime, 2) EntryTime
							
                                FROM TRN.GateEntry GTE
	                            Left JOIn hkp.Party p on p.Id=GTE.PartyId
                                 	LEFT JOin dbo.EmployeeInformation EI ON  EI.SystemId=GTE.EmployeeId
                                    LEFT JOin dbo.EmployeeInformation E2 ON  E2.SystemId= GTE.EmployeeIdForGateEntry
	                            Where  GTE.Id='" + GateEntryId + @"'";

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







        public DataTable RequsitionMaster(string purchaseOrderId)
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
        public void PoApproved(string PoId, string PoValue,string CheckedStataus, string AuthorizedBy)
        {
            try
            {
                var AuthorizedById = "";
                
                PoValue = "0";
                var Id = GetPK();
                if(CheckedStataus=="Checked")
                {
                    if(AuthorizedBy==null || AuthorizedBy =="")
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
                string _sql = "Update TRN.PurchaseOrder set IsApproved='0',CheckedByStatus='"+ Status + "',AuthorizedBy='" + AuthorizedById + "' where id='" + PoId + "'";
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
                string _sql = "Update TRN.PurchaseOrder set AuthorizedByStatus='" + Status + "',IsApproved='"+ IsApproved + "', AuthorizedBy='" + identity.EmployeeId + "' where id='" + PoId + "'";
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
               // var res =_inventoryReceiveRepository.SqlQuery<Int32>($"select distinct statusflag=Case when B.POId is not null then 1 else 0 end	from trn.PurchaseOrder A	Left JOIN trn.InventoryReceiveDetail B on B.POID=A.Id where A.Id='"+PoId+"'").First();               

               //if(Convert.ToBoolean(res))
               // {
               //     throw new CustomException("You can not un Approved the PO? GRN already Received");
               // }
               // else
               // {
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
                // var res =_inventoryReceiveRepository.SqlQuery<Int32>($"select distinct statusflag=Case when B.POId is not null then 1 else 0 end	from trn.PurchaseOrder A	Left JOIN trn.InventoryReceiveDetail B on B.POID=A.Id where A.Id='"+PoId+"'").First();               

                //if(Convert.ToBoolean(res))
                // {
                //     throw new CustomException("You can not un Approved the PO? GRN already Received");
                // }
                // else
                // {
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
        public IEnumerable<object> getTaxCategoryListForFGService(string companyGroupId, string plantId, string hsnCodeId,string partyPlantId)
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



        //public IEnumerable<object> GetSupervisorCbo()
        //{
        //    try
        //    {
        //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //        var sql = @"select E.SystemId As Value, E.EmployeeName As Text from dbo.AuthorizationConfig A 
        //                  Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
        //                  where A.PlantId='" + identity.PlantId+ "' AND A.ActionStatus='CheckedBy'";
        //        return _sqlRepository.GetDataCollection(sql);

        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
        //    }
        //}

        public IEnumerable<object> GetSupervisorCbo()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select E.SystemId As Value, E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where A.PlantId='" + identity.PlantId + "' AND A.ActionStatus='CheckedBy'";
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
                var sql = @"select E.SystemId As Value, E.SystemId+'-'+E.EmployeeName As Text from dbo.AuthorizationConfig A 
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
     
        public IEnumerable<object> GetAllReqdata(string IsSysAdmin, string UserId, string plantId) 
        {
			var sql = "";

			try
            {
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				if (IsSysAdmin == "True")
				{
					sql = @"SELECT G.Id,G.PlantWiseGateId
                          ,ISNULL(PWG.UserName,'') GateName
                          ,Replace(CONVERT(VARCHAR(11),G.EntryDate, 106), ' ', '-') EntryDate 
                           ,Replace(CONVERT(VARCHAR(11),G.EntryDate, 106), ' ', '-') EntryDate1 
                          ,p.Code PartyCode,P.Id PartyId
						  ,isnull(p.UserName,'') PartyName
						  ,CG.UserName CompanyGrpName
						  ,C.UserName CompanyName
						  ,Pl.UserName PlantName
	                      --,isnull(P.UserName,'') As PartyName
                          ,isnull(G.Description,'') Description
                          ,G.PackageQty
                          ,G.ModeofTransport
                          ,G.Bill
                          ,G.PersonName
                          ,G.MobileNo,ISNULL(G.LocalImported,'') LocalImported
                          ,Isnull(G.Remarks,'') Remarks
                          ,G.AddedBy
                          ,G.AddedDate
                          ,G.AddedFromIP
                          ,G.UpdatedBy
                          ,G.UpdatedDate
                          ,G.UpdatedFromIp
                          ,EI.EmployeeName As MaterialReceivedBy,G.GateEntryTime
                          ,IR.Id GRNId
						  ,EI1.SystemId +'-'+  EI1.EmployeeName EmployeeName
                          ,EI.SystemId +'-'+   EI.EmployeeName  EmployeeName1
                      FROM TRN.[GateEntry] G
                      LEFT Join hkp.Party p ON P.Id= G.PartyId
					  LEFT Join ORG.CompanyGroup CG ON CG .Id= G.CompanyGroupId
					  LEFT Join ORG.Company C ON C.Id= G.CompanyId
					  LEFT Join ORG.Plant Pl ON Pl.Id= G.PlantId
                      Left join trn.InventoryReceive IR ON IR.GateEntryNo=G.Id
                      LEFT JOin dbo.EmployeeInformation EI ON  EI.SystemId=G.EmployeeId
                      LEFT JOin dbo.EmployeeInformation EI1 ON  EI1.SystemId=G.EmployeeIdForGateEntry
                      LEFT JOIN dbo.PlantWiseGate PWG ON PWG.Id=G.PlantWiseGateId
                      --Left JOIN SEC.UserPlantGate UPG ON UPG.PlantGateId=PWG.Id
                      Where G.FlagStatus!='Cancel' AND G.PlantId='" + identity.PlantId + @"' --and G.Id not in (select GateEntryNo from trn.InventoryReceive)
                    Order By G.AddedDate Desc";
				}
				else
				{
					sql = @"SELECT G.Id,G.PlantWiseGateId
                          ,ISNULL(PWG.UserName,'') GateName
                          ,Replace(CONVERT(VARCHAR(11),G.EntryDate, 106), ' ', '-') EntryDate 
                           ,Replace(CONVERT(VARCHAR(11),G.EntryDate, 106), ' ', '-') EntryDate1 
                          ,p.Code PartyCode,P.Id PartyId
						  ,Isnull(p.UserName,'') PartyName
						  ,CG.UserName CompanyGrpName
						  ,C.UserName CompanyName
						  ,Pl.UserName PlantName
	                      --,isnull(P.UserName,'') As PartyName
                          ,isnull(G.Description,'') Description
                          ,G.PackageQty
                          ,G.ModeofTransport,ISNULL(G.LocalImported,'') LocalImported 
                          ,G.Bill
                          ,G.PersonName
                          ,G.MobileNo
                          ,Isnull(G.Remarks,'') Remarks
                          ,G.AddedBy
                          ,G.AddedDate
                          ,G.AddedFromIP
                          ,G.UpdatedBy
                          ,G.UpdatedDate
                          ,G.UpdatedFromIp
                           ,EI.EmployeeName As MaterialReceivedBy,G.GateEntryTime
                          ,IR.Id GRNId
,EI1.SystemId +'-'+  EI1.EmployeeName EmployeeName
                      FROM TRN.[GateEntry] G
                      LEFT Join hkp.Party p ON P.Id= G.PartyId
					  LEFT Join ORG.CompanyGroup CG ON CG .Id= G.CompanyGroupId
					  LEFT Join ORG.Company C ON C.Id= G.CompanyId
					  LEFT Join ORG.Plant Pl ON Pl.Id= G.PlantId
                      Left join trn.InventoryReceive IR ON IR.GateEntryNo=G.Id
                      LEFT JOin dbo.EmployeeInformation EI ON  EI.SystemId=G.EmployeeId
                      LEFT JOin dbo.EmployeeInformation EI1 ON  EI1.SystemId=G.EmployeeIdForGateEntry
                      LEFT JOIN dbo.PlantWiseGate PWG ON PWG.Id=G.PlantWiseGateId
                      Left JOIN SEC.UserPlantGate UPG ON UPG.PlantGateId=PWG.Id
                      Where G.FlagStatus!='Cancel' AND G.PlantId='" + identity.PlantId + @"' AND UPG.UserId='" + UserId + @"'--and G.Id not in (select GateEntryNo from trn.InventoryReceive)
                      Order By G.AddedDate Desc";

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
                var sql = @"SELECT G.Id
                              ,G.EntryDate
                              ,G.PartyCode 
	                          ,P.UserName As PartyName
                              ,G.Description
                              ,G.PackageQty
                              ,G.ModeofTransport
                              ,G.Bill
                              ,G.PersonName
                              ,G.MobileNo
                              ,G.Remarks
                              ,G.AddedBy
                              ,G.AddedDate
                              ,G.AddedFromIP
                              ,G.UpdatedBy
                              ,G.UpdatedDate
                              ,G.UpdatedFromIp
                              ,G.PlantWiseGateId
                              ,G.GateEntryTime
                            ,ep.FirstName as EmployeeName
							,ep1.FirstName as ResponsiblePersonName
							,G.GateEntryType
                          FROM TRN.[GateEntry] G
                          LEFT Join hkp.Party p ON P.Id= G.PartyCode
						  left join dbo.EmployeeInformation ep on ep.SystemId=G.EmployeeId
						   left join dbo.EmployeeInformation ep1 on ep1.SystemId=G.EmployeeIdForGateEntry
                    where G.Id='" + Id + @"'";
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

        public void UpdateMaterial(IEnumerable<MaterialRequisitionDetailViewModel> entity, IEnumerable<PurchaseOrderTax> receiveTaxList)
        {
            try
            {
                //int currentId = 0;
                //foreach (var item1 in inventoryMaterialList)
                //{
                //    if (string.IsNullOrEmpty(item1.Id))
                //    {
                //        currentId++;
                //        item1.Id = MakePK(item1.Id, currentId, 2);                       
                //        AuditService.AddedLog(item1);
                //        _receiveTaxRepository.Insert(item1);
                //    }
                //    else
                //    {
                //        AuditService.UpdatedLog(item1);
                //        _receiveTaxRepository.Update(item1);
                //    }
                //}

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
                        //if (item1.BaseTaxAmount == null)
                        //{
                        //    item1.BaseTaxAmount = "0.00";
                        //}
                        //string _sql = "Update TRN.purchaseOrderDetail set TransactionQty='" + item1.TransactionQty + "', BaseQty='" + item1.TransactionQty + "',BaseUOMFactor='" + item1.TransactionQty + "', TransactionRate='" + item1.TransactionRate + "', TransactionAmount='" + item1.TrnAmount + "',TotalTaxAmount='" + item1.BaseTaxAmount + "' ,BaseAmount='" + item1.TrnAmount + "',Description ='" + item1.Description + "',UpdatedBy='" + UpdatedBy + "',UpdatedDate='" + updatedDate + "',UpdatedFromIP='" + ip + "'  where id='" + ReqDetailId + "'";
                        //var _sql = @"UPDATE [TRN].[MaterialRequsitionDetails]
                        //       SET 
                                 
                        //          //[ActivityId] = '" + item1.ActivityId + "'" +
                        //         //",[MaterialMasterId] = '" + item1.MaterialMasterId + "'" +
                        //         //",[ArticleId] = '" + item1.ArticleId + "'" +
                        //         //",[FirstCharacteristicsId] = '" + item1.FirstCharacteristicsId + "'" +
                        //         //",[FirstCharacteristicsValueId] = '" + item1.FirstCharacteristicsValueId + "'" +
                        //         //",[SecondCharacteristicsId] = '" + item1.SecondCharacteristicsId + "'" +
                        //         //",[SecondCharacteristicsValueId] = '" + item1.SecondCharacteristicsValueId + "'" +
                        //         //",[ThirdCharacteristicsId] = '" + item1.ThirdCharacteristicsId + "'" +
                        //         //",[ThirdCharacteristicsValueId] = '" + item1.ThirdCharacteristicsValueId + "'" +
                        //         //",[MaterialDetail] = '" + item1.MaterialDetail + "'" +
                        //         //",[TransactionUoMId] = '" + item1.TransactionUoMId + "'" +
                        //         //",[CurrencyId] = '" + item1.CurrencyId + "'" +
                        //         "[TransactionQty] = '" + item1.TransactionQty + "'" +
                        //         ",[EstimatedRate] = '" + item1.EstimatedRate + "'" +
                        //         ",[TotalAmount] = '" + item1.TotalAmount + "'" +
                        //         //",[BudgetType] = '"+item1.BudgetType + "'" +
                        //         //",[Reason] = '"+item1.Reason + "'" +
                        //         //",[Remarks] = '"+item1.Remarks + "'--" +
                        //         //",[QualityApprovalResponsiblePersonId] = '"+item1.QualityApprovalResponsiblePersonId + "'" +
                        //         //",[NeedSpecialAppId] = '"+item1.NeedSpecialAppId + "'" +
                        //         //",[FutureReqApp] = '"+item1.FutureReqApp + "'" +
                        //         //",[DeliveryDate] = '"+item1.DeliveryDate + "'" +
                        //         ",[UpdatedBy] = '" + identity.UserId + "'" +
                        //         ",[UpdatedDate] = '" + DateTime.Now + "'" +
                        //         ",[UpdatedFromIP] = '" + identity.IPAddress + "'" +
                        //         ",[BudgetMasterId] = '" + item1.BudgetMasterId + "'" +
                        //         ",[GLGeneralInfoId] = '" + item1.GLGeneralInfoId + "'" +
                        //         " where id = '" + ReqDetailId + "'";
                        var _sql = "UPDATE [TRN].[MaterialRequsitionDetails]   SET [MaterialDetail] = '" + item1.MaterialDetail + "',[TransactionQty] =  '" + Convert.ToDecimal(item1.TransactionQty) + "',[EstimatedRate] = '"+ Convert.ToDecimal(item1.EstimatedRate) + "',[TotalAmount] = '"+ Convert.ToDecimal(item1.TotalAmount) + "',[UpdatedBy] = '"+ identity.UserId+ "',[UpdatedDate] = '"+Convert.ToDateTime(DateTime.Now)+"',[UpdatedFromIP] = '"+identity.IPAddress+"' where id = '" + ReqDetailId + "'";
                        _sqlRepository.ExecuteSqlCommand(_sql);
                    }
                }
                //if (receiveTaxList.IsNotNull())
                //{
                //    // var currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId='{entity.InventoryReceiveDetailId}'").First();
                //    foreach (var item in receiveTaxList)
                //    {
                //        // currentId++;
                //        //item.Id = MakePK(entity.InventoryReceiveDetailId, currentId, 2);
                //        //item.InventoryReceiveId = entity.InventoryReceiveId;
                //        //item.InventoryReceiveDetailId = entity.InventoryReceiveDetailId;
                //        //item.InventoryServiceId = null;

                //        //AuditService.AddedLog(item);
                //        //_receiveTaxRepository.Insert(item);

                //        var TaxAutoId = item.Id;
                //        string _sql1 = "Update TRN.PurchaseOrderTax set TaxAmount='" + item.TaxAmount + "' where id='" + TaxAutoId + "' ";
                //        _sqlRepository.ExecuteSqlCommand(_sql1);
                //        //AuditService.UpdatedLog(item);
                //        //_receiveTaxRepository.Update(item);
                //    }
                //}
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
                //var detail = Convert.ToBoolean(_inventoryReceiveRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM TRN.MaterialRequsitionDetails WHERE Id='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                ////var service = Convert.ToBoolean(_inventoryReceiveRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM TRN.InventoryService WHERE InventoryReceiveId='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                //if (!detail)
                //{
                    
                    var data = _materialRequsitionDetailsRepository.Find(id);
                    if (data.IsNull()) throw new CustomException(ServiceResources.RecordNoLonger);
                    _materialRequsitionDetailsRepository.Delete(data.Id);
                _unitOfWork.SaveChanges();
                //}
                //else throw new CustomException("Please delete first line item.");
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
                var detail = Convert.ToBoolean(_gatePassMasterRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM TRN.InventoryReceive WHERE GateEntryNo='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                //var service = Convert.ToBoolean(_inventoryReceiveRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM TRN.InventoryService WHERE InventoryReceiveId='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                if (!detail)
                {
                    var data = base.Find(id);
                    if (data.IsNull()) 
                        throw new CustomException(ServiceResources.RecordNoLonger);
                    _gatePassMasterRepository.Delete(data);
                    _unitOfWork.SaveChanges();
                }
                else throw new CustomException("You Can not delete the Gate entry!Already used in GRN.");
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public void DeleteGatePass(string id)
        {
            try
            {
                var detail = Convert.ToBoolean(_gatePassMasterRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM TRN.GatePassDetails WHERE GatePassMasterId='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                //var service = Convert.ToBoolean(_inventoryReceiveRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM TRN.InventoryService WHERE InventoryReceiveId='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                if (!detail)
                {
                    var data = _gatePassMasterRepository.Find(id);
                    if (data.IsNull()) 
                        throw new CustomException(ServiceResources.RecordNoLonger);
                    _gatePassMasterRepository.Delete(data);
                    _unitOfWork.SaveChanges();
                }
                else throw new CustomException("First delete your line items");
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public void CancelGateEntry(string id)
		{
			try
			{
                string Res = _gateEntryRepository.SqlQuery<string>(@"Select GateEntryNo from TRn.InventoryReceive  where GateEntryNo='" + id + "'").FirstOrDefault();
                if(string.IsNullOrEmpty(Res))
                {
                    string _sql2 = "Update TRN.GateEntry set FlagStatus='Cancel' where id='" + id + "'";
                    _gateEntryRepository.ExecuteSqlCommand(_sql2);
                }
                else
                {
                    throw new CustomException("Gate No Already used in the GRN.So you can not cancel this Gate no.");
                }
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
        #endregion

        //Command

        //public void RequisitionReportby(string CompanyGroupId, string RequisitionId)
        //{
        //    try
        //    {
        //        string sqlTxt = @"SELECT PO.Id PONumber
        //                                        ,PO.CompanyGroupId
        //                                        ,PO.CompanyId
        //                                        ,REPLACE(Convert(VARCHAR(11), PO.PODate, 106), ' ', '-') AS PODate
        //                                        ,REPLACE(Convert(VARCHAR(11), PO.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
        //                                        ,REPLACE(Convert(VARCHAR(11), PO.MatureDate, 106), ' ', '-') AS MatureDate
        //                                  ,PO.InvoicingPartyPlantId
        //                                  ,INVPARTYPL.UserName InvoiceParty
        //                                  ,PO.InvoicingByAddress
        //                                  ,PO.DeliveryByAddress
        //                                  ,DPARTYPL.UserName DeliveryParty
        //                                  ,PO.DeliveryPartyPlantId		
        //                                  ,POM.MaterialMasterId
        //                                  ,PO.DocRefNo
        //                                  ,PO.DocDate
        //                                  ,PO.AddedBy
        //                                  ,PO.AddedDate
        //                                  ,PO.UpdatedBy
        //                                  ,PO.UpdatedDate
        //                                  ,PO.IsApproved
        //                                  ,PO.PartyType
        //                                  ,PO.IsNonCreditable
        //                                  ,PO.CurrencyId
        //                                     ,CRNC.Code AS CurrencyName
        //                                     ,PO.ToCurrencyRate
        //                                  ,BASECRNC.Code AS BaseCurrencyName
        //                                  ,PayTerm.UserName PaymentTerm
        //                                   ,MM.UserName MaterialMaster
        //                                   ,MM.MaterialGroupMasterId
        //                                   ,MGM.UserName MaterialGroupMaster
        //                                   ,POM.ArticleId
        //                                   ,MMA.StandardName Article
        //                                   ,FC.Id FirstCharId
        //                                   ,FC.UserName FirstChar
        //                                      ,POM.FirstCharacteristicsValueId
        //                                   ,FCV.UserName AS FirstCharacteristicsValue
        //                                      ,POM.SecondCharacteristicsValueId
        //                                   ,SCV.UserName AS SecondCharacteristicsValue
        //                                   ,POM.ThirdCharacteristicsValueId
        //                                   ,TCV.UserName AS ThirdCharacteristicsValue
        //                                   ,SC.Id SecondCharId
        //                                   ,SC.UserName SecondChar
        //                                   ,TC.Id ThirdCharId
        //                                   ,TC.UserName ThirdChar
        //                                   ,ROUND(POD.TransactionQty, 2) POTransactionQty
        //                                   ,ROUND(POD.TransactionRate, 2) TransactionRate
        //                                   ,ROUND((POD.TransactionQty * POD.TransactionRate), 2) AS TrnAmount
        //                                   ,POD.BaseAmount
        //                                   ,POD.TotalTaxAmount AS BaseTaxAmount
        //                                   ,TaxAmount = (
        //                                      SELECT SUM(TaxAmount)
        //                                      FROM [TRN].[PurchaseOrderTax]
        //                                      WHERE InventoryReceiveDetailId = POD.Id
        //                                      )
        //                                   ,ServiceTaxAmount = (
        //                                      SELECT SUM(TotalTaxAmount)
        //                                      FROM [TRN].[POService]
        //                                      WHERE InventoryReceiveId = POM.Id
        //                                      )
        //                                   ,POD.ChargesAmount
        //                                   ,POD.CountryId

        //                                   ,POD.TransactionUoMId
        //                                   ,TUoM.UserName AS TransactionUoM
        //                                      FROM TRN.PurchaseOrder PO
        //                                 LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = PO.CompanyGroupId
        //                                 LEFT JOIN ORG.Company Cmp ON Cmp.Id = PO.CompanyId
        //                                 LEFT JOIN ORG.Plant Plant ON Plant.Id = PO.PlantId
        //                                 LEFT JOIN SCS.Currency CRNC ON CRNC.Id = PO.CurrencyId
        //                                 LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = PO.BaseCurrencyId
        //                                 LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = PO.PaymentTermId
        //                                 LEFT JOIN HKP.PartyPlant  INVPARTYPL ON INVPARTYPL.Id = PO.InvoicingPartyPlantId
        //                                 LEFT JOIN HKP.PartyPlant  DPARTYPL ON DPARTYPL.Id = PO.DeliveryPartyPlantId 
        //                                 LEFT JOIN TRN.PurchaseOrderDetail POD ON PO.Id = POD.InventoryReceiveId

        //                                 LEFT JOIN TRN.POMaterial AS POM ON POD.InventoryMaterialId = POM.Id
        //                                 INNER JOIN MST.MaterialMaster AS MM ON MM.Id = POM.MaterialMasterId
        //                                 INNER JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
        //                                 INNER JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = POM.ArticleId
        //                                 LEFT JOIN HKP.Characteristics AS FC ON POM.FirstCharacteristicsId = FC.Id
        //                                 LEFT JOIN HKP.Characteristics AS SC ON POM.SecondCharacteristicsId = SC.Id
        //                                 LEFT JOIN HKP.Characteristics AS TC ON POM.ThirdCharacteristicsId = TC.Id
        //                                 LEFT JOIN HKP.CharacteristicsValue AS FCV ON POM.FirstCharacteristicsValueId = FCV.Id
        //                                 LEFT JOIN HKP.CharacteristicsValue AS SCV ON POM.SecondCharacteristicsValueId = SCV.Id
        //                                 LEFT JOIN HKP.CharacteristicsValue AS TCV ON POM.ThirdCharacteristicsValueId = TCV.Id
        //                                 JOIN [SCS].[UnitOfMeasurement] AS TUoM ON POD.TransactionUoMId = TUoM.Id
        //                                 WHERE PO.Id = '" + RequisitionId + @"' and PO.IsApproved = 0";

        //        _sqlRepository.GetDataTable(sqlTxt);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public IEnumerable<object> PlantWiseGateCbo(string IsSysAdmin,string UserId,string plantId)
        {
            try
            {
                var sql = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (IsSysAdmin=="True")
                {
                    sql = @"select Id As Value,UserName Text from dbo.PlantWiseGate A
                          where A.PlantId='" + plantId + "'";
                }
                else
                {
                    sql = @"select PWG.Id  Value,PWG.UserName Text 
                            from dbo.PlantWiseGate PWG
                            Left JOIN SEC.UserPlantGate UPG ON UPG.PlantGateId=PWG.Id
                            where PWG.PlantId='" + plantId + "' AND UPG.UserId='" + UserId + "'";
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
        public GridModel EmployeeListByDepartment(GridParameter parameters, string DepartmentId) 
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT EI.SystemId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
								, EI.EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS Designation, MB.EntityId,PR.UserName PositionName
        						, DEG.UserName GivenDesignation,DEPT.UserName Department
                                    FROM dbo.EmployeeInformation AS EI
									LEFT JOIN HKP.Designation AS DEG ON DEG.Id=EI.DesignationSystemID
                                    LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
        					        LEFT OUTER JOIN ORG.Position PR ON MB.PositionId=PR.Id
									LEFT OUTER JOIN ORG.Entity E ON MB.EntityId=E.Id
        					        LEFT OUTER JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id where  EI.EmployeeStatus='Active'";
                //Where EI.DepartmentId='" + DepartmentId + @"' AND EI.EmployeeStatus='Active'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void InsertOrUpdateGraph(GatePassDetailsViewModel entity ,string ChallanNo) 
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;

                // Insert in receive detail
                if (string.IsNullOrEmpty(entity.Id))
                {
                    var NewId = entity.GatePassMasterId + "-";
                    //var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[MaterialRequsitionDetails] WHERE MaterialReqqusitionMasterId='{entity.MaterialReqqusitionMasterId}'").First();
                    var currentId = _gatePassMasterDetailsRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id))    AS INT)), 0) Id FROM[TRN].[GatePassDetails] WHERE GatePassMasterId='{entity.GatePassMasterId}'").First();


                    currentId++;
                    var receiveDetail = new GatePassDetails
                    {

                        Id = NewId + currentId, //MakePK(NewId + currentId, 0,0),    
                        GatePassMasterId = entity.GatePassMasterId,
                        MaterialMasterId = entity.MaterialMasterId,
                        ArticleId = entity.ArticleId,
                        FirstCharacteristicsId = entity.FirstCharacteristicsId,
                        FirstCharacteristicsValueId = entity.FirstCharacteristicsValueId,
                        SecondCharacteristicsId = entity.SecondCharacteristicsId,
                        SecondCharacteristicsValueId = entity.SecondCharacteristicsValueId,
                        ThirdCharacteristicsId = entity.ThirdCharacteristicsId,
                        ThirdCharacteristicsValueId = entity.ThirdCharacteristicsValueId,
                        MaterialDetail = entity.MaterialDetail,
                        TransactionQty = Convert.ToDecimal( entity.TransactionQty),
                        TransactionUoMId = entity.TransactionUoMId,
                        Remarks = entity.Remarks,
                        IsReturnable = Convert.ToBoolean(entity.IsReturnable),
                        ReturnableDate = entity.ReturnableDate,
                        IsMutilated = Convert.ToBoolean(entity.IsMutilated),
                        Rate = Math.Round(Convert.ToDecimal(entity.Rate),4)
                    };
                    AuditService.AddedLog(receiveDetail);
                    _gatePassMasterDetailsRepository.Insert(receiveDetail);
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
        public void InsertOrUpdateGraphDispatch(IEnumerable<GatePassDetailsViewModel> entity, string ChallanNo, string MasterId)  
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                var currentId = _gatePassMasterDetailsRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id))    AS INT)), 0) Id FROM[TRN].[GatePassDetails] WHERE GatePassMasterId='{MasterId}'").First();//itemDetail.GatePassMasterId

                foreach (var itemDetail in entity)
                {


                    // Insert in receive detail
                    if (string.IsNullOrEmpty(itemDetail.Id))
                    {
                        var NewId = itemDetail.GatePassMasterId + "-";
                        //var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[MaterialRequsitionDetails] WHERE MaterialReqqusitionMasterId='{entity.MaterialReqqusitionMasterId}'").First();
                        

                        currentId++;
                        var receiveDetail = new GatePassDetails
                        {

                            Id = NewId + currentId, //MakePK(NewId + currentId, 0,0),    
                            GatePassMasterId = itemDetail.GatePassMasterId,
                            MaterialMasterId = itemDetail.MaterialMasterId,
                            ArticleId = itemDetail.ArticleId,
                            FirstCharacteristicsId = itemDetail.FirstCharacteristicsId,
                            FirstCharacteristicsValueId = itemDetail.FirstCharacteristicsValueId,
                            SecondCharacteristicsId = itemDetail.SecondCharacteristicsId,
                            SecondCharacteristicsValueId = itemDetail.SecondCharacteristicsValueId,
                            ThirdCharacteristicsId = itemDetail.ThirdCharacteristicsId,
                            ThirdCharacteristicsValueId = itemDetail.ThirdCharacteristicsValueId,
                            MaterialDetail = itemDetail.MaterialDetail,
                            TransactionQty = Convert.ToDecimal(itemDetail.TransactionQty),
                            TransactionUoMId = itemDetail.TransactionUoMId,
                            Remarks = itemDetail.Remarks,
                            //IsReturnable = false,// Convert.ToBoolean(itemDetail.IsReturnable),
                            //ReturnableDate = "10-Jan-2020",//itemDetail.ReturnableDate,
                            //IsMutilated = false//Convert.ToBoolean(itemDetail.IsMutilated),
                            Rate= Math.Round(Convert.ToDecimal(itemDetail.Rate),4),
                            ChallanNo= itemDetail.ChallanNo,
                            ChallanNoDetailId = itemDetail.ChallanNoDetailId
                        };
                        AuditService.AddedLog(receiveDetail);
                        _gatePassMasterDetailsRepository.Insert(receiveDetail);
                    }

                    
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

        public GridModel QueryForGatePassDetail(GridParameter parameters, string inveReveiveId,string GatePassNewId)
        {
            try
            {
                parameters.CmdText = @"DECLARE @GatePassId VARCHAR(10)='" + inveReveiveId + @"'	                                                 
                          SELECT                   IM.Id
                        ,IM.Id AS MaterialReqqusitionMasterId,IM.GatePassMasterId AS ChallanNo,IM.Id AS ChallanNoDetailId
                        , MGM.UserName AS MaterialGroupMasterName
                        , IM.MaterialMasterId, MM.UserName
                        , IM.ArticleId, ART.StandardName
                        , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                        , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                        , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                        , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                        , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                        , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
                        , ROUND(IM.TransactionQty,2) Qty
						, isnull(ROUND(RCVed.TransactionQty,2),0) ReturnedQty
						, TransactionQty= CASE WHEN isnull((ROUND(IM.TransactionQty,2)-ROUND(RCVed.TransactionQty,2)),0)=0 THEN ROUND(IM.TransactionQty,2) ELSE isnull((ROUND(IM.TransactionQty,2)-ROUND(RCVed.TransactionQty,2)),0) END  

                        , IM.TransactionUoMId
                        ,Replace(CONVERT(VARCHAR(11), IM.ReturnableDate, 106), ' ', '-') ReturnableDate
                        , TUoM.UserName AS TransactionUoM                       
                        ,IM.MaterialDetail        
                        ,IM.Remarks
                        ,IsReturnable = CASE WHen IM.IsReturnable=1 Then 'Yes' Else 'No' End
						,IM.ReturnableDate
						,IsMutilated= CASE When IM.IsMutilated=1 Then 'Yes' ELSE 'No' END 
                        ,'false' Active,IM.Rate,TotalAmount=ROUND(ISNULL(IM.TransactionQty,0),2)* ISNULL(IM.Rate,0)
                        FROM TRN.GatePassDetails AS IM
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
                        LEFT JOIN [TRN].[GatePassMaster] AS IR ON IM.GatePassMasterId=IR.Id
                        
                        LEFT JOIN (Select sum(bb.TransactionQty) TransactionQty,bb.ChallanNoDetailId
									from TRN.GatePassDetails bb
									LEFT JOIN TRN.GatePassMaster GPMa ON GPMa.Id=bb.GatePassMasterId
									where  GPMa.ChallanNo =@GatePassId AND GPMa.Id <> '" + GatePassNewId + @"'
									group by bb.ChallanNoDetailId
								  ) RCVed ON  RCVed.ChallanNoDetailId=IM.Id
                        WHERE IM.GatePassMasterId=@GatePassId AND ROUND(IM.TransactionQty,2) !=isnull(ROUND(RCVed.TransactionQty,2),0)";
                return _sqlRepository.GetDifferentGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public void DeleteGatePassDEtails(string id)
        {
            try
            {
                //var detail = Convert.ToBoolean(_inventoryReceiveRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM TRN.MaterialRequsitionDetails WHERE Id='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                ////var service = Convert.ToBoolean(_inventoryReceiveRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM TRN.InventoryService WHERE InventoryReceiveId='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                //if (!detail)
                //{

                var data = _gatePassMasterDetailsRepository.Find(id);
                if (data.IsNull()) 
                    throw new CustomException(ServiceResources.RecordNoLonger);
                _gatePassMasterDetailsRepository.Delete(data.Id);
                _unitOfWork.SaveChanges();
                //}
                //else throw new CustomException("Please delete first line item.");
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        
    }

}