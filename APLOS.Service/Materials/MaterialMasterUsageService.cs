using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Materials;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Linq;
using System.Reflection;

namespace Library.Service.Materials
{
    public class MaterialMasterUsageService : Service<MaterialMasterUsage>, IMaterialMasterUsageService
    {
        #region Constructor

        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public MaterialMasterUsageService(
            IRepositoryAsync<MaterialMasterUsage> charaterValueRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork,
            IMaterialTypeService materialTypeService
            , ISqlRepository sqlRepository
            ) : base(charaterValueRepository, unitOfWork, pkGeneratorService)
        {
            _pkGeneratorService = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InsertOrUpdate(MaterialMasterUsage entity, string materialMasterId)
        {
            try
            {
                if (entity != null)
                {
                    if (!string.IsNullOrEmpty(entity.Id))
                    {
                        UpdateGraph(entity);
                    }
                    else
                    {
                        if (entity.BOM || entity.Recipe)
                        {
                            entity.Id = GetPK();
                            entity.MaterialMasterId = materialMasterId;
                            InsertGraph(entity);
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        private string GetPK()
        {
            return "MMU-" + GetAutoNumber(nameof(MaterialMasterUsage), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void DeleteGraph(string materialMasterId)
        {
            try
            {
                var data = base.Query(r => r.MaterialMasterId == materialMasterId).Select().FirstOrDefault();
                if (data != null)
                {
                    base.DeleteGraph(data);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public MaterialMasterUsage Query(string materialMasterId)
        {
            return base.Query(r => r.MaterialMasterId == materialMasterId).Select().FirstOrDefault();
        }

        public MaterialMasterUsage Get(string materialMasterId)
        {
            return base.Query(r => r.MaterialMasterId == materialMasterId).Select().FirstOrDefault();
        }
    }
}