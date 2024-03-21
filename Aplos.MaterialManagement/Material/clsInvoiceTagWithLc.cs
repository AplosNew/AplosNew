using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Extension;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.MaterialManagement.Material
{
    public class clsInvoiceTagWithLc
    {
        ISqlRepository _sqlRepository;
        public clsInvoiceTagWithLc()
        {
            _sqlRepository = new SqlRepository();
        }
        public IEnumerable<object> VendorAvailableInvoiceList(string companyGroupId, string companyId, string FromDate, string ToDate, bool DateRange)
        {
            try
            {
                string DatewiseData = "";
                if (DateRange)
                {
                    DatewiseData = "AND IV.ActualDueDate between '" + FromDate + @"' And '" + ToDate + @"'";

                }
                else
                {
                    DatewiseData = "AND IV.ActualDueDate <= '" + FromDate + @"'";
                }
                string strSQL = string.Empty;
                strSQL = @" SELECT IVD.GLGeneralInfoId AS GLGeneralInfoId
									,GLGI.AccountCode AS GLGeneralInfoCode
									,GLGI.UserName AS GLGeneralInfoName
									,IVD.BudgetMasterId
									,B.UserName AS BudgetName
									,IVD.ActivityId
									,EN.UserName AS EntityName
									,A.UserName AS ActivityName
									,V.VoucherNo
									,Replace(CONVERT(VARCHAR(11), IV.DocDate, 106), ' ', '-') DocDate
									,Replace(CONVERT(VARCHAR(11), IV.PostingDate, 106), ' ', '-') PostingDate
									,IV.DocRefNo
									,IV.Narration
									,IV.Id AS InvoiceId
									,EN.Id EntityId
									,VD.PlantId
									,IVD.Id AS InvoiceDetailId
									,IV.VoucherId
									,Replace(CONVERT(VARCHAR(11),IV.ActualDueDate, 106), ' ', '-') ActualDueDate
									,Replace(CONVERT(VARCHAR(11),IV.BaseOnDueDate, 106), ' ', '-') BaseOnDueDate
									,IV.BaseNoOfDays, CASE WHEN  IV.SourceType = 'VendorInvoice' THEN 'Inbound Invoice'  
															WHEN  IV.SourceType = 'InventoryPayable' THEN  'GRN' 
															WHEN  IV.SourceType = 'PostInvoice' THEN  'Post Invoice' 
														END SourceType
									,VD.Id AS VoucherDetailId
									,IV.CurrencyId
									,C.Code AS CurrencyCode
									,IV.PartyId
									,IVD.Amount AS Receivable
									,V.ExchangeType
									,0 ExchangeAmount
									,IVD.WrittenOffAmount+ISNULL(ITLC.TaggedAmount,0) AS Received
									,IVD.NetAmount - (ISNULL(IVD.WrittenOffAmount,0)+ISNULL(ITLC.TaggedAmount,0)) AS Balance
									,IVD.Amount - IVD.WrittenOffAmount AS Amount
									,IV.PartyPlantId
									,PP.UserName AS PartyPlantName
									,CC.CompanyCurrencyId
									,CC.CompanyFromCurrencyId
									,CC.ToCurrencyId
									,CC.CompanyCurrencyRate
									,CC.CompanyCurrencyConversion
									,GC.CompanyGroupCurrencyId
									,GC.CompanyGroupFromCurrencyId
									,GC.CompanyGroupCurrencyRate
									,GC.CompanyGroupCurrencyConversion
									,HC.HardCurrencyId
									,HC.HardFromCurrencyId
									,HC.HardCurrencyRate
									,HC.HardCurrencyConversion
									,Particular = REPLACE(REPLACE(STUFF((
													SELECT DISTINCT ',' + xpo.UserName
													FROM hkp.Activity xpo
													INNER JOIN TRN.VoucherDetail xPDAMAP ON xpo.id = xPDAMAP.ActivityId
													WHERE VD.ActivityId != xPDAMAP.ActivityId
														AND xPDAMAP.VoucherId = V.Id
													FOR XML path('')
														,TYPE
													).value('.', 'VARCHAR(MAX)'), 1, 1, ''), '&amp;', '&'), 'amp;', '')
									,AcceptanceNo = STUFF((
											SELECT DISTINCT ',' + XPDA.AcceptanceNo
											FROM TRN.PurchaseDocAcceptance XPDA
											LEFT JOIN TRN.Voucher XV ON XV.Id = XPDA.VoucherId
											WHERE XV.Id = V.Id
											FOR XML path('')
												,TYPE
											).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,LCRef = STUFF((
											SELECT DISTINCT ',' + XLC.LCRef
											FROM dbo.PurchaseLC XLC
											LEFT JOIN TRN.PurchaseDocAcceptance XPDA ON XPDA.PurchaseLCId = XLC.Id
											LEFT JOIN TRN.Voucher XV ON XV.Id = XPDA.VoucherId
											WHERE XV.Id = V.Id
											FOR XML path('')
												,TYPE
											).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,ContractNo = STUFF((
											SELECT DISTINCT ',' + XC.ContractNo
											FROM dbo.PurchaseLC XLC
											JOIN TRN.PurchaseDocAcceptance XPDA ON XPDA.PurchaseLCId = XLC.Id
											LEFT JOIN dbo.Contract XC ON XC.Id = XLC.ContractId
											LEFT JOIN TRN.Voucher XV ON XV.Id = XPDA.VoucherId
											WHERE XV.Id = V.Id
											FOR XML path('')
												,TYPE
											).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,Customer = STUFF((
											SELECT DISTINCT ',' + XP.UserName
											FROM dbo.PurchaseLC XLC
											JOIN TRN.PurchaseDocAcceptance XPDA ON XPDA.PurchaseLCId = XLC.Id
											LEFT JOIN dbo.Contract XC ON XC.Id = XLC.ContractId
											LEFT JOIN HKP.Party XP ON XP.Id = XC.CustomerId
											LEFT JOIN TRN.Voucher XV ON XV.Id = XPDA.VoucherId
											WHERE XV.Id = V.Id
											FOR XML path('')
												,TYPE
											).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,MasterLCNo = STUFF((
											SELECT DISTINCT ',' + MLC.LCRef
											FROM dbo.PurchaseLC XLC
											JOIN TRN.PurchaseDocAcceptance XPDA ON XPDA.PurchaseLCId = XLC.Id
											LEFT JOIN dbo.Contract XC ON XC.Id = XLC.ContractId
											LEFT JOIN dbo.MasterLC MLC ON MLC.Id = XC.MasterLCId
											LEFT JOIN TRN.Voucher XV ON XV.Id = XPDA.VoucherId
											WHERE XV.Id = V.Id
											FOR XML path('')
												,TYPE
											).value('.', 'VARCHAR(MAX)'), 1, 1, ''), NULL PurchaseLcId, NULL LoanNo,NULL LCDate,NULL OpeningBank,NULL OpeningBankMasterId
								FROM [TRN].[InvoiceDetail] AS IVD
								LEFT JOIN [TRN].[Invoice] AS IV ON IVD.InvoiceId = IV.Id
								LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id = IV.PartyPlantId
								LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId = IVD.Id
								LEFT JOIN [TRN].[Voucher] AS V ON V.Id = IV.VoucherId
								LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id = IVD.GLGeneralInfoId
								LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id = IVD.BudgetMasterId
								LEFT JOIN [HKP].[Budget] AS B ON B.Id = BM.BudgetId
								LEFT JOIN [HKP].[Activity] AS A ON A.Id = IVD.ActivityId
								LEFT JOIN [SCS].[Currency] AS C ON C.Id = IV.CurrencyId
								LEFT JOIN [ORG].[Entity] AS EN ON EN.Id = IV.EntityId
								LEFT JOIN (SELECT invoiceDetailId,SUM(ITLD.Amount) TaggedAmount FROM InvoiceTaggingWithLCDetail ITLD 
								join InvoiceTaggingWithLCMaster ITM ON ITM.Id=ITLD.InvoiceTaggingWithLCMasterId
										WHERE ITM.VoucherId IS NULL
										Group By invoiceDetailId) ITLC ON IVD.Id=ITLC.invoiceDetailId
								LEFT JOIN (
									SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId
										,VDC.FromCurrencyId AS CompanyFromCurrencyId
										,VDC.ToCurrencyId
										,VDC.ToCurrencyRate AS CompanyCurrencyRate
										,VDC.ToCurrencyConversion AS CompanyCurrencyConversion
										,VDC.DrAmount AS CompanyCurrencyAmount
										,VDC.VoucherDetailId
									FROM [TRN].[VoucherDetailCurrency] AS VDC
									JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId = VDC.ParallelCurrencyId
									WHERE CPC.ParallelCurrencyType = 'CompanyCurrency'
										AND CPC.CompanyId = '" + companyId + @"'
									) AS CC ON CC.VoucherDetailId = VD.Id
								LEFT JOIN (
									SELECT VDC.ParallelCurrencyId AS CompanyGroupCurrencyId
										,VDC.FromCurrencyId AS CompanyGroupFromCurrencyId
										,VDC.ToCurrencyId
										,VDC.ToCurrencyRate AS CompanyGroupCurrencyRate
										,VDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion
										,VDC.DrAmount AS CompanyGroupCurrencyAmount
										,VDC.VoucherDetailId
									FROM [TRN].[VoucherDetailCurrency] AS VDC
									JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId = VDC.ParallelCurrencyId
									WHERE CPC.ParallelCurrencyType = 'CompanyGroupCurrency'
										AND CPC.CompanyId = '" + companyId + @"'
									) AS GC ON GC.VoucherDetailId = VD.Id
								LEFT JOIN (
									SELECT VDC.ParallelCurrencyId AS HardCurrencyId
										,VDC.FromCurrencyId AS HardFromCurrencyId
										,VDC.ToCurrencyId
										,VDC.ToCurrencyRate AS HardCurrencyRate
										,VDC.ToCurrencyConversion AS HardCurrencyConversion
										,VDC.DrAmount AS HardCurrencyAmount
										,VDC.VoucherDetailId
									FROM [TRN].[VoucherDetailCurrency] AS VDC
									JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId = VDC.ParallelCurrencyId
									WHERE CPC.ParallelCurrencyType = 'HardCurrency'
										AND CPC.CompanyId = '" + companyId + @"'
									) AS HC ON HC.VoucherDetailId = VD.Id
								WHERE IV.Archive = 0
									AND IV.IsWrittenOff = 0
									AND IVD.IsWrittenOff = 0
									AND V.IsPark = 0
									AND IVD.IsBlock = 0
									AND IV.SourceType IN (
										'" + SourceType.VendorInvoice + @"'
										,'" + SourceType.SuspensePayable + @"'
										,'" + SourceType.ServicePayable + @"'
										,'" + SourceType.EmployeePayable + @"'
										,'" + SourceType.PostInvoice + @"'
										)
									AND IV.CompanyGroupId = '" + companyGroupId + @"'
									AND IV.CompanyId = '" + companyId + @"'
									" + DatewiseData + @"
								--AND IV.Id NOT IN (SELECT InvoiceId FROM InvoiceTaggingWithLCDetail)
								AND ISNULL(IV.PurchaseLCId,'')=''
								UNION ALL
								
								SELECT IVD.GLGeneralInfoId AS GLGeneralInfoId
									,GLGI.AccountCode AS GLGeneralInfoCode
									,GLGI.UserName AS GLGeneralInfoName
									,IVD.BudgetMasterId
									,B.UserName AS BudgetName
									,IVD.ActivityId
									,EN.UserName AS EntityName
									,A.UserName AS ActivityName
									,V.VoucherNo
									,Replace(CONVERT(VARCHAR(11), IV.DocDate, 106), ' ', '-') DocDate
									,Replace(CONVERT(VARCHAR(11), IV.PostingDate, 106), ' ', '-') PostingDate
									,IV.DocRefNo
									,IV.Narration
									,IV.Id AS InvoiceId
									,EN.Id EntityId
									,VD.PlantId
									,IVD.Id AS InvoiceDetailId
									,IV.VoucherId
									,Replace(CONVERT(VARCHAR(11),IV.ActualDueDate, 106), ' ', '-') ActualDueDate
									,Replace(CONVERT(VARCHAR(11),IV.BaseOnDueDate, 106), ' ', '-') BaseOnDueDate
									,IV.BaseNoOfDays, CASE WHEN  IV.SourceType = 'VendorInvoice' THEN 'Inbound Invoice'  WHEN  IV.SourceType = 'InventoryPayable' THEN  'GRN' END SourceType
									,VD.Id AS VoucherDetailId
									,IV.CurrencyId
									,C.Code AS CurrencyCode
									,IV.PartyId
									,IVD.NetAmount AS Receivable
									,V.ExchangeType
									,0 ExchangeAmount
									,IVD.WrittenOffAmount+ISNULL(ITLC.TaggedAmount,0) AS Received
									,IVD.NetAmount - (ISNULL(IVD.WrittenOffAmount,0)+ISNULL(ITLC.TaggedAmount,0)) AS Balance
									,IVD.NetAmount - IVD.WrittenOffAmount AS Amount
									, IV.PartyPlantId
									,PP.UserName AS PartyPlantName
									,CC.CompanyCurrencyId
									,CC.CompanyFromCurrencyId
									,CC.ToCurrencyId
									,CC.CompanyCurrencyRate
									,CC.CompanyCurrencyConversion
									,GC.CompanyGroupCurrencyId
									,GC.CompanyGroupFromCurrencyId
									,GC.CompanyGroupCurrencyRate
									,GC.CompanyGroupCurrencyConversion
									,HC.HardCurrencyId
									,HC.HardFromCurrencyId
									,HC.HardCurrencyRate
									,HC.HardCurrencyConversion
									,Particular = REPLACE(REPLACE(STUFF((
													SELECT DISTINCT ',' + xpo.UserName
													FROM hkp.Activity xpo
													INNER JOIN TRN.VoucherDetail xPDAMAP ON xpo.id = xPDAMAP.ActivityId
													WHERE VD.ActivityId != xPDAMAP.ActivityId
														AND xPDAMAP.VoucherId = V.Id
													FOR XML path('')
														, TYPE
													).value('.', 'VARCHAR(MAX)'), 1, 1, ''), '&amp;', '&'), 'amp;', '')
									,NULL AcceptanceNo
									, NULL LCRef
									,NULL ContractNo
									, NULL Customer
									,NULL MasterLCNo, NULL PurchaseLcId, NULL LoanNo,NULL LCDate,NULL OpeningBank,NULL OpeningBankMasterId
								FROM[TRN].[InvoiceDetail] AS IVD
								LEFT JOIN[TRN].[Invoice] AS IV ON IVD.InvoiceId = IV.Id
								LEFT JOIN[HKP].[PartyPlant] AS PP ON PP.Id = IV.PartyPlantId
								LEFT JOIN[TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId = IVD.Id
								LEFT JOIN[TRN].[Voucher] AS V ON V.Id = VD.VoucherId
								LEFT JOIN[HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id = IVD.GLGeneralInfoId
								LEFT JOIN[MST].[BudgetMaster] AS BM ON BM.Id = IVD.BudgetMasterId
								LEFT JOIN[HKP].[Budget] AS B ON B.Id = BM.BudgetId
								LEFT JOIN[HKP].[Activity] AS A ON A.Id = IVD.ActivityId
								LEFT JOIN[SCS].[Currency] AS C ON C.Id = IV.CurrencyId
								LEFT JOIN[ORG].[Entity] AS EN ON EN.Id = IV.EntityId
								LEFT JOIN TRN.InventoryReceive IR ON IR.Id = IV.InventoryReceiveId
								LEFT JOIN (SELECT invoiceDetailId,SUM(ITLD.Amount) TaggedAmount FROM InvoiceTaggingWithLCDetail ITLD 
								join InvoiceTaggingWithLCMaster ITM ON ITM.Id=ITLD.InvoiceTaggingWithLCMasterId
										WHERE ITM.VoucherId IS NULL
										Group By invoiceDetailId) ITLC ON IVD.Id=ITLC.invoiceDetailId
								LEFT JOIN(
									SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId
										, VDC.FromCurrencyId AS CompanyFromCurrencyId
										, VDC.ToCurrencyId
										, VDC.ToCurrencyRate AS CompanyCurrencyRate
										, VDC.ToCurrencyConversion AS CompanyCurrencyConversion
										, VDC.DrAmount AS CompanyCurrencyAmount
										, VDC.VoucherDetailId
									FROM [TRN].[VoucherDetailCurrency] AS VDC
									JOIN[SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId = VDC.ParallelCurrencyId
									WHERE CPC.ParallelCurrencyType = 'CompanyCurrency'
										AND CPC.CompanyId = '" + companyId + @"'
									) AS CC ON CC.VoucherDetailId = VD.Id
								LEFT JOIN(
									SELECT VDC.ParallelCurrencyId AS CompanyGroupCurrencyId
										, VDC.FromCurrencyId AS CompanyGroupFromCurrencyId
										, VDC.ToCurrencyId
										, VDC.ToCurrencyRate AS CompanyGroupCurrencyRate
										, VDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion
										, VDC.DrAmount AS CompanyGroupCurrencyAmount
										, VDC.VoucherDetailId
									FROM [TRN].[VoucherDetailCurrency] AS VDC
									JOIN[SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId = VDC.ParallelCurrencyId
									WHERE CPC.ParallelCurrencyType = 'CompanyGroupCurrency'
										AND CPC.CompanyId = '" + companyId + @"'
									) AS GC ON GC.VoucherDetailId = VD.Id
								LEFT JOIN(
									SELECT VDC.ParallelCurrencyId AS HardCurrencyId
										, VDC.FromCurrencyId AS HardFromCurrencyId
										, VDC.ToCurrencyId
										, VDC.ToCurrencyRate AS HardCurrencyRate
										, VDC.ToCurrencyConversion AS HardCurrencyConversion
										, VDC.DrAmount AS HardCurrencyAmount
										, VDC.VoucherDetailId
									FROM [TRN].[VoucherDetailCurrency] AS VDC
									JOIN[SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId = VDC.ParallelCurrencyId
									WHERE CPC.ParallelCurrencyType = 'HardCurrency'
										AND CPC.CompanyId = '" + companyId + @"'
									) AS HC ON HC.VoucherDetailId = VD.Id
								WHERE IV.Archive = 0
									AND IV.IsWrittenOff = 0
									AND IVD.IsWrittenOff = 0
									AND V.IsPark = 0
									AND IVD.IsBlock = 0
									AND IV.SourceType IN('" + SourceType.InventoryPayable + @"')
									AND IV.CompanyGroupId = '" + companyGroupId + @"'
									AND IV.CompanyId = '" + companyId + @"'
									AND IR.PurchaseDocumentAcceptanceId IS NULL
									" + DatewiseData + @"
								--AND IV.Id NOT IN (SELECT InvoiceId FROM InvoiceTaggingWithLCDetail)
								AND ISNULL(IV.PurchaseLCId,'')=''";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function
        public void Save(List<Dictionary<string, object>> DataList, Dictionary<string, object> LcData)
        {
            try
            {
                #region Variable
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster;
                DataSet dsDetail;
                string Id = string.Empty;
                string TempId = string.Empty;
                bplib.clsGenID objGenID = null;
                objGenID = new bplib.clsGenID();
                objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "InvoiceTaggingWithLCMaster", out TempId);
                int count = 0;
                DataRow drSave;
                DataRow drMSave;
                string MasterId = string.Empty;
                #endregion

                string sql = "SELECT * FROM InvoiceTaggingWithLCMaster WHERE 1=2";
                string sql2 = "SELECT * FROM InvoiceTaggingWithLCDetail WHERE 1=2";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                objCon.OpenDataSetThroughAdapter(sql2, out dsDetail, false, "1");
                if (dsMaster.Tables[0].DefaultView.Count == 0)
                {
                    drMSave = dsMaster.Tables[0].NewRow();
                    drMSave["Id"] = "M" + TempId;
                    MasterId = drMSave["Id"].ToString();
                    drMSave["CompanyGroupId"] = identity.CompanyGroupId;
                    drMSave["CompanyId"] = identity.CompanyId;
                    drMSave["PlantId"] = identity.PlantId;
                    drMSave["EntityId"] = DataList[0]["EntityId"];
                    drMSave["CurrencyId"] = DataList[0]["CurrencyId"];
					drMSave["PartyId"] = DataList[0]["PartyId"];
					drMSave["PartyPlantId"] = DataList[0]["PartyPlantId"];
					drMSave["PurchaseLcId"] = LcData["Id"];
					drMSave["VoucherId"] = DBNull.Value;
					drMSave["BankMasterId"] = LcData["OpeningBankMasterId"];
                    if (Convert.ToBoolean(LcData["IsLoan"]))
                    {
						drMSave["LoanDate"] = LcData["LoanDate"];
						drMSave["LoanNo"] = LcData["LoanNo"];
						drMSave["Amount"] = LcData["LoanAmount"];
					}
                    else
                    {
						drMSave["LoanDate"] = DBNull.Value;
						drMSave["LoanNo"] = DBNull.Value;
						drMSave["Amount"] = DBNull.Value;
					}
                    drMSave["IsLoan"] = LcData["IsLoan"];

                    drMSave["AddedBy"] = identity.Name;
                    drMSave["AddedDate"] = DateTime.Now;
                    drMSave["AddedFromIP"] = identity.IPAddress;
                    drMSave["UpdatedBy"] = identity.Name;
                    drMSave["UpdatedDate"] = DateTime.Now;
                    drMSave["UpdatedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(drMSave);
                }
                foreach (var item in DataList)
                {
                    dsDetail.Tables[0].DefaultView.RowFilter = "Id = '" + item["InvoiceId"] + "'";
                    if (dsDetail.Tables[0].DefaultView.Count == 0)
                    {
                        count++;
                        drSave = dsDetail.Tables[0].NewRow();
                        drSave["Id"] = "D" + TempId + count;
                        drSave["InvoiceTaggingWithLCMasterId"] = MasterId;
                        drSave["InvoiceId"] = item["InvoiceId"];
                        drSave["InvoiceDetailId"] = item["InvoiceDetailId"];
                        drSave["Amount"] = item["Amount"];
                        drSave["PurchaseLcId"] = item["PurchaseLcId"];
                        drSave["OpeningBankMasterId"] = item["OpeningBankMasterId"];
                       
                        drSave["AddedBy"] = identity.Name;
                        drSave["AddedDate"] = DateTime.Now;
                        drSave["AddedFromIP"] = identity.IPAddress;

                        drSave["UpdatedBy"] = identity.Name;
                        drSave["UpdatedDate"] = DateTime.Now;
                        drSave["UpdatedFromIP"] = identity.IPAddress;
                        dsDetail.Tables[0].Rows.Add(drSave);

                    }
                    else
                    {
                        drSave = dsDetail.Tables[0].DefaultView[0].Row;
                        drSave.BeginEdit();

                        drSave["UpdatedBy"] = identity.Name;
                        drSave["UpdatedDate"] = DateTime.Now;
                        drSave["UpdatedFromIP"] = identity.IPAddress;
                        drSave.EndEdit();
                    }
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsDetail);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

		public void SaveWithoutLoan(List<Dictionary<string, object>> DataList, Dictionary<string, object> LcData)
		{
			try
			{
				var vendorAdWr = new System.Text.StringBuilder();
				var vendorAdWrsql = "";

				foreach (var item in DataList)
				{
					vendorAdWrsql = @"update TRN.Invoice set PurchaseLCId='" + LcData["Id"] + "' where Id ='" + item["InvoiceId"] + "' ";
					vendorAdWr.Append(vendorAdWrsql);
				}
				_sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
			}
			catch (Exception ex)
			{

				throw ex;
			}
		}

		public IEnumerable<object> GetMaster(string CompanyGroupId, string CompanyId, string PlantId)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"SELECT m.Id ,pl.LCRef,p.UserName Vendor ,FORMAT(m.LoanDate, 'dd-MMM-yyyy') LoanDate
										,m.LoanNo ,c.Code Currency ,ITLC.Amount,m.VoucherId,V.VoucherNo
										,CASE WHEN ISNULL(m.VoucherId, '') = '' THEN 'Park' ELSE 'Post' END [Status]
									FROM InvoiceTaggingWithLCMaster m
									LEFT JOIN (SELECT InvoiceTaggingWithLCMasterId,SUM(Amount)Amount FROM InvoiceTaggingWithLCDetail Group By InvoiceTaggingWithLCMasterId) ITLC ON m.Id=ITLC.InvoiceTaggingWithLCMasterId
									LEFT JOIN SCS.Currency AS c ON c.Id = m.CurrencyId
									LEFT JOIN HKP.Party AS p ON p.Id = m.PartyID
									LEFT JOIN PurchaseLC AS pl ON pl.Id = m.PurchaseLcId
									LEFT JOIN [TRN].[Voucher] AS V ON V.Id=m.VoucherId
									WHERE m.PlantId = '" + PlantId + @"'
										AND m.CompanyGroupId = '" + CompanyGroupId + @"'
										AND m.companyId = '" + CompanyId + "' ";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

    }
}
