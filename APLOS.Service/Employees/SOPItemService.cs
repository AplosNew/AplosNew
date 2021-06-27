#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Employees
{
    public partial class SOPItemService : Service<SOPItem>, ISOPItemService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly ISOPAttachmentDetailService _sopAttachmentDetailService;
        private readonly IRepositoryAsync<SOPItem> _sopItemRepository;

        public SOPItemService(
            IRepositoryAsync<SOPItem> sopItemRepository
            , IPKGeneratorService pkGeneratorService
            , ISOPAttachmentDetailService SOPAttachmentDetailService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork
            ) : base(sopItemRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sopAttachmentDetailService = SOPAttachmentDetailService;
            _sqlRepository = sqlRepository;
            _sopItemRepository = sopItemRepository;
        }

        #endregion Constructor

        public override void Delete(object id)
        {
            var flag = false;
            try
            {
                UseChecking(id);
                _unitOfWork.BeginTransaction();
                flag = true;
                _sopAttachmentDetailService.DeleteGraphBySOPItem(id.ToString());
                DeleteGraph(id);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private void UseChecking(object id)
        {
            if (_sopItemRepository.FKDependency("[HKP].[SOPItem]", id.ToString(), "[HKP].[SOPAttachmentDetail]"))
                throw new CustomException("Delete is not allowed after transaction.");
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(SOPItem), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void InsertGraph(SOPItem entity, IEnumerable<SOPAttachmentDetail> sopAttachmentDetail)
        {
            var flag = false;
            try
            {
                if (CheckUniqueRow(entity))
                    throw new CustomException("This combination already exists!");
                _unitOfWork.BeginTransaction();
                flag = true;
                entity.Id = GetPK();
                _sopAttachmentDetailService.InsertGraph(sopAttachmentDetail, entity.Id);
                base.InsertGraph(entity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void UpdateGraph(SOPItem entity, IEnumerable<SOPAttachmentDetail> sopAttachmentDetail)
        {
            var flag = false;
            try
            {
                if (CheckUniqueRow(entity))
                    throw new CustomException("This combination already exists!");
                _unitOfWork.BeginTransaction();
                flag = true;
                base.UpdateGraph(entity);
                _sopAttachmentDetailService.InsertGraph(sopAttachmentDetail, entity.Id);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public GridModel Query(GridParameter parameters, string companyGroupId)
        {
            try
            {
                parameters.sort = "Sequence, SOPCategory, SOPSubCategory";
                parameters.CmdText = @"SELECT  SOPI.Id
		                                        ,SOPI.CompanyGroupId
		                                        ,SOPI.Sequence
		                                        ,SOPI.Code
		                                        ,SOPI.SOPCategoryId
	                                            ,SOPC.UserName AS SOPCategory
		                                        ,SOPI.SOPSubCategoryId
		                                        ,SOPSC.UserName AS SOPSubCategory
		                                        ,SOPI.ShortName
		                                        ,SOPI.StandardName
		                                        ,SOPI.UserName
		                                        ,SOPI.Objective
		                                        ,SOPI.Mission
		                                        ,SOPI.Vision
		                                        ,SOPI.Description
		                                        ,SOPI.Remarks
		                                        ,SOPI.Active
                                                ,SOPAM.TotalAttachment
                                        FROM [HKP].[SOPItem] AS SOPI
                                        LEFT OUTER JOIN [HKP].[SOPCategory] SOPC ON SOPI.SOPCategoryId = SOPC.Id
                                        LEFT OUTER JOIN [HKP].[SOPSubCategory] SOPSC ON SOPI.SOPSubCategoryId = SOPSC.Id
                                        LEFT OUTER JOIN (SELECT COUNT(Id) TotalAttachment,SOPItemId FROM [HKP].[SOPAttachmentDetail] group by SOPItemId) SOPAM on SOPAM.SOPItemId=SOPI.Id
                                        WHERE SOPI.CompanyGroupId='" + companyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        /// <summary>
        /// This list data show without grid existing sopItemId
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="companyGroupId"></param>
        /// <param name="sopItemIds"></param>
        /// <returns></returns>
        public GridModel Query(GridParameter parameters, string companyGroupId, string[] sopItemIds)
        {
            try
            {
                var sopItemId = "";
                if (sopItemIds.Length > 0)
                    sopItemId = string.Join(",", sopItemIds.Select(item => "'" + item + "'"));
                else
                    sopItemId = "' '";
                parameters.CmdText = @"SELECT  SOPI.Id
		                                        ,SOPI.CompanyGroupId
		                                        ,SOPI.Sequence
		                                        ,SOPI.Code
		                                        ,SOPI.SOPCategoryId
	                                            ,SOPC.UserName AS SOPCategory
		                                        ,SOPI.SOPSubCategoryId
		                                        ,SOPSC.UserName AS SOPSubCategory
		                                        ,SOPI.ShortName
		                                        ,SOPI.StandardName
		                                        ,SOPI.UserName
		                                        ,SOPI.Objective
		                                        ,SOPI.Mission
		                                        ,SOPI.Vision
		                                        ,SOPI.Description
		                                        ,SOPI.Remarks
		                                        ,SOPI.Active
                                                ,SOPAM.TotalAttachment
                                        FROM [HKP].[SOPItem] AS SOPI
                                        LEFT OUTER JOIN [HKP].[SOPCategory] SOPC ON SOPI.SOPCategoryId = SOPC.Id
                                        LEFT OUTER JOIN [HKP].[SOPSubCategory] SOPSC ON SOPI.SOPSubCategoryId = SOPSC.Id
                                        LEFT OUTER JOIN (SELECT COUNT(Id) TotalAttachment,SOPItemId FROM [HKP].[SOPAttachmentDetail] group by SOPItemId) SOPAM on SOPAM.SOPItemId=SOPI.Id
                                        WHERE SOPI.CompanyGroupId='" + companyGroupId + "'  AND SOPI.Id NOT IN (" + sopItemId + ")";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        private bool CheckUniqueRow(SOPItem sopItem)
        {
            try
            {
                CustomIdentity identiy = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Any(r => r.Id != sopItem.Id && r.CompanyGroupId == identiy.CompanyGroupId && r.SOPCategoryId == sopItem.SOPCategoryId
                  && r.SOPSubCategoryId == sopItem.SOPSubCategoryId
                  && r.UserName == sopItem.UserName);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetFileBySOPId(string sopItemId)
        {
            try
            {
                string _sql = @"SELECT SOPD.Id,SOPD.SOPItemId,SOPD.FileName,SOPD.FileId From  [HKP].[SOPAttachmentDetail] SOPD
                                WHERE SOPD.SOPItemId='" + sopItemId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #region GetSequence

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets automatic sequence. </summary>
        /// <returns>   The automatic sequence. </returns>
        ///-------------------------------------------------------------------------------------------------

        public decimal GetAutoSequence()
        {
            try
            {
                return Query().Select().Max(r => r.Sequence + 1);
            }
            catch
            {
                return 1.00M;
            }
        }

        #endregion GetSequence
    }
}