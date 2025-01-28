using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Core;
using Library.Service.Enums;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.OrderManagement.OrderControl
{
    public class OrderControl
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
       // CustomIdentity identity;
        public DataTable OrderControlBaseData;
        #region Constructor
        public OrderControl()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
           // identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            OrderControlBaseDataConstruction();
        }
        #endregion Constructor

        public void GetData(out DataSet dsSavedData, IdentityParameter para)
        {
            ConManager = new ConnectionManager.clsConnectionManager();
            ConManager.getDataSet(@"SELECT OT.*,ei.EmployeeName AS ResponsiblePerson FROM OrderControlTypes AS ot
                LEFT OUTER JOIN EmployeeInformation AS ei ON ei.SystemId = ot.ResponsiblePersonId where ot.CompanyGroupId='" + para.CompanyGroupId + "'", out dsSavedData);


            foreach (DataRow drBase in OrderControlBaseData.Rows)
            {

                dsSavedData.Tables[0].DefaultView.RowFilter = "ControlType='" + drBase["ControlType"].ToString() + "'";
                if (dsSavedData.Tables[0].DefaultView.Count == 0)
                {
                    DataRow dr = dsSavedData.Tables[0].NewRow();

                    dr["ControlType"] = drBase["ControlType"].ToString();
                    dr["ControlTypeDesc"] = drBase["ControlTypeDesc"].ToString();
                    dr["Form"] = drBase["Form"].ToString();
                    dr["DependentDate"] = drBase["DependentDate"].ToString();

                    dsSavedData.Tables[0].Rows.Add(dr);

                }
                else
                {
                    DataRow dr = dsSavedData.Tables[0].DefaultView[0].Row;
                    dr["ControlTypeDesc"] = drBase["ControlTypeDesc"].ToString();
                    dr["Form"] = drBase["Form"].ToString();
                    dr["DependentDate"] = drBase["DependentDate"].ToString();

                }

            }

        }

        public void saveData(List<Dictionary<string, object>> Data, IdentityParameter para)
        {
            try
            {
                ConManager = new ConnectionManager.clsConnectionManager();
                ConManager.getDataSet(@"SELECT * FROM OrderControlTypes AS ot where ot.CompanyGroupId='" + para.CompanyGroupId + "'", out DataSet dsSavedData);

                string Id = "";
                int index = 0;
                foreach (Dictionary<string, object> drBase in Data)
                {
                    index++;

                    dsSavedData.Tables[0].DefaultView.RowFilter = "ControlType='" + drBase["ControlType"].ToString() + "'";

                    #region Attachment Item
                    if (dsSavedData.Tables[0].DefaultView.Count == 0)
                    {
                        if (Id == "")
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID("ORDER CONTROL TYPE", out Id);
                            Id = "CONF-" + Id;
                        }



                        drBase["Id"] = Id + "-" + index.ToString();
                        AddNewRow(dsSavedData.Tables[0], drBase);
                        dsSavedData.Tables[0].Rows[dsSavedData.Tables[0].Rows.Count - 1]["CompanyGroupId"] = para.CompanyGroupId;
                    }
                    else
                    {

                        EditRow(dsSavedData.Tables[0].DefaultView[0].Row, drBase);
                    }
                    #endregion Attachment Item


                }


                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsSavedData);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        private void OrderControlBaseDataConstruction()
        {
            OrderControlBaseData = new DataTable("BaseData");
            OrderControlBaseData.Columns.Add("ControlType");
            OrderControlBaseData.Columns.Add("ControlTypeDesc");
            OrderControlBaseData.Columns.Add("Form");
            OrderControlBaseData.Columns.Add("DependentDate");

            MakeOrderControlBaseData(OrderControlTypes.ShipmentControl, "Shipment Control (SO Level)", OrderControlDependentDates.SOShipment);
            MakeOrderControlBaseData(OrderControlTypes.MainRMInhouse, "Input Control (PRLevel)", OrderControlDependentDates.PRMainRMInhouseDate);
            MakeOrderControlBaseData(OrderControlTypes.OtherRMInhouse, "Input Control (PRLevel)", OrderControlDependentDates.PROtherRMInhouseDate);
            MakeOrderControlBaseData(OrderControlTypes.MainRMShipment, "Input Control (PRLevel)", OrderControlDependentDates.PRMainRMInhouseDate);
            MakeOrderControlBaseData(OrderControlTypes.OtherRMShipment, "Input Control (PRLevel)", OrderControlDependentDates.PROtherRMInhouseDate);
            MakeOrderControlBaseData(OrderControlTypes.BaseProcessInput, "Input Control (PRLevel)", OrderControlDependentDates.BaseProcessStartDate);
        }
        private void MakeOrderControlBaseData(OrderControlTypes type, string FormName, OrderControlDependentDates dependentDates)
        {
            DataRow dr = OrderControlBaseData.NewRow();

            dr["ControlType"] = type.ToString();
            dr["ControlTypeDesc"] = type.GetDescription();
            dr["Form"] = FormName;
            dr["DependentDate"] = dependentDates.ToString();


            OrderControlBaseData.Rows.Add(dr);
        }

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }




            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }


            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }

        public List<Dictionary<string, object>> SearchEmployee(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"
                      select top 100 * from (  
                                                                      SELECT
                                                 Emp.SystemID AS Id,
                        EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,
                      isnull(D.UserName,'') Designation,
      
                            DEPT.UserName Department,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,'' Plant
                              FROM EmployeeInformation AS EMP 
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            WHERE  isnull(empType,'')<>'Guest'  AND EMP.EmployeeStatus='active' AND  emp.GroupID ='" + identity.CompanyGroupId + @"'
                ) AS TEMP where " + strkey + " Order By Id";





            return _sqlRepository.GetDataCollection(sql, null);
        }


        #region Druv's code for API

        public IEnumerable<object> GetControlType()
        {
            try
            {
                var _sql = @"select * from dbo.OrderControlTypes";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetForm(string ControlType)
        {
            string sql = @"select Id, form from [dbo].[OrderControlTypes] where ISNULL(ControlType,'')='" + ControlType + "' ";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public string Create(IEnumerable<OrderControlData> DataToSave)
        {

            try
            {
                DataSet dsMaster;
                string TableName = "dbo.OrderControl";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";

                List<OrderControlData> items = DataToSave.ToList();


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + items[0].Id + "'", out dsMaster, false, "1");

                string _Id = "";

                foreach (OrderControlData item in DataToSave)
                {
                    if (dsMaster.Tables[0].Rows.Count == 0 && items[0].Id == null)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);

                        dr["Id"] = "OC" + _Id;
                        dr["ControlTypeId"] = item.ControlTypeId;
                        dr["SalesOrderId"] = item.SalesOrderId;
                        dr["ProductionOrderId"] = item.ProductionOrderId;
                        dr["CriticalityLevel"] = item.CriticalityLevel;
                        dr["Status"] = item.Status;

                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = item.AddedFromIP;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }

                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                string MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                return MasterId;


            }

            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public string SaveRemarks(IEnumerable<OrderControlRemarks> DataToSave, string MasterId)
        {

            try
            {
                DataSet dsMaster;
                string TableName = "dbo.OrderControlRemarks";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                List<OrderControlRemarks> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + items[0].Id + "'", out dsMaster, false, "1");

                string _Id = "";

                foreach (OrderControlRemarks item in DataToSave)
                {
                    if (dsMaster.Tables[0].Rows.Count == 0 && items[0].Id == null)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);

                        dr["Id"] = "OR" + _Id;
                        dr["OrderControlId"] = MasterId;
                        dr["Remarks"] = item.Remarks;

                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = item.AddedFromIP;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }

                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                string Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                return Id;

            }

            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public IEnumerable<object> GetSalesOrderId(string level)
        {
            try
            {
                string date = DateTime.Now.ToString("dd-MMM-yyyy");
                string strSQL = string.Empty;

                strSQL = @"SELECT oct.Id,so.Id SalesOrderId, FORMAT(so.DeliveryDate,'dd-MMM-yyyy') [Date],oct.CriticalityLevel 
                        ,SO.Description,soi.PlannedQty SOQty,CPO.PONumber ,B.UserName Buyer,mm.UserName MaterialMaster,ISNULL(mma.StandardName, '') Article, PM.UserName AS ProductMasterName ,p.UserName Customer
                        ,MO.BuyerReferenceNo BuyerOrder,MO.OwnReferenceNo OwnOrder,moi.BuyerReferenceNo BuyerItem,moi.OwnReferenceNo OwnItem,SO.Qty
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
                        LEFT OUTER JOIN [HKP].[Party] p on P.Id=MO.plantID
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

                        WHERE so.OrderStatusId='Active' ";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        public IEnumerable<object> GetProductionOrderId(string level)
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

															PO.Qty,FORMAT(PO.LSD,'dd-MMM-yyyy') LSD,FORMAT(PO.CommitmentDate,'dd-MMM-yyyy') CommitmentDate
															, PD.Product, PD.ProductCategory
                            from trn.ProductionOrder PO
                            LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS T ON t.ProductionOrderID=po.Id
                            LEFT OUTER JOIN OrderControl AS OCT ON oct.ProductionOrderId=PO.Id AND ISNULL(oct.ProductionOrderId,'')<>''
                            LEFT OUTER JOIN OrderControlTypes AS oct2 ON oct2.Id=oct.ControlTypeId AND oct2.ControlType<>'ShipmentControl' AND oct.[Status]<>'Closed'
                            LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
                             left outer join org.Entity E  on e.Id=po.EntityID
                            LEFT OUTER JOIN org.Unit AS u ON u.Id=e.UnitId
                            left outer join org.Plant PLN on pln.Id=PO.PlantId
							left join (
							select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory FROM TRN.SalesOrder SO
							       LEFT JOIN  TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
								   LEFT JOIN TRN.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                   LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
								   LEFT JOIN TRN.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
								   LEFT JOIN [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                   LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
							)PD ON PD.ProductionOrderId=PO.Id

                           ";

                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        #endregion

        public IEnumerable<object> GetMasterOrderData(string CompanyId)
        {

            string sql = @"SELECT A.Id, A.CompanyId, A.CommitmentId, A.PlantId, A.EntityId,FORMAT( A.AddedDate,'dd-MMM_yyyy') CreationDate
                                    , a.AddedBy AS CreatedBy
                                    , A.OrderType, A.PartyId, P.UserName AS CustomerName, A.BuyerId,B.UserName Buyer
                                    , A.BuyerBrandId, A.BuyerDivisionId, A.TestingStandardId, A.MasterOrderNo, A.OrderStatusId	
                                    , A.OrderCategoryId,OC.UserName AS OrderCategory, A.SeasonId, A.OrderYear, A.CurrencyId, A.TotalQty	
                                    , A.NoOfLineItem, A.ResponsiblePersonId, EI.EmployeeName AS ResponsiblePersonName
                                    , A.InvoicingPartyPlantId, InvPP.UserName AS InvoicingPartyPlant, A.InvoicingByAddress
		                            , A.DeliveryPartyPlantId, DeliPP.UserName AS DeliveryPartyPlant, A.DeliveryByAddress
		                            , PartyAccountGroupId=(SELECT DISTINCT PartyAccountGroupId FROM [HKP].[CompanyParty] WHERE CompanyId=A.CompanyId
								                            AND PartyId=A.PartyId AND PartyType='Customer' AND PlantId=A.PlantId)
								    ,A.OrderWastagePercentage
								    ,A.ExtraOrderPercentage,A.BuyerDepartmentId
								    ,A.TotalQtyUOMId,PL.UserName,A.IsReplacement,A.Type,C.Code Currency,A.SpecialTaxId,A.IsExtraOrderPercentage,PM.UserName ProductMaster,OS.UserName OrderStatus,FORMAT( A.AddedDate,'dd-MMM_yyyy') AddedDate,A.AddedBy
                                      ,A.OwnReferenceNo,A.BuyerReferenceNo,A.PaymentTermId,A.PaymentTermDays,A.ExceptionalProcessId,A.ExceptionalSubProcessId
                                    ,[BuyerItem]=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                     [OwnItem]=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                    ContractNo=STUFF((select distinct ','+CNT.ContractNo from dbo.Contract CNT
															INNER JOIN trn.SalesOrder XSO  ON XSO.ContractId=CNT.Id	  
															INNER JOIN trn.MasterOrderItem XMOI  ON XSO.MasterOrderItemId=XMOI.Id
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
									MasterLCNo=STUFF((select distinct ','+MLC.LCRef from dbo.Contract CNT
															INNER JOIN trn.SalesOrder XSO  ON XSO.ContractId=CNT.Id	  
															INNER JOIN trn.MasterOrderItem XMOI  ON XSO.MasterOrderItemId=XMOI.Id
															LEFT JOIN dbo.MasterLC MLC ON MLC.Id=CNT.MasterLCId	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            															
                            FROM [TRN].[MasterOrder] AS A
						
                            JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN [HKP].[PartyPlant] AS InvPP ON A.InvoicingPartyPlantId=InvPP.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DeliPP ON A.DeliveryPartyPlantId=DeliPP.Id
                            LEFT JOIN EmployeeInformation AS EI ON A.ResponsiblePersonId=EI.SystemId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            LEFT JOIN TRN.Commitment COM ON COM.Id=A.CommitmentId
							LEFT JOIN [MST].[ProductMaster] PM ON COM.ProductMasterId=PM.Id
                            LEFT JOIN HKP.OrderStatus OS ON OS.Id=A.OrderStatusId
                            LEFT JOIN hkp.OrderCategory AS oc ON oc.Id=a.OrderCategoryId
                            LEFT JOIN HKP.Buyer B ON B.Id=A.BuyerId
                            WHERE A.CompanyId='" + CompanyId + @"'";
            return _sqlRepository.GetDataCollection(sql);
        }

      
        public IEnumerable<object> GetSOData(string MasterOrderId)
        {

            string sql = @"SELECT SO.Id,SO.ParentId
                            , SO.MasterOrderItemId
                            , MOI.MaterialMasterId
                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), SO.DeliveryDate, 106),' ','-')
                            , SO.DestinationId, D.UserName Destination
                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), SO.CommitmentDate, 106),' ','-')
                            , SO.ShipmentModeId
                            , SO.CustomerPOId
		                    , po.PONumber BuyerPo
                            ,MOI.TotalQty MOIQty
                            ,SO.DestinationDescription
                            , SO.OrderStatusId, SO.OrderCategoryId
                            , SO.SOType, SO.ResponsiblePersonId
                            , SO.UpCharge, SO.Qty, SO.Rate, SO.IsFirstEntry,SO.Discount,EMP.EmployeeName ResponsiblePersonName
                            ,FORMAT (SO.LSD, 'dd-MMM-yyyy') as LSD ,FORMAT (SO.MainRawMaterialInhouseDate, 'dd-MMM-yyyy') as MainRawMaterialInhouseDate
                            ,FORMAT (SO.OtherRawMaterialInhouseDate, 'dd-MMM-yyyy') as OtherRawMaterialInhouseDate
                            ,FORMAT (SO.PlanExFactoryDate, 'dd-MMM-yyyy') as PlanExFactoryDate
                            , hasFirst=(SELECT ISNULL(COUNT(DISTINCT SalesOrderId),0) FROM [TRN].[FirstCharacteristics] WHERE SalesOrderId=SO.Id)                            
                            ,(SELECT ISNULL(sum(Qty),0) FROM TRN.FirstCharacteristics AS FCS WHERE SO.Id= FCS.SalesOrderId) SKUQty
                            , isTax=(SELECT ISNULL(COUNT(DISTINCT SalesOrderId),0) FROM [TRN].[SalesOrderTax] WHERE SalesOrderId=SO.Id)
                            ,ISNULL(POD.ProductionOrderId,'') ProductionOrderId,SO.Reason,SO.Description,SO.CM,SO.SalesOrderYear,SO.WeekNo
                            ,SO.ProductionBookedQty,SO.ProductionBookingLevel,SO.SalesExpense,C.Code As Currency,MMA.StandardName Article,MOI.BuyerReferenceNo,SO.CM,SO.ApprovedStatus,SO.CheckByStatus,SO.CheckByDate,SO.ApproveBy,SO.ApproveByDate
							,SO.Qty-(isnull(OtherDispatchQty.ScanQty,0) + isnull(sm.DispatchQty,0)) as BalanceToDispatch
                    FROM [TRN].[SalesOrder] AS SO
                    JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
                    LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=MOI.ArticleId
					left outer join TRN.MasterOrder MO on Mo.Id=MOI.MasterOrderId
					left outer join SCS.Currency C on C.Id=MO.CurrencyId
                    LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                    LEFT JOIN dbo.EmployeeInformation AS EMP ON EMP.SystemId = SO.ResponsiblePersonId
                    LEFT JOIN TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
                    LEFT JOIN [MST].[Destination] D ON D.Id=SO.DestinationId
					 left join
                            (
                            Select SalesOrderId , SUM(isnull(sm.TransactionQty , 0)) as DispatchQty
                            from trn.SalesMaterial sm
                            group by SalesOrderId
                            ) as sm on sm.SalesOrderId = so.Id
					left join (select PLI.SOId, sum(isc.NetWeight) ScanQty
                from itemscanchild isc
                left join trn.POLotReference PLR on PLR.Id = isc.PackingId
                left join trn.PackingLineItem pli on pli.PackingLineItemId = PLR.PackingLineItemId
                    where isc.IsDespatch = 0
                    group by PLI.SOId) OtherDispatchQty on OtherDispatchQty.SOId = SO.Id
                    WHERE  MOI.MasterOrderId='" + MasterOrderId + @"' ORDER BY SO.DeliveryDate";
            return _sqlRepository.GetDataCollection(sql);
        }

    }
    public class ControlType 
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string Controltype { get; set; }
        public string ControltypeDesc { get; set; }
        public string Form { get; set; }
        public string ResponsiblePersonId { get; set; }
        public string DependentDate { get; set; }
        public string CompanyGroupId { get; set; }
        public int days { get; set; }
        public int LagDays { get; set; }



        #endregion Scalar Properties

        #region Audit Properties

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties

    }

    public class OrderControlData 
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string ControlTypeId { get; set; }
        public string SalesOrderId { get; set; }
        public string ProductionOrderId { get; set; }
        public string Remarks { get; set; }
        public string CriticalityLevel { get; set; }
        public string Status { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties
    }

    public class OrderControlRemarks 
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string OrderControlId { get; set; }

        public string Remarks { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties
    }
}
