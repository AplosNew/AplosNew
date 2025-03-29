using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace Library.OrderManagement.ShipmentControl
{
    public class ShipmentControl
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        #region Constructor
        public ShipmentControl()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();


        }
        #endregion Constructor

        public void SaveMasterData(List<ShipmentControlModel> data, IdentityParameter para)
        {
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    string _Id = "";
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [dbo].[OrderControl] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "OrderControl", out _Id);

                            DataRow dr = dsMaster.Tables[0].NewRow();
                            dr["Id"] = _Id;
                            dr["ControlTypeId"] = item.ControlTypeId;
                            dr["SalesOrderId"] = item.SalesOrderId;
                            dr["ProductionOrderId"] = item.ProductionOrderId;
                            dr["CriticalityLevel"] = item.CriticalityLevel;
                            dr["Status"] = item.Status;

                            dr["AddedBy"] = para.AddedBy;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = para.AddedFromIP;

                            dr["UpdatedBy"] = para.UpdatedBy;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = para.UpdatedFromIP;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["ControlTypeId"] = item.ControlTypeId;
                            dr["SalesOrderId"] = item.SalesOrderId;
                            dr["ProductionOrderId"] = item.ProductionOrderId;
                            dr["CriticalityLevel"] = item.CriticalityLevel;
                            dr["Status"] = item.Status;

                            dr["UpdatedBy"] = para.UpdatedBy;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = para.UpdatedFromIP;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public void SaveData(ShipmentControlModel item, OrderControlRemarks entity)
        {
            try
            {
                if (item != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    DataSet dsRemarks;
                    string _Id = "";

                    string sql = "SELECT * FROM [dbo].[OrderControl] WHERE Id='" + item.Id + "'";
                    string sqlRemarks = "SELECT * FROM [dbo].[OrderControlRemarks] WHERE Id='" + entity.Id + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                    objCon.OpenDataSetThroughAdapter(sqlRemarks, out dsRemarks, false, "1");


                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "OrderControl", out _Id);

                        DataRow dr = dsMaster.Tables[0].NewRow();
                        dr["Id"] = _Id;
                        dr["ControlTypeId"] = item.ControlTypeId;
                        dr["SalesOrderId"] = item.SalesOrderId;
                        dr["ProductionOrderId"] = item.ProductionOrderId;
                        dr["CriticalityLevel"] = item.CriticalityLevel;
                        dr["Status"] = item.Status;


                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = item.AddedFromIP;

                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = item.UpdatedFromIP;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        //edit
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["ControlTypeId"] = item.ControlTypeId;
                        dr["SalesOrderId"] = item.SalesOrderId;
                        dr["ProductionOrderId"] = item.ProductionOrderId;
                        dr["CriticalityLevel"] = item.CriticalityLevel;
                        dr["Status"] = item.Status;

                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = item.UpdatedFromIP;

                        dr.EndEdit();
                    }

                    if (dsRemarks.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "OrderControlRemarks", out _Id);

                        DataRow dr = dsRemarks.Tables[0].NewRow();
                        dr["Id"] = _Id;
                        dr["OrderControlId"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                        dr["Remarks"] = entity.Remarks;

                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = item.AddedFromIP;

                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = item.UpdatedFromIP;

                        dsRemarks.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        //edit
                        DataRow dr = dsRemarks.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["OrderControlId"] = item.Id;
                        dr["Remarks"] = entity.Remarks;

                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = item.UpdatedFromIP;

                        dr.EndEdit();
                    }

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster, dsRemarks);
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public IEnumerable<object> GetControlTypeCbo()
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"SELECT Id,ControlType, [Day]=Days,LagDays FROM OrderControlTypes";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public IEnumerable<object> GetSalesOrderData(int day, int lagdays, string level)
        {
            try
            {
                string date = DateTime.Now.ToString("dd-MMM-yyyy");
                string strSQL = string.Empty;

                strSQL = @"SELECT oct.Id,so.Id SalesOrderId, FORMAT(so.DeliveryDate,'dd-MMM-yyyy') [Date],oct.CriticalityLevel 
                        ,SO.Description,soi.PlannedQty SOQty,CPO.PONumber ,B.UserName Buyer,mm.UserName MaterialMaster,ISNULL(mma.StandardName, '') Article, PM.UserName AS ProductMasterName ,p.UserName Customer
                        ,MO.BuyerReferenceNo BuyerOrder,MO.OwnReferenceNo OwnOrder,moi.BuyerReferenceNo BuyerItem,moi.OwnReferenceNo OwnItem,SO.Qty,SO.LineItemReference
						FROM trn.SalesOrder AS so
						LEFT JOIN (
	                                SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
		                                ,s.Id,s.MasterOrderItemId
	                                FROM trn.SalesOrder AS s
	                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
	                                GROUP BY S.Id,s.MasterOrderItemId
	                                ) soi ON soi.Id = SO.Id

                        LEFT OUTER JOIN dbo.OrderControl AS OCT ON oct.SalesOrderId=so.Id
                        LEFT OUTER JOIN OrderControlTypes AS oct2 ON oct2.Id=oct.ControlTypeId AND oct2.ControlType='" + level + @"'

                        LEFT OUTER JOIN TRN.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                        LEFT OUTER JOIN TRN.MasterOrder MO on mo.Id=moi.MasterOrderId
                        LEFT OUTER JOIN [HKP].[Party] p on P.Id=MO.PartyId
                        LEFT OUTER JOIN [HKP].[PartyPlant] PPI on ppi.id=mo.InvoicingPartyPlantId
                        LEFT OUTER JOIN [HKP].[PartyPlant] PPD on ppd.id=mo.DeliveryPartyPlantId
                        LEFT OUTER JOIN [HKP].[Buyer] B on b.id=mo.BuyerId
                        LEFT OUTER JOIN [HKP].[BuyerBrand] BB on bb.id=mo.BuyerBrandId
                        LEFT OUTER JOIN [HKP].[BuyerDivision] BD on bd.id=mo.BuyerBrandId
                        LEFT OUTER JOIN [HKP].[OrderCategory] OC on oc.id=mo.OrderCategoryId
                        LEFT OUTER JOIN TRN.Commitment AS c ON c.Id=mo.CommitmentId
                        LEFT OUTER JOIN MST.Destination DEST on dest.Id=so.DestinationId
                        LEFT OUTER JOIN [TRN].[CustomerPO] CPO ON CPO.Id=so.CustomerPOId
                        LEFT OUTER JOIN [MST].[ShipMode] SMO on SMO.Id=so.ShipmentModeId
                        LEFT OUTER JOIN HKP.Season S on s.id=mo.SeasonId
                        LEFT OUTER JOIN EmployeeInformation EI on ei.SystemId= MO.ResponsiblePersonId
						LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
						LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
						LEFT JOIN [MST].[ProductMaster] AS PM ON PM.Id = MM.ProductMasterId

                        WHERE --so.OrderStatusId='Active' 
                        (so.OrderStatusId NOT IN ('Closed','Cancelled','Cancel') AND  MO.OrderStatusId NOT IN ('Closed','Cancelled','Cancel'))
                        AND DATEADD(DAY," + lagdays + ",so.DeliveryDate)   <=  DATEADD(DAY," + day + ",'" + date + "') ORDER BY so.DeliveryDate";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function
        public IEnumerable<object> GetProductoinOrderData(int day, int lagdays, string level)
        {
            try
            {
                string date = DateTime.Now.ToString("dd-MMM-yyyy");
                string strSQL = string.Empty;
                string wc = "";
                if (level == "MainRMInhouse" || level == "MainRMShipment")
                {
                    wc = "t.MainRawMaterialInhouseDate";
                }
                else if (level == "OtherRMInhouse" || level == "OtherRMShipment")
                {
                    wc = "t.OtherRawMaterialInhouseDate";
                }
                else if (level == "BaseProcessInput")
                {
                    wc = "t.LSD";
                }

                DataTable dt = _sqlRepository.GetDataTable(@"select * from OrderControlTypes where ControlType='" + level + @"'");

                strSQL = @"select oct.Id,po.Id AS ProductionOrderId,PS.UserName AS ProductionStatus,oct.CriticalityLevel,oct.[Status],FORMAT(" + wc + @",'dd-MMM-yyyy') [Date],

                              Buyer =STUFF((select distinct ','+XB.UserName from 
                                    trn.SalesOrder XSO 
                                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                        left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                                        left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                                        left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
										                                 
                                            where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
											 Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                             StyleGroup=STUFF((select distinct ','+xmoi.ProductionGrouping from 
                                        trn.SalesOrder XSO 
                                               JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                               left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
            
                                                   where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
            
                            MasterOrderNo=STUFF((select distinct ','+XMO.MasterOrderNo from 
                                           trn.SalesOrder XSO 
                                               JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                               left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                                              left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                                                   where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
               
                            OrderStatus=STUFF((select distinct ','+os.UserName from 
                                        trn.SalesOrder XSO 
                                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                                           left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                                           left outer join [HKP].[OrderStatus] OS on OS.id=XMO.OrderStatusId
                            where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

							    BuyerOrder =STUFF((select distinct ','+xmo.BuyerReferenceNo from 
                                    trn.SalesOrder XSO 
                                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                        left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                                        left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId										                                 
                                            where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                            
                                            
                                OwnOrder =STUFF((select distinct ','+xmo.OwnReferenceNo from 
                                    trn.SalesOrder XSO 
                                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                        left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                                        left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId										                                 
                                            where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                            
                                            
                                           BuyerItem =STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
                                    trn.SalesOrder XSO 
                                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                        left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId							                                 
                                            where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                            
                                            
                                           OwnItem =STUFF((select distinct ','+XMOI.OwnReferenceNo from 
                                    trn.SalesOrder XSO 
                                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                        left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId							                                 
                                            where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                    
                                [Description]=STUFF((select distinct ','+xso.[Description] from 
                                        trn.SalesOrder XSO 
                                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                            where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            LineItemReference=STUFF((select distinct ','+xso.LineItemReference from 
                                        trn.SalesOrder XSO 
                                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                            where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
															PO.Qty,FORMAT(PO.LSD,'dd-MMM-yyyy') LSD,FORMAT(PO.CommitmentDate,'dd-MMM-yyyy') CommitmentDate
															,ISNULL(PD.Product,'') Product, PD.ProductCategory,PD.MaterialMaster,PD.Article
                            --from trn.ProductionOrder PO
                            --LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS T ON t.ProductionOrderID=po.Id
                            --LEFT OUTER JOIN OrderControl AS OCT ON oct.ProductionOrderId=PO.Id AND ISNULL(oct.ProductionOrderId,'')<>'' AND oct.ControlTypeId='" + dt.Rows[0]["Id"].ToString() + @"' 
                            --LEFT OUTER JOIN OrderControlTypes AS oct2 ON oct2.Id=oct.ControlTypeId AND oct2.ControlType<>'ShipmentControl' AND oct.[Status]<>'Closed' 

                            from OrderControlTypes AS oct2
                          LEFT JOIN  trn.ProductionOrder PO ON 1=1
                          JOIN ProductionOrderSchedulingParametersType1 AS T ON t.ProductionOrderID=po.Id
                           LEFT JOIN OrderControl AS OCT ON oct.ProductionOrderId=PO.Id AND oct2.Id=oct.ControlTypeId 
                           

                            LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
                             left outer join org.Entity E  on e.Id=po.EntityID
                            LEFT OUTER JOIN org.Unit AS u ON u.Id=e.UnitId
                            left outer join org.Plant PLN on pln.Id=PO.PlantId
							left join (
							select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory,mm.UserName MaterialMaster,mma.StandardName Article FROM TRN.SalesOrder SO
							       LEFT JOIN  TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
								   LEFT JOIN TRN.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                   LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                                   LEFT JOIN MST.MaterialMasterArticle mma on mma.id=MOI.ArticleId
								   LEFT JOIN TRN.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
								   LEFT JOIN [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                   LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
							)PD ON PD.ProductionOrderId=PO.Id

                            WHERE oct2.Id='" + dt.Rows[0]["Id"].ToString() + @"' AND DATEADD(DAY," + lagdays + "," + wc + ")   <=  DATEADD(DAY," + day + ",'" + date + @"')  AND  ps.UserName IN ('Active','Running') AND po.Id IN (SELECT pod.ProductionOrderId
                                  FROM trn.ProductionOrderDetail AS pod
                          INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId WHERE so.OrderStatusId NOT IN ('Closed','Cancelled','Cancel')) ORDER BY " + wc + "";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public IEnumerable<object> GetRemarksByMaster(string OrderControlId)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"SELECT * FROM OrderControlRemarks Where OrderControlId='" + OrderControlId + "'";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function


        public IEnumerable<object> GetTermsAndConditionPopUp(string TermsAndConditionsDetailId)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"select * from TermsAndConditionsDetails where TermsAndConditionsChildId='" + TermsAndConditionsDetailId + "' order by sequence";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public IEnumerable<object> GetTermsAndConditionPOPopUp(string TermsAndConditionsPODetailId)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"select * from TermsAndConditionsPODetails where TermsAndConditionsPOChildId='" + TermsAndConditionsPODetailId + "' order by sequence";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function


        public IEnumerable<object> Title(string masterID)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"select * from TermsAndConditionsChild where TermsAndConditionsMasterId='" + masterID + "'";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function


        public void DeleteData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM dbo.OrderControlRemarks WHERE Id = '" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw exx;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function
    }

    public class ShipmentControlModel
    {
        public string Id { get; set; }
        public string ControlTypeId { get; set; }
        public string SalesOrderId { get; set; }
        public string ProductionOrderId { get; set; }
        public string CriticalityLevel { get; set; }
        public string Status { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }


    }

    public class OrderControlRemarks
    {
        public string Id { get; set; }
        public string OrderControlId { get; set; }
        public string Remarks { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }


    }


}
