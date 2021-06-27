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
    public class CommitmentMonthService : Service<CommitmentMonth>, ICommitmentMonthService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public CommitmentMonthService(
            IRepositoryAsync<CommitmentMonth> commitmentMonthRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(commitmentMonthRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public IEnumerable<object> Query(string masterId)
        {
            try
            {
                var sql = @"SELECT Id, CommitmentId, CMonth,CYear,Qty, MonthYear=CONCAT(LEFT((DATENAME( MONTH , DATEADD( MONTH , CMonth , 0 ))),3),'-',CYEAR)
                     FROM TRN.CommitmentMonth WHERE CommitmentId='" + masterId + "'";
                return _sqlRepository.GetDataCollection(sql).ToList();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private string PK => GetAutoNumber(nameof(CommitmentMonth), PKGeneratorEnum.Yearly, null, DateTime.Now);

        public void InsertGraph(string masterId, IEnumerable<CommitmentMonth> entities)
        {
            if (entities != null)
            {
                foreach (var item in entities)
                {
                    item.Id = PK;
                    item.CommitmentId = masterId;
                    base.InsertGraph(item);
                }
            }
        }

        public void UpdateGraph(string masterId, IEnumerable<CommitmentMonth> entities)
        {
            if (entities != null)
            {
                foreach (var item in entities)
                {
                    item.CommitmentId = masterId;
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        item.Id = PK;
                        base.InsertGraph(item);
                    }
                    else
                        base.UpdateGraph(item);
                }
            }
            var dbList = base.Query(t => t.CommitmentId == masterId).Select().ToList();
            if (dbList != null)
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
                            base.DeleteGraph(item);
                    }
                }
            }
        }

        public void DeleteMonth(string masterId)
        {
            var dbList = base.Query(t => t.CommitmentId == masterId).Select().ToList();
            if (dbList != null)
            {
                foreach (var item in dbList)
                {
                    base.Delete(item);
                }
            }
        }
    }
}