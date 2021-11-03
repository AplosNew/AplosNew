#region using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.FixedAssets;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

#endregion using

namespace Library.Service.FixedAssets
{
    public class FixedAssetRegisterCharacteristicsValueService : Service<FixedAssetRegisterCharacteristicsValue>, IFixedAssetRegisterCharacteristicsValueService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;

        public FixedAssetRegisterCharacteristicsValueService(
            IRepositoryAsync<FixedAssetRegisterCharacteristicsValue> fixedAssetRegisterCharacteristicsValueRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(fixedAssetRegisterCharacteristicsValueRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public GridModel GetCharacteristicsValueList(GridParameter parameters, string assignment, string mMasterId, string charateristicsId)
        {
            try
            {
                if (ValueAssignmentEnum.General.ToString() == assignment)
                {
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    parameters.CmdText = @"SELECT Id AS CharacteristicsValueId, NULL AS MaterialMasterCharacteristicsValueId, [Sequence], Code, ShortName, StandardName, UserName
                                           FROM HKP.CharacteristicsValue WHERE CompanyGroupId='" + identity.CompanyGroupId + @"' AND CharacteristicsId = '" + charateristicsId + "'";
                }
                else
                {
                    parameters.CmdText = @"SELECT MCV.Id AS MaterialMasterCharacteristicsValueId, NULL AS CharacteristicsValueId, MCV.[Sequence], MCV.Code, MCV.ShortName, MCV.StandardName, MCV.UserName
                                      FROM MST.MaterialMasterCharacteristicsValue MCV
									  LEFT JOIN MST.MaterialMasterCharacteristics MC ON MC.Id= MCV.MaterialMasterCharacteristicsId
									  WHERE MC.CharacteristicsId='" + charateristicsId + @"'
                                      AND MCV.MaterialMasterId = '" + mMasterId + "'";
                }
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public void InsertOrUpdateGraph(FixedAssetRegister fixedAssetRegister, IEnumerable<FixedAssetRegisterCharacteristicsValue> entity)
        {
            if (entity != null)
            {
                foreach (var item in entity)
                {
                    var temp = item.Copy<FixedAssetRegisterCharacteristicsValue>();
                    if (temp.Id == 0)//Insert
                    {
                        if (string.IsNullOrEmpty(temp.CharacteristicsValueId) &&
                            string.IsNullOrEmpty(temp.MaterialMasterCharacteristicsValueId) && string.IsNullOrEmpty(temp.CharacteristicsValueFreeText))
                        {
                            //Do Nothing.
                        }
                        else
                        {
                            SetMaterialAttributeValueId(temp);
                            temp.FixedAssetRegisterId = fixedAssetRegister.Id;
                            temp.MaterialMasterId = fixedAssetRegister.MaterialMasterId;
                            AuditService.AddedLog(temp);
                            Insert(temp);
                        }
                    }
                    else
                    {
                        //Edit
                        if (string.IsNullOrEmpty(temp.CharacteristicsId) && string.IsNullOrEmpty(temp.MaterialMasterCharacteristicsValueId)
                            && string.IsNullOrEmpty(temp.CharacteristicsValueFreeText))
                        {
                            Delete(temp);
                        }
                        else
                        {
                            SetMaterialAttributeValueId(temp);
                            AuditService.UpdatedLog(temp);
                            Update(temp);
                        }
                    }
                }
            }
        }

        private static void SetMaterialAttributeValueId(FixedAssetRegisterCharacteristicsValue item)
        {
            if (item.CharacteristicsValueId != null || item.MaterialMasterCharacteristicsValueId != null)//
            {
                item.CharacteristicsValueFreeText = null;
            }
            else
            {
                if (item.CharacteristicsValueFreeText == null)
                {
                    throw new CustomException("Free Text can not be null");
                }
            }
        }

        public IEnumerable<object> GetMaterialMasterCharacteristicsList(string materialMasterId, string registerId)
        {
            try
            {
                var _sql = @"SELECT  FCH.Id,FCH.FixedAssetRegisterId,FCH.MaterialMasterCharacteristicsValueId,FCH.CharacteristicsValueId
           ,CharacteristicsValueFreeText = CASE WHEN FCH.MaterialMasterCharacteristicsValueId<>'' THEN C.UserName
												WHEN FCH.CharacteristicsValueId<>'' THEN CH2.UserName
												WHEN B.ValueAssignmentLevel='" + ValueAssignmentEnum.Specific + @"' AND  C.IsDefault=1 THEN C.UserName
												WHEN B.ValueAssignmentLevel='" + ValueAssignmentEnum.General + @"'AND  CH.IsDefault=1 THEN CH.UserName
												ELSE NULL END
             , A.CharacteristicsId
             , A.Id AS MaterialMasterCharacteristicsId, A.MaterialMasterId, A.[Sequence], A.IsFreeField, A.IsPreDefinedField, A.IsMandatory, A.Active
             , B.UserName AS CharacteristicsName, B.ValueAssignmentLevel
             , C.UserName, CH.UserName AS CHV
            FROM MST.MaterialMasterCharacteristics AS A
            LEFT JOIN HKP.Characteristics AS B ON A.CharacteristicsId=B.Id
            LEFT JOIN (SELECT * FROM TRN.FixedAssetRegisterCharacteristicsValue WHERE FixedAssetRegisterId = '" + registerId + @"') AS FCH ON FCH.MaterialMasterCharacteristicsId=A.Id
            LEFT JOIN (SELECT * FROM HKP.CharacteristicsValue WHERE Active = 1) AS C ON C.CharacteristicsId=A.Id AND C.MaterialMasterId=A.MaterialMasterId AND FCH.MaterialMasterCharacteristicsValueId=C.Id
            LEFT JOIN (SELECT * FROM HKP.CharacteristicsValue WHERE Active = 1 AND IsDefault = 1)
	            AS CH ON A.CharacteristicsId = CH.CharacteristicsId AND CH.CharacteristicsId=B.Id
            LEFT JOIN (SELECT * FROM HKP.CharacteristicsValue WHERE Active = 1)
	            AS CH2 ON FCH.CharacteristicsValueId=CH2.Id AND CH2.CharacteristicsId=FCH.CharacteristicsId
            WHERE A.MaterialMasterId='" + materialMasterId + "' AND A.Active=1 AND A.Archive=0";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}