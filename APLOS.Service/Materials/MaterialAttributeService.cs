#region Using

using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.Model.Enums;
using Library.Model.Materials;
using Library.Data;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using System;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Library.ViewModel.Materials;

#endregion Using

namespace Library.Service.Materials
{
    public class MaterialAttributeService : Service<MaterialAttribute>, IMaterialAttributeService
    {
        #region Constructor

        private readonly IRepositoryAsync<MaterialAttribute> _charaterRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ICompanyGroupWiseMaterialAttributeService _groupAttrService;
        private readonly ISqlRepository _sqlRepository;

        public MaterialAttributeService(
            IRepositoryAsync<MaterialAttribute> charaterRepository,
            IPKGeneratorService pkGeneratorService,
            ICompanyGroupWiseMaterialAttributeService groupAttrService,
            ISqlRepository sqlRepository,
            IUnitOfWork unitOfWork) :
            base(charaterRepository, unitOfWork)
        {
            _charaterRepository = charaterRepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _groupAttrService = groupAttrService;
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

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = $"SELECT c.* FROM [{DbSchema.HKP}].[{DbTable.MaterialAttribute}] AS c left outer join " +
                           $"(SELECT * FROM {DbSchema.HKP}.[{DbTable.CompanyGroupMaterialAttribute}] WHERE CompanyGroupId='{identity.CompanyGroupId}') cgc " +
                           $" ON c.Id=cgc.MaterialAttributeId  WHERE ISNULL(cgc.Id,'')<>'' AND c.Archive=0 AND (c.[CreationLevel]<>'Material' OR c.[CreationLevel] IS NULL)";
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

        public GridModel GetMaterialAttributeData(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = "SELECT MA.Id, " +
                                     "MA.Code, " +
                                     "MA.UserName, " +
                                     "MA.[Description]  " +
                             $"FROM [{DbSchema.HKP}].[{DbTable.MaterialAttribute}] AS MA LEFT OUTER JOIN " +
                             $"(SELECT * FROM {DbSchema.HKP}.[{DbTable.CompanyGroupMaterialAttribute}] WHERE CompanyGroupId='{identity.CompanyGroupId}') CGMA  " +
                             $"ON MA.Id=CGMA.MaterialAttributeId  WHERE ISNULL(CGMA.Id,'')<>'' AND MA.Archive=0 AND MA.Active=1 ";
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

        public override void Insert(MaterialAttribute entity)
        {
            var flag = false;
            try
            {
                Check(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                entity.Id = GetPK();
                //entity.IsFixedNoOfCharacter = entity.AttributeProperty == AttributePropertiesEnum.Alphanumeric.ToString() ? true : false;
                entity.NoOfCharacter = entity.AttributeProperty == AttributePropertiesEnum.Alphanumeric.ToString() ? entity.NoOfCharacter : 0;
                InsertGraph(entity);
                _groupAttrService.InsertGraph(new CompanyGroupMaterialAttribute { MaterialAttributeId = entity.Id, Active = entity.Active });
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
                     ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public override void Update(MaterialAttribute entity)
        {
            var flag = false;
            try
            {
                Check(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                //entity.IsFixedNoOfCharacter = entity.AttributeProperty == AttributePropertiesEnum.Alphanumeric.ToString() ? true : false;
                entity.NoOfCharacter = entity.AttributeProperty == AttributePropertiesEnum.Alphanumeric.ToString() ? entity.NoOfCharacter : 0;
                UpdateGraph(entity);
                _groupAttrService.Update(new CompanyGroupMaterialAttribute { MaterialAttributeId = entity.Id, Active = entity.Active, Archive = entity.Archive });
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
                     ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void InsertOrUpdate(MaterialAttributeViewModel viewModel)
        {
            try
            {
                if (Convert.ToBoolean(_charaterRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(
                                            SELECT A.Code FROM [HKP].[MaterialAttribute] AS A
                                            LEFT JOIN [MST].[MaterialMasterAttribute] AS B ON B.MaterialAttributeId=A.Id
                                            LEFT JOIN [HKP].[CompanyGroupMaterialAttribute] AS C ON C.MaterialAttributeId=A.Id
                                            WHERE C.CompanyGroupId='" + viewModel.CompanyGroupId + "' AND B.MaterialAttributeId='" + viewModel.MaterialAttributeId + "' AND A.Code='" + viewModel.Code + @"' AND A.CreationLevel='Material'
                                            AND B.MaterialMasterId='" + viewModel.MaterialMasterId + "' AND A.Id<>'" + viewModel.MaterialAttributeId + "')AA) SELECT 1 ELSE SELECT 0 RETURN").First()))
                    throw new CustomException("This Code :[" + viewModel.Code + "] already exist.");

                if (Convert.ToBoolean(_charaterRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(
                                            SELECT A.UserName FROM [HKP].[MaterialAttribute] AS A
                                            LEFT JOIN [MST].[MaterialMasterAttribute] AS B ON B.MaterialAttributeId=A.Id
                                            LEFT JOIN [HKP].[CompanyGroupMaterialAttribute] AS C ON C.MaterialAttributeId=A.Id
                                            WHERE C.CompanyGroupId='" + viewModel.CompanyGroupId + "' AND B.MaterialAttributeId='" + viewModel.MaterialAttributeId + "' AND A.UserName='" + viewModel.UserName + @"' AND A.CreationLevel='Material'
                                            AND B.MaterialMasterId='" + viewModel.MaterialMasterId + "' AND A.Id<>'" + viewModel.MaterialAttributeId + "')AA) SELECT 1 ELSE SELECT 0 RETURN").First()))
                    throw new CustomException("This UserName :[" + viewModel.UserName + "] already exist.");
                if (!string.IsNullOrEmpty(viewModel.TempMaterialAttributeId) &&viewModel.TempMaterialAttributeId.StartsWith("n-"))
                {
                    var entity = new MaterialAttribute
                    {
                        Id = GetPK(),
                        Sequence = viewModel.Sequence,
                        Code = viewModel.Code,
                        ShortName = viewModel.ShortName,
                        StandardName = viewModel.StandardName,
                        UserName = viewModel.UserName,
                        AttributeProperty = viewModel.AttributeProperty,
                        IsFixedNoOfCharacter = viewModel.IsFixedNoOfCharacter,
                        IsFreeField = viewModel.IsFreeField,
                        IsPreDefinedField = viewModel.IsPreDefinedField,
                        IsMandatory = viewModel.IsMandatory,
                        ValueAssignmentLevel = viewModel.ValueAssignmentLevel,
                        Remarks = viewModel.Remarks,
                        Description = viewModel.Description,
                        Active = viewModel.Active,
                        CreationLevel = "Material"
                    };
                    entity.NoOfCharacter = entity.AttributeProperty == AttributePropertiesEnum.Alphanumeric.ToString() ? viewModel.NoOfCharacter : 0;
                    viewModel.MaterialAttributeId = entity.Id;
                    base.InsertGraph(entity);
                    _groupAttrService.Insert(new CompanyGroupMaterialAttribute { MaterialAttributeId = entity.Id, Active = entity.Active, CompanyGroupId = viewModel.CompanyGroupId });

                }
                else
                {
                    var data = base.Find(viewModel.MaterialAttributeId);
                    data.Sequence = viewModel.Sequence;
                    data.Code = viewModel.Code;
                    data.ShortName = viewModel.ShortName;
                    data.StandardName = viewModel.StandardName;
                    data.UserName = viewModel.UserName;
                    data.AttributeProperty = viewModel.AttributeProperty;
                    data.IsFixedNoOfCharacter = viewModel.IsFixedNoOfCharacter;
                    data.IsFreeField = viewModel.IsFreeField;
                    data.IsPreDefinedField = viewModel.IsPreDefinedField;
                    data.IsMandatory = viewModel.IsMandatory;
                    data.ValueAssignmentLevel = viewModel.ValueAssignmentLevel;
                    data.Remarks = viewModel.Remarks;
                    data.Description = viewModel.Description;
                    data.Active = viewModel.Active;
                    AuditService.UpdatedLog(data);
                    base.UpdateGraph(data);
                }
            }
            catch (CustomException)
            {
                throw;
            }
        }

        private string GetPK()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(MaterialAttribute), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private void Check(MaterialAttribute entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, t => t.Id != entity.Id && t.Code == entity.Code && t.ValueAssignmentLevel == entity.ValueAssignmentLevel && !t.Archive);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, t => t.Id != entity.Id && t.UserName == entity.UserName && t.ValueAssignmentLevel == entity.ValueAssignmentLevel && !t.Archive);
        }

        public override void Archive(string key)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var entity = Find(key);
                _groupAttrService.DeleteGraph(key);
                base.DeleteGraph(entity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteGraph(string key)
        {
            try
            {
                var entity = Find(key);
                _groupAttrService.DeleteGraph(key);
                base.DeleteGraph(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public override IQueryFluent<MaterialAttribute> Query()
        {
            return base.Query(r => !r.Archive);
        }

        public IEnumerable<object> GetMaterialAttributeCbo(string groupId, string valueAssignment)
        {
            try
            {
                //valueAssignment = valueAssignment == "G" ? "AND mg.ValueAssignmentLevel='" + ValueAssignmentEnum.General + "'" : "";
                valueAssignment = "";

                var _sql = @"SELECT MG.Id AS Value, MG.UserName as Text, MG.IsFreeField, MG.IsPreDefinedField, MG.IsMandatory, MG.AttributeProperty
							, MG.IsFixedNoOfCharacter, MG.NoOfCharacter, MG.UserName AS MaterialAttributeName, MG.ValueAssignmentLevel
						FROM [HKP].[MaterialAttribute] AS MG
						LEFT JOIN [HKP].[CompanyGroupMaterialAttribute] AS CMG ON MG.Id = CMG.MaterialAttributeId
						WHERE CMG.CompanyGroupId='" + groupId + "' AND  MG.Archive=0 AND MG.Active=1 AND (MG.[CreationLevel]<>'Material' OR MG.[CreationLevel] IS NULL) " + valueAssignment + " ORDER BY MG.UserName";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false,
                    ModuleEnum.Material.ToString()));
            }
        }

    }
}