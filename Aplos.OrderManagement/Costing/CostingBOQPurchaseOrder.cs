using ConnectionManager.DAL;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Parties;
using Library.Service.Enums;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.OrderManagement.Costing
{
    public class CostingBOQPurchaseOrder
    {
        CustomIdentity identity;
        SqlRepository _sqlRepository;
        public CostingBOQPurchaseOrder()
        {
            _sqlRepository = new SqlRepository();
            identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        }


        public List<Dictionary<string, object>> GetBOMList(string column, string value, Dictionary<string, DateTime> Date)
        {
            try
            {


                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string fromDateFilter = "";
                string sql = "";
                if (Date != null && Date.Count > 0)
                {
                    if (Convert.ToDateTime(Date["FromDate"].ToString()) > Convert.ToDateTime(Date["ToDate"].ToString()))
                        throw new Exception("To date is greater than from date");

                    fromDateFilter = " AND cb.AddedDate between '" + Date["FromDate"].ToString("dd-MMM-yyyy") + @"' AND '" + Date["ToDate"].ToString("dd-MMM-yyyy") + @"'";

                    sql = @"select top 100 * from (
                                        SELECT  convert(bit,0) AS Checked,QS.CostingItemId,p.UserName AS Customer,QS.VendorCode,cb.Id AS BOQRef,FORMAT(cb.AddedDate,'dd-MMM-yyyy') AS BOMCreationDate,bi.BOMMaterialRefNo,ci.UserName AS CostingItem,qs.NoOfBOQItems,
                                       qs.RequiredQty,uom.UserName AS UOM,0 AS OrderQty,qs.RequiredQty-0 AS BalanceToOrderQty,cb.Remarks,ci.[Sequence],
                                       QS.VendorId,QS.Vendor,QS.Material,qs.Article,qs.MaterialMasterId
                                 FROM CostingBOQMaster AS cb
                                JOIN (select distinct CostingBOQMasterId,BI.BOMMaterialRefNo, BI.CostingItemId,BI.OrderProcurementCostingDirectMaterialId--very important because of multiple SO associated with single item
                                        from CostingBOQItems AS BI) AS BI ON cb.Id=bi.CostingBOQMasterId
                                LEFT JOIN hkp.CostingItem AS ci ON ci.Id=bi.CostingItemId
                                JOIN (SELECT boq.CostingItemId,boq.CostingBOQMasterId,boq.VendorId,p.UserName AS Vendor,P.Code AS VendorCode,
                                boq.MaterialMasterId, mm.UserName AS Material,mma.StandardName AS Article,
                                COUNT(*) AS NoOfBOQItems,SUM(RequiredQty) AS RequiredQty  
                                      FROM boq 
                                      LEFT JOIN hkp.Party AS p ON p.Id=boq.VendorId
                                      LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=boq.MaterialMasterId
                                      LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=boq.ArticleId
                                      GROUP BY P.Code, boq.MaterialMasterId,mm.UserName,mma.StandardName, boq.VendorId,p.UserName, boq.CostingItemId,boq.CostingBOQMasterId
                                )
                                AS QS ON qs.CostingItemId=bi.CostingItemId AND qs.CostingBOQMasterId=cb.Id
                                LEFT JOIN scs.UnitOfMeasurement AS uom ON uom.Id=ci.UnitOfMeasurementId
                                LEFT JOIN hkp.Party AS p ON p.Id=cb.CustomerId 
                        WHERE 1=1 " + fromDateFilter + @"
                            ) AS TEMP  ORDER BY BOQRef DESC, Sequence ASC";


                    return _sqlRepository.GetDataCollection(sql, null);

                }


                sql = @"select top 100 * from (
                                        SELECT  convert(bit,0) AS Checked,QS.CostingItemId,p.UserName AS Customer,QS.VendorCode,cb.Id AS BOQRef,FORMAT(cb.AddedDate,'dd-MMM-yyyy') AS BOMCreationDate,bi.BOMMaterialRefNo,ci.UserName AS CostingItem,qs.NoOfBOQItems,
                                       qs.RequiredQty,uom.UserName AS UOM,0 AS OrderQty,qs.RequiredQty-0 AS BalanceToOrderQty,cb.Remarks,ci.[Sequence],
                                       QS.VendorId,QS.Vendor,QS.Material,qs.Article,qs.MaterialMasterId
                                 FROM CostingBOQMaster AS cb
                                JOIN (select distinct CostingBOQMasterId,BI.BOMMaterialRefNo, BI.CostingItemId,BI.OrderProcurementCostingDirectMaterialId--very important because of multiple SO associated with single item
                                        from CostingBOQItems AS BI) AS BI ON cb.Id=bi.CostingBOQMasterId
                                LEFT JOIN hkp.CostingItem AS ci ON ci.Id=bi.CostingItemId
                                JOIN (SELECT boq.CostingItemId,boq.CostingBOQMasterId,boq.VendorId,p.UserName AS Vendor,P.Code AS VendorCode,
                                boq.MaterialMasterId, mm.UserName AS Material,mma.StandardName AS Article,
                                COUNT(*) AS NoOfBOQItems,SUM(RequiredQty) AS RequiredQty  
                                      FROM boq 
                                      LEFT JOIN hkp.Party AS p ON p.Id=boq.VendorId
                                      LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=boq.MaterialMasterId
                                      LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=boq.ArticleId
                                      GROUP BY P.Code, boq.MaterialMasterId,mm.UserName,mma.StandardName, boq.VendorId,p.UserName, boq.CostingItemId,boq.CostingBOQMasterId
                                )
                                AS QS ON qs.CostingItemId=bi.CostingItemId AND qs.CostingBOQMasterId=cb.Id
                                LEFT JOIN scs.UnitOfMeasurement AS uom ON uom.Id=ci.UnitOfMeasurementId
                                LEFT JOIN hkp.Party AS p ON p.Id=cb.CustomerId 
                        WHERE 1=1 
                            ) AS TEMP WHERE 1=1 AND " + strkey + @" ORDER BY BOQRef DESC, Sequence ASC";


                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public List<Dictionary<string, object>> GetPartyInformationById(string VendorId)
        {
            try
            {
                string sql = @"SELECT  P.Id AS PartyId, P.Code AS PartyCode, P.UserName AS PartyName, P.Id, P.Code, P.UserName, CP.PartyType, CP.PartyAccountGroupId, PAG.Code AS PartyAccountGroupCode, PAG.UserName AS PartyAccountGroupName, CP.CurrencyId, C.Code AS CurrencyCode, C.[Name] AS CurrencyName
                                    , CP.PaymentTermId, PT.Code AS PaymentTermCode, PT.UserName AS PaymentTermName, CP.IsPaymentTermChangeable
                                    , NULL AS InvoicingPartyPlantId, NULL AS DeliveryPartyPlantId, CO.Code AS CountryCode, CO.UserName AS CountryName, S.Code AS StateCode, S.UserName AS StateName
                                    , RGL.ReconciliationGLId, RGL.ReconciliationGLCode, RGL.ReconciliationGLName
                                    , RGL.ReconciliationBudgetId, RGL.ReconciliationBudgetCode, RGL.ReconciliationBudgetName
                                    , RGL.ReconciliationActivityId, RGL.ReconciliationActivityCode, RGL.ReconciliationActivityName
                                    , DGL.DownPaymentGLId, DGL.DownPaymentGLCode, DGL.DownPaymentGLName
                                    , DGL.DownPaymentBudgetId, DGL.DownPaymentBudgetCode, DGL.DownPaymentBudgetName
                                    , DGL.DownPaymentActivityId, DGL.DownPaymentActivityCode, DGL.DownPaymentActivityName
                                    , SGL.SuspenseGLId, SGL.SuspenseGLCode, SGL.SuspenseGLName
                                    , SGL.SuspenseBudgetId, SGL.SuspenseBudgetCode, SGL.SuspenseBudgetName
                                    , SGL.SuspenseActivityId, SGL.SuspenseActivityCode, SGL.SuspenseActivityName
                                    , CP.TaxApplicable, CP.IsTaxApplicableChangeable
									, (SELECT COUNT(Id) FROM [HKP].[PartyPlant] WHERE PartyId=P.Id) AS TotalPartyPlant
                                    FROM [HKP].[Party] AS P
                                    LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=P.Id
                                    LEFT JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=CP.PartyAccountGroupId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=CP.CurrencyId
                                    LEFT JOIN [MST].[PaymentTerm] AS PT ON PT.Id=CP.PaymentTermId
                                    LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=P.AddressMasterId
									LEFT JOIN [SCS].[Country] AS CO ON CO.Id=AM.CountryId
									LEFT JOIN [SCS].[State] AS S ON S.Id=AM.StateId
                                    LEFT JOIN(
                                    SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS ReconciliationGLId, GL.AccountCode AS ReconciliationGLCode, GL.UserName AS ReconciliationGLName
                                    , CPGL.BudgetMasterId AS ReconciliationBudgetId, B.Code AS ReconciliationBudgetCode, B.UserName AS ReconciliationBudgetName
                                    , CPGL.ActivityId AS ReconciliationActivityId, A.Code AS ReconciliationActivityCode, A.UserName AS ReconciliationActivityName
                                    FROM [HKP].[CompanyPartyGL] AS CPGL
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
                                    WHERE CPGL.PartyGLType='" + PartyGLType.ReconciliationGL + @"'
                                    ) AS RGL ON RGL.CompanyPartyId=CP.Id
                                    LEFT JOIN(
                                    SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS DownPaymentGLId, GL.AccountCode AS DownPaymentGLCode, GL.UserName AS DownPaymentGLName
                                    , CPGL.BudgetMasterId AS DownPaymentBudgetId, B.Code AS DownPaymentBudgetCode, B.UserName AS DownPaymentBudgetName
                                    , CPGL.ActivityId AS DownPaymentActivityId, A.Code AS DownPaymentActivityCode, A.UserName AS DownPaymentActivityName
                                    FROM [HKP].[CompanyPartyGL] AS CPGL
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
                                    WHERE CPGL.PartyGLType='" + PartyGLType.DownPaymentGL + @"'
                                    ) AS DGL ON DGL.CompanyPartyId=CP.Id
                                    LEFT JOIN(
										SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS SuspenseGLId, GL.AccountCode AS SuspenseGLCode, GL.UserName AS SuspenseGLName
										, CPGL.BudgetMasterId AS SuspenseBudgetId, B.Code AS SuspenseBudgetCode, B.UserName AS SuspenseBudgetName
										, CPGL.ActivityId AS SuspenseActivityId, A.Code AS SuspenseActivityCode, A.UserName AS SuspenseActivityName
										FROM [HKP].[CompanyPartyGL] AS CPGL
										LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
										LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
										LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
										LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
										WHERE CPGL.PartyGLType='" + PartyGLType.SuspenseGL + @"'
                                    ) AS SGL ON SGL.CompanyPartyId=CP.Id
                                    WHERE P.Archive=0 AND P.Active=1 AND P.Id='" + VendorId + "'";
             
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
               
            }

            return null;
        }



    }
}
