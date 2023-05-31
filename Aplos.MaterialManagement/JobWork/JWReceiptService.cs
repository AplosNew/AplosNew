using System;
using System.Collections.Generic;
using Library.Data.Sql;
using System.Data;
using OTSBD;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data;
using Library.Service.Logs;
using System.Reflection;
using Library.Service.Enums;

//using Library.Service.Extension;
using Library.Service.Helpers;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Library.MaterialManagement.JobWork
{

    public class JWReceiptService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        string TableName = "dbo.JobWorkReceiptValueAdded";
        string TableName1 = "dbo.JobWorkReceiptValueAddedChild";

        public JWReceiptService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        // RECEIPT

        public IEnumerable<object> GetListOfPOGateEntry(string CompanyGroupId, string CompanyId, string PlantId, string partyCode)
        {
            try
            {

                var Sql = @"Select
                            GE.Id
                            ,REPLACE(CONVERT(CHAR(11), GE.EntryDate , 106),' ','-') AS EntryDate
                            ,P.Code PartyCode
                            ,GE.InvoicingPartyPlantId
                            ,GE.InvoicingByAddress
                            ,GE.DeliveryPartyPlantId
                            ,GE.DeliveryByAddress
                            ,GE.Description
                            ,GE.PackageQty
                            ,GE.ModeofTransport
                            ,GE.Bill
                            ,GE.PersonName
                            ,MobileNo
                            ,GE.Remarks
                            ,GE.AddedBy
                            ,p.UserName
                            ,p.Id as PartyId
                            FROM TRN.GateEntry GE
                            left Join hkp.Party p on p.Id=GE.PartyId
                            Where GE.CompanyGroupId='" + CompanyGroupId + "' AND GE.CompanyId='" + CompanyId + "' AND GE.PlantId='" + PlantId + "' and p.Id='" + partyCode + "' and GE.GateEntryType='Vendor' AND isnull(GE.Id,'') not in (select isnull(GateEntryNo, '') from trn.InventoryReceive) Order By GE.EntryDate DESC";
                //AND GE.Id not in(select GateEntryNo from trn.InventoryReceive)
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetListGateEntry(string CompanyGroupId, string CompanyId, string PlantId, string partyCode)
        {
            try
            {

                var Sql = @"Select
                        GE.Id
                        ,REPLACE(CONVERT(CHAR(11), GE.EntryDate , 106),' ','-') AS EntryDate
                        ,P.Code PartyCode
                        ,GE.InvoicingPartyPlantId
                        ,GE.InvoicingByAddress
                        ,GE.DeliveryPartyPlantId
                        ,GE.DeliveryByAddress
                        ,GE.Description
                        ,GE.PackageQty
                        ,GE.ModeofTransport
                        ,GE.Bill
                        ,GE.PersonName
                        ,MobileNo
                        ,GE.Remarks
                        ,GE.AddedBy
                        ,p.UserName
                        ,p.Id,GE.PartyId
                        FROM TRN.GateEntry GE
                        left Join hkp.Party p on p.Id=GE.PartyId
                        Where GE.CompanyGroupId='" + CompanyGroupId + "' AND GE.CompanyId='" + CompanyId + "' AND GE.PlantId='" + PlantId + "' and p.Id='" + partyCode + "' and GE.GateEntryType='Vendor' and GE.GateEntryType='Vendor' AND isnull(GE.Id,'') not in (select isnull(GateEntryNo, '') from trn.InventoryReceive) Order By GE.EntryDate DESC";
                //AND GE.Id not in(select GateEntryNo from trn.InventoryReceive)
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetIndividualReportData(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select distinct rt.Id,tc.Id as ContractId, rt.Date, FORMAT(rt.Date,'dd-MMM-yyyy') as ReceiveDate, rt.ByWhomId, rt.DocumentReferenceNo,rt.InvoiceNo, rt.GateEntryNoId 
                   ,rt.Remarks, FORMAT(rt.DocumentDate,'dd-MMM-yyyy') as ReceiveDocumentDate, FORMAT(rt.InvoiceDate,'dd-MMM-yyyy') as ReceiveInvoiceDate
                   ,emp.EmployeeName, emp.EmployeeCode
                    from dbo.JobWorkReceiptTransformation rt left join dbo.JobWorkReceiptTransformationChild rtc on rt.Id=rtc.JobWorkReceiptTransformationMasterId
                    left join dbo.EmployeeInformation emp on emp.SystemId=rt.ByWhomId
					left join TRN.GateEntry ge on ge.Id=rt.GateEntryNoId
                    left join dbo.OSTransformationPODetail mp on mp.Id=rtc.MaterialPlanningId
                    left join dbo.JobWorkTransformationContract tc on tc.Id=mp.OSTransformationPOId
                    where tc.Id='" + Id + @"' order by rt.Date desc ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> GetIndividualValAddedReportData(string Id, string ReceivedId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //           string sql = @"select distinct rt.Id,rtc.Id as ReceiveId,rtc.TransformationContractId as ContractId, rt.PODate, FORMAT(rtc.GRNDate,'dd-MMM-yyyy') as ReceiveDate,rtc.ByWhomEmployeeId
                //               , rtc.DocRefNo,rtc.InvoiceNo, rtc.GateEntryNo 
                //              ,rt.Remarks, FORMAT(rtc.DocDate,'dd-MMM-yyyy') as ReceiveDocumentDate, FORMAT(rtc.InvoiceDate,'dd-MMM-yyyy') as ReceiveInvoiceDate
                //              ,emp.EmployeeName, emp.EmployeeCode, ISNULL(kk.TotalIssuedQty,'0') as TotalReceivedQty
                //               from dbo.OSTransformationPO rt left join TRN.InventoryReceive rtc on rt.Id=rtc.TransformationContractId
                //               left join dbo.EmployeeInformation emp on emp.SystemId=rtc.ByWhomEmployeeId
                //left join TRN.GateEntry ge on ge.Id=rtc.GateEntryNo
                //left join TRN.InventoryReceiveDetail ird on ird.InventoryReceiveId=rtc.Id
                //               left join dbo.OSTransformationPODetail mp on mp.Id=ird.OSTransformationPODetailId
                //       --        left join dbo.JobWorkTransformationContract tc on tc.Id=mp.OSTransformationPOId
                //left join(select Sum(IRD.TransactionQty) as TotalIssuedQty, IM.MaterialMasterId,mm.UserName as Material,mma.StandardName as Article
                //                               , IM.ArticleId,IRD.InventoryMaterialId,IRD.OSTransformationPODetailId                                       
                //                               from TRN.InventoryReceive IR inner join TRN.InventoryReceiveDetail IRD on IR.Id = IRD.InventoryReceiveId
                //                                   left join TRN.InventoryMaterial IM on IM.Id = IRD.InventoryMaterialId
                //                                   left join MST.MaterialMaster mm on mm.Id = IM.MaterialMasterId
                //                                   left join MST.MaterialMasterArticle mma on mma.Id = IM.ArticleId
                //                                   where IR.TransformationContractId = '"+ Id + @"'
                //                                   group by IM.MaterialMasterId,IM.ArticleId,IRD.InventoryMaterialId,mm.UserName,mma.StandardName,IRD.OSTransformationPODetailId)
                //					kk on kk.InventoryMaterialId = ird.InventoryMaterialId and kk.OSTransformationPODetailId=mp.Id
                //               where rtc.TransformationContractId='"+ Id + @"' --and rtc.Id='"+ ReceivedId + @"' 
                //               order by rt.PODate desc  ";

                string sql = @"select distinct rt.Id,rtc.Id as ReceiveId,rtc.TransformationContractId as ContractId, rt.PODate, FORMAT(rtc.GRNDate,'dd-MMM-yyyy') as ReceiveDate,rtc.ByWhomEmployeeId
                    , rtc.DocRefNo,rtc.InvoiceNo, rtc.GateEntryNo 
                   ,rt.Remarks, FORMAT(rtc.DocDate,'dd-MMM-yyyy') as ReceiveDocumentDate, FORMAT(rtc.InvoiceDate,'dd-MMM-yyyy') as ReceiveInvoiceDate
                   ,emp.EmployeeName, emp.EmployeeCode--, ISNULL(kk.TotalIssuedQty,'0') as TotalReceivedQty
				   , ISNULL(RR.TotalIssuedQty,'0') as TotalReceivedQty
                    from dbo.OSTransformationPO rt left join TRN.InventoryReceive rtc on rt.Id=rtc.TransformationContractId
                    left join dbo.EmployeeInformation emp on emp.SystemId=rtc.ByWhomEmployeeId
					left join TRN.GateEntry ge on ge.Id=rtc.GateEntryNo
					left join TRN.InventoryReceiveDetail ird on ird.InventoryReceiveId=rtc.Id
                    left join dbo.OSTransformationPODetail mp on mp.Id=ird.OSTransformationPODetailId

					  left join(select Sum(IRD.TransactionQty) as TotalIssuedQty,IRD.InventoryReceiveId                                   
                                    from TRN.InventoryReceive IR inner join TRN.InventoryReceiveDetail IRD on IR.Id = IRD.InventoryReceiveId
                                        left join TRN.InventoryMaterial IM on IM.Id = IRD.InventoryMaterialId
                                        left join MST.MaterialMaster mm on mm.Id = IM.MaterialMasterId
                                        left join MST.MaterialMasterArticle mma on mma.Id = IM.ArticleId
                                        where IR.TransformationContractId = '" + Id + @"'
                                        group by IRD.InventoryReceiveId)
										RR on RR.InventoryReceiveId=rtc.Id

                    where rtc.TransformationContractId='" + Id + @"' --and rtc.Id='' 
                    order by rt.PODate desc    ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> GetReceiptVAChildData(string PKId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select
                        tc.Id JWTransformationPOId
                        , mp.Id JWTransformationPODetailId
                         , jwi.UserName as JWOutputItem
                        ,jwa.UserName as JobWorkActivity
                        , MGM.UserName AS MaterialGroupMasterName
                        , MM.Id MaterialMasterId
                        , MM.UserName
                        , mma.Id ArticleId
                        , mma.StandardName as StandardName
                        ,null MaterialStorageId
                        --,TUoM.Id BaseUOMId
						,BaseUOMId=case when mp.BaseUOMId is not null then TUoM.Id else TUoMM.Id End

						, MOI.MasterOrderId, MO.MasterOrderNo, SO.MasterOrderItemId,moi.BuyerReferenceNo,moi.OwnReferenceNo,mo.BuyerReferenceNo BuyerOrderNo,mo.OwnReferenceNo AS OwnOrderNo
	                            , SO.Id AS SalesOrderId, Pr.UserName AS Customer,B.UserName AS Buyer,PM.Id AS ProductID,isnull(MOI.ProductionGrouping,'') AS ProductionGrouping
	                            --, MOI.MaterialMasterId, MM.UserName AS MaterialMasterName
								,PM.UserName AS ProductName
	                            --, MOI.ArticleId, ART.StandardName AS ArticleName
								,CN.ContractNo,MLC.LCRef MasterLCNo, owrUom.UserName as MasterOrderUoM
                               ,owr.Id as JWOrderWiseId, owr.JWTransformationPODetailId, owr.OrderType,owr.Quantity as OWRQuantity--,owr.PlanQuantity

                        ,mp.FirstCharacteristicsId,mp.FirstCharacteristicsValueId
						   ,ISNULL(FChar.UserName,'') FirstCharacteristics,ISNULL(FCharValue.UserName,'') FirstCharacteristicsValue
						   ,mp.SecondCharacteristicsId,mp.SecondCharacteristicsValueId
                                ,ISNULL(SChar.UserName,'') SecondCharacteristics,ISNULL(SCharValue.UserName,'') SecondCharacteristicsValue
								,mp.ThirdCharacteristicsId,mp.ThirdCharacteristicsValueId
                                ,ISNULL(TChar.UserName,'') ThirdCharacteristics,ISNULL(TCharValue.UserName,'') ThirdCharacteristicsValue
                        --, SUM(mp.Quantity) as PlanQuantity
                        --,TotalReceivedQty = ISNULL(kk.TotalReceivedQuantity, '0')
                        --,ToReceive = Sum(mp.Quantity) - ISNULL(kk.TotalReceivedQuantity, '0')
                        --, mp.Quantity AS PlanQuantity
						,PlanQuantity=case when owr.Id is not null then owr.Quantity else mp.Quantity End
                         , ISNULL(rcvqty.TransactionQty, '0') AS GRNRcvQty
                         ,0 AS TransactionQty
                        -- , ISNULL(mp.Quantity, 0)-ISNULL(rcvqty.TransactionQty, '0') As Balance
						 ,Balance=case when owr.Id is not null then ISNULL(owr.Quantity, 0)-ISNULL(rcvqty.TransactionQty, '0') else ISNULL(mp.Quantity, 0)-ISNULL(rcvqty.TransactionQty, '0') End
                           ,null QtyStatus
                         , TransactionUoMId = CASE when mp.OutputMaterialUOMId IS NULL THEN mp.TransactionUoMId ELSE mp.OutputMaterialUOMId END
                          , TransactionUoM = CASE when mp.OutputMaterialUOMId IS NULL then TUoM1.UserName ELSE TUoM.UserName END
                          , 0 TransactionRate
                        , null  CurrencyName
                        , 0 ToCurrencyRate
                        ,0 TransactionAmount
                        ,0 AS TrnAmount
                        ,0 AS BaseTaxAmount
                        ,0 AS TaxAmount
                        , 0 AS ChargesAmount
                        ,0 AS ServiceCharge
                        , 0 AS ServiceTax
                        , null CountryId
                        ,'True' enableid
                        ,null POMaterialTaxList
                        ,0 AS TotalMaterialTranAmount
                        , 0 AS ToTalMaterialBooksCurrencyAmount
                        ,null InvoicingByAddress
                        ,null DeliveryByAddress
                        ,null RequisitionId
                        ,null RequisitionDetailId
                        ,0 ShortageQty
                        ,0 RejectionQty
                        ,null MaterialDetail
                        ,null AS[check]
                        ,null MaterialDetail
                        ,null PurchaseDocAcceptanceDetailId
                        ,0 POClosStatus
                        ,null CountryName
                        ,null CountryId
                        ,MM.IsAsset
                        ,0 TotalTaxAmount
                        ,0 GrossAmount
                        ,0 DiscountAmount
                        ,'' QualityStatus
                        ,null POUoMId
                        ,0 Tolerance
						,vvvv.ConsumptionAmount as GrossConsumption
                        ,vvvv.Rate as IssueRate
                        from dbo.JWTransformationPODetail mp
                        left join dbo.JWTransformationPO tc on tc.Id = mp.JWTransformationPOId

                   left join hkp.JobWorkActivity jwa on jwa.Id = mp.JobActivityId

                   left join HKP.JobWorkItem jwi on jwi.Id = mp.JobWorkItemMasterId

                   left join MST.MaterialMasterArticle mma on mma.Id = mp.ArticleId

                   left JOIN MST.MaterialMaster AS MM ON MM.Id = mma.MaterialMasterId
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id

                        LEFT JOIN[SCS].[UnitOfMeasurement] AS TUoMM ON mp.OutputMaterialUOMId = TUoMM.Id
						LEFT JOIN[SCS].[UnitOfMeasurement] AS TUoM ON mp.BaseUOMId = TUoM.Id

                            LEFT JOIN[SCS].[UnitOfMeasurement] AS TUoM1 ON mp.TransactionUoMId = TUoM1.Id

							left join dbo.JWTransformationPOMasterOrderItem owr on owr.JWTransformationPODetailId=mp.Id
							   left join [TRN].[SalesOrder] AS SO on SO.Id=owr.SalesOrderId
							   left JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
							   left JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
							   LEFT JOIN [MST].[MaterialMaster] ON MOI.MaterialMasterId = MM.Id 
							   LEFT JOIN trn.ProductDefinition AS pd ON pd.MaterialMasterId=moi.MaterialMasterId
							   LEFT JOIN [MST].[ProductMaster] PM ON pm.Id=pd.ProductMasterId
							   LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id
							   LEFT JOIN [HKP].[Party] AS Pr ON MO.PartyId = Pr.Id
							   LEFT JOIN HKP.BUYER b on b.Id=MO.BuyerId
							   --LEFT JOIN [MST].[Destination] AS DEST ON SO.DestinationId = DEST.Id
							   --LEFT JOIN [MST].[ShipMode] AS SHP ON SO.ShipmentModeId = SHP.Id
							   --LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
							   --LEFT JOIN [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
							   --LEFT JOIN [HKP].[OrderCategory] AS OC ON SO.OrderCategoryId = OC.Id
							   LEFT JOIN dbo.[Contract] AS CN ON CN.Id=MOI.ContractId
							   LEFT JOIN dbo.MasterLC AS MLC ON MLC.Id=CN.MasterLCId
							   left join SCS.UnitOfMeasurement owrUom on owrUom.Id=MO.TotalQtyUOMId

                         LEFT JOIN [HKP].[Characteristics]  FChar  ON FChar.Id = mp.FirstCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   FCharValue  ON FCharValue.Id = mp.FirstCharacteristicsValueId
                            LEFT JOIN [HKP].[Characteristics]   SChar  ON SChar.Id = mp.SecondCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   SCharValue  ON SCharValue.Id = mp.SecondCharacteristicsValueId
                            LEFT JOIN [HKP].[Characteristics]   TChar  ON TChar.Id = mp.ThirdCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   TCharValue  ON TCharValue.Id = mp.ThirdCharacteristicsValueId

                        left join(select JWTransformationPODetailId, Sum(isnull(TransactionQty,0)) TransactionQty from trn.InventoryReceiveDetail group by JWTransformationPODetailId)rcvqty ON rcvqty.JWTransformationPODetailId = mp.Id
                        left join(select IID.JWTransformationPOId, II.JobWorkContractId

                                 , sum(IID.PolicyAmount) PolicyAmt, sum(IID.TransactionQty) TQty
                                 , Rate= round((sum(IID.PolicyAmount) / sum(IID.TransactionQty)), 4)
                                 , ConsumptionAmount= (round((sum(IID.PolicyAmount) / sum(IID.TransactionQty)), 4) * sum(IID.TransactionQty))

                                 FROM trn.InventoryIssueDetail IID

                                 left join trn.InventoryIssue II On II.Id= IID.InventoryIssueId

                                 left join trn.InventoryMaterial IM ON IM.Id= IID.InventoryMaterialId

                                 left JOIN MST.MaterialMaster AS MM ON MM.Id= IM.MaterialMasterId

                                 left join MST.MaterialMasterArticle mma on mma.Id= IM.ArticleId

                                 left join dbo.JWTransformationPO tc on tc.Id= II.JobWorkContractId

                                 where II.JobWorkContractId= '"+ PKId + @"'

                                 group by II.JobWorkContractId, IID.JWTransformationPOId
                                 )vvvv ON vvvv.JobWorkContractId = tc.Id and vvvv.JWTransformationPOId = mp.Id
                        where tc.Id = '"+ PKId + @"'
                         group by mp.Quantity ,ISNULL(rcvqty.TransactionQty, '0'),mp.Id,jwi.UserName, mma.StandardName,jwa.UserName--,kk.TotalReceivedQuantity
                        , MGM.UserName, MM.Id, MM.UserName, mma.Id ,MM.IsAsset,tc.Id, TUoM.Id, TUoM.UserName,TUoM.Id,TUoM1.Id,TUoM1.UserName,mp.TransactionUoMId
						, mp.OutputMaterialUOMId ,vvvv.ConsumptionAmount,FChar.UserName,FCharValue.UserName,SChar.UserName,SCharValue.UserName ,TChar.UserName,TCharValue.UserName
						 ,mp.FirstCharacteristicsId,mp.FirstCharacteristicsValueId,mp.SecondCharacteristicsId,mp.SecondCharacteristicsValueId
						 ,mp.ThirdCharacteristicsId,mp.ThirdCharacteristicsValueId
						 , MOI.MasterOrderId, MO.MasterOrderNo, SO.MasterOrderItemId,moi.BuyerReferenceNo,moi.OwnReferenceNo,mo.BuyerReferenceNo,mo.OwnReferenceNo
	                            , SO.Id, Pr.UserName,B.UserName,PM.Id,MOI.ProductionGrouping
	                            --, MOI.MaterialMasterId, MM.UserName AS MaterialMasterName
								,PM.UserName
	                            --, MOI.ArticleId, ART.StandardName AS ArticleName
								,CN.ContractNo,MLC.LCRef, owrUom.UserName,owr.Id, owr.JWTransformationPODetailId, owr.OrderType,owr.Quantity
								--,owr.PlanQuantity
                                ,mp.BaseUOMId,TUoMM.Id,vvvv.Rate
                                 ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> GetReceiptVAChildDatabyId(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select rvc.Id,rvc.ContractLineItemId,rvc.OrderChildId, jwi.UserName as JWOutputItem, mma.StandardName as Article, mp.OrderSpecific
                                        ,TotalIssuedQty= case when mp.OrderSpecific='Yes' then (ISNULL(kk.TotalIssuedQty,'0')) else (ISNULL(k.TIssuedQty,'0')) end
                                       ,TotalReceivedQty= case when mp.OrderSpecific='Yes' then (ISNULL(rq.TotalReceQty,'0')) else (ISNULL(r.TRQty,'0')) end
									   ,ToReceive= case when mp.OrderSpecific='Yes' then kk.TotalIssuedQty- rq.TotalReceQty else k.TIssuedQty - r.TRQty end
                                        ,rvc.ReceivedQuantity as ReceivedQty
                                         from dbo.JobWorkReceiptValueAddedChild rvc left join dbo.JobWorkIssueReturnChild irc on 
										 irc.OrderChildId=rvc.OrderChildId
										 left join(select Sum(Quantity) as TotalIssuedQty, OrderChildId from dbo.JobWorkIssueReturnChild group by OrderChildId )
										 kk on kk.OrderChildId=rvc.OrderChildId
										 left join(select Sum(Quantity) as TIssuedQty,ContractLineItemId from dbo.JobWorkIssueReturnChild group by ContractLineItemId )
										 k on k.ContractLineItemId=rvc.ContractLineItemId
										 left join(select Sum(ReceivedQuantity) as TotalReceQty, OrderChildId from dbo.JobWorkReceiptValueAddedChild group by OrderChildId )
										 rq on rq.OrderChildId=rvc.OrderChildId
										 left join(select Sum(ReceivedQuantity) as TRQty,ContractLineItemId from dbo.JobWorkReceiptValueAddedChild group by ContractLineItemId )
										 r on r.ContractLineItemId=rvc.ContractLineItemId
										 left join dbo.JobWorkValueAddedContractChild mp on mp.Id=rvc.ContractLineItemId
										 left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
									     left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleCodeId
                                         where rvc.JobWorkReceiptValueAddedMasterId='" + Id + @"'
										 group by rvc.Id,rvc.ContractLineItemId,rvc.OrderChildId,rvc.ReceivedQuantity,kk.TotalIssuedQty,k.TIssuedQty,rq.TotalReceQty, r.TRQty, mp.OrderSpecific,jwi.UserName,mma.StandardName ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> GetGradeWiseQuantityList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select * from dbo.GradeWiseQuantityDetails order by GradeNo ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetVAGradeWiseQuantityList(string MasterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select gw.*,gwd.GradeName from dbo.ReceiptValueAddedGradeWise gw left join dbo.JobWorkReceiptValueAddedChild rvc on rvc.Id=gw.JobWorkReceiptValueAddedChildMasterId
                                           left join dbo.GradeWiseQuantityDetails gwd on gwd.Id=gw.GradeWiseQuantityId
										   where gw.JobWorkReceiptValueAddedChildMasterId='" + MasterId + @"' ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetTransformationReceiptCurrency(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select c.Id as Value, c.Code as Text from SCS.Currency c left join dbo.JWTransformationPO po on c.Id=po.CurrencyId
                               where po.Id='"+ Id + @"' ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobWorkReceiptValueAdded", out sID);
            return sID;
        }

        public void Create(Dictionary<string, object> data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");               

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = "RV" + GetPK();

                    dr["Date"] = data["Date"];
                    dr["ByWhomId"] = data["ByWhomId"];
                    dr["DocumentReferenceNo"] = data["DocumentReferenceNo"];
                    dr["DocumentDate"] = data["DocumentDate"];

                    dr["InvoiceNo"] = data["InvoiceNo"];
                    dr["InvoiceDate"] = data["InvoiceDate"];
                    dr["GateEntryNoId"] = data["GateEntryNoId"];
                    dr["Remarks"] = data["Remarks"];

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();

                    dr["Date"] = data["Date"];
                    dr["ByWhomId"] = data["ByWhomId"];
                    dr["DocumentReferenceNo"] = data["DocumentReferenceNo"];
                    dr["DocumentDate"] = data["DocumentDate"];

                    dr["InvoiceNo"] = data["InvoiceNo"];
                    dr["InvoiceDate"] = data["InvoiceDate"];
                    dr["GateEntryNoId"] = data["GateEntryNoId"];
                    dr["Remarks"] = data["Remarks"];

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dr.EndEdit();
                }
                data["Id"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetReceiptVCPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobWorkReceiptValueAddedChild", out sID);
            return sID;
        }

        public void SaveReceiptVAChildTab(IEnumerable<jobworkreceiptvalueaddedchild> ReceiptVAChildData, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet ExistOrNot;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var MPId = "' '";
                var OWRId = "''";
                foreach (var empitem in ReceiptVAChildData)
                {
                    MPId += ",'" + empitem.ContractLineItemId + "' ";
                    OWRId += ",'" + empitem.OrderChildId + "' ";
                }
                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where (ContractLineItemId IN ( " + MPId + " ) or OrderChildId IN (" + OWRId + ")) and JobWorkReceiptValueAddedMasterId='" + MasterId + "'  ", out ExistOrNot, false, "1");

                foreach (var item in ReceiptVAChildData)
                {
                    if (ExistOrNot.Tables[0].DefaultView.Count == 0 || ExistOrNot.Tables[0].DefaultView.Count > 0)
                    {
                        DataRow dr = ExistOrNot.Tables[0].NewRow();
                        dr["Id"] = "RVA" + GetReceiptVCPK();

                        dr["JobWorkReceiptValueAddedMasterId"] = MasterId;

                        dr["ContractLineItemId"] = item.ContractLineItemId;
                        dr["OrderChildId"] = item.OrderChildId;
                        dr["ReceivedQuantity"] = item.ReceivedQty;

                        dr["Remarks"] = item.Remarks;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        ExistOrNot.Tables[0].Rows.Add(dr);

                    }

                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(ExistOrNot);

            }
            catch (Exception ex)
            {

            }
        }

        // GRADE WISE VALUE ADDED

        private string GetGradeWiseVAPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ReceiptValueAddedGradeWise", out sID);
            return sID;
        }

        public void SaveGradeWiseValueAdded(IEnumerable<ReceiptValueAddedGradeWise> VAGradeWiseData, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet ExistOrNot;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from dbo.ReceiptValueAddedGradeWise where JobWorkReceiptValueAddedChildMasterId='" + MasterId + "'  ", out ExistOrNot, false, "1");

                foreach (var item in VAGradeWiseData)
                {
                    if (ExistOrNot.Tables[0].DefaultView.Count == 0 || ExistOrNot.Tables[0].DefaultView.Count > 0)
                    {
                        DataRow dr = ExistOrNot.Tables[0].NewRow();
                        dr["Id"] = "GV" + GetGradeWiseVAPK();

                        dr["JobWorkReceiptValueAddedChildMasterId"] = MasterId;

                        dr["GradeWiseQuantityId"] = item.Id;
                        dr["GradeWiseQuantity"] = item.GradeWQty;
                        dr["Remarks"] = item.GWRemarks;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        ExistOrNot.Tables[0].Rows.Add(dr);

                    }

                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(ExistOrNot);

            }
            catch (Exception ex)
            {

            }
        }

        // RECEIPT TRANSFORMATION

        private string GetReceiptTPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobWorkReceiptTransformation", out sID);
            return sID;
        }

        public void SaveReceiptTransformation(Dictionary<string, object> data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from TRN.InventoryReceive where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "InventoryReceive", out _Id);

                    data["Id"] =  _Id;

                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();

                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                //#region data update
                //if (dsMaster.Tables[0].Rows.Count == 0)
                //{
                //    DataRow dr = dsMaster.Tables[0].NewRow();
                //    dr["Id"] = "RT" + GetReceiptTPK();

                //    dr["Date"] = data["Date"];
                //    dr["ByWhomId"] = data["ByWhomId"];
                //    dr["DocumentReferenceNo"] = data["DocumentReferenceNo"];
                //    dr["DocumentDate"] = data["DocumentDate"];

                //    dr["InvoiceNo"] = data["InvoiceNo"];
                //    dr["InvoiceDate"] = data["InvoiceDate"];
                //    dr["GateEntryNoId"] = data["GateEntryNoId"];
                //    dr["Remarks"] = data["Remarks"];

                //    dr["AddedBy"] = identity.Name;
                //    dr["AddedDate"] = System.DateTime.Now.ToString();
                //    dr["AddedFromIP"] = identity.IPAddress;
                //    dr["UpdatedBy"] = identity.Name;
                //    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                //    dr["UpdatedFromIP"] = identity.IPAddress;


                //    dsMaster.Tables[0].Rows.Add(dr);
                //}
                //else
                //{
                //    //edit
                //    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                //    dr.BeginEdit();

                //    dr["Date"] = data["Date"];
                //    dr["ByWhomId"] = data["ByWhomId"];
                //    dr["DocumentReferenceNo"] = data["DocumentReferenceNo"];
                //    dr["DocumentDate"] = data["DocumentDate"];

                //    dr["InvoiceNo"] = data["InvoiceNo"];
                //    dr["InvoiceDate"] = data["InvoiceDate"];
                //    dr["GateEntryNoId"] = data["GateEntryNoId"];
                //    dr["Remarks"] = data["Remarks"];

                //    dr["AddedBy"] = identity.Name;
                //    dr["AddedDate"] = System.DateTime.Now.ToString();
                //    dr["AddedFromIP"] = identity.IPAddress;
                //    dr["UpdatedBy"] = identity.Name;
                //    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                //    dr["UpdatedFromIP"] = identity.IPAddress;


                //    dr.EndEdit();
                //}
                //data["Id"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                //#endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

            }
            catch (Exception ex)
            {
                throw ex;
            }
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

        // RECEIPT TRANSFORMATION CHILD DATA

        public IEnumerable<object> GetReceiptTransChildData(string PKId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select  tc.Id JWTransformationPOId  ,mp.Id JWTransformationPODetailId  ,jwi.UserName as JWOutputItem ,jwa.UserName as JobWorkActivity  , MGM.UserName AS MaterialGroupMasterName
                        , MM.Id MaterialMasterId  , MM.UserName  , mma.Id ArticleId  , mma.StandardName as StandardName  ,null MaterialStorageId  ,TUoM.Id BaseUOMId 
						,mp.FirstCharacteristicsId ,mp.FirstCharacteristicsValueId ,ISNULL(FChar.UserName,'') FirstCharacteristics,ISNULL(FCharValue.UserName,'') FirstCharacteristicsValue  
						,mp.SecondCharacteristicsId,mp.SecondCharacteristicsValueId ,ISNULL(SChar.UserName,'') SecondCharacteristics,ISNULL(SCharValue.UserName,'') SecondCharacteristicsValue ,mp.ThirdCharacteristicsId,mp.ThirdCharacteristicsValueId
                        ,ISNULL(TChar.UserName,'') ThirdCharacteristics,ISNULL(TCharValue.UserName,'') ThirdCharacteristicsValue , mp.Quantity AS PlanQuantity
                         , ISNULL(rcvqty.TransactionQty,'0') AS GRNRcvQty  ,0 AS TransactionQty ,ISNULL(mp.Quantity,0)- ISNULL(rcvqty.TransactionQty,'0') As Balance
                        ,null QtyStatus , TransactionUoMId=CASE when mp.OutputMaterialUOMId IS NULL THEN mp.TransactionUoMId ELSE mp.OutputMaterialUOMId END
                        , TransactionUoM= CASE when mp.OutputMaterialUOMId  IS NULL then TUoM1.UserName ELSE TUoM.UserName END
                        , 0 TransactionRate , null  CurrencyName , 0 ToCurrencyRate ,0 TransactionAmount ,0 AS TrnAmount  ,0 AS BaseTaxAmount ,0 AS TaxAmount
                        , 0 AS ChargesAmount ,0 AS  ServiceCharge  , 0 AS ServiceTax  , null CountryId  ,'True' enableid ,null POMaterialTaxList ,0 AS TotalMaterialTranAmount
                        , 0 AS ToTalMaterialBooksCurrencyAmount ,null InvoicingByAddress ,null DeliveryByAddress ,null RequisitionId ,null RequisitionDetailId ,0 ShortageQty
                        ,0 RejectionQty ,null MaterialDetail ,null AS [check]  ,null MaterialDetail  ,null PurchaseDocAcceptanceDetailId ,0 POClosStatus ,null CountryName
                        ,null CountryId ,MM.IsAsset ,0 TotalTaxAmount ,0 GrossAmount ,0 DiscountAmount ,'' QualityStatus ,null POUoMId ,0 Tolerance ,vvvv.ConsumptionAmount as GrossConsumption
                        from dbo.JWTransformationPODetail mp 
                        left join dbo.JWTransformationPO tc on tc.Id=mp.JWTransformationPOId
                        left join hkp.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
                        left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
                        left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleId
                        left JOIN MST.MaterialMaster AS MM ON MM.Id=mma.MaterialMasterId
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON mp.OutputMaterialUOMId=TUoM.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM1 ON mp.TransactionUoMId=TUoM1.Id
                        LEFT JOIN [HKP].[Characteristics]  FChar  ON FChar.Id = mp.FirstCharacteristicsId
                        LEFT JOIN [HKP].[CharacteristicsValue]   FCharValue  ON FCharValue.Id = mp.FirstCharacteristicsValueId
                        LEFT JOIN [HKP].[Characteristics]   SChar  ON SChar.Id = mp.SecondCharacteristicsId
                        LEFT JOIN [HKP].[CharacteristicsValue]   SCharValue  ON SCharValue.Id = mp.SecondCharacteristicsValueId
                        LEFT JOIN [HKP].[Characteristics]   TChar  ON TChar.Id = mp.ThirdCharacteristicsId
                        LEFT JOIN [HKP].[CharacteristicsValue]   TCharValue  ON TCharValue.Id = mp.ThirdCharacteristicsValueId
                        left join (
                         select Sum(x.GrossConsumption) as GrossConsumption ,x.JWTransformationPODetailId
									from ( Select GrossConsumption, JWTransformationPODetailId from dbo.JWTransformationPOInputMaterial mi
									left join TRN.InventoryIssueDetail IID on IID.JWTransformationPOId=mi.JWTransformationPODetailId
									left join TRN.InventoryIssue II on II.Id=IID.InventoryIssueId
									where II.JobWorkContractId='"+ PKId + @"'
									group by mi.ArticleId, mi.JWTransformationPODetailId,mi.GrossConsumption
									) x group by x.JWTransformationPODetailId
                         )CC3 ON CC3.JWTransformationPODetailId=mp.Id
                        left join(select JWTransformationPODetailId, Sum(isnull(TransactionQty,0)) TransactionQty from trn.InventoryReceiveDetail group by JWTransformationPODetailId)rcvqty ON rcvqty.JWTransformationPODetailId=mp.Id
                        left join(  select  IID.JWTransformationPOId,II.JobWorkContractId 
								 ,sum(IID.PolicyAmount) PolicyAmt,sum(IID.TransactionQty) TQty
								 ,Rate=round((sum(IID.PolicyAmount) / sum(IID.TransactionQty)),4)
                                 ,ConsumptionAmount= (round((sum(IID.PolicyAmount) / sum(IID.TransactionQty)),4) * sum(IID.TransactionQty))
								 FROM trn.InventoryIssueDetail IID
								 left join trn.InventoryIssue II On II.Id=IID.InventoryIssueId
								 left join trn.InventoryMaterial IM ON IM.Id=IID.InventoryMaterialId
								 left JOIN MST.MaterialMaster AS MM ON MM.Id=IM.MaterialMasterId
								 left join MST.MaterialMasterArticle mma on mma.Id=IM.ArticleId
								 left join dbo.JWTransformationPO tc on tc.Id=II.JobWorkContractId
								 where II.JobWorkContractId='" + PKId + @"'
								 group by II.JobWorkContractId,IID.JWTransformationPOId
                                 )vvvv ON vvvv.JobWorkContractId=tc.Id and vvvv.JWTransformationPOId=mp.Id
                        where tc.Id='" + PKId + @"'
                         group by mp.Quantity ,ISNULL(rcvqty.TransactionQty,'0'),mp.Id,jwi.UserName, mma.StandardName,jwa.UserName 
                        , MGM.UserName, MM.Id, MM.UserName, mma.Id ,MM.IsAsset,tc.Id, TUoM.Id, TUoM.UserName,TUoM.Id,TUoM1.Id,TUoM1.UserName,mp.TransactionUoMId , mp.OutputMaterialUOMId
                         ,vvvv.ConsumptionAmount,FChar.UserName,FCharValue.UserName,SChar.UserName,SCharValue.UserName ,TChar.UserName,TCharValue.UserName
						 ,mp.FirstCharacteristicsId,mp.FirstCharacteristicsValueId,mp.SecondCharacteristicsId,mp.SecondCharacteristicsValueId
						 ,mp.ThirdCharacteristicsId,mp.ThirdCharacteristicsValueId";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        private string GetTransChildPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobWorkReceiptTransformationChild", out sID);
            return sID;
        }

        public void SaveReceiptTransChildTab(IEnumerable<JobWorkReceiptTransformationChild> ReceiptTransChildData, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet ExistOrNot;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from dbo.JobWorkReceiptTransformationChild where JobWorkReceiptTransformationMasterId='" + MasterId + "'  ", out ExistOrNot, false, "1");

                foreach (var item in ReceiptTransChildData)
                {
                    if (ExistOrNot.Tables[0].DefaultView.Count == 0 || ExistOrNot.Tables[0].DefaultView.Count > 0)
                    {
                        DataRow dr = ExistOrNot.Tables[0].NewRow();
                        dr["Id"] = "RTC" + GetTransChildPK();

                        dr["JobWorkReceiptTransformationMasterId"] = MasterId;

                        dr["MaterialPlanningId"] = item.Id;
                        dr["ReceivedQuantity"] = item.ReceivedQty;
                        dr["Remarks"] = item.Remarks;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        ExistOrNot.Tables[0].Rows.Add(dr);

                    }

                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(ExistOrNot);

            }
            catch (Exception ex)
            {

            }
        }

        //  GRADE WISE QUANTITY TRANSFORMATION

        public IEnumerable<object> GetTransGradeQuantityList(string MasterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select gw.*,gwd.GradeName from dbo.ReceiptTransformationGradeWise gw left join dbo.JobWorkReceiptTransformationChild rtc on rtc.Id=gw.JobWorkReceiptTransformationChildMasterId
                                           left join dbo.GradeWiseQuantityDetails gwd on gwd.Id=gw.GradeWiseQuantityId
										   where gw.JobWorkReceiptTransformationChildMasterId='" + MasterId + @"' ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetGradeWiseTransPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ReceiptTransformationGradeWise", out sID);
            return sID;
        }

        public void SaveGradeWiseTrans(IEnumerable<ReceiptTransformationGradeWise> TransGradeWiseData, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet ExistOrNot;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from dbo.ReceiptTransformationGradeWise where JobWorkReceiptTransformationChildMasterId='" + MasterId + "'  ", out ExistOrNot, false, "1");

                foreach (var item in TransGradeWiseData)
                {
                    if (ExistOrNot.Tables[0].DefaultView.Count == 0 || ExistOrNot.Tables[0].DefaultView.Count > 0)
                    {
                        DataRow dr = ExistOrNot.Tables[0].NewRow();
                        dr["Id"] = "GT" + GetGradeWiseTransPK();

                        dr["JobWorkReceiptTransformationChildMasterId"] = MasterId;

                        dr["GradeWiseQuantityId"] = item.Id;
                        dr["GradeWiseQuantity"] = item.GradeWQty;
                        dr["Remarks"] = item.GWRemarks;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        ExistOrNot.Tables[0].Rows.Add(dr);

                    }

                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(ExistOrNot);

            }
            catch (Exception ex)
            {

            }
        }

        public IEnumerable<object> GetReceiptTransChildDatabyId(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select mp.Id, SUM(mp.Quantity) as PlanQuantity,jwi.UserName as JWOutputItem,jwa.UserName as JobWorkActivity, mma.StandardName as Article
,TotalReceivedQty=ISNULL( kk.TotalReceivedQuantity,'0')
,ToReceive= Sum(mp.Quantity)- ISNULL( kk.TotalReceivedQuantity,'0'), rtc.ReceivedQuantity as ReceivedQty, rtc.Id as ReceiptTransChildId, rtc.Remarks
from dbo.OSTransformationPODetail mp left join dbo.JobWorkTransformationContract tc on tc.Id=mp.OSTransformationPOId
left join hkp.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleCodeId
left join (select Sum(ReceivedQuantity) as TotalReceivedQuantity,MaterialPlanningId from dbo.JobWorkReceiptTransformationChild group by MaterialPlanningId)
kk on kk.MaterialPlanningId=mp.Id
left join dbo.JobWorkReceiptTransformationChild rtc on rtc.MaterialPlanningId=mp.Id
where rtc.JobWorkReceiptTransformationMasterId='" + Id + @"'
group by mp.Id,jwi.UserName, mma.StandardName,jwa.UserName,kk.TotalReceivedQuantity,rtc.ReceivedQuantity,rtc.Id,rtc.Remarks ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // BY PRODUCT RECEIPT

        public IEnumerable<object> GetByProductApplicableList(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"select 
                                mi.Id JWTransformationPOInputMaterialId
                                ,tbp.Id JWTransformationPOByProductId
                                ,jwit.UserName as JWOutputItem
                                ,jwi.UserName as ByProductItem
                                , mma.Id ArticleId
                                , mma.StandardName as StandardName
                                , MM.UserName
                                , MM.Id MaterialMasterId
                                ,null MaterialStorageId
                                ,TUoM.Id BaseUOMId
                                , null FirstCharacteristicsId, null  FirstCharacteristics
                                , null FirstCharacteristicsValueId, null  FirstCharacteristicsValue
                                , null SecondCharacteristicsId, null  SecondCharacteristics
                                , null SecondCharacteristicsValueId, null SecondCharacteristicsValue
                                , null ThirdCharacteristicsId, null ThirdCharacteristics
                                , null ThirdCharacteristicsValueId, null  ThirdCharacteristicsValue
                                , sum(((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100)) AS PlanQuantity
                                , Sum(ISNULL(rcvqty.TransactionQty,'0')) AS GRNRcvQty          
                                ,0 AS TransactionQty
                                ,SUM(((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) - ISNULL(rcvqty.TransactionQty,'0')) As Balance
                                ,null QtyStatus
                                , TUoM.Id TransactionUoMId
                                , TUoM.UserName TransactionUoM
                                --, 0 TransactionRate
                                , null  CurrencyName
                                , tbp.StandardRate TransactionRate
                                ,0 TransactionAmount
                                ,0 AS TrnAmount  
                                ,0 AS BaseTaxAmount
                                ,0 AS TaxAmount
                                , 0 AS ChargesAmount
                                ,0 AS  ServiceCharge
                                , 0 AS ServiceTax
                                , null CountryId
                                ,'True' enableid
                                ,null POMaterialTaxList                            
                                ,0 AS TotalMaterialTranAmount
                                , 0 AS ToTalMaterialBooksCurrencyAmount
                                ,null InvoicingByAddress
                                ,null DeliveryByAddress
                                ,null RequisitionId
                                ,null RequisitionDetailId
                                ,0 ShortageQty
                                ,0 RejectionQty
                                ,null MaterialDetail
                                ,null AS [check] 
                                ,null MaterialDetail
                                ,null PurchaseDocAcceptanceDetailId
                                ,0 POClosStatus
                                ,null CountryName
                                ,null CountryId 
                                ,MM.IsAsset
                                ,0 TotalTaxAmount
                                ,0 GrossAmount
                                ,0 DiscountAmount
                                ,'' QualityStatus
                                ,null POUoMId
                                ,0 Tolerance
                                from dbo.JWTransformationPOByProduct tbp 
                                left join HKP.JobWorkItem jwi on jwi.Id=tbp.JobWorkItemId
                                left join dbo.JWTransformationPOInputMaterial mi on mi.Id=tbp.JWTransformationPOInputMaterialId
                                left join dbo.JWTransformationPODetail mp on mp.Id=mi.JWTransformationPODetailId
                                left join HKP.JobWorkItem jwit on jwit.Id=mp.JobWorkItemMasterId
                                left join MST.MaterialMasterArticle mma on mma.Id=tbp.ArticleId
                                left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
                                left join (select Sum(IRD.TransactionQty) as TotalReceivedQuantity,IR.JobWorkContractId from TRN.InventoryReceiveDetail IRD left join TRN.InventoryReceive IR
                                 on IRD.InventoryReceiveId=IR.Id where MaterialFor='JobWorkBYPRODUCTMaterial' group by IR.JobWorkContractId)
                                rvbp on rvbp.JobWorkContractId=mp.JWTransformationPOId
                                --rvbp on rvbp.ByProductId=tbp.Id
                                left join dbo.JWTransformationPO tc on tc.Id=mp.JWTransformationPOId
                                LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON mp.OutputMaterialUOMId=TUoM.Id
                                left join(select JWTransformationPOByProductId, Sum(isnull(TransactionQty,0)) TransactionQty from trn.InventoryReceiveDetail group by JWTransformationPOByProductId)rcvqty ON rcvqty.JWTransformationPOByProductId=tbp.Id
                                where tc.Id='" + Id + @"'
                                group by tbp.Id
                                ,jwit.UserName 
                                ,jwi.UserName 
                                , mma.Id 
                                , mma.StandardName 
                                , MM.UserName
                                , MM.Id 
                                ,MM.IsAsset,mi.Id,TUoM.Id, TUoM.UserName,TUoM.Id, tbp.StandardRate";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetByProductPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobWorkReceiptTransformationByProduct", out sID);
            return sID;
        }

        public void SaveByProduct(IEnumerable<JobWorkReceiptTransformationByProduct> ByProductData, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet ExistOrNot;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from dbo.JobWorkReceiptTransformationByProduct where JobWorkReceiptTransformationMasterId='" + MasterId + "'  ", out ExistOrNot, false, "1");

                foreach (var item in ByProductData)
                {
                    if (ExistOrNot.Tables[0].DefaultView.Count == 0 || ExistOrNot.Tables[0].DefaultView.Count > 0)
                    {
                        DataRow dr = ExistOrNot.Tables[0].NewRow();
                        dr["Id"] = "RBP" + GetByProductPK();

                        dr["JobWorkReceiptTransformationMasterId"] = MasterId;

                        dr["ByProductId"] = item.Id;
                        dr["ReceivedQuantity"] = item.ReceiveQuantity;
                        dr["Remarks"] = item.Remarks;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        ExistOrNot.Tables[0].Rows.Add(dr);

                    }

                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(ExistOrNot);

            }
            catch (Exception ex)
            {

            }
        }

        // Report code

        public DataTable GetTransformationContractReportDataById(string PrintTabId, string IssueId)
        {
            try
            {

                string _sql = @"select tc.Id,TabType='Transformation', tc.EntityId,tc.PartyId,tc.Remarks,FORMAT(tc.PODate,'dd-MMM-yyyy') as ValueAddedDate
                                    ,FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
                                    FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate,
                                    e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName
									,rt.Id as ReceiptId, rt.GRNDate, FORMAT(rt.GRNDate,'dd-MMM-yyyy') as JWGRNDate, rt.ByWhomEmployeeId, rt.DocRefNo,rt.InvoiceNo
									, rt.GateEntryNo 
                                   --,rt.Remarks as ReceiptRemarks
								   , FORMAT(rt.DocDate,'dd-MMM-yyyy') as ReceiveDocumentDate, FORMAT(rt.InvoiceDate,'dd-MMM-yyyy') as ReceiveInvoiceDate
                                    ,emp.EmployeeName, emp.EmployeeCode
                                    from dbo.JWTransformationPO tc left join ORG.Entity e on e.Id=tc.EntityId
									left join HKP.Party p on p.Id=tc.PartyId
									left join dbo.JWTransformationPODetail mp on tc.Id=mp.JWTransformationPOId
									left join trn.inventoryreceive rt on rt.JobWorkContractId=tc.Id
					     			left join dbo.EmployeeInformation emp on emp.SystemId=rt.ByWhomEmployeeId
                                    WHERE tc.Id='" + PrintTabId + @"' and rt.Id='" + IssueId + @"' ";

                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetTransformationIssueReturnChildDataById(string PrintTabId, string IssueId)
        {
            try
            {

                string _sql = @"select mp.Id, SUM(mp.Quantity) as PlanQuantity,jwi.UserName as JWOutputItem,jwa.UserName as JobWorkActivity,mm.Code as MaterialCode,mm.UserName as Material
                                ,mma.Code as ArticleCode, mma.StandardName as Article
                               ,TotalReceivedQty=ISNULL( kk.TotalReceivedQuantity,'0')
                               ,ToReceive= Sum(mp.Quantity)- ISNULL( kk.TotalReceivedQuantity,'0'), rtc.TransactionQty
                               from dbo.JWTransformationPODetail mp left join dbo.JWTransformationPO tc on tc.Id=mp.JWTransformationPOId
                               left join hkp.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
                               left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
                               left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleId
							   left join MST.MaterialMaster mm on mm.Id=mp.MaterialMasterId
                               left join (select Sum(TransactionQty) as TotalReceivedQuantity,JWTransformationPODetailId from TRN.InventoryReceiveDetail 
							   where JWTransformationPODetailId is not null group by JWTransformationPODetailId)
                               kk on kk.JWTransformationPODetailId=mp.Id
                               left join TRN.InventoryReceiveDetail rtc on rtc.JWTransformationPODetailId=mp.Id
                               where tc.Id='" + PrintTabId + @"' and rtc.InventoryReceiveId='" + IssueId + @"'
                               group by mp.Id,jwi.UserName, mma.StandardName,jwa.UserName,kk.TotalReceivedQuantity,rtc.TransactionQty,mm.Code,mm.UserName,mma.Code ";

                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetTransformationByProductDataById(string PrintTabId, string IssueId)
        {
            try
            {
                string _sql = @"select tbp.Id,jwit.UserName as JWOutputItem,jwi.UserName as ByProductItem,mma.StandardName as ByProductArticle, mm.UserName as ByProductMaterial           
							  ,TQty=(mi.NetConsumption * mp.Quantity)
                              ,TotalReqQty=((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100)
                              ,ISNULL(rvbp.TotalReceivedQuantity,'0') as TotalReceivedQty
                              , ToReceive=((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) - ISNULL(rvbp.TotalReceivedQuantity,'0')
							  ,rtbp.TransactionQty
                              from dbo.JWTransformationPOByProduct tbp 
                              left join HKP.JobWorkItem jwi on jwi.Id=tbp.JobWorkItemId
                              left join dbo.JWTransformationPOInputMaterial mi on mi.Id=tbp.JWTransformationPOInputMaterialId
                              left join dbo.JWTransformationPODetail mp on mp.Id=mi.JWTransformationPODetailId
                              left join HKP.JobWorkItem jwit on jwit.Id=mp.JobWorkItemMasterId
                              left join MST.MaterialMasterArticle mma on mma.Id=tbp.ArticleId
							  left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
                              left join (Select SUM(TransactionQty) as TotalReceivedQuantity,JWTransformationPOByProductId 
							  from TRN.InventoryReceiveDetail where JWTransformationPOByProductId is not null group by JWTransformationPOByProductId)
                              rvbp on rvbp.JWTransformationPOByProductId=tbp.Id
                              left join dbo.JWTransformationPO tc on tc.Id=mp.JWTransformationPOId
							  left join TRN.InventoryReceiveDetail rtbp on rtbp.JWTransformationPOByProductId=tbp.Id
                              where tc.Id='" + PrintTabId + @"' and rtbp.InventoryReceiveId='" + IssueId + @"' ";

                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetTransformationWIPData(string PrintTabId, string IssueId)
        {
            try
            {
                string _sql = @"select distinct mi.Id,mi.JWTransformationPODetailId, jwi.UserName as JWOutputItem,jwii.UserName as JWInputItem ,mm.Id as JWInputMaterialMasterId
                            , mm.UserName as JWInputMaterial ,mma.Id as JWInputMaterialArticleId, mma.StandardName as JWInputArticle
                            ,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
                            ,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalIssuedQty,'0'))
                            ,kk.TotalIssuedQty as TIRCTotalQty, ISNULL(R.TotalReceivedQuantity,'0')  as TotalReceiptQuantity, ISNULL(rtc.TransactionQty,'0') as ReceiptQuantity
							,QuantityUsed=ISNULL(rtc.TransactionQty * mi.GrossConsumption,'0'), TotalQuantityUsed=ISNULL(R.TotalReceivedQuantity * mi.GrossConsumption,'0')
							,WIPQuantity= isnull((kk.TotalIssuedQty - (R.TotalReceivedQuantity * mi.GrossConsumption)),'0')
							 from TRN.InventoryIssueDetail tirc left join dbo.JWTransformationPODetail mp on mp.Id=tirc.JWTransformationPOId
							 left join dbo.JWTransformationPOInputMaterial mi on mp.Id=mi.JWTransformationPODetailId
							 left join HKP.JobWorkItem jwii on jwii.Id=mi.JobWorkItemId
							 left join TRN.InventoryMaterial IM on IM.Id=tirc.InventoryMaterialId
							 left join MST.MaterialMasterArticle mma on mma.Id=IM.ArticleId
							 left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId and mm.Id=IM.MaterialMasterId
							 left  join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
                             left join(
							            select Sum(IID.TransactionQty) as TotalIssuedQty, IM.MaterialMasterId,mm.UserName as Material,mma.StandardName as Article, IM.ArticleId,IID.InventoryMaterialId
							            from TRN.InventoryIssue II inner join TRN.InventoryIssueDetail IID on II.Id=IID.InventoryIssueId
                                        left join TRN.InventoryMaterial IM on IM.Id=IID.InventoryMaterialId
                                        left join MST.MaterialMaster mm on mm.Id=IM.MaterialMasterId
                                        left join MST.MaterialMasterArticle mma on mma.Id=IM.ArticleId
										where II.JobWorkContractId='"+ PrintTabId + @"'
										group by IM.MaterialMasterId,IM.ArticleId,IID.InventoryMaterialId,mm.UserName,mma.StandardName) 
							 kk on kk.InventoryMaterialId=IM.Id
							 left join (select Sum(TransactionQty) as TotalReceivedQuantity,JWTransformationPODetailId 
							 from TRN.InventoryReceiveDetail where JWTransformationPODetailId is not null group by JWTransformationPODetailId)
							 R on  R.JWTransformationPODetailId=mp.Id
							 left join TRN.InventoryReceiveDetail rtc on rtc.JWTransformationPODetailId=mp.Id
						   	where rtc.InventoryReceiveId='" + IssueId + @"'
							 group by mi.Id, mm.Id, mm.UserName,mma.Id, mma.StandardName,mp.Quantity,mi.GrossConsumption,kk.TotalIssuedQty
							 ,mi.JWTransformationPODetailId,jwi.UserName,jwii.UserName,R.TotalReceivedQuantity,rtc.TransactionQty  ";

                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Value Added Receipt Report

        public DataTable GetValueAddedContractReportDataById(string PrintTabId, string IssueId)
        {
            try
            {

                string _sql = @"select tc.Id,TabType='ValueAdded', tc.EntityId,tc.PartyId,tc.Remarks,FORMAT(tc.PODate,'dd-MMM-yyyy') as ValueAddedDate--,CONVERT(varchar(5),tc.[Time],108)[VACTime]
                                    ,FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
                                    FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate,
                                    e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName
									,rt.Id as ReceiptId, rt.GRNDate, FORMAT(rt.GRNDate,'dd-MMM-yyyy') as JWGRNDate, rt.ByWhomEmployeeId, rt.DocRefNo,rt.InvoiceNo
									, rt.GateEntryNo 
                                   --,rt.Remarks as ReceiptRemarks
								   , FORMAT(rt.DocDate,'dd-MMM-yyyy') as ReceiveDocumentDate, FORMAT(rt.InvoiceDate,'dd-MMM-yyyy') as ReceiveInvoiceDate
                                    ,emp.EmployeeName, emp.EmployeeCode
                                    from dbo.JWTransformationPO tc left join ORG.Entity e on e.Id=tc.EntityId
									left join HKP.Party p on p.Id=tc.PartyId
									left join dbo.JWTransformationPODetail mp on tc.Id=mp.JWTransformationPOId
									left join trn.inventoryreceive rt on rt.JobWorkContractId=tc.Id
					     			left join dbo.EmployeeInformation emp on emp.SystemId=rt.ByWhomEmployeeId
                                    WHERE tc.Id='" + PrintTabId + @"' and rt.Id='" + IssueId + @"' ";

                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetValueAddedIssueReturnChildDataById(string PrintTabId, string IssueId)
        {
            try
            {

                string _sql = @"select mp.Id, SUM(mp.Quantity) as PlanQuantity,jwi.UserName as JWOutputItem,jwa.UserName as JobWorkActivity,mm.Code as MaterialCode,mm.UserName as Material
                                ,mma.Code as ArticleCode, mma.StandardName as Article
                               ,TotalReceivedQty=ISNULL( kk.TotalReceivedQuantity,'0')
                               ,ToReceive= Sum(mp.Quantity)- ISNULL( kk.TotalReceivedQuantity,'0'), rtc.TransactionQty
                               from dbo.JWTransformationPODetail mp left join dbo.JWTransformationPO tc on tc.Id=mp.JWTransformationPOId
                               left join hkp.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
                               left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
                               left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleId
							   left join MST.MaterialMaster mm on mm.Id=mp.MaterialMasterId
                               left join (select Sum(TransactionQty) as TotalReceivedQuantity,JWTransformationPODetailId 
							   from TRN.InventoryReceiveDetail where JWTransformationPODetailId is not null group by JWTransformationPODetailId)
                               kk on kk.JWTransformationPODetailId=mp.Id
                               left join TRN.InventoryReceiveDetail rtc on rtc.JWTransformationPODetailId=mp.Id
                               where tc.Id='" + PrintTabId + @"' and rtc.InventoryReceiveId='" + IssueId + @"'
                               group by mp.Id,jwi.UserName, mma.StandardName,jwa.UserName,kk.TotalReceivedQuantity,rtc.TransactionQty,mm.Code,mm.UserName,mma.Code ";

                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        //public DataTable GetTransformationByProductDataById(string PrintTabId, string IssueId)
        //{
        //    try
        //    {

        //        string _sql = @"select tbp.Id,jwit.UserName as JWOutputItem,jwi.UserName as ByProductItem,mma.StandardName as ByProductArticle, mm.UserName as ByProductMaterial           
							 // ,TQty=(mi.NetConsumption * mp.Quantity)
        //                      ,TotalReqQty=((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100)
        //                      ,ISNULL(rvbp.TotalReceivedQuantity,'0') as TotalReceivedQty
        //                      , ToReceive=((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) - ISNULL(rvbp.TotalReceivedQuantity,'0')
							 // ,rtbp.TransactionQty
        //                      from dbo.OSTransformationPOByProduct tbp 
        //                      left join HKP.JobWorkItem jwi on jwi.Id=tbp.JobWorkItemId
        //                      left join dbo.OSTransformationPOInputMaterial mi on mi.Id=tbp.OSTransformationPOInputMaterialId
        //                      left join dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
        //                      left join HKP.JobWorkItem jwit on jwit.Id=mp.JobWorkItemMasterId
        //                      left join MST.MaterialMasterArticle mma on mma.Id=tbp.ArticleId
							 // left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
        //                      left join (Select SUM(TransactionQty) as TotalReceivedQuantity,OSTransformationPOByProductId from TRN.InventoryReceiveDetail where OSTransformationPOByProductId is not null group by OSTransformationPOByProductId)
        //                      rvbp on rvbp.OSTransformationPOByProductId=tbp.Id
        //                      left join dbo.OSTransformationPO tc on tc.Id=mp.OSTransformationPOId
							 // left join TRN.InventoryReceiveDetail rtbp on rtbp.OSTransformationPOByProductId=tbp.Id
        //                      where tc.Id='" + PrintTabId + @"' and rtbp.InventoryReceiveId='" + IssueId + @"' ";

        //        return _sqlRepository.GetDataTable(_sql);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

       // public DataTable GetValueAddedWIPData(string PrintTabId, string IssueId)
       // {
       //     try
       //     {
       //         string _sql = @"select distinct mi.Id,mi.OSTransformationPODetailId, jwi.UserName as JWOutputItem,jwii.UserName as JWInputItem ,mm.Id as JWInputMaterialMasterId
       //                     , mm.UserName as JWInputMaterial ,mma.Id as JWInputMaterialArticleId, mma.StandardName as JWInputArticle
       //                     ,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
       //                     ,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalIssuedQty,'0'))
       //                     ,kk.TotalIssuedQty as TIRCTotalQty, ISNULL(R.TotalReceivedQuantity,'0')  as TotalReceiptQuantity, ISNULL(rtc.TransactionQty,'0') as ReceiptQuantity
							//,QuantityUsed=ISNULL(rtc.TransactionQty * mi.GrossConsumption,'0'), TotalQuantityUsed=ISNULL(R.TotalReceivedQuantity * mi.GrossConsumption,'0')
							//,WIPQuantity= isnull((kk.TotalIssuedQty - (R.TotalReceivedQuantity * mi.GrossConsumption)),'0')
							// from TRN.InventoryIssueDetail tirc left join dbo.OSTransformationPODetail mp on mp.Id=tirc.OSTransformationPOId
							// left join dbo.OSTransformationPOInputMaterial mi on mp.Id=mi.OSTransformationPODetailId
							// left join HKP.JobWorkItem jwii on jwii.Id=mi.JobWorkItemId
							// left join TRN.InventoryMaterial IM on IM.Id=tirc.InventoryMaterialId
							// left join MST.MaterialMasterArticle mma on mma.Id=IM.ArticleId
							// left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId and mm.Id=IM.MaterialMasterId
							// left  join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
       //                      left join(
							//            select Sum(IID.TransactionQty) as TotalIssuedQty, IM.MaterialMasterId,mm.UserName as Material,mma.StandardName as Article, IM.ArticleId,IID.InventoryMaterialId
							//            from TRN.InventoryIssue II inner join TRN.InventoryIssueDetail IID on II.Id=IID.InventoryIssueId
       //                                 left join TRN.InventoryMaterial IM on IM.Id=IID.InventoryMaterialId
       //                                 left join MST.MaterialMaster mm on mm.Id=IM.MaterialMasterId
       //                                 left join MST.MaterialMasterArticle mma on mma.Id=IM.ArticleId
							//			where II.JWContractId='" + PrintTabId + @"'
							//			group by IM.MaterialMasterId,IM.ArticleId,IID.InventoryMaterialId,mm.UserName,mma.StandardName) 
							// kk on kk.InventoryMaterialId=IM.Id
							// left join (select Sum(TransactionQty) as TotalReceivedQuantity,OSTransformationPODetailId from TRN.InventoryReceiveDetail where OSTransformationPODetailId is not null group by OSTransformationPODetailId)
							// R on  R.OSTransformationPODetailId=mp.Id
							//-- left join TRN.InventoryReceiveDetail rtc on rtc.OSTransformationPOId=mp.Id
       //                      left join TRN.InventoryReceiveDetail rtc on rtc.OSTransformationPODetailId=mp.Id
						 //  	where rtc.InventoryReceiveId='" + IssueId + @"'
							// group by mi.Id, mm.Id, mm.UserName,mma.Id, mma.StandardName,mp.Quantity,mi.GrossConsumption,kk.TotalIssuedQty
							// ,mi.OSTransformationPODetailId,jwi.UserName,jwii.UserName,R.TotalReceivedQuantity,rtc.TransactionQty ";

       //         return _sqlRepository.GetDataTable(_sql);
       //     }
       //     catch (Exception)
       //     {
       //         throw;
       //     }
       // }

        // GET Issued Material data

        public IEnumerable<object> GetIfIssuedOrNot(string JWOutputId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"select OSTransformationPODetailId,ArticleId,JobWorkItemId 
                                from dbo.OSTransformationPOInputMaterial 
                                where OSTransformationPODetailId='" + JWOutputId + @"'
                                group by ArticleId,OSTransformationPODetailId,JobWorkItemId ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetIssuedMatInputList(string JWPOId, string JWOutputId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                //string sql = @"select distinct IID.Id,IID.InventoryIssueId,IID.InventoryMaterialId,IID.OSTransformationPOId,IM.MaterialMasterId,mi.ArticleId
                //                from TRN.InventoryIssueDetail IID inner join dbo.OSTransformationPOInputMaterial mi on mi.OSTransformationPODetailId=IID.OSTransformationPOId
                //                inner join TRN.InventoryIssue II on II.Id=IID.InventoryIssueId
                //                inner join TRN.InventoryMaterial IM on IM.Id=IID.InventoryMaterialId and IM.ArticleId=mi.ArticleId
                //                where IID.OSTransformationPOId='"+ JWOutputId + @"' and II.JWContractId='"+ JWPOId + @"' ";

                string sql = @"select distinct IID.InventoryMaterialId,IID.OSTransformationPOId,IM.MaterialMasterId,mi.ArticleId
                                ,mi.GrossConsumption,AA.TotalIssuedQuantity,QtyForOutput=round((AA.TotalIssuedQuantity/mi.GrossConsumption),4)
                                 ,IID.JWTCInputId
                                from TRN.InventoryIssueDetail IID inner join dbo.OSTransformationPOInputMaterial mi on mi.OSTransformationPODetailId=IID.OSTransformationPOId
                                inner join TRN.InventoryIssue II on II.Id=IID.InventoryIssueId
                                inner join TRN.InventoryMaterial IM on IM.Id=IID.InventoryMaterialId and IM.ArticleId=mi.ArticleId
                                left join (Select SUM(TransactionQty) as TotalIssuedQuantity,InventoryMaterialId from TRN.InventoryIssueDetail 
                                where OSTransformationPOId='" + JWOutputId + @"' group by InventoryMaterialId)
                                AA on AA.InventoryMaterialId=IM.Id
                                where IID.OSTransformationPOId='"+ JWOutputId + @"' and II.JWContractId='"+ JWPOId + @"' 
                                group by IID.InventoryMaterialId,IID.OSTransformationPOId,IM.MaterialMasterId,mi.ArticleId,mi.GrossConsumption,AA.TotalIssuedQuantity,IID.JWTCInputId ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetIfIssuedOrNotValAdded(string JWPOId, string JWOutputId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                //string sql = @"select  Id, OSTransformationPOId,ArticleId,JobWorkItemMasterId
                //                from dbo.OSTransformationPODetail
                //                where OSTransformationPOId='"+ JWPOId + @"'
                //                group by ArticleId,OSTransformationPOId,JobWorkItemMasterId,Id ";

                string sql = @"select  mp.Id, mp.OSTransformationPOId,mp.ArticleId,mp.JobWorkItemMasterId,owr.Id as JWOrderWiseId
                                from dbo.OSTransformationPODetail mp left join dbo.OSTransformationPOMasterOrderItem owr 
								on owr.OSTransformationPODetailId=mp.Id
                                where mp.OSTransformationPOId='"+ JWPOId + @"' and mp.Id='"+ JWOutputId + @"' 
                                group by mp.Id, mp.OSTransformationPOId,mp.ArticleId,mp.JobWorkItemMasterId,owr.Id";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetIssuedMatInputListValAdded(string JWPOId, string JWOutputId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                //        string sql = @"select distinct IID.InventoryMaterialId,IID.OSTransformationPOId,IM.MaterialMasterId,om.ArticleId
                //                        --,mi.GrossConsumption
                //,AA.TotalIssuedQuantity--,QtyForOutput=round((AA.TotalIssuedQuantity/om.GrossConsumption),4)
                //,QtyForOutput=round((AA.TotalIssuedQuantity),4)
                //                         ,IID.JWTCInputId
                //                        from TRN.InventoryIssueDetail IID inner join dbo.OSTransformationPODetail om on om.Id=IID.OSTransformationPOId
                //                        inner join TRN.InventoryIssue II on II.Id=IID.InventoryIssueId
                //                        inner join TRN.InventoryMaterial IM on IM.Id=IID.InventoryMaterialId and IM.ArticleId=om.ArticleId
                //                        left join (Select SUM(TransactionQty) as TotalIssuedQuantity,InventoryMaterialId from TRN.InventoryIssueDetail 
                //                        where OSTransformationPOId='"+ JWOutputId + @"' group by InventoryMaterialId)
                //                        AA on AA.InventoryMaterialId=IM.Id
                //                        where IID.OSTransformationPOId='"+ JWOutputId + @"' and II.JWContractId='"+ JWPOId + @"' 
                //                        group by IID.InventoryMaterialId,IID.OSTransformationPOId,IM.MaterialMasterId,om.ArticleId--,mi.GrossConsumption
                //,AA.TotalIssuedQuantity,IID.JWTCInputId  ";

                string sql = @"select distinct IID.InventoryMaterialId,IID.OSTransformationPOId,IM.MaterialMasterId,om.ArticleId,IID.JWOrderWiseId
								,AA.TotalIssuedQuantity--,QtyForOutput=round((AA.TotalIssuedQuantity/om.GrossConsumption),4)
								--,QtyForOutput=round((AA.TotalIssuedQuantity),4)
								--,ISNULL(BB.TotalIssuedQuantity,'0') as OrderWiseIssuedQty
                                ,QtyForOutput=case when IID.InventoryMaterialId is not null then round((AA.TotalIssuedQuantity),4) else ISNULL(BB.TotalIssuedQuantity,'0') End
                                from TRN.InventoryIssueDetail IID left join dbo.OSTransformationPODetail om on om.Id=IID.OSTransformationPOId
                                left join TRN.InventoryIssue II on II.Id=IID.InventoryIssueId
                                left join TRN.InventoryMaterial IM on IM.Id=IID.InventoryMaterialId and IM.ArticleId=om.ArticleId
                                left join (Select SUM(TransactionQty) as TotalIssuedQuantity,InventoryMaterialId,OSTransformationPOId from TRN.InventoryIssueDetail 
								group by InventoryMaterialId,OSTransformationPOId)
                                AA on AA.InventoryMaterialId=IM.Id and AA.OSTransformationPOId=om.Id
								left join (Select SUM(TransactionQty) as TotalIssuedQuantity,OSTransformationPOId,JWOrderWiseId from TRN.InventoryIssueDetail 
								group by OSTransformationPOId,JWOrderWiseId) BB on BB.OSTransformationPOId=om.Id
                                where II.JWContractId='" + JWPOId + @"' and IID.OSTransformationPOId='" + JWOutputId + @"'
                                group by IID.InventoryMaterialId,IID.OSTransformationPOId,IM.MaterialMasterId,om.ArticleId
								,AA.TotalIssuedQuantity
								,IID.JWOrderWiseId,BB.TotalIssuedQuantity";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Template Report

        public void GRNReport(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId)
        {

            var fileName = "";
            var strPath = "";

            var File = "";

            ReportUtility ru = new ReportUtility();

            //tempId = dtLangName.Rows[0]["UserName"].ToString();
            fileName = "JobWorkGRNReceiptReport" + plantId + ".docx";
            strPath = Path.Combine(ResourcesPathReader.GetJWReceiptTemplatePath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }

            //makeDictionary();
            ////A opens input document.
            WordDocument document = new WordDocument(File, FormatType.Docx);
            //Gets the paragraph at index 1
            try
            {
                WSection section = document.Sections[0];

                DataTable dtOrderMaster;


                dtOrderMaster = loadGRNMaterialMaster(grnId);


                var invoicePartyAddress = ru.GetAddress(dtOrderMaster.Rows[0]["InvoicePartyAddressMasterId"].ToString(), dtOrderMaster.Rows[0]["InvoicingByAddress"].ToString());
                document.Replace("{InvoicingPartyAddress}", invoicePartyAddress, false, false);

                var vendorPartyAddress = ru.GetAddress(dtOrderMaster.Rows[0]["VendorAddressMasterId"].ToString(), "");
                document.Replace("{VendorAddress}", vendorPartyAddress, false, false);

                Dictionary<string, string> columns = new Dictionary<string, string>();

                var poApprovedStatus = "";
                foreach (DataColumn item in dtOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);
                var dsServiceItems = loadGRNServiceMaster(grnId);
                var materialTotal = makeOrderDetailsTable(document, dtOrderMaster, grnId);//Material Details 
                var dsInventoryReceiveAdditionalTax = loadInventoryReceiveAdditionalTax(grnId);
                var InventoryReceiveAdditionalTax = 0.00;
                if (dsInventoryReceiveAdditionalTax.Rows.Count > 0)

                {
                    InventoryReceiveAdditionalTax = makeInventoryReceiveAdditionalTaxTable(document, dsInventoryReceiveAdditionalTax, grnId);//Service Details 
                                                                                                                                             //document.Replace("{ServiceDetails}", "Service Details", true, true);

                    //{TotalInWords}
                }
                loadGRNShortageTable(document, grnId);
                loadGRNRejectionTable(document, grnId);
                var serviceTotal = 0.00;
                if (dsServiceItems.Rows.Count > 0)

                {
                    serviceTotal = makeOrderServiceTable(document, dsServiceItems, grnId);//Service Details 
                    document.Replace("{ServiceDetails}", "Service Details", true, true);

                    //{TotalInWords}
                }
                document.Replace("{GrandTotal}", ((materialTotal + serviceTotal) + InventoryReceiveAdditionalTax).ToString("#,##0.00") + " " + dtOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{TotalInWords}", ru.InWord(((materialTotal + serviceTotal) + InventoryReceiveAdditionalTax), dtOrderMaster.Rows[0]["CurrencyId"].ToString()), true, true);

                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                List<string> strReplace = new List<string>();

                StringCollection strColDistinct = new StringCollection();

                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());

                for (int i = 0; i < strReplace.Count; i++)
                {
                    if (strColDistinct.Contains(strReplace[i].ToUpper()))
                        continue;

                    strColDistinct.Add(strReplace[i].ToUpper());             //For Same Name Use
                    string text = strReplace[i].ToUpper();

                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        ReplaceInfo[text] = document.Replace(text, dtOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);


                //removing any unused place holder
                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "", false, false);

                }

                //Region that is for Pdf.Document
                DocToPDFConverter converter = new DocToPDFConverter();

                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                pdfDocument.PageSettings.Width = 1200;
                pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();

                //Closes the instance of document objects

                //Saves the PDF file 
                string Prefix = "GRN" + grnId;

                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);

                document.Close();
            }


            catch (Exception ex)
            {
                throw ex;

            }
            document.Close();
        }

        public DataTable loadGRNMaterialMaster(string OrderMasterID)
        {
            string strSQL;
            try
            {

                strSQL = @"SELECT IR.Id grnNumber
							--,PO1.PODate
							,FORMAT(PO.PODate,'dd-MMM-yyyy') as PODate
							,GTE.ModeofTransport
							,HSNC.Code HSNCode
                            ,IR.CompanyGroupId
                            ,IR.CompanyId
                            ,Plant.GSTIN
                            ,ir.PODepended
                          --  ,PO1.POId PONumber
							,PO.Id as PONumber
							,CNO.ContractNo as ContractNO
						 --   ,PO1.ContractNO ContractNO 
							,PLC.LCRef LCNumber
							,PLC.BenificiaryBank BeneficiaryBank
							,PLC.BenificiaryBank OpeningBank
							--,B.UserName BeneficiaryBank
							--,B.UserName OpeningBank
							,PDA.AcceptanceNo AcceptanceNo
							,REPLACE(Convert(VARCHAR(11), PLC.LCDate, 106), ' ', '-') AS LCODate
							,REPLACE(Convert(VARCHAR(11), PDA.AcceptanceDate, 106), ' ', '-') AS AcceptanceDate
                            ,REPLACE(Convert(VARCHAR(11), IR.GRNDate, 106), ' ', '-') AS GRNDate
							 --,GRNType=CASE WHEN IR.GRNType='GRN' then 'GRN Without PO' ELSE 'GRN With PO' END
							  ,GRNType=CASE WHEN IR.GRNType='GRNBYJW' then 'GRN By JW' ELSE 'GRN by JW' END
                            ,REPLACE(Convert(VARCHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
                            ,REPLACE(Convert(VARCHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
                            ,IR.InvoicingPartyPlantId
                            ,INVPARTYPL.UserName InvoicingPartyName
                            ,INVPARTYPL.AddressMasterId InvoicePartyAddressMasterId
                            ,INVPARTYPL.GSTIN InvoicingPartyGSTIN
                            ,ISNULL(IR.InvoicingByAddress,'') InvoicingByAddress
                            ,IR.DeliveryByAddress
                            ,DPARTYPL.UserName DeliveryParty
                            ,IR.DeliveryPartyPlantId
                            ,IOM.MaterialMasterId
                            ,IR.DocRefNo
                            ,REPLACE(Convert(VARCHAR(11), IR.DocDate, 106), ' ', '-') AS DocDate
                            ,IR.GateEntryNo,REPLACE(Convert(VARCHAR(11), IR.EntryDate, 106), ' ', '-') AS GateEntryDate
                            ,CheckedBy=CASE WHEN IR.CheckedByStatus='Checked' Then eI.EmployeeName else '' END
                            ,AuthorizedBy=CASE When IR.AuthorizedByStatus='Approved'then eI1.EmployeeName else '' END
                             ,AddedBy=CASE 
									When IR.CheckedByStatus='ForChecked' Then eI3.EmployeeName
									When IR.CheckedByStatus='Hold' Then eI3.EmployeeName
									When IR.CheckedByStatus='Reject' Then eI3.EmployeeName
									When IR.CheckedByStatus='Checked' Then eI3.EmployeeName
									When IR.CheckedByStatus IS NULL then IR.AddedBy 
									
									else ''
							END
                            ,IR.AddedDate
                            ,IR.UpdatedBy
                            ,IR.UpdatedDate
                            ,IR.IsApproved
                            ,IR.PartyType
                            ,EMPIN.EmployeeName
                            ,Party.UserName VendorName
                            ,Party.AddressMasterId VendorAddressMasterId
                            ,Party.TINNO VendorGSTIN
                            ,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
                            ,IR.IsNonCreditable
                            ,IR.CurrencyId
                            ,CRNC.Code AS CurrencyName
                            ,IR.ToCurrencyRate
                            ,BASECRNC.Code AS BaseCurrencyName
                            ,PayTerm.UserName PaymentTerm
                            ,MM.UserName MaterialMaster
                            ,MM.MaterialGroupMasterId
                            ,MGM.UserName MaterialGroupMaster
                            ,IOM.ArticleId
                            ,MMA.StandardName Article
                            ,FC.Id FirstCharId
                            ,FC.UserName FirstChar
                            ,IOM.FirstCharacteristicsValueId
                            ,FCV.UserName AS FirstCharacteristicsValue
                            ,IOM.SecondCharacteristicsValueId
                            ,SCV.UserName AS SecondCharacteristicsValue
                            ,IOM.ThirdCharacteristicsValueId
                            ,TCV.UserName AS ThirdCharacteristicsValue
                            ,SC.Id SecondCharId
                            ,SC.UserName SecondChar
                            ,TC.Id ThirdCharId
                            ,TC.UserName ThirdChar
                            ,ROUND(IRD.TransactionQty, 2) POTransactionQty
                             -- ,ROUND(IRD.MaterialTranRate, 2) TransactionRate
                             ,ROUND(IRD.MaterialTranRate, 4) TransactionRate
						   -- ,ROUND(IRD.MaterialTranRate * IR.ToCurrencyRate, 4) TransactionRate
                            --,ROUND((IRD.TransactionQty * IRD.MaterialTranRate), 2) AS TrnAmount
							,TrnAmount=(IRD.GrossAmount-IRD.DiscountAmount)
							--,TrnAmount=((IRD.GrossAmount-IRD.DiscountAmount) * IR.ToCurrencyRate)
                            ,IRD.TotalMaterialTranAmount BaseAmount
                            ,IRD.TotalTaxAmount AS BaseTaxAmount
                            ,TaxAmount = (
                            SELECT SUM(TaxAmount)
                            FROM dbo.JWTransformationPOTax --[TRN].[PurchaseOrderTax]
                            WHERE JWTransformationPODetailId = IRD.JWTransformationPODetailId
                            )
                            ,ServiceTaxAmount = (
                            SELECT SUM(TotalTaxAmount)
                            FROM dbo.JWTransformationPOService  --[TRN].[POService]
                            WHERE JWTransformationPOId = IRD.JWTransformationPOId
                            )
                            ,IRD.ChargesTranAmount
                            ,IRD.CountryId

                            ,IRD.TransactionUoMId
                            ,TUoM.ShortName  AS TransactionUoM
                            ,IRD.Id InventoryReceiveDetailId--,MRD.MaterialDetail
							,POD.Description,IRD.Description AS GRDDescrition
                            ,CheckStatus= CASE when IR.CheckedByStatus='ForChecked' Then 'To be checked'
                            when IR.CheckedByStatus='Hold' Then 'Hold'
                            when IR.CheckedByStatus='Reject' Then 'Reject'
                            when IR.CheckedByStatus='Checked' Then 'Checked'
                            else ''

                            END
                            ,ApproveStatus= CASE
                            when IR.AuthorizedByStatus='Reject' Then 'Reject For Approved'
                            when IR.AuthorizedByStatus='Hold' Then 'Hold For Approved'
                            when IR.AuthorizedByStatus='For Approval' Then 'To be Approval'
                            when IR.AuthorizedByStatus='Approved' Then 'Approved'
                            else ''
							END
						,GRNStatus= CASE when IR.CheckedByStatus='ForChecked' Then 'To be checked'
								when IR.CheckedByStatus='Hold' Then 'Hold'
								when IR.CheckedByStatus='Reject' Then 'Reject'
								when IR.CheckedByStatus='Checked' AND IR.AuthorizedByStatus='To be Approval' Then 'Checked' 
								when IR.CheckedByStatus='Checked'  AND IR.AuthorizedByStatus='Reject' Then 'Reject For Approved'
								when IR.CheckedByStatus='Checked' AND IR.AuthorizedByStatus='Hold' Then 'Hold For Approved'
								when IR.CheckedByStatus='Checked' and IR.AuthorizedByStatus='For Approval' Then 'To be Approval'
								when IR.CheckedByStatus='Checked' and IR.AuthorizedByStatus='Approved' Then 'Approved'
								when IR.CheckedByStatus Is null and IR.AuthorizedByStatus Is null Then 'Approved'
                            else ''
                            END
							,IRD.LotNo , IRD.QualityStatus , IRD.GrossAmount ,IRD.DiscountAmount
							--,GrossAmount=(IRD.GrossAmount * IR.ToCurrencyRate) ,DiscountAmount=(IRD.DiscountAmount * IR.ToCurrencyRate)
                            FROM TRN.InventoryReceive IR
                            LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
                            LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                            LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
                            LEFT JOIN SCS.Currency CRNC ON CRNC.Id = IR.CurrencyId
                            LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = IR.BaseCurrencyId
                            LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = IR.PaymentTermId
                            LEFT JOIN HKP.PartyPlant INVPARTYPL ON INVPARTYPL.Id = IR.InvoicingPartyPlantId
                            LEFT JOIN HKP.PartyPlant DPARTYPL ON DPARTYPL.Id = IR.DeliveryPartyPlantId
                            LEFT JOIN trn.inventoryReceiveDetail IRD ON IR.Id = IRD.InventoryReceiveId
                            LEFT JOIN HKP.Party Party ON Party.Id = IR.PartyId
                            LEFT JOIN trn.InventoryMaterial AS IOM ON IRD.InventoryMaterialId = IOM.Id
                             LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = IOM.MaterialMasterId
							LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
                            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                            LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IOM.ArticleId
                            LEFT JOIN HKP.Characteristics AS FC ON IOM.FirstCharacteristicsId = FC.Id
                            LEFT JOIN HKP.Characteristics AS SC ON IOM.SecondCharacteristicsId = SC.Id
                            LEFT JOIN HKP.Characteristics AS TC ON IOM.ThirdCharacteristicsId = TC.Id
                            LEFT JOIN HKP.CharacteristicsValue AS FCV ON IOM.FirstCharacteristicsValueId = FCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS SCV ON IOM.SecondCharacteristicsValueId = SCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS TCV ON IOM.ThirdCharacteristicsValueId = TCV.Id
                            JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
                       --     LEFT JOIN trn.PurchaseOrderDetail POD ON POD.Id = IRD.PODetailsId
							LEFT JOIN dbo.JWTransformationPODetail POD ON POD.Id = IRD.JWTransformationPODetailId
	                    --    LEFT JOIN TRN.PurchaseOrder PO ON PO.Id = IRD.POId
							 LEFT JOIN dbo.JWTransformationPO PO ON PO.Id = IRD.JWTransformationPOId
                            	LEFT JOIN (select Distinct PDAA.Id,AcceptanceDate,AcceptanceNo,ACMAP.GRNId from TRN.GRNAcceptanceMap ACMAP 
									left Join trn.PurchaseDocAcceptance  PDAA ON PDAA.Id=ACMAP.PurchaseDocumentAcceptanceId
									)PDA ON PDA.GRNId=IR.Id
							LEFT JOIN [dbo].[Contract] CNO ON CNO.Id = PO.ContractId
							LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id = PO.PurchaseLCId

	                        --LEFT JOIN [HKP].[Bank] B ON B.Id = PLC.BenificiaryBankId
                       --     Left Join TRN.MaterialRequsitionDetails MRD ON MRD.Id=POD.RequisitionDetailId
                         --   LEFT JOIN EmployeeInformation AS EMPIN ON EMPIN.SystemId= IR.EmployeeId
							LEFT JOIN EmployeeInformation AS EMPIN ON EMPIN.SystemId= IR.ByWhomEmployeeId
                            LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
                            left join [SEC].[User] U on U.UserId=IR.AddedBy
                            LEFT JOIN dbo.EmployeeInformation eI3 ON eI3.SystemId=U.EmployeeId
							LEFT JOIN [TRN].[GateEntry] GTE  ON GTE.ID= IR.GateEntryNo
                            WHERE IR.Id ='"+ OrderMasterID + @"' and IOM.MaterialMasterId is not NULL

                            Union ALL
                            SELECT IR.Id grnNumber
							,GTE.ModeofTransport
							--,PO1.PODate
							,FORMAT(PO.PODate,'dd-MMM-yyyy') as PODate
							,HSNC.Code HSNCode
                            ,IR.CompanyGroupId
                            ,IR.CompanyId
                            ,Plant.GSTIN
                            ,ir.PODepended
                            --,PO1.POId PONumber
							,PO.Id as PONumber
						    ,CNO.ContractNo as ContractNO
						 -- ,PO1.ContractNO ContractNO 
							,PLC.LCRef LCNumber
							,PLC.BenificiaryBank BeneficiaryBank
							,PLC.BenificiaryBank OpeningBank
							,PDA.AcceptanceNo AcceptanceNo
							,REPLACE(Convert(VARCHAR(11), PLC.LCDate, 106), ' ', '-') AS LCODate
							,REPLACE(Convert(VARCHAR(11), PDA.AcceptanceDate, 106), ' ', '-') AS AcceptanceDate
                            ,REPLACE(Convert(VARCHAR(11), IR.GRNDate, 106), ' ', '-') AS GRNDate
						--	,GRNType=CASE WHEN IR.GRNType='GRN' then 'GRN Without PO' ELSE 'GRN With PO' END
							,GRNType=CASE WHEN IR.GRNType='GRNBYJW' then 'GRN By JW' ELSE 'GRN by JW' END
                            ,REPLACE(Convert(VARCHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
                            ,REPLACE(Convert(VARCHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
                            ,IR.InvoicingPartyPlantId

                            ,INVPARTYPL.UserName InvoicingPartyName
                            ,INVPARTYPL.AddressMasterId InvoicePartyAddressMasterId
                            ,INVPARTYPL.GSTIN InvoicingPartyGSTIN
                            ,ISNULL(IR.InvoicingByAddress,'') InvoicingByAddress
                            ,IR.DeliveryByAddress
                            ,DPARTYPL.UserName DeliveryParty
                            ,IR.DeliveryPartyPlantId
                            ,IOM.MaterialMasterId
                            ,IR.DocRefNo
                            ,REPLACE(Convert(VARCHAR(11), IR.DocDate, 106), ' ', '-') AS DocDate
                            ,IR.GateEntryNo,REPLACE(Convert(VARCHAR(11), IR.EntryDate, 106), ' ', '-') AS GateEntryDate
                            ,CheckedBy=CASE WHEN IR.CheckedByStatus='Checked' Then eI.EmployeeName else '' END
                            ,AuthorizedBy=CASE When IR.AuthorizedByStatus='Approved'then eI1.EmployeeName else '' END
                             ,AddedBy=CASE 
									When IR.CheckedByStatus='ForChecked' Then eI3.EmployeeName
									When IR.CheckedByStatus='Hold' Then eI3.EmployeeName
									When IR.CheckedByStatus='Reject' Then eI3.EmployeeName
									When IR.CheckedByStatus='Checked' Then eI3.EmployeeName
									When IR.CheckedByStatus IS NULL then IR.AddedBy 
									else ''
							END
                            ,IR.AddedDate
                            ,IR.UpdatedBy
                            ,IR.UpdatedDate
                            ,IR.IsApproved
                            ,IR.PartyType
                            ,EMPIN.EmployeeName
                            ,Party.UserName VendorName
                            ,Party.AddressMasterId VendorAddressMasterId
                            ,Party.TINNO VendorGSTIN
                            ,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
                            ,IR.IsNonCreditable
                            ,IR.CurrencyId
                            ,CRNC.Code AS CurrencyName
                            ,IR.ToCurrencyRate
                            ,BASECRNC.Code AS BaseCurrencyName
                            ,PayTerm.UserName PaymentTerm
                            ,'-' MaterialMaster
                            ,'-' MaterialGroupMasterId
                            ,'-' MaterialGroupMaster
                            ,IOM.ArticleId
                            ,'-' Article
                            ,'-' FirstCharId
                            ,'-' FirstChar
                            ,IOM.FirstCharacteristicsValueId
                            ,'' AS FirstCharacteristicsValue
                            ,IOM.SecondCharacteristicsValueId
                            ,'' AS SecondCharacteristicsValue
                            ,IOM.ThirdCharacteristicsValueId
                            ,'' AS ThirdCharacteristicsValue
                            ,'' SecondCharId
                            ,'' SecondChar
                            ,'' ThirdCharId
                            ,'' ThirdChar
                            ,ROUND(IRD.TransactionQty, 2) POTransactionQty
                             -- ,ROUND(IRD.MaterialTranRate, 2) TransactionRate
                              ,ROUND(IRD.MaterialTranRate, 4) TransactionRate
						   -- ,ROUND(IRD.MaterialTranRate * IR.ToCurrencyRate, 4) TransactionRate
                           ,TrnAmount=(IRD.GrossAmount-IRD.DiscountAmount)
							--,TrnAmount=((IRD.GrossAmount-IRD.DiscountAmount) * IR.ToCurrencyRate)
                            ,IRD.TotalMaterialTranAmount BaseAmount
                            ,IRD.TotalTaxAmount AS BaseTaxAmount
                            ,TaxAmount = (
                            SELECT SUM(TaxAmount)
                            FROM dbo.JWTransformationPOTax --[TRN].[PurchaseOrderTax]
                            WHERE JWTransformationPODetailId = IRD.JWTransformationPODetailId
                            )
                            ,ServiceTaxAmount = (
                            SELECT SUM(TotalTaxAmount)
                            FROM dbo.JWTransformationPOService  --[TRN].[POService]
                            WHERE JWTransformationPOId = IRD.JWTransformationPOId
                            )
                            ,IRD.ChargesTranAmount
                            ,IRD.CountryId

                            ,IRD.TransactionUoMId
                            ,TUoM.ShortName  AS TransactionUoM
                            ,IRD.Id InventoryReceiveDetailId--,MRD.MaterialDetail
							,POD.Description,IRD.Description AS GRDDescrition
                            ,PurOrCheckedStatus= CASE when IR.CheckedByStatus='ForChecked' Then 'To be checked'
                            when IR.CheckedByStatus='Hold' Then 'Hold'
                            when IR.CheckedByStatus='Reject' Then 'Reject'
                            when IR.CheckedByStatus='Checked' Then 'Checked'
                            else ''

                            END
                            ,PurOrApprovedStatus= CASE
                            when IR.AuthorizedByStatus='Reject' Then 'Reject For Approved'
                            when IR.AuthorizedByStatus='Hold' Then 'Hold For Approved'
                            when IR.AuthorizedByStatus='For Approval' Then 'To be Approval'
                            when IR.AuthorizedByStatus='Approved' Then 'Approved'
                            else ''
							END
						,GRNStatus= CASE when IR.CheckedByStatus='ForChecked' Then 'To be checked'
								when IR.CheckedByStatus='Hold' Then 'Hold'
								when IR.CheckedByStatus='Reject' Then 'Reject'
								when IR.CheckedByStatus='Checked' AND IR.AuthorizedByStatus='To be Approval' Then 'Checked' 
								when IR.CheckedByStatus='Checked'  AND IR.AuthorizedByStatus='Reject' Then 'Reject For Approved'
								when IR.CheckedByStatus='Checked' AND IR.AuthorizedByStatus='Hold' Then 'Hold For Approved'
								when IR.CheckedByStatus='Checked' and IR.AuthorizedByStatus='For Approval' Then 'To be Approval'
								when IR.CheckedByStatus='Checked' and IR.AuthorizedByStatus='Approved' Then 'Approved'
								when IR.CheckedByStatus Is null and IR.AuthorizedByStatus Is null Then 'Approved'
                            else ''
                            END
							,Null LotNo , Null QualityStatus , Null GrossAmount ,Null DiscountAmount
                            FROM TRN.InventoryReceive IR
                            LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
                            LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                            LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
                            LEFT JOIN SCS.Currency CRNC ON CRNC.Id = IR.CurrencyId
                            LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = IR.BaseCurrencyId
                            LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = IR.PaymentTermId
                            LEFT JOIN HKP.PartyPlant INVPARTYPL ON INVPARTYPL.Id = IR.InvoicingPartyPlantId
                            LEFT JOIN HKP.PartyPlant DPARTYPL ON DPARTYPL.Id = IR.DeliveryPartyPlantId
                            LEFT JOIN trn.inventoryReceiveDetail IRD ON IR.Id = IRD.InventoryReceiveId
                            LEFT JOIN HKP.Party Party ON Party.Id = IR.PartyId
                            --     LEFT JOIN trn.PurchaseOrderDetail POD ON POD.Id = IRD.PODetailsId
							LEFT JOIN dbo.JWTransformationPODetail POD ON POD.Id = IRD.JWTransformationPODetailId
	                    --    LEFT JOIN TRN.PurchaseOrder PO ON PO.Id = IRD.POId
							 LEFT JOIN dbo.JWTransformationPO PO ON PO.Id = IRD.JWTransformationPOId
                            LEFT JOIN (select Distinct PDAA.Id,AcceptanceDate,AcceptanceNo,ACMAP.GRNId from TRN.GRNAcceptanceMap ACMAP 
									left Join trn.PurchaseDocAcceptance  PDAA ON PDAA.Id=ACMAP.PurchaseDocumentAcceptanceId
									)PDA ON PDA.GRNId=IR.Id
							LEFT JOIN [dbo].[Contract] CNO ON CNO.Id = PO.ContractId
							LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id = PO.PurchaseLCId
	                        --LEFT JOIN [HKP].[Bank] B ON B.Id = PLC.BenificiaryBankId
                        --    Left Join TRN.MaterialRequsitionDetails MRD ON MRD.Id=POD.RequisitionDetailId
                            JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
                            LEFT JOIN trn.InventoryMaterial AS IOM ON IRD.InventoryMaterialId = IOM.Id
							LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = IOM.MaterialMasterId
							LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
                            --   LEFT JOIN EmployeeInformation AS EMPIN ON EMPIN.SystemId= IR.EmployeeId
							LEFT JOIN EmployeeInformation AS EMPIN ON EMPIN.SystemId= IR.ByWhomEmployeeId
                            LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
                            left join [SEC].[User] U on U.UserId=IR.AddedBy
                            LEFT JOIN dbo.EmployeeInformation eI3 ON eI3.SystemId=U.EmployeeId
							LEFT JOIN [TRN].[GateEntry] GTE  ON GTE.ID= IR.GateEntryNo
                            WHERE IR.Id ='"+ OrderMasterID + @"' and IOM.MaterialMasterId is NULL";


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

        public DataTable loadGRNServiceMaster(string OrderMasterID)
        {
            string strSQL;

            try
            {
                strSQL = @"SELECT IOS.Id ServiceId, SM.UserName  Service ,IOS.Amount,IOS.TotalTaxAmount,IOS.AddedBy,IOS.AddedDate,IOS.UpdatedBy,IOS.UpdatedDate 
                               FROM TRN.InventoryReceive   IR
                            INNER join trn.inventoryservice IOS ON IOS.InventoryReceiveId = IR.Id
                            INNER JOIN HKP.ServiceMaster SM ON IOS.ServiceMasterId = SM.Id 
                            where IR.Id = '" + OrderMasterID + "'";

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

        public double makeOrderDetailsTable(WordDocument document, DataTable dsOrderMaster, string grnId)
        {
            string replaceString = "{materialItems}";

            DataTable dsOrderItems, dsTax;

            dsOrderItems = loadOrderMasterItems(grnId);
            dsTax = loadOrderMasterTax(grnId);

            int LasColumnIndex = 15;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));
            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    LasColumnIndex++;
                }
            }
            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            wTable.TableFormat.IsAutoResized = true;

            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();
            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;
            
            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SL");
            range.ApplyCharacterFormat(FontBold);
            int colRo = COL; COL++;
            wTable.Rows[ROW].Cells[colRo].Width = 30;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("RowId");
            range.ApplyCharacterFormat(FontBold);
            int colRowId = COL; COL++;
            //wTable.Rows[ROW].Cells[colRowId].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Materials");
            range.ApplyCharacterFormat(FontBold);
            int colMaterialGroup = COL; COL++;
            wTable.Rows[ROW].Cells[colMaterialGroup].Width = 80;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article ");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            wTable.Rows[ROW].Cells[colArticle].Width = 80;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU1");
            range.ApplyCharacterFormat(FontBold);
            int colChar1 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar1].Width = 36;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU2");
            range.ApplyCharacterFormat(FontBold);
            int colChar2 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar2].Width = 36;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU3");
            range.ApplyCharacterFormat(FontBold);
            int colChar3 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar3].Width = 36;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("HSN No");
            range.ApplyCharacterFormat(FontBold);
            int colHSNCode = COL; COL++;
            wTable.Rows[ROW].Cells[colHSNCode].Width = 35;


            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("HSN NO");
            //range.ApplyCharacterFormat(FontBold);
            //int colHSNCode = COL; COL++;
            //wTable.Rows[ROW].Cells[colChar3].Width = 45;


            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Po Material Detail");
            //range.ApplyCharacterFormat(FontBold);
            //int colDescription = COL; COL++;
            //wTable.Rows[ROW].Cells[colDescription].Width = 40;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("GRN Material Detail");
            //range.ApplyCharacterFormat(FontBold);
            //int colGRNMaterialDetail = COL; COL++;
            //wTable.Rows[ROW].Cells[colGRNMaterialDetail].Width = 40;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Lot No");
            range.ApplyCharacterFormat(FontBold);
            int colLotNo = COL; COL++;



           // range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate (" + dsOrderMaster.Rows[0]["CurrencyName"].ToString() + ")");
            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate (" + dsOrderMaster.Rows[0]["BaseCurrencyName"].ToString() + ")");
            range.ApplyCharacterFormat(FontBold);
            int colRate = COL; COL++;
            wTable.Rows[ROW].Cells[colRate].Width = 55;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UOM");
            range.ApplyCharacterFormat(FontBold);
            int colUoM = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Quality Status");
            range.ApplyCharacterFormat(FontBold);
            int colQualityStatus = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("GrossAmount");
            range.ApplyCharacterFormat(FontBold);
            int colGrossAmount = COL; COL++;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("DiscountAmount");
            range.ApplyCharacterFormat(FontBold);
            int colDiscountAmount = COL;
          
            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
                wTable.Rows[ROW].Cells[colTotalTaxableAmount].Width = 65;
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        //two columns required for tax
                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                        range.ApplyCharacterFormat(FontBold);
                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
                range.ApplyCharacterFormat(FontBold);
            }


            if (dv.Count > 0)
            {
              
                wTable.Rows.Add(TemplateRow);
                ROW++;
                WTableRow TROW = wTable.LastRow;
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
                        range.ApplyCharacterFormat(FontBold);
                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }
            #endregion column headers
            //if (dv.Count > 0)
            //{
            //    wTable.Rows.Add(TemplateRow);

            //    WTableRow TROW = wTable.LastRow;
            //    for (int CE = 0; CE < TROW.Cells.Count; CE++)
            //    {
            //        foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
            //        {
            //            item.Text = "";
            //        }
            //        TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
            //    }
            //    for (int i = 0; i < dv.Count; i++)
            //    {

            //        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
            //        range.ApplyCharacterFormat(FontBold);
            //        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
            //        range.ApplyCharacterFormat(FontBold);
            //    }
            //    ROW++;
            //}
// #endregion column headers

            double totalValue = 0;
            int sl = 0;
            ROW++;
            
            //wTable.AddRow();
            int startRow = 1;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                sl++;
                ROW++;
                wTable.AddRow();
                //wTable.AddRow();
                WTableRow TROW = wTable.LastRow;
                
                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                TROW.Cells[colRo].AddParagraph().AppendText(sl.ToString());
                TROW.Cells[colRowId].AddParagraph().AppendText(dsOrderMaster.Rows[i]["InventoryReceiveDetailId"].ToString());
                TROW.Cells[colMaterialGroup].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialMaster"].ToString());
                TROW.Cells[colArticle].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Article"].ToString());
                TROW.Cells[colChar1].AddParagraph().AppendText(dsOrderMaster.Rows[i]["FirstCharacteristicsValue"].ToString());
                TROW.Cells[colChar2].AddParagraph().AppendText(dsOrderMaster.Rows[i]["SecondCharacteristicsValue"].ToString());
                TROW.Cells[colChar3].AddParagraph().AppendText(dsOrderMaster.Rows[i]["ThirdCharacteristicsValue"].ToString());
                TROW.Cells[colHSNCode].AddParagraph().AppendText(dsOrderMaster.Rows[i]["HSNCode"].ToString());
                //            TROW.Cells[colDescription].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Description"].ToString());
                //TROW.Cells[colGRNMaterialDetail].AddParagraph().AppendText(dsOrderMaster.Rows[i]["GRDDescrition"].ToString());
                TROW.Cells[colLotNo].AddParagraph().AppendText(dsOrderMaster.Rows[i]["LotNo"].ToString());
                TROW.Cells[colQty].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["POTransactionQty"].ToString()).ToString("F2"));
                TROW.Cells[colRate].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["TransactionRate"].ToString()).ToString("F4"));
                TROW.Cells[colUoM].AddParagraph().AppendText(dsOrderMaster.Rows[i]["TransactionUoM"].ToString().ToString());
                TROW.Cells[colQualityStatus].AddParagraph().AppendText(dsOrderMaster.Rows[i]["QualityStatus"].ToString().ToString());
                TROW.Cells[colGrossAmount].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["GrossAmount"].ToString()).ToString("F2"));
                TROW.Cells[colDiscountAmount].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["DiscountAmount"].ToString()).ToString("F2"));
                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString()).ToString("#,##0.00"));
                totalValue += clsStaticInfo.dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString());
                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(totalValue.ToString("F2"));
                if (dv.Count > 0)
                {
                    //dsTax.Tables[0].DefaultView.RowFilter = "MasterOrderItemId='" + dsOrderItems.Tables[0].Rows[i]["MasterOrderItemId"].ToString() + "'";
                    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
                    //double totalTax = 0;
                    for (int T = 0; T < dv.Count; T++)
                    {
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND InventoryReceiveDetailId ='" + dsOrderMaster.Rows[i]["InventoryReceiveDetailId"].ToString() + "'";
                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("#,##0.00"));
                        }
                    }
                }
            }

            ROW++;
            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);



            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (C == colHSNCode || C == colRate || C == colQualityStatus || C == colLotNo || C == colMaterialGroup || C == colArticle || C == colChar1 || C == colChar2 || C == colChar3 || /*C == colMaterialDetail ||*/ /*C == colDescription || C == colGRNMaterialDetail ||*/ C == colRowId || C == colRate || C == colUoM || dicTaxes.ContainsValue(C))
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStaticInfo.dbl(item.Text);
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

            double total = clsStaticInfo.dbl(dsOrderMaster.Compute("SUM(TrnAmount)", "").ToString())
                //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                + clsStaticInfo.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());

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
                // TROW.Cells[0].Width = 120;
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


            IWParagraphStyle myStyleRightAlign = document.AddParagraphStyle("MyStyleRightAlign");
            //Sets the formatting of the style
            myStyleRightAlign.CharacterFormat.FontSize = 8f;
            //myStyleRightAlign.CharacterFormat.TextColor = Color.Black;
            myStyleRightAlign.CharacterFormat.TextColor = System.Drawing.Color.Black;
            myStyleRightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;



            for (int R = 1; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];



                foreach (WParagraph item in TROW.Cells[colQty].Paragraphs)
                {
                    item.ApplyStyle("MyStyleRightAlign");
                }


                foreach (WParagraph item in TROW.Cells[colRate].Paragraphs)
                {
                    item.ApplyStyle("MyStyleRightAlign");
                }


                foreach (WParagraph item in TROW.Cells[colTotalTaxableAmount].Paragraphs)
                {
                    item.ApplyStyle("MyStyleRightAlign");
                }


            }

            #endregion paragrpath formats
            #region merging section

            //tax codes merging (horizontal)
            ROW = 0;
            for (int i = 0; i < dv.Count; i++)
                wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            WTableRow TROWe = wTable.LastRow;
            //for (int i = 0; i <= colTotalTaxableAmount; i++)
            //{
            //    TROWe.Cells[i].Width = wTable.Rows[0].Cells[i].Width;
            //    wTable.ApplyVerticalMerge(i, ROW - 1, ROW);
            //}
            //wTable.ApplyVerticalMerge(i, ROW - 1, ROW);




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

        public DataTable loadOrderMasterItems(string OrderMasterID)
        {
            string strSQL;

            try
            {
                strSQL = @"select so.MasterOrderItemId,so.Id AS SOID,CONCAT( mm.[Description],' ',a.StandardName) AS MaterialDesc,
                                so.Qty,uom.UserName AS UOM,SO.Rate,so.Qty*so.Rate AS Amount,isnull(SO.Discount,0) AS Discount
                                  from [TRN].[MasterOrderItem] T
                                INNER JOIN [TRN].[MasterOrder] O ON o.Id=t.MasterOrderId
                                INNER JOIN [TRN].[SalesOrder]  SO ON so.MasterOrderItemId=t.Id
                                LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=t.MaterialMasterId
                                LEFT OUTER JOIN mst.MaterialGroupMaster AS mgm ON mgm.Id=mm.MaterialGroupMasterId
                                LEFT OUTER JOIN [MST].[MaterialMasterArticle] A ON a.Id=t.ArticleId
                                LEFT OUTER JOIN [SCS].[UnitOfMeasurement] UOM ON uom.Id=o.TotalQtyUOMId
                                where MasterOrderId='" + OrderMasterID + "'";

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
        public DataTable loadOrderMasterTax(string OrderMasterID)
        {
            string strSQL;

            try
            {
                strSQL = @"select InventoryServiceId,PO.Id PurchaseOrderId,InventoryReceiveDetailId,tg.Code AS TaxCode,IRT.Percentage, IRT.TaxAmount from TRN.InventoryReceive PO
                               INNER JOIN trn.inventoryReceiveDetail IRD ON IRD.InventoryReceiveId = PO.Id
                               Inner join trn.InventoryReceiveTax IRT ON IRT.InventoryReceiveId = PO.Id and IRT.InventoryReceiveDetailId = IRD.Id
                               LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=IRT.TaxCategoryId
                                 WHERE PO.Id='" + OrderMasterID + @"' 
								 and InventoryReceiveDetailId  is not null and  InventoryServiceId is null 
								 ORDER BY tg.[Sequence] ";

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

        public DataTable loadInventoryReceiveAdditionalTax(string grnId)
        {
            string strSQL;

            try
            {
                strSQL = @"Select TxC.UserName Taxname  ,IRAT.ID ,IRAT.TaxCodeId TaxCode,IRAT.TaxAmount,IRAT.Percentage   from [TRN].[InventoryReceiveAdditionalTax] IRAT
						LEFT JOIN TRN.InventoryReceive IR ON IR.ID= IRAT.InventoryReceiveId
						LEFT JOIN [MST].[TaxCode] TxC ON TxC.Id= IRAT.TaxCodeId
                        where IR.Id = '" + grnId + "'";

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

        public double makeInventoryReceiveAdditionalTaxTable(WordDocument document, DataTable dsOrderMaster, string grnId)
        {
            string replaceString = "{InventoryReceiveAdditionalTax}";

            ReportUtility ru = new ReportUtility();

            DataTable dsTax;
            //clsDataContext data = new clsDataContext();

            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign1");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 8f;
            //rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.CharacterFormat.TextColor = System.Drawing.Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;


            dsTax = loadInventoryReceiveAdditionalTax(grnId);

            int LasColumnIndex = 1;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));

            //LasColumnIndex++;
            //dicTaxes.Add("totaltax", LasColumnIndex);
            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    //LasColumnIndex++;
                }
            }

            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();


            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;
            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxname");
            range.ApplyCharacterFormat(FontBold);
            int colTaxname = COL; COL++;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Percentage");
            range.ApplyCharacterFormat(FontBold);
            int colPercentage = COL;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Tax Amount");
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                //for (int i = 0; i < dv.Count; i++)
                //{
                //	//two columns required for tax
                //	COL++;
                //	range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                //	range.ApplyCharacterFormat(FontBold);
                //	COL++;
                //	range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                //}
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
            }

            wTable.Rows.Add(TemplateRow);
            ROW++;

            //if (dv.Count > 0)
            //{
            //	for (int i = 0; i < dv.Count; i++)
            //	{

            //		range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
            //		range.ApplyCharacterFormat(FontBold);
            //		range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
            //		range.ApplyCharacterFormat(FontBold);

            //	}
            //}

            #endregion column headers
            double totalValue = 0;
            int startRow = ROW + 1;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                //ROW++;
                //wTable.AddRow();
                WTableRow TROW = wTable.LastRow;


                IParagraphItem p = TROW.Cells[colTaxname].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Taxname"].ToString());
                TROW.Cells[colPercentage].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["Percentage"].ToString()).ToString("#,##0.0000"));

                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["TaxAmount"].ToString()).ToString("#,##0.00"));
            }

            #region Sub Total


            double total = clsStaticInfo.dbl(dsOrderMaster.Compute("SUM(TaxAmount)", "").ToString());

            #endregion Total


            //ROW++;

            #region Total Payable
            #endregion Total Payable

            //ROW++;

            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle3 = document.AddParagraphStyle("MyStyle3");
            //Sets the formatting of the style
            myStyle3.CharacterFormat.FontSize = 8f;
            //myStyle3.CharacterFormat.TextColor = Color.Black;
            myStyle3.CharacterFormat.TextColor = System.Drawing.Color.Black;
            myStyle3.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 35;
                if (dv.Count < 3)
                    TROW.Cells[0].Width = +((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle3");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            #endregion merging section

            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            int k = document.Replace(replaceString, textBodyPart, false, false);
            return total;
        }

        public void loadGRNShortageTable(WordDocument document, string grnId)
        {
            string replaceString = "{shortage}";

            DataTable dtlOrderItems;

            dtlOrderItems = loadGRNShortageMaster(grnId);
            if (dtlOrderItems.Rows.Count > 0)
            {
                document.Replace("{ShortageDetails}", "Shortage Details", true, true);

                //dsTax = loadOrderMasterTax(grnId);

                int LasColumnIndex = 6;
                Dictionary<string, int> dicTaxes = new Dictionary<string, int>();

                WTable wTable = new WTable(document);
                wTable.TableFormat.Borders.LineWidth = 1;
                wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
                int ROW = 0; int COL = 0;
                wTable.ResetCells(1, LasColumnIndex + 1);

                WTableRow TemplateRow = wTable.Rows[0].Clone();

                #region column headers
                document.EnsureMinimal();
                //wTable.Title = "Material Details";
                //wTable.Description = "This table shows the price details of PI";
                //wTable.IndentFromLeft = 10;


                //string UOM = dsOrderMaster.Tables[0].Rows[0]["UOM"].ToString();
                //string Currency = dsOrderMaster.Tables[0].Rows[0]["Currency"].ToString();
                WCharacterFormat FontBold = new WCharacterFormat(document);
                FontBold.Bold = true;
                // = true;




                IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("RowId");
                range.ApplyCharacterFormat(FontBold);
                int colRowIdShort = COL; COL++;
                wTable.Rows[ROW].Cells[COL].Width = 50;

                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Material");
                wTable.Rows[ROW].Cells[COL].Width = 100;
                range.ApplyCharacterFormat(FontBold);
                int colMaterial = COL; COL++;

                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article");
                wTable.Rows[ROW].Cells[COL].Width = 50;
                range.ApplyCharacterFormat(FontBold);
                int colArticle = COL; COL++;

                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("InvoiceRate");
                wTable.Rows[ROW].Cells[COL].Width = 50;
                range.ApplyCharacterFormat(FontBold);
                int colMaterialTranRate = COL; COL++;


                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
                wTable.Rows[ROW].Cells[COL].Width = 50;
                range.ApplyCharacterFormat(FontBold);
                int colShortageQty = COL; COL++;



                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate(%)");
                wTable.Rows[ROW].Cells[COL].Width = 50;
                range.ApplyCharacterFormat(FontBold);
                int colShortageRatePercent = COL;
                COL++;


                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Value (" + dtlOrderItems.Rows[0]["Code"].ToString() + ")");
                wTable.Rows[ROW].Cells[COL].Width = 60;
                range.ApplyCharacterFormat(FontBold);
                int colShortageValue = COL;



                //int colTotalTaxableAmount = COL;
                //if (dv.Count > 0)
                //{
                //    COL++;
                //    colTotalTaxableAmount = COL;
                //    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
                //    range.ApplyCharacterFormat(FontBold);
                //    //COL++;
                //    //for (int i = 0; i < dv.Count; i++)
                //    //{
                //    //    //two columns required for tax
                //    //    COL++;
                //    //    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                //    //    range.ApplyCharacterFormat(FontBold);

                //    //    COL++;
                //    //    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                //    //}
                //}
                //else
                //{
                //    COL++;
                //    colTotalTaxableAmount = COL;
                //    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Value");
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
                double totalValue = 0;
                int startRow = ROW + 1;
                for (int i = 0; i < dtlOrderItems.Rows.Count; i++)
                {
                    //if (Convert.ToDouble(dtlOrderItems.Rows[i]["ShortageQty"]) > 0)
                    //{



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
                    TROW.Cells[colRowIdShort].AddParagraph().AppendText(dtlOrderItems.Rows[i]["InventoryReceiveDetailsId"].ToString());
                    TROW.Cells[colMaterial].AddParagraph().AppendText(dtlOrderItems.Rows[i]["MaterialMaster"].ToString());
                    TROW.Cells[colArticle].AddParagraph().AppendText(dtlOrderItems.Rows[i]["Article"].ToString());

                    TROW.Cells[colMaterialTranRate].AddParagraph().AppendText(Convert.ToDouble(dtlOrderItems.Rows[i]["MaterialTranRate"]).ToString("F2"));
                    //TROW.Cells[colMaterialTranRate].Width = 60;
                    TROW.Cells[colShortageQty].AddParagraph().AppendText(Convert.ToDouble(dtlOrderItems.Rows[i]["ShortageQty"]).ToString("F2"));
                    //TROW.Cells[colShortageQty].Width = 60;
                    TROW.Cells[colShortageRatePercent].AddParagraph().AppendText(Convert.ToDouble(dtlOrderItems.Rows[i]["ShortageRatePercent"]).ToString("F2"));
                    //TROW.Cells[colShortageRatePercent].Width = 60;
                    TROW.Cells[colShortageValue].AddParagraph().AppendText(Convert.ToDouble(dtlOrderItems.Rows[i]["ShortageValue"]).ToString("F2"));
                    //TROW.Cells[colShortageValue].Width = 60;

                    //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dtOrderItems.Rows[i]["TrnAmount"].ToString()).ToString("F2"));

                    //totalValue += clsStdLib.dbl(dtOrderItems.Rows[i]["TrnAmount"].ToString());

                    //if (dv.Count > 0)
                    //{
                    //    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
                    //    //double totalTax = 0;

                    //    for (int T = 0; T < dv.Count; T++)
                    //    {
                    //        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND InventoryReceiveDetailId ='" + dsOrderMaster.Rows[i]["InventoryReceiveDetailId"].ToString() + "'";
                    //        if (dvtax.Count > 0)
                    //        {
                    //            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));

                    //            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("F2"));

                    //        }
                    //    }
                    //}
                    //}
                }

                ROW++;
                #region Total
                int TotalRow = ROW;
                wTable.AddRow();
                WTableRow _TROW = wTable.LastRow;
                _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);


                for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
                {
                    if (C == colMaterialTranRate || C == colShortageRatePercent || C == colRowIdShort || dicTaxes.ContainsValue(C))
                        continue;

                    double value = 0;
                    for (int i = startRow; i < TotalRow; i++)
                    {

                        foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                        {
                            value += clsStaticInfo.dbl(item.Text);
                        }
                    }
                    _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);
                }
                #endregion Total


                ROW++;
                //#region Sub Total
                //int SubTotalRow = ROW;
                //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
                //wTable.AddRow();
                //_TROW = wTable.LastRow;

                //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

                //double total = clsStdLib.dbl(dtlOrderItems.Compute("SUM(ShortageQty)", "").ToString())

                //+ clsStdLib.dbl(dtlOrderItems.Compute("SUM(ShortageValue)", "").ToString());

                //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2"));

                //#endregion Total


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
                IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyleS");
                //Sets the formatting of the style
                myStyle.CharacterFormat.FontSize = 8f;
                //myStyle.CharacterFormat.TextColor = Color.Black;
                myStyle.CharacterFormat.TextColor = System.Drawing.Color.Black;
                myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

                for (int R = 0; R < wTable.Rows.Count; R++)
                {
                    WTableRow TROW = wTable.Rows[R];
                    TROW.Cells[0].Width = 50;

                    for (int CE = 0; CE < TROW.Cells.Count; CE++)
                    {
                        foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                        {
                            item.ApplyStyle("MyStyleS");
                        }
                    }

                }

                #endregion paragrpath formats

                //#region paragrpath formats
                //Adds a new paragraph style named "MyStyle"
                //IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyles");
                ////Sets the formatting of the style
                //myStyle.CharacterFormat.FontSize = 8f;
                //myStyle.CharacterFormat.TextColor = Color.Black;
                //myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

                //for (int R = 0; R < wTable.Rows.Count; R++)
                //{
                //    WTableRow TROW = wTable.Rows[R];
                //    TROW.Cells[0].Width = 35;
                //    //if (dv.Count < 3)
                //    //    TROW.Cells[0].Width = 70 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                //    for (int CE = 0; CE < TROW.Cells.Count; CE++)
                //    {
                //        foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                //        {
                //            item.ApplyStyle("MyStyles");
                //        }
                //    }
                //}

                //#endregion paragrpath formats

                #region
                //tax codes merging (horizontal)
                ROW = 0;
                //for (int i = 0; i < dv.Count; i++)
                //    wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

                //primary cells merging (veritcal)
                //ROW++;
                //for (int i = 0; i <= colTotalTaxableAmount; i++)
                //    wTable.ApplyVerticalMerge(i, ROW - 1, ROW);


                //WParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
                //style.CharacterFormat.Bold = true;
                //style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;I
                //Adds new paragraph to the section


                //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
                //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
                //        PARA.ApplyStyle("SubTotalStyle");

                //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
                #endregion merging section

                TextBodyPart textBodyPart = new TextBodyPart(document);
                textBodyPart.BodyItems.Add(wTable);
                document.Replace(replaceString, textBodyPart, true, true);

                //return total;
            }
        }

        public DataTable loadGRNShortageMaster(string OrderMasterID)
        {
            string strSQL;

            try
            {
                strSQL = @"select IRD.Id AS InventoryReceiveDetailsId,IRD.MaterialTranRate
	                                ,IRD.ShortageQty
	                                ,IRD.ShortageRatePercent
	                                ,IRD.ShortageValue 
									,C.Code
									,MM.UserName MaterialMaster
									,MMA.StandardName Article
                                FROM trn.InventoryReceiveDetail IRD
								 LEFT JOIN trn.InventoryMaterial AS IOM ON IRD.InventoryMaterialId = IOM.Id
								 LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = IOM.MaterialMasterId
								 LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IOM.ArticleId
								LEFT JOIn trn.InventoryReceive IR ON IR.Id=InventoryReceiveId
								LEFT JOIN scs.Currency C on C.Id=IR.CurrencyId
                                where IRD.InventoryReceiveId='" + OrderMasterID + "'and isnull(IRD.ShortageQty,0)>0";


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


        public void loadGRNRejectionTable(WordDocument document, string grnId)
        {
            string replaceString = "{rejection}";



            DataTable dtOrderItems, dsTax;
            dtOrderItems = loadGRNRejectionMaster(grnId);
            if (dtOrderItems.Rows.Count > 0)
            {
                document.Replace("{RejectionDetails}", "Rejection Details", true, true);


                //  dsTax = loadOrderMasterTax(grnId);

                int LasColumnIndex = 6;
                Dictionary<string, int> dicTaxes = new Dictionary<string, int>();


                WTable wTable = new WTable(document);
                wTable.TableFormat.Borders.LineWidth = 1;
                wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
                int ROW = 0; int COL = 0;
                wTable.ResetCells(1, LasColumnIndex + 1);

                WTableRow TemplateRow = wTable.Rows[0].Clone();

                #region column headers
                document.EnsureMinimal();

                WCharacterFormat FontBold = new WCharacterFormat(document);
                FontBold.Bold = true;
                IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("RowId");
                range.ApplyCharacterFormat(FontBold);
                int colRowIdRej = COL; COL++;
                wTable.Rows[ROW].Cells[COL].Width = 50;

                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Material");
                wTable.Rows[ROW].Cells[COL].Width = 50;
                range.ApplyCharacterFormat(FontBold);
                int colMaterial = COL; COL++;

                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article");
                wTable.Rows[ROW].Cells[COL].Width = 50;
                range.ApplyCharacterFormat(FontBold);
                int colArticle = COL; COL++;

                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("InvoiceRate");
                wTable.Rows[ROW].Cells[COL].Width = 50;
                range.ApplyCharacterFormat(FontBold);
                int colMaterialTranRate = COL; COL++;


                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty ");
                wTable.Rows[ROW].Cells[COL].Width = 50;
                range.ApplyCharacterFormat(FontBold);
                int colRejectionQty = COL; COL++;



                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate(%) ");
                wTable.Rows[ROW].Cells[COL].Width = 50;
                range.ApplyCharacterFormat(FontBold);
                int colRejectRatePercent = COL; COL++;


                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Value (" + dtOrderItems.Rows[0]["Code"].ToString() + ")");
                wTable.Rows[ROW].Cells[COL].Width = 60;
                range.ApplyCharacterFormat(FontBold);
                int colRejectValue = COL;

                #endregion column headers

                double totalValue = 0;
                int startRow = ROW + 1;
                for (int i = 0; i < dtOrderItems.Rows.Count; i++)
                {
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

                    TROW.Cells[colRowIdRej].AddParagraph().AppendText(dtOrderItems.Rows[i]["InventoryReceiveDetailsId"].ToString());
                    TROW.Cells[colMaterial].AddParagraph().AppendText(dtOrderItems.Rows[i]["MaterialMaster"].ToString());
                    TROW.Cells[colArticle].AddParagraph().AppendText(dtOrderItems.Rows[i]["Article"].ToString());
                    TROW.Cells[colMaterialTranRate].AddParagraph().AppendText(Convert.ToDouble(dtOrderItems.Rows[i]["MaterialTranRate"]).ToString());
                    //TROW.Cells[colMaterialTranRate].Width = 60;
                    TROW.Cells[colRejectionQty].AddParagraph().AppendText(Convert.ToDouble(dtOrderItems.Rows[i]["RejectionQty"]).ToString());
                    //TROW.Cells[colRejectionQty].Width = 60;
                    TROW.Cells[colRejectRatePercent].AddParagraph().AppendText(Convert.ToDouble(dtOrderItems.Rows[i]["RejectRatePercent"]).ToString());
                    //TROW.Cells[colRejectRatePercent].Width = 60;
                    TROW.Cells[colRejectValue].AddParagraph().AppendText(Convert.ToDouble(dtOrderItems.Rows[i]["RejectValue"]).ToString());
                    //TROW.Cells[colRejectValue].Width = 60;


                }

                ROW++;
                #region Total
                int TotalRow = ROW;
                wTable.AddRow();
                WTableRow _TROW = wTable.LastRow;
                _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);


                for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
                {
                    if (C == colMaterialTranRate || C == colRejectRatePercent || dicTaxes.ContainsValue(C))
                        continue;

                    double value = 0;
                    for (int i = startRow; i < TotalRow; i++)
                    {

                        foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                        {
                            value += clsStaticInfo.dbl(item.Text);
                        }
                    }
                    _TROW.Cells[C].AddParagraph().AppendText(value.ToString("F2")).ApplyCharacterFormat(FontBold);
                }
                #endregion Total


                ROW++;
                //#region Sub Total
                //int SubTotalRow = ROW;
                //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
                //wTable.AddRow();
                //_TROW = wTable.LastRow;

                //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

                //double total = clsStdLib.dbl(dtOrderItems.Compute("SUM(RejectValue)", "").ToString());
                ////- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                ////+ clsStdLib.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());

                //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2"));

                //#endregion Total




                ROW++;

                #region paragrpath formats
                IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyleR");
                //Sets the formatting of the style
                myStyle.CharacterFormat.FontSize = 8f;
                //myStyle.CharacterFormat.TextColor = Color.Black;
                myStyle.CharacterFormat.TextColor = System.Drawing.Color.Black;
                myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

                for (int R = 0; R < wTable.Rows.Count; R++)
                {
                    WTableRow TROW = wTable.Rows[R];
                    TROW.Cells[0].Width = 50;

                    for (int CE = 0; CE < TROW.Cells.Count; CE++)
                    {
                        foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                        {
                            item.ApplyStyle("MyStyleR");
                        }
                    }

                }

                #endregion paragrpath formats


                #region merging section


                //tax codes merging (horizontal)
                ROW = 0;

                ROW++;

                #endregion merging section

                TextBodyPart textBodyPart = new TextBodyPart(document);
                textBodyPart.BodyItems.Add(wTable);
                document.Replace(replaceString, textBodyPart, true, true);
            }

        }

        public DataTable loadGRNRejectionMaster(string OrderMasterID)
        {
            string strSQL;

            try
            {
                strSQL = @"select IRD.Id AS InventoryReceiveDetailsId,IRD.MaterialTranRate
	                                ,IRD.RejectionQty
	                                ,IRD.RejectRatePercent
	                                ,IRD.RejectValue
									,C.Code
                                    ,MM.UserName MaterialMaster
									,MMA.StandardName Article
                                FROM trn.InventoryReceiveDetail IRD
                                LEFT JOIN trn.InventoryMaterial AS IOM ON IRD.InventoryMaterialId = IOM.Id
								 LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = IOM.MaterialMasterId
								 LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IOM.ArticleId
								LEFT JOIn trn.InventoryReceive IR ON IR.Id=InventoryReceiveId
								LEFT JOIN scs.Currency C on C.Id=IR.CurrencyId
                        where IRD.InventoryReceiveId='" + OrderMasterID + "'and isnull(IRD.ShortageQty,0)>0";


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

        public double makeOrderServiceTable(WordDocument document, DataTable dsOrderMaster, string grnId)
        {
            string replaceString = "{ServiceItems}";

            ReportUtility ru = new ReportUtility();

            DataTable dsTax;
            //clsDataContext data = new clsDataContext();

            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 8f;
            // rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.CharacterFormat.TextColor = System.Drawing.Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;


            dsTax = loadGRNServiceMasterTex(grnId);

            int LasColumnIndex = 1;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));

            //LasColumnIndex++;
            //dicTaxes.Add("totaltax", LasColumnIndex);
            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    LasColumnIndex++;
                }
            }

            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();


            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;
            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Service Name");
            range.ApplyCharacterFormat(FontBold);
            //wTable.Rows[ROW].Cells[COL].Width = 100;
            int colServiceName = COL; //COL++;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    //two columns required for tax
                    COL++;
                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                    range.ApplyCharacterFormat(FontBold);
                    COL++;
                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                }
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
            }

            wTable.Rows.Add(TemplateRow);
            ROW++;

            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {

                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
                    range.ApplyCharacterFormat(FontBold);
                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                    range.ApplyCharacterFormat(FontBold);

                }
            }

            #endregion column headers
            double totalValue = 0;
            int startRow = ROW + 1;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
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
                IParagraphItem p = TROW.Cells[colServiceName].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Service"].ToString());

                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["Amount"].ToString()).ToString("#,##0.00"));

                totalValue += clsStaticInfo.dbl(dsOrderMaster.Rows[i]["Amount"].ToString());

                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(totalValue.ToString("F2"));

                if (dv.Count > 0)
                {
                    //dsTax.Tables[0].DefaultView.RowFilter = "MasterOrderItemId='" + dsOrderItems.Tables[0].Rows[i]["MasterOrderItemId"].ToString() + "'";
                    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
                    //double totalTax = 0;

                    for (int T = 0; T < dv.Count; T++)
                    {
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND InventoryServiceId='" + dsOrderMaster.Rows[i]["ServiceId"] + "'";
                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("F2"));
                        }
                    }
                }
            }

            ROW++;

            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            //wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;

            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);

            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (dicTaxes.ContainsValue(C))
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStaticInfo.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("F2"));
            }


            #endregion Total


            ROW++;


            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStaticInfo.dbl(dsOrderMaster.Compute("SUM(Amount)", "").ToString())
//- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
+ clsStaticInfo.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());



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
            IWParagraphStyle myStyle2 = document.AddParagraphStyle("MyStyle2");
            //Sets the formatting of the style
            myStyle2.CharacterFormat.FontSize = 8f;
            //myStyle2.CharacterFormat.TextColor = Color.Black;
            myStyle2.CharacterFormat.TextColor = System.Drawing.Color.Black;
            myStyle2.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 35;
                if (dv.Count < 3)
                    TROW.Cells[0].Width = +((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle2");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            for (int i = 0; i < dv.Count; i++)
                wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            for (int i = 0; i <= colTotalTaxableAmount; i++)
                wTable.ApplyVerticalMerge(i, ROW - 1, ROW);

            IWParagraphStyle style2 = document.AddParagraphStyle("SubTotalStyle2");
            style2.CharacterFormat.Bold = true;
            style2.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;


            //Adds new paragraph to the section


            //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
            //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
            //        PARA.ApplyStyle("SubTotalStyle2");

            //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
            #endregion merging section



            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);
            return total;
        }

        public DataTable loadGRNServiceMasterTex(string OrderMasterID)
        {
            string strSQL;
            try
            {
                strSQL = @"select InventoryServiceId,IR.Id PurchaseOrderId,tg.Code AS TaxCode,IRT.Percentage, IRT.TaxAmount
                    from TRN.InventoryReceive IR
                              INNER JOIN trn.InventoryService ISER ON ISER.InventoryReceiveId = IR.Id
                              Inner join trn.InventoryReceiveTax IRT ON IRT.InventoryReceiveId = IR.Id and IRT.InventoryServiceId = ISER.Id
                               LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=IRT.TaxCategoryId
                                WHERE IR.Id='" + OrderMasterID + @"'
								and InventoryServiceId  is not null and   InventoryReceiveDetailId is null 
								 ORDER BY tg.[Sequence]";
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

        // Value Added Receipt template

        public void ValAddedGRNReport(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId)
        {

            var fileName = "";
            var strPath = "";

            var File = "";

            ReportUtility ru = new ReportUtility();

            //tempId = dtLangName.Rows[0]["UserName"].ToString();
            fileName = "JobWorkValAddedGRNReceiptReport" + plantId + ".docx";
            strPath = Path.Combine(ResourcesPathReader.GetJobWorkValAddedReceiptTemplatePath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }

            //makeDictionary();
            ////A opens input document.
            WordDocument document = new WordDocument(File, FormatType.Docx);
            //Gets the paragraph at index 1
            try
            {
                WSection section = document.Sections[0];

                DataTable dtOrderMaster;


                dtOrderMaster = ValAddedloadGRNMaterialMaster(grnId);


                var invoicePartyAddress = ru.GetAddress(dtOrderMaster.Rows[0]["InvoicePartyAddressMasterId"].ToString(), dtOrderMaster.Rows[0]["InvoicingByAddress"].ToString());
                document.Replace("{InvoicingPartyAddress}", invoicePartyAddress, false, false);

                var vendorPartyAddress = ru.GetAddress(dtOrderMaster.Rows[0]["VendorAddressMasterId"].ToString(), "");
                document.Replace("{VendorAddress}", vendorPartyAddress, false, false);

                Dictionary<string, string> columns = new Dictionary<string, string>();

                var poApprovedStatus = "";
                foreach (DataColumn item in dtOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);
                var dsServiceItems = ValAddedloadGRNServiceMaster(grnId);
                var materialTotal = ValAddedmakeOrderDetailsTable(document, dtOrderMaster, grnId);//Material Details 
                var dsInventoryReceiveAdditionalTax = ValAddedloadInventoryReceiveAdditionalTax(grnId);
                var InventoryReceiveAdditionalTax = 0.00;
                if (dsInventoryReceiveAdditionalTax.Rows.Count > 0)

                {
                    InventoryReceiveAdditionalTax = ValAddedmakeInventoryReceiveAdditionalTaxTable(document, dsInventoryReceiveAdditionalTax, grnId);//Service Details 
                                                                                                                                             //document.Replace("{ServiceDetails}", "Service Details", true, true);

                    //{TotalInWords}
                }
                ValAddedloadGRNShortageTable(document, grnId);
                ValAddedloadGRNRejectionTable(document, grnId);
                var serviceTotal = 0.00;
                if (dsServiceItems.Rows.Count > 0)

                {
                    serviceTotal = ValAddedmakeOrderServiceTable(document, dsServiceItems, grnId);//Service Details 
                    document.Replace("{ServiceDetails}", "Service Details", true, true);

                    //{TotalInWords}
                }
                document.Replace("{GrandTotal}", ((materialTotal + serviceTotal) + InventoryReceiveAdditionalTax).ToString("#,##0.00") + " " + dtOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{TotalInWords}", ru.InWord(((materialTotal + serviceTotal) + InventoryReceiveAdditionalTax), dtOrderMaster.Rows[0]["CurrencyId"].ToString()), true, true);

                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                List<string> strReplace = new List<string>();

                StringCollection strColDistinct = new StringCollection();

                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());

                for (int i = 0; i < strReplace.Count; i++)
                {
                    if (strColDistinct.Contains(strReplace[i].ToUpper()))
                        continue;

                    strColDistinct.Add(strReplace[i].ToUpper());             //For Same Name Use
                    string text = strReplace[i].ToUpper();

                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        ReplaceInfo[text] = document.Replace(text, dtOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);


                //removing any unused place holder
                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "", false, false);

                }

                //Region that is for Pdf.Document
                DocToPDFConverter converter = new DocToPDFConverter();

                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                pdfDocument.PageSettings.Width = 1200;
                pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();

                //Closes the instance of document objects

                //Saves the PDF file 
                string Prefix = "GRN" + grnId;

                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);

                document.Close();
            }


            catch (Exception ex)
            {
                throw ex;

            }
            document.Close();
        }

        public DataTable ValAddedloadGRNMaterialMaster(string OrderMasterID)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT IR.Id grnNumber
							--,PO1.PODate
							,FORMAT(PO.PODate,'dd-MMM-yyyy') as PODate
							,GTE.ModeofTransport
							,HSNC.Code HSNCode
                            ,IR.CompanyGroupId
                            ,IR.CompanyId
                            ,Plant.GSTIN
                            ,ir.PODepended
                          --  ,PO1.POId PONumber
							,PO.Id as PONumber
							,CNO.ContractNo as ContractNO
						 --   ,PO1.ContractNO ContractNO 
							,PLC.LCRef LCNumber
							,PLC.BenificiaryBank BeneficiaryBank
							,PLC.BenificiaryBank OpeningBank
							--,B.UserName BeneficiaryBank
							--,B.UserName OpeningBank
							,PDA.AcceptanceNo AcceptanceNo
							,REPLACE(Convert(VARCHAR(11), PLC.LCDate, 106), ' ', '-') AS LCODate
							,REPLACE(Convert(VARCHAR(11), PDA.AcceptanceDate, 106), ' ', '-') AS AcceptanceDate
                            ,REPLACE(Convert(VARCHAR(11), IR.GRNDate, 106), ' ', '-') AS GRNDate
							 --,GRNType=CASE WHEN IR.GRNType='GRN' then 'GRN Without PO' ELSE 'GRN With PO' END
							  ,GRNType=CASE WHEN IR.GRNType='GRNBYJW' then 'GRN By JW' ELSE 'GRN by JW' END
                            ,REPLACE(Convert(VARCHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
                            ,REPLACE(Convert(VARCHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
                            ,IR.InvoicingPartyPlantId
                            ,INVPARTYPL.UserName InvoicingPartyName
                            ,INVPARTYPL.AddressMasterId InvoicePartyAddressMasterId
                            ,INVPARTYPL.GSTIN InvoicingPartyGSTIN
                            ,ISNULL(IR.InvoicingByAddress,'') InvoicingByAddress
                            ,IR.DeliveryByAddress
                            ,DPARTYPL.UserName DeliveryParty
                            ,IR.DeliveryPartyPlantId
                            ,IOM.MaterialMasterId
                            ,IR.DocRefNo
                            ,REPLACE(Convert(VARCHAR(11), IR.DocDate, 106), ' ', '-') AS DocDate
                            ,IR.GateEntryNo,REPLACE(Convert(VARCHAR(11), IR.EntryDate, 106), ' ', '-') AS GateEntryDate
                            ,CheckedBy=CASE WHEN IR.CheckedByStatus='Checked' Then eI.EmployeeName else '' END
                            ,AuthorizedBy=CASE When IR.AuthorizedByStatus='Approved'then eI1.EmployeeName else '' END
                             ,AddedBy=CASE 
									When IR.CheckedByStatus='ForChecked' Then eI3.EmployeeName
									When IR.CheckedByStatus='Hold' Then eI3.EmployeeName
									When IR.CheckedByStatus='Reject' Then eI3.EmployeeName
									When IR.CheckedByStatus='Checked' Then eI3.EmployeeName
									When IR.CheckedByStatus IS NULL then IR.AddedBy 
									
									else ''
							END
                            ,IR.AddedDate
                            ,IR.UpdatedBy
                            ,IR.UpdatedDate
                            ,IR.IsApproved
                            ,IR.PartyType
                            ,EMPIN.EmployeeName
                              --,Party.UserName VendorName
                            --,Party.AddressMasterId VendorAddressMasterId
                            --,Party.TINNO VendorGSTIN
							,VendorName=case when IR.PartyId is not null then Party.UserName else Pty.UserName End
							,VendorAddressMasterId=case when IR.PartyId is not null then Party.AddressMasterId else Pty.AddressMasterId End
							,VendorGSTIN=case when IR.PartyId is not null then Party.TINNO else Pty.TINNO End
                            ,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
                            ,IR.IsNonCreditable
                            ,IR.CurrencyId
                            ,CRNC.Code AS CurrencyName
                            ,IR.ToCurrencyRate
                            ,BASECRNC.Code AS BaseCurrencyName
                            ,PayTerm.UserName PaymentTerm
                            ,MM.UserName MaterialMaster
                            ,MM.MaterialGroupMasterId
                            ,MGM.UserName MaterialGroupMaster
                            ,IOM.ArticleId
                            ,MMA.StandardName Article
                            ,FC.Id FirstCharId
                            ,FC.UserName FirstChar
                            ,IOM.FirstCharacteristicsValueId
                            ,FCV.UserName AS FirstCharacteristicsValue
                            ,IOM.SecondCharacteristicsValueId
                            ,SCV.UserName AS SecondCharacteristicsValue
                            ,IOM.ThirdCharacteristicsValueId
                            ,TCV.UserName AS ThirdCharacteristicsValue
                            ,SC.Id SecondCharId
                            ,SC.UserName SecondChar
                            ,TC.Id ThirdCharId
                            ,TC.UserName ThirdChar
                            ,ROUND(IRD.TransactionQty, 2) POTransactionQty
                            ,ROUND(IRD.MaterialTranRate, 2) TransactionRate
                            --,ROUND((IRD.TransactionQty * IRD.MaterialTranRate), 2) AS TrnAmount
							,TrnAmount=(IRD.GrossAmount-IRD.DiscountAmount)
                            ,IRD.TotalMaterialTranAmount BaseAmount
                            ,IRD.TotalTaxAmount AS BaseTaxAmount
                            ,TaxAmount = (
                            SELECT SUM(TaxAmount)
                            FROM dbo.JWTransformationPOTax --[TRN].[PurchaseOrderTax]
                            WHERE JWTransformationPODetailId = IRD.JWTransformationPODetailId
                            )
                            ,ServiceTaxAmount = (
                            SELECT SUM(TotalTaxAmount)
                            FROM dbo.JWTransformationPOService  --[TRN].[POService]
                            WHERE JWTransformationPOId = IRD.JWTransformationPOId
                            )
                            ,IRD.ChargesTranAmount
                            ,IRD.CountryId

                            ,IRD.TransactionUoMId
                            ,TUoM.ShortName  AS TransactionUoM
                            ,IRD.Id InventoryReceiveDetailId--,MRD.MaterialDetail
							,POD.Description,IRD.Description AS GRDDescrition
                            ,CheckStatus= CASE when IR.CheckedByStatus='ForChecked' Then 'To be checked'
                            when IR.CheckedByStatus='Hold' Then 'Hold'
                            when IR.CheckedByStatus='Reject' Then 'Reject'
                            when IR.CheckedByStatus='Checked' Then 'Checked'
                            else ''

                            END
                            ,ApproveStatus= CASE
                            when IR.AuthorizedByStatus='Reject' Then 'Reject For Approved'
                            when IR.AuthorizedByStatus='Hold' Then 'Hold For Approved'
                            when IR.AuthorizedByStatus='For Approval' Then 'To be Approval'
                            when IR.AuthorizedByStatus='Approved' Then 'Approved'
                            else ''
							END
						,GRNStatus= CASE when IR.CheckedByStatus='ForChecked' Then 'To be checked'
								when IR.CheckedByStatus='Hold' Then 'Hold'
								when IR.CheckedByStatus='Reject' Then 'Reject'
								when IR.CheckedByStatus='Checked' AND IR.AuthorizedByStatus='To be Approval' Then 'Checked' 
								when IR.CheckedByStatus='Checked'  AND IR.AuthorizedByStatus='Reject' Then 'Reject For Approved'
								when IR.CheckedByStatus='Checked' AND IR.AuthorizedByStatus='Hold' Then 'Hold For Approved'
								when IR.CheckedByStatus='Checked' and IR.AuthorizedByStatus='For Approval' Then 'To be Approval'
								when IR.CheckedByStatus='Checked' and IR.AuthorizedByStatus='Approved' Then 'Approved'
								when IR.CheckedByStatus Is null and IR.AuthorizedByStatus Is null Then 'Approved'
                            else ''
                            END
							,IRD.LotNo , IRD.QualityStatus , IRD.GrossAmount ,IRD.DiscountAmount
                            FROM TRN.InventoryReceive IR
                            LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
                            LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                            LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
                            LEFT JOIN SCS.Currency CRNC ON CRNC.Id = IR.CurrencyId
                            LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = IR.BaseCurrencyId
                            LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = IR.PaymentTermId
                            LEFT JOIN HKP.PartyPlant INVPARTYPL ON INVPARTYPL.Id = IR.InvoicingPartyPlantId
                            LEFT JOIN HKP.PartyPlant DPARTYPL ON DPARTYPL.Id = IR.DeliveryPartyPlantId
                            LEFT JOIN trn.inventoryReceiveDetail IRD ON IR.Id = IRD.InventoryReceiveId
                            LEFT JOIN HKP.Party Party ON Party.Id = IR.PartyId
                            LEFT JOIN trn.InventoryMaterial AS IOM ON IRD.InventoryMaterialId = IOM.Id
                             LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = IOM.MaterialMasterId
							LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
                            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                            LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IOM.ArticleId
                            LEFT JOIN HKP.Characteristics AS FC ON IOM.FirstCharacteristicsId = FC.Id
                            LEFT JOIN HKP.Characteristics AS SC ON IOM.SecondCharacteristicsId = SC.Id
                            LEFT JOIN HKP.Characteristics AS TC ON IOM.ThirdCharacteristicsId = TC.Id
                            LEFT JOIN HKP.CharacteristicsValue AS FCV ON IOM.FirstCharacteristicsValueId = FCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS SCV ON IOM.SecondCharacteristicsValueId = SCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS TCV ON IOM.ThirdCharacteristicsValueId = TCV.Id
                            JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
                       --     LEFT JOIN trn.PurchaseOrderDetail POD ON POD.Id = IRD.PODetailsId
							LEFT JOIN dbo.JWTransformationPODetail POD ON POD.Id = IRD.JWTransformationPODetailId
	                    --    LEFT JOIN TRN.PurchaseOrder PO ON PO.Id = IRD.POId
							 LEFT JOIN dbo.JWTransformationPO PO ON PO.Id = IRD.JWTransformationPOId
                             left join HKP.Party Pty on Pty.Id=PO.PartyId
                            	LEFT JOIN (select Distinct PDAA.Id,AcceptanceDate,AcceptanceNo,ACMAP.GRNId from TRN.GRNAcceptanceMap ACMAP 
									left Join trn.PurchaseDocAcceptance  PDAA ON PDAA.Id=ACMAP.PurchaseDocumentAcceptanceId
									)PDA ON PDA.GRNId=IR.Id
							LEFT JOIN [dbo].[Contract] CNO ON CNO.Id = PO.ContractId
							LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id = PO.PurchaseLCId

	                        --LEFT JOIN [HKP].[Bank] B ON B.Id = PLC.BenificiaryBankId
                       --     Left Join TRN.MaterialRequsitionDetails MRD ON MRD.Id=POD.RequisitionDetailId
                         --   LEFT JOIN EmployeeInformation AS EMPIN ON EMPIN.SystemId= IR.EmployeeId
							LEFT JOIN EmployeeInformation AS EMPIN ON EMPIN.SystemId= IR.ByWhomEmployeeId
                            LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
                            left join [SEC].[User] U on U.UserId=IR.AddedBy
                            LEFT JOIN dbo.EmployeeInformation eI3 ON eI3.SystemId=U.EmployeeId
							LEFT JOIN [TRN].[GateEntry] GTE  ON GTE.ID= IR.GateEntryNo
                            WHERE IR.Id ='"+ OrderMasterID + @"' and IOM.MaterialMasterId is not NULL

                            Union ALL
                            SELECT IR.Id grnNumber
							,GTE.ModeofTransport
							--,PO1.PODate
							,FORMAT(PO.PODate,'dd-MMM-yyyy') as PODate
							,HSNC.Code HSNCode
                            ,IR.CompanyGroupId
                            ,IR.CompanyId
                            ,Plant.GSTIN
                            ,ir.PODepended
                            --,PO1.POId PONumber
							,PO.Id as PONumber
						    ,CNO.ContractNo as ContractNO
						 -- ,PO1.ContractNO ContractNO 
							,PLC.LCRef LCNumber
							,PLC.BenificiaryBank BeneficiaryBank
							,PLC.BenificiaryBank OpeningBank
							,PDA.AcceptanceNo AcceptanceNo
							,REPLACE(Convert(VARCHAR(11), PLC.LCDate, 106), ' ', '-') AS LCODate
							,REPLACE(Convert(VARCHAR(11), PDA.AcceptanceDate, 106), ' ', '-') AS AcceptanceDate
                            ,REPLACE(Convert(VARCHAR(11), IR.GRNDate, 106), ' ', '-') AS GRNDate
						--	,GRNType=CASE WHEN IR.GRNType='GRN' then 'GRN Without PO' ELSE 'GRN With PO' END
							,GRNType=CASE WHEN IR.GRNType='GRNBYJW' then 'GRN By JW' ELSE 'GRN by JW' END
                            ,REPLACE(Convert(VARCHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
                            ,REPLACE(Convert(VARCHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
                            ,IR.InvoicingPartyPlantId

                            ,INVPARTYPL.UserName InvoicingPartyName
                            ,INVPARTYPL.AddressMasterId InvoicePartyAddressMasterId
                            ,INVPARTYPL.GSTIN InvoicingPartyGSTIN
                            ,ISNULL(IR.InvoicingByAddress,'') InvoicingByAddress
                            ,IR.DeliveryByAddress
                            ,DPARTYPL.UserName DeliveryParty
                            ,IR.DeliveryPartyPlantId
                            ,IOM.MaterialMasterId
                            ,IR.DocRefNo
                            ,REPLACE(Convert(VARCHAR(11), IR.DocDate, 106), ' ', '-') AS DocDate
                            ,IR.GateEntryNo,REPLACE(Convert(VARCHAR(11), IR.EntryDate, 106), ' ', '-') AS GateEntryDate
                            ,CheckedBy=CASE WHEN IR.CheckedByStatus='Checked' Then eI.EmployeeName else '' END
                            ,AuthorizedBy=CASE When IR.AuthorizedByStatus='Approved'then eI1.EmployeeName else '' END
                             ,AddedBy=CASE 
									When IR.CheckedByStatus='ForChecked' Then eI3.EmployeeName
									When IR.CheckedByStatus='Hold' Then eI3.EmployeeName
									When IR.CheckedByStatus='Reject' Then eI3.EmployeeName
									When IR.CheckedByStatus='Checked' Then eI3.EmployeeName
									When IR.CheckedByStatus IS NULL then IR.AddedBy 
									else ''
							END
                            ,IR.AddedDate
                            ,IR.UpdatedBy
                            ,IR.UpdatedDate
                            ,IR.IsApproved
                            ,IR.PartyType
                            ,EMPIN.EmployeeName
                              --,Party.UserName VendorName
                            --,Party.AddressMasterId VendorAddressMasterId
                            --,Party.TINNO VendorGSTIN
							,VendorName=case when IR.PartyId is not null then Party.UserName else Pty.UserName End
							,VendorAddressMasterId=case when IR.PartyId is not null then Party.AddressMasterId else Pty.AddressMasterId End
							,VendorGSTIN=case when IR.PartyId is not null then Party.TINNO else Pty.TINNO End
                            ,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
                            ,IR.IsNonCreditable
                            ,IR.CurrencyId
                            ,CRNC.Code AS CurrencyName
                            ,IR.ToCurrencyRate
                            ,BASECRNC.Code AS BaseCurrencyName
                            ,PayTerm.UserName PaymentTerm
                            ,'-' MaterialMaster
                            ,'-' MaterialGroupMasterId
                            ,'-' MaterialGroupMaster
                            ,IOM.ArticleId
                            ,'-' Article
                            ,'-' FirstCharId
                            ,'-' FirstChar
                            ,IOM.FirstCharacteristicsValueId
                            ,'' AS FirstCharacteristicsValue
                            ,IOM.SecondCharacteristicsValueId
                            ,'' AS SecondCharacteristicsValue
                            ,IOM.ThirdCharacteristicsValueId
                            ,'' AS ThirdCharacteristicsValue
                            ,'' SecondCharId
                            ,'' SecondChar
                            ,'' ThirdCharId
                            ,'' ThirdChar
                            ,ROUND(IRD.TransactionQty, 2) POTransactionQty
                            ,ROUND(IRD.MaterialTranRate, 2) TransactionRate
                           ,TrnAmount=(IRD.GrossAmount-IRD.DiscountAmount)
                            ,IRD.TotalMaterialTranAmount BaseAmount
                            ,IRD.TotalTaxAmount AS BaseTaxAmount
                            ,TaxAmount = (
                            SELECT SUM(TaxAmount)
                            FROM dbo.JWTransformationPOTax --[TRN].[PurchaseOrderTax]
                            WHERE JWTransformationPODetailId = IRD.JWTransformationPODetailId
                            )
                            ,ServiceTaxAmount = (
                            SELECT SUM(TotalTaxAmount)
                            FROM dbo.JWTransformationPOService  --[TRN].[POService]
                            WHERE JWTransformationPOId = IRD.JWTransformationPOId
                            )
                            ,IRD.ChargesTranAmount
                            ,IRD.CountryId

                            ,IRD.TransactionUoMId
                            ,TUoM.ShortName  AS TransactionUoM
                            ,IRD.Id InventoryReceiveDetailId--,MRD.MaterialDetail
							,POD.Description,IRD.Description AS GRDDescrition
                            ,PurOrCheckedStatus= CASE when IR.CheckedByStatus='ForChecked' Then 'To be checked'
                            when IR.CheckedByStatus='Hold' Then 'Hold'
                            when IR.CheckedByStatus='Reject' Then 'Reject'
                            when IR.CheckedByStatus='Checked' Then 'Checked'
                            else ''

                            END
                            ,PurOrApprovedStatus= CASE
                            when IR.AuthorizedByStatus='Reject' Then 'Reject For Approved'
                            when IR.AuthorizedByStatus='Hold' Then 'Hold For Approved'
                            when IR.AuthorizedByStatus='For Approval' Then 'To be Approval'
                            when IR.AuthorizedByStatus='Approved' Then 'Approved'
                            else ''
							END
						,GRNStatus= CASE when IR.CheckedByStatus='ForChecked' Then 'To be checked'
								when IR.CheckedByStatus='Hold' Then 'Hold'
								when IR.CheckedByStatus='Reject' Then 'Reject'
								when IR.CheckedByStatus='Checked' AND IR.AuthorizedByStatus='To be Approval' Then 'Checked' 
								when IR.CheckedByStatus='Checked'  AND IR.AuthorizedByStatus='Reject' Then 'Reject For Approved'
								when IR.CheckedByStatus='Checked' AND IR.AuthorizedByStatus='Hold' Then 'Hold For Approved'
								when IR.CheckedByStatus='Checked' and IR.AuthorizedByStatus='For Approval' Then 'To be Approval'
								when IR.CheckedByStatus='Checked' and IR.AuthorizedByStatus='Approved' Then 'Approved'
								when IR.CheckedByStatus Is null and IR.AuthorizedByStatus Is null Then 'Approved'
                            else ''
                            END
							,Null LotNo , Null QualityStatus , Null GrossAmount ,Null DiscountAmount
                            FROM TRN.InventoryReceive IR
                            LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
                            LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                            LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
                            LEFT JOIN SCS.Currency CRNC ON CRNC.Id = IR.CurrencyId
                            LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = IR.BaseCurrencyId
                            LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = IR.PaymentTermId
                            LEFT JOIN HKP.PartyPlant INVPARTYPL ON INVPARTYPL.Id = IR.InvoicingPartyPlantId
                            LEFT JOIN HKP.PartyPlant DPARTYPL ON DPARTYPL.Id = IR.DeliveryPartyPlantId
                            LEFT JOIN trn.inventoryReceiveDetail IRD ON IR.Id = IRD.InventoryReceiveId
                            LEFT JOIN HKP.Party Party ON Party.Id = IR.PartyId
                            --     LEFT JOIN trn.PurchaseOrderDetail POD ON POD.Id = IRD.PODetailsId
							LEFT JOIN dbo.JWTransformationPODetail POD ON POD.Id = IRD.JWTransformationPODetailId
	                    --    LEFT JOIN TRN.PurchaseOrder PO ON PO.Id = IRD.POId
							 LEFT JOIN dbo.JWTransformationPO PO ON PO.Id = IRD.JWTransformationPOId
                            left join HKP.Party Pty on Pty.Id=PO.PartyId
                            LEFT JOIN (select Distinct PDAA.Id,AcceptanceDate,AcceptanceNo,ACMAP.GRNId from TRN.GRNAcceptanceMap ACMAP 
									left Join trn.PurchaseDocAcceptance  PDAA ON PDAA.Id=ACMAP.PurchaseDocumentAcceptanceId
									)PDA ON PDA.GRNId=IR.Id
							LEFT JOIN [dbo].[Contract] CNO ON CNO.Id = PO.ContractId
							LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id = PO.PurchaseLCId
	                        --LEFT JOIN [HKP].[Bank] B ON B.Id = PLC.BenificiaryBankId
                        --    Left Join TRN.MaterialRequsitionDetails MRD ON MRD.Id=POD.RequisitionDetailId
                            JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
                            LEFT JOIN trn.InventoryMaterial AS IOM ON IRD.InventoryMaterialId = IOM.Id
							LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = IOM.MaterialMasterId
							LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
                            --   LEFT JOIN EmployeeInformation AS EMPIN ON EMPIN.SystemId= IR.EmployeeId
							LEFT JOIN EmployeeInformation AS EMPIN ON EMPIN.SystemId= IR.ByWhomEmployeeId
                            LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
                            left join [SEC].[User] U on U.UserId=IR.AddedBy
                            LEFT JOIN dbo.EmployeeInformation eI3 ON eI3.SystemId=U.EmployeeId
							LEFT JOIN [TRN].[GateEntry] GTE  ON GTE.ID= IR.GateEntryNo
                            WHERE IR.Id ='"+ OrderMasterID + @"' and IOM.MaterialMasterId is NULL";


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

        public DataTable ValAddedloadGRNServiceMaster(string OrderMasterID)
        {
            string strSQL;

            try
            {
                strSQL = @"SELECT IOS.Id ServiceId, SM.UserName  Service ,IOS.Amount,IOS.TotalTaxAmount,IOS.AddedBy,IOS.AddedDate,IOS.UpdatedBy,IOS.UpdatedDate 
                               FROM TRN.InventoryReceive   IR
                            INNER join trn.inventoryservice IOS ON IOS.InventoryReceiveId = IR.Id
                            INNER JOIN HKP.ServiceMaster SM ON IOS.ServiceMasterId = SM.Id 
                            where IR.Id = '" + OrderMasterID + "'";

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

        public double ValAddedmakeOrderDetailsTable(WordDocument document, DataTable dsOrderMaster, string grnId)
        {
            string replaceString = "{materialItems}";

            DataTable dsOrderItems, dsTax;

            dsOrderItems = ValAddedloadOrderMasterItems(grnId);
            dsTax = ValAddedloadOrderMasterTax(grnId);

            int LasColumnIndex = 15;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));
            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    LasColumnIndex++;
                }
            }
            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            wTable.TableFormat.IsAutoResized = true;

            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();
            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SL");
            range.ApplyCharacterFormat(FontBold);
            int colRo = COL; COL++;
            wTable.Rows[ROW].Cells[colRo].Width = 30;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("RowId");
            range.ApplyCharacterFormat(FontBold);
            int colRowId = COL; COL++;
            //wTable.Rows[ROW].Cells[colRowId].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Materials");
            range.ApplyCharacterFormat(FontBold);
            int colMaterialGroup = COL; COL++;
            wTable.Rows[ROW].Cells[colMaterialGroup].Width = 80;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article ");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            wTable.Rows[ROW].Cells[colArticle].Width = 80;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU1");
            range.ApplyCharacterFormat(FontBold);
            int colChar1 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar1].Width = 36;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU2");
            range.ApplyCharacterFormat(FontBold);
            int colChar2 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar2].Width = 36;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU3");
            range.ApplyCharacterFormat(FontBold);
            int colChar3 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar3].Width = 36;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("HSN No");
            range.ApplyCharacterFormat(FontBold);
            int colHSNCode = COL; COL++;
            wTable.Rows[ROW].Cells[colHSNCode].Width = 35;


            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("HSN NO");
            //range.ApplyCharacterFormat(FontBold);
            //int colHSNCode = COL; COL++;
            //wTable.Rows[ROW].Cells[colChar3].Width = 45;


            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Po Material Detail");
            //range.ApplyCharacterFormat(FontBold);
            //int colDescription = COL; COL++;
            //wTable.Rows[ROW].Cells[colDescription].Width = 40;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("GRN Material Detail");
            //range.ApplyCharacterFormat(FontBold);
            //int colGRNMaterialDetail = COL; COL++;
            //wTable.Rows[ROW].Cells[colGRNMaterialDetail].Width = 40;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Lot No");
            range.ApplyCharacterFormat(FontBold);
            int colLotNo = COL; COL++;



            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate (" + dsOrderMaster.Rows[0]["CurrencyName"].ToString() + ")");
            range.ApplyCharacterFormat(FontBold);
            int colRate = COL; COL++;
            wTable.Rows[ROW].Cells[colRate].Width = 55;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UOM");
            range.ApplyCharacterFormat(FontBold);
            int colUoM = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Quality Status");
            range.ApplyCharacterFormat(FontBold);
            int colQualityStatus = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("GrossAmount");
            range.ApplyCharacterFormat(FontBold);
            int colGrossAmount = COL; COL++;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("DiscountAmount");
            range.ApplyCharacterFormat(FontBold);
            int colDiscountAmount = COL;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
                wTable.Rows[ROW].Cells[colTotalTaxableAmount].Width = 65;
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        //two columns required for tax
                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                        range.ApplyCharacterFormat(FontBold);
                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
                range.ApplyCharacterFormat(FontBold);
            }


            if (dv.Count > 0)
            {

                wTable.Rows.Add(TemplateRow);
                ROW++;
                WTableRow TROW = wTable.LastRow;
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
                        range.ApplyCharacterFormat(FontBold);
                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }
            #endregion column headers
            //if (dv.Count > 0)
            //{
            //    wTable.Rows.Add(TemplateRow);

            //    WTableRow TROW = wTable.LastRow;
            //    for (int CE = 0; CE < TROW.Cells.Count; CE++)
            //    {
            //        foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
            //        {
            //            item.Text = "";
            //        }
            //        TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
            //    }
            //    for (int i = 0; i < dv.Count; i++)
            //    {

            //        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
            //        range.ApplyCharacterFormat(FontBold);
            //        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
            //        range.ApplyCharacterFormat(FontBold);
            //    }
            //    ROW++;
            //}
            // #endregion column headers

            double totalValue = 0;
            int sl = 0;
            ROW++;

            //wTable.AddRow();
            int startRow = 1;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                sl++;
                ROW++;
                wTable.AddRow();
                //wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                TROW.Cells[colRo].AddParagraph().AppendText(sl.ToString());
                TROW.Cells[colRowId].AddParagraph().AppendText(dsOrderMaster.Rows[i]["InventoryReceiveDetailId"].ToString());
                TROW.Cells[colMaterialGroup].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialMaster"].ToString());
                TROW.Cells[colArticle].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Article"].ToString());
                TROW.Cells[colChar1].AddParagraph().AppendText(dsOrderMaster.Rows[i]["FirstCharacteristicsValue"].ToString());
                TROW.Cells[colChar2].AddParagraph().AppendText(dsOrderMaster.Rows[i]["SecondCharacteristicsValue"].ToString());
                TROW.Cells[colChar3].AddParagraph().AppendText(dsOrderMaster.Rows[i]["ThirdCharacteristicsValue"].ToString());
                TROW.Cells[colHSNCode].AddParagraph().AppendText(dsOrderMaster.Rows[i]["HSNCode"].ToString());
                //            TROW.Cells[colDescription].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Description"].ToString());
                //TROW.Cells[colGRNMaterialDetail].AddParagraph().AppendText(dsOrderMaster.Rows[i]["GRDDescrition"].ToString());
                TROW.Cells[colLotNo].AddParagraph().AppendText(dsOrderMaster.Rows[i]["LotNo"].ToString());
                TROW.Cells[colQty].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["POTransactionQty"].ToString()).ToString("F2"));
                TROW.Cells[colRate].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["TransactionRate"].ToString()).ToString("F2"));
                TROW.Cells[colUoM].AddParagraph().AppendText(dsOrderMaster.Rows[i]["TransactionUoM"].ToString().ToString());
                TROW.Cells[colQualityStatus].AddParagraph().AppendText(dsOrderMaster.Rows[i]["QualityStatus"].ToString().ToString());
                TROW.Cells[colGrossAmount].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["GrossAmount"].ToString()).ToString("F2"));
                TROW.Cells[colDiscountAmount].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["DiscountAmount"].ToString()).ToString("F2"));
                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString()).ToString("#,##0.00"));
                totalValue += clsStaticInfo.dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString());
                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(totalValue.ToString("F2"));
                if (dv.Count > 0)
                {
                    //dsTax.Tables[0].DefaultView.RowFilter = "MasterOrderItemId='" + dsOrderItems.Tables[0].Rows[i]["MasterOrderItemId"].ToString() + "'";
                    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
                    //double totalTax = 0;
                    for (int T = 0; T < dv.Count; T++)
                    {
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND InventoryReceiveDetailId ='" + dsOrderMaster.Rows[i]["InventoryReceiveDetailId"].ToString() + "'";
                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("#,##0.00"));
                        }
                    }
                }
            }

            ROW++;
            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);



            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (C == colHSNCode || C == colRate || C == colQualityStatus || C == colLotNo || C == colMaterialGroup || C == colArticle || C == colChar1 || C == colChar2 || C == colChar3 || /*C == colMaterialDetail ||*/ /*C == colDescription || C == colGRNMaterialDetail ||*/ C == colRowId || C == colRate || C == colUoM || dicTaxes.ContainsValue(C))
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStaticInfo.dbl(item.Text);
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

            double total = clsStaticInfo.dbl(dsOrderMaster.Compute("SUM(TrnAmount)", "").ToString())
                //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                + clsStaticInfo.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());

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
                // TROW.Cells[0].Width = 120;
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


            IWParagraphStyle myStyleRightAlign = document.AddParagraphStyle("MyStyleRightAlign");
            //Sets the formatting of the style
            myStyleRightAlign.CharacterFormat.FontSize = 8f;
            //myStyleRightAlign.CharacterFormat.TextColor = Color.Black;
            myStyleRightAlign.CharacterFormat.TextColor = System.Drawing.Color.Black;
            myStyleRightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;



            for (int R = 1; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];



                foreach (WParagraph item in TROW.Cells[colQty].Paragraphs)
                {
                    item.ApplyStyle("MyStyleRightAlign");
                }


                foreach (WParagraph item in TROW.Cells[colRate].Paragraphs)
                {
                    item.ApplyStyle("MyStyleRightAlign");
                }


                foreach (WParagraph item in TROW.Cells[colTotalTaxableAmount].Paragraphs)
                {
                    item.ApplyStyle("MyStyleRightAlign");
                }


            }

            #endregion paragrpath formats
            #region merging section

            //tax codes merging (horizontal)
            ROW = 0;
            for (int i = 0; i < dv.Count; i++)
                wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            WTableRow TROWe = wTable.LastRow;
            //for (int i = 0; i <= colTotalTaxableAmount; i++)
            //{
            //    TROWe.Cells[i].Width = wTable.Rows[0].Cells[i].Width;
            //    wTable.ApplyVerticalMerge(i, ROW - 1, ROW);
            //}
            //wTable.ApplyVerticalMerge(i, ROW - 1, ROW);




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

        public DataTable ValAddedloadOrderMasterItems(string OrderMasterID)
        {
            string strSQL;

            try
            {
                strSQL = @"select so.MasterOrderItemId,so.Id AS SOID,CONCAT( mm.[Description],' ',a.StandardName) AS MaterialDesc,
                                so.Qty,uom.UserName AS UOM,SO.Rate,so.Qty*so.Rate AS Amount,isnull(SO.Discount,0) AS Discount
                                  from [TRN].[MasterOrderItem] T
                                INNER JOIN [TRN].[MasterOrder] O ON o.Id=t.MasterOrderId
                                INNER JOIN [TRN].[SalesOrder]  SO ON so.MasterOrderItemId=t.Id
                                LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=t.MaterialMasterId
                                LEFT OUTER JOIN mst.MaterialGroupMaster AS mgm ON mgm.Id=mm.MaterialGroupMasterId
                                LEFT OUTER JOIN [MST].[MaterialMasterArticle] A ON a.Id=t.ArticleId
                                LEFT OUTER JOIN [SCS].[UnitOfMeasurement] UOM ON uom.Id=o.TotalQtyUOMId
                                where MasterOrderId='" + OrderMasterID + "'";

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
        public DataTable ValAddedloadOrderMasterTax(string OrderMasterID)
        {
            string strSQL;

            try
            {
                strSQL = @"select InventoryServiceId,PO.Id PurchaseOrderId,InventoryReceiveDetailId,tg.Code AS TaxCode,IRT.Percentage, IRT.TaxAmount from TRN.InventoryReceive PO
                               INNER JOIN trn.inventoryReceiveDetail IRD ON IRD.InventoryReceiveId = PO.Id
                               Inner join trn.InventoryReceiveTax IRT ON IRT.InventoryReceiveId = PO.Id and IRT.InventoryReceiveDetailId = IRD.Id
                               LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=IRT.TaxCategoryId
                                 WHERE PO.Id='" + OrderMasterID + @"' 
								 and InventoryReceiveDetailId  is not null and  InventoryServiceId is null 
								 ORDER BY tg.[Sequence] ";

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

        public DataTable ValAddedloadInventoryReceiveAdditionalTax(string grnId)
        {
            string strSQL;

            try
            {
                strSQL = @"Select TxC.UserName Taxname  ,IRAT.ID ,IRAT.TaxCodeId TaxCode,IRAT.TaxAmount,IRAT.Percentage   from [TRN].[InventoryReceiveAdditionalTax] IRAT
						LEFT JOIN TRN.InventoryReceive IR ON IR.ID= IRAT.InventoryReceiveId
						LEFT JOIN [MST].[TaxCode] TxC ON TxC.Id= IRAT.TaxCodeId
                        where IR.Id = '" + grnId + "'";

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

        public double ValAddedmakeInventoryReceiveAdditionalTaxTable(WordDocument document, DataTable dsOrderMaster, string grnId)
        {
            string replaceString = "{InventoryReceiveAdditionalTax}";

            ReportUtility ru = new ReportUtility();

            DataTable dsTax;
            //clsDataContext data = new clsDataContext();

            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign1");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 8f;
            //rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.CharacterFormat.TextColor = System.Drawing.Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;


            dsTax = ValAddedloadInventoryReceiveAdditionalTax(grnId);

            int LasColumnIndex = 1;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));

            //LasColumnIndex++;
            //dicTaxes.Add("totaltax", LasColumnIndex);
            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    //LasColumnIndex++;
                }
            }

            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();


            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;
            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxname");
            range.ApplyCharacterFormat(FontBold);
            int colTaxname = COL; COL++;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Percentage");
            range.ApplyCharacterFormat(FontBold);
            int colPercentage = COL;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Tax Amount");
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                //for (int i = 0; i < dv.Count; i++)
                //{
                //	//two columns required for tax
                //	COL++;
                //	range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                //	range.ApplyCharacterFormat(FontBold);
                //	COL++;
                //	range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                //}
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
            }

            wTable.Rows.Add(TemplateRow);
            ROW++;

            //if (dv.Count > 0)
            //{
            //	for (int i = 0; i < dv.Count; i++)
            //	{

            //		range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
            //		range.ApplyCharacterFormat(FontBold);
            //		range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
            //		range.ApplyCharacterFormat(FontBold);

            //	}
            //}

            #endregion column headers
            double totalValue = 0;
            int startRow = ROW + 1;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                //ROW++;
                //wTable.AddRow();
                WTableRow TROW = wTable.LastRow;


                IParagraphItem p = TROW.Cells[colTaxname].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Taxname"].ToString());
                TROW.Cells[colPercentage].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["Percentage"].ToString()).ToString("#,##0.0000"));

                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["TaxAmount"].ToString()).ToString("#,##0.00"));
            }

            #region Sub Total


            double total = clsStaticInfo.dbl(dsOrderMaster.Compute("SUM(TaxAmount)", "").ToString());

            #endregion Total


            //ROW++;

            #region Total Payable
            #endregion Total Payable

            //ROW++;

            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle3 = document.AddParagraphStyle("MyStyle3");
            //Sets the formatting of the style
            myStyle3.CharacterFormat.FontSize = 8f;
            //myStyle3.CharacterFormat.TextColor = Color.Black;
            myStyle3.CharacterFormat.TextColor = System.Drawing.Color.Black;
            myStyle3.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 35;
                if (dv.Count < 3)
                    TROW.Cells[0].Width = +((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle3");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            #endregion merging section

            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            int k = document.Replace(replaceString, textBodyPart, false, false);
            return total;
        }

        public void ValAddedloadGRNShortageTable(WordDocument document, string grnId)
        {
            string replaceString = "{shortage}";

            DataTable dtlOrderItems;

            dtlOrderItems = ValAddedloadGRNShortageMaster(grnId);
            if (dtlOrderItems.Rows.Count > 0)
            {
                document.Replace("{ShortageDetails}", "Shortage Details", true, true);

                //dsTax = loadOrderMasterTax(grnId);

                int LasColumnIndex = 6;
                Dictionary<string, int> dicTaxes = new Dictionary<string, int>();

                WTable wTable = new WTable(document);
                wTable.TableFormat.Borders.LineWidth = 1;
                wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
                int ROW = 0; int COL = 0;
                wTable.ResetCells(1, LasColumnIndex + 1);

                WTableRow TemplateRow = wTable.Rows[0].Clone();

                #region column headers
                document.EnsureMinimal();
                //wTable.Title = "Material Details";
                //wTable.Description = "This table shows the price details of PI";
                //wTable.IndentFromLeft = 10;


                //string UOM = dsOrderMaster.Tables[0].Rows[0]["UOM"].ToString();
                //string Currency = dsOrderMaster.Tables[0].Rows[0]["Currency"].ToString();
                WCharacterFormat FontBold = new WCharacterFormat(document);
                FontBold.Bold = true;
                // = true;




                IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("RowId");
                range.ApplyCharacterFormat(FontBold);
                int colRowIdShort = COL; COL++;
                wTable.Rows[ROW].Cells[COL].Width = 50;

                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Material");
                wTable.Rows[ROW].Cells[COL].Width = 100;
                range.ApplyCharacterFormat(FontBold);
                int colMaterial = COL; COL++;

                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article");
                wTable.Rows[ROW].Cells[COL].Width = 50;
                range.ApplyCharacterFormat(FontBold);
                int colArticle = COL; COL++;

                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("InvoiceRate");
                wTable.Rows[ROW].Cells[COL].Width = 50;
                range.ApplyCharacterFormat(FontBold);
                int colMaterialTranRate = COL; COL++;


                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
                wTable.Rows[ROW].Cells[COL].Width = 50;
                range.ApplyCharacterFormat(FontBold);
                int colShortageQty = COL; COL++;



                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate(%)");
                wTable.Rows[ROW].Cells[COL].Width = 50;
                range.ApplyCharacterFormat(FontBold);
                int colShortageRatePercent = COL;
                COL++;


                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Value (" + dtlOrderItems.Rows[0]["Code"].ToString() + ")");
                wTable.Rows[ROW].Cells[COL].Width = 60;
                range.ApplyCharacterFormat(FontBold);
                int colShortageValue = COL;



                //int colTotalTaxableAmount = COL;
                //if (dv.Count > 0)
                //{
                //    COL++;
                //    colTotalTaxableAmount = COL;
                //    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
                //    range.ApplyCharacterFormat(FontBold);
                //    //COL++;
                //    //for (int i = 0; i < dv.Count; i++)
                //    //{
                //    //    //two columns required for tax
                //    //    COL++;
                //    //    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                //    //    range.ApplyCharacterFormat(FontBold);

                //    //    COL++;
                //    //    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                //    //}
                //}
                //else
                //{
                //    COL++;
                //    colTotalTaxableAmount = COL;
                //    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Value");
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
                double totalValue = 0;
                int startRow = ROW + 1;
                for (int i = 0; i < dtlOrderItems.Rows.Count; i++)
                {
                    //if (Convert.ToDouble(dtlOrderItems.Rows[i]["ShortageQty"]) > 0)
                    //{



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
                    TROW.Cells[colRowIdShort].AddParagraph().AppendText(dtlOrderItems.Rows[i]["InventoryReceiveDetailsId"].ToString());
                    TROW.Cells[colMaterial].AddParagraph().AppendText(dtlOrderItems.Rows[i]["MaterialMaster"].ToString());
                    TROW.Cells[colArticle].AddParagraph().AppendText(dtlOrderItems.Rows[i]["Article"].ToString());

                    TROW.Cells[colMaterialTranRate].AddParagraph().AppendText(Convert.ToDouble(dtlOrderItems.Rows[i]["MaterialTranRate"]).ToString("F2"));
                    //TROW.Cells[colMaterialTranRate].Width = 60;
                    TROW.Cells[colShortageQty].AddParagraph().AppendText(Convert.ToDouble(dtlOrderItems.Rows[i]["ShortageQty"]).ToString("F2"));
                    //TROW.Cells[colShortageQty].Width = 60;
                    TROW.Cells[colShortageRatePercent].AddParagraph().AppendText(Convert.ToDouble(dtlOrderItems.Rows[i]["ShortageRatePercent"]).ToString("F2"));
                    //TROW.Cells[colShortageRatePercent].Width = 60;
                    TROW.Cells[colShortageValue].AddParagraph().AppendText(Convert.ToDouble(dtlOrderItems.Rows[i]["ShortageValue"]).ToString("F2"));
                    //TROW.Cells[colShortageValue].Width = 60;

                    //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dtOrderItems.Rows[i]["TrnAmount"].ToString()).ToString("F2"));

                    //totalValue += clsStdLib.dbl(dtOrderItems.Rows[i]["TrnAmount"].ToString());

                    //if (dv.Count > 0)
                    //{
                    //    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
                    //    //double totalTax = 0;

                    //    for (int T = 0; T < dv.Count; T++)
                    //    {
                    //        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND InventoryReceiveDetailId ='" + dsOrderMaster.Rows[i]["InventoryReceiveDetailId"].ToString() + "'";
                    //        if (dvtax.Count > 0)
                    //        {
                    //            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));

                    //            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("F2"));

                    //        }
                    //    }
                    //}
                    //}
                }

                ROW++;
                #region Total
                int TotalRow = ROW;
                wTable.AddRow();
                WTableRow _TROW = wTable.LastRow;
                _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);


                for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
                {
                    if (C == colMaterialTranRate || C == colShortageRatePercent || C == colRowIdShort || dicTaxes.ContainsValue(C))
                        continue;

                    double value = 0;
                    for (int i = startRow; i < TotalRow; i++)
                    {

                        foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                        {
                            value += clsStaticInfo.dbl(item.Text);
                        }
                    }
                    _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);
                }
                #endregion Total


                ROW++;
                //#region Sub Total
                //int SubTotalRow = ROW;
                //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
                //wTable.AddRow();
                //_TROW = wTable.LastRow;

                //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

                //double total = clsStdLib.dbl(dtlOrderItems.Compute("SUM(ShortageQty)", "").ToString())

                //+ clsStdLib.dbl(dtlOrderItems.Compute("SUM(ShortageValue)", "").ToString());

                //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2"));

                //#endregion Total


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
                IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyleS");
                //Sets the formatting of the style
                myStyle.CharacterFormat.FontSize = 8f;
                //myStyle.CharacterFormat.TextColor = Color.Black;
                myStyle.CharacterFormat.TextColor = System.Drawing.Color.Black;
                myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

                for (int R = 0; R < wTable.Rows.Count; R++)
                {
                    WTableRow TROW = wTable.Rows[R];
                    TROW.Cells[0].Width = 50;

                    for (int CE = 0; CE < TROW.Cells.Count; CE++)
                    {
                        foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                        {
                            item.ApplyStyle("MyStyleS");
                        }
                    }

                }

                #endregion paragrpath formats

                //#region paragrpath formats
                //Adds a new paragraph style named "MyStyle"
                //IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyles");
                ////Sets the formatting of the style
                //myStyle.CharacterFormat.FontSize = 8f;
                //myStyle.CharacterFormat.TextColor = Color.Black;
                //myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

                //for (int R = 0; R < wTable.Rows.Count; R++)
                //{
                //    WTableRow TROW = wTable.Rows[R];
                //    TROW.Cells[0].Width = 35;
                //    //if (dv.Count < 3)
                //    //    TROW.Cells[0].Width = 70 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                //    for (int CE = 0; CE < TROW.Cells.Count; CE++)
                //    {
                //        foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                //        {
                //            item.ApplyStyle("MyStyles");
                //        }
                //    }
                //}

                //#endregion paragrpath formats

                #region
                //tax codes merging (horizontal)
                ROW = 0;
                //for (int i = 0; i < dv.Count; i++)
                //    wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

                //primary cells merging (veritcal)
                //ROW++;
                //for (int i = 0; i <= colTotalTaxableAmount; i++)
                //    wTable.ApplyVerticalMerge(i, ROW - 1, ROW);


                //WParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
                //style.CharacterFormat.Bold = true;
                //style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;I
                //Adds new paragraph to the section


                //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
                //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
                //        PARA.ApplyStyle("SubTotalStyle");

                //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
                #endregion merging section

                TextBodyPart textBodyPart = new TextBodyPart(document);
                textBodyPart.BodyItems.Add(wTable);
                document.Replace(replaceString, textBodyPart, true, true);

                //return total;
            }
        }

        public DataTable ValAddedloadGRNShortageMaster(string OrderMasterID)
        {
            string strSQL;

            try
            {
                strSQL = @"select IRD.Id AS InventoryReceiveDetailsId,IRD.MaterialTranRate
	                                ,IRD.ShortageQty
	                                ,IRD.ShortageRatePercent
	                                ,IRD.ShortageValue 
									,C.Code
									,MM.UserName MaterialMaster
									,MMA.StandardName Article
                                FROM trn.InventoryReceiveDetail IRD
								 LEFT JOIN trn.InventoryMaterial AS IOM ON IRD.InventoryMaterialId = IOM.Id
								 LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = IOM.MaterialMasterId
								 LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IOM.ArticleId
								LEFT JOIn trn.InventoryReceive IR ON IR.Id=InventoryReceiveId
								LEFT JOIN scs.Currency C on C.Id=IR.CurrencyId
                                where IRD.InventoryReceiveId='" + OrderMasterID + "'and isnull(IRD.ShortageQty,0)>0";


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


        public void ValAddedloadGRNRejectionTable(WordDocument document, string grnId)
        {
            string replaceString = "{rejection}";



            DataTable dtOrderItems, dsTax;
            dtOrderItems = ValAddedloadGRNRejectionMaster(grnId);
            if (dtOrderItems.Rows.Count > 0)
            {
                document.Replace("{RejectionDetails}", "Rejection Details", true, true);


                //  dsTax = loadOrderMasterTax(grnId);

                int LasColumnIndex = 6;
                Dictionary<string, int> dicTaxes = new Dictionary<string, int>();


                WTable wTable = new WTable(document);
                wTable.TableFormat.Borders.LineWidth = 1;
                wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
                int ROW = 0; int COL = 0;
                wTable.ResetCells(1, LasColumnIndex + 1);

                WTableRow TemplateRow = wTable.Rows[0].Clone();

                #region column headers
                document.EnsureMinimal();

                WCharacterFormat FontBold = new WCharacterFormat(document);
                FontBold.Bold = true;
                IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("RowId");
                range.ApplyCharacterFormat(FontBold);
                int colRowIdRej = COL; COL++;
                wTable.Rows[ROW].Cells[COL].Width = 50;

                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Material");
                wTable.Rows[ROW].Cells[COL].Width = 50;
                range.ApplyCharacterFormat(FontBold);
                int colMaterial = COL; COL++;

                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article");
                wTable.Rows[ROW].Cells[COL].Width = 50;
                range.ApplyCharacterFormat(FontBold);
                int colArticle = COL; COL++;

                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("InvoiceRate");
                wTable.Rows[ROW].Cells[COL].Width = 50;
                range.ApplyCharacterFormat(FontBold);
                int colMaterialTranRate = COL; COL++;


                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty ");
                wTable.Rows[ROW].Cells[COL].Width = 50;
                range.ApplyCharacterFormat(FontBold);
                int colRejectionQty = COL; COL++;



                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate(%) ");
                wTable.Rows[ROW].Cells[COL].Width = 50;
                range.ApplyCharacterFormat(FontBold);
                int colRejectRatePercent = COL; COL++;


                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Value (" + dtOrderItems.Rows[0]["Code"].ToString() + ")");
                wTable.Rows[ROW].Cells[COL].Width = 60;
                range.ApplyCharacterFormat(FontBold);
                int colRejectValue = COL;

                #endregion column headers

                double totalValue = 0;
                int startRow = ROW + 1;
                for (int i = 0; i < dtOrderItems.Rows.Count; i++)
                {
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

                    TROW.Cells[colRowIdRej].AddParagraph().AppendText(dtOrderItems.Rows[i]["InventoryReceiveDetailsId"].ToString());
                    TROW.Cells[colMaterial].AddParagraph().AppendText(dtOrderItems.Rows[i]["MaterialMaster"].ToString());
                    TROW.Cells[colArticle].AddParagraph().AppendText(dtOrderItems.Rows[i]["Article"].ToString());
                    TROW.Cells[colMaterialTranRate].AddParagraph().AppendText(Convert.ToDouble(dtOrderItems.Rows[i]["MaterialTranRate"]).ToString());
                    //TROW.Cells[colMaterialTranRate].Width = 60;
                    TROW.Cells[colRejectionQty].AddParagraph().AppendText(Convert.ToDouble(dtOrderItems.Rows[i]["RejectionQty"]).ToString());
                    //TROW.Cells[colRejectionQty].Width = 60;
                    TROW.Cells[colRejectRatePercent].AddParagraph().AppendText(Convert.ToDouble(dtOrderItems.Rows[i]["RejectRatePercent"]).ToString());
                    //TROW.Cells[colRejectRatePercent].Width = 60;
                    TROW.Cells[colRejectValue].AddParagraph().AppendText(Convert.ToDouble(dtOrderItems.Rows[i]["RejectValue"]).ToString());
                    //TROW.Cells[colRejectValue].Width = 60;


                }

                ROW++;
                #region Total
                int TotalRow = ROW;
                wTable.AddRow();
                WTableRow _TROW = wTable.LastRow;
                _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);


                for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
                {
                    if (C == colMaterialTranRate || C == colRejectRatePercent || dicTaxes.ContainsValue(C))
                        continue;

                    double value = 0;
                    for (int i = startRow; i < TotalRow; i++)
                    {

                        foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                        {
                            value += clsStaticInfo.dbl(item.Text);
                        }
                    }
                    _TROW.Cells[C].AddParagraph().AppendText(value.ToString("F2")).ApplyCharacterFormat(FontBold);
                }
                #endregion Total


                ROW++;
                //#region Sub Total
                //int SubTotalRow = ROW;
                //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
                //wTable.AddRow();
                //_TROW = wTable.LastRow;

                //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

                //double total = clsStdLib.dbl(dtOrderItems.Compute("SUM(RejectValue)", "").ToString());
                ////- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                ////+ clsStdLib.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());

                //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2"));

                //#endregion Total




                ROW++;

                #region paragrpath formats
                IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyleR");
                //Sets the formatting of the style
                myStyle.CharacterFormat.FontSize = 8f;
                //myStyle.CharacterFormat.TextColor = Color.Black;
                myStyle.CharacterFormat.TextColor = System.Drawing.Color.Black;
                myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

                for (int R = 0; R < wTable.Rows.Count; R++)
                {
                    WTableRow TROW = wTable.Rows[R];
                    TROW.Cells[0].Width = 50;

                    for (int CE = 0; CE < TROW.Cells.Count; CE++)
                    {
                        foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                        {
                            item.ApplyStyle("MyStyleR");
                        }
                    }

                }

                #endregion paragrpath formats


                #region merging section


                //tax codes merging (horizontal)
                ROW = 0;

                ROW++;

                #endregion merging section

                TextBodyPart textBodyPart = new TextBodyPart(document);
                textBodyPart.BodyItems.Add(wTable);
                document.Replace(replaceString, textBodyPart, true, true);
            }

        }

        public DataTable ValAddedloadGRNRejectionMaster(string OrderMasterID)
        {
            string strSQL;

            try
            {
                strSQL = @"select IRD.Id AS InventoryReceiveDetailsId,IRD.MaterialTranRate
	                                ,IRD.RejectionQty
	                                ,IRD.RejectRatePercent
	                                ,IRD.RejectValue
									,C.Code
                                    ,MM.UserName MaterialMaster
									,MMA.StandardName Article
                                FROM trn.InventoryReceiveDetail IRD
                                LEFT JOIN trn.InventoryMaterial AS IOM ON IRD.InventoryMaterialId = IOM.Id
								 LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = IOM.MaterialMasterId
								 LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IOM.ArticleId
								LEFT JOIn trn.InventoryReceive IR ON IR.Id=InventoryReceiveId
								LEFT JOIN scs.Currency C on C.Id=IR.CurrencyId
                        where IRD.InventoryReceiveId='" + OrderMasterID + "'and isnull(IRD.ShortageQty,0)>0";


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

        public double ValAddedmakeOrderServiceTable(WordDocument document, DataTable dsOrderMaster, string grnId)
        {
            string replaceString = "{ServiceItems}";

            ReportUtility ru = new ReportUtility();

            DataTable dsTax;
            //clsDataContext data = new clsDataContext();

            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 8f;
            // rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.CharacterFormat.TextColor = System.Drawing.Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;


            dsTax = ValAddedloadGRNServiceMasterTex(grnId);

            int LasColumnIndex = 1;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));

            //LasColumnIndex++;
            //dicTaxes.Add("totaltax", LasColumnIndex);
            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    LasColumnIndex++;
                }
            }

            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();


            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;
            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Service Name");
            range.ApplyCharacterFormat(FontBold);
            //wTable.Rows[ROW].Cells[COL].Width = 100;
            int colServiceName = COL; //COL++;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    //two columns required for tax
                    COL++;
                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                    range.ApplyCharacterFormat(FontBold);
                    COL++;
                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                }
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
            }

            wTable.Rows.Add(TemplateRow);
            ROW++;

            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {

                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
                    range.ApplyCharacterFormat(FontBold);
                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                    range.ApplyCharacterFormat(FontBold);

                }
            }

            #endregion column headers
            double totalValue = 0;
            int startRow = ROW + 1;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
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
                IParagraphItem p = TROW.Cells[colServiceName].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Service"].ToString());

                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["Amount"].ToString()).ToString("#,##0.00"));

                totalValue += clsStaticInfo.dbl(dsOrderMaster.Rows[i]["Amount"].ToString());

                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(totalValue.ToString("F2"));

                if (dv.Count > 0)
                {
                    //dsTax.Tables[0].DefaultView.RowFilter = "MasterOrderItemId='" + dsOrderItems.Tables[0].Rows[i]["MasterOrderItemId"].ToString() + "'";
                    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
                    //double totalTax = 0;

                    for (int T = 0; T < dv.Count; T++)
                    {
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND InventoryServiceId='" + dsOrderMaster.Rows[i]["ServiceId"] + "'";
                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("F2"));
                        }
                    }
                }
            }

            ROW++;

            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            //wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;

            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);

            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (dicTaxes.ContainsValue(C))
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStaticInfo.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("F2"));
            }


            #endregion Total


            ROW++;


            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStaticInfo.dbl(dsOrderMaster.Compute("SUM(Amount)", "").ToString())
//- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
+ clsStaticInfo.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());



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
            IWParagraphStyle myStyle2 = document.AddParagraphStyle("MyStyle2");
            //Sets the formatting of the style
            myStyle2.CharacterFormat.FontSize = 8f;
            //myStyle2.CharacterFormat.TextColor = Color.Black;
            myStyle2.CharacterFormat.TextColor = System.Drawing.Color.Black;
            myStyle2.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 35;
                if (dv.Count < 3)
                    TROW.Cells[0].Width = +((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle2");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            for (int i = 0; i < dv.Count; i++)
                wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            for (int i = 0; i <= colTotalTaxableAmount; i++)
                wTable.ApplyVerticalMerge(i, ROW - 1, ROW);

            IWParagraphStyle style2 = document.AddParagraphStyle("SubTotalStyle2");
            style2.CharacterFormat.Bold = true;
            style2.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;


            //Adds new paragraph to the section


            //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
            //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
            //        PARA.ApplyStyle("SubTotalStyle2");

            //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
            #endregion merging section



            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);
            return total;
        }

        public DataTable ValAddedloadGRNServiceMasterTex(string OrderMasterID)
        {
            string strSQL;
            try
            {
                strSQL = @"select InventoryServiceId,IR.Id PurchaseOrderId,tg.Code AS TaxCode,IRT.Percentage, IRT.TaxAmount
                    from TRN.InventoryReceive IR
                              INNER JOIN trn.InventoryService ISER ON ISER.InventoryReceiveId = IR.Id
                              Inner join trn.InventoryReceiveTax IRT ON IRT.InventoryReceiveId = IR.Id and IRT.InventoryServiceId = ISER.Id
                               LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=IRT.TaxCategoryId
                                WHERE IR.Id='" + OrderMasterID + @"'
								and InventoryServiceId  is not null and   InventoryReceiveDetailId is null 
								 ORDER BY tg.[Sequence]";
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

        // Receipt Transformation Without Material

        private string GetTransformationPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "InventoryReceive", out sID);
            return sID;
        }

        public void SaveTransformationWOMaterial(Dictionary<string, object> data, string ContractId, string PartyId, IEnumerable<JobWorkTransformationReceiptWOMaterial> SelectedQtyDataWOMat, IEnumerable<JWTransformationReceiptWOMaterialByProduct> SelectedByProductQtyDataWOMat)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                //con.OpenDataSetThroughAdapter("select * from " + TableName + " where PositionCodeId='" + data["PositionCodeId"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //    throw new Exception("Same Position Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from trn.InventoryReceive where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = GetTransformationPK();

                    dr["MaterialStorageId"] = data["MaterialStorageId"];
                    dr["CurrencyId"] = data["CurrencyId"];
                    dr["PartyId"] = PartyId;
                    dr["DocRefNo"] = data["DocRefNo"];
                    dr["DocDate"] = data["DocDate"];
                    dr["GateEntryNo"] = data["GateEntryNo"];
                    dr["GRNDate"] = data["GRNDate"];
                    dr["ToCurrencyRate"] = data["ToCurrencyRate"];
                    dr["PartyType"] = "Vendor";
                    dr["GRNType"] = "GRNBYPO";

                    dr["NoteForAccounts"] = data["NoteForAccounts"];
                    dr["ByWhomEmployeeId"] = data["ByWhomId"];
                    dr["TransformationContractId"] = ContractId;
                    dr["IsNonCreditable"] = data["IsNonCreditable"];

                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["CompanyId"] = identity.CompanyId;
                    dr["PlantId"] = identity.PlantId;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();

                    dr["MaterialStorageId"] = data["MaterialStorageId"];
                    dr["CurrencyId"] = data["CurrencyId"];
                    dr["PartyId"] = PartyId;
                    dr["DocRefNo"] = data["DocRefNo"];
                    dr["DocDate"] = data["DocDate"];
                    dr["GateEntryNo"] = data["GateEntryNo"];
                    dr["GRNDate"] = data["GRNDate"];
                    dr["ToCurrencyRate"] = data["ToCurrencyRate"];
                    dr["PartyType"] = "Vendor";
                    dr["GRNType"] = "GRNBYPO";

                    dr["NoteForAccounts"] = data["NoteForAccounts"];
                    dr["ByWhomEmployeeId"] = data["ByWhomId"];
                    dr["TransformationContractId"] = ContractId;
                    dr["IsNonCreditable"] = data["IsNonCreditable"];

                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["CompanyId"] = identity.CompanyId;
                    dr["PlantId"] = identity.PlantId;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dr.EndEdit();
                }
                data["Id"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                string MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                SaveReceiptTransformationWOMaterial(SelectedQtyDataWOMat, MasterId);
                SaveReceiptByProductWOMaterial(SelectedByProductQtyDataWOMat, MasterId);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetTransformationChildPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "InventoryIssueDetail", out sID);
            return sID;
        }

        public void SaveReceiptTransformationWOMaterial(IEnumerable<JobWorkTransformationReceiptWOMaterial> SelectedQtyDataWOMat, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet ExistOrNot;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
          //      var JWItemId = "' '";
                var OtMatId = "' '";

                foreach (var empitem in SelectedQtyDataWOMat)
                {
           //         JWItemId += ",'" + empitem.JWInputItemId + "' ";
                    OtMatId += ",'" + empitem.OSTransformationPODetailId + "' ";

                }
                con.OpenDataSetThroughAdapter("select * from TRN.InventoryReceiveDetail where OSTransformationPODetailId IN ( " + OtMatId + ") and InventoryReceiveId='" + MasterId + "'  ", out ExistOrNot, false, "1");

                foreach (var item in SelectedQtyDataWOMat)
                {

                    ExistOrNot.Tables[0].DefaultView.RowFilter = "OSTransformationPODetailId='" + item.OSTransformationPODetailId + "' and InventoryReceiveId='" + MasterId + "' ";

                    if (ExistOrNot.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = ExistOrNot.Tables[0].NewRow();
                        //        dr["Id"] = "TC" + GetTransformationChildPK();
                        dr["Id"] = GetTransformationChildPK();

                        dr["InventoryReceiveId"] = MasterId;
                        dr["TransactionQty"] = item.TransactionQty;
                        dr["TransactionUoMId"] = item.TransactionUoMId;
                        dr["BaseQty"] = item.TransactionQty;
                        dr["QualityStatus"] = item.QualityStatus;
                        dr["OSTransformationPOId"] = item.OSTransformationPOId;
                        dr["OSTransformationPODetailId"] = item.OSTransformationPODetailId;
                        dr["MaterialFor"] = "JWOUTPUTMaterial";

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        //dr["UpdatedBy"] = identity.Name;
                        //dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        //dr["UpdatedFromIP"] = identity.IPAddress;

                        ExistOrNot.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        ExistOrNot.Tables[0].DefaultView.RowFilter = "OSTransformationPODetailId='" + item.OSTransformationPODetailId + "' and InventoryReceiveId='" + MasterId + "' ";

                        if (ExistOrNot.Tables[0].DefaultView.Count == 0)
                        {
                            DataRow dr = ExistOrNot.Tables[0].NewRow();
                            dr["Id"] = GetTransformationChildPK();

                            dr["InventoryReceiveId"] = MasterId;
                            dr["TransactionQty"] = item.TransactionQty;
                            dr["TransactionUoMId"] = item.TransactionUoMId;
                            dr["BaseQty"] = item.TransactionQty;
                            dr["QualityStatus"] = item.QualityStatus;
                            dr["OSTransformationPOId"] = item.OSTransformationPOId;
                            dr["OSTransformationPODetailId"] = item.OSTransformationPODetailId;
                            dr["MaterialFor"] = "JWOUTPUTMaterial";

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;

                            ExistOrNot.Tables[0].Rows.Add(dr);

                        }
                        else
                        {
                            //edit
                            DataRow dr = ExistOrNot.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["InventoryReceiveId"] = MasterId;
                            dr["TransactionQty"] = item.TransactionQty;
                            dr["TransactionUoMId"] = item.TransactionUoMId;
                            dr["BaseQty"] = item.TransactionQty;
                            dr["QualityStatus"] = item.QualityStatus;
                            dr["OSTransformationPOId"] = item.OSTransformationPOId;
                            dr["OSTransformationPODetailId"] = item.OSTransformationPODetailId;
                            dr["MaterialFor"] = "JWOUTPUTMaterial";

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;


                            dr.EndEdit();
                        }


                    }

                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(ExistOrNot);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetTransformationBYProdPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "InventoryIssueDetail", out sID);
            return sID;
        }

        public void SaveReceiptByProductWOMaterial(IEnumerable<JWTransformationReceiptWOMaterialByProduct> SelectedByProductQtyDataWOMat, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet ExistOrNot;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var MIId = "' '";
                var BPId = "' '";

                foreach (var empitem in SelectedByProductQtyDataWOMat)
                {
                    MIId += ",'" + empitem.OSTransformationPOInputMaterialId + "' ";
                    BPId += ",'" + empitem.OSTransformationPOByProductId + "' ";

                }
                con.OpenDataSetThroughAdapter("select * from TRN.InventoryReceiveDetail where OSTransformationPOInputMaterialId IN ( " + MIId + ") and OSTransformationPOByProductId IN ("+ BPId + ") and InventoryReceiveId='" + MasterId + "'  ", out ExistOrNot, false, "1");

                foreach (var item in SelectedByProductQtyDataWOMat)
                {

                    ExistOrNot.Tables[0].DefaultView.RowFilter = "OSTransformationPOInputMaterialId='" + item.OSTransformationPOInputMaterialId + "' and OSTransformationPOByProductId='" + item.OSTransformationPOByProductId + "' and InventoryReceiveId='" + MasterId + "' ";

                    if (ExistOrNot.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = ExistOrNot.Tables[0].NewRow();
                        //        dr["Id"] = "TC" + GetTransformationChildPK();
                        dr["Id"] = GetTransformationBYProdPK();

                        dr["InventoryReceiveId"] = MasterId;
                        dr["TransactionQty"] = item.TransactionQty;
                        dr["TransactionUoMId"] = item.TransactionUoMId;
                        dr["BaseQty"] = item.TransactionQty;
                        dr["QualityStatus"] = item.QualityStatus;
                        dr["OSTransformationPOInputMaterialId"] = item.OSTransformationPOInputMaterialId;
                        dr["OSTransformationPOByProductId"] = item.OSTransformationPOByProductId;
                        dr["MaterialFor"] = "JWBYPRODUCTMaterial";

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        //dr["UpdatedBy"] = identity.Name;
                        //dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        //dr["UpdatedFromIP"] = identity.IPAddress;

                        ExistOrNot.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        ExistOrNot.Tables[0].DefaultView.RowFilter = "OSTransformationPOInputMaterialId='" + item.OSTransformationPOInputMaterialId + "' and OSTransformationPOByProductId='" + item.OSTransformationPOByProductId + "' and InventoryReceiveId='" + MasterId + "' ";

                        if (ExistOrNot.Tables[0].DefaultView.Count == 0)
                        {
                            DataRow dr = ExistOrNot.Tables[0].NewRow();
                            dr["Id"] = GetTransformationBYProdPK();

                            dr["InventoryReceiveId"] = MasterId;
                            dr["TransactionQty"] = item.TransactionQty;
                            dr["TransactionUoMId"] = item.TransactionUoMId;
                            dr["BaseQty"] = item.TransactionQty;
                            dr["QualityStatus"] = item.QualityStatus;
                            dr["OSTransformationPOInputMaterialId"] = item.OSTransformationPOInputMaterialId;
                            dr["OSTransformationPOByProductId"] = item.OSTransformationPOByProductId;
                            dr["MaterialFor"] = "JWBYPRODUCTMaterial";

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;

                            ExistOrNot.Tables[0].Rows.Add(dr);

                        }
                        else
                        {
                            //edit
                            DataRow dr = ExistOrNot.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["InventoryReceiveId"] = MasterId;
                            dr["TransactionQty"] = item.TransactionQty;
                            dr["TransactionUoMId"] = item.TransactionUoMId;
                            dr["BaseQty"] = item.TransactionQty;
                            dr["QualityStatus"] = item.QualityStatus;
                            dr["OSTransformationPOInputMaterialId"] = item.OSTransformationPOInputMaterialId;
                            dr["OSTransformationPOByProductId"] = item.OSTransformationPOByProductId;
                            dr["MaterialFor"] = "JWBYPRODUCTMaterial";

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;


                            dr.EndEdit();
                        }


                    }

                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(ExistOrNot);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
//public class jobworkreceiptvalueaddedchild
//{

//    #region Scalar Properties
//    public string Id { get; set; }
//    public string ContractLineItemId { get; set; }
//    public string OrderChildId { get; set; }
//    public string ReceivedQty { get; set; }
//    public string Remarks { get; set; }

//    public string JWOutputItem { get; set; }
//    public string Article { get; set; }
//    public string TotalIssuedQty { get; set; }
//    public string TotalReceivedQty { get; set; }
//    public string ToReceive { get; set; }


//    #endregion Scalar Properties
//}
//public class ReceiptValueAddedGradeWise
//{

//    #region Scalar Properties
//    public string Id { get; set; }
//    public string GradeName { get; set; }
//    public string GradeWQty { get; set; }
//    public string GWRemarks { get; set; }

//    #endregion Scalar Properties
//}
//public class JobWorkReceiptTransformationChild
//{

//    #region Scalar Properties
//    public string Id { get; set; }
//    public string ReceivedQty { get; set; }
//    public string Remarks { get; set; }

//    #endregion Scalar Properties
//}
//public class ReceiptTransformationGradeWise
//{

//    #region Scalar Properties
//    public string Id { get; set; }
//    public string GradeName { get; set; }
//    public string GradeWQty { get; set; }
//    public string GWRemarks { get; set; }

//    #endregion Scalar Properties
//}
//public class JobWorkReceiptTransformationByProduct
//{

//    #region Scalar Properties
//    public string Id { get; set; }
//    public string ReceiveQuantity { get; set; }
//    public string Remarks { get; set; }

//    #endregion Scalar Properties
//}

//public class JobWorkTransformationReceiptWOMaterial
//{

//    #region Scalar Properties

//    public string Id { get; set; }
// //   public string CostCenterId { get; set; }
//    public string OSTransformationPOId { get; set; }

//    public string OSTransformationPODetailId { get; set; }
//    public string QualityStatus { get; set; }
//    //public string JWInputItemId { get; set; }
//    public string TransactionUoM { get; set; }

//    public string TransactionUoMId { get; set; }

//    public string BaseUoMId { get; set; }
//    public string TransactionQty { get; set; }
//    //public string Remarks { get; set; }
//    //public string Value { get; set; }
//    //public string LotNumber { get; set; }


//    #endregion Scalar Properties
//}

//public class JWTransformationReceiptWOMaterialByProduct
//{

//    #region Scalar Properties

//    public string Id { get; set; }
//    //   public string CostCenterId { get; set; }
//    public string OSTransformationPOInputMaterialId { get; set; }

//    public string OSTransformationPOByProductId { get; set; }
//    public string QualityStatus { get; set; }
//    //public string JWInputItemId { get; set; }
//    public string TransactionUoM { get; set; }

//    public string TransactionUoMId { get; set; }

//    public string BaseUoMId { get; set; }
//    public string TransactionQty { get; set; }

//    #endregion Scalar Properties
//}