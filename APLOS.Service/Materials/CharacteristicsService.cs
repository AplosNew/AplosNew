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
    public class CharacteristicsService : Service<Characteristics>, ICharacteristicsService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ICompanyGroupCharacteristicsService _comgroupcharService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<Characteristics> _charaterRepository;

        public CharacteristicsService(
            IRepositoryAsync<Characteristics> charaterRepository,
            IPKGeneratorService pkGeneratorService,
            ICompanyGroupCharacteristicsService comgroupcharService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(charaterRepository, unitOfWork)
        {
            _charaterRepository = charaterRepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _comgroupcharService = comgroupcharService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public decimal GetAutoSequence()
        {
            try
            {
                return Query().Select().Max(r => r.Sequence + 1);
            }
            catch (Exception)
            {
                return 1.00M;
            }
        }

        public GridModel GetSearchData(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT  C.Id, C.[Sequence], C.Code, C.ShortName, C.StandardName, C.UserName, C.ValueAssignmentLevel, C.AttributeProperty, C.IsFixedNoOfCharacter, C.NoOfCharacter, C.IsMandatory, C.IsFreeField,	IsPreDefinedField, C.Active
                           FROM [" + DbSchema.HKP + @"].[" + DbTable.Characteristics + @"] AS C
                           LEFT OUTER JOIN (SELECT * FROM [" + DbSchema.HKP + @"].[" + DbTable.CompanyGroupCharacteristics + @"] WHERE CompanyGroupId='" + identity.CompanyGroupId + @"') cgc
                           ON c.Id=cgc.CharacteristicsId  WHERE ISNULL(cgc.Id,'')<>'' AND c.Archive=0 ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }

        public GridModel GetCharacteristicsSearch(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = $"SELECT C.StandardName, " +
                                  "C.Alias, " +
                                  "C.[Description], " +
                                  "C.NoOfCharacter, " +
                                  "C.IsFixedNoOfCharacter, " +
                                  "C.IsMandatory, " +
                                  "C.IsFreeField, " +
                                  "C.IsPreDefinedField, " +
                                  "C.Id " +
                           $"FROM HKP.[{DbTable.Characteristics}] AS C LEFT OUTER join " +
                           $"(SELECT * FROM HKP.[{DbTable.CompanyGroupCharacteristics}] WHERE CompanyGroupId='{identity.CompanyGroupId}') cgc " +
                           $" ON C.Id=cgc.CharacteristicsId  WHERE ISNULL(cgc.Id,'')<>'' C.Active=1 AND AND C.Archive=0 ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }

        public override void Insert(Characteristics entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var flag = false;

            try
            {
                CheckUnique(entity);
                entity.Id = GetPK();
                //entity.IsFixedNoOfCharacter = entity.AttributeProperty == AttributePropertiesEnum.Alphanumeric.ToString() ? true : false;
                entity.NoOfCharacter = entity.AttributeProperty == AttributePropertiesEnum.Alphanumeric.ToString() ? entity.NoOfCharacter : 0;
                InsertGraph(entity);
                var comgroupcharteristics = new CompanyGroupCharacteristics
                {
                    CharacteristicsId = entity.Id,
                    CompanyGroupId = identity.CompanyGroupId
                };
                _comgroupcharService.Insert(comgroupcharteristics);

                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private string GetPK()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(Characteristics), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Update(Characteristics entity)
        {
            try
            {
                CheckUnique(entity);
                //entity.IsFixedNoOfCharacter = entity.AttributeProperty == AttributePropertiesEnum.Alphanumeric.ToString() ? true : false;
                entity.NoOfCharacter = entity.AttributeProperty == AttributePropertiesEnum.Alphanumeric.ToString() ? entity.NoOfCharacter : 0;
                base.Update(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                       Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                       ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public override void Archive(string key)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var flag = false;
            try
            {
                CheckIdUse(key);
                var entity = Find(key);
                _unitOfWork.BeginTransaction();
                flag = true;
                _comgroupcharService.DeleteGraph(key);
                DeleteGraph(entity.Id);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                       Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                       ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public IEnumerable<object> GetCbo(string valueAssignment, string companyGroupId)
        {
            try
            {
				valueAssignment = valueAssignment == "G" ? "AND C.ValueAssignmentLevel='" + ValueAssignmentEnum.General+ "'" : "";

				var _sql = @"SELECT C.Id AS Value, C.UserName AS Text, C.IsFreeField, C.IsPreDefinedField, C.IsMandatory, C.AttributeProperty
								, C.IsFixedNoOfCharacter, C.NoOfCharacter, C.UserName AS CharacteristicsName, C.ValueAssignmentLevel
						FROM [HKP].[Characteristics] AS C 
						LEFT JOIN [HKP].[CompanyGroupCharacteristics] CGC ON C.Id=CGC.CharacteristicsId
						WHERE CGC.CompanyGroupId='"+ companyGroupId + @"' AND C.Active=1 AND C.Archive=0 AND CGC.Archive=0 " 
						+ valueAssignment + " ORDER BY C.UserName";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        private void CheckIdUse(string id)
        {
            //string sql = $"DECLARE @materialGridId VARCHAR(10) " +
            //             $"SELECT  @materialGridId=MaterialGridId FROM HKP.[{DbTable.MaterialGridCharacteristics}] WHERE CharacteristicsId='{id}' " +
            //             $"IF EXISTS(SELECT 1 FROM (  " +
            //             $"SELECT MaterialGridId AS CheckingColumn FROM HKP.[{DbTable.MaterialMaster}] WHERE Archive=0 " +
            //              ") A WHERE CheckingColumn=@materialGridId ) SELECT 1 ELSE SELECT 0 RETURN ";
            //var data = Convert.ToBoolean(_charaterRepository.SqlQuery<int>(sql).Single());
            //if (data)
            //    throw new CustomException("Already material master exist, you can't delete....!");
        }

        private void CheckUnique(Characteristics entity)
        {
            CheckUniqueColumn(UniqueColumnName.StandardName, entity.StandardName, r => r.StandardName == entity.StandardName && r.Id != entity.Id && !r.Archive);
        }

        public Characteristics GetForCharacteristicsValue(string characteristicsId)
        {
            return Query(r => r.Id == characteristicsId && r.Active && !r.Archive).Select().FirstOrDefault();
        }
    }
}