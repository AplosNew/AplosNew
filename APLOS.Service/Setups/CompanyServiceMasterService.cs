#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Setups;
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

namespace Library.Service.Setups
{
    public class CompanyServiceMasterService : Service<CompanyServiceMaster>, ICompanyServiceMasterService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public CompanyServiceMasterService(
            IRepositoryAsync<CompanyServiceMaster> companyServiceRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
            : base(companyServiceRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InsertOrUpdate(IEnumerable<CompanyServiceMaster> entity, string companyId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                if (entity != null)
                {
                    foreach (var item in entity)
                    {
                        var _pk = GetPK();
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            item.Id = _pk;
                            InsertGraph(item);
                        }
                        else if (!string.IsNullOrEmpty(item.Id))
                            UpdateGraph(item);
                    }
                }
                var dbDataList = Query(t => t.CompanyId == companyId).Select().AsEnumerable();
                if (dbDataList.Count() > 0)
                {
                    if (entity == null)
                    {
                        foreach (var item in dbDataList)
                        {
                            DeleteGraph(item);
                        }
                    }
                    else
                    {
                        foreach (var item in dbDataList)
                        {
                            if (!entity.Any(t => t.Id == item.Id))
                                DeleteGraph(item);
                        }
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(CompanyServiceMaster), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Update(CompanyServiceMaster entity)
        {
            try
            {
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public void Delete(string Id)
        {
            try
            {
                var data = Find(Id);
                Delete(data);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public IEnumerable<object> Query(string companyId)
        {
            try
            {
                string CmdText = @"SELECT CCCE.Id
                                        ,CCCE.ServiceMasterId
                                        ,CCCE.CompanyId
                                        ,CC.Code
                                        ,CC.StandardName
                                        ,CC.UserName
                                        ,CC.ServiceGroupId
                                        ,CCSC.UserName AS ServiceGroupName
                                FROM [HKP].[CompanyServiceMaster] AS CCCE
                                LEFT JOIN [HKP].[ServiceMaster] AS CC ON CCCE.ServiceMasterId = CC.Id
                                LEFT JOIN [HKP].[ServiceGroup] AS CCSC ON CC.ServiceGroupId = CCSC.Id
                                WHERE CCCE.CompanyId ='" + companyId + "' Order by CC.Sequence";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetCboByCompany(string companyId)
        {
            try
            {
				//var _sql = @"SELECT CC.Id AS Value, CC.UserName AS Text, SG.HSNCodeId,HN.Code HSNCode FROM [HKP].[CompanyServiceMaster] AS CCCE
				//            LEFT JOIN [HKP].[ServiceMaster] AS CC  ON CC.Id=CCCE.ServiceMasterId
				//            LEFT JOIN [HKP].[ServiceGroup] AS SG ON CC.ServiceGroupId=SG.Id
				//            LEFT JOIN [HKP].[HSNCode] AS HN ON HN.Id=SG.HSNCodeId
				//            WHERE CCCE.CompanyId = '" + companyId + @"' ORDER BY CC.UserName ";
				//var _sql = @"SELECT SG.Id AS Value, SG.UserName AS Text, SG.HSNCodeId FROM [HKP].[ServiceGroup] AS SG  ORDER BY SG.UserName";
				var _sql = @"SELECT CC.Id AS Value, CC.UserName AS Text, CC.HSNCodeId,HN.Code HSNCode FROM [HKP].[CompanyServiceMaster] AS CCCE
                            LEFT JOIN [HKP].[ServiceMaster] AS CC  ON CC.Id=CCCE.ServiceMasterId
                           -- LEFT JOIN [HKP].[ServiceGroup] AS SG ON CC.ServiceGroupId=SG.Id
                            LEFT JOIN [HKP].[HSNCode] AS HN ON HN.Id=CC.HSNCodeId
                            WHERE CCCE.CompanyId = '" + companyId + @"' ORDER BY CC.UserName ";
				return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IEnumerable<object> GetCboService()
        {
            try
            {
                var _sql = @"SELECT CC.Id AS Value, CC.UserName AS Text, SG.HSNCodeId FROM [HKP].[ServiceMaster] CC
                           
                            LEFT JOIN [HKP].[ServiceGroup] AS SG ON CC.ServiceGroupId=SG.Id
                             ORDER BY CC.Sequence  ";
                //var _sql = @"SELECT SG.Id AS Value, SG.UserName AS Text, SG.HSNCodeId FROM [HKP].[ServiceGroup] AS SG  ORDER BY SG.UserName";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}