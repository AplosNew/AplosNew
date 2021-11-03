#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Materials;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Materials
{
    public class CompanyGroupCharacteristicsService : Service<CompanyGroupCharacteristics>, ICompanyGroupCharacteristicsService
    {
        #region Constructor

        /// <summary>   The unit of work. </summary>
        private readonly IUnitOfWork _unitOfWork;

        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Constructor. </summary>
        /// <param name="charaterRepository">    The repArea. </param>
        /// <param name="unitOfWork">   The unit of work. </param>
        ///-------------------------------------------------------------------------------------------------
        public CompanyGroupCharacteristicsService(
            IRepositoryAsync<CompanyGroupCharacteristics> charaterRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(charaterRepository, unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public GridModel GetSearchData(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = $"SELECT * FROM {DbSchema.HKP}.[{DbTable.CompanyGroupCharacteristics}] WHERE Archive=0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }

        public override void Insert(CompanyGroupCharacteristics charater)
        {
            try
            {
                charater.Id = GetPK();
                InsertGraph(charater);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, charater.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        private string GetPK()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(CompanyGroupCharacteristics), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Update(CompanyGroupCharacteristics charater)
        {
            try
            {
                AuditService.Log(charater);
                UpdateGraph(charater);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                     Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, charater.AddedBy,
                     ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public void DeleteGraph(string charaterId)
        {
            try
            {
                var data = base.Query(t => t.CharacteristicsId == charaterId).Select().FirstOrDefault();
                base.DeleteGraph(data);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                     Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                     ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public override IQueryFluent<CompanyGroupCharacteristics> Query()
        {
            return base.Query(r => !r.Archive);
        }

        public IEnumerable<object> GetCompanyGroupWiseCharacteristicsList()
        {
            try
            {
                return from m in base.Query(m => !m.Archive).Select()
                       select new { Text = m.CompanyGroupId, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }
    }
}