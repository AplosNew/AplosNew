using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
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
                var str = @"SELECT plm.Id PackingId,p.UserName Customer,p.id CustomerId,c.Code Currency,c.Id CurrencyId,FORMAT (plm.AddedDate,'dd-MMM-yyyy')AddedDate
                                            FROM PIPackingListMaster AS plm
                                            LEFT JOIN PIMaster AS pm ON pm.Id=plm.PImasterId
                                            LEFT JOIN hkp.Party p on p.Id = pm.CustomerId
                                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=pm.CurrencyId";
                //var str = @"SELECT Convert(bit,0) Active,PackingId, format(Date,'dd-MMM-yyyy') as AddedDate, format(InactiveDate,'dd-MMM-yyyy') as InActiveDate, p.UserName as Customer, ms.UserName as StorageLoc , e.EmployeeName as ByWhom,
                //            ei.Employeename as DRespPerson, en.UserName as Entity, pk.Remarks,pk.CustomerId,pk.EntityId,CP.CurrencyId,C.Code AS Currency 
                //            FROM TRN.Packing pk
                //            LEFT JOIN hkp.Party p on p.Id = pk.CustomerId
                //            LEFT JOIN dbo.EmployeeInformation e on e.SystemId = pk.ByWhom
                //            LEFT JOIN dbo.EmployeeInformation ei on ei.SystemId = pk.DispatchResponsiblePersonId
                //            LEFT JOIN hkp.MaterialStorage ms on ms.Id = pk.StorageLocId
                //            LEFT JOIN org.Entity en on en.Id = pk.EntityId
                //            LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=P.Id
                //            LEFT JOIN [SCS].[Currency] AS C ON C.Id=CP.CurrencyId
                //            WHERE Pk.PackingId NOT IN (Select PackingId from dbo.SalesPacking)
                //            AND pk.PackingId IN(Select distinct pli.PackingId from trn.PackingLineItem pli
                //            left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
                //            left join ItemScanChild sc on sc.PackingId = pol.Id
                //            where ISNULL(sc.RefNo,'')<>'')";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> GetPackingSOData(string PackingId)
        {
            try
            {
                var _sql = @"SELECT p.Id AS PIMaterialId,MGM.UserName AS MaterialGroup, p.[Description],FORMAT(p.DeliveryDate,'dd-MMM-yyyy')DeliveryDate,p.Quantity,uom.UserName AS UOM,p.Rate, p.Amount
                                          FROM PIPackingListMaster AS PM
                                        INNER JOIN PIPackingListMaterial AS M ON pm.Id=m.PIPackingListMasterId
                                        INNER JOIN PIMaterial AS p ON p.Id=m.PIMaterialId
                                        INNER JOIN mst.MaterialGroupMaster AS mgm ON mgm.Id=p.MaterialGroupMasterId
                                        INNER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=p.UoMId
                                        WHERE PM.Id " + PackingId + "";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void save(List<Dictionary<string, object>> PackingData)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster; DataRow dr;
                string ids = "";
                bplib.clsGenID objGenID = null;
                objGenID = new bplib.clsGenID();
                objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "SalesPacking", out string TempId);
                int count = 0;



                for (int i = 0; i < PackingData.Count; i++)
                {
                    if (ids == "")
                    {
                        ids = "'" + PackingData[i]["Id"] + "'";
                    }
                    else
                    {
                        ids += ",'" + PackingData[i]["Id"] + "'";
                    }
                }

                string sql = "SELECT * FROM [dbo].[SalesPacking] WHERE Id in (" + ids + ") ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                foreach (var item in PackingData)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = "PIPackingListMasterId = '" + item["Id"] + "'  ";
                    if (dsMaster.Tables[0].DefaultView.Count == 0)
                    {
                        count++;
                        dr = dsMaster.Tables[0].NewRow();
                        dr["Id"] = "S" + count + TempId;
                        dr["PIPackingListMasterId"] = item["Id"];

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dsMaster.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["PIPackingListMasterId"] = item["PIPackingListMasterId"];

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();

                    }
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
