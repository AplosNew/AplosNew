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
        public IEnumerable<object> GetGRNDetailsForSoAllocationBOQ(string InventoryReceiveDetailId, string PODetailId)
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


        public IEnumerable<object> GetMaterialListForProductionReq(string Material, string Article, string Skuvalue1, string Skuvalue2, string Skuvalue3, string processId, string parameters, string SOMATART, string queryString)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string paramter = "";
            
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
                
                sql = @"select 
						 Convert(bit, 'False') 'check',Null as uoMList
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
						,Sum(ISNULL(x.TransactionQty,0))  TransactionQty
						--,Sum(ISNULL(x.BaseUOMFactor,0))  BaseUOMFactor
						,Sum(ISNULL(x.TotalQty,0))  TotalQty	 

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
						--,ISNULL(x.TransactionQty,0)  TransactionQty
						,ISNULL(x.BaseUOMFactor,0)  BaseUOMFactor
						--,ISNULL(x.TotalQty,0)  TotalQty	 
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
						,BOQD.Id AS BOQId

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
					
					
						FROM 
						BOQ BOQD
					
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
						left join (select a.SalesOrderId, b.BOQDetailId,sum(a.TransactionQty) TransactionQty ,UOM.UserName,UOM.Id StockTransactionUoMId,a.BaseUoMId
														 from trn.GRNPORequisitionAllocation a
														left join trn.POBOQMap b ON b.Id=a.POBOQMapId
														LEFT JOIN [SCS].[UnitOfMeasurement] UOM ON UOM.Id=a.BaseUoMId
															--where a.SalesOrderId='212160101' --and b.BOQDetailId='21223-25'
														group by b.BOQDetailId,UOM.UserName,UOM.Id,a.SalesOrderId,a.BaseUoMId
														
														UNION ALL

														select a.SalesOrderId, OSPOBOQM.BOQDetailId,sum(a.TransactionQty) TransactionQty ,UOM.UserName,UOM.Id StockTransactionUoMId,a.BaseUoMId
														 from trn.GRNPORequisitionAllocation a
														left join [dbo].OSPOBOQMAP OSPOBOQM ON OSPOBOQM.Id=a.OSPOBOQMAPId
														LEFT JOIN [SCS].[UnitOfMeasurement] UOM ON UOM.Id=a.BaseUoMId
															--where a.SalesOrderId='212160101' --and b.BOQDetailId='21223-25'
														group by OSPOBOQM.BOQDetailId,UOM.UserName,UOM.Id,a.SalesOrderId,a.BaseUoMId
								) GRNALLO ON GRNALLO.BOQDetailId=BOQD.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] POUoMId ON POUoMId.Id=BOQD.POUoMId
						LEFT JOIN [SCS].[UnitOfMeasurement] BaseUoMId ON BaseUoMId.Id=BOQD.BaseUoMId
						LEFT JOIN [SCS].[UnitOfMeasurement] consumptionUoMId ON consumptionUoMId.Id=BOQD.UoMId
						Left JOIN (Select a.MaterialMasterId,a.AlternativeUOMId,a.BaseUOMId ,Sum(a.BaseUOMFactor) BaseUOMFactor 
									from [MST].[MaterialMasterAlternativeUOM] a
									left JOIN [SCS].[UnitOfMeasurement] UOM ON UOM.Id=a.AlternativeUOMId
									--left join mst.MaterialMaster mm ON mm.Id=a.MaterialMasterId
									Group by a.MaterialMasterId,a.AlternativeUOMId,a.BaseUOMId
									) AS MMAU ON MMAU.MaterialMasterId = BOQD.MaterialMasterId AND MMAU.AlternativeUOMId=TUoM.Id --And MMAU.BaseUOMId=BOQD.BaseUoMId AND MMAU.BaseUOMId=mm.BaseUOMId
                      



						left outer join (
							
							Select DISTINCT BOQD.BOQId, FGQty.Qty							
							FROM BOQDetail BOQD
						LEFT JOIN BOQFGMapping BOQFGM on BOQD.Id=BOQFGM.BOQDetailId
						JOIN (   

				            " + queryString + @"

							--Select '212160101-800-411-' ID, 15 Qty
							--union all
							--Select '212160101-800-410-' ID, 10 Qty
							--union all
							--Select '212160103-800-409-' ID,5 Qty

						) FGQty ON FGQty.ID=Concat(BOQD.SalesOrderId,'-',ISNULL(BOQFGM.FirstCharacteristicsValueId,''),'-',ISNULL(BOQFGM.SecondCharacteristicsValueId,''),'-',ISNULL(BOQFGM.ThirdCharacteristicsValueId,''))

						
						
						
						)FGQty ON FGQty.BOQId=boqd.Id

						Where BOQD.ProcessId='" + processId + @"'   and 
						boqd.Id IN (
						SELECT distinct boqd.BOQId FROM BOQDetail BOQD
						LEFT JOIN BOQFGMapping BOQFGM on BOQD.Id=BOQFGM.BOQDetailId
						WHERE Concat(BOQD.SalesOrderId,'-',ISNULL(BOQFGM.FirstCharacteristicsValueId,''),'-',ISNULL(BOQFGM.SecondCharacteristicsValueId,''),'-',ISNULL(BOQFGM.ThirdCharacteristicsValueId,'')) in (" + SOMATART + @")))x	

						group by x.MaterialMasterGroupName,x.MaterialType,x.MaterialMasterId,x.MaterialMasterName,x.ArticleId,x.StandardName,x.FirstCharacteristicsId,x.FirstCharacteristics,x.FirstCharacteristicsValue,x.SecondCharacteristicsId,x.SecondCharacteristics,x.SecondCharacteristicsValue,x.ThirdCharacteristicsId,x.ThirdCharacteristics,x.ThirdCharacteristicsValue,x.TransactionUoMId,x.TransactionUoMName ,x.POUoMId,x.POUoM,x.BaseUoMId,x.BaseUoM,x.StockTransactionUoMId,x.StockUOM ,x.consumptionUoM,x.consumptionUoMId,x.RwFirstCharacteristicsValueId,x.RwSecondCharacteristicsValueId,x.RwThirdCharacteristicsValueId,x.SalesOrderId,x.BOQDFirstCharacteristicsValueId,x.BOQDSecondCharacteristicsValueId,x.BOQDThirdCharacteristicsValueId,x.BOQId ,ISNULL(x.Consumption,0)  ,ISNULL(x.WastagePer,0) ,ISNULL(x.RequestedQty,0)  ,ISNULL(x.RequestedQtyNew,0)  
						,ISNULL(x.RejectedQty,0),ISNULL(x.ShortageQty,0) ,ISNULL(x.RejectionQty,0) ,ISNULL(x.BaseUOMFactor,0)";

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


        //Sales Wise

       

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
                            dr["InventoryReceiveId"] = MasterId.ToString();
                            dr["TaxCategoryId"] = UserSendData[i]["TaxCategoryId"];
                            dsDetail.Tables[0].Rows.Add(dr);
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
            public IEnumerable<object> GetAdvanceTaxInfoBOQ(string InventoryReceiveId)
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


        private string PurchaseReturnAddiTaxId()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "PurchaseReturnAdditionalTax", out sID);
            return sID;
        }
        public void SaveAdditinalTaxInPurchaseReturn(string MasterId, List<Dictionary<string, object>> UserSendData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string sql = "select * from TRN.PurchaseReturnAdditionalTax where PurchaseReturnId='" + MasterId + "'";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(sql, out DataSet dsDetail, false, "1");
                if (UserSendData != null)
                {
					for (int i = 0; i < UserSendData.Count; i++)
					{
						dsDetail.Tables[0].DefaultView.RowFilter = "TaxCodeId='" + UserSendData[i]["TaxCodeId"].ToString() + "'";
						if (dsDetail.Tables[0].DefaultView.Count == 0)
						{

							DataRow dr = dsDetail.Tables[0].NewRow();
							dr["Id"] = PurchaseReturnAddiTaxId();
							dr["TaxCodeId"] = UserSendData[i]["TaxCodeId"];
							dr["Percentage"] = UserSendData[i]["ValueOfFixed"];
							dr["TaxAmount"] = UserSendData[i]["TaxAmount"];
							dr["AddedBy"] = identity.Name;
							dr["AddedDate"] = System.DateTime.Now.ToString();
							dr["AddedFromIP"] = identity.IPAddress;

							dr["PurchaseReturnId"] = MasterId.ToString();
							dr["TaxCategoryId"] = UserSendData[i]["TaxCategoryId"];
							dsDetail.Tables[0].Rows.Add(dr);
						}
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

                   
                    dsDetail.Tables[0].DefaultView.RowFilter = "Id='" + UserSendData["Id"].ToString() + "'";
                    if (dsDetail.Tables[0].DefaultView.Count == 0)
                    {
                        
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

                    dsDetail.Tables[0].DefaultView.RowFilter = "Id='" + UserSendData["Id"].ToString() + "'";
                    if (dsDetail.Tables[0].DefaultView.Count == 0)
                    {

                        
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
                        
                        drlog["GRNID"] = MasterId.ToString();
                        dsDetailLog.Tables[0].Rows.Add(drlog);
                    }

                    clsStaticInfo info = new clsStaticInfo();
                    info.SaveDataSets(dsDetail, dsDetailLog);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            #region Stock Balance
            public IWorkbook CreateMaterialStockBalanceSheet(string companyId, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string Asset, string Inventory, string Country, string materialStorage, bool bale, bool brand)
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
                    InventoryStockReportService inventoryStockReportService = new InventoryStockReportService();

                if (fromDate == null || fromDate=="")
                {
					inventoryStockReportService.CreateMaterialStockBalanceSheet(ref sheet1, ref sheet2, report, Head, "Summary", companyId, plantId, fromDate, toDate, Qty, Amount, RcptIssue, Asset, Inventory, Country, materialStorage,bale,brand);
				}
                else
                {
					inventoryStockReportService.CreateMaterialStockBalanceForThePeriodSheet(ref sheet1, ref sheet2, report, Head, "Summary", companyId, plantId, fromDate, toDate, Qty, Amount, RcptIssue, Asset, Inventory, Country, materialStorage, bale, brand);
				}
					
				workbook.Version = ExcelVersion.Excel2016;
                    return workbook;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
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

                    dsDetail.Tables[0].DefaultView.RowFilter = "Id='" + UserSendData["Id"].ToString() + "'";
                    if (dsDetail.Tables[0].DefaultView.Count == 0)
                    {
                       
                    }
                    else
                    {
                        DataRow dr = dsDetail.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["CheckedByStatus"] = CheckedApprovedStataus;
                        dr["CheckedHoldRejectReason"] = CheckedHoldRejectReason;
                        dr.EndEdit();
                        
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

                    dsDetail.Tables[0].DefaultView.RowFilter = "Id='" + UserSendData["Id"].ToString() + "'";
                    if (dsDetail.Tables[0].DefaultView.Count> 0)
                    {
                        DataRow dr = dsDetail.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["GateOutStatus"] = true;
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
                    ,IsPark=case when IR.VoucherId<>'' then 'No' else 'Yes' end,GL.UserName GL,B.UserName Budget,A.UserName Activity
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
					,IRD.LotNo,P.UserName VendorName
					,IR.DocRefNo,IRD.POId PONo
                    ,FCV.UserName FirstCharacteristics,SCV.UserName SecondCharacteristics,MC.UserName MaterialCategory
					FROM TRN.InventoryMaterial AS IM
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
						left JOIN MST.MaterialMaster AS MM ON ART.MaterialMasterId=MM.Id
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
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
						left join HKP.Party P on IR.PartyId=p.Id
                        left join [HKP].[MaterialCategory] MC on MC.Id=MM.MaterialCategoryId
                        LEFT JOIN HKP.GLGeneralinfo GL ON GL.Id=ird.postdrglgeneralinfoid
						LEFT JOIN MST.BudgetMaster BM ON BM.Id=ird.postdrBudgetmasterid
						LEFT JOIN HKP.Budget B ON B.Id=bm.BudgetId
						LEFT JOIN HKP.Activity A ON A.Id=ird.postdrActivityId
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
                    ,IsPark=case when IR.VoucherId<>'' then 'No' else 'Yes' end,GL.UserName GL,B.UserName Budget,A.UserName Activity
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
					,IRD.LotNo,P.UserName VendorName
					,IR.DocRefNo,IRD.POId PONo
                    ,FCV.UserName FirstCharacteristics,SCV.UserName SecondCharacteristics,MC.UserName MaterialCategory
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
						left join HKP.Party P on IR.PartyId=p.Id
                        left join [HKP].[MaterialCategory] MC on MC.Id=MM.MaterialCategoryId
                        LEFT JOIN HKP.GLGeneralinfo GL ON GL.Id=ird.postdrglgeneralinfoid
						LEFT JOIN MST.BudgetMaster BM ON BM.Id=ird.postdrBudgetmasterid
						LEFT JOIN HKP.Budget B ON B.Id=bm.BudgetId
						LEFT JOIN HKP.Activity A ON A.Id=ird.postdrActivityId
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
                    ,IR.DocRefNo,IRD.POId PONo
                    ,FCV.UserName FirstCharacteristics,SCV.UserName SecondCharacteristics
                    ,IRD.LotNo,P.UserName VendorName,MC.UserName MaterialCategory

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
                        left join HKP.Party P on IR.PartyId=p.Id
                        left join [HKP].[MaterialCategory] MC on MC.Id=MM.MaterialCategoryId
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

                    _row++;

                    sheet1[_row, 1].Text = "RECEIPTS";
                    sheet1[_row, 1].CellStyle.Font.Size = 10;
                    sheet1[_row, 1].CellStyle.Font.Bold = true;
                    //sheet1.UsedRange.WrapText = true;
                    sheet1[_row, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //sheet1[_row, 1].CellStyle.Interior.Color = System.Drawing.Color.GreenYellow;
                    sheet1.Range[_row, 1, _row, 15].BorderAround(ExcelLineStyle.Thick);
                    sheet1.Range[_row, 1, _row, 15].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[_row, 1, _row, 15].CellStyle.FillBackground = ExcelKnownColors.Tan;
                    sheet1.Range[_row, 1, _row, 15].Merge();



                    sheet1[_row, 16].Text = "PURCHASE RETURN";
                    sheet1[_row, 16].CellStyle.Font.Size = 10;
                    sheet1[_row, 16].CellStyle.Font.Bold = true;
                    sheet1[_row, 16].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //sheet1[_row, 1].CellStyle.Interior.Color = System.Drawing.Color.GreenYellow;
                    sheet1.Range[_row, 16, _row, 20].BorderAround(ExcelLineStyle.Thick);
                    sheet1.Range[_row, 16, _row, 20].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[_row, 16, _row, 20].CellStyle.FillBackground = ExcelKnownColors.Tan;
                    sheet1.Range[_row, 16, _row, 20].Merge();


                    sheet1[_row, 21].Text = "ISSUE";
                    sheet1[_row, 21].CellStyle.Font.Size = 10;
                    sheet1[_row, 21].CellStyle.Font.Bold = true;
                    sheet1[_row, 21].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //sheet1[_row, 6].CellStyle.Interior.Color = System.Drawing.Color.Gray;
                    sheet1.Range[_row, 21, _row, 27].BorderAround(ExcelLineStyle.Thick);
                    sheet1.Range[_row, 21, _row, 27].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[_row, 21, _row, 27].CellStyle.FillBackground = ExcelKnownColors.Tan;
                    sheet1.Range[_row, 21, _row, 27].Merge();


                    sheet1[_row, 28].Text = "ISSUE RETURN";
                    sheet1[_row, 28].CellStyle.Font.Size = 10;
                    sheet1[_row, 28].CellStyle.Font.Bold = true;
                    sheet1[_row, 28].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //sheet1[_row, 6].CellStyle.Interior.Color = System.Drawing.Color.Gray;
                    sheet1.Range[_row, 28, _row, 32].BorderAround(ExcelLineStyle.Thick);
                    sheet1.Range[_row, 28, _row, 32].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[_row, 28, _row, 32].CellStyle.FillBackground = ExcelKnownColors.Tan;
                    sheet1.Range[_row, 28, _row, 32].Merge();


                    sheet1[_row, 33].Text = "ADJUSTMENT";
                    sheet1[_row, 33].CellStyle.Font.Size = 10;
                    sheet1[_row, 33].CellStyle.Font.Bold = true;
                    sheet1[_row, 33].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //sheet1[_row, 6].CellStyle.Interior.Color = System.Drawing.Color.Gray;
                    sheet1.Range[_row, 33, _row, 37].BorderAround(ExcelLineStyle.Thick);
                    sheet1.Range[_row, 33, _row, 37].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[_row, 33, _row, 37].CellStyle.FillBackground = ExcelKnownColors.Tan;
                    sheet1.Range[_row, 33, _row, 37].Merge();


                    sheet1[_row, 38].Text = "STOCK BALANCE";
                    sheet1[_row, 38].CellStyle.Font.Size = 10;
                    sheet1[_row, 38].CellStyle.Font.Bold = true;
                    sheet1[_row, 38].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //sheet1[_row, 11].CellStyle.Interior.Color = System.Drawing.Color.HotPink;
                    sheet1.Range[_row, 38, _row, 40].BorderAround(ExcelLineStyle.Thick);
                    sheet1.Range[_row, 38, _row, 40].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[_row, 38, _row, 40].CellStyle.FillBackground = ExcelKnownColors.Tan;
                    sheet1.Range[_row, 38, _row, 40].Merge();

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

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "MRNo");
                    var colRCVMRNo = sheet1headreColIndex;
				    sheet1.Range[_rowL, sheet1headreColIndex].Text = "MRNo";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;
				
					var colRCVMaterial = sheet1headreColIndex;
                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 22;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    var colRCVMaterialCategory = sheet1headreColIndex;
                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Category";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 22;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    var colRCVArticle = sheet1headreColIndex;
                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
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

                    var colRCVVoucherNo = sheet1headreColIndex;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Voucher No";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 12;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "UOM");
                    //sheet1headreColIndex++;
                    var colUOM = sheet1headreColIndex;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "UOM";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 8;
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

                var colRCVGL = sheet1headreColIndex;
                //sheet1headreColIndex++;

                sheet1.Range[_rowL, sheet1headreColIndex].Text = "GL";
                sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                sheet1headreColIndex++;

                var colRCVBudget = sheet1headreColIndex;
                //sheet1headreColIndex++;

                sheet1.Range[_rowL, sheet1headreColIndex].Text = "Budget";
                sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                sheet1headreColIndex++;


                var colRCVActivity = sheet1headreColIndex;
                //sheet1headreColIndex++;

                sheet1.Range[_rowL, sheet1headreColIndex].Text = "Activity";
                sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                sheet1headreColIndex++;

                var colRCVIsPark = sheet1headreColIndex;
                //sheet1headreColIndex++;

                sheet1.Range[_rowL, sheet1headreColIndex].Text = "IsPark";
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

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Return No";
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
                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Voucher No";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 12;
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
                        if (clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Seq"].ToString()) == 1)
                        {

                            report.SetText(ref sheet1, _rowL, colRCVDate, inventoryMaterialList.Rows[n]["RcvDate"].ToString());
                            report.SetText(ref sheet1, _rowL, colRCVMRNo, rcvid);
                            report.SetText(ref sheet1, _rowL, colIsAssetStatus, inventoryMaterialList.Rows[n]["IsAssetStatus"].ToString());
                            report.SetText(ref sheet1, _rowL, colUOM, inventoryMaterialList.Rows[n]["UOM"].ToString());
                            report.SetText(ref sheet1, _rowL, colRCVQuantity, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["RcvQty"].ToString()));
                            report.SetText(ref sheet1, _rowL, colRCVRate, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["RcvRate"].ToString()));
                            report.SetText(ref sheet1, _rowL, colRCVAmount, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["RcvAmount"].ToString()));
                            report.SetText(ref sheet1, _rowL, colRCVVoucherNo, inventoryMaterialList.Rows[n]["VoucherNo"].ToString());

                            report.SetText(ref sheet1, _rowL, colRCVMaterial, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
                            report.SetText(ref sheet1, _rowL, colRCVMaterialCategory, inventoryMaterialList.Rows[n]["MaterialCategory"].ToString());
                            report.SetText(ref sheet1, _rowL, colRCVArticle, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
                            report.SetText(ref sheet1, _rowL, colRCVGL, inventoryMaterialList.Rows[n]["GL"].ToString());
                            report.SetText(ref sheet1, _rowL, colRCVBudget, inventoryMaterialList.Rows[n]["Budget"].ToString());
                            report.SetText(ref sheet1, _rowL, colRCVActivity, inventoryMaterialList.Rows[n]["Activity"].ToString());
                            report.SetText(ref sheet1, _rowL, colRCVIsPark, inventoryMaterialList.Rows[n]["IsPark"].ToString());

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
                        report.SetText(ref sheet1, _rowL, colIssueIssueVoucherNo, inventoryMaterialList.Rows[n]["IssueVoucherNo"].ToString());
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

                // Start Details
                var _rowd2 = 4;

                if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
                {
                    sheet2[_rowd2, 4].Text = fromDate + "To" + toDate;
                    sheet2[_rowd2, 4].CellStyle.Font.Size = 8;
                    sheet2[_rowd2, 4].CellStyle.Font.Bold = false;
                    sheet2.Range[_rowd2, 3, _rowd2, 4].Merge();
                    //sheet1.Range[_rowd, 3, _rowd , 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                }

                var _rows2 = 5;
                sheet2[_rows2, 5].Text = "Report Ref No: ";
                sheet2[_rowd2, 5].CellStyle.Font.Size = 8;
                sheet2[_rowd2, 5].CellStyle.Font.Bold = false;
                sheet2.Range[_rows2, 3, _rows2, 6].Merge();
                var _row2 = 7;

                _row2++;

                sheet2[_row2, 1].Text = "RECEIPTS";
                sheet2[_row2, 1].CellStyle.Font.Size = 10;
                sheet2[_row2, 1].CellStyle.Font.Bold = true;
                //sheet1.UsedRange.WrapText = true;
                sheet2[_row2, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                //sheet1[_row, 1].CellStyle.Interior.Color = System.Drawing.Color.GreenYellow;
                sheet2.Range[_row2, 1, _row2, 16].BorderAround(ExcelLineStyle.Thick);
                sheet2.Range[_row2, 1, _row2, 16].BorderInside(ExcelLineStyle.Hair);
                sheet2.Range[_row2, 1, _row2, 16].CellStyle.FillBackground = ExcelKnownColors.Tan;
                sheet2.Range[_row2, 1, _row2, 16].Merge();



                sheet2[_row2, 17].Text = "PURCHASE RETURN";
                sheet2[_row2, 17].CellStyle.Font.Size = 10;
                sheet2[_row2, 17].CellStyle.Font.Bold = true;
                //sheet1.UsedRange.WrapText = true;
                sheet2[_row2, 17].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                //sheet1[_row, 1].CellStyle.Interior.Color = System.Drawing.Color.GreenYellow;
                sheet2.Range[_row2, 17, _row2, 21].BorderAround(ExcelLineStyle.Thick);
                sheet2.Range[_row2, 17, _row2, 21].BorderInside(ExcelLineStyle.Hair);
                sheet2.Range[_row2, 17, _row2, 21].CellStyle.FillBackground = ExcelKnownColors.Tan;
                sheet2.Range[_row2, 17, _row2, 21].Merge();
                                 

                sheet2[_row2, 22].Text = "ISSUE";
                sheet2[_row2, 22].CellStyle.Font.Size = 10;
                sheet2[_row2, 22].CellStyle.Font.Bold = true;
                //sheet1.UsedRange.WrapText = true;
                sheet2[_row2, 22].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                //sheet1[_row, 6].CellStyle.Interior.Color = System.Drawing.Color.Gray;
                sheet2.Range[_row2, 22, _row2, 28].BorderAround(ExcelLineStyle.Thick);
                sheet2.Range[_row2, 22, _row2, 28].BorderInside(ExcelLineStyle.Hair);
                sheet2.Range[_row2, 22, _row2, 28].CellStyle.FillBackground = ExcelKnownColors.Tan;
                sheet2.Range[_row2, 22, _row2, 28].Merge();


                sheet2[_row2, 29].Text = "ISSUE RETURN";
                sheet2[_row2, 29].CellStyle.Font.Size = 10;
                sheet2[_row2, 29].CellStyle.Font.Bold = true;
                //sheet1.UsedR7nge.WrapText = true;
                sheet2[_row2, 29].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                //sheet1[_row, 6].CellStyle.Interior.Color = System.Drawing.Color.Gray;
                sheet2.Range[_row2, 29, _row2, 33].BorderAround(ExcelLineStyle.Thick);
                sheet2.Range[_row2, 29, _row2, 33].BorderInside(ExcelLineStyle.Hair);
                sheet2.Range[_row2, 29, _row2, 33].CellStyle.FillBackground = ExcelKnownColors.Tan;
                sheet2.Range[_row2, 29, _row2, 33].Merge();


                sheet2[_row2, 34].Text = "ADJUSTMENT";
                sheet2[_row2, 34].CellStyle.Font.Size = 10;
                sheet2[_row2, 34].CellStyle.Font.Bold = true;
                //sheet1.UsedR2nge.WrapText = true;
                sheet2[_row2, 34].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                //sheet1[_row, 6].CellStyle.Interior.Color = System.Drawing.Color.Gray;
                sheet2.Range[_row2, 34, _row2, 38].BorderAround(ExcelLineStyle.Thick);
                sheet2.Range[_row2, 34, _row2, 38].BorderInside(ExcelLineStyle.Hair);
                sheet2.Range[_row2, 34, _row2, 38].CellStyle.FillBackground = ExcelKnownColors.Tan;
                sheet2.Range[_row2, 34, _row2, 38].Merge();
                                            

                sheet2[_row2, 39].Text = "STOCK BALANCE";
                sheet2[_row2, 39].CellStyle.Font.Size = 10;
                sheet2[_row2, 39].CellStyle.Font.Bold = true;
                //sheet1.UsedR7nge.WrapText = true;
                sheet2[_row2, 39].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                //sheet1[_row, 11].CellStyle.Interior.Color = System.Drawing.Color.HotPink;
                sheet2.Range[_row2, 39, _row2, 41].BorderAround(ExcelLineStyle.Thick);
                sheet2.Range[_row2, 39, _row2, 41].BorderInside(ExcelLineStyle.Hair);
                sheet2.Range[_row2, 39, _row2, 41].CellStyle.FillBackground = ExcelKnownColors.Tan;
                sheet2.Range[_row2, 39, _row2, 41].Merge();

                var _rowL2 = _row2;
                var row2 = _row2 + 1;


                var sheet1headreColIndex2 = 1;
                //var sheet2headreColIndex = 1;
                _rowL2 += 1;
                var Row_Total_Start2 = _rowL2;
                //Receive
                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Date");
                //sheet1headreColIndex++;
                var colRCVDate2 = sheet1headreColIndex2;
                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Date";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "MRNo");
                var colRCVMRNo2 = sheet1headreColIndex2;
                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "MRNo";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                var colDocRefNo2 = sheet1headreColIndex2;
                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Doc Ref No";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                var colPONo2 = sheet1headreColIndex2;
                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "PO No";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                var colVendorName2 = sheet1headreColIndex2;
                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Vendor Name";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 29;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                var colLotNo2 = sheet1headreColIndex2;
                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Lot No";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                var colRCVMaterial2 = sheet1headreColIndex2;
                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Material";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 22;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                var colRCVArticle2 = sheet1headreColIndex2;
                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Article";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 25;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                var colFirstCharacteristics = sheet1headreColIndex2;
                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "SKU1";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 25;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                var colSecondCharacteristics = sheet1headreColIndex2;
                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "SKU2";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 25;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Type");
                //sheet1headreColIndex++;
                var colIsAssetStatus2 = sheet1headreColIndex2;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Type";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                var colRCVVoucherNo2 = sheet1headreColIndex2;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Voucher No";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 12;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "UOM");
                //sheet1headreColIndex++;
                var colUOM2 = sheet1headreColIndex2;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "UOM";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 8;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Quantity");
                var colRCVQuantity2 = sheet1headreColIndex2;
                //sheet1headreColIndex++;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Quantity";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Rate");
                var colRCVRate2 = sheet1headreColIndex2;
                //sheet1headreColIndex++;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Rate";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                var colRCVAmount2 = sheet1headreColIndex2;
                //sheet1headreColIndex++;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Amount";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;


                // Purchase Return
                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Date");
                var colPOReturnDate2 = sheet1headreColIndex2;
                //sheet1headreColIndex++;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Date";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "ReturnNo");
                var colPurchaseReturnNo2 = sheet1headreColIndex2;
                //sheet1headreColIndex++;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "ReturnNo";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                var colPurchaseReturnQty2 = sheet1headreColIndex2;
                //sheet1headreColIndex++;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Qty";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Rate");
                var colPurchaseReturnRate2 = sheet1headreColIndex2;
                //sheet1headreColIndex++;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Rate";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                var colPurchaseReturnAmount2 = sheet1headreColIndex2;
                //sheet1headreColIndex++;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Amount";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                //Issue

                var colIssueDate2 = sheet1headreColIndex2;
                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Date";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SlipNo");
                var colIssueNo2 = sheet1headreColIndex2;
                //sheet1headreColIndex++;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "SlipNo";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                var colIssueIssueVoucherNo2 = sheet1headreColIndex2;
                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Voucher No";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Type");
                var colType2 = sheet1headreColIndex2;
                //sheet1headreColIndex++;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Type";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;


                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Quantity");
                var colIssueQty2 = sheet1headreColIndex2;
                //sheet1headreColIndex++;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Quantity";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Rate");
                var colIssueRate2 = sheet1headreColIndex2;
                //sheet1headreColIndex++;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Rate";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                var colIssueAmount2 = sheet1headreColIndex2;
                //sheet1headreColIndex++;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Amount";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                //Issue Return
                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Date");
                var colIssuereturnDate2 = sheet1headreColIndex2;
                //sheet1headreColIndex++;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Date";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "ReturnNo");
                var colIssuereturnNo2 = sheet1headreColIndex2;
                //sheet1headreColIndex++;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "ReturnNo";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;


                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Quantity");
                var colIssuereturnQty2 = sheet1headreColIndex2;
                //sheet1headreColIndex++;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Quantity";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Rate");
                var colIssuereturnRate2 = sheet1headreColIndex2;
                //sheet1headreColIndex++;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Rate";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                var colIssuereturnAmount2 = sheet1headreColIndex2;
                //sheet1headreColIndex++;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Amount";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                //Adjustment 

                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Date");
                var colAdjustmentDate2 = sheet1headreColIndex2;
                //sheet1headreColIndex++;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Date";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;


                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "AdjustmentsNo");
                var colAdjustmentNo2 = sheet1headreColIndex2;
                //sheet1headreColIndex++;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "AdjustmentsNo";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 15;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Quantity");
                var colAdjustmentQty2 = sheet1headreColIndex2;
                //sheet1headreColIndex++;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Quantity";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;


                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Rate");
                var colAdjustmentRate2 = sheet1headreColIndex2;
                //sheet1headreColIndex++;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Rate";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                var colAdjustmentAmount2 = sheet1headreColIndex2;
                //sheet1headreColIndex++;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Amount";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;

                //Balance

                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Quantity");
                var colBalanceQty2 = sheet1headreColIndex2;
                //sheet1headreColIndex++;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Quantity";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;


                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Rate");
                //sheet1headreColIndex++;
                var colBalanceRate2 = sheet1headreColIndex2;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Rate";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                sheet1headreColIndex2++;


                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                var colBalanceAmount2 = sheet1headreColIndex2;

                sheet2.Range[_rowL2, sheet1headreColIndex2].Text = "Amount";
                sheet2.Range[_rowL2, sheet1headreColIndex2].ColumnWidth = 10;
                sheet2.Range[_rowL2, sheet1headreColIndex2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[_rowL2, sheet1headreColIndex2].CellStyle.Font.Bold = true;
                //sheet1headreColIndex++;

                sheet2.Range[_rowL2, 1, _rowL2, sheet1headreColIndex2].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                sheet2.Range[_rowL2, 1, _rowL2, sheet1headreColIndex2].CellStyle.Font.Size = 10;
                sheet2.Range[_rowL2, 1, _rowL2, sheet1headreColIndex2].RowHeight = 22;

                var balanceQty2 = 0.00;
                List<string> list2 = new List<string>();
                for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
                {
                    _rowL2++;
                    var rcvid = inventoryMaterialList.Rows[n]["Id"].ToString();
                    if (clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Seq"].ToString()) == 1)
                    {

                        report.SetText(ref sheet2, _rowL2, colRCVDate2, inventoryMaterialList.Rows[n]["RcvDate"].ToString());
                        report.SetText(ref sheet2, _rowL2, colRCVMRNo2, rcvid);
                        report.SetText(ref sheet2, _rowL2, colDocRefNo2, inventoryMaterialList.Rows[n]["DocRefNo"].ToString());
                        report.SetText(ref sheet2, _rowL2, colPONo2, inventoryMaterialList.Rows[n]["PONo"].ToString());
                        report.SetText(ref sheet2, _rowL2, colVendorName2, inventoryMaterialList.Rows[n]["VendorName"].ToString());
                        report.SetText(ref sheet2, _rowL2, colLotNo2, inventoryMaterialList.Rows[n]["LotNo"].ToString());
                        report.SetText(ref sheet2, _rowL2, colIsAssetStatus2, inventoryMaterialList.Rows[n]["IsAssetStatus"].ToString());
                        report.SetText(ref sheet2, _rowL2, colUOM2, inventoryMaterialList.Rows[n]["UOM"].ToString());
                        report.SetText(ref sheet2, _rowL2, colRCVQuantity2, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["RcvQty"].ToString()));
                        report.SetText(ref sheet2, _rowL2, colRCVRate2, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["RcvRate"].ToString()));
                        report.SetText(ref sheet2, _rowL2, colRCVAmount2, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["RcvAmount"].ToString()));
                        report.SetText(ref sheet2, _rowL2, colRCVVoucherNo2, inventoryMaterialList.Rows[n]["VoucherNo"].ToString());

                        report.SetText(ref sheet2, _rowL2, colRCVMaterial2, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
                        report.SetText(ref sheet2, _rowL2, colRCVArticle2, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
                        report.SetText(ref sheet2, _rowL2, colFirstCharacteristics, inventoryMaterialList.Rows[n]["FirstCharacteristics"].ToString());
                        report.SetText(ref sheet2, _rowL2, colSecondCharacteristics, inventoryMaterialList.Rows[n]["SecondCharacteristics"].ToString());

                        //start
                        var RcvQty = clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["RcvQty"].ToString());
                        var IssueQty = clsStaticInfo.dbl(inventoryMaterialList.Compute("SUM(IssueQty)", "Id = '" + rcvid + "'").ToString());
                        var AdjustmentQty = clsStaticInfo.dbl(inventoryMaterialList.Compute("SUM(AdjustmentQty)", "Id = '" + rcvid + "'").ToString());
                        var PurchaseReturnQty = clsStaticInfo.dbl(inventoryMaterialList.Compute("SUM(PurchaseReturnQty)", "Id = '" + rcvid + "'").ToString());
                        var IssueReturnQty = clsStaticInfo.dbl(inventoryMaterialList.Compute("SUM(IssueReturnQty)", "Id = '" + rcvid + "'").ToString());

                        //end

                        //balanceQty = clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["RcvQty"].ToString()) - clsStaticInfo.dbl(inventoryMaterialList.Compute("SUM(IssueQty)", "Id = '" + rcvid + "'").ToString());
                        balanceQty = RcvQty - IssueQty - AdjustmentQty - PurchaseReturnQty + IssueReturnQty;
                        report.SetText(ref sheet2, _rowL2, colBalanceQty2, balanceQty);
                        report.SetText(ref sheet2, _rowL2, colBalanceQty2, balanceQty);
                        if (balanceQty == 0)
                        {
                            var colBalanceRate1 = 0;
                            report.SetText(ref sheet2, _rowL2, colBalanceQty2, colBalanceRate1);

                        }
                        else
                        {
                            report.SetText(ref sheet2, _rowL2, colBalanceRate2, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BalanceRate"].ToString()));
                            report.SetText(ref sheet2, _rowL2, colBalanceAmount2, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BalanceRate"].ToString()) * balanceQty);
                        }
                    }

                    //Purchase Return
                    report.SetText(ref sheet2, _rowL2, colPOReturnDate2, inventoryMaterialList.Rows[n]["PurchaseReturnDate"].ToString());
                    report.SetText(ref sheet2, _rowL2, colPurchaseReturnNo2, inventoryMaterialList.Rows[n]["PurchaseReturnNo"].ToString());
                    report.SetText(ref sheet2, _rowL2, colPurchaseReturnQty2, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQty"].ToString()));
                    report.SetText(ref sheet2, _rowL2, colPurchaseReturnRate2, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnRate"].ToString()));
                    report.SetText(ref sheet2, _rowL2, colPurchaseReturnAmount2, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnAmount"].ToString()));

                    //Issue
                    report.SetText(ref sheet2, _rowL2, colIssueDate2, inventoryMaterialList.Rows[n]["IssueDate"].ToString());
                    report.SetText(ref sheet2, _rowL2, colIssueNo2, inventoryMaterialList.Rows[n]["IssueNo"].ToString());
                    report.SetText(ref sheet2, _rowL2, colType2, inventoryMaterialList.Rows[n]["IssueType"].ToString());
                    report.SetText(ref sheet2, _rowL2, colIssueQty2, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueQty"].ToString()));
                    report.SetText(ref sheet2, _rowL2, colIssueRate2, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Rate"].ToString()));
                    report.SetText(ref sheet2, _rowL2, colIssueAmount2, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueAmount"].ToString()));
                    report.SetText(ref sheet2, _rowL2, colIssueIssueVoucherNo2, inventoryMaterialList.Rows[n]["IssueVoucherNo"].ToString());
                    //Issue Return

                    report.SetText(ref sheet2, _rowL2, colIssuereturnDate2, inventoryMaterialList.Rows[n]["IssueReturnDate"].ToString());
                    report.SetText(ref sheet2, _rowL2, colIssuereturnNo2, inventoryMaterialList.Rows[n]["IssueReturnNo"].ToString());
                    report.SetText(ref sheet2, _rowL2, colIssuereturnQty2, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQty"].ToString()));
                    report.SetText(ref sheet2, _rowL2, colIssuereturnRate2, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnRate"].ToString()));
                    report.SetText(ref sheet2, _rowL2, colIssuereturnAmount2, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnAmount"].ToString()));

                    //Adjustment

                    report.SetText(ref sheet2, _rowL2, colAdjustmentDate2, inventoryMaterialList.Rows[n]["AdjustmentDate"].ToString());
                    report.SetText(ref sheet2, _rowL2, colAdjustmentNo2, inventoryMaterialList.Rows[n]["AdjustmentNo"].ToString());
                    report.SetText(ref sheet2, _rowL2, colAdjustmentQty2, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQty"].ToString()));
                    report.SetText(ref sheet2, _rowL2, colAdjustmentRate2, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentRate"].ToString()));
                    report.SetText(ref sheet2, _rowL2, colAdjustmentAmount2, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentAmount"].ToString()));

                }

                //_rowL++;

                sheet2.Range[(Row_Total_Start2), 1, _rowL2, sheet1headreColIndex2].CellStyle.Font.Size = 8;
                sheet2.Range[Row_Total_Start2, colRCVDate2, _rowL2, colRCVAmount2].BorderAround(ExcelLineStyle.Thin);
                sheet2.Range[Row_Total_Start2, colRCVDate2, _rowL2, colRCVAmount2].BorderInside(ExcelLineStyle.Hair);

                sheet2.Range[Row_Total_Start2, colPOReturnDate2, _rowL2, colPurchaseReturnAmount2].BorderAround(ExcelLineStyle.Thin);
                sheet2.Range[Row_Total_Start2, colPOReturnDate2, _rowL2, colPurchaseReturnAmount2].BorderInside(ExcelLineStyle.Hair);

                sheet2.Range[Row_Total_Start2, colIssuereturnDate2, _rowL2, colIssuereturnAmount2].BorderAround(ExcelLineStyle.Thin);
                sheet2.Range[Row_Total_Start2, colIssuereturnDate2, _rowL2, colIssuereturnAmount2].BorderInside(ExcelLineStyle.Hair);

                sheet2.Range[Row_Total_Start2, colAdjustmentDate2, _rowL2, colAdjustmentAmount2].BorderAround(ExcelLineStyle.Thin);
                sheet2.Range[Row_Total_Start2, colAdjustmentDate2, _rowL2, colAdjustmentAmount2].BorderInside(ExcelLineStyle.Hair);


                sheet2.Range[Row_Total_Start2, colIssueDate2, _rowL2, colIssueAmount2].BorderAround(ExcelLineStyle.Thin);
                sheet2.Range[Row_Total_Start2, colIssueDate2, _rowL2, colIssueAmount2].BorderInside(ExcelLineStyle.Hair);

                sheet2.Range[Row_Total_Start2, colBalanceQty2, _rowL2, colBalanceAmount2].BorderAround(ExcelLineStyle.Thin);
                sheet2.Range[Row_Total_Start2, colBalanceQty2, _rowL2, colBalanceAmount2].BorderInside(ExcelLineStyle.Hair);

                //End Details

                sheet1.Name = "Material Ledger Summary";
                sheet2.Name = "Material Ledger Details";
                sheet2.UsedRange.WrapText = true;
                //sheet1.UsedRange.CellStyle.Font.Size = 8;
                sheet2.IsGridLinesVisible = false;
                    //report.PlantHeader(ref sheet1, sheet1headreColIndex, sheet1Name, plantId);
                    report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }

            public IEnumerable<object> QueryGetListGRNMasterData(string plantId, string GRNbyPOCheckStatus,string grntype)

            {
                try
                {
                    var tempsql = "";
                    if (GRNbyPOCheckStatus == "ForChecked")
                    {
                        tempsql = @"AND ((IR.CheckedByStatus='ForChecked' And IR.IsApproved = 0 ) OR (IR.CheckedByStatus IS NULL and IR.AuthorizedByStatus='For Approval' And IR.IsApproved = 0 )
						 OR(IR.CheckedByStatus IS NULL and IR.AuthorizedByStatus Is null And IR.IsApproved = 1) ) AND ISNULL(IR.[Status],'')<>'Posting' ";

                    }
                    else if (GRNbyPOCheckStatus == "Checked")
                    {
                        tempsql = @"AND IR.CheckedByStatus='Checked' And IR.IsApproved = 0 AND ISNULL(IR.[Status],'')<>'Posting' ";
                    }
                    else if (GRNbyPOCheckStatus == "Approved")
                    {
                        tempsql = @"AND IR.AuthorizedByStatus='Approved' And IR.IsApproved = 1 AND ISNULL(IR.[Status],'')<>'Posting' ";
                    }
                    else if (GRNbyPOCheckStatus == "Posted")
                    {
                        tempsql = @"AND IR.AuthorizedByStatus='Approved' And IR.IsApproved = 1 AND ISNULL(IR.[Status],'')='Posting' ";
                    }
                    else if (GRNbyPOCheckStatus == "CheckedHoldReject")
                    {
                        tempsql = @"AND (IR.CheckedByStatus='Hold' OR IR.CheckedByStatus='Reject' And IR.IsApproved = 0 AND ISNULL(IR.[Status],'')<>'Posting') ";

                    }
                    var sql = "";

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
							,PO.UDNo,ISNULL(MLC.OpeningBank,'') OpeningBank,ISNULL(Pr.UserName ,'') CustomerName,OthP.UserName OtherPartyName,IR.OtherPartyId
,IR.OtherPartyPlantId,IR.OtherPartyDocRefNo,IR.OtherPartyRCMApplicable,IR.OtherPartyPlantId OtherInvoicingPartyPlantId
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
                                 LEFT JOIN [HKP].[Party] OthP ON OthP.Id =IR.OtherPartyId 
								 LEFT JOIN dbo.MasterLC MLC ON MLC.Id=CON.MasterLCId
                        WHERE IR.PlantId='" + plantId + @"' 
                         
                        AND IR.OpeningBalanceId IS NULL 
                        AND IR.EmployeeId IS NULL 
                        AND (IR.GRNType='"+ grntype + @"') " + tempsql + @"
						
                        )x
                        ORDER BY 3,2 DESC";


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


            public IEnumerable<object> GetJWGRNDataChecking(string plantId, string GRNbyPOCheckStatus, string POId)

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
									,EI2.SystemId ByWhomEmployeeId,EI2.EmployeeCode EmpCode,IR.TransformationContractId
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
                        and IR.GRNType='GRNBYOS' and IR.TransformationContractId='" + POId + @"'
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
									,EI2.SystemId ByWhomEmployeeId,EI2.EmployeeCode EmpCode,IR.TransformationContractId
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
                        and IR.GRNType='GRNBYOS' and IR.TransformationContractId='" + POId + @"'
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
									,EI2.SystemId ByWhomEmployeeId,EI2.EmployeeCode EmpCode,IR.TransformationContractId
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
                        and IR.GRNType='GRNBYOS' and IR.TransformationContractId='" + POId + @"'
                        )x
                        --where x.TransformationContractId=' " + POId + @"'
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
                        and IR.GRNType='GRNBYOS'
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
								 left JOIN dbo.MasterLC MLC ON MLC.CustomerId=Pr.Id
                        WHERE  IR.CheckedByStatus='Checked'  AND IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL And IR.IsApproved = 0 --And IR.POId Is not NULL  
                         and IR.GRNType='GRNBYOS'  order by IR.GRNDate ASC";


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

            // Job Work

            public IEnumerable<object> GetJobWorkGRNDataChecking(string plantId, string GRNbyPOCheckStatus, string POId)

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
									,EI2.SystemId ByWhomEmployeeId,EI2.EmployeeCode EmpCode,IR.JobWorkContractId
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
                        and IR.GRNType='GRNBYJW' and IR.JobWorkContractId='" + POId + @"'
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
									,EI2.SystemId ByWhomEmployeeId,EI2.EmployeeCode EmpCode,IR.JobWorkContractId
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
                        and IR.GRNType='GRNBYJW' and IR.JobWorkContractId='" + POId + @"'
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
									,EI2.SystemId ByWhomEmployeeId,EI2.EmployeeCode EmpCode,IR.JobWorkContractId
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
                        and IR.GRNType='GRNBYJW' and IR.JobWorkContractId='" + POId + @"'
                        )x
                        --where x.JobWorkContractId=' " + POId + @"'
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

                    var sql = @"DECLARE @inventoryReceiveId VARCHAR(10) = ''
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
									--	,CU.Code AS CurrencyName
									,CUR.Code as CurrencyName
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
								--	,(PID.TransactionQty - IRD.TransactionQty - ISNULL(Pre.OtherReceived, 0)) AS Balance
									,(PID.Quantity - IRD.TransactionQty - ISNULL(Pre.OtherReceived, 0)) AS Balance
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
								--	,MRD.MaterialDetail
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
						--		LEFT JOIN [TRN].[PurchaseOrderDetail] AS PID ON PID.Id = IRD.PODetailsId
								LEFT JOIN dbo.OSTransformationPODetail AS PID ON PID.Id = IRD.OSTransformationPODetailId
								--LEFT JOIN (
								--	SELECT PODetailsId
								--		,Sum(TransactionQty) AS OtherReceived
								--	FROM trn.InventoryReceiveDetail	
								--	GROUP BY PODetailsId
								--	) AS Pre ON pre.PODetailsId = IRD.PODetailsId

									LEFT JOIN (
									SELECT OSTransformationPODetailId
										,Sum(TransactionQty) AS OtherReceived
									FROM trn.InventoryReceiveDetail	
									GROUP BY OSTransformationPODetailId
									) AS Pre ON pre.OSTransformationPODetailId = IRD.OSTransformationPODetailId
								LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
								LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId = IR.Id
								--	LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id
								LEFT JOIN [SCS].[Currency] AS CUR ON CUR.Id=IR.BaseCurrencyId
							--	LEFT JOIN TRN.MaterialRequsitionDetails MRD ON MRD.ID = PID.RequisitionDetailId
                                Left Join SCS.Country C ON C.Id=IM.CountryId
								 LEFT JOIN [TRN].[MasterOrderItem] MOI ON IRD.MasterOrderItemId=MOI.Id
								WHERE IM.MaterialMasterId IS NOT NULL  AND (IRD.MaterialFor='JWOUTPUTMaterial' OR IRD.MaterialFor='JWBYPRODUCTMaterial')
	 

								UNION ALL

								SELECT  IM.Id
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
									--	,CU.Code AS CurrencyName
									,CUR.Code as CurrencyName
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
									--	,(PID.TransactionQty - IRD.TransactionQty - ISNULL(Pre.OtherReceived, 0)) AS Balance
									,(PID.Quantity - IRD.TransactionQty - ISNULL(Pre.OtherReceived, 0)) AS Balance
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
							--		,MRD.MaterialDetail
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
								--		LEFT JOIN [TRN].[PurchaseOrderDetail] AS PID ON PID.Id = IRD.PODetailsId
								LEFT JOIN dbo.OSTransformationPODetail AS PID ON PID.Id = IRD.OSTransformationPODetailId
								--LEFT JOIN (
								--	SELECT PODetailsId
								--		,Sum(TransactionQty) AS OtherReceived
								--	FROM trn.InventoryReceiveDetail	
								--	GROUP BY PODetailsId
								--	) AS Pre ON pre.PODetailsId = IRD.PODetailsId

									LEFT JOIN (
									SELECT OSTransformationPODetailId
										,Sum(TransactionQty) AS OtherReceived
									FROM trn.InventoryReceiveDetail	
									GROUP BY OSTransformationPODetailId
									) AS Pre ON pre.OSTransformationPODetailId = IRD.OSTransformationPODetailId
								LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
								LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId = IR.Id
								--	LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id
								LEFT JOIN [SCS].[Currency] AS CUR ON CUR.Id=IR.BaseCurrencyId
						--		LEFT JOIN TRN.MaterialRequsitionDetails MRD ON MRD.ID = PID.RequisitionDetailId
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

            public IEnumerable<object> JobWorkGRNDetailsData(string inveReveiveId, string POID)

            {
                //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                try
                {

                    var sql = @"DECLARE @inventoryReceiveId VARCHAR(10) = ''
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
									--	,CU.Code AS CurrencyName
									,CUR.Code as CurrencyName
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
								--	,(PID.TransactionQty - IRD.TransactionQty - ISNULL(Pre.OtherReceived, 0)) AS Balance
									,(PID.Quantity - IRD.TransactionQty - ISNULL(Pre.OtherReceived, 0)) AS Balance
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
								--	,MRD.MaterialDetail
                                    ,C.Id,C.UserName CountryName,IRD.GrossAmount,IRD.DiscountAmount,MOI.MasterOrderId MasterOrderNo,IRD.MaterialFor
								    ,MaterialBy=CASE WHEN IRD.MaterialFor='JobWorkOUTPUTMaterial' THEN 'OutPut' WHEN IRD.MaterialFor='JobWorkBYPRODUCTMaterial' THEN 'By Product' END
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
						--		LEFT JOIN [TRN].[PurchaseOrderDetail] AS PID ON PID.Id = IRD.PODetailsId
								LEFT JOIN dbo.JWTransformationPODetail AS PID ON PID.Id = IRD.JWTransformationPODetailId
								--LEFT JOIN (
								--	SELECT PODetailsId
								--		,Sum(TransactionQty) AS OtherReceived
								--	FROM trn.InventoryReceiveDetail	
								--	GROUP BY PODetailsId
								--	) AS Pre ON pre.PODetailsId = IRD.PODetailsId

									LEFT JOIN (
									SELECT JWTransformationPODetailId
										,Sum(TransactionQty) AS OtherReceived
									FROM trn.InventoryReceiveDetail	
									GROUP BY JWTransformationPODetailId
									) AS Pre ON pre.JWTransformationPODetailId = IRD.JWTransformationPODetailId
								LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
								LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId = IR.Id
								--	LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id
								LEFT JOIN [SCS].[Currency] AS CUR ON CUR.Id=IR.BaseCurrencyId
							--	LEFT JOIN TRN.MaterialRequsitionDetails MRD ON MRD.ID = PID.RequisitionDetailId
                                Left Join SCS.Country C ON C.Id=IM.CountryId
								 LEFT JOIN [TRN].[MasterOrderItem] MOI ON IRD.MasterOrderItemId=MOI.Id
								WHERE IM.MaterialMasterId IS NOT NULL  AND (IRD.MaterialFor='JobWorkOUTPUTMaterial' OR IRD.MaterialFor='JobWorkBYPRODUCTMaterial')
	 

								UNION ALL

								SELECT  IM.Id
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
									--	,CU.Code AS CurrencyName
									,CUR.Code as CurrencyName
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
									--	,(PID.TransactionQty - IRD.TransactionQty - ISNULL(Pre.OtherReceived, 0)) AS Balance
									,(PID.Quantity - IRD.TransactionQty - ISNULL(Pre.OtherReceived, 0)) AS Balance
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
							--		,MRD.MaterialDetail
	                                ,C.Id,C.UserName CountryName,IRD.GrossAmount,IRD.DiscountAmount,MOI.MasterOrderId MasterOrderNo,IRD.MaterialFor
									,MaterialBy=CASE WHEN IRD.MaterialFor='JobWorkOUTPUTMaterial' THEN 'OutPut' WHEN IRD.MaterialFor='JobWorkBYPRODUCTMaterial' THEN 'By Product' END
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
								--		LEFT JOIN [TRN].[PurchaseOrderDetail] AS PID ON PID.Id = IRD.PODetailsId
								LEFT JOIN dbo.JWTransformationPODetail AS PID ON PID.Id = IRD.JWTransformationPODetailId
								--LEFT JOIN (
								--	SELECT PODetailsId
								--		,Sum(TransactionQty) AS OtherReceived
								--	FROM trn.InventoryReceiveDetail	
								--	GROUP BY PODetailsId
								--	) AS Pre ON pre.PODetailsId = IRD.PODetailsId

									LEFT JOIN (
									SELECT JWTransformationPODetailId
										,Sum(TransactionQty) AS OtherReceived
									FROM trn.InventoryReceiveDetail	
									GROUP BY JWTransformationPODetailId
									) AS Pre ON pre.JWTransformationPODetailId = IRD.JWTransformationPODetailId
								LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
								LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId = IR.Id
								--	LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id
								LEFT JOIN [SCS].[Currency] AS CUR ON CUR.Id=IR.BaseCurrencyId
						--		LEFT JOIN TRN.MaterialRequsitionDetails MRD ON MRD.ID = PID.RequisitionDetailId
                                Left Join SCS.Country C ON C.Id=IM.CountryId
								 LEFT JOIN [TRN].[MasterOrderItem] MOI ON IRD.MasterOrderItemId=MOI.Id
								WHERE IM.MaterialMasterId IS NULL  AND (IRD.MaterialFor='JobWorkOUTPUTMaterial' OR IRD.MaterialFor='JobWorkBYPRODUCTMaterial')";

                    return _sqlRepository.GetDataCollection(sql);
                }
                catch (Exception ex)
                {
                    throw new CustomException(ex.Message, ex,
                        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
                }
            }

            public IEnumerable<object> GetInventoryReceiveByTransformationContractId(string plantId, string contractId)
            {
                try
                {
                    string sql = @"SELECT Active=CAST(0 AS bit),(ROW_NUMBER() OVER (ORDER BY  IR.Id)) as Rows,null Id,IR.Id InventoryReceiveId
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
									,EI2.SystemId ByWhomEmployeeId,EI2.SystemId EmpCode,IR.JWChangeInInvVoucherId,IR.[Status]
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
                        WHERE IR.PlantId='" + plantId + @"'  --AND ISNULL(IR.[Status],'')='Posting' 
And IR.IsApproved = 1 and IR.GRNType='GRNBYPO' AND IR.TransformationContractId='" + contractId + "'";
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
							DECLARE @inventoryReceiveId VARCHAR(10)='" + POId + @"'
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


            public IEnumerable<object> GetSOWiseMaterialStock(string Material, string Article, string Skuvalue1, string Skuvalue2, string Skuvalue3, string processId, string parameters, string SOMATART, string SalesOrderId)
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string paramter = "";
               

                if (string.IsNullOrEmpty(Skuvalue1) || Skuvalue1 == "null")
                {
                    Skuvalue1 = "";
                }
                if (string.IsNullOrEmpty(Skuvalue2) || Skuvalue2 == "null")
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
						Where IM.MaterialMasterId='" + Material + @"' AND IM.ArticleId='" + Article + @"'
						AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + Skuvalue1 + @"'
						AND ISNULL(IM.SecondCharacteristicsValueId,'')='" + Skuvalue2 + @"'
						AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + Skuvalue3 + @"'
						AND GRNAllocation.SalesOrderId ='" + SalesOrderId + @"'
						Group By GRNAllocation.TransactionUoMId,UOM.UserName,GRNAllocation.SalesOrderId,mm.Id ,mm.UserName ,MMM.Id,MMM.StandardName,FC.Id,FC.UserName,IM.FirstCharacteristicsValueId,isnull(v1.UserName,''),SC.Id,SC.UserName,IM.SecondCharacteristicsValueId,isnull(v2.UserName,''),TC.Id,TC.UserName,IM.ThirdCharacteristicsValueId,isnull(v3.UserName,'')";// ,MMAU.BaseUOMFactor
                    return _sqlRepository.GetDataCollection(sql);

                }
                catch (Exception ex)
                {
                    throw new CustomException(ex.Message, ex,
                        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
                }
            }


            public IEnumerable<object> GetJWReceiptDataForAllocation()
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

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
				left join [dbo].[JWPOBOQMAP] POBOQMAP ON POBOQMAP.JWPODetailId =IRD.OSTransformationPODetailId
				left join BOQ Boq on Boq.Id=POBOQMAP.BOQDetailId
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
				WHERE IR.GRNType='GRNBYPO' and Boq.SalesOrderId IS NOT NULL Order by IRD.Id ASC";// ,MMAU.BaseUOMFactor
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
