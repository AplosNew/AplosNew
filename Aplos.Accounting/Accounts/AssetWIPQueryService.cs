using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.FixedAssets;
using Library.Service.Enums;
using Library.Service.FixedAssets;
using Library.Service.Logs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;

namespace Library.Accounting.FixedAssets
{
    public class AssetWIPQueryService
    {
        private readonly ISqlRepository _sqlRepository;
        public AssetWIPQueryService(ISqlRepository sqlRepository )
        {
            _sqlRepository = sqlRepository;
           
        }

		public List<Dictionary<string, object>> GetFixedAssetRegisterElasticSearchDataList(string companyGroupId, string companyId, string plantId, string materialMasterId, string materialMasterArticleId, string fixedAssetMasterId, string vendorId, string isAsset, string machine)
		{
			var sql = @"select distinct MM.UserName MaterialMaster,MMA.StandardName Article,FA.UserName AssetMaster,P.UserName Party
                , FAR.MaterialMasterId,FAR.MaterialMasterArticleId,FAR.VendorId,FAR.FixedAssetMasterId

                 ,IsAsset =case when MM.IsAsset =1 then 'Yes' else  'No'  end
				 , Machine=case when MBP.BusinessProcessName ='MachineDefinition' Then 'Yes' else 'No' end 
				 , count(FAR.FixedAssetMasterId) FACount

				-- ,sum( ISNULL(FAR.FABaseAmount,0))FABaseAmount
				 --,sum( ISNULL(FAR.ADBaseAmount,0)) ADBaseAmount
				-- ,sum( ISNULL(FAR.FABaseAmount,0)- ISNULL(FAR.ADBaseAmount,0)) NetFixedAssetsAmount
				 -- ,sum( isnull(sar.SubAssetAmount,0))SubAssetAmount
				 -- ,TotalAssetsBaseAmount= sum( ISNULL(FAR.FABaseAmount,0) + (isnull(sar.SubAssetAmount,0) )) 
				 ,sum( ISNULL(FAR.FABaseAmount,0))FABaseAmount
				  ,sum( isnull(sar.SubAssetAmount,0))SubAssetAmount
				  ,sum(ISNULL(FAR.FABaseAmount,0) + isnull(sar.SubAssetAmount,0) ) TotalBaseAmount
				 ,sum( ISNULL(FAR.ADBaseAmount,0)) ADBaseAmount
				 ,sum( ISNULL(FAR.FABaseAmount,0) + isnull(sar.SubAssetAmount,0) - ISNULL(FAR.ADBaseAmount,0) ) NetFixedAssetsAmount
				  --,TotalAssetsBaseAmount= sum( ISNULL(FAR.FABaseAmount,0) + (isnull(sar.SubAssetAmount,0) )) 


		        from TRN.FixedAssetRegister FAR 
				JOIN MST.MaterialMaster MM ON MM.Id=FAR.MaterialMasterId
				JOIN MST.MaterialMasterArticle MMA ON MMA.Id=FAR.MaterialMasterArticleId
				JOIN MST.FixedAssetMaster FA ON FA.Id=FAR.FixedAssetMasterId
				LEFT JOIN HKP.Party P ON P.Id=FAR.VendorId

			    LEFT JOIN (SELECT MBP.MaterialMasterId,BP.BusinessProcessName FROM [MST].[MaterialMasterBusinessProcess] AS MBP
                LEFT JOIN [SCS].[BusinessProcess] AS BP ON MBP.BusinessProcessId = BP.Id
                WHERE BP.BusinessProcessName ='MachineDefinition') AS MBP ON MBP.MaterialMasterId=MM.Id


		        left join(select sum(Amount * CapitalizationRate) SubAssetAmount,FixedAssetRegisterId from  trn.SubFixedAssetRegister
				group by FixedAssetRegisterId
				) sar on sar.FixedAssetRegisterId=FAR.Id


		        WHERE FAR.CompanyGroupId='" + companyGroupId + "' AND FAR.CompanyId='" + companyId + "' AND FAR.PlantId='" + plantId + @"'  AND FAR.Status is null
				  --and FAR.MaterialMasterId in (" + materialMasterId + ") AND FAR.MaterialMasterArticleId in (" + materialMasterArticleId + ") AND FAR.FixedAssetMasterId in (" + fixedAssetMasterId + @")
					-- and FAR.VendorId in (" + vendorId + ") AND MM.IsAsset in (" + isAsset + ") AND MBP.BusinessProcessName in (" + machine + @")

               GROUP BY FAR.MaterialMasterId ,MM.UserName ,MMA.StandardName ,FA.UserName,P.UserName 
			   ,MM.IsAsset,MBP.BusinessProcessName,FAR.FixedAssetMasterId
			    ,FAR.MaterialMasterId,FAR.MaterialMasterArticleId,FAR.VendorId,FAR.FixedAssetMasterId";
			return _sqlRepository.GetDataCollection(sql);

		}


	}
}
