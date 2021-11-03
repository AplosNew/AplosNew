#region Using

using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Processes;
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

namespace Library.Service.Processes
{
    public class ProcessConfigService : Service<ProcessConfig>, IProcessConfigService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<ProcessConfig> _processConfigRepository;

        public ProcessConfigService(
            IRepositoryAsync<ProcessConfig> processConfigRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(processConfigRepository, unitOfWork, pkGeneratorService)
        {
            _processConfigRepository = processConfigRepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region EnumDropdown

        [Obsolete("Have to shift in controller")]
        public IEnumerable<object> GetProcessConfigBomOrRecipeCbo()
        {
            return Enum.GetValues(typeof(EnumProcessConfigBomOrRecipeList)).Cast<EnumProcessConfigBomOrRecipeList>().Select(v => new
            {
                Text = v.ToString(),
                Value = v.ToString()
            });
        }

        [Obsolete("Have to shift in controller")]
        public IEnumerable<object> GetProcessConfigLevelCbo()
        {
            return Enum.GetValues(typeof(EnumProcessConfigLevelList)).Cast<EnumProcessConfigLevelList>().Select(v => new
            {
                Text = v.ToString(),
                Value = v.ToString()
            });
        }

        [Obsolete("Have to shift in controller")]
        public IEnumerable<object> GetProcessConfigMaterialTaggingTypeCbo()
        {
            return Enum.GetValues(typeof(EnumProcessConfigMaterialTaggingTypeList)).Cast<EnumProcessConfigMaterialTaggingTypeList>().Select(v => new
            {
                Text = v.ToString(),
                Value = v.ToString()
            });
        }

        #endregion EnumDropdown

        private string GetPK()
        {
            return GetAutoNumber(nameof(ProcessConfig), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void Insert(IEnumerable<ProcessConfig> processConfig)
        {
            var flag = false;
            string id = GetPK();
            var count = 0;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _unitOfWork.BeginTransaction();
                flag = true;
                foreach (var item in processConfig)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        count++;
                        item.Id = id + "-" + count;
                        item.CompanyGroupId = identity.CompanyGroupId;
                        item.Active = true;
                        base.Insert(item);
                    }
                    else
                    {
                        CheckIdUse(processConfig.FirstOrDefault().MaterialMasterId);
                        item.Active = true;
                        Update(item);
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        //public override void Update(Product entity)
        //{
        //    try
        //    {
        //        base.Update(entity);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
        //        entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
        //    }
        //}

        public void Archive(IEnumerable<ProcessConfig> processConfig)
        {
            var flag = false;
            try
            {
                CheckIdUse(processConfig.FirstOrDefault().MaterialMasterId);
                var list = processConfig.Select(r => r.Id);
                var data = base.Query(r => list.Contains(r.Id)).Select();
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DeleteGraph(item);
                    }
                }
                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        public IEnumerable<object> Query(string materialMasterId)
        {
            try
            {
                string _sql = "SELECT PC.Id, " +
                                                "PC.CompanyGroupId, " +
                                                "PC.DefaultPlanning, " +
                                                "P.Id AS ProcessId, " +
                                                "P.UserName AS ProcessName, " +
                                                "PC.[Days], " +
                                                "PC.BomOrRecipe, " +
                                                "PC.[Level], " +
                                                "PC.MaterialTaggingType, " +
                                                "P.[Sequence], " +
                                                "PC.Symbol, " +
                                                "MM.MaterialGridId, " +
                                                "MM.Id AS MaterialMasterId, " +
                                                "Characteristics1Id, " +
                                            " Characteristics1Selected =  CAST(CASE ISNULL(PC.Characteristics1Id,'') " +
                                                    "WHEN '' THEN 0 " +
                                                    "ELSE 1 " +
                                                    "END AS BIT), " +
                                                "Characteristics2Id, " +
                                            " Characteristics2Selected =  CAST(CASE ISNULL(PC.Characteristics2id,'') " +
                                                     "WHEN '' THEN 0 " +
                                                     "ELSE 1 " +
                                                     "END AS BIT), " +
                                                "Characteristics3Id, " +
                                            " Characteristics3Selected =  CAST(CASE ISNULL(PC.Characteristics3id,'') " +
                                                     "WHEN '' THEN 0 " +
                                                     "ELSE 1 " +
                                                     "END AS BIT) " +
                                    $"FROM {DbSchema.Masters}.[{DbTable.MaterialMasterProcessRouting}] AS MPR " +
                                    $"LEFT OUTER JOIN {DbSchema.HKP}.[{DbTable.Process}] AS P ON MPR.ProcessId=P.Id " +
                                    $"LEFT OUTER JOIN {DbSchema.Masters}.[{DbTable.MaterialMaster}] AS MM ON MPR.MaterialMasterId = MM.Id " +
                                    $"LEFT OUTER JOIN (SELECT * FROM {DbSchema.Masters}.[{DbTable.ProcessConfig}] WHERE ISNULL(MaterialMasterId,'')='{materialMasterId}') " +
                                    $"AS PC ON PC.MaterialMasterId=MPR.MaterialMasterId and pc.ProcessId=MPR.ProcessId " +
                                    $"WHERE MPR.MaterialMasterId='{materialMasterId}' ORDER BY P.[Sequence] ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetCharacteristicsName(string materialMasterId)
        {
            try
            {
                string _sql = "SELECT  C.Id, " +
                                      "C.Alias AS Characteristics, " +
                                      "MGC.Sort " +
                                     $"FROM {DbSchema.Masters}.[{DbTable.MaterialMaster}] AS MM " +
                                     $"LEFT OUTER JOIN {DbSchema.HKP}.[{DbTable.MaterialGrid}] AS MG ON MM.MaterialGridId=MG.Id " +
                                     $"LEFT OUTER JOIN {DbSchema.HKP}.[{DbTable.MaterialGridCharacteristics}] AS MGC ON MGC.MaterialGridId=MG.Id " +
                                     $"LEFT OUTER JOIN {DbSchema.HKP}.[{DbTable.Characteristics}] AS C ON MGC.CharacteristicsId=C.Id " +
                                     $"WHERE MM.Id='{materialMasterId}' ORDER BY MGC.Sort";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public void CheckIdUse(string id)
        {
            try
            {
                string sql = "IF EXISTS(SELECT 1 FROM( " +
                                $"SELECT MaterialMasterId AS CheckingColumn FROM [{DbSchema.Transaction}].[{DbTable.RecipeMaster}] " +
                                $") A WHERE CheckingColumn = '{id}') SELECT 1 ELSE SELECT 0 RETURN ";
                var data = Convert.ToBoolean(_processConfigRepository.SqlQuery<int>(sql).Single());
                if (data)
                    throw new CustomException("Already recipe master exist, you can't delete or modifie....!");
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
    }
}