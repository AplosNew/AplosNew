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
using Library.ViewModel.OrderManagements;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
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
    public class ProcurementMasterDetailService : Service<ProcurementMasterDetail>, IProcurementMasterDetailService

    {

        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<ProcurementMasterDetail> _procurementMasterDetailRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ProcurementMasterDetailService(
            IRepositoryAsync<ProcurementMasterDetail> procurementMasterDetailRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            //, IRepositoryAsync<ProcurementMasterDetail> materialRequsitionDetailsRepository
            ) : base(procurementMasterDetailRepository, unitOfWork, pkGeneratorService)
        {
            _procurementMasterDetailRepository = procurementMasterDetailRepository;
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;

        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(ProcurementMasterDetail), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public override void Insert(ProcurementMasterDetail entity)
        {
            try
            {
                entity.Id = GetPK();
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public void DetailDeleteReq(string id)
        {
            try
            {
                var detail = Convert.ToBoolean(_procurementMasterDetailRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM [TRN].[ProcurementMasterDetail] WHERE Id='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                if (detail)
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


        //public void InsertOrUpdateGraphEdit(ProcurementMasterDetail entity)
        //{
        //    var flag = false;
        //    try
        //    {
        //        _unitOfWork.BeginTransaction();
        //        flag = true;

        //        if (!string.IsNullOrEmpty(entity.Id))
        //        {
        //            var receiveDetail = new ProcurementDetail
        //            {

        //                Id = entity.Id,
        //                CompanyGroupId = entity.CompanyGroupId,
        //                //MaterialReqqusitionMasterId = entity.MaterialMasterId,
        //                //ActivityId = entity.ActivityId,
        //                MaterialMasterId = entity.MaterialMasterId,
        //                ArticleId = entity.ArticleId,
        //                FirstCharacteristicsId = entity.FirstCharacteristicsId,
        //                FirstCharacteristicsValueId = entity.FirstCharacteristicsValueId,
        //                SecondCharacteristicsId = entity.SecondCharacteristicsId,
        //                SecondCharacteristicsValueId = entity.SecondCharacteristicsValueId,
        //                ThirdCharacteristicsId = entity.ThirdCharacteristicsId,
        //                ThirdCharacteristicsValueId = entity.ThirdCharacteristicsValueId,
        //                //MaterialDetail = entity.MaterialDetail,
        //                //TransactionUoMId = entity.TransactionUoMId,
        //                //CurrencyId = entity.CurrencyId,
        //                //TransactionQty = Convert.ToDecimal(entity.TransactionQty),
        //                //EstimatedRate = Convert.ToDecimal(entity.EstimatedRate),
        //                //TotalAmount = Convert.ToDecimal(entity.TotalAmount),
        //                //BudgetType = entity.BudgetType,
        //                //Reason = entity.Reason,
        //                //Remarks = entity.Remarks,
        //                //QualityApprovalResponsiblePersonId = entity.QualityApprovalResponsiblePersonId,
        //                //NeedSpecialAppId = entity.NeedSpecialAppId,
        //                //FutureReqApp = entity.FutureReqApp,
        //                //DeliveryDate = Convert.ToDateTime(entity.DeliveryDate),
        //                //LocalImported = entity.LocalImported,
        //                //CommitmentDate = entity.CommitmentDate
        //            };
        //            UpdateGraph(receiveDetail);
        //        }

        //        _unitOfWork.SaveChanges();
        //        flag = false;
        //        _unitOfWork.Commit();
        //    }
        //    catch (CustomException)
        //    {
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //         ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
        //    }
        //    finally
        //    {
        //        if (flag)
        //        {
        //            _unitOfWork.Rollback();
        //        }
        //    }







        //}
        private void UpdateGraph(ProcurementMasterDetail receiveDetail)
        {
            throw new NotImplementedException();
        }

        //public GridModel Query(GridParameter parameters, string companyGroupId)
        //{
        //    try
        //    {
        //        parameters.CmdText = @"SELECT * FROM [HKP].[PersonalAllowance] WHERE CompanyGroupId='" + companyGroupId + "'";
        //        return _sqlRepository.GetGridData(parameters);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
        //    }
        //}


        public IEnumerable<object> GetProcurementMasterDetailsByMasterId(string procurementMasterId)
        {
            try
            {
                var sql = @"SELECT  PMD.Id As POMasterDetailid ,PMD.ProcurementMasterId,P.Id as PartyId, p.StandardName as PartyName,PMD.PartyBaseRate,PMd.PartyPreference
										 FROM [TRN].[ProcurementMasterDetail] PMD
                                          LEFT JOIN [TRN].[ProcurementMaster] PM
                                          ON PMD.ProcurementMasterId = PM.Id
										  LEFT JOIN [HKP].[Party] P
										  ON PMD.PartyId = P.id
										  WHERE PM.Id = '" + procurementMasterId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        //public void DetailDeleteReq(string id)
        //{
        //    try
        //    {
        //        var detail = Convert.ToBoolean(_procurementMasterRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM [TRN].[ProcurementMasterDetail] WHERE Id='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
        //        if (!detail)
        //        {
        //            var data = base.Find(id);
        //            if (data.IsNull()) throw new CustomException(ServiceResources.RecordNoLonger);
        //            base.Delete(data);
        //            _unitOfWork.SaveChanges();
        //        }
        //        else throw new CustomException("Please delete first line item.");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
        //    }
        //}

        public object SqlQuery<T>(string v)
        {
            throw new NotImplementedException();
        }

        public object GetAutoSequence()
        {
            throw new NotImplementedException();
        }
    }
}