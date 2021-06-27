#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.HumanResources;
using Library.Service.Attendances;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.HumanResources
{
    /// <summary>
    ///  Class ProductService.
    /// </summary>
    public partial class CompliedShiftGroupDetailService : Service<CompliedShiftGroupDetail>, ICompliedShiftGroupDetailService
    {
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;

        public CompliedShiftGroupDetailService(
            IRepositoryAsync<CompliedShiftGroupDetail> compliedShiftGroupDetailDetailRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
            : base(compliedShiftGroupDetailDetailRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InsertOrUpdate(IEnumerable<CompliedShiftGroupDetail> entities, string masterId)
        {
            try
            {
                var shiftName = string.Empty;
                var groupName = string.Empty;
                if (entities != null)
                {
                    var pk = GetMaxNumber(nameof(CompliedShiftGroupDetail), PKGeneratorEnum.Auto, null, DateTime.Now);
                    foreach (var item in entities)
                    {
                        var actualShift = Query(t => t.Id != item.Id && t.ActualShiftId == item.ActualShiftId).Select().FirstOrDefault();

                        //var shiftName = _shiftDefinationService.Query(t => t.SystemId == item.ActualShiftId).Select(t => t.ShiftDefinationName).FirstOrDefault();
                        var data = GetShiftNameAndGroup(item.ActualShiftId);
                        if (data.Count > 0)
                        {
                            shiftName = data[0]["ShiftDefinationName"].ToString();
                            groupName = data[0]["Code"].ToString();

                            if (actualShift != null)
                            {
                                throw new CustomException("Shift " + shiftName + " is already exists in shift group "+ groupName + ".");
                            }
                        }
                        if (actualShift == null)
                        {
                            if (string.IsNullOrEmpty(item.Id))
                            {
                                pk.MaxNumber++;
                                item.Id = DateTime.Now.ToString("yy") + "-" + pk.MaxNumber.ToString();
                                item.CompliedShiftGroupingId = masterId;
                                InsertGraph(item);
                            }
                            else if (!string.IsNullOrEmpty(item.Id))
                            {
                                UpdateGraph(item);
                            }
                        }
                    }
                }
                var dbList = base.Query(r => r.CompliedShiftGroupingId == masterId).Select().ToList();
                if (dbList.IsNotNull() && dbList.Count > 0)
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
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public void DeleteWithMaster(string masterId)
        {
            try
            {
                var data = Query(r => r.CompliedShiftGroupingId == masterId).Select().ToList();
                if (data != null)
                {
                    for (int i = 0; i < data.Count(); i++)
                    {
                        base.Delete(data[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public void DeleteWithChild(string Id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var data = Query(r => r.Id == Id).Select().FirstOrDefault();
                if (data != null)
                {
                    DeleteGraph(data);
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private List<Dictionary<string, object>> GetShiftNameAndGroup(string shift)
        {
            var sql = @"
                    SELECT SD.ShiftDefinationName,G.Code FROM [MST].[CompliedShiftGroupDetail] D
					LEFT JOIN dbo.ShiftDefination SD ON SD.SystemID=D.ActualShiftId
					LEFT JOIN MST.CompliedShiftGrouping G ON G.Id=D.CompliedShiftGroupingId
					WHERE ActualShiftId='" + shift + "'";
            return _sqlRepository.GetDataCollection(sql);
        }
    }
}