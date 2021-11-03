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

#endregion Using

namespace Library.MaterialManagement.InventoryManagements
{
	public class GatePass
	{
		private readonly SqlRepository _sqlRepository;

		#region Constructor
		public GatePass()
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


		public IEnumerable<object> GetPurchaseReturnData(string InventoryReceiveId) 
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";
				sql = @"Select * from trn.PurchaseReturn";
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
