#region Using

using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.OrderManagements;
using Library.Service.Core;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion Using

namespace Library.Service.OrderManagements
{
    public class SampleOrderSubMaterialValueService : Service<SampleOrderSubMaterialValue>, ISampleOrderSubMaterialValueService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public SampleOrderSubMaterialValueService(
            IRepositoryAsync<SampleOrderSubMaterialValue> sampleOrderValueRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(sampleOrderValueRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public IEnumerable<object> Query(string materialGroupMasterId, string sampleOrderSubMaterialId)
        {
            try
            {
                string _sql = @"SELECT MAM.MaterialAttributeId AS MaterialAttributeId
                                      ,MA.UserName AS MaterialAttributeName
                                      ,MAM.IsFreeField
                                      ,MAM.IsPreDefinedField
                                      ,MAM.IsMandatory
                                      ,MMAV.Id
                                      ,MaterialAttributeValueId = CASE WHEN (ISNULL(MMAV.Id, '') = '' AND MAV.IsDefault = 1)
                                                                  THEN MAV.Id ELSE MMAV.MaterialAttributeValueId END
                                      ,MaterialAttributeValueFreeText =CASE WHEN (ISNULL(MMAV.Id, '') = '' AND MAV.IsDefault = 1)
	                                                                   THEN MAV.[Description] ELSE (ISNULL(MMAV.MaterialAttributeValueFreeText, '')
	                                                                   + ISNULL(MMAVe.[Description], '')) END
                                      ,'True' AS FlagDisable
                                FROM (SELECT * FROM MST.MaterialAttributeMaster WHERE MaterialGroupMasterId = '" + materialGroupMasterId + @"') AS MAM
                                LEFT JOIN HKP.MaterialAttribute AS MA ON MAM.MaterialAttributeId = MA.Id
                                LEFT OUTER JOIN (SELECT * FROM TRN.SampleOrderSubMaterialValue WHERE SampleOrderSubMaterialId = '" + sampleOrderSubMaterialId + @"') AS MMAV ON MMAV.MaterialAttributeId = MA.Id
                                LEFT JOIN (SELECT * FROM HKP.MaterialAttributeValue WHERE Active = 1
                                     AND IsDefault = 1) AS MAV ON MAM.MaterialAttributeId = MAV.MaterialAttributeId
                                LEFT JOIN (SELECT * FROM HKP.MaterialAttributeValue WHERE Active = 1
	                                 AND IsDefault = 1) AS MMAVe ON MMAVe.MaterialAttributeId = MMAV.MaterialAttributeId
	                                 AND MMAV.MaterialAttributeValueId = MMAVe.Id ORDER BY MAM.MaterialAttributeId, MAM.[Sequence]";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetAttributeByMgm(string materialGroupMasterId, string subMaterialId)
        {
            try
            {
                string _sql = @"SELECT MMAV.Id
                                , MMAV.SampleOrderId
                                , MMAV.SampleOrderSubMaterialId
                                , MAM.MaterialAttributeId, MA.UserName AS MaterialAttributeName
                                , MA.IsFreeField, MA.IsPreDefinedField, MA.IsMandatory, MA.ValueAssignmentLevel
		                        , MAV.Id AS MaterialAttributeValueId
		                        , MaterialAttributeValueFreeText =CASE WHEN MMAV.MaterialAttributeValueId<>'' THEN MAV.UserName
											                           WHEN MAV.IsDefault=1 THEN MAV.UserName ELSE MMAV.MaterialAttributeValueFreeText END
                        FROM [MST].[MaterialAttributeMaster] AS MAM
                        LEFT JOIN [HKP].[MaterialAttribute] AS MA ON MAM.MaterialAttributeId = MA.Id
                        LEFT JOIN (SELECT * FROM TRN.SampleOrderSubMaterialValue WHERE SampleOrderSubMaterialId='" + subMaterialId + @"') AS MMAV ON MMAV.MaterialAttributeId = MA.Id
                        LEFT JOIN (SELECT * FROM HKP.MaterialAttributeValue WHERE Active=1 AND IsDefault=1) AS MAV ON MAV.MaterialAttributeId=MAM.MaterialAttributeId AND MAV.Id=MMAV.MaterialAttributeValueId
                        WHERE MAM.MaterialGroupMasterId='"+ materialGroupMasterId + "' ORDER BY MAM.[Sequence]";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertOrUpdateGraph(string masterId, SampleOrderSubMaterial entity)
        {
            if (entity.MaterialAttributeValues != null)
            {
                foreach (var item in entity.MaterialAttributeValues)
                {
                    item.MaterialAttribute = null;
                    if (item.Id == 0)//Insert
                    {
                        if (string.IsNullOrEmpty(item.MaterialAttributeValueId) && string.IsNullOrEmpty(item.MaterialAttributeValueFreeText))
                        {
                            //Do Nothing.
                        }
                        else
                        {
                            SetMaterialAttributeValueId(item);
                            item.SampleOrderId = masterId;
                            item.SampleOrderSubMaterialId = entity.Id;
                            InsertGraph(item);
                        }
                    }
                    else
                    {
                        //Edit
                        if (string.IsNullOrEmpty(item.MaterialAttributeValueId) && string.IsNullOrEmpty(item.MaterialAttributeValueFreeText))
                        {
                            base.DeleteGraph(item);
                        }
                        else
                        {
                            SetMaterialAttributeValueId(item);
                            UpdateGraph(item);
                        }
                    }
                }
            }
        }

        public void DeleteGraph(SampleOrderSubMaterial subMaterials)
        {
            if (subMaterials.MaterialAttributeValues != null)
            {
                foreach (var item in subMaterials.MaterialAttributeValues.ToList())
                {
                    base.DeleteGraph(item);
                }
            }
        }

        private static void SetMaterialAttributeValueId(SampleOrderSubMaterialValue item)
        {
            if (item.MaterialAttributeValueId != null)//
            {
                item.MaterialAttributeValueFreeText = null;
            }
            //else
            //{
            //    if (item.MaterialAttributeValueFreeText == null)
            //    {
            //        throw new CustomException("Free Text can not be null");
            //    }
            //}
        }
    }
}