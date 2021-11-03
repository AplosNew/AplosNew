#region Using

using Library.Core;
using Library.Crosscutting.Security;
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
using System.Threading;

#endregion Using

namespace Library.Service.OrderManagements
{
    public class LineDayCriticalityService : Service<LineDayCriticality>, ILineDayCriticalityService
    {
        #region Constructor

        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<LineDayCriticality> _lineDayCriticalRepository;
        public LineDayCriticalityService(
             IRepositoryAsync<LineDayCriticality> lineDayCriticalRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(lineDayCriticalRepository, unitOfWork, pkGeneratorService)
        {
            _lineDayCriticalRepository = lineDayCriticalRepository;
            _pkGeneratorService = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor



        #region InsertUpdate

        public void InsertOrUpdate(IEnumerable<LineDayCriticality> entities)
        {
            var flag = false;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _unitOfWork.BeginTransaction();
                flag = true;
                var pk = GetMaxNumber(nameof(LineDayCriticality), PKGeneratorEnum.Auto, identity.CompanyGroupId, DateTime.Now);
                //Check(entities);
                foreach (var item in entities)
                {
                    var dbOb = base.Query(t => t.WorkDay == item.WorkDay && t.CriticalId==item.CriticalId).Select().FirstOrDefault();
                    if (dbOb ==null)
                    {
                        if (item.Efficiency !=null)
                        {
                            pk.MaxNumber++;
                            item.Id = pk.MaxNumber.ToString();
                            InsertGraph(item);
                        }
                    }
                    else
                    {
                        if (item.Efficiency ==null)
                        {
                            base.Delete(dbOb);
                        }
                        else
                        {
                            item.Id = dbOb.Id;
                            UpdateGraph(item);
                        }

                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name.ToString(), null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private string GetAutoId()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(LineDayCriticality), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Update(LineDayCriticality entity)
        {
            try
            {
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, nameof(Setups)));
            }
        }

        #endregion InsertUpdate

        public void DeleteGraph(string workday)
        {
            var flag = false;

            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                int wDay = Convert.ToInt32(workday);
                var dbOb = base.Query(t => t.WorkDay == wDay).Select().ToList();
                foreach (var item in dbOb)
                {
                base.DeleteGraph(item);
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, nameof(Setups)));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public GridModel Query(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                parameters.sort = "WorkDay";
                parameters.order = "ASC";
                parameters.CmdText = @"SELECT LC.*,C.UserName FROM SCS.LineDayCriticality LC
                                        LEFT JOIN HKP.Critical C ON LC.CriticalId=C.Id";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }
    }
}