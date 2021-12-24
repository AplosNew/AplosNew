using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.OrderManagement.Packing
{
    public class clsPIInvoice
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
        public clsPIInvoice()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        public IEnumerable<object> GetPackingData()
        {
            try
            {
                var str = @"SELECT Convert(bit,0) Active,PackingId, format(Date,'dd-MMM-yyyy') as AddedDate, format(InactiveDate,'dd-MMM-yyyy') as InActiveDate, p.UserName as Customer, ms.UserName as StorageLoc , e.EmployeeName as ByWhom,
                            ei.Employeename as DRespPerson, en.UserName as Entity, pk.Remarks,pk.CustomerId,pk.EntityId,CP.CurrencyId,C.Code AS Currency 
                            FROM TRN.Packing pk
                            LEFT JOIN hkp.Party p on p.Id = pk.CustomerId
                            LEFT JOIN dbo.EmployeeInformation e on e.SystemId = pk.ByWhom
                            LEFT JOIN dbo.EmployeeInformation ei on ei.SystemId = pk.DispatchResponsiblePersonId
                            LEFT JOIN hkp.MaterialStorage ms on ms.Id = pk.StorageLocId
                            LEFT JOIN org.Entity en on en.Id = pk.EntityId
                            LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=P.Id
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=CP.CurrencyId
                            WHERE Pk.PackingId NOT IN (Select PackingId from dbo.SalesPacking)
                            AND pk.PackingId IN(Select distinct pli.PackingId from trn.PackingLineItem pli
                            left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
                            left join ItemScanChild sc on sc.PackingId = pol.Id
                            where ISNULL(sc.RefNo,'')<>'')";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

    }
}
