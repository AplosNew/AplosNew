using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Library.Data.Sql;
using OTSBD;

namespace Library.OrderManagement.Production
{
    public class DailyProductionData
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public DailyProductionData()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }     

       
      
        public IEnumerable<object> GetWk(string AddedBy)
        {
            try
            {
                var Sql = @"select distinct ope.WorkCenterId as Value,wk.UserName as Text from dbo.OperationWiseEmployee ope 
                            left join scs.WorkCenterMaster wk on ope.WorkCenterId=wk.Id
                            where ope.AddedBy='" + AddedBy + "'";
                return _sqlRepository.GetDataCollection(Sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetOP(string AddedBy, string WkId)
        {
            try
            {
                var Sql = @"select distinct ope.OperationVariationId as Value,ov.UserName as Text from dbo.OperationWiseEmployee ope 
                left join mst.OperationVariation ov on ope.OperationVariationId=ov.Id
                where ope.AddedBy='" + AddedBy + "' and ope.WorkCenterId='" + WkId + "'";
                return _sqlRepository.GetDataCollection(Sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
              
        
    }

}


    public class DailyProduction
    {

        #region Scalar Properties

        public string Id { get; set; }
        public DateTime? Date { get; set; }
        public decimal Qty { get; set; }
        public string ShiftId { get; set; }
        public string PeriodId { get; set; }
        public string Remarks { get; set; }
        public string ProcessId { get; set; }
        public string ProductionOrderId { get; set; }
        public string WorkCenterId { get; set; }
        public string OperationVariationId { get; set; }
        public string EmployeeId { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

    public string AddedBy { get; set; }
    public DateTime AddedDate { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string AddedFromIP { get; set; }
    public string UpdatedFromIP { get; set; }

    #endregion Audit Properties

    }

    public class operationwise
    {
        #region Scalar Properties
        public string Id { get; set; }
        public DateTime? EntryDate { get; set; }
        public string WorkCenterId { get; set; }
        public string OperationVariationId { get; set; }
        public string EmployeeId { get; set; }

        #endregion Scalar Properties 

        #region Audit Properties

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties

    }




