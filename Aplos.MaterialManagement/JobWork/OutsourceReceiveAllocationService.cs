using System;
using System.Collections.Generic;
using Library.Data.Sql;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data;
using Library.Service.Logs;
using Library.Service.Enums;
using System.Reflection;

namespace Library.MaterialManagement.JobWork
{

    public class OutsourceReceiveAllocationService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public OutsourceReceiveAllocationService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

		public IEnumerable<object> GetOutSourceReceiptDataForAllocation()
		{
			try
			{
				var sql = "";
				sql = @"SELECT DISTINCT
				 IR.GRNType
				, IRD.Id InventoryReceiveDetailId
				,'' POReqDetailsId
				,ISNULL(IRD.TransactionQty,0) TransactionQty1
				,ISNULL(IRD.TransactionQty,0) TransactionQty
	            ,ISNULL(AlreadyAllo.TransactionQty,0) AllocatedQty
				,Isnull(IRD.TransactionUoMId,'') TransactionUoMId
				,ISNULL(TUoM.UserName,'') TransactionUoM
				,ISNULL(IRD.BaseQty,0) BaseQty1
				,ISNULL(IRD.BaseQty,0) BaseQty
				,ISNULL(IRD.BaseUOMId,'') BaseUoMId
				,ISNULL(BUoM.UserName,'') BaseUoM
				--,Isnull(POBOQMAP.POBOQQty,0) POBOQQty
				--,ISNULL(PUoM.Id,'') POUoMId
				--,ISNULL(PUoM.UserName,'') POUoM
				--,ISNULL(Boq.SalesOrderId,'') SalesOrderId
				,ISNULL(IRD.OSTransformationPOId,'') POId
				,ISNULL(IRD.OSTransformationPODetailId,'') PODetailsId 

				,IM.MaterialMasterId
				,MM.UserName MaterialMasterName
				, IM.ArticleId
				, ART.StandardName ArticleName
				,IsAsset=CASE WHEN MM.IsAsset=0 then 'No' else 'Yes' END
				, IM.FirstCharacteristicsValueId
				, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
				, IM.SecondCharacteristicsValueId
				, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
				, IM.ThirdCharacteristicsValueId
				, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
				, BaseUOMFactor=CASE WHEN MaA.BaseUOMFactor IS null then 1 else MaA.BaseUOMFactor end
				FROM trn.InventoryReceive IR
				Left JOIN TRN.InventoryReceiveDetail IRD on IR.Id=IRD.InventoryReceiveId
				left join [dbo].OSPOBOQMAP POBOQMAP ON POBOQMAP.OSTransformationPODetailId =IRD.OSTransformationPODetailId
				left join BOQ Boq on Boq.Id=POBOQMAP.BOQDetailId
				LEFT JOIN SCS.UnitOfMeasurement TUoM ON TUoM.Id=IRD.TransactionUoMId
				LEFT JOIN SCS.UnitOfMeasurement BUoM ON BUoM.Id=IRD.BaseUOMId
				--LEFT JOIN SCS.UnitOfMeasurement PUoM ON PUoM.Id=POBOQMAP.POUoMId
				LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id=IRD.InventoryMaterialId
				left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
				LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
				LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
				LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
				LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
				LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
				LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
				LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
				LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
				left join [MST].[MaterialMasterAlternativeUOM] MaA ON MaA.MaterialMasterId=mm.Id
				left join(select InventoryReceiveDetailId ,Sum(TransactionQty) TransactionQty 
						  from trn.GRNPORequisitionAllocation 
						  group by InventoryReceiveDetailId
						  )AlreadyAllo ON AlreadyAllo.InventoryReceiveDetailId=IRD.Id
				WHERE IR.GRNType='GRNBYPO' 
				AND Boq.SalesOrderId IS NOT NULL 
				Order by IRD.Id ASC";// ,MMAU.BaseUOMFactor
				return _sqlRepository.GetDataCollection(sql);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}

		public IEnumerable<object> GetOutSourceReceiptDetailDataForAllocation(string inventoryReceiveDetailId)
		{
			try
			{
				var sql = "";
				sql = @"select Convert(bit, 'False') Active, '' Id
				, IR.GRNType
				, IRD.Id InventoryReceiveDetailId
				, ISNULL(POBOQMAP.Id,'') POBOQMapId
				,'' POReqDetailsId
				,ISNULL(IRD.TransactionQty,0) TransactionQty1
				,ISNULL(IRD.TransactionQty,0) TransactionQty
	            ,ISNULL(AlreadyAllo.TransactionQty,0) AllocatedQty
				,Isnull(IRD.TransactionUoMId,'') TransactionUoMId
				,ISNULL(TUoM.UserName,'') TransactionUoM
				,ISNULL(IRD.BaseQty,0) BaseQty1
				,ISNULL(IRD.BaseQty,0) BaseQty
				,ISNULL(IRD.BaseUOMId,'') BaseUoMId
				,ISNULL(BUoM.UserName,'') BaseUoM
				,Isnull(POBOQMAP.POBOQQty,0) POBOQQty
				,ISNULL(PUoM.Id,'') POUoMId
				,ISNULL(PUoM.UserName,'') POUoM
				,ISNULL(Boq.SalesOrderId,'') SalesOrderId
				,ISNULL(IRD.OSTransformationPOId,'') POId
				,ISNULL(IRD.OSTransformationPODetailId,'') PODetailsId 

				,IM.MaterialMasterId
				,MM.UserName MaterialMasterName
				, IM.ArticleId
				, ART.StandardName ArticleName
				,IsAsset=CASE WHEN MM.IsAsset=0 then 'No' else 'Yes' END
				--, IM.FirstCharacteristicsId
				--, FC.UserName AS FirstCharacteristics
				, IM.FirstCharacteristicsValueId
				, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
				--, IM.SecondCharacteristicsId
				--, SC.UserName AS SecondCharacteristics
				, IM.SecondCharacteristicsValueId
				, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
				--, IM.ThirdCharacteristicsId
				--, TC.UserName AS ThirdCharacteristics
				, IM.ThirdCharacteristicsValueId
				, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
				, BaseUOMFactor=CASE WHEN MaA.BaseUOMFactor IS null then 1 else MaA.BaseUOMFactor end
				FROM trn.InventoryReceive IR
				Left JOIN TRN.InventoryReceiveDetail IRD on IR.Id=IRD.InventoryReceiveId
				left join [dbo].OSPOBOQMAP SOBOQMAP ON SOBOQMAP.OSTransformationPODetailId =IRD.OSTransformationPODetailId
				LEFT JOIN TRN.POBOQMAP POBOQMAP ON POBOQMAP.BOQDetailId=SOBOQMAP.BOQDetailId
				left join BOQ Boq on Boq.Id=SOBOQMAP.BOQDetailId
				LEFT JOIN SCS.UnitOfMeasurement TUoM ON TUoM.Id=IRD.TransactionUoMId
				LEFT JOIN SCS.UnitOfMeasurement BUoM ON BUoM.Id=IRD.BaseUOMId
				LEFT JOIN SCS.UnitOfMeasurement PUoM ON PUoM.Id=POBOQMAP.POUoMId
				LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id=IRD.InventoryMaterialId
				left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
				LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
				LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
				LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
				LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
				LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
				LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
				LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
				LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
				left join [MST].[MaterialMasterAlternativeUOM] MaA ON MaA.MaterialMasterId=mm.Id
				left join(select InventoryReceiveDetailId ,Sum(TransactionQty) TransactionQty 
						  from trn.GRNPORequisitionAllocation 
						  group by InventoryReceiveDetailId
						  )AlreadyAllo ON AlreadyAllo.InventoryReceiveDetailId=IRD.Id
				WHERE IR.GRNType='GRNBYPO' and ISNULL(Boq.SalesOrderId,'') IS NOT NULL  
				AND IRD.Id='" + inventoryReceiveDetailId + @"' 
				Order by IRD.Id ASC";// ,MMAU.BaseUOMFactor
				return _sqlRepository.GetDataCollection(sql);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}

	}
}

