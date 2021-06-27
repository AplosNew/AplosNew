#region Using

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
    public class SamplePackingListFormService : Service<SamplePackingListForm>, ISamplePackingListFormService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public SamplePackingListFormService(
            IRepositoryAsync<SamplePackingListForm> samplePackingListFormRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(samplePackingListFormRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(SamplePackingListForm), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        #region FirstPackingForm

        public void InsertFirstPackingForm(IEnumerable<SamplePackingListForm> entities, string id)
        {
            try
            {
                if (entities != null)
                {
                    var pk = GetMaxNumber(nameof(SamplePackingListForm), PKGeneratorEnum.Auto, null, DateTime.Now);
                    foreach (var item in entities)
                    {
                        pk.MaxNumber++;
                        item.Id = pk.MaxNumber.ToString();
                        item.SamplePackingListMaterialId = id;
                        item.PackFormType = EnumPackFormType.First.ToString();
                        InsertGraph(item);
                    }
                }
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

        public void UpdateFirstPackingForm(IEnumerable<SamplePackingListForm> entities)
        {
            try
            {
                if (entities != null)
                {
                    var smpMaterialId = entities.FirstOrDefault().SamplePackingListMaterialId;
                    var dbList = Query(t => t.SamplePackingListMaterialId == smpMaterialId).Select().ToList();

                    var pk = GetMaxNumber(nameof(SamplePackingListForm), PKGeneratorEnum.Auto, null, DateTime.Now);
                    foreach (var item in entities)
                    {
                        if (item.Id.StartsWith("n-"))
                        {
                            pk.MaxNumber++;
                            item.Id = pk.MaxNumber.ToString();
                            item.PackFormType = EnumPackFormType.First.ToString();
                            InsertGraph(item);
                        }
                        else
                        {
                            item.PackFormType = EnumPackFormType.First.ToString();
                            UpdateGraph(item);
                        }
                    }
                    if (dbList != null)
                    {
                        foreach (var item in dbList)
                        {
                            if (!entities.Any(t => t.Id == item.Id))
                            {
                                base.DeleteGraph(item);
                            }
                        }
                    }
                }
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

        public void InsertOrUpdateSecondPackingForm(IEnumerable<SamplePackingListForm> entities)
        {
            var flag = false;
            try
            {
                if (entities != null || entities.Count() > 0)
                {
                    var pk = GetMaxNumber(nameof(SamplePackingListForm), PKGeneratorEnum.Auto, null, DateTime.Now);
                    foreach (var item in entities)
                    {
                        if (item.Id.StartsWith("n-"))
                        {
                            pk.MaxNumber++;
                            item.Id = pk.MaxNumber.ToString();
                            item.PackFormType = EnumPackFormType.Second.ToString();
                            InsertGraph(item);
                        }
                        else
                        {
                            item.PackFormType = EnumPackFormType.Second.ToString();
                            UpdateGraph(item);
                        }
                    }
                    var firstFormId = entities.First().FirstFormId;
                    var dbList = Query(t => t.FirstFormId == firstFormId && t.PackFormType == EnumPackFormType.Second.ToString()).Select().AsEnumerable();
                    if (dbList != null || dbList.Count() > 0)
                    {
                        foreach (var item in dbList)
                        {
                            if (!entities.Any(t => t.Id == item.Id))
                            {
                                base.DeleteGraph(item);
                            }
                        }
                    }
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
                else
                    throw new CustomException("Insert atleast one row.....!");
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
            try
            {
                var dbSecondForm = Query(t => t.FirstFormId == firstPackId).Select().AsEnumerable();
                if (dbSecondForm != null || dbSecondForm.Count() > 0)
                {
                    foreach (var second in dbSecondForm)
                    {
                        base.DeleteGraph(second);
                    }
                }
                base.DeleteGraph(firstPackId);
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

        public void DeleteGraph(string masterId)
        {
            try
            {
                var dbList = Query(t => t.SamplePackingListId == masterId).Select().AsEnumerable();
                if (dbList != null || dbList.Count() > 0)
                {
                    foreach (var second in dbList)
                    {
                        base.DeleteGraph(second);
                    }
                }
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

        public IEnumerable<object> GetPackingFormList(string masterId)
        {
            try
            {
                var sql = @"SELECT SPF.Id
	                              ,SPF.PackingFormId
	                              ,PC.UserName AS PackingForm
	                              ,SPF.PackingFormNo
	                              ,SPF.ContainerQty
	                              ,SPF.ContentQty
								  ,SPF.SamplePackingListMaterialId
								  ,UoMId=(SELECT TOP(1)UoMId FROM TRN.SamplePackingListMaterialDetails AS SPM
	                                 INNER JOIN SCS.UnitOfMeasurement AS UoM ON SPM.UoMId=UoM.Id WHERE SamplePackingListMaterialId=SPF.SamplePackingListMaterialId)
								  ,UoMName=(SELECT TOP(1)UoM.UserName FROM TRN.SamplePackingListMaterialDetails AS SPM
								     INNER JOIN SCS.UnitOfMeasurement AS UoM ON SPM.UoMId=UoM.Id WHERE SamplePackingListMaterialId=SPF.SamplePackingListMaterialId)
                            FROM TRN.SamplePackingListForm AS SPF
                            INNER JOIN HKP.PackingForm AS PC ON SPF.PackingFormId=PC.Id
                            WHERE SPF.SamplePackingListId='" + masterId + "' AND PackFormType='" + EnumPackFormType.First+ "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public IEnumerable<object> GetFirstPackingForm(string id, string samplePackingMaterialId, string materialGroupMstId)
        {
            try
            {
                var sql = @"SELECT DISTINCT SPF.Id
	                              ,SPF.SamplePackingListId
                                  , SPF.SamplePackingListMaterialId
	                              , SPF.PackingFormId
	                              , PC.UserName AS PackingForm
	                              , SPF.PackingFormNo
	                              , SPF.ContainerQty
	                              , SPF.ContentQty
	                              , SPF.MaterialGroupPackingFormId
	                              , MGP.[Sequence]
	                              , MGP.IsSingleEntry,B.UoMId,B.UoMName
	                              ,'' AS SecondPackingList
	                              ,[Count]=(SELECT COALESCE(COUNT(DISTINCT Id),0) FROM MST.MaterialGroupPackingForm WHERE MaterialGroupMasterId='" + materialGroupMstId + @"')
                            FROM TRN.SamplePackingListForm AS SPF
                            INNER JOIN MST.MaterialGroupPackingForm AS MGP ON SPF.MaterialGroupPackingFormId=MGP.Id
                            INNER JOIN MST.MaterialGroupMaster AS MG ON MGP.MaterialGroupMasterId=MG.Id
                            INNER JOIN HKP.PackingForm AS PC ON MGP.PackingFormId=PC.Id
                            INNER JOIN (SELECT DISTINCT(ReferenceDocNo),MaterialGroupMasterId FROM TRN.SampleOrderSubMaterial AS SM
                                        INNER JOIN TRN.SampleOrder AS SO ON SM.SampleOrderId=SO.Id) AS SSM ON SSM.MaterialGroupMasterId=MG.Id
                            INNER JOIN (SELECT TOP(1) UoMId, UoM.UserName AS UoMName,SamplePackingListMaterialId FROM TRN.SamplePackingListMaterialDetails AS SPM
                              INNER JOIN SCS.UnitOfMeasurement AS UoM ON SPM.UoMId=UoM.Id WHERE SamplePackingListMaterialId='" + samplePackingMaterialId + @"') AS B ON B.SamplePackingListMaterialId=SPF.SamplePackingListMaterialId
                            WHERE PackFormType='" + EnumPackFormType.First+ "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public IEnumerable<object> GetSecondPackByFirstPackId(string firstFormId, string samplePackingListMaterialId)
        {
            try
            {
                var sql = @"SELECT SPL.Id
	                              ,SPL.PackingFormId
	                              ,PC.UserName AS PackingForm
	                              ,SPL.SamplePackingListId
	                              ,SPL.FirstFormId
	                              ,SPL.PackingFormNo
	                              ,SPL.ContainerQty
                                  ,SPL.ContentQty
	                              ,SPL.MaterialGroupPackingFormId
	                              ,MGP.[Sequence]
	                              ,MGP.IsSingleEntry,B.UoMId,B.UoMName
	                              ,SPL.SamplePackingListMaterialId
                            FROM TRN.SamplePackingListForm AS SPL
                            INNER JOIN MST.MaterialGroupPackingForm AS MGP ON SPL.MaterialGroupPackingFormId=MGP.Id
                            INNER JOIN MST.MaterialGroupMaster AS MG ON MGP.MaterialGroupMasterId=MG.Id
                            INNER JOIN HKP.PackingForm AS PC ON MGP.PackingFormId=PC.Id
                            INNER JOIN (SELECT TOP(1) UoMId, UoM.UserName AS UoMName,SamplePackingListMaterialId FROM TRN.SamplePackingListMaterialDetails AS SPM
                                 INNER JOIN SCS.UnitOfMeasurement AS UoM ON SPM.UoMId=UoM.Id WHERE SamplePackingListMaterialId='" + samplePackingListMaterialId + @"')
                                 AS B ON B.SamplePackingListMaterialId=SPL.SamplePackingListMaterialId
                            WHERE SPL.FirstFormId='" + firstFormId + "' AND PackFormType='" + EnumPackFormType.Second+ "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        #endregion FirstPackingForm
    }
}