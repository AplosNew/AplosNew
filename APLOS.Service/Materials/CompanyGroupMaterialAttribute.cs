#region Using

using Library.Core;
using Library.Crosscutting.Security;
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
using System.Threading;

#endregion Using

namespace Library.Service.Materials
{
    public class CompanyGroupWiseMaterialAttributeService : Service<CompanyGroupMaterialAttribute>, ICompanyGroupWiseMaterialAttributeService
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
        public CompanyGroupWiseMaterialAttributeService(
            IRepositoryAsync<CompanyGroupMaterialAttribute> charaterRepository,
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
                parameters.CmdText = $"SELECT * FROM {DbSchema.HKP}.[{DbTable.CompanyGroupMaterialAttribute}] WHERE Archive=0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public override void InsertGraph(CompanyGroupMaterialAttribute entity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                entity.Id = GetAutoId();
                entity.CompanyGroupId = identity.CompanyGroupId;
                entity.Archive = false;
                base.InsertGraph(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        private string GetAutoId()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(CompanyGroupMaterialAttribute), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Update(CompanyGroupMaterialAttribute entity)
        {
            try
            {
                var data = base.Query(t => t.MaterialAttributeId == entity.MaterialAttributeId).Select().FirstOrDefault();
                data.Active = entity.Active;
                data.Archive = entity.Archive;
                UpdateGraph(data);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                entity.AddedBy, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public void DeleteGraph(string materialAttributeId)
        {
            try
            {
                var data = base.Query(t => t.MaterialAttributeId == materialAttributeId).Select().FirstOrDefault();
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public override IQueryFluent<CompanyGroupMaterialAttribute> Query()
        {
            return base.Query(r => !r.Archive);
        }

        public IEnumerable<object> GetCompanyGroupWiseMaterialAttributeList()
        {
            try
            {
                return from m in base.Query(m => !m.Archive).Select()
                       select new { Text = m.CompanyGroupId, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }
    }
}