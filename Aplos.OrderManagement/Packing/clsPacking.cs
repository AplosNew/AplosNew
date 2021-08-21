using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace Library.OrderManagement.Packing
{
    public class clsPacking
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        #region Constructor
        public clsPacking()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();


        }
        #endregion Constructor

        #region PackingConfirmation

        public IEnumerable<object> GetDataList(string PlantId)
        {
            string sql = @"SELECT P.*,ISNULL(FP.UserName,FSFG.UserName) [From], ISNULL(TP.UserName,TSFG.UserName) [To]
							,MM.UserName MaterialMaster,MMA.StandardName Article,EN.UserName Entity,PL.UserName Plant,FORMAT(p.ProductionDate,'dd-MMM-yyyy') ProdDate
							FROM [dbo].[PackingConfirmation] P
							LEFT JOIN HKP.ProductionBookingPeriod PBP ON PBP.Id=P.ProductionBookingPeriodId
							LEFT JOIN MST.MaterialMaster MM ON MM.Id=P.MaterialMasterId
							LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=P.ArticleId
							LEFT JOIN HKP.Process FP ON FP.Id=P.ProcessId
							LEFT JOIN HKP.Process TP ON TP.Id=P.ToProcessId
							LEFT JOIN HKP.SFGInventory FSFG ON FSFG.Id=FromSFGInventoryId
							LEFT JOIN HKP.SFGInventory TSFG ON TSFG.Id=ToSFGInventoryId
							LEFT JOIN TRN.ProductionOrder PO ON PO.Id=P.ProductionOrderId
							LEFT JOIN ORG.Entity EN ON EN.Id=P.EntityId
							LEFT JOIN ORG.Plant PL ON PL.Id=P.PlantId
							Where P.PlantId='" + PlantId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetPackingProcessCbo(string entity, bool IsSysAdmin, bool IsControlAdmin, string UserId)
        {
            string sql;

            if (IsSysAdmin || IsControlAdmin)
            {
                sql = @"Select P.Id,P.UserName from HKP.EntityProcessTag E
                         LEFT JOIN [HKP].Process P ON E.ProcessId = P.Id 
                        Where ProcessNature='Packing' AND E.EntityId='" + entity + "'";
            }
            else
            {
                sql = @"Select P.Id,P.UserName from HKP.EntityProcessTag E
                        LEFT JOIN [HKP].Process P ON E.ProcessId = P.Id 
                        LEFT JOIN SEC.UserProcess U on U.ProcessId= P.Id  AND U.UserId='" + UserId + @"'
                        Where ProcessNature='Packing' AND E.EntityId='" + entity + "'";
            }

            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetProductionOrderDataList()
        {
            string sql = @"SELECT PO.Id POId,PS.UserName ProductionStatus, PO.RequiredTimeUnit, Qty,FORMAT(LSD,'dd-MMM-yyyy') LSD 
								   ,FORMAT(CommitmentDate,'dd-MMM-yyyy') CommitmentDate, PD.Product, PD.ProductCategory,PD.Buyer,PD.Customer 
                                   ,PD.BuyerOrder,PD.OwnOrder,PD.BuyerItem,PD.OwnItem,PD.Description,PD.PONumber,PO.EntityId,E.UserName Entity
								   FROM TRN.ProductionOrder PO 
								   LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
								   LEFT JOIN ORG.Entity E ON E.Id=PO.EntityId
								  
								   LEFT JOIN 
								   (select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory
								   
								   ,Buyer=  REPLACE(REPLACE(
										            STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                            ,'&amp;','&'), 'amp;', '')	
								,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                        ,'&amp;','&'), 'amp;', '')	
                                ,BuyerOrder = REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								                		,'&amp;','&'), 'amp;', '')
                                ,OwnOrder =REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									                	,'&amp;','&'), 'amp;', '')
							 ,BuyerItem=REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                ,'&amp;','&'), 'amp;', '')	                                                
                              ,OwnItem=REPLACE(REPLACE(
										STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	 
                               ,PONumber=REPLACE(REPLACE(
										 STUFF((select distinct ','+CPO.PONumber from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
                            , Description=REPLACE(REPLACE(
										 STUFF((select distinct ','+XSO.Description from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                        
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
								   FROM TRN.SalesOrder SO
							       LEFT JOIN  TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
								   LEFT JOIN TRN.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                   LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
								   LEFT JOIN TRN.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
								   LEFT JOIN [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                   LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
								   ) PD ON PD.ProductionOrderId=PO.Id
								   WHERE PS.UserName = 'Running' AND PO.Id IN (Select ProductionOrderId from dbo.PackingContentMaster)";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetPackDataList(string MasterId)
        {
            string sql = @"SELECT  PCD.Id PCDId,MM.UserName AS MaterialMasterName,MMA.StandardName AS ArticleName,CV1.UserName as FirstCharacteristicsValue
                         ,CV2.UserName as SecondCharacteristicsValue, CV3.UserName as ThirdCharacteristicsValue
                        ,PCQty=PC.C*PCD.Qty
                         FROM [dbo].[PackingContentDetail] PCD
                         LEFT JOIN MST.MaterialMaster MM on MM.id= PCD.MaterialMasterId
                         LEFT JOIN MST.MaterialMasterArticle MMA on MMA.id= PCD.ArticleId
                         LEFT JOIN HKP.CharacteristicsValue CV1 on cv1.id= PCD.FirstCharacteristicsValueId
                         LEFT JOIN HKP.CharacteristicsValue CV2 on cv2.id= PCD.SecondCharacteristicsValueId
                         LEFT JOIN HKP.CharacteristicsValue CV3 on CV3.id= PCD.ThirdCharacteristicsValueId
                         JOIN (Select count(Id) C,PackingContentMasterId  From dbo.PackingChild WHERE ISNULL(IsConfirmed,0)=0 GROUP BY PackingContentMasterId) PC ON PC.PackingContentMasterId=PCD.PackingContentMasterId
                         WHERE PCD.PackingContentMasterId='" + MasterId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetToProcessCbo(string FromId, string EntityId, bool IsSysAdmin, bool IsControlAdmin, string UserId)
        {
            string processId = string.Empty;
            string inventoryId = string.Empty;
            string flag = "PROCESS";
            if (flag == "PROCESS")
            {
                processId = FromId;
            }


            string sql;

            if (IsSysAdmin || IsControlAdmin)
            {
                sql = @"SELECT A.* FROM (
                        SELECT DISTINCT  'PROCESS' AS Status, SFGM.FromProcessId, SFGM.FromSFGInventoryId, SFGM.ToProcessId AS ToId,  P.UserName
                        FROM MST.SFGMovement AS SFGM  
                        INNER JOIN  HKP.EntityProcessTag E on E.ProcessId=SFGM.ToProcessId AND E.EntityId='" + EntityId + @"'
                        LEFT JOIN [HKP].Process P ON SFGM.ToProcessId = P.Id WHERE ISNULL(SFGM.ToProcessId,'')<>''
                        UNION ALL
                        SELECT DISTINCT 'INVENTORY'as Status, SFGM.FromProcessId, SFGM.FromSFGInventoryId, SFGM.ToSFGInventoryId AS ToId, SFGI.UserName
                        FROM MST.SFGMovement AS SFGM 
                        INNER JOIN  MST.EntitySFGInventory E ON E.SFGInventoryId=SFGM.ToSFGInventoryId AND E.EntityId='" + EntityId + @"'
                        LEFT JOIN [HKP].[SFGInventory] SFGI ON SFGM.ToSFGInventoryId = SFGI.Id WHERE ISNULL(SFGM.ToSFGInventoryId,'')<>''
                        ) A WHERE A.FromProcessId = '" + processId + @"' OR A.FromSFGInventoryId = '" + inventoryId + @"' ";
            }
            else
            {
                sql = @"SELECT A.* FROM (
                        SELECT DISTINCT  'PROCESS' AS Status, SFGM.FromProcessId, SFGM.FromSFGInventoryId, SFGM.ToProcessId AS ToId,  P.UserName 
                        FROM MST.SFGMovement AS SFGM  
                        INNER JOIN  HKP.EntityProcessTag E on E.ProcessId=SFGM.ToProcessId AND E.EntityId='" + EntityId + @"'
                        LEFT JOIN [HKP].Process p ON SFGM.ToProcessId = P.Id 
                        LEFT JOIN SEC.UserProcess U on U.ProcessId= p.Id AND U.UserId='" + UserId + @"'
                        WHERE ISNULL(SFGM.ToProcessId,'')<>''
                        UNION ALL
                        SELECT DISTINCT 'INVENTORY' AS Status, SFGM.FromProcessId, SFGM.FromSFGInventoryId, SFGM.ToSFGInventoryId AS ToId, SFGI.UserName
                        FROM MST.SFGMovement AS SFGM 
                        INNER JOIN  MST.EntitySFGInventory E ON E.SFGInventoryId=SFGM.ToSFGInventoryId AND E.EntityId='" + EntityId + @"'
                        LEFT JOIN [HKP].[SFGInventory] SFGI ON SFGM.ToSFGInventoryId = SFGI.Id 
                        LEFT JOIN SEC.UserSFGInventory U on U.SFGInventoryId= SFGI.Id  AND U.UserId='" + UserId + @"'
                        WHERE ISNULL(SFGM.ToSFGInventoryId,'')<>''
                        ) A WHERE A.FromProcessId = '" + processId + @"' OR A.FromSFGInventoryId = '" + inventoryId + @"' ";
            }
            return _sqlRepository.GetDataCollection(sql);

        }

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(PackingConfirmation), out sID);
            return sID;
        }

        public void SavePackingConfirmationData(PackingConfirmation data, OTSBD.IdentityParameter para, out string masterId)
        {

            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;

            string id = string.Empty;

            try
            {
                string sql = "SELECT * FROM [dbo].[PackingConfirmation] WHERE Id='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = "PC" + GetPK();
                    dr["PlantId"] = data.PlantId;
                    dr["EntityId"] = data.EntityId;
                    dr["ProcessId"] = data.ProcessId;
                    dr["SalesOrderId"] = data.SalesOrderId;
                    dr["MaterialMasterId"] = data.MaterialMasterId;
                    dr["ArticleId"] = data.ArticleId;
                    dr["WorkCenterMasterId"] = data.WorkCenterMasterId;
                    dr["ProductionDate"] = data.ProductionDate;
                    dr["Quantity"] = data.Quantity;
                    dr["ProductionShiftId"] = data.ProductionShiftId;
                    dr["ProductionBookingPeriodId"] = data.ProductionBookingPeriodId;
                    dr["ProductionOrderId"] = data.ProductionOrderId;
                    dr["ToProcessId"] = data.ToProcessId;
                    dr["ToWorkCenterMasterId"] = data.ToWorkCenterMasterId;
                    dr["FromSFGInventoryId"] = data.FromSFGInventoryId;
                    dr["ToSFGInventoryId"] = data.ToSFGInventoryId;

                    dr["AddedBy"] = para.AddedDate;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = para.AddedFromIP;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();
                    dr["PlantId"] = data.PlantId;
                    dr["EntityId"] = data.EntityId;
                    dr["ProcessId"] = data.ProcessId;
                    dr["SalesOrderId"] = data.SalesOrderId;
                    dr["MaterialMasterId"] = data.MaterialMasterId;
                    dr["ArticleId"] = data.ArticleId;
                    dr["WorkCenterMasterId"] = data.WorkCenterMasterId;
                    dr["ProductionDate"] = data.ProductionDate;
                    dr["Quantity"] = data.Quantity;
                    dr["ProductionShiftId"] = data.ProductionShiftId;
                    dr["ProductionBookingPeriodId"] = data.ProductionBookingPeriodId;
                    dr["ProductionOrderId"] = data.ProductionOrderId;
                    dr["ToProcessId"] = data.ToProcessId;
                    dr["ToWorkCenterMasterId"] = data.ToWorkCenterMasterId;
                    dr["FromSFGInventoryId"] = data.FromSFGInventoryId;
                    dr["ToSFGInventoryId"] = data.ToSFGInventoryId;

                    dr["UpdatedBy"] = para.UpdatedBy;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = para.UpdatedFromIP;

                    dr.EndEdit();
                }

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);

                    masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                
            }

            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void SaveConfirmPackingChildData(IEnumerable<PackingChild> data, OTSBD.IdentityParameter para, string masterId)
        {

            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;

                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [dbo].[PackingChild] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                        if (dsMaster.Tables[0].Rows.Count > 0)
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["PackingContentMasterId"] = item.PackingContentMasterId;
                            dr["PackingConfirmationId"] = masterId;
                            dr["Sequence"] = item.Sequence;
                            dr["IsConfirmed"] = item.IsConfirmed;

                            dr["UpdatedBy"] = para.UpdatedBy;
                            dr["UpdatedDate"] = DateTime.Now.ToString();
                            dr["UpdatedFromIp"] = para.UpdatedFromIP;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        #endregion PackingConfirmation

        public IEnumerable<object> GetSalesOrderListSearch(string column, string value, string productionorderid, string PlantId)
        {
           
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";


            string activeStatus = "";
            string plantSql = @"select * from scs.PlantConfig where plantid='" + PlantId + "'";
            DataTable dtPlantConfig = _sqlRepository.GetDataTable(plantSql);
            if (dtPlantConfig.Rows.Count > 0)
                if (bplib.clsWebLib.GetBoolData(dtPlantConfig.Rows[0]["IsProductionOrderCreatedAfterConfirmationOfSO"].ToString()))
                    activeStatus = " AND isnull(SO.IsConfirm,0)=1 ";


            string sql = @"SELECT
                                pod.ProductionOrderId,so.Id SalesOrderId,moi.MaterialMasterId,moi.ArticleId,MM.UserName AS MaterialMasterName,MMA.StandardName AS ArticleName,
                                WithSKU=CASE WHEN MM.WithSKU=1 THEN 'Yes' WHEN MM.WithSKU=0 THEN 'No' END,
                                fc.CharacteristicsValueId FirstCharacteristicsValueId,sc.CharacteristicsValueId SecondCharacteristicsValueId,tc.CharacteristicsValueId ThirdCharacteristicsValueId,
                                c1.UserName AS FirstCharacteristics,cv1.UserName AS FirstCharacteristicsValue,
                                c2.UserName AS SecondCharacteristics,cv2.UserName AS SecondCharacteristicsValue,
                                c3.UserName AS ThirdCharacteristics,cv3.UserName AS ThirdCharacteristicsValue,

                                SUM(
                                CASE WHEN isnull(tc.Id,'')<>'' THEN tc.Qty ELSE
                                CASE WHEN ISNULL(sc.Id,'')<>'' THEN sc.Qty ELSE
                                CASE WHEN ISNULL(fc.Id,'')<>'' THEN fc.Qty ELSE so.Qty END END END
                                ) AS TotalQty

                                FROM trn.SalesOrder AS so
                                INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=moi.MaterialMasterId
                                LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=moi.ArticleId

                                LEFT JOIN trn.FirstCharacteristics AS fc ON fc.SalesOrderId=so.Id
                                LEFT JOIN trn.SecondCharacteristics AS sc ON sc.FirstCharacteristicsId=fc.Id AND sc.SalesOrderId=so.Id
                                LEFT JOIN trn.ThirdCharacteristics AS tc ON tc.SecondCharacteristicsId=sc.Id AND tc.SalesOrderId=so.Id

                                LEFT JOIN hkp.CharacteristicsValue AS cv1 ON cv1.Id=fc.CharacteristicsValueId
                                LEFT JOIN hkp.Characteristics AS c1 ON c1.Id=cv1.CharacteristicsId

                                LEFT JOIN hkp.CharacteristicsValue AS cv2 ON cv2.Id=sc.CharacteristicsValueId
                                LEFT JOIN hkp.Characteristics AS c2 ON c2.Id=cv2.CharacteristicsId

                                LEFT JOIN hkp.CharacteristicsValue AS cv3 ON cv3.Id=tc.CharacteristicsValueId
                                LEFT JOIN hkp.Characteristics AS c3 ON c3.Id=cv3.CharacteristicsId
                                WHERE pod.ProductionOrderId " + productionorderid + @"
                                GROUP BY pod.ProductionOrderId,so.Id,moi.MaterialMasterId,moi.ArticleId,MM.UserName,MMA.StandardName,fc.CharacteristicsValueId,sc.CharacteristicsValueId,tc.CharacteristicsValueId
                                ,c1.UserName,cv1.UserName,c2.UserName,cv2.UserName,c3.UserName,cv3.UserName,WithSKU";

            return _sqlRepository.GetDataCollection(sql, null);
        }
    }
    public class PackingContentDetail
    {
        public string Id { get; set; }
        public string PackingContentMasterId { get; set; }
        public string SalesOrderId { get; set; }
        public string MaterialMasterId { get; set; }
        public string ArticleId { get; set; }
        public decimal Qty { get; set; }
        public string FirstCharacteristicsValueId { get; set; }
        public string SecondCharacteristicsValueId { get; set; }
        public string ThirdCharacteristicsValueId { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }

    public class PackingChild
    {
        public string Id { get; set; }
        public string PackingContentMasterId { get; set; }
        public decimal Sequence { get; set; }
        public bool IsConfirmed { get; set; }

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }

    public class PackingConfirmation
    {
        public string Id { get; set; }
        public string PlantId { get; set; }
        public string EntityId { get; set; }
        public string ProcessId { get; set; }
        public string SalesOrderId { get; set; }
        public string MaterialMasterId { get; set; }
        public string ArticleId { get; set; }
        public string WorkCenterMasterId { get; set; }
        public DateTime ProductionDate { get; set; }
        public decimal Quantity { get; set; }
        public string ProductionShiftId { get; set; }
        public string ProductionBookingPeriodId { get; set; }
        public string ProductionOrderId { get; set; }
        public string ToProcessId { get; set; }
        public string ToWorkCenterMasterId { get; set; }
        public string FromSFGInventoryId { get; set; }
        public string ToSFGInventoryId { get; set; }

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }
}
