using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
#region Using

using Library.Service.Enums;
using Library.Service.Logs;
using Library.Model.Products;
using Library.Core;
using Library.Model.Accounts;
using Library.Service.Core;
using Syncfusion.DocIO.DLS;
using Library.Service.Helpers;
using System.IO;
using Syncfusion.DocIO;
using System.Collections.Specialized;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using System.Text.RegularExpressions;
using Zen.Barcode;

#endregion Using

namespace Library.MaterialManagement.InventoryManagements
{
	public class GatePassAndGateEntryService
	{
		private readonly SqlRepository _sqlRepository;

		#region Constructor
		public GatePassAndGateEntryService()
		{
			_sqlRepository = new SqlRepository();
		}
		#endregion Constructor


		//public IEnumerable<object> GetShortageRejectionValue(string InventoryReceiveId)
		//{
		//	var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
		//	try
		//	{
		//		var sql = "";
		//		sql = @"Select IRD.Id InventoryReceiveDetailId
		//                      , MGM.UserName AS MaterialGroupMasterName
		//                      , IM.MaterialMasterId, MM.UserName
		//                      , IM.ArticleId, ART.StandardName
		//                      , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
		//                      , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
		//                      , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
		//                      , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
		//                      , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
		//                      , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
		//                      ,IRD.MaterialTranRate TransactionRate,IRD.ShortageQty,IRD.ShortageRatePercent ShortageRate,IRD.RejectionQty,IRD.ShortageValue,IRD.RejectRatePercent RejectionRate,IRD.RejectClamPercent , IRD.RejectValue RejectionValue,IRD.RejectClamPercent RejectionClamRate
		//                      FROM trn.InventoryReceiveDetail IRD
		//                      LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id=IRD.InventoryMaterialId
		//                      LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
		//                      LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
		//                      LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
		//                      LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
		//                      LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
		//                      LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
		//                      LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
		//                      LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
		//                      LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
		//                      where IRD.InventoryReceiveId='" + InventoryReceiveId + "'";
		//		return _sqlRepository.GetDataCollection(sql);
		//	}
		//	catch (Exception ex)
		//	{
		//		throw new CustomException(ex.Message, ex,
		//			Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
		//			ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
		//	}
		//}

		//public void UpdateShortageRejectionValue(string MasterId, List<Dictionary<string, object>> UserSendData)
		//{
		//	try
		//	{
		//		string sql = "select * from trn.InventoryReceiveDetail where InventoryReceiveId='" + MasterId + "'";
		//		ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
		//		con.OpenDataSetThroughAdapter(sql, out DataSet dsDetail, false, "1");

		//		for (int i = 0; i < UserSendData.Count; i++)
		//		{
		//			dsDetail.Tables[0].DefaultView.RowFilter = "Id='" + UserSendData[i]["InventoryReceiveDetailId"].ToString() + "'";
		//			if (dsDetail.Tables[0].DefaultView.Count == 0)
		//			{
		//				//genId.GenID("TNA MASTER", out TNAMasterSystemID);
		//				//TNAMasterSystemID = "TM" + TNAMasterSystemID;
		//				//DataRow dr = dsMaster.Tables[0].NewRow();
		//				//dr["Id"] = TNAMasterSystemID;
		//				//dr[columnname] = TransactionId;
		//				//dr["TNAAppliedOn"] = ScheduleFor.ToString();
		//				//dr["AddedBy"] = "Scheduler";
		//				//dr["AddedDate"] = System.DateTime.Now.ToString();
		//				//dr["AddedFromIP"] = "";
		//				//dr["UpdatedBy"] = "Scheduler";
		//				//dr["UpdatedDate"] = System.DateTime.Now.ToString();
		//				//dr["UpdatedFromIP"] = "";

		//				//dsMaster.Tables[0].Rows.Add(dr);
		//			}
		//			else
		//			{
		//				DataRow dr = dsDetail.Tables[0].DefaultView[0].Row;
		//				dr.BeginEdit();
		//				dr["ShortageRatePercent"] = UserSendData[i]["ShortageRate"];
		//				dr["ShortageValue"] = UserSendData[i]["ShortageValue"];
		//				dr["RejectRatePercent"] = UserSendData[i]["RejectionRate"];
		//				dr["RejectValue"] = UserSendData[i]["RejectionValue"];
		//				dr["RejectClamPercent"] = UserSendData[i]["RejectionClamRate"];
		//				dr.EndEdit();
		//			}
		//		}

		//		clsStaticInfo info = new clsStaticInfo();
		//		info.SaveDataSets(dsDetail);
		//	}
		//	catch (Exception ex)
		//	{
		//		throw ex;
		//	}
		//}

		//public void UpdateShortageRejectionValueMap(string MasterId, List<Dictionary<string, object>> UserSendData)
		//{
		//	try
		//	{
		//		string sql = "select * from trn.InventoryReceiveDetail where InventoryReceiveId='" + MasterId + "'";
		//		ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
		//		con.OpenDataSetThroughAdapter(sql, out DataSet dsDetail, false, "1");

		//		for (int i = 0; i < UserSendData.Count; i++)
		//		{
		//			dsDetail.Tables[0].DefaultView.RowFilter = "Id='" + UserSendData[i]["InventoryReceiveDetailId"].ToString() + "'";
		//			if (dsDetail.Tables[0].DefaultView.Count == 0)
		//			{
		//				//genId.GenID("TNA MASTER", out TNAMasterSystemID);
		//				//TNAMasterSystemID = "TM" + TNAMasterSystemID;
		//				//DataRow dr = dsMaster.Tables[0].NewRow();
		//				//dr["Id"] = TNAMasterSystemID;
		//				//dr[columnname] = TransactionId;
		//				//dr["TNAAppliedOn"] = ScheduleFor.ToString();
		//				//dr["AddedBy"] = "Scheduler";
		//				//dr["AddedDate"] = System.DateTime.Now.ToString();
		//				//dr["AddedFromIP"] = "";
		//				//dr["UpdatedBy"] = "Scheduler";
		//				//dr["UpdatedDate"] = System.DateTime.Now.ToString();
		//				//dr["UpdatedFromIP"] = "";

		//				//dsMaster.Tables[0].Rows.Add(dr);
		//			}
		//			else
		//			{
		//				DataRow dr = dsDetail.Tables[0].DefaultView[0].Row;
		//				dr.BeginEdit();
		//				dr["ShortageRatePercent"] = UserSendData[i]["ShortageRate"];
		//				dr["ShortageValue"] = UserSendData[i]["ShortageValue"];
		//				dr["RejectRatePercent"] = UserSendData[i]["RejectionRate"];
		//				dr["RejectValue"] = UserSendData[i]["RejectionValue"];
		//				dr["RejectClamPercent"] = UserSendData[i]["RejectionClamRate"];
		//				dr.EndEdit();

		//			}
		//		}

		//		clsStaticInfo info = new clsStaticInfo();
		//		info.SaveDataSets(dsDetail);
		//	}
		//	catch (Exception ex)
		//	{
		//		throw ex;
		//	}
		//	try
		//	{
		//		//string sql = "select * from trn.InventoryReceiveDetail where InventoryReceiveId='" + MasterId + "'";
		//		string sql = "select * from TRN.GRNRejectionDetails where GRNDeailsId in(select id from trn.InventoryReceiveDetail where InventoryReceiveId = '" + MasterId + "')";

		//		ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
		//		con.OpenDataSetThroughAdapter(sql, out DataSet dsDetailREjectionMap, false, "1");

		//		for (int i = 0; i < UserSendData.Count; i++)
		//		{
		//			dsDetailREjectionMap.Tables[0].DefaultView.RowFilter = "GRNDeailsId='" + UserSendData[i]["InventoryReceiveDetailId"].ToString() + "'";
		//			if (dsDetailREjectionMap.Tables[0].DefaultView.Count == 0)
		//			{
		//				//genId.GenID("TNA MASTER", out TNAMasterSystemID);
		//				//TNAMasterSystemID = "TM" + TNAMasterSystemID;
		//				//DataRow dr = dsMaster.Tables[0].NewRow();
		//				//dr["Id"] = TNAMasterSystemID;
		//				//dr[columnname] = TransactionId;
		//				//dr["TNAAppliedOn"] = ScheduleFor.ToString();
		//				//dr["AddedBy"] = "Scheduler";
		//				//dr["AddedDate"] = System.DateTime.Now.ToString();
		//				//dr["AddedFromIP"] = "";
		//				//dr["UpdatedBy"] = "Scheduler";
		//				//dr["UpdatedDate"] = System.DateTime.Now.ToString();
		//				//dr["UpdatedFromIP"] = "";

		//				//dsMaster.Tables[0].Rows.Add(dr);
		//			}
		//			else
		//			{
		//				DataRow dr = dsDetailREjectionMap.Tables[0].DefaultView[0].Row;
		//				dr.BeginEdit();
		//				dr["RejectionQty"] = UserSendData[i]["RejectionQty"];
		//				dr["RejectionRate"] = UserSendData[i]["RejectionRate"];
		//				dr["RejeactionValue"] = UserSendData[i]["RejectionValue"];
		//				dr.EndEdit();

		//			}
		//		}

		//		clsStaticInfo info = new clsStaticInfo();
		//		info.SaveDataSets(dsDetailREjectionMap);
		//	}
		//	catch (Exception ex)
		//	{
		//		throw ex;
		//	}
		//}
		//public bool GetDocRef(string UserDocRefNo,string PartyId,string DocDate,string Id)  
		//{

		//	string sql = "Select * From trn.InventoryReceive where DocRefNo='" + UserDocRefNo + "' AND PartyId = '" + PartyId + "' and Id<>'"+ Id + "'";
		//	ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
		//	con.OpenDataSetThroughAdapter(sql, out DataSet dsDetail, false, "1");
		//	//dsDetail.Tables[0].DefaultView.RowFilter = "DocRefNo='" + UserDocRefNo + "' AND PartyId = '" + PartyId + "'";// AND DocDate = '" + DocDate + "'";
		//	if (dsDetail.Tables[0].Rows.Count > 0)
		//	{
		//		return true;

		//	}
		//	else return false;
		//}

		public IEnumerable<object> GetPurchaseReturn() 
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";
				sql = @"Select * from(
				SELECT PR.Id ComId, REPLACE(CONVERT(CHAR(11), PR.POReturnDate, 106), ' ', '-') AS  CreatedDate, 'PurchaseReturn' GatePassFor,p.UserName VendorNameOrCuetomerName 
						,PR.PlantId AS PlantId
						FROM Trn.PurchaseReturn PR
						LEFT JOIN hkp.Party P On P.Id=PR.PartyId
                        Where PR.Id not in (Select PurchaseReturnId from trn.InOutGatePassMaster where PurchaseReturnId is not null)
                        --And PR.CheckedByStatus='Checked' And PR.ApprovedByStatus='Approved'
						UNION All
						SELECT InvSales.Id ComId,REPLACE(CONVERT(CHAR(11), InvSales.SalesDate, 106), ' ', '-') AS  CreatedDate,'InventorySales' GatePassFor,p.UserName VendorNameOrCuetomerName 
						,InvSales.PlantId AS PlantId
						from trn.InventorySales InvSales
						LEFT JOIN hkp.Party P On P.Id=InvSales.CustomerId
						UNION All
						SELECT Id ComId,REPLACE(CONVERT(CHAR(11), ScrapDate, 106), ' ', '-') AS  CreatedDate,'InventoryScrap' GatePassFor,'N/A' VendorNameOrCuetomerName 
						,PlantId AS PlantId
						from trn.InventoryScrap
						UNION All
						SELECT Distinct IR.Id ComId,REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106), ' ', '-') AS  CreatedDate,'InventoryTransfer' GatePassFor ,p.UserName VendorNameOrCuetomerName  
						,IR.PlantId AS PlantId
						FROM Trn.InventoryReceive IR
						LEFT JOIN Trn.InventoryReceiveDetail IRD ON IR.Id=IRD.InventoryReceiveId
						LEFT JOIN hkp.Party P On P.Id=IR.PartyId
						WHERE IRD.TransferedFromGrnId is not null

                        Union all
						SELECT FARD.Id ComId, REPLACE(CONVERT(CHAR(11), FARD.DocDate, 106), ' ', '-') AS  CreatedDate
						,'FixedAssetSales' GatePassFor,p.UserName VendorNameOrCuetomerName 
						,null PlantId
						FROM Trn.FixedAssetRegisterDisposed FARD
						LEFT JOIN hkp.Party P On P.Id=FARD.PartyId
                        Where FARD.Status='Sales' and 
						FARD.Id not in (Select FixedAssetRegisterDisposedId from trn.InOutGatePassMaster where FixedAssetRegisterDisposedId is not null)

                        Union all
						SELECT distinct FARD.Id ComId, REPLACE(CONVERT(CHAR(11), FARD.DocDate, 106), ' ', '-') AS  CreatedDate
						,'FixedAssetScrap' GatePassFor,p.UserName VendorNameOrCuetomerName 
						,null PlantId
						FROM Trn.FixedAssetRegisterDisposed FARD
						LEFT JOIN hkp.Party P On P.Id=FARD.PartyId
                        Where FARD.Status='Scrap' and 
						FARD.Id not in (Select FixedAssetScrapId from trn.InOutGatePassMaster where FixedAssetScrapId is not null)

						


				)x
                order by x.CreatedDate desc
				--where x.PlantId='" + identity.PlantId+ "'";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}
		public IEnumerable<object> GetPurchaseReturnMaterialDetails(string Id) 
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";
				sql = @"SELECT 
						ROW_NUMBER() OVER(ORDER BY ART.StandardName ASC) AS RowId
						,MT.UserName MaterialType
						,MGM.UserName AS MaterialGroupMasterName
						,IM.MaterialMasterId
						,MM.UserName MaterialMasterName
						, ART.StandardName ArticleName
						,IsAsset=CASE WHEN MM.IsAsset=0 then 'No' else 'Yes' END
						, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
						, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
						, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
						,TUoM.UserName AS UOM
						,IRD.TransactionQty
						,Round(IRD.TotalMaterialBooksCurrencyAmount,2) TotalMaterialBooksCurrencyAmount
						FROM TRN.InventoryMaterial AS IM
						left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
						left jOIN [TRN].[PurchaseReturnDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
						left jOIN [TRN].[PurchaseReturn] AS IR ON IR.Id=IRD.InventoryReceiveId
						left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id				
						left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id				
						LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId  
						LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
						LEFT JOIN hkp.MaterialStorage AS MS ON MS.Id=IR.MaterialStorageId
						LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.CheckedBy
						WHERE PurchaseReturnId='" + Id+"'";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}

        #region FA register
        public List<Dictionary<string, object>> GetFixedAssetRegisterElasticSearchDataList(string companyGroupId, string companyId, string plantId, string fixedAssetRegisterDisposeId)
        {
            var sql = @"select distinct FAR.Id AssetNo,FAR.SerialNo,MM.UserName MaterialMaster,MMA.StandardName Article,FA.UserName AssetMaster,P.UserName Party
                , FAR.MaterialMasterId,FAR.MaterialMasterArticleId,FAR.VendorId,FAR.FixedAssetMasterId

                 ,IsAsset =case when MM.IsAsset =1 then 'Yes' else  'No'  end
				 , Machine=case when MBP.BusinessProcessName ='MachineDefinition' Then 'Yes' else 'No' end 
				,FAR.Status,format( frd.DocDate,'dd-MMM-yyyy')DocDate,v.VoucherNo,frd.Id DisposalNo
				,CASE WHEN frd.IsPark=0 THEN 'Posted' ELSE 'Non Posted' END PostingStatus
				 , count(FAR.FixedAssetMasterId) FACount
				 ,sum( ISNULL(FAR.FABaseAmount,0))FABaseAmount
				  ,sum( isnull(sar.SubAssetAmount,0))SubAssetAmount
				  ,sum(ISNULL(FAR.FABaseAmount,0) + isnull(sar.SubAssetAmount,0) ) TotalBaseAmount
				 ,sum( ISNULL(FAR.ADBaseAmount,0)) ADBaseAmount
				 ,sum( ISNULL(FAR.FABaseAmount,0) + isnull(sar.SubAssetAmount,0) - ISNULL(FAR.ADBaseAmount,0) ) NetFixedAssetsAmount
				 ,Customer.UserName CustomerName,CU.Code Currency,CAST(frd.ToCurrencyRate AS decimal(18,4))ToCurrencyRate,sum(rdd.NegotiationValue)NegotiationValue
				 ,sum(rdd.BaseNagotiationValue)BaseNagotiationValue
				 ,( sum(rdd.BaseNagotiationValue)- sum( ISNULL(FAR.FABaseAmount,0) + isnull(sar.SubAssetAmount,0) - ISNULL(FAR.ADBaseAmount,0) ))LossOrGain
				 ,GP.Id GatePassNo, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
				 ,ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue
				 ,PC.Code PurchaseCurrency,isnull( FAR.Price,0 )PurchasePrice
		        from TRN.FixedAssetRegister FAR 
				JOIN MST.MaterialMaster MM ON MM.Id=FAR.MaterialMasterId
				JOIN MST.MaterialMasterArticle MMA ON MMA.Id=FAR.MaterialMasterArticleId
				JOIN MST.FixedAssetMaster FA ON FA.Id=FAR.FixedAssetMasterId
				LEFT JOIN HKP.Party P ON P.Id=FAR.VendorId
				LEFT JOIN TRN.FixedAssetRegisterDisposedDetail rdd ON rdd.FixedAssetRegisterId=FAR.Id
				LEFT JOIN TRN.FixedAssetRegisterDisposed frd ON rdd.FixedAssetRegisterDisposedId=frd.Id
				LEFT JOIN TRN.Voucher V ON V.Id =frd.DisposedVoucherId
				LEFT JOIN HKP.Party Customer ON Customer.Id = frd.PartyId
                LEFT JOIN SCS.Currency CU ON CU.Id =frd.CurrencyId
				LEFT JOIN [TRN].[InOutGatePassMaster] GP ON GP.FixedAssetRegisterDisposedId =frd.Id
				LEFT JOIN HKP.CharacteristicsValue AS FCV ON MM.Id=FCV.MaterialMasterId
				LEFT JOIN HKP.CharacteristicsValue AS SCV ON MM.Id=SCV.MaterialMasterId
				LEFT JOIN HKP.CharacteristicsValue AS TCV ON MM.Id=TCV.MaterialMasterId
				left join scs.Currency PC on PC.Id= FAR.CurrencyId
	
               LEFT JOIN (SELECT MBP.MaterialMasterId,BP.BusinessProcessName FROM [MST].[MaterialMasterBusinessProcess] AS MBP
                LEFT JOIN [SCS].[BusinessProcess] AS BP ON MBP.BusinessProcessId = BP.Id
                WHERE BP.BusinessProcessName ='MachineDefinition') AS MBP ON MBP.MaterialMasterId=MM.Id


		        left join(select sum(Amount * CapitalizationRate) SubAssetAmount,FixedAssetRegisterId from  trn.SubFixedAssetRegister
				group by FixedAssetRegisterId
				) sar on sar.FixedAssetRegisterId=FAR.Id
                --WHERE FR.CompanyId='" + companyId + @"' and FR.Archive=0 and FR.IsAUC=0
               -- AND FR.Id NOT IN(' ')

                     WHERE FAR.CompanyGroupId='" + companyGroupId + "'and FAR.CompanyId='" + companyId + "' AND FAR.PlantId='" + plantId + @"'
                                    and FAR.Archive=0 and FAR.IsAUC=0
                                    AND rdd.FixedAssetRegisterDisposedId ='" + fixedAssetRegisterDisposeId+ @"'
				                    GROUP BY FAR.MaterialMasterId ,MM.UserName ,MMA.StandardName ,FA.UserName,P.UserName 
			   ,MM.IsAsset,MBP.BusinessProcessName,FAR.FixedAssetMasterId
			    ,FAR.MaterialMasterId,FAR.MaterialMasterArticleId,FAR.VendorId,FAR.FixedAssetMasterId,FAR.Status,frd.DocDate,v.VoucherNo,frd.Id,frd.IsPark
				,Customer.UserName,CU.Code,frd.ToCurrencyRate,GP.Id,ISNULL(FCV.UserName,''),ISNULL(SCV.UserName,''),ISNULL(TCV.UserName,'')
				,FAR.Id,FAR.SerialNo,PC.Code,FAR.Price";
            return _sqlRepository.GetDataCollection(sql);

        }



        #endregion FA register

        private string GetPK()
		{
			string sID = string.Empty;
			bplib.clsGenID objGenID = new bplib.clsGenID();
			objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(InOutGatePassMaster), out sID);
			return sID;
		}		
		public void createInOutGatePass(InOutGatePassMaster inOutGatePassMasterModel)
		{
			if(string.IsNullOrEmpty(inOutGatePassMasterModel.Id))
			{
				var inOutGatePassMaster = new InOutGatePassMaster
				{
					Id = GetPK(),
					CompanyGroupId = inOutGatePassMasterModel.CompanyGroupId,
					CompanyId = inOutGatePassMasterModel.CompanyId,
					PlantId = inOutGatePassMasterModel.PlantId,
					GatePassType = inOutGatePassMasterModel.GatePassType,
					GatePassStatus = inOutGatePassMasterModel.GatePassStatus,
					ReturnableDate = inOutGatePassMasterModel.ReturnableDate,
					GatePassEntryDate = inOutGatePassMasterModel.GatePassEntryDate,
					FromEmployeeId = inOutGatePassMasterModel.FromEmployeeId,
					Through = inOutGatePassMasterModel.Through,
					CourierName = inOutGatePassMasterModel.CourierName,
					RunnerEmployeeId = inOutGatePassMasterModel.RunnerEmployeeId,
					ToType = inOutGatePassMasterModel.ToType,
					ToPartyCode = inOutGatePassMasterModel.ToPartyCode,
					ToBuyerId = inOutGatePassMasterModel.ToBuyerId,
					ToPlantId = inOutGatePassMasterModel.ToPlantId,
					ToUnitId = inOutGatePassMasterModel.ToUnitId,
					ToDivisionId = inOutGatePassMasterModel.ToDivisionId,
					ToDepartment = inOutGatePassMasterModel.ToDepartment,
					DepartmentEmployeeId = inOutGatePassMasterModel.DepartmentEmployeeId,
					OtherCompanyName = inOutGatePassMasterModel.OtherCompanyName,
					PersonName = inOutGatePassMasterModel.PersonName,
					MobileNo = inOutGatePassMasterModel.MobileNo,
                    VehicleNo = inOutGatePassMasterModel.VehicleNo,
                    TransportAgentMobileNo = inOutGatePassMasterModel.TransportAgentMobileNo,
                    TransportAgentName = inOutGatePassMasterModel.TransportAgentName,
                    Address = inOutGatePassMasterModel.Address,
					Remarks = inOutGatePassMasterModel.Remarks,
					CheckedBy = inOutGatePassMasterModel.CheckedBy,
					CheckedByStatus = inOutGatePassMasterModel.CheckedByStatus,
					CheckedHoldRejectReason = inOutGatePassMasterModel.CheckedHoldRejectReason,
					ApprovedBy = inOutGatePassMasterModel.ApprovedBy,
					ApprovedByStatus = inOutGatePassMasterModel.ApprovedByStatus,
					ApprovedHoldRejectReason = inOutGatePassMasterModel.ApprovedHoldRejectReason,
					SenderSecurityEmployeeId = inOutGatePassMasterModel.SenderSecurityEmployeeId,
					SenderSecurityApprovedStatus = inOutGatePassMasterModel.SenderSecurityApprovedStatus,
					ReceiverSecurityEmployeeId = inOutGatePassMasterModel.ReceiverSecurityEmployeeId,
					ReceiverSecurityApprovedStatus = inOutGatePassMasterModel.ReceiverSecurityApprovedStatus,
					VendorBuyerOtherCompanyReceivedStatus = inOutGatePassMasterModel.VendorBuyerOtherCompanyReceivedStatus,
					PurchaseReturnId = inOutGatePassMasterModel.PurchaseReturnId,
					InventoryTransferId = inOutGatePassMasterModel.InventoryTransferId,
					InventorySalesId = inOutGatePassMasterModel.InventorySalesId,
					InventoryScrapId = inOutGatePassMasterModel.InventoryScrapId,
                    FixedAssetRegisterDisposedId = inOutGatePassMasterModel.FixedAssetRegisterDisposedId,
					FixedAssetScrapId = inOutGatePassMasterModel.FixedAssetScrapId,
				};
				InsertInOutGatePass(inOutGatePassMaster, out DataSet _InOutGatePassdataset);
				clsStaticInfo objApp = new clsStaticInfo();
				objApp.SaveDataSets(_InOutGatePassdataset);
			}
			else
			{
				var inOutGatePassMaster = new InOutGatePassMaster
				{
					Id = inOutGatePassMasterModel.Id,
					CompanyGroupId = inOutGatePassMasterModel.CompanyGroupId,
					CompanyId = inOutGatePassMasterModel.CompanyId,
					PlantId = inOutGatePassMasterModel.PlantId,
					GatePassType = inOutGatePassMasterModel.GatePassType,
					GatePassStatus = inOutGatePassMasterModel.GatePassStatus,
					ReturnableDate = inOutGatePassMasterModel.ReturnableDate,
					GatePassEntryDate = inOutGatePassMasterModel.GatePassEntryDate,
					FromEmployeeId = inOutGatePassMasterModel.FromEmployeeId,
					Through = inOutGatePassMasterModel.Through,
					CourierName = inOutGatePassMasterModel.CourierName,
					RunnerEmployeeId = inOutGatePassMasterModel.RunnerEmployeeId,
					ToType = inOutGatePassMasterModel.ToType,
					ToPartyCode = inOutGatePassMasterModel.ToPartyCode,
					ToBuyerId = inOutGatePassMasterModel.ToBuyerId,
					ToPlantId = inOutGatePassMasterModel.ToPlantId,
					ToUnitId = inOutGatePassMasterModel.ToUnitId,
					ToDivisionId = inOutGatePassMasterModel.ToDivisionId,
					ToDepartment = inOutGatePassMasterModel.ToDepartment,
					DepartmentEmployeeId = inOutGatePassMasterModel.DepartmentEmployeeId,
					OtherCompanyName = inOutGatePassMasterModel.OtherCompanyName,
					PersonName = inOutGatePassMasterModel.PersonName,
					MobileNo = inOutGatePassMasterModel.MobileNo,
					Address = inOutGatePassMasterModel.Address,
					Remarks = inOutGatePassMasterModel.Remarks,
					CheckedBy = inOutGatePassMasterModel.CheckedBy,
					CheckedByStatus = inOutGatePassMasterModel.CheckedByStatus,
					CheckedHoldRejectReason = inOutGatePassMasterModel.CheckedHoldRejectReason,
					ApprovedBy = inOutGatePassMasterModel.ApprovedBy,
					ApprovedByStatus = inOutGatePassMasterModel.ApprovedByStatus,
					ApprovedHoldRejectReason = inOutGatePassMasterModel.ApprovedHoldRejectReason,
					SenderSecurityEmployeeId = inOutGatePassMasterModel.SenderSecurityEmployeeId,
					SenderSecurityApprovedStatus = inOutGatePassMasterModel.SenderSecurityApprovedStatus,
					ReceiverSecurityEmployeeId = inOutGatePassMasterModel.ReceiverSecurityEmployeeId,
					ReceiverSecurityApprovedStatus = inOutGatePassMasterModel.ReceiverSecurityApprovedStatus,
					VendorBuyerOtherCompanyReceivedStatus = inOutGatePassMasterModel.VendorBuyerOtherCompanyReceivedStatus,
					PurchaseReturnId = inOutGatePassMasterModel.PurchaseReturnId,
					InventoryTransferId = inOutGatePassMasterModel.InventoryTransferId,
					InventorySalesId = inOutGatePassMasterModel.InventorySalesId,
					InventoryScrapId = inOutGatePassMasterModel.InventoryScrapId,
                    FixedAssetRegisterDisposedId = inOutGatePassMasterModel.FixedAssetRegisterDisposedId,
					FixedAssetScrapId = inOutGatePassMasterModel.FixedAssetScrapId,
				};
				UpdateInOutGatePass(inOutGatePassMaster, out DataSet _InOutGatePassdataset);
				clsStaticInfo objApp = new clsStaticInfo();
				objApp.SaveDataSets(_InOutGatePassdataset);
			}
			
		}
		public InOutGatePassMaster InsertInOutGatePass(InOutGatePassMaster inOutGatePassMaster, out DataSet dsData)
		{
			MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
			if (string.IsNullOrEmpty(inOutGatePassMaster.AddedBy))
				AuditService.AddedLog(inOutGatePassMaster);
			ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
			con.getDataSet("Select * from TRN.InOutGatePassMaster where 1=2", out dsData);
			materialCommonService.AddNewRow<InOutGatePassMaster>(dsData.Tables[0], inOutGatePassMaster);

			return inOutGatePassMaster;
		}
		public InOutGatePassMaster UpdateInOutGatePass(InOutGatePassMaster inOutGatePassMaster, out DataSet dsData)
		{
			MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
			if (string.IsNullOrEmpty(inOutGatePassMaster.AddedBy))
				AuditService.AddedLog(inOutGatePassMaster);
			ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
			con.getDataSet("Select * from TRN.InOutGatePassMaster where Id='"+ inOutGatePassMaster.Id+ "'", out dsData);
			materialCommonService.EditRow<InOutGatePassMaster>(dsData.Tables[0].Rows[0], inOutGatePassMaster);
			return inOutGatePassMaster;
		}
		public void DeleteInOutGatePass(string GatePassId)   
		{
			try
			{
				if (string.IsNullOrEmpty(GatePassId))
					throw new Exception("In/Out Gate Pass Id Not Found");

				ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
				con.BeginTransaction();
				con.executeQuery("delete from TRN.InOutGatePassMaster where Id='" + GatePassId + "'"); 
				con.CommitTransaction();
			}
			catch (Exception ex)
			{
							

			}
		}

		public IEnumerable<object> GetInOutGateIndexGridDataList(string Name,string PendingApprvedGateOut)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";
                if(PendingApprvedGateOut=="Pending")
                {
                    sql = @"SELECT  GPM.[Id]
                                ,INR.ID GRNNO
                                , EI.EmployeeName CheckedByName
                                , EI1.EmployeeName ApprovedByName  
								,GPM.[CompanyGroupId]
								,GPM.[CompanyId]
								,GPM.[PlantId]
								,GPM.[GatePassType]
								,GPM.[GatePassStatus]                             
								,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
								,GPM.[FromEmployeeId]
								,EI.EmployeeName SenderName
								,GPM.[Through]
								,GPM.[CourierName]
								,GPM.[RunnerEmployeeId]
								,EI1.EmployeeName RunnerEmployee                              
								,GPM.[Remarks]
								,GPM.[CheckedBy]
								,GPM.[CheckedByStatus]
								,GPM.[CheckedHoldRejectReason]
								,GPM.[ApprovedBy]
								,GPM.[ApprovedByStatus]
								,GPM.[ApprovedHoldRejectReason]                            
								,GPM.[AddedBy]
								,GPM.[AddedDate]
								,GPM.[AddedFromIP]
								,GPM.[UpdatedBy]
								,GPM.[UpdatedDate]
								,GPM.[UpdatedFromIP]--,GPM.ChallanNo
								,PurchaseReturnId,InventoryTransferId,InventorySalesId,InventoryScrapId,FixedAssetRegisterDisposedId,FixedAssetScrapId
							FROM [TRN].[InOutGatePassMaster] GPM
								LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.CheckedBy
							LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.ApprovedBy   
                            left jOIN [TRN].[PurchaseReturn] AS IR ON IR.Id=GPM.PurchaseReturnId
                            LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId   
	                        LEFT JOIN TRn.InventoryReceive AS INR ON INR.ID=IR.InventoryReceiveId
                            Where GPM.Addedby='" + Name + @"' AND GPM.CheckedByStatus ='Forchecked' OR (isnull(GPM.CheckedByStatus,'')='Hold' OR isnull(GPM.CheckedByStatus,'')='Reject')  Order By GPM.AddedDate DESC";
                }
                else if (PendingApprvedGateOut == "Checked")
                {
                    sql = @"SELECT  GPM.[Id]
                                ,INR.ID GRNNO
                                , EI.EmployeeName CheckedByName
                                , EI1.EmployeeName ApprovedByName  
								,GPM.[CompanyGroupId]
								,GPM.[CompanyId]
								,GPM.[PlantId]
								,GPM.[GatePassType]
								,GPM.[GatePassStatus]                             
								,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
								,GPM.[FromEmployeeId]
								,EI.EmployeeName SenderName
								,GPM.[Through]
								,GPM.[CourierName]
								,GPM.[RunnerEmployeeId]
								,EI1.EmployeeName RunnerEmployee                              
								,GPM.[Remarks]
								,GPM.[CheckedBy]
								,GPM.[CheckedByStatus]
								,GPM.[CheckedHoldRejectReason]
								,GPM.[ApprovedBy]
								,GPM.[ApprovedByStatus]
								,GPM.[ApprovedHoldRejectReason]                            
								,GPM.[AddedBy]
								,GPM.[AddedDate]
								,GPM.[AddedFromIP]
								,GPM.[UpdatedBy]
								,GPM.[UpdatedDate]
								,GPM.[UpdatedFromIP]--,GPM.ChallanNo
								,PurchaseReturnId,InventoryTransferId,InventorySalesId,InventoryScrapId,FixedAssetScrapId
							FROM [TRN].[InOutGatePassMaster] GPM
								LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.CheckedBy
							LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.ApprovedBy  
                            left jOIN [TRN].[PurchaseReturn] AS IR ON IR.Id=GPM.PurchaseReturnId
                            LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId   
	                        LEFT JOIN TRn.InventoryReceive AS INR ON INR.ID=IR.InventoryReceiveId
                            Where GPM.Addedby='" + Name + @"' AND isnull(GPM.CheckedByStatus,'')='Checked'  Order By GPM.AddedDate DESC";

                }
                else if (PendingApprvedGateOut == "GateOut")
                {
                    sql = @"SELECT  GPM.[Id]
                                ,INR.ID GRNNO
								,GPM.[CompanyGroupId]
								,GPM.[CompanyId]
								,GPM.[PlantId]
								,GPM.[GatePassType]
								,GPM.[GatePassStatus]                             
								,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
								,GPM.[FromEmployeeId]
                                , EI.EmployeeName CheckedByName
                                , EI1.EmployeeName ApprovedByName  
								,EI.EmployeeName SenderName
								,GPM.[Through]
								,GPM.[CourierName]
								,GPM.[RunnerEmployeeId]
								,EI1.EmployeeName RunnerEmployee                              
								,GPM.[Remarks]
								,GPM.[CheckedBy]
								,GPM.[CheckedByStatus]
								,GPM.[CheckedHoldRejectReason]
								,GPM.[ApprovedBy]
								,GPM.[ApprovedByStatus]
								,GPM.[ApprovedHoldRejectReason]                            
								,GPM.[AddedBy]
								,GPM.[AddedDate]
								,GPM.[AddedFromIP]
								,GPM.[UpdatedBy]
								,GPM.[UpdatedDate]
								,GPM.[UpdatedFromIP]--,GPM.ChallanNo
								,PurchaseReturnId,InventoryTransferId,InventorySalesId,InventoryScrapId,FixedAssetScrapId
							FROM [TRN].[InOutGatePassMaster] GPM
								LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.CheckedBy
							LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.ApprovedBy   
                            left jOIN [TRN].[PurchaseReturn] AS IR ON IR.Id=GPM.PurchaseReturnId
                            LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId   
	                        LEFT JOIN TRn.InventoryReceive AS INR ON INR.ID=IR.InventoryReceiveId
                            Where GPM.Addedby='" + Name + @"' AND  GPM.CheckedByStatus = 'Checked' And Isnull(GPM.GateOutStatus,0)= 1  Order By GPM.AddedDate DESC";
                }
                

                return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}



        #region IN OUT Gate Pass Report
        public IWordDocument InOutGatePassReport(string companyGroupId, string plantId, string GatePassId)
        {

            ReportUtility ru = new ReportUtility();
            var fileName = "";
            var strPath = "";
            var File = "";
            fileName = "InOutGatePass" + plantId + ".docx";
            strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }

            ////A opens input document.
            WordDocument document = new WordDocument(File, FormatType.Docx);

            //Gets the paragraph at index 1
            try
            {
                string invoicePartyAddress = "";
                string vendorPartyAddress = "";
                WSection section = document.Sections[0];

                DataTable dsOrderMaster, dsServiceItems;
                dsOrderMaster = loadInOutGatePassMaster(GatePassId);//sql
                Dictionary<string, string> columns = new Dictionary<string, string>();
                var poApprovedStatus = "";
                //invoicePartyAddress = ru.GetAddress(dsOrderMaster.Rows[0]["InvoicePartyAddressMasterId"].ToString(), dsOrderMaster.Rows[0]["InvoicingByAddress"].ToString());
                document.Replace("{InvoicingPartyAddress}", invoicePartyAddress, false, false);
                //vendorPartyAddress = ru.GetAddress(dsOrderMaster.Rows[0]["VendorAddressMasterId"].ToString(), "");
                document.Replace("{VendorAddress}", vendorPartyAddress, false, false);
                //document.Replace("{DeliveryInstruction}", dsOrderMaster.Rows[0]["DeliveryInstruction"].ToString(), false, false);
                //document.Replace("{SpecialInstruction}", dsOrderMaster.Rows[0]["SpecialInstruction"].ToString(), false, false);
                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);
                // dsServiceItems = loadServicerMasterItems(purchaseOrderId);
                var materialTotal = makeInOutGatePassDetailTable(document, dsOrderMaster, GatePassId);//Material Details 
                var serviceTotal = 0.00;
                //if (dsServiceItems.Rows.Count > 0)
                //{
                //    //{ServiceItems}
                //    serviceTotal = makeServiceDetailsTable(document, dsServiceItems, purchaseOrderId);//Service Details 
                //    document.Replace("{ServiceDetails}", "Service Details", true, true);
                //}
                //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                //document.Replace("{TotalInWords}", ru.InWord((materialTotal + serviceTotal), dsOrderMaster.Rows[0]["CurrencyId"].ToString()), true, true);
                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();
                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                //creating secondary array to prevent memory leak and accidental over-writing (Tarek Talukder-26-May-2019)
                List<string> strReplace = new List<string>();
                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());
                StringCollection strColDistinct = new StringCollection();
                for (int i = 0; i < strReplace.Count; i++)
                {
                    if (strColDistinct.Contains(strReplace[i].ToUpper()))
                        continue;

                    strColDistinct.Add(strReplace[i].ToUpper());

                    string text = strReplace[i].ToUpper();
                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        ReplaceInfo[text] = document.Replace(text, dsOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }

                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);
                //removing any unused place holder
                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "", false, false);

                }
                DocToPDFConverter converter = new DocToPDFConverter();
                //Converts Word document into PDF document
                //Syncfusion.Pdf.PdfDocument pdfDocument = converter.ConvertToPDF(document);
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                pdfDocument.PageSettings.Width = 1200;
                pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();
                //Closes the instance of document objects
                document.Close();
                string Prefix = "InOutGatePass" + GatePassId;
                //Saves the PDF file 
                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);
                document.Close();

                return document;
            }

            catch (Exception ex)
            {
                throw ex;
            }
            //Closes the instance of document objects


        }


        public DataTable loadInOutGatePassMaster(string GatePassId)
        {
            string strSQL;
            //clsConnection objCon;
            try
            {
                strSQL = @"SELECT  GPM.[Id]
								,GPM.[CompanyGroupId]
								,GPM.[CompanyId]
								,GPM.[PlantId]
								,GPM.[GatePassType]
								,GPM.[GatePassStatus]                             
								,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
								,GPM.[FromEmployeeId]
								,EI.EmployeeName SenderName
								,GPM.[Through]
								,GPM.[CourierName]
								,GPM.[RunnerEmployeeId]
								,EI1.EmployeeName RunnerEmployee                              
								,GPM.[Remarks]
								,GPM.[CheckedBy]
								,GPM.[CheckedByStatus]
								,GPM.[CheckedHoldRejectReason]
								,GPM.[ApprovedBy]
								,GPM.[ApprovedByStatus]
								,GPM.[ApprovedHoldRejectReason]                            
								,GPM.[AddedBy]
								,GPM.[AddedDate]
								,GPM.[AddedFromIP]
								,GPM.[UpdatedBy]
								,GPM.[UpdatedDate]
								,GPM.[UpdatedFromIP]
								,GPM.PurchaseReturnId
								,GPM.InventorySalesId
								,GPM.InventoryScrapId
								,GPM.InventoryTransferId
								,GPM.FixedAssetRegisterDisposedId
								,GPM.FixedAssetScrapId
						,MT.UserName MaterialType
						,MGM.UserName AS MaterialGroupMasterName
						,IM.MaterialMasterId
						,MM.UserName MaterialMasterName
						, ART.StandardName ArticleName
						,IsAsset=CASE WHEN MM.IsAsset=0 then 'No' else 'Yes' END
						, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
						, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
						, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
						,TUoM.UserName AS UOM
						,IRD.TransactionQty
	                    ,p.UserName  PartyName
                        ,INR.ID GRNNO
						,Round(IRD.TotalMaterialBooksCurrencyAmount,2) TotalMaterialBooksCurrencyAmount
							FROM [TRN].[InOutGatePassMaster] GPM
							LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.FromEmployeeId
							LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.RunnerEmployeeId     
							left jOIN [TRN].[PurchaseReturn] AS IR ON IR.Id=GPM.PurchaseReturnId
							left jOIN [TRN].[PurchaseReturnDetail] AS IRD ON IRD.PurchaseReturnId=IR.Id
							Left JOIN  TRN.InventoryMaterial AS IM  ON IM.Id=IRD.InventoryMaterialId
							left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
							LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
							LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
							LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
							LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
							LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
							LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
							left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id				
							left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
							LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id				
							LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId   
                            LEFT JOIN TRn.InventoryReceive AS INR ON INR.ID=IR.InventoryReceiveId
                    Where GPM.Id='" + GatePassId + @"'Order By GPM.[Id] DESC";

                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        public double makeInOutGatePassDetailTable(WordDocument document, DataTable dsMaterialItems, string GatePassId)
        {
            string replaceString = "{materialItems}";
            ReportUtility ru = new ReportUtility();
            DataTable dsTax;
            //clsDataContext data = new clsDataContext();


            //dsTax = loadMaterialTax(purchaseOrderId);

            int LasColumnIndex = 8;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            //DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));

            //if (dv.Count > 0)
            //{
            //    for (int i = 0; i < dv.Count; i++)
            //    {
            //        LasColumnIndex++;
            //        dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
            //        LasColumnIndex++;
            //    }
            //}


            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();
            //wTable.Title = "Material Details";
            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;



            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SL");
            int colRo = COL; COL++;
            //wTable.Rows[ROW].Cells[colRo].Width = 25;

            //wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("RowId");
            //int colRowId = COL; COL++;
            //wTable.Rows[ROW].Cells[colRowId].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Materials Type");
            range.ApplyCharacterFormat(FontBold);
            int colMaterialType = COL; COL++;
            //wTable.Rows[ROW].Cells[colMaterialType].Width = 85;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Materials Group");
            range.ApplyCharacterFormat(FontBold);
            int colMaterialGroup = COL; COL++;
            //wTable.Rows[ROW].Cells[colMaterialGroup].Width = 85;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Material");
            range.ApplyCharacterFormat(FontBold);
            int colMaterial = COL; COL++;
            //wTable.Rows[ROW].Cells[colMaterial].Width = 85;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article ");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            //wTable.Rows[ROW].Cells[colArticle].Width = 85;
            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU1");
            range.ApplyCharacterFormat(FontBold);
            int colChar1 = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU2");
            range.ApplyCharacterFormat(FontBold);
            int colChar2 = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU3");
            range.ApplyCharacterFormat(FontBold);
            int colChar3 = COL; COL++;
            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Origin");//TRN.PurchaseOrderDetail ->CountryId
            //range.ApplyCharacterFormat(FontBold);
            //int colOriginCountry = COL; COL++;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Material Detail");
            //range.ApplyCharacterFormat(FontBold);
            //int colMaterialDetail = COL; COL++;


            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Required Date");
            //range.ApplyCharacterFormat(FontBold);
            //int colRequiredDate = COL; COL++;
            //wTable.Rows[ROW].Cells[colRequiredDate].Width = 60;



            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate (" + dsMaterialItems.Rows[0]["CurrencyName"].ToString() + ")");
            //range.ApplyCharacterFormat(FontBold);
            //int colRate = COL++;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UOM");
            //range.ApplyCharacterFormat(FontBold);
            //int colUOM = COL; COL++;
            //wTable.Rows[ROW].Cells[colUOM].Width = 25;





            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
            //range.ApplyCharacterFormat(FontBold);
            //int colTotalTaxableAmount = COL; COL++;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Normal/Over Budget/New");
            //range.ApplyCharacterFormat(FontBold);
            //int colNormal = COL; COL++;
            //wTable.Rows[ROW].Cells[colNormal].Width = 58;
            ////range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Normal/OverBudget/New");
            ////range.ApplyCharacterFormat(FontBold);
            ////int colOB = COL; COL++;

            ////range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Normal/OverBudget/New");
            ////range.ApplyCharacterFormat(FontBold);
            ////int colNew = COL; COL++;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Own Stock");
            //range.ApplyCharacterFormat(FontBold);
            //int colOnStock = COL; COL++;
            //wTable.Rows[ROW].Cells[colUOM].Width = 40;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Other Stock");
            //range.ApplyCharacterFormat(FontBold);
            //int colOtherStock = COL; COL++;
            //wTable.Rows[ROW].Cells[colOtherStock].Width = 40;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Reason");
            //range.ApplyCharacterFormat(FontBold);
            //int colReason = COL; COL++;


            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Remarks");
            //range.ApplyCharacterFormat(FontBold);
            //int colRemarks = COL;



            //if (dv.Count > 0)
            //{
            //    COL++;
            //    colTotalTaxableAmount = COL;
            //    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
            //    range.ApplyCharacterFormat(FontBold);
            //    //COL++;
            //    for (int i = 0; i < dv.Count; i++)
            //    {
            //        //two columns required for tax
            //        COL++;
            //        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
            //        range.ApplyCharacterFormat(FontBold);
            //        COL++;
            //        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
            //        range.ApplyCharacterFormat(FontBold);

            //    }
            //}
            //else
            //{


            //}


            //wTable.Rows.Add(TemplateRow);
            //ROW++;

            //if (dv.Count > 0)
            //{
            //    for (int i = 0; i < dv.Count; i++)
            //    {

            //        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate");
            //        range.ApplyCharacterFormat(FontBold);
            //        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
            //        range.ApplyCharacterFormat(FontBold);

            //    }
            //}
            #endregion column headers
            var NormalOverBudgetNew = "";
            var normalBudgetType = "";
            var overBudgetType = "";
            var MaterialDetail = "";
            var RequiredDate = "";
            var Remarks = "";
            var Reason = "";
            var OwnStock = "";
            var OtherStock = " ";


            double totalValue = 0;
            int startRow = ROW;
            int sl = 0;
            for (int i = 0; i < dsMaterialItems.Rows.Count; i++)
            {
                sl++;
                ROW++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                }
                //if (dsMaterialItems.Rows[i]["BudgetType"].ToString().ToUpper() == "NORMAL")
                //{
                //    NormalOverBudgetNew = "Normal";
                //}
                //if (dsMaterialItems.Rows[i]["BudgetType"].ToString().ToUpper() == "OVER BUDGET")
                //{
                //    NormalOverBudgetNew = "Over budget";
                //}
                //if (dsMaterialItems.Rows[i]["BudgetType"].ToString().ToUpper() != "NORMAL" && dsMaterialItems.Rows[i]["BudgetType"].ToString().ToUpper() != "OVER BUDGET")
                //{
                //    NormalOverBudgetNew = "New";
                //}

                TROW.Cells[colRo].AddParagraph().AppendText(sl.ToString());

                TROW.Cells[colMaterialType].AddParagraph().AppendText(dsMaterialItems.Rows[i]["MaterialType"].ToString());
                TROW.Cells[colMaterialGroup].AddParagraph().AppendText(dsMaterialItems.Rows[i]["MaterialGroupMasterName"].ToString());
                TROW.Cells[colMaterial].AddParagraph().AppendText(dsMaterialItems.Rows[i]["MaterialMasterName"].ToString());
                TROW.Cells[colArticle].AddParagraph().AppendText(dsMaterialItems.Rows[i]["ArticleName"].ToString());
                TROW.Cells[colChar1].AddParagraph().AppendText(dsMaterialItems.Rows[i]["FirstCharacteristicsValue"].ToString());
                TROW.Cells[colChar2].AddParagraph().AppendText(dsMaterialItems.Rows[i]["SecondCharacteristicsValue"].ToString());
                TROW.Cells[colChar3].AddParagraph().AppendText(dsMaterialItems.Rows[i]["ThirdCharacteristicsValue"].ToString());
                TROW.Cells[colQty].AddParagraph().AppendText(clsStdLib.dbl(dsMaterialItems.Rows[i]["TransactionQty"].ToString()).ToString("F2"));
 
                //ROW++;
            }

            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);
            double value = 0;
            for (int C = 0; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                value = 0;
                if (C == colRo || C == colMaterialType || C == colMaterial  || C == colArticle || C == colChar1 || C == colChar2 || C == colChar3 || /*C == colQty ||*/  C == colMaterialGroup)
                    continue;


                for (int i = startRow; i <= TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);

            }
            #endregion Total


            ROW++;
            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStdLib.dbl(dsMaterialItems.Compute("", "").ToString());
            //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
            //+ clsStdLib.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());

            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2") + " (" + ru.InWord(total, dsOrderMaster.Rows[0]["CurrencyId"].ToString()) + ")");

            #endregion Total


            ROW++;
            #region Total Payable
            //int TotalPayableRow = ROW;
            //int TotalPayableColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[TotalPayableColumn].AddParagraph().AppendText("Total Amount Payable");
            //_TROW.Cells[TotalPayableColumn + 1].AddParagraph().AppendText("Need To Discuss");

            #endregion Total Payable


            ROW++;

            //for (int R = 1; R < wTable.Rows.Count; R++)
            //{
            //    WTableRow TROW = wTable.Rows[R];



            //    foreach (WParagraph item in TROW.Cells[colQty].Paragraphs)
            //    {
            //        item.ApplyStyle("MyStyleRightAlign");
            //    }

            //}
            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            //myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                //TROW.Cells[0].Width = 20;
                //if (dv.Count < 3)
                //    TROW.Cells[0].Width = 120 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            //for (int i = 0; i < dv.Count; i++)
            //    wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            //for (int i = 0; i <= colTotalTaxableAmount; i++)
            //    wTable.ApplyVerticalMerge(i, ROW - 1, ROW);




            IWParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
            style.CharacterFormat.Bold = true;
            style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section

            #endregion merging section

            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);
            return total;
        }

        public IWordDocument InOutGatePassSalesReport(string companyGroupId, string plantId, string GatePassId)
        {

            ReportUtility ru = new ReportUtility();
            var fileName = "";
            var strPath = "";
            var File = "";
            DataTable dsOrderMaster;
            dsOrderMaster = loadInOutGatePassMasterSales(GatePassId); 
            fileName = "InOutGatePassSales" + plantId + ".docx";

            strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }

            ////A opens input document.
            WordDocument document = new WordDocument(File, FormatType.Docx);

            //Gets the paragraph at index 1
            try
            {
                string invoicePartyAddress = "";
                string vendorPartyAddress = "";
                WSection section = document.Sections[0];

                DataTable  dsServiceItems;
                /*dsOrderMaster = loadInOutGatePassMaster(GatePassId);*///sql
                Dictionary<string, string> columns = new Dictionary<string, string>();
                var poApprovedStatus = "";
                //invoicePartyAddress = ru.GetAddress(dsOrderMaster.Rows[0]["InvoicePartyAddressMasterId"].ToString(), dsOrderMaster.Rows[0]["InvoicingByAddress"].ToString());
                document.Replace("{InvoicingPartyAddress}", invoicePartyAddress, false, false);
                //vendorPartyAddress = ru.GetAddress(dsOrderMaster.Rows[0]["VendorAddressMasterId"].ToString(), "");
                document.Replace("{VendorAddress}", vendorPartyAddress, false, false);
                //document.Replace("{DeliveryInstruction}", dsOrderMaster.Rows[0]["DeliveryInstruction"].ToString(), false, false);
                //document.Replace("{SpecialInstruction}", dsOrderMaster.Rows[0]["SpecialInstruction"].ToString(), false, false);
                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);
                // dsServiceItems = loadServicerMasterItems(purchaseOrderId);
                var materialTotal = makeInOutGatePassDetailSalesTable(document, dsOrderMaster, GatePassId);//Material Details 

                var serviceTotal = 0.00;
                //if (dsServiceItems.Rows.Count > 0)
                //{
                //    //{ServiceItems}
                //    serviceTotal = makeServiceDetailsTable(document, dsServiceItems, purchaseOrderId);//Service Details 
                //    document.Replace("{ServiceDetails}", "Service Details", true, true);
                //}
                //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                //document.Replace("{TotalInWords}", ru.InWord((materialTotal + serviceTotal), dsOrderMaster.Rows[0]["CurrencyId"].ToString()), true, true);
                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();
                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                //creating secondary array to prevent memory leak and accidental over-writing (Tarek Talukder-26-May-2019)
                List<string> strReplace = new List<string>();
                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());
                StringCollection strColDistinct = new StringCollection();
                for (int i = 0; i < strReplace.Count; i++)
                {
                    if (strColDistinct.Contains(strReplace[i].ToUpper()))
                        continue;

                    strColDistinct.Add(strReplace[i].ToUpper());

                    string text = strReplace[i].ToUpper();
                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        ReplaceInfo[text] = document.Replace(text, dsOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }

                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);
                CodeQrBarcodeDraw qrCode = BarcodeDrawFactory.CodeQr;
                System.Drawing.Image barcodeImg = qrCode.Draw(dsOrderMaster.Rows[0]["Id"].ToString(), 200, 2);
                
                WPicture picture=  OTSBD.clsStaticInfo.GetWordDocumentPicture(document, "GatepassQR");
                if(picture!=null)
                {
                    picture.LoadImage(barcodeImg);
                }
                
                
                //removing any unused place holder
                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "", false, false);

                }
                DocToPDFConverter converter = new DocToPDFConverter();
                //Converts Word document into PDF document
                //Syncfusion.Pdf.PdfDocument pdfDocument = converter.ConvertToPDF(document);
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                pdfDocument.PageSettings.Width = 1200;
                pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();
                //Closes the instance of document objects
                document.Close();
                string Prefix = "InOutGatePass" + GatePassId;
                //Saves the PDF file 
                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);
                document.Close();

                return document;
            }

            catch (Exception ex)
            {
                throw ex;
            }
            //Closes the instance of document objects


        }


        public DataTable loadInOutGatePassMasterSales(string GatePassId)
        {
            string strSQL;
            //clsConnection objCon;
            try
            {
                strSQL = @"SELECT  GPM.[Id]
								,GPM.[CompanyGroupId]
								,GPM.[CompanyId]
								,GPM.[PlantId]
								,GPM.[GatePassType]
								,GPM.[GatePassStatus]                             
								,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
								,GPM.[FromEmployeeId]
								,EI.EmployeeName SenderName
								,GPM.[Through]
								,GPM.[CourierName]
								,GPM.[RunnerEmployeeId]
								,EI1.EmployeeName RunnerEmployee                              
								,GPM.[Remarks]
								,GPM.[CheckedBy]
								,GPM.[CheckedByStatus]
								,GPM.[CheckedHoldRejectReason]
								,GPM.[ApprovedBy]
								,GPM.[ApprovedByStatus]
								,GPM.[ApprovedHoldRejectReason]                            
								,GPM.[AddedBy]
								,GPM.[AddedDate]
								,GPM.[AddedFromIP]
								,GPM.[UpdatedBy]
								,GPM.[UpdatedDate]
								,GPM.[UpdatedFromIP]
								,GPM.PurchaseReturnId
								,GPM.InventorySalesId
								,GPM.InventoryScrapId
								,GPM.InventoryTransferId
								,GPM.FixedAssetRegisterDisposedId
								,GPM.FixedAssetScrapId
						,MT.UserName MaterialType
						,MGM.UserName AS MaterialGroupMasterName
						,FR.MaterialMasterId
						,MM.UserName MaterialMasterName
						, ART.StandardName ArticleName
						,FA.UserName AssetMaster
						,IsAsset=CASE WHEN MM.IsAsset=0 then 'No' else 'Yes' END
						, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
						, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
						, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
						,Round(FARD.NegotiationValue,2) TotalMaterialBooksCurrencyAmount
						, FR.SerialNo, FR.Id AssetNo,FAD.Id DisposalNo,FR.Status,FAD.DeliveryByAddress,VPL.UserName DeliveryParty
							FROM [TRN].[InOutGatePassMaster] GPM
							LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.FromEmployeeId
							LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.RunnerEmployeeId 
							LEFT JOIN TRN.FixedAssetRegisterDisposed FAD ON FAD.Id=GPM.FixedAssetRegisterDisposedId
							LEFT JOIN TRN.FixedAssetRegisterDisposedDetail FARD ON FARD.FixedAssetRegisterDisposedId=FAD.Id
                            LEFT JOIN [TRN].[FixedAssetRegister] FR   ON FR.Id=FARD.FixedAssetRegisterId 
	                        LEFT JOIN HKP.Party Customer ON Customer.Id = FAD.PartyId
                            LEFT JOIN SCS.Currency CU ON CU.Id =FAD.CurrencyId
							LEFT JOIN HKP.PartyPlant VPL ON VPL.Id = FAD.DeliveryPartyPlantId
							left JOIN MST.MaterialMaster AS MM ON FR.MaterialMasterId=MM.Id
							LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
							LEFT JOIN MST.MaterialMasterArticle AS ART ON FR.MaterialMasterArticleId=ART.Id
							LEFT JOIN HKP.CharacteristicsValue AS FCV ON MM.Id=FCV.MaterialMasterId
							LEFT JOIN HKP.CharacteristicsValue AS SCV ON MM.Id=SCV.MaterialMasterId
							LEFT JOIN HKP.CharacteristicsValue AS TCV ON MM.Id=TCV.MaterialMasterId
							LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
						    LEFT JOIN MST.FixedAssetMaster FA ON FA.Id=FR.FixedAssetMasterId
                    Where GPM.Id='" + GatePassId + @"'Order By GPM.[Id] DESC";

                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        public IWordDocument InOutGatePassScrapReport(string companyGroupId, string plantId, string GatePassId)
        {

            ReportUtility ru = new ReportUtility();
            var fileName = "";
            var strPath = "";
            var File = "";
            DataTable dsOrderMaster;
            dsOrderMaster = loadInOutGatePassMasterScrap(GatePassId);
            fileName = "InOutGatePassScrap" + plantId + ".docx";

            strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }

            ////A opens input document.
            WordDocument document = new WordDocument(File, FormatType.Docx);

            //Gets the paragraph at index 1
            try
            {
                string invoicePartyAddress = "";
                string vendorPartyAddress = "";
                WSection section = document.Sections[0];

                DataTable dsServiceItems;
                /*dsOrderMaster = loadInOutGatePassMaster(GatePassId);*///sql
                Dictionary<string, string> columns = new Dictionary<string, string>();
                var poApprovedStatus = "";
                //invoicePartyAddress = ru.GetAddress(dsOrderMaster.Rows[0]["InvoicePartyAddressMasterId"].ToString(), dsOrderMaster.Rows[0]["InvoicingByAddress"].ToString());
                document.Replace("{InvoicingPartyAddress}", invoicePartyAddress, false, false);
                //vendorPartyAddress = ru.GetAddress(dsOrderMaster.Rows[0]["VendorAddressMasterId"].ToString(), "");
                document.Replace("{VendorAddress}", vendorPartyAddress, false, false);
                //document.Replace("{DeliveryInstruction}", dsOrderMaster.Rows[0]["DeliveryInstruction"].ToString(), false, false);
                //document.Replace("{SpecialInstruction}", dsOrderMaster.Rows[0]["SpecialInstruction"].ToString(), false, false);
                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);
                // dsServiceItems = loadServicerMasterItems(purchaseOrderId);
                var materialTotal = makeInOutGatePassDetailSalesTable(document, dsOrderMaster, GatePassId);//Material Details 

                var serviceTotal = 0.00;
                //if (dsServiceItems.Rows.Count > 0)
                //{
                //    //{ServiceItems}
                //    serviceTotal = makeServiceDetailsTable(document, dsServiceItems, purchaseOrderId);//Service Details 
                //    document.Replace("{ServiceDetails}", "Service Details", true, true);
                //}
                //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                //document.Replace("{TotalInWords}", ru.InWord((materialTotal + serviceTotal), dsOrderMaster.Rows[0]["CurrencyId"].ToString()), true, true);
                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();
                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                //creating secondary array to prevent memory leak and accidental over-writing (Tarek Talukder-26-May-2019)
                List<string> strReplace = new List<string>();
                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());
                StringCollection strColDistinct = new StringCollection();
                for (int i = 0; i < strReplace.Count; i++)
                {
                    if (strColDistinct.Contains(strReplace[i].ToUpper()))
                        continue;

                    strColDistinct.Add(strReplace[i].ToUpper());

                    string text = strReplace[i].ToUpper();
                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        ReplaceInfo[text] = document.Replace(text, dsOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }

                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);
                CodeQrBarcodeDraw qrCode = BarcodeDrawFactory.CodeQr;
                System.Drawing.Image barcodeImg = qrCode.Draw(dsOrderMaster.Rows[0]["Id"].ToString(), 200, 2);

                WPicture picture = OTSBD.clsStaticInfo.GetWordDocumentPicture(document, "GatepassQR");
                if (picture != null)
                {
                    picture.LoadImage(barcodeImg);
                }


                //removing any unused place holder
                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "", false, false);

                }
                DocToPDFConverter converter = new DocToPDFConverter();
                //Converts Word document into PDF document
                //Syncfusion.Pdf.PdfDocument pdfDocument = converter.ConvertToPDF(document);
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                pdfDocument.PageSettings.Width = 1200;
                pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();
                //Closes the instance of document objects
                document.Close();
                string Prefix = "InOutGatePass" + GatePassId;
                //Saves the PDF file 
                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);
                document.Close();

                return document;
            }

            catch (Exception ex)
            {
                throw ex;
            }
            //Closes the instance of document objects


        }


        public DataTable loadInOutGatePassMasterScrap(string GatePassId)
        {
            string strSQL;
            //clsConnection objCon;
            try
            {
                strSQL = @"SELECT  GPM.[Id]
								,GPM.[CompanyGroupId]
								,GPM.[CompanyId]
								,GPM.[PlantId]
								,GPM.[GatePassType]
								,GPM.[GatePassStatus]                             
								,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
								,GPM.[FromEmployeeId]
								,EI.EmployeeName SenderName
								,GPM.[Through]
								,GPM.[CourierName]
								,GPM.[RunnerEmployeeId]
								,EI1.EmployeeName RunnerEmployee                              
								,GPM.[Remarks]
								,GPM.[CheckedBy]
								,GPM.[CheckedByStatus]
								,GPM.[CheckedHoldRejectReason]
								,GPM.[ApprovedBy]
								,GPM.[ApprovedByStatus]
								,GPM.[ApprovedHoldRejectReason]                            
								,GPM.[AddedBy]
								,GPM.[AddedDate]
								,GPM.[AddedFromIP]
								,GPM.[UpdatedBy]
								,GPM.[UpdatedDate]
								,GPM.[UpdatedFromIP]
								,GPM.PurchaseReturnId
								,GPM.InventorySalesId
								,GPM.InventoryScrapId
								,GPM.InventoryTransferId
								,GPM.FixedAssetRegisterDisposedId
								,GPM.FixedAssetScrapId
						,MT.UserName MaterialType
						,MGM.UserName AS MaterialGroupMasterName
						,FR.MaterialMasterId
						,MM.UserName MaterialMasterName
						, ART.StandardName ArticleName
						,FA.UserName AssetMaster
						,IsAsset=CASE WHEN MM.IsAsset=0 then 'No' else 'Yes' END
						, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
						, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
						, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
						,Round(FARD.NegotiationValue,2) TotalMaterialBooksCurrencyAmount
						, FR.SerialNo, FR.Id AssetNo,FAD.Id DisposalNo,FR.Status,FAD.DeliveryByAddress,VPL.UserName DeliveryParty
							FROM [TRN].[InOutGatePassMaster] GPM
							LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.FromEmployeeId
							LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.RunnerEmployeeId 
							LEFT JOIN TRN.FixedAssetRegisterDisposed FAD ON FAD.Id=GPM.FixedAssetScrapId
							LEFT JOIN TRN.FixedAssetRegisterDisposedDetail FARD ON FARD.FixedAssetRegisterDisposedId=FAD.Id
                            LEFT JOIN [TRN].[FixedAssetRegister] FR   ON FR.Id=FARD.FixedAssetRegisterId 
	                        LEFT JOIN HKP.Party Customer ON Customer.Id = FAD.PartyId
                            LEFT JOIN SCS.Currency CU ON CU.Id =FAD.CurrencyId
							LEFT JOIN HKP.PartyPlant VPL ON VPL.Id = FAD.DeliveryPartyPlantId
							left JOIN MST.MaterialMaster AS MM ON FR.MaterialMasterId=MM.Id
							LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
							LEFT JOIN MST.MaterialMasterArticle AS ART ON FR.MaterialMasterArticleId=ART.Id
							LEFT JOIN HKP.CharacteristicsValue AS FCV ON MM.Id=FCV.MaterialMasterId
							LEFT JOIN HKP.CharacteristicsValue AS SCV ON MM.Id=SCV.MaterialMasterId
							LEFT JOIN HKP.CharacteristicsValue AS TCV ON MM.Id=TCV.MaterialMasterId
							LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
						    LEFT JOIN MST.FixedAssetMaster FA ON FA.Id=FR.FixedAssetMasterId
                    Where GPM.Id='" + GatePassId + @"'Order By GPM.[Id] DESC";

                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        public double makeInOutGatePassDetailSalesTable(WordDocument document, DataTable dsMaterialItems, string GatePassId)
        {
            string replaceString = "{materialItems}";
            ReportUtility ru = new ReportUtility();
            DataTable dsTax;
            //clsDataContext data = new clsDataContext();


            //dsTax = loadMaterialTax(purchaseOrderId);

            int LasColumnIndex = 7;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            //DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));

            //if (dv.Count > 0)
            //{
            //    for (int i = 0; i < dv.Count; i++)
            //    {
            //        LasColumnIndex++;
            //        dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
            //        LasColumnIndex++;
            //    }
            //}


            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();
            //wTable.Title = "Material Details";
            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;



            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SL");
            int colRo = COL; COL++;
            //wTable.Rows[ROW].Cells[colRo].Width = 25;

            //wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("RowId");
            //int colRowId = COL; COL++;
            //wTable.Rows[ROW].Cells[colRowId].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Asset No");
            range.ApplyCharacterFormat(FontBold);
            int colAssetNo = COL; COL++;
            //wTable.Rows[ROW].Cells[colMaterialType].Width = 85;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Serial No");
            range.ApplyCharacterFormat(FontBold);
            int colSerialNo = COL; COL++;
            //wTable.Rows[ROW].Cells[colMaterialGroup].Width = 85;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Material Master");
            range.ApplyCharacterFormat(FontBold);
            int colMaterial = COL; COL++;
            //wTable.Rows[ROW].Cells[colMaterial].Width = 85;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article ");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            //wTable.Rows[ROW].Cells[colArticle].Width = 85;
            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU1");
            range.ApplyCharacterFormat(FontBold);
            int colChar1 = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU2");
            range.ApplyCharacterFormat(FontBold);
            int colChar2 = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU3");
            range.ApplyCharacterFormat(FontBold);
            int colChar3 = COL; COL++;
            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Asset Master");//TRN.PurchaseOrderDetail ->CountryId
            //range.ApplyCharacterFormat(FontBold);
            //int colAssetMaster = COL; COL++;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Material Detail");
            //range.ApplyCharacterFormat(FontBold);
            //int colMaterialDetail = COL; COL++;


            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Required Date");
            //range.ApplyCharacterFormat(FontBold);
            //int colRequiredDate = COL; COL++;
            //wTable.Rows[ROW].Cells[colRequiredDate].Width = 60;



            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
            //range.ApplyCharacterFormat(FontBold);
            //int colQty = COL; COL++;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate (" + dsMaterialItems.Rows[0]["CurrencyName"].ToString() + ")");
            //range.ApplyCharacterFormat(FontBold);
            //int colRate = COL++;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UOM");
            //range.ApplyCharacterFormat(FontBold);
            //int colUOM = COL; COL++;
            //wTable.Rows[ROW].Cells[colUOM].Width = 25;





            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
            //range.ApplyCharacterFormat(FontBold);
            //int colTotalTaxableAmount = COL; COL++;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Normal/Over Budget/New");
            //range.ApplyCharacterFormat(FontBold);
            //int colNormal = COL; COL++;
            //wTable.Rows[ROW].Cells[colNormal].Width = 58;
            ////range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Normal/OverBudget/New");
            ////range.ApplyCharacterFormat(FontBold);
            ////int colOB = COL; COL++;

            ////range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Normal/OverBudget/New");
            ////range.ApplyCharacterFormat(FontBold);
            ////int colNew = COL; COL++;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Own Stock");
            //range.ApplyCharacterFormat(FontBold);
            //int colOnStock = COL; COL++;
            //wTable.Rows[ROW].Cells[colUOM].Width = 40;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Other Stock");
            //range.ApplyCharacterFormat(FontBold);
            //int colOtherStock = COL; COL++;
            //wTable.Rows[ROW].Cells[colOtherStock].Width = 40;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Reason");
            //range.ApplyCharacterFormat(FontBold);
            //int colReason = COL; COL++;


            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Remarks");
            //range.ApplyCharacterFormat(FontBold);
            //int colRemarks = COL;



            //if (dv.Count > 0)
            //{
            //    COL++;
            //    colTotalTaxableAmount = COL;
            //    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
            //    range.ApplyCharacterFormat(FontBold);
            //    //COL++;
            //    for (int i = 0; i < dv.Count; i++)
            //    {
            //        //two columns required for tax
            //        COL++;
            //        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
            //        range.ApplyCharacterFormat(FontBold);
            //        COL++;
            //        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
            //        range.ApplyCharacterFormat(FontBold);

            //    }
            //}
            //else
            //{


            //}


            //wTable.Rows.Add(TemplateRow);
            //ROW++;

            //if (dv.Count > 0)
            //{
            //    for (int i = 0; i < dv.Count; i++)
            //    {

            //        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate");
            //        range.ApplyCharacterFormat(FontBold);
            //        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
            //        range.ApplyCharacterFormat(FontBold);

            //    }
            //}
            #endregion column headers
            var NormalOverBudgetNew = "";
            var normalBudgetType = "";
            var overBudgetType = "";
            var newBudgetType = "";
            var MaterialDetail = "";
            var RequiredDate = "";
            var Remarks = "";
            var Reason = "";
            var OwnStock = "";
            var OtherStock = " ";


            double totalValue = 0;
            int startRow = ROW;
            int sl = 0;
            for (int i = 0; i < dsMaterialItems.Rows.Count; i++)
            {
                sl++;
                ROW++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                }
                //if (dsMaterialItems.Rows[i]["BudgetType"].ToString().ToUpper() == "NORMAL")
                //{
                //    NormalOverBudgetNew = "Normal";
                //}
                //if (dsMaterialItems.Rows[i]["BudgetType"].ToString().ToUpper() == "OVER BUDGET")
                //{
                //    NormalOverBudgetNew = "Over budget";
                //}
                //if (dsMaterialItems.Rows[i]["BudgetType"].ToString().ToUpper() != "NORMAL" && dsMaterialItems.Rows[i]["BudgetType"].ToString().ToUpper() != "OVER BUDGET")
                //{
                //    NormalOverBudgetNew = "New";
                //}

                TROW.Cells[colRo].AddParagraph().AppendText(sl.ToString());

                TROW.Cells[colAssetNo].AddParagraph().AppendText(dsMaterialItems.Rows[i]["AssetNo"].ToString());
                TROW.Cells[colSerialNo].AddParagraph().AppendText(dsMaterialItems.Rows[i]["SerialNo"].ToString());
                TROW.Cells[colMaterial].AddParagraph().AppendText(dsMaterialItems.Rows[i]["MaterialMasterName"].ToString());
                TROW.Cells[colArticle].AddParagraph().AppendText(dsMaterialItems.Rows[i]["ArticleName"].ToString());
                TROW.Cells[colChar1].AddParagraph().AppendText(dsMaterialItems.Rows[i]["FirstCharacteristicsValue"].ToString());
                TROW.Cells[colChar2].AddParagraph().AppendText(dsMaterialItems.Rows[i]["SecondCharacteristicsValue"].ToString());
                TROW.Cells[colChar3].AddParagraph().AppendText(dsMaterialItems.Rows[i]["ThirdCharacteristicsValue"].ToString());
                //TROW.Cells[colAssetMaster].AddParagraph().AppendText(dsMaterialItems.Rows[i]["AssetMaster"].ToString());
                //TROW.Cells[colQty].AddParagraph().AppendText(clsStdLib.dbl(dsMaterialItems.Rows[i]["TransactionQty"].ToString()).ToString("F2"));

                //ROW++;
            }

            #region Total
            //int TotalRow = ROW;
            //wTable.AddRow();
            //WTableRow _TROW = wTable.LastRow;
            //_TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);
            //double value = 0;
            //for (int C = 0; C <= wTable.LastCell.GetCellIndex(); C++)
            //{
            //    value = 0;
            //    if (C == colRo || C == colAssetNo || C == colMaterial || C == colArticle || C == colChar1 || C == colChar2 || C == colChar3 || /*C == colQty ||*/  C == colSerialNo)
            //        continue;


            //    for (int i = startRow; i <= TotalRow; i++)
            //    {

            //        foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
            //        {
            //            value += clsStdLib.dbl(item.Text);
            //        }
            //    }
            //    _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);

            //}
            #endregion Total


            ROW++;
            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStdLib.dbl(dsMaterialItems.Compute("", "").ToString());
            //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
            //+ clsStdLib.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());

            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2") + " (" + ru.InWord(total, dsOrderMaster.Rows[0]["CurrencyId"].ToString()) + ")");

            #endregion Total


            ROW++;
            #region Total Payable
            //int TotalPayableRow = ROW;
            //int TotalPayableColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[TotalPayableColumn].AddParagraph().AppendText("Total Amount Payable");
            //_TROW.Cells[TotalPayableColumn + 1].AddParagraph().AppendText("Need To Discuss");

            #endregion Total Payable


            ROW++;

            //for (int R = 1; R < wTable.Rows.Count; R++)
            //{
            //    WTableRow TROW = wTable.Rows[R];



            //    foreach (WParagraph item in TROW.Cells[colQty].Paragraphs)
            //    {
            //        item.ApplyStyle("MyStyleRightAlign");
            //    }

            //}
            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            //myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                //TROW.Cells[0].Width = 20;
                //if (dv.Count < 3)
                //    TROW.Cells[0].Width = 120 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            //for (int i = 0; i < dv.Count; i++)
            //    wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            //for (int i = 0; i <= colTotalTaxableAmount; i++)
            //    wTable.ApplyVerticalMerge(i, ROW - 1, ROW);




            IWParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
            style.CharacterFormat.Bold = true;
            style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section

            #endregion merging section

            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);
            return total;
        }
        


        // public DataTable loadMaterialTax(string GatePassId)
        // {
        //     string strSQL;

        //     try
        //     {
        //         strSQL = @"select InventoryServiceId,PO.Id PurchaseOrderId,POD.Id PurchaseOrderDetailId,tg.Code AS TaxCode,PODT.Percentage, PODT.TaxAmount from TRN.PurchaseOrder PO
        //                     INNER JOIN TRN.PurchaseOrderDetail POD ON POD.InventoryReceiveId = PO.Id
        //                     Inner join TRN.PurchaseOrderTax PODT ON PODT.InventoryReceiveId = PO.Id and PODT.InventoryReceiveDetailId = POD.Id
        //                     LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=PODT.TaxCategoryId
        //                     WHERE PO.Id='" + purchaseOrderId + @"' 
        //and InventoryReceiveDetailId  is not null and  InventoryServiceId is null 
        //ORDER BY tg.[Sequence] ";


        //         return _sqlRepository.GetDataTable(strSQL);

        //     }
        //     catch (Exception ex)
        //     {
        //         throw (ex);
        //     }
        //     finally
        //     {

        //     }
        // }





        //public double makeServiceDetailsTable(WordDocument document, DataTable dsServiceItems, string GatePassId)
        //{
        //    string replaceString = "{ServiceItems}";
        //    ReportUtility ru = new ReportUtility();

        //    DataTable dsTax;
        //    //clsDataContext data = new clsDataContext();

        //    IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign");
        //    //Sets the formatting of the style
        //    rightAlign.CharacterFormat.FontSize = 8f;
        //   // rightAlign.CharacterFormat.TextColor = Color.Black;
        //    rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;


        //  //  dsTax = loadServiceMasterTax(purchaseOrderId);

        //    int LasColumnIndex = 1;
        //    Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
        //    //DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));

        //    //LasColumnIndex++;
        //    //dicTaxes.Add("totaltax", LasColumnIndex);
        //    //if (dv.Count > 0)
        //    //{
        //    //    for (int i = 0; i < dv.Count; i++)
        //    //    {
        //    //        LasColumnIndex++;
        //    //        dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
        //    //        LasColumnIndex++;
        //    //    }
        //    //}


        //    WTable wTable = new WTable(document);
        //    wTable.TableFormat.Borders.LineWidth = 1;
        //    wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
        //    int ROW = 0; int COL = 0;
        //    wTable.ResetCells(1, LasColumnIndex + 1);

        //    WTableRow TemplateRow = wTable.Rows[0].Clone();

        //    #region column headers
        //    document.EnsureMinimal();

        //    WCharacterFormat FontBold = new WCharacterFormat(document);
        //    FontBold.Bold = true;

        //    IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Services");
        //    int colServiceName = COL; //COL++;           
        //    range.ApplyCharacterFormat(FontBold);




        //    //int colTotalTaxableAmount = COL;
        //    //if (dv.Count > 0)
        //    //{
        //    //    COL++;
        //    //    colTotalTaxableAmount = COL;
        //    //    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
        //    //    range.ApplyCharacterFormat(FontBold);

        //    //    //COL++;
        //    //    for (int i = 0; i < dv.Count; i++)
        //    //    {
        //    //        //two columns required for tax
        //    //        COL++;
        //    //        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
        //    //        range.ApplyCharacterFormat(FontBold);

        //    //        COL++;
        //    //        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
        //    //        range.ApplyCharacterFormat(FontBold);

        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    COL++;
        //    //    colTotalTaxableAmount = COL;
        //    //    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
        //    //    range.ApplyCharacterFormat(FontBold);

        //    //}


        //    wTable.Rows.Add(TemplateRow);
        //    ROW++;

        //    //if (dv.Count > 0)
        //    //{
        //    //    for (int i = 0; i < dv.Count; i++)
        //    //    {

        //    //        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate");
        //    //        range.ApplyCharacterFormat(FontBold);
        //    //        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
        //    //        range.ApplyCharacterFormat(FontBold);

        //    //    }
        //    //}
        //    #endregion column headers
        //    double totalValue = 0;
        //    int startRow = ROW + 1;
        //    for (int i = 0; i < dsServiceItems.Rows.Count; i++)
        //    {
        //        ROW++;
        //        wTable.AddRow();
        //        WTableRow TROW = wTable.LastRow;

        //        // WTableRow TROW = wTable.Rows[1].Clone();
        //        for (int CE = 0; CE < TROW.Cells.Count; CE++)
        //        {
        //            foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
        //            {
        //                item.Text = "";
        //            }
        //        }
        //        IParagraphItem p = TROW.Cells[colServiceName].AddParagraph().AppendText(dsServiceItems.Rows[i]["Service"].ToString());
        //        //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(dsServiceItems.Rows[i]["Amount"].ToString());
        //        //TROW.Cells[colChar1].AddParagraph().AppendText(dsOrderMaster.Tables[0].Rows[i]["FirstCharacteristicsValue"].ToString());
        //        //TROW.Cells[colChar2].AddParagraph().AppendText(dsOrderMaster.Tables[0].Rows[i]["SecondCharacteristicsValue"].ToString());
        //        //TROW.Cells[colChar3].AddParagraph().AppendText(dsOrderMaster.Tables[0].Rows[i]["ThirdCharacteristicsValue"].ToString());
        //        //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsServiceItems.Rows[i]["Amount"].ToString()).ToString("#,##0.00"));

        //        //TROW.Cells[colRate].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Tables[0].Rows[i]["TransactionRate"].ToString()).ToString("F2"));
        //        //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Tables[0].Rows[i]["TrnAmount"].ToString()).ToString("F2"));

        //        totalValue += clsStdLib.dbl(dsServiceItems.Rows[i]["Amount"].ToString());

        //        //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(totalValue.ToString("F2"));

        //        //if (dv.Count > 0)
        //        //{
        //        //    //dsTax.Tables[0].DefaultView.RowFilter = "MasterOrderItemId='" + dsOrderItems.Tables[0].Rows[i]["MasterOrderItemId"].ToString() + "'";
        //        //    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
        //        //    //double totalTax = 0;

        //        //    for (int T = 0; T < dv.Count; T++)
        //        //    {
        //        //        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND InventoryServiceId='" + dsServiceItems.Rows[i]["ServiceId"] + "'";
        //        //        if (dvtax.Count > 0)
        //        //        {
        //        //            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));
        //        //            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("#,##0.00"));
        //        //        }
        //        //    }
        //        //}
        //    }

        //    ROW++;
        //    #region Total
        //    int TotalRow = ROW;
        //    wTable.AddRow();
        //    WTableRow _TROW = wTable.LastRow;
        //    _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);

        //    for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
        //    {
        //        if (dicTaxes.ContainsValue(C))
        //            continue;

        //        double value = 0;
        //        for (int i = startRow; i < TotalRow; i++)
        //        {

        //            foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
        //            {
        //                value += clsStdLib.dbl(item.Text);
        //            }
        //        }
        //        _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);

        //    }
        //    #endregion Total


        //    ROW++;
        //    #region Sub Total
        //    //int SubTotalRow = ROW;
        //    //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
        //    //wTable.AddRow();
        //    //_TROW = wTable.LastRow;

        //    //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

        //    //double total = clsStdLib.dbl(dsServiceItems.Compute("SUM(Amount)", "").ToString())
        //        //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
        //       // + clsStdLib.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());

        //    //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2") + " (" + ru.InWord(total, dsOrderMaster.Rows[0]["CurrencyId"].ToString()) + ")");

        //    #endregion Total


        //    ROW++;
        //    #region Total Payable
        //    //int TotalPayableRow = ROW;
        //    //int TotalPayableColumn = 0;//_TROW.Cells.Count - 5;
        //    //wTable.AddRow();
        //    //_TROW = wTable.LastRow;

        //    //_TROW.Cells[TotalPayableColumn].AddParagraph().AppendText("Total Amount Payable");
        //    //_TROW.Cells[TotalPayableColumn + 1].AddParagraph().AppendText("Need To Discuss");

        //    #endregion Total Payable


        //    ROW++;


        //    #region paragrpath formats
        //    //Adds a new paragraph style named "MyStyle"
        //    IWParagraphStyle myStyle2 = document.AddParagraphStyle("MyStyle2");
        //    //Sets the formatting of the style
        //    myStyle2.CharacterFormat.FontSize = 8f;
        //    //myStyle2.CharacterFormat.TextColor = Color.Black;
        //    myStyle2.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

        //    for (int R = 0; R < wTable.Rows.Count; R++)
        //    {
        //        WTableRow TROW = wTable.Rows[R];
        //        //TROW.Cells[0].Width = 20;
        //        //if (dv.Count < 3)
        //        //    TROW.Cells[0].Width = 120 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

        //        for (int CE = 0; CE < TROW.Cells.Count; CE++)
        //        {
        //            foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
        //            {
        //                item.ApplyStyle("MyStyle2");
        //            }
        //        }
        //    }


        //    #endregion paragrpath formats


        //    #region merging section


        //    //tax codes merging (horizontal)
        //    ROW = 0;
        //   // for (int i = 0; i < dv.Count; i++)
        //   //     wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

        //   // //primary cells merging (veritcal)
        //   // ROW++;
        //   //// for (int i = 0; i <= colTotalTaxableAmount; i++)
        //   //     wTable.ApplyVerticalMerge(i, ROW - 1, ROW);




        //    IWParagraphStyle style2 = document.AddParagraphStyle("SubTotalStyle2");
        //    style2.CharacterFormat.Bold = true;
        //    style2.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
        //    //Adds new paragraph to the section


        //    //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
        //    //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
        //    //        PARA.ApplyStyle("SubTotalStyle2");

        //    //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
        //    #endregion merging section



        //    TextBodyPart textBodyPart = new TextBodyPart(document);
        //    textBodyPart.BodyItems.Add(wTable);
        //    document.Replace(replaceString, textBodyPart, true, true);
        //    //return total;
        //}

        //private DataTable InOutGatePassDetail(string GatePassId)
        //{
        //    try
        //    {
        //        string sqlText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + requistionId + @"'

        //                   SELECT IM.Id AS MDetailId
        //                     , IM.MaterialReqqusitionMasterId 
        //               	     , REPLACE(CONVERT(CHAR(11),IM.DeliveryDate, 106),' ','-') AS RequiredDate
        //                    , MGM.UserName AS MaterialGroupMasterName
        //                    , IM.MaterialMasterId, MM.UserName MaterialName
        //                    , IM.ArticleId, ART.StandardName Article
        //                    , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
        //                    , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
        //                    , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
        //                    , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
        //                    , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
        //                    , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
        //                    , ROUND(IM.TransactionQty,2) TransactionQty
        //                 , IM.TransactionUoMId
        //                     ,IM.BudgetType 
        //                 , TUoM.UserName AS TransactionUoM
        //                 , ROUND(IM.EstimatedRate,2) TransactionRate 
        //                 , CU.Code AS CurrencyName
        //                  , CU.Id AS CurrencyId
        //                    , ROUND((IM.TransactionQty*IM.EstimatedRate),2) AS TrnAmount   
        //                    ,IM.MaterialDetail
        //                    ,Replace(CONVERT(VARCHAR(11), IM.DeliveryDate, 106), ' ', '-') DeliveryDate

        //                 ,Act.Id As Activity
        //                 ,Act.UserName As ActivityName
        //                 ,IM.OwnStock
        //                    ,IM.OtherStock 
        //                 ,IM.Reason
        //                 ,IM.Remarks
        //                 ,IM.FutureReqApp
        //                 --,BudgetMasterId
        //                 --,GLGeneralInfoId
        //                FROM TRN.MaterialRequsitionDetails AS IM
        //                LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
        //                LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
        //                LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
        //                LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
        //                LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
        //                LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
        //                LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
        //                LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
        //                LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id

        //                LEft JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IM.TransactionUoMId=TUoM.Id
        //                LEFT JOIN [TRN].[MaterialRequsitionMaster] AS IR ON IM.MaterialReqqusitionMasterId=IR.Id
        //                LEFT JOIN [SCS].[Currency] AS CU ON IM.CurrencyId=CU.Id 
        //                LEFT JOIN [HKP].[Activity] As Act On ACT.Id=IM.ActivityId
        //                --JOIN [HKP].Budget
        //                --JOIN [HKP].Gl
        //                WHERE IM.MaterialReqqusitionMasterId=@inventoryReceiveId";

        //        return _sqlRepository.GetDataTable(sqlText);
        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;
        //    }
        //}

        class clsStdLib
        {
            public static string passWord = "prodDisplay";
            public clsStdLib()
            {

            }
            public enum mType
            {
                Error,
                Success,
                Information
            }
            public static bool passwordGet = true;
            public static string[] sMonth = new string[] { "<Unselect>", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

            public static string DataRankNames(int dayNo)
            {

                if (dayNo <= 0)
                    return "";

                if (dayNo.ToString().Length > 1)
                {
                    string Right = dayNo.ToString().Substring(dayNo.ToString().Length - 2, 2);
                    if (clsStdLib.dbl(Right) >= 10 && clsStdLib.dbl(Right) <= 20)
                        return dayNo + "th";
                }

                string RightString = dayNo.ToString().Substring(dayNo.ToString().Length - 1, 1);
                switch (RightString)
                {
                    case "1":
                        return dayNo + "st";
                    case "2":
                        return dayNo + "nd";
                    case "3":
                        return dayNo + "rd";
                    default:
                        return dayNo + "th";

                }




            }

            #region date related
            public static readonly string dateFormat = "dd-MMM-yyyy";
            public static readonly string sqliteDateFormat = "yyyy-MM-dd";
            public static readonly string AppToDBdateFormat = "yyyy-MM-dd hh:mm:ss";
            public static bool IsDateOK(string strdate)
            {
                try
                {
                    if (strdate.Length != 11)
                    {
                        return false;
                    }
                    if (strdate.Substring(2, 1) != "-" && strdate.Substring(6, 1) != "-")
                    {
                        return false;
                    }
                    System.DateTime myDt = System.Convert.ToDateTime(strdate);
                    return true;
                }
                catch (System.Exception ex)
                {
                    return false;
                }
                finally
                {
                    //
                }
            }// end function
            private static bool DateOkCheck(string strdate)
            {
                try
                {
                    System.DateTime myDt = System.Convert.ToDateTime(strdate);
                    return true;
                }
                catch (System.Exception ex)
                {
                    return false;
                }
                finally
                {
                    //
                }
            }// end function
            public static object chk_NullDateData(object dateValue)
            {
                if (DateOkCheck("" + dateValue.ToString()) == false)
                {
                    dateValue = "";
                }

                if (("" + dateValue.ToString()) == "")
                {
                    System.DateTime dt = new System.DateTime(1901, 1, 1);
                    dateValue = (object)dt;
                }
                return (object)dateValue;
            }
            public static System.DateTime AppDateConvert(object dateValue, string input_date_format, string output_date_format)
            {
                string strDate = null;
                dateValue = chk_NullDateData(dateValue);
                strDate = dateValue.ToString();
                if (strDate != "")
                {
                    if (input_date_format.Trim() != "")
                    {
                        if (output_date_format.Trim() != "")
                        {
                            System.Globalization.DateTimeFormatInfo InputFormat = new System.Globalization.DateTimeFormatInfo();
                            InputFormat.ShortDatePattern = input_date_format;
                            System.DateTime myDt = System.Convert.ToDateTime(strDate, InputFormat);
                            strDate = myDt.ToString(output_date_format);
                        }
                    }
                }
                return System.Convert.ToDateTime(strDate);
            }// End of function
            public static Object DateData_AppToDB(object dateValue, string DB_Level_date_format)
            {
                if (string.IsNullOrEmpty((string)dateValue))
                    return DBNull.Value;

                string strDate = null;
                strDate = dateValue.ToString();
                if (DB_Level_date_format != "")
                {
                    // Collecting the user terminal set format 
                    System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
                    strDate = AppDateConvert(strDate, USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString(), DB_Level_date_format).ToString();
                }

                string m = System.Convert.ToDateTime(strDate).ToString(AppToDBdateFormat);
                return System.Convert.ToDateTime(strDate).ToString(AppToDBdateFormat);


            }// End of function
            public static System.DateTime DateData_DBToApp(object dateValue)
            {
                string strDate = null;
                strDate = dateValue.ToString();

                System.Globalization.DateTimeFormatInfo myDBDateFormat = new System.Globalization.CultureInfo("en-US", false).DateTimeFormat;
                strDate = DateData_DBToApp(dateValue, myDBDateFormat.ShortDatePattern.ToString()).ToString();
                return System.Convert.ToDateTime(strDate);
            }// End function
            public static System.DateTime DateData_DBToApp(object dateValue, string DB_Level_date_format)
            {
                string strDate = null;
                strDate = dateValue.ToString();
                if (DB_Level_date_format != "")
                {
                    // Collecting the user terminal set format 
                    System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
                    strDate = AppDateConvert(strDate, DB_Level_date_format, USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString()).ToString();
                }
                return System.Convert.ToDateTime(strDate);
            }// End of function
            public static String makeBaseBlank(object dateValue)
            {
                System.DateTime dt;
                dt = System.Convert.ToDateTime(dateValue.ToString());
                if (dt.Year == 1901)
                {
                    return "";
                }
                else
                {
                    return dateValue.ToString();
                }
            }// End of function
            ///<summary>
            ///return day difference in integer. 
            ///    Example 1: firstDate[Less Than]lastDate returns positive value
            ///    Example 2: firstDate>lastDate returns negative value
            ///    Example 3: firstDate=lastDate returns 0 [zero]**/
            /// </summary>
            public static int dateDiff(string firstDate, string lastDate)
            {

                int difference = 0;
                try
                {
                    firstDate = Convert.ToDateTime(firstDate).ToString("dd-MMM-yyyy");
                    lastDate = Convert.ToDateTime(lastDate).ToString("dd-MMM-yyyy");

                    if (IsDateOK(firstDate) == false)
                    {
                        Exception ex = new Exception("Invalid [First Date]");
                        throw (ex);
                    }
                    if (IsDateOK(lastDate) == false)
                    {
                        Exception ex = new Exception("Invalid [Last Date]");
                        throw (ex);
                    }
                    DateTime dateFirstDate = Convert.ToDateTime(firstDate);
                    DateTime dateLastDate = Convert.ToDateTime(lastDate);
                    TimeSpan TimeSpan = dateLastDate.Subtract(dateFirstDate);


                    difference = TimeSpan.Days;
                }
                catch (Exception ex)
                {
                    throw (ex);
                }

                return difference;
            }



            public static string getSqliteDate(string standardDate)
            {
                return (Convert.ToDateTime(standardDate).ToString(sqliteDateFormat));
            }
            public static string getStandardDateFromSqliteDate(string SqliteDate)
            {
                if (SqliteDate.Length != 10)
                    return "";
                if (SqliteDate.Split('-').Length != 3)
                    return "";
                //many things to validate 
                //but i have less time :)
                string month = ValidLength(sMonth[Convert.ToInt32(SqliteDate.Split('-')[1])], 3).ToString();


                return SqliteDate.Split('-')[2] + "-" + month + "-" + SqliteDate.Split('-')[0];
            }
            #endregion date related

            #region numeric
            public static bool IsNumeric(string strNumber)
            {
                Double d;
                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
                if (strNumber.Length == 0)
                {
                    return false;
                }
                return Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d);
            } // End Function
            public static string GetNumericData(string strNumber)
            {
                double d;
                strNumber = strNumber.Replace(",", "");
                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
                if (strNumber.Trim() == "")
                { return "0"; }
                else if (System.Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
                {
                    return strNumber;
                }
                else
                {
                    return "0";
                }
            }// end function
            public static string GetNumericDataInDecimalFormat(string strNumber, int precision)
            {
                if (precision < 1)
                    return strNumber;

                string s_precision = new String('0', precision);

                double d;
                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
                if (strNumber.Trim() == "")
                { return "0." + s_precision; }
                else if (System.Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
                {
                    return string.Format("{0:0." + s_precision + "}", d);
                }
                else
                {
                    return "0." + s_precision;
                }
            }// end function
            public static double dbl(string d)
            {
                return Convert.ToDouble(GetNumericData(d));

            }
            public static int Percentage(int total, double percentage)
            {
                return (int)(total * (percentage / 100));

            }
            //validation
            public static void numericValidation(string value, bool isMandatory, bool isInteger, bool negativeAllowed, string fieldName)
            {

                try
                {



                    if (isMandatory == true)
                    {
                        if (value.Trim() == "")
                        {
                            Exception ex = new Exception("please insert [" + fieldName + "]");
                            throw (ex);
                        }
                        if (Convert.ToDouble(GetNumericData(value.Trim())) == 0)
                        {
                            Exception ex = new Exception("please insert [" + fieldName + "]");
                            throw (ex);
                        }

                        if (value.Trim() != "")
                        {
                            if (IsNumeric(value.Trim()) == false)
                            {
                                Exception ex = new Exception("Invalid numeric value [" + value + "] for the field [" + fieldName + "]");
                                throw (ex);
                            }
                        }
                    }

                    if (value.Trim() != "")
                    {
                        if (IsNumeric(value.Trim()) == false)
                        {
                            Exception ex = new Exception("Invalid numeric value [" + value + "] for the field [" + fieldName + "]");
                            throw (ex);
                        }
                        if (isInteger == true)
                        {

                            if (isInt(value.Trim()) == false)
                            {
                                Exception ex = new Exception("Number must be integer for the field [" + fieldName + "]");
                                throw (ex);
                            }

                        }
                        if (negativeAllowed == false)
                        {
                            if (Convert.ToDouble(GetNumericData(value.Trim())) < 0)
                            {
                                Exception ex = new Exception("Negative values are not allowed for the field [" + fieldName + "]");
                                throw (ex);
                            }
                        }
                    }



                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {

                }


            }

            ///<summary>
            ///check whether a value is integer or not returns true if integer, 
            ///false if floating or string containing alpahnumeric
            ///</summary>
            public static bool isInt(string num)
            {

                bool isInt;
                int number;
                try
                {
                    isInt = System.Int32.TryParse(num, out number);
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {

                }
                return isInt;
            }


            #endregion numeric

            #region string

            public static readonly string excelNegativePOsitiveSign = @"+#,##0.00;-#,##0.00;* ??;@";
            public static readonly string NegativePOsitiveSign = @"+#,##0.00;-#,##0.00;0";
            public static readonly string NumberFormatString = "#,##0.000;(#,##0.000);* ??;@";
            public static readonly string NumberFormatStringFourDecimal = "#,##0.0000;(#,##0.0000);* ??;@";
            public static readonly string NumberFormatStringFiveDecimal = "#,##0.00000;(#,##0.00000);* ??;@";
            public static readonly string NumberFormatStringTwoDecimal = "#,##0.00;(#,##0.00);* ??;@";
            public static readonly string NumberFormatStringTwoDecimalWithZero = "#,##0.00;(#,##0.00)";
            public static readonly string NumberFormatStringInteger = "#,##0;(#,##0);* ??;@";
            public static readonly string NumberFormatStringIntegerWithZero = "#,##0;(#,##0)";
            public static readonly string NumberFormatStringText = "@"; //format cell data as text


            public static object ValidLength(string str)
            {

                string removechar = "";
                if (str.Trim() == "")
                {
                    return (object)Convert.DBNull;
                }
                removechar = str.Trim();
                removechar = removechar.Replace("'", " ");

                return (object)removechar.Trim();

            }
            public static object ValidLength(string str, int length)
            {

                string removechar = "";
                if (str.Trim() == "")
                {
                    return (object)Convert.DBNull;
                }
                removechar = str.Trim();
                removechar = removechar.Replace("'", " ");


                int strLen = removechar.Length;
                if (strLen > length)
                    removechar = removechar.Substring(0, length);

                return (object)removechar.Trim();

            }
            public static string FileNameLegalChar(string fileName)
            {
                string illegalChar = @"~`!@#$%^&*=/\|>,<";
                foreach (char c in illegalChar)
                {
                    fileName = fileName.Replace(c.ToString(), " ");
                }

                return fileName;
            }
            private StringCollection getTableColumns(ref DataSet dsLocal)
            {
                StringCollection strcol = new StringCollection();
                for (int COL = 0; COL < dsLocal.Tables[0].Columns.Count; COL++)
                {
                    strcol.Add(dsLocal.Tables[0].Columns[COL].ColumnName.ToUpper());
                }

                return strcol;

            }
            public static string emptyString(string str)
            {
                //this function returns an empty string(not a null) from null or empty or '&nbsp;' from the page
                if (str == "&nbsp;")
                    str = "";
                if (string.IsNullOrEmpty(str) == true)
                    str = "";


                return str;
            }//this function returns an empty string(not a null) from null or empty '&nbsp;' from the page
            #endregion string


            #region others
            //public void copyDataset(DataSet source, ref DataSet destination)
            //{
            //    //StringCollection strColDestinationColumns = getTableColumns(ref destination);//upper case
            //    DataRow drLocal = null;
            //    for (int ROW = 0; ROW < source.Tables[0].Rows.Count; ROW++)
            //    {
            //        drLocal = destination.Tables[0].NewRow();
            //        for (int COL = 0; COL < source.Tables[0].Columns.Count; COL++)
            //        {
            //            if (strColDestinationColumns.Contains(source.Tables[0].Columns[COL].ToString().ToUpper()))
            //            {
            //                drLocal[source.Tables[0].Columns[COL].ToString()] = ValidLength(source.Tables[0].Rows[ROW][source.Tables[0].Columns[COL].ToString()].ToString());
            //            }
            //        }
            //        destination.Tables[0].Rows.Add(drLocal);
            //    }


            //}
            public static string GetxlsCol(int intCol)
            {
                //returns excel columns based on column number. tested 1 to 256 column numbers
                try
                {
                    if (intCol < 1 || intCol > 256)
                    {
                        System.Exception ex = new Exception("Invalid Column Value");
                        throw (ex);
                    }
                    intCol = intCol - 1;
                    int intFirstLetter = ((intCol) / 512) + 64;
                    int intSecondLetter = ((intCol % 512) / 26) + 64;
                    int intThirdLetter = (intCol % 26) + 65;
                    char FirstLetter;
                    char SecondLetter;
                    if (intFirstLetter > 64)
                        FirstLetter = (char)intFirstLetter;
                    else
                        FirstLetter = ' ';

                    if (intSecondLetter > 64)
                        SecondLetter = (char)intSecondLetter;
                    else
                        SecondLetter = ' ';

                    char ThirdLetter = (char)intThirdLetter;
                    return string.Concat(FirstLetter, SecondLetter, ThirdLetter).Trim();
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {

                }
            }//returns excel columns based on column number. tested 1 to 256 column numbers
            #endregion others

            public static object RetValidLen(string Data)
            {
                if (string.IsNullOrEmpty(Data))
                    return DBNull.Value;

                return Data;
            }
            public static double sum(string columnName, DataTable dtLocal, string criteria)
            {
                double total = 0;
                DataRow[] dr = dtLocal.Select(criteria);
                foreach (DataRow d in dr)
                {
                    total += dbl(d[columnName].ToString());
                }


                return total;
            }
        }
        #endregion





        #region Indivisul Gate Pass Report
        public IWordDocument IndivisualGatePassTeamplateReport(string companyGroupId, string plantId, string GatePassId)
        {

            ReportUtility ru = new ReportUtility();
            var fileName = "";
            var strPath = "";
            var File = "";
            fileName = "IndivisualGatePass" + plantId + ".docx";
            strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }

            ////A opens input document.
            WordDocument document = new WordDocument(File, FormatType.Docx);

            //Gets the paragraph at index 1
            try
            {
                string invoicePartyAddress = "";
                string vendorPartyAddress = "";
                WSection section = document.Sections[0];

                DataTable dsOrderMaster, dsServiceItems;
                dsOrderMaster = loadIndivisulGatePassMaster(GatePassId);//sql
                Dictionary<string, string> columns = new Dictionary<string, string>();
                var poApprovedStatus = "";
                //invoicePartyAddress = ru.GetAddress(dsOrderMaster.Rows[0]["InvoicePartyAddressMasterId"].ToString(), dsOrderMaster.Rows[0]["InvoicingByAddress"].ToString());
                document.Replace("{InvoicingPartyAddress}", invoicePartyAddress, false, false);
                //vendorPartyAddress = ru.GetAddress(dsOrderMaster.Rows[0]["VendorAddressMasterId"].ToString(), "");
                document.Replace("{VendorAddress}", vendorPartyAddress, false, false);
                //document.Replace("{DeliveryInstruction}", dsOrderMaster.Rows[0]["DeliveryInstruction"].ToString(), false, false);
                //document.Replace("{SpecialInstruction}", dsOrderMaster.Rows[0]["SpecialInstruction"].ToString(), false, false);
                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);
                // dsServiceItems = loadServicerMasterItems(purchaseOrderId);
                var materialTotal = makeloadIndivisulGatePassMasterGatePassDetailTable(document, dsOrderMaster, GatePassId);//Material Details 
                var serviceTotal = 0.00;
				//if (dsServiceItems.Rows.Count > 0)
				//{
				//    //{ServiceItems}
				//    serviceTotal = makeServiceDetailsTable(document, dsServiceItems, purchaseOrderId);//Service Details 
				//    document.Replace("{ServiceDetails}", "Service Details", true, true);
				//}
				document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("#,##0.00") + " " + "", true, true);//dsOrderMaster.Rows[0]["CurrencyName"].ToString()
                document.Replace("{TotalInWords}", ru.InWord((materialTotal + serviceTotal), ""), true, true);
				Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();
                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                //creating secondary array to prevent memory leak and accidental over-writing (Tarek Talukder-26-May-2019)
                List<string> strReplace = new List<string>();
                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());
                StringCollection strColDistinct = new StringCollection();
                for (int i = 0; i < strReplace.Count; i++)
                {
                    if (strColDistinct.Contains(strReplace[i].ToUpper()))
                        continue;

                    strColDistinct.Add(strReplace[i].ToUpper());

                    string text = strReplace[i].ToUpper();
                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        ReplaceInfo[text] = document.Replace(text, dsOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }

                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);
                //removing any unused place holder
                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "", false, false);

                }

                //document.Save("tarek.docx",FormatType.Docx, System.Web.HttpContext.Current.Response,HttpContentDisposition.InBrowser);

                DocToPDFConverter converter = new DocToPDFConverter();
                //Converts Word document into PDF document
                //Syncfusion.Pdf.PdfDocument pdfDocument = converter.ConvertToPDF(document);
                DocToPDFConverterSettings settings = new DocToPDFConverterSettings();
                settings.AutoDetectComplexScript = true;
                settings.UpdateDocumentFields = true;
                converter.Settings = settings;


                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                pdfDocument.PageSettings.Width = 1200;
                pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();
                //Closes the instance of document objects
                document.Close();
                string Prefix = "InOutGatePass" + GatePassId;
                //Saves the PDF file 
                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);
                document.Close();

                return document;
            }

            catch (Exception ex)
            {
                throw ex;
            }
            //Closes the instance of document objects


        }


        public DataTable loadIndivisulGatePassMaster(string GatePassId)
        {
            string strSQL;
            //clsConnection objCon;
            try
            {
                strSQL = @"SELECT                  
								IR.[Id] Id--GapassMId
								,IR.[CompanyGroupId]
								--,IR.[CompanyId]
								,IR.[PlantId]
								,IR.[GatePassType]
								,IR.[GatePassStatus]                             
								,REPLACE(CONVERT(CHAR(11), IR.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
								,IR.[FromEmployeeId]
								,EI.EmployeeName SenderName
								,IR.[Through]
								,IR.[CourierName]
                                ,IR.[OtherCompanyName]
						        ,IR.[PersonName]
                                ,IR.[MobileNo]
                                ,IR.[Address]
								,IR.[RunnerEmployeeId]
								,EI1.EmployeeName RunnerEmployee                              
								,IR.[Remarks] Remarks
								    ,CheckedBy=CASE WHEN IR.CheckedByStatus='Checked' Then CheckedBy.EmployeeName else '' END
                            ,Approvedby=CASE When IR.ApprovedByStatus='Approved'then ApprvedBy.EmployeeName else '' END
							,GateOutBy.EmployeeName SSEmployeeName
                             --,AddedBy=CASE 
									--When IR.CheckedByStatus='ForChecked' Then CheckedBy.EmployeeName
									--When IR.CheckedByStatus='Hold' Then CheckedBy.EmployeeName
									--When IR.CheckedByStatus='Reject' Then CheckedBy.EmployeeName
									--When IR.CheckedByStatus='Checked' Then CheckedBy.EmployeeName
									--When IR.CheckedByStatus IS NULL then IR.AddedBy 
									
									--else ''
									--END
                                ,AddedBy=EI.EmployeeName
								,IR.[CheckedHoldRejectReason]
								,IR.[ApprovedByStatus]
								,IR.[ApprovedHoldRejectReason] 
	                            ,MT.UserName MaterialType
								,IM.Id GapassDId
                    	,MM.UserName MaterialMasterName
                        ,IR.Id AS GatePassMasterId
                        , MGM.UserName AS MaterialGroupMasterName
                        , ART.StandardName ArticleName
                        , IM.MaterialMasterId, MM.UserName
                        , IM.ArticleId, ART.StandardName
                        , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                        , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                        , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                        , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                        , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                        , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
                        , ROUND(IM.TransactionQty,2) TransactionQty
                        , IM.TransactionUoMId
                        ,Replace(CONVERT(VARCHAR(11), IM.ReturnableDate, 106), ' ', '-') ReturnableDate
                        , TUoM.UserName AS TransactionUoM                       
                        ,IM.MaterialDetail  
						,B.UserName BuyerName
                        ,P.UserName OtherPlant
                        ,IsReturnable = CASE WHen IM.IsReturnable=1 Then 'Yes' Else 'No' End
						,IsMutilated= CASE When IM.IsMutilated=1 Then 'Yes' ELSE 'No' END    
                        ,PC.UserName PartyName,Isnull(IM.Rate,0) Rate,TotalAmount=Isnull(IM.Rate,0)*ROUND(IM.TransactionQty,2)
                        ,AM.Address1 PartyAddress1 
						,AM.Address2 PartyAddress2
						,AM.Address3 PartyAddress3
                        ,GateRegisterType=CASE WHEN IR.GateRegisterType='Out' THEN 'OUT GATE PASS / DELIVERY CHALLAN' ELSE 'IN GATE PASS' END
                        ,IR.PurposeofGatePass,IR.ConsignmentNo,IR.DriverName,IR.InvoiceNo,IR.InvoiceValue,IR.TransportAgentName,IR.TransportAgentMobileNo,IR.VehicleNo,IR.NoofPackages,PP.GSTIN
                        FROM TRN.GatePassDetails AS IM
                        LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IM.TransactionUoMId=TUoM.Id
                        LEFT JOIN [TRN].[GatePassMaster] AS IR ON IM.GatePassMasterId=IR.Id 
						LEFT JOIN Employeeinformation EI on EI.SystemId= IR.FromEmployeeId
						LEFT JOIN Employeeinformation EI1 on EI1.SystemId= IR.RunnerEmployeeId  
	                    LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
						LEFT JOIN Employeeinformation CheckedBy on CheckedBy.SystemId= IR.CheckedBy
						LEFT JOIN Employeeinformation ApprvedBy on ApprvedBy.SystemId= IR.ApprovedBy  
						LEFT JOIN Employeeinformation GateOutBy on GateOutBy.SystemId= IR.SenderSecurityEmployeeId 
                        LEFT Join [HKP].[Buyer] B ON B.Id=IR.ToBuyerId
                        LEFT JOIN [ORG].[Plant] P ON P.Id=IR.ToPlantId
                        Left JOIN HKP.Party PC ON PC.Id=IR.ToPartyCode
                        left JOIN [MST].[AddressMaster] AM ON AM.Id=pC.AddressMasterId
                         LEFT JOIN hkp.PartyPlant PP ON PP.AddressMasterId=AM.Id
                    Where IR.Id='" + GatePassId + @"'Order By IR.[Id] DESC";

                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        public double makeloadIndivisulGatePassMasterGatePassDetailTable(WordDocument document, DataTable dsMaterialItems, string GatePassId)
        {
            string replaceString = "{materialItems}";
            ReportUtility ru = new ReportUtility();

            int LasColumnIndex = 11;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.IsBreakAcrossPages = true;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();
            //wTable.Title = "Material Details";
            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SL");
            int colRo = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Material");
            range.ApplyCharacterFormat(FontBold);
            int colMaterial = COL; COL++;
            wTable.Rows[ROW].Cells[colMaterial].Width = 120;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article ");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            wTable.Rows[ROW].Cells[colArticle].Width = 100;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU1");
            range.ApplyCharacterFormat(FontBold);
            int colChar1 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar1].Width = 60;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU2");
            range.ApplyCharacterFormat(FontBold);
            int colChar2 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar2].Width = 60;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU3");
            //range.ApplyCharacterFormat(FontBold);
            //int colChar3 = COL; COL++;
            //wTable.Rows[ROW].Cells[colChar3].Width = 60;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Returnable Date");
            range.ApplyCharacterFormat(FontBold);
            int colReturnableDate = COL; COL++;
            wTable.Rows[ROW].Cells[colReturnableDate].Width = 70;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;
            wTable.Rows[ROW].Cells[colQty].Width = 60;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UoM");
            range.ApplyCharacterFormat(FontBold);
            int colUoM = COL; COL++;
            wTable.Rows[ROW].Cells[colUoM].Width = 60;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate");
            range.ApplyCharacterFormat(FontBold);
            int colRate = COL; COL++;
            wTable.Rows[ROW].Cells[colQty].Width = 60;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
            range.ApplyCharacterFormat(FontBold);
            int colTotalAmount = COL; COL++;
            wTable.Rows[ROW].Cells[colQty].Width = 60;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Remarks");
            //range.ApplyCharacterFormat(FontBold);
            //int colRemarks = COL; COL++;
            //wTable.Rows[ROW].Cells[colRemarks].Width = 120;

            #endregion column headers

            int startRow = ROW;
            int sl = 0;
            for (int i = 0; i < dsMaterialItems.Rows.Count; i++)
            {
                sl++;
                ROW++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                }
           
                TROW.Cells[colRo].AddParagraph().AppendText(sl.ToString());

                
                TROW.Cells[colMaterial].AddParagraph().AppendText(dsMaterialItems.Rows[i]["MaterialMasterName"].ToString());
                TROW.Cells[colArticle].AddParagraph().AppendText(dsMaterialItems.Rows[i]["ArticleName"].ToString());
                TROW.Cells[colChar1].AddParagraph().AppendText(dsMaterialItems.Rows[i]["FirstCharacteristicsValue"].ToString());
                TROW.Cells[colChar2].AddParagraph().AppendText(dsMaterialItems.Rows[i]["SecondCharacteristicsValue"].ToString());
                //TROW.Cells[colChar3].AddParagraph().AppendText(dsMaterialItems.Rows[i]["ThirdCharacteristicsValue"].ToString());
             
                TROW.Cells[colReturnableDate].AddParagraph().AppendText(dsMaterialItems.Rows[i]["ReturnableDate"].ToString());
                TROW.Cells[colQty].AddParagraph().AppendText(clsStdLib.dbl(dsMaterialItems.Rows[i]["TransactionQty"].ToString()).ToString("F2"));
                TROW.Cells[colUoM].AddParagraph().AppendText(dsMaterialItems.Rows[i]["TransactionUoM"].ToString());
                TROW.Cells[colRate].AddParagraph().AppendText(clsStdLib.dbl(dsMaterialItems.Rows[i]["Rate"].ToString()).ToString("F2"));
                TROW.Cells[colTotalAmount].AddParagraph().AppendText(clsStdLib.dbl(dsMaterialItems.Rows[i]["TotalAmount"].ToString()).ToString("F2"));
                
                //TROW.Cells[colRemarks].AddParagraph().AppendText(dsMaterialItems.Rows[i]["Remarks"].ToString());
              
            }

            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);
            double value = 0;
            for (int C = 0; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                value = 0;
                if (C == colRo || C == colMaterial || C == colArticle || C == colChar1 || C == colChar2  || /*C == colMaterialDetail || C == colIsReturnable ||*/ C == colReturnableDate ||  C == colUoM  || C == colRate)
                    continue;


                for (int i = startRow; i <= TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);
                
            }
            #endregion Total


            ROW++;
            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStdLib.dbl(dsMaterialItems.Compute("SUM(TotalAmount)", "").ToString());
            //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
            //+ clsStdLib.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());

            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2") + " (" + ru.InWord(total, dsOrderMaster.Rows[0]["CurrencyId"].ToString()) + ")");

            #endregion Total


            ROW++;
            #region Total Payable
            //int TotalPayableRow = ROW;
            //int TotalPayableColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[TotalPayableColumn].AddParagraph().AppendText("Total Amount Payable");
            //_TROW.Cells[TotalPayableColumn + 1].AddParagraph().AppendText("Need To Discuss");

            #endregion Total Payable


            ROW++;
            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            //myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
             
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle");
                    }
                }
            }


            #endregion paragrpath formats
            #region merging section

            ROW = 0;
       
            ROW++;
         
            IWParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
            style.CharacterFormat.Bold = true;
            style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section

            #endregion merging section

            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);
            return total;
        }


    
        #endregion

    }
}
