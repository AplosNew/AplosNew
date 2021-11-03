#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Setups;
using Library.Model.Systems;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Setups
{
    public class TnaSettingDetailService : Service<TnaSettingDetail>, ITnaSettingDetailService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public TnaSettingDetailService(
            IRepositoryAsync<TnaSettingDetail> skillCategoryRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(skillCategoryRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber("TnaSettingDetail", PKGeneratorEnum.Yearly, null, DateTime.Now);
        }


        public void InsertUpdate(IEnumerable<TnaSettingDetail> entities,string masterId)
        {
            var flag = false;
            try
            {
                if (entities != null)
                {
                    var pk = GetMaxNumber();
                    foreach (var item in entities)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            pk.MaxNumber++;
                            item.Id = pk.MaxNumber.ToString();
                            item.TnaSettingMasterId = masterId;
                            InsertGraph(item);
                        }
                        else
                        {
                            UpdateGraph(item);
                        }
                    }
                    var dbList = base.Query(t => t.TnaSettingMasterId == masterId).Select().ToList();
                    if (dbList != null && dbList.Count() > 0)
                    {
                        if (entities == null)
                        {
                            foreach (var item in dbList)
                            {
                                base.DeleteGraph(item);
                            }
                        }
                        else
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
                else
                {
                    throw new CustomException("No data to save.");
                }
                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private PKGenerator GetMaxNumber()
        {
            return base.GetMaxNumber(nameof(TnaSettingDetail), PKGeneratorEnum.Auto, null, DateTime.Now);
        }


        public void DeleteGraph(string id)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new CustomException(string.Format(ResourcesCore.IsNull, "TnaSettingDetail Id"));

                _unitOfWork.BeginTransaction();
                flag = true;
                TnaSettingDetail entity = Find(id);
                // If section row inactive
                DeleteGraph(entity.Id);
                base.DeleteGraph(entity);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Organization.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        public GridModel Query(GridParameter parameters, string shiftGroupId,string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT CASE ISNULL(SGD.Id,'') when '' then CAST('False' as bit)else CAST('TRUE' as bit) end Flag,SGD.*,SD.SystemID,SD.UserName,SD.ShiftType FROM [dbo].[ShiftDefination] SD 
                                      LEFT JOIN SCS.TnaSettingDetail SGD ON SD.SystemID=SGD.ShiftDefinationId AND  SGD.ShiftGroupId='"+ shiftGroupId + @"' WHERE PlantID='"+ plantId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }
    }
}