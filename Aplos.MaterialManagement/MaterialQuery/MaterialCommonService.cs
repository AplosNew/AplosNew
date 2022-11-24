using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Inventory;
using Library.Model.Parties;
using Library.Model.Taxations;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.ViewModel.Materials;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Aplos.MaterialManagement.MaterialQuery
{
    public class MaterialCommonService
    {
        private readonly ISqlRepository _sqlRepository;
        public MaterialCommonService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }

        public IEnumerable<POMaterial> GetInventoryMaterialByUpToSku(InventoryMaterialViewModel entity)
        {
            string materialmaster = "";
            string articleId = "";
            string firstCharacteristicsId = "";
            string firstCharacteristicsValueId = "";
            //string secondCharacteristicsId = "";
            //string secondCharacteristicsValueId = "";
            //string thirdCharacteristicsId = entity.ThirdCharacteristicsId;
            //string ThirdCharacteristicsValueId = entity.ThirdCharacteristicsValueId;

            if (string.IsNullOrEmpty(articleId)) { articleId = "AND  ArticleId=NULL"; } else { articleId = "AND  ArticleId = '" + entity.ArticleId + @"'"; }
            if (string.IsNullOrEmpty(firstCharacteristicsId)) { firstCharacteristicsId = "AND  FirstCharacteristicsId=NULL"; } else { firstCharacteristicsId = "AND  FirstCharacteristicsId = '" + entity.ArticleId + @"'"; }
            if (string.IsNullOrEmpty(firstCharacteristicsValueId)) { firstCharacteristicsValueId = "AND  FirstCharacteristicsValueId=NULL"; } else { firstCharacteristicsValueId = "AND  FirstCharacteristicsValueId = '" + entity.FirstCharacteristicsValueId + @"'"; }

            var sql = @"SELECT Top(1) * FROM TRN.InventoryMaterial WHERE MaterialMasterId='" + entity.MaterialMasterId + @"' " + articleId + " " + firstCharacteristicsId + " " + firstCharacteristicsValueId + @"
                                AND  SecondCharacteristicsId =ISNULL( '" + entity.SecondCharacteristicsId + @"',NULL) AND  SecondCharacteristicsValueId =ISNULL( '" + entity.SecondCharacteristicsValueId + @"',NULL)
                                AND  ThirdCharacteristicsId =ISNULL( '" + entity.ThirdCharacteristicsId + @"',NULL) AND  ThirdCharacteristicsValueId =ISNULL( '" + entity.ThirdCharacteristicsValueId + @"',NULL)
                                AND  CompanyId = '" + entity.CompanyId + @"' AND  PlantId ='" + entity.PlantId + @"'";
            return _sqlRepository.GetModelCollection<POMaterial>(sql);
        }

        public IEnumerable<object> GetALLUOMCbo()
        {
            var sql = @"SELECT u.Id Value,u.UserName [Text],mu.Id MaterialMasterId,1 BaseUOMFactor FROM scs.[UnitOfMeasurement] u  
                                    LEFT OUTER JOIN [MST].[MaterialMaster]  
                                    mu ON u.Id=mu.BaseUOMId 
                                    WHERE mu.Id IS NOT NULL  
									UNION ALL
									SELECT u.Id Value,u.UserName [Text],mu.MaterialMasterId,mu.BaseUOMFactor FROM scs.[UnitOfMeasurement] u  
                                    LEFT OUTER JOIN [MST].[MaterialMasterAlternativeUOM]  
                                    mu ON u.Id=mu.AlternativeUOMId 
                                    WHERE mu.MaterialMasterId IS NOT NULL  ";
             return _sqlRepository.GetDataCollection(sql);
        }
        public Dictionary<string, object> GetCompanyParty(string companyId, string plantId, string partyId, string partyType)
        {
            var sql = @"select TOP(1) * from hkp.CompanyParty where CompanyId='" + companyId + "'  and PartyId='" + partyId + "' and PartyType='" + partyType + @"'";
            var partyPlantTemp = _sqlRepository.GetData(sql);

            if (null == sql || partyPlantTemp.Count == 0)
                throw new CustomException("Plant party mapping not found.");
            return partyPlantTemp;
        }

      
    }
}
