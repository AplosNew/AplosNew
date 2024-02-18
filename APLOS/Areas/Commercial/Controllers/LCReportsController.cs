#region Using

using Aplos.Controllers;
using Aplos.MaterialManagement;
using Aplos.Properties;
using Library.Accounting.FixedAssets;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Setups;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Commercial.Controllers
{
    public class LCReportsController : BaseController
    {

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public LCReportsController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor
        private string CellAddr(int Col, int Row)
        {
            return clsStaticInfo.GetxlsCol(Col) + Row.ToString();
        }


        public ActionResult Aplos()
        {
            return View();
        }
		public ActionResult btb()
		{
			return View();
		}

		[HttpPost, Authorize]
		public ActionResult MasterLCDataXls(List<Dictionary<string, object>> data, string reportFileName)
		{
			try
			{
				DataTable dt = new DataTable("DD");
				foreach (string item in data[0].Keys)
				{
					if (item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
						continue;

					dt.Columns.Add(item);
				}


				for (int i = 0; i < data.Count; i++)
				{
					DataRow dr = dt.NewRow();
					foreach (string item in data[i].Keys)
					{
						if (item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
							continue;

						dr[item] = data[i][item];
					}

					dt.Rows.Add(dr);
				}

				string fileName = "";
				InventoryReceiveQueryService obj = new InventoryReceiveQueryService(_sqlRepository);
				fileName = obj.GetMasterLCReport(dt, "", reportFileName);
				//fileName = GetMasterLCReport(dt, "", reportFileName);
				return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
	
        [HttpPost, Authorize]
        public ActionResult GetMasterLCList(string FromDate, string ToDate, string lcType)
        {
            try
            {
                var datePic = "";
                if (FromDate != null && ToDate != null)
                {
                    FromDate = Convert.ToDateTime(FromDate).ToString("dd-MMM-yyyy");
                    ToDate = Convert.ToDateTime(ToDate).ToString("dd-MMM-yyyy");

                    if (Convert.ToDateTime(FromDate) > Convert.ToDateTime(ToDate))
                        throw new Exception("To date cannot be earlier than from date");

					if (lcType == "contract")
					{
                        datePic = "where c.AddedDate between '" + FromDate + @"' and '" + ToDate + @"' "; 
                    }
					else if (lcType == "masterLC")
					{
						datePic = "where MLC.AddedDate between '" + FromDate + @"' and '" + ToDate + @"' ";
					}
                    else
                    {
						datePic = "where PLC.AddedDate between '" + FromDate + @"' and '" + ToDate + @"' ";
					}
				}

                string sql = "";
                if (lcType == "contract")
                { 
					sql = @"select convert(bit,0) AS isSelected,
				 isnull(PLC.Id,'') PurchaseLCId 
				,BN.AccountTitle Bank
				,ISNULL(mlc.LCRef,'') as MasterLCRefNo
                    ,PurchaseLCRef= isnull(STUFF((select distinct ','+XVD.LCRef 
                    from dbo.PurchaseLC XVD 
                    where XVD.Id=PLC.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
				    ,Format(PLC.LCDate,'dd-MMM-yyyy') LCOpeningDate				 
				 ,Format(PLC.ExpiryDate,'dd-MMM-yyyy') ExpiryDate				 
		            ,isnull(XC.Code,'') PurCurrencyCode 
							,AmendmentAmount = case when PLC.Version=1 then 0 else PLC.Amount end
							,ISNULL( plc.Rate,0)Rate
                           ,isnull(PLC.Type,'') Type,isnull(PLC.Tenure,0)Tenure
							,isnull( bm.AccountTitle ,'')OpeningBank
							,isnull(PLC.BenificiaryBank,'') BenificiaryBank
							,isnull(PLC.BenificiaryBankDescription,'')BenificiaryBankDescription
                           ,ISNULL(PLC.LeinBank,'')LeinBank
							, ISNULL(plc.LeinBankDescription,'')LeinBankDescription
						
					
							,plc.VendorId
							,isnull( P.UserName,'') as Vendor
							,isnull(plc.PortOfLoading,'') PortOfLoading
							,isnull(plc.FinalDestination,'') FinalDestination
							,isnull(plc.Status,'') Status
							,isnull(PLC.LCANo,'') LCANo
							,isnull(PLC.PaymentBasedOn,'') PaymentBasedOn
							,format( plc.ShipmentDate, 'dd-MMMM-yyyyy')ShipmentDate
						   ,isnull(plc.PINo,0) PINo
							,c.Id ContractId,isnull(c.FileNo,'') FileNo
							,isnull(c.ContractNo,'') ContractNo
							,PA.UserName Customer 
							,isnull(c.MasterLCId,'') MasterLCNo
							,isnull(c.UDNo,'') UDNo
							,isnull( plc.OrderSpecific,'')OrderSpecific
							,cf.Percentage as PurchaseMargin,cfc.Percentage as CommissionPercentage
							
							,c.TotalQty  ContractOrderQty
							,c.Amount ContractOrderValue
							,PurchaseCur.Code PurchasePLCurrency
							
							,po.POValue,ISNULL(PLCV.Amount,PLC.Amount) as PurchaseLcOpeningValue
							,format(PLC.AmendmentDate, 'dd-MMM-yyyy') AS LastAmendmentDate
							,MLC.Amount as MasterLCValue
							,PLC.Amount as PresentLCValue
							,format(PLC.LCDate, 'dd-MMM-yyyy')As PurchaseLCOpeningDate,PurchaseCur.Code AS MasterLCcurrency
							,MLC.Id MasterLCId,PLC.LCRef PurchaseLCRefNo
							,PONo= isnull( STUFF((select distinct ','+xpomap.POId 
                    from  dbo.PurchaseLC XVD 
					Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
                    LEFT JOIN trn.PurchaseDocAcceptancePOMap xpomap on xpomap.PurchaseDocAcceptanceId=xp.Id
                    where XVD.Id=PLC.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')

                    ,GRNNo= isnull( STUFF((select distinct ','+xgrnmap.GRNId from
                    dbo.PurchaseLC XVD 
					Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
                    LEFT JOIN trn.GRNAcceptanceMap xgrnmap on xgrnmap.PurchaseDocumentAcceptanceId=xp.Id
                    where XVD.Id=XP.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'') 

					,buyer=STUFF((select distinct ','+XB.UserName from trn.MasterOrder XMO 
				 									left outer join trn.MasterOrderItem XMOI on XMO.Id=XMOI.MasterOrderId
				 									inner join trn.SalesOrder SO on SO.MasterOrderItemId=XMOI.Id
				 								   left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
				 								   where C.Id=SO.ContractId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

					,MasterOrderCurrency=STUFF((select distinct ','+CurOrder.Code from trn.MasterOrder XMO 
				 									left outer join trn.MasterOrderItem XMOI on XMO.Id=XMOI.MasterOrderId
				 									inner join trn.SalesOrder SO on SO.MasterOrderItemId=XMOI.Id
				 								   left join scs.Currency CurOrder on CurOrder.id=XMO.CurrencyId
				 								   where C.Id=SO.ContractId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

                            from dbo.Contract c
                            left join PurchaseLC PLC on c.Id = PLC.ContractId
							left outer join scs.Currency PurchaseCur on PurchaseCur.Id = PLC.CurrencyId
                            left outer join mst.BankMaster bm on bm.id = PLC.OpeningBankMasterId
                            left outer join hkp.Bank b on b.id = bm.BankId
                            left outer join hkp.Party as P on P.Id = PLC.VendorId
                            left join dbo.MasterLC MLC on MLC.Id = c.MasterLCId
							left join hkp.Party PA on PA.Id=c.CustomerId
                            left join contractfund as cf on cf.ContractId= c.Id and cf.FundUtilization='Purchase'
                            left join contractfund as cfc on cfc.ContractId= c.Id and cfc.FundUtilization='LessCommission'
                            LEFT JOIN SCS.Currency XC ON XC.Id=PLC.CurrencyId
							LEFT JOIN MST.BankMaster BN ON BN.Id=C.BankId
					                left outer join (select po.PurchaseLCId,SUM(pod.TransactionQty*pod.TransactionRate) AS POValue from trn.PurchaseOrder PO
									Left outer join  trn.PurchaseOrderDetail POD on pod.InventoryReceiveId=po.Id
					                group by po.PurchaseLCId) AS PO on po.PurchaseLCId=plc.Id
									LEFT JOIN dbo.PurchaseLCVersion PLCV ON PLCV.PurchaseLCId = PLC.Id 
                                AND PLCV.Id=(SELECT TOP 1 ID FROM dbo.PurchaseLCVersion EII WHERE EII.PurchaseLCId = PLC.Id  ORDER BY EII.Version ASC )

                            " + datePic + "" + 

							" order by c.ContractDate";
                }

                else if (lcType == "masterLC")
                {
                    sql = @"select convert(bit,0) AS isSelected,
				 PLC.Id PurchaseLCId 
                ,PLC.LCRef PurchaseLCRefNo ,BN.AccountTitle Bank
				,ISNULL(mlc.LCRef,'') as MasterLCRefNo
                    ,PurchaseLCRef= STUFF((select distinct ','+XVD.LCRef 
                    from dbo.PurchaseLC XVD 
                    where XVD.Id=PLC.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

					-- ,Format( PLC.LCDate,'dd-MMM-yyyy') as PurchaseLCOpeningDate
				   ,LCOpeningDate= STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), XVD.LCDate, 106),' ','-') 
				   from dbo.PurchaseLC XVD 
				   where XVD.Id=PLC.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

				 ,Format( PLC.ExpiryDate, 'dd-MMM-yyyy')as ExpiryDate
				 
                  --  , PurchaseCur.Code as PurchaseCurrency
		            ,PurchaseCurrency= STUFF((select distinct ','+XC.Code
                    from dbo.PurchaseLC XVD 
                    LEFT JOIN SCS.Currency XC ON XC.Id=XVD.CurrencyId
                    where XVD.Id=PLC.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

 							,ISNULL( PLC.Amount,0) as PurchaseLCAmount
							,ISNULL( plc.Rate,0)Rate
                           , PLC.Type, PLC.Tenure

							--,isnull( bm.AccountTitle ,'')OpeningBank
							,OpeningBank=isnull( STUFF((select distinct ','+xbm.AccountTitle 
						    from dbo.PurchaseLC XVD 
							left join MST.BankMaster xbm on xbm.Id=XVD.OpeningBankMasterId
							where XVD.Id=PLC.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')

							--, PLC.BenificiaryBank
							,BenificiaryBank=isnull( STUFF((select distinct ','+XVD.BenificiaryBank 
							from dbo.PurchaseLC XVD 
							where XVD.Id=PLC.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')

							,PLC.BenificiaryBankDescription
                           ,ISNULL(PLC.LeinBank,'')LeinBank
							, ISNULL(plc.LeinBankDescription,'')LeinBankDescription
						
					
							,plc.VendorId
							,isnull( P.UserName,'') as Vendor
							,plc.PortOfLoading
							,plc.FinalDestination
							,plc.Status
							,PLC.LCANo
							,PLC.PaymentBasedOn
							,format( plc.ShipmentDate, 'dd-MMMM-yyyyy')ShipmentDate

							--,plc.PINo
						   ,PINo= STUFF((select distinct ','+XVD.PINo 
						   from dbo.PurchaseLC XVD 
						   where XVD.Id=PLC.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


							,plc.ContractId,isnull(c.FileNo,'') FileNo
						--	c.ContractNo,c.ContractDate,p.UserName Customer

                    ,ContractNo= isnull( STUFF((select distinct ','+XC.ContractNo 
					from dbo.[Contract] XC 
					left join PurchaseLC xPlc ON XC.Id=xPlc.ContractId
                    where xPlc.Id=PLC.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')

                    ,Customer= isnull( STUFF((select distinct ','+XCU.UserName 
					from dbo.PurchaseLC XVD 
                    LEFT JOIN dbo.[Contract] XC ON XC.Id=XVD.ContractId
                    join HKP.Party XCU ON XCU.Id=XC.CustomerId
                    where XVD.Id=PLC.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')

                    ,MasterLCNo= isnull( STUFF((select distinct ','+XC.MasterLCId
					from dbo.PurchaseLC XVD 
                    LEFT JOIN dbo.[Contract] XC ON XC.Id=XVD.ContractId
                    where XVD.Id=PLC.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')

                    ,UDNo= isnull( STUFF((select distinct ','+XC.UDNo 
                    from dbo.PurchaseLC XVD 
                    LEFT JOIN dbo.[Contract] XC ON XC.Id=XVD.ContractId
                    where XVD.Id=PLC.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')

				    ,isnull( plc.OrderSpecific,'')OrderSpecific

					,PONo= isnull( STUFF((select distinct ','+xpomap.POId 
                    from  dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
                    LEFT JOIN trn.PurchaseDocAcceptancePOMap xpomap on xpomap.PurchaseDocAcceptanceId=xp.Id
                    where XVD.Id=PLC.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')

                    ,GRNNo= isnull( STUFF((select distinct ','+xgrnmap.GRNId from
                    dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
                    LEFT JOIN trn.GRNAcceptanceMap xgrnmap on xgrnmap.PurchaseDocumentAcceptanceId=xp.Id
                    where XVD.Id=XP.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
                    ,cf.Percentage as PurchaseMargin,cfc.Percentage as CommissionPercentage,cov.Buyer,cov.MasterOrderCurrency,cov.ContractOrderQty
                    ,cov.ContractOrderValue,COV.MasterOrderCurrency PurchasePLCurrency,po.POValue,PLC.Amount as PurchaseLcOpeningValue,format(PLCV.AmendmentDate, 'dd-MMM-yyyy') AS LastAmendmentDate,MLC.Amount as MasterLCValue
					,PLCV.Amount as PresentLCValue
					,format(PLC.LCDate, 'dd-MMM-yyyy')As PurchaseLCOpeningDate,PurchaseCur.Code AS MasterLCcurrency,MLC.Id MasterLCId

                            from dbo.MasterLC MLC
                            left join dbo.Contract c on MLC.Id=c.MasterLCId
							left join PurchaseLC PLC on c.Id=PLC.ContractId
                            left outer join scs.Currency PurchaseCur on PurchaseCur.Id= PLC.CurrencyId
                            left outer join mst.BankMaster bm on bm.id=PLC.OpeningBankMasterId
                            left outer join hkp.Bank b on b.id= bm.BankId
                           -- left outer join mst.Destination fd on fd.id= PLC.FinalDestinationId
                            left outer join hkp.Party as P on P.Id=PLC.VendorId
							LEFT JOIN MST.BankMaster BN ON BN.Id=C.BankId
                            left join contractfund as cf on cf.ContractId= c.Id and cf.FundUtilization='Purchase'
                            left join contractfund as cfc on cfc.ContractId= c.Id and cfc.FundUtilization='LessCommission'

							left outer join (select  buyer=STUFF((select distinct ','+XB.UserName from 
				 								trn.MasterOrder XMO 
				 								
				 											left outer join trn.MasterOrderItem XMOI on XMO.Id=XMOI.MasterOrderId
				 											inner join trn.SalesOrder SO on SO.MasterOrderItemId=XMOI.Id
				 											inner join Contract XC on XC.Id=SO.ContractId
				 									left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
				 										where C.Id=XC.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')			 																		,
				 																		
				 						 c.Id As ContractNo,CurOrder.Code AS MasterOrderCurrency
				                ,SUM(so.Qty) AS ContractOrderQty,sum(so.qty*so.rate) AS ContractOrderValue from MasterLC MLC
				 				inner join Contract C on mlc.Id=c.MasterLCId
                                LEFT JOIN TRN.SalesOrder so on c.Id=so.ContractId
				 				left outer join trn.MasterOrderItem moi on moi.id=so.MasterOrderItemId				 
				 				inner join trn.MasterOrder mo on moi.MasterOrderId=mo.Id 				 				
				 				left join scs.Currency CurOrder on CurOrder.id=mo.CurrencyId
				 				group by c.Id,CurOrder.Code) AS COV on cov.ContractNo=c.Id  

							left outer join (select po.PurchaseLCId,SUM(pod.TransactionQty*pod.TransactionRate) AS POValue from trn.PurchaseOrder PO
					            Left outer join  trn.PurchaseOrderDetail POD on pod.InventoryReceiveId=po.Id
                                group by po.PurchaseLCId) AS PO on po.PurchaseLCId=plc.Id
							LEFT JOIN dbo.PurchaseLCVersion PLCV ON PLCV.PurchaseLCId = PLC.Id 
                                AND PLCV.Id=(SELECT TOP 1 ID FROM dbo.PurchaseLCVersion EII WHERE EII.PurchaseLCId = PLC.Id  ORDER BY EII.Version ASC )

                            " + datePic + "" +
                            "order by MLC.LCDate";
                }
                else
                {
                    sql = @"select convert(bit,0) AS isSelected
				         ,PLC.Id PurchaseLCId ,BN.AccountTitle Bank
				        ,ISNULL(mlc.LCRef,'') as MasterLCRefNo
                            ,PurchaseLCRef= STUFF((select distinct ','+XVD.LCRef 
                            from dbo.PurchaseLC XVD 
                            where XVD.Id=PLC.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

					        -- ,Format( PLC.LCDate,'dd-MMM-yyyy') as PurchaseLCOpeningDate
				           ,plc.LCDate LCOpeningDate 
				         ,Format( PLC.ExpiryDate, 'dd-MMM-yyyy')as ExpiryDate				 
		                    ,XC.Code PurchaseCurrency 
 							,ISNULL( PLC.Amount,0) as PurchaseLCAmount
							,AmendmentAmount = case when PLC.Version=1 then 0 else PLC.Amount end
							,ISNULL( plc.Rate,0)Rate
                           , PLC.Type, PLC.Tenure
							,isnull( bm.AccountTitle ,'')OpeningBank
							,PLC.BenificiaryBank 
							,PLC.BenificiaryBankDescription
                           ,ISNULL(PLC.LeinBank,'')LeinBank
							, ISNULL(plc.LeinBankDescription,'')LeinBankDescription
											
							,plc.VendorId
							,isnull( P.UserName,'') as Vendor
							,plc.PortOfLoading
							,plc.FinalDestination
							,plc.Status
							,PLC.LCANo
							,PLC.PaymentBasedOn
							,format( plc.ShipmentDate, 'dd-MMMM-yyyyy')ShipmentDate
						   ,plc.PINo
							,c.Id ContractId,isnull(c.FileNo,'') FileNo
							,isnull(c.ContractNo,'') ContractNo
							,XCU.UserName Customer 
                            ,MLC.Id MasterLCId
							,c.MasterLCId MasterLCNo
							,isnull(c.UDNo,'') UDNo
							,isnull( plc.OrderSpecific,'')OrderSpecific
                            ,cf.Percentage as PurchaseMargin,cfc.Percentage as CommissionPercentage,cov.Buyer,cov.MasterOrderCurrency,cov.ContractOrderQty
							,cov.ContractOrderValue,COV.MasterOrderCurrency PurchasePLCurrency,po.POValue,PLC.Amount as PurchaseLcOpeningValue
							,format(PLCV.AmendmentDate, 'dd-MMM-yyyy') AS LastAmendmentDate
							,MLC.Amount as MasterLCValue
							,PLCV.Amount as PresentLCValue
							,format(PLC.LCDate, 'dd-MMM-yyyy')As PurchaseLCOpeningDate,PurchaseCur.Code AS MasterLCcurrency,PLC.LCRef PurchaseLCRefNo
							,PONo= isnull( STUFF((select distinct ','+xpomap.POId
							from  dbo.PurchaseLC XVD 
							Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
							LEFT JOIN trn.PurchaseDocAcceptancePOMap xpomap on xpomap.PurchaseDocAcceptanceId=xp.Id
							where XVD.Id=PLC.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')

							,GRNNo= isnull( STUFF((select distinct ','+xgrnmap.GRNId from
							dbo.PurchaseLC XVD 
							Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
							LEFT JOIN trn.GRNAcceptanceMap xgrnmap on xgrnmap.PurchaseDocumentAcceptanceId=xp.Id
							where XVD.Id=XP.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'') 

                            from PurchaseLC PLC
                            left outer join scs.Currency PurchaseCur on PurchaseCur.Id= PLC.CurrencyId
                            left outer join mst.BankMaster bm on bm.id=PLC.OpeningBankMasterId
                            left outer join hkp.Bank b on b.id= bm.BankId
                            left outer join hkp.Party as P on P.Id=PLC.VendorId
                            left join dbo.Contract c on c.Id=PLC.ContractId
							left join dbo.MasterLC MLC on MLC.Id=c.MasterLCId
							left join HKP.Party XCU ON XCU.Id=c.CustomerId
							LEFT JOIN SCS.Currency XC ON XC.Id=PLC.CurrencyId
							LEFT JOIN MST.BankMaster BN ON BN.Id=C.BankId
                            left join contractfund as cf on cf.ContractId= c.Id and cf.FundUtilization='Purchase'
                            left join contractfund as cfc on cfc.ContractId= c.Id and cfc.FundUtilization='LessCommission'

							left outer join (select  buyer=STUFF((select distinct ','+XB.UserName from 
				 								trn.MasterOrder XMO 
				 								
				 											left outer join trn.MasterOrderItem XMOI on XMO.Id=XMOI.MasterOrderId
				 											inner join trn.SalesOrder SO on SO.MasterOrderItemId=XMOI.Id
				 											inner join Contract XC on XC.Id=SO.ContractId
				 									left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
				 										where C.Id=XC.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')			 																		,				 																		
				 						 c.Id As ContractNo,CurOrder.Code AS MasterOrderCurrency
				                ,SUM(so.Qty) AS ContractOrderQty,sum(so.qty*so.rate) AS ContractOrderValue from MasterLC MLC
				 				inner join Contract C on mlc.Id=c.MasterLCId
                                LEFT JOIN TRN.SalesOrder so on c.Id=so.ContractId
				 				left outer join trn.MasterOrderItem moi on moi.id=so.MasterOrderItemId				 
				 				inner join trn.MasterOrder mo on moi.MasterOrderId=mo.Id 				 				
				 				left join scs.Currency CurOrder on CurOrder.id=mo.CurrencyId
				 				group by c.Id,CurOrder.Code) AS COV on cov.ContractNo=c.Id  

							left outer join (select po.PurchaseLCId,SUM(pod.TransactionQty*pod.TransactionRate) AS POValue from trn.PurchaseOrder PO
					            Left outer join  trn.PurchaseOrderDetail POD on pod.InventoryReceiveId=po.Id
                                group by po.PurchaseLCId) AS PO on po.PurchaseLCId=plc.Id
							LEFT JOIN dbo.PurchaseLCVersion PLCV ON PLCV.PurchaseLCId = PLC.Id 
                                AND PLCV.Id=(SELECT TOP 1 ID FROM dbo.PurchaseLCVersion EII WHERE EII.PurchaseLCId = PLC.Id  ORDER BY EII.Version ASC )
                            " + datePic + "" +
                            "order by PLC.LCDate";
                }

                var data = _sqlRepository.GetDataCollection(sql);
				var jsondata = Json(data, JsonRequestBehavior.AllowGet);
				jsondata.MaxJsonLength = int.MaxValue;
				return jsondata;

			}
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }

		[HttpPost]
		public ActionResult GetBTBPerformanceDataList()
		{
			FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
			return Json(_fixedAssetQueryService.BTBPerformanceData(), JsonRequestBehavior.AllowGet);
		}

		[HttpPost, Authorize]
		public ActionResult BTBPerformanceDataXls(List<Dictionary<string, object>> data, string reportFileName)
		{
			try
			{
				string fileName = "";
				InventoryReceiveQueryService obj = new InventoryReceiveQueryService(_sqlRepository);
				fileName = obj.GetBTBPerformanceReport(data, "", reportFileName);
				return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

	}
}