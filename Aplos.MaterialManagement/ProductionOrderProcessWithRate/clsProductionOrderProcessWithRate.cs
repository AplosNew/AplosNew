using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.MaterialManagement.ProductionOrderProcessWithRate
{
    public class clsProductionOrderProcessWithRate
    {
        ISqlRepository _sqlRepository;
        public clsProductionOrderProcessWithRate()
        {
            _sqlRepository = new SqlRepository();
        }
        public IEnumerable<object> GetSKU(string ProcessId)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"select c.Id Value,c.UserName Text from [TRN].[ProductionOrderProcessSet] p
                            left join MST.MaterialMasterCharacteristics m on m.MaterialMasterId=p.MaterialMasterId
                            left join HKP.Characteristics c on c.Id=m.CharacteristicsId
                            where p.ProcessId='" + ProcessId + "'";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function
    }
}
