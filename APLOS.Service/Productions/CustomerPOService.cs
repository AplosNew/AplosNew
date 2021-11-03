#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Productions;
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

namespace Library.Service.Productions
{
    public class CustomerPOService : Service<CustomerPO>, ICustomerPOService
    {
        #region Constructor

        private readonly IRepositoryAsync<CustomerPO> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public CustomerPOService(
            IRepositoryAsync<CustomerPO> repository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(repository, unitOfWork, pkGeneratorService)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public GridModel GetListByMasterOrder(string companyId, string masterOrderId)
        {
            try
            {
                var sql = @"SELECT cp.Id, cp.PONumber,cp.CustomerId, p.UserName AS CustomerName,m.MasterOrderNo
			                            ,Replace(CONVERT(VARCHAR(11), cp.PODate, 106), ' ', '-') PODate
                            FROM TRN.CustomerPO cp
                            LEFT JOIN HKP.Party p ON cp.CustomerId = p.Id
                            LEFT JOIN TRN.MasterOrder m ON cp.MasterOrderId = m.Id
                            WHERE cp.Archive=0 AND cp.CompanyId='" + companyId + "' AND cp.MasterOrderId='" + masterOrderId + "'";
                return _sqlRepository.GetGridData(new GridParameter { CmdText = sql });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Tuple<string, string> InsertGraphPo(CustomerPO entity)
        {
            try
            {
                var dbList = base.Query(t => t.MasterOrderId == entity.MasterOrderId && !t.Archive).Select().ToList();
                if (dbList.Any(t => t.PONumber == entity.PONumber)) throw new CustomException(entity.PONumber + " already inserted");

                //var count = _repository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[CustomerPO] WHERE MasterOrderId='{entity.MasterOrderId}'").First();
                var count = _repository.SqlQuery<int>($"SELECT ISNULL(CAST (MAX(Id) AS int),0) Id FROM [TRN].[CustomerPO]").First();
                count++;
                //entity.Id = MakePK(entity.MasterOrderId, count, 2);
                entity.Id = count.ToString();
                entity.Active = true;
                base.Insert(entity);
                return Tuple.Create(entity.Id, entity.PONumber);
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

    }
}