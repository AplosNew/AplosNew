using Library.Core;
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

namespace Library.Service.HumanResources
{
    public class MaternityLeavePolicyService : Service<MaternityLeavePolicy>, IMaternityLeavePolicyService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MaternityLeavePolicyService(
            IRepositoryAsync<MaternityLeavePolicy> maternityLeavePolicyRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork) : base(maternityLeavePolicyRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public override void Insert(MaternityLeavePolicy entity)
        {
            var CheckLeave = GetCheck(entity.Id, entity.CompanyId, entity.PlantId, entity.ChildNo, entity.EffectiveDate);
            if (CheckLeave.Tables[0].Rows.Count > 0)
            {
                throw new CustomException("Child No :" + entity.ChildNo + " already exists.");
            }
            try
            {
                entity.Id = "MLV-" + GetAutoNumber(nameof(MaternityLeavePolicy), PKGeneratorEnum.Auto, null, DateTime.Now);
                base.Insert(entity);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public DataSet GetCheck(string Id, string CompanyId, string PlantId, int ChildNo, DateTime EffectiveDate)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"select  m.Id from [MST].[MaternityLeavePolicy] m
                            where m.EffectiveDate='" + EffectiveDate + @"'
                            and m.CompanyId='" + CompanyId + @"'
                            and  m.PlantId= '" + PlantId + @"'
                            and  m.ChildNo= '" + ChildNo + @"'
                              and  m.Id <> '" + Id + "'"

            };
            return _sqlRepository.GetGridData(parameters).Source;
        }

        public override void Update(MaternityLeavePolicy entity)
        {
            var CheckLeave = GetCheck(entity.Id, entity.CompanyId, entity.PlantId, entity.ChildNo, entity.EffectiveDate);
            if (CheckLeave.Tables[0].Rows.Count > 0)
            {
                throw new CustomException("Child No :" + entity.ChildNo + " already exists.");
            }

            try
            {
                    base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> Query(string plantId)
        {

            try
            {
                string CmdText = @"SELECT Format(EffectiveDate,'dd-MMM-yyyy') ED,*  FROM MST.MaternityLeavePolicy
                                       
                                       WHERE PlantId = '" + plantId + "' ORDER BY ChildNo";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


    }
}