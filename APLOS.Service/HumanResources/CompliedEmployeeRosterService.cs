using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.HumanResources;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Systems;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;

namespace Library.Service.HumanResources
{
    public class CompliedEmployeeRosterService : Service<CompliedEmployeeRoster>, ICompliedEmployeeRosterService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CompliedEmployeeRosterService(
            IRepositoryAsync<CompliedEmployeeRoster> compliedEmployeeRosterRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork
            ) : base(compliedEmployeeRosterRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InsertOrUpdateRoster(CompliedEmployeeRoster entity)
        {
            try
            {
                if (!string.IsNullOrEmpty(entity.EmpSystemId) && string.IsNullOrEmpty(entity.CompliedShiftRosterMasterID))
                {
                    var data = GetRosterEmployeeRosterData(entity.EmpSystemId);
                    if (data.Tables[0].Rows.Count > 0)
                    {
                        string id = data.Tables[0].Rows[0]["Id"].ToString();
                        CompliedEmployeeRoster d = Find(id);
                        base.Delete(d);
                    }
                    entity = null;
                }
                if (entity != null)
                {
                    if (string.IsNullOrEmpty(entity.Id))
                    {
                        entity.Id = GetAutoNumber(nameof(CompliedEmployeeRoster), PKGeneratorEnum.Auto, null, DateTime.Now);
                        base.Insert(entity);
                    }
                    else
                    {
                        base.Update(entity);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        //public object GetRosterEmployeeRosterData(string employeeId)
        //{
        //    var sql = @"SELECT * FROM [dbo].[CompliedEmployeeRoster] Where [EmpSystemId] ='" + employeeId + "'";
        //    return _sqlRepository.GetDataCollection(sql, null);
           
        //}
        public DataSet GetRosterEmployeeRosterData(string employeeId)
        {
            GridParameter parameters = null;
            parameters = new GridParameter

            {
                ExportType = "DATASET",
                CmdText = @"SELECT * FROM [dbo].[CompliedEmployeeRoster] Where [EmpSystemId] ='" + employeeId + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }
    }
}