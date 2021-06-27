using Library.Crosscutting.Security;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.OrderManagement.Production
{
    public class FGValuation
	{

        SqlRepository _sqlRepository;
        public FGValuation()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> GetValuationData(string fromDate, string toDate)
        {
            try
            {
                var sql = @"
                            SELECT ISC.POId, SUM(CONVERT(decimal(18,0),ISC.NetWeight)) FPQty,ISNULL(R.CostPerUnit,0) CostPerUnit, FPAmount =ISNULL(SUM(CONVERT(decimal(18,0),ISC.NetWeight))*R.CostPerUnit,0)
                            ,W.Qty WIPQty, WIPAmount=W.Qty*ISNULL(R.CostPerUnit,0)
                              FROM [dbo].[ItemScan] ITS
                              INNER JOIN [dbo].[ItemScanChild] ISC ON ISC.MasterId=ITS.Id
                              LEFT JOIN 
                              (  
                            Select POD.ProductionOrderId,SUM(QB.GrossConsumption*QB.MaterialCostPerUnit) AS CostPerUnit
                            from TRN.ProductionOrderDetail POD
                            INNER JOIN TRN.SalesOrder SO ON SO.Id=POD.SalesOrderId AND SO.Id=(Select top 1 SalesOrderId from TRN.ProductionOrderDetail XPD Where XPD.ProductionOrderId=POD.ProductionOrderId)
                            INNER JOIN TRN.MasterOrderItem MOI ON MOI.Id=SO.MasterOrderItemId AND MOI.Id =
                            (
                            Select top 1 MasterOrderItemId from dbo.QuickBOQ XQ Where XQ.MasterOrderItemId=SO.MasterOrderItemId
                            )
                            INNER JOIN dbo.QuickBOQ QB ON QB.MasterOrderItemId=MOI.Id
                            GROUP BY POD.ProductionOrderId
                              ) R ON R.ProductionOrderId=ISC.POId
                              LEFT JOIN 
                              (
                              SELECT ISC.POId, SUM(CONVERT(decimal(18,0),ISC.NetWeight)) Qty
                              FROM [dbo].[ItemScan] ITS
                              INNER JOIN [dbo].[ItemScanChild] ISC ON ISC.MasterId=ITS.Id  
                              Where WorkDate <='" + toDate + @"'
                              GROUP BY ISC.POId
                              ) W ON W.POId=ISC.POId
                              Where WorkDate between '" + fromDate + @"' AND '"+ toDate + @"'
                              GROUP BY ISC.POId,R.CostPerUnit,W.Qty";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

    }
}
