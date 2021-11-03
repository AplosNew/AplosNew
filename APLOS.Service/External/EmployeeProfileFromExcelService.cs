#region Using

using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.External;
using Library.Service.Core;
using Library.Service.Systems;
using System;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.External
{
    public class EmployeeProfileFromExcelService : Service<EmployeeProfileFromExcel>, IEmployeeProfileFromExcelService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public EmployeeProfileFromExcelService(
              IRepositoryAsync<EmployeeProfileFromExcel> baseRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(baseRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region Operation

        public void Insert(List<EmployeeProfileFromExcel> entities)
        {
            try
            {
                foreach (var item in entities)
                {
                    var dbdata = Find(item.SystemId);
                    if (dbdata == null)
                    {
                        base.Insert(item);
                    }
                    else
                    {
                        Update(item);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion Operation
    }
}