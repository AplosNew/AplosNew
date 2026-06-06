#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.OrderManagements;
using Library.Model.Systems;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.OrderManagements
{
    public class ProductionOrderService : Service<ProductionOrder>, IProductionOrderService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<ProductionOrderDetail> _detailsRepository;
        private readonly IRepositoryAsync<ProductionOrderProcessSet> _processSetRepository;
        private readonly IRepositoryAsync<ProductionOrderEntity> _entityRepository;
        private readonly IRepositoryAsync<ProductionOrderWorkCenter> _workCenterRepository;
        private readonly IRepositoryAsync<ProductionOrderFirstProcessWorkCenter> _fpworkCenterRepository;
        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        public ProductionOrderService(
            IRepositoryAsync<ProductionOrder> baseRepository
            , IRepositoryAsync<ProductionOrderDetail> detailsRepository
            , IRepositoryAsync<ProductionOrderProcessSet> processSetRepository
            , IRepositoryAsync<ProductionOrderEntity> entityRepository
            , IRepositoryAsync<ProductionOrderWorkCenter> workCenterRepository
            , IRepositoryAsync<ProductionOrderFirstProcessWorkCenter> fpworkCenterRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(baseRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _detailsRepository = detailsRepository;
            _processSetRepository = processSetRepository;
            _entityRepository = entityRepository;
            _workCenterRepository = workCenterRepository;
            _fpworkCenterRepository = fpworkCenterRepository;
            _sqlRepository = sqlRepository;
            _pkGeneratorService = pkGeneratorService;
        }

        #endregion Constructor

        private string GetPK() => GetAutoNumber(nameof(ProductionOrder), PKGeneratorEnum.Auto, null, DateTime.Now);

        public GridModel Query(GridParameter parameters, string plantId)
        {
            parameters.CmdText = @"SELECT PO.*,s.UserName AS ProductionStatus, EN.UserName AS EntityName, PS.UserName AS ProductionStatusName
                                FROM [TRN].[ProductionOrder] AS PO
                            JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                            LEFT JOIN [HKP].[ProductionStatus] AS PS ON PO.EntityId = PS.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                            WHERE PO.PlantId='" + plantId + "'";
            return _sqlRepository.GetGridData(parameters);
        }

        private string GetPadding(string iv)
        {
            while (iv.Length < bplib.clsWebLib.PrOId)
            {
                iv = DateTime.Now.ToString("yy")+ "000000" + iv;
            }
            return iv;
        }

        private PKGenerator GetMaxNumber()
        {
            return base.GetMaxNumber(nameof(ProductionOrder), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }
        public void InsertGraph(ProductionOrder master, IEnumerable<ProductionOrderDetail> detaillist
            , IEnumerable<ProductionOrderProcessSet> processSetlist
            , IEnumerable<ProductionOrderEntity> entitylist
            , IEnumerable<ProductionOrderWorkCenter> workcenterlist,
            DataTable Runningworkcenterlist)
        {
            var flag = false;
            try
            {
                //old
                string systemid = "";
                bplib.clsGenID objID = new bplib.clsGenID();
                objID.GenHRID(System.DateTime.Now.ToShortDateString(), "PRODUCTION ORDER", out systemid);

                //systemid = GetAutoNumber(nameof(ProductionOrder), PKGeneratorEnum.Auto, null, DateTime.Now);


                // new 
                //bplib.clsGenID objID = new bplib.clsGenID();
                //objID.GenerateIDMaxYearly(DateTime.Now.ToShortDateString().ToString(), "ProductionOrder", out string systemid);
                //master.Id = DateTime.Now.ToString("yy")+ systemid.ToString().PadLeft(4,"0".ToString()[0]);
                master.Id = systemid;
                base.InsertGraph(master);
                InsertUpdateOrDeleteGraph(master.Id, detaillist);
                InsertUpdateOrDeleteGraph(master.Id, processSetlist);
                InsertUpdateOrDeleteGraph(master.Id, entitylist);
                //InsertUpdateOrDeleteGraph(master.Id, workcenterlist);
                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                updateRunningOrder(Runningworkcenterlist, master.Id);

                
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void UpdateGraph(ProductionOrder master, IEnumerable<ProductionOrderDetail> detaillist
            , IEnumerable<ProductionOrderProcessSet> processSetlist
            , IEnumerable<ProductionOrderEntity> entitylist
            , IEnumerable<ProductionOrderWorkCenter> workcenterlist,
            DataTable Runningworkcenterlist
            , IEnumerable<ProductionOrderFirstProcessWorkCenter> fpworkcenterlist)
        {
            var flag = false;
            try
            {
                updateProductionOrderStatus(master, master.Id);
                base.Update(master);
                InsertUpdateOrDeleteGraph(master.Id, detaillist);
                InsertUpdateOrDeleteGraph(master.Id, processSetlist);
                InsertUpdateOrDeleteGraph(master.Id, entitylist);
                 InsertUpdateOrDeleteGraph(master.Id, workcenterlist);
                InsertUpdateOrDeleteGraphPFPWC(master.Id, fpworkcenterlist);
                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                updateRunningOrder(Runningworkcenterlist, master.Id);

                
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }


        private void updateRunningOrder(DataTable dtRunningWcPreference, string ProductionOrderID)
        {
            try
            {
                DataSet dsRunningWorkcenter = null;
                string sql = "SELECT * FROM [TRN].[RunningOrderWorkCenter] where ProductionOrderID='" + ProductionOrderID + "'";
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsRunningWorkcenter, false, "1");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string SystemID = "";
                for (int i = 0; i < dtRunningWcPreference.Rows.Count; i++)
                {
                    dsRunningWorkcenter.Tables[0].DefaultView.RowFilter = "WorkcenterMasterID='" + dtRunningWcPreference.Rows[i]["WorkCenterMasterId"].ToString() + "'";
                    if (dsRunningWorkcenter.Tables[0].DefaultView.Count == 0)
                    {
                        if (SystemID == "")
                        {
                            bplib.clsGenID id = new bplib.clsGenID();
                            id.GenID(System.DateTime.Now.ToShortDateString(), "PRUNNING WC", out SystemID);
                        }
                        DataRow dr = dsRunningWorkcenter.Tables[0].NewRow();

                        dr["id"] = SystemID + "-" + (i + 1).ToString();
                        dr["ProductionOrderID"] = ProductionOrderID;
                        dr["WorkcenterMasterID"] = dtRunningWcPreference.Rows[i]["WorkCenterMasterId"].ToString();
                        dr["isResidualApplicable"] = false;
                        dr["Qty"] = 0;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dsRunningWorkcenter.Tables[0].Rows.Add(dr);
                    }
                }




                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsRunningWorkcenter);

            }
            catch (Exception ex)
            {

                throw (ex);
            }


        }
        private void updateProductionOrderStatus(ProductionOrder Prod, string ProductionOrderID)
        {
            try
            {
                DataSet dsRunningWorkcenter = null;
                string sql = "SELECT * FROM [TRN].ProductionOrder where ID='" + ProductionOrderID + "'";
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsRunningWorkcenter, false, "1");

                if (dsRunningWorkcenter.Tables[0].Rows.Count == 0)
                    return;


                if (dsRunningWorkcenter.Tables[0].Rows[0]["EntityId"].ToString().ToUpper() == Prod.EntityId.ToUpper())
                    return;



                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();


                objCon.ExecuteNonQueryWrapper("Delete from trn.ProductionOrderWorkCenter where ProductionOrderId='" + ProductionOrderID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete from trn.RunningOrderWorkCenter where ProductionOrderId='" + ProductionOrderID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete from ProductionPlanningType1 where ProductionOrderId='" + ProductionOrderID + "'", true, "1");

                objCon.ExecuteNonQueryWrapper(@"UPDATE trn.productionorder SET ProductionStatusId = (SELECT ps.Id FROM hkp.ProductionStatus AS ps WHERE ps.UserName='Active')
                                                        FROM trn.ProductionOrder AS po
                                                        INNER JOIN hkp.ProductionStatus PS ON ps.id=po.ProductionStatusId
                                                        WHERE po.Id='" + ProductionOrderID + @"' AND ps.UserName='Running'", true, "1");

                objCon.CommitTransaction();


                sql = "SELECT ps.Id FROM hkp.ProductionStatus AS ps WHERE ps.UserName='Active'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsRunningWorkcenter, false, "1");

                Prod.ProductionStatusId = dsRunningWorkcenter.Tables[0].Rows[0]["Id"].ToString();
            }
            catch (Exception ex)
            {

                throw (ex);
            }


        }

        public void DeleteGraph(string id)
        {
            var flag = false;
            try
            {
                var entity = base.Find(id);
                DetailDeleteGraph(id);
                ProcessSetDeleteGraph(id);
                EntityDeleteGraph(id);
                WorkCenterDeleteGraph(id);
                base.DeleteGraph(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        #region -- Production Order Details

        public IEnumerable<object> GetSalesOrderList(GridParameter parameters, string PlantId)
        {
            try
            {
                var sql = @"SELECT ROW_NUMBER() OVER (ORDER BY MasterOrderItemId) AS RN,0 AS Checked
	                            , MOI.MasterOrderId, MO.MasterOrderNo, SO.MasterOrderItemId
	                            , SO.Id AS SalesOrderId, P.UserName AS Customer
	                            , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName
	                            , MOI.ArticleId, ART.StandardName AS ArticleName
	                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), DeliveryDate, 106),' ','-')
	                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), CommitmentDate, 106),' ','-')
	                            , DEST.UserName AS DestinationName, SHP.UserName AS ShipmentModeName
	                            , PO.PONumber, OS.UserName AS OrderStatusName, OC.UserName AS OrderCategoryName
	                            , SO.Qty, SO.Rate,SO.Description
	                            , Flag = CAST(0 AS BIT)
                       FROM [TRN].[SalesOrder] AS SO 
                       JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                       JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
                       LEFT JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id 
                       LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id
                       LEFT JOIN [HKP].[Party] AS P ON MO.PartyId = P.Id
                       LEFT JOIN [MST].[Destination] AS DEST ON SO.DestinationId = DEST.Id
                       LEFT JOIN [MST].[ShipMode] AS SHP ON SO.ShipmentModeId = SHP.Id
                       LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                       LEFT JOIN [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
                       LEFT JOIN [HKP].[OrderCategory] AS OC ON SO.OrderCategoryId = OC.Id
                       WHERE MO.PlantId='" + PlantId + "' " +
                       "AND (OS.Id='" + Library.Model.Enums.OrderStatusEnum.Active.ToString() + @"' OR  SO.Id IN (SELECT DISTINCT SalesOrderId FROM [TRN].[ProductionOrderDetail])) AND MOI.ArticleId<>''" +
                       " ORDER BY  MOI.MaterialMasterId,MOI.ArticleId";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)

            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public IEnumerable<object> GetProductionRecipeMaterialList(string productionOrderId)
        {
            try
            {
                //var _sql = @"SELECT POD.Id,0 AS Checked, POD.ProductionOrderId, POD.SalesOrderId
	               //             --, RM.Id AS RecipeMaterialId
	               //             , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName
	               //             , MOI.ArticleId, ART.StandardName AS ArticleName
	               //             , DeliveryDate = REPLACE(CONVERT(CHAR(11), DeliveryDate, 106),' ','-')
                //                , LSD = REPLACE(CONVERT(CHAR(11), LSD, 106),' ','-')
	               //             , CommitmentDate = REPLACE(CONVERT(CHAR(11), CommitmentDate, 106),' ','-')
	               //             , DEST.UserName AS DestinationName, SHP.UserName AS ShipmentModeName
	               //             , PO.PONumber, OS.UserName AS OrderStatusName, OC.UserName AS OrderCategoryName
	               //             , SO.Qty, SO.CM, SO.Rate,SO.Description
	               //             , Flag = CAST(0 AS BIT),SO.DestinationDescription
                //            FROM [TRN].[ProductionOrderDetail] AS POD
                //            LEFT JOIN [TRN].[SalesOrder] AS SO ON POD.SalesOrderId=SO.Id
                //            LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
                //            --LEFT JOIN [TRN].[RecipeMaterial] AS RM ON RM.MaterialMasterId = MOI.MaterialMasterId AND RM.ArticleId = MOI.ArticleId
                //            JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id --RM.MaterialMasterId = MM.Id AND 
                //            JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id --RM.ArticleId = ART.Id AND 
                //            LEFT JOIN [MST].[Destination] AS DEST ON SO.DestinationId = DEST.Id
                //            LEFT JOIN [MST].[ShipMode] AS SHP ON SO.ShipmentModeId = SHP.Id
                //            LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                //            LEFT JOIN [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
                //            LEFT JOIN [HKP].[OrderCategory] AS OC ON SO.OrderCategoryId = OC.Id
                //            WHERE POD.ProductionOrderId = '" + productionOrderId + "'";

              var  _sql = @"SELECT ROW_NUMBER() OVER (ORDER BY MasterOrderItemId) AS RN
                                ,MO.Type,isnull(moi.Consignment,0) AS Consignment
                                ,CASE WHEN ISNULL(eout.Id,'')<>'' OR ISNULL(TOUT.Id,'')<>'' THEN CONCAT(POWN.UserName,'(',EOWN.UserName,')') ELSE '' END AS OrderOwner

	                            ,POD.Id, POD.ProductionOrderId, MOI.MasterOrderId, MO.MasterOrderNo, SO.MasterOrderItemId
	                            , SO.Id AS SalesOrderId,SO.Id SONo, P.UserName AS Customer,B.UserName AS Buyer,PM.Id AS ProductID,isnull(MOI.ProductionGrouping,'') AS ProductionGrouping
	                            , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName,PM.UserName AS ProductName,SO.LineItemReference
	                            , MOI.ArticleId, ART.StandardName AS ArticleName,MOI.BuyerReferenceNo,MOI.OwnReferenceNo,MO.BuyerReferenceNo AS BuyerOrderNo,MO.OwnReferenceNo AS OwnOrderNo
	                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), DeliveryDate, 106),' ','-')
	                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), CommitmentDate, 106),' ','-')
                                , LSD = REPLACE(CONVERT(CHAR(11), SO.LSD, 106),' ','-')
	                            , isnull(DEST.UserName,'') AS DestinationName, isnull(SHP.UserName,'') AS ShipmentModeName
	                            , isnull(PO.PONumber,'') AS PONumber, OS.UserName AS OrderStatusName, OC.UserName AS OrderCategoryName
	                            , SO.Qty, SO.CM, SO.Rate,ISNULL(SO.Description,'')Description
	                            , Flag = CAST(0 AS BIT),ISNULL(SO.DestinationDescription,'')DestinationDescription
								--,ISNULL(fc.CharacteristicsValueId,'') FirstCharacteristicsValueId,ISNULL(sc.CharacteristicsValueId,'') SecondCharacteristicsValueId
								--,ISNULL(tc.CharacteristicsValueId,'') ThirdCharacteristicsValueId,
                              -- ISNULL(c1.UserName,'') AS FirstCharacteristics,ISNULL(cv1.UserName,'') AS FirstCharacteristicsValue,
                                --ISNULL(c2.UserName,'') AS SecondCharacteristics,ISNULL(cv2.UserName,'') AS SecondCharacteristicsValue,
                                --ISNULL(c3.UserName,'') AS ThirdCharacteristics,ISNULL(cv3.UserName,'') AS ThirdCharacteristicsValue
                       FROM 
                       [TRN].[SalesOrder] AS SO 
                       JOIN [TRN].[ProductionOrderDetail] AS POD ON pod.SalesOrderId=so.Id
                       JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                       JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
                       LEFT JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id 
                       LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id
					   LEFT JOIN trn.ProductDefinition AS pd ON pd.MaterialMasterId=moi.MaterialMasterId
					   LEFT JOIN [MST].[ProductMaster] PM ON pm.Id=pd.ProductMasterId
                       LEFT JOIN [HKP].[Party] AS P ON MO.PartyId = P.Id
					   LEFT JOIN HKP.BUYER b on b.Id=MO.BuyerId
                       LEFT JOIN [MST].[Destination] AS DEST ON SO.DestinationId = DEST.Id
                       LEFT JOIN [MST].[ShipMode] AS SHP ON SO.ShipmentModeId = SHP.Id
                       LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                       LEFT JOIN [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
                       LEFT JOIN [HKP].[OrderCategory] AS OC ON SO.OrderCategoryId = OC.Id
					   --LEFT JOIN trn.FirstCharacteristics AS fc ON fc.SalesOrderId=so.Id


							LEFT JOIN org.Entity AS EOUT ON EOUT.Id=ISNULL(moi.EntityIdWithinCompany,moi.EntityIdWithinGroup)
							LEFT JOIN org.Plant AS POUT ON POUT.Id=EOUT.PlantId
							LEFT JOIN hkp.Party AS TOUT ON tout.Id=moi.PartyId
							LEFT JOIN org.Plant AS POWN ON POWN.Id=MO.PlantId
							LEFT JOIN org.Entity AS EOWN ON EOWN.Id=MO.EntityId
                       --LEFT JOIN trn.SecondCharacteristics AS sc ON sc.FirstCharacteristicsId=fc.Id AND sc.SalesOrderId=so.Id
                       --LEFT JOIN trn.ThirdCharacteristics AS tc ON tc.SecondCharacteristicsId=sc.Id AND tc.SalesOrderId=so.Id

                       --LEFT JOIN hkp.CharacteristicsValue AS cv1 ON cv1.Id=fc.CharacteristicsValueId
                       --LEFT JOIN hkp.Characteristics AS c1 ON c1.Id=cv1.CharacteristicsId

                       --LEFT JOIN hkp.CharacteristicsValue AS cv2 ON cv2.Id=sc.CharacteristicsValueId
                      -- LEFT JOIN hkp.Characteristics AS c2 ON c2.Id=cv2.CharacteristicsId

                       --LEFT JOIN hkp.CharacteristicsValue AS cv3 ON cv3.Id=tc.CharacteristicsValueId
                       --LEFT JOIN hkp.Characteristics AS c3 ON c3.Id=cv3.CharacteristicsId

                      WHERE POD.ProductionOrderId = '" + productionOrderId + "'" +
                        "ORDER BY MOI.MATERIALMASTERID,MOI.ArticleID";

                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)

            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public IEnumerable<object> GetProductionOrderType2MaterialList(string productionOrderId)
        {
            try
            {
                
                var _sql = @"SELECT ROW_NUMBER() OVER (ORDER BY MasterOrderItemId) AS RN
                                ,MO.Type,isnull(moi.Consignment,0) AS Consignment
                                ,CASE WHEN ISNULL(eout.Id,'')<>'' OR ISNULL(TOUT.Id,'')<>'' THEN CONCAT(POWN.UserName,'(',EOWN.UserName,')') ELSE '' END AS OrderOwner

	                            ,POD.Id, POD.ProductionOrderId, MOI.MasterOrderId, MO.MasterOrderNo, SO.MasterOrderItemId
	                            , SO.Id AS SalesOrderId,SO.Id SONo, P.UserName AS Customer,B.UserName AS Buyer,PM.Id AS ProductID,isnull(MOI.ProductionGrouping,'') AS ProductionGrouping
	                            , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName,PM.UserName AS ProductName
	                            , MOI.ArticleId, ART.StandardName AS ArticleName,MOI.BuyerReferenceNo,MOI.OwnReferenceNo,MO.BuyerReferenceNo AS BuyerOrderNo,MO.OwnReferenceNo AS OwnOrderNo
	                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), DeliveryDate, 106),' ','-')
	                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), CommitmentDate, 106),' ','-')
                                , LSD = REPLACE(CONVERT(CHAR(11), SO.LSD, 106),' ','-')
	                            , isnull(DEST.UserName,'') AS DestinationName, isnull(SHP.UserName,'') AS ShipmentModeName
	                            , isnull(PO.PONumber,'') AS PONumber, OS.UserName AS OrderStatusName, OC.UserName AS OrderCategoryName
	                            , SO.Qty, SO.CM, SO.Rate,ISNULL(SO.Description,'')Description
	                            , Flag = CAST(0 AS BIT),ISNULL(SO.DestinationDescription,'')DestinationDescription
								--,ISNULL(fc.CharacteristicsValueId,'') FirstCharacteristicsValueId,ISNULL(sc.CharacteristicsValueId,'') SecondCharacteristicsValueId
								--,ISNULL(tc.CharacteristicsValueId,'') ThirdCharacteristicsValueId,
                              -- ISNULL(c1.UserName,'') AS FirstCharacteristics,ISNULL(cv1.UserName,'') AS FirstCharacteristicsValue,
                                --ISNULL(c2.UserName,'') AS SecondCharacteristics,ISNULL(cv2.UserName,'') AS SecondCharacteristicsValue,
                                --ISNULL(c3.UserName,'') AS ThirdCharacteristics,ISNULL(cv3.UserName,'') AS ThirdCharacteristicsValue
                       FROM 
                       [TRN].[SalesOrder] AS SO 
                       JOIN [TRN].[ProductionOrderType2Detail] AS POD ON pod.SalesOrderId=so.Id
                       JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                       JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
                       LEFT JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id 
                       LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id
					   LEFT JOIN trn.ProductDefinition AS pd ON pd.MaterialMasterId=moi.MaterialMasterId
					   LEFT JOIN [MST].[ProductMaster] PM ON pm.Id=pd.ProductMasterId
                       LEFT JOIN [HKP].[Party] AS P ON MO.PartyId = P.Id
					   LEFT JOIN HKP.BUYER b on b.Id=MO.BuyerId
                       LEFT JOIN [MST].[Destination] AS DEST ON SO.DestinationId = DEST.Id
                       LEFT JOIN [MST].[ShipMode] AS SHP ON SO.ShipmentModeId = SHP.Id
                       LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                       LEFT JOIN [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
                       LEFT JOIN [HKP].[OrderCategory] AS OC ON SO.OrderCategoryId = OC.Id
					   --LEFT JOIN trn.FirstCharacteristics AS fc ON fc.SalesOrderId=so.Id


							LEFT JOIN org.Entity AS EOUT ON EOUT.Id=ISNULL(moi.EntityIdWithinCompany,moi.EntityIdWithinGroup)
							LEFT JOIN org.Plant AS POUT ON POUT.Id=EOUT.PlantId
							LEFT JOIN hkp.Party AS TOUT ON tout.Id=moi.PartyId
							LEFT JOIN org.Plant AS POWN ON POWN.Id=MO.PlantId
							LEFT JOIN org.Entity AS EOWN ON EOWN.Id=MO.EntityId
                       --LEFT JOIN trn.SecondCharacteristics AS sc ON sc.FirstCharacteristicsId=fc.Id AND sc.SalesOrderId=so.Id
                       --LEFT JOIN trn.ThirdCharacteristics AS tc ON tc.SecondCharacteristicsId=sc.Id AND tc.SalesOrderId=so.Id

                       --LEFT JOIN hkp.CharacteristicsValue AS cv1 ON cv1.Id=fc.CharacteristicsValueId
                       --LEFT JOIN hkp.Characteristics AS c1 ON c1.Id=cv1.CharacteristicsId

                       --LEFT JOIN hkp.CharacteristicsValue AS cv2 ON cv2.Id=sc.CharacteristicsValueId
                      -- LEFT JOIN hkp.Characteristics AS c2 ON c2.Id=cv2.CharacteristicsId

                       --LEFT JOIN hkp.CharacteristicsValue AS cv3 ON cv3.Id=tc.CharacteristicsValueId
                       --LEFT JOIN hkp.Characteristics AS c3 ON c3.Id=cv3.CharacteristicsId

                      WHERE POD.ProductionOrderId = '" + productionOrderId + "'" +
                          "ORDER BY MOI.MATERIALMASTERID,MOI.ArticleID";

                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)

            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }
        private void InsertUpdateOrDeleteGraph(string masterId, IEnumerable<ProductionOrderDetail> entities)
        {
            try
            {
                if (entities != null)
                {
                    var count = _detailsRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[ProductionOrderDetail] WHERE ProductionOrderId='{masterId}'").First();

                    string _newId = "";


                    foreach (var item in entities)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            count++;
                            if (_newId == "")
                            {
                                bplib.clsGenID _tempid = new bplib.clsGenID();
                                _tempid.GenID("[TRN].[ProductionOrderDetail]", out _newId);
                            }
                            item.Id = "D" + masterId + (_newId) + count.ToString(); //MakePK(masterId, count, 2);
                            item.ProductionOrderId = masterId;
                            AuditService.AddedLog(item);
                            _detailsRepository.Insert(item);
                        }
                        else
                        {
                            _detailsRepository.Update(item);
                        }

                    }
               
                var dbList = _detailsRepository.Query(t => t.ProductionOrderId == masterId).Select().ToList();
                if (dbList.IsNotNull() && dbList.Count > 0)
                {
                    //if (entities == null)
                    //{
                    //    foreach (var item in dbList)
                    //    {
                    //        _detailsRepository.Delete(item);
                    //    }
                    //}
                    //else
                    //{
                    foreach (var item in dbList)
                    {
                        if (!entities.Any(t => t.Id == item.Id))
                        {
                            _detailsRepository.Delete(item);
                        }
                    }
                        //}
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void DetailDeleteGraph(string masterId)
        {
            try
            {
                var dbList = _detailsRepository.Query(t => t.ProductionOrderId == masterId).Select().ToList();
                if (dbList.IsNotNull() && dbList.Count > 0)
                {
                    foreach (var item in dbList)
                    {
                        _detailsRepository.Delete(item);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion -- Production Order Details

        #region -- Production Order Process Set

        public IEnumerable<object> GetProductionOrderProcessSetList(string productionOrderId)
        {
            try
            {
                var _sql = @"SELECT MMPS.Id
		                        , MMPS.ProductionOrderId
		                        , MMPS.ProcessId, p.UserName AS ProcessName
		                        , MMPS.[Sequence], MMPS.IsBaseProcess, MMPS.[Days], MMPS.Symbol
		                        , MMPS.ProductionCycleTime, MMPS.JobWorkApplicable, MMPS.JobWorkType
		                        , MMPS.EntityIdWithinCompany, MMPS.EntityIdWithinGroup, MMPS.PartyId
		                        , EntityOrVendorName= CASE WHEN MMPS.EntityIdWithinCompany<>'' THEN EWC.UserName 
					                        WHEN MMPS.EntityIdWithinGroup<>'' THEN EWG.UserName
					                        WHEN MMPS.PartyId<>'' THEN PRT.UserName
					                        ELSE PRT.UserName END
                                , MMPS.MaterialMasterId, MM.UserName AS MaterialMasterName
	                            , MMPS.ArticleId, ART.StandardName AS ArticleName,MMPS.Qty,MMPS.UOMId,MMPS.ProductionBookingLevel
                                ,RelaySequence=CASE WHEN MMPS.RelaySequence=0 THEN P.Sequence ELSE MMPS.RelaySequence END,MMPS.IsInventory,MMPS.IsProductionVerification
                        FROM [TRN].[ProductionOrderProcessSet] AS MMPS
                        LEFT JOIN HKP.Process AS P ON MMPS.ProcessId=P.Id
                        LEFT JOIN ORG.Entity AS EWC ON MMPS.EntityIdWithinCompany=EWC.Id
                        LEFT JOIN ORG.Entity AS EWG ON MMPS.EntityIdWithinGroup=EWG.Id
                        LEFT JOIN HKP.Party AS PRT ON MMPS.PartyId=PRT.Id
                        LEFT JOIN MST.MaterialMaster AS MM ON MMPS.MaterialMasterId=MM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON MMPS.ArticleId=ART.Id
						LEFT OUTER JOIN [SCS].[UnitOfMeasurement] UOM ON uom.Id=MMPS.UOMId
                        WHERE MMPS.ProductionOrderId='" + productionOrderId + "' ORDER BY MMPS.[Sequence]";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)

            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public IEnumerable<object> GetProductionOrderType2ProcessSetList(string productionOrderId)
        {
            try
            {
                var _sql = @"SELECT MMPS.Id
		                        , MMPS.ProductionOrderId
		                        , MMPS.ProcessId, p.UserName AS ProcessName
		                        , MMPS.[Sequence], MMPS.IsBaseProcess, MMPS.[Days], MMPS.Symbol
		                        , MMPS.ProductionCycleTime, MMPS.JobWorkApplicable, MMPS.JobWorkType
		                        , MMPS.EntityIdWithinCompany, MMPS.EntityIdWithinGroup, MMPS.PartyId
		                        , EntityOrVendorName= CASE WHEN MMPS.EntityIdWithinCompany<>'' THEN EWC.UserName 
					                        WHEN MMPS.EntityIdWithinGroup<>'' THEN EWG.UserName
					                        WHEN MMPS.PartyId<>'' THEN PRT.UserName
					                        ELSE PRT.UserName END
                                , MMPS.MaterialMasterId, MM.UserName AS MaterialMasterName
	                            , MMPS.ArticleId, ART.StandardName AS ArticleName,MMPS.Qty,MMPS.UOMId,MMPS.ProductionBookingLevel
                                ,RelaySequence=CASE WHEN MMPS.RelaySequence=0 THEN P.Sequence ELSE MMPS.RelaySequence END,MMPS.IsInventory,MMPS.IsProductionVerification
                        FROM [TRN].[ProductionOrderType2ProcessSet] AS MMPS
                        LEFT JOIN HKP.Process AS P ON MMPS.ProcessId=P.Id
                        LEFT JOIN ORG.Entity AS EWC ON MMPS.EntityIdWithinCompany=EWC.Id
                        LEFT JOIN ORG.Entity AS EWG ON MMPS.EntityIdWithinGroup=EWG.Id
                        LEFT JOIN HKP.Party AS PRT ON MMPS.PartyId=PRT.Id
                        LEFT JOIN MST.MaterialMaster AS MM ON MMPS.MaterialMasterId=MM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON MMPS.ArticleId=ART.Id
						LEFT OUTER JOIN [SCS].[UnitOfMeasurement] UOM ON uom.Id=MMPS.UOMId
                        WHERE MMPS.ProductionOrderId='" + productionOrderId + "' ORDER BY MMPS.[Sequence]";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)

            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private void InsertUpdateOrDeleteGraph(string masterId, IEnumerable<ProductionOrderProcessSet> entities)
        {
            try
            {
                if (entities != null)
                {
                    var count = _processSetRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[ProductionOrderProcessSet] WHERE ProductionOrderId='{masterId}'").First();
                    int sequence = 0;
                    foreach (var item in entities)
                    {
                        sequence++;
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            count++;
                            item.Id = MakePK(masterId, count, 2);
                            item.ProductionOrderId = masterId;
                            item.Sequence = sequence;
                            AuditService.AddedLog(item);
                            _processSetRepository.Insert(item);
                        }
                        else
                        {
                            item.Sequence = sequence;
                            _processSetRepository.Update(item);
                        }

                    }
               
                var dbList = _processSetRepository.Query(t => t.ProductionOrderId == masterId).Select().ToList();
                if (dbList.IsNotNull() && dbList.Count > 0)
                {
                    //if (entities == null)
                    //{
                    //    foreach (var item in dbList)
                    //    {
                    //        _processSetRepository.Delete(item);
                    //    }
                    //}
                    //else
                    //{
                        foreach (var item in dbList)
                        {
                            if (!entities.Any(t => t.Id == item.Id))
                            {
                                _processSetRepository.Delete(item);
                            }
                        }
                        //}
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ProcessSetDeleteGraph(string masterId)
        {
            try
            {
                var dbList = _processSetRepository.Query(t => t.ProductionOrderId == masterId).Select().ToList();
                if (dbList.IsNotNull() && dbList.Count > 0)
                {
                    foreach (var item in dbList)
                    {
                        _processSetRepository.Delete(item);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region -- Production Order Entity

        public IEnumerable<object> GetProductionOrderEntityList(string productionOrderId)
        {
            try
            {
                var _sql = @"SELECT POEN.Id, POEN.ProductionOrderId, POEN.EntityId
                            FROM [TRN].[ProductionOrderEntity] AS POEN
                            JOIN [ORG].[Entity] AS EN ON POEN.EntityId = EN.Id
                            WHERE POEN.ProductionOrderId='" + productionOrderId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)

            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private void InsertUpdateOrDeleteGraph(string masterId, IEnumerable<ProductionOrderEntity> entities)
        {
            try
            {
                if (entities != null)
                {
                    var count = _entityRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[ProductionOrderEntity] WHERE ProductionOrderId='{masterId}'").First();

                    foreach (var item in entities)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            count++;
                            item.Id = MakePK(masterId, count, 2);
                            item.ProductionOrderId = masterId;
                            AuditService.AddedLog(item);
                            _entityRepository.Insert(item);
                        }
                        else
                        {
                            _entityRepository.Update(item);
                        }

                    }
                }
                var dbList = _entityRepository.Query(t => t.ProductionOrderId == masterId).Select().ToList();
                if (dbList.IsNotNull() && dbList.Count > 0)
                {
                    if (entities == null)
                    {
                        foreach (var item in dbList)
                        {
                            _entityRepository.Delete(item);
                        }
                    }
                    else
                    {
                        foreach (var item in dbList)
                        {
                            if (!entities.Any(t => t.Id == item.Id))
                            {
                                _entityRepository.Delete(item);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void EntityDeleteGraph(string masterId)
        {
            try
            {
                var dbList = _entityRepository.Query(t => t.ProductionOrderId == masterId).Select().ToList();
                if (dbList.IsNotNull() && dbList.Count > 0)
                {
                    foreach (var item in dbList)
                    {
                        _entityRepository.Delete(item);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region -- Production Order Work Center

        public IEnumerable<object> GetWorkCenterList(string[] entityIds, string processid)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //var _sql = @"SELECT WCM.Id AS WorkCenterMasterId, NULL AS ProductionOrderId,e.UserName AS Entity,p.UserName AS Plant
                //              , WCM.EntityId, WCM.Code, WCM.UserName, Flag = Convert(bit,0)
                //            FROM SCS.WorkCenterMaster AS WCM
                //            INNER JOIN org.Entity AS e ON e.Id=wcm.EntityId
                //            INNER JOIN org.Plant AS p ON p.Id=wcm.PlantId
                //            WHERE WCM.ProcessID='" + processid + @"' AND WCM.CompanyId='" + identity.CompanyId + "' order by p.userName, e.UserName,WCM.sequence";

                var _sql = @"SELECT WCM.Id AS WorkCenterMasterId, NULL AS ProductionOrderId,e.UserName AS Entity,p.UserName AS Plant
	                             , WCM.EntityId, WCM.Code, WCM.UserName, Flag = Convert(bit,0)
                            FROM SCS.WorkCenterMaster AS WCM
                            INNER JOIN org.Entity AS e ON e.Id=wcm.EntityId
                            INNER JOIN org.Plant AS p ON p.Id=wcm.PlantId
                            WHERE WCM.ProcessID='" + processid + @"' AND  WCM.EntityId IN(" + ReturnStringArray(entityIds) + ") order by p.userName, e.UserName,WCM.sequence";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)

            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public IEnumerable<object> GetProductionOrderWorkCenterList(string productionOrderId)
        {

            try
            {
                var _sql = @"SELECT PWCM.Id,e.UserName AS Entity,p.UserName AS Plant, PWCM.ProductionOrderId, PWCM.WorkCenterMasterId, WCM.Code, WCM.UserName
                                FROM [TRN].[ProductionOrderWorkCenter] AS PWCM
                                JOIN [SCS].[WorkCenterMaster] AS WCM ON PWCM.WorkCenterMasterId = WCM.Id
                                INNER JOIN org.Entity AS e ON e.Id=wcm.EntityId
                                INNER JOIN org.Plant AS p ON p.Id=wcm.PlantId
                                WHERE PWCM.ProductionOrderId='" + productionOrderId + "' ORDER BY p.UserName,e.UserName,wcm.sequence";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)

            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
            return null;
        }

        public IEnumerable<object> GetProductionOrderType2WorkCenterList(string productionOrderId)
        {

            try
            {
                var _sql = @"SELECT PWCM.Id,e.UserName AS Entity,p.UserName AS Plant, PWCM.ProductionOrderId, PWCM.WorkCenterMasterId, WCM.Code, WCM.UserName
                                FROM [TRN].[ProductionOrderType2WorkCenter] AS PWCM
                                JOIN [SCS].[WorkCenterMaster] AS WCM ON PWCM.WorkCenterMasterId = WCM.Id
                                INNER JOIN org.Entity AS e ON e.Id=wcm.EntityId
                                INNER JOIN org.Plant AS p ON p.Id=wcm.PlantId
                                WHERE PWCM.ProductionOrderId='" + productionOrderId + "' ORDER BY p.UserName,e.UserName,wcm.sequence";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)

            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
            return null;
        }


        public IEnumerable<object> GetWorkCenterListByEntity(string entityId)
        {

            try
            {

                var _sql = @"SELECT WCM.Id AS WorkCenterMasterId, NULL AS ProductionOrderId,e.UserName AS Entity,p.UserName AS Plant
	                             , WCM.EntityId, WCM.Code, WCM.UserName, Flag = Convert(bit,0)
                            FROM SCS.WorkCenterMaster AS WCM
                            INNER JOIN org.Entity AS e ON e.Id=wcm.EntityId
                            INNER JOIN org.Plant AS p ON p.Id=wcm.PlantId
                            WHERE WCM.EntityId='"+entityId+"' order by p.userName, e.UserName,WCM.sequence";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)

            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public IEnumerable<object> GetWorkCenterListByEntityandFirstProcess(string entityId, string processId, string productionOrderId)
        {

            try
            {

                var _sql = @"SELECT Selection = Convert(bit,0),WCM.Id AS WorkCenterMasterId, NULL AS ProductionOrderId,e.UserName AS Entity,p.UserName AS Plant
	                             , WCM.EntityId, WCM.Code, WCM.UserName,NULL Remark
                            FROM SCS.WorkCenterMaster AS WCM
                            INNER JOIN org.Entity AS e ON e.Id=wcm.EntityId
                            INNER JOIN org.Plant AS p ON p.Id=wcm.PlantId
                            WHERE WCM.EntityId='" + entityId + "' AND ProcessId='"+ processId + "' AND wcm.id not in (select WorkCenterMasterId from ProductionOrderFirstProcessWorkCenter where ProductionOrderId='"+ productionOrderId +@"') order by p.userName, e.UserName,WCM.sequence";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)

            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public IEnumerable<object> GetSavedWorkCenterListByEntityandFirstProcess(string ProductionOrderId)
        {

            try
            {

                var _sql = @"SELECT FP.*,FP.WorkCenterMasterId, e.UserName AS Entity,p.UserName AS Plant
	                             , WCM.EntityId, WCM.Code, WCM.UserName
                            FROM dbo.ProductionOrderFirstProcessWorkCenter FP 
							LEFT JOIN SCS.WorkCenterMaster AS WCM ON WCM.Id=FP.WorkCenterMasterId
                            INNER JOIN org.Entity AS e ON e.Id=wcm.EntityId
                            INNER JOIN org.Plant AS p ON p.Id=wcm.PlantId
                           Where FP.ProductionOrderId='" + ProductionOrderId + "' order by p.userName, e.UserName,WCM.sequence";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)

            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public IEnumerable<object> GetSavedType2WorkCenterListByEntityandFirstProcess(string ProductionOrderId)
        {

            try
            {

                var _sql = @"SELECT FP.*,FP.WorkCenterMasterId, e.UserName AS Entity,p.UserName AS Plant
	                             , WCM.EntityId, WCM.Code, WCM.UserName
                            FROM dbo.ProductionOrderType2FirstProcessWorkCenter FP 
							LEFT JOIN SCS.WorkCenterMaster AS WCM ON WCM.Id=FP.WorkCenterMasterId
                            INNER JOIN org.Entity AS e ON e.Id=wcm.EntityId
                            INNER JOIN org.Plant AS p ON p.Id=wcm.PlantId
                           Where FP.ProductionOrderId='" + ProductionOrderId + "' order by p.userName, e.UserName,WCM.sequence";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)

            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }


        private void InsertUpdateOrDeleteGraph(string masterId, IEnumerable<ProductionOrderWorkCenter> entities)
        {
            try
            {
                if (entities != null)
                {
                    var count = _workCenterRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[ProductionOrderWorkCenter] WHERE ProductionOrderId='{masterId}'").First();

                    foreach (var item in entities)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            count++;
                            item.Id = MakePK(masterId, count, 2);
                            item.ProductionOrderId = masterId;
                            AuditService.AddedLog(item);
                            _workCenterRepository.Insert(item);
                        }
                        else
                        {
                            _workCenterRepository.Update(item);
                        }

                    }
                }
                var dbList = _workCenterRepository.Query(t => t.ProductionOrderId == masterId).Select().ToList();
                if (dbList.IsNotNull() && dbList.Count > 0)
                {
                    if (entities == null)
                    {
                        foreach (var item in dbList)
                        {
                            _workCenterRepository.Delete(item);
                        }
                    }
                    else
                    {
                        foreach (var item in dbList)
                        {
                            if (!entities.Any(t => t.Id == item.Id))
                            {
                                _workCenterRepository.Delete(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void InsertUpdateOrDeleteGraphPFPWC(string masterId, IEnumerable<ProductionOrderFirstProcessWorkCenter> entities)
        {
            try
            {
                if (entities != null)
                {
                    var count = _fpworkCenterRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM dbo.ProductionOrderFirstProcessWorkCenter WHERE ProductionOrderId='{masterId}'").First();

                    foreach (var item in entities)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            count++;
                            item.Id = MakePK(masterId, count, 2);
                            item.ProductionOrderId = masterId;
                            AuditService.AddedLog(item);
                            _fpworkCenterRepository.Insert(item);
                        }
                        else
                        {
                            _fpworkCenterRepository.Update(item);
                        }

                    }
                }
                var dbList = _fpworkCenterRepository.Query(t => t.ProductionOrderId == masterId).Select().ToList();
                if (dbList.IsNotNull() && dbList.Count > 0)
                {
                    if (entities == null)
                    {
                        foreach (var item in dbList)
                        {
                            _fpworkCenterRepository.Delete(item);
                        }
                    }
                    else
                    {
                        foreach (var item in dbList)
                        {
                            if (!entities.Any(t => t.Id == item.Id))
                            {
                                _fpworkCenterRepository.Delete(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void WorkCenterDeleteGraph(string masterId)
        {
            try
            {
                var dbList = _workCenterRepository.Query(t => t.ProductionOrderId == masterId).Select().ToList();
                if (dbList.IsNotNull() && dbList.Count > 0)
                {
                    foreach (var item in dbList)
                    {
                        _workCenterRepository.Delete(item);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
    }
}