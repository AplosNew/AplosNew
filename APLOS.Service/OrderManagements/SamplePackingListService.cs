#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.OrderManagements;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.OrderManagements
{
	public class SamplePackingListService : Service<SamplePackingList>, ISamplePackingListService
	{
		#region Constructor

		private readonly IUnitOfWork _unitOfWork;
		private readonly ISqlRepository _sqlRepository;
		private readonly ISamplePackingListFormService _samplePackingListFormService;
		private readonly ISamplePackingListMaterialDetailsService _samplePackingListMaterialService;
		private readonly IRepositoryAsync<SamplePackingListMaterial> _samplePackingListMaterialRepository;
		private readonly IPKGeneratorService _pkService;

		public SamplePackingListService(
			IRepositoryAsync<SamplePackingList> samplePackingListRepository
			, IPKGeneratorService pkGeneratorService
			, ISamplePackingListFormService samplePackingListFormService
			, ISamplePackingListMaterialDetailsService samplePackingListMaterialService
			, IRepositoryAsync<SamplePackingListMaterial> samplePackingListMaterialRepository
			, IPKGeneratorService pkService
			, IUnitOfWork unitOfWork
			, ISqlRepository sqlRepository
			) : base(samplePackingListRepository, unitOfWork, pkGeneratorService)
		{
			_samplePackingListMaterialRepository = samplePackingListMaterialRepository;
			_pkService = pkService;
			_unitOfWork = unitOfWork;
			_samplePackingListFormService = samplePackingListFormService;
			_samplePackingListMaterialService = samplePackingListMaterialService;
			_sqlRepository = sqlRepository;
		}

		#endregion Constructor

		private string GetPK()
		{
			return GetAutoNumber(nameof(SamplePackingList), PKGeneratorEnum.Yearly, null, DateTime.Now);
		}

		public GridModel Query(GridParameter parameters, string plantId)
		{
			parameters.CmdText = @"SELECT SPL.Id, SPL.PartyId,P.UserName AS PartyName, SPL.PlantId, PL.UserName AS Plant, SPL.EntityId, EN.UserName AS Entity
											, SPL.SalesOrganisationId, SPL.PackingDate, SPL.InvoicingPartyPlantId, SPL.InvoicingByAddress
											, SPL.DeliveryPartyPlantId, SPL.DeliveryByAddress, SPL.PendingQty, SO.UserName AS SalesOrganisation
									FROM TRN.SamplePackingList AS SPL
									LEFT JOIN ORG.Plant AS PL ON SPL.PlantId=PL.Id
									LEFT JOIN ORG.Entity AS EN ON SPL.EntityId=EN.Id
									LEFT JOIN ORG.SalesOrganisation AS SO ON SPL.SalesOrganisationId=SO.Id
									LEFT JOIN HKP.[Party] AS P ON SPL.PartyId=P.Id WHERE SPL.PlantId='" + plantId + "'";
			return _sqlRepository.GetGridData(parameters);
		}

		public IEnumerable<object> GetPackingListByMaterialGroupMaster(string materialGroupMasterId)
		{
			try
			{
				var sql = @"SELECT TOP(1) '' Id
	                            ,'' AS SamplePackingListId
	                            , MGP.PackingFormId
	                            , PC.UserName AS PackingForm
	                            , '' AS PackingFormNo
	                            , ContainerQty=CASE WHEN MGP.IsSingleEntry=1 THEN 1 ELSE 0 END
	                            , '' AS ContentQty
	                            , MGP.Id AS MaterialGroupPackingFormId
	                            , MGP.[Sequence]
	                            , MGP.IsSingleEntry
	                            ,'' AS SecondPackingList
	                            ,[Count]=(SELECT COALESCE(COUNT(DISTINCT Id),0) FROM MST.MaterialGroupPackingForm WHERE MaterialGroupMasterId='" + materialGroupMasterId + @"')
                            FROM MST.MaterialGroupPackingForm AS MGP
                            INNER JOIN MST.MaterialGroupMaster AS MG ON MGP.MaterialGroupMasterId=MG.Id
                            INNER JOIN HKP.PackingForm AS PC ON MGP.PackingFormId=PC.Id
                            INNER JOIN (SELECT DISTINCT(ReferenceDocNo),MaterialGroupMasterId FROM TRN.SampleOrderSubMaterial AS SM
			                            INNER JOIN TRN.SampleOrder AS SO ON SM.SampleOrderId=SO.Id WHERE SM.MaterialGroupMasterId='" + materialGroupMasterId + @"')
                                        AS SSM ON SSM.MaterialGroupMasterId=MG.Id
                            WHERE MG.Id='" + materialGroupMasterId + "' ORDER BY MGP.[Sequence]";
				return _sqlRepository.GetDataCollection(sql, null);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
			}
		}

		public IEnumerable<object> Get2ndPackingListByMaterialGroupMaster(string firstFormId)
		{
			try
			{
				var sql = @"SELECT TOP(1) '' Id, MGP.PackingFormId
	                              ,PC.UserName AS PackingForm
	                              ,'' AS SamplePackingListId
	                              ,'' AS FirstFormId
	                              ,'' AS PackingFormNo
	                              ,ContainerQty=CASE WHEN MGP.IsSingleEntry=1 THEN 1 ELSE 0 END
	                              , '' AS ContentQty
	                              ,MGP.Id AS MaterialGroupPackingFormId
	                              ,MGP.[Sequence]
	                              ,MGP.IsSingleEntry
                            FROM MST.MaterialGroupPackingForm AS MGP
                            INNER JOIN MST.MaterialGroupMaster AS MG ON MGP.MaterialGroupMasterId=MG.Id
                            INNER JOIN HKP.PackingForm AS PC ON MGP.PackingFormId=PC.Id
                            WHERE MG.Id IN (SELECT DISTINCT MaterialGroupMasterId FROM TRN.SamplePackingListMaterialDetails WHERE SamplePackingListMaterialId='" + firstFormId + @"')
                            ORDER BY MGP.[Sequence] DESC";
				return _sqlRepository.GetDataCollection(sql, null);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
			}
		}

		public override void Insert(SamplePackingList entity)
		{
			try
			{
				entity.Id = GetPK();
				base.Insert(entity);
			}
			catch (CustomException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
			}
		}

		public void DeleteGraph(string id)
		{
			var flag = false;
			try
			{
				var smpMat = _samplePackingListMaterialRepository.Query(t => t.SamplePackingListId == id).Select().ToList();
				_samplePackingListMaterialService.DeleteGraph(id);
				_samplePackingListFormService.DeleteGraph(id);
				_samplePackingListMaterialRepository.Delete(smpMat);
				base.DeleteGraph(id);
				_unitOfWork.BeginTransaction();
				flag = true;
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
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
			}
			finally
			{
				if (flag)
					_unitOfWork.Rollback();
			}
		}

		#region FirstPacking

		public void InsertPackingForm(IEnumerable<SamplePackingListMaterialDetails> materialList, IEnumerable<SamplePackingListForm> firstPackingList)
		{
			var flag = false;
			try
			{
				SamplePackingListMaterial sampleMaterial = new SamplePackingListMaterial
				{
					Id = _pkService.GetAutoNumber(nameof(SamplePackingListMaterial), PKGeneratorEnum.Auto, null, DateTime.Now),
					SamplePackingListId = materialList.FirstOrDefault().SamplePackingListId
				};
				AuditService.AddedLog(sampleMaterial);
				_samplePackingListMaterialRepository.Insert(sampleMaterial);
				_samplePackingListFormService.InsertFirstPackingForm(firstPackingList, sampleMaterial.Id);
				_samplePackingListMaterialService.InsertPackingMaterial(materialList, sampleMaterial.Id);
				_unitOfWork.BeginTransaction();
				flag = true;
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
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
			}
			finally
			{
				if (flag)
					_unitOfWork.Rollback();
			}
		}

		public void UpdatePackingForm(IEnumerable<SamplePackingListMaterialDetails> materialList, IEnumerable<SamplePackingListForm> firstPackingList)
		{
			var flag = false;
			try
			{
				var samplePackingListId = materialList.FirstOrDefault().SamplePackingListMaterialId;
				SamplePackingListMaterial sampleMaterial = _samplePackingListMaterialRepository.Find(samplePackingListId);
				AuditService.UpdatedLog(sampleMaterial);
				_samplePackingListMaterialRepository.Update(sampleMaterial);
				_samplePackingListFormService.UpdateFirstPackingForm(firstPackingList);
				_samplePackingListMaterialService.UpdatePackingMaterial(materialList);
				_unitOfWork.BeginTransaction();
				flag = true;
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
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
			}
			finally
			{
				if (flag)
					_unitOfWork.Rollback();
			}
		}

		public void DeletePackingForm(string firstPackId)
		{
			var flag = false;
			try
			{
				_samplePackingListMaterialService.DeletePackingMaterial(firstPackId);
				_samplePackingListFormService.DeletePackingForm(firstPackId);
				_unitOfWork.BeginTransaction();
				flag = true;
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
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
			}
			finally
			{
				if (flag)
					_unitOfWork.Rollback();
			}
		}

		#endregion FirstPacking
	}
}