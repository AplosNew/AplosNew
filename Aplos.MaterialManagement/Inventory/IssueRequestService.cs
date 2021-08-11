#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Inventory;
using Library.Model.OrderManagements;
using Library.Model.Productions;
using Library.Model.Products;
using Library.Service.Core;
using Library.Service.Enums;
using Library.MaterialManagement.Inventory;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using Library.ViewModel.OrderManagements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.MaterialManagement.Products
{
	public class IssueRequestService : Service<IssueRequest>, IIssueRequestService
	{

		#region Constructor

		private readonly ISqlRepository _sqlRepository;
		private readonly IRepositoryAsync<PurchaseOrderGroup> _purchaseOrderGroupMaster;
		private readonly IRepositoryAsync<IssueRequest> _IssueRequest;
		private readonly IRepositoryAsync<PurchaseOrderGroupDetails> _purchaseOrderGroupDetails;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IRepositoryAsync<IssueRequest> _issueRequestRepository;
		private readonly IRepositoryAsync<IssueRequestMaster> _issueRequestMasterRepository;
		private readonly IIssueRequestMasterService _issueRequestMasterService;

		private readonly IRepositoryAsync<IssueRequestMasterSalesOrderMap> _issueRequestMasterSalesOrderMap;
		private readonly IRepositoryAsync<IssueRequestMasterProcessMap> _issueRequestMasterProcessMap;
		private readonly IRepositoryAsync<IssueRequestSKUMap> _issueRequestSKUMap;
		private readonly IRepositoryAsync<IssueRequestBOQMap> _issueRequestBOQMap;
		

		public IssueRequestService(
			 IRepositoryAsync<PurchaseOrderGroup> purchaseOrderGroupMaster
			 , IRepositoryAsync<IssueRequest> issueRequestRepository
			, IRepositoryAsync<IssueRequest> issueRequest
			, IPKGeneratorService pkGeneratorService
			, IUnitOfWork unitOfWork
			, ISqlRepository sqlRepository
			, IRepositoryAsync<PurchaseOrderGroupDetails> purchaseOrderGroupDetails
			, IIssueRequestMasterService issueRequestMasterService
			, IRepositoryAsync<IssueRequestMaster> issueRequestMasterRepository
			, IRepositoryAsync<IssueRequestMasterSalesOrderMap> issueRequestMasterSalesOrderMap
			, IRepositoryAsync<IssueRequestMasterProcessMap> issueRequestMasterProcessMap
			, IRepositoryAsync<IssueRequestSKUMap> issueRequestSKUMap
			, IRepositoryAsync<IssueRequestBOQMap> issueRequestBOQMap			
			) : base(issueRequest, unitOfWork, pkGeneratorService)
		{
			_sqlRepository = sqlRepository;
			_unitOfWork = unitOfWork;
			_purchaseOrderGroupMaster = purchaseOrderGroupMaster;
			_IssueRequest = issueRequest;
			_purchaseOrderGroupDetails = purchaseOrderGroupDetails;
			_issueRequestRepository = issueRequestRepository;
			_issueRequestMasterService = issueRequestMasterService;
			_issueRequestMasterRepository = issueRequestMasterRepository;
			_issueRequestMasterSalesOrderMap = issueRequestMasterSalesOrderMap;
			_issueRequestMasterProcessMap = issueRequestMasterProcessMap;
			_issueRequestSKUMap = issueRequestSKUMap;
			_issueRequestBOQMap = issueRequestBOQMap;
		}

		#endregion Constructor

		private string GetPK()
		{
			return GetAutoNumber(nameof(PurchaseOrderGroup), PKGeneratorEnum.Yearly, null, DateTime.Now);
		}
		private string GetIssueRequestMasterSalesOrderMapPK()
		{
			return GetAutoNumber(nameof(IssueRequestMasterSalesOrderMap), PKGeneratorEnum.Yearly, null, DateTime.Now);
		}
		private string GetIssueRequestMasterProcessMapPK()
		{
			return GetAutoNumber(nameof(IssueRequestMasterProcessMap), PKGeneratorEnum.Yearly, null, DateTime.Now);
		}
		private string GetIssueRequestSKUMapPK()
		{
			return GetAutoNumber(nameof(IssueRequestSKUMap), PKGeneratorEnum.Yearly, null, DateTime.Now);
		}
		#region IssueSlip
		private string GetPK1()
		{
			string sID = string.Empty;
			bplib.clsGenID objGenID = new bplib.clsGenID();
			objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(IssueRequest), out sID);
			return sID;
		}
		public void InsertOrUpdateGraphIssueSlipCreate(IssueRequestMaster Issentry, IEnumerable<IssueRequestViewModel> entity, IEnumerable<IssueRequestViewModel> entityGroupData, string IssueSlipType, string CheckedByStatusForNoti, string ApprovedByStatusForNoti, IEnumerable<IssueRequestViewModel> SOListSelectedNew, IEnumerable<IssueRequestViewModel> MaterialColorListNew, string ProcessId)
		{
			var flag = false;
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{

				_unitOfWork.BeginTransaction();
				var currentId1 = _issueRequestRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[IssueRequest]  WHERE IssueRequestMasterId ='{Issentry.Id}'").First();

				if (identity.EmployeeId == Issentry.CheckedBy)
				{
					throw new CustomException("Please select another employee for Check by.");
				}
				else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "True")
				{

					Issentry.AuthorizedBy = Issentry.CheckedBy;
					Issentry.AuthorizedByStatus = "For Approval";
					Issentry.CheckedBy = null;
					Issentry.CheckedByStatus = null;
				}
				else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
				{
					Issentry.CheckedByStatus = null;
					Issentry.AuthorizedByStatus = null;
					Issentry.CheckedBy = null;
					Issentry.AuthorizedBy = null;
				}
				else
				{
					Issentry.CheckedBy = Issentry.CheckedBy;
					Issentry.CheckedByStatus = "ForChecked";
					Issentry.AuthorizedBy = null;
					Issentry.AuthorizedByStatus = null;

				}

				Issentry.Preparedby = identity.EmployeeId;
				Issentry.IssueSlipType = IssueSlipType;

				_issueRequestMasterService.Insert(Issentry);
				if (SOListSelectedNew.IsNotNull())
				{
					foreach (var item in SOListSelectedNew)
					{
						var itemD = new IssueRequestMasterSalesOrderMap
						{
							Id = GetIssueRequestMasterSalesOrderMapPK(),
							IssueRequestMasterId = Issentry.Id,
							SalesOrderId = item.SalesOrderId,
							ModelState = ModelState.Added
						};
						AuditService.AddedLog(itemD);
						_issueRequestMasterSalesOrderMap.Insert(itemD);

					}


					var itemcolorD = new IssueRequestMasterProcessMap
					{
						Id = GetIssueRequestMasterProcessMapPK(),
						IssueRequestMasterId = Issentry.Id,
						ProcessId = ProcessId,
						ModelState = ModelState.Added
					};


					AuditService.AddedLog(itemcolorD);
					_issueRequestMasterProcessMap.Insert(itemcolorD); 

					foreach (var itemcolor in MaterialColorListNew)
					{
						var SKUMapD = new IssueRequestSKUMap
						{
							Id = GetIssueRequestSKUMapPK(),
							IssueRequestMasterId = Issentry.Id,
							FirstCharacteristicsValueId = itemcolor.FirstCharacteristicsValueId,
							SecondCharacteristicsValueId = itemcolor.SecondCharacteristicsValueId,
							ThirdCharacteristicsValueId = itemcolor.ThirdCharacteristicsValueId,
							RequisitionForQty = itemcolor.RequisitionForQty, 
							ModelState = ModelState.Added
						};
						AuditService.AddedLog(SKUMapD);
						_issueRequestSKUMap.Insert(SKUMapD); 
					}
				}
				var slipDetailId = "";
				var Material = "";
				var Article = "";
				var SKU1 = "";
				var SKU2 = "";
				var SKU3 = "";
				var SalesOrderId = "";
				var TransactionUoMId = "";
				flag = true;
			foreach (var itemDetail in entityGroupData)
			{
				//if (itemDetail.CostCenterId == "" || itemDetail.CostCenterId == null)
				//{
				//	throw new CustomException("Select Cost Center !");
				//}
				//else if (itemDetail.RequestedQty == 0)
				//{
				//	throw new CustomException("Input Requested Qty !");
				//}
				////else if (itemDetail.RejectedQty == 0)
				////{
				////    throw new CustomException("Input Rejected Qty !");
				////}
				//else if (itemDetail.ExpenseActivityId == "" || itemDetail.ExpenseActivityId == null)
				//{
				//	throw new CustomException("Select Expense Activity !");
				//}

				//else
				//{
					// Insert in receive detail
					if (string.IsNullOrEmpty(itemDetail.Id))
					{
						var NewId = Issentry.Id + "-";
						currentId1++;
						//grndId = NewId + currentId1;
						var IssueRequstD = new IssueRequest
						{
							Id = NewId + currentId1,
							IssueRequestMasterId = Issentry.Id,
							RequisitionId = itemDetail.RequisitionNo,
							RequisitionDetailId = itemDetail.RequisitionDetailId,
							CostCenterId = itemDetail.CostCenterId,
							ExpenseActivityId = itemDetail.ExpenseActivityId,
							RequestedQty = Convert.ToDecimal(itemDetail.RequestedQtyNew),
							RejectedQty = itemDetail.RejectedQty,
							BudgetMasterId = itemDetail.BudgetMasterId,
							GLGeneralInfoId = itemDetail.GLGeneralInfoId,
							MaterialMasterId = itemDetail.MaterialMasterId,
							ArticleId = itemDetail.ArticleId,
							FirstCharacteristicsId = itemDetail.FirstCharacteristicsId,
							FirstCharacteristicsValueId = itemDetail.BOQDFirstCharacteristicsValueId,
							SecondCharacteristicsId = itemDetail.SecondCharacteristicsId,
							SecondCharacteristicsValueId = itemDetail.BOQDSecondCharacteristicsValueId,
							ThirdCharacteristicsId = itemDetail.ThirdCharacteristicsId,
							ThirdCharacteristicsValueId = itemDetail.BOQDThirdCharacteristicsValueId,
							TransactionUoMId = itemDetail.TransactionUoMId,
							InventoryMaterialId = itemDetail.InventoryMaterialId,
							CountryId = itemDetail.CountryId
						};
						try
						{
							//InsertGraph(receiveDetail); AuditService.UpdatedLog(receiveDetail);

							AuditService.AddedLog(IssueRequstD);
							_issueRequestRepository.Insert(IssueRequstD);

							slipDetailId = IssueRequstD.Id;
							Material= IssueRequstD.MaterialMasterId;
							Article = IssueRequstD.ArticleId;
							SKU1 = IssueRequstD.FirstCharacteristicsValueId;
							SKU2 = IssueRequstD.SecondCharacteristicsValueId;
							SKU3 = IssueRequstD.ThirdCharacteristicsValueId;
							SalesOrderId = itemDetail.SalesOrderId;
							TransactionUoMId = IssueRequstD.TransactionUoMId; 



						}
						catch (DivideByZeroException ex)
						{

						}
						finally
						{

						}
					}
					var FilterentityData = entity.Where(r => r.MaterialMasterId == Material && r.ArticleId == Article && r.BOQDFirstCharacteristicsValueId == SKU1 && r.BOQDSecondCharacteristicsValueId == SKU2 && r.BOQDThirdCharacteristicsValueId == SKU3 && r.SalesOrderId == itemDetail.SalesOrderId && r.TransactionUoMId==TransactionUoMId).ToList();
					foreach (var itemDetailentity in FilterentityData)
					{

						// Insert in receive detail
						if (string.IsNullOrEmpty(itemDetailentity.Id))
						{
							var NewId = Issentry.Id + "-";
							currentId1++;
							//grndId = NewId + currentId1;
							var IssueRequestBOQMap = new IssueRequestBOQMap
							{
								Id = NewId + currentId1,
								IssueRequestDetailId = slipDetailId,
								BOQID = itemDetailentity.BOQId,
								Qty = Convert.ToDecimal(itemDetailentity.RequestedQty)
							};
							try
							{
								AuditService.AddedLog(IssueRequestBOQMap);
								_issueRequestBOQMap.Insert(IssueRequestBOQMap);
							}
							catch (DivideByZeroException ex)
							{

							}
							finally
							{

							}
						}
					}//
				}

				

			

				// insert in receive tax


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

        public void InsertOrUpdateGraphIssueSlipUpdate(IssueRequestMaster Issentity, IEnumerable<IssueRequestViewModel> entity, string Ids, string IssueSlipType, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
{
	var flag = false;
	var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
	try
	{
		_unitOfWork.BeginTransaction();
		//var currentId1 = _issueRequestRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[IssueRequest]  WHERE IssueRequestMasterId ='{Issentry.Id}'").First();

		if (identity.EmployeeId == Issentity.CheckedBy)
		{
			throw new CustomException("Please select another employee for Check by.");
		}
		else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "True")
		{

			Issentity.AuthorizedBy = Issentity.CheckedBy;
			Issentity.AuthorizedByStatus = "For Approval";
			Issentity.CheckedBy = null;
			Issentity.CheckedByStatus = null;
		}
		else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
		{
			Issentity.CheckedByStatus = null;
			Issentity.AuthorizedByStatus = null;
			Issentity.CheckedBy = null;
			Issentity.AuthorizedBy = null;
		}
		else
		{
			Issentity.CheckedBy = Issentity.CheckedBy;
			Issentity.CheckedByStatus = "ForChecked";
			Issentity.AuthorizedBy = null;
			Issentity.AuthorizedByStatus = null;

		}

		Issentity.Preparedby = identity.EmployeeId;
		Issentity.IssueSlipType = IssueSlipType;

		Issentity.Preparedby = identity.EmployeeId;
		Issentity.IssueSlipType = IssueSlipType;




		_issueRequestMasterService.Update(Issentity);
		flag = true;
		foreach (var itemDetail in entity)
		{
			if (itemDetail.CostCenterId == "" || itemDetail.CostCenterId == null)
			{
				throw new CustomException("Select Cost Center !");
			}
			else if (itemDetail.RequestedQty == 0)
			{
				throw new CustomException("Input Requested Qty !");
			}
			//else if (itemDetail.RejectedQty == 0)
			//{
			//    throw new CustomException("Input Rejected Qty !");
			//}
			else if (itemDetail.ExpenseActivityId == "")
			{
				throw new CustomException("Select Expense Activity !");
			}

			else
			{
				// Insert in receive detail
				//if (string.IsNullOrEmpty(itemDetail.Id))
				//{

				var IssueRequstD = new IssueRequest
				{
					Id = itemDetail.Id,
					IssueRequestMasterId = Ids,
					RequisitionId = itemDetail.RequisitionNo,
					RequisitionDetailId = itemDetail.RequisitionDetailId,
					CostCenterId = itemDetail.CostCenterId,
					ExpenseActivityId = itemDetail.ExpenseActivityId,
					RequestedQty = Convert.ToDecimal(itemDetail.RequestedQty),
					RejectedQty = itemDetail.RejectedQty,
					BudgetMasterId = itemDetail.BudgetMasterId,
					GLGeneralInfoId = itemDetail.GLGeneralInfoId,
					MaterialMasterId = itemDetail.MaterialMasterId,
					ArticleId = itemDetail.ArticleId,
					FirstCharacteristicsId = itemDetail.FirstCharacteristicsId,
					FirstCharacteristicsValueId = itemDetail.FirstCharacteristicsValueId,
					SecondCharacteristicsId = itemDetail.SecondCharacteristicsId,
					SecondCharacteristicsValueId = itemDetail.SecondCharacteristicsValueId,
					ThirdCharacteristicsId = itemDetail.ThirdCharacteristicsId,
					ThirdCharacteristicsValueId = itemDetail.ThirdCharacteristicsValueId,
					InventoryMaterialId = itemDetail.InventoryMaterialId
					//Preparedby = identity.EmployeeId,
					//CheckedBy = itemDetail.CheckedBy,
					//CheckedByStatus = "ForChecked",

				};
				try
				{
					//InsertGraph(receiveDetail); AuditService.UpdatedLog(receiveDetail);

					AuditService.UpdatedLog(IssueRequstD);
					_issueRequestRepository.Update(IssueRequstD);

				}
				catch (DivideByZeroException ex)
				{

				}
				finally
				{

				}
				// }
			}



		}

		// insert in receive tax


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
#endregion
//private void Check(PurchaseOrderGroup entity)
//{
//    CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code);
//    CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName);
//}


//public void Insert(PurchaseOrderGroup entity)
//{
//    try
//    {
//        Check(entity);
//        entity.Id = GetPK();
//        AuditService.AddedLog(entity);
//        entity.ModelState = ModelState.Added;
//        _purchaseOrderGroupMaster.Insert(entity);
//        _unitOfWork.SaveChanges();
//    }
//    catch (Exception ex)
//    {
//        throw new CustomException(ex.Message, ex,
//            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
//            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
//    }
//}

//public override void Update(PurchaseOrderGroup entity)
//{
//    try
//    {
//        Check(entity);
//        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
//        entity.CompanyGroupId = identity.CompanyGroupId;
//        base.Update(entity);
//    }
//    catch (Exception ex)
//    {
//        throw new CustomException(ex.Message, ex,
//            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
//            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
//    }
//}

public void DeleteReq(string id)
{
	try
	{
		var detail = Convert.ToBoolean(_purchaseOrderGroupMaster.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM [TRN].[PurchaseOrderGroupDetails] WHERE Id='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
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




//public decimal GetAutoSequence()
//{
//    try
//    {
//        return base.Query().Select().Max(r => r.Sequence + 1);
//    }
//    catch
//    {
//        return 1.00M;
//    }
//}


public IEnumerable<object> GetPurchaseOrderGroupGridData()
{
	try
	{
		var sql = @"SELECT       POG.Id
	                                    ,POG.CompanyGroupId
	                                    ,POG.Sequence
	                                    ,POG.Code 
	                                    ,POG.UserName
	                                    ,POG.ShortName
	                                    ,POG.StandardName
	                                    ,POG.UserName As PartyName
	                                    ,POG.Description
	                                    ,POG.Remarks
	                                    ,POG.Active
	                                    ,POG.AddedBy
                                       FROM TRN.PurchaseOrderGroup POG   ";


		return _sqlRepository.GetDataCollection(sql);
	}
	catch (Exception ex)
	{
		throw new CustomException(ex.Message, ex,
			Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
			ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
	}
}


public IEnumerable<object> GetAllPurchaseOrderGroupDetails(string Id)//string ReqDetailId
{
	try
	{
		//var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
		var _sql = @"select
                             POGD.Id
                             ,MGM.UserName As MateralMasterGroupName
                             ,MM.Id AS MaterialMasterId
                            ,MM.UserName as MaterialMasterName
                                ,POGD.ArticleId
	                            ,ART.StandardName
	                            ,Pr.UserName As PartyName
	                            ,POGD.FirstCharacteristicsId
	                            ,FC.UserName AS FirstCharacteristics
	                            ,POGD.FirstCharacteristicsValueId
	                            ,FCV.UserName AS FirstCharacteristicsValue
	                            ,POGD.SecondCharacteristicsId
	                            ,SC.UserName AS SecondCharacteristics
	                            ,POGD.SecondCharacteristicsValueId
	                            ,SCV.UserName AS SecondCharacteristicsValue
	                            ,POGD.ThirdCharacteristicsId
	                            ,TC.UserName AS ThirdCharacteristics
	                            ,POGD.ThirdCharacteristicsValueId
	                            ,TCV.UserName AS ThirdCharacteristicsValue
                             FROM 
                            TRn.PurchaseOrderGroupDetails POGD
                            Left JOIn TRn.PurchaseOrderGroup POG ON POG.Id=POGD.PurchaseOrderGroupId
                            Left JOin mst.MaterialMaster MM ON MM.Id=POGD.MaterialMasterId
                            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                            LEFT JOIN MST.MaterialMasterArticle  ART ON ART.Id= POGD.ArticleId
                            LEFT JOIN HKP.Characteristics AS FC ON POGD.FirstCharacteristicsId = FC.Id
                            LEFT JOIN HKP.Characteristics AS SC ON POGD.SecondCharacteristicsId = SC.Id
                            LEFT JOIN HKP.Characteristics AS TC ON POGD.ThirdCharacteristicsId = TC.Id
                            LEFT JOIN HKP.CharacteristicsValue AS FCV ON POGD.FirstCharacteristicsValueId = FCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS SCV ON POGD.SecondCharacteristicsValueId = SCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS TCV ON POGD.ThirdCharacteristicsValueId = TCV.Id
                            LEFT Join [HKP].[Party] As Pr ON POGD.PartyId=Pr.Id
                           Where POG.Id ='" + Id + "' ";
		return _sqlRepository.GetDataCollection(_sql);

	}
	catch (Exception ex)
	{
		throw new CustomException(ex.Message, ex,
			Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
			ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
	}
}

public IEnumerable<object> GetAllReqdata1()
{
	throw new NotImplementedException();
}


public object SqlQuery<T>(string v)
{
	throw new NotImplementedException();
}



public void UpdateMaterial(IEnumerable<PurchaseOrderGroupDetails> entity, IEnumerable<PurchaseOrderTax> receiveTaxList)
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

				var _sql = "UPDATE [TRN].[PurchaseOrderGroupDetails] SET [TransactionQty] =  '" + Convert.ToDecimal(item1.TransactionQty) + "',[EstimatedRate] = '" + Convert.ToDecimal(item1.EstimatedRate) + "',[TotalAmount] = '" + Convert.ToDecimal(item1.TotalAmount) + "',[UpdatedBy] = '" + identity.UserId + "',[UpdatedDate] = '" + Convert.ToDateTime(DateTime.Now) + "',[UpdatedFromIP] = '" + identity.IPAddress + "' where id = '" + ReqDetailId + "'";
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
		//var detail = Convert.ToBoolean(_inventoryReceiveRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM TRN.MaterialRequsitionDetails WHERE Id='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
		////var service = Convert.ToBoolean(_inventoryReceiveRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM TRN.InventoryService WHERE InventoryReceiveId='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
		//if (!detail)
		//{

		var data = _purchaseOrderGroupDetails.Find(id);
		if (data.IsNull()) throw new CustomException(ServiceResources.RecordNoLonger);
		_purchaseOrderGroupDetails.Delete(data.Id);
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



public decimal GetToCurrencyRate(string currencyId, string baseCurrencyId, DateTime docDate, string companyId)
{
	try
	{
		decimal toCurrencyRate = 0;
		if (currencyId != baseCurrencyId)
		{
			var sql = @"SELECT ISNULL((SELECT TOP(1) ISNULL(A.ToCurrencyBankSelling,0) FROM SCS.ExchangeRate AS A WHERE
                                            FromCurrencyCode='" + currencyId + "'   AND A.CompanyId='" + companyId + "' ORDER BY CAST(FromDate AS DATE) DESC), 0)";
			toCurrencyRate = _purchaseOrderGroupDetails.SqlQuery<decimal>(sql).First();
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

public void Insert(PurchaseOrderGroup entity)
{
	throw new NotImplementedException();
}

public decimal GetAutoSequence()
{
	throw new NotImplementedException();
}


public IEnumerable<object> IssueListData(string IssueStatus, string IssueSlipType)
{
	try
	{
		var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
		var sql = "";
		if (IssueSlipType == "InventorySlip" || IssueSlipType == "undefined")
		{
			if (IssueStatus == "ForChecked")
			{
				//sql = @" select x.Id ,x.PreparedBy,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty,x.CheckedBy from
				//    (
				//        SELECT IRM.Id
				//        ,CC.UserName AS CostCenterName
				//     ,B.UserName ActivityName      
				//     ,IR.RequisitionId
				//        ,IR.RequisitionDetailId                           
				//     ,EI.EmployeeName  PreparedBy	                          
				//        ,IRM.AddedBy
				//        ,IRM.AddedDate
				//        ,IRM.AddedFromIP
				//        ,IRM.UpdatedBy
				//        ,IRM.UpdatedDate
				//        ,IRM.UpdatedFromIP	  
				//       -- ,IRM.Preparedby
				//        ,IRM.CheckedBy
				//        ,IRM.CheckedByStatus
				//        ,IRM.AuthorizedBy
				//        ,IRM.AuthorizedByStatus
				//     ,RequestedQty
				//    ,RejectedQty
				//    FROM TRN.IssueRequestMaster IRM
				//    Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
				//    Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
				//    Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
				//    LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
				//     Where IRM.CheckedBy IS NOT NULL AND IRM.CheckedByStatus='ForChecked' AND IRM.AuthorizedByStatus IS NULL AND IRM.AuthorizedBy IS null  AND IRM.IssueSlipType='InventorySlip' And IRM.PreparedBy='" + identity.EmployeeId + @"'
				//    )x 
				//    Group by Id ,x.PreparedBy,x.AddedDate,x.CheckedBy                            
				//  ";
				sql = @" select x.Id 
                                 ,x.PreparedBy
                                 ,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate
                                 ,Sum(x.RequestedQty) RequestedQty 
                                 ,Sum(x.RejectedQty) RejectedQty
                                 ,x.CheckedBy
                                 ,CheckedByStatus
                                 ,x.AuthorizedBy
                                 ,AuthorizedByStatus,SalesOrderId

                                 FROM
                                (
                                    SELECT IRM.Id
                                    ,CC.UserName AS CostCenterName
	                                ,B.UserName ActivityName      
	                                ,IR.RequisitionId
                                    ,IR.RequisitionDetailId                           
	                                ,EI.EmployeeName  PreparedBy	                          
                                    ,IRM.AddedBy
                                    ,IRM.AddedDate
                                    ,IRM.AddedFromIP
                                    ,IRM.UpdatedBy
                                    ,IRM.UpdatedDate
                                    ,IRM.UpdatedFromIP	  
                                    -- ,IRM.Preparedby
                                    ,EI1.EmployeeName CheckedBy
                                    ,IRM.CheckedByStatus
                                    ,EI2.EmployeeName AuthorizedBy
                                    ,IRM.AuthorizedByStatus
	                                ,RequestedQty,SalesOrderId
                                ,RejectedQty
                                FROM TRN.IssueRequestMaster IRM
                                Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                                Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                                Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                                LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                                LEFT JOIN EmployeeInformation EI1 On EI1.SystemId=IRM.CheckedBy
                                LEFT JOIN EmployeeInformation EI2 On EI2.SystemId=IRM.AuthorizedBy
								LEFT JOIN(
											SELECT distinct PDAMAP.IssueRequestMasterId
												,SalesOrderId=STUFF((select distinct ','+xPDAMAP.SalesOrderId from
												trn.IssueRequestMaster xpo
												INNER JOin trn.IssueRequestMasterSalesOrderMap xPDAMAP on xpo.Id=xPDAMAP.IssueRequestMasterId
												where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	
												from  trn.IssueRequestMasterSalesOrderMap PDAMAP 
												LEFT JOIN [TRN].IssueRequestMaster IR ON IR.Id = PDAMAP.IssueRequestMasterId
							  
												group by  PDAMAP.IssueRequestMasterId
									)PDA ON PDA.IssueRequestMasterId=IRM.Id
                                Where IRM.CheckedBy IS NOT NULL 
                                AND IRM.CheckedByStatus='ForChecked' 
                                AND IRM.AuthorizedByStatus IS NULL 
                                AND IRM.AuthorizedBy IS null  
                                AND IRM.IssueSlipType='InventorySlip' 
                                And IRM.PreparedBy='" + identity.EmployeeId + @"'

                                UNION ALL
                                SELECT IRM.Id
                                    ,CC.UserName AS CostCenterName
	                                ,B.UserName ActivityName      
	                                ,IR.RequisitionId
                                    ,IR.RequisitionDetailId                           
	                                ,EI.EmployeeName  PreparedBy	                          
                                    ,IRM.AddedBy
                                    ,IRM.AddedDate
                                    ,IRM.AddedFromIP
                                    ,IRM.UpdatedBy
                                    ,IRM.UpdatedDate
                                    ,IRM.UpdatedFromIP	  
                                    -- ,IRM.Preparedby
                                    ,EI1.EmployeeName CheckedBy
                                    ,IRM.CheckedByStatus
                                    ,EI2.EmployeeName AuthorizedBy
                                    ,IRM.AuthorizedByStatus
	                                ,RequestedQty,SalesOrderId
                                ,RejectedQty
                                FROM TRN.IssueRequestMaster IRM
                                Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                                Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                                Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                                LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                                LEFT JOIN EmployeeInformation EI1 On EI1.SystemId=IRM.CheckedBy
                                LEFT JOIN EmployeeInformation EI2 On EI2.SystemId=IRM.AuthorizedBy
								LEFT JOIN(
											SELECT distinct PDAMAP.IssueRequestMasterId
												,SalesOrderId=STUFF((select distinct ','+xPDAMAP.SalesOrderId from
												trn.IssueRequestMaster xpo
												INNER JOin trn.IssueRequestMasterSalesOrderMap xPDAMAP on xpo.Id=xPDAMAP.IssueRequestMasterId
												where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	
												from  trn.IssueRequestMasterSalesOrderMap PDAMAP 
												LEFT JOIN [TRN].IssueRequestMaster IR ON IR.Id = PDAMAP.IssueRequestMasterId
							  
												group by  PDAMAP.IssueRequestMasterId
									)PDA ON PDA.IssueRequestMasterId=IRM.Id
                                Where  IRM.CheckedByStatus IS  NULL 
                                AND IRM.AuthorizedByStatus ='For Approval' 
                                AND IRM.IssueSlipType='InventorySlip' 
                                And IRM.PreparedBy='" + identity.EmployeeId + @"'
                                UNION ALL
                                SELECT IRM.Id
                                    ,CC.UserName AS CostCenterName
	                                ,B.UserName ActivityName      
	                                ,IR.RequisitionId
                                    ,IR.RequisitionDetailId                           
	                                ,EI.EmployeeName  PreparedBy	                          
                                    ,IRM.AddedBy
                                    ,IRM.AddedDate
                                    ,IRM.AddedFromIP
                                    ,IRM.UpdatedBy
                                    ,IRM.UpdatedDate
                                    ,IRM.UpdatedFromIP	  
                                    -- ,IRM.Preparedby
                                    ,EI1.EmployeeName CheckedBy
                                    ,IRM.CheckedByStatus
                                    ,EI2.EmployeeName AuthorizedBy
                                    ,IRM.AuthorizedByStatus
	                                ,RequestedQty,SalesOrderId
                                ,RejectedQty
                                FROM TRN.IssueRequestMaster IRM
                                Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                                Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                                Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                                LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                                LEFT JOIN EmployeeInformation EI1 On EI1.SystemId=IRM.CheckedBy
                                LEFT JOIN EmployeeInformation EI2 On EI2.SystemId=IRM.AuthorizedBy
								LEFT JOIN(
											SELECT distinct PDAMAP.IssueRequestMasterId
												,SalesOrderId=STUFF((select distinct ','+xPDAMAP.SalesOrderId from
												trn.IssueRequestMaster xpo
												INNER JOin trn.IssueRequestMasterSalesOrderMap xPDAMAP on xpo.Id=xPDAMAP.IssueRequestMasterId
												where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	
												from  trn.IssueRequestMasterSalesOrderMap PDAMAP 
												LEFT JOIN [TRN].IssueRequestMaster IR ON IR.Id = PDAMAP.IssueRequestMasterId
							  
												group by  PDAMAP.IssueRequestMasterId
									)PDA ON PDA.IssueRequestMasterId=IRM.Id
                                Where  IRM.CheckedByStatus IS  NULL 
                                AND IRM.AuthorizedByStatus IS  NULL
                                AND IRM.IssueSlipType='InventorySlip' 
                                And IRM.PreparedBy='" + identity.EmployeeId + @"'
                                )x 
                                Group by Id ,x.PreparedBy,x.AddedDate,x.CheckedBy,x.CheckedBy
                                 ,CheckedByStatus
                                 ,x.AuthorizedBy
                                 ,AuthorizedByStatus,SalesOrderId";
			}
			else if (IssueStatus == "HoldReject")
			{
				sql = @" select x.Id ,x.PreparedBy,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty  ,x.CheckedBy
							 ,CheckedByStatus
							 ,x.AuthorizedBy
							 ,AuthorizedByStatus from
                            (
                                SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.FirstName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                 ,EI1.EmployeeName CheckedBy
                                ,IRM.CheckedByStatus
                                ,EI2.EmployeeName AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                           LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                            LEFT JOIN EmployeeInformation EI1 On EI1.SystemId=IRM.CheckedBy
                            LEFT JOIN EmployeeInformation EI2 On EI2.SystemId=IRM.AuthorizedBy
                            Where IRM.CheckedBy IS NOT NULL 
                            AND IRM.CheckedByStatus='Hold'OR IRM.CheckedByStatus='Reject' 
                            AND IRM.AuthorizedByStatus IS NULL 
                            AND IRM.IssueSlipType='InventorySlip' 
                            AND IRM.AuthorizedBy IS null 
                            And IRM.PreparedBy='" + identity.EmployeeId + @"'
                            )x 
                            Group by Id ,x.PreparedBy,x.AddedDate ,x.CheckedBy
                             ,CheckedByStatus
                             ,x.AuthorizedBy
                             ,AuthorizedByStatus                            
                                                      
                          ";

			}
			else
			{
				sql = @" select x.Id ,x.PreparedBy,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty  ,x.CheckedBy
 ,CheckedByStatus
 ,x.AuthorizedBy
 ,AuthorizedByStatus from
                            (
                                SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.FirstName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,EI1.EmployeeName CheckedBy
                                ,IRM.CheckedByStatus
                                ,EI2.EmployeeName AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                            LEFT JOIN EmployeeInformation EI1 On EI1.SystemId=IRM.CheckedBy
                            LEFT JOIN EmployeeInformation EI2 On EI2.SystemId=IRM.AuthorizedBy
                            Where IRM.CheckedBy IS NOT NULL AND IRM.CheckedByStatus='Checked' 
                            AND IRM.AuthorizedByStatus ='For Approval' AND IRM.IssueSlipType='InventorySlip' 
                            AND IRM.AuthorizedBy IS not null 
                            And IRM.PreparedBy='" + identity.EmployeeId + @"'

                            )x 
                            Group by Id ,x.PreparedBy,x.AddedDate ,x.CheckedBy
                             ,CheckedByStatus
                             ,x.AuthorizedBy
                             ,AuthorizedByStatus                            
                                                      
                          ";

			}


		}

		//else
		//{
		//    if(IssueSlipType == "AssetSlip" || IssueSlipType == "undefined")

		//    {
		//        if (IssueStatus == "ForChecked")
		//        {
		//            sql = @" select x.Id ,x.PreparedBy,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty from
		//            (
		//                SELECT IRM.Id
		//                ,CC.UserName AS CostCenterName
		//             ,B.UserName ActivityName      
		//             ,IR.RequisitionId
		//                ,IR.RequisitionDetailId                           
		//             ,EI.EmployeeName  PreparedBy	                          
		//                ,IRM.AddedBy
		//                ,IRM.AddedDate
		//                ,IRM.AddedFromIP
		//                ,IRM.UpdatedBy
		//                ,IRM.UpdatedDate
		//                ,IRM.UpdatedFromIP	  
		//               -- ,IRM.Preparedby
		//                ,IRM.CheckedBy
		//                ,IRM.CheckedByStatus
		//                ,IRM.AuthorizedBy
		//                ,IRM.AuthorizedByStatus
		//             ,RequestedQty
		//            ,RejectedQty
		//            FROM TRN.IssueRequestMaster IRM
		//            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
		//            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
		//            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
		//            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
		//             Where IRM.CheckedBy IS NOT NULL AND IRM.CheckedByStatus='ForChecked' AND IRM.AuthorizedByStatus IS NULL AND IRM.IssueSlipType='AssetSlip' AND IRM.AuthorizedBy IS null And IRM.PreparedBy='" + identity.EmployeeId + @"'
		//            )x 
		//            Group by Id ,x.PreparedBy,x.AddedDate                             
		//          ";
		//        }
		//        else if (IssueStatus == "HoldReject")
		//        {
		//            sql = @" select x.Id ,x.PreparedBy,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty from
		//            (
		//                SELECT IRM.Id
		//                ,CC.UserName AS CostCenterName
		//             ,B.UserName ActivityName      
		//             ,IR.RequisitionId
		//                ,IR.RequisitionDetailId                           
		//             ,EI.FirstName  PreparedBy	                          
		//                ,IRM.AddedBy
		//                ,IRM.AddedDate
		//                ,IRM.AddedFromIP
		//                ,IRM.UpdatedBy
		//                ,IRM.UpdatedDate
		//                ,IRM.UpdatedFromIP	  
		//               -- ,IRM.Preparedby
		//                ,IRM.CheckedBy
		//                ,IRM.CheckedByStatus
		//                ,IRM.AuthorizedBy
		//                ,IRM.AuthorizedByStatus
		//             ,RequestedQty
		//            ,RejectedQty
		//            FROM TRN.IssueRequestMaster IRM
		//            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
		//            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
		//            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
		//            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
		//         Where IRM.CheckedBy IS NOT NULL AND IRM.CheckedByStatus='Hold'OR IRM.CheckedByStatus='Reject' AND IRM.AuthorizedByStatus IS NULL  AND IRM.IssueSlipType='AssetSlip'  AND IRM.AuthorizedBy IS null And IRM.PreparedBy='" + identity.EmployeeId + @"'
		//            )x 
		//            Group by Id ,x.PreparedBy,x.AddedDate                             
		//          ";

		//        }
		//        else
		//        {
		//            sql = @" select x.Id ,x.PreparedBy,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty from
		//            (
		//                SELECT IRM.Id
		//                ,CC.UserName AS CostCenterName
		//             ,B.UserName ActivityName      
		//             ,IR.RequisitionId
		//                ,IR.RequisitionDetailId                           
		//             ,EI.FirstName  PreparedBy	                          
		//                ,IRM.AddedBy
		//                ,IRM.AddedDate
		//                ,IRM.AddedFromIP
		//                ,IRM.UpdatedBy
		//                ,IRM.UpdatedDate
		//                ,IRM.UpdatedFromIP	  
		//               -- ,IRM.Preparedby
		//                ,IRM.CheckedBy
		//                ,IRM.CheckedByStatus
		//                ,IRM.AuthorizedBy
		//                ,IRM.AuthorizedByStatus
		//             ,RequestedQty
		//            ,RejectedQty
		//            FROM TRN.IssueRequestMaster IRM
		//            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
		//            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
		//            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
		//            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
		//            Where IRM.CheckedBy IS NOT NULL AND IRM.CheckedByStatus='Checked' AND IRM.AuthorizedByStatus IS NULL  AND IRM.IssueSlipType='AssetSlip' AND IRM.AuthorizedBy IS not null And IRM.PreparedBy='" + identity.EmployeeId + @"'

		//            )x 
		//            Group by Id ,x.PreparedBy,x.AddedDate                             
		//          ";

		//        }
		//    }

		//}


		return _sqlRepository.GetDataCollection(sql);
	}

	catch (Exception ex)
	{
		throw new CustomException(ex.Message, ex,
			Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
			ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
	}
}





public IEnumerable<object> AssetIssueListData(string IssueStatus, string IssueSlipType)
{
	try
	{
		var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
		var sql = "";
		if (IssueSlipType == "AssetSlip" || IssueSlipType == "undefined")
		{
			if (IssueStatus == "ForChecked")
			{
				sql = @"  select x.Id ,x.PreparedBy,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty,x.CheckedBy from
                            (
                                SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.EmployeeName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,IRM.CheckedBy
                                ,IRM.CheckedByStatus
                                ,IRM.AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                             Where IRM.PreparedBy='" + identity.EmployeeId + @"'
						     AND IRM.CheckedByStatus='ForChecked'
						     AND IRM.AuthorizedBy IS null 
                       

							 UNION ALL
							   SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.EmployeeName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,IRM.CheckedBy
                                ,IRM.CheckedByStatus
                                ,IRM.AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
							     Where IRM.PreparedBy='" + identity.EmployeeId + @"'
						     AND IRM.CheckedByStatus IS NULL
						     AND IRM.AuthorizedBy ='For Approval'
                            AND IRM.IssueSlipType='AssetSlip' 

							 UNION ALL

							   SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.EmployeeName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,IRM.CheckedBy
                                ,IRM.CheckedByStatus
                                ,IRM.AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                               Where  IRM.PreparedBy='" + identity.EmployeeId + @"'
						     AND IRM.CheckedByStatus IS NULL
						     AND IRM.AuthorizedBy IS null 
                            AND IRM.IssueSlipType='AssetSlip' 
                            )x 
                            Group by Id ,x.PreparedBy,x.AddedDate,x.CheckedBy                             
                                                    
                          ";
			}
			else if (IssueStatus == "HoldReject")
			{
				sql = @" select x.Id ,x.PreparedBy,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty from
                            (
                                SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.FirstName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,IRM.CheckedBy
                                ,IRM.CheckedByStatus
                                ,IRM.AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                           Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                         Where IRM.PreparedBy='" + identity.EmployeeId + @"'
                        AND IRM.CheckedByStatus='Hold'OR IRM.CheckedByStatus='Reject' 
                        AND IRM.AuthorizedByStatus IS NULL 
                       AND IRM.IssueSlipType='AssetSlip' 
                            )x 
                            Group by Id ,x.PreparedBy,x.AddedDate                             
                          ";

			}
			else
			{
				sql = @" select x.Id ,x.PreparedBy,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty from
                            (
                                SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.FirstName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,IRM.CheckedBy
                                ,IRM.CheckedByStatus
                                ,IRM.AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                            Where IRM.PreparedBy='" + identity.EmployeeId + @"'
                            AND IRM.CheckedByStatus='Checked' 
                            AND IRM.AuthorizedByStatus ='For Approval' 
                            AND IRM.IssueSlipType='AssetSlip' 
                            )x 
                            Group by Id ,x.PreparedBy,x.AddedDate                             
                          ";

			}


		}




		return _sqlRepository.GetDataCollection(sql);
	}

	catch (Exception ex)
	{
		throw new CustomException(ex.Message, ex,
			Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
			ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
	}
}


public IEnumerable<object> ApprovedIssueSlipGridData(string IssueStatusApproval, string IssueSlipType)
{
	var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
	try
	{
		var _sql = "";
		if (IssueSlipType == "InventorySlip")
		{
			if (IssueStatusApproval == "Approval")
			{

				_sql = @" select x.Id ,x.PreparedBy,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty  ,x.CheckedBy
                             ,CheckedByStatus
                             ,x.AuthorizedBy
                             ,AuthorizedByStatus from
                            (
                                SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.EmployeeName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,EI1.EmployeeName CheckedBy
                                ,IRM.CheckedByStatus
                                ,EI2.EmployeeName AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                            LEFT JOIN EmployeeInformation EI1 On EI1.SystemId=IRM.CheckedBy
                            LEFT JOIN EmployeeInformation EI2 On EI2.SystemId=IRM.AuthorizedBy
                            where IRM.CheckedByStatus='Checked' 
                            AND IRM.IssueSlipType ='InventorySlip' 
                            AND IRM.AuthorizedByStatus='Approved' 
                            And IRM.AuthorizedBy='" + identity.EmployeeId + @"'
                            UNION ALL
                            SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.EmployeeName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,EI1.EmployeeName CheckedBy
                                ,IRM.CheckedByStatus
                                ,EI2.EmployeeName AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                            LEFT JOIN EmployeeInformation EI1 On EI1.SystemId=IRM.CheckedBy
                            LEFT JOIN EmployeeInformation EI2 On EI2.SystemId=IRM.AuthorizedBy
                            where IRM.CheckedByStatus IS NULL
                            AND IRM.IssueSlipType ='InventorySlip' 
                            AND IRM.AuthorizedByStatus='Approved' 
                            And IRM.AuthorizedBy='" + identity.EmployeeId + @"'
                             UNION ALL
                            SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.EmployeeName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,EI1.EmployeeName CheckedBy
                                ,IRM.CheckedByStatus
                                ,EI2.EmployeeName AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                            LEFT JOIN EmployeeInformation EI1 On EI1.SystemId=IRM.CheckedBy
                            LEFT JOIN EmployeeInformation EI2 On EI2.SystemId=IRM.AuthorizedBy
                            where IRM.CheckedByStatus IS NULL
                            AND IRM.IssueSlipType ='InventorySlip' 
                            AND IRM.AuthorizedByStatus Is NULL 
                            And IRM.AuthorizedBy='" + identity.EmployeeId + @"'
                            )x 
                            Group by Id ,x.PreparedBy,x.AddedDate,x.CheckedBy
                             ,CheckedByStatus
                             ,x.AuthorizedBy
                             ,AuthorizedByStatus";
			}
			else
			{
				_sql = @"select x.Id ,x.PreparedBy,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty  ,x.CheckedBy
                         ,CheckedByStatus
                         ,x.AuthorizedBy
                         ,AuthorizedByStatus from
                            (
                                SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.EmployeeName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                    ,IRM.CheckedBy
                                ,IRM.CheckedByStatus
                                ,IRM.AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                            where IRM.AuthorizedBy='" + identity.EmployeeId + @"'
                            AND IRM.CheckedByStatus='Checked' 
                            AND IRM.AuthorizedByStatus='Hold' OR  IRM.AuthorizedByStatus='Reject' 
                            AND IRM.IssueSlipType ='AssetSlip'
                            )x 
                            Group by Id ,x.PreparedBy,x.AddedDate,x.CheckedBy
                             ,CheckedByStatus
                             ,x.AuthorizedBy
                             ,AuthorizedByStatus";
			}
		}

		else
		{
			if (IssueStatusApproval == "Approval")
			{

				_sql = @" select x.Id ,x.PreparedBy,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty from
                            (
                                SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.EmployeeName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,IRM.CheckedBy
                                ,IRM.CheckedByStatus
                                ,IRM.AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                              where IRM.AuthorizedBy='" + identity.EmployeeId + @"'
                            AND IRM.CheckedByStatus='Checked' 
                            AND IRM.AuthorizedByStatus='Approved'
                            AND IRM.IssueSlipType ='AssetSlip'

                            UNION ALL 

                              SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.EmployeeName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,IRM.CheckedBy
                                ,IRM.CheckedByStatus
                                ,IRM.AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                              where IRM.AuthorizedBy='" + identity.EmployeeId + @"'
                            AND IRM.CheckedByStatus IS NULL
                            AND IRM.AuthorizedByStatus='Approved'
                            AND IRM.IssueSlipType ='AssetSlip'



                      UNION ALL 
                          SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.EmployeeName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,IRM.CheckedBy
                                ,IRM.CheckedByStatus
                                ,IRM.AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                              where IRM.AuthorizedBy='" + identity.EmployeeId + @"'
                            AND IRM.CheckedByStatus IS NULL
                            AND IRM.AuthorizedByStatus  IS NULL
                            AND IRM.IssueSlipType ='AssetSlip'



                            )x 
                            Group by Id ,x.PreparedBy,x.AddedDate";
			}
			else
			{
				_sql = @" select x.Id ,x.PreparedBy,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty from
                            (
                                SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.EmployeeName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,IRM.CheckedBy
                                ,IRM.CheckedByStatus
                                ,IRM.AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                            where IRM.AuthorizedBy='" + identity.EmployeeId + @"'
                            AND IRM.CheckedByStatus='Checked' 
                            AND IRM.AuthorizedByStatus='Hold' OR  IRM.AuthorizedByStatus='Reject' 
                            AND IRM.IssueSlipType ='AssetSlip'

                            )x 
                            Group by Id ,x.PreparedBy,x.AddedDate";
			}
		}





		return _sqlRepository.GetDataCollection(_sql);
	}
	catch (Exception ex)
	{
		throw new CustomException(ex.Message, ex,
			Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
			ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
	}
}

public IEnumerable<object> GetSavedPOList(string GRNId)
{
	var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
	try
	{
		var _sql = "";
		_sql = @" 
                                    --DECLARE @plantId VARCHAR(10)='20171';
                                    SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
                                    , CP.UserName AS PartyAccountGroupName
                                    , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
                                    --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
                                    , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
                                    , IR.FixedAssetOrInventory, IR.PODepended,'' PurchaseDocAcceptanceDetailId
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
                                    , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
                                    , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
                                    , IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,IPP.UserName As InvoicingByName
                                    ,pgl.CtnId,0'Active',CU.Code Currency
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
                                    LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                                    LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
                                    LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                                    LEFT JOIN (SELECT A.InventoryReceiveId,A.QtyStatus, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
                                    JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId,A.QtyStatus) AS IRD ON IRD.InventoryReceiveId=IR.Id
                                    LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id

                                    WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                                    LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                                    LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approval' group by POID) as pgl  on pgl.POID=IR.Id
                                    LEFT JOIN trn.POGGRNMap Aa ON Aa.POid=IR.Id
                                    WHERE IR.PlantId='" + identity.PlantId + @"'
                                    AND IR.IsClosed=0 and IRD.QtyStatus=0  AND IR.POType='PO' AND pgl.CtnId is not null
                                    AND Aa.GRNID='" + GRNId + @"'
                                    Order by IR.PODate ASC";
		return _sqlRepository.GetDataCollection(_sql);
	}
	catch (Exception ex)
	{
		throw new CustomException(ex.Message, ex,
			Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
			ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
	}
}

public GridModel IssueListById(GridParameter parameters, string Id)
{
	try
	{
		parameters.CmdText = @"DECLARE @plantId VARCHAR(10)='" + Id + @"'; 
                                     Select 
                                    IR.Id,outIDR1.Id InventoryMaterialId
                                    ,CC.UserName AS CostCenterName
                                    ,B.UserName ActivityName 
                                    ,IR.RequisitionId
                                    ,IR.RequisitionDetailId
                                    
                                   -- ,EI.FirstName+''+ EI.MiddleName+''+EI.LastName AS PreparedBy


                                    ,En.Username As EntityName
                                    ,MRM.EntityId
                                    ,Bu.Code
                                    ,Bu.UserName 
                                    ,Bu1.Code
                                    ,Bu1.UserName Activity
                                    ,Us.FullName AddedBy
                                    ,MRM.Id RequisitionNo
                                    ,MRD.ArticleId
                                    ,Dp.UserName DepartmentName
                                    ,MGM.UserName MaterialMasterGroupName
                                    ,mm.UserName MaterialMasterName,mm.Id MaterialMasterId
                                    ,ART.StandardName StandardName
                                    ,MT.UserName MaterialType
                                    ,MRD.FirstCharacteristicsId
                                    ,FC.UserName AS FirstCharacteristics
                                    ,MRD.FirstCharacteristicsValueId
                                    ,FCV.UserName AS FirstCharacteristicsValue
                                    ,MRD.SecondCharacteristicsId
                                    ,SC.UserName AS SecondCharacteristics
                                    ,MRD.SecondCharacteristicsValueId
                                    ,SCV.UserName AS SecondCharacteristicsValue
                                    ,MRD.ThirdCharacteristicsId
                                    ,TC.UserName AS ThirdCharacteristics
                                    ,MRD.ThirdCharacteristicsValueId
                                    ,TCV.UserName AS ThirdCharacteristicsValue
                                    ,IR.ExpenseActivityId
                                    ,IR.CostCenterId
                                    ,IR.ExpenseActivityId
                                    ,IR.BudgetMasterId
                                    ,IR.GLGeneralInfoId 
                                    ,isnull(outIDR.ApprovedQty,0) ApprovedQty
									,isnull(outIDR.RejectionQty,0) RejectionQty1	
									
									,ISNULL(outIDR1.TotalQty,0) TotalQty                           
								    ,ISNULL(IssuedQtyOut.IssueQty,0) AS IssuedQty
									,IR.RequestedQty
                                    ,IR.RejectedQty
                                    ,isnull(IGL1.UserName,'') AS CGL									
									,isnull(B1.UserName,'') AS CBUdget
									,isnull(IA1.UserName,'') AS GLBudgetActivity
                                    --,IR.AddedBy
                                    --,IR.AddedDate
                                    --,IR.AddedFromIP
                                    --,IR.UpdatedBy
                                    --,IR.UpdatedDate
                                    --,IR.UpdatedFromIP	
                                    --,IR.Preparedby
                                    --,IR.CheckedBy
                                    --,IR.CheckedByStatus
                                    --,IR.AuthorizedBy
                                    --,IR.AuthorizedByStatus
                                    ,c.UserName CountryName, C.Id CountryId
                                    from trn.IssueRequest IR
                                    left Join [TRN].[MaterialRequsitionDetails] As MRD on MRD.Id=IR.REquisitionDetailId
                                    Left Join [TRN].[MaterialRequsitionMaster] As MRM On MRD.MaterialReqqusitionMasterId=MRM.Id
                                    Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                                    Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                                   -- LEFT JOIN EmployeeInformation EI On EI.SystemId=IR.Preparedby
                                    Left Join [ORG].[Entity] As En On MRM.EntityId=En.Id
                                    Left Join [HKP].[Budget] As Bu On Bu.Id=MRD.ActivityId
                                    Left Join [HKP].[Budget] As Bu1 On Bu1.Id=IR.ExpenseActivityId
                                    Left JOIN MST.MaterialMaster AS MM ON IR.MaterialMasterId = MM.Id
                                    LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                                    LEFT JOIN MST.MaterialMasterArticle AS ART ON IR.ArticleId = ART.Id
                                    LEFT JOIN HKP.Characteristics AS FC ON IR.FirstCharacteristicsId = FC.Id
                                    LEFT JOIN HKP.Characteristics AS SC ON IR.SecondCharacteristicsId = SC.Id
                                    LEFT JOIN HKP.Characteristics AS TC ON IR.ThirdCharacteristicsId = TC.Id
                                    LEFT JOIN HKP.CharacteristicsValue AS FCV ON IR.FirstCharacteristicsValueId = FCV.Id
                                    LEFT JOIN HKP.CharacteristicsValue AS SCV ON IR.SecondCharacteristicsValueId = SCV.Id
                                    LEFT JOIN HKP.CharacteristicsValue AS TCV ON IR.ThirdCharacteristicsValueId = TCV.Id
                                    LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MRD.TransactionUoMId = TUoM.Id
                                    LEFT JOIN [SEC].[User] As Us On MRM.AddedBy=Us.UserId
                                    LEFT JOIN dbo.EmployeeInformation As Em On Us.EmployeeId=Em.SystemId
                                    LEFT JOIN [ORG].[Department] AS Dp On Dp.Id=Em.DepartmentId
                                    LEFT JOIN(  Select B.RequisitionDetailId,Sum(A.TransactionQty) ApprovedQty, Sum(A.RejectQty) RejectionQty 
												FROM TRN.GRNPORequisitionAllocation A 
												LEFT JOIN trn.PoRequisitionDetail B On A.POReqDetailsID=B.id
												group by  RequisitionDetailId
									) outIDR ON outIDR.RequisitionDetailId=IR.RequisitionDetailId

									LEFT JOIN(						
											  SELECT IM.Id ,Sum((((isnull(IRD.TransactionQty,0)-isnull(IRD.IssueQty,0))-isnull(IRD.PurchaseReturnQty,0))+isnull(IRD.IssueReturnQty,0))-isnull(IRD.ReductionByAdjustmentQty,0)) TotalQty
												FROM  TRN.InventoryMaterial IM  
												LEFT JOIN TRN.InventoryReceiveDetail IRD  ON IM.Id=IRD.InventoryMaterialId	
												----where IM.Id='220' 
												Group BY IM.Id
								 ) outIDR1 ON  outIDR1.Id=IR.InventoryMaterialId

								 Left JOIN (SELECT MRD.MaterialReqqusitionMasterId, MRD.Id AS RequisitionDetailId 
								 ,sum(RID.IssueQty) IssueQty, sum(RID.IssueRejectedQty) IssueRejectedQty
										FROM TRN.RequisitionIssueDetail RID
										Left JOIn TRN.InventoryIssue II ON II.Id=RID.IssueMasterId
										LEFT JOIN TRN.InventoryIssueDetail IID ON IID.Id=RID.IssueDetailId
										Left JOIN TRN. IssueRequest IR ON IR.Id=RID.IssueRequestId
										Left JOIN TRN.MaterialRequsitionDetails MRD On MRD.id=IR.RequisitionDetailId
									Group By MRD.MaterialReqqusitionMasterId, MRD.Id
									) IssuedQtyOut ON IssuedQtyOut.RequisitionDetailId=IR.RequisitionDetailId

                                    LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id 

									LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IR.GLGeneralInfoId 
									LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IR.BudgetMasterId
									LEFT JOIN HKP.Activity IA1 ON IA1.Id=IR.ExpenseActivityId
									Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
                                    Left Join scs.country C On C.Id=Ir.CountryId
									where IR.IssueRequestMasterId='" + Id + "'";



		return _sqlRepository.GetDifferentGridData(parameters);
	}
	catch (Exception ex)
	{
		throw new CustomException(ex.Message, ex,
			Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
			ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
	}
}

public IEnumerable<object> IssueSlipDetail(string Id)
{
	try
	{
		var _sql = @"DECLARE @plantId VARCHAR(10)='" + Id + @"'; 
                                    Select 
                                    IR.Id
                                    ,CC.UserName AS CostCenterName
                                    ,B.UserName ActivityName 
                                    ,IR.RequisitionId
                                    ,IR.RequisitionDetailId
                                    ,IR.RequestedQty
                                    ,IR.RejectedQty
                                   -- ,EI.FirstName+''+ EI.MiddleName+''+EI.LastName AS PreparedBy


                                    ,En.Username As EntityName
                                    ,MRM.EntityId
                                    ,Bu.Code
                                    ,Bu.UserName ,IR.IssueRequestMasterId
                                    ,Bu1.Code
                                    ,Bu1.UserName Activity
                                    ,Us.FullName AddedBy
                                    ,MRM.Id RequisitionNo
                                    ,MRD.ArticleId
                                    ,Dp.UserName DepartmentName
                                    ,MGM.UserName MaterialMasterGroupName
                                    ,mm.UserName Material
                                    ,ART.StandardName ArticleName
                                    ,MT.UserName MaterialType
                                    ,MRD.FirstCharacteristicsId
                                    ,FC.UserName AS FirstCharacteristics
                                    ,MRD.FirstCharacteristicsValueId
                                    ,FCV.UserName AS Sku1
                                    ,MRD.SecondCharacteristicsId
                                    ,SC.UserName AS SecondCharacteristics
                                    ,MRD.SecondCharacteristicsValueId
                                    ,SCV.UserName AS Sku2
                                    ,MRD.ThirdCharacteristicsId
                                    ,TC.UserName AS ThirdCharacteristics
                                    ,MRD.ThirdCharacteristicsValueId
                                    ,TCV.UserName AS Sku3
                                    ,IR.ExpenseActivityId
                                    ,IR.CostCenterId
                                    ,IR.ExpenseActivityId
                                    ,IR.BudgetMasterId
                                    ,IR.GLGeneralInfoId 
                                    --,IR.AddedBy
                                    --,IR.AddedDate
                                    --,IR.AddedFromIP
                                    --,IR.UpdatedBy
                                    --,IR.UpdatedDate
                                    --,IR.UpdatedFromIP	
                                    --,IR.Preparedby
                                    --,IR.CheckedBy
                                    --,IR.CheckedByStatus
                                    --,IR.AuthorizedBy
                                    --,IR.AuthorizedByStatus
                                      ,IR.IssueRequestMasterId
                                      ,isnull(IGL1.UserName,'') AS CGL									
									,isnull(B1.UserName,'') AS CBUdget
									,isnull(IA1.UserName,'') AS GLBudgetActivity,TUoM.UserName UOM
                                    from trn.IssueRequest IR
                                    left Join [TRN].[MaterialRequsitionDetails] As MRD on MRD.Id=IR.REquisitionDetailId
                                    Left Join [TRN].[MaterialRequsitionMaster] As MRM On MRD.MaterialReqqusitionMasterId=MRM.Id
                                    Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                                    Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                                   -- LEFT JOIN EmployeeInformation EI On EI.SystemId=IR.Preparedby
                                    Left Join [ORG].[Entity] As En On MRM.EntityId=En.Id
                                    Left Join [HKP].[Budget] As Bu On Bu.Id=MRD.ActivityId
                                    Left Join [HKP].[Budget] As Bu1 On Bu1.Id=IR.ExpenseActivityId
                                    Left JOIN MST.MaterialMaster AS MM ON IR.MaterialMasterId = MM.Id
                                    LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                                    LEFT JOIN MST.MaterialMasterArticle AS ART ON IR.ArticleId = ART.Id
                                    LEFT JOIN HKP.Characteristics AS FC ON IR.FirstCharacteristicsId = FC.Id
                                    LEFT JOIN HKP.Characteristics AS SC ON IR.SecondCharacteristicsId = SC.Id
                                    LEFT JOIN HKP.Characteristics AS TC ON IR.ThirdCharacteristicsId = TC.Id
                                    LEFT JOIN HKP.CharacteristicsValue AS FCV ON IR.FirstCharacteristicsValueId = FCV.Id
                                    LEFT JOIN HKP.CharacteristicsValue AS SCV ON IR.SecondCharacteristicsValueId = SCV.Id
                                    LEFT JOIN HKP.CharacteristicsValue AS TCV ON IR.ThirdCharacteristicsValueId = TCV.Id
                                     LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IR.TransactionUoMId = TUoM.Id
                                    LEFT JOIN [SEC].[User] As Us On IR.AddedBy=Us.UserId
                                    LEFT JOIN dbo.EmployeeInformation As Em On Us.EmployeeId=Em.SystemId
                                    LEFT JOIN [ORG].[Department] AS Dp On Dp.Id=Em.DepartmentId
                                    LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
                                    LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IR.GLGeneralInfoId 
									LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IR.BudgetMasterId
									LEFT JOIN HKP.Activity IA1 ON IA1.Id=IR.ExpenseActivityId
									Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId";




		return _sqlRepository.GetDataCollection(_sql);
	}
	catch (Exception ex)
	{
		throw new CustomException(ex.Message, ex,
			Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
			ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
	}
}

#region  IssueSlipChecked and Approval
//public IEnumerable<object> IssueSlipUnChecked(string IssuStatus)
//{
//    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

//    try
//    {
//        var _sql = @" select x.Id ,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty from
//                    (
//                        SELECT IRM.Id
//                        ,CC.UserName AS CostCenterName
//                     ,B.UserName ActivityName      
//                     ,IR.RequisitionId
//                        ,IR.RequisitionDetailId                           
//                     ,EI.FirstName  PreparedBy	                          
//                        ,IRM.AddedBy
//                        ,IRM.AddedDate
//                        ,IRM.AddedFromIP
//                        ,IRM.UpdatedBy
//                        ,IRM.UpdatedDate
//                        ,IRM.UpdatedFromIP	  
//                       -- ,IRM.Preparedby
//                        ,IRM.CheckedBy
//                        ,IRM.CheckedByStatus
//                        ,IRM.AuthorizedBy
//                        ,IRM.AuthorizedByStatus
//                     ,RequestedQty
//                    ,RejectedQty
//                    FROM TRN.IssueRequestMaster IRM
//                    Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
//                    Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
//                    Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
//                    LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
//                    Where IRM.CheckedByStatus='ForChecked' And IRM.CheckedBy='" + identity.EmployeeId + @"')x Group by Id";   

//        return _sqlRepository.GetDataCollection(_sql);
//    }
//    catch (Exception ex)
//    {
//        throw new CustomException(ex.Message, ex,
//            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
//            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
//    }
//}




public IEnumerable<object> IssueSlipUnChecked(string IssuStatus)
{
	try
	{
		var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
		var sql = "";
		if (IssuStatus == "ForChecked")
		{
			sql = @" select x.Id ,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty from
                            (
                                SELECT IRM.Id
                                ,EI.SystemId
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.FirstName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,IRM.CheckedBy
                                ,IRM.CheckedByStatus
                                ,IRM.AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                            Where IRM.CheckedByStatus='ForChecked' And IRM.CheckedBy='" + identity.EmployeeId + @"')x Group by Id,SystemId";
		}
		else if (IssuStatus == "HoldReject")
		{
			sql = @" select x.Id ,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty from
                            (
                                SELECT IRM.Id
                                ,EI.SystemId
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.FirstName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,IRM.CheckedBy
                                ,IRM.CheckedByStatus
                                ,IRM.AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                            Where IRM.CheckedByStatus<>'Checked' And IRM.CheckedBy='" + identity.EmployeeId + @"')x Group by Id,SystemId";



		}
		else
		{
			sql = @" select x.Id ,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty from
                            (
                                SELECT IRM.Id
                                ,EI.SystemId
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.FirstName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,IRM.CheckedBy
                                ,IRM.CheckedByStatus
                                ,IRM.AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                           Where IRM.CheckedByStatus='Checked' And IRM.CheckedBy='" + identity.EmployeeId + @"')x Group by Id,SystemId";




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


public IEnumerable<object> IssueSlipChecked()
{
	var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
	try
	{
		var _sql = @" select x.Id ,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty from
                            (
                                SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.FirstName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,IRM.CheckedBy
                                ,IRM.CheckedByStatus
                                ,IRM.AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                           Where IRM.CheckedByStatus='Checked' And IRM.CheckedBy='" + identity.EmployeeId + @"')x Group by Id";



		return _sqlRepository.GetDataCollection(_sql);
	}
	catch (Exception ex)
	{
		throw new CustomException(ex.Message, ex,
			Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
			ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
	}
}



public void IssueSlipToChecked(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy)
{
	try
	{

		var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
		if (identity.EmployeeId == AuthorizedBy)
		{
			throw new CustomException("Please select Another Id");
		}

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
			AuthorizedById = AuthorizedBy;
			AuthorizedByStatus = "For Approval";
		}
		else if (CheckedStataus == "Hold" || CheckedStataus == "Reject")
		{

			AuthorizedById = null;

		}
		//else
		//{
		//    AuthorizedById = null;

		//}
		var Status = CheckedStataus;
		var UpdatedBy = "";
		var ip = identity.IPAddress;
		var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
		var AddedBy = identity.Name;
		var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
		var CompanyGroupId = identity.CompanyGroupId;
		var CompanyId = identity.CompanyId;
		var PlantId = identity.PlantId;
		string _sql = "Update TRN.IssueRequestMaster set CheckedByStatus='" + Status + "',AuthorizedBy='" + AuthorizedById + "',AuthorizedByStatus='" + AuthorizedByStatus + "' where id='" + PoId + "'";
		_sqlRepository.ExecuteSqlCommand(_sql);
		string _sql1 = "Insert into [TRN].[IssueSlipLog](Id," +
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
		"UpdatedFromIp,ISSUEID) " +
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


#region  ApprovingIssueSlip




public IEnumerable<object> IssueSlipUnApproved()

{

	var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
	try
	{
		var _sql = @" select x.Id ,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty from
                            (
                                SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.FirstName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,IRM.CheckedBy
                                ,IRM.CheckedByStatus
                                ,IRM.AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                            Where  IRM.AuthorizedBy='" + identity.EmployeeId + @"' And IRM.AuthorizedByStatus is null
                            )x 
                            Group by Id                          
                          ";


		return _sqlRepository.GetDataCollection(_sql);
	}
	catch (Exception ex)
	{
		throw new CustomException(ex.Message, ex,
			Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
			ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
	}
}


public IEnumerable<object> IssueSlipApproved()

{

	var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
	try
	{
		var _sql = @" select x.Id ,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty from
                            (
                                SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.FirstName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,IRM.CheckedBy
                                ,IRM.CheckedByStatus
                                ,IRM.AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                              Where  IRM.AuthorizedBy='" + identity.EmployeeId + @"' And IRM.AuthorizedByStatus='Approval'
                            )x 
                            Group by Id                          
                          ";



		return _sqlRepository.GetDataCollection(_sql);
	}
	catch (Exception ex)
	{
		throw new CustomException(ex.Message, ex,
			Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
			ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
	}
}



public void IssueSlipToApproved(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy)
{
	try
	{
		var IsApproved = 0;

		PoValue = "0";
		//  var Id = GetPK();
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
		string _sql = "Update TRN.IssueRequestMaster set AuthorizedByStatus='" + Status + "' where id='" + PoId + "'";
		_sqlRepository.ExecuteSqlCommand(_sql);
		string _sql1 = "Insert into [TRN].[IssueSlipLog](Id," +
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
		"UpdatedFromIp,ISSUEID) " +
		"values ('" + GetPK() + "'," +
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

#region Requisition Issue 
public IEnumerable<object> RequisitionIssueListData()
{
	try
	{
		var _sql = @" SELECT x.Id, Replace(CONVERT(VARCHAR(11), x.IssueDate, 106), ' ', '-') IssueDate,Sum(x.BaseQty) IssueQty ,Sum(x.TransactionQty) RejectedQty FROM
                            (
                                SELECT IVS.Id
                                ,IVS.IssueDate
                                ,IVS.AddedBy
                                ,IVS.AddedDate
                                ,IVS.AddedFromIP
                                ,IVS.UpdatedBy
                                ,IVS.UpdatedDate
                                ,IVS.UpdatedFromIP	  
	                            ,IRD.BaseQty
                            ,IRD.TransactionQty
                            FROM TRN.InventoryIssue IVS
                            Left JOin TRN.InventoryIssueDetail IRD ON IRD.InventoryIssueId=IVS.Id
                            )x 
                            GROUP BY Id ,IssueDate                              
                          ";


		return _sqlRepository.GetDataCollection(_sql);
	}
	catch (Exception ex)
	{
		throw new CustomException(ex.Message, ex,
			Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
			ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
	}
}

#endregion

#region Issue Approval

public IEnumerable<object> IssueSlipUnApproved(string IssuAppStatus)
{
	try
	{
		var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
		var sql = "";
		if (IssuAppStatus == "Approved")
		{
			sql = @" select x.Id ,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty from
                            (
                                SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.FirstName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,IRM.CheckedBy
                                ,IRM.CheckedByStatus
                                ,IRM.AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                            Where  IRM.AuthorizedBy='" + identity.EmployeeId + @"' 
                            And IRM.AuthorizedByStatus ='Approved'
                            And IRM.CheckedByStatus='Checked'
UNION ALL
SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.FirstName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,IRM.CheckedBy
                                ,IRM.CheckedByStatus
                                ,IRM.AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                            Where  IRM.AuthorizedBy='" + identity.EmployeeId + @"' 
                            And IRM.AuthorizedByStatus ='Approved'
                            And IRM.CheckedByStatus Is null
                            )x 
                            Group by Id                          
                          ";
		}
		else if (IssuAppStatus == "HoldReject")
		{
			sql = @" select x.Id ,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty from
                            (
                                SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.FirstName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,IRM.CheckedBy
                                ,IRM.CheckedByStatus
                                ,IRM.AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                            Where IRM.CheckedByStatus <> 'Checked' AND  IRM.AuthorizedBy='" + identity.EmployeeId + @"' And IRM.AuthorizedByStatus is null
                            )x 
                            Group by Id                          
                          ";



		}
		else
		{
			sql = @" select x.Id ,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty from
                            (
                                SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.FirstName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,IRM.CheckedBy
                                ,IRM.CheckedByStatus
                                ,IRM.AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                             Where  IRM.AuthorizedBy='" + identity.EmployeeId + @"' 
                            And IRM.AuthorizedByStatus='For Approval'
                            )x 
                            Group by Id                          
                          ";




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

        #endregion
    }
}