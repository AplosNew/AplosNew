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
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.Collections.Specialized;
using System.Linq;
#endregion Using

namespace Library.MaterialManagement.InventoryManagements
{
	public class InventoryReceiveService
	{
		private readonly SqlRepository _sqlRepository;

		#region Constructor
		public InventoryReceiveService()
		{
			_sqlRepository = new SqlRepository();
		}
		#endregion Constructor


		public IEnumerable<object> GetShortageRejectionValue(string InventoryReceiveId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";
				sql = @"Select IRD.Id InventoryReceiveDetailId
                        , MGM.UserName AS MaterialGroupMasterName
                        , IM.MaterialMasterId, MM.UserName
                        , IM.ArticleId, ART.StandardName
                        , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                        , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                        , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                        , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                        , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                        , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
                        ,IRD.MaterialTranRate TransactionRate,IRD.ShortageQty,IRD.ShortageRatePercent ShortageRate,IRD.RejectionQty,IRD.ShortageValue,IRD.RejectRatePercent RejectionRate,IRD.RejectClamPercent , IRD.RejectValue RejectionValue,IRD.RejectClamPercent RejectionClamRate
                        FROM trn.InventoryReceiveDetail IRD
                        LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id=IRD.InventoryMaterialId
                        LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                        where IRD.InventoryReceiveId='" + InventoryReceiveId + "'";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}


		public IEnumerable<object> GetAssetInventoryIssueNew(string plantId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";
				sql = @"SELECT II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
	                                 ,FORMAT(II.IssueDate,'dd-MMM-yyyy') IssueDate, MS.UserName AS MaterialStorage
									 ,EI.EmployeeCode+' - '+EI.EmployeeName EmployeeName,SUM(IID.TransactionQty) Qty,SUM(IID.PolicyAmount) Amount,II.OrderRefNo
                                FROM [TRN].[InventoryIssue] AS II
                                JOIN TRN.InventoryIssueDetail AS IID ON IID.InventoryIssueId=II.Id AND IID.IsAsset=1
                                JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId=MS.Id
								left join dbo.EmployeeInformation AS EI ON EI.SystemId=II.EmployeeId
                                WHERE II.PlantId='" + plantId + @"' AND ISNULL(II.[Status],'') <>'Posting' AND II.IssueType='Capital'
                                GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
	                                 , II.IssueDate, MS.UserName
									 ,EI.EmployeeCode,EI.EmployeeName,II.OrderRefNo";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}

		public void UpdateShortageRejectionValue(string MasterId, List<Dictionary<string, object>> UserSendData)
		{
			try
			{
				string sql = "select * from trn.InventoryReceiveDetail where InventoryReceiveId='" + MasterId + "'";
				ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
				con.OpenDataSetThroughAdapter(sql, out DataSet dsDetail, false, "1");

				for (int i = 0; i < UserSendData.Count; i++)
				{
					dsDetail.Tables[0].DefaultView.RowFilter = "Id='" + UserSendData[i]["InventoryReceiveDetailId"].ToString() + "'";
					if (dsDetail.Tables[0].DefaultView.Count == 0)
					{
						//genId.GenID("TNA MASTER", out TNAMasterSystemID);
						//TNAMasterSystemID = "TM" + TNAMasterSystemID;
						//DataRow dr = dsMaster.Tables[0].NewRow();
						//dr["Id"] = TNAMasterSystemID;
						//dr[columnname] = TransactionId;
						//dr["TNAAppliedOn"] = ScheduleFor.ToString();
						//dr["AddedBy"] = "Scheduler";
						//dr["AddedDate"] = System.DateTime.Now.ToString();
						//dr["AddedFromIP"] = "";
						//dr["UpdatedBy"] = "Scheduler";
						//dr["UpdatedDate"] = System.DateTime.Now.ToString();
						//dr["UpdatedFromIP"] = "";

						//dsMaster.Tables[0].Rows.Add(dr);
					}
					else
					{
						DataRow dr = dsDetail.Tables[0].DefaultView[0].Row;
						dr.BeginEdit();
						dr["ShortageRatePercent"] = UserSendData[i]["ShortageRate"];
						dr["ShortageValue"] = UserSendData[i]["ShortageValue"];
						dr["RejectRatePercent"] = UserSendData[i]["RejectionRate"];
						dr["RejectValue"] = UserSendData[i]["RejectionValue"];
						dr["RejectClamPercent"] = UserSendData[i]["RejectionClamRate"];
						dr.EndEdit();
					}
				}

				clsStaticInfo info = new clsStaticInfo();
				info.SaveDataSets(dsDetail);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public void UpdateShortageRejectionValueMap(string MasterId, List<Dictionary<string, object>> UserSendData)
		{
			try
			{
				string sql = "select * from trn.InventoryReceiveDetail where InventoryReceiveId='" + MasterId + "'";
				ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
				con.OpenDataSetThroughAdapter(sql, out DataSet dsDetail, false, "1");

				for (int i = 0; i < UserSendData.Count; i++)
				{
					dsDetail.Tables[0].DefaultView.RowFilter = "Id='" + UserSendData[i]["InventoryReceiveDetailId"].ToString() + "'";
					if (dsDetail.Tables[0].DefaultView.Count == 0)
					{
						//genId.GenID("TNA MASTER", out TNAMasterSystemID);
						//TNAMasterSystemID = "TM" + TNAMasterSystemID;
						//DataRow dr = dsMaster.Tables[0].NewRow();
						//dr["Id"] = TNAMasterSystemID;
						//dr[columnname] = TransactionId;
						//dr["TNAAppliedOn"] = ScheduleFor.ToString();
						//dr["AddedBy"] = "Scheduler";
						//dr["AddedDate"] = System.DateTime.Now.ToString();
						//dr["AddedFromIP"] = "";
						//dr["UpdatedBy"] = "Scheduler";
						//dr["UpdatedDate"] = System.DateTime.Now.ToString();
						//dr["UpdatedFromIP"] = "";

						//dsMaster.Tables[0].Rows.Add(dr);
					}
					else
					{
						DataRow dr = dsDetail.Tables[0].DefaultView[0].Row;
						dr.BeginEdit();
						dr["ShortageRatePercent"] = UserSendData[i]["ShortageRate"];
						dr["ShortageValue"] = UserSendData[i]["ShortageValue"];
						dr["RejectRatePercent"] = UserSendData[i]["RejectionRate"];
						dr["RejectValue"] = UserSendData[i]["RejectionValue"];
						dr["RejectClamPercent"] = UserSendData[i]["RejectionClamRate"];
						dr.EndEdit();

					}
				}

				clsStaticInfo info = new clsStaticInfo();
				info.SaveDataSets(dsDetail);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			try
			{
				//string sql = "select * from trn.InventoryReceiveDetail where InventoryReceiveId='" + MasterId + "'";
				string sql = "select * from TRN.GRNRejectionDetails where GRNDeailsId in(select id from trn.InventoryReceiveDetail where InventoryReceiveId = '" + MasterId + "')";

				ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
				con.OpenDataSetThroughAdapter(sql, out DataSet dsDetailREjectionMap, false, "1");

				for (int i = 0; i < UserSendData.Count; i++)
				{
					dsDetailREjectionMap.Tables[0].DefaultView.RowFilter = "GRNDeailsId='" + UserSendData[i]["InventoryReceiveDetailId"].ToString() + "'";
					if (dsDetailREjectionMap.Tables[0].DefaultView.Count == 0)
					{
						//genId.GenID("TNA MASTER", out TNAMasterSystemID);
						//TNAMasterSystemID = "TM" + TNAMasterSystemID;
						//DataRow dr = dsMaster.Tables[0].NewRow();
						//dr["Id"] = TNAMasterSystemID;
						//dr[columnname] = TransactionId;
						//dr["TNAAppliedOn"] = ScheduleFor.ToString();
						//dr["AddedBy"] = "Scheduler";
						//dr["AddedDate"] = System.DateTime.Now.ToString();
						//dr["AddedFromIP"] = "";
						//dr["UpdatedBy"] = "Scheduler";
						//dr["UpdatedDate"] = System.DateTime.Now.ToString();
						//dr["UpdatedFromIP"] = "";

						//dsMaster.Tables[0].Rows.Add(dr);
					}
					else
					{
						DataRow dr = dsDetailREjectionMap.Tables[0].DefaultView[0].Row;
						dr.BeginEdit();
						dr["RejectionQty"] = UserSendData[i]["RejectionQty"];
						dr["RejectionRate"] = UserSendData[i]["RejectionRate"];
						dr["RejeactionValue"] = UserSendData[i]["RejectionValue"];
						dr.EndEdit();

					}
				}

				clsStaticInfo info = new clsStaticInfo();
				info.SaveDataSets(dsDetailREjectionMap);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		public bool GetDocRef(string UserDocRefNo, string PartyId, string DocDate, string Id)
		{

			string sql = "Select * From trn.InventoryReceive where DocRefNo='" + UserDocRefNo + "' AND PartyId = '" + PartyId + "' and Id<>'" + Id + "'";
			ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
			con.OpenDataSetThroughAdapter(sql, out DataSet dsDetail, false, "1");
			//dsDetail.Tables[0].DefaultView.RowFilter = "DocRefNo='" + UserDocRefNo + "' AND PartyId = '" + PartyId + "'";// AND DocDate = '" + DocDate + "'";
			if (dsDetail.Tables[0].Rows.Count > 0)
			{
				return true;

			}
			else return false;
		}
		private Dictionary<string, object> GetCompanyCurrencyId(string InventoryReceiveDetailId)
		{
			var cmdText = @"select * From trn.GRNPORequisitionAllocation  GRN where InventoryReceiveDetailId='" + InventoryReceiveDetailId + "'";
			return _sqlRepository.GetData(cmdText);
		}
		public IEnumerable<object> GetGRNDetailsForSoAllocation(string InventoryReceiveDetailId, string PODetailId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var currentStatusCount = GetCompanyCurrencyId(InventoryReceiveDetailId);

				var sql = "";
				if (!string.IsNullOrEmpty(InventoryReceiveDetailId))
				{
					if (currentStatusCount.Count == 0)
					{
						sql = @"select a.Id GRNID
						--,b.Id POdetailId
						--,c.BOQDetailId
						--,d.SalesOrderId 
						,MGM.UserName AS MaterialGroupMasterName
						,MM.Id AS MaterialMasterId
						,MM.UserName
						,IM.ArticleId
						,ART.StandardName
						,IM.FirstCharacteristicsId
						,FC.UserName AS FirstCharacteristics
						,IM.FirstCharacteristicsValueId
						,FCV.UserName AS FirstCharacteristicsValue
						,IM.SecondCharacteristicsId
						,SC.UserName AS SecondCharacteristics
						,IM.SecondCharacteristicsValueId
						,SCV.UserName AS SecondCharacteristicsValue
						,IM.ThirdCharacteristicsId
						,TC.UserName AS ThirdCharacteristics
						,IM.ThirdCharacteristicsValueId
						,TCV.UserName AS ThirdCharacteristicsValue
						,c.PODetailId
						,C.BOQDetailId
						,C.Id POBOQMAPID
						,C.TransactionQty TransactionQtyForPO
						,C.TransactionUoMId,uom.UserName TransactionUoM
						,C.BaseQty
						,C.BaseUoMId
						,C.POBOQQty
						,C.POUoMId
						,d.BOMQty ReqQty
						,0 allowQty
						,b.TransactionQty POTransactionQty
						,a.TransactionQty GRNQty
						,a.RejectionQty  GRNRejectionQty
						,isnull(GRN.Qty,0) allowCatedQty
						,0 TransactionQty
						,0 RejectionQty
						,null Active
						--,GRN.Id 
						,GRN.RejectQty
						,d.SalesOrderId
						From trn.InventoryReceiveDetail a
						LEFT JOIN trn.PurchaseOrderDetail b on b.Id=a.PODetailsId
						left join trn.POBOQMAP c on c.PODetailId=b.Id
						left join boq d On d.Id=c.BOQDetailId
						left JOIN trn.InventoryMaterial IM ON IM.Id=a.InventoryMaterialId
						left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
						--LEFT JOIN (select Sum(TransactionQty) Qty,InventoryReceiveDetailid,Id,sum(RejectQty) RejectQty from trn.GRNPORequisitionAllocation where InventoryReceiveDetailid='" + InventoryReceiveDetailId + @"'  group by InventoryReceiveDetailid,Id ) GRN ON GRN.InventoryReceiveDetailid=a.Id
                        left JOIN(select POBOQMapId ,Sum(TransactionQty) Qty,sum(RejectQty) RejectQty from trn.GRNPORequisitionAllocation  GROUP BY POBOQMapId)GRN ON GRN.POBOQMapId=c.Id						
						left join scs.UnitOfMeasurement uom ON uom.Id=c.TransactionUoMId where a.Id='" + InventoryReceiveDetailId + "' Order By d.SalesOrderId";

					}
					else
					{
						sql = @"select           a.Id GRNID				
								,MGM.UserName AS MaterialGroupMasterName
								,MM.Id AS MaterialMasterId
								,MM.UserName
								,IM.ArticleId
								,ART.StandardName
								,IM.FirstCharacteristicsId
								,FC.UserName AS FirstCharacteristics
								,IM.FirstCharacteristicsValueId
								,FCV.UserName AS FirstCharacteristicsValue
								,IM.SecondCharacteristicsId
								,SC.UserName AS SecondCharacteristics
								,IM.SecondCharacteristicsValueId
								,SCV.UserName AS SecondCharacteristicsValue
								,IM.ThirdCharacteristicsId
								,TC.UserName AS ThirdCharacteristics
								,IM.ThirdCharacteristicsValueId
								,TCV.UserName AS ThirdCharacteristicsValue
								,c.PODetailId
								,C.BOQDetailId
								,C.Id POBOQMAPID
								,C.TransactionQty TransactionQtyForPO
								,C.TransactionUoMId,uom.UserName TransactionUoM
								,C.BaseQty
								,C.BaseUoMId
								,C.POBOQQty
								,C.POUoMId
								,d.BOMQty ReqQty
								,0 allowQty
								,b.TransactionQty POTransactionQty
								,a.TransactionQty GRNQty
								,a.RejectionQty  GRNRejectionQty				
								,0 TransactionQty
								,0 RejectionQty
								,null Active
								,GRN.Id 
								,GRN.RejectQty
								,isnull(GRN.TransactionQty,0) allowCatedQty1
								,d.SalesOrderId
								,isnull(AllocatedSOQty.AllocatedSOQty,0) allowCatedQty
								From trn.GRNPORequisitionAllocation  GRN
								left join trn.POBOQMAP c on c.Id=GRN.POBOQMapId
								left join boq d On d.Id=c.BOQDetailId
								LEFT JOIN trn.InventoryReceiveDetail a ON a.Id=GRN.InventoryReceiveDetailId
								LEFT JOIN trn.PurchaseOrderDetail b on b.Id=a.PODetailsId			
								left JOIN trn.InventoryMaterial IM ON IM.Id=a.InventoryMaterialId
								left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
								LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
								LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
								LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
								LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
								LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
								LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
								LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
								LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
								left join scs.UnitOfMeasurement uom ON uom.Id=c.TransactionUoMId 
								--left JOIN(select SalesOrderId, Sum(TransactionQty) AllocatedSOQty from trn.GRNPORequisitionAllocation GROUP BY SalesOrderId)AllocatedSOQty ON AllocatedSOQty.SalesOrderId=d.SalesOrderId
								left JOIN(select POBOQMapId, Sum(TransactionQty) AllocatedSOQty from trn.GRNPORequisitionAllocation GROUP BY POBOQMapId)AllocatedSOQty ON AllocatedSOQty.POBOQMapId=c.Id
								where a.Id='" + InventoryReceiveDetailId + "' Order By d.SalesOrderId";
					}
				}
				else
				{
					sql = @"select '' GRNID
				--,b.Id POdetailId
				--,c.BOQDetailId
				--,d.SalesOrderId 
				,MGM.UserName AS MaterialGroupMasterName
				,MM.Id AS MaterialMasterId
				,MM.UserName
				,b.ArticleId
				,ART.StandardName
				,b.FirstCharacteristicsId
				,FC.UserName AS FirstCharacteristics
				,b.FirstCharacteristicsValueId
				,FCV.UserName AS FirstCharacteristicsValue
				,b.SecondCharacteristicsId
				,SC.UserName AS SecondCharacteristics
				,b.SecondCharacteristicsValueId
				,SCV.UserName AS SecondCharacteristicsValue
				,b.ThirdCharacteristicsId
				,TC.UserName AS ThirdCharacteristics
				,b.ThirdCharacteristicsValueId
				,TCV.UserName AS ThirdCharacteristicsValue
				,c.PODetailId
				,C.BOQDetailId
				,C.Id POBOQMAPID
				,C.TransactionQty TransactionQtyForPO
				,C.TransactionUoMId,uom.UserName TransactionUoM
				,C.BaseQty
				,C.BaseUoMId
				,C.POBOQQty
				,C.POUoMId
				,d.BOMQty ReqQty
				,0 allowQty
				,b.TransactionQty POTransactionQty
				,0 GRNQty
				,0  GRNRejectionQty
				--,GRN.Qty allowCatedQty
				,0 TransactionQty
				,0 RejectionQty
				,null Active
				--,GRN.Id 
				--,GRN.RejectQty
				,d.SalesOrderId
				From trn.PurchaseOrderDetail b 
				left join trn.POBOQMAP c on c.PODetailId=b.Id
				left join boq d On d.Id=c.BOQDetailId
				--left JOIN trn.InventoryMaterial IM ON IM.Id=b.InventoryMaterialId
				left JOIN MST.MaterialMaster AS MM ON mm.Id = b.InventoryMaterialId
				LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
				LEFT JOIN MST.MaterialMasterArticle AS ART ON b.ArticleId = ART.Id
				LEFT JOIN HKP.Characteristics AS FC ON b.FirstCharacteristicsId = FC.Id
				LEFT JOIN HKP.Characteristics AS SC ON b.SecondCharacteristicsId = SC.Id
				LEFT JOIN HKP.Characteristics AS TC ON b.ThirdCharacteristicsId = TC.Id
				LEFT JOIN HKP.CharacteristicsValue AS FCV ON b.FirstCharacteristicsValueId = FCV.Id
				LEFT JOIN HKP.CharacteristicsValue AS SCV ON b.SecondCharacteristicsValueId = SCV.Id
				LEFT JOIN HKP.CharacteristicsValue AS TCV ON b.ThirdCharacteristicsValueId = TCV.Id
				--LEFT JOIN (select Sum(TransactionQty) Qty,InventoryReceiveDetailid,Id,sum(RejectQty) RejectQty from trn.GRNPORequisitionAllocation where InventoryReceiveDetailid='" + InventoryReceiveDetailId + @"'  group by InventoryReceiveDetailid,Id ) GRN ON GRN.InventoryReceiveDetailid=a.Id
				left join scs.UnitOfMeasurement uom ON uom.Id=c.TransactionUoMId
				where b.Id='" + PODetailId + @"'";
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


		public IEnumerable<object> GetMaterialListForProductionReq(string Material, string Article, string Skuvalue1, string Skuvalue2, string Skuvalue3, string processId, string parameters,string SOMATART, string queryString)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			string paramter = "";
			//if (Material != "")
			//{
			//	if (paramter == "")
			//		paramter += "ISNULL(mm.Id,'') in(" + Material + ")";
			//	else
			//		paramter += " AND ISNULL(mm.Id,'') in(" + Material + ")";
			//}
			//if (Article != "")
			//{
			//	if (paramter == "")
			//		paramter += "ISNULL(MRD.ArticleId,'') in(" + Article + ")";
			//	else
			//		paramter += " AND ISNULL(MRD.ArticleId,'') in(" + Article + ")";
			//}
			if (Skuvalue1 != "")
			{
				if (paramter == "")
					paramter += "ISNULL(BOQD.FirstCharacteristicsValueId,'') in(" + Skuvalue1 + ")";
				else
					paramter += " AND ISNULL(BOQD.FirstCharacteristicsValueId,'') in(" + Skuvalue1 + ")";
			}
			if (Skuvalue2 != "")
			{
				if (paramter == "")
					paramter += "ISNULL(BOQD.SecondCharacteristicsValueId,'') in(" + Skuvalue2 + ")";
				else
					paramter += " AND ISNULL(BOQD.SecondCharacteristicsValueId,'') in(" + Skuvalue2 + ")";
			}
			if (Skuvalue3 != "")
			{
				if (paramter == "")
					paramter += "ISNULL(BOQD.ThirdCharacteristicsValueId,'') in(" + Skuvalue3 + ")";
				else
					paramter += " AND ISNULL(BOQD.ThirdCharacteristicsValueId,'') in(" + Skuvalue3 + ")";
			}

			//AND MRD.MaterialMasterId='MM-20183' AND MRD.ArticleId='MM-20183'  AND MRD.FirstCharacteristicsValueId='103' and MRD.FirstCharacteristicsValueId='57' AND MRD.FirstCharacteristicsValueId=''
			try
			{
				var sql = "";
				//sql = @"Select MGM.UserName MaterialMasterGroupName,IRD.InventoryMaterialId
				//		,MT.UserName MaterialType
				//		,mm.Id MaterialMasterId
				//		,mm.UserName MaterialMasterName
				//		,MRD.ArticleId		
				//		,ART.StandardName									
				//		,MRD.FirstCharacteristicsId
				//		,FC.UserName AS FirstCharacteristics
				//		,MRD.FirstCharacteristicsValueId
				//		,isnull(FCV.UserName,'') AS FirstCharacteristicsValue
				//		,MRD.SecondCharacteristicsId
				//		,SC.UserName AS SecondCharacteristics
				//		,MRD.SecondCharacteristicsValueId
				//		,isnull(SCV.UserName,'') AS SecondCharacteristicsValue
				//		,MRD.ThirdCharacteristicsId
				//		,TC.UserName AS ThirdCharacteristics
				//		,MRD.ThirdCharacteristicsValueId
				//		,isnull(TCV.UserName,'') AS ThirdCharacteristicsValue
				//                    ,Isnull(C.UserName,'') CountryName,C.Id CountryId
				//                    ,TUoM.Id AS TransactionUoMId
				//                    ,TUoM.UserName AS UOM
				//		,Sum(0) RequestedQty
				//		,Sum(0) RejectedQty
				//		,sum(((isnull(IRD.TransactionQty,0)-(isnull(IRD.IssueQty,0)+isnull(IRD.PurchaseReturnQty,0) +isnull(IRD.ReductionByAdjustmentQty,0)+isnull(IRD.InventorySalesQty,0)+isnull(IRD.InventoryScrapQty,0)+isnull(IRD.InventoryTransferQty,0))) +isnull(IRD.IssueReturnQty,0))) TotalQty
				//	    ,Sum(IRD.ShortageQty)  ShortageQty
				//	    ,Sum(IRD.RejectionQty)RejectionQty
				//	FROM [TRN].[InventoryMaterial] As MRD				                    
				//	Left JOIN MST.MaterialMaster AS MM ON MRD.MaterialMasterId = MM.Id
				//	LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
				//	LEFT JOIN MST.MaterialMasterArticle AS ART ON MRD.ArticleId = ART.Id
				//	LEFT JOIN HKP.Characteristics AS FC ON MRD.FirstCharacteristicsId = FC.Id
				//	LEFT JOIN HKP.Characteristics AS SC ON MRD.SecondCharacteristicsId = SC.Id
				//	LEFT JOIN HKP.Characteristics AS TC ON MRD.ThirdCharacteristicsId = TC.Id
				//	LEFT JOIN HKP.CharacteristicsValue AS FCV ON MRD.FirstCharacteristicsValueId = FCV.Id
				//	LEFT JOIN HKP.CharacteristicsValue AS SCV ON MRD.SecondCharacteristicsValueId = SCV.Id
				//	LEFT JOIN HKP.CharacteristicsValue AS TCV ON MRD.ThirdCharacteristicsValueId = TCV.Id
				//	LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.InventoryMaterialId=MRD.Id
				//	LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
				//	LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON TUoM.Id =IRD.TransactionUoMId 				                  
				//	LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id	
				//	--where mm.UserName like 'Envelop' AND ART.StandardName='Envelop'
				//                 LEFT JOIN SCS.Country C On C.Id=MRD.CountryId
				//	where Isnull(IRD.TransactionQty,0)-isnull(IRD.IssueQty,0)>0 
				//	AND IR.IsApproved=1 AND MM.IsAsset=0 AND "+ paramter + @"
				//	--and mm.UserName like '3 Finger Metal Glove' AND ART.StandardName='3 Finger Metal Glove- Left'
				//	--AND MRD.MaterialMasterId='MM-20183' AND MRD.ArticleId='MM-20183'  AND MRD.FirstCharacteristicsValueId='103' and MRD.FirstCharacteristicsValueId='57' AND MRD.FirstCharacteristicsValueId=''
				//	group by  MRD.ArticleId				                    
				//	,MGM.UserName
				//	,mm.Id
				//	,mm.UserName
				//	,ART.StandardName
				//	,MT.UserName
				//	,MRD.FirstCharacteristicsId
				//	,FC.UserName
				//	,MRD.FirstCharacteristicsValueId
				//	,FCV.UserName
				//	,MRD.SecondCharacteristicsId
				//	,SC.UserName
				//	,MRD.SecondCharacteristicsValueId
				//	,SCV.UserName
				//	,MRD.ThirdCharacteristicsId
				//	,TC.UserName
				//	,MRD.ThirdCharacteristicsValueId
				//	,TCV.UserName
				//	,TUoM.UserName
				//	,TUoM.Id
				//	,IRD.InventoryMaterialId
				//	,Isnull(C.UserName,'') 
				//	,C.Id";
				//return _sqlRepository.GetDataCollection(sql);
				//				sql = @" Select Convert(bit, 'False') 'check', MGM.UserName MaterialMasterGroupName,IRD.InventoryMaterialId
				//						,MT.UserName MaterialType
				//						,mm.Id MaterialMasterId
				//						,mm.UserName MaterialMasterName
				//						,MRD.ArticleId		
				//						,ART.StandardName									
				//						,FC.Id FirstCharacteristicsId
				//						,FC.UserName AS FirstCharacteristics
				//						,BFG.FirstCharacteristicsValueId
				//						,isnull(v1.UserName,'') AS FirstCharacteristicsValue
				//						,SC.Id SecondCharacteristicsId
				//						,SC.UserName AS SecondCharacteristics
				//						,BFG.SecondCharacteristicsValueId
				//						,isnull(v2.UserName,'') AS SecondCharacteristicsValue
				//						,TC.Id ThirdCharacteristicsId
				//						,TC.UserName AS ThirdCharacteristics
				//						,BFG.ThirdCharacteristicsValueId
				//						,isnull(v3.UserName,'') AS ThirdCharacteristicsValue
				//						--,Isnull(C.UserName,'') CountryName,C.Id CountryId
				//                       ,TUoM.Id AS TransactionUoMId
				//                        --,TUoM.UserName AS UOM
				//							,MRD.RequiredQtyPO RequestedQty
				//						,0 RejectedQty
				//						--,sum(((isnull(IRD.TransactionQty,0)-(isnull(IRD.IssueQty,0)+isnull(IRD.PurchaseReturnQty,0) +isnull(IRD.ReductionByAdjustmentQty,0)+isnull(IRD.InventorySalesQty,0)+isnull(IRD.InventoryScrapQty,0)+isnull(IRD.InventoryTransferQty,0))) +isnull(IRD.IssueReturnQty,0))) TotalQty
				//						,ISNULL(GRNALLO.TransactionQty,0) TotalQty
				//					    ,ISNULL(IRD.ShortageQty,0)  ShortageQty
				//					    ,ISNULL(IRD.RejectionQty,0) RejectionQty
				//						,BO.Consumption,BO.WastagePer,POUOM.UserName UOM,0 RequestedQty,BO.SalesOrderId
				//					FROM BOQ BO
				//					LEFT JOIN BOQDetail As MRD ON MRD.BOQId=BO.Id		
				//					LEFT JOIN BOQFGMapping BFG ON BFG.BOQDetailId=MRD.Id
				//					Left JOIN MST.MaterialMaster AS MM ON MRD.MaterialMasterId = MM.Id
				//					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
				//					LEFT JOIN MST.MaterialMasterArticle AS ART ON MRD.ArticleId = ART.Id
				//					LEFT OUTER JOIN[HKP].[CharacteristicsValue] V1 ON v1.Id = MRD.FirstCharacteristicsValueId
				//                    LEFT OUTER JOIN[HKP].[CharacteristicsValue] V2 ON v2.Id = MRD.SecondCharacteristicsValueId
				//                    LEFT OUTER JOIN[HKP].[CharacteristicsValue] V3 ON v3.Id = MRD.ThirdCharacteristicsValueId
				//                    LEFT JOIN HKP.Characteristics AS FC ON FC.Id = V1.CharacteristicsId
				//                    LEFT JOIN HKP.Characteristics AS SC ON SC.Id = V2.CharacteristicsId
				//                    LEFT JOIN HKP.Characteristics AS TC ON TC.Id = V3.CharacteristicsId
				//					LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.InventoryMaterialId=MRD.Id
				//					LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
				//					LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON TUoM.Id =IRD.TransactionUoMId 				                  
				//					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
				//					left join (select b.BOQDetailId,sum(a.TransactionQty) TransactionQty 
				//								from trn.GRNPORequisitionAllocation a
				//								left join trn.POBOQMap b ON b.Id=a.POBOQMapId
				//							    group by b.BOQDetailId
				//							   ) GRNALLO ON GRNALLO.BOQDetailId=MRD.Id
				//					 LEFT JOIN [SCS].[UnitOfMeasurement] POUOM ON POUOM.Id=BO.POUoMId
				//					--where mm.UserName like 'Envelop' AND ART.StandardName='Envelop'
				//                    -- LEFT JOIN SCS.Country C On C.Id=MRD.CountryId 
				//					where  " + paramter + @"  AND 
				//					MM.IsAsset=0 AND MRD.ProcessId='"+processId+ @"' AND MRD.SalesOrderId in("+ parameters + @")
				//					--AND Isnull(IRD.TransactionQty,0)-isnull(IRD.IssueQty,0)>0 
				//					--AND IR.IsApproved=1
				//					--GROUP BY  MGM.UserName ,IRD.InventoryMaterialId,MT.UserName 
				//						--,mm.Id ,mm.UserName ,MRD.ArticleId,ART.StandardName	,FC.Id 
				//						--,FC.UserName ,BFG.FirstCharacteristicsValueId,isnull(v1.UserName,'') ,SC.Id 
				//						--,SC.UserName ,BFG.SecondCharacteristicsValueId,isnull(v2.UserName,'') ,TC.Id 
				//						--,TC.UserName  ,BFG.ThirdCharacteristicsValueId,isnull(v3.UserName,'') ,TUoM.UserName ,TUoM.Id
				//--                      --,Isnull(C.UserName,'') CountryName,C.Id CountryId
				//						--,BO.Consumption,BO.WastagePer,POUOM.UserName
				//				";//test cbddfh
				//sql = @"Select Convert(bit, 'False') 'check', MGM.UserName MaterialMasterGroupName--,IRD.InventoryMaterialId
				//		,MT.UserName MaterialType
				//		,mm.Id MaterialMasterId
				//		,mm.UserName MaterialMasterName
				//		,BOQD.ArticleId		
				//		,ART.StandardName									
				//		,FC.Id FirstCharacteristicsId
				//		,FC.UserName AS FirstCharacteristics
				//		,BOQFGM.FirstCharacteristicsValueId
				//		,isnull(v1.UserName,'') AS FirstCharacteristicsValue
				//		,SC.Id SecondCharacteristicsId
				//		,SC.UserName AS SecondCharacteristics
				//		,BOQFGM.SecondCharacteristicsValueId
				//		,isnull(v2.UserName,'') AS SecondCharacteristicsValue
				//		,TC.Id ThirdCharacteristicsId
				//		,TC.UserName AS ThirdCharacteristics
				//		,BOQFGM.ThirdCharacteristicsValueId
				//		,isnull(v3.UserName,'') AS ThirdCharacteristicsValue						
				//		--,TUoM.Id AS TransactionUoMId
				//		,consumptionUoMId.Id AS TransactionUoMId
				//		,null TransactionUoMName                 
				//		,BOQD.RequiredQtyPO RequestedQty1
				//		,null RequestedQty,0 RequestedQtyNew   
				//		,0 RejectedQty,null RequisitionQtyOrginal


				//		,0  ShortageQty
				//		,0 RejectionQty
				//		,BOQD.Consumption,BOQD.WastagePer

				//		,POUoMId.Id POUoMId
				//		,POUoMId.UserName POUoM
				//		,BaseUoMId.Id BaseUoMId
				//		,BaseUoMId.UserName BaseUoM

				//		,GRNALLO.StockTransactionUoMId
				//		,GRNALLO.UserName StockUOM 

				//		,consumptionUoMId.UserName consumptionUoM
				//		,consumptionUoMId.Id consumptionUoMId

				//		,0 RequisitionQty,BOQD.SalesOrderId
				//		,BOQD.FirstCharacteristicsValueId BOQDFirstCharacteristicsValueId
				//		,BOQD.SecondCharacteristicsValueId BOQDSecondCharacteristicsValueId
				//		,BOQD.ThirdCharacteristicsValueId BOQDThirdCharacteristicsValueId
				//		,BOQD.BOQId
				//		,Sum(ISNULL(GRNALLO.TransactionQty,0)) TransactionQty
				//		,Isnull(MMAU.BaseUOMFactor,0) BaseUOMFactor
				//		,Sum(ISNULL(GRNALLO.TransactionQty,0))  TotalQty--* Isnull(MMAU.BaseUOMFactor,0)
				//		FROM BOQDetail BOQD
				//		LEFT JOIN BOQFGMapping BOQFGM on BOQD.Id=BOQFGM.BOQDetailId
				//		Left JOIN MST.MaterialMaster AS MM ON BOQD.MaterialMasterId = MM.Id

				//		LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
				//		LEFT JOIN MST.MaterialMasterArticle AS ART ON BOQD.ArticleId = ART.Id
				//		LEFT OUTER JOIN[HKP].[CharacteristicsValue] V1 ON v1.Id = BOQD.FirstCharacteristicsValueId
				//		LEFT OUTER JOIN[HKP].[CharacteristicsValue] V2 ON v2.Id = BOQD.SecondCharacteristicsValueId
				//		LEFT OUTER JOIN[HKP].[CharacteristicsValue] V3 ON v3.Id = BOQD.ThirdCharacteristicsValueId
				//		LEFT JOIN HKP.Characteristics AS FC ON FC.Id = V1.CharacteristicsId
				//		LEFT JOIN HKP.Characteristics AS SC ON SC.Id = V2.CharacteristicsId
				//		LEFT JOIN HKP.Characteristics AS TC ON TC.Id = V3.CharacteristicsId	
				//		--Left JOIN [MST].[MaterialMasterAlternativeUOM] AS MMAU ON MMAU.MaterialMasterId = MM.Id
				//		LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON TUoM.Id =mm.StockUOMId 	
				//		LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
				//		left join (select b.BOQDetailId,sum(a.BaseQty) TransactionQty ,UOM.UserName,UOM.Id StockTransactionUoMId
				//				from trn.GRNPORequisitionAllocation a
				//				left join trn.POBOQMap b ON b.Id=a.POBOQMapId
				//				--LEFT JOIN [SCS].[UnitOfMeasurement] UOM ON UOM.Id=a.TransactionUoMId
				//				LEFT JOIN [SCS].[UnitOfMeasurement] UOM ON UOM.Id=a.BaseUoMId
				//				group by b.BOQDetailId,UOM.UserName,UOM.Id 
				//				) GRNALLO ON GRNALLO.BOQDetailId=BOQD.Id
				//		LEFT JOIN [SCS].[UnitOfMeasurement] POUoMId ON POUoMId.Id=BOQD.POUoMId
				//		LEFT JOIN [SCS].[UnitOfMeasurement] BaseUoMId ON BaseUoMId.Id=BOQD.BaseUoMId
				//		LEFT JOIN [SCS].[UnitOfMeasurement] consumptionUoMId ON consumptionUoMId.Id=BOQD.BaseUoMId
				//		Left JOIN (Select a.MaterialMasterId,a.AlternativeUOMId,a.BaseUOMId ,Sum(a.BaseUOMFactor) BaseUOMFactor 
				//					from [MST].[MaterialMasterAlternativeUOM] a
				//					left JOIN [SCS].[UnitOfMeasurement] UOM ON UOM.Id=a.AlternativeUOMId
				//					--left join mst.MaterialMaster mm ON mm.Id=a.MaterialMasterId
				//					Group by a.MaterialMasterId,a.AlternativeUOMId,a.BaseUOMId
				//					) AS MMAU ON MMAU.MaterialMasterId = BOQD.MaterialMasterId AND MMAU.AlternativeUOMId=TUoM.Id --And MMAU.BaseUOMId=BOQD.BaseUoMId AND MMAU.BaseUOMId=mm.BaseUOMId
				//		--Where " + paramter + @"  AND MM.IsAsset=0 AND BOQD.ProcessId='" + processId + @"' AND BOQD.SalesOrderId in(" + parameters + @")
				//		Where BOQD.ProcessId='" + processId + @"' and 
				//		Concat(BOQD.SalesOrderId,'-',ISNULL(BOQFGM.FirstCharacteristicsValueId,''),'-',ISNULL(BOQFGM.SecondCharacteristicsValueId,''),'-',ISNULL(BOQFGM.ThirdCharacteristicsValueId,'')) in (" + SOMATART + ")" +
				//		"group by MGM.UserName	,MT.UserName,mm.Id,mm.UserName,BOQD.ArticleId,ART.StandardName,FC.Id,FC.UserName,BOQFGM.FirstCharacteristicsValueId	,isnull(v1.UserName, ''),SC.Id,SC.UserName,BOQFGM.SecondCharacteristicsValueId,isnull(v2.UserName, ''),TC.Id,TC.UserName,BOQFGM.ThirdCharacteristicsValueId,isnull(v3.UserName, ''),TUoM.Id,TUoM.UserName	,BOQD.RequiredQtyPO,BOQD.Consumption,BOQD.WastagePer,POUoMId.Id	,POUoMId.UserName,BaseUoMId.Id,BaseUoMId.UserName,GRNALLO.StockTransactionUoMId,GRNALLO.UserName,consumptionUoMId.UserName,consumptionUoMId.Id,BOQD.SalesOrderId,BOQD.FirstCharacteristicsValueId,BOQD.SecondCharacteristicsValueId,BOQD.ThirdCharacteristicsValueId,BOQD.BOQId,Isnull(MMAU.BaseUOMFactor,0)";
				//sql = @"Select Distinct Convert(bit, 'False') 'check', MGM.UserName MaterialMasterGroupName--,IRD.InventoryMaterialId
				//		,MT.UserName MaterialType
				//		,mm.Id MaterialMasterId
				//		,mm.UserName MaterialMasterName
				//		,BOQD.ArticleId		
				//		,ART.StandardName									
				//		,FC.Id FirstCharacteristicsId
				//		,FC.UserName AS FirstCharacteristics
				//		--,BOQFGM.FirstCharacteristicsValueId
				//		,isnull(v1.UserName,'') AS FirstCharacteristicsValue
				//		,SC.Id SecondCharacteristicsId
				//		,SC.UserName AS SecondCharacteristics
				//		--,BOQFGM.SecondCharacteristicsValueId
				//		,isnull(v2.UserName,'') AS SecondCharacteristicsValue
				//		,TC.Id ThirdCharacteristicsId
				//		,TC.UserName AS ThirdCharacteristics
				//		--,BOQFGM.ThirdCharacteristicsValueId
				//		,isnull(v3.UserName,'') AS ThirdCharacteristicsValue						
				//		--,TUoM.Id AS TransactionUoMId
				//		,consumptionUoMId.Id AS TransactionUoMId
				//		,null TransactionUoMName                 
				//		,BOQD.RequiredQtyPO RequestedQty1
				//		,null RequestedQty,0 RequestedQtyNew   
				//		,0 RejectedQty,null RequisitionQtyOrginal


				//		,0  ShortageQty
				//		,0 RejectionQty
				//		,BOQD.Consumption,BOQD.WastagePer

				//		,POUoMId.Id POUoMId
				//		,POUoMId.UserName POUoM
				//		,BaseUoMId.Id BaseUoMId
				//		,BaseUoMId.UserName BaseUoM

				//		,GRNALLO.StockTransactionUoMId
				//		,GRNALLO.UserName StockUOM 

				//		,consumptionUoMId.UserName consumptionUoM
				//		,consumptionUoMId.Id consumptionUoMId

				//		,0 RequisitionQty,BOQD.SalesOrderId
				//		,BOQD.FirstCharacteristicsValueId BOQDFirstCharacteristicsValueId
				//		,BOQD.SecondCharacteristicsValueId BOQDSecondCharacteristicsValueId
				//		,BOQD.ThirdCharacteristicsValueId BOQDThirdCharacteristicsValueId
				//		,BOQD.BOQId
				//		,ISNULL(GRNALLO.TransactionQty,0) TransactionQty
				//		,Isnull(MMAU.BaseUOMFactor,0) BaseUOMFactor
				//		,ISNULL(GRNALLO.TransactionQty,0)  TotalQty--* Isnull(MMAU.BaseUOMFactor,0)
				//		,BOQD.FirstCharacteristicsValueId RwFirstCharacteristicsValueId
				//		,BOQD.SecondCharacteristicsValueId RwSecondCharacteristicsValueId
				//		,BOQD.ThirdCharacteristicsValueId RwThirdCharacteristicsValueId
				//                    ,Concat(FGChar.SalesOrderId,'-',ISNULL(FGChar.FirstCharacteristicsValueId,''),'-',ISNULL(FGChar.SecondCharacteristicsValueId,''),'-',ISNULL(FGChar.ThirdCharacteristicsValueId,'')) SOFSTId
				//		--,Concat(BOQD.SalesOrderId,'-',ISNULL(BOQFGM.FirstCharacteristicsValueId,''),'-',ISNULL(BOQFGM.SecondCharacteristicsValueId,''),'-',ISNULL(BOQFGM.ThirdCharacteristicsValueId,'')) SOFSTId
				//		FROM BOQDetail BOQD
				//		LEFT JOIN BOQFGMapping BOQFGM on BOQD.Id=BOQFGM.BOQDetailId
				//		Left JOIN MST.MaterialMaster AS MM ON BOQD.MaterialMasterId = MM.Id

				//		LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
				//		LEFT JOIN MST.MaterialMasterArticle AS ART ON BOQD.ArticleId = ART.Id
				//		LEFT OUTER JOIN[HKP].[CharacteristicsValue] V1 ON v1.Id = BOQD.FirstCharacteristicsValueId
				//		LEFT OUTER JOIN[HKP].[CharacteristicsValue] V2 ON v2.Id = BOQD.SecondCharacteristicsValueId
				//		LEFT OUTER JOIN[HKP].[CharacteristicsValue] V3 ON v3.Id = BOQD.ThirdCharacteristicsValueId
				//		LEFT JOIN HKP.Characteristics AS FC ON FC.Id = V1.CharacteristicsId
				//		LEFT JOIN HKP.Characteristics AS SC ON SC.Id = V2.CharacteristicsId
				//		LEFT JOIN HKP.Characteristics AS TC ON TC.Id = V3.CharacteristicsId	
				//		--Left JOIN [MST].[MaterialMasterAlternativeUOM] AS MMAU ON MMAU.MaterialMasterId = MM.Id
				//		LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON TUoM.Id =mm.StockUOMId 	
				//		LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
				//		left join (select b.BOQDetailId,sum(a.BaseQty) TransactionQty ,UOM.UserName,UOM.Id StockTransactionUoMId
				//				from trn.GRNPORequisitionAllocation a
				//				left join trn.POBOQMap b ON b.Id=a.POBOQMapId
				//				--LEFT JOIN [SCS].[UnitOfMeasurement] UOM ON UOM.Id=a.TransactionUoMId
				//				LEFT JOIN [SCS].[UnitOfMeasurement] UOM ON UOM.Id=a.BaseUoMId
				//				group by b.BOQDetailId,UOM.UserName,UOM.Id 
				//				) GRNALLO ON GRNALLO.BOQDetailId=BOQD.Id
				//		LEFT JOIN [SCS].[UnitOfMeasurement] POUoMId ON POUoMId.Id=BOQD.POUoMId
				//		LEFT JOIN [SCS].[UnitOfMeasurement] BaseUoMId ON BaseUoMId.Id=BOQD.BaseUoMId
				//		LEFT JOIN [SCS].[UnitOfMeasurement] consumptionUoMId ON consumptionUoMId.Id=BOQD.BaseUoMId
				//		Left JOIN (Select a.MaterialMasterId,a.AlternativeUOMId,a.BaseUOMId ,Sum(a.BaseUOMFactor) BaseUOMFactor 
				//					from [MST].[MaterialMasterAlternativeUOM] a
				//					left JOIN [SCS].[UnitOfMeasurement] UOM ON UOM.Id=a.AlternativeUOMId
				//					--left join mst.MaterialMaster mm ON mm.Id=a.MaterialMasterId
				//					Group by a.MaterialMasterId,a.AlternativeUOMId,a.BaseUOMId
				//					) AS MMAU ON MMAU.MaterialMasterId = BOQD.MaterialMasterId AND MMAU.AlternativeUOMId=TUoM.Id --And MMAU.BaseUOMId=BOQD.BaseUoMId AND MMAU.BaseUOMId=mm.BaseUOMId
				//                   LEFT JOIN(
				//			SELECT distinct PDAMAP.BOQDetailId
				//				,SalesOrderId=STUFF((select distinct top 1 '-'+xpo.SalesOrderId from
				//				BOQDetail xpo
				//				INNER JOin BOQFGMapping xPDAMAP on xpo.Id=xPDAMAP.BOQDetailId
				//				where xPDAMAP.BOQDetailId=PDAMAP.BOQDetailId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	

				//				,FirstCharacteristicsValueId=STUFF((select distinct top 1 '-'+xPDAMAP.FirstCharacteristicsValueId from
				//				BOQDetail xpo
				//				INNER JOin BOQFGMapping xPDAMAP on xpo.Id=xPDAMAP.BOQDetailId
				//				where xPDAMAP.BOQDetailId=PDAMAP.BOQDetailId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

				//				,SecondCharacteristicsValueId=STUFF((select distinct top 1 '-'+xPDAMAP.SecondCharacteristicsValueId from
				//			    BOQDetail xpo
				//				INNER JOin BOQFGMapping xPDAMAP on xpo.Id=xPDAMAP.BOQDetailId
				//				where xPDAMAP.BOQDetailId=PDAMAP.BOQDetailId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

				//				,ThirdCharacteristicsValueId=STUFF((select distinct top 1 '-'+xPDAMAP.ThirdCharacteristicsValueId from
				//				BOQDetail xpo
				//				INNER JOin BOQFGMapping xPDAMAP on xpo.Id=xPDAMAP.BOQDetailId
				//				where xPDAMAP.BOQDetailId=PDAMAP.BOQDetailId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')							

				//				from  BOQFGMapping PDAMAP 
				//			  LEFT JOIN BOQDetail IR ON IR.Id = PDAMAP.BOQDetailId

				//			  group by  PDAMAP.BOQDetailId
				//			)FGChar ON FGChar.BOQDetailId=BOQD.Id
				//		--Where " + paramter + @"  AND MM.IsAsset=0 AND BOQD.ProcessId='" + processId + @"' AND BOQD.SalesOrderId in(" + parameters + @")
				//		Where BOQD.ProcessId='" + processId + @"' and 
				//		Concat(BOQD.SalesOrderId,'-',ISNULL(BOQFGM.FirstCharacteristicsValueId,''),'-',ISNULL(BOQFGM.SecondCharacteristicsValueId,''),'-',ISNULL(BOQFGM.ThirdCharacteristicsValueId,'')) in (" + SOMATART + ")";
				////"group by MGM.UserName	,MT.UserName,mm.Id,mm.UserName,BOQD.ArticleId,ART.StandardName,FC.Id,FC.UserName,BOQFGM.FirstCharacteristicsValueId	,isnull(v1.UserName, ''),SC.Id,SC.UserName,BOQFGM.SecondCharacteristicsValueId,isnull(v2.UserName, ''),TC.Id,TC.UserName,BOQFGM.ThirdCharacteristicsValueId,isnull(v3.UserName, ''),TUoM.Id,TUoM.UserName	,BOQD.RequiredQtyPO,BOQD.Consumption,BOQD.WastagePer,POUoMId.Id	,POUoMId.UserName,BaseUoMId.Id,BaseUoMId.UserName,GRNALLO.StockTransactionUoMId,GRNALLO.UserName,consumptionUoMId.UserName,consumptionUoMId.Id,BOQD.SalesOrderId,BOQD.FirstCharacteristicsValueId,BOQD.SecondCharacteristicsValueId,BOQD.ThirdCharacteristicsValueId,BOQD.BOQId,Isnull(MMAU.BaseUOMFactor,0)";
				////return _sqlRepository.GetDataCollection(sql);
				sql = @"select distinct
						 Convert(bit, 'False') 'check'
						,x.MaterialMasterGroupName
						,x.MaterialType
						,x.MaterialMasterId
						,x.MaterialMasterName
						,x.ArticleId		
						,x.StandardName									
						,x.FirstCharacteristicsId
						,x.FirstCharacteristics
						--,BOQFGM.FirstCharacteristicsValueId
						,x.FirstCharacteristicsValue
						,x.SecondCharacteristicsId
						,x.SecondCharacteristics
						--,BOQFGM.SecondCharacteristicsValueId
						,x.SecondCharacteristicsValue
						,x.ThirdCharacteristicsId
						,x.ThirdCharacteristics
						--,BOQFGM.ThirdCharacteristicsValueId
						,x.ThirdCharacteristicsValue						
						--,TUoM.Id AS TransactionUoMId
						,x.TransactionUoMId
						,x.TransactionUoMName 

						,x.POUoMId
						,x.POUoM
						,x.BaseUoMId
						,x.BaseUoM
						,x.StockTransactionUoMId
						,x.StockUOM 
						,x.consumptionUoM
						,x.consumptionUoMId
						,x.RwFirstCharacteristicsValueId
						,x.RwSecondCharacteristicsValueId
						,x.RwThirdCharacteristicsValueId
						--,x.SOFSTId
						,x.SalesOrderId
						,x.BOQDFirstCharacteristicsValueId
						,x.BOQDSecondCharacteristicsValueId
						,x.BOQDThirdCharacteristicsValueId
						,x.BOQId 
						--,Sum(ISNULL(x.Qty,0))  Qty
						--,Sum(ISNULL(x.Consumption,0))  Consumption
						--,Sum(ISNULL(x.WastagePer,0))  WastagePer
						--,Sum(ISNULL(x.RequisitionQty,0))  RequisitionQty
						--,Sum(ISNULL(x.RequisitionQtyOrginal,0))  RequisitionQtyOrginal
						,SUM(ISNULL(x.RequisitionQtyOrginal,0))  RequestedQtyOrginal
						--,Sum(ISNULL(x.RequestedQty,0))  RequestedQty
						--,Sum(ISNULL(x.RequestedQtyNew,0))  RequestedQtyNew 
						--,Sum(ISNULL(x.RejectedQty,0))  RejectedQty				
						--,Sum(ISNULL(x.ShortageQty,0))  ShortageQty
						--,Sum(ISNULL(x.RejectionQty,0))  RejectionQty			
						--,Sum(ISNULL(x.TransactionQty,0))  TransactionQty
						--,Sum(ISNULL(x.BaseUOMFactor,0))  BaseUOMFactor
						--,Sum(ISNULL(x.TotalQty,0))  TotalQty	 

						--,sum(ISNULL(x.Qty,0))  Qty
						,ISNULL(x.Consumption,0)  Consumption
						,ISNULL(x.WastagePer,0)  WastagePer
						,SUM(ISNULL(x.RequisitionQty,0))  RequisitionQty
						,SUM(ISNULL(x.RequisitionQtyOrginal,0))  RequisitionQtyOrginal
						,ISNULL(x.RequestedQty,0)  RequestedQty
						,ISNULL(x.RequestedQtyNew,0)  RequestedQtyNew 
						,ISNULL(x.RejectedQty,0)  RejectedQty				
						,ISNULL(x.ShortageQty,0) ShortageQty
						,ISNULL(x.RejectionQty,0)  RejectionQty			
						,ISNULL(x.TransactionQty,0)  TransactionQty
						,ISNULL(x.BaseUOMFactor,0)  BaseUOMFactor
						,ISNULL(x.TotalQty,0)  TotalQty	 
						from(
						Select distinct Convert(bit, 'False') 'check'
						, MGM.UserName MaterialMasterGroupName
						,MT.UserName MaterialType
						,mm.Id MaterialMasterId
						,mm.UserName MaterialMasterName
						,BOQD.ArticleId		
						,ART.StandardName									
						,FC.Id FirstCharacteristicsId
						,FC.UserName AS FirstCharacteristics
						--,BOQFGM.FirstCharacteristicsValueId
						,isnull(v1.UserName,'') AS FirstCharacteristicsValue
						,SC.Id SecondCharacteristicsId
						,SC.UserName AS SecondCharacteristics
						--,BOQFGM.SecondCharacteristicsValueId
						,isnull(v2.UserName,'') AS SecondCharacteristicsValue
						,TC.Id ThirdCharacteristicsId
						,TC.UserName AS ThirdCharacteristics
						--,BOQFGM.ThirdCharacteristicsValueId
						,isnull(v3.UserName,'') AS ThirdCharacteristicsValue						
						--,TUoM.Id AS TransactionUoMId
						,consumptionUoMId.Id AS TransactionUoMId
						,null TransactionUoMName 

						,POUoMId.Id POUoMId
						,POUoMId.UserName POUoM
						,BaseUoMId.Id BaseUoMId
						,BaseUoMId.UserName BaseUoM

						,GRNALLO.StockTransactionUoMId
						,GRNALLO.UserName StockUOM 

						,consumptionUoMId.UserName consumptionUoM
						,consumptionUoMId.Id consumptionUoMId
						,BOQD.FirstCharacteristicsValueId RwFirstCharacteristicsValueId
						,BOQD.SecondCharacteristicsValueId RwSecondCharacteristicsValueId
						,BOQD.ThirdCharacteristicsValueId RwThirdCharacteristicsValueId
						-- ,Concat(FGChar.SalesOrderId,'-',ISNULL(FGChar.FirstCharacteristicsValueId,''),'-',ISNULL(FGChar.SecondCharacteristicsValueId,''),'-',ISNULL(FGChar.ThirdCharacteristicsValueId,'')) SOFSTId
						--,Concat(BOQD.SalesOrderId,'-',ISNULL(BOQFGM.FirstCharacteristicsValueId,''),'-',ISNULL(BOQFGM.SecondCharacteristicsValueId,''),'-',ISNULL(BOQFGM.ThirdCharacteristicsValueId,'')) SOFSTId
						,BOQD.SalesOrderId
						,BOQD.FirstCharacteristicsValueId BOQDFirstCharacteristicsValueId
						,BOQD.SecondCharacteristicsValueId BOQDSecondCharacteristicsValueId
						,BOQD.ThirdCharacteristicsValueId BOQDThirdCharacteristicsValueId
						,BOQD.BOQId

						--,FGQty.Qty
						,BOQD.Consumption
						,BOQD.WastagePer
						,RequisitionQty=(BOQD.Consumption*FGQty.Qty)+(((BOQD.Consumption*FGQty.Qty)*BOQD.WastagePer)/100)
						, RequisitionQtyOrginal=(BOQD.Consumption*FGQty.Qty)+(((BOQD.Consumption*FGQty.Qty)*BOQD.WastagePer)/100)					
						,BOQD.RequiredQtyPO RequestedQty1
						,null RequestedQty
						,0 RequestedQtyNew   
						,0 RejectedQty
						
						,0  ShortageQty
						,0 RejectionQty
					
						,ISNULL(GRNALLO.TransactionQty,0) TransactionQty
						,Isnull(MMAU.BaseUOMFactor,0) BaseUOMFactor
						,(ISNULL(GRNALLO.TransactionQty,0))	  TotalQty--* Isnull(MMAU.BaseUOMFactor,0)						
					
					
						FROM BOQDetail BOQD
						LEFT JOIN BOQFGMapping BOQFGM on BOQD.Id=BOQFGM.BOQDetailId
						Left JOIN MST.MaterialMaster AS MM ON BOQD.MaterialMasterId = MM.Id
						
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
						LEFT JOIN MST.MaterialMasterArticle AS ART ON BOQD.ArticleId = ART.Id
						LEFT OUTER JOIN[HKP].[CharacteristicsValue] V1 ON v1.Id = BOQD.FirstCharacteristicsValueId
						LEFT OUTER JOIN[HKP].[CharacteristicsValue] V2 ON v2.Id = BOQD.SecondCharacteristicsValueId
						LEFT OUTER JOIN[HKP].[CharacteristicsValue] V3 ON v3.Id = BOQD.ThirdCharacteristicsValueId
						LEFT JOIN HKP.Characteristics AS FC ON FC.Id = V1.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS SC ON SC.Id = V2.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS TC ON TC.Id = V3.CharacteristicsId	
						--Left JOIN [MST].[MaterialMasterAlternativeUOM] AS MMAU ON MMAU.MaterialMasterId = MM.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON TUoM.Id =mm.StockUOMId 	
						LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
						left join (select a.SalesOrderId, b.BOQDetailId,sum(a.BaseQty) TransactionQty ,UOM.UserName,UOM.Id StockTransactionUoMId,a.BaseUoMId
														 from trn.GRNPORequisitionAllocation a
														left join trn.POBOQMap b ON b.Id=a.POBOQMapId
														LEFT JOIN [SCS].[UnitOfMeasurement] UOM ON UOM.Id=a.BaseUoMId
															--where a.SalesOrderId='212160101' --and b.BOQDetailId='21223-25'
														group by b.BOQDetailId,UOM.UserName,UOM.Id,a.SalesOrderId,a.BaseUoMId
								) GRNALLO ON GRNALLO.BOQDetailId=BOQD.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] POUoMId ON POUoMId.Id=BOQD.POUoMId
						LEFT JOIN [SCS].[UnitOfMeasurement] BaseUoMId ON BaseUoMId.Id=BOQD.BaseUoMId
						LEFT JOIN [SCS].[UnitOfMeasurement] consumptionUoMId ON consumptionUoMId.Id=BOQD.BaseUoMId
						Left JOIN (Select a.MaterialMasterId,a.AlternativeUOMId,a.BaseUOMId ,Sum(a.BaseUOMFactor) BaseUOMFactor 
									from [MST].[MaterialMasterAlternativeUOM] a
									left JOIN [SCS].[UnitOfMeasurement] UOM ON UOM.Id=a.AlternativeUOMId
									--left join mst.MaterialMaster mm ON mm.Id=a.MaterialMasterId
									Group by a.MaterialMasterId,a.AlternativeUOMId,a.BaseUOMId
									) AS MMAU ON MMAU.MaterialMasterId = BOQD.MaterialMasterId AND MMAU.AlternativeUOMId=TUoM.Id --And MMAU.BaseUOMId=BOQD.BaseUoMId AND MMAU.BaseUOMId=mm.BaseUOMId
                      

						left outer join (Select * from 
						(   

				            " + queryString+ @"

							--Select '212160101-800-411-' ID, 15 Qty
							--union all
							--Select '212160101-800-410-' ID, 10 Qty
							--union all
							--Select '212160103-800-409-' ID,5 Qty

						) FGQty)FGQty ON FGQty.ID=Concat(BOQD.SalesOrderId,'-',ISNULL(BOQFGM.FirstCharacteristicsValueId,''),'-',ISNULL(BOQFGM.SecondCharacteristicsValueId,''),'-',ISNULL(BOQFGM.ThirdCharacteristicsValueId,''))


						Where BOQD.ProcessId='" + processId + @"'   and Concat(BOQD.SalesOrderId,'-',ISNULL(BOQFGM.FirstCharacteristicsValueId,''),'-',ISNULL(BOQFGM.SecondCharacteristicsValueId,''),'-',ISNULL(BOQFGM.ThirdCharacteristicsValueId,'')) in (" + SOMATART+"))x	group by x.MaterialMasterGroupName,x.MaterialType,x.MaterialMasterId,x.MaterialMasterName,x.ArticleId,x.StandardName,x.FirstCharacteristicsId,x.FirstCharacteristics,x.FirstCharacteristicsValue,x.SecondCharacteristicsId,x.SecondCharacteristics,x.SecondCharacteristicsValue,x.ThirdCharacteristicsId,x.ThirdCharacteristics,x.ThirdCharacteristicsValue,x.TransactionUoMId,x.TransactionUoMName ,x.POUoMId,x.POUoM,x.BaseUoMId,x.BaseUoM,x.StockTransactionUoMId,x.StockUOM ,x.consumptionUoM,x.consumptionUoMId,x.RwFirstCharacteristicsValueId,x.RwSecondCharacteristicsValueId,x.RwThirdCharacteristicsValueId,x.SalesOrderId,x.BOQDFirstCharacteristicsValueId,x.BOQDSecondCharacteristicsValueId,x.BOQDThirdCharacteristicsValueId,x.BOQId ,ISNULL(x.Consumption,0)  ,ISNULL(x.WastagePer,0) ,ISNULL(x.RequestedQty,0)  ,ISNULL(x.RequestedQtyNew,0)   ,ISNULL(x.RejectedQty,0),ISNULL(x.ShortageQty,0) ,ISNULL(x.RejectionQty,0),ISNULL(x.TransactionQty,0)  ,ISNULL(x.BaseUOMFactor,0)  ,ISNULL(x.TotalQty,0)";
								
				var Data = _sqlRepository.GetDataCollection(sql);
				StringCollection strCol = new StringCollection();
				string MaterialMasterList = "''";
				for (int i = 0; i < Data.Count; i++)
				{
					if (strCol.Contains(Data[i]["MaterialMasterId"].ToString()) == true)
						continue;
					strCol.Add(Data[i]["MaterialMasterId"].ToString());
					MaterialMasterList += ",'" + Data[i]["MaterialMasterId"].ToString() + "'";

				}

				var UOMList = _sqlRepository.GetDataCollection(@"select M.Id AS MaterialMasterId, UOM1.Id AS [Value],UOM1.UserName AS [Text] from (select Id,BaseUOMId UOMId from mst.MaterialMaster
																	union
																	select MaterialMasterId,AlternativeUOMId from mst.MaterialMasterAlternativeUOM
																	) AS M
																	 JOIN scs.UnitOfMeasurement AS uom1 ON uom1.Id=m.UOMId
																	 where m.Id in (" + MaterialMasterList + @")");

				for (int i = 0; i < Data.Count; i++)
				{
					var temp = UOMList.Where(ee => ee["MaterialMasterId"].ToString() == Data[i]["MaterialMasterId"].ToString()).ToList();
					Data[i]["uoMList"] = temp;
				}

				return Data;
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}


		public IWorkbook CreatePurchaseRegisterReportSheet(string companyId, string plantId, string fromDate, string toDate, string Type)
		{
			try
			{

				var excelEngine = new ExcelEngine();
				var report = new Service.Helpers.ReportUtility();
				var workbook = report.GetWorkbook(ref excelEngine, 2);
				var sheet1 = workbook.Worksheets[0];
				var Head = "Purchase Register";// + " " + fromDate + " " + "To" + " " + toDate ;
				CreatePurchaseRegisterReportSheets(ref sheet1, report, Head, "Summary", companyId, plantId, fromDate, toDate, Type);
				workbook.Version = ExcelVersion.Excel2016;
				return workbook;
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		private void CreatePurchaseRegisterReportSheets(ref IWorksheet sheet1, ReportUtility report, string sheet1Name, string sheet2Name, string companyId, string plantId, string fromDate, string toDate, string Type)
		{


			var cmdText = "";
			//cmdText = @"Select * from (SELECT   --ROW_NUMBER() OVER(ORDER BY IRD.Id ASC) AS SLNo                           
			//			   Distinct REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNEntryDate
			//				,RCM= CASE When IR.IsTaxApplicable=0 Then 'No' Else 'Yes' END
			//				,IR.Id As GRNId
			//				,HSNC.Code HSNCode
			//			   ,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
			//			   --,p.Id
			//                         ,p.UserName AS PartyName
			//			   ,EI.EmployeeName FirstName						   
			//			   ,IRD.Id As GrnDetailId
			//			   ,IR.GateEntryNo,ISNULL(PWG.UserName,'') GateName
			//			   --,IR.InvoiceNo
			//			   --, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
			//			   ,IR.DocRefNo,   REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
			//			   ,DATEDIFF(day, IR.DocDate,IR.GRNDate) AS 'GrnInvoiceDateDifference'
			//			  ,MT.UserName MaterialType
			//			  ,MGM.UserName AS MaterialGroupMasterName
			//			  ,IM.MaterialMasterId
			//			  ,MM.UserName MaterialMasterName
			//		   -- , IM.ArticleId
			//			, ART.StandardName ArticleName
			//                     ,IsAsset=CASE WHEN MM.IsAsset=0 then 'No' else 'Yes' END
			//                     ,GRNAsset=CASE WHEN IRD.IsAsset =0 then 'No' else 'Yes' END 
			//			--, IM.FirstCharacteristicsId
			//			--, FC.UserName AS FirstCharacteristics
			//			--, IM.FirstCharacteristicsValueId
			//			, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
			//			--, IM.SecondCharacteristicsId
			//			--, SC.UserName AS SecondCharacteristics
			//			--, IM.SecondCharacteristicsValueId
			//			, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
			//			--, IM.ThirdCharacteristicsId
			//			--, TC.UserName AS ThirdCharacteristics
			//			--, IM.ThirdCharacteristicsValueId
			//			, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
			//			,TUoM.UserName AS UOM
			//			,IRD.TransactionQty
			//			,IRD.ShortageQty
			//			,IRD.ShortageRatePercent
			//			,IRD.ShortageValue
			//			,IRD.RejectionQty
			//			,IRD.RejectRatePercent
			//			,IRD.RejectValue
			//			,IRD.RejectClamPercent
			//			,IRD.ApprovedQty
			//			,IR.IsNonCreditable
			//			,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
			//			,ROUND(Isnull(IRD.MaterialTranRate,0),2) MaterialTranRate
			//			,ROUND(Isnull(IRD.MaterialTranAmount,0),2) MaterialTranAmount
			//			,ROUND(Isnull(IRD.TrnCurrencyBaseRate,0),2) TrnCurrencyBaseRate,ROUND(Isnull(IRD.BooksCurrencyBaseRate,0),2) BooksCurrencyBaseRate
			//			,TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)

			//			,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
			//			,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
			//			,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
			//			,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
			//			,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage

			//			--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo.Percentage,0))/100) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
			//			--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo2.Percentage,0))/100) SGST,TAxInfo2.Percentage SGSTTaxPercentage
			//			--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo1.Percentage,0))/100) IGST,TAxInfo1.Percentage IGSTTaxPercentage
			//			--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo3.Percentage,0))/100) TDS,TAxInfo3.Percentage TDSTaxPercentage
			//			--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo6.Percentage,0))/100) TCS,TAxInfo6.Percentage TCSTaxPercentage

			//                     --, IRD.ChargesTranAmount AS ChargesAmount	   
			//			--,totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id)
			//			--,totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)
			//			--,totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')
			//                      --,ServiceCharge=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
			//                      --,ServiceTax=((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
			//			,IRD.ChargesTranAmount ServiceCharge
			//			,IRD.ChargesTaxTranAmount ServiceTax
			//			--,ROUND(Isnull(IRD.TotalMaterialTranAmount,0),2) TotalMaterialTranAmount
			//			--,ROUND(Isnull(IRD.TotalMaterialBaseAmount,0),2) TotalMaterialBaseAmount
			//			,Case When IR.IsNonCreditable = 1 
			//				then ROUND(Isnull(IRD.MaterialTranAmount,0),2) + (SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id) + ((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount +((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
			//			when IR.IsNonCreditable = 0
			//				then ROUND(Isnull(IRD.MaterialTranAmount,0),2)  + ((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
			//			end TotalMaterialTranAmount
			//                    ,ROUND(Isnull(IRD.TotalMaterialBooksCurrencyAmount,0),2) TotalMaterialBaseAmount ,IR.AddedBy
			//                    ,CASE 
			//		        	WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  AND IR.AuthorizedByStatus = 'Approved' Then 'Approved'
			//					WHEN IR.CheckedBy is not null And IR.CheckedByStatus = 'ForChecked' AND IR.AuthorizedBy is null And IR.AuthorizedByStatus is null Then 'To be Checked'										
			//					WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  Then 'To be approved'


			//					WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Hold' Then 'Checking Hold'
			//					WHEN IR.CheckedBy is not null AND IR.CheckedByStatus = 'Rejected' Then 'Checking Rejected'
			//                             WHEN IR.CheckedBy is not null ANd IR.AuthorizedByStatus = 'Hold' Then 'Approving Hold'
			//					WHEN IR.CheckedBy is not null AND IR.AuthorizedByStatus = 'Rejected' Then 'Approving Rejected'	 
			//					END GRNCheckStatus
			//			,IGL.UserName AS GL
			//			,IGL.AccountCode GLCode
			//			,IA.Id ActivityId
			//			,IA.UserName Activity
			//			,IA.Code ActivityCode
			//			,IBM.RefNo BudgetrefNo
			//			,B.UserName AS Budget
			//                     ,IGL1.UserName AS CGL
			//			,IGL1.AccountCode CGLCode
			//			,IA1.Id CActivityId
			//			,IA1.UserName AS CActivity
			//			,IA1.Code CActivityCode
			//			,IBM1.RefNo CBudgetrefNo
			//			,B1.UserName AS CBUdget
			//                     ,EI1.EmployeeName CheckedBY
			//			,EI2.EmployeeName AuthorizedBy
			//                     ,IR.POId
			//			,IRD.PODetailsId AS PORowId
			//                     ,MS.UserName as StorageLocation--,V.VoucherNo

			//			,VoucherNo=CASE WHEN IR.EmployeeId <> '' Then V1.VoucherNo else V.VoucherNo END
			//			,Posted=CASE WHEN IR.Status <>'' then 'Yes' else 'No' END						
			//			,PostingDate= CASE WHEN IR.EmployeeId <> '' Then REPLACE(CONVERT(CHAR(11), ep.PostingDate, 106),' ','-')   else REPLACE(CONVERT(CHAR(11), I.PostingDate, 106),' ','-')  END 
			//			,PostedBy=CASE WHEN IR.EmployeeId <> '' Then ep.AddedBy else I.AddedBy END,IR.EmployeeId
			//                     --,isnull(p.TINNO,'') GSTINNo
			//			,isnull(PP.GSTIN,'') GSTINNo
			//			,IR.PartyId ,IR.InvoicingPartyPlantId,PP.UserName InvoicingPartyPlant
			//			,IR.DeliveryPartyPlantId,PPD.UserName DeliveryPartyPlant
			//		from TRN.InventoryMaterial AS IM
			//		JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
			//		--LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
			//		LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
			//		LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
			//		LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
			//		LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
			//		LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
			//		LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
			//		LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
			//		LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
			//		left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id --and ird.InventoryReceiveId='1987'
			//		left join trn.InventoryReceiveTax IRT ON IRT.InventoryReceiveDetailId=IRD.Id
			//	     --JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=IRT.HSNCodeId
			//		Left JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=IRT.HSNCodeId
			//		left jOIN [TRN].[InventoryReceive] AS IR ON IR.Id=IRD.InventoryReceiveId
			//		left JOIN [TRN].[PurchaseOrderDetail] AS PID on PID.Id=IRD.PODetailsId 
			//		left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id				
			//		left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
			//		LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id				
			//		LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId 
			//		LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
			//		LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
			//		LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
			//                 LEFT JOIN hkp.MaterialStorage AS MS ON MS.Id=IR.MaterialStorageId
			//		LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.CheckedBy
			//		LEFT JOIN EmployeeInformation EI2 ON EI2.SystemId=IR.AuthorizedBy
			//                 left JOIN trn.Invoice as I ON I.InventoryReceiveId=IR.Id					
			//		left join trn.Voucher V on V.Id=I.VoucherId
			//                 left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
			//		left join trn.Voucher V1 on V1.Id=ep.VoucherId
			//                 LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=IRD.PostDrGLGeneralInfoId 
			//		LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=IRD.PostDrBudgetMasterId
			//		LEFT JOIN HKP.Activity IA ON IA.Id=IRD.PostDrActivityId
			//		Left JOIN hkp.Budget B On B.Id=IBM.BudgetId
			//                 LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IRD.PostCrGLGeneralInfoId 
			//		LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IRD.PostCrBudgetMasterId
			//		LEFT JOIN HKP.Activity IA1 ON IA1.Id=IRD.PostCrActivityId
			//		Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
			//             LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,
			//	--sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//	A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
			//WHERE B.Code='CGST' and A.InventoryServiceId IS NULL
			//--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 


			//) TAxInfo	ON TAxInfo.InventoryReceiveDetailId=IRD.Id 
			//			LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,
			//			--sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//			A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//						LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
			//						WHERE B.Code='IGST' and A.InventoryServiceId IS NULL 
			//		--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 


			//						) TAxInfo1	ON TAxInfo1.InventoryReceiveDetailId=IRD.Id 

			//			LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,
			//		--,sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//		A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//						LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
			//						WHERE B.Code='SGST' and A.InventoryServiceId IS NULL
			//		--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 


			//						) TAxInfo2	ON TAxInfo2.InventoryReceiveDetailId=IRD.Id 

			//			LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,
			//		--sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//			A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//						LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
			//						WHERE B.Code='TDS' and A.InventoryServiceId IS NULL 
			//			--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 

			//						) TAxInfo3	ON TAxInfo3.InventoryReceiveDetailId=IRD.Id 



			//			LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,
			//			--sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//			A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
			//						WHERE B.Code='VAT' and A.InventoryServiceId IS NULL 
			//			--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 

			//			) TAxInfo4 ON TAxInfo4.InventoryReceiveDetailId=IRD.Id

			//			LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,
			//					--sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//					A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
			//						WHERE B.Code='AIT' and A.InventoryServiceId IS NULL 
			//			--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 

			//			) TAxInfo5 ON TAxInfo5.InventoryReceiveDetailId=IRD.Id
			//			LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,
			//					--sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//					A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
			//						WHERE B.Code='TCS' and A.InventoryServiceId IS NULL 
			//					--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 

			//			) TAxInfo6 ON TAxInfo6.InventoryReceiveDetailId=IRD.Id

			//			LEFT JOIN trn.GateEntry  GE ON GE.Id=Ir.GateEntryNo					
			//			LEFT JOIN dbo.PlantWiseGate PWG ON PWG.Id=GE.PlantWiseGateId
			//			 where  IR.PlantId='" + plantId + "' AND convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' --ORDER BY IR.GRNDate ASC

			//				UNION ALL
			//			SELECT 	--ROW_NUMBER() OVER(ORDER BY IRD.Id ASC) AS SLNo                           
			//			   Distinct REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNEntryDate
			//				,RCM= CASE When IR.IsTaxApplicable=0 Then 'No' Else 'Yes' END
			//				,IR.Id As GRNId
			//				,HSNC.Code HSNCode
			//			   ,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
			//			   --,p.Id
			//                         ,p.UserName AS PartyName
			//			   ,EI.EmployeeName FirstName						   
			//			   ,IRD.Id As GrnDetailId
			//			   ,IR.GateEntryNo,ISNULL(PWG.UserName,'') GateName
			//			   --,IR.InvoiceNo
			//			   --, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
			//			   ,IR.DocRefNo,   REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
			//			   ,DATEDIFF(day, IR.DocDate,IR.GRNDate) AS 'GrnInvoiceDateDifference'
			//			  ,'' MaterialType
			//			  ,'' MaterialGroupMasterName
			//			  ,'' MaterialMasterId
			//			    ,SM.UserName MaterialMasterName
			//		   -- , IM.ArticleId
			//			, '' ArticleName
			//                     ,IsAsset=CASE WHEN MM.IsAsset=0 then 'No' else 'Yes' END
			//                     ,GRNAsset=CASE WHEN IRD.IsAsset =0 then 'No' else 'Yes' END 
			//			--, IM.FirstCharacteristicsId
			//			--, FC.UserName AS FirstCharacteristics
			//			--, IM.FirstCharacteristicsValueId
			//			, '' FirstCharacteristicsValue
			//			--, IM.SecondCharacteristicsId
			//			--, SC.UserName AS SecondCharacteristics
			//			--, IM.SecondCharacteristicsValueId
			//			, '' SecondCharacteristicsValue
			//			--, IM.ThirdCharacteristicsId
			//			--, TC.UserName AS ThirdCharacteristics
			//			--, IM.ThirdCharacteristicsValueId
			//			, '' ThirdCharacteristicsValue 
			//			,TUoM.UserName AS UOM
			//			,0 TransactionQty
			//			,0 ShortageQty
			//			,0 ShortageRatePercent
			//			,0 ShortageValue
			//			,0 RejectionQty
			//			,0 RejectRatePercent
			//			,0 RejectValue
			//			,0 RejectClamPercent
			//			,0 ApprovedQty
			//			,IsNULL(IR.IsNonCreditable,'') IsNonCreditable
			//			,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
			//			,0 MaterialTranRate
			//			,IRD.ChargesTranAmount MaterialTranAmount
			//			,0 TrnCurrencyBaseRate
			//			,0 BooksCurrencyBaseRate
			//			,TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IRD.InventoryReceiveId AND InventoryReceiveDetailId is null)

			//			,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
			//			,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
			//			,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
			//			,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
			//			,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
			//			--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo.Percentage,0))/100) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
			//			--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo2.Percentage,0))/100) SGST,TAxInfo2.Percentage SGSTTaxPercentage
			//			--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo1.Percentage,0))/100) IGST,TAxInfo1.Percentage IGSTTaxPercentage
			//			--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo3.Percentage,0))/100) TDS,TAxInfo3.Percentage TDSTaxPercentage
			//			--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo6.Percentage,0))/100) TCS,TAxInfo6.Percentage TCSTaxPercentage

			//                     --, IRD.ChargesTranAmount AS ChargesAmount	   
			//			--,totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id)
			//			--,totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)
			//			--,totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')
			//                      --,ServiceCharge=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
			//                      --,ServiceTax=((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
			//			,0 ServiceCharge
			//			,0 ServiceTax
			//			--,ROUND(Isnull(IRD.TotalMaterialTranAmount,0),2) TotalMaterialTranAmount
			//			--,ROUND(Isnull(IRD.TotalMaterialBaseAmount,0),2) TotalMaterialBaseAmount
			//			--,Case When IR.IsNonCreditable = 1 
			//			--	then ROUND(Isnull(IRD.MaterialTranAmount,0),2) + (SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id) + ((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount +((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
			//			--when IR.IsNonCreditable = 0
			//			--	then ROUND(Isnull(IRD.MaterialTranAmount,0),2)  + ((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
			//			--end TotalMaterialTranAmount
			//			,0 TotalMaterialTranAmount
			//                    ,0 TotalMaterialBaseAmount ,IR.AddedBy
			//                    ,CASE 
			//		        	WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  AND IR.AuthorizedByStatus = 'Approved' Then 'Approved'
			//					WHEN IR.CheckedBy is not null And IR.CheckedByStatus = 'ForChecked' AND IR.AuthorizedBy is null And IR.AuthorizedByStatus is null Then 'To be Checked'										
			//					WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  Then 'To be approved'


			//					WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Hold' Then 'Checking Hold'
			//					WHEN IR.CheckedBy is not null AND IR.CheckedByStatus = 'Rejected' Then 'Checking Rejected'
			//                             WHEN IR.CheckedBy is not null ANd IR.AuthorizedByStatus = 'Hold' Then 'Approving Hold'
			//					WHEN IR.CheckedBy is not null AND IR.AuthorizedByStatus = 'Rejected' Then 'Approving Rejected'	 
			//					END GRNCheckStatus
			//			,IGL.UserName AS GL
			//			,IGL.AccountCode GLCode
			//			,IA.Id ActivityId
			//			,IA.UserName Activity
			//			,IA.Code ActivityCode
			//			,IBM.RefNo BudgetrefNo
			//			,B.UserName AS Budget
			//                     ,IGL1.UserName AS CGL
			//			,IGL1.AccountCode CGLCode
			//			,IA1.Id CActivityId
			//			,IA1.UserName AS CActivity
			//			,IA1.Code CActivityCode
			//			,IBM1.RefNo CBudgetrefNo
			//			,B1.UserName AS CBUdget
			//                     ,EI1.EmployeeName CheckedBY
			//			,EI2.EmployeeName AuthorizedBy
			//                     ,IR.POId
			//			,IRD.PODetailsId AS PORowId
			//                     ,MS.UserName as StorageLocation--,V.VoucherNo

			//			,VoucherNo=CASE WHEN IR.EmployeeId <> '' Then V1.VoucherNo else V.VoucherNo END
			//			,Posted=CASE WHEN IR.Status <>'' then 'Yes' else 'No' END						
			//			,PostingDate= CASE WHEN IR.EmployeeId <> '' Then REPLACE(CONVERT(CHAR(11), ep.PostingDate, 106),' ','-')   else REPLACE(CONVERT(CHAR(11), I.PostingDate, 106),' ','-')  END 
			//			,PostedBy=CASE WHEN IR.EmployeeId <> '' Then ep.AddedBy else I.AddedBy END,IR.EmployeeId
			//                    -- ,isnull(p.TINNO,'') GSTINNo
			//		,isnull(PP.GSTIN,'') GSTINNo
			//		,IR.PartyId ,IR.InvoicingPartyPlantId,PP.UserName InvoicingPartyPlant
			//			,IR.DeliveryPartyPlantId,PPD.UserName DeliveryPartyPlant
			//from TRN.InventoryMaterial AS IM
			//JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
			//LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
			//LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
			//LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
			//LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
			//LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
			//LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
			//LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
			//LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
			//left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id --and ird.InventoryReceiveId='1987'
			//left join trn.InventoryReceiveTax IRT ON IRT.InventoryReceiveId=IRD.InventoryReceiveId 
			//  --JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=IRT.HSNCodeId 
			//Left JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=IRT.HSNCodeId 
			//left jOIN [TRN].[InventoryReceive] AS IR ON IR.Id=IRD.InventoryReceiveId
			//left JOIN [TRN].[PurchaseOrderDetail] AS PID on PID.Id=IRD.PODetailsId 
			//left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id				
			//left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
			//LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id				

			//LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId 
			//LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
			//LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
			//LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
			//LEFT JOIN hkp.MaterialStorage AS MS ON MS.Id=IR.MaterialStorageId
			//LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.CheckedBy
			//LEFT JOIN EmployeeInformation EI2 ON EI2.SystemId=IR.AuthorizedBy

			//left JOIN trn.Invoice as I ON I.InventoryReceiveId=IR.Id					
			//left join trn.Voucher V on V.Id=I.VoucherId

			//left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
			//left join trn.Voucher V1 on V1.Id=ep.VoucherId


			//LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=IRD.PostDrGLGeneralInfoId 
			//LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=IRD.PostDrBudgetMasterId
			//LEFT JOIN HKP.Activity IA ON IA.Id=IRD.PostDrActivityId
			//Left JOIN hkp.Budget B On B.Id=IBM.BudgetId


			//LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IRD.PostCrGLGeneralInfoId 
			//LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IRD.PostCrBudgetMasterId
			//LEFT JOIN HKP.Activity IA1 ON IA1.Id=IRD.PostCrActivityId
			//Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
			//left JOIN trn.InventoryService ISs ON ISS.InventoryReceiveId=IR.Id
			//left JOin [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
			//LEFT JOIN (SELECT A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,
			//			--sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//			A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//			LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
			//			WHERE B.Code='CGST' and A.InventoryServiceId IS NOT NULL
			//			--Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 


			//			) TAxInfo	ON TAxInfo.InventoryReceiveId=IRD.InventoryReceiveId 
			//LEFT JOIN (SELECT A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,
			//			--sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//			A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//			LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
			//			WHERE B.Code='IGST' and A.InventoryServiceId IS NOT NULL 
			//		--Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 


			//			) TAxInfo1	ON TAxInfo1.InventoryReceiveId=IRD.InventoryReceiveId 

			//LEFT JOIN (SELECT A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,
			//			--sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//			A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//			LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
			//			WHERE B.Code='SGST' and A.InventoryServiceId IS NOT NULL 
			//			--Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 


			//			) TAxInfo2	ON TAxInfo2.InventoryReceiveId=IRD.InventoryReceiveId 

			//LEFT JOIN (SELECT A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,
			//			--sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//			A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//			LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
			//			WHERE B.Code='TDS' and A.InventoryServiceId IS NOT NULL 
			//			--Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 

			//			) TAxInfo3	ON TAxInfo3.InventoryReceiveId=IRD.Id 



			//LEFT JOIN (SELECT A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,
			//			--sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//			A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//			LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
			//			WHERE B.Code='VAT' and A.InventoryServiceId IS NOT NULL 
			//			--Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 

			//) TAxInfo4 ON TAxInfo4.InventoryReceiveId=IRD.Id

			//LEFT JOIN (SELECT A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,
			//	--sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//			A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//			LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
			//			WHERE B.Code='AIT' and A.InventoryServiceId IS NOT NULL 
			//			--Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 

			//) TAxInfo5 ON TAxInfo5.InventoryReceiveId=IRD.Id
			//LEFT JOIN (SELECT A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,
			//			--sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//			A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
			//			LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
			//			WHERE B.Code='TCS' and A.InventoryServiceId IS NOT NULL 
			//                    --Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 

			//) TAxInfo6 ON TAxInfo6.InventoryReceiveId=IRD.Id

			//LEFT JOIN trn.GateEntry  GE ON GE.Id=Ir.GateEntryNo					
			//LEFT JOIN dbo.PlantWiseGate PWG ON PWG.Id=GE.PlantWiseGateId
			//where  IR.PlantId='" + plantId + "' AND convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND IRT.InventoryServiceId is not null
			//)x
			//Order By X.GRNEntryDate ASC";


			cmdText = @"Select * from (SELECT   --ROW_NUMBER() OVER(ORDER BY IRD.Id ASC) AS SLNo                           
						   Distinct REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNEntryDate
							,RCM= CASE When IR.IsTaxApplicable=0 Then 'No' Else 'Yes' END
							,IR.Id As GRNId
							, HSNCode=case when TAxInfo.HSCode<>'' then TAxInfo.HSCode
							when TAxInfo1.HSCode<>'' then TAxInfo1.HSCode
							when TAxInfo2.HSCode<>'' then TAxInfo2.HSCode
									else '' end
						   ,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
						   --,p.Id
                            ,p.UserName AS PartyName
						   ,EI.EmployeeName FirstName						   
						   ,IRD.Id As GrnDetailId
						   ,IR.GateEntryNo,ISNULL(PWG.UserName,'') GateName
						   --,IR.InvoiceNo
						   --, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
						   ,IR.DocRefNo,   REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
						   ,DATEDIFF(day, IR.DocDate,IR.GRNDate) AS 'GrnInvoiceDateDifference'
						  ,MT.UserName MaterialType
						  ,MGM.UserName AS MaterialGroupMasterName
						  ,IM.MaterialMasterId
						  ,MM.UserName MaterialMasterName
					   -- , IM.ArticleId
						, ART.StandardName ArticleName
                        ,IsAsset=CASE WHEN MM.IsAsset=0 then 'No' else 'Yes' END
                        ,GRNAsset=CASE WHEN IRD.IsAsset =0 then 'No' else 'Yes' END 
						--, IM.FirstCharacteristicsId
						--, FC.UserName AS FirstCharacteristics
						--, IM.FirstCharacteristicsValueId
						, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
						--, IM.SecondCharacteristicsId
						--, SC.UserName AS SecondCharacteristics
						--, IM.SecondCharacteristicsValueId
						, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
						--, IM.ThirdCharacteristicsId
						--, TC.UserName AS ThirdCharacteristics
						--, IM.ThirdCharacteristicsValueId
						, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
						,TUoM.UserName AS UOM
						,IRD.TransactionQty
						,IRD.ShortageQty
						,IRD.ShortageRatePercent
						,IRD.ShortageValue
						,IRD.RejectionQty
						,IRD.RejectRatePercent
						,IRD.RejectValue
						,IRD.RejectClamPercent
						,IRD.ApprovedQty
						,IR.IsNonCreditable
						,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
						,ROUND(Isnull(IRD.MaterialTranRate,0),2) MaterialTranRate
						,ROUND(Isnull(IRD.MaterialTranAmount,0),2) MaterialTranAmount
						,ROUND(Isnull(IRD.TrnCurrencyBaseRate,0),2) TrnCurrencyBaseRate,ROUND(Isnull(IRD.BooksCurrencyBaseRate,0),2) BooksCurrencyBaseRate
						,TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)

						,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
						,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
						,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
						,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
						,round(isnull(TAxInfo6.TaxAmount,0),2) MaterialTCS,TAxInfo6.Percentage MaterialTCSTaxPercentage
                        ,round(isnull(TAxInfo7.TaxAmount,0),2) GRNTCS,TAxInfo7.Percentage GRNTCSTaxPercentage
						,round(isnull(TAxInfo8.TaxAmount,0),2) MandiTax,TAxInfo8.Percentage MandiTaxPercentage
						,round(isnull(TAxInfo9.TaxAmount,0),2) NirasritTax,TAxInfo9.Percentage NirasritTaxPercentage
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo.Percentage,0))/100) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo2.Percentage,0))/100) SGST,TAxInfo2.Percentage SGSTTaxPercentage
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo1.Percentage,0))/100) IGST,TAxInfo1.Percentage IGSTTaxPercentage
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo3.Percentage,0))/100) TDS,TAxInfo3.Percentage TDSTaxPercentage
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo6.Percentage,0))/100) TCS,TAxInfo6.Percentage TCSTaxPercentage

                        --, IRD.ChargesTranAmount AS ChargesAmount	   
						--,totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id)
						--,totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)
						--,totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')
                         --,ServiceCharge=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
                         --,ServiceTax=((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
						,IRD.ChargesTranAmount ServiceCharge
						,IRD.ChargesTaxTranAmount ServiceTax
						--,ROUND(Isnull(IRD.TotalMaterialTranAmount,0),2) TotalMaterialTranAmount
						--,ROUND(Isnull(IRD.TotalMaterialBaseAmount,0),2) TotalMaterialBaseAmount
						,Case When IR.IsNonCreditable = 1 
							then ROUND(Isnull(IRD.MaterialTranAmount,0),2) + (SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id) + ((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount +((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
						when IR.IsNonCreditable = 0
							then ROUND(Isnull(IRD.MaterialTranAmount,0),2)  + ((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
						end TotalMaterialTranAmount
                       ,ROUND(Isnull(IRD.TotalMaterialBooksCurrencyAmount,0),2) TotalMaterialBaseAmount ,IR.AddedBy
                       ,CASE 
					        	WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  AND IR.AuthorizedByStatus = 'Approved' Then 'Approved'
								WHEN IR.CheckedBy is not null And IR.CheckedByStatus = 'ForChecked' AND IR.AuthorizedBy is null And IR.AuthorizedByStatus is null Then 'To be Checked'										
								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  Then 'To be approved'
							

								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Hold' Then 'Checking Hold'
								WHEN IR.CheckedBy is not null AND IR.CheckedByStatus = 'Rejected' Then 'Checking Rejected'
                                WHEN IR.CheckedBy is not null ANd IR.AuthorizedByStatus = 'Hold' Then 'Approving Hold'
								WHEN IR.CheckedBy is not null AND IR.AuthorizedByStatus = 'Rejected' Then 'Approving Rejected'	 
								END GRNCheckStatus
						,IGL.UserName AS GL
						,IGL.AccountCode GLCode
						,IA.Id ActivityId
						,IA.UserName Activity
						,IA.Code ActivityCode
						,IBM.RefNo BudgetrefNo
						,B.UserName AS Budget
                        ,IGL1.UserName AS CGL
						,IGL1.AccountCode CGLCode
						,IA1.Id CActivityId
						,IA1.UserName AS CActivity
						,IA1.Code CActivityCode
						,IBM1.RefNo CBudgetrefNo
						,B1.UserName AS CBUdget
                        ,EI1.EmployeeName CheckedBY
						,EI2.EmployeeName AuthorizedBy
                        ,IRD.POId
						,IRD.PODetailsId AS PORowId
                        ,MS.UserName as StorageLocation--,V.VoucherNo

						,VoucherNo=CASE WHEN IR.EmployeeId <> '' Then V1.VoucherNo else V.VoucherNo END
						,Posted=CASE WHEN IR.Status <>'' then 'Yes' else 'No' END						
						,PostingDate= CASE WHEN IR.EmployeeId <> '' Then REPLACE(CONVERT(CHAR(11), ep.PostingDate, 106),' ','-')   else REPLACE(CONVERT(CHAR(11), I.PostingDate, 106),' ','-')  END 
						,PostedBy=CASE WHEN IR.EmployeeId <> '' Then ep.AddedBy else I.AddedBy END,IR.EmployeeId
                        --,isnull(p.TINNO,'') GSTINNo
						,isnull(PP.GSTIN,'') GSTINNo
						,IR.PartyId ,P.Code,IR.InvoicingPartyPlantId,PP.UserName InvoicingPartyPlant
						,IR.DeliveryPartyPlantId,PPD.UserName DeliveryPartyPlant
						,IRD.LotNo , IRD.QualityStatus , IRD.GrossAmount ,IRD.DiscountAmount--,Isnull(C.ContractNo,'') ContractNo
						,ISNULL(PID.RefferenceNo,'') RefferenceNo
						--,isnull(PO.POId,'') POId
						,isnull(PO.PurchaseLCId,'') PurchaseLCId
						,isnull(PO.ContractId,'') ContractId						
						,ISNull(po.ContractNo,'') ContractNo
						,isnull(PO.LCANo,'') LCANo
						,isnull(PO.LCDate,'') LCDate
					from TRN.InventoryMaterial AS IM
					JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					--LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id --and ird.InventoryReceiveId='1987'
					left join trn.InventoryReceiveTax IRT ON IRT.InventoryReceiveDetailId=IRD.Id
				    --Left JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=IRT.HSNCodeId
					left jOIN [TRN].[InventoryReceive] AS IR ON IR.Id=IRD.InventoryReceiveId
					left JOIN [TRN].[PurchaseOrderDetail] AS PID on PID.Id=IRD.PODetailsId 
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id				
					left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id				
					LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId 
					LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
					LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
					LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
                    LEFT JOIN hkp.MaterialStorage AS MS ON MS.Id=IR.MaterialStorageId
					LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.CheckedBy
					LEFT JOIN EmployeeInformation EI2 ON EI2.SystemId=IR.AuthorizedBy
                    left JOIN trn.Invoice as I ON I.InventoryReceiveId=IR.Id					
					left join trn.Voucher V on V.Id=I.VoucherId
                    left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
					left join trn.Voucher V1 on V1.Id=ep.VoucherId
                    LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=IRD.PostDrGLGeneralInfoId 
					LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=IRD.PostDrBudgetMasterId
					LEFT JOIN HKP.Activity IA ON IA.Id=IRD.PostDrActivityId
					Left JOIN hkp.Budget B On B.Id=IBM.BudgetId
                    LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IRD.PostCrGLGeneralInfoId 
					LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IRD.PostCrBudgetMasterId
					LEFT JOIN HKP.Activity IA1 ON IA1.Id=IRD.PostCrActivityId
					Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
	               LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode 
				   FROM [TRN].[InventoryReceiveTax] A
			LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
			left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
			WHERE B.Code='CGST' and A.InventoryServiceId IS NULL
			--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 

								
			) TAxInfo	ON TAxInfo.InventoryReceiveDetailId=IRD.Id 
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='IGST' and A.InventoryServiceId IS NULL
									--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								

									) TAxInfo1	ON TAxInfo1.InventoryReceiveDetailId=IRD.Id 
							  		 
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='SGST' and A.InventoryServiceId IS NULL 
									--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								

									) TAxInfo2	ON TAxInfo2.InventoryReceiveDetailId=IRD.Id 

						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									WHERE B.Code='TDS' and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								
									) TAxInfo3	ON TAxInfo3.InventoryReceiveDetailId=IRD.Id 


							
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='VAT' and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								
						) TAxInfo4 ON TAxInfo4.InventoryReceiveDetailId=IRD.Id

						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='AIT' and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
							
						) TAxInfo5 ON TAxInfo5.InventoryReceiveDetailId=IRD.Id
						--LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						--			LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						--			WHERE B.Code='TCS' and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								
						--) TAxInfo6 ON TAxInfo6.InventoryReceiveDetailId=IRD.Id
	                    LEFT JOIN (SELECT A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount 
						           FROM trn.InventoryReceiveAdditionalTax A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='TCS' --and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								
						) TAxInfo6 ON TAxInfo6.InventoryReceiveId=IR.Id

                        LEFT JOIN (SELECT A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount 
						           FROM trn.InventoryReceiveAdditionalTax A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='TCS' --and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								
						) TAxInfo7 ON TAxInfo7.InventoryReceiveId=IR.Id


						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='Mandi Tax' and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								
						) TAxInfo8 ON TAxInfo8.InventoryReceiveDetailId=IRD.Id


						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='Nirasrit T' and A.InventoryServiceId IS NULL 
						) TAxInfo9 ON TAxInfo9.InventoryReceiveDetailId=IRD.Id
						LEFT JOIN trn.GateEntry  GE ON GE.Id=Ir.GateEntryNo					
						LEFT JOIN dbo.PlantWiseGate PWG ON PWG.Id=GE.PlantWiseGateId
						--left join  trn.POGGRNMap POGGRNMap ON POGGRNMap.GRNId=IR.Id
						--Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
						LEFT JOIN(
							   SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
							    ,UDNo=STUFF((select distinct ','+C.UDNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
						 --where  IR.PlantId='20181'  AND convert(Date,IR.GRNDate) BETWEEN  '01-OCT-2020' AND '31-OCT-2020' --ORDER BY IR.GRNDate ASC
						 where  IR.PlantId='" + plantId + "' AND convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'


							UNION ALL

						SELECT 	--ROW_NUMBER() OVER(ORDER BY IRD.Id ASC) AS SLNo                           
						   Distinct REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNEntryDate
							,RCM= CASE When IR.IsTaxApplicable=0 Then 'No' Else 'Yes' END
							,IR.Id As GRNId
							, HSNCode=case when TAxInfo.HSCode<>'' then TAxInfo.HSCode
							when TAxInfo1.HSCode<>'' then TAxInfo1.HSCode
							when TAxInfo2.HSCode<>'' then TAxInfo2.HSCode
									else '' end --HSNC.Code HSNCode
						   ,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
						   --,p.Id
                            ,p.UserName AS PartyName
						   ,EI.EmployeeName FirstName						   
						   ,Null As GrnDetailId
						   ,IR.GateEntryNo,ISNULL(PWG.UserName,'') GateName
						   --,IR.InvoiceNo
						   --, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
						   ,IR.DocRefNo,   REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
						   ,DATEDIFF(day, IR.DocDate,IR.GRNDate) AS 'GrnInvoiceDateDifference'
						  ,NULL MaterialType
						  ,NULL MaterialGroupMasterName
						  ,NULL MaterialMasterId
						    ,SM.UserName MaterialMasterName
					   -- , IM.ArticleId
						, SM.UserName ArticleName
                        ,'No' IsAsset
                        ,'No' GRNAsset
						--, IM.FirstCharacteristicsId
						--, FC.UserName AS FirstCharacteristics
						--, IM.FirstCharacteristicsValueId
						, NULL FirstCharacteristicsValue
						--, IM.SecondCharacteristicsId
						--, SC.UserName AS SecondCharacteristics
						--, IM.SecondCharacteristicsValueId
						, NULL SecondCharacteristicsValue
						--, IM.ThirdCharacteristicsId
						--, TC.UserName AS ThirdCharacteristics
						--, IM.ThirdCharacteristicsValueId
						, NULL ThirdCharacteristicsValue 
						,NULL AS UOM
						,0 TransactionQty
						,0 ShortageQty
						,0 ShortageRatePercent
						,0 ShortageValue
						,0 RejectionQty
						,0 RejectRatePercent
						,0 RejectValue
						,0 RejectClamPercent
						,0 ApprovedQty
						,IsNULL(IR.IsNonCreditable,0) IsNonCreditable
						,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
						,0 MaterialTranRate
						,ISs.Amount MaterialTranAmount
						,0 TrnCurrencyBaseRate
						,0 BooksCurrencyBaseRate
						, 0 TaxAmount
						--,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
						--,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
						--,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
						--,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
						--,round(isnull(TAxInfo5.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
							,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
							,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
							,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
							,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
							,round(isnull(TAxInfo6.TaxAmount,0),2) MaterialTCS,TAxInfo6.Percentage MaterialTCSTaxPercentage					
							,round(isnull(TAxInfo7.TaxAmount,0),2) GRNTCS,TAxInfo7.Percentage GRNTCSTaxPercentage
							,round(isnull(TAxInfo8.TaxAmount,0),2) MandiTax,TAxInfo8.Percentage MandiTaxPercentage
							,round(isnull(TAxInfo9.TaxAmount,0),2) NirasritTax,TAxInfo9.Percentage NirasritTaxPercentage
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo.Percentage,0))/100) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo2.Percentage,0))/100) SGST,TAxInfo2.Percentage SGSTTaxPercentage
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo1.Percentage,0))/100) IGST,TAxInfo1.Percentage IGSTTaxPercentage
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo3.Percentage,0))/100) TDS,TAxInfo3.Percentage TDSTaxPercentage
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo6.Percentage,0))/100) TCS,TAxInfo6.Percentage TCSTaxPercentage

                        --, IRD.ChargesTranAmount AS ChargesAmount	   
						--,totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id)
						--,totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)
						--,totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')
                         --,ServiceCharge=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
                         --,ServiceTax=((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
						,0 ServiceCharge
						,0 ServiceTax
						--,ROUND(Isnull(IRD.TotalMaterialTranAmount,0),2) TotalMaterialTranAmount
						--,ROUND(Isnull(IRD.TotalMaterialBaseAmount,0),2) TotalMaterialBaseAmount
						--,Case When IR.IsNonCreditable = 1 
						--	then ROUND(Isnull(IRD.MaterialTranAmount,0),2) + (SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id) + ((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount +((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
						--when IR.IsNonCreditable = 0
						--	then ROUND(Isnull(IRD.MaterialTranAmount,0),2)  + ((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
						--end TotalMaterialTranAmount
						,0 TotalMaterialTranAmount
                       ,0 TotalMaterialBaseAmount ,IR.AddedBy
                       ,CASE 
					        	WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  AND IR.AuthorizedByStatus = 'Approved' Then 'Approved'
								WHEN IR.CheckedBy is not null And IR.CheckedByStatus = 'ForChecked' AND IR.AuthorizedBy is null And IR.AuthorizedByStatus is null Then 'To be Checked'										
								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  Then 'To be approved'
							

								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Hold' Then 'Checking Hold'
								WHEN IR.CheckedBy is not null AND IR.CheckedByStatus = 'Rejected' Then 'Checking Rejected'
                                WHEN IR.CheckedBy is not null ANd IR.AuthorizedByStatus = 'Hold' Then 'Approving Hold'
								WHEN IR.CheckedBy is not null AND IR.AuthorizedByStatus = 'Rejected' Then 'Approving Rejected'	 
								END GRNCheckStatus
						,Null AS GL
						,Null GLCode
						,Null ActivityId
						,Null Activity
						,Null ActivityCode
						,Null BudgetrefNo
						,Null AS Budget
                        ,Null AS CGL
						,Null CGLCode
						,Null CActivityId
						,Null AS CActivity
						,Null CActivityCode
						,Null CBudgetrefNo
						,NULL AS CBUdget
                        ,EI1.EmployeeName CheckedBY
						,EI2.EmployeeName AuthorizedBy
                        ,IR.POId
						,NULL AS PORowId
                        ,MS.UserName as StorageLocation--,V.VoucherNo

						,VoucherNo=CASE WHEN IR.EmployeeId <> '' Then V1.VoucherNo else V.VoucherNo END
						,Posted=CASE WHEN IR.Status <>'' then 'Yes' else 'No' END						
						,PostingDate= CASE WHEN IR.EmployeeId <> '' Then REPLACE(CONVERT(CHAR(11), ep.PostingDate, 106),' ','-')   else REPLACE(CONVERT(CHAR(11), I.PostingDate, 106),' ','-')  END 
						,PostedBy=CASE WHEN IR.EmployeeId <> '' Then ep.AddedBy else I.AddedBy END,IR.EmployeeId
                       -- ,isnull(p.TINNO,'') GSTINNo
					,isnull(PP.GSTIN,'') GSTINNo
					,IR.PartyId ,P.Code,IR.InvoicingPartyPlantId,PP.UserName InvoicingPartyPlant
						,IR.DeliveryPartyPlantId,PPD.UserName DeliveryPartyPlant
						,Null LotNo , Null QualityStatus , Null GrossAmount ,Null DiscountAmount--,Isnull(C.ContractNo,'') ContractNo
						,'' RefferenceNo
					,'' PurchaseLCId
					,'' ContractId						
					,'' ContractNo
					,'' LCANo
					,'' LCDate
			from trn.InventoryService AS ISs
			LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
			left jOIN [TRN].[InventoryReceive] AS IR ON IR.Id=ISs.InventoryReceiveId
			--left jOIN [TRN].[InventoryReceive] AS IR ON IR.Id=ISs.InventoryReceiveId
			LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId 
			LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
			LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
			LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
			LEFT JOIN hkp.MaterialStorage AS MS ON MS.Id=IR.MaterialStorageId
			LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.CheckedBy
			LEFT JOIN EmployeeInformation EI2 ON EI2.SystemId=IR.AuthorizedBy
			left JOIN trn.Invoice as I ON I.InventoryReceiveId=IR.Id					
			left join trn.Voucher V on V.Id=I.VoucherId
			left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
			left join trn.Voucher V1 on V1.Id=ep.VoucherId
			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
						,A.TaxAmount TaxAmount,HS.Code HSCode 
						FROM  [TRN].[InventoryReceiveTax] A
						LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
						left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
						WHERE B.Code='CGST'  
						--Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
						) TAxInfo	ON TAxInfo.InventoryServiceId=ISs.Id AND TAxInfo.InventoryServiceId IS NOT NULL

			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,HS.Code HSCode 
			FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
						left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
						WHERE B.Code='IGST' and A.InventoryServiceId IS NOT NULL --Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 

						) TAxInfo1	ON TAxInfo1.InventoryServiceId=ISs.Id AND TAxInfo1.InventoryServiceId IS NOT NULL 
							  		 
			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,HS.Code HSCode 
			FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
						left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
						WHERE B.Code='SGST' and A.InventoryServiceId IS NOT NULL --Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
								

						) TAxInfo2	ON TAxInfo2.InventoryServiceId=ISs.Id AND TAxInfo2.InventoryServiceId IS NOT NULL

			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
						WHERE B.Code='TDS' and A.InventoryServiceId IS NOT NULL --Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
								
						) TAxInfo3	ON TAxInfo3.InventoryServiceId=ISs.Id AND TAxInfo3.InventoryServiceId IS NOT NULL


							
			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='VAT' and A.InventoryServiceId IS NOT NULL --Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
								
			) TAxInfo4 ON TAxInfo4.InventoryServiceId=ISs.Id AND TAxInfo4.InventoryServiceId IS NOT NULL

			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='AIT' and A.InventoryServiceId IS NOT NULL --Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
							
			) TAxInfo5 ON TAxInfo5.InventoryServiceId=ISs.Id AND TAxInfo5.InventoryServiceId IS NOT NULL
			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='TCS' and A.InventoryServiceId IS NOT NULL --Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
								
			) TAxInfo6 ON TAxInfo6.InventoryServiceId=ISs.Id AND TAxInfo6.InventoryServiceId IS NOT NULL
          
            LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='TCS' and A.InventoryServiceId IS NOT NULL  
								
			) TAxInfo7 ON TAxInfo7.InventoryServiceId=ISs.Id AND TAxInfo7.InventoryServiceId IS NOT NULL
			 LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='Mandi Tax' and A.InventoryServiceId IS NOT NULL  
								
			) TAxInfo8 ON TAxInfo8.InventoryServiceId=ISs.Id AND TAxInfo8.InventoryServiceId IS NOT NULL
			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='Nirasrit T' and A.InventoryServiceId IS NOT NULL 
								
			) TAxInfo9 ON TAxInfo9.InventoryServiceId=ISs.Id AND TAxInfo9.InventoryServiceId IS NOT NULL
	               
			LEFT JOIN trn.GateEntry  GE ON GE.Id=Ir.GateEntryNo					
			LEFT JOIN dbo.PlantWiseGate PWG ON PWG.Id=GE.PlantWiseGateId
			--Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
			where  IR.PlantId='" + plantId + "' AND convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'
			--AND IRT.InventoryServiceId is not null
			)x
			Order By X.GRNEntryDate ASC";


			var inventoryMaterialList = _sqlRepository.GetDataTable(cmdText);
			var plantName = new DataView(_sqlRepository.GetDataTable(@"SELECT UserName from org.Plant WHERE Id='" + plantId + "'")).ToTable(true, "UserName").Rows[0]["UserName"].ToString();


			var colTransactionQtyTotal = 0.00;
			var colTransactionAmountTotal = 0.00;
			var colTotalMaterialTranAmountTotal = 0.00;
			var colTaxAmountTotal = 0.00;
			var colTotalMaterialBooksCurrencyAmountTotal = 0.00;
			var colTrnCurrencyBaseRateTotal = 0.00;
			var colBooksCurrencyBaseRateTotal = 0.00;
			var colShortageQtyTotal = 0.00;
			var colRejectionQtyTotal = 0.00;
			var colApprovedQtyTotal = 0.00;

			var colCGSTTotal = 0.00;
			var colSGSTTotal = 0.00;
			var colIGSTTotal = 0.00;
			var colTDSTotal = 0.00;
			var colTCSTotal = 0.00;

			var colCGSTTotal1 = 0.00;
			var colSGSTTotal1 = 0.00;
			var colIGSTTotal1 = 0.00;
			var colTDSTotal1 = 0.00;
			var colTCSTotal1 = 0.00;


			if (inventoryMaterialList.Rows.Count == 0)
				throw new Exception("No Data Found !!!");

			var _rowd = 4;

			if (fromDate != "" && toDate != "")
			{


				sheet1[_rowd, 4].Text = fromDate + " " + "To" + " " + toDate;
				sheet1[_rowd, 4].CellStyle.Font.Size = 8;
				sheet1[_rowd, 4].CellStyle.Font.Bold = false;
				sheet1.Range[_rowd, 3, _rowd, 4].Merge();
				//sheet1.Range[_rowd, 3, _rowd , 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;

			}

			var _rows = 5;
			sheet1[_rows, 5].Text = "Report Ref No: ";
			sheet1[_rows, 5].CellStyle.Font.Size = 8;
			sheet1[_rows, 5].CellStyle.Font.Bold = false;
			sheet1.Range[_rows, 3, _rows, 6].Merge();

			var _row = 6;

			//sheet1[_row, 69].Text = "Posted (Dr.)";
			//sheet1[_row, 69].CellStyle.Font.Size = 10;
			//sheet1[_row, 69].CellStyle.Font.Bold = true;
			//sheet1.UsedRange.WrapText = true;
			//sheet1[_row, 69].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			//sheet1[_row, 69].VerticalAlignment = ExcelVAlign.VAlignCenter;
			//sheet1.Range[_row, 69, _row, 75].BorderAround(ExcelLineStyle.Hair);
			//sheet1.Range[_row, 69, _row, 75].BorderInside(ExcelLineStyle.Hair);
			//sheet1.Range[_row, 69, _row, 75].Merge();
			//sheet1.Range[_row, 69, _row, 75].CellStyle.FillBackground = ExcelKnownColors.Tan;

			//sheet1[_row, 76].Text = "Posted (Cr.)";
			//sheet1[_row, 76].CellStyle.Font.Size = 10;
			//sheet1[_row, 76].CellStyle.Font.Bold = true;
			//sheet1.UsedRange.WrapText = true;
			//sheet1[_row, 76].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			//sheet1[_row, 76].VerticalAlignment = ExcelVAlign.VAlignCenter;
			//sheet1.Range[_row, 76, _row, 82].BorderAround(ExcelLineStyle.Hair);
			//sheet1.Range[_row, 76, _row, 82].BorderInside(ExcelLineStyle.Hair);
			//sheet1.Range[_row, 76, _row, 82].Merge();
			//sheet1.Range[_row, 76, _row, 82].CellStyle.FillBackground = ExcelKnownColors.Tan;
			//sheet1[_row, 15].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			var _rowL = _row;
			var row = _row + 1;
			//var xlsCol = 0;
			//var Article = 0;
			//var xlsRow = 0;

			var sheet1headreColIndex = 1;
			//var sheet2headreColIndex = 1;

			_rowL += 1;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GRN No");
			////wTable.Rows[ROW].Cells[sheet1headreColIndex].Width = 60;
			//sheet1headreColIndex++;
			sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRN No";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;
			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GRN Date");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRN Date";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Type");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Type";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;
			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Party");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Party";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "PartyId";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "PartyCode";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "InvoicingPartyPlantId";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "InvoicingPartyPlant";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "DeliveryPartyPlantId";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "DeliveryPartyPlant";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GSTIN No");
			//sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "GSTIN No";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;


			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Employee");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Employee";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Gate Entry No");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Gate Entry No";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Gate Name");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Gate Name";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;


			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Doc Ref No");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Doc Ref No";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Doc Ref Date");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Doc Ref Date";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Grn Doc Date Difference");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Grn Doc Date Difference";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Type");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Type";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Group");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Group";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;


			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Transaction Qty");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Transaction Qty";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTransactionQtyTotal = sheet1headreColIndex;
			sheet1headreColIndex++;
			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "UoM");
			//sheet1headreColIndex++;
			sheet1.Range[_rowL, sheet1headreColIndex].Text = "UoM";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 8;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Transaction Rate");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Transaction Rate";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;

			sheet1headreColIndex++;



			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Lot No";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;



			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Quality Status";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;




			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Gross Amount";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;



			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Discount Amount";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Transaction Amount");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Taxable Amount";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTransactionAmountTotal = sheet1headreColIndex;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Total Material Tran Amount");
			//sheet1headreColIndex++;

			//sheet1.Range[_rowL, sheet1headreColIndex].Text = "Total Material Tran Amount";
			//sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
			//sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			//sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			//sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			//colTotalMaterialTranAmountTotal = sheet1headreColIndex;
			//sheet1headreColIndex++;


			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Credtible Status");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Credtible Status";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Tax Amount");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "RCM";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTaxAmountTotal = sheet1headreColIndex;
			sheet1headreColIndex++;

			//sheet1.Range[_rowL, sheet1headreColIndex].Text = "Tax Amount";
			//sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			//sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			//sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			//sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			//colTaxAmountTotal = sheet1headreColIndex;
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "CGST";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colCGSTTotal = sheet1headreColIndex;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "CGST Tax (%)";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colCGSTTotal1 = sheet1headreColIndex;
			sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "SGST";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colSGSTTotal = sheet1headreColIndex;
			sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "SGST Tax (%)";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colSGSTTotal1 = sheet1headreColIndex;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "IGST";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colIGSTTotal = sheet1headreColIndex;
			sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "IGST Tax (%)";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colIGSTTotal1 = sheet1headreColIndex;
			sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "TDS";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTDSTotal = sheet1headreColIndex;
			sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "TDS Tax (%)";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTDSTotal1 = sheet1headreColIndex;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "MaterialTCS";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTCSTotal = sheet1headreColIndex;
			sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "MaterialTCS Tax (%)";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTCSTotal1 = sheet1headreColIndex;
			sheet1headreColIndex++;









			sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRNTCS";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTCSTotal = sheet1headreColIndex;
			sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRNTCS Tax (%)";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTCSTotal1 = sheet1headreColIndex;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "MandiTax";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTCSTotal = sheet1headreColIndex;
			sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "MandiTax Tax (%)";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTCSTotal1 = sheet1headreColIndex;
			sheet1headreColIndex++;



			sheet1.Range[_rowL, sheet1headreColIndex].Text = "NirasritTax";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTCSTotal = sheet1headreColIndex;
			sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "NirasritTax Tax (%)";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTCSTotal1 = sheet1headreColIndex;
			sheet1headreColIndex++;
			//sheet1.Range[_rowL, sheet1headreColIndex].Text = "AIT";
			//sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			//sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			//sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			//sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			//colTaxAmountTotal = sheet1headreColIndex;
			//sheet1headreColIndex++;


			//sheet1.Range[_rowL, sheet1headreColIndex].Text = "AIT Tax (%)";
			//sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			//sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			//sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			//sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			//colTaxAmountTotal = sheet1headreColIndex;
			//sheet1headreColIndex++;


			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Service Charge");
			//sheet1headreColIndex++;

			//sheet1.Range[_rowL, sheet1headreColIndex].Text = "Service Charge";
			//sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			//sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			//sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			//sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			//sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Service Tax");
			//sheet1headreColIndex++;

			//sheet1.Range[_rowL, sheet1headreColIndex].Text = "Service Tax";
			//sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			//sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			//sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			//sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			//sheet1headreColIndex++;


			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Total Material Books Currency Amount");
			//sheet1headreColIndex++;

			//sheet1.Range[_rowL, sheet1headreColIndex].Text = "Total Material Books Currency Amount";
			//sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
			//sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			//sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			//sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			//colTotalMaterialBooksCurrencyAmountTotal = sheet1headreColIndex;
			//sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Trn Currency Base Rate");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Trn Currency Base Rate";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTrnCurrencyBaseRateTotal = sheet1headreColIndex;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Books Currency Base Rate");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Books Currency Base Rate";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colBooksCurrencyBaseRateTotal = sheet1headreColIndex;
			sheet1headreColIndex++;



			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "MMIsAsset");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "MMIsAsset";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GRNIsAsset");
			//sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRNIsAsset";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "PO Id");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "PO Id";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;


			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Storage Location");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Storage Location";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;


			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Shortage Qty");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Shortage Qty";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colShortageQtyTotal = sheet1headreColIndex;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "ShortageRatePercent");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "ShortageRatePercent";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 22;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;


			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "ShortageValuet");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "ShortageValuet";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;


			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Rejection Qty");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Rejection Qty";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colRejectionQtyTotal = sheet1headreColIndex;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Reject Rate Per");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Reject Rate Per";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "RejectionValue");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "RejectionValue";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "RejectionClam");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "RejectionClam";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "ApprovedQty");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Approved Qty";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colApprovedQtyTotal = sheet1headreColIndex;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GRN Row ID");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRN Row ID";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GRN No");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRN No";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;


			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Prepared By");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Prepared By";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Status");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Status";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;



			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Checking Name");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Checking Name";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Approving Name");
			//sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Approving Name";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Posted");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Posted";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Posted By");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Posted By";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Voucher No");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Voucher No";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;


			//sheet1.Range[_rowL, sheet1headreColIndex].Text = "Contract No";
			//sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			//sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			//sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			//sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			//sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Posting Date");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Posting Date";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GL");
			//sheet1headreColIndex++;
			sheet1.Range[_rowL, sheet1headreColIndex].Text = "GL Code";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "GL";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Budget");
			//sheet1headreColIndex++;
			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Budget Code";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Budget";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;




			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Activity ID";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Activity Code";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Activity";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GL");
			//sheet1headreColIndex++;
			sheet1.Range[_rowL, sheet1headreColIndex].Text = "GL Code";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "GL";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "BUdget");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Budget Code";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Budget";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;



			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Activity");
			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Activity ID";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Activity Code";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Activity";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;			
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "POREfference";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "LCANo";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "ContractNo";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;

			sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
			sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
			sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;
			//sheet1headreColIndex++;

			var Row_Total_Start = _rowL + 1;
			for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
			{
				_rowL++;

				report.SetText(ref sheet1, _rowL, 1, inventoryMaterialList.Rows[n]["GRNId"].ToString());
				report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["GRNEntryDate"].ToString());
				report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["GRNType"].ToString());
				report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["PartyName"].ToString());
				report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["PartyId"].ToString());
				report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["Code"].ToString());
				report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["InvoicingPartyPlantId"].ToString());
				report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["InvoicingPartyPlant"].ToString());
				report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["DeliveryPartyPlantId"].ToString());
				report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["DeliveryPartyPlant"].ToString());
				report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["GSTINNo"].ToString());
				report.SetText(ref sheet1, _rowL, 12, inventoryMaterialList.Rows[n]["FirstName"].ToString());
				report.SetText(ref sheet1, _rowL, 13, inventoryMaterialList.Rows[n]["GateEntryNo"].ToString());
				report.SetText(ref sheet1, _rowL, 14, inventoryMaterialList.Rows[n]["GateName"].ToString());
				report.SetText(ref sheet1, _rowL, 15, inventoryMaterialList.Rows[n]["DocRefNo"].ToString());
				report.SetText(ref sheet1, _rowL, 16, inventoryMaterialList.Rows[n]["DocDate"].ToString());
				report.SetText(ref sheet1, _rowL, 17, inventoryMaterialList.Rows[n]["GrnInvoiceDateDifference"].ToString());
				report.SetText(ref sheet1, _rowL, 18, inventoryMaterialList.Rows[n]["MaterialType"].ToString());
				report.SetText(ref sheet1, _rowL, 19, inventoryMaterialList.Rows[n]["MaterialGroupMasterName"].ToString());
				report.SetText(ref sheet1, _rowL, 20, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
				report.SetText(ref sheet1, _rowL, 21, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
				report.SetText(ref sheet1, _rowL, 22, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString());
				report.SetText(ref sheet1, _rowL, 23, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString());
				report.SetText(ref sheet1, _rowL, 24, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString());
				report.SetText(ref sheet1, _rowL, 25, inventoryMaterialList.Rows[n]["HSNCode"].ToString());
				report.SetText(ref sheet1, _rowL, 26, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TransactionQty"].ToString()));
				report.SetText(ref sheet1, _rowL, 27, inventoryMaterialList.Rows[n]["UOM"].ToString());
				report.SetText(ref sheet1, _rowL, 28, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MaterialTranRate"].ToString()));
				report.SetText(ref sheet1, _rowL, 29, inventoryMaterialList.Rows[n]["LotNo"].ToString());
				report.SetText(ref sheet1, _rowL, 30, inventoryMaterialList.Rows[n]["QualityStatus"].ToString());
				report.SetText(ref sheet1, _rowL, 31, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["GrossAmount"].ToString()));
				report.SetText(ref sheet1, _rowL, 32, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["DiscountAmount"].ToString()));
				report.SetText(ref sheet1, _rowL, 33, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MaterialTranAmount"].ToString()));
				//report.SetText(ref sheet1, _rowL, 29, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TotalMaterialTranAmount"].ToString()));
				report.SetText(ref sheet1, _rowL, 34, inventoryMaterialList.Rows[n]["CredtibleStatus"].ToString());
				report.SetText(ref sheet1, _rowL, 35, inventoryMaterialList.Rows[n]["RCM"].ToString());
				//report.SetText(ref sheet1, _rowL, 31, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TaxAmount"].ToString()));
				//report.SetText(ref sheet1, _rowL, 27, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["CGSTTaxPercentage"].ToString()));
				report.SetText(ref sheet1, _rowL, 36, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["CGST"].ToString()));
				report.SetText(ref sheet1, _rowL, 37, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["CGSTTaxPercentage"].ToString()));
				report.SetText(ref sheet1, _rowL, 38, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["SGST"].ToString()));
				report.SetText(ref sheet1, _rowL, 39, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["SGSTTaxPercentage"].ToString()));
				report.SetText(ref sheet1, _rowL, 40, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IGST"].ToString()));
				report.SetText(ref sheet1, _rowL, 41, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IGSTTaxPercentage"].ToString()));
				report.SetText(ref sheet1, _rowL, 42, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TDS"].ToString()));
				report.SetText(ref sheet1, _rowL, 43, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TDSTaxPercentage"].ToString()));
				//report.SetText(ref sheet1, _rowL, 44, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TCS"].ToString()));
				////report.SetText(ref sheet1, _rowL, 35, inventoryMaterialList.Rows[n]["TCS"].ToString());
				//report.SetText(ref sheet1, _rowL, 45, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TCSTaxPercentage"].ToString()));

				report.SetText(ref sheet1, _rowL, 44, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MaterialTCS"].ToString()));
				//report.SetText(ref sheet1, _rowL, 35, inventoryMaterialList.Rows[n]["TCS"].ToString());
				report.SetText(ref sheet1, _rowL, 45, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MaterialTCSTaxPercentage"].ToString()));

				report.SetText(ref sheet1, _rowL, 46, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["GRNTCS"].ToString()));
				//report.SetText(ref sheet1, _rowL, 35, inventoryMaterialList.Rows[n]["TCS"].ToString());
				report.SetText(ref sheet1, _rowL, 47, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["GRNTCSTaxPercentage"].ToString()));

				report.SetText(ref sheet1, _rowL, 48, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MandiTax"].ToString()));
				//report.SetText(ref sheet1, _rowL, 35, inventoryMaterialList.Rows[n]["TCS"].ToString());
				report.SetText(ref sheet1, _rowL, 49, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MandiTaxPercentage"].ToString()));

				report.SetText(ref sheet1, _rowL, 50, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["NirasritTax"].ToString()));
				//report.SetText(ref sheet1, _rowL, 35, inventoryMaterialList.Rows[n]["TCS"].ToString());
				report.SetText(ref sheet1, _rowL, 51, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["NirasritTaxPercentage"].ToString()));

				report.SetText(ref sheet1, _rowL, 52, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TrnCurrencyBaseRate"].ToString()));
				report.SetText(ref sheet1, _rowL, 53, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BooksCurrencyBaseRate"].ToString()));
				report.SetText(ref sheet1, _rowL, 54, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
				report.SetText(ref sheet1, _rowL, 55, inventoryMaterialList.Rows[n]["GRNAsset"].ToString());
				report.SetText(ref sheet1, _rowL, 56, inventoryMaterialList.Rows[n]["POId"].ToString());
				report.SetText(ref sheet1, _rowL, 57, inventoryMaterialList.Rows[n]["StorageLocation"].ToString());
				report.SetText(ref sheet1, _rowL, 58, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ShortageQty"].ToString()));
				report.SetText(ref sheet1, _rowL, 59, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ShortageRatePercent"].ToString()));
				report.SetText(ref sheet1, _rowL, 60, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ShortageValue"].ToString()));
				report.SetText(ref sheet1, _rowL, 61, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["RejectionQty"].ToString()));
				report.SetText(ref sheet1, _rowL, 62, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["RejectRatePercent"].ToString()));
				report.SetText(ref sheet1, _rowL, 63, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["RejectValue"].ToString()));
				report.SetText(ref sheet1, _rowL, 64, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["RejectClamPercent"].ToString()));
				report.SetText(ref sheet1, _rowL, 65, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ApprovedQty"].ToString()));
				report.SetText(ref sheet1, _rowL, 66, inventoryMaterialList.Rows[n]["GrnDetailId"].ToString());
				report.SetText(ref sheet1, _rowL, 67, inventoryMaterialList.Rows[n]["GRNId"].ToString());
				report.SetText(ref sheet1, _rowL, 68, inventoryMaterialList.Rows[n]["AddedBy"].ToString());
				report.SetText(ref sheet1, _rowL, 69, inventoryMaterialList.Rows[n]["GRNCheckStatus"].ToString());
				report.SetText(ref sheet1, _rowL, 70, inventoryMaterialList.Rows[n]["CheckedBY"].ToString());
				report.SetText(ref sheet1, _rowL, 71, inventoryMaterialList.Rows[n]["AuthorizedBy"].ToString());
				report.SetText(ref sheet1, _rowL, 72, inventoryMaterialList.Rows[n]["Posted"].ToString());
				report.SetText(ref sheet1, _rowL, 73, inventoryMaterialList.Rows[n]["PostedBy"].ToString());
				report.SetText(ref sheet1, _rowL, 74, inventoryMaterialList.Rows[n]["VoucherNo"].ToString());
				//report.SetText(ref sheet1, _rowL, 68, inventoryMaterialList.Rows[n]["ContractNo"].ToString());
				report.SetText(ref sheet1, _rowL, 75, inventoryMaterialList.Rows[n]["PostingDate"].ToString());
				report.SetText(ref sheet1, _rowL, 76, inventoryMaterialList.Rows[n]["GLCode"].ToString());
				report.SetText(ref sheet1, _rowL, 77, inventoryMaterialList.Rows[n]["GL"].ToString());
				report.SetText(ref sheet1, _rowL, 78, inventoryMaterialList.Rows[n]["BudgetrefNo"].ToString());
				report.SetText(ref sheet1, _rowL, 79, inventoryMaterialList.Rows[n]["Budget"].ToString());
				report.SetText(ref sheet1, _rowL, 80, inventoryMaterialList.Rows[n]["ActivityId"].ToString());
				report.SetText(ref sheet1, _rowL, 81, inventoryMaterialList.Rows[n]["ActivityCode"].ToString());
				report.SetText(ref sheet1, _rowL, 82, inventoryMaterialList.Rows[n]["Activity"].ToString());
				report.SetText(ref sheet1, _rowL, 83, inventoryMaterialList.Rows[n]["CGLCode"].ToString());
				report.SetText(ref sheet1, _rowL, 84, inventoryMaterialList.Rows[n]["CGL"].ToString());
				report.SetText(ref sheet1, _rowL, 85, inventoryMaterialList.Rows[n]["CBudgetrefNo"].ToString());
				report.SetText(ref sheet1, _rowL, 86, inventoryMaterialList.Rows[n]["CBUdget"].ToString());
				report.SetText(ref sheet1, _rowL, 87, inventoryMaterialList.Rows[n]["CActivityId"].ToString());
				report.SetText(ref sheet1, _rowL, 88, inventoryMaterialList.Rows[n]["CActivityCode"].ToString());
				report.SetText(ref sheet1, _rowL, 89, inventoryMaterialList.Rows[n]["CActivity"].ToString());

				report.SetText(ref sheet1, _rowL, 90, inventoryMaterialList.Rows[n]["RefferenceNo"].ToString());
				report.SetText(ref sheet1, _rowL, 91, inventoryMaterialList.Rows[n]["LCANo"].ToString());
				report.SetText(ref sheet1, _rowL, 92, inventoryMaterialList.Rows[n]["ContractNo"].ToString());

			}
			_rowL++;

			if (fromDate != "" && toDate != "")
			{


				report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTransactionQtyTotal) - 1, "Total");
				sheet1.Range[_rowL, Convert.ToInt32(colTransactionQtyTotal) - 1].CellStyle.Font.Bold = true;
				//sheet1.Range[1, _rowL, Convert.ToInt32(colTransactionQtyTotal) - 1, _rowL].Merge();
				object sumObject;
				sumObject = inventoryMaterialList.Compute("Sum(MaterialTranAmount)", "");
				sheet1.Range[_rowL, Convert.ToInt32(colTransactionAmountTotal)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTransactionAmountTotal), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet1.Range[_rowL, Convert.ToInt32(colTransactionAmountTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet1.Range[_rowL, Convert.ToInt32(colTransactionAmountTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;
				sumObject = inventoryMaterialList.Compute("Sum(CGST)", "");
				sheet1.Range[_rowL, Convert.ToInt32(colCGSTTotal)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet1, _rowL, Convert.ToInt32(colCGSTTotal), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet1.Range[_rowL, Convert.ToInt32(colCGSTTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet1.Range[_rowL, Convert.ToInt32(colCGSTTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;
				sumObject = inventoryMaterialList.Compute("Sum(SGST)", "");
				sheet1.Range[_rowL, Convert.ToInt32(colSGSTTotal)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet1, _rowL, Convert.ToInt32(colSGSTTotal), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet1.Range[_rowL, Convert.ToInt32(colSGSTTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet1.Range[_rowL, Convert.ToInt32(colSGSTTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;
				sumObject = inventoryMaterialList.Compute("Sum(IGST)", "");
				sheet1.Range[_rowL, Convert.ToInt32(colIGSTTotal)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet1, _rowL, Convert.ToInt32(colIGSTTotal), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet1.Range[_rowL, Convert.ToInt32(colIGSTTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet1.Range[_rowL, Convert.ToInt32(colIGSTTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = inventoryMaterialList.Compute("Sum(TDS)", "");
				sheet1.Range[_rowL, Convert.ToInt32(colTDSTotal)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTDSTotal), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet1.Range[_rowL, Convert.ToInt32(colTDSTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet1.Range[_rowL, Convert.ToInt32(colTDSTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = inventoryMaterialList.Compute("Sum(MaterialTCS)", "");
				sheet1.Range[_rowL, Convert.ToInt32(colTCSTotal)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTCSTotal), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet1.Range[_rowL, Convert.ToInt32(colTCSTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet1.Range[_rowL, Convert.ToInt32(colTCSTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = inventoryMaterialList.Compute("Sum(ShortageQty)", "");
				sheet1.Range[_rowL, Convert.ToInt32(colShortageQtyTotal)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet1, _rowL, Convert.ToInt32(colShortageQtyTotal), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet1.Range[_rowL, Convert.ToInt32(colShortageQtyTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet1.Range[_rowL, Convert.ToInt32(colShortageQtyTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = inventoryMaterialList.Compute("Sum(RejectionQty)", "");
				sheet1.Range[_rowL, Convert.ToInt32(colRejectionQtyTotal)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet1, _rowL, Convert.ToInt32(colRejectionQtyTotal), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet1.Range[_rowL, Convert.ToInt32(colRejectionQtyTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet1.Range[_rowL, Convert.ToInt32(colRejectionQtyTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = inventoryMaterialList.Compute("Sum(ApprovedQty)", "");
				sheet1.Range[_rowL, Convert.ToInt32(colApprovedQtyTotal)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet1, _rowL, Convert.ToInt32(colApprovedQtyTotal), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet1.Range[_rowL, Convert.ToInt32(colApprovedQtyTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet1.Range[_rowL, Convert.ToInt32(colApprovedQtyTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;
			}

			sheet1.Range[(Row_Total_Start), 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 8;

			sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
			sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);
			//_rowL++;

			sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
			sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);

			sheet1.Name = sheet1Name;
			sheet1.UsedRange.WrapText = true;
			//sheet1.UsedRange.CellStyle.Font.Size = 8;
			sheet1.IsGridLinesVisible = false;
			report.PlantHeader(ref sheet1, sheet1headreColIndex, sheet1Name, plantId);
			report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);


		}
		private string GRNDAddiTaxId()
		{
			string sID = string.Empty;
			bplib.clsGenID objGenID = new bplib.clsGenID();
			objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "InventoryReceiveAdditionalTax", out sID);
			return sID;
		}



		public void SaveAdditinalTaxInGRN(string MasterId, List<Dictionary<string, object>> UserSendData)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				string sql = "select * from TRN.InventoryReceiveAdditionalTax where InventoryReceiveId='" + MasterId + "'";
				ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
				con.OpenDataSetThroughAdapter(sql, out DataSet dsDetail, false, "1");

				for (int i = 0; i < UserSendData.Count; i++)
				{
					dsDetail.Tables[0].DefaultView.RowFilter = "TaxCodeId='" + UserSendData[i]["TaxCodeId"].ToString() + "'";
					if (dsDetail.Tables[0].DefaultView.Count == 0)
					{

						DataRow dr = dsDetail.Tables[0].NewRow();
						dr["Id"] = GRNDAddiTaxId();
						dr["TaxCodeId"] = UserSendData[i]["TaxCodeId"];
						dr["Percentage"] = UserSendData[i]["ValueOfFixed"];
						dr["TaxAmount"] = UserSendData[i]["TaxAmount"];
						dr["AddedBy"] = identity.Name;
						dr["AddedDate"] = System.DateTime.Now.ToString();
						dr["AddedFromIP"] = identity.IPAddress;
						//dr["UpdatedBy"] = "";
						//dr["UpdatedDate"] = "";
						//dr["UpdatedFromIP"] = "";
						dr["InventoryReceiveId"] = MasterId.ToString();
						dr["TaxCategoryId"] = UserSendData[i]["TaxCategoryId"];
						dsDetail.Tables[0].Rows.Add(dr);
					}
					//else
					//{
					//	DataRow dr = dsDetail.Tables[0].DefaultView[0].Row;
					//	dr.BeginEdit();
					//	dr["ShortageRatePercent"] = UserSendData[i]["ShortageRate"];
					//	dr["ShortageValue"] = UserSendData[i]["ShortageValue"];
					//	dr["RejectRatePercent"] = UserSendData[i]["RejectionRate"];
					//	dr["RejectValue"] = UserSendData[i]["RejectionValue"];
					//	dr["RejectClamPercent"] = UserSendData[i]["RejectionClamRate"];
					//	dr.EndEdit();
					//}
				}


				clsStaticInfo info = new clsStaticInfo();
				info.SaveDataSets(dsDetail);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		public IEnumerable<object> GetAdvanceTaxInfo(string InventoryReceiveId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";
				sql = @"Select a.Id,a.TaxCodeId,a.Percentage ValueOfFixed,a.TaxAmount,a.AddedBy,a.AddedDate,a.AddedFromIP,b.UserName TaxName,InventoryReceiveId
						from [TRN].[InventoryReceiveAdditionalTax] a
						left join [mst].[TAXCode] b ON b.Id=a.TaxCodeId where a.InventoryReceiveId='" + InventoryReceiveId + "'";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}
		public IEnumerable<object> AdditionalTaxDelete(string Id)
		{
			try
			{
				var _sql = @" Delete from [TRN].[InventoryReceiveAdditionalTax] where Id='" + Id + @"'";
				return _sqlRepository.GetDataCollection(_sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}



		public void SaveAdditinalTaxInPurchaseReturn(string MasterId, List<Dictionary<string, object>> UserSendData)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				string sql = "select * from TRN.PurchaseReturnAdditionalTax where PurchaseReturnId='" + MasterId + "'";
				ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
				con.OpenDataSetThroughAdapter(sql, out DataSet dsDetail, false, "1");

				for (int i = 0; i < UserSendData.Count; i++)
				{
					dsDetail.Tables[0].DefaultView.RowFilter = "TaxCodeId='" + UserSendData[i]["TaxCodeId"].ToString() + "'";
					if (dsDetail.Tables[0].DefaultView.Count == 0)
					{

						DataRow dr = dsDetail.Tables[0].NewRow();
						dr["Id"] = GRNDAddiTaxId();
						dr["TaxCodeId"] = UserSendData[i]["TaxCodeId"];
						dr["Percentage"] = UserSendData[i]["ValueOfFixed"];
						dr["TaxAmount"] = UserSendData[i]["TaxAmount"];
						dr["AddedBy"] = identity.Name;
						dr["AddedDate"] = System.DateTime.Now.ToString();
						dr["AddedFromIP"] = identity.IPAddress;
						//dr["UpdatedBy"] = "";
						//dr["UpdatedDate"] = "";
						//dr["UpdatedFromIP"] = "";
						dr["PurchaseReturnId"] = MasterId.ToString();
						dr["TaxCategoryId"] = UserSendData[i]["TaxCategoryId"];
						dsDetail.Tables[0].Rows.Add(dr);
					}
					//else
					//{
					//	DataRow dr = dsDetail.Tables[0].DefaultView[0].Row;
					//	dr.BeginEdit();
					//	dr["ShortageRatePercent"] = UserSendData[i]["ShortageRate"];
					//	dr["ShortageValue"] = UserSendData[i]["ShortageValue"];
					//	dr["RejectRatePercent"] = UserSendData[i]["RejectionRate"];
					//	dr["RejectValue"] = UserSendData[i]["RejectionValue"];
					//	dr["RejectClamPercent"] = UserSendData[i]["RejectionClamRate"];
					//	dr.EndEdit();
					//}
				}


				clsStaticInfo info = new clsStaticInfo();
				info.SaveDataSets(dsDetail);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		public IEnumerable<object> GetAdvanceTaxInfoPurchaseReturn(string InventoryReceiveId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";
				sql = @"Select a.Id,a.TaxCodeId,a.Percentage ValueOfFixed,a.TaxAmount,a.AddedBy,a.AddedDate,a.AddedFromIP,b.UserName TaxName,PurchaseReturnId
						from [TRN].[PurchaseReturnAdditionalTax] a
						left join [mst].[TAXCode] b ON b.Id=a.TaxCodeId where a.PurchaseReturnId='" + InventoryReceiveId + "'";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}
		public IEnumerable<object> AdditionalTaxDeletePurchaseReturn(string Id)
		{
			try
			{
				var _sql = @" Delete from [TRN].[PurchaseReturnAdditionalTax] where Id='" + Id + @"'";
				return _sqlRepository.GetDataCollection(_sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}






		public IEnumerable<object> GRNDocumentMapDataAll(string POID)
		{
			try
			{
				var _sql = @"DECLARE @pathval varchar(200)='POPResources/GRN'
							SELECT GRNId,Remarks,'<a href='''  + @pathval+'/'+SystemFileName + ''' target=''_blank''>'+ UserFilename +'</a>' As UserFilename,Description
							--stuff(
							--(
							--  SELECT '<a href=''' + SystemFileName + ''' target=''_blank''>'+ UserFilename +'</a>'
							--  FROM [TRN].[GRNDocumentMap] 	 WHERE GRNId = t.GRNId FOR XML path('')
							--),1,1,' ') UserFilename
							FROM (select Id,CompanyGroupId	,GRNId,UserFilename ,SystemFileName,Description,Remarks,AddedBy,AddedDate,AddedFromIP,UpdatedBy,UpdatedDate,UpdatedFromIP 
							FROM [TRN].[GRNDocumentMap] )t
							ORDER BY t.UserFilename";
				return _sqlRepository.GetDataCollection(_sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public IEnumerable<object> getGRNCheckedListData(string plantId)

		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                     ,REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate
                                     ,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId,IR.PartyId,IR.AddedBy,IR.CheckedByStatus, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                ,  REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty
                                    , TU.TransactionUoMId, UoM.UserName AS TransactionUoM
                                    , Round(IRD.TransactionAmount,2) TransactionAmount
                                    , IRD.BaseAmount, IR.ToCurrencyRate
                                    ,Round(IRD.TotalMaterialTranAmount,2) TotalMaterialTranAmount
                                    ,Round(IRD.TotalMaterialBooksCurrencyAmount,2) TotalMaterialBooksCurrencyAmount
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID, ISNULL(GAG.CtnId, 0) AS CtnId 
                                    ,ei1.FirstName  AuthorizedBy
									,ei.FirstName  CheckedBy,Isnull(ei2.EmployeeName,'') As EmployeeName
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName
                        FROM [TRN].[InventoryReceive] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        --JOIN [TRN].[InventoryReceiveDetail] AS IRCD ON IRCD.InventoryReceiveId=IR.Id
                        left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount ,SUM(A.TotalMaterialTranAmount) AS TotalMaterialTranAmount, sum(A.TotalMaterialBooksCurrencyAmount) As TotalMaterialBooksCurrencyAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id  GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                             GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN EmployeeInformation ei on ei.SystemId=IR.CheckedBy 
						LEFT JOIN EmployeeInformation ei1 on ei1.SystemId=IR.AuthorizedBy
                        LEFT JOIN EmployeeInformation ei2 on ei2.SystemId=IR.EmployeeId
                        LEFT JOIN (Select count(Id) as CtnId, GRNID from TRN.GRNApprovalLogTbl where Status='APPROVED' group by GRNID) as GAG on GAG.GRNID=IR.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                        WHERE ISNULL(IR.[Status],'')<>'Posting' " +
						"AND IR.OpeningBalanceId IS NULL   " +
						"And IR.IsApproved =0 " +
						"AND IR.CheckedBy is not null " +
						"AND IR.CheckedByStatus='Checked'" +
						"AND IR.AuthorizedByStatus = 'For Approval'" +
						"AND IRD.TransactionQty>0 " +
						"Order by IR.GRNDate DESC";
				return _sqlRepository.GetDataCollection(Sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public IEnumerable<object> getGRNApprovedListData(string plantId)

		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                     ,REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate
                                     ,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId,IR.PartyId,IR.AddedBy,IR.CheckedByStatus, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                ,  REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty
                                    , TU.TransactionUoMId, UoM.UserName AS TransactionUoM
                                    , Round(IRD.TransactionAmount,2) TransactionAmount
                                    , IRD.BaseAmount, IR.ToCurrencyRate
                                    ,Round(IRD.TotalMaterialTranAmount,2) TotalMaterialTranAmount
                                    ,Round(IRD.TotalMaterialBooksCurrencyAmount,2) TotalMaterialBooksCurrencyAmount
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID, ISNULL(GAG.CtnId, 0) AS CtnId 
                                    ,ei1.FirstName  AuthorizedBy
									,ei.FirstName  CheckedBy,Isnull(ei2.EmployeeName,'') As EmployeeName
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName
                        FROM [TRN].[InventoryReceive] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        --JOIN [TRN].[InventoryReceiveDetail] AS IRCD ON IRCD.InventoryReceiveId=IR.Id
                        left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount ,SUM(A.TotalMaterialTranAmount) AS TotalMaterialTranAmount, sum(A.TotalMaterialBooksCurrencyAmount) As TotalMaterialBooksCurrencyAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id  GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                             GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN EmployeeInformation ei on ei.SystemId=IR.CheckedBy 
						LEFT JOIN EmployeeInformation ei1 on ei1.SystemId=IR.AuthorizedBy
                        LEFT JOIN EmployeeInformation ei2 on ei2.SystemId=IR.EmployeeId
                        LEFT JOIN (Select count(Id) as CtnId, GRNID from TRN.GRNApprovalLogTbl where Status='APPROVED' group by GRNID) as GAG on GAG.GRNID=IR.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                        WHERE ISNULL(IR.[Status],'')<>'Posting' " +
						"AND IR.OpeningBalanceId IS NULL   " +
						"And IR.IsApproved =1 " +
						"AND IR.CheckedBy is not NULL " +
						"AND IR.CheckedByStatus='Checked'" +
						"AND IR.AuthorizedByStatus = 'Approved'" +
						"AND IRD.TransactionQty>0 " +
						"Order by IR.GRNDate DESC";
				return _sqlRepository.GetDataCollection(Sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		private string GRNApprovalLogTblId()
		{
			string sID = string.Empty;
			bplib.clsGenID objGenID = new bplib.clsGenID();
			objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "GRNApprovalLogTbl", out sID);
			return sID;
		}
		public void GRNUncheckUpdate(string MasterId, Dictionary<string, object> UserSendData)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				string sql = "select * from TRN.InventoryReceive where Id='" + MasterId + "'";
				ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
				con.OpenDataSetThroughAdapter(sql, out DataSet dsDetail, false, "1");

				string sqllog = "select * from TRN.GRNApprovalLogTbl where 1=2";
				con.OpenDataSetThroughAdapter(sqllog, out DataSet dsDetailLog, false, "1");

				//for (int i = 0; i < UserSendData.Count; i++)
				//{
				dsDetail.Tables[0].DefaultView.RowFilter = "Id='" + UserSendData["Id"].ToString() + "'";
				if (dsDetail.Tables[0].DefaultView.Count == 0)
				{
					//DataRow dr = dsDetail.Tables[0].NewRow();
					//dr["Id"] = GRNDAddiTaxId();
					//dr["TaxCodeId"] = UserSendData[i]["TaxCodeId"];
					//dr["Percentage"] = UserSendData[i]["ValueOfFixed"];
					//dr["TaxAmount"] = UserSendData[i]["TaxAmount"];
					//dr["AddedBy"] = identity.Name;
					//dr["AddedDate"] = System.DateTime.Now.ToString();
					//dr["AddedFromIP"] = identity.IPAddress;
					////dr["UpdatedBy"] = "";
					////dr["UpdatedDate"] = "";
					////dr["UpdatedFromIP"] = "";
					//dr["InventoryReceiveId"] = MasterId.ToString();
					//dsDetail.Tables[0].Rows.Add(dr);
				}
				else
				{
					DataRow dr = dsDetail.Tables[0].DefaultView[0].Row;
					dr.BeginEdit();
					dr["CheckedByStatus"] = "ForChecked";
					dr["AuthorizedBy"] = null;
					dr["AuthorizedByStatus"] = null;
					dr["IsApproved"] = 0;
					dr.EndEdit();
					DataRow drlog = dsDetailLog.Tables[0].NewRow();
					drlog["Id"] = MasterId.ToString() + '-' + GRNApprovalLogTblId();
					drlog["CompanyGroupId"] = identity.CompanyGroupId;
					drlog["CompanyId"] = identity.CompanyId;
					drlog["PlantId"] = identity.PlantId;
					drlog["ApprovedBy"] = identity.EmployeeId;
					drlog["Date"] = System.DateTime.Now.ToString();
					drlog["POValue"] = UserSendData["TransactionQty"];
					drlog["Status"] = "UnChecked";
					drlog["AddedBy"] = identity.Name;
					drlog["AddedDate"] = System.DateTime.Now.ToString();
					drlog["AddedFromIP"] = identity.IPAddress;
					//dr["UpdatedBy"] = "";
					//dr["UpdatedDate"] = "";
					//dr["UpdatedFromIP"] = "";
					drlog["GRNID"] = MasterId.ToString();
					dsDetailLog.Tables[0].Rows.Add(drlog);
				}
				//}


				clsStaticInfo info = new clsStaticInfo();
				info.SaveDataSets(dsDetail, dsDetailLog);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public void GRNUnapprovedUpdate(string MasterId, Dictionary<string, object> UserSendData)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				string sql = "select * from TRN.InventoryReceive where Id='" + MasterId + "'";
				ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
				con.OpenDataSetThroughAdapter(sql, out DataSet dsDetail, false, "1");

				string sqllog = "select * from TRN.GRNApprovalLogTbl where 1=2";
				con.OpenDataSetThroughAdapter(sqllog, out DataSet dsDetailLog, false, "1");

				//for (int i = 0; i < UserSendData.Count; i++)
				//{
				dsDetail.Tables[0].DefaultView.RowFilter = "Id='" + UserSendData["Id"].ToString() + "'";
				if (dsDetail.Tables[0].DefaultView.Count == 0)
				{

					//DataRow dr = dsDetail.Tables[0].NewRow();
					//dr["Id"] = GRNDAddiTaxId();
					//dr["TaxCodeId"] = UserSendData[i]["TaxCodeId"];
					//dr["Percentage"] = UserSendData[i]["ValueOfFixed"];
					//dr["TaxAmount"] = UserSendData[i]["TaxAmount"];
					//dr["AddedBy"] = identity.Name;
					//dr["AddedDate"] = System.DateTime.Now.ToString();
					//dr["AddedFromIP"] = identity.IPAddress;
					////dr["UpdatedBy"] = "";
					////dr["UpdatedDate"] = "";
					////dr["UpdatedFromIP"] = "";
					//dr["InventoryReceiveId"] = MasterId.ToString();
					//dsDetail.Tables[0].Rows.Add(dr);
				}
				else
				{
					DataRow dr = dsDetail.Tables[0].DefaultView[0].Row;
					dr.BeginEdit();
					dr["CheckedByStatus"] = "ForChecked";
					dr["AuthorizedBy"] = null;
					dr["AuthorizedByStatus"] = null;
					dr["IsApproved"] = 0;
					dr.EndEdit();
					//Id,CompanyGroupId,CompanyId,PlantId,ApprovedBy,Date,POValue,Status,AddedBy,AddedDate,AddedFromIP,UpdatedBy,UpdatedDate,UpdatedFromIP,GRNID
					DataRow drlog = dsDetailLog.Tables[0].NewRow();
					drlog["Id"] = MasterId.ToString() + '-' + GRNApprovalLogTblId();
					drlog["CompanyGroupId"] = identity.CompanyGroupId;
					drlog["CompanyId"] = identity.CompanyId;
					drlog["PlantId"] = identity.PlantId;
					drlog["ApprovedBy"] = identity.EmployeeId;
					drlog["Date"] = System.DateTime.Now.ToString();
					drlog["POValue"] = UserSendData["TransactionQty"];
					drlog["Status"] = "UnApproved";
					drlog["AddedBy"] = identity.Name;
					drlog["AddedDate"] = System.DateTime.Now.ToString();
					drlog["AddedFromIP"] = identity.IPAddress;
					//dr["UpdatedBy"] = "";
					//dr["UpdatedDate"] = "";
					//dr["UpdatedFromIP"] = "";
					drlog["GRNID"] = MasterId.ToString();
					dsDetailLog.Tables[0].Rows.Add(drlog);
				}
				//}

				clsStaticInfo info = new clsStaticInfo();
				info.SaveDataSets(dsDetail, dsDetailLog);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		#region Stock Balance
		public IWorkbook CreateMaterialStockBalanceSheet(string companyId, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string Asset, string Inventory, string Country)
		{
			try
			{
				var excelEngine = new ExcelEngine();
				var report = new Service.Helpers.ReportUtility();
				var workbook = report.GetWorkbook(ref excelEngine, 2);
				var sheet1 = workbook.Worksheets[0];
				var sheet2 = workbook.Worksheets[1];
				var Head = "";
				if (Asset == "false" && Inventory == "true")
				{

					Head = "Material Stock Balance ( Of Inventory )";


				}

				else if (Asset == "true" && Inventory == "false")
				{

					Head = "Material Stock Balance (Of Fixed Asset)";


				}

				else if (Asset == "true" && Inventory == "true")
				{
					Head = "Material Stock Balance (Of Fixed Asset And Inventory)";

				}

				CreateMaterialStockBalanceSheet(ref sheet1, ref sheet2, report, Head, "Summary", companyId, plantId, fromDate, toDate, Qty, Amount, RcptIssue, Asset, Inventory, Country);
				workbook.Version = ExcelVersion.Excel2016;
				return workbook;
			}
			catch (Exception)
			{
				throw;
			}
		}

		private void CreateMaterialStockBalanceSheet(ref IWorksheet sheet1, ref IWorksheet sheet2, ReportUtility report, string sheet1Name, string sheet2Name, string CompanyId, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string Asset, string Inventory, string Country)
		{

			var cmdText = "";
			if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
			{
				if (Country == "undefined" || Country == "null") Country = "false";
				#region without country
				if (Asset == "true" && Inventory == "true" && Country == "false")
				{

					cmdText = @"
					select x.MaterialMasterName	
					,x.MaterialMasterId	
					,x.HSNCode
					,x.ArticleName	
					,x.ArticleId		
					,x.FirstCharacteristicsValue
					,x.SecondCharacteristicsValue
					,x.ThirdCharacteristicsValue 
					--,x.MaterialStorageLocation	
					--,x.InventoryMaterialId
					--,x.GRNDate
					,x.UOM,ISNULL(x.IsAsset,0) IsAsset
					,(SUM( x.OpeningBalance)-Sum(x.OpeningIssueQty)-sum(ISNULL(x.PurchaseReturnOpeningQty,0)))  OpeningBalance
					,(Sum(x.OpeningBalanceAmount)-Sum(x.PolicyAmount)-sum(isnull(PurchaseReturnOpeningAmount,0))) OpeningBalanceAmount
					,Sum(x.OpeningIssueQty) OpeningIssueQty
					,Sum(x.PolicyAmount) PolicyAmount
					,sum(x.ReceivedForThePeriod) ReceivedForThePeriod
					,Sum(x.ReceivedForThePeriodAmount) ReceivedForThePeriodAmount
					,Sum(x.IssueForThePeriod) IssueForThePeriod
					,Sum(x.IssueForThePeriodAmount) IssueForThePeriodAmount 
					,sum(x.IssueReturnQtyForThePeriod) IssueReturnQtyForThePeriod
					,sum(x.IssueReturnForThePeriodAmount) IssueReturnForThePeriodAmount

					,sum(ISNULL(x.PurchaseReturnOpeningQty,0))  PurchaseReturnOpeningQty
					,sum(isnull(PurchaseReturnOpeningAmount,0)) PurchaseReturnOpeningAmount

					,sum(x.PurchaseReturnQtyForThePeriod) PurchaseReturnQtyForThePeriod
					,sum(x.PurchaseReturnForThePeriodAmount) PurchaseReturnForThePeriodAmount
					,sum(x.AdjustmentQtyForThePeriod) AdjustmentQtyForThePeriod
					,sum(x.AdjustmentForThePeriodAmount) AdjustmentForThePeriodAmount
					,sum(x.InventorySalesQtyForThePeriod) InventorySalesQtyForThePeriod
					,sum(x.InventorySalesForThePeriodAmount) InventorySalesForThePeriodAmount
					,Sum(x.InventoryScrapQtyForThePeriod) InventoryScrapQtyForThePeriod
					,Sum(x.InventoryScrapForThePeriodAmount) InventoryScrapForThePeriodAmount
					,Sum(x.InventoryTransferQtyForThePeriod) InventoryTransferQtyForThePeriod
					,Sum(x.InventoryTransferForThePeriodAmount) InventoryTransferForThePeriodAmount							
					,sum(((((((((isnull(x.OpeningBalance,0)-isnull(x.OpeningIssueQty,0) + isnull(x.ReceivedForThePeriod,0))-isnull(x.IssueForThePeriod,0)) + isnull(x.IssueReturnQtyForThePeriod,0))  - ISNULL(x.PurchaseReturnQtyForThePeriod,0))- isnull(x.AdjustmentQtyForThePeriod,0))-isnull(x.InventorySalesQtyForThePeriod,0))-isnull(x.InventoryScrapQtyForThePeriod,0))-Isnull(InventoryTransferQtyForThePeriod,0))- ISNULL(x.PurchaseReturnOpeningQty,0)) Closing 
					,sum((((((((isnull(x.OpeningBalanceAmount,0)-isnull(x.PolicyAmount,0) + isnull(x.ReceivedForThePeriodAmount,0))-isnull(x.IssueForThePeriodAmount,0)-isnull(x.AdjustmentForThePeriodAmount,0))-isnull(x.PurchaseReturnForThePeriodAmount,0))+isnull(x.IssueReturnForThePeriodAmount,0)-isnull(x.InventorySalesForThePeriodAmount,0))-isnull(x.InventoryScrapForThePeriodAmount,0))-isnull(x.InventoryTransferForThePeriodAmount,0))-isnull(PurchaseReturnOpeningAmount,0))) ClosingAmount




					FROM (


					--------------Opening Balance

					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, opbal.IsAsset--,opbal.InventoryMaterialId,opbal.GRNDate
							
					--Opening Balance
					,isnull(opbal.TransactionQty,0) As OpeningBalance	
					,isnull(opbal.TotalMaterialBooksCurrencyAmount,0) AS OpeningBalanceAmount

					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					--Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left join( select t.InventoryMaterialId,t.IsAsset--,t.MaterialStorageId,t.GRNDate
							  , sum(t.TransactionQty) TransactionQty,sum(t.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount--,Sum(t.OpeningIssueQty) OpeningIssueQty,Sum(t.OpeningIssueAmt)  OpeningIssueAmt 
					from (
					SELECT IRD.InventoryMaterialId,IRD.IsAsset--, IRD.MaterialStorageId,IR.GRNDate
							,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
					--SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount, Sum(IRD.IssueQty) OpeningIssueQty, Round(Sum(IRD.IssueQty * IRD.MaterialTranRate* IR.ToCurrencyRate),2) OpeningIssueAmt
					FROM [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate) <= '" + fromDate + @"'
					AND IR.OpeningBalanceId IS NOT NULL 
					group By IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					UNION ALL
					SELECT IRD.InventoryMaterialId,IRD.IsAsset--, IRD.MaterialStorageId,IR.GRNDate
							,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
						--SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Round(Sum(IRD.TransactionQty* IRD.MaterialTranRate*IR.ToCurrencyRate),2) TotalMaterialBooksCurrencyAmount, Sum(IRD.IssueQty) OpeningIssueQty, Round(Sum(IRD.IssueQty * IRD.MaterialTranRate* IR.ToCurrencyRate),2) OpeningIssueAmt
					FROM [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate) < '" + fromDate + @"' AND IR.OpeningBalanceId IS NULL 
					group By IRD.InventoryMaterialId,IRD.IsAsset--,--,IRD.MaterialStorageId,IR.GRNDate
					)as t group by t.InventoryMaterialId,t.IsAsset--,t.MaterialStorageId,t.GRNDate
					) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                       
					--left join [HKP].[MaterialStorage] MS on ms.id=opbal.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  IM.PlantId='" + plantId + @"' AND MM.UserName is not null


					-------------------Opening Issued--------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IFD3.IsAsset--,IFD3.InventoryMaterialId,IFD3.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,IFD3.OpeningIssueQty
					,IFD3.PolicyAmount
                          

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left join (select IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, sum(IIH.Qty) OpeningIssueQty,Sum(IIH.TotalAmount) PolicyAmount--Sum(IID.TransactionQty) OpeningIssueQty, Sum(IID.PolicyAmount) PolicyAmount
					FROM TRN.InventoryIssueDetail IID  
					LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	
					Left JOIN TRN.InventoryIssueHistory IIH ON IIH.InventoryIssueDetailId=IID.Id
					Left JOIn TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
					Left JOIn Trn.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) < '" + fromDate + @"' AND II.PlantId='" + plantId + @"' AND IR.OpeningBalanceId IS NULL
					GROUP BY IID.InventoryMaterialId,IRD.IsAsset
					--,IRD.MaterialStorageId,IR.GRNDate
					) IFD3 On IFD3.InventoryMaterialId=IM.Id
                       
					--left join [HKP].[MaterialStorage] MS on ms.id=IFD3.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where   IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					--------------------------Received-----------------------
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, opbal2.IsAsset--,opbal2.InventoryMaterialId,opbal2.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                          

					--   --Receive

					,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
					,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

						

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id						
					left join(SELECT IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
					FROM  [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate)		
					BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' --AND IR.OpeningBalanceId IS  NULL 
					GROUP By IRD.InventoryMaterialId,IRD.IsAsset,IRD.MaterialStorageId,IR.GRNDate
					) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

					--left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
					--left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					--left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
					--left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
					--left join [HKP].[MaterialStorage] MS on ms.id=opbal2.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null



					----------------------------Issue------------------------------
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IFD1.IsAsset--,IFD1.InventoryMaterialId,IFD1.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                           
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,isnull(IFD1.IssueQty,0) IssueForThePeriod	
					,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id						
					--              

					left join (select IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, Sum(IH.Qty) IssueQty , sum(IH.Qty*IH.Rate) PolicyAmount--Sum(IH.TotalAmount) PolicyAmount--Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
					FROM TRN.InventoryIssueDetail IID  
					LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
					LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
					LEFT join trn.InventoryReceiveDetail IRD On IRD.Id=IH.InventoryReceiveDetailId
					LEft JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					--AND IR.OpeningBalanceId IS  NULL 
					GROUP BY IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					) IFD1 On IFD1.InventoryMaterialId=IM.Id

                        
					--left join [HKP].[MaterialStorage] MS on ms.id=IFD1.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null


					---------------------Issue Return-----------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IssueReturnData.IsAsset--,IssueReturnData.InventoryMaterialId,IssueReturnData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,IssueReturnData.Qty IssueReturnQtyForThePeriod	
					,IssueReturnData.IssueReturnAmount IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							
					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	                       
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty)*sum(IRD.MaterialTranRate)) IssueReturnAmount 
					From trn.InventoryIssueReturnHistory IH
					Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
					Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
					Left JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=IssueReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-----------------Opening Purchase Return ------------------------------
					
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					, TUoM.UserName UOM, PurchaseReturnDataOpening.IsAsset--,PurchaseReturnData.InventoryMaterialId,PurchaseReturnData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,PurchaseReturnDataOpening.PurchaseReturnOpeningQty PurchaseReturnOpeningQty	
					,PurchaseReturnDataOpening.PurchaseReturnOpeningAmount PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IH.TransactionQty) PurchaseReturnOpeningQty,sum(IRD.MaterialTranRate) MaterialTranOpeningRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnOpeningAmount 
						from trn.PurchaseReturnDetail IH
						Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.POReturnDate) < '" + fromDate + @"'  AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)PurchaseReturnDataOpening ON PurchaseReturnDataOpening.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=PurchaseReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null


					
					----------------------Purchase return-------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					, TUoM.UserName UOM, PurchaseReturnData.IsAsset--,PurchaseReturnData.InventoryMaterialId,PurchaseReturnData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					 --PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,PurchaseReturnData.PurchaseReturnQty PurchaseReturnQtyForThePeriod	
					,PurchaseReturnData.PurchaseReturnAmount PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IH.TransactionQty) PurchaseReturnQty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
						from trn.PurchaseReturnDetail IH
						Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.POReturnDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=PurchaseReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					------------------------------- Adjustment-----------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, AdjustmentData.IsAsset--,AdjustmentData.InventoryMaterialId,AdjustmentData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                          

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Adjust Return
					,AdjustmentData.PhysicalStockAdjustmentQty AdjustmentQtyForThePeriod	
					,AdjustmentData.PhysicalStockAdjustmentAmount AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
				
					Left join (select psad.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
							  ,sum(IH.Qty) PhysicalStockAdjustmentQty,sum(IH.Rate) Rate, (sum(IH.Qty)*sum(IH.Rate)) PhysicalStockAdjustmentAmount 
						from trn.PhysicalStockAdjustmentHistory IH
						Left JOIN TRN.PhysicalStockAdjustmentDetail psad on psad.Id=IH.PhysicalStockAdjustmentDetailId
						Left join trn.PhysicalStockAdjustmentMaster II ON II.Id=psad.PhysicalStockAdjustmentMasterID
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
						GROUP BY psad.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)AdjustmentData ON AdjustmentData.InventoryMaterialId=IM.Id
                      
					--left join [HKP].[MaterialStorage] MS on ms.id=AdjustmentData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null


					----------------------- InventorySales--------------------------------
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventorySalesData.IsAsset--,InventorySalesData.InventoryMaterialId,InventorySalesData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	


					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,InventorySalesData.InventorySalesQty InventorySalesQtyForThePeriod	
					,InventorySalesData.InventorySalesAmount InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
						
					Left join (select ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
							  ,sum(ISH.Qty) InventorySalesQty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty)*sum(ISH.BaseRate)) InventorySalesAmount 
						from [TRN].[InventorySalesHistory] ISH
						Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
						Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=ISH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,Ins.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND Ins.PlantId='" + plantId + @"'
						GROUP BY ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                                
					--left join [HKP].[MaterialStorage] MS on ms.id=InventorySalesData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					------------------------- InventoryScrap---------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventoryScrapData.IsAsset--,InventoryScrapData.InventoryMaterialId,InventoryScrapData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,InventoryScrapData.InventoryScrapQty InventoryScrapQtyForThePeriod	
					,InventoryScrapData.InventoryScrapAmount InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

						

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select ISCD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
						,sum(ISCH.Qty) InventoryScrapQty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
						from [TRN].[InventoryScrapHistory] ISCH
						Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
						Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=ISCH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,ISC.ScrapDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' 
						GROUP BY ISCD.InventoryMaterialId,IRD.IsAsset,IRD.MaterialStorageId,IR.GRNDate
					)InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id  
						
					--left join [HKP].[MaterialStorage] MS on ms.id=InventoryScrapData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where   IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					----------------------- Inventory Transfer------------------
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventoryTransferData.IsAsset--,InventoryTransferData.InventoryMaterialId,InventoryTransferData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,InventoryTransferData.InventoryTransferDataQty InventoryTransferQtyForThePeriod	
					,InventoryTransferData.InventoryTransferAmount InventoryTransferForThePeriodAmount

							
					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	
					Left join (Select IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IRD.InventoryTransferQty) InventoryTransferDataQty,sum(IRD.MaterialTranRate) Rate, (sum(IRD.InventoryTransferQty)*sum(IRD.MaterialTranRate)) InventoryTransferAmount 
					FROM [TRN].[InventoryTransferHistory] ITH
					Left JOIN [TRN].[InventoryReceiveDetail] IRD on IRD.Id=ITH.InventoryReceiveDetailId
					Left join [TRN].[InventoryReceive] IR on IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND IR.PlantId='" + plantId + @"'
					GROUP BY IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id   
						
					--left join [HKP].[MaterialStorage] MS on ms.id=InventoryTransferData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null
					)x
					where not
					(ISNULL(OpeningBalance,0)=0
				    and  ISNULL(OpeningIssueQty,0)=0
					and  ISNULL(ReceivedForThePeriod,0)=0
					and  ISNULL(IssueForThePeriod,0)=0
					and  ISNULL(IssueReturnQtyForThePeriod,0)=0
					AND ISNULL(PurchaseReturnOpeningQty,0)=0 
					and  ISNULL(PurchaseReturnQtyForThePeriod,0)=0
					and  ISNULL(AdjustmentQtyForThePeriod,0)=0
					and  ISNULL(InventorySalesQtyForThePeriod,0)=0
					and  ISNULL(InventoryScrapQtyForThePeriod,0)=0
					and  ISNULL(InventoryTransferQtyForThePeriod,0)=0)
					--Where convert(Date,x.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' --and InventoryMaterialId='103' --x.MaterialStorageLocation is not NULL
					Group BY x.MaterialMasterName,x.MaterialMasterId,x.HSNCode,x.ArticleName,x.ArticleId,x.FirstCharacteristicsValue,x.SecondCharacteristicsValue,x.ThirdCharacteristicsValue,x.UOM,ISNULL(x.IsAsset,0)
					--,x.MaterialStorageLocation
					--,x.InventoryMaterialId
					--,x.GRNDate
					";
				}
				else if (Asset == "false" && Inventory == "true" && Country == "false")
				{

					cmdText = @"
					select x.MaterialMasterName	
					,x.MaterialMasterId	
					,x.HSNCode
					,x.ArticleName	
					,x.ArticleId		
					,x.FirstCharacteristicsValue
					,x.SecondCharacteristicsValue
					,x.ThirdCharacteristicsValue 
					--,x.MaterialStorageLocation	
					--,x.InventoryMaterialId
					--,x.GRNDate
					,x.UOM,ISNULL(x.IsAsset,0) IsAsset
					,(SUM( x.OpeningBalance)-Sum(x.OpeningIssueQty)-sum(ISNULL(x.PurchaseReturnOpeningQty,0)))  OpeningBalance
					,(Sum(x.OpeningBalanceAmount)-Sum(x.PolicyAmount)-sum(isnull(PurchaseReturnOpeningAmount,0))) OpeningBalanceAmount
					,Sum(x.OpeningIssueQty) OpeningIssueQty
					,Sum(x.PolicyAmount) PolicyAmount
					,sum(x.ReceivedForThePeriod) ReceivedForThePeriod
					,Sum(x.ReceivedForThePeriodAmount) ReceivedForThePeriodAmount
					,Sum(x.IssueForThePeriod) IssueForThePeriod
					,Sum(x.IssueForThePeriodAmount) IssueForThePeriodAmount 
					,sum(x.IssueReturnQtyForThePeriod) IssueReturnQtyForThePeriod
					,sum(x.IssueReturnForThePeriodAmount) IssueReturnForThePeriodAmount

					,sum(ISNULL(x.PurchaseReturnOpeningQty,0))  PurchaseReturnOpeningQty
					,sum(isnull(PurchaseReturnOpeningAmount,0)) PurchaseReturnOpeningAmount

					,sum(x.PurchaseReturnQtyForThePeriod) PurchaseReturnQtyForThePeriod
					,sum(x.PurchaseReturnForThePeriodAmount) PurchaseReturnForThePeriodAmount
					,sum(x.AdjustmentQtyForThePeriod) AdjustmentQtyForThePeriod
					,sum(x.AdjustmentForThePeriodAmount) AdjustmentForThePeriodAmount
					,sum(x.InventorySalesQtyForThePeriod) InventorySalesQtyForThePeriod
					,sum(x.InventorySalesForThePeriodAmount) InventorySalesForThePeriodAmount
					,Sum(x.InventoryScrapQtyForThePeriod) InventoryScrapQtyForThePeriod
					,Sum(x.InventoryScrapForThePeriodAmount) InventoryScrapForThePeriodAmount
					,Sum(x.InventoryTransferQtyForThePeriod) InventoryTransferQtyForThePeriod
					,Sum(x.InventoryTransferForThePeriodAmount) InventoryTransferForThePeriodAmount							
					,sum(((((((((isnull(x.OpeningBalance,0)-isnull(x.OpeningIssueQty,0) + isnull(x.ReceivedForThePeriod,0))-isnull(x.IssueForThePeriod,0)) + isnull(x.IssueReturnQtyForThePeriod,0))  - ISNULL(x.PurchaseReturnQtyForThePeriod,0))- isnull(x.AdjustmentQtyForThePeriod,0))-isnull(x.InventorySalesQtyForThePeriod,0))-isnull(x.InventoryScrapQtyForThePeriod,0))-Isnull(InventoryTransferQtyForThePeriod,0))- ISNULL(x.PurchaseReturnOpeningQty,0)) Closing 
					,sum((((((((isnull(x.OpeningBalanceAmount,0)-isnull(x.PolicyAmount,0) + isnull(x.ReceivedForThePeriodAmount,0))-isnull(x.IssueForThePeriodAmount,0)-isnull(x.AdjustmentForThePeriodAmount,0))-isnull(x.PurchaseReturnForThePeriodAmount,0))+isnull(x.IssueReturnForThePeriodAmount,0)-isnull(x.InventorySalesForThePeriodAmount,0))-isnull(x.InventoryScrapForThePeriodAmount,0))-isnull(x.InventoryTransferForThePeriodAmount,0))-isnull(PurchaseReturnOpeningAmount,0))) ClosingAmount



					FROM (
					-------------------------Opening Balance--------------

					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, opbal.IsAsset--,opbal.InventoryMaterialId,opbal.GRNDate
							
					--Opening Balance
					,isnull(opbal.TransactionQty,0) As OpeningBalance	
					,isnull(opbal.TotalMaterialBooksCurrencyAmount,0) AS OpeningBalanceAmount

					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					--Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left join( select t.InventoryMaterialId,t.IsAsset--,t.MaterialStorageId,t.GRNDate
							  , sum(t.TransactionQty) TransactionQty,sum(t.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount--,Sum(t.OpeningIssueQty) OpeningIssueQty,Sum(t.OpeningIssueAmt)  OpeningIssueAmt 
					from (
					SELECT IRD.InventoryMaterialId,IRD.IsAsset--, IRD.MaterialStorageId,IR.GRNDate
							,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
					--SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount, Sum(IRD.IssueQty) OpeningIssueQty, Round(Sum(IRD.IssueQty * IRD.MaterialTranRate* IR.ToCurrencyRate),2) OpeningIssueAmt
					FROM [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate) <= '" + fromDate + @"'
					AND IR.OpeningBalanceId IS NOT NULL 
					group By IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					UNION ALL
					SELECT IRD.InventoryMaterialId,IRD.IsAsset--, IRD.MaterialStorageId,IR.GRNDate
							,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
						--SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Round(Sum(IRD.TransactionQty* IRD.MaterialTranRate*IR.ToCurrencyRate),2) TotalMaterialBooksCurrencyAmount, Sum(IRD.IssueQty) OpeningIssueQty, Round(Sum(IRD.IssueQty * IRD.MaterialTranRate* IR.ToCurrencyRate),2) OpeningIssueAmt
					FROM [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate) < '" + fromDate + @"' AND IR.OpeningBalanceId IS NULL 
					group By IRD.InventoryMaterialId,IRD.IsAsset--,--,IRD.MaterialStorageId,IR.GRNDate
					)as t group by t.InventoryMaterialId,t.IsAsset--,t.MaterialStorageId,t.GRNDate
					) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                       
					--left join [HKP].[MaterialStorage] MS on ms.id=opbal.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					--------------------------Opening Issued-----------------------
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IFD3.IsAsset--,IFD3.InventoryMaterialId,IFD3.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,IFD3.OpeningIssueQty
					,IFD3.PolicyAmount
                          

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left join (select IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, sum(IIH.Qty) OpeningIssueQty,Sum(IIH.TotalAmount) PolicyAmount--Sum(IID.TransactionQty) OpeningIssueQty, Sum(IID.PolicyAmount) PolicyAmount
					FROM TRN.InventoryIssueDetail IID  
					LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	
					Left JOIN TRN.InventoryIssueHistory IIH ON IIH.InventoryIssueDetailId=IID.Id
					Left JOIn TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
					Left JOIn Trn.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) < '" + fromDate + @"' AND II.PlantId='" + plantId + @"' AND IR.OpeningBalanceId IS NULL
					GROUP BY IID.InventoryMaterialId,IRD.IsAsset
					--,IRD.MaterialStorageId,IR.GRNDate
					) IFD3 On IFD3.InventoryMaterialId=IM.Id
                       
					--left join [HKP].[MaterialStorage] MS on ms.id=IFD3.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-----------------------------Received--------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, opbal2.IsAsset--,opbal2.InventoryMaterialId,opbal2.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                          

					--   --Receive

					,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
					,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount

					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

						

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id						
					left join(SELECT IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
					FROM  [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate)		
					BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' --AND IR.OpeningBalanceId IS  NULL 
					GROUP By IRD.InventoryMaterialId,IRD.IsAsset,IRD.MaterialStorageId,IR.GRNDate
					) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

					--left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
					--left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					--left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
					--left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
					--left join [HKP].[MaterialStorage] MS on ms.id=opbal2.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					---------------------------------Issue--------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IFD1.IsAsset--,IFD1.InventoryMaterialId,IFD1.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                           
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,isnull(IFD1.IssueQty,0) IssueForThePeriod	
					,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id						
					--              

					left join (select IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, Sum(IH.Qty) IssueQty , sum(IH.TotalAmount) PolicyAmount--Sum(IH.TotalAmount) PolicyAmount--Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
					FROM TRN.InventoryIssueDetail IID  
					LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
					LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
					LEFT join trn.InventoryReceiveDetail IRD On IRD.Id=IH.InventoryReceiveDetailId
					LEft JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					--AND IR.OpeningBalanceId IS  NULL 
					GROUP BY IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					) IFD1 On IFD1.InventoryMaterialId=IM.Id

                        
					--left join [HKP].[MaterialStorage] MS on ms.id=IFD1.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-------------------------------Issue Return-------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IssueReturnData.IsAsset--,IssueReturnData.InventoryMaterialId,IssueReturnData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,IssueReturnData.Qty IssueReturnQtyForThePeriod	
					,IssueReturnData.IssueReturnAmount IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							
					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	                       
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty)*sum(IRD.MaterialTranRate)) IssueReturnAmount 
					From trn.InventoryIssueReturnHistory IH
					Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
					Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
					Left JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=IssueReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-----------------Opening Purchase Return ------------------------------
					
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					, TUoM.UserName UOM, PurchaseReturnDataOpening.IsAsset--,PurchaseReturnData.InventoryMaterialId,PurchaseReturnData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,PurchaseReturnDataOpening.PurchaseReturnOpeningQty PurchaseReturnOpeningQty	
					,PurchaseReturnDataOpening.PurchaseReturnOpeningAmount PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IH.TransactionQty) PurchaseReturnOpeningQty,sum(IRD.MaterialTranRate) MaterialTranOpeningRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnOpeningAmount 
						from trn.PurchaseReturnDetail IH
						Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.POReturnDate) < '" + fromDate + @"'  AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)PurchaseReturnDataOpening ON PurchaseReturnDataOpening.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=PurchaseReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null


					---------------------------------------Purchase return-------------------------------
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					, TUoM.UserName UOM, PurchaseReturnData.IsAsset--,PurchaseReturnData.InventoryMaterialId,PurchaseReturnData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					 --PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount


					--Purchase Return
					,PurchaseReturnData.PurchaseReturnQty PurchaseReturnQtyForThePeriod	
					,PurchaseReturnData.PurchaseReturnAmount PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IH.TransactionQty) PurchaseReturnQty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
						from trn.PurchaseReturnDetail IH
						Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.POReturnDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=PurchaseReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null


					---------------------------------- Adjustment-------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, AdjustmentData.IsAsset--,AdjustmentData.InventoryMaterialId,AdjustmentData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                          

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					 --PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,AdjustmentData.PhysicalStockAdjustmentQty AdjustmentQtyForThePeriod	
					,AdjustmentData.PhysicalStockAdjustmentAmount AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
				
					Left join (select psad.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
							  ,sum(IH.Qty) PhysicalStockAdjustmentQty,sum(IH.Rate) Rate, (sum(IH.Qty)*sum(IH.Rate)) PhysicalStockAdjustmentAmount 
						from trn.PhysicalStockAdjustmentHistory IH
						Left JOIN TRN.PhysicalStockAdjustmentDetail psad on psad.Id=IH.PhysicalStockAdjustmentDetailId
						Left join trn.PhysicalStockAdjustmentMaster II ON II.Id=psad.PhysicalStockAdjustmentMasterID
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
						GROUP BY psad.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)AdjustmentData ON AdjustmentData.InventoryMaterialId=IM.Id
                      
					--left join [HKP].[MaterialStorage] MS on ms.id=AdjustmentData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					--------------------------------- InventorySales----------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventorySalesData.IsAsset--,InventorySalesData.InventoryMaterialId,InventorySalesData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					 
					 --PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,InventorySalesData.InventorySalesQty InventorySalesQtyForThePeriod	
					,InventorySalesData.InventorySalesAmount InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
						
					Left join (select ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
							  ,sum(ISH.Qty) InventorySalesQty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty)*sum(ISH.BaseRate)) InventorySalesAmount 
						from [TRN].[InventorySalesHistory] ISH
						Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
						Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=ISH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,Ins.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND Ins.PlantId='" + plantId + @"'
						GROUP BY ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                                
					--left join [HKP].[MaterialStorage] MS on ms.id=InventorySalesData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					--------------------------------- InventoryScrap----------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventoryScrapData.IsAsset--,InventoryScrapData.InventoryMaterialId,InventoryScrapData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					 --PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount


					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,InventoryScrapData.InventoryScrapQty InventoryScrapQtyForThePeriod	
					,InventoryScrapData.InventoryScrapAmount InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

						

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select ISCD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
						,sum(ISCH.Qty) InventoryScrapQty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
						from [TRN].[InventoryScrapHistory] ISCH
						Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
						Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=ISCH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,ISC.ScrapDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' 
						GROUP BY ISCD.InventoryMaterialId,IRD.IsAsset,IRD.MaterialStorageId,IR.GRNDate
					)InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id  
						
					--left join [HKP].[MaterialStorage] MS on ms.id=InventoryScrapData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					---------------------------------------------- Inventory Transfer----------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventoryTransferData.IsAsset--,InventoryTransferData.InventoryMaterialId,InventoryTransferData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount

					 --PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,InventoryTransferData.InventoryTransferDataQty InventoryTransferQtyForThePeriod	
					,InventoryTransferData.InventoryTransferAmount InventoryTransferForThePeriodAmount

							
					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	
					Left join (Select IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IRD.InventoryTransferQty) InventoryTransferDataQty,sum(IRD.MaterialTranRate) Rate, (sum(IRD.InventoryTransferQty)*sum(IRD.MaterialTranRate)) InventoryTransferAmount 
					FROM [TRN].[InventoryTransferHistory] ITH
					Left JOIN [TRN].[InventoryReceiveDetail] IRD on IRD.Id=ITH.InventoryReceiveDetailId
					Left join [TRN].[InventoryReceive] IR on IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND IR.PlantId='" + plantId + @"'
					GROUP BY IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id   
						
					--left join [HKP].[MaterialStorage] MS on ms.id=InventoryTransferData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null
					)x
					where not
					(ISNULL(OpeningBalance,0)=0
				    and  ISNULL(OpeningIssueQty,0)=0
					and  ISNULL(ReceivedForThePeriod,0)=0
					and  ISNULL(IssueForThePeriod,0)=0
					and  ISNULL(IssueReturnQtyForThePeriod,0)=0
					AND  ISNULL(PurchaseReturnOpeningQty,0)=0				

					and  ISNULL(PurchaseReturnQtyForThePeriod,0)=0
					and  ISNULL(AdjustmentQtyForThePeriod,0)=0
					and  ISNULL(InventorySalesQtyForThePeriod,0)=0
					and  ISNULL(InventoryScrapQtyForThePeriod,0)=0
					and  ISNULL(InventoryTransferQtyForThePeriod,0)=0)
					--Where convert(Date,x.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' --and InventoryMaterialId='103' --x.MaterialStorageLocation is not NULL
					Group BY x.MaterialMasterName,x.MaterialMasterId,x.HSNCode,x.ArticleName,x.ArticleId,x.FirstCharacteristicsValue,x.SecondCharacteristicsValue,x.ThirdCharacteristicsValue,x.UOM,ISNULL(x.IsAsset,0)
					--,x.MaterialStorageLocation
					--,x.InventoryMaterialId
					--,x.GRNDate
					";

				}
				else if (Asset == "true" && Inventory == "false" && Country == "false")
				{
					cmdText = @"
					select x.MaterialMasterName	
					,x.MaterialMasterId	
					,x.HSNCode
					,x.ArticleName	
					,x.ArticleId		
					,x.FirstCharacteristicsValue
					,x.SecondCharacteristicsValue
					,x.ThirdCharacteristicsValue 
					--,x.MaterialStorageLocation	
					--,x.InventoryMaterialId
					--,x.GRNDate
					,x.UOM,ISNULL(x.IsAsset,0) IsAsset
					,(SUM( x.OpeningBalance)-Sum(x.OpeningIssueQty)-sum(ISNULL(x.PurchaseReturnOpeningQty,0)))  OpeningBalance
					,(Sum(x.OpeningBalanceAmount)-Sum(x.PolicyAmount)-sum(isnull(PurchaseReturnOpeningAmount,0))) OpeningBalanceAmount
					,Sum(x.OpeningIssueQty) OpeningIssueQty
					,Sum(x.PolicyAmount) PolicyAmount
					,sum(x.ReceivedForThePeriod) ReceivedForThePeriod
					,Sum(x.ReceivedForThePeriodAmount) ReceivedForThePeriodAmount
					,Sum(x.IssueForThePeriod) IssueForThePeriod
					,Sum(x.IssueForThePeriodAmount) IssueForThePeriodAmount 
					,sum(x.IssueReturnQtyForThePeriod) IssueReturnQtyForThePeriod
					,sum(x.IssueReturnForThePeriodAmount) IssueReturnForThePeriodAmount

					,sum(ISNULL(x.PurchaseReturnOpeningQty,0))  PurchaseReturnOpeningQty
					,sum(isnull(PurchaseReturnOpeningAmount,0)) PurchaseReturnOpeningAmount

					,sum(x.PurchaseReturnQtyForThePeriod) PurchaseReturnQtyForThePeriod
					,sum(x.PurchaseReturnForThePeriodAmount) PurchaseReturnForThePeriodAmount
					,sum(x.AdjustmentQtyForThePeriod) AdjustmentQtyForThePeriod
					,sum(x.AdjustmentForThePeriodAmount) AdjustmentForThePeriodAmount
					,sum(x.InventorySalesQtyForThePeriod) InventorySalesQtyForThePeriod
					,sum(x.InventorySalesForThePeriodAmount) InventorySalesForThePeriodAmount
					,Sum(x.InventoryScrapQtyForThePeriod) InventoryScrapQtyForThePeriod
					,Sum(x.InventoryScrapForThePeriodAmount) InventoryScrapForThePeriodAmount
					,Sum(x.InventoryTransferQtyForThePeriod) InventoryTransferQtyForThePeriod
					,Sum(x.InventoryTransferForThePeriodAmount) InventoryTransferForThePeriodAmount							
					,sum(((((((((isnull(x.OpeningBalance,0)-isnull(x.OpeningIssueQty,0) + isnull(x.ReceivedForThePeriod,0))-isnull(x.IssueForThePeriod,0)) + isnull(x.IssueReturnQtyForThePeriod,0))  - ISNULL(x.PurchaseReturnQtyForThePeriod,0))- isnull(x.AdjustmentQtyForThePeriod,0))-isnull(x.InventorySalesQtyForThePeriod,0))-isnull(x.InventoryScrapQtyForThePeriod,0))-Isnull(InventoryTransferQtyForThePeriod,0))- ISNULL(x.PurchaseReturnOpeningQty,0)) Closing 
					,sum((((((((isnull(x.OpeningBalanceAmount,0)-isnull(x.PolicyAmount,0) + isnull(x.ReceivedForThePeriodAmount,0))-isnull(x.IssueForThePeriodAmount,0)-isnull(x.AdjustmentForThePeriodAmount,0))-isnull(x.PurchaseReturnForThePeriodAmount,0))+isnull(x.IssueReturnForThePeriodAmount,0)-isnull(x.InventorySalesForThePeriodAmount,0))-isnull(x.InventoryScrapForThePeriodAmount,0))-isnull(x.InventoryTransferForThePeriodAmount,0))-isnull(PurchaseReturnOpeningAmount,0))) ClosingAmount




					FROM (
					-----------------------Opening Balance------------------------

					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, opbal.IsAsset--,opbal.InventoryMaterialId,opbal.GRNDate
							
					--Opening Balance
					,isnull(opbal.TransactionQty,0) As OpeningBalance	
					,isnull(opbal.TotalMaterialBooksCurrencyAmount,0) AS OpeningBalanceAmount

					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					--Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left join( select t.InventoryMaterialId,t.IsAsset--,t.MaterialStorageId,t.GRNDate
							  , sum(t.TransactionQty) TransactionQty,sum(t.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount--,Sum(t.OpeningIssueQty) OpeningIssueQty,Sum(t.OpeningIssueAmt)  OpeningIssueAmt 
					from (
					SELECT IRD.InventoryMaterialId,IRD.IsAsset--, IRD.MaterialStorageId,IR.GRNDate
							,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
					--SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount, Sum(IRD.IssueQty) OpeningIssueQty, Round(Sum(IRD.IssueQty * IRD.MaterialTranRate* IR.ToCurrencyRate),2) OpeningIssueAmt
					FROM [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate) <= '" + fromDate + @"'
					AND IR.OpeningBalanceId IS NOT NULL 
					group By IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					UNION ALL
					SELECT IRD.InventoryMaterialId,IRD.IsAsset--, IRD.MaterialStorageId,IR.GRNDate
							,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
						--SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Round(Sum(IRD.TransactionQty* IRD.MaterialTranRate*IR.ToCurrencyRate),2) TotalMaterialBooksCurrencyAmount, Sum(IRD.IssueQty) OpeningIssueQty, Round(Sum(IRD.IssueQty * IRD.MaterialTranRate* IR.ToCurrencyRate),2) OpeningIssueAmt
					FROM [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate) < '" + fromDate + @"' AND IR.OpeningBalanceId IS NULL 
					group By IRD.InventoryMaterialId,IRD.IsAsset--,--,IRD.MaterialStorageId,IR.GRNDate
					)as t group by t.InventoryMaterialId,t.IsAsset--,t.MaterialStorageId,t.GRNDate
					) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                       
					--left join [HKP].[MaterialStorage] MS on ms.id=opbal.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-----------------------------Opening Issued----------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IFD3.IsAsset--,IFD3.InventoryMaterialId,IFD3.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,IFD3.OpeningIssueQty
					,IFD3.PolicyAmount
                          

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left join (select IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, sum(IIH.Qty) OpeningIssueQty,Sum(IIH.TotalAmount) PolicyAmount--Sum(IID.TransactionQty) OpeningIssueQty, Sum(IID.PolicyAmount) PolicyAmount
					FROM TRN.InventoryIssueDetail IID  
					LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	
					Left JOIN TRN.InventoryIssueHistory IIH ON IIH.InventoryIssueDetailId=IID.Id
					Left JOIn TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
					Left JOIn Trn.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) < '" + fromDate + @"' AND II.PlantId='" + plantId + @"' AND IR.OpeningBalanceId IS NULL
					GROUP BY IID.InventoryMaterialId,IRD.IsAsset
					--,IRD.MaterialStorageId,IR.GRNDate
					) IFD3 On IFD3.InventoryMaterialId=IM.Id
                       
					--left join [HKP].[MaterialStorage] MS on ms.id=IFD3.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					---------------------------------Received------------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, opbal2.IsAsset--,opbal2.InventoryMaterialId,opbal2.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                          

					--   --Receive

					,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
					,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

						

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id						
					left join(SELECT IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
					FROM  [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate)		
					BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' --AND IR.OpeningBalanceId IS  NULL 
					GROUP By IRD.InventoryMaterialId,IRD.IsAsset,IRD.MaterialStorageId,IR.GRNDate
					) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

					--left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
					--left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					--left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
					--left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
					--left join [HKP].[MaterialStorage] MS on ms.id=opbal2.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					--------------------------------------Issue-------------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IFD1.IsAsset--,IFD1.InventoryMaterialId,IFD1.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                           
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,isnull(IFD1.IssueQty,0) IssueForThePeriod	
					,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id						
					--              

					left join (select IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, Sum(IH.Qty) IssueQty , sum(IH.Qty*IH.Rate) PolicyAmount--Sum(IH.TotalAmount) PolicyAmount--Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
					FROM TRN.InventoryIssueDetail IID  
					LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
					LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
					LEFT join trn.InventoryReceiveDetail IRD On IRD.Id=IH.InventoryReceiveDetailId
					LEft JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					--AND IR.OpeningBalanceId IS  NULL 
					GROUP BY IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					) IFD1 On IFD1.InventoryMaterialId=IM.Id

                        
					--left join [HKP].[MaterialStorage] MS on ms.id=IFD1.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-----------------------------------Issue Return-----------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IssueReturnData.IsAsset--,IssueReturnData.InventoryMaterialId,IssueReturnData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,IssueReturnData.Qty IssueReturnQtyForThePeriod	
					,IssueReturnData.IssueReturnAmount IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							
					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	                       
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty)*sum(IRD.MaterialTranRate)) IssueReturnAmount 
					From trn.InventoryIssueReturnHistory IH
					Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
					Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
					Left JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=IssueReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-----------------Opening Purchase Return ------------------------------
					
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					, TUoM.UserName UOM, PurchaseReturnDataOpening.IsAsset--,PurchaseReturnData.InventoryMaterialId,PurchaseReturnData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,PurchaseReturnDataOpening.PurchaseReturnOpeningQty PurchaseReturnOpeningQty	
					,PurchaseReturnDataOpening.PurchaseReturnOpeningAmount PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IH.TransactionQty) PurchaseReturnOpeningQty,sum(IRD.MaterialTranRate) MaterialTranOpeningRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnOpeningAmount 
						from trn.PurchaseReturnDetail IH
						Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.POReturnDate) <'"+fromDate+ @"'  AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)PurchaseReturnDataOpening ON PurchaseReturnDataOpening.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=PurchaseReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-------------------------------Purchase return--------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					, TUoM.UserName UOM, PurchaseReturnData.IsAsset--,PurchaseReturnData.InventoryMaterialId,PurchaseReturnData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,PurchaseReturnData.PurchaseReturnQty PurchaseReturnQtyForThePeriod	
					,PurchaseReturnData.PurchaseReturnAmount PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IH.TransactionQty) PurchaseReturnQty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
						from trn.PurchaseReturnDetail IH
						Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.POReturnDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=PurchaseReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-------------------------------------- Adjustment-----------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, AdjustmentData.IsAsset--,AdjustmentData.InventoryMaterialId,AdjustmentData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                          

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,AdjustmentData.PhysicalStockAdjustmentQty AdjustmentQtyForThePeriod	
					,AdjustmentData.PhysicalStockAdjustmentAmount AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
				
					Left join (select psad.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
							  ,sum(IH.Qty) PhysicalStockAdjustmentQty,sum(IH.Rate) Rate, (sum(IH.Qty)*sum(IH.Rate)) PhysicalStockAdjustmentAmount 
						from trn.PhysicalStockAdjustmentHistory IH
						Left JOIN TRN.PhysicalStockAdjustmentDetail psad on psad.Id=IH.PhysicalStockAdjustmentDetailId
						Left join trn.PhysicalStockAdjustmentMaster II ON II.Id=psad.PhysicalStockAdjustmentMasterID
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
						GROUP BY psad.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)AdjustmentData ON AdjustmentData.InventoryMaterialId=IM.Id
                      
					--left join [HKP].[MaterialStorage] MS on ms.id=AdjustmentData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					------------------------------------ InventorySales-------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventorySalesData.IsAsset--,InventorySalesData.InventoryMaterialId,InventorySalesData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,InventorySalesData.InventorySalesQty InventorySalesQtyForThePeriod	
					,InventorySalesData.InventorySalesAmount InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
						
					Left join (select ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
							  ,sum(ISH.Qty) InventorySalesQty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty)*sum(ISH.BaseRate)) InventorySalesAmount 
						from [TRN].[InventorySalesHistory] ISH
						Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
						Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=ISH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,Ins.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND Ins.PlantId='" + plantId + @"'
						GROUP BY ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                                
					--left join [HKP].[MaterialStorage] MS on ms.id=InventorySalesData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  MM.IsAsset=1 AND IM.PlantId='202034' AND MM.UserName is not null

					----------------------------------- InventoryScrap--------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventoryScrapData.IsAsset--,InventoryScrapData.InventoryMaterialId,InventoryScrapData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,InventoryScrapData.InventoryScrapQty InventoryScrapQtyForThePeriod	
					,InventoryScrapData.InventoryScrapAmount InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

						

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select ISCD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
						,sum(ISCH.Qty) InventoryScrapQty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
						from [TRN].[InventoryScrapHistory] ISCH
						Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
						Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=ISCH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,ISC.ScrapDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' 
						GROUP BY ISCD.InventoryMaterialId,IRD.IsAsset,IRD.MaterialStorageId,IR.GRNDate
					)InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id  
						
					--left join [HKP].[MaterialStorage] MS on ms.id=InventoryScrapData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					---------------------------- Inventory Transfer--------------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventoryTransferData.IsAsset--,InventoryTransferData.InventoryMaterialId,InventoryTransferData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount


					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,InventoryTransferData.InventoryTransferDataQty InventoryTransferQtyForThePeriod	
					,InventoryTransferData.InventoryTransferAmount InventoryTransferForThePeriodAmount

							
					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	
					Left join (Select IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IRD.InventoryTransferQty) InventoryTransferDataQty,sum(IRD.MaterialTranRate) Rate, (sum(IRD.InventoryTransferQty)*sum(IRD.MaterialTranRate)) InventoryTransferAmount 
					FROM [TRN].[InventoryTransferHistory] ITH
					Left JOIN [TRN].[InventoryReceiveDetail] IRD on IRD.Id=ITH.InventoryReceiveDetailId
					Left join [TRN].[InventoryReceive] IR on IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND IR.PlantId='" + plantId + @"'
					GROUP BY IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id   
						
					--left join [HKP].[MaterialStorage] MS on ms.id=InventoryTransferData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null
					)x
					where not
					(ISNULL(OpeningBalance,0)=0
				    and  ISNULL(OpeningIssueQty,0)=0
					and  ISNULL(ReceivedForThePeriod,0)=0
					and  ISNULL(IssueForThePeriod,0)=0
					and  ISNULL(IssueReturnQtyForThePeriod,0)=0
					and  ISNULL(PurchaseReturnOpeningQty,0)=0
					and  ISNULL(PurchaseReturnQtyForThePeriod,0)=0
					and  ISNULL(AdjustmentQtyForThePeriod,0)=0
					and  ISNULL(InventorySalesQtyForThePeriod,0)=0
					and  ISNULL(InventoryScrapQtyForThePeriod,0)=0
					and  ISNULL(InventoryTransferQtyForThePeriod,0)=0)
					--Where convert(Date,x.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' --and InventoryMaterialId='103' --x.MaterialStorageLocation is not NULL
					Group BY x.MaterialMasterName,x.MaterialMasterId,x.HSNCode,x.ArticleName,x.ArticleId,x.FirstCharacteristicsValue,x.SecondCharacteristicsValue,x.ThirdCharacteristicsValue,x.UOM,ISNULL(x.IsAsset,0)
					--,x.MaterialStorageLocation
					--,x.InventoryMaterialId
					--,x.GRNDate
					";



				}
				#endregion
				#region WITH country
				else if (Asset == "true" && Inventory == "true" && Country == "true")
				{

					cmdText = @"
					select x.MaterialMasterName	
					,x.MaterialMasterId	
					,x.HSNCode
					,x.ArticleName	
					,x.ArticleId		
					,x.FirstCharacteristicsValue
					,x.SecondCharacteristicsValue
					,x.ThirdCharacteristicsValue 
					--,x.MaterialStorageLocation	
					--,x.InventoryMaterialId
					--,x.GRNDate
					,x.CountryName
					,x.UOM,ISNULL(x.IsAsset,0) IsAsset
					,(SUM( x.OpeningBalance)-Sum(x.OpeningIssueQty)-sum(ISNULL(x.PurchaseReturnOpeningQty,0)))  OpeningBalance
					,(Sum(x.OpeningBalanceAmount)-Sum(x.PolicyAmount)-sum(isnull(PurchaseReturnOpeningAmount,0))) OpeningBalanceAmount
					,Sum(x.OpeningIssueQty) OpeningIssueQty
					,Sum(x.PolicyAmount) PolicyAmount
					,sum(x.ReceivedForThePeriod) ReceivedForThePeriod
					,Sum(x.ReceivedForThePeriodAmount) ReceivedForThePeriodAmount
					,Sum(x.IssueForThePeriod) IssueForThePeriod
					,Sum(x.IssueForThePeriodAmount) IssueForThePeriodAmount 
					,sum(x.IssueReturnQtyForThePeriod) IssueReturnQtyForThePeriod
					,sum(x.IssueReturnForThePeriodAmount) IssueReturnForThePeriodAmount

					,sum(ISNULL(x.PurchaseReturnOpeningQty,0))  PurchaseReturnOpeningQty
					,sum(isnull(PurchaseReturnOpeningAmount,0)) PurchaseReturnOpeningAmount

					,sum(x.PurchaseReturnQtyForThePeriod) PurchaseReturnQtyForThePeriod
					,sum(x.PurchaseReturnForThePeriodAmount) PurchaseReturnForThePeriodAmount
					,sum(x.AdjustmentQtyForThePeriod) AdjustmentQtyForThePeriod
					,sum(x.AdjustmentForThePeriodAmount) AdjustmentForThePeriodAmount
					,sum(x.InventorySalesQtyForThePeriod) InventorySalesQtyForThePeriod
					,sum(x.InventorySalesForThePeriodAmount) InventorySalesForThePeriodAmount
					,Sum(x.InventoryScrapQtyForThePeriod) InventoryScrapQtyForThePeriod
					,Sum(x.InventoryScrapForThePeriodAmount) InventoryScrapForThePeriodAmount
					,Sum(x.InventoryTransferQtyForThePeriod) InventoryTransferQtyForThePeriod
					,Sum(x.InventoryTransferForThePeriodAmount) InventoryTransferForThePeriodAmount							
					,sum(((((((((isnull(x.OpeningBalance,0)-isnull(x.OpeningIssueQty,0) + isnull(x.ReceivedForThePeriod,0))-isnull(x.IssueForThePeriod,0)) + isnull(x.IssueReturnQtyForThePeriod,0))  - ISNULL(x.PurchaseReturnQtyForThePeriod,0))- isnull(x.AdjustmentQtyForThePeriod,0))-isnull(x.InventorySalesQtyForThePeriod,0))-isnull(x.InventoryScrapQtyForThePeriod,0))-Isnull(InventoryTransferQtyForThePeriod,0))- ISNULL(x.PurchaseReturnOpeningQty,0)) Closing 
					,sum((((((((isnull(x.OpeningBalanceAmount,0)-isnull(x.PolicyAmount,0) + isnull(x.ReceivedForThePeriodAmount,0))-isnull(x.IssueForThePeriodAmount,0)-isnull(x.AdjustmentForThePeriodAmount,0))-isnull(x.PurchaseReturnForThePeriodAmount,0))+isnull(x.IssueReturnForThePeriodAmount,0)-isnull(x.InventorySalesForThePeriodAmount,0))-isnull(x.InventoryScrapForThePeriodAmount,0))-isnull(x.InventoryTransferForThePeriodAmount,0))-isnull(PurchaseReturnOpeningAmount,0))) ClosingAmount


					FROM (
					--------------------------Opening Balance----------------------------------

					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, opbal.IsAsset--,opbal.InventoryMaterialId,opbal.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,isnull(opbal.TransactionQty,0) As OpeningBalance	
					,isnull(opbal.TotalMaterialBooksCurrencyAmount,0) AS OpeningBalanceAmount

					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					--Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left join( select t.InventoryMaterialId,t.IsAsset--,t.MaterialStorageId,t.GRNDate
							  , sum(t.TransactionQty) TransactionQty,sum(t.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount--,Sum(t.OpeningIssueQty) OpeningIssueQty,Sum(t.OpeningIssueAmt)  OpeningIssueAmt 
					from (
					SELECT IRD.InventoryMaterialId,IRD.IsAsset--, IRD.MaterialStorageId,IR.GRNDate
							,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
					--SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount, Sum(IRD.IssueQty) OpeningIssueQty, Round(Sum(IRD.IssueQty * IRD.MaterialTranRate* IR.ToCurrencyRate),2) OpeningIssueAmt
					FROM [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate) <= '" + fromDate + @"'
					AND IR.OpeningBalanceId IS NOT NULL 
					group By IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					UNION ALL
					SELECT IRD.InventoryMaterialId,IRD.IsAsset--, IRD.MaterialStorageId,IR.GRNDate
							,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
						--SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Round(Sum(IRD.TransactionQty* IRD.MaterialTranRate*IR.ToCurrencyRate),2) TotalMaterialBooksCurrencyAmount, Sum(IRD.IssueQty) OpeningIssueQty, Round(Sum(IRD.IssueQty * IRD.MaterialTranRate* IR.ToCurrencyRate),2) OpeningIssueAmt
					FROM [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate) < '" + fromDate + @"' AND IR.OpeningBalanceId IS NULL 
					group By IRD.InventoryMaterialId,IRD.IsAsset--,--,IRD.MaterialStorageId,IR.GRNDate
					)as t group by t.InventoryMaterialId,t.IsAsset--,t.MaterialStorageId,t.GRNDate
					) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                       
					--left join [HKP].[MaterialStorage] MS on ms.id=opbal.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					------------------------------Opening Issued---------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IFD3.IsAsset--,IFD3.InventoryMaterialId,IFD3.GRNDate
				    ,C.UserName CountryName			
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,IFD3.OpeningIssueQty
					,IFD3.PolicyAmount
                          

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left join (select IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, sum(IIH.Qty) OpeningIssueQty,Sum(IIH.TotalAmount) PolicyAmount--Sum(IID.TransactionQty) OpeningIssueQty, Sum(IID.PolicyAmount) PolicyAmount
					FROM TRN.InventoryIssueDetail IID  
					LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	
					Left JOIN TRN.InventoryIssueHistory IIH ON IIH.InventoryIssueDetailId=IID.Id
					Left JOIn TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
					Left JOIn Trn.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) < '" + fromDate + @"' AND II.PlantId='" + plantId + @"' AND IR.OpeningBalanceId IS NULL
					GROUP BY IID.InventoryMaterialId,IRD.IsAsset
					--,IRD.MaterialStorageId,IR.GRNDate
					) IFD3 On IFD3.InventoryMaterialId=IM.Id
                       
					--left join [HKP].[MaterialStorage] MS on ms.id=IFD3.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where   IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					--------------------------------Received--------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, opbal2.IsAsset--,opbal2.InventoryMaterialId,opbal2.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                          

					--   --Receive

					,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
					,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

						

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id						
					left join(SELECT IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
					FROM  [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate)		
					BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' --AND IR.OpeningBalanceId IS  NULL 
					GROUP By IRD.InventoryMaterialId,IRD.IsAsset,IRD.MaterialStorageId,IR.GRNDate
					) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

					--left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
					--left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					--left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
					--left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
					--left join [HKP].[MaterialStorage] MS on ms.id=opbal2.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					--------------------------------Issue---------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IFD1.IsAsset--,IFD1.InventoryMaterialId,IFD1.GRNDate
					,C.UserName CountryName	
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                           
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,isnull(IFD1.IssueQty,0) IssueForThePeriod	
					,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id						
					--              

					left join (select IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, Sum(IH.Qty) IssueQty , sum(IH.Qty*IH.Rate) PolicyAmount--Sum(IH.TotalAmount) PolicyAmount--Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
					FROM TRN.InventoryIssueDetail IID  
					LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
					LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
					LEFT join trn.InventoryReceiveDetail IRD On IRD.Id=IH.InventoryReceiveDetailId
					LEft JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					--AND IR.OpeningBalanceId IS  NULL 
					GROUP BY IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					) IFD1 On IFD1.InventoryMaterialId=IM.Id

                        
					--left join [HKP].[MaterialStorage] MS on ms.id=IFD1.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					------------------------------Issue Return-------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IssueReturnData.IsAsset--,IssueReturnData.InventoryMaterialId,IssueReturnData.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,IssueReturnData.Qty IssueReturnQtyForThePeriod	
					,IssueReturnData.IssueReturnAmount IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							
					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	                       
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty)*sum(IRD.MaterialTranRate)) IssueReturnAmount 
					From trn.InventoryIssueReturnHistory IH
					Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
					Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
					Left JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=IssueReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id		
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-----------------Opening Purchase Return ------------------------------
					
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					, TUoM.UserName UOM, PurchaseReturnDataOpening.IsAsset--,PurchaseReturnData.InventoryMaterialId,PurchaseReturnData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,PurchaseReturnDataOpening.PurchaseReturnOpeningQty PurchaseReturnOpeningQty	
					,PurchaseReturnDataOpening.PurchaseReturnOpeningAmount PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IH.TransactionQty) PurchaseReturnOpeningQty,sum(IRD.MaterialTranRate) MaterialTranOpeningRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnOpeningAmount 
						from trn.PurchaseReturnDetail IH
						Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.POReturnDate) < '"+fromDate+@"'  AND II.PlantId='"+plantId+ @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)PurchaseReturnDataOpening ON PurchaseReturnDataOpening.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=PurchaseReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					----------------------------------Purchase return-----------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					, TUoM.UserName UOM, PurchaseReturnData.IsAsset--,PurchaseReturnData.InventoryMaterialId,PurchaseReturnData.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,PurchaseReturnData.PurchaseReturnQty PurchaseReturnQtyForThePeriod	
					,PurchaseReturnData.PurchaseReturnAmount PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IH.TransactionQty) PurchaseReturnQty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
						from trn.PurchaseReturnDetail IH
						Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.POReturnDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=PurchaseReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					----------------------------------------------- Adjustment--------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, AdjustmentData.IsAsset--,AdjustmentData.InventoryMaterialId,AdjustmentData.GRNDate
					,c.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                          

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,AdjustmentData.PhysicalStockAdjustmentQty AdjustmentQtyForThePeriod	
					,AdjustmentData.PhysicalStockAdjustmentAmount AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
				
					Left join (select psad.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
							  ,sum(IH.Qty) PhysicalStockAdjustmentQty,sum(IH.Rate) Rate, (sum(IH.Qty)*sum(IH.Rate)) PhysicalStockAdjustmentAmount 
						from trn.PhysicalStockAdjustmentHistory IH
						Left JOIN TRN.PhysicalStockAdjustmentDetail psad on psad.Id=IH.PhysicalStockAdjustmentDetailId
						Left join trn.PhysicalStockAdjustmentMaster II ON II.Id=psad.PhysicalStockAdjustmentMasterID
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
						GROUP BY psad.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)AdjustmentData ON AdjustmentData.InventoryMaterialId=IM.Id
                      
					--left join [HKP].[MaterialStorage] MS on ms.id=AdjustmentData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					----------------------------------- InventorySales---------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventorySalesData.IsAsset--,InventorySalesData.InventoryMaterialId,InventorySalesData.GRNDate
					,c.UserName CountryName
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,InventorySalesData.InventorySalesQty InventorySalesQtyForThePeriod	
					,InventorySalesData.InventorySalesAmount InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
						
					Left join (select ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
							  ,sum(ISH.Qty) InventorySalesQty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty)*sum(ISH.BaseRate)) InventorySalesAmount 
						from [TRN].[InventorySalesHistory] ISH
						Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
						Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=ISH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,Ins.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND Ins.PlantId='" + plantId + @"'
						GROUP BY ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                                
					--left join [HKP].[MaterialStorage] MS on ms.id=InventorySalesData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id		
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					------------------------------------- InventoryScrap-------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventoryScrapData.IsAsset--,InventoryScrapData.InventoryMaterialId,InventoryScrapData.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,InventoryScrapData.InventoryScrapQty InventoryScrapQtyForThePeriod	
					,InventoryScrapData.InventoryScrapAmount InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

						

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select ISCD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
						,sum(ISCH.Qty) InventoryScrapQty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
						from [TRN].[InventoryScrapHistory] ISCH
						Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
						Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=ISCH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,ISC.ScrapDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' 
						GROUP BY ISCD.InventoryMaterialId,IRD.IsAsset,IRD.MaterialStorageId,IR.GRNDate
					)InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id  
						
					--left join [HKP].[MaterialStorage] MS on ms.id=InventoryScrapData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id		
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where   IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					---------------------------- Inventory Transfer--------------------------
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventoryTransferData.IsAsset--,InventoryTransferData.InventoryMaterialId,InventoryTransferData.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,InventoryTransferData.InventoryTransferDataQty InventoryTransferQtyForThePeriod	
					,InventoryTransferData.InventoryTransferAmount InventoryTransferForThePeriodAmount

							
					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	
					Left join (Select IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IRD.InventoryTransferQty) InventoryTransferDataQty,sum(IRD.MaterialTranRate) Rate, (sum(IRD.InventoryTransferQty)*sum(IRD.MaterialTranRate)) InventoryTransferAmount 
					FROM [TRN].[InventoryTransferHistory] ITH
					Left JOIN [TRN].[InventoryReceiveDetail] IRD on IRD.Id=ITH.InventoryReceiveDetailId
					Left join [TRN].[InventoryReceive] IR on IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND IR.PlantId='" + plantId + @"'
					GROUP BY IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id   
						
					--left join [HKP].[MaterialStorage] MS on ms.id=InventoryTransferData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null
					)x
					where not
					(ISNULL(OpeningBalance,0)=0
				    and  ISNULL(OpeningIssueQty,0)=0
					and  ISNULL(ReceivedForThePeriod,0)=0
					and  ISNULL(IssueForThePeriod,0)=0
					and  ISNULL(IssueReturnQtyForThePeriod,0)=0

					and  ISNULL(PurchaseReturnOpeningQty,0)=0				

					and  ISNULL(PurchaseReturnQtyForThePeriod,0)=0
					and  ISNULL(AdjustmentQtyForThePeriod,0)=0
					and  ISNULL(InventorySalesQtyForThePeriod,0)=0
					and  ISNULL(InventoryScrapQtyForThePeriod,0)=0
					and  ISNULL(InventoryTransferQtyForThePeriod,0)=0)
					--Where convert(Date,x.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' --and InventoryMaterialId='103' --x.MaterialStorageLocation is not NULL
					Group BY x.MaterialMasterName,x.MaterialMasterId,x.HSNCode,x.ArticleName,x.ArticleId,x.FirstCharacteristicsValue,x.SecondCharacteristicsValue,x.ThirdCharacteristicsValue,x.UOM,ISNULL(x.IsAsset,0),x.CountryName
					--,x.MaterialStorageLocation
					--,x.InventoryMaterialId
					--,x.GRNDate
					";

				}
				else if (Asset == "true" && Inventory == "false" && Country == "true")
				{


					cmdText = @"
					select x.MaterialMasterName	
					,x.MaterialMasterId	
					,x.HSNCode
					,x.ArticleName	
					,x.ArticleId		
					,x.FirstCharacteristicsValue
					,x.SecondCharacteristicsValue
					,x.ThirdCharacteristicsValue 
					--,x.MaterialStorageLocation	
					--,x.InventoryMaterialId
					--,x.GRNDate
					,x.CountryName
					,x.UOM,ISNULL(x.IsAsset,0) IsAsset
					,(SUM( x.OpeningBalance)-Sum(x.OpeningIssueQty)-sum(ISNULL(x.PurchaseReturnOpeningQty,0)))  OpeningBalance
					,(Sum(x.OpeningBalanceAmount)-Sum(x.PolicyAmount)-sum(isnull(PurchaseReturnOpeningAmount,0))) OpeningBalanceAmount
					,Sum(x.OpeningIssueQty) OpeningIssueQty
					,Sum(x.PolicyAmount) PolicyAmount
					,sum(x.ReceivedForThePeriod) ReceivedForThePeriod
					,Sum(x.ReceivedForThePeriodAmount) ReceivedForThePeriodAmount
					,Sum(x.IssueForThePeriod) IssueForThePeriod
					,Sum(x.IssueForThePeriodAmount) IssueForThePeriodAmount 
					,sum(x.IssueReturnQtyForThePeriod) IssueReturnQtyForThePeriod
					,sum(x.IssueReturnForThePeriodAmount) IssueReturnForThePeriodAmount

					,sum(ISNULL(x.PurchaseReturnOpeningQty,0))  PurchaseReturnOpeningQty
					,sum(isnull(PurchaseReturnOpeningAmount,0)) PurchaseReturnOpeningAmount

					,sum(x.PurchaseReturnQtyForThePeriod) PurchaseReturnQtyForThePeriod
					,sum(x.PurchaseReturnForThePeriodAmount) PurchaseReturnForThePeriodAmount
					,sum(x.AdjustmentQtyForThePeriod) AdjustmentQtyForThePeriod
					,sum(x.AdjustmentForThePeriodAmount) AdjustmentForThePeriodAmount
					,sum(x.InventorySalesQtyForThePeriod) InventorySalesQtyForThePeriod
					,sum(x.InventorySalesForThePeriodAmount) InventorySalesForThePeriodAmount
					,Sum(x.InventoryScrapQtyForThePeriod) InventoryScrapQtyForThePeriod
					,Sum(x.InventoryScrapForThePeriodAmount) InventoryScrapForThePeriodAmount
					,Sum(x.InventoryTransferQtyForThePeriod) InventoryTransferQtyForThePeriod
					,Sum(x.InventoryTransferForThePeriodAmount) InventoryTransferForThePeriodAmount							
					,sum(((((((((isnull(x.OpeningBalance,0)-isnull(x.OpeningIssueQty,0) + isnull(x.ReceivedForThePeriod,0))-isnull(x.IssueForThePeriod,0)) + isnull(x.IssueReturnQtyForThePeriod,0))  - ISNULL(x.PurchaseReturnQtyForThePeriod,0))- isnull(x.AdjustmentQtyForThePeriod,0))-isnull(x.InventorySalesQtyForThePeriod,0))-isnull(x.InventoryScrapQtyForThePeriod,0))-Isnull(InventoryTransferQtyForThePeriod,0))- ISNULL(x.PurchaseReturnOpeningQty,0)) Closing 
					,sum((((((((isnull(x.OpeningBalanceAmount,0)-isnull(x.PolicyAmount,0) + isnull(x.ReceivedForThePeriodAmount,0))-isnull(x.IssueForThePeriodAmount,0)-isnull(x.AdjustmentForThePeriodAmount,0))-isnull(x.PurchaseReturnForThePeriodAmount,0))+isnull(x.IssueReturnForThePeriodAmount,0)-isnull(x.InventorySalesForThePeriodAmount,0))-isnull(x.InventoryScrapForThePeriodAmount,0))-isnull(x.InventoryTransferForThePeriodAmount,0))-isnull(PurchaseReturnOpeningAmount,0))) ClosingAmount



					FROM (
					-----------------------Opening Balance-----------------------

					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, opbal.IsAsset--,opbal.InventoryMaterialId,opbal.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,isnull(opbal.TransactionQty,0) As OpeningBalance	
					,isnull(opbal.TotalMaterialBooksCurrencyAmount,0) AS OpeningBalanceAmount

					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					--Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left join( select t.InventoryMaterialId,t.IsAsset--,t.MaterialStorageId,t.GRNDate
							  , sum(t.TransactionQty) TransactionQty,sum(t.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount--,Sum(t.OpeningIssueQty) OpeningIssueQty,Sum(t.OpeningIssueAmt)  OpeningIssueAmt 
					from (
					SELECT IRD.InventoryMaterialId,IRD.IsAsset--, IRD.MaterialStorageId,IR.GRNDate
							,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
					--SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount, Sum(IRD.IssueQty) OpeningIssueQty, Round(Sum(IRD.IssueQty * IRD.MaterialTranRate* IR.ToCurrencyRate),2) OpeningIssueAmt
					FROM [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate) <= '" + fromDate + @"'
					AND IR.OpeningBalanceId IS NOT NULL 
					group By IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					UNION ALL
					SELECT IRD.InventoryMaterialId,IRD.IsAsset--, IRD.MaterialStorageId,IR.GRNDate
							,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
						--SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Round(Sum(IRD.TransactionQty* IRD.MaterialTranRate*IR.ToCurrencyRate),2) TotalMaterialBooksCurrencyAmount, Sum(IRD.IssueQty) OpeningIssueQty, Round(Sum(IRD.IssueQty * IRD.MaterialTranRate* IR.ToCurrencyRate),2) OpeningIssueAmt
					FROM [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate) < '" + fromDate + @"' AND IR.OpeningBalanceId IS NULL 
					group By IRD.InventoryMaterialId,IRD.IsAsset--,--,IRD.MaterialStorageId,IR.GRNDate
					)as t group by t.InventoryMaterialId,t.IsAsset--,t.MaterialStorageId,t.GRNDate
					) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                       
					--left join [HKP].[MaterialStorage] MS on ms.id=opbal.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					----------------------------Opening Issued-------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IFD3.IsAsset--,IFD3.InventoryMaterialId,IFD3.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,IFD3.OpeningIssueQty
					,IFD3.PolicyAmount
                          

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left join (select IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, sum(IIH.Qty) OpeningIssueQty,Sum(IIH.TotalAmount) PolicyAmount--Sum(IID.TransactionQty) OpeningIssueQty, Sum(IID.PolicyAmount) PolicyAmount
					FROM TRN.InventoryIssueDetail IID  
					LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	
					Left JOIN TRN.InventoryIssueHistory IIH ON IIH.InventoryIssueDetailId=IID.Id
					Left JOIn TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
					Left JOIn Trn.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) < '" + fromDate + @"' AND II.PlantId='" + plantId + @"' AND IR.OpeningBalanceId IS NULL
					GROUP BY IID.InventoryMaterialId,IRD.IsAsset
					--,IRD.MaterialStorageId,IR.GRNDate
					) IFD3 On IFD3.InventoryMaterialId=IM.Id
                       
					--left join [HKP].[MaterialStorage] MS on ms.id=IFD3.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-----------------------Received-------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, opbal2.IsAsset--,opbal2.InventoryMaterialId,opbal2.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                          

					--   --Receive

					,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
					,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

						

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id						
					left join(SELECT IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
					FROM  [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate)		
					BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' --AND IR.OpeningBalanceId IS  NULL 
					GROUP By IRD.InventoryMaterialId,IRD.IsAsset,IRD.MaterialStorageId,IR.GRNDate
					) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

					--left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
					--left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					--left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
					--left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
					--left join [HKP].[MaterialStorage] MS on ms.id=opbal2.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id		
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					----------------------------------Issue-----------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IFD1.IsAsset--,IFD1.InventoryMaterialId,IFD1.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                           
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,isnull(IFD1.IssueQty,0) IssueForThePeriod	
					,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id						
					--              

					left join (select IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, Sum(IH.Qty) IssueQty , sum(IH.Qty*IH.Rate) PolicyAmount--Sum(IH.TotalAmount) PolicyAmount--Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
					FROM TRN.InventoryIssueDetail IID  
					LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
					LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
					LEFT join trn.InventoryReceiveDetail IRD On IRD.Id=IH.InventoryReceiveDetailId
					LEft JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					--AND IR.OpeningBalanceId IS  NULL 
					GROUP BY IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					) IFD1 On IFD1.InventoryMaterialId=IM.Id

                        
					--left join [HKP].[MaterialStorage] MS on ms.id=IFD1.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id		
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					--------------------------------------Issue Return-----------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IssueReturnData.IsAsset--,IssueReturnData.InventoryMaterialId,IssueReturnData.GRNDate
						,C.UserName CountryName	
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,IssueReturnData.Qty IssueReturnQtyForThePeriod	
					,IssueReturnData.IssueReturnAmount IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							
					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	                       
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty)*sum(IRD.MaterialTranRate)) IssueReturnAmount 
					From trn.InventoryIssueReturnHistory IH
					Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
					Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
					Left JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=IssueReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id		
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-----------------Opening Purchase Return ------------------------------
					
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					, TUoM.UserName UOM, PurchaseReturnDataOpening.IsAsset--,PurchaseReturnData.InventoryMaterialId,PurchaseReturnData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,PurchaseReturnDataOpening.PurchaseReturnOpeningQty PurchaseReturnOpeningQty	
					,PurchaseReturnDataOpening.PurchaseReturnOpeningAmount PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IH.TransactionQty) PurchaseReturnOpeningQty,sum(IRD.MaterialTranRate) MaterialTranOpeningRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnOpeningAmount 
						from trn.PurchaseReturnDetail IH
						Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.POReturnDate) <'"+fromDate+@"'  AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)PurchaseReturnDataOpening ON PurchaseReturnDataOpening.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=PurchaseReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where IM.PlantId='" + plantId+@"' AND MM.UserName is not null

					--------------------------Purchase return----------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					, TUoM.UserName UOM, PurchaseReturnData.IsAsset--,PurchaseReturnData.InventoryMaterialId,PurchaseReturnData.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,PurchaseReturnData.PurchaseReturnQty PurchaseReturnQtyForThePeriod	
					,PurchaseReturnData.PurchaseReturnAmount PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IH.TransactionQty) PurchaseReturnQty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
						from trn.PurchaseReturnDetail IH
						Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.POReturnDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=PurchaseReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-------------------------- Adjustment------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, AdjustmentData.IsAsset--,AdjustmentData.InventoryMaterialId,AdjustmentData.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                          

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,AdjustmentData.PhysicalStockAdjustmentQty AdjustmentQtyForThePeriod	
					,AdjustmentData.PhysicalStockAdjustmentAmount AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
				
					Left join (select psad.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
							  ,sum(IH.Qty) PhysicalStockAdjustmentQty,sum(IH.Rate) Rate, (sum(IH.Qty)*sum(IH.Rate)) PhysicalStockAdjustmentAmount 
						from trn.PhysicalStockAdjustmentHistory IH
						Left JOIN TRN.PhysicalStockAdjustmentDetail psad on psad.Id=IH.PhysicalStockAdjustmentDetailId
						Left join trn.PhysicalStockAdjustmentMaster II ON II.Id=psad.PhysicalStockAdjustmentMasterID
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
						GROUP BY psad.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)AdjustmentData ON AdjustmentData.InventoryMaterialId=IM.Id
                      
					--left join [HKP].[MaterialStorage] MS on ms.id=AdjustmentData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					----------------------------------------- InventorySales----------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventorySalesData.IsAsset--,InventorySalesData.InventoryMaterialId,InventorySalesData.GRNDate
					,c.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,InventorySalesData.InventorySalesQty InventorySalesQtyForThePeriod	
					,InventorySalesData.InventorySalesAmount InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
						
					Left join (select ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
							  ,sum(ISH.Qty) InventorySalesQty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty)*sum(ISH.BaseRate)) InventorySalesAmount 
						from [TRN].[InventorySalesHistory] ISH
						Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
						Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=ISH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND Ins.PlantId='" + plantId + @"'
						GROUP BY ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                                
					--left join [HKP].[MaterialStorage] MS on ms.id=InventorySalesData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					----------------------------------- InventoryScrap---------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventoryScrapData.IsAsset--,InventoryScrapData.InventoryMaterialId,InventoryScrapData.GRNDate
						,C.UserName CountryName	
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,InventoryScrapData.InventoryScrapQty InventoryScrapQtyForThePeriod	
					,InventoryScrapData.InventoryScrapAmount InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

						

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select ISCD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
						,sum(ISCH.Qty) InventoryScrapQty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
						from [TRN].[InventoryScrapHistory] ISCH
						Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
						Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=ISCH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,ISC.ScrapDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' 
						GROUP BY ISCD.InventoryMaterialId,IRD.IsAsset,IRD.MaterialStorageId,IR.GRNDate
					)InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id  
						
					--left join [HKP].[MaterialStorage] MS on ms.id=InventoryScrapData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					---------------------------- Inventory Transfer-----------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventoryTransferData.IsAsset--,InventoryTransferData.InventoryMaterialId,InventoryTransferData.GRNDate
						,C.UserName CountryName	
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,InventoryTransferData.InventoryTransferDataQty InventoryTransferQtyForThePeriod	
					,InventoryTransferData.InventoryTransferAmount InventoryTransferForThePeriodAmount

							
					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	
					Left join (Select IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IRD.InventoryTransferQty) InventoryTransferDataQty,sum(IRD.MaterialTranRate) Rate, (sum(IRD.InventoryTransferQty)*sum(IRD.MaterialTranRate)) InventoryTransferAmount 
					FROM [TRN].[InventoryTransferHistory] ITH
					Left JOIN [TRN].[InventoryReceiveDetail] IRD on IRD.Id=ITH.InventoryReceiveDetailId
					Left join [TRN].[InventoryReceive] IR on IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND IR.PlantId='" + plantId + @"'
					GROUP BY IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id   
						
					--left join [HKP].[MaterialStorage] MS on ms.id=InventoryTransferData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null
					)x
					where not
					(ISNULL(OpeningBalance,0)=0
				    and  ISNULL(OpeningIssueQty,0)=0
					and  ISNULL(ReceivedForThePeriod,0)=0
					and  ISNULL(IssueForThePeriod,0)=0
					and  ISNULL(IssueReturnQtyForThePeriod,0)=0
					and  ISNULL(PurchaseReturnOpeningQty,0)=0
					and  ISNULL(PurchaseReturnQtyForThePeriod,0)=0
					and  ISNULL(AdjustmentQtyForThePeriod,0)=0
					and  ISNULL(InventorySalesQtyForThePeriod,0)=0
					and  ISNULL(InventoryScrapQtyForThePeriod,0)=0
					and  ISNULL(InventoryTransferQtyForThePeriod,0)=0)
					--Where convert(Date,x.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' --and InventoryMaterialId='103' --x.MaterialStorageLocation is not NULL
					Group BY x.MaterialMasterName,x.MaterialMasterId,x.HSNCode,x.ArticleName,x.ArticleId,x.FirstCharacteristicsValue,x.SecondCharacteristicsValue,x.ThirdCharacteristicsValue,x.UOM,ISNULL(x.IsAsset,0),x.CountryName
					--,x.MaterialStorageLocation
					--,x.InventoryMaterialId
					--,x.GRNDate
					";

				}
				else if (Asset == "false" && Inventory == "true" && Country == "true")
				{


					cmdText = @"
					select x.MaterialMasterName	
					,x.MaterialMasterId	
					,x.HSNCode
					,x.ArticleName	
					,x.ArticleId		
					,x.FirstCharacteristicsValue
					,x.SecondCharacteristicsValue
					,x.ThirdCharacteristicsValue 
					--,x.MaterialStorageLocation	
					--,x.InventoryMaterialId
					--,x.GRNDate
					,x.CountryName
					,x.UOM,ISNULL(x.IsAsset,0) IsAsset
					,(SUM( x.OpeningBalance)-Sum(x.OpeningIssueQty)-sum(ISNULL(x.PurchaseReturnOpeningQty,0)))  OpeningBalance
					,(Sum(x.OpeningBalanceAmount)-Sum(x.PolicyAmount)-sum(isnull(PurchaseReturnOpeningAmount,0))) OpeningBalanceAmount
					,Sum(x.OpeningIssueQty) OpeningIssueQty
					,Sum(x.PolicyAmount) PolicyAmount
					,sum(x.ReceivedForThePeriod) ReceivedForThePeriod
					,Sum(x.ReceivedForThePeriodAmount) ReceivedForThePeriodAmount
					,Sum(x.IssueForThePeriod) IssueForThePeriod
					,Sum(x.IssueForThePeriodAmount) IssueForThePeriodAmount 
					,sum(x.IssueReturnQtyForThePeriod) IssueReturnQtyForThePeriod
					,sum(x.IssueReturnForThePeriodAmount) IssueReturnForThePeriodAmount

					,sum(ISNULL(x.PurchaseReturnOpeningQty,0))  PurchaseReturnOpeningQty
					,sum(isnull(PurchaseReturnOpeningAmount,0)) PurchaseReturnOpeningAmount

					,sum(x.PurchaseReturnQtyForThePeriod) PurchaseReturnQtyForThePeriod
					,sum(x.PurchaseReturnForThePeriodAmount) PurchaseReturnForThePeriodAmount
					,sum(x.AdjustmentQtyForThePeriod) AdjustmentQtyForThePeriod
					,sum(x.AdjustmentForThePeriodAmount) AdjustmentForThePeriodAmount
					,sum(x.InventorySalesQtyForThePeriod) InventorySalesQtyForThePeriod
					,sum(x.InventorySalesForThePeriodAmount) InventorySalesForThePeriodAmount
					,Sum(x.InventoryScrapQtyForThePeriod) InventoryScrapQtyForThePeriod
					,Sum(x.InventoryScrapForThePeriodAmount) InventoryScrapForThePeriodAmount
					,Sum(x.InventoryTransferQtyForThePeriod) InventoryTransferQtyForThePeriod
					,Sum(x.InventoryTransferForThePeriodAmount) InventoryTransferForThePeriodAmount							
					,sum(((((((((isnull(x.OpeningBalance,0)-isnull(x.OpeningIssueQty,0) + isnull(x.ReceivedForThePeriod,0))-isnull(x.IssueForThePeriod,0)) + isnull(x.IssueReturnQtyForThePeriod,0))  - ISNULL(x.PurchaseReturnQtyForThePeriod,0))- isnull(x.AdjustmentQtyForThePeriod,0))-isnull(x.InventorySalesQtyForThePeriod,0))-isnull(x.InventoryScrapQtyForThePeriod,0))-Isnull(InventoryTransferQtyForThePeriod,0))- ISNULL(x.PurchaseReturnOpeningQty,0)) Closing 
					,sum((((((((isnull(x.OpeningBalanceAmount,0)-isnull(x.PolicyAmount,0) + isnull(x.ReceivedForThePeriodAmount,0))-isnull(x.IssueForThePeriodAmount,0)-isnull(x.AdjustmentForThePeriodAmount,0))-isnull(x.PurchaseReturnForThePeriodAmount,0))+isnull(x.IssueReturnForThePeriodAmount,0)-isnull(x.InventorySalesForThePeriodAmount,0))-isnull(x.InventoryScrapForThePeriodAmount,0))-isnull(x.InventoryTransferForThePeriodAmount,0))-isnull(PurchaseReturnOpeningAmount,0))) ClosingAmount



					FROM (
					-----------------------------Opening Balance-------------------------------

					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, opbal.IsAsset--,opbal.InventoryMaterialId,opbal.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,isnull(opbal.TransactionQty,0) As OpeningBalance	
					,isnull(opbal.TotalMaterialBooksCurrencyAmount,0) AS OpeningBalanceAmount

					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					--Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left join( select t.InventoryMaterialId,t.IsAsset--,t.MaterialStorageId,t.GRNDate
							  , sum(t.TransactionQty) TransactionQty,sum(t.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount--,Sum(t.OpeningIssueQty) OpeningIssueQty,Sum(t.OpeningIssueAmt)  OpeningIssueAmt 
					from (
					SELECT IRD.InventoryMaterialId,IRD.IsAsset--, IRD.MaterialStorageId,IR.GRNDate
							,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
					--SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount, Sum(IRD.IssueQty) OpeningIssueQty, Round(Sum(IRD.IssueQty * IRD.MaterialTranRate* IR.ToCurrencyRate),2) OpeningIssueAmt
					FROM [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate) <= '" + fromDate + @"'
					AND IR.OpeningBalanceId IS NOT NULL 
					group By IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					UNION ALL
					SELECT IRD.InventoryMaterialId,IRD.IsAsset--, IRD.MaterialStorageId,IR.GRNDate
							,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
						--SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Round(Sum(IRD.TransactionQty* IRD.MaterialTranRate*IR.ToCurrencyRate),2) TotalMaterialBooksCurrencyAmount, Sum(IRD.IssueQty) OpeningIssueQty, Round(Sum(IRD.IssueQty * IRD.MaterialTranRate* IR.ToCurrencyRate),2) OpeningIssueAmt
					FROM [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate) < '" + fromDate + @"' AND IR.OpeningBalanceId IS NULL 
					group By IRD.InventoryMaterialId,IRD.IsAsset--,--,IRD.MaterialStorageId,IR.GRNDate
					)as t group by t.InventoryMaterialId,t.IsAsset--,t.MaterialStorageId,t.GRNDate
					) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                       
					--left join [HKP].[MaterialStorage] MS on ms.id=opbal.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					----------------------------Opening Issued------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IFD3.IsAsset--,IFD3.InventoryMaterialId,IFD3.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,IFD3.OpeningIssueQty
					,IFD3.PolicyAmount
                          

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left join (select IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, sum(IIH.Qty) OpeningIssueQty,Sum(IIH.TotalAmount) PolicyAmount--Sum(IID.TransactionQty) OpeningIssueQty, Sum(IID.PolicyAmount) PolicyAmount
					FROM TRN.InventoryIssueDetail IID  
					LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	
					Left JOIN TRN.InventoryIssueHistory IIH ON IIH.InventoryIssueDetailId=IID.Id
					Left JOIn TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
					Left JOIn Trn.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) < '" + fromDate + @"' AND II.PlantId='" + plantId + @"' AND IR.OpeningBalanceId IS NULL
					GROUP BY IID.InventoryMaterialId,IRD.IsAsset
					--,IRD.MaterialStorageId,IR.GRNDate
					) IFD3 On IFD3.InventoryMaterialId=IM.Id
                       
					--left join [HKP].[MaterialStorage] MS on ms.id=IFD3.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					---------------------------------------Received-------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, opbal2.IsAsset--,opbal2.InventoryMaterialId,opbal2.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                          

					--   --Receive

					,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
					,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

						

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id						
					left join(SELECT IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
					FROM  [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate)		
					BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' --AND IR.OpeningBalanceId IS  NULL 
					GROUP By IRD.InventoryMaterialId,IRD.IsAsset,IRD.MaterialStorageId,IR.GRNDate
					) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

					--left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
					--left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					--left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
					--left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
					--left join [HKP].[MaterialStorage] MS on ms.id=opbal2.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id		
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					----------------------------Issue-------------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IFD1.IsAsset--,IFD1.InventoryMaterialId,IFD1.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                           
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,isnull(IFD1.IssueQty,0) IssueForThePeriod	
					,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id						
					--              

					left join (select IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, Sum(IH.Qty) IssueQty , sum(IH.Qty*IH.Rate) PolicyAmount--Sum(IH.TotalAmount) PolicyAmount--Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
					FROM TRN.InventoryIssueDetail IID  
					LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
					LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
					LEFT join trn.InventoryReceiveDetail IRD On IRD.Id=IH.InventoryReceiveDetailId
					LEft JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					--AND IR.OpeningBalanceId IS  NULL 
					GROUP BY IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					) IFD1 On IFD1.InventoryMaterialId=IM.Id

                        
					--left join [HKP].[MaterialStorage] MS on ms.id=IFD1.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id		
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					--------------------------------------Issue Return-------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IssueReturnData.IsAsset--,IssueReturnData.InventoryMaterialId,IssueReturnData.GRNDate
						,C.UserName CountryName	
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,IssueReturnData.Qty IssueReturnQtyForThePeriod	
					,IssueReturnData.IssueReturnAmount IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							
					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	                       
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty)*sum(IRD.MaterialTranRate)) IssueReturnAmount 
					From trn.InventoryIssueReturnHistory IH
					Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
					Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
					Left JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=IssueReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id		
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-----------------Opening Purchase Return ------------------------------
					
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					, TUoM.UserName UOM, PurchaseReturnDataOpening.IsAsset--,PurchaseReturnData.InventoryMaterialId,PurchaseReturnData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,PurchaseReturnDataOpening.PurchaseReturnOpeningQty PurchaseReturnOpeningQty	
					,PurchaseReturnDataOpening.PurchaseReturnOpeningAmount PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IH.TransactionQty) PurchaseReturnOpeningQty,sum(IRD.MaterialTranRate) MaterialTranOpeningRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnOpeningAmount 
						from trn.PurchaseReturnDetail IH
						Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.POReturnDate) <'" + fromDate + @"'  AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)PurchaseReturnDataOpening ON PurchaseReturnDataOpening.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=PurchaseReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					--------------------------------------------Purchase return-----------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					, TUoM.UserName UOM, PurchaseReturnData.IsAsset--,PurchaseReturnData.InventoryMaterialId,PurchaseReturnData.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,PurchaseReturnData.PurchaseReturnQty PurchaseReturnQtyForThePeriod	
					,PurchaseReturnData.PurchaseReturnAmount PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IH.TransactionQty) PurchaseReturnQty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
						from trn.PurchaseReturnDetail IH
						Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.POReturnDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=PurchaseReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					------------------------------- Adjustment-----------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, AdjustmentData.IsAsset--,AdjustmentData.InventoryMaterialId,AdjustmentData.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                          

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,AdjustmentData.PhysicalStockAdjustmentQty AdjustmentQtyForThePeriod	
					,AdjustmentData.PhysicalStockAdjustmentAmount AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
				
					Left join (select psad.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
							  ,sum(IH.Qty) PhysicalStockAdjustmentQty,sum(IH.Rate) Rate, (sum(IH.Qty)*sum(IH.Rate)) PhysicalStockAdjustmentAmount 
						from trn.PhysicalStockAdjustmentHistory IH
						Left JOIN TRN.PhysicalStockAdjustmentDetail psad on psad.Id=IH.PhysicalStockAdjustmentDetailId
						Left join trn.PhysicalStockAdjustmentMaster II ON II.Id=psad.PhysicalStockAdjustmentMasterID
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
						GROUP BY psad.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)AdjustmentData ON AdjustmentData.InventoryMaterialId=IM.Id
                      
					--left join [HKP].[MaterialStorage] MS on ms.id=AdjustmentData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					------------------------------------- InventorySales-------------------------------------


					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventorySalesData.IsAsset--,InventorySalesData.InventoryMaterialId,InventorySalesData.GRNDate
					,c.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,InventorySalesData.InventorySalesQty InventorySalesQtyForThePeriod	
					,InventorySalesData.InventorySalesAmount InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
						
					Left join (select ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
							  ,sum(ISH.Qty) InventorySalesQty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty)*sum(ISH.BaseRate)) InventorySalesAmount 
						from [TRN].[InventorySalesHistory] ISH
						Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
						Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=ISH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,Ins.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND Ins.PlantId='" + plantId + @"'
						GROUP BY ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                                
					--left join [HKP].[MaterialStorage] MS on ms.id=InventorySalesData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					------------------------------ InventoryScrap------------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventoryScrapData.IsAsset--,InventoryScrapData.InventoryMaterialId,InventoryScrapData.GRNDate
						,C.UserName CountryName	
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,InventoryScrapData.InventoryScrapQty InventoryScrapQtyForThePeriod	
					,InventoryScrapData.InventoryScrapAmount InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

						

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select ISCD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
						,sum(ISCH.Qty) InventoryScrapQty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
						from [TRN].[InventoryScrapHistory] ISCH
						Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
						Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=ISCH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,ISC.ScrapDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' 
						GROUP BY ISCD.InventoryMaterialId,IRD.IsAsset,IRD.MaterialStorageId,IR.GRNDate
					)InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id  
						
					--left join [HKP].[MaterialStorage] MS on ms.id=InventoryScrapData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					------------------------------ Inventory Transfer-------------------
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventoryTransferData.IsAsset--,InventoryTransferData.InventoryMaterialId,InventoryTransferData.GRNDate
						,C.UserName CountryName	
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,InventoryTransferData.InventoryTransferDataQty InventoryTransferQtyForThePeriod	
					,InventoryTransferData.InventoryTransferAmount InventoryTransferForThePeriodAmount

							
					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	
					Left join (Select IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IRD.InventoryTransferQty) InventoryTransferDataQty,sum(IRD.MaterialTranRate) Rate, (sum(IRD.InventoryTransferQty)*sum(IRD.MaterialTranRate)) InventoryTransferAmount 
					FROM [TRN].[InventoryTransferHistory] ITH
					Left JOIN [TRN].[InventoryReceiveDetail] IRD on IRD.Id=ITH.InventoryReceiveDetailId
					Left join [TRN].[InventoryReceive] IR on IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND IR.PlantId='" + plantId + @"'
					GROUP BY IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id   
						
					--left join [HKP].[MaterialStorage] MS on ms.id=InventoryTransferData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null
					)x
					where not
					(ISNULL(OpeningBalance,0)=0
				    and  ISNULL(OpeningIssueQty,0)=0
					and  ISNULL(ReceivedForThePeriod,0)=0
					and  ISNULL(IssueForThePeriod,0)=0
					and  ISNULL(IssueReturnQtyForThePeriod,0)=0

					and  ISNULL(PurchaseReturnOpeningQty,0)=0

					and  ISNULL(PurchaseReturnQtyForThePeriod,0)=0
					and  ISNULL(AdjustmentQtyForThePeriod,0)=0
					and  ISNULL(InventorySalesQtyForThePeriod,0)=0
					and  ISNULL(InventoryScrapQtyForThePeriod,0)=0
					and  ISNULL(InventoryTransferQtyForThePeriod,0)=0)
					--Where convert(Date,x.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' --and InventoryMaterialId='103' --x.MaterialStorageLocation is not NULL
					Group BY x.MaterialMasterName,x.MaterialMasterId,x.HSNCode,x.ArticleName,x.ArticleId,x.FirstCharacteristicsValue,x.SecondCharacteristicsValue,x.ThirdCharacteristicsValue,x.UOM,ISNULL(x.IsAsset,0),x.CountryName
					--,x.MaterialStorageLocation
					--,x.InventoryMaterialId
					--,x.GRNDate
					";

				}
				#endregion

			}
			else
			{
				if (Country == "undefined" || Country == "null") Country = "false";

				#region without country
				if (Asset == "true" && Inventory == "true" && Country == "false")
				{
					cmdText = @"SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]                
							   -- isnull(MT.UserName,'') MaterialType
							--, isnull(MGM.UserName,'') AS MaterialGroupMasterName						
								 ,isnull(MM.UserName,'') MaterialMasterName	
							 ,MM.Id		MaterialMasterId		 
							,HSNC.Code HSNCode
							,isnull( ART.StandardName,'') ArticleName	
							 ,ART.Id ArticleId	
							, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
							, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
							, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 	,MS.UserName MaterialStorageLocation
							,TUoM.UserName UOM,  opbal.IsAsset
							--,isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) As OpeningBalance	
							--,isnull(opbal.TotalMaterialBooksCurrencyAmount,0) AS OpeningBalanceAmount					

							--,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
							--,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
                         
							--,isnull(IFD1.IssueQty,0) IssueForThePeriod	
							--,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

							--,(isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0) Closing
							--,(isnull(opbal.TotalMaterialBooksCurrencyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0) ClosingAmount


                           -- ,isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) As OpeningBalance	
							--,isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) AS OpeningBalanceAmount

						

							--,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
							--,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount

                         
							--,isnull(IFD1.IssueQty,0) IssueForThePeriod	
							--,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

							--,(isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0) Closing
							--,(isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0) ClosingAmount
                            
                            --Opening Balance
                           ,isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) As OpeningBalance	
							,isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) AS OpeningBalanceAmount

						    --Receive

							,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
							,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
							--Issue
                         
							,isnull(IFD1.IssueQty,0) IssueForThePeriod	
							,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

							--Issue Return
							,isnull(IssueReturnData.Qty,0) IssueReturnQtyForThePeriod	
							,isnull(IssueReturnData.IssueReturnAmount,0) IssueReturnForThePeriodAmount	

							--Purchase Return
							,isnull(PurchaseReturnData.Qty,0) PurchaseReturnQtyForThePeriod	
							,isnull(PurchaseReturnData.PurchaseReturnAmount,0) PurchaseReturnForThePeriodAmount	

							--Adjust Return
							,isnull(AdjustmentData.Qty,0) AdjustmentQtyForThePeriod	
							,isnull(AdjustmentData.AdjustmentAmount,0) AdjustmentForThePeriodAmount	

				

					

                            --Inventory Sales
							,isnull(InventorySalesData.Qty,0) InventorySalesQtyForThePeriod	
							,isnull(InventorySalesData.InventorySalesAmount,0) InventorySalesForThePeriodAmount	
							--Inventory Scrap
							,isnull(InventoryScrapData.Qty,0) InventoryScrapQtyForThePeriod	
							,isnull(InventoryScrapData.InventoryScrapAmount,0) InventoryScrapForThePeriodAmount	

							--Inventory Transfer Data
							,isnull(InventoryTransferData.Qty,0) InventoryTransferQtyForThePeriod	
							,isnull(InventoryTransferData.InventoryTransferAmount,0) InventoryTransferForThePeriodAmount


							--Balance
							,(((((((isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0)-isnull(PurchaseReturnData.Qty,0))-isnull(AdjustmentData.Qty,0))+isnull(IssueReturnData.Qty,0))-isnull(InventorySalesData.Qty,0))-isnull(InventoryScrapData.Qty,0))-isnull(InventoryTransferData.Qty,0)) Closing 
							,(((((((isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0)-isnull(AdjustmentData.AdjustmentAmount,0))-isnull(PurchaseReturnData.PurchaseReturnAmount,0))+isnull(IssueReturnData.IssueReturnAmount,0)-isnull(InventorySalesData.InventorySalesAmount,0))-isnull(InventoryScrapData.InventoryScrapAmount,0))-isnull(InventoryTransferData.InventoryTransferAmount,0))) ClosingAmount



						from TRN.InventoryMaterial AS IM
						left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id						

						left join( select t.InventoryMaterialId,t.MaterialStorageId,t.IsAsset, sum(t.TransactionQty) TransactionQty,sum(t.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount 
			                        from (
			                        SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
			                        where convert(Date,IR.GRNDate) <= '" + toDate + @"' AND IR.OpeningBalanceId IS NOT NULL group By IRD.InventoryMaterialId,IRD.MaterialStorageId,IRD.IsAsset
			                        UNION ALL
			                        SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
			                        where convert(Date,IR.GRNDate) < '" + toDate + @"' AND IR.OpeningBalanceId IS NULL group By IRD.InventoryMaterialId,IRD.MaterialStorageId,IRD.IsAsset
			                        )as t group by t.InventoryMaterialId,t.MaterialStorageId,t.IsAsset
                                    ) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                        --left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
                        --left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                        --left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                        --left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        left join [HKP].[MaterialStorage] MS on ms.id=opbal.MaterialStorageId
                        LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
                        left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id

						left join(  SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) > '" + toDate + @"'  group By IRD.InventoryMaterialId--) AS opbal1 ON opbal1.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                                    UNION ALL
                                    SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) = '" + toDate + @"'  group By IRD.InventoryMaterialId) AS opbal1 ON opbal1.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

						left join(SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate)= '" + toDate + @"' AND IR.OpeningBalanceId IS  NULL group By IRD.InventoryMaterialId) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

                        left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) OpeningIssueQty, Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 									
									WHERE convert(Date,II.IssueDate) < '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IID.InventoryMaterialId
									) IFD3 On IFD3.InventoryMaterialId=IM.Id
						left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									--LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
									WHERE convert(Date,II.IssueDate) = '" + toDate + @"' AND II.PlantId='" + plantId + @"'  GROUP BY IID.InventoryMaterialId
									) IFD On IFD.InventoryMaterialId=IM.Id

						left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) IssueQty ,Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									--LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
								WHERE convert(Date,II.IssueDate) = '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IID.InventoryMaterialId
								) IFD1 On IFD1.InventoryMaterialId=IM.Id




                        --Issue Return
                        Left join (select IH.InventoryMaterialId,sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty)*sum(IRD.MaterialTranRate)) IssueReturnAmount from trn.InventoryIssueReturnHistory IH
									 Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.IssueDate) = '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id
					    --Purchase return
                       Left join (select IH.InventoryMaterialId,sum(IH.TransactionQty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
					                 from trn.PurchaseReturnDetail IH
									 Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.[POReturnDate]) = '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id

                       -- Adjustment
						Left join (select psad.InventoryMaterialId,sum(IH.Qty) Qty,sum(IH.Rate) Rate, (sum(IH.Qty)*sum(IH.Rate)) AdjustmentAmount 
					                 from trn.PhysicalStockAdjustmentHistory IH
									 Left JOIN TRN.PhysicalStockAdjustmentDetail psad on psad.Id=IH.PhysicalStockAdjustmentDetailId
									 Left join trn.PhysicalStockAdjustmentMaster II ON II.Id=psad.PhysicalStockAdjustmentMasterID
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,II.IssueDate) = '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY psad.InventoryMaterialId
								 )AdjustmentData ON AdjustmentData.InventoryMaterialId=IM.Id    
                                -- where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

						-- InventorySales
						Left join (select ISD.InventoryMaterialId,sum(ISH.Qty) Qty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty)*sum(ISH.BaseRate)) InventorySalesAmount 
					                 from [TRN].[InventorySalesHistory] ISH
									 Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
									 Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,Ins.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND Ins.PlantId='" + plantId + @"' GROUP BY ISD.InventoryMaterialId
								 )InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                                -- where IM.PlantId='20201' AND MM.UserName is not null --AND MM.UserName like '%Bed Sheet%'

                   --InventoryScrap
								Left join (select ISCD.InventoryMaterialId,sum(ISCH.Qty) Qty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
					                 from [TRN].[InventoryScrapHistory] ISCH
									 Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
									 Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,ISC.ScrapDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' GROUP BY ISCD.InventoryMaterialId
								 )InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id   
					--InventoryTransfer
								Left join ( select IRD.InventoryMaterialId,sum(IRD.InventoryTransferQty) Qty,sum(IRD.MaterialTranRate) Rate, (sum(IRD.InventoryTransferQty)*sum(IRD.MaterialTranRate)) InventoryTransferAmount 
					                 from [TRN].[InventoryTransferHistory] ITH
									 Left JOIN [TRN].[InventoryReceiveDetail] IRD on IRD.Id=ITH.InventoryReceiveDetailId
									 Left join [TRN].[InventoryReceive] IR on IR.Id=IRD.InventoryReceiveId
									 WHERE convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND IR.PlantId='" + plantId + @"'
									 GROUP BY IRD.InventoryMaterialId
								 )InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id      
                                  where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

--AND MM.UserName like '%Bed Sheet%'
								--AND isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0)> 0
							    --and isnull(opbal2.TransactionQty,0)>0
								--and isnull(IFD1.IssueQty,0)>0
                         --Order By 
                         
                          --MM.UserName 					 
						--,ART.StandardName 	
						--,FCV.UserName 
						--,SCV.UserName 
						--,TCV.UserName

						--GROUP BY 
						--MT.UserName 
						--,MGM.UserName 			
						--,MM.UserName 					 
						--,ART.StandardName 	
						--,FCV.UserName 
						--,SCV.UserName 
						--,TCV.UserName";
				}
				else if (Asset == "false" && Inventory == "true" && Country == "false")
				{
					cmdText = @"SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]             
							   -- isnull(MT.UserName,'') MaterialType
							--, isnull(MGM.UserName,'') AS MaterialGroupMasterName						
							 ,isnull(MM.UserName,'') MaterialMasterName	
							 ,MM.Id	MaterialMasterId	
							,HSNC.Code HSNCode
							,isnull( ART.StandardName,'') ArticleName	
							 ,ART.Id ArticleId		
							, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
							, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
							, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue ,MS.UserName MaterialStorageLocation	
							,TUoM.UserName UOM,  opbal.IsAsset
							--,isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) As OpeningBalance	
							--,isnull(opbal.TotalMaterialBooksCurrencyAmount,0) AS OpeningBalanceAmount					

							--,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
							--,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
                         
							--,isnull(IFD1.IssueQty,0) IssueForThePeriod	
							--,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

							--,(isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0) Closing
							--,(isnull(opbal.TotalMaterialBooksCurrencyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0) ClosingAmount


                           -- ,isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) As OpeningBalance	
							--,isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) AS OpeningBalanceAmount

						

							--,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
							--,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount

                         
							--,isnull(IFD1.IssueQty,0) IssueForThePeriod	
							--,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

							--,(isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0) Closing
							--,(isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0) ClosingAmount
                            
                            --Opening Balance
                           ,isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) As OpeningBalance	
							,isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) AS OpeningBalanceAmount

						    --Receive

							,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
							,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
							--Issue
                         
							,isnull(IFD1.IssueQty,0) IssueForThePeriod	
							,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

							--Issue Return
							,isnull(IssueReturnData.Qty,0) IssueReturnQtyForThePeriod	
							,isnull(IssueReturnData.IssueReturnAmount,0) IssueReturnForThePeriodAmount	

							--Purchase Return
							,isnull(PurchaseReturnData.Qty,0) PurchaseReturnQtyForThePeriod	
							,isnull(PurchaseReturnData.PurchaseReturnAmount,0) PurchaseReturnForThePeriodAmount	

							--Adjust Return
							,isnull(AdjustmentData.Qty,0) AdjustmentQtyForThePeriod	
							,isnull(AdjustmentData.AdjustmentAmount,0) AdjustmentForThePeriodAmount	

                               --Inventory Sales
							,isnull(InventorySalesData.Qty,0) InventorySalesQtyForThePeriod	
							,isnull(InventorySalesData.InventorySalesAmount,0) InventorySalesForThePeriodAmount	
							--Inventory Scrap
							,isnull(InventoryScrapData.Qty,0) InventoryScrapQtyForThePeriod	
							,isnull(InventoryScrapData.InventoryScrapAmount,0) InventoryScrapForThePeriodAmount	

							--Inventory Transfer Data
							,isnull(InventoryTransferData.Qty,0) InventoryTransferQtyForThePeriod	
							,isnull(InventoryTransferData.InventoryTransferAmount,0) InventoryTransferForThePeriodAmount

							--Balance
							,(((((((isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0)-isnull(PurchaseReturnData.Qty,0))-isnull(AdjustmentData.Qty,0))+isnull(IssueReturnData.Qty,0))-isnull(InventorySalesData.Qty,0))-isnull(InventoryScrapData.Qty,0))-isnull(InventoryTransferData.Qty,0)) Closing 
							,(((((((isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0)-isnull(AdjustmentData.AdjustmentAmount,0))-isnull(PurchaseReturnData.PurchaseReturnAmount,0))+isnull(IssueReturnData.IssueReturnAmount,0)-isnull(InventorySalesData.InventorySalesAmount,0))-isnull(InventoryScrapData.InventoryScrapAmount,0))-isnull(InventoryTransferData.InventoryTransferAmount,0))) ClosingAmount


						from TRN.InventoryMaterial AS IM
						left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
						left join( select t.InventoryMaterialId,t.MaterialStorageId,t.IsAsset, sum(t.TransactionQty) TransactionQty,sum(t.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount 
			                        from (
			                        SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
			                        where convert(Date,IR.GRNDate) <= '" + toDate + @"' AND IR.OpeningBalanceId IS NOT NULL group By IRD.InventoryMaterialId,IRD.MaterialStorageId,IRD.IsAsset
			                        UNION ALL
			                        SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
			                        where convert(Date,IR.GRNDate) < '" + toDate + @"' AND IR.OpeningBalanceId IS NULL group By IRD.InventoryMaterialId,IRD.MaterialStorageId,IRD.IsAsset
			                        )as t group by t.InventoryMaterialId,t.MaterialStorageId,t.IsAsset
                                    ) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                        --left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
                        --left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                        --left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                        --left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        left join [HKP].[MaterialStorage] MS on ms.id=opbal.MaterialStorageId
                        LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
                        left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id

						left join(  SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) > '" + toDate + @"'  group By IRD.InventoryMaterialId--) AS opbal1 ON opbal1.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                                    UNION ALL
                                    SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) = '" + toDate + @"'  group By IRD.InventoryMaterialId) AS opbal1 ON opbal1.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

						left join(SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate)	<= '" + toDate + @"' AND IR.OpeningBalanceId IS  NULL group By IRD.InventoryMaterialId) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

                        left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) OpeningIssueQty, Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 									
									WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IID.InventoryMaterialId
									) IFD3 On IFD3.InventoryMaterialId=IM.Id
						left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									--LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
									WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"'  GROUP BY IID.InventoryMaterialId
									) IFD On IFD.InventoryMaterialId=IM.Id

						left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									--LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
								WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IID.InventoryMaterialId
								) IFD1 On IFD1.InventoryMaterialId=IM.Id


                       --Issue Return
                        Left join (select IH.InventoryMaterialId,sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty)*sum(IRD.MaterialTranRate)) IssueReturnAmount from trn.InventoryIssueReturnHistory IH
									 Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id
					    --Purchase return
                       Left join (select IH.InventoryMaterialId,sum(IH.TransactionQty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
					                 from trn.PurchaseReturnDetail IH
									 Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.[POReturnDate]) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id

                       -- Adjustment
						Left join (select psad.InventoryMaterialId,sum(IH.Qty) Qty,sum(IH.Rate) Rate, (sum(IH.Qty)*sum(IH.Rate)) AdjustmentAmount 
					                 from trn.PhysicalStockAdjustmentHistory IH
									 Left JOIN TRN.PhysicalStockAdjustmentDetail psad on psad.Id=IH.PhysicalStockAdjustmentDetailId
									 Left join trn.PhysicalStockAdjustmentMaster II ON II.Id=psad.PhysicalStockAdjustmentMasterID
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY psad.InventoryMaterialId
								 )AdjustmentData ON AdjustmentData.InventoryMaterialId=IM.Id

                               --  where  opbal.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null 
						-- InventorySales
						Left join (select ISD.InventoryMaterialId,sum(ISH.Qty) Qty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty)*sum(ISH.BaseRate)) InventorySalesAmount 
					                 from [TRN].[InventorySalesHistory] ISH
									 Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
									 Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,Ins.SalesDate) <= '" + toDate + @"' AND Ins.PlantId='" + plantId + @"' GROUP BY ISD.InventoryMaterialId
								 )InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                                -- where IM.PlantId='20201' AND MM.UserName is not null --AND MM.UserName like '%Bed Sheet%'

                   --InventoryScrap
								Left join (select ISCD.InventoryMaterialId,sum(ISCH.Qty) Qty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
					                 from [TRN].[InventoryScrapHistory] ISCH
									 Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
									 Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,ISC.ScrapDate) <= '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' GROUP BY ISCD.InventoryMaterialId
								 )InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id   
					--InventoryTransfer
								Left join ( select IRD.InventoryMaterialId,sum(IRD.InventoryTransferQty) Qty,sum(IRD.MaterialTranRate) Rate, (sum(IRD.InventoryTransferQty)*sum(IRD.MaterialTranRate)) InventoryTransferAmount 
					                 from [TRN].[InventoryTransferHistory] ITH
									 Left JOIN [TRN].[InventoryReceiveDetail] IRD on IRD.Id=ITH.InventoryReceiveDetailId
									 Left join [TRN].[InventoryReceive] IR on IR.Id=IRD.InventoryReceiveId
									 WHERE convert(Date,IR.GRNDate) <= '" + toDate + @"' AND IR.PlantId='" + plantId + @"'
									 GROUP BY IRD.InventoryMaterialId
								 )InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id      
                                 where  opbal.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

								--AND MM.UserName like '%Bed Sheet%'
								--AND isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0)> 0
							    --and isnull(opbal2.TransactionQty,0)>0
								--and isnull(IFD1.IssueQty,0)>0
                         --Order By 
                        --IRD.IsAsset,
                       -- MM.UserName 					 
						--,ART.StandardName 	
						--,FCV.UserName 
						--,SCV.UserName 
						--,TCV.UserName

						--GROUP BY 
						--MT.UserName 
						--,MGM.UserName 			
						--,MM.UserName 					 
						--,ART.StandardName 	
						--,FCV.UserName 
						--,SCV.UserName 
						--,TCV.UserName";

				}
				else if (Asset == "true" && Inventory == "false" && Country == "false")
				{

					cmdText = @"SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]                 
							   -- isnull(MT.UserName,'') MaterialType
							--, isnull(MGM.UserName,'') AS MaterialGroupMasterName						
							 ,isnull(MM.UserName,'') MaterialMasterName	
							 ,MM.Id	MaterialMasterId		
							,HSNC.Code HSNCode
							,isnull( ART.StandardName,'') ArticleName	
							 ,ART.Id ArticleId			
							, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
							, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
							, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue ,MS.UserName MaterialStorageLocation	
							,TUoM.UserName UOM,  opbal.IsAsset
							--,isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) As OpeningBalance	
							--,isnull(opbal.TotalMaterialBooksCurrencyAmount,0) AS OpeningBalanceAmount					

							--,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
							--,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
                         
							--,isnull(IFD1.IssueQty,0) IssueForThePeriod	
							--,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

							--,(isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0) Closing
							--,(isnull(opbal.TotalMaterialBooksCurrencyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0) ClosingAmount


                           -- ,isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) As OpeningBalance	
							--,isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) AS OpeningBalanceAmount

						

							--,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
							--,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount

                         
							--,isnull(IFD1.IssueQty,0) IssueForThePeriod	
							--,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

							--,(isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0) Closing
							--,(isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0) ClosingAmount
                            
                            --Opening Balance
                           ,isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) As OpeningBalance	
							,isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) AS OpeningBalanceAmount

						    --Receive

							,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
							,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
							--Issue
                         
							,isnull(IFD1.IssueQty,0) IssueForThePeriod	
							,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

							--Issue Return
							,isnull(IssueReturnData.Qty,0) IssueReturnQtyForThePeriod	
							,isnull(IssueReturnData.IssueReturnAmount,0) IssueReturnForThePeriodAmount	

							--Purchase Return
							,isnull(PurchaseReturnData.Qty,0) PurchaseReturnQtyForThePeriod	
							,isnull(PurchaseReturnData.PurchaseReturnAmount,0) PurchaseReturnForThePeriodAmount	

							--Adjust Return
							,isnull(AdjustmentData.Qty,0) AdjustmentQtyForThePeriod	
							,isnull(AdjustmentData.AdjustmentAmount,0) AdjustmentForThePeriodAmount	
             --Inventory Sales
							,isnull(InventorySalesData.Qty,0) InventorySalesQtyForThePeriod	
							,isnull(InventorySalesData.InventorySalesAmount,0) InventorySalesForThePeriodAmount	
							--Inventory Scrap
							,isnull(InventoryScrapData.Qty,0) InventoryScrapQtyForThePeriod	
							,isnull(InventoryScrapData.InventoryScrapAmount,0) InventoryScrapForThePeriodAmount		

							--Inventory Transfer Data
							,isnull(InventoryTransferData.Qty,0) InventoryTransferQtyForThePeriod	
							,isnull(InventoryTransferData.InventoryTransferAmount,0) InventoryTransferForThePeriodAmount

							--Balance
							,(((((((isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0)-isnull(PurchaseReturnData.Qty,0))-isnull(AdjustmentData.Qty,0))+isnull(IssueReturnData.Qty,0))-isnull(InventorySalesData.Qty,0))-isnull(InventoryScrapData.Qty,0))-isnull(InventoryTransferData.Qty,0)) Closing 
							,(((((((isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0)-isnull(AdjustmentData.AdjustmentAmount,0))-isnull(PurchaseReturnData.PurchaseReturnAmount,0))+isnull(IssueReturnData.IssueReturnAmount,0)-isnull(InventorySalesData.InventorySalesAmount,0))-isnull(InventoryScrapData.InventoryScrapAmount,0))-isnull(InventoryTransferData.InventoryTransferAmount,0))) ClosingAmount



						from TRN.InventoryMaterial AS IM
						left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
						left join( select t.InventoryMaterialId,t.MaterialStorageId,t.IsAsset, sum(t.TransactionQty) TransactionQty,sum(t.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount 
			                        from (
			                        SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
			                        where convert(Date,IR.GRNDate) <= '" + toDate + @"' AND IR.OpeningBalanceId IS NOT NULL group By IRD.InventoryMaterialId,IRD.MaterialStorageId,IRD.IsAsset
			                        UNION ALL
			                        SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
			                        where convert(Date,IR.GRNDate) < '" + toDate + @"' AND IR.OpeningBalanceId IS NULL group By IRD.InventoryMaterialId,IRD.MaterialStorageId,IRD.IsAsset
			                        )as t group by t.InventoryMaterialId,t.MaterialStorageId,t.IsAsset
                                    ) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                        --left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
                        --left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                        --left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                        --left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        left join [HKP].[MaterialStorage] MS on ms.id=opbal.MaterialStorageId
                        LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
                        left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id

						left join(  SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) > '" + toDate + @"'  group By IRD.InventoryMaterialId--) AS opbal1 ON opbal1.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                                    UNION ALL
                                    SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) = '" + toDate + @"'  group By IRD.InventoryMaterialId) AS opbal1 ON opbal1.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

						left join(SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate)	<='" + toDate + @"' AND IR.OpeningBalanceId IS  NULL group By IRD.InventoryMaterialId) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

                        left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) OpeningIssueQty, Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 									
									WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IID.InventoryMaterialId
									) IFD3 On IFD3.InventoryMaterialId=IM.Id
						left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									--LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
									WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"'  GROUP BY IID.InventoryMaterialId
									) IFD On IFD.InventoryMaterialId=IM.Id

						left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									--LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
								WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IID.InventoryMaterialId
								) IFD1 On IFD1.InventoryMaterialId=IM.Id

                        --Issue Return
                        Left join (select IH.InventoryMaterialId,sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty)*sum(IRD.MaterialTranRate)) IssueReturnAmount from trn.InventoryIssueReturnHistory IH
									 Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id
					    --Purchase return
                       Left join (select IH.InventoryMaterialId,sum(IH.TransactionQty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
					                 from trn.PurchaseReturnDetail IH
									 Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.[POReturnDate]) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id

                       -- Adjustment
						Left join (select psad.InventoryMaterialId,sum(IH.Qty) Qty,sum(IH.Rate) Rate, (sum(IH.Qty)*sum(IH.Rate)) AdjustmentAmount 
					                 from trn.PhysicalStockAdjustmentHistory IH
									 Left JOIN TRN.PhysicalStockAdjustmentDetail psad on psad.Id=IH.PhysicalStockAdjustmentDetailId
									 Left join trn.PhysicalStockAdjustmentMaster II ON II.Id=psad.PhysicalStockAdjustmentMasterID
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY psad.InventoryMaterialId
								 )AdjustmentData ON AdjustmentData.InventoryMaterialId=IM.Id


                               --  where opbal.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null 
						-- InventorySales
						Left join (select ISD.InventoryMaterialId,sum(ISH.Qty) Qty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty)*sum(ISH.BaseRate)) InventorySalesAmount 
					                 from [TRN].[InventorySalesHistory] ISH
									 Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
									 Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,Ins.SalesDate) <= '" + toDate + @"' AND Ins.PlantId='" + plantId + @"' GROUP BY ISD.InventoryMaterialId
								 )InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                                -- where IM.PlantId='20201' AND MM.UserName is not null --AND MM.UserName like '%Bed Sheet%'

                   --InventoryScrap
								Left join (select ISCD.InventoryMaterialId,sum(ISCH.Qty) Qty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
					                 from [TRN].[InventoryScrapHistory] ISCH
									 Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
									 Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,ISC.ScrapDate) <= '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' GROUP BY ISCD.InventoryMaterialId
								 )InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id   
					--InventoryTransfer
								Left join ( select IRD.InventoryMaterialId,sum(IRD.InventoryTransferQty) Qty,sum(IRD.MaterialTranRate) Rate, (sum(IRD.InventoryTransferQty)*sum(IRD.MaterialTranRate)) InventoryTransferAmount 
					                 from [TRN].[InventoryTransferHistory] ITH
									 Left JOIN [TRN].[InventoryReceiveDetail] IRD on IRD.Id=ITH.InventoryReceiveDetailId
									 Left join [TRN].[InventoryReceive] IR on IR.Id=IRD.InventoryReceiveId
									 WHERE convert(Date,IR.GRNDate) <= '" + toDate + @"' AND IR.PlantId='" + plantId + @"'
									 GROUP BY IRD.InventoryMaterialId
								 )InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id      
                                where MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

								--AND MM.UserName like '%Bed Sheet%'
								--AND isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0)> 0
							    --and isnull(opbal2.TransactionQty,0)>0
								--and isnull(IFD1.IssueQty,0)>0
                        -- Order By 
                        --IRD.IsAsset,
                        --MM.UserName				 
						--,ART.StandardName 	
						--,FCV.UserName 
						--,SCV.UserName 
						--,TCV.UserName

						--GROUP BY 
						--MT.UserName 
						--,MGM.UserName 			
						--,MM.UserName 					 
						--,ART.StandardName 	
						--,FCV.UserName 
						--,SCV.UserName 
						--,TCV.UserName";

				}
				#endregion

				#region with country
				if (Asset == "true" && Inventory == "true" && Country == "true")
				{
					cmdText = @"SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]         
							   -- isnull(MT.UserName,'') MaterialType
							--, isnull(MGM.UserName,'') AS MaterialGroupMasterName						
								 ,isnull(MM.UserName,'') MaterialMasterName	
							 ,MM.Id	MaterialMasterId		
							,HSNC.Code HSNCode
							,isnull( ART.StandardName,'') ArticleName	
							 ,ART.Id ArticleId		
							, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
							, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
							, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 	,MS.UserName MaterialStorageLocation
                            ,isnull(C.UserName,'') CountryName
							,TUoM.UserName UOM, opbal.IsAsset
							--,isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) As OpeningBalance	
							--,isnull(opbal.TotalMaterialBooksCurrencyAmount,0) AS OpeningBalanceAmount					

							--,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
							--,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
                         
							--,isnull(IFD1.IssueQty,0) IssueForThePeriod	
							--,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

							--,(isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0) Closing
							--,(isnull(opbal.TotalMaterialBooksCurrencyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0) ClosingAmount


                           -- ,isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) As OpeningBalance	
							--,isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) AS OpeningBalanceAmount

						

							--,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
							--,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount

                         
							--,isnull(IFD1.IssueQty,0) IssueForThePeriod	
							--,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

							--,(isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0) Closing
							--,(isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0) ClosingAmount
                            
                            --Opening Balance
                           ,isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) As OpeningBalance	
							,isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) AS OpeningBalanceAmount

						    --Receive

							,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
							,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
							--Issue
                         
							,isnull(IFD1.IssueQty,0) IssueForThePeriod	
							,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

							--Issue Return
							,isnull(IssueReturnData.Qty,0) IssueReturnQtyForThePeriod	
							,isnull(IssueReturnData.IssueReturnAmount,0) IssueReturnForThePeriodAmount	

							--Purchase Return
							,isnull(PurchaseReturnData.Qty,0) PurchaseReturnQtyForThePeriod	
							,isnull(PurchaseReturnData.PurchaseReturnAmount,0) PurchaseReturnForThePeriodAmount	

							--Adjust Return
							,isnull(AdjustmentData.Qty,0) AdjustmentQtyForThePeriod	
							,isnull(AdjustmentData.AdjustmentAmount,0) AdjustmentForThePeriodAmount	

                                  --Inventory Sales
							,isnull(InventorySalesData.Qty,0) InventorySalesQtyForThePeriod	
							,isnull(InventorySalesData.InventorySalesAmount,0) InventorySalesForThePeriodAmount	
							--Inventory Scrap
							,isnull(InventoryScrapData.Qty,0) InventoryScrapQtyForThePeriod	
							,isnull(InventoryScrapData.InventoryScrapAmount,0) InventoryScrapForThePeriodAmount	

							--Inventory Transfer Data
							,isnull(InventoryTransferData.Qty,0) InventoryTransferQtyForThePeriod	
							,isnull(InventoryTransferData.InventoryTransferAmount,0) InventoryTransferForThePeriodAmount

							--Balance
							,(((((((isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0)-isnull(PurchaseReturnData.Qty,0))-isnull(AdjustmentData.Qty,0))+isnull(IssueReturnData.Qty,0))-isnull(InventorySalesData.Qty,0))-isnull(InventoryScrapData.Qty,0))-isnull(InventoryTransferData.Qty,0)) Closing 
							,(((((((isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0)-isnull(AdjustmentData.AdjustmentAmount,0))-isnull(PurchaseReturnData.PurchaseReturnAmount,0))+isnull(IssueReturnData.IssueReturnAmount,0)-isnull(InventorySalesData.InventorySalesAmount,0))-isnull(InventoryScrapData.InventoryScrapAmount,0))-isnull(InventoryTransferData.InventoryTransferAmount,0))) ClosingAmount



						from TRN.InventoryMaterial AS IM
						left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
						
                       
						left join( select t.InventoryMaterialId,t.MaterialStorageId,t.IsAsset, sum(t.TransactionQty) TransactionQty,sum(t.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount 
			                        from (
			                        SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
			                        where convert(Date,IR.GRNDate) <= '" + toDate + @"' AND IR.OpeningBalanceId IS NOT NULL group By IRD.InventoryMaterialId,IRD.MaterialStorageId,IRD.IsAsset
			                        UNION ALL
			                        SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
			                        where convert(Date,IR.GRNDate) < '" + toDate + @"' AND IR.OpeningBalanceId IS NULL group By IRD.InventoryMaterialId,IRD.MaterialStorageId,IRD.IsAsset
			                        )as t group by t.InventoryMaterialId,t.MaterialStorageId,t.IsAsset
                                    ) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                        --left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
                        --left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                        --left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                        --left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        left join [HKP].[MaterialStorage] MS on ms.id=opbal.MaterialStorageId
                        LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
                        left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id
                        Left JOIN SCS.Country C On C.Id=IM.CountryId
						left join(  SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) > '" + toDate + @"'  group By IRD.InventoryMaterialId--) AS opbal1 ON opbal1.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                                    UNION ALL
                                    SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) = '" + toDate + @"'  group By IRD.InventoryMaterialId) AS opbal1 ON opbal1.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

						left join(SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate)= '" + toDate + @"' AND IR.OpeningBalanceId IS  NULL group By IRD.InventoryMaterialId) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

                        left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) OpeningIssueQty, Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 									
									WHERE convert(Date,II.IssueDate) < '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IID.InventoryMaterialId
									) IFD3 On IFD3.InventoryMaterialId=IM.Id
						left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									--LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
									WHERE convert(Date,II.IssueDate) = '" + toDate + @"' AND II.PlantId='" + plantId + @"'  GROUP BY IID.InventoryMaterialId
									) IFD On IFD.InventoryMaterialId=IM.Id

						left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									--LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
								WHERE convert(Date,II.IssueDate) = '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IID.InventoryMaterialId
								) IFD1 On IFD1.InventoryMaterialId=IM.Id




                        --Issue Return
                        Left join (select IH.InventoryMaterialId,sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty)*sum(IRD.MaterialTranRate)) IssueReturnAmount from trn.InventoryIssueReturnHistory IH
									 Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.IssueDate) = '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id
					    --Purchase return
                       Left join (select IH.InventoryMaterialId,sum(IH.TransactionQty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
					                 from trn.PurchaseReturnDetail IH
									 Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.[POReturnDate]) = '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id

                       -- Adjustment
						Left join (select psad.InventoryMaterialId,sum(IH.Qty) Qty,sum(IH.Rate) Rate, (sum(IH.Qty)*sum(IH.Rate)) AdjustmentAmount 
					                 from trn.PhysicalStockAdjustmentHistory IH
									 Left JOIN TRN.PhysicalStockAdjustmentDetail psad on psad.Id=IH.PhysicalStockAdjustmentDetailId
									 Left join trn.PhysicalStockAdjustmentMaster II ON II.Id=psad.PhysicalStockAdjustmentMasterID
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,II.IssueDate) = '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY psad.InventoryMaterialId
								 )AdjustmentData ON AdjustmentData.InventoryMaterialId=IM.Id    
                                 --where IM.PlantId='" + plantId + @"' AND MM.UserName is not null 

             -- InventorySales
						Left join (select ISD.InventoryMaterialId,sum(ISH.Qty) Qty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty)*sum(ISH.BaseRate)) InventorySalesAmount 
					                 from [TRN].[InventorySalesHistory] ISH
									 Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
									 Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,Ins.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND Ins.PlantId='" + plantId + @"' GROUP BY ISD.InventoryMaterialId
								 )InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                                -- where IM.PlantId='20201' AND MM.UserName is not null --AND MM.UserName like '%Bed Sheet%'

                   --InventoryScrap
								Left join (select ISCD.InventoryMaterialId,sum(ISCH.Qty) Qty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
					                 from [TRN].[InventoryScrapHistory] ISCH
									 Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
									 Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,ISC.ScrapDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' GROUP BY ISCD.InventoryMaterialId
								 )InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id   
					--InventoryTransfer
								Left join ( select IRD.InventoryMaterialId,sum(IRD.InventoryTransferQty) Qty,sum(IRD.MaterialTranRate) Rate, (sum(IRD.InventoryTransferQty)*sum(IRD.MaterialTranRate)) InventoryTransferAmount 
					                 from [TRN].[InventoryTransferHistory] ITH
									 Left JOIN [TRN].[InventoryReceiveDetail] IRD on IRD.Id=ITH.InventoryReceiveDetailId
									 Left join [TRN].[InventoryReceive] IR on IR.Id=IRD.InventoryReceiveId
									 WHERE convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND IR.PlantId='" + plantId + @"'
									 GROUP BY IRD.InventoryMaterialId
								 )InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id       
                               where MM.IsAsset='" + plantId + @"' AND MM.UserName is not null



--AND MM.UserName like '%Bed Sheet%'
								--AND isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0)> 0
							    --and isnull(opbal2.TransactionQty,0)>0
								--and isnull(IFD1.IssueQty,0)>0
                         --Order By 
                         
                          --MM.UserName 					 
						--,ART.StandardName 	
						--,FCV.UserName 
						--,SCV.UserName 
						--,TCV.UserName

						--GROUP BY 
						--MT.UserName 
						--,MGM.UserName 			
						--,MM.UserName 					 
						--,ART.StandardName 	
						--,FCV.UserName 
						--,SCV.UserName 
						--,TCV.UserName";
				}
				else if (Asset == "false" && Inventory == "true" && Country == "true")
				{
					cmdText = @"SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]    
							   -- isnull(MT.UserName,'') MaterialType
							--, isnull(MGM.UserName,'') AS MaterialGroupMasterName						
								,isnull(MM.UserName,'') MaterialMasterName	
							 ,MM.Id	MaterialMasterId	
							,HSNC.Code HSNCode
							,isnull( ART.StandardName,'') ArticleName	
							 ,ART.Id	ArticleId
							, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
							, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
							, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue ,MS.UserName MaterialStorageLocation	
                            ,isnull(C.UserName,'') CountryName
							,TUoM.UserName UOM,  opbal.IsAsset
							--,isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) As OpeningBalance	
							--,isnull(opbal.TotalMaterialBooksCurrencyAmount,0) AS OpeningBalanceAmount					

							--,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
							--,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
                         
							--,isnull(IFD1.IssueQty,0) IssueForThePeriod	
							--,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

							--,(isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0) Closing
							--,(isnull(opbal.TotalMaterialBooksCurrencyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0) ClosingAmount


                           -- ,isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) As OpeningBalance	
							--,isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) AS OpeningBalanceAmount

						

							--,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
							--,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount

                         
							--,isnull(IFD1.IssueQty,0) IssueForThePeriod	
							--,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

							--,(isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0) Closing
							--,(isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0) ClosingAmount
                            
                            --Opening Balance
                           ,isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) As OpeningBalance	
							,isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) AS OpeningBalanceAmount

						    --Receive

							,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
							,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
							--Issue
                         
							,isnull(IFD1.IssueQty,0) IssueForThePeriod	
							,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

							--Issue Return
							,isnull(IssueReturnData.Qty,0) IssueReturnQtyForThePeriod	
							,isnull(IssueReturnData.IssueReturnAmount,0) IssueReturnForThePeriodAmount	

							--Purchase Return
							,isnull(PurchaseReturnData.Qty,0) PurchaseReturnQtyForThePeriod	
							,isnull(PurchaseReturnData.PurchaseReturnAmount,0) PurchaseReturnForThePeriodAmount	

							--Adjust Return
							,isnull(AdjustmentData.Qty,0) AdjustmentQtyForThePeriod	
							,isnull(AdjustmentData.AdjustmentAmount,0) AdjustmentForThePeriodAmount	
             --Inventory Sales
							,isnull(InventorySalesData.Qty,0) InventorySalesQtyForThePeriod	
							,isnull(InventorySalesData.InventorySalesAmount,0) InventorySalesForThePeriodAmount	
							--Inventory Scrap
							,isnull(InventoryScrapData.Qty,0) InventoryScrapQtyForThePeriod	
							,isnull(InventoryScrapData.InventoryScrapAmount,0) InventoryScrapForThePeriodAmount	

							--Inventory Transfer Data
							,isnull(InventoryTransferData.Qty,0) InventoryTransferQtyForThePeriod	
							,isnull(InventoryTransferData.InventoryTransferAmount,0) InventoryTransferForThePeriodAmount

							--Balance
							,(((((((isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0)-isnull(PurchaseReturnData.Qty,0))-isnull(AdjustmentData.Qty,0))+isnull(IssueReturnData.Qty,0))-isnull(InventorySalesData.Qty,0))-isnull(InventoryScrapData.Qty,0))-isnull(InventoryTransferData.Qty,0)) Closing 
							,(((((((isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0)-isnull(AdjustmentData.AdjustmentAmount,0))-isnull(PurchaseReturnData.PurchaseReturnAmount,0))+isnull(IssueReturnData.IssueReturnAmount,0)-isnull(InventorySalesData.InventorySalesAmount,0))-isnull(InventoryScrapData.InventoryScrapAmount,0))-isnull(InventoryTransferData.InventoryTransferAmount,0))) ClosingAmount



						from TRN.InventoryMaterial AS IM
						left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
						left join( select t.InventoryMaterialId,t.MaterialStorageId,t.IsAsset, sum(t.TransactionQty) TransactionQty,sum(t.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount 
			                        from (
			                        SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
			                        where convert(Date,IR.GRNDate) <= '" + toDate + @"' AND IR.OpeningBalanceId IS NOT NULL group By IRD.InventoryMaterialId,IRD.MaterialStorageId,IRD.IsAsset
			                        UNION ALL
			                        SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
			                        where convert(Date,IR.GRNDate) < '" + toDate + @"' AND IR.OpeningBalanceId IS NULL group By IRD.InventoryMaterialId,IRD.MaterialStorageId,IRD.IsAsset
			                        )as t group by t.InventoryMaterialId,t.MaterialStorageId,t.IsAsset
                                    ) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                        --left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
                        --left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                        --left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                        --left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        left join [HKP].[MaterialStorage] MS on ms.id=opbal.MaterialStorageId
                        LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
                        left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id
                        Left JOIN SCS.Country C On C.Id=IM.CountryId

						left join(  SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) > '" + toDate + @"'  group By IRD.InventoryMaterialId--) AS opbal1 ON opbal1.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                                    UNION ALL
                                    SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) = '" + toDate + @"'  group By IRD.InventoryMaterialId) AS opbal1 ON opbal1.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

						left join(SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate)	<= '" + toDate + @"' AND IR.OpeningBalanceId IS  NULL group By IRD.InventoryMaterialId) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

                        left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) OpeningIssueQty, Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 									
									WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IID.InventoryMaterialId
									) IFD3 On IFD3.InventoryMaterialId=IM.Id
						left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									--LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
									WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"'  GROUP BY IID.InventoryMaterialId
									) IFD On IFD.InventoryMaterialId=IM.Id

						left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									--LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
								WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IID.InventoryMaterialId
								) IFD1 On IFD1.InventoryMaterialId=IM.Id


                       --Issue Return
                        Left join (select IH.InventoryMaterialId,sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty)*sum(IRD.MaterialTranRate)) IssueReturnAmount from trn.InventoryIssueReturnHistory IH
									 Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id
					    --Purchase return
                       Left join (select IH.InventoryMaterialId,sum(IH.TransactionQty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
					                 from trn.PurchaseReturnDetail IH
									 Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.[POReturnDate]) = '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id

                       -- Adjustment
						Left join (select psad.InventoryMaterialId,sum(IH.Qty) Qty,sum(IH.Rate) Rate, (sum(IH.Qty)*sum(IH.Rate)) AdjustmentAmount 
					                 from trn.PhysicalStockAdjustmentHistory IH
									 Left JOIN TRN.PhysicalStockAdjustmentDetail psad on psad.Id=IH.PhysicalStockAdjustmentDetailId
									 Left join trn.PhysicalStockAdjustmentMaster II ON II.Id=psad.PhysicalStockAdjustmentMasterID
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY psad.InventoryMaterialId
								 )AdjustmentData ON AdjustmentData.InventoryMaterialId=IM.Id

                                -- where  opbal.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null
					-- InventorySales
						Left join (select ISD.InventoryMaterialId,sum(ISH.Qty) Qty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty)*sum(ISH.BaseRate)) InventorySalesAmount 
					                 from [TRN].[InventorySalesHistory] ISH
									 Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
									 Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,Ins.SalesDate) <= '" + toDate + @"' AND Ins.PlantId='" + plantId + @"' GROUP BY ISD.InventoryMaterialId
								 )InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                                -- where IM.PlantId='20201' AND MM.UserName is not null --AND MM.UserName like '%Bed Sheet%'
                   --InventoryScrap
								Left join (select ISCD.InventoryMaterialId,sum(ISCH.Qty) Qty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
					                 from [TRN].[InventoryScrapHistory] ISCH
									 Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
									 Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,ISC.ScrapDate) <= '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' GROUP BY ISCD.InventoryMaterialId
								 )InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id   
					--InventoryTransfer
								Left join ( select IRD.InventoryMaterialId,sum(IRD.InventoryTransferQty) Qty,sum(IRD.MaterialTranRate) Rate, (sum(IRD.InventoryTransferQty)*sum(IRD.MaterialTranRate)) InventoryTransferAmount 
					                 from [TRN].[InventoryTransferHistory] ITH
									 Left JOIN [TRN].[InventoryReceiveDetail] IRD on IRD.Id=ITH.InventoryReceiveDetailId
									 Left join [TRN].[InventoryReceive] IR on IR.Id=IRD.InventoryReceiveId
									 WHERE convert(Date,IR.GRNDate) <= '" + toDate + @"' AND IR.PlantId='" + plantId + @"'
									 GROUP BY IRD.InventoryMaterialId
								 )InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id   
                                where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null


								--AND MM.UserName like '%Bed Sheet%'
								--AND isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0)> 0
							    --and isnull(opbal2.TransactionQty,0)>0
								--and isnull(IFD1.IssueQty,0)>0
                         --Order By 
                        --IRD.IsAsset,
                       -- MM.UserName 					 
						--,ART.StandardName 	
						--,FCV.UserName 
						--,SCV.UserName 
						--,TCV.UserName

						--GROUP BY 
						--MT.UserName 
						--,MGM.UserName 			
						--,MM.UserName 					 
						--,ART.StandardName 	
						--,FCV.UserName 
						--,SCV.UserName 
						--,TCV.UserName";

				}
				else if (Asset == "true" && Inventory == "false" && Country == "true")
				{

					cmdText = @"SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]                
							   -- isnull(MT.UserName,'') MaterialType
							--, isnull(MGM.UserName,'') AS MaterialGroupMasterName						
								 ,isnull(MM.UserName,'') MaterialMasterName	
							 ,MM.Id		MaterialMasterId	
							,HSNC.Code HSNCode
							,isnull( ART.StandardName,'') ArticleName	
							 ,ART.Id	ArticleId			
							, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
							, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
							, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue ,MS.UserName MaterialStorageLocation	
                            ,isnull(C.UserName,'') CountryName
							,TUoM.UserName UOM,  opbal.IsAsset
							--,isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) As OpeningBalance	
							--,isnull(opbal.TotalMaterialBooksCurrencyAmount,0) AS OpeningBalanceAmount					

							--,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
							--,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
                         
							--,isnull(IFD1.IssueQty,0) IssueForThePeriod	
							--,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

							--,(isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0) Closing
							--,(isnull(opbal.TotalMaterialBooksCurrencyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0) ClosingAmount


                           -- ,isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) As OpeningBalance	
							--,isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) AS OpeningBalanceAmount

						

							--,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
							--,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount

                         
							--,isnull(IFD1.IssueQty,0) IssueForThePeriod	
							--,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

							--,(isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0) Closing
							--,(isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0) ClosingAmount
                            
                            --Opening Balance
                           ,isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) As OpeningBalance	
							,isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) AS OpeningBalanceAmount

						    --Receive

							,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
							,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
							--Issue
                         
							,isnull(IFD1.IssueQty,0) IssueForThePeriod	
							,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

							--Issue Return
							,isnull(IssueReturnData.Qty,0) IssueReturnQtyForThePeriod	
							,isnull(IssueReturnData.IssueReturnAmount,0) IssueReturnForThePeriodAmount	

							--Purchase Return
							,isnull(PurchaseReturnData.Qty,0) PurchaseReturnQtyForThePeriod	
							,isnull(PurchaseReturnData.PurchaseReturnAmount,0) PurchaseReturnForThePeriodAmount	

							--Adjust Return
							,isnull(AdjustmentData.Qty,0) AdjustmentQtyForThePeriod	
							,isnull(AdjustmentData.AdjustmentAmount,0) AdjustmentForThePeriodAmount	

                           --Inventory Sales
							,isnull(InventorySalesData.Qty,0) InventorySalesQtyForThePeriod	
							,isnull(InventorySalesData.InventorySalesAmount,0) InventorySalesForThePeriodAmount	

							--Inventory Scrap
							,isnull(InventoryScrapData.Qty,0) InventoryScrapQtyForThePeriod	
							,isnull(InventoryScrapData.InventoryScrapAmount,0) InventoryScrapForThePeriodAmount	
							--Inventory Transfer Data
							,isnull(InventoryTransferData.Qty,0) InventoryTransferQtyForThePeriod	
							,isnull(InventoryTransferData.InventoryTransferAmount,0) InventoryTransferForThePeriodAmount
							--Balance
							,(((((((isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0)-isnull(PurchaseReturnData.Qty,0))-isnull(AdjustmentData.Qty,0))+isnull(IssueReturnData.Qty,0))-isnull(InventorySalesData.Qty,0))-isnull(InventoryScrapData.Qty,0))-isnull(InventoryTransferData.Qty,0)) Closing 
							,(((((((isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0)-isnull(AdjustmentData.AdjustmentAmount,0))-isnull(PurchaseReturnData.PurchaseReturnAmount,0))+isnull(IssueReturnData.IssueReturnAmount,0)-isnull(InventorySalesData.InventorySalesAmount,0))-isnull(InventoryScrapData.InventoryScrapAmount,0))-isnull(InventoryTransferData.InventoryTransferAmount,0))) ClosingAmount



						from TRN.InventoryMaterial AS IM
						left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
						left join( select t.InventoryMaterialId,t.MaterialStorageId,t.IsAsset, sum(t.TransactionQty) TransactionQty,sum(t.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount 
			                        from (
			                        SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
			                        where convert(Date,IR.GRNDate) <= '" + toDate + @"' AND IR.OpeningBalanceId IS NOT NULL group By IRD.InventoryMaterialId,IRD.MaterialStorageId,IRD.IsAsset
			                        UNION ALL
			                        SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
			                        where convert(Date,IR.GRNDate) < '" + toDate + @"' AND IR.OpeningBalanceId IS NULL group By IRD.InventoryMaterialId,IRD.MaterialStorageId,IRD.IsAsset
			                        )as t group by t.InventoryMaterialId,t.MaterialStorageId,t.IsAsset
                                    ) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                        --left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
                        --left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                        --left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                        --left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        left join [HKP].[MaterialStorage] MS on ms.id=opbal.MaterialStorageId
                        LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
                        left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id
                        Left JOIN SCS.Country C On C.Id=IM.CountryId

						left join(  SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) > '" + toDate + @"'  group By IRD.InventoryMaterialId--) AS opbal1 ON opbal1.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                                    UNION ALL
                                    SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) = '" + toDate + @"'  group By IRD.InventoryMaterialId) AS opbal1 ON opbal1.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

						left join(SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate)	='" + toDate + @"' AND IR.OpeningBalanceId IS  NULL group By IRD.InventoryMaterialId) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

                        left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) OpeningIssueQty, Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 									
									WHERE convert(Date,II.IssueDate) < '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IID.InventoryMaterialId
									) IFD3 On IFD3.InventoryMaterialId=IM.Id
						left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									--LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
									WHERE convert(Date,II.IssueDate) = '" + toDate + @"' AND II.PlantId='" + plantId + @"'  GROUP BY IID.InventoryMaterialId
									) IFD On IFD.InventoryMaterialId=IM.Id

						left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									--LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
								WHERE convert(Date,II.IssueDate) = '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IID.InventoryMaterialId
								) IFD1 On IFD1.InventoryMaterialId=IM.Id

                        --Issue Return
                        Left join (select IH.InventoryMaterialId,sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty)*sum(IRD.MaterialTranRate)) IssueReturnAmount from trn.InventoryIssueReturnHistory IH
									 Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.IssueDate) = '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id
					    --Purchase return
                       Left join (select IH.InventoryMaterialId,sum(IH.TransactionQty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
					                 from trn.PurchaseReturnDetail IH
									 Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.[POReturnDate]) = '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id

                       -- Adjustment
						Left join (select psad.InventoryMaterialId,sum(IH.Qty) Qty,sum(IH.Rate) Rate, (sum(IH.Qty)*sum(IH.Rate)) AdjustmentAmount 
					                 from trn.PhysicalStockAdjustmentHistory IH
									 Left JOIN TRN.PhysicalStockAdjustmentDetail psad on psad.Id=IH.PhysicalStockAdjustmentDetailId
									 Left join trn.PhysicalStockAdjustmentMaster II ON II.Id=psad.PhysicalStockAdjustmentMasterID
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,II.IssueDate) = '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY psad.InventoryMaterialId
								 )AdjustmentData ON AdjustmentData.InventoryMaterialId=IM.Id


                                -- where opbal.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null 

					-- InventorySales
						Left join (select ISD.InventoryMaterialId,sum(ISH.Qty) Qty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty)*sum(ISH.BaseRate)) InventorySalesAmount 
					                 from [TRN].[InventorySalesHistory] ISH
									 Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
									 Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,Ins.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND Ins.PlantId='" + plantId + @"' GROUP BY ISD.InventoryMaterialId
								 )InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                                -- where IM.PlantId='20201' AND MM.UserName is not null --AND MM.UserName like '%Bed Sheet%'

                   --InventoryScrap
								Left join (select ISCD.InventoryMaterialId,sum(ISCH.Qty) Qty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
					                 from [TRN].[InventoryScrapHistory] ISCH
									 Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
									 Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,ISC.ScrapDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' GROUP BY ISCD.InventoryMaterialId
								 )InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id   
					--InventoryTransfer
								Left join ( select IRD.InventoryMaterialId,sum(IRD.InventoryTransferQty) Qty,sum(IRD.MaterialTranRate) Rate, (sum(IRD.InventoryTransferQty)*sum(IRD.MaterialTranRate)) InventoryTransferAmount 
					                 from [TRN].[InventoryTransferHistory] ITH
									 Left JOIN [TRN].[InventoryReceiveDetail] IRD on IRD.Id=ITH.InventoryReceiveDetailId
									 Left join [TRN].[InventoryReceive] IR on IR.Id=IRD.InventoryReceiveId
									 WHERE convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND IR.PlantId='" + plantId + @"'
									 GROUP BY IRD.InventoryMaterialId
								 )InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id    
                                  where opbal.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

--AND MM.UserName like '%Bed Sheet%'
								--AND isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0)> 0
							    --and isnull(opbal2.TransactionQty,0)>0
								--and isnull(IFD1.IssueQty,0)>0
                        -- Order By 
                        --IRD.IsAsset,
                        --MM.UserName				 
						--,ART.StandardName 	
						--,FCV.UserName 
						--,SCV.UserName 
						--,TCV.UserName

						--GROUP BY 
						--MT.UserName 
						--,MGM.UserName 			
						--,MM.UserName 					 
						--,ART.StandardName 	
						--,FCV.UserName 
						--,SCV.UserName 
						--,TCV.UserName";

				}
				#endregion
			}



			var inventoryMaterialList = _sqlRepository.GetDataTable(cmdText);
			var plantName = new DataView(_sqlRepository.GetDataTable(@"SELECT UserName from org.Plant WHERE Id='" + plantId + "'")).ToTable(true, "UserName").Rows[0]["UserName"].ToString();


			if (inventoryMaterialList.Rows.Count == 0)
				throw new Exception("No Data Found !!!");




			var _rowd = 4;

			if (fromDate != "" && toDate != "")
			{


				sheet1[_rowd, 3].Text = fromDate + " " + "To" + " " + toDate;

				sheet1.Range[_rowd, 3, _rowd, 6].CellStyle.Font.Size = 8;
				sheet1.Range[_rowd, 3, _rowd, 6].CellStyle.Font.Bold = false;
				sheet1.Range[_rowd, 3, _rowd, 6].Merge();
				//sheet1.Range[_rowd, 3, _rowd , 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;

			}

			else
			{

				sheet1.Range[_rowd, 3, _rowd, 4].Text = toDate;
				sheet1.Range[_rowd, 3, _rowd, 4].CellStyle.Font.Size = 8;
				sheet1.Range[_rowd, 3, _rowd, 4].CellStyle.Font.Bold = false;
				sheet1.Range[_rowd, 3, _rowd, 4].Merge();
				//sheet1.Range[_rowd, 3, _rowd , 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;

			}
			var _rows = 5;
			sheet1[_rows, 3].Text = "Report Ref No: ";
			sheet1.Range[_rows, 3, _rows, 6].CellStyle.Font.Size = 8;
			sheet1.Range[_rows, 3, _rows, 6].Merge();
			sheet1.Range[_rows, 3].CellStyle.Font.Bold = false;


			var _row = 7;
			var _rowL = _row;
			var row = _row + 1;


			var sheet1headreColIndex = 1;
			_rowL += 1;


			row++;
			if (fromDate != "" && toDate != "")
			{

				if (Amount != "" && Qty != "")
				{
					if (Country == "true")
					{
						sheet1.Range[_row, 13, _row, 14].Text = "Opening Balance";
						sheet1.Range[_row, 13, _row, 14].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 13, _row, 14].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 13, _row, 14].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 13, _row, 14].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 13, _row, 14].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 13, _row, 14].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 13, _row, 14].Merge();

						sheet1.Range[_row, 15, _row, 16].Text = "Material Receipts";
						sheet1.Range[_row, 15, _row, 16].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 15, _row, 16].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 15, _row, 16].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 15, _row, 16].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 15, _row, 16].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 15, _row, 16].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 15, _row, 16].Merge();



						sheet1.Range[_row, 17, _row, 18].Text = "Issue Material";
						sheet1.Range[_row, 17, _row, 18].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 17, _row, 18].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 17, _row, 18].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 17, _row, 18].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 17, _row, 18].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 17, _row, 18].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 17, _row, 18].Merge();

						sheet1.Range[_row, 19, _row, 20].Text = "Issue Material Return";
						sheet1.Range[_row, 19, _row, 20].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 19, _row, 20].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 19, _row, 20].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 19, _row, 20].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 19, _row, 20].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 19, _row, 20].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 19, _row, 20].Merge();


						sheet1.Range[_row, 21, _row, 22].Text = "Purchase Material Return";
						sheet1.Range[_row, 21, _row, 22].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 21, _row, 22].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 21, _row, 22].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 21, _row, 22].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 21, _row, 22].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 21, _row, 22].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 21, _row, 22].Merge();

						sheet1.Range[_row, 23, _row, 24].Text = "Adjustment Material";
						sheet1.Range[_row, 23, _row, 24].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 23, _row, 24].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 23, _row, 24].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 23, _row, 24].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 23, _row, 24].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 23, _row, 24].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 23, _row, 24].Merge();

						sheet1.Range[_row, 25, _row, 26].Text = "Inventory Sales";
						sheet1.Range[_row, 25, _row, 26].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 25, _row, 26].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 25, _row, 26].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 25, _row, 26].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 25, _row, 26].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 25, _row, 26].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 25, _row, 26].Merge();


						sheet1.Range[_row, 27, _row, 28].Text = "Inventory Scrap";
						sheet1.Range[_row, 27, _row, 28].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 27, _row, 28].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 27, _row, 28].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 27, _row, 28].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 27, _row, 28].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 27, _row, 28].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 27, _row, 28].Merge();

						sheet1.Range[_row, 29, _row, 30].Text = "Inventory Transfer";
						sheet1.Range[_row, 29, _row, 30].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 29, _row, 30].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 29, _row, 30].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 29, _row, 30].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 29, _row, 30].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 29, _row, 30].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 29, _row, 30].Merge();

						sheet1.Range[_row, 31, _row, 32].Text = "Closing Balance";
						sheet1.Range[_row, 31, _row, 32].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 31, _row, 32].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 31, _row, 32].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 31, _row, 32].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 31, _row, 32].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 31, _row, 32].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 31, _row, 32].Merge();
					}
					else
					{
						sheet1.Range[_row, 12, _row, 13].Text = "Opening Balance";
						sheet1.Range[_row, 12, _row, 13].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 12, _row, 13].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 12, _row, 13].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 12, _row, 13].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 12, _row, 13].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 12, _row, 13].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 12, _row, 13].Merge();

						sheet1.Range[_row, 14, _row, 15].Text = "Material Receipts";
						sheet1.Range[_row, 14, _row, 15].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 14, _row, 15].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 14, _row, 15].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 14, _row, 15].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 14, _row, 15].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 14, _row, 15].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 14, _row, 15].Merge();



						sheet1.Range[_row, 16, _row, 17].Text = "Issue Material";
						sheet1.Range[_row, 16, _row, 17].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 16, _row, 17].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 16, _row, 17].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 16, _row, 17].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 16, _row, 17].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 16, _row, 17].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 16, _row, 17].Merge();

						sheet1.Range[_row, 18, _row, 19].Text = "Issue Material Return";
						sheet1.Range[_row, 18, _row, 19].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 18, _row, 19].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 18, _row, 19].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 18, _row, 19].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 18, _row, 19].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 18, _row, 19].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 18, _row, 19].Merge();


						sheet1.Range[_row, 20, _row, 21].Text = "Purchase Material Return";
						sheet1.Range[_row, 20, _row, 21].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 20, _row, 21].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 20, _row, 21].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 20, _row, 21].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 20, _row, 21].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 20, _row, 21].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 20, _row, 21].Merge();

						sheet1.Range[_row, 22, _row, 23].Text = "Adjustment Material";
						sheet1.Range[_row, 22, _row, 23].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 22, _row, 23].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 22, _row, 23].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 22, _row, 23].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 22, _row, 23].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 22, _row, 23].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 22, _row, 23].Merge();

						sheet1.Range[_row, 24, _row, 25].Text = "Inventory Sales";
						sheet1.Range[_row, 24, _row, 25].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 24, _row, 25].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 24, _row, 25].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 24, _row, 25].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 24, _row, 25].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 24, _row, 25].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 24, _row, 25].Merge();


						sheet1.Range[_row, 26, _row, 27].Text = "Inventory Scrap";
						sheet1.Range[_row, 26, _row, 27].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 26, _row, 27].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 26, _row, 27].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 26, _row, 27].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 26, _row, 27].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 26, _row, 27].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 26, _row, 27].Merge();

						sheet1.Range[_row, 28, _row, 29].Text = "Inventory Transfer";
						sheet1.Range[_row, 28, _row, 29].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 28, _row, 29].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 28, _row, 29].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 28, _row, 29].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 28, _row, 29].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 28, _row, 29].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 28, _row, 29].Merge();

						sheet1.Range[_row, 30, _row, 31].Text = "Closing Balance";
						sheet1.Range[_row, 30, _row, 31].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 30, _row, 31].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 30, _row, 31].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 30, _row, 31].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 30, _row, 31].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 30, _row, 31].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 30, _row, 31].Merge();
					}


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "SL";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 5;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Mat.Mst.Id";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 23;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
					//sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article Id";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//sheet1.Range[_rowL, sheet1headreColIndex].Text = "Storage Location";
					//sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					//sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					//sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					//sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					//sheet1headreColIndex++;

					//sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRN Date";
					//sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					//sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					//sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					//sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					if (Country == "true")
					{
						//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Country Name");
						//sheet1headreColIndex++;

						sheet1.Range[_rowL, sheet1headreColIndex].Text = "Country Name";
						sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
						sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
						sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
						sheet1headreColIndex++;


					}

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "UOM";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Is Asset";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;
					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;


					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
					//sheet1headreColIndex++;



					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;


					sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
					sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
					sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;

				}
				else if (Amount != "" && Qty == "")
				{

					//               
					sheet1.Range[_rowL, sheet1headreColIndex].Text = "SL";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 5;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Mat.Mst.Id";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 23;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
					//sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article Id";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;


					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					if (Country == "true")
					{
						//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Country Name");
						//sheet1headreColIndex++;

						sheet1.Range[_rowL, sheet1headreColIndex].Text = "Country Name";
						sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
						sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
						sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
						sheet1headreColIndex++;



					}




					//sheet1.Range[_rowL, sheet1headreColIndex].Text = "UOM";
					//sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					//sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					//sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					//sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					//sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Is Asset";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Opening Qty");
					//sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Opening Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;



					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Receipts Qty");
					//sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Receipts Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 28;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;


					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Material Qty");
					//sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Material Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 28;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;



					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Material Return Qty");
					//sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Material Return Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Purchase Material Return Qty");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Purchase Material Return Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Adjustment Material Qty");
					//sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Adjustment Material Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Sales Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;




					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Scrap Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Transfer Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;



					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Closing Qty");


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Closing Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;


					sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
					sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
					sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;

				}
				else if (Amount == "" && Qty != "")
				{



					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material");
					//sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "SL";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 5;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Mat.Mst.Id";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 23;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
					//sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article Id";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;


					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					if (Country == "true")
					{
						//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Country Name");
						//sheet1headreColIndex++;

						sheet1.Range[_rowL, sheet1headreColIndex].Text = "Country Name";
						sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
						sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
						sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
						sheet1headreColIndex++;




					}
					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "UOM");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "UOM";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Is Asset";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Opening Qty");
					//sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Opening Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;



					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Receipts Qty");
					//sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Receipts Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 28;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;


					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Material Qty");
					//sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Material Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 28;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;



					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Material Return Qty");
					//sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Material Return Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Purchase Material Return Qty");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Purchase Material Return Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Adjustment Material Qty");
					//sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Adjustment Material Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Sales Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;




					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Scrap Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Transfer Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;



					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Closing Qty");


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Closing Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;



					sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
					sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
					sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;


				}
			}

			if (fromDate == "" && toDate != "")
			{


				if (Amount != "" && Qty != "")
				{
					if (Country == "true")
					{
						sheet1.Range[_row, 13, _row, 14].Text = "Opening Balance";
						sheet1.Range[_row, 13, _row, 14].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 13, _row, 14].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 13, _row, 14].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 13, _row, 14].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 13, _row, 14].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 13, _row, 14].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 13, _row, 14].Merge();

						sheet1.Range[_row, 15, _row, 16].Text = "Material Receipts";
						sheet1.Range[_row, 15, _row, 16].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 15, _row, 16].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 15, _row, 16].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 15, _row, 16].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 15, _row, 16].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 15, _row, 16].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 15, _row, 16].Merge();



						sheet1.Range[_row, 17, _row, 18].Text = "Issue Material";
						sheet1.Range[_row, 17, _row, 18].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 17, _row, 18].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 17, _row, 18].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 17, _row, 18].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 17, _row, 18].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 17, _row, 18].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 17, _row, 18].Merge();

						sheet1.Range[_row, 19, _row, 20].Text = "Issue Material Return";
						sheet1.Range[_row, 19, _row, 20].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 19, _row, 20].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 19, _row, 20].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 19, _row, 20].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 19, _row, 20].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 19, _row, 20].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 19, _row, 20].Merge();


						sheet1.Range[_row, 21, _row, 22].Text = "Purchase Material Return";
						sheet1.Range[_row, 21, _row, 22].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 21, _row, 22].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 21, _row, 22].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 21, _row, 22].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 21, _row, 22].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 21, _row, 22].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 21, _row, 22].Merge();

						sheet1.Range[_row, 23, _row, 24].Text = "Adjustment Material";
						sheet1.Range[_row, 23, _row, 24].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 23, _row, 24].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 23, _row, 24].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 23, _row, 24].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 23, _row, 24].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 23, _row, 24].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 23, _row, 24].Merge();

						sheet1.Range[_row, 25, _row, 26].Text = "Inventory Sales";
						sheet1.Range[_row, 25, _row, 26].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 25, _row, 26].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 25, _row, 26].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 25, _row, 26].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 25, _row, 26].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 25, _row, 26].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 25, _row, 26].Merge();


						sheet1.Range[_row, 27, _row, 28].Text = "Inventory Scrap";
						sheet1.Range[_row, 27, _row, 28].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 27, _row, 28].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 27, _row, 28].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 27, _row, 28].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 27, _row, 28].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 27, _row, 28].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 27, _row, 28].Merge();

						sheet1.Range[_row, 29, _row, 30].Text = "Inventory Transfer";
						sheet1.Range[_row, 29, _row, 30].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 29, _row, 30].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 29, _row, 30].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 29, _row, 30].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 29, _row, 30].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 29, _row, 30].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 29, _row, 30].Merge();

						sheet1.Range[_row, 31, _row, 32].Text = "Closing Balance";
						sheet1.Range[_row, 31, _row, 32].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 31, _row, 32].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 31, _row, 32].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 31, _row, 32].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 31, _row, 32].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 31, _row, 32].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 31, _row, 32].Merge();
					}
					else
					{
						sheet1.Range[_row, 12, _row, 13].Text = "Opening Balance";
						sheet1.Range[_row, 12, _row, 13].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 12, _row, 13].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 12, _row, 13].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 12, _row, 13].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 12, _row, 13].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 12, _row, 13].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 12, _row, 13].Merge();

						sheet1.Range[_row, 14, _row, 15].Text = "Material Receipts";
						sheet1.Range[_row, 14, _row, 15].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 14, _row, 15].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 14, _row, 15].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 14, _row, 15].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 14, _row, 15].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 14, _row, 15].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 14, _row, 15].Merge();



						sheet1.Range[_row, 16, _row, 17].Text = "Issue Material";
						sheet1.Range[_row, 16, _row, 17].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 16, _row, 17].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 16, _row, 17].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 16, _row, 17].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 16, _row, 17].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 16, _row, 17].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 16, _row, 17].Merge();

						sheet1.Range[_row, 18, _row, 19].Text = "Issue Material Return";
						sheet1.Range[_row, 18, _row, 19].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 18, _row, 19].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 18, _row, 19].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 18, _row, 19].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 18, _row, 19].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 18, _row, 19].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 18, _row, 19].Merge();


						sheet1.Range[_row, 20, _row, 21].Text = "Purchase Material Return";
						sheet1.Range[_row, 20, _row, 21].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 20, _row, 21].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 20, _row, 21].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 20, _row, 21].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 20, _row, 21].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 20, _row, 21].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 20, _row, 21].Merge();

						sheet1.Range[_row, 22, _row, 23].Text = "Adjustment Material";
						sheet1.Range[_row, 22, _row, 23].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 22, _row, 23].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 22, _row, 23].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 22, _row, 23].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 22, _row, 23].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 22, _row, 23].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 22, _row, 23].Merge();

						sheet1.Range[_row, 24, _row, 25].Text = "Inventory Sales";
						sheet1.Range[_row, 24, _row, 25].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 24, _row, 25].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 24, _row, 25].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 24, _row, 25].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 24, _row, 25].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 24, _row, 25].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 24, _row, 25].Merge();


						sheet1.Range[_row, 26, _row, 27].Text = "Inventory Scrap";
						sheet1.Range[_row, 26, _row, 27].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 26, _row, 27].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 26, _row, 27].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 26, _row, 27].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 26, _row, 27].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 26, _row, 27].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 26, _row, 27].Merge();

						sheet1.Range[_row, 28, _row, 29].Text = "Inventory Transfer";
						sheet1.Range[_row, 28, _row, 29].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 28, _row, 29].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 28, _row, 29].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 28, _row, 29].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 28, _row, 29].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 28, _row, 29].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 28, _row, 29].Merge();

						sheet1.Range[_row, 30, _row, 31].Text = "Closing Balance";
						sheet1.Range[_row, 30, _row, 31].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 30, _row, 31].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 30, _row, 31].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 30, _row, 31].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 30, _row, 31].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 30, _row, 31].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 30, _row, 31].Merge();
					}






					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Type");
					//sheet1headreColIndex++;
					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Group");
					//sheet1headreColIndex++;
					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "SL";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 5;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Mat.Mst.Id";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 23;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
					//sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article Id";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					if (Country == "true")
					{
						//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Country Name");
						//sheet1headreColIndex++;

						sheet1.Range[_rowL, sheet1headreColIndex].Text = "Country Name";
						sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
						sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
						sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
						sheet1headreColIndex++;


					}

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "UOM";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Is Asset";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;
					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;


					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
					//sheet1headreColIndex++;



					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;



					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;


					sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
					sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
					sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;

				}
				else if (Amount != "" && Qty == "")
				{

					//               
					sheet1.Range[_rowL, sheet1headreColIndex].Text = "SL";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 5;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Mat.Mst.Id";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 23;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
					//sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article Id";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;
					if (Country == "true")
					{
						//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Country Name");
						//sheet1headreColIndex++;

						sheet1.Range[_rowL, sheet1headreColIndex].Text = "Country Name";
						sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
						sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
						sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
						sheet1headreColIndex++;



					}




					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Is Asset";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;


					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Opening Amount");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Opening Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;



					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Receipts Amount");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Receipts Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;




					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Amount");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Material Return Amount");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Material Return Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Purchase Material Return Amount");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Purchase Material Return Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;


					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Adjustment Material Amount");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Adjustment Material Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 28;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;



					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Sales Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;




					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Scrap Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Transfer Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;
					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Closing Amount");

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Closing Amount";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 28;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;


					sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
					sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
					sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;

				}
				else if (Amount == "" && Qty != "")
				{



					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material");
					//sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "SL";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 5;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Mat.Mst.Id";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 23;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
					//sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article Id";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;


					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					if (Country == "true")
					{
						//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Country Name");
						//sheet1headreColIndex++;

						sheet1.Range[_rowL, sheet1headreColIndex].Text = "Country Name";
						sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
						sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
						sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
						sheet1headreColIndex++;




					}
					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "UOM");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "UOM";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Is Asset";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Opening Qty");
					//sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Opening Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;



					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Receipts Qty");
					//sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Receipts Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 28;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;


					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Material Qty");
					//sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Material Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 28;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;



					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Material Return Qty");
					//sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Material Return Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Purchase Material Return Qty");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Purchase Material Return Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Adjustment Material Qty");
					//sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Adjustment Material Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Sales Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;




					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Scrap Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Transfer Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;


					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Closing Qty");


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Closing Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;



					sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
					sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
					sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;


				}
			}
			int sl = 0;
			var Row_Total_Start = _rowL + 1;
			for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
			{
				sl++;
				_rowL++;

				if (fromDate != "" && toDate != "")
				{
					if (Amount != "" && Qty != "")
					{
						report.SetText(ref sheet1, _rowL, 1, sl.ToString());
						report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["MaterialMasterId"].ToString());
						report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
						report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["ArticleId"].ToString());
						report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
						report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString());
						//report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["MaterialStorageLocation"].ToString());
						//report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["GRNDate"].ToString());
						report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["HSNCode"].ToString());

						if (Country == "true")
						{
							report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["CountryName"].ToString());
							report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["UOM"].ToString());
							report.SetText(ref sheet1, _rowL, 12, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
							report.SetText(ref sheet1, _rowL, 13, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
							report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 15, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));



							report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 21, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 22, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 23, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 24, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 25, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 26, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 27, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 28, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 29, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 30, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 31, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString()));
							report.SetText(ref sheet1, _rowL, 32, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ClosingAmount"].ToString()));
						}
						else
						{
							report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["UOM"].ToString());
							report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
							report.SetText(ref sheet1, _rowL, 12, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
							report.SetText(ref sheet1, _rowL, 13, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 15, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));



							report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 21, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 22, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 23, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));


							report.SetText(ref sheet1, _rowL, 24, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 25, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 26, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 27, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapForThePeriodAmount"].ToString()));


							report.SetText(ref sheet1, _rowL, 28, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 29, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferForThePeriodAmount"].ToString()));


							report.SetText(ref sheet1, _rowL, 30, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString()));
							report.SetText(ref sheet1, _rowL, 31, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ClosingAmount"].ToString()));
						}





					}
					else if (Amount != "" && Qty == "")
					{

						report.SetText(ref sheet1, _rowL, 1, sl.ToString());
						report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["MaterialMasterId"].ToString());
						report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
						report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["ArticleId"].ToString());
						report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
						report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["HSNCode"].ToString());

						if (Country == "true")
						{
							report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["CountryName"].ToString());
							report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
							//report.SetText(ref sheet1, _rowL, 6, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
							report.SetText(ref sheet1, _rowL, 12, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

							//report.SetText(ref sheet1, _rowL, 8, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 13, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

							//report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));


							// report.SetText(ref sheet1, _rowL, 12, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 15, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

							// report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

							//report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesForThePeriodAmount"].ToString()));
							report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapForThePeriodAmount"].ToString()));


							report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 21, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ClosingAmount"].ToString()));
						}
						else
						{
							//report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["CountryName"].ToString());
							//report.SetText(ref sheet1, _rowL, 6, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
							report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
							report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

							//report.SetText(ref sheet1, _rowL, 8, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 12, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

							//report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 13, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));


							// report.SetText(ref sheet1, _rowL, 12, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

							// report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 15, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

							//report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesForThePeriodAmount"].ToString()));
							report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferForThePeriodAmount"].ToString()));


							report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ClosingAmount"].ToString()));

						}
					}
					else if (Amount == "" && Qty != "")
					{

						report.SetText(ref sheet1, _rowL, 1, sl.ToString());
						report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["MaterialMasterId"].ToString());
						report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
						report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["ArticleId"].ToString());
						report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
						report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["HSNCode"].ToString());
						if (Country == "true")
						{
							report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["CountryName"].ToString());
							report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["UOM"].ToString());
							report.SetText(ref sheet1, _rowL, 12, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
							report.SetText(ref sheet1, _rowL, 13, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
							//report.SetText(ref sheet1, _rowL, 7, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 15, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));

							//report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
							// report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL,11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));
							report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));


							report.SetText(ref sheet1, _rowL, 21, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferQtyForThePeriod"].ToString()));

							report.SetText(ref sheet1, _rowL, 22, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString()));
						}
						else
						{

							report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["UOM"].ToString());
							report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
							report.SetText(ref sheet1, _rowL, 12, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
							//report.SetText(ref sheet1, _rowL, 7, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 13, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));

							//report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 15, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
							// report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));
							report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));

							report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferQtyForThePeriod"].ToString()));

							report.SetText(ref sheet1, _rowL, 21, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString()));

						}

					}
				}
				if (fromDate == "" && toDate != "")
				{

					if (Amount != "" && Qty != "")
					{
						report.SetText(ref sheet1, _rowL, 1, sl.ToString());
						report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["MaterialMasterId"].ToString());
						report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
						report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["ArticleId"].ToString());
						report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
						report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["HSNCode"].ToString());
						if (Country == "true")
						{
							report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["CountryName"].ToString());
							report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["UOM"].ToString());
							report.SetText(ref sheet1, _rowL, 12, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
							report.SetText(ref sheet1, _rowL, 13, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
							report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 15, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));



							report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 21, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 22, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 23, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 24, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 25, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 26, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 27, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 28, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapForThePeriodAmount"].ToString()));



							report.SetText(ref sheet1, _rowL, 29, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString()));
							report.SetText(ref sheet1, _rowL, 30, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ClosingAmount"].ToString()));
						}
						else
						{
							report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["UOM"].ToString());
							report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
							report.SetText(ref sheet1, _rowL, 12, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
							report.SetText(ref sheet1, _rowL, 13, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 15, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));



							report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 21, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 22, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 23, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));


							report.SetText(ref sheet1, _rowL, 24, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 25, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 26, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 27, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 28, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 29, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 30, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString()));
							report.SetText(ref sheet1, _rowL, 31, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ClosingAmount"].ToString()));
						}





					}
					else if (Amount != "" && Qty == "")
					{

						report.SetText(ref sheet1, _rowL, 1, sl.ToString());
						report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["MaterialMasterId"].ToString());
						report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
						report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["ArticleId"].ToString());
						report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
						report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["HSNCode"].ToString());

						if (Country == "true")
						{
							report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["CountryName"].ToString());
							//report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["UOM"].ToString());
							report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
							report.SetText(ref sheet1, _rowL, 12, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
							//report.SetText(ref sheet1, _rowL, 7, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 13, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL,9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));

							//report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 15, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
							// report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));
							report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));


							report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));

							report.SetText(ref sheet1, _rowL, 21, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString()));
						}
						else
						{

							//report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["UOM"].ToString());
							report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
							report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
							//report.SetText(ref sheet1, _rowL, 7, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 12, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 13, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));

							//report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 15, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
							// report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));
							report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));

							report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));

							report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString()));

						}
					}
					else if (Amount == "" && Qty != "")
					{

						report.SetText(ref sheet1, _rowL, 1, sl.ToString());
						report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["MaterialMasterId"].ToString());
						report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
						report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["ArticleId"].ToString());
						report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
						report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["HSNCode"].ToString());
						if (Country == "true")
						{
							report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["CountryName"].ToString());
							report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["UOM"].ToString());
							report.SetText(ref sheet1, _rowL, 12, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
							report.SetText(ref sheet1, _rowL, 13, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
							//report.SetText(ref sheet1, _rowL, 7, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 15, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));

							//report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
							// report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));
							report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));


							report.SetText(ref sheet1, _rowL, 21, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));

							report.SetText(ref sheet1, _rowL, 22, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString()));
						}
						else
						{

							report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["UOM"].ToString());
							report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
							report.SetText(ref sheet1, _rowL, 12, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
							//report.SetText(ref sheet1, _rowL, 7, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 13, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));

							//report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 15, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
							// report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));
							report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));

							report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));

							report.SetText(ref sheet1, _rowL, 21, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString()));

						}

					}


				}


			}


			sheet1.Range[(Row_Total_Start), 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 8;
			sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
			//_rowL++;
			sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
			sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);

			sheet1.Name = sheet1Name;
			sheet1.UsedRange.WrapText = true;
			//sheet1.UsedRange.CellStyle.Font.Size = 8;
			sheet1.IsGridLinesVisible = false;
			report.PlantHeader(ref sheet1, sheet1headreColIndex, sheet1Name, plantId);
			report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);


		}


		#endregion


		public void InOutGatePassUncheckUpdate(string ComId, string CheckedApprovedStataus, string CheckedHoldRejectReason, Dictionary<string, object> UserSendData)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				string sql = "select * from [TRN].InOutGatePassMaster where Id='" + ComId + "'";
				ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
				con.OpenDataSetThroughAdapter(sql, out DataSet dsDetail, false, "1");

				//string sqllog = "select * from TRN.GRNApprovalLogTbl where 1=2";
				//con.OpenDataSetThroughAdapter(sqllog, out DataSet dsDetailLog, false, "1");

				//for (int i = 0; i < UserSendData.Count; i++)
				//{
				dsDetail.Tables[0].DefaultView.RowFilter = "Id='" + UserSendData["Id"].ToString() + "'";
				if (dsDetail.Tables[0].DefaultView.Count == 0)
				{
					//DataRow dr = dsDetail.Tables[0].NewRow();
					//dr["Id"] = GRNDAddiTaxId();
					//dr["TaxCodeId"] = UserSendData[i]["TaxCodeId"];
					//dr["Percentage"] = UserSendData[i]["ValueOfFixed"];
					//dr["TaxAmount"] = UserSendData[i]["TaxAmount"];
					//dr["AddedBy"] = identity.Name;
					//dr["AddedDate"] = System.DateTime.Now.ToString();
					//dr["AddedFromIP"] = identity.IPAddress;
					////dr["UpdatedBy"] = "";
					////dr["UpdatedDate"] = "";
					////dr["UpdatedFromIP"] = "";
					//dr["InventoryReceiveId"] = MasterId.ToString();
					//dsDetail.Tables[0].Rows.Add(dr);
				}
				else
				{
					DataRow dr = dsDetail.Tables[0].DefaultView[0].Row;
					dr.BeginEdit();
					dr["CheckedByStatus"] = CheckedApprovedStataus;
					dr["CheckedHoldRejectReason"] = CheckedHoldRejectReason;
					//dr["AuthorizedByStatus"] = null;
					//dr["IsApproved"] = 0;
					dr.EndEdit();
					//DataRow drlog = dsDetailLog.Tables[0].NewRow();
					//drlog["Id"] = MasterId.ToString() + '-' + GRNApprovalLogTblId();
					//drlog["CompanyGroupId"] = identity.CompanyGroupId;
					//drlog["CompanyId"] = identity.CompanyId;
					//drlog["PlantId"] = identity.PlantId;
					//drlog["ApprovedBy"] = identity.EmployeeId;
					//drlog["Date"] = System.DateTime.Now.ToString();
					//drlog["POValue"] = UserSendData["TransactionQty"];
					//drlog["Status"] = "UnChecked";
					//drlog["AddedBy"] = identity.Name;
					//drlog["AddedDate"] = System.DateTime.Now.ToString();
					//drlog["AddedFromIP"] = identity.IPAddress;
					////dr["UpdatedBy"] = "";
					////dr["UpdatedDate"] = "";
					////dr["UpdatedFromIP"] = "";
					//drlog["GRNID"] = MasterId.ToString();
					//dsDetailLog.Tables[0].Rows.Add(drlog);
				}
				//}


				clsStaticInfo info = new clsStaticInfo();
				info.SaveDataSets(dsDetail);//, dsDetailLog
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public void PendingInOutGatePassUpdate(string ComId, Dictionary<string, object> UserSendData)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				string sql = "select * from [TRN].InOutGatePassMaster where Id='" + ComId + "'";
				ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
				con.OpenDataSetThroughAdapter(sql, out DataSet dsDetail, false, "1");

				//string sqllog = "select * from TRN.GRNApprovalLogTbl where 1=2";
				//con.OpenDataSetThroughAdapter(sqllog, out DataSet dsDetailLog, false, "1");

				//for (int i = 0; i < UserSendData.Count; i++)
				//{
				dsDetail.Tables[0].DefaultView.RowFilter = "Id='" + UserSendData["Id"].ToString() + "'";
				if (dsDetail.Tables[0].DefaultView.Count == 0)
				{
					//DataRow dr = dsDetail.Tables[0].NewRow();

				}
				else
				{
					DataRow dr = dsDetail.Tables[0].DefaultView[0].Row;
					dr.BeginEdit();
					dr["GateOutStatus"] = true;
					//dr["CheckedByStatus"] = CheckedApprovedStataus;
					//dr["CheckedHoldRejectReason"] = CheckedHoldRejectReason;
					//dr["AuthorizedByStatus"] = null;
					//dr["IsApproved"] = 0;
					dr.EndEdit();

				}



				clsStaticInfo info = new clsStaticInfo();
				info.SaveDataSets(dsDetail);//, dsDetailLog
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}


		public void PendingIndivisulGatePassUpdate(string ComId, Dictionary<string, object> UserSendData)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				string sql = "select * from [TRN].GatePassMaster where Id='" + ComId + "'";
				ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
				con.OpenDataSetThroughAdapter(sql, out DataSet dsDetail, false, "1");
				dsDetail.Tables[0].DefaultView.RowFilter = "Id='" + UserSendData["Id"].ToString() + "'";
				if (dsDetail.Tables[0].DefaultView.Count == 0)
				{

				}
				else
				{
					DataRow dr = dsDetail.Tables[0].DefaultView[0].Row;
					dr.BeginEdit();
					dr["GateOutStatus"] = true;
					dr["SenderSecurityApprovedStatus"] = "GateOut";
					dr.EndEdit();
				}



				clsStaticInfo info = new clsStaticInfo();
				info.SaveDataSets(dsDetail);//, dsDetailLog
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}


		public IWorkbook CreateMaterialStoreLedgerAll(string companyId, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string MaterialId, string ArticleId, string Asset, string Inventory)
		{
			try
			{
				var excelEngine = new ExcelEngine();
				var report = new Service.Helpers.ReportUtility();
				var workbook = report.GetWorkbook(ref excelEngine, 2);
				var sheet1 = workbook.Worksheets[0];
				var sheet2 = workbook.Worksheets[1];
				var Head = "";
				if (Asset == "" || Asset == "undefined" || Asset == null || Asset == "false")
					Asset = null;
				if (Inventory == "" || Inventory == "undefined" || Inventory == null || Inventory == "false")
					Inventory = null;
				if (Asset == null && Inventory != null)
				{
					Head = "Material Store Ledger Of Inventory";// + fromDate + "To" + toDate + "";
				}
				if (Asset != null && Inventory == null)
				{
					Head = "Material Store Ledger Of Asset";// + fromDate + "To" + toDate + "";
				}
				if (Asset != null && Inventory != null)
				{
					Head = "Material Store Ledger Of Inventory And Asset";// + fromDate + "To" + toDate + "";
				}
				//Head = "Material Store Ledger ";//Material Store Ledger as on"+ " " + toDate;
				CreateMaterialStoreLedgerAll(ref sheet1, ref sheet2, report, Head, "Summary", companyId, plantId, fromDate, toDate, Qty, Amount, RcptIssue, MaterialId, ArticleId, Asset, Inventory);
				workbook.Version = ExcelVersion.Excel2016;
				return workbook;
			}
			catch (Exception)
			{
				throw;
			}
		}

		private void CreateMaterialStoreLedgerAll(ref IWorksheet sheet1, ref IWorksheet sheet2, ReportUtility report, string sheet1Name, string sheet2Name, string companyId, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string MaterialId, string ArticleId, string Asset, string Inventory)
		{
			try
			{


				if (Asset == "" || Asset == "undefined" || Asset == null || Asset == "false")
					Asset = null;
				if (Inventory == "" || Inventory == "undefined" || Inventory == null || Inventory == "false")
					Inventory = null;

				var cmdText = "";
				if (Asset != null && Inventory != null)
				{
					cmdText = @"select 					
					dense_rank() over (partition by IR.GRNDate, IR.AddedDate,IRD.Id order by IR.GRNDate, IR.AddedDate,IRD.Id ,main.IssueDate DESC) AS Seq,	 IRD.Id
                     ,isnull(MM.UserName,'') MaterialMasterName	
					,MM.id MId
					,isnull( ART.StandardName,'') ArticleName		
					,ART.id ARTId
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 	
					,TUoM.UserName UOM,main.IssueType
					,REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') RcvDate
					,Round(IRD.TransactionQty,2) RcvQty
					,Round(IRD.BooksCurrencyBaseRate,2) RcvRate
					,Round(IRD.TotalMaterialBooksCurrencyAmount,2) RcvAmount	
					,IR.VoucherId VoucherNo

					,REPLACE(CONVERT(CHAR(11), main.IssueDate, 106),' ','-') IssueDate
					,main.IssueNo IssueNo
					,isnull(Round(main.IssueQty,2),0) IssueQty
					,isnull(Round(main.Rate,2),0) Rate
					,isnull(Round(main.IssueAmount,2),0) IssueAmount
					,main.IssueVoucherNo


                      ,REPLACE(CONVERT(CHAR(11), main.POReturnDate, 106),' ','-') PurchaseReturnDate
					,main.PurchaseReturnNo PurchaseReturnNo
					,isnull(Round(main.PurchaseReturnQty,2),0) PurchaseReturnQty
					,isnull(Round(main.PurchaseReturnRate,2),0) PurchaseReturnRate
					,isnull(Round(main.PurchaseReturnAmount,2),0) PurchaseReturnAmount


                      ,REPLACE(CONVERT(CHAR(11), main.ReturnIssueDate, 106),' ','-') IssueReturnDate
					,main.ReturnIssueReturnNo IssueReturnNo
					,isnull(Round(main.IssueReturnQty,2),0) IssueReturnQty
					,isnull(Round(main.IssueReturnRate,2),0) IssueReturnRate
					,isnull(Round(main.IssueReturnAmount,2),0) IssueReturnAmount


					,REPLACE(CONVERT(CHAR(11), main.PhysicalIssueDate, 106),' ','-') AdjustmentDate
					,main.PhysicalIssueNo AdjustmentNo
					,isnull(Round(main.PhysicalStockAdjustmentqty,2),0) AdjustmentQty
					,isnull(Round(main.PhysicalStockAdjustmentRate,2),0) AdjustmentRate
					,isnull(Round(main.PhysicalStockAdjustmentAmount,2),0) AdjustmentAmount						




					,Round(((((IRD.TransactionQty-isnull(Round(main.PurchaseReturnQty,2),0))-isnull(Round(main.IssueQty,2),0))+isnull(Round(main.IssueReturnQty,2),0))-isnull(Round(main.PhysicalStockAdjustmentqty,2),0)),2) BalanceQty

					,CASE WHEN Round((IRD.TransactionQty- isnull(main.IssueQty,0)),2)>0 then Round(IRD.BooksCurrencyBaseRate,2) else 0 END BalanceRate
					,Round((((((IRD.TransactionQty-isnull(Round(main.PurchaseReturnQty,2),0))-isnull(Round(main.IssueQty,2),0))+isnull(Round(main.IssueReturnQty,2),0))-isnull(Round(main.PhysicalStockAdjustmentqty,2),0))* IRD.BooksCurrencyBaseRate),2) BalanceAmount

					,IRD.IsAsset						
					,CASE WHEN IRD.IsAsset=1 THEN 'Asset' ELSE 'Inventory' END IsAssetStatus
					--select x.InventoryReceiveDetailId,x.IssueDate,x.IssueNo,x.IssueType,x.PurchaseReturnNo,x.POReturnDate,x.ReturnIssueDate,x.ReturnIssueReturnNo,x.PhysicalIssueDate,x.PhysicalIssueNo,

					-- x.IssueAmount,x.IssueQty,x.Rate,x.PurchaseReturnQty,x.PurchaseReturnRate,x.PurchaseReturnAmount,x.IssueReturnQty,x.IssueReturnRate

					-- ,x.IssueReturnAmount,x.PhysicalStockAdjustmentqty,x.PhysicalStockAdjustmentRate,x.PhysicalStockAdjustmentAmount
 
					--from (
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
						left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id	
						left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
						left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
						left join(select IH.InventoryReceiveDetailId,II.IssueDate,II.Id IssueNo,II.IssueType,II.VoucherId IssueVoucherNo,NULL POReturnDate,NULL PurchaseReturnNo, NULL ReturnIssueDate  ,NULL ReturnIssueReturnNo, NULL PhysicalIssueDate,NULL PhysicalIssueNo

                                    ,(Sum(Isnull(Ih.Qty,0))) IssueQty 
                                    ,Sum(IH.Rate) Rate
                                    ,(Sum(Isnull(Ih.Qty,0))*Sum(IH.Rate)) AS IssueAmount
                                    ,0 PurchaseReturnQty,0 PurchaseReturnRate,0 PurchaseReturnAmount
                                    ,0 IssueReturnQty,0 IssueReturnRate,0 IssueReturnAmount
                                    , 0 PhysicalStockAdjustmentqty   ,0 PhysicalStockAdjustmentRate ,0 PhysicalStockAdjustmentAmount
                                     from  [TRN].[InventoryReceiveDetail] AS IRD 
                                     LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryReceiveDetailId=IRD.ID 
                                     LEFT JOIN  TRN.InventoryIssueDetail IID on IID.ID=IH.InventoryIssueDetailId
                                     LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id 
                                     Where  Ih.Qty>0 --ANd IH.InventoryReceiveDetailId='2020258-1'
                                     group by IH.InventoryReceiveDetailId,II.IssueDate,II.Id,II.IssueType,II.VoucherId

                                     Union all

                        select  IH.InventoryReceiveDetailId,NULL IssueDate,NULL IssueNo,NULL IssueType,NULL IssueVoucherNo,II.POReturnDate  ,II.Id PurchaseReturnNo, NULL ReturnIssueDate  ,NULL ReturnIssueReturnNo, NULL PhysicalIssueDate,NULL PhysicalIssueNo

                                    , 0 IssueQty,0 Rate,0 IssueAmount
                                     ,(Sum(Isnull(IH.TransactionQty,0))) PurchaseReturnQty 
                                     ,Sum(IH.MaterialTranRate) PurchaseReturnRate
                                     ,(Sum(Isnull(IH.TransactionQty,0))*Sum(IH.MaterialTranRate)) AS PurchaseReturnAmount
                                       ,0 IssueReturnQty,0 IssueReturnRate,0 IssueReturnAmount
                                   , 0 PhysicalStockAdjustmentqty   ,0 PhysicalStockAdjustmentRate ,0 PhysicalStockAdjustmentAmount
                                     from  [TRN].[InventoryReceiveDetail] AS IRD 
                                     LEFT JOIN TRN.PurchaseReturnDetail IH On IH.InventoryReceiveDetailId=IRD.ID 
                                     LEFT JOIN TRN.PurchaseReturn II ON IH.PurchaseReturnId=II.Id 
                                     Where Ih.TransactionQty>0 --ANd IH.InventoryReceiveDetailId='2020258-1'
                                     group by IH.InventoryReceiveDetailId,II.POReturnDate ,II.Id


                             Union all
 
 

                        select  IH.InventoryReceiveDetailId,NULL IssueDate,NULL IssueNo,NULL IssueType,NULL IssueVoucherNo,NULL POReturnDate  ,NULL PurchaseReturnNo,    II.IssueDate ReturnIssueDate ,II.Id ReturnIssueReturnNo, NULL PhysicalIssueDate,NULL PhysicalIssueNo

                                     , 0 IssueQty,0 Rate,0 IssueAmount,0 PurchaseReturnQty
                                     ,0 PurchaseReturnRate,0PurchaseReturnAmount
                                    ,(Sum(Isnull(IH.Qty,0))) IssueReturnQty 
                                    ,Sum(IH.Rate) IssueReturnRate
                                    ,(Sum(Isnull(IH.Qty,0))*Sum(IH.Rate)) AS IssueReturnAmount 
                                    , 0 PhysicalStockAdjustmentqty   ,0 PhysicalStockAdjustmentRate ,0 PhysicalStockAdjustmentAmount
                                     from  [TRN].[InventoryReceiveDetail] AS IRD 
                                     LEFT JOIN TRN.InventoryIssueReturnHistory IH On IH.InventoryReceiveDetailId=IRD.ID 
                                     --LEFT JOIN  TRN.InventoryIssueDetail IID on IID.ID=IH.InventoryIssueDetailId
                                     LEFT JOIN TRN.InventoryIssueReturn II ON IH.InventoryIssueReturnId=II.Id 
                                     Where  Ih.Qty>0 --ANd IH.InventoryReceiveDetailId='2020258-1'
                                     group by IH.InventoryReceiveDetailId,II.IssueDate ,II.Id

                             Union all
                        select  IH.InventoryReceiveDetailId,NULL IssueDate,NULL IssueNo,NULL IssueType,NULL IssueVoucherNo,NULL POReturnDate  ,NULL PurchaseReturnNo,    NULL ReturnIssueDate ,NULL IssueReturnNo, II.IssueDate PhysicalIssueDate,II.Id PhysicalIssueNo
									, 0 IssueQty,0 Rate,0 IssueAmount,0 PurchaseReturnQty
                                     ,0 PurchaseReturnRate,0PurchaseReturnAmount
                                      ,0 IssueReturnQty,0 IssueReturnRate,0 IssueReturnAmount
                                    ,(Sum(Isnull(Ih.Qty,0))) PhysicalStockAdjustmentqty 
                                    ,Sum(IH.Rate) PhysicalStockAdjustmentRate
                                    ,(Sum(Isnull(Ih.Qty,0))*Sum(IH.Rate)) AS PhysicalStockAdjustmentAmount
                                     from  [TRN].[InventoryReceiveDetail] AS IRD 
                                     LEFT JOIN TRN.PhysicalStockAdjustmentHistory IH On IH.InventoryReceiveDetailId=IRD.ID 
                                     LEFT JOIN  TRN.PhysicalStockAdjustmentDetail IID on IID.ID=IH.PhysicalStockAdjustmentDetailId
                                     LEFT JOIN TRN.PhysicalStockAdjustmentMaster II ON IID.PhysicalStockAdjustmentMasterID=II.Id 
                                     Where Ih.Qty>0 --ANd IH.InventoryReceiveDetailId='2020258-1'
                                     group by IH.InventoryReceiveDetailId,II.IssueDate,II.Id,II.IssueType
                                     )main on main.InventoryReceiveDetailId=IRD.Id									 
						where  	Convert(date ,IR.GRNDate) between '" + fromDate + @"' AND '" + toDate + @"'
						Order By IR.GRNDate, IR.AddedDate,IRD.Id ,main.IssueDate DESC";
				}
				if (Asset == null && Inventory != null)
				{
					cmdText = @"select 					
					 dense_rank() over (partition by IR.GRNDate, IR.AddedDate,IRD.Id order by IR.GRNDate, IR.AddedDate,IRD.Id ,main.IssueDate DESC) AS Seq,	 IRD.Id
                     ,isnull(MM.UserName,'') MaterialMasterName	
					,MM.id MId
					,isnull( ART.StandardName,'') ArticleName		
					,ART.id ARTId
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 	
					,TUoM.UserName UOM,main.IssueType
					,REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') RcvDate
					,Round(IRD.TransactionQty,2) RcvQty
					,Round(IRD.BooksCurrencyBaseRate,2) RcvRate
					,Round(IRD.TotalMaterialBooksCurrencyAmount,2) RcvAmount	
					,IR.VoucherId VoucherNo

					,REPLACE(CONVERT(CHAR(11), main.IssueDate, 106),' ','-') IssueDate
					,main.IssueNo IssueNo
					,isnull(Round(main.IssueQty,2),0) IssueQty
					,isnull(Round(main.Rate,2),0) Rate
					,isnull(Round(main.IssueAmount,2),0) IssueAmount
					,main.IssueVoucherNo


                      ,REPLACE(CONVERT(CHAR(11), main.POReturnDate, 106),' ','-') PurchaseReturnDate
					,main.PurchaseReturnNo PurchaseReturnNo
					,isnull(Round(main.PurchaseReturnQty,2),0) PurchaseReturnQty
					,isnull(Round(main.PurchaseReturnRate,2),0) PurchaseReturnRate
					,isnull(Round(main.PurchaseReturnAmount,2),0) PurchaseReturnAmount


                      ,REPLACE(CONVERT(CHAR(11), main.ReturnIssueDate, 106),' ','-') IssueReturnDate
					,main.ReturnIssueReturnNo IssueReturnNo
					,isnull(Round(main.IssueReturnQty,2),0) IssueReturnQty
					,isnull(Round(main.IssueReturnRate,2),0) IssueReturnRate
					,isnull(Round(main.IssueReturnAmount,2),0) IssueReturnAmount


					,REPLACE(CONVERT(CHAR(11), main.PhysicalIssueDate, 106),' ','-') AdjustmentDate
					,main.PhysicalIssueNo AdjustmentNo
					,isnull(Round(main.PhysicalStockAdjustmentqty,2),0) AdjustmentQty
					,isnull(Round(main.PhysicalStockAdjustmentRate,2),0) AdjustmentRate
					,isnull(Round(main.PhysicalStockAdjustmentAmount,2),0) AdjustmentAmount						




					,Round(((((IRD.TransactionQty-isnull(Round(main.PurchaseReturnQty,2),0))-isnull(Round(main.IssueQty,2),0))+isnull(Round(main.IssueReturnQty,2),0))-isnull(Round(main.PhysicalStockAdjustmentqty,2),0)),2) BalanceQty

					,CASE WHEN Round((IRD.TransactionQty- isnull(main.IssueQty,0)),2)>0 then Round(IRD.BooksCurrencyBaseRate,2) else 0 END BalanceRate
					,Round((((((IRD.TransactionQty-isnull(Round(main.PurchaseReturnQty,2),0))-isnull(Round(main.IssueQty,2),0))+isnull(Round(main.IssueReturnQty,2),0))-isnull(Round(main.PhysicalStockAdjustmentqty,2),0))* IRD.BooksCurrencyBaseRate),2) BalanceAmount

					,IRD.IsAsset						
					,CASE WHEN IRD.IsAsset=1 THEN 'Asset' ELSE 'Inventory' END IsAssetStatus
					--select x.InventoryReceiveDetailId,x.IssueDate,x.IssueNo,x.IssueType,x.PurchaseReturnNo,x.POReturnDate,x.ReturnIssueDate,x.ReturnIssueReturnNo,x.PhysicalIssueDate,x.PhysicalIssueNo,

					-- x.IssueAmount,x.IssueQty,x.Rate,x.PurchaseReturnQty,x.PurchaseReturnRate,x.PurchaseReturnAmount,x.IssueReturnQty,x.IssueReturnRate

					-- ,x.IssueReturnAmount,x.PhysicalStockAdjustmentqty,x.PhysicalStockAdjustmentRate,x.PhysicalStockAdjustmentAmount
 
					--from (
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
						left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id	
						left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
						left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
						left join(select IH.InventoryReceiveDetailId,II.IssueDate,II.Id IssueNo,II.IssueType,II.VoucherId IssueVoucherNo,NULL POReturnDate,NULL PurchaseReturnNo, NULL ReturnIssueDate  ,NULL ReturnIssueReturnNo, NULL PhysicalIssueDate,NULL PhysicalIssueNo

                                    ,(Sum(Isnull(Ih.Qty,0))) IssueQty 
                                    ,Sum(IH.Rate) Rate
                                    ,(Sum(Isnull(Ih.Qty,0))*Sum(IH.Rate)) AS IssueAmount
                                    ,0 PurchaseReturnQty,0 PurchaseReturnRate,0 PurchaseReturnAmount
                                    ,0 IssueReturnQty,0 IssueReturnRate,0 IssueReturnAmount
                                    , 0 PhysicalStockAdjustmentqty   ,0 PhysicalStockAdjustmentRate ,0 PhysicalStockAdjustmentAmount
                                     from  [TRN].[InventoryReceiveDetail] AS IRD 
                                     LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryReceiveDetailId=IRD.ID 
                                     LEFT JOIN  TRN.InventoryIssueDetail IID on IID.ID=IH.InventoryIssueDetailId
                                     LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id 
                                     Where  Ih.Qty>0 --ANd IH.InventoryReceiveDetailId='2020258-1'
                                     group by IH.InventoryReceiveDetailId,II.IssueDate,II.Id,II.IssueType,II.VoucherId

                                     Union all

                        select  IH.InventoryReceiveDetailId,NULL IssueDate,NULL IssueNo,NULL IssueType,NULL IssueVoucherNo,II.POReturnDate  ,II.Id PurchaseReturnNo, NULL ReturnIssueDate  ,NULL ReturnIssueReturnNo, NULL PhysicalIssueDate,NULL PhysicalIssueNo

                                    , 0 IssueQty,0 Rate,0 IssueAmount
                                     ,(Sum(Isnull(IH.TransactionQty,0))) PurchaseReturnQty 
                                     ,Sum(IH.MaterialTranRate) PurchaseReturnRate
                                     ,(Sum(Isnull(IH.TransactionQty,0))*Sum(IH.MaterialTranRate)) AS PurchaseReturnAmount
                                       ,0 IssueReturnQty,0 IssueReturnRate,0 IssueReturnAmount
                                   , 0 PhysicalStockAdjustmentqty   ,0 PhysicalStockAdjustmentRate ,0 PhysicalStockAdjustmentAmount
                                     from  [TRN].[InventoryReceiveDetail] AS IRD 
                                     LEFT JOIN TRN.PurchaseReturnDetail IH On IH.InventoryReceiveDetailId=IRD.ID 
                                     LEFT JOIN TRN.PurchaseReturn II ON IH.PurchaseReturnId=II.Id 
                                     Where Ih.TransactionQty>0 --ANd IH.InventoryReceiveDetailId='2020258-1'
                                     group by IH.InventoryReceiveDetailId,II.POReturnDate ,II.Id


                             Union all
 
 

                        select  IH.InventoryReceiveDetailId,NULL IssueDate,NULL IssueNo,NULL IssueType,NULL IssueVoucherNo,NULL POReturnDate  ,NULL PurchaseReturnNo,    II.IssueDate ReturnIssueDate ,II.Id ReturnIssueReturnNo, NULL PhysicalIssueDate,NULL PhysicalIssueNo

                                     , 0 IssueQty,0 Rate,0 IssueAmount,0 PurchaseReturnQty
                                     ,0 PurchaseReturnRate,0PurchaseReturnAmount
                                    ,(Sum(Isnull(IH.Qty,0))) IssueReturnQty 
                                    ,Sum(IH.Rate) IssueReturnRate
                                    ,(Sum(Isnull(IH.Qty,0))*Sum(IH.Rate)) AS IssueReturnAmount 
                                    , 0 PhysicalStockAdjustmentqty   ,0 PhysicalStockAdjustmentRate ,0 PhysicalStockAdjustmentAmount
                                     from  [TRN].[InventoryReceiveDetail] AS IRD 
                                     LEFT JOIN TRN.InventoryIssueReturnHistory IH On IH.InventoryReceiveDetailId=IRD.ID 
                                     --LEFT JOIN  TRN.InventoryIssueDetail IID on IID.ID=IH.InventoryIssueDetailId
                                     LEFT JOIN TRN.InventoryIssueReturn II ON IH.InventoryIssueReturnId=II.Id 
                                     Where  Ih.Qty>0 --ANd IH.InventoryReceiveDetailId='2020258-1'
                                     group by IH.InventoryReceiveDetailId,II.IssueDate ,II.Id

                             Union all
                        select  IH.InventoryReceiveDetailId,NULL IssueDate,NULL IssueNo,NULL IssueType,NULL IssueVoucherNo,NULL POReturnDate  ,NULL PurchaseReturnNo,    NULL ReturnIssueDate ,NULL IssueReturnNo, II.IssueDate PhysicalIssueDate,II.Id PhysicalIssueNo
									, 0 IssueQty,0 Rate,0 IssueAmount,0 PurchaseReturnQty
                                     ,0 PurchaseReturnRate,0PurchaseReturnAmount
                                      ,0 IssueReturnQty,0 IssueReturnRate,0 IssueReturnAmount
                                    ,(Sum(Isnull(Ih.Qty,0))) PhysicalStockAdjustmentqty 
                                    ,Sum(IH.Rate) PhysicalStockAdjustmentRate
                                    ,(Sum(Isnull(Ih.Qty,0))*Sum(IH.Rate)) AS PhysicalStockAdjustmentAmount
                                     from  [TRN].[InventoryReceiveDetail] AS IRD 
                                     LEFT JOIN TRN.PhysicalStockAdjustmentHistory IH On IH.InventoryReceiveDetailId=IRD.ID 
                                     LEFT JOIN  TRN.PhysicalStockAdjustmentDetail IID on IID.ID=IH.PhysicalStockAdjustmentDetailId
                                     LEFT JOIN TRN.PhysicalStockAdjustmentMaster II ON IID.PhysicalStockAdjustmentMasterID=II.Id 
                                     Where Ih.Qty>0 --ANd IH.InventoryReceiveDetailId='2020258-1'
                                     group by IH.InventoryReceiveDetailId,II.IssueDate,II.Id,II.IssueType
                                     )main on main.InventoryReceiveDetailId=IRD.Id									 
						where IRD.IsAsset=0 AND Convert(date ,IR.GRNDate) between '" + fromDate + @"' AND '" + toDate + @"'
						Order By IR.GRNDate, IR.AddedDate,IRD.Id ,main.IssueDate DESC";
				}
				if (Asset != null && Inventory == null)
				{
					cmdText = @"select 					
					 dense_rank() over (partition by IR.GRNDate, IR.AddedDate,IRD.Id order by IR.GRNDate, IR.AddedDate,IRD.Id ,main.IssueDate DESC) AS Seq,	 IRD.Id
                     ,isnull(MM.UserName,'') MaterialMasterName	
					,MM.id MId
					,isnull( ART.StandardName,'') ArticleName		
					,ART.id ARTId
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 	
					,TUoM.UserName UOM,main.IssueType
					,REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') RcvDate
					,Round(IRD.TransactionQty,2) RcvQty
					,Round(IRD.BooksCurrencyBaseRate,2) RcvRate
					,Round(IRD.TotalMaterialBooksCurrencyAmount,2) RcvAmount	
					,IR.VoucherId VoucherNo

					,REPLACE(CONVERT(CHAR(11), main.IssueDate, 106),' ','-') IssueDate
					,main.IssueNo IssueNo
					,isnull(Round(main.IssueQty,2),0) IssueQty
					,isnull(Round(main.Rate,2),0) Rate
					,isnull(Round(main.IssueAmount,2),0) IssueAmount
					,main.IssueVoucherNo


                      ,REPLACE(CONVERT(CHAR(11), main.POReturnDate, 106),' ','-') PurchaseReturnDate
					,main.PurchaseReturnNo PurchaseReturnNo
					,isnull(Round(main.PurchaseReturnQty,2),0) PurchaseReturnQty
					,isnull(Round(main.PurchaseReturnRate,2),0) PurchaseReturnRate
					,isnull(Round(main.PurchaseReturnAmount,2),0) PurchaseReturnAmount


                      ,REPLACE(CONVERT(CHAR(11), main.ReturnIssueDate, 106),' ','-') IssueReturnDate
					,main.ReturnIssueReturnNo IssueReturnNo
					,isnull(Round(main.IssueReturnQty,2),0) IssueReturnQty
					,isnull(Round(main.IssueReturnRate,2),0) IssueReturnRate
					,isnull(Round(main.IssueReturnAmount,2),0) IssueReturnAmount


					,REPLACE(CONVERT(CHAR(11), main.PhysicalIssueDate, 106),' ','-') AdjustmentDate
					,main.PhysicalIssueNo AdjustmentNo
					,isnull(Round(main.PhysicalStockAdjustmentqty,2),0) AdjustmentQty
					,isnull(Round(main.PhysicalStockAdjustmentRate,2),0) AdjustmentRate
					,isnull(Round(main.PhysicalStockAdjustmentAmount,2),0) AdjustmentAmount						




					,Round(((((IRD.TransactionQty-isnull(Round(main.PurchaseReturnQty,2),0))-isnull(Round(main.IssueQty,2),0))+isnull(Round(main.IssueReturnQty,2),0))-isnull(Round(main.PhysicalStockAdjustmentqty,2),0)),2) BalanceQty

					,CASE WHEN Round((IRD.TransactionQty- isnull(main.IssueQty,0)),2)>0 then Round(IRD.BooksCurrencyBaseRate,2) else 0 END BalanceRate
					,Round((((((IRD.TransactionQty-isnull(Round(main.PurchaseReturnQty,2),0))-isnull(Round(main.IssueQty,2),0))+isnull(Round(main.IssueReturnQty,2),0))-isnull(Round(main.PhysicalStockAdjustmentqty,2),0))* IRD.BooksCurrencyBaseRate),2) BalanceAmount

					,IRD.IsAsset						
					,CASE WHEN IRD.IsAsset=1 THEN 'Asset' ELSE 'Inventory' END IsAssetStatus
					--select x.InventoryReceiveDetailId,x.IssueDate,x.IssueNo,x.IssueType,x.PurchaseReturnNo,x.POReturnDate,x.ReturnIssueDate,x.ReturnIssueReturnNo,x.PhysicalIssueDate,x.PhysicalIssueNo,

					-- x.IssueAmount,x.IssueQty,x.Rate,x.PurchaseReturnQty,x.PurchaseReturnRate,x.PurchaseReturnAmount,x.IssueReturnQty,x.IssueReturnRate

					-- ,x.IssueReturnAmount,x.PhysicalStockAdjustmentqty,x.PhysicalStockAdjustmentRate,x.PhysicalStockAdjustmentAmount
 
					--from (
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
						left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id	
						left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
						left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
						left join(select IH.InventoryReceiveDetailId,II.IssueDate,II.Id IssueNo,II.IssueType,II.VoucherId IssueVoucherNo,NULL POReturnDate,NULL PurchaseReturnNo, NULL ReturnIssueDate  ,NULL ReturnIssueReturnNo, NULL PhysicalIssueDate,NULL PhysicalIssueNo

                                    ,(Sum(Isnull(Ih.Qty,0))) IssueQty 
                                    ,Sum(IH.Rate) Rate
                                    ,(Sum(Isnull(Ih.Qty,0))*Sum(IH.Rate)) AS IssueAmount
                                    ,0 PurchaseReturnQty,0 PurchaseReturnRate,0 PurchaseReturnAmount
                                    ,0 IssueReturnQty,0 IssueReturnRate,0 IssueReturnAmount
                                    , 0 PhysicalStockAdjustmentqty   ,0 PhysicalStockAdjustmentRate ,0 PhysicalStockAdjustmentAmount
                                     from  [TRN].[InventoryReceiveDetail] AS IRD 
                                     LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryReceiveDetailId=IRD.ID 
                                     LEFT JOIN  TRN.InventoryIssueDetail IID on IID.ID=IH.InventoryIssueDetailId
                                     LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id 
                                     Where  Ih.Qty>0 --ANd IH.InventoryReceiveDetailId='2020258-1'
                                     group by IH.InventoryReceiveDetailId,II.IssueDate,II.Id,II.IssueType,II.VoucherId

                                     Union all

                        select  IH.InventoryReceiveDetailId,NULL IssueDate,NULL IssueNo,NULL IssueType,NULL IssueVoucherNo,II.POReturnDate  ,II.Id PurchaseReturnNo, NULL ReturnIssueDate  ,NULL ReturnIssueReturnNo, NULL PhysicalIssueDate,NULL PhysicalIssueNo

                                    , 0 IssueQty,0 Rate,0 IssueAmount
                                     ,(Sum(Isnull(IH.TransactionQty,0))) PurchaseReturnQty 
                                     ,Sum(IH.MaterialTranRate) PurchaseReturnRate
                                     ,(Sum(Isnull(IH.TransactionQty,0))*Sum(IH.MaterialTranRate)) AS PurchaseReturnAmount
                                       ,0 IssueReturnQty,0 IssueReturnRate,0 IssueReturnAmount
                                   , 0 PhysicalStockAdjustmentqty   ,0 PhysicalStockAdjustmentRate ,0 PhysicalStockAdjustmentAmount
                                     from  [TRN].[InventoryReceiveDetail] AS IRD 
                                     LEFT JOIN TRN.PurchaseReturnDetail IH On IH.InventoryReceiveDetailId=IRD.ID 
                                     LEFT JOIN TRN.PurchaseReturn II ON IH.PurchaseReturnId=II.Id 
                                     Where Ih.TransactionQty>0 --ANd IH.InventoryReceiveDetailId='2020258-1'
                                     group by IH.InventoryReceiveDetailId,II.POReturnDate ,II.Id


                             Union all
 
 

                        select  IH.InventoryReceiveDetailId,NULL IssueDate,NULL IssueNo,NULL IssueType,NULL IssueVoucherNo,NULL POReturnDate  ,NULL PurchaseReturnNo,    II.IssueDate ReturnIssueDate ,II.Id ReturnIssueReturnNo, NULL PhysicalIssueDate,NULL PhysicalIssueNo

                                     , 0 IssueQty,0 Rate,0 IssueAmount,0 PurchaseReturnQty
                                     ,0 PurchaseReturnRate,0PurchaseReturnAmount
                                    ,(Sum(Isnull(IH.Qty,0))) IssueReturnQty 
                                    ,Sum(IH.Rate) IssueReturnRate
                                    ,(Sum(Isnull(IH.Qty,0))*Sum(IH.Rate)) AS IssueReturnAmount 
                                    , 0 PhysicalStockAdjustmentqty   ,0 PhysicalStockAdjustmentRate ,0 PhysicalStockAdjustmentAmount
                                     from  [TRN].[InventoryReceiveDetail] AS IRD 
                                     LEFT JOIN TRN.InventoryIssueReturnHistory IH On IH.InventoryReceiveDetailId=IRD.ID 
                                     --LEFT JOIN  TRN.InventoryIssueDetail IID on IID.ID=IH.InventoryIssueDetailId
                                     LEFT JOIN TRN.InventoryIssueReturn II ON IH.InventoryIssueReturnId=II.Id 
                                     Where  Ih.Qty>0 --ANd IH.InventoryReceiveDetailId='2020258-1'
                                     group by IH.InventoryReceiveDetailId,II.IssueDate ,II.Id

                             Union all
                        select  IH.InventoryReceiveDetailId,NULL IssueDate,NULL IssueNo,NULL IssueType,NULL IssueVoucherNo,NULL POReturnDate  ,NULL PurchaseReturnNo,    NULL ReturnIssueDate ,NULL IssueReturnNo, II.IssueDate PhysicalIssueDate,II.Id PhysicalIssueNo
									, 0 IssueQty,0 Rate,0 IssueAmount,0 PurchaseReturnQty
                                     ,0 PurchaseReturnRate,0PurchaseReturnAmount
                                      ,0 IssueReturnQty,0 IssueReturnRate,0 IssueReturnAmount
                                    ,(Sum(Isnull(Ih.Qty,0))) PhysicalStockAdjustmentqty 
                                    ,Sum(IH.Rate) PhysicalStockAdjustmentRate
                                    ,(Sum(Isnull(Ih.Qty,0))*Sum(IH.Rate)) AS PhysicalStockAdjustmentAmount
                                     from  [TRN].[InventoryReceiveDetail] AS IRD 
                                     LEFT JOIN TRN.PhysicalStockAdjustmentHistory IH On IH.InventoryReceiveDetailId=IRD.ID 
                                     LEFT JOIN  TRN.PhysicalStockAdjustmentDetail IID on IID.ID=IH.PhysicalStockAdjustmentDetailId
                                     LEFT JOIN TRN.PhysicalStockAdjustmentMaster II ON IID.PhysicalStockAdjustmentMasterID=II.Id 
                                     Where Ih.Qty>0 --ANd IH.InventoryReceiveDetailId='2020258-1'
                                     group by IH.InventoryReceiveDetailId,II.IssueDate,II.Id,II.IssueType
                                     )main on main.InventoryReceiveDetailId=IRD.Id									 
						where IRD.IsAsset=1 AND Convert(date ,IR.GRNDate) between '" + fromDate + @"' AND '" + toDate + @"'
						Order By IR.GRNDate, IR.AddedDate,IRD.Id ,main.IssueDate DESC";
				}
				var inventoryMaterialList = _sqlRepository.GetDataTable(cmdText);
				var plantName = new DataView(_sqlRepository.GetDataTable(@"SELECT UserName from org.Plant WHERE Id='" + plantId + "'")).ToTable(true, "UserName").Rows[0]["UserName"].ToString();
				if (inventoryMaterialList.Rows.Count == 0)
					throw new Exception("No Data Found !!!");

				var _rowd = 4;

				if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
				{


					sheet1[_rowd, 4].Text = fromDate + "To" + toDate;
					sheet1[_rowd, 4].CellStyle.Font.Size = 8;
					sheet1[_rowd, 4].CellStyle.Font.Bold = false;
					sheet1.Range[_rowd, 3, _rowd, 4].Merge();
					//sheet1.Range[_rowd, 3, _rowd , 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;

				}

				var _rows = 5;
				sheet1[_rows, 5].Text = "Report Ref No: ";
				sheet1[_rowd, 5].CellStyle.Font.Size = 8;
				sheet1[_rowd, 5].CellStyle.Font.Bold = false;
				sheet1.Range[_rows, 3, _rows, 6].Merge();
				var _row = 7;

				//sheet1[_row, 1].Text = "Material";
				//sheet1[_row, 1].CellStyle.Font.Size = 10;
				//sheet1[_row, 1].CellStyle.Font.Bold = true;
				//sheet1[_row, 2].Text = inventoryMaterialList.Rows[0]["MaterialMasterName"].ToString();
				//sheet1.Range[_row, 2, _row, 5].Merge();
				//sheet1.Range[_row, 1, _row, 5].BorderAround(ExcelLineStyle.Thin);
				//sheet1.Range[_row, 1, _row, 2].BorderInside(ExcelLineStyle.Thin);


				////if (inventoryMaterialList.Rows[0]["IsAsset"].ToString() == "False")
				////{
				////    sheet1[_row - 1, 16].Text = "Of Inventory";
				////    sheet1.UsedRange.CellStyle.Font.Size = 15;
				////    sheet1.UsedRange.CellStyle.Font.Bold = true;
				////    sheet1.UsedRange.WrapText = true;
				////    sheet1.Range[_row, 1].BorderAround(ExcelLineStyle.Thick);


				////}
				////else
				////{
				////    sheet1[_row - 1, 16].Text = "Of Fixed Asset";
				////    sheet1.UsedRange.CellStyle.Font.Size = 15;
				////    sheet1.UsedRange.CellStyle.Font.Bold = true;
				////    sheet1.UsedRange.WrapText = true;
				////    sheet1.Range[_row, 1].BorderAround(ExcelLineStyle.Thick);


				////}
				//_row++;

				//sheet1[_row, 1].Text = "Article";
				//sheet1[_row, 1].CellStyle.Font.Size = 10;
				//sheet1[_row, 1].CellStyle.Font.Bold = true;
				//sheet1[_row, 2].Text = inventoryMaterialList.Rows[0]["ArticleName"].ToString();
				//sheet1.Range[_row, 2, _row, 5].Merge();
				//sheet1.Range[_row, 1, _row, 5].BorderAround(ExcelLineStyle.Thin);
				//sheet1.Range[_row, 1, _row, 2].BorderInside(ExcelLineStyle.Thin);

				//_row++;
				//sheet1[_row, 1].Text = "UOM";
				//sheet1[_row, 1].CellStyle.Font.Size = 10;
				//sheet1[_row, 1].CellStyle.Font.Bold = true;
				////sheet1.UsedRange.WrapText = true;
				//sheet1[_row, 2].Text = inventoryMaterialList.Rows[0]["UOM"].ToString();
				//sheet1.Range[_row, 2, _row, 5].Merge();
				//sheet1.Range[_row, 1, _row, 5].BorderAround(ExcelLineStyle.Thin);
				//sheet1.Range[_row, 1, _row, 2].BorderInside(ExcelLineStyle.Thin);

				_row++;

				sheet1[_row, 1].Text = "RECEIPTS";
				sheet1[_row, 1].CellStyle.Font.Size = 10;
				sheet1[_row, 1].CellStyle.Font.Bold = true;
				//sheet1.UsedRange.WrapText = true;
				sheet1[_row, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				//sheet1[_row, 1].CellStyle.Interior.Color = System.Drawing.Color.GreenYellow;
				sheet1.Range[_row, 1, _row, 10].BorderAround(ExcelLineStyle.Thick);
				sheet1.Range[_row, 1, _row, 10].BorderInside(ExcelLineStyle.Hair);
				sheet1.Range[_row, 1, _row, 10].CellStyle.FillBackground = ExcelKnownColors.Tan;
				sheet1.Range[_row, 1, _row, 10].Merge();



				sheet1[_row, 11].Text = "PURCHASE RETURN";
				sheet1[_row, 11].CellStyle.Font.Size = 10;
				sheet1[_row, 11].CellStyle.Font.Bold = true;
				//sheet1.UsedRange.WrapText = true;
				sheet1[_row, 11].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				//sheet1[_row, 1].CellStyle.Interior.Color = System.Drawing.Color.GreenYellow;
				sheet1.Range[_row, 11, _row, 15].BorderAround(ExcelLineStyle.Thick);
				sheet1.Range[_row, 11, _row, 15].BorderInside(ExcelLineStyle.Hair);
				sheet1.Range[_row, 11, _row, 15].CellStyle.FillBackground = ExcelKnownColors.Tan;
				sheet1.Range[_row, 11, _row, 15].Merge();


				sheet1[_row, 16].Text = "ISSUE";
				sheet1[_row, 16].CellStyle.Font.Size = 10;
				sheet1[_row, 16].CellStyle.Font.Bold = true;
				//sheet1.UsedRange.WrapText = true;
				sheet1[_row, 16].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				//sheet1[_row, 6].CellStyle.Interior.Color = System.Drawing.Color.Gray;
				sheet1.Range[_row, 16, _row, 22].BorderAround(ExcelLineStyle.Thick);
				sheet1.Range[_row, 16, _row, 22].BorderInside(ExcelLineStyle.Hair);
				sheet1.Range[_row, 16, _row, 22].CellStyle.FillBackground = ExcelKnownColors.Tan;
				sheet1.Range[_row, 16, _row, 22].Merge();


				sheet1[_row, 23].Text = "ISSUE RETURN";
				sheet1[_row, 23].CellStyle.Font.Size = 10;
				sheet1[_row, 23].CellStyle.Font.Bold = true;
				//sheet1.UsedRange.WrapText = true;
				sheet1[_row, 23].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				//sheet1[_row, 6].CellStyle.Interior.Color = System.Drawing.Color.Gray;
				sheet1.Range[_row, 23, _row, 27].BorderAround(ExcelLineStyle.Thick);
				sheet1.Range[_row, 23, _row, 27].BorderInside(ExcelLineStyle.Hair);
				sheet1.Range[_row, 23, _row, 27].CellStyle.FillBackground = ExcelKnownColors.Tan;
				sheet1.Range[_row, 23, _row, 27].Merge();


				sheet1[_row, 28].Text = "ADJUSTMENT";
				sheet1[_row, 28].CellStyle.Font.Size = 10;
				sheet1[_row, 28].CellStyle.Font.Bold = true;
				//sheet1.UsedRange.WrapText = true;
				sheet1[_row, 28].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				//sheet1[_row, 6].CellStyle.Interior.Color = System.Drawing.Color.Gray;
				sheet1.Range[_row, 28, _row, 32].BorderAround(ExcelLineStyle.Thick);
				sheet1.Range[_row, 28, _row, 32].BorderInside(ExcelLineStyle.Hair);
				sheet1.Range[_row, 28, _row, 32].CellStyle.FillBackground = ExcelKnownColors.Tan;
				sheet1.Range[_row, 28, _row, 32].Merge();


				sheet1[_row, 33].Text = "STOCK BALANCE";
				sheet1[_row, 33].CellStyle.Font.Size = 10;
				sheet1[_row, 33].CellStyle.Font.Bold = true;
				//sheet1.UsedRange.WrapText = true;
				sheet1[_row, 33].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				//sheet1[_row, 11].CellStyle.Interior.Color = System.Drawing.Color.HotPink;
				sheet1.Range[_row, 33, _row, 35].BorderAround(ExcelLineStyle.Thick);
				sheet1.Range[_row, 33, _row, 35].BorderInside(ExcelLineStyle.Hair);
				sheet1.Range[_row, 33, _row, 35].CellStyle.FillBackground = ExcelKnownColors.Tan;
				sheet1.Range[_row, 33, _row, 35].Merge();

				var _rowL = _row;
				var row = _row + 1;


				var sheet1headreColIndex = 1;
				//var sheet2headreColIndex = 1;
				_rowL += 1;
				var Row_Total_Start = _rowL;
				//Receive
				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Date");
				//sheet1headreColIndex++;
				var colRCVDate = sheet1headreColIndex;
				sheet1.Range[_rowL, sheet1headreColIndex].Text = "Date";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;

				var colRCVMaterial = sheet1headreColIndex;
				sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;
				var colRCVArticle = sheet1headreColIndex;
				sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;

				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Type");
				//sheet1headreColIndex++;
				var colIsAssetStatus = sheet1headreColIndex;



				sheet1.Range[_rowL, sheet1headreColIndex].Text = "Type";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;



				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "MRNo");
				var colRCVMRNo = sheet1headreColIndex;

				sheet1.Range[_rowL, sheet1headreColIndex].Text = "MRNo";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;

				var colRCVVoucherNo = sheet1headreColIndex;

				sheet1.Range[_rowL, sheet1headreColIndex].Text = "VoucherNo";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;

				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "UOM");
				//sheet1headreColIndex++;
				var colUOM = sheet1headreColIndex;


				sheet1.Range[_rowL, sheet1headreColIndex].Text = "UOM";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;

				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Quantity");
				var colRCVQuantity = sheet1headreColIndex;
				//sheet1headreColIndex++;

				sheet1.Range[_rowL, sheet1headreColIndex].Text = "Quantity";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;

				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Rate");
				var colRCVRate = sheet1headreColIndex;
				//sheet1headreColIndex++;

				sheet1.Range[_rowL, sheet1headreColIndex].Text = "Rate";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;

				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
				var colRCVAmount = sheet1headreColIndex;
				//sheet1headreColIndex++;

				sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;


				// Purchase Return
				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Date");
				var colPOReturnDate = sheet1headreColIndex;
				//sheet1headreColIndex++;

				sheet1.Range[_rowL, sheet1headreColIndex].Text = "Date";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;

				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "ReturnNo");
				var colPurchaseReturnNo = sheet1headreColIndex;
				//sheet1headreColIndex++;

				sheet1.Range[_rowL, sheet1headreColIndex].Text = "ReturnNo";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;

				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
				var colPurchaseReturnQty = sheet1headreColIndex;
				//sheet1headreColIndex++;

				sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;

				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Rate");
				var colPurchaseReturnRate = sheet1headreColIndex;
				//sheet1headreColIndex++;

				sheet1.Range[_rowL, sheet1headreColIndex].Text = "Rate";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;

				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
				var colPurchaseReturnAmount = sheet1headreColIndex;
				//sheet1headreColIndex++;

				sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;




				//Issue

				var colIssueDate = sheet1headreColIndex;
				sheet1.Range[_rowL, sheet1headreColIndex].Text = "Date";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;

				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SlipNo");
				var colIssueNo = sheet1headreColIndex;
				//sheet1headreColIndex++;

				sheet1.Range[_rowL, sheet1headreColIndex].Text = "SlipNo";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;
				var colIssueIssueVoucherNo = sheet1headreColIndex;
				sheet1.Range[_rowL, sheet1headreColIndex].Text = "VoucherNo";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;

				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Type");
				var colType = sheet1headreColIndex;
				//sheet1headreColIndex++;

				sheet1.Range[_rowL, sheet1headreColIndex].Text = "Type";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;


				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Quantity");
				var colIssueQty = sheet1headreColIndex;
				//sheet1headreColIndex++;

				sheet1.Range[_rowL, sheet1headreColIndex].Text = "Quantity";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;

				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Rate");
				var colIssueRate = sheet1headreColIndex;
				//sheet1headreColIndex++;

				sheet1.Range[_rowL, sheet1headreColIndex].Text = "Rate";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;

				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
				var colIssueAmount = sheet1headreColIndex;
				//sheet1headreColIndex++;

				sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;

				//Issue Return
				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Date");
				var colIssuereturnDate = sheet1headreColIndex;
				//sheet1headreColIndex++;

				sheet1.Range[_rowL, sheet1headreColIndex].Text = "Date";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;

				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "ReturnNo");
				var colIssuereturnNo = sheet1headreColIndex;
				//sheet1headreColIndex++;

				sheet1.Range[_rowL, sheet1headreColIndex].Text = "ReturnNo";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;


				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Quantity");
				var colIssuereturnQty = sheet1headreColIndex;
				//sheet1headreColIndex++;

				sheet1.Range[_rowL, sheet1headreColIndex].Text = "Quantity";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;

				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Rate");
				var colIssuereturnRate = sheet1headreColIndex;
				//sheet1headreColIndex++;

				sheet1.Range[_rowL, sheet1headreColIndex].Text = "Rate";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;

				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
				var colIssuereturnAmount = sheet1headreColIndex;
				//sheet1headreColIndex++;

				sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;

				//Adjustment 

				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Date");
				var colAdjustmentDate = sheet1headreColIndex;
				//sheet1headreColIndex++;

				sheet1.Range[_rowL, sheet1headreColIndex].Text = "Date";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;


				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "AdjustmentsNo");
				var colAdjustmentNo = sheet1headreColIndex;
				//sheet1headreColIndex++;

				sheet1.Range[_rowL, sheet1headreColIndex].Text = "AdjustmentsNo";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;

				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Quantity");
				var colAdjustmentQty = sheet1headreColIndex;
				//sheet1headreColIndex++;

				sheet1.Range[_rowL, sheet1headreColIndex].Text = "Quantity";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;


				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Rate");
				var colAdjustmentRate = sheet1headreColIndex;
				//sheet1headreColIndex++;

				sheet1.Range[_rowL, sheet1headreColIndex].Text = "Rate";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;

				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
				var colAdjustmentAmount = sheet1headreColIndex;
				//sheet1headreColIndex++;

				sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;

				//Balance

				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Quantity");
				var colBalanceQty = sheet1headreColIndex;
				//sheet1headreColIndex++;

				sheet1.Range[_rowL, sheet1headreColIndex].Text = "Quantity";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;


				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Rate");
				//sheet1headreColIndex++;
				var colBalanceRate = sheet1headreColIndex;


				sheet1.Range[_rowL, sheet1headreColIndex].Text = "Rate";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				sheet1headreColIndex++;


				//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
				var colBalanceAmount = sheet1headreColIndex;

				sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
				sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
				sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
				sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
				//sheet1headreColIndex++;

				sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
				sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
				sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;
				var balanceQty = 0.00;
				List<string> list = new List<string>();
				for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
				{
					_rowL++;
					var rcvid = inventoryMaterialList.Rows[n]["Id"].ToString();
					if (clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Seq"].ToString())== 1)
					{
						
						report.SetText(ref sheet1, _rowL, colRCVDate, inventoryMaterialList.Rows[n]["RcvDate"].ToString());
						report.SetText(ref sheet1, _rowL, colRCVMRNo, rcvid);
						report.SetText(ref sheet1, _rowL, colIsAssetStatus, inventoryMaterialList.Rows[n]["IsAssetStatus"].ToString());
						report.SetText(ref sheet1, _rowL, colUOM, inventoryMaterialList.Rows[n]["UOM"].ToString());
						report.SetText(ref sheet1, _rowL, colRCVQuantity, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["RcvQty"].ToString()));
						report.SetText(ref sheet1, _rowL, colRCVRate, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["RcvRate"].ToString()));
						report.SetText(ref sheet1, _rowL, colRCVAmount, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["RcvAmount"].ToString()));
						report.SetText(ref sheet1, _rowL, colRCVVoucherNo, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["VoucherNo"].ToString()));

						report.SetText(ref sheet1, _rowL, colRCVMaterial, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
						report.SetText(ref sheet1, _rowL, colRCVArticle, inventoryMaterialList.Rows[n]["ArticleName"].ToString());







						//start
						var RcvQty = clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["RcvQty"].ToString());
						var IssueQty = clsStaticInfo.dbl(inventoryMaterialList.Compute("SUM(IssueQty)", "Id = '" + rcvid + "'").ToString());
						var AdjustmentQty = clsStaticInfo.dbl(inventoryMaterialList.Compute("SUM(AdjustmentQty)", "Id = '" + rcvid + "'").ToString());
						var PurchaseReturnQty = clsStaticInfo.dbl(inventoryMaterialList.Compute("SUM(PurchaseReturnQty)", "Id = '" + rcvid + "'").ToString());
						var IssueReturnQty = clsStaticInfo.dbl(inventoryMaterialList.Compute("SUM(IssueReturnQty)", "Id = '" + rcvid + "'").ToString());

						//end

						//balanceQty = clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["RcvQty"].ToString()) - clsStaticInfo.dbl(inventoryMaterialList.Compute("SUM(IssueQty)", "Id = '" + rcvid + "'").ToString());
						balanceQty = RcvQty - IssueQty - AdjustmentQty - PurchaseReturnQty + IssueReturnQty;
						report.SetText(ref sheet1, _rowL, colBalanceQty, balanceQty);
						report.SetText(ref sheet1, _rowL, colBalanceQty, balanceQty);
						if (balanceQty == 0)
						{
							var colBalanceRate1 = 0;
							report.SetText(ref sheet1, _rowL, colBalanceQty, colBalanceRate1);

						}
						else
						{
							report.SetText(ref sheet1, _rowL, colBalanceRate, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BalanceRate"].ToString()));
							report.SetText(ref sheet1, _rowL, colBalanceAmount, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BalanceRate"].ToString()) * balanceQty);
						}



					}

					//Purchase Return
					report.SetText(ref sheet1, _rowL, colPOReturnDate, inventoryMaterialList.Rows[n]["PurchaseReturnDate"].ToString());
					report.SetText(ref sheet1, _rowL, colPurchaseReturnNo, inventoryMaterialList.Rows[n]["PurchaseReturnNo"].ToString());
					report.SetText(ref sheet1, _rowL, colPurchaseReturnQty, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQty"].ToString()));
					report.SetText(ref sheet1, _rowL, colPurchaseReturnRate, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnRate"].ToString()));
					report.SetText(ref sheet1, _rowL, colPurchaseReturnAmount, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnAmount"].ToString()));




					//Issue
					report.SetText(ref sheet1, _rowL, colIssueDate, inventoryMaterialList.Rows[n]["IssueDate"].ToString());
					report.SetText(ref sheet1, _rowL, colIssueNo, inventoryMaterialList.Rows[n]["IssueNo"].ToString());
					report.SetText(ref sheet1, _rowL, colType, inventoryMaterialList.Rows[n]["IssueType"].ToString());
					report.SetText(ref sheet1, _rowL, colIssueQty, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueQty"].ToString()));
					report.SetText(ref sheet1, _rowL, colIssueRate, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Rate"].ToString()));
					report.SetText(ref sheet1, _rowL, colIssueAmount, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueAmount"].ToString()));
					report.SetText(ref sheet1, _rowL, colIssueIssueVoucherNo, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueVoucherNo"].ToString()));

					//Issue Return

					report.SetText(ref sheet1, _rowL, colIssuereturnDate, inventoryMaterialList.Rows[n]["IssueReturnDate"].ToString());
					report.SetText(ref sheet1, _rowL, colIssuereturnNo, inventoryMaterialList.Rows[n]["IssueReturnNo"].ToString());
					report.SetText(ref sheet1, _rowL, colIssuereturnQty, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQty"].ToString()));
					report.SetText(ref sheet1, _rowL, colIssuereturnRate, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnRate"].ToString()));
					report.SetText(ref sheet1, _rowL, colIssuereturnAmount, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnAmount"].ToString()));

					//Adjustment

					report.SetText(ref sheet1, _rowL, colAdjustmentDate, inventoryMaterialList.Rows[n]["AdjustmentDate"].ToString());
					report.SetText(ref sheet1, _rowL, colAdjustmentNo, inventoryMaterialList.Rows[n]["AdjustmentNo"].ToString());
					report.SetText(ref sheet1, _rowL, colAdjustmentQty, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQty"].ToString()));
					report.SetText(ref sheet1, _rowL, colAdjustmentRate, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentRate"].ToString()));
					report.SetText(ref sheet1, _rowL, colAdjustmentAmount, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentAmount"].ToString()));

				}

				//_rowL++;

				sheet1.Range[(Row_Total_Start), 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 8;
				sheet1.Range[Row_Total_Start, colRCVDate, _rowL, colRCVAmount].BorderAround(ExcelLineStyle.Thin);
				sheet1.Range[Row_Total_Start, colRCVDate, _rowL, colRCVAmount].BorderInside(ExcelLineStyle.Hair);

				sheet1.Range[Row_Total_Start, colPOReturnDate, _rowL, colPurchaseReturnAmount].BorderAround(ExcelLineStyle.Thin);
				sheet1.Range[Row_Total_Start, colPOReturnDate, _rowL, colPurchaseReturnAmount].BorderInside(ExcelLineStyle.Hair);

				sheet1.Range[Row_Total_Start, colIssuereturnDate, _rowL, colIssuereturnAmount].BorderAround(ExcelLineStyle.Thin);
				sheet1.Range[Row_Total_Start, colIssuereturnDate, _rowL, colIssuereturnAmount].BorderInside(ExcelLineStyle.Hair);

				sheet1.Range[Row_Total_Start, colAdjustmentDate, _rowL, colAdjustmentAmount].BorderAround(ExcelLineStyle.Thin);
				sheet1.Range[Row_Total_Start, colAdjustmentDate, _rowL, colAdjustmentAmount].BorderInside(ExcelLineStyle.Hair);


				sheet1.Range[Row_Total_Start, colIssueDate, _rowL, colIssueAmount].BorderAround(ExcelLineStyle.Thin);
				sheet1.Range[Row_Total_Start, colIssueDate, _rowL, colIssueAmount].BorderInside(ExcelLineStyle.Hair);

				sheet1.Range[Row_Total_Start, colBalanceQty, _rowL, colBalanceAmount].BorderAround(ExcelLineStyle.Thin);
				sheet1.Range[Row_Total_Start, colBalanceQty, _rowL, colBalanceAmount].BorderInside(ExcelLineStyle.Hair);


				//#region sumCalc

				//_rowL++;
				//sheet1.Range[_rowL, 1, _rowL, 2].Merge();
				//report.SetText(ref sheet1, _rowL, 1, "Total :", true);
				////report.SetText(ref sheet2, _rowL, 1, "Total :", true);
				//sheet1.Range[_rowL, 1, _rowL, 2].CellStyle.Font.Underline = ExcelUnderline.Double;

				//sheet1.Range[_rowL, colRCVQuantity].Formula = "=SUM(" + report.GetColumnNameForXls(colRCVQuantity) + Row_Total_Start + ":" + report.GetColumnNameForXls(colRCVQuantity) + (_rowL - 1) + ")";
				//sheet1.Range[_rowL, colRCVQuantity].NumberFormat = report.NumberFormatDecimalTwo();
				//sheet1.Range[_rowL, colRCVQuantity].CellStyle.Font.Bold = true;
				//sheet1.Range[_rowL, 1, _rowL, 3].CellStyle.Font.Underline = ExcelUnderline.Double;

				////BorderAround(ExcelLineStyle.Thick);



				//sheet1.Range[_rowL, colRCVAmount].Formula = "=SUM(" + report.GetColumnNameForXls(colRCVAmount) + Row_Total_Start + ":" + report.GetColumnNameForXls(colRCVAmount) + (_rowL - 1) + ")";
				//sheet1.Range[_rowL, colRCVAmount].NumberFormat = report.NumberFormatDecimalTwo();
				//sheet1.Range[_rowL, colRCVAmount].CellStyle.Font.Bold = true;
				//sheet1.Range[_rowL, 1, _rowL, 5].CellStyle.Font.Underline = ExcelUnderline.Double;

				////PO Return

				//sheet1.Range[_rowL, colPurchaseReturnQty].Formula = "=SUM(" + report.GetColumnNameForXls(colPurchaseReturnQty) + Row_Total_Start + ":" + report.GetColumnNameForXls(colPurchaseReturnQty) + (_rowL - 1) + ")";
				//sheet1.Range[_rowL, colPurchaseReturnQty].NumberFormat = report.NumberFormatDecimalTwo();
				//sheet1.Range[_rowL, colPurchaseReturnQty].CellStyle.Font.Bold = true;
				//sheet1.Range[_rowL, 1, _rowL, 3].CellStyle.Font.Underline = ExcelUnderline.Double;

				//sheet1.Range[_rowL, colPurchaseReturnAmount].Formula = "=SUM(" + report.GetColumnNameForXls(colPurchaseReturnAmount) + Row_Total_Start + ":" + report.GetColumnNameForXls(colPurchaseReturnAmount) + (_rowL - 1) + ")";
				//sheet1.Range[_rowL, colPurchaseReturnAmount].NumberFormat = report.NumberFormatDecimalTwo();
				//sheet1.Range[_rowL, colPurchaseReturnAmount].CellStyle.Font.Bold = true;
				//sheet1.Range[_rowL, 1, _rowL, 10].CellStyle.Font.Underline = ExcelUnderline.Double;




				////Issue 


				//sheet1.Range[_rowL, colIssueQty].Formula = "=SUM(" + report.GetColumnNameForXls(colIssueQty) + Row_Total_Start + ":" + report.GetColumnNameForXls(colIssueQty) + (_rowL - 1) + ")";
				//sheet1.Range[_rowL, colIssueQty].NumberFormat = report.NumberFormatDecimalTwo();
				//sheet1.Range[_rowL, colIssueQty].CellStyle.Font.Bold = true;
				//sheet1.Range[_rowL, 1, _rowL, 3].CellStyle.Font.Underline = ExcelUnderline.Double;


				//sheet1.Range[_rowL, colIssueAmount].Formula = "=SUM(" + report.GetColumnNameForXls(colIssueAmount) + Row_Total_Start + ":" + report.GetColumnNameForXls(colIssueAmount) + (_rowL - 1) + ")";
				//sheet1.Range[_rowL, colIssueAmount].NumberFormat = report.NumberFormatDecimalTwo();
				//sheet1.Range[_rowL, colIssueAmount].CellStyle.Font.Bold = true;
				//sheet1.Range[_rowL, 1, _rowL, 10].CellStyle.Font.Underline = ExcelUnderline.Double;

				////STOCK BALANCE
				//sheet1.Range[_rowL, colBalanceQty].Formula = "=SUM(" + report.GetColumnNameForXls(colBalanceQty) + Row_Total_Start + ":" + report.GetColumnNameForXls(colBalanceQty) + (_rowL - 1) + ")";
				//sheet1.Range[_rowL, colBalanceQty].NumberFormat = report.NumberFormatDecimalTwo();
				//sheet1.Range[_rowL, colBalanceQty].CellStyle.Font.Bold = true;
				//sheet1.Range[_rowL, 1, _rowL, 11].CellStyle.Font.Underline = ExcelUnderline.Double;


				//sheet1.Range[_rowL, colBalanceAmount].Formula = "=SUM(" + report.GetColumnNameForXls(colBalanceAmount) + Row_Total_Start + ":" + report.GetColumnNameForXls(colBalanceAmount) + (_rowL - 1) + ")";
				//sheet1.Range[_rowL, colBalanceAmount].NumberFormat = report.NumberFormatDecimalTwo();
				//sheet1.Range[_rowL, colBalanceAmount].CellStyle.Font.Bold = true;
				//sheet1.Range[_rowL, 1, _rowL, 13].CellStyle.Font.Underline = ExcelUnderline.Double;



				////Issue Return 
				//sheet1.Range[_rowL, colIssuereturnQty].Formula = "=SUM(" + report.GetColumnNameForXls(colIssuereturnQty) + Row_Total_Start + ":" + report.GetColumnNameForXls(colIssuereturnQty) + (_rowL - 1) + ")";
				//sheet1.Range[_rowL, colIssuereturnQty].NumberFormat = report.NumberFormatDecimalTwo();
				//sheet1.Range[_rowL, colIssuereturnQty].CellStyle.Font.Bold = true;
				//sheet1.Range[_rowL, 1, _rowL, 8].CellStyle.Font.Underline = ExcelUnderline.Double;


				//sheet1.Range[_rowL, colIssuereturnAmount].Formula = "=SUM(" + report.GetColumnNameForXls(colIssuereturnAmount) + Row_Total_Start + ":" + report.GetColumnNameForXls(colIssuereturnAmount) + (_rowL - 1) + ")";
				//sheet1.Range[_rowL, colIssuereturnAmount].NumberFormat = report.NumberFormatDecimalTwo();
				//sheet1.Range[_rowL, colIssuereturnAmount].CellStyle.Font.Bold = true;
				//sheet1.Range[_rowL, 1, _rowL, 10].CellStyle.Font.Underline = ExcelUnderline.Double;


				////Adustment 
				//sheet1.Range[_rowL, colAdjustmentQty].Formula = "=SUM(" + report.GetColumnNameForXls(colAdjustmentQty) + Row_Total_Start + ":" + report.GetColumnNameForXls(colAdjustmentQty) + (_rowL - 1) + ")";
				//sheet1.Range[_rowL, colAdjustmentQty].NumberFormat = report.NumberFormatDecimalTwo();
				//sheet1.Range[_rowL, colAdjustmentQty].CellStyle.Font.Bold = true;
				//sheet1.Range[_rowL, 1, _rowL, 8].CellStyle.Font.Underline = ExcelUnderline.Double;



				//sheet1.Range[_rowL, colAdjustmentAmount].Formula = "=SUM(" + report.GetColumnNameForXls(colAdjustmentAmount) + Row_Total_Start + ":" + report.GetColumnNameForXls(colAdjustmentAmount) + (_rowL - 1) + ")";
				//sheet1.Range[_rowL, colAdjustmentAmount].NumberFormat = report.NumberFormatDecimalTwo();
				//sheet1.Range[_rowL, colAdjustmentAmount].CellStyle.Font.Bold = true;
				//sheet1.Range[_rowL, 1, _rowL, 10].CellStyle.Font.Underline = ExcelUnderline.Double;


				//#endregion sumCalc


				sheet1.Name = sheet1Name;
				sheet1.UsedRange.WrapText = true;
				//sheet1.UsedRange.CellStyle.Font.Size = 8;
				sheet1.IsGridLinesVisible = false;
				//report.PlantHeader(ref sheet1, sheet1headreColIndex, sheet1Name, plantId);
				report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);
			}
			catch (Exception ex)
			{
				throw ex;
			}

		}

		public IEnumerable<object> QueryGetListForMasterData(string plantId, string GRNbyPOCheckStatus)

		{
			try
			{
				//parameters.sort = "GRNDate";
				//parameters.order = "DESC";
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				var sql = "";
				if (GRNbyPOCheckStatus == "ForChecked")
				{
					sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         select * from (SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
                                    ,isnull(PO.POId,'') POId
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId
                                    ,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCANo,isnull(PO.LCDate,'') LCDate,isnull(PDA.AcceptanceNo,'') AcceptanceNo
                                    ,IR.CheckedByStatus,IR.AuthorizedByStatus
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName,IR.NoteForAccounts--,ISNULL(PDA.Id,'') PurchaseDocumentAcceptanceId
									,EI.EmployeeName CheckedBy,EI1.EmployeeName ApprovedBy
                                    , REPLACE(CONVERT(CHAR(11), PDA.AcceptanceDate, 106),' ','-') AcceptanceDate,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById,IR.GRNType
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
							,PO.UDNo,ISNULL(MLC.OpeningBank,'') OpeningBank,ISNULL(Pr.UserName ,'') CustomerName
							FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
						, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
						--LEFT JOIN (select Distinct PDAA.Id,AcceptanceDate,AcceptanceNo,ACMAP.GRNId from TRN.GRNAcceptanceMap ACMAP 
									--left Join trn.PurchaseDocAcceptance  PDAA ON PDAA.Id=ACMAP.PurchaseDocumentAcceptanceId
									--)PDA ON PDA.GRNId=IR.Id

						LEFT JOIN(
							SELECT distinct PDAMAP.GRNId
								,AcceptanceNo=STUFF((select distinct ','+xpo.AcceptanceNo from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	

								,AcceptanceDate=STUFF((select distinct ','+ REPLACE(CONVERT(CHAR(11), xpo.AcceptanceDate, 106),' ','-')  
								from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.GRNAcceptanceMap PDAMAP 
							  LEFT JOIN [TRN].PurchaseDocAcceptance IR ON IR.Id = PDAMAP.PurchaseDocumentAcceptanceId
							  
							  group by  PDAMAP.GRNId
							)PDA ON PDA.GRNId=IR.Id

                         LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,UDNo=STUFF((select distinct ','+C.UDNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
							LEFT JOIN [dbo].[Contract] CON on CON.Id= PO.ContractId
								 LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId 
								 LEFT JOIN dbo.MasterLC MLC ON MLC.Id=CON.MasterLCId
                        WHERE IR.CheckedByStatus='ForChecked' 
                        AND IR.PlantId='" + plantId + @"' 
                        AND ISNULL(IR.[Status],'')<>'Posting' 
                        AND IR.OpeningBalanceId IS NULL 
                        AND IR.EmployeeId IS NULL 
                        And IR.IsApproved = 0 --And IR.POId Is not NULL 
                        and (IR.GRNType='GRNBYPO' OR IR.GRNType='GRNBYREQPO')
                        Union All
                        SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
                                    ,isnull(PO.POId,'') POId
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId
                                    ,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCANo,isnull(PO.LCDate,'') LCDate,isnull(PDA.AcceptanceNo,'') AcceptanceNo
                                    ,IR.CheckedByStatus,IR.AuthorizedByStatus
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName,IR.NoteForAccounts--,ISNULL(PDA.Id,'') PurchaseDocumentAcceptanceId
									,EI.EmployeeName CheckedBy,EI1.EmployeeName ApprovedBy
                                    , REPLACE(CONVERT(CHAR(11), PDA.AcceptanceDate, 106),' ','-') AcceptanceDate,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById,IR.GRNType
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
						,PO.UDNo,ISNULL(MLC.OpeningBank,'') OpeningBank,ISNULL(Pr.UserName ,'') CustomerName
						FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
						, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                        --LEFT JOIN (select Distinct PDAA.Id,AcceptanceDate,AcceptanceNo,ACMAP.GRNId from TRN.GRNAcceptanceMap ACMAP 
									--left Join trn.PurchaseDocAcceptance  PDAA ON PDAA.Id=ACMAP.PurchaseDocumentAcceptanceId
									--)PDA ON PDA.GRNId=IR.Id

						LEFT JOIN(
							SELECT distinct PDAMAP.GRNId
								,AcceptanceNo=STUFF((select distinct ','+xpo.AcceptanceNo from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	

								,AcceptanceDate=STUFF((select distinct ','+ REPLACE(CONVERT(CHAR(11), xpo.AcceptanceDate, 106),' ','-')  
								from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.GRNAcceptanceMap PDAMAP 
							  LEFT JOIN [TRN].PurchaseDocAcceptance IR ON IR.Id = PDAMAP.PurchaseDocumentAcceptanceId
							  
							  group by  PDAMAP.GRNId
							)PDA ON PDA.GRNId=IR.Id

                         LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,UDNo=STUFF((select distinct ','+C.UDNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
							LEFT JOIN [dbo].[Contract] CON on CON.Id= PO.ContractId
								 LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId 
								 LEFT JOIN dbo.MasterLC MLC ON MLC.Id=CON.MasterLCId
                        WHERE IR.CheckedByStatus IS NULL and IR.AuthorizedByStatus='For Approval' 
                        AND IR.PlantId='" + plantId + @"' 
                        AND ISNULL(IR.[Status],'')<>'Posting' 
                        AND IR.OpeningBalanceId IS NULL 
                        AND IR.EmployeeId IS NULL 
                        And IR.IsApproved = 0 --And IR.POId Is not NULL 
                        and (IR.GRNType='GRNBYPO' OR IR.GRNType='GRNBYREQPO')
                         Union All
                        SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
                                    ,isnull(PO.POId,'') POId
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId
                                    ,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCANo,isnull(PO.LCDate,'') LCDate,isnull(PDA.AcceptanceNo,'') AcceptanceNo
                                    ,IR.CheckedByStatus,IR.AuthorizedByStatus
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName,IR.NoteForAccounts--,ISNULL(PDA.Id,'') PurchaseDocumentAcceptanceId
									,EI.EmployeeName CheckedBy,EI1.EmployeeName ApprovedBy
                                    , REPLACE(CONVERT(CHAR(11), PDA.AcceptanceDate, 106),' ','-') AcceptanceDate,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById,IR.GRNType
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
									,PO.UDNo,ISNULL(MLC.OpeningBank,'') OpeningBank,ISNULL(Pr.UserName ,'') CustomerName
						FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
						, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                        --LEFT JOIN (select Distinct PDAA.Id,AcceptanceDate,AcceptanceNo,ACMAP.GRNId from TRN.GRNAcceptanceMap ACMAP 
									--left Join trn.PurchaseDocAcceptance  PDAA ON PDAA.Id=ACMAP.PurchaseDocumentAcceptanceId
									--)PDA ON PDA.GRNId=IR.Id

						LEFT JOIN(
							SELECT distinct PDAMAP.GRNId
								,AcceptanceNo=STUFF((select distinct ','+xpo.AcceptanceNo from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	

								,AcceptanceDate=STUFF((select distinct ','+ REPLACE(CONVERT(CHAR(11), xpo.AcceptanceDate, 106),' ','-')  
								from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.GRNAcceptanceMap PDAMAP 
							  LEFT JOIN [TRN].PurchaseDocAcceptance IR ON IR.Id = PDAMAP.PurchaseDocumentAcceptanceId
							  
							  group by  PDAMAP.GRNId
							)PDA ON PDA.GRNId=IR.Id

                         LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,UDNo=STUFF((select distinct ','+C.UDNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
							LEFT JOIN [dbo].[Contract] CON on CON.Id= PO.ContractId
								 LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId 
								 LEFT JOIN dbo.MasterLC MLC ON MLC.Id=CON.MasterLCId
                        WHERE IR.CheckedByStatus IS NULL and IR.AuthorizedByStatus Is null
                        AND IR.PlantId='" + plantId + @"' 
                        AND ISNULL(IR.[Status],'')<>'Posting' 
                        AND IR.OpeningBalanceId IS NULL 
                        AND IR.EmployeeId IS NULL 
                        And IR.IsApproved = 1 --And IR.POId Is not NULL 
                        and (IR.GRNType='GRNBYPO' OR IR.GRNType='GRNBYREQPO')
                        )x
                        order by GRNDate DESC";

				}

				else if (GRNbyPOCheckStatus == "CheckedHoldReject")

				{

					sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                               , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
                                    ,isnull(PO.POId,'') POId
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId
                                    ,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCANo,isnull(PO.LCDate,'') LCDate
                                    ,IR.CheckedByStatus,IR.AuthorizedByStatus
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,0) GateName,IR.NoteForAccounts--,ISNULL(PDA.Id,'') PurchaseDocumentAcceptanceId
									,EI.EmployeeName CheckedBy,EI1.EmployeeName ApprovedBy, REPLACE(CONVERT(CHAR(11), PDA.AcceptanceDate, 106),' ','-') AcceptanceDate,IR.GRNType
                                    ,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
									,PO.UDNo,ISNULL(MLC.OpeningBank,'') OpeningBank,ISNULL(Pr.UserName ,'') CustomerName
						FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount
						, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                        	--LEFT JOIN (select Distinct PDAA.Id,AcceptanceDate,AcceptanceNo,ACMAP.GRNId from TRN.GRNAcceptanceMap ACMAP 
									--left Join trn.PurchaseDocAcceptance  PDAA ON PDAA.Id=ACMAP.PurchaseDocumentAcceptanceId
									--)PDA ON PDA.GRNId=IR.Id

						LEFT JOIN(
							SELECT distinct PDAMAP.GRNId
								,AcceptanceNo=STUFF((select distinct ','+xpo.AcceptanceNo from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	

								,AcceptanceDate=STUFF((select distinct ','+ REPLACE(CONVERT(CHAR(11), xpo.AcceptanceDate, 106),' ','-')  
								from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.GRNAcceptanceMap PDAMAP 
							  LEFT JOIN [TRN].PurchaseDocAcceptance IR ON IR.Id = PDAMAP.PurchaseDocumentAcceptanceId
							  
							  group by  PDAMAP.GRNId
							)PDA ON PDA.GRNId=IR.Id

                         LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,UDNo=STUFF((select distinct ','+C.UDNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
						LEFT JOIN [dbo].[Contract] CON on CON.Id= PO.ContractId
								 LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId 
								 LEFT JOIN dbo.MasterLC MLC ON MLC.Id=CON.MasterLCId
                        WHERE IR.CheckedByStatus='Hold' OR IR.CheckedByStatus='Reject' AND IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL And IR.IsApproved = 0 --And IR.POId Is not NULL 
                        and (IR.GRNType='GRNBYPO' OR IR.GRNType='GRNBYREQPO')
                order by IR.GRNDate ASC";
				}

				else if (GRNbyPOCheckStatus == "Checked")
				{
					sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
                                    ,isnull(PO.POId,'') POId
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId
                                    ,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCANo,isnull(PO.LCDate,'') LCDate
                                    ,IR.CheckedByStatus
                                    ,IR.AuthorizedByStatus
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,0) GateName,IR.NoteForAccounts
                                    --,ISNULL(PDA.Id,'') PurchaseDocumentAcceptanceId
									,EI.EmployeeName CheckedBy,EI1.EmployeeName ApprovedBy,REPLACE(CONVERT(CHAR(11), PDA.AcceptanceDate, 106),' ','-') AcceptanceDate,IR.GRNType
							
                        ,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
						,PO.UDNo,ISNULL(MLC.OpeningBank,'') OpeningBank,ISNULL(Pr.UserName ,'') CustomerName
						FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
						, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                       	--LEFT JOIN (select Distinct PDAA.Id,AcceptanceDate,AcceptanceNo,ACMAP.GRNId from TRN.GRNAcceptanceMap ACMAP 
									--left Join trn.PurchaseDocAcceptance  PDAA ON PDAA.Id=ACMAP.PurchaseDocumentAcceptanceId
									--)PDA ON PDA.GRNId=IR.Id

						LEFT JOIN(
							SELECT distinct PDAMAP.GRNId
								,AcceptanceNo=STUFF((select distinct ','+xpo.AcceptanceNo from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	

								,AcceptanceDate=STUFF((select distinct ','+ REPLACE(CONVERT(CHAR(11), xpo.AcceptanceDate, 106),' ','-')  
								from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.GRNAcceptanceMap PDAMAP 
							  LEFT JOIN [TRN].PurchaseDocAcceptance IR ON IR.Id = PDAMAP.PurchaseDocumentAcceptanceId
							  
							  group by  PDAMAP.GRNId
							)PDA ON PDA.GRNId=IR.Id

                         LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,UDNo=STUFF((select distinct ','+C.UDNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
							LEFT JOIN [dbo].[Contract] CON on CON.Id= PO.ContractId
								 LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId 
								 LEFT JOIN dbo.MasterLC MLC ON MLC.Id=CON.MasterLCId
                        WHERE  IR.CheckedByStatus='Checked'  AND IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL And IR.IsApproved = 0 --And IR.POId Is not NULL  
                         and (IR.GRNType='GRNBYPO' OR IR.GRNType='GRNBYREQPO')  order by IR.GRNDate ASC";


				}
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}


		public IEnumerable<object> GetListEmpPosted()
		{
			try
			{
				// parameters.sort = "Id";
				// parameters.order = "DESC";
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				var sql1 = @"--DECLARE @plantId VARCHAR(10)='" + identity.PlantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus,EI2.EmployeeName As EmployeeName,EI2.SystemId EmployeeId,IR.IsNonVendor,IR.Reason
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1
                                    ,MS.UserName as StorageLocation
									,V.VoucherNo
									,Posted=CASE WHEN IR.Status <>'' then 'Yes' else 'No' END						
									,I.PostingDate
									,I.AddedBy PostedBy,IR.AddedBy
                                     ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId
                                    ,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCRef
									--,IR.ContractId,C.ContractNo,PL.Id PurchaseLCId,PL.LCRef
                        FROM [TRN].[InventoryReceive] AS IR Left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
						,SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN dbo.EmployeeInformation EI2 ON EI2.SystemId=IR.EmployeeId
						LEFT JOIN hkp.MaterialStorage AS MS ON MS.Id=IR.MaterialStorageId
						left JOIN trn.EmployeePayable as I ON I.InventoryReceiveId=IR.Id					
						left join trn.Voucher V on V.Id=I.VoucherId
                    	left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
						--Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
							--Left JOIN dbo.PurchaseLC PL ON PL.ContractId=C.Id
							LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
                        WHERE IR.Status='Posting' And IR. AuthorizedByStatus='Approved' And IR.CheckedByStatus='Checked' And IR.PlantId='" + identity.PlantId + @"' AND ISNULL(IR.[Status],'')='Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NOT NULL And IR.IsApproved = 1 ANd IR.POId is null AND IR.GRNType='EMPGRN'";
				return _sqlRepository.GetDataCollection(sql1);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		public IEnumerable<object> GetListEmpApprovedNotPost()
		{
			try
			{
				// parameters.sort = "Id";
				// parameters.order = "DESC";
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				var sql1 = @"--DECLARE @plantId VARCHAR(10)='" + identity.PlantId + @"';
                         Select * from (SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus,EI2.EmployeeName As EmployeeName,EI2.SystemId EmployeeId,IR.IsNonVendor,IR.Reason
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1,IR.AddedBy
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId
                                    ,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCRef
									--,IR.ContractId,C.ContractNo,PL.Id PurchaseLCId,PL.LCRef
                        FROM [TRN].[InventoryReceive] AS IR Left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                         LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                         LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
						,SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN dbo.EmployeeInformation EI2 ON EI2.SystemId=IR.EmployeeId
  	                    left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
						--Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
							--Left JOIN dbo.PurchaseLC PL ON PL.ContractId=C.Id
							LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
                        WHERE IR. AuthorizedByStatus='Approved' 
                        And IR.CheckedByStatus='Checked' 
                        And IR.PlantId='" + identity.PlantId + @"' 
                        AND ISNULL(IR.[Status],'')<>'Posting' 
                        AND IR.OpeningBalanceId IS NULL 
                        AND IR.EmployeeId IS NOT NULL 
                        And IR.IsApproved = 1 
                        ANd IR.POId is null 
                        AND IR.GRNType='EMPGRN' 
                        UNION ALL
                        SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus,EI2.EmployeeName As EmployeeName,EI2.SystemId EmployeeId,IR.IsNonVendor,IR.Reason
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1,IR.AddedBy
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId
                                    ,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCRef
									--,IR.ContractId,C.ContractNo,PL.Id PurchaseLCId,PL.LCRef
                        FROM [TRN].[InventoryReceive] AS IR Left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                         LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                         LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
						,SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN dbo.EmployeeInformation EI2 ON EI2.SystemId=IR.EmployeeId
  	                    left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
						--Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
							--Left JOIN dbo.PurchaseLC PL ON PL.ContractId=C.Id
							LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
                        WHERE IR. AuthorizedByStatus='Approved' 
                        And IR.CheckedByStatus IS NULL 
                        And IR.PlantId='" + identity.PlantId + @"' 
                        AND ISNULL(IR.[Status],'')<>'Posting' 
                        AND IR.OpeningBalanceId IS NULL 
                        AND IR.EmployeeId IS NOT NULL 
                        And IR.IsApproved = 1 
                        ANd IR.POId is null 
                        AND IR.GRNType='EMPGRN'
                        )x
                        Order by GRNDate ASC";
				return _sqlRepository.GetDataCollection(sql1); ;
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}


		public IEnumerable<object> GetJWGRNDataChecking(string plantId, string GRNbyPOCheckStatus)

		{
			try
			{
				//parameters.sort = "GRNDate";
				//parameters.order = "DESC";
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				var sql = "";
				if (GRNbyPOCheckStatus == "ForChecked")
				{
					sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         select * from (SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
                                    ,isnull(PO.POId,'') POId
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId
                                    ,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCANo,isnull(PO.LCDate,'') LCDate,isnull(PDA.AcceptanceNo,'') AcceptanceNo
                                    ,IR.CheckedByStatus,IR.AuthorizedByStatus
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName,IR.NoteForAccounts--,ISNULL(PDA.Id,'') PurchaseDocumentAcceptanceId
									,EI.EmployeeName CheckedBy,EI1.EmployeeName ApprovedBy
                                    , REPLACE(CONVERT(CHAR(11), PDA.AcceptanceDate, 106),' ','-') AcceptanceDate,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById,IR.GRNType
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
							,PO.UDNo,ISNULL(MLC.OpeningBank,'') OpeningBank,ISNULL(Pr.UserName ,'') CustomerName
							,EI2.EmployeeName ByWhomName
									,EI2.SystemId ByWhomEmployeeId,EI2.SystemId EmpCode
							FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
						LEFT JOIN dbo.EmployeeInformation EI2 ON EI2.SystemId=IR.ByWhomEmployeeId
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
						, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
						--LEFT JOIN (select Distinct PDAA.Id,AcceptanceDate,AcceptanceNo,ACMAP.GRNId from TRN.GRNAcceptanceMap ACMAP 
									--left Join trn.PurchaseDocAcceptance  PDAA ON PDAA.Id=ACMAP.PurchaseDocumentAcceptanceId
									--)PDA ON PDA.GRNId=IR.Id

						LEFT JOIN(
							SELECT distinct PDAMAP.GRNId
								,AcceptanceNo=STUFF((select distinct ','+xpo.AcceptanceNo from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	

								,AcceptanceDate=STUFF((select distinct ','+ REPLACE(CONVERT(CHAR(11), xpo.AcceptanceDate, 106),' ','-')  
								from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.GRNAcceptanceMap PDAMAP 
							  LEFT JOIN [TRN].PurchaseDocAcceptance IR ON IR.Id = PDAMAP.PurchaseDocumentAcceptanceId
							  
							  group by  PDAMAP.GRNId
							)PDA ON PDA.GRNId=IR.Id

                         LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,UDNo=STUFF((select distinct ','+C.UDNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
							LEFT JOIN [dbo].[Contract] CON on CON.Id= PO.ContractId
								 LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId 
								 left JOIN dbo.MasterLC MLC ON MLC.CustomerId=Pr.Id
                        WHERE IR.CheckedByStatus='ForChecked' 
                        AND IR.PlantId='" + plantId + @"' 
                        AND ISNULL(IR.[Status],'')<>'Posting' 
                        AND IR.OpeningBalanceId IS NULL 
                        AND IR.EmployeeId IS NULL 
                        And IR.IsApproved = 0 --And IR.POId Is not NULL 
                        and IR.GRNType='GRNBYJW'
                        Union All
                        SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
                                    ,isnull(PO.POId,'') POId
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId
                                    ,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCANo,isnull(PO.LCDate,'') LCDate,isnull(PDA.AcceptanceNo,'') AcceptanceNo
                                    ,IR.CheckedByStatus,IR.AuthorizedByStatus
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName,IR.NoteForAccounts--,ISNULL(PDA.Id,'') PurchaseDocumentAcceptanceId
									,EI.EmployeeName CheckedBy,EI1.EmployeeName ApprovedBy
                                    , REPLACE(CONVERT(CHAR(11), PDA.AcceptanceDate, 106),' ','-') AcceptanceDate,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById,IR.GRNType
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
						,PO.UDNo,ISNULL(MLC.OpeningBank,'') OpeningBank,ISNULL(Pr.UserName ,'') CustomerName
						,EI2.EmployeeName ByWhomName
									,EI2.SystemId ByWhomEmployeeId,EI2.SystemId EmpCode
						FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
						LEFT JOIN dbo.EmployeeInformation EI2 ON EI2.SystemId=IR.ByWhomEmployeeId
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
						, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                        --LEFT JOIN (select Distinct PDAA.Id,AcceptanceDate,AcceptanceNo,ACMAP.GRNId from TRN.GRNAcceptanceMap ACMAP 
									--left Join trn.PurchaseDocAcceptance  PDAA ON PDAA.Id=ACMAP.PurchaseDocumentAcceptanceId
									--)PDA ON PDA.GRNId=IR.Id

						LEFT JOIN(
							SELECT distinct PDAMAP.GRNId
								,AcceptanceNo=STUFF((select distinct ','+xpo.AcceptanceNo from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	

								,AcceptanceDate=STUFF((select distinct ','+ REPLACE(CONVERT(CHAR(11), xpo.AcceptanceDate, 106),' ','-')  
								from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.GRNAcceptanceMap PDAMAP 
							  LEFT JOIN [TRN].PurchaseDocAcceptance IR ON IR.Id = PDAMAP.PurchaseDocumentAcceptanceId
							  
							  group by  PDAMAP.GRNId
							)PDA ON PDA.GRNId=IR.Id

                         LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,UDNo=STUFF((select distinct ','+C.UDNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
							LEFT JOIN [dbo].[Contract] CON on CON.Id= PO.ContractId
								 LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId 
								 left JOIN dbo.MasterLC MLC ON MLC.CustomerId=Pr.Id
                        WHERE IR.CheckedByStatus IS NULL and IR.AuthorizedByStatus='For Approval' 
                        AND IR.PlantId='" + plantId + @"' 
                        AND ISNULL(IR.[Status],'')<>'Posting' 
                        AND IR.OpeningBalanceId IS NULL 
                        AND IR.EmployeeId IS NULL 
                        And IR.IsApproved = 0 --And IR.POId Is not NULL 
                        and IR.GRNType='GRNBYJW'
                         Union All
                        SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
                                    ,isnull(PO.POId,'') POId
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId
                                    ,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCANo,isnull(PO.LCDate,'') LCDate,isnull(PDA.AcceptanceNo,'') AcceptanceNo
                                    ,IR.CheckedByStatus,IR.AuthorizedByStatus
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName,IR.NoteForAccounts--,ISNULL(PDA.Id,'') PurchaseDocumentAcceptanceId
									,EI.EmployeeName CheckedBy,EI1.EmployeeName ApprovedBy
                                    , REPLACE(CONVERT(CHAR(11), PDA.AcceptanceDate, 106),' ','-') AcceptanceDate,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById,IR.GRNType
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
									,PO.UDNo,ISNULL(MLC.OpeningBank,'') OpeningBank,ISNULL(Pr.UserName ,'') CustomerName
									,EI2.EmployeeName ByWhomName
									,EI2.SystemId ByWhomEmployeeId,EI2.SystemId EmpCode
						FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
						LEFT JOIN dbo.EmployeeInformation EI2 ON EI2.SystemId=IR.ByWhomEmployeeId
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
						, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                        --LEFT JOIN (select Distinct PDAA.Id,AcceptanceDate,AcceptanceNo,ACMAP.GRNId from TRN.GRNAcceptanceMap ACMAP 
									--left Join trn.PurchaseDocAcceptance  PDAA ON PDAA.Id=ACMAP.PurchaseDocumentAcceptanceId
									--)PDA ON PDA.GRNId=IR.Id

						LEFT JOIN(
							SELECT distinct PDAMAP.GRNId
								,AcceptanceNo=STUFF((select distinct ','+xpo.AcceptanceNo from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	

								,AcceptanceDate=STUFF((select distinct ','+ REPLACE(CONVERT(CHAR(11), xpo.AcceptanceDate, 106),' ','-')  
								from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.GRNAcceptanceMap PDAMAP 
							  LEFT JOIN [TRN].PurchaseDocAcceptance IR ON IR.Id = PDAMAP.PurchaseDocumentAcceptanceId
							  
							  group by  PDAMAP.GRNId
							)PDA ON PDA.GRNId=IR.Id

                         LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,UDNo=STUFF((select distinct ','+C.UDNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
							LEFT JOIN [dbo].[Contract] CON on CON.Id= PO.ContractId
								 LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId 
								 left JOIN dbo.MasterLC MLC ON MLC.CustomerId=Pr.Id
                        WHERE IR.CheckedByStatus IS NULL and IR.AuthorizedByStatus Is null
                        AND IR.PlantId='" + plantId + @"' 
                        AND ISNULL(IR.[Status],'')<>'Posting' 
                        AND IR.OpeningBalanceId IS NULL 
                        AND IR.EmployeeId IS NULL 
                        And IR.IsApproved = 1 --And IR.POId Is not NULL 
                        and IR.GRNType='GRNBYJW'
                        )x
                        order by GRNDate DESC";

				}

				else if (GRNbyPOCheckStatus == "CheckedHoldReject")

				{

					sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                               , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
                                    ,isnull(PO.POId,'') POId
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId
                                    ,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCANo,isnull(PO.LCDate,'') LCDate
                                    ,IR.CheckedByStatus,IR.AuthorizedByStatus
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,0) GateName,IR.NoteForAccounts--,ISNULL(PDA.Id,'') PurchaseDocumentAcceptanceId
									,EI.EmployeeName CheckedBy,EI1.EmployeeName ApprovedBy, REPLACE(CONVERT(CHAR(11), PDA.AcceptanceDate, 106),' ','-') AcceptanceDate,IR.GRNType
                                    ,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
									,PO.UDNo,ISNULL(MLC.OpeningBank,'') OpeningBank,ISNULL(Pr.UserName ,'') CustomerName
						FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount
						, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                        	--LEFT JOIN (select Distinct PDAA.Id,AcceptanceDate,AcceptanceNo,ACMAP.GRNId from TRN.GRNAcceptanceMap ACMAP 
									--left Join trn.PurchaseDocAcceptance  PDAA ON PDAA.Id=ACMAP.PurchaseDocumentAcceptanceId
									--)PDA ON PDA.GRNId=IR.Id

						LEFT JOIN(
							SELECT distinct PDAMAP.GRNId
								,AcceptanceNo=STUFF((select distinct ','+xpo.AcceptanceNo from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	

								,AcceptanceDate=STUFF((select distinct ','+ REPLACE(CONVERT(CHAR(11), xpo.AcceptanceDate, 106),' ','-')  
								from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.GRNAcceptanceMap PDAMAP 
							  LEFT JOIN [TRN].PurchaseDocAcceptance IR ON IR.Id = PDAMAP.PurchaseDocumentAcceptanceId
							  
							  group by  PDAMAP.GRNId
							)PDA ON PDA.GRNId=IR.Id

                         LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,UDNo=STUFF((select distinct ','+C.UDNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
						LEFT JOIN [dbo].[Contract] CON on CON.Id= PO.ContractId
								 LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId 
								 left JOIN dbo.MasterLC MLC ON MLC.CustomerId=Pr.Id
                        WHERE IR.CheckedByStatus='Hold' OR IR.CheckedByStatus='Reject' AND IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL And IR.IsApproved = 0 --And IR.POId Is not NULL 
                        and IR.GRNType='GRNBYJW'
                order by IR.GRNDate ASC";
				}

				else if (GRNbyPOCheckStatus == "Checked")
				{
					sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
                                    ,isnull(PO.POId,'') POId
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId
                                    ,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCANo,isnull(PO.LCDate,'') LCDate
                                    ,IR.CheckedByStatus
                                    ,IR.AuthorizedByStatus
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,0) GateName,IR.NoteForAccounts
                                    --,ISNULL(PDA.Id,'') PurchaseDocumentAcceptanceId
									,EI.EmployeeName CheckedBy,EI1.EmployeeName ApprovedBy,REPLACE(CONVERT(CHAR(11), PDA.AcceptanceDate, 106),' ','-') AcceptanceDate,IR.GRNType
							
                        ,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
						,PO.UDNo,ISNULL(MLC.OpeningBank,'') OpeningBank,ISNULL(Pr.UserName ,'') CustomerName
						,EI2.EmployeeName ByWhomName
									,EI2.SystemId ByWhomEmployeeId
						FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
						, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                       	--LEFT JOIN (select Distinct PDAA.Id,AcceptanceDate,AcceptanceNo,ACMAP.GRNId from TRN.GRNAcceptanceMap ACMAP 
									--left Join trn.PurchaseDocAcceptance  PDAA ON PDAA.Id=ACMAP.PurchaseDocumentAcceptanceId
									--)PDA ON PDA.GRNId=IR.Id

						LEFT JOIN(
							SELECT distinct PDAMAP.GRNId
								,AcceptanceNo=STUFF((select distinct ','+xpo.AcceptanceNo from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	

								,AcceptanceDate=STUFF((select distinct ','+ REPLACE(CONVERT(CHAR(11), xpo.AcceptanceDate, 106),' ','-')  
								from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.GRNAcceptanceMap PDAMAP 
							  LEFT JOIN [TRN].PurchaseDocAcceptance IR ON IR.Id = PDAMAP.PurchaseDocumentAcceptanceId
							  
							  group by  PDAMAP.GRNId
							)PDA ON PDA.GRNId=IR.Id

                         LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,UDNo=STUFF((select distinct ','+C.UDNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
							LEFT JOIN [dbo].[Contract] CON on CON.Id= PO.ContractId
								 LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId 
								 left JOIN dbo.MasterLC MLC ON MLC.CustomerId=Pr.Id
                        WHERE  IR.CheckedByStatus='Checked'  AND IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL And IR.IsApproved = 0 --And IR.POId Is not NULL  
                         and IR.GRNType='GRNBYJW'  order by IR.GRNDate ASC";


				}
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public IEnumerable<object> JWGRNDetailsData(string inveReveiveId, string POID)

		{
			//var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{

				var sql = @"DECLARE @inventoryReceiveId VARCHAR(10) = '1985'
									,@totalReceiveAmount DECIMAL(18, 4) = 0
									,@totalServiceAmount DECIMAL(18, 4) = 0
									,@totalSvcTaxAmount DECIMAL(18, 4) = 0

								SET @totalReceiveAmount = (
										SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)), 1)
										FROM [TRN].[InventoryReceiveDetail]
										WHERE InventoryReceiveId = @inventoryReceiveId
										)
								SET @totalServiceAmount = (
										SELECT ISNULL(SUM(ISNULL(Amount, 0)), 0)
										FROM [TRN].[InventoryService]
										WHERE InventoryReceiveId = @inventoryReceiveId
										)
								SET @totalSvcTaxAmount = (
										SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)), 0)
										FROM [TRN].[InventoryReceiveTax]
										WHERE InventoryReceiveId = @inventoryReceiveId
											AND InventoryServiceId <> ''
										)

								SELECT IM.Id
									,IRD.Id AS InventoryReceiveDetailId
									,IRD.id AS RCBDetailsID
									,IRD.PODetailsId
									,IRD.POId
									,IRD.InventoryReceiveId
									,REPLACE(CONVERT(CHAR(11), PID.AddedDate, 106), ' ', '-') AS AddedDate
									,MGM.UserName AS MaterialGroupName
									,IM.MaterialMasterId
									,MM.UserName MaterialName
									,ART.StandardName Article
									,IM.FirstCharacteristicsId
									,FC.UserName AS SKU1 
									,IM.FirstCharacteristicsValueId
									,FCV.UserName AS FirstCharacteristicsValue
									,IM.SecondCharacteristicsId
									,SC.UserName AS SKU2
									,IM.SecondCharacteristicsValueId
									,SCV.UserName AS SecondCharacteristicsValue
									,IM.ThirdCharacteristicsId
									,TC.UserName AS SKU3
									,IM.ThirdCharacteristicsValueId
									,TCV.UserName AS ThirdCharacteristicsValue
									,IRD.TransactionUoMId
									,TUoM.UserName AS TransactionUoM
									,IRD.MaterialTranRate AS TransactionRate
									,CU.Code AS CurrencyName
									,IR.ToCurrencyRate
									,(IRD.TransactionQty * IRD.MaterialTranRate) AS TrnAmount
									,IRD.TotalMaterialBooksCurrencyAmount AS BaseAmount
									--,IRD.TotalTaxAmount AS BaseTaxAmount
	
									--,TaxAmount = (
									--	SELECT SUM(TaxAmount)
										--FROM [TRN].[InventoryReceiveTax]
										--WHERE InventoryReceiveDetailId = IRD.Id
										--)
									--,IRD.ChargesTranAmount AS ChargesAmount
									--,ServiceCharge = (@totalServiceAmount / ISNULL(NULLIF(@totalReceiveAmount, 0), 1)) * IRD.MaterialTranAmount
									--,ServiceTax = (@totalSvcTaxAmount / ISNULL(NULLIF(@totalReceiveAmount, 0), 1)) * IRD.MaterialTranAmount
									--,IRD.CountryId
									--,PID.TransactionQty AS POQty
									--,ISNULL(Pre.OtherReceived, 0) OtherReceived
									,IRD.TransactionQty
									,(PID.TransactionQty - IRD.TransactionQty - ISNULL(Pre.OtherReceived, 0)) AS Balance
									,IRD.TransactionUoMId
									,IRD.BaseUOMId
									,IRD.TotalMaterialTranAmount
									,IRD.TotalMaterialBooksCurrencyAmount
									--,IRD.ShortageQty
									--,IRD.RejectionQty
									--,IRD.ApprovedQty
								--,IRD.ShortageQty
									--,IRD.RejectionQty
									--,IRD.ApprovedQty
									--,IRD.TransactionQty AS PreviousQty
									--,IRD.ShortageRatePercent AS ShortageRate
									--,IRD.ShortageValue
									--,IRD.RejectRatePercent AS RejectionRate
									--,IRD.RejectValue AS RejectionValue
									--,IRD.RejectClamPercent RejectionClamRate
									,IR.CheckedBy
									,MRD.MaterialDetail
                                    ,C.Id,C.UserName CountryName,IRD.GrossAmount,IRD.DiscountAmount,MOI.MasterOrderId MasterOrderNo,IRD.MaterialFor
								    ,MaterialBy=CASE WHEN IRD.MaterialFor='JWOUTPUTMaterial' THEN 'OutPut' WHEN IRD.MaterialFor='JWBYPRODUCTMaterial' THEN 'By Product' END
								FROM TRN.InventoryMaterial AS IM
								LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
								LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
								LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
								LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
								LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
								LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
								LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
								LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
								LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
								LEFT JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId = IM.Id									
								LEFT JOIN [TRN].[PurchaseOrderDetail] AS PID ON PID.Id = IRD.PODetailsId
								LEFT JOIN (
									SELECT PODetailsId
										,Sum(TransactionQty) AS OtherReceived
									FROM trn.InventoryReceiveDetail	
									GROUP BY PODetailsId
									) AS Pre ON pre.PODetailsId = IRD.PODetailsId
								LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
								LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId = IR.Id
								LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id
								LEFT JOIN TRN.MaterialRequsitionDetails MRD ON MRD.ID = PID.RequisitionDetailId
                                Left Join SCS.Country C ON C.Id=IM.CountryId
								 LEFT JOIN [TRN].[MasterOrderItem] MOI ON IRD.MasterOrderItemId=MOI.Id
								WHERE IM.MaterialMasterId IS NOT NULL  AND (IRD.MaterialFor='JWOUTPUTMaterial' OR IRD.MaterialFor='JWBYPRODUCTMaterial')
	 

								UNION ALL

								SELECT IM.Id
									,IRD.Id AS InventoryReceiveDetailId
									,IRD.id AS RCBDetailsID
									,IRD.PODetailsId
									,IRD.POId
									,IRD.InventoryReceiveId
									,REPLACE(CONVERT(CHAR(11), PID.AddedDate, 106), ' ', '-') AS AddedDate
									,MGM.UserName AS MaterialGroupName
									,IM.MaterialMasterId
									,MM.UserName MaterialName
									,ART.StandardName Article
									,IM.FirstCharacteristicsId
									,FC.UserName AS SKU1
									,IM.FirstCharacteristicsValueId
									,FCV.UserName AS FirstCharacteristicsValue
									,IM.SecondCharacteristicsId
									,SC.UserName AS SKU2
									,IM.SecondCharacteristicsValueId
									,SCV.UserName AS SecondCharacteristicsValue
									,IM.ThirdCharacteristicsId
									,TC.UserName AS SKU3
									,IM.ThirdCharacteristicsValueId
									,TCV.UserName AS ThirdCharacteristicsValue
									,IRD.TransactionUoMId
									,TUoM.UserName AS TransactionUoM
									,IRD.MaterialTranRate AS TransactionRate
									,CU.Code AS CurrencyName
									,IR.ToCurrencyRate
									,(IRD.TransactionQty * IRD.MaterialTranRate) AS TrnAmount
									,IRD.TotalMaterialBooksCurrencyAmount AS BaseAmount
									--,IRD.TotalTaxAmount AS BaseTaxAmount
	
									--,TaxAmount = (
									--	SELECT SUM(TaxAmount)
										--FROM [TRN].[InventoryReceiveTax]
										--WHERE InventoryReceiveDetailId = IRD.Id
										--)
									--,IRD.ChargesTranAmount AS ChargesAmount
									--,ServiceCharge = (@totalServiceAmount / ISNULL(NULLIF(@totalReceiveAmount, 0), 1)) * IRD.MaterialTranAmount
									--,ServiceTax = (@totalSvcTaxAmount / ISNULL(NULLIF(@totalReceiveAmount, 0), 1)) * IRD.MaterialTranAmount
									--,IRD.CountryId
									--,PID.TransactionQty AS POQty
									--,ISNULL(Pre.OtherReceived, 0) OtherReceived
									,IRD.TransactionQty
									,(PID.TransactionQty - IRD.TransactionQty - ISNULL(Pre.OtherReceived, 0)) AS Balance
									,IRD.TransactionUoMId
									,IRD.BaseUOMId
									,IRD.TotalMaterialTranAmount
									,IRD.TotalMaterialBooksCurrencyAmount
									--,IRD.ShortageQty
									--,IRD.RejectionQty
									--,IRD.ApprovedQty
									--,IRD.TransactionQty AS PreviousQty
									--,IRD.ShortageRatePercent AS ShortageRate
									--,IRD.ShortageValue
									--,IRD.RejectRatePercent AS RejectionRate
									--,IRD.RejectValue AS RejectionValue
									--,IRD.RejectClamPercent RejectionClamRate
									,IR.CheckedBy
									,MRD.MaterialDetail
	                                ,C.Id,C.UserName CountryName,IRD.GrossAmount,IRD.DiscountAmount,MOI.MasterOrderId MasterOrderNo,IRD.MaterialFor
									,MaterialBy=CASE WHEN IRD.MaterialFor='JWOUTPUTMaterial' THEN 'OutPut' WHEN IRD.MaterialFor='JWBYPRODUCTMaterial' THEN 'By Product' END
								FROM TRN.InventoryMaterial AS IM
								LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
								LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
								LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
								LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
								LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
								LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
								LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
								LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
								LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
								LEFT JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId = IM.Id									
								LEFT JOIN [TRN].[PurchaseOrderDetail] AS PID ON PID.Id = IRD.PODetailsId
								LEFT JOIN (
									SELECT PODetailsId
										,Sum(TransactionQty) AS OtherReceived
									FROM trn.InventoryReceiveDetail	
									GROUP BY PODetailsId
									) AS Pre ON pre.PODetailsId = IRD.PODetailsId
								LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
								LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId = IR.Id
								LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id
								LEFT JOIN TRN.MaterialRequsitionDetails MRD ON MRD.ID = PID.RequisitionDetailId
                                Left Join SCS.Country C ON C.Id=IM.CountryId
								 LEFT JOIN [TRN].[MasterOrderItem] MOI ON IRD.MasterOrderItemId=MOI.Id
								WHERE IM.MaterialMasterId IS NULL  AND (IRD.MaterialFor='JWOUTPUTMaterial' OR IRD.MaterialFor='JWBYPRODUCTMaterial')";

				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public IEnumerable<object> GetInventoryReceiveByTransformationContractId(string plantId,string contractId)
        {
            try
            {
				string sql = @"SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
                                    ,isnull(PO.POId,'') POId
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId
                                    ,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCANo,isnull(PO.LCDate,'') LCDate,isnull(PDA.AcceptanceNo,'') AcceptanceNo
                                    ,IR.CheckedByStatus,IR.AuthorizedByStatus
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName,IR.NoteForAccounts--,ISNULL(PDA.Id,'') PurchaseDocumentAcceptanceId
									,EI.EmployeeName CheckedBy,EI1.EmployeeName ApprovedBy
                                    , REPLACE(CONVERT(CHAR(11), PDA.AcceptanceDate, 106),' ','-') AcceptanceDate,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById,IR.GRNType
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
							,PO.UDNo,ISNULL(MLC.OpeningBank,'') OpeningBank,ISNULL(Pr.UserName ,'') CustomerName
							,EI2.EmployeeName ByWhomName
									,EI2.SystemId ByWhomEmployeeId,EI2.SystemId EmpCode
							FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
						LEFT JOIN dbo.EmployeeInformation EI2 ON EI2.SystemId=IR.ByWhomEmployeeId
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
						, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='"+ plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='"+ plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
						--LEFT JOIN (select Distinct PDAA.Id,AcceptanceDate,AcceptanceNo,ACMAP.GRNId from TRN.GRNAcceptanceMap ACMAP 
									--left Join trn.PurchaseDocAcceptance  PDAA ON PDAA.Id=ACMAP.PurchaseDocumentAcceptanceId
									--)PDA ON PDA.GRNId=IR.Id

						LEFT JOIN(
							SELECT distinct PDAMAP.GRNId
								,AcceptanceNo=STUFF((select distinct ','+xpo.AcceptanceNo from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	

								,AcceptanceDate=STUFF((select distinct ','+ REPLACE(CONVERT(CHAR(11), xpo.AcceptanceDate, 106),' ','-')  
								from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.GRNAcceptanceMap PDAMAP 
							  LEFT JOIN [TRN].PurchaseDocAcceptance IR ON IR.Id = PDAMAP.PurchaseDocumentAcceptanceId
							  
							  group by  PDAMAP.GRNId
							)PDA ON PDA.GRNId=IR.Id

                         LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,UDNo=STUFF((select distinct ','+C.UDNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
							LEFT JOIN [dbo].[Contract] CON on CON.Id= PO.ContractId
								 LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId 
								 left JOIN dbo.MasterLC MLC ON MLC.CustomerId=Pr.Id
                        WHERE IR.PlantId='"+ plantId + @"'  AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL And IR.IsApproved = 0 and IR.GRNType='GRNBYJW'
						AND IR.TransformationContractId='"+contractId+"'";
				return _sqlRepository.GetDataCollection(sql);
			}
            catch (Exception ex)
            {
                throw ex;
            }
        }

		public IEnumerable<object> GetJWPODetail(string POId)
        {
            try
            {
				string sql = @"
							DECLARE @inventoryReceiveId VARCHAR(10)='"+ POId + @"'
	                                  , @totalReceiveAmount DECIMAL(18, 4)=0
	                                  , @totalServiceAmount DECIMAL(18, 4)=0
	                                  , @totalSvcTaxAmount DECIMAL(18, 4)=0
                        SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(TransactionAmount, 0)),1) FROM [TRN].[PurchaseOrderDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                        SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount - GRNServiceAmount, 0)),0) As Amount FROM [TRN].[POService] WHERE InventoryReceiveId=@inventoryReceiveId)
                        SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>'')
                           SELECT 
                              --IM.Id
                             IR.Id AS POID,IRD.Id AS PODetailsID
                            ,IRD.Id AS InventoryReceiveDetailId
                            , MGM.UserName AS MaterialGroupMasterName
                            , MM.Id MaterialMasterId
							, MM.UserName
                            --,IRD.MaterialStorageId
                            ,IRD.BaseUOMId
                            , IRD.ArticleId, ART.StandardName
                            , IRD.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                            , IRD.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                            , IRD.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                            , IRD.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                            , IRD.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                            , IRD.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
                            , IRD.TransactionQty AS POQty
                            , ISNULL(PAD.AcptTransactionQty,0) AS GRNRcvQty     
                              ,'' AS TransactionQty, ISNULL(PAD.AcptTransactionQty,0) Otherqty							 
							  ,(IRD.TransactionQty-PAD.AcptTransactionQty) As Balance
                            ,ISNULL(IRD.QtyStatus,0) QtyStatus
                            , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM, IRD.TransactionRate, CU.Code AS CurrencyName, IR.ToCurrencyRate                            
                            ,0 AS TrnAmount  
                            ,0 AS BaseTaxAmount
                            ,0 AS TaxAmount
	                        , 0 AS ChargesAmount
                            ,0 AS  ServiceCharge
                            , 0 AS ServiceTax
	                        , IRD.CountryId
                            ,'True' enableid
                            ,null POMaterialTaxList                            
                             ,IRD.TransactionQty*IRD.TransactionRate AS TotalMaterialTranAmount
                            , 0 AS ToTalMaterialBooksCurrencyAmount
                           ,IR.InvoicingByAddress,IR.DeliveryByAddress
         --                  ,IRD.RequisitionId
						   --,IRD.RequisitionDetailId
                           --,MRD.MaterialDetail
                           
                         FROM dbo.JWTransformationPurchaseOrderDetail AS IRD
                         left JOIN MST.MaterialMaster AS MM ON IRD.MaterialMasterId=MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IRD.ArticleId=ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IRD.FirstCharacteristicsId=FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IRD.SecondCharacteristicsId=SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IRD.ThirdCharacteristicsId=TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IRD.FirstCharacteristicsValueId=FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IRD.SecondCharacteristicsValueId=SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IRD.ThirdCharacteristicsValueId=TCV.Id
                       -- JOIN [TRN].[PurchaseOrderDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                        LEFT JOIN [dbo].[JWTransformationPurchaseOrder] AS IR ON IRD.JWTransformationPurchaseOrderId=IR.Id
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        --LEFT join [trn].MaterialRequsitionDetails MRD on MRD.Id=IRD.RequisitionDetailId
						LEFT JOIN (SELECT POId,PODetailId,Sum(TransactionQty) AcptTransactionQty FROM TRN.PurchaseDocAcceptanceDetail GROUP BY POId,PODetailId) PAD ON PAD.POId=IRD.JWTransformationPurchaseOrderId AND PAD.PODetailId=IRD.Id
                        WHERE IRD.JWTransformationPurchaseOrderId=@inventoryReceiveId and IRD.QtyStatus=0";
				return _sqlRepository.GetDataCollection(sql);
			}
            catch (Exception)
            {

                throw;
            }
        }


		public IEnumerable<object> GetSOWiseMaterialStock(string Material, string Article, string Skuvalue1, string Skuvalue2, string Skuvalue3, string processId, string parameters, string SOMATART,string SalesOrderId) 
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity; 
			string paramter = "";
			//if (Material != "")
			//{
			//	if (paramter == "")
			//		paramter += "ISNULL(mm.Id,'') in(" + Material + ")";
			//	else
			//		paramter += " AND ISNULL(mm.Id,'') in(" + Material + ")";
			//}
			//if (Article != "")
			//{
			//	if (paramter == "")
			//		paramter += "ISNULL(MRD.ArticleId,'') in(" + Article + ")";
			//	else
			//		paramter += " AND ISNULL(MRD.ArticleId,'') in(" + Article + ")";
			//}
			//if (Skuvalue1 != "")
			//{
			//	if (paramter == "")
			//		paramter += "ISNULL(BOQD.FirstCharacteristicsValueId,'') in(" + Skuvalue1 + ")";
			//	else
			//		paramter += " AND ISNULL(BOQD.FirstCharacteristicsValueId,'') in(" + Skuvalue1 + ")";
			//}
			//if (Skuvalue2 != "")
			//{
			//	if (paramter == "")
			//		paramter += "ISNULL(BOQD.SecondCharacteristicsValueId,'') in(" + Skuvalue2 + ")";
			//	else
			//		paramter += " AND ISNULL(BOQD.SecondCharacteristicsValueId,'') in(" + Skuvalue2 + ")";
			//}
			//if (Skuvalue3 != "")
			//{
			//	if (paramter == "")
			//		paramter += "ISNULL(BOQD.ThirdCharacteristicsValueId,'') in(" + Skuvalue3 + ")";
			//	else
			//		paramter += " AND ISNULL(BOQD.ThirdCharacteristicsValueId,'') in(" + Skuvalue3 + ")";
			//}

			if(string.IsNullOrEmpty(Skuvalue1) || Skuvalue1 == "null")
			{
				Skuvalue1 = "";
			}
			if (string.IsNullOrEmpty(Skuvalue2) || Skuvalue2=="null")
			{
				Skuvalue2 = "";
			}
			if (string.IsNullOrEmpty(Skuvalue3) || Skuvalue3 == "null")
			{
				Skuvalue3 = "";
			}
			try
			{
				var sql = "";				
				sql = @"SELECT mm.Id MaterialMasterId
						,mm.UserName MaterialMasterName
						,MMM.Id ArticleId			
						,MMM.StandardName ArticleName									
						,FC.Id FirstCharacteristicsId
						,FC.UserName AS FirstCharacteristics
						,IM.FirstCharacteristicsValueId
						,isnull(v1.UserName,'') AS FirstCharacteristicsValue
						,SC.Id SecondCharacteristicsId
						,SC.UserName AS SecondCharacteristics
						,IM.SecondCharacteristicsValueId
						,isnull(v2.UserName,'') AS SecondCharacteristicsValue
						,TC.Id ThirdCharacteristicsId
						,TC.UserName AS ThirdCharacteristics
						,IM.ThirdCharacteristicsValueId
						,isnull(v3.UserName,'') AS ThirdCharacteristicsValue		
						 ,Sum(GRNAllocation.TransactionQty) TransactionQty
						,GRNAllocation.TransactionUoMId
						,UOM.UserName TransactionUoMName
						,GRNAllocation.SalesOrderId,null RequestedQty--,MMAU.BaseUOMFactor
						FROM TRN.GRNPORequisitionAllocation GRNAllocation
						Left join TRN.InventoryReceiveDetail IRD ON IRD.Id=GRNAllocation.InventoryReceiveDetailId
						LEFT JOIN [SCS].[UnitOfMeasurement] UOM ON UOM.Id=GRNAllocation.TransactionUoMId
						LEFT JOIN trn.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
						left JOIN Mst.MaterialMaster MM ON MM.Id=IM.MaterialMasterId
						left JOIN Mst.MaterialMasterArticle MMM ON MMM.Id=IM.ArticleId


						LEFT OUTER JOIN[HKP].[CharacteristicsValue] V1 ON v1.Id = IM.FirstCharacteristicsValueId
						LEFT OUTER JOIN[HKP].[CharacteristicsValue] V2 ON v2.Id = IM.SecondCharacteristicsValueId
						LEFT OUTER JOIN[HKP].[CharacteristicsValue] V3 ON v3.Id = IM.ThirdCharacteristicsValueId
						LEFT JOIN HKP.Characteristics AS FC ON FC.Id = V1.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS SC ON SC.Id = V2.CharacteristicsId
						LEFT JOIN HKP.Characteristics AS TC ON TC.Id = V3.CharacteristicsId	
						--Left JOIN [MST].[MaterialMasterAlternativeUOM] AS MMAU ON MMAU.MaterialMasterId = MM.Id
						Where IM.MaterialMasterId='" + Material+@"' AND IM.ArticleId='"+Article+ @"'
						AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + Skuvalue1+@"'
						AND ISNULL(IM.SecondCharacteristicsValueId,'')='"+Skuvalue2+@"'
						AND ISNULL(IM.ThirdCharacteristicsValueId,'')='"+Skuvalue3+@"'
						AND GRNAllocation.SalesOrderId ='"+ SalesOrderId + @"'
						Group By GRNAllocation.TransactionUoMId,UOM.UserName,GRNAllocation.SalesOrderId,mm.Id ,mm.UserName ,MMM.Id,MMM.StandardName,FC.Id,FC.UserName,IM.FirstCharacteristicsValueId,isnull(v1.UserName,''),SC.Id,SC.UserName,IM.SecondCharacteristicsValueId,isnull(v2.UserName,''),TC.Id,TC.UserName,IM.ThirdCharacteristicsValueId,isnull(v3.UserName,'')";// ,MMAU.BaseUOMFactor
				return _sqlRepository.GetDataCollection(sql);

				//var Data = _sqlRepository.GetDataCollection(sql);
				//StringCollection strCol = new StringCollection();
				//string MaterialMasterList = "''";
				//for (int i = 0; i < Data.Count; i++)
				//{
				//	if (strCol.Contains(Data[i]["MaterialMasterId"].ToString()) == true)
				//		continue;
				//	strCol.Add(Data[i]["MaterialMasterId"].ToString());
				//	MaterialMasterList += ",'" + Data[i]["MaterialMasterId"].ToString() + "'";

				//}

				//var UOMList = _sqlRepository.GetDataCollection(@"select M.Id AS MaterialMasterId, UOM1.Id AS [Value],UOM1.UserName AS [Text] from (select Id,BaseUOMId UOMId from mst.MaterialMaster
				//													union
				//													select MaterialMasterId,AlternativeUOMId from mst.MaterialMasterAlternativeUOM
				//													) AS M
				//													 JOIN scs.UnitOfMeasurement AS uom1 ON uom1.Id=m.UOMId
				//													 where m.Id in (" + MaterialMasterList + @")");

				//for (int i = 0; i < Data.Count; i++)
				//{
				//	var temp = UOMList.Where(ee => ee["MaterialMasterId"].ToString() == Data[i]["MaterialMasterId"].ToString()).ToList();
				//	Data[i]["uoMList"] = temp;
				//}

				//return Data;
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
