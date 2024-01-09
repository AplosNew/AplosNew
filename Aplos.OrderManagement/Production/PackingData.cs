using System;
using System.Collections.Generic;
using Library.Data.Sql;
using System.Data;
using OTSBD;
using Library.Crosscutting.Security;
using System.Threading;
using System.Linq;


namespace Library.OrderManagement.Production
{
    public class PackingData
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
        public PackingData()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        public void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
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
        public void CreateAll(Dictionary<string, object> Packingdata, Dictionary<string, object> PackingLineItemdata, Dictionary<string, object> POLotRefData, List<Dictionary<string, object>> Cartons, List<Dictionary<string, object>> POLotCollection, int lastIndex, out int ll)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ///
                string TableName = "trn.Packing";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where PackingId='" + Packingdata["PackingId"] + "'", out dsMaster, false, "1");


                DateTime now = DateTime.Now;
                Packingdata["Date"] = now;
                string DateId = ((DateTime.Now.Year).ToString()).Substring(2);
                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    Packingdata["PackingId"] = DateId + _Id.ToString().PadLeft(4, '0');
                    AddNewRow(dsMaster.Tables[0], Packingdata);
                }

                #endregion data update


                string TableName1 = "trn.PackingLineItem";
                DataSet dsMaster1;
                ConnectionManager.DAL.ConManager con1 = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where  SOId ='" + PackingLineItemdata["SOId"] + "'  and PackingId = '" + Packingdata["PackingId"] + "'", out dsMaster1, false, "1");
                if (dsMaster1.Tables[0].Rows.Count > 0)
                    throw new Exception("Same PackingLineItem With Same SO already exists!!!");


                int ind = lastIndex;
                #region data update
                if (dsMaster1.Tables[0].Rows.Count == 0)
                {
                    ind++;
                    PackingLineItemdata["PackingId"] = Packingdata["PackingId"];
                    PackingLineItemdata["PackingLineItemId"] = Packingdata["PackingId"] + ind.ToString().PadLeft(2, '0');
                    AddNewRow(dsMaster1.Tables[0], PackingLineItemdata);
                }

                ll = ind;
                #endregion data update



                string tt = "trn.PoLotReference";
                DataSet dsMastersad = new DataSet();

                ConnectionManager.DAL.ConManager con2er = new ConnectionManager.DAL.ConManager("1");
                con2er.OpenDataSetThroughAdapter("select * from " + tt + " where Id='" + POLotCollection[0]["Id"] + "'", out dsMastersad, false, "1");
                int indexa = 0;
                for (int i = 0; i < POLotCollection.Count; i++)
                {
                    Dictionary<string, object> jj = POLotCollection[i];

                    indexa++;
                    jj["PackingLineItemId"] = PackingLineItemdata["PackingLineItemId"];
                    jj["Id"] = PackingLineItemdata["PackingLineItemId"] + indexa.ToString().PadLeft(2, '0');

                    AddNewRow(dsMastersad.Tables[0], jj);
                    if (Cartons != null)
                    {
                        for (int j = 0; j < Cartons.Count; j++)
                        {

                            if (Cartons[j]["LotNo"].ToString().Trim() == jj["LotNo"].ToString().Trim() && Cartons[j]["ProductCode"].ToString().Trim() == jj["ProductCode"].ToString().Trim() && Cartons[j]["PO"].ToString().Trim() == jj["PONo"].ToString().Trim())
                            {
                                var sqls = @"Update dbo.ItemScanChild Set PackingId = '" + jj["Id"].ToString() + "' , UpdatedBy = '" + identity.Name + "' ,BookedDate = GETDATE(), Booked = 1  where RefNo IN(" + Cartons[j]["RefNo"] + @")  AND SalesReturnId IS NULL AND Booked=0";

                                ConnectionManager.DAL.ConManager objCone = null;
                                objCone = new ConnectionManager.DAL.ConManager("1");
                                objCone.OpenConnection("1");
                                objCone.BeginTransaction();

                                objCone.ExecuteNonQueryWrapper(sqls, true, "1");
                                objCone.CommitTransaction();
                            }
                        }

                    }

                }

                DataSet dd = dsMastersad;

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsMaster1, dsMastersad);





            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public IEnumerable<object> getLocations()
        {
            try
            {
                var str = @"Select  distinct ToLocation as text, ToStorageLocId as value from mst.MaterialMovementMaster";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public IEnumerable<object> getPackingList()
        {
            try
            {
                var str = @"Select pk.PackingId , pli.PackingLineItemId , pol.LotNo,pol.Id as PoLotRefId ,p.UserName as Customer,  isnull(sum(isc.NetWeight),0) as NetWeight , Count(isc.RefNo) as Cartons,
                            isnull(sum(isc.GWeight),0) as GrossWeight, isnull(sum(pol.PlanQty),0) as PlanQty , isnull(sum(pol.BookQty),0) as BookQty, uom.UserName as UOM
                            from trn.Packing pk
                            left join trn.PackingLineItem pli on pli.PackingId = pk.PackingId
                            left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
                            left join dbo.ItemScanChild isc on isc.PackingId = pol.Id
                            left join hkp.Party p on p.Id = pk.CustomerId
                            left join trn.MasterOrder mo on mo.PartyId = p.Id
                            left join scs.UnitOfMeasurement uom on uom.Id = mo.TotalQtyUOMId
                            left join trn.MasterOrderItem moi on moi.MasterOrderId = mo.Id
                            left join mst.MaterialMaster ma on ma.Id=moi.MaterialMasterId
                            left join trn.ProductDefinition AS pd ON pd.MaterialMasterId=ma.Id
                            left join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId  
                            group by pk.PackingId , p.UserName , pli.PackingLineItemId , pol.Id , uom.UserName , pol.LotNo ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> GetEntity()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var _sql = @"select UserName as Text,Id as Value from org.Entity where CompanyId='" + identity.CompanyId + "'" +
                    " and PlantId='" + identity.PlantId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }
        public IEnumerable<object> getPurposeCategory()
        {
            try
            {
                var _sql = @"SELECT UserName AS Text,Id as Value 
                FROM HKP.MaterialMovementPurpose ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> getClickData(string poid, string productCode)
        {

            try
            {
                var str = @"Select pl.Code, po.id as POId, so.Id as SoId , so.Qty as Qty , moi.Id as ItemId ,PM.UserName as Product,
                            mo.Id as MasterOrderNo , p.Username as Customer, mma.StandardName as ItemArticle
                            from
                            trn.MasterOrder mo
                            left join trn.MasterOrderItem moi on moi.MasterOrderId = mo.Id
                            left join trn.SalesOrder so on so.MasterOrderItemId = moi.Id
                            left join trn.ProductionOrderDetail pod on pod.SalesOrderId = so.Id
                            left join trn.ProductionOrder po on po.Id = pod.ProductionOrderId
                            left join dbo.ProductLibrary pl on pl.Id = moi.ProductLibraryId
                            left join mst.MaterialMaster ma on ma.Id=moi.MaterialMasterId
                            left join trn.ProductDefinition AS pd ON pd.MaterialMasterId=ma.Id
                            left join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                            left join hkp.Party p on p.Id = mo.PartyId
                            LEFT OUTER JOIN [MST].[MaterialMasterArticle] mma ON mma.Id=moi.ArticleId
                            where pl.Code is not null and pl.Code = '" + productCode + @"' and po.Id = '" + poid + @"'
                            ";
                DataTable dt = _sqlRepository.GetDataTable(str);

                dt.Columns.Add("checked", typeof(bool));
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["checked"] = false;
                }
                return Service.Helpers.DataTableExtensions.DataTableToJson(dt);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public IEnumerable<object> getCustomers()
        {
            try
            {
                var str = @"Select distinct p.UserName as username , p.Id as id
                            from trn.MasterOrder mo
                            left join trn.MasterOrderItem moi on moi.MasterOrderId = mo.Id
                            left join trn.SalesOrder so on so.MasterOrderItemId = moi.Id
                            left join hkp.Party p on p.Id = mo.PartyId
                            left join dbo.ProductLibrary pl on pl.Id = moi.ProductLibraryId
                            where mo.OrderStatusId not in ( 'Closed' , 'Cancelled' , 'Hold') and so.OrderStatusId not in ( 'Closed' , 'Cancelled' , 'Hold')
				            and pl.Code !='null'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public IEnumerable<object> getEmployees()
        {
            try
            {
                var str = @"select EmployeeCode , SystemId , EmployeeName from dbo.EmployeeInformation";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getByWhomEmployees()
        {
            try
            {
                var str = @"Select ei.SystemId , ei.EmployeeCode , ei.EmployeeName from SEC.[User] u
                            left join dbo.EmployeeInformation ei on ei.SystemId = u.EmployeeId
                            where u.EmployeeId is not null";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public IEnumerable<object> getStorageLoc()
        {
            try
            {
                var str = @"Select username as Text , Id as Value from hkp.MaterialStorage";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public IEnumerable<object> getEntity()
        {
            try
            {
                var str = @"Select username as Text , Id as Value from org.Entity";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getMOwithCustomers(string Customers)
        {
            try
            {
                string str = @"Select mo.Id as MOId, moi.Id as MasterItemId, mo.MasterOrderNo , mo.OrderStatusId , p.Id as CustomerId, p.UserName as Customer , so.Id as SoNO , so.Qty  from trn.MasterOrder mo 
				left join hkp.Party p on p.Id = mo.PartyId
				left join trn.MasterOrderItem moi on moi.MasterOrderId = mo.id
				left join trn.SalesOrder so on so.MasterOrderItemId = moi.Id
				where p.Id in (" + Customers + @")

                ";
                DataTable dt = _sqlRepository.GetDataTable(str);

                dt.Columns.Add("checked", typeof(bool));
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["checked"] = false;
                }
                return Service.Helpers.DataTableExtensions.DataTableToJson(dt);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getpackingGridOne(string productCode)
        {
            try
            {
                var str = @"Select distinct sc.ProductCode, pl.Code, sc.LotNo , sc.POId as PO , fd.FD , fp.FP, ud.UD , isnull(desp.Despatch,0) as Despatch , isnull(ret.Ret ,0) as [Return] from 
                            dbo.ItemScanChild sc
                            left join dbo.ProductLibrary pl on pl.code = sc.ProductCode
                            left join (
                            Select isc.ProductCode , isc.POId , sum(isc.NetWeight) as FD from
                            dbo.ItemScanChild isc
                            left join dbo.ItemScan isch on isch.Id = isc.LocMasterId
                            --where isch.WorkDate ='ToDate' 
                            group by ProductCode , POId
                            ) fd on fd.ProductCode = sc.ProductCode and fd.POId = sc.POId
                            left join(
                            Select isc.ProductCode , isc.POId , sum(isc.NetWeight) as FP from
                            dbo.ItemScanChild isc 
                            left join dbo.ItemScan isch on isch.Id = isc.LocMasterId
                            --where isch.WorkDate between 'ToDate' and 'FromDate'
                            group by ProductCode , POId
                            ) fp on fp.ProductCode = sc.ProductCode and fp.POId = sc.POId
                            left join(
                            Select isc.ProductCode , isc.POId , sum(isc.NetWeight) as UD from
                            dbo.ItemScanChild isc 
                            left join dbo.ItemScan isch on isch.Id = isc.MasterId
                            --where isch.WorkDate <= 'FromDate' 
                            group by ProductCode , POId
                            ) ud on ud.ProductCode = sc.ProductCode and ud.POId = sc.POId
                            left join(
                            Select isc.ProductCode , isc.POId , sum(isc.NetWeight) as Despatch from
                            dbo.ItemScanChild isc 
                            left join dbo.ItemScan isch on isch.Id = isc.MasterId
                            where isc.IsDespatch = 1
                            group by ProductCode , POId
                            ) desp on desp.ProductCode = sc.ProductCode and desp.POId = sc.POId
                            left join (
                            Select isc.ProductCode , isc.POId, sum(isc.NetWeight) as Ret from
                            dbo.ItemScanChild isc 
                            left join dbo.ItemScan isch on isch.Id = isc.MasterId
                            left join hkp.MaterialMovementPurpose mvp on mvp.Id = isch.PurposeId
                            where mvp.UserName = 'Return'
                            group by ProductCode, POId
                            )as ret on ret.ProductCode = sc.ProductCode and ret.POId = sc.POId
                            where sc.ProductCode in (" + productCode + @")
				
				";

                DataTable dt = _sqlRepository.GetDataTable(str);


                dt.Columns.Add("checked", typeof(bool));
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["checked"] = false;
                }
                return Service.Helpers.DataTableExtensions.DataTableToJson(dt);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getCartonsDetails(string LotNo, string ProductCode, string PO, out List<Dictionary<string, object>> dts) // , string Qty , out double quant
        {
            try
            {
                var str = @"Select LotNo , RefNo , cast(NetWeight as decimal(18,2)) as NetWeight , GWeight ,ProductCode , POId 
                            from dbo.ItemScanChild S
                            LEFT JOIN MST.MaterialMovementMaster R ON R.ID = S.LocMasterId
							LEFT JOIN HKP.MaterialMovementPurpose PURP ON PURP.Id = R.PurposeId
                            where s.LotNo = '" + LotNo + @"' and s.ProductCode = '" + ProductCode + @"' and s.POId = '" + PO + @"'  and
							isnull(s.booked,0) = 0 AND isnull(PURP.IsInventoryOut,0) = 0
                            --and InventoryReceiveDetailId is not null";
                DataTable dt = _sqlRepository.GetDataTable(str);
                var str1 = @"Select LotNo , RefNo , cast(NetWeight as decimal(18,2)) as NetWeight , GWeight ,ProductCode , POId from dbo.ItemScanChild where LotNo = '" + LotNo + "' and ProductCode = '" + ProductCode + @"' and POId = '" + PO + @"' and Booked = 1 and IsDespatch = 0";
                DataTable dt2 = _sqlRepository.GetDataTable(str1);
                dts = Library.Service.Helpers.DataTableExtensions.DataTableToJson(dt2);
                dt.Columns.Add("checked", typeof(bool));

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["checked"] = false;
                }




                return Service.Helpers.DataTableExtensions.DataTableToJson(dt);
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        public IEnumerable<object> getPoLotReference(string productCode, string toDispatch, string PO, string FromDate, string ToDate)
        {
            try
            {
                //          var str = @" Select sc.ProductCode , sc.POId as PO, sc.LotNo , isnull(plann.PlanQty,0) as PlannedQty , isnull(StockQty.StockQty,0)  as StockQty , isnull(desp.Despatch,0) as Despatch , isnull(bb.BookQty,0) as BookedQty,
                //                  (Case when bb.BookQty >plann.PlanQty then (isnull(StockQty.StockQty,0) - isnull(bb.BookQty,0)) else (isnull(StockQty.StockQty,0)  - isnull(plann.PlanQty,0)) end) as Available
                //                 , PO.Qty POQty, PS.StandardName POStatus , PORemainingQty = SUM(SC.NetWeight) - PO.Qty
                //                  from
                //                  dbo.ItemScanChild sc
                //                  left join trn.POLotReference pol  on pol.Id = sc.PackingId
                //                  left join TRN.ProductionOrder PO on PO.Id = SC.POId
                //left join HKP.ProductionStatus PS on PS.Id = PO.ProductionStatusId
                //                  left join dbo.ItemScan isch on isch.Id = sc.MasterId
                //left join HKP.MaterialMovementPurpose PM on PM.Id = isch.PurposeId
                //                  left join trn.POLotReference por  on por.Id = sc.PackingId
                //                  left join(
                //                  Select isc.ProductCode , isc.POId , isc.LotNo , sum(isc.NetWeight) as StockQty from
                //                  dbo.ItemScanChild isc 
                //                  left join dbo.ItemScan isch on isch.Id = isc.MasterId
                //                  left join trn.POLotReference pol on pol.Id = isc.PackingId
                //                  where isch.WorkDate <= '" + ToDate + @"'
                //                  and isc.IsDespatch = 0 and isc.Booked = 0
                //                  --and isc.InventoryReceiveDetailId is not null
                //                   group by isc.ProductCode , POId , isc.LotNo
                //                  ) StockQty on StockQty.ProductCode = sc.ProductCode and StockQty.POId = sc.POId and StockQty.LotNo = sc.LotNo
                //                  left join(
                //                  Select isc.ProductCode , isc.POId , isnull(sum(isc.NetWeight),0) as Despatch from
                //                  dbo.ItemScanChild isc 
                //                  left join dbo.ItemScan isch on isch.Id = isc.MasterId
                //                  where isc.IsDespatch = 1 and isch.WorkDate <= '" + ToDate + @"'
                //                  group by ProductCode , POId
                //                  ) desp on desp.ProductCode = sc.ProductCode and desp.POId = sc.POId
                //                  left join (
                //                  Select isc.ProductCode , isc.POId ,isc.LotNo, isnull(sum(isc.NetWeight),0) as BookQty from
                //                  dbo.ItemScanChild isc 
                //                  left join dbo.ItemScan isch on isch.Id = isc.MasterId
                //                  where isc.Booked = 1 and isch.WorkDate <= '" + ToDate + @"'
                //                  group by ProductCode , POId , LotNo
                //                  ) as bb on  bb.ProductCode = sc.ProductCode and bb.LotNo = sc.LotNo and bb.POId=sc.POId 
                //                  left join(
                //                  Select ProductCode , PONo , LotNo , sum(PlanQty) as PlanQty from trn.POLotReference
                //                  where Status = 'Active'
                //                  group by ProductCode , PONo , LotNo
                //                  ) as plann on plann.ProductCode=sc.ProductCode and plann.PONo = sc.POId and plann.LotNo=sc.LotNo
                //                  where sc.ProductCode = '" + productCode + @"'
                //                  group by sc.ProductCode , sc.POId, sc.LotNo ,StockQty.StockQty,desp.Despatch,bb.BookQty,plann.PlanQty,PO.Qty, PS.StandardName
                //                  ";

                string str = @"Select G.QualityStatus,sc.ProductCode , sc.POId as PO,PS.StandardName POStatus ,PO.Qty POQty ,pack.ProducedQty 
                        ,case when pack.ProducedQty > PO.Qty then 0 else (isnull(PO.Qty,0)-isnull(pack.ProducedQty,0)) end as BalanceQty ,sc.LotNo 
                        ,isnull(plann.PlanQty,0) as PlannedQty , isnull(StockQty.StockQty,0)  as StockQty , isnull(desp.Despatch,0) as Despatch , isnull(bb.BookQty,0) as BookedQty,
                        (Case when bb.BookQty >plann.PlanQty then (isnull(StockQty.StockQty,0) - isnull(bb.BookQty,0)) else (isnull(StockQty.StockQty,0)  - isnull(plann.PlanQty,0)) end) as Available
                       --, PO.Qty POQty, PS.StandardName POStatus , PORemainingQty = SUM(SC.NetWeight) - PO.Qty
                        from
                        dbo.ItemScanChild sc
                        left join trn.POLotReference pol  on pol.Id = sc.PackingId

                        left join(
                        Select isc.ProductCode , isc.POId , isc.LotNo , sum(isc.NetWeight) as StockQty from
                        dbo.ItemScanChild isc 
                        left join dbo.ItemScan isch on isch.Id = isc.MasterId
                        left join trn.POLotReference pol on pol.Id = isc.PackingId
                        where isch.WorkDate <= '" + ToDate + @"'
                        and isc.IsDespatch = 0 and isc.Booked = 0
                        --and isc.InventoryReceiveDetailId is not null
                         group by isc.ProductCode , POId , isc.LotNo
                        ) StockQty on StockQty.ProductCode = sc.ProductCode and StockQty.POId = sc.POId and StockQty.LotNo = sc.LotNo
                        left join 

                        (select POId,sum(NetWeight) ProducedQty from ItemScanChild ISC
                        left join ItemScan ISM on ISM.Id = ISC.MasterId
                        where ISM.WorkDate <= '2023-07-05' group by POId) pack on pack.POId = sc.POId

                        left join(
                        Select isc.ProductCode , isc.POId , isnull(sum(isc.NetWeight),0) as Despatch from
                        dbo.ItemScanChild isc 
                        left join dbo.ItemScan isch on isch.Id = isc.MasterId
                        where isc.IsDespatch = 1 and isch.WorkDate <= '" + ToDate + @"'
                        group by ProductCode , POId
                        ) desp on desp.ProductCode = sc.ProductCode and desp.POId = sc.POId
                        left join (
                        Select isc.ProductCode , isc.POId ,isc.LotNo, isnull(sum(isc.NetWeight),0) as BookQty from
                        dbo.ItemScanChild isc 
                        left join dbo.ItemScan isch on isch.Id = isc.MasterId
                        where isc.Booked = 1 and isch.WorkDate <= '" + ToDate + @"'
                        group by ProductCode , POId , LotNo
                        ) as bb on  bb.ProductCode = sc.ProductCode and bb.LotNo = sc.LotNo and bb.POId=sc.POId 
                        left join(
                        Select ProductCode , PONo , LotNo , sum(PlanQty) as PlanQty from trn.POLotReference
                        where Status = 'Active'
                        group by ProductCode , PONo , LotNo
                        ) as plann on plann.ProductCode=sc.ProductCode and plann.PONo = sc.POId and plann.LotNo=sc.LotNo
                       
                       left join trn.ProductionOrder PO on PO.Id = sc.POId
                       left join hkp.ProductionStatus PS on PS.Id = PO.ProductionStatusId

LEFT JOIN (
					   select (case when sum(Convert(Int,Z.RejectValue)) > 0  then 'Reject'
when sum(Convert(Int,Z.FailValue)) > 0  then 'Fail'
when sum(Z.EntryMissing) > 0  then 'Pending'
else 'Pass' end) QualityStatus,
Z.PONo,Z.LotNumber

from (select distinct M.ProductionOrderId PONo,isnull(QCData.LotNumber,M.LotNumber) LotNumber
,QCData.PassValue,QCData.FailValue,QCData.RejectValue,
(Case When (QCData.Value is null or QCData.Value = '0') then 1 else 0 end) EntryMissing
from (Select  P.*,CP.IssueId,CP.ParameterId from (select Distinct PS.ProductionOrderId,PS.LotNumber,1 PlanSet
from TRN.ProductionSummary PS
left join trn.ProductionOrder PO on PO.Id=PS.ProductionOrderId

) P
inner Join (select QMP.QMID IssueId,QMP.Id ParameterId,1 as PlanSet,PR.UserName Process,QMP.ProcessId
 from MST.QualityManagementParameterItem QMP
 left join MST.QualityManagementMaster QMM on QMM.Id=QMP.QMID
 left join Hkp.ParameterMaster PM on PM.Id=QMP.ParameterId
 left join SCS.UnitOfMeasurement UOM on UOM.Id=QMP.UOMId
 left join hkp.Process PR on  PR.Id=QMP.ProcessId
 where CustomerParameter = 1) CP on CP.PlanSet=P.PlanSet) M
 left join (select QC.IssueId,QMM.UserName IssueName,QCD.ItemId ParameterId,PM.UserName ParameterName,QC.LotNumber,QC.ProductionOrderId,
 QCD.Value,QGD.GradeName,QCD.Remarks ParameterRemark,QAT.ActionToBeTakenName,EI.EmployeeName ResponsiblePerson,
 format(QC.AddedDate,'dd-MMM-yyyy') QCDate,format(QCD.AddedDate,'dd-MMM-yyyy') QCDDate,QC.Id HeaderId,QCD.Id ChildId,QGD.IsPassValue PassValue,QGD.IsFailValue FailValue,QGD.IsRejectValue RejectValue,
 (case when QCD.GradeId is null then 1 end) FailGrade,
 (case when (QGD.IsFailValue <> 0 and QCD.Status not in ('Close','Complete')) then 1 else 0 end) ToClose,
 (case when (QGD.IsFailValue <> 0 and QCD.Status not in ('Complete')) then 1 else 0 end) ToConfirm,QC.ProcessId
 from TRN.QualityControlDetails QCD
 left join TRN.QualityControl QC on QC.Id=QCD.QCId
 left join MST.QualityManagementMaster QMM on QMM.Id=QC.IssueId
 left join MST.QualityManagementParameterItem QMP on QMP.Id=QCD.ItemId
 left join Hkp.ParameterMaster PM on PM.Id=QMP.ParameterId
 left join MST.QualityGradeDetails QGD on QGD.Id=QCD.GradeId
 left join MST.QualityActionToBeTakenDetails QAT on QAT.Id=QCD.ActionToBeTaken
 left join EmployeeInformation EI on EI.SystemId=QCD.ResponsiblePersonId
 where QCD.ItemId in (select Id from MST.QualityManagementParameterItem where CustomerParameter = 1)) QCData on 
 QCData.IssueId=M.IssueId and QCData.ParameterId=M.ParameterId and QCData.ProductionOrderId=M.ProductionOrderId and QCData.LotNumber=M.LotNumber)Z 
 where 1=1    
 Group By Z.PONo,Z.LotNumber
					   )G ON G.PONo=sc.POId AND G.LotNumber=sc.LotNo

                        where sc.ProductCode = '" + productCode + @"' and StockQty.StockQty <> 0
                        group by G.QualityStatus,sc.ProductCode , sc.POId, sc.LotNo ,StockQty.StockQty,desp.Despatch,bb.BookQty,plann.PlanQty,PS.StandardName,PO.Qty,pack.ProducedQty";

                DataTable dt = _sqlRepository.GetDataTable(str);

                dt.Columns.Add("checked", typeof(bool));
                dt.Columns.Add("quant", typeof(double));
                dt.Columns.Add("comment", typeof(string));
                dt.Columns.Add("remarks", typeof(string));
                dt.Columns.Add("bookQty", typeof(double));
                dt.Columns.Add("Assigned", typeof(string));



                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["checked"] = false;
                    dt.Rows[i]["quant"] = 0.0;
                    if (dt.Rows[i]["PO"].ToString() == PO)
                    {
                        dt.Rows[i]["Assigned"] = "Assigned";
                    }
                    else
                    {
                        dt.Rows[i]["Assigned"] = "UnAssigned";
                    }

                    if (dt.Rows[i]["ProductCode"].ToString() == productCode && dt.Rows[i]["PO"].ToString() == PO)
                    {
                        dt.Rows[i]["comment"] = "OK";
                    }
                    if (dt.Rows[i]["ProductCode"].ToString() != productCode && dt.Rows[i]["PO"].ToString() == PO)
                    {
                        dt.Rows[i]["comment"] = "Product Code Not Match";
                    }
                    if (dt.Rows[i]["ProductCode"].ToString() == productCode && dt.Rows[i]["PO"].ToString() != PO)
                    {
                        dt.Rows[i]["comment"] = "PO Not Match";
                    }
                    if (dt.Rows[i]["ProductCode"].ToString() != productCode && dt.Rows[i]["PO"].ToString() != PO)
                    {
                        dt.Rows[i]["comment"] = "PO & Product Code Not Match";
                    }
                }

                //Code for the Dispatch

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["quant"] = 0.0;
                    dt.Rows[i]["bookQty"] = 0.0;
                }

                return Service.Helpers.DataTableExtensions.DataTableToJson(dt);
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public IEnumerable<object> getSoFromCustomer(string customer)
        {
            try
            {
                var str = @"Select * from
                (SELECT distinct so.Id as SO
                ,CEILING(SUM((isnull(so.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0))))) SoQty
                , sos.Despatch , (CEILING(SUM((isnull(so.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0))))) - sos.Despatch) as toDespatch
                ,format(so.DeliveryDate,'dd-MMM-yyyy') as DeliveryDate, CAST(so.DeliveryDate as DATE) as Del ,pl.Code  as ProductCode,
                 po.id as PO,uom.UserName as UOM,os.UserName as SOStatus,
               moi.MasterOrderId as MasterOrderNo,pc.UserName as ProductCategory, psc.UserName as ProductSubCategory,moi.Id as ItemId,mma.StandardName as ItemArticle,
                pl.Remarks,ma.Code as MaterialCode,ma.UserName as Material,ma.Id as MaterialId,PM.UserName as Product , prod.UserName as Prod,PM.Id as ProductId,
                pl.Id as ProductLibId, par.UserName as Customer,par.Id as CustomerId,moi.BuyerReferenceNo , moi.OwnReferenceNo
				
                FROM trn.masterorder mo 
				left join trn.MasterOrderItem moi on moi.MasterOrderId = mo.Id
				left join dbo.ProductLibrary pl on pl.Id = moi.ProductLibraryId
				left join trn.SalesOrder so on so.MasterOrderItemId = moi.Id
				left join trn.ProductionOrderDetail podd on podd.SalesOrderId = so.Id
                left join dbo.ProductLibraryAttribute pa on pa.ProductLibraryId=pl.Id
                left join trn.ProductionOrder po on po.Id=podd.ProductionOrderId 
                 left join hkp.ProductionStatus ps on ps.Id=po.ProductionStatusId
                 left join mst.MaterialMaster ma on ma.Id=moi.MaterialMasterId
                 left join trn.ProductDefinition AS pd ON pd.MaterialMasterId=ma.Id
                 left join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
				 left join hkp.ProductCategory pc on pc.Id = pm.ProductCategoryId
				 left join scs.UnitOfMeasurement uom on uom.Id = mo.TotalQtyUOMId
				 left join hkp.OrderStatus os on os.Id = so.OrderStatusId
                left join hkp.Party par on par.Id=mo.PartyId
                left join dbo.ScanItem si on si.Id=pa.ScanItemId     
                left join hkp.Product prod on prod.Id = pm.ProductId
				left join hkp.ProductSubCategory psc on psc.Id = pm.ProductSubCategoryId
				LEFT OUTER JOIN [MST].[MaterialMasterArticle] mma ON mma.Id=moi.ArticleId
				left join (
				Select so.Id, isnull(sum(sm.TransactionQty),0) as Despatch from trn.SalesOrder so 
				left join trn.SalesMaterial sm on sm.SalesOrderId = so.Id
				group by so.Id
				) as sos on sos.Id = so.Id
                where 
				mo.OrderStatusId not in ( 'Closed' , 'Cancelled' , 'Hold') and so.OrderStatusId not in ( 'Closed' , 'Cancelled' , 'Hold')
				 and pl.Code !='null'
				 group by so.Id,sos.Despatch,so.DeliveryDate,pl.Code,po.id,uom.UserName,os.UserName,
				 moi.MasterOrderId,pc.UserName, psc.UserName,moi.Id,mma.StandardName,
                pl.Remarks,ma.Code,ma.UserName,ma.Id,PM.UserName, prod.UserName,PM.Id,
                pl.Id, par.UserName,par.Id,moi.BuyerReferenceNo , moi.OwnReferenceNo
				) as req

       where CustomerId = '" + customer + @"'
				order by Del

				";

                DataTable dt = _sqlRepository.GetDataTable(str);

                dt.Columns.Add("checked", typeof(bool));
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["checked"] = false;
                }
                return Service.Helpers.DataTableExtensions.DataTableToJson(dt);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getSOfromProduct(string column, string value)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                {
                    strkey = column + " like '%" + value + "%'";
                }
                var str = @"Select * from
                (SELECT distinct so.Id as SO, so.Qty as SoQty,format(so.DeliveryDate,'dd-MMM-yyyy') as DeliveryDate, CAST(so.DeliveryDate as DATE) as Del ,pl.Code  as ProductCode,
                 po.id as PO,
               moi.MasterOrderId as MasterOrderNo,pc.UserName as ProductCategory, psc.UserName as ProductSubCategory,moi.Id as ItemId,mma.StandardName as ItemArticle,
                pl.Remarks,ma.Code as MaterialCode,ma.UserName as Material,ma.Id as MaterialId,PM.UserName as Product , prod.UserName as Prod,PM.Id as ProductId,
                pl.Id as ProductLibId, par.UserName as Customer,par.Id as CustomerId,moi.BuyerReferenceNo , moi.OwnReferenceNo
				
                FROM trn.masterorder mo 
				left join trn.MasterOrderItem moi on moi.MasterOrderId = mo.Id
				left join dbo.ProductLibrary pl on pl.Id = moi.ProductLibraryId
				left join trn.SalesOrder so on so.MasterOrderItemId = moi.Id
				left join trn.ProductionOrderDetail podd on podd.SalesOrderId = so.Id
                left join dbo.ProductLibraryAttribute pa on pa.ProductLibraryId=pl.Id
                left join trn.ProductionOrder po on po.Id=podd.ProductionOrderId 
                 left join hkp.ProductionStatus ps on ps.Id=po.ProductionStatusId
                 left join mst.MaterialMaster ma on ma.Id=moi.MaterialMasterId
                 left join trn.ProductDefinition AS pd ON pd.MaterialMasterId=ma.Id
                 left join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
				 left join hkp.ProductCategory pc on pc.Id = pm.ProductCategoryId
				 
                left join hkp.Party par on par.Id=mo.PartyId
                left join dbo.ScanItem si on si.Id=pa.ScanItemId     
                left join hkp.Product prod on prod.Id = pm.ProductId
				left join hkp.ProductSubCategory psc on psc.Id = pm.ProductSubCategoryId
				LEFT OUTER JOIN [MST].[MaterialMasterArticle] mma ON mma.Id=moi.ArticleId
                where 
				mo.OrderStatusId not in ( 'Closed' , 'Cancelled' , 'Hold') 
				 and pl.Code !='null'
				) as req

       where " + strkey + @"
				order by Del
				
				
				";


                DataTable dt = _sqlRepository.GetDataTable(str);

                dt.Columns.Add("checked", typeof(bool));
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["checked"] = false;
                }
                return Service.Helpers.DataTableExtensions.DataTableToJson(dt);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> GetData(string ToDate, string FromDate, string type, string group, string column, string value, string Loc)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                {
                    strkey = column + " like '%" + value + "%'";
                }

                string assign = "1=1";
                if (type == "Assigned")
                {
                    assign = "Assigned = 'Assigned'";
                }
                else if (type == "Unassigned")
                {
                    assign = "Assigned = 'Unassigned'";
                }
                else
                {
                    assign = "1=1";
                }

                string stocks = "1=1";
                if (group == "WithStock")
                {
                    stocks = "StockQty is not null";
                }
                else if (group == "SOStock")
                {
                    stocks = "StockQty is not null and SoQty is not null";
                }
                else if (group == "SONoStock")
                {
                    stocks = "StockQty is not null and SoQty is null";
                }
                else
                {
                    stocks = "1=1";
                }

                string loc = "";
                if (Loc == "All")
                {
                    loc = "";
                }
                else
                {
                    loc = "where mmv.ToStorageLocId = '" + Loc + "'";
                }

                var _sql = @"Select * from 
(Select (Case when Sos.SoQty is null or Scan.Available is null then 'Unassigned' else 'Assigned' end) as Assigned
,(Case when Scan.PC is null then sos.Code else scan.PC end) as ProductCode ,
(Case when Scan.POId is null then sos.PONo else Scan.POId end) as PO , Scan.*
, Sos.*
from 
(
Select distinct sc.ProductCode as PC , sc.POId , sc.LotNo , isnull(plann.PlanQty,0) as PlannedQty , isnull(StockQty.StockQty,0) as StockQty , isnull(desp.Despatch,0) as Despatch , isnull(bb.BookQty,0) as BookedQty,
(Case when bb.BookQty >plann.PlanQty then (StockQty.StockQty - isnull(bb.BookQty,0)) else (StockQty.StockQty - isnull(plann.PlanQty,0)) end) as Available,
isnull(ud.ud,0) as ud , isnull(fd.fd,0) as fd , isnull(fp.fp,0) as fp
from
dbo.ItemScanChild sc
left join mst.MaterialMovementMaster mmv on mmv.Id = sc.LocMasterId
left join trn.POLotReference pol  on pol.Id = sc.PackingId
left join(
Select isc.ProductCode , isc.POId , isc.LotNo ,sum(isc.NetWeight) as StockQty from
dbo.ItemScanChild isc 
left join dbo.ItemScan isch on isch.Id = isc.MasterId
left join trn.POLotReference pol on pol.Id = isc.PackingId
where isch.WorkDate between '" + FromDate + @"' and '" + ToDate + @"' 
and isc.IsDespatch = 0  
--and isc.InventoryReceiveDetailId is not null
group by isc.ProductCode , POId , isc.LotNo
) StockQty on StockQty.ProductCode = sc.ProductCode and StockQty.POId = sc.POId and StockQty.LotNo = sc.LotNo
left join(
Select isc.ProductCode , isc.POId , isc.LotNo, isnull(sum(isc.NetWeight),0) as Despatch from
dbo.ItemScanChild isc 
left join dbo.ItemScan isch on isch.Id = isc.MasterId
where isc.IsDespatch = 1 and isch.WorkDate between '" + FromDate + @"' and '" + ToDate + @"'
group by ProductCode , POId , isc.LotNo
) desp on desp.ProductCode = sc.ProductCode and desp.POId = sc.POId and desp.LotNo = sc.LotNo
left join (
Select isc.ProductCode , isc.POId ,isc.LotNo, isnull(sum(isc.NetWeight),0) as BookQty from
dbo.ItemScanChild isc 
left join dbo.ItemScan isch on isch.Id = isc.MasterId
where isc.Booked = 1 and isch.WorkDate between '" + FromDate + @"' and '" + ToDate + @"'
group by ProductCode , POId , LotNo
) as bb on  bb.ProductCode = sc.ProductCode and bb.LotNo = sc.LotNo and bb.POId=sc.POId 
left join(
Select ProductCode , PONo , LotNo , sum(PlanQty) as PlanQty from trn.POLotReference
where Status = 'Active'
group by ProductCode , PONo , LotNo
) as plann on plann.ProductCode=sc.ProductCode and plann.PONo = sc.POId and plann.LotNo=sc.LotNo
left join (
Select isc.POId, isc.ProductCode , isc.LotNo ,(sum(isc.netweight)) as ud from dbo.ItemScanChild isc
left join dbo.ItemScan isch on isch.Id = isc.MasterId
where isch.WorkDate <= '" + ToDate + @"'  
group by isc.POId, isc.ProductCode , isc.LotNo
) as ud on ud.ProductCode = sc.ProductCode and ud.POId = sc.POId and ud.LotNo = sc.LotNo
left join (
Select isc.POId, isc.ProductCode , isc.LotNo ,(sum(isc.netweight)) as fd from dbo.ItemScanChild isc
left join dbo.ItemScan isch on isch.Id = isc.MasterId
where isch.WorkDate = '" + FromDate + @"'  
group by isc.POId, isc.ProductCode , isc.LotNo
) as fd on fd.ProductCode = sc.ProductCode and fd.POId = sc.POId and fd.LotNo = sc.LotNo
left join (
Select isc.POId, isc.ProductCode , isc.LotNo ,(sum(isc.netweight)) as fp from dbo.ItemScanChild isc
left join dbo.ItemScan isch on isch.Id = isc.MasterId
where isch.WorkDate between '" + FromDate + @"' and '" + ToDate + @"'
group by isc.POId, isc.ProductCode , isc.LotNo
) as fp on fp.ProductCode = sc.ProductCode and fp.POId = sc.POId and fp.LotNo = sc.LotNo
" + loc + @"
group by sc.ProductCode , sc.POId, sc.LotNo ,StockQty.StockQty,desp.Despatch,bb.BookQty,plann.PlanQty , ud.ud ,fd.fd, fp.fp

) as Scan


full outer join 
(Select pl.Code, po.id as PONo, Count(so.Id) as NoOfSo , sum(so.Qty) as SoQty, moi.Id as ItemId ,PM.UserName as Product,
mo.Id as MasterOrderNo , p.Username as Customer, mma.StandardName as ItemArticle
from
trn.MasterOrder mo
left join trn.MasterOrderItem moi on moi.MasterOrderId = mo.Id
left join trn.SalesOrder so on so.MasterOrderItemId = moi.Id
left join trn.ProductionOrderDetail pod on pod.SalesOrderId = so.Id
left join trn.ProductionOrder po on po.Id = pod.ProductionOrderId
left join dbo.ProductLibrary pl on pl.Id = moi.ProductLibraryId
--left join dbo.ProductLibraryAttribute pla on pla.ProductLibraryId = pl.Id
left join mst.MaterialMaster ma on ma.Id=moi.MaterialMasterId
left join trn.ProductDefinition AS pd ON pd.MaterialMasterId=ma.Id
left join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
left join hkp.Party p on p.Id = mo.PartyId
LEFT OUTER JOIN [MST].[MaterialMasterArticle] mma ON mma.Id=moi.ArticleId
where pl.Code is not null and mo.OrderStatusId = 'Active'
group by pl.Code, po.Id, moi.Id,PM.UserName ,mo.Id , p.UserName , mma.StandardName
)
as Sos on sos.PONo = scan.POId and sos.Code = scan.PC

				) ff 
where " + strkey + @" and " + stocks + @" and " + assign + @"
order by  Assigned, ProductCode , PO 
				
               ";
                DataTable dt = _sqlRepository.GetDataTable(_sql);





                return Service.Helpers.DataTableExtensions.DataTableToJson(dt);
            }
            catch (Exception)
            {
                throw;
            }

        }


        public IEnumerable<object> getgridPacking()
        {
            try
            {
                var str = @"Select pk.PackingId, format(pk.Date,'dd-MMM-yyyy') as AddedDate, format(pk.InactiveDate,'dd-MMM-yyyy') as InActiveDate, DATEDIFF(Day,GETDATE() , InactiveDate) as Active , p.UserName as Customer, ms.UserName as StorageLoc , e.EmployeeName as ByWhom,
ei.Employeename as DRespPerson, en.UserName as Entity, pk.Remarks,pk.CustomerId,pk.EntityId,PS.SalesId
from trn.Packing pk
left join hkp.Party p on p.Id = pk.CustomerId
left join dbo.EmployeeInformation e on e.SystemId = pk.ByWhom
left join dbo.EmployeeInformation ei on ei.SystemId = pk.DispatchResponsiblePersonId
left join hkp.MaterialStorage ms on ms.Id = pk.StorageLocId
left join org.Entity en on en.Id = pk.EntityId
left join dbo.SalesPacking PS on PS.PackingId = pk.PackingId                            
order by pk.Date  DESC";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getPackingLineItemModal(string PackingId)
        {
            try
            {
                var str = @"Select * from 
                            trn.PackingLineItem where PackingId = '" + PackingId + @"'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getPOLotRefGridModal(string PackingLineItemId)
        {
            try
            {
                var str = @"Select pol.Id,pol.PackingLineItemId,pol.ProductCode,pol.PONo,pol.LotNo,pol.PlanQty,pol.Status,pol.Remarks, isnull(bk.booked,0) as BookQty from trn.POLotReference pol 
							left join
							(Select sum(NetWeight) as booked , PackingId from dbo.ItemScanChild where Booked = 1 and SalesReturnId is null
							group by PackingId) as bk on bk.PackingId = pol.Id
                            where PackingLineItemId = '" + PackingLineItemId + @"'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public DataTable GetReportData(string PackingId)
        {
            try
            {
                var str = @"Select pli.PackingLineItemId , mo.Id as MasterOrderNo, moi.Id as ItemId, ma.UserName as Material, mma.StandardName as Article ,so.id as SoId ,
                            pol.LotNo  , pol.ProductCode , pol.PONo , sc.StockQty , pp.Cartons as NoOfPackages,
                            pol.PlanQty , (Case when po.Id != pol.PONo then po.Id else '' end) as SoPoNo , ssd.Bages,PLA.AttributeValue ShadeNo
                            from trn.PackingLineItem pli
                            left join trn.SalesOrder so on so.Id = pli.SOId
                            left join trn.ProductionOrderDetail pod on pod.SalesOrderId = so.Id
                            left join trn.ProductionOrder po on po.Id = pod.ProductionOrderId
                            left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                            left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
							left join ProductLibrary PL on PL.Id = MOI.ProductLibraryId
							left join ProductLibraryAttribute PLA on PLA.ProductLibraryId = PL.Id
                            left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
                            left join 
                            (
                            Select sum(sc.netWeight) as StockQty ,  sc.ProductCode ,sc.POId , sc.LotNo ,count(RefNo) Bages from
                            dbo.ItemScanChild sc where IsDespatch = 0
                            group by  sc.ProductCode ,sc.POId , sc.LotNo
                            ) as sc on sc.LotNo = pol.LotNo and sc.ProductCode = pol.ProductCode and sc.POId = pol.PONo
                            left join ( Select   count(RefNo) Bages , sc.LotNo from
                            dbo.ItemScanChild sc	where  Booked = 0
                            group by   sc.LotNo
							) as ssd on ssd.LotNo = pol.LotNo
							left join mst.MaterialMaster ma on ma.Id = moi.MaterialMasterId
                            left join mst.MaterialMasterArticle mma ON mma.Id=moi.ArticleId
							left join (
							Select pli.PackingLineItemId,pol.LotNo,pol.PONo,pol.ProductCode,count(sc.RefNo) as Cartons from ItemScanChild sc
							left join trn.POLotReference pol on pol.Id = sc.PackingId
							left join trn.PackingLineItem pli on pli.PackingLineItemId = pol.PackingLineItemId
							 where pli.PackingId = '" + PackingId + @"'
							group by pli.PackingLineItemId,pol.LotNo,pol.PONo,pol.ProductCode
							) as pp on pp.PackingLineItemId = pli.PackingLineItemId and pp.LotNo = pol.LotNo and pp.ProductCode = pol.ProductCode and pp.PONo = pol.PONo
                            where pol.Status = 'Active' and pli.PackingId = '" + PackingId + @"'

                            ";
                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public DataTable getMainHeadersReport(string PackingId)
        {
            try
            {
                var str = @"Select p.UserName as Customer, pk.PackingId , format(pk.Date,'dd-MMM-yyyy') as Date , format(pk.InactiveDate,'dd-MMM-yyyy') as InactiveDate, ei.EmployeeName as ByWhom,
                            eii.EmployeeName as DRespPerson, ms.UserName as StorageLoc , e.UserName as Entity , pk.Remarks
                            from
                            trn.Packing pk
                            left join org.Entity e on e.Id = pk.EntityId
                            left join hkp.MaterialStorage ms on ms.Id = pk.StorageLocId
                            left join dbo.EmployeeInformation ei on ei.SystemId = pk.ByWhom
                            left join dbo.EmployeeInformation eii on eii.SystemId = pk.DispatchResponsiblePersonId
                            left join hkp.party p on p.Id = pk.CustomerId
                            where pk.PackingId = '" + PackingId + @"'";
                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public DataTable getPackageDetailReport(string PackingId)
        {
            try
            {
                var str = @"Select distinct pli.PackingLineItemId , 
                            (Select Stuff((Select ','+RefNo
                            from dbo.ItemScanChild sc 
                            left join trn.POLotReference pol on pol.Id = sc.PackingId
                            left join trn.PackingLineItem pl on pl.PackingLineItemId = pol.PackingLineItemId
                            where pl.PackingLineItemId = pli.PackingLineItemId
                            for xml path('')
                            ),1,1,'')) as Cartons
                            from trn.PackingLineItem pli 
                            left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
                            left join dbo.ItemScanChild sc on sc.PackingId = pol.Id
                            where pli.PackingId = '" + PackingId + @"' and pol.Status = 'Active'";
                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> GetPackingData()
        {
            try
            {
                var str = @"SELECT distinct pk.PackingId,Convert(bit,0) Active, format(pk.Date,'dd-MMM-yyyy') as AddedDate, format(pk.InactiveDate,'dd-MMM-yyyy') as InActiveDate, p.UserName as Customer, ms.UserName as StorageLoc , e.EmployeeName as ByWhom,
                            ei.Employeename as DRespPerson, en.UserName as Entity, pk.Remarks,pk.CustomerId,pk.EntityId,A.CurrencyId,A.Currency 
                           ,A.PaymentTermId,A.Code PaymentTermCode,A.UserName PaymentTermName, A.BaseLineDate, A.NoOfDay,A.PaymentMode
						   FROM TRN.Packing pk
                            LEFT JOIN hkp.Party p on p.Id = pk.CustomerId
                            LEFT JOIN dbo.EmployeeInformation e on e.SystemId = pk.ByWhom
                            LEFT JOIN dbo.EmployeeInformation ei on ei.SystemId = pk.DispatchResponsiblePersonId
                            LEFT JOIN hkp.MaterialStorage ms on ms.Id = pk.StorageLocId
                            LEFT JOIN org.Entity en on en.Id = pk.EntityId                             
                             JOIN
                            (
							SELECT DISTINCT PLI.PackingId,PT.PaymentTermId,PT.Code,PT.UserName,PT.IsPaymentTermChangeable
							, PT.BaseLineDate, PT.NoOfDay, PT.Code [PaymentTermCode],PT.PaymentMode,POLR.Qty
							,C.Code AS Currency,MO.CurrencyId
                               FROM  trn.PackingLineItem PLI 
                            LEFT JOIN TRN.SalesOrder SO ON SO.Id=PLI.SOId
                            LEFT JOIN TRN.MasterOrderItem MOI ON MOI.Id=SO.MasterOrderItemId
                            LEFT JOIN TRN.MasterOrder MO ON MO.Id=MOI.MasterOrderId
                            LEFT JOIN(
							Select PT.Id PaymentTermId,PT.Code,PT.UserName,PT.BaseLineDate, PTD.NoOfDay, PT.Code [PaymentTermCode],PT.PaymentMode,MCP.IsPaymentTermChangeable from [MST].[PaymentTerm] AS PT 
                            LEFT JOIN [HKP].[CompanyParty] AS MCP ON  MCP.PartyType='Customer' AND PT.Id=MCP.PaymentTermId
							LEFT JOIN [MST].[PaymentTermDetail] PTD ON PTD.PaymentTermId=PT.Id
                            WHERE PTD.[Sequence]='3' AND PT.Active=1 AND PT.Archive=0 AND PT.IsCustomer=1)PT ON PT.PaymentTermId=MO.PaymentTermId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=MO.CurrencyId
 JOIN 
(							
Select ISNULL(SUM(sc.NetWeight),0) Qty, ISNULL(SUM(PlanQty),0) PlanQty,PackingLineItemId from trn.POLotReference po
							left join dbo.ItemScanChild sc on sc.PackingId = po.Id AND Booked = 1
							 GROUP BY PackingLineItemId 
							Having ISNULL(SUM(sc.NetWeight),0)!=0
)POLR ON POLR.PackingLineItemId=PLI.PackingLineItemId
                            ) A ON A.PackingId=pk.PackingId AND A.Qty<>0                               
                            WHERE Pk.PackingId NOT IN (Select PackingId from dbo.SalesPacking) and PK.InactiveDate >= Cast(GETDATE() as date)";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public DataTable GetStockData(string ToDate, string FromDate, string type, string group, string column, string value, string Loc)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                {
                    strkey = column + " like '%" + value + "%'";
                }

                string assign = "1=1";
                if (type == "Assigned")
                {
                    assign = "Assigned = 'Assigned'";
                }
                else if (type == "Unassigned")
                {
                    assign = "Assigned = 'Unassigned'";
                }
                else
                {
                    assign = "1=1";
                }

                string stocks = "1=1";
                if (group == "WithStock")
                {
                    stocks = "StockQty is not null";
                }
                else if (group == "SOStock")
                {
                    stocks = "StockQty is not null and SoQty is not null";
                }
                else if (group == "SONoStock")
                {
                    stocks = "StockQty is not null and SoQty is null";
                }
                else
                {
                    stocks = "1=1";
                }

                string loc = "";
                if (Loc == "All")
                {
                    loc = "";
                }
                else
                {
                    loc = "where mmv.ToStorageLocId = '" + Loc + "'";
                }


                var _sql = @"Select distinct * from 
                    (Select (Case when Sos.SoQty is null or Scan.Available is null then 'Unassigned' else 'Assigned' end) as Assigned
                    ,(Case when Scan.PC is null then sos.Code else scan.PC end) as ProductCode ,
                    (Case when Scan.POId is null then sos.PONo else Scan.POId end) as PO , Scan.*
                    , Sos.*
                    from 
                    (
                   Select distinct sc.ProductCode as PC , sc.POId , sc.LotNo  , isnull(bb.BookQty,0) as AssignedQty , (isnull(ntw.NetWeight,0) - isnull(bb.BookQty,0)) as Available
					,isnull(ntw.NetWeight,0) as StockQty , crt.Refs as Cartons
                    from
                    dbo.ItemScanChild sc
                    left join mst.MaterialMovementMaster mmv on mmv.Id = sc.LocMasterId
                    left join trn.POLotReference pol  on pol.Id = sc.PackingId
                    left join (
                    Select isc.ProductCode , isc.POId ,isc.LotNo, isnull(sum(isc.NetWeight),0) as BookQty from
                    dbo.ItemScanChild isc 
                    left join dbo.ItemScan isch on isch.Id = isc.MasterId
                    where isc.Booked = 1 
                    group by ProductCode , POId , LotNo
                    ) as bb on  bb.ProductCode = sc.ProductCode and bb.LotNo = sc.LotNo and bb.POId=sc.POId 
					left join (
                    Select isc.ProductCode , isc.POId ,isc.LotNo, isnull(sum(isc.NetWeight),0) as NetWeight from
                    dbo.ItemScanChild isc 
                    group by ProductCode , POId , LotNo
                    ) as ntw on  ntw.ProductCode = sc.ProductCode and ntw.LotNo = sc.LotNo and ntw.POId=sc.POId 
					left join (
                    Select isc.ProductCode , isc.POId ,isc.LotNo, isnull(count(isc.RefNo),0) as Refs from
                    dbo.ItemScanChild isc 
                    group by ProductCode , POId , LotNo
                    ) as crt on  crt.ProductCode = sc.ProductCode and crt.LotNo = sc.LotNo and crt.POId=sc.POId
                    " + loc + @"
                    --group by sc.ProductCode , sc.POId, sc.LotNo ,desp.Despatch,bb.BookQty

                    ) as Scan


                    full outer join 
                    (Select distinct pl.Code, po.id as PONo, so.Id as SoId , so.Qty as SoQty, moi.Id as ItemId ,PM.UserName as Product,
                    mo.Id as MasterOrderNo , p.Username as Customer, mma.StandardName as ItemArticle , sales.Dispatch,
					(so.Qty - sales.Dispatch) as ToBeDispatch , format(so.CommitmentDate,'dd-MMM-yyyy') as CommitmentDate ,format(so.PlanExFactoryDate,'dd-MMM-yyyy') as ExFactoryDate,
                        moi.Remark as Remarks , c.ContractNo
                    from
                    trn.MasterOrder mo
                    left join trn.MasterOrderItem moi on moi.MasterOrderId = mo.Id
                    left join trn.SalesOrder so on so.MasterOrderItemId = moi.Id
                    left join trn.ProductionOrderDetail pod on pod.SalesOrderId = so.Id
                    left join trn.ProductionOrder po on po.Id = pod.ProductionOrderId
                    left join dbo.ProductLibrary pl on pl.Id = moi.ProductLibraryId
                    --left join dbo.ProductLibraryAttribute pla on pla.ProductLibraryId = pl.Id
                    left join mst.MaterialMaster ma on ma.Id=moi.MaterialMasterId
                    left join trn.ProductDefinition AS pd ON pd.MaterialMasterId=ma.Id
                    left join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                    left join hkp.Party p on p.Id = mo.PartyId
                    LEFT OUTER JOIN [MST].[MaterialMasterArticle] mma ON mma.Id=moi.ArticleId
                    Left Join dbo.Contract c on c.Id = so.ContractId

					left join(
					Select so.Id , isnull(sum(sm.TransactionQty),0) as Dispatch from trn.SalesOrder so 
					left join trn.SalesMaterial sm on sm.SalesOrderId = so.Id
					group  by so.Id , sm.SalesOrderId
					) as sales on sales.Id = so.Id
                    where pl.Code is not null and mo.OrderStatusId = 'Active'
                    )
                    as Sos on sos.PONo = scan.POId and sos.Code = scan.PC

				                    ) ff 
                    where " + strkey + @" and " + stocks + @" and " + assign + @"
                    order by  Assigned, ProductCode , PO 
				
               ";
                DataTable dt = _sqlRepository.GetDataTable(_sql);
                return dt;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public DataTable GetScanDataReport(string packingId)
        {
            try
            {

                var str = @"SELECT MMA.StandardName Article, MMA.StandardName ATS, POLR.netWeight,PLA.AttributeValue Shade, POLR.GWeight,POLR.RefNo,POLR.Cones,POLR.LotNo
,SP.SalesId InvoiceNo,FORMAT(S.InvoiceDate,'dd-MMM-yyyy') InvoiceDate,pc.UserName as ConsigneeBilltoName 
FROM [TRN].[SalesOrder] AS SO
JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id
JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
LEFT JOIN [MST].[MaterialMasterArticle] AS MMA ON MOI.ArticleId = MMA.Id	
left join ProductLibrary PL on PL.Id = MOI.ProductLibraryId
left join ProductLibraryAttribute PLA on PLA.ProductLibraryId  = PL.Id and PLA.StandardName like '%SH%'
LEFT JOIN trn.PackingLineItem PLI ON PLI.SOId=SO.Id
--LEFT JOIN dbo.[contract] as c on c.id = SO.contractId
LEFT JOIN HKP.Party as pc on pc.Id=MO.PartyId
--LEFT JOIN HKP.PartyPlant as pbt on pbt.Id=c.InvoicingPartyPlantId
LEFT JOIN dbo.SalesPacking SP on SP.PackingId=pli.PackingId
LEFT JOIN TRN.Sales S on S.Id=SP.SalesId
LEFT JOIN 
(							
Select ISNULL((sc.NetWeight),0) netWeight, ISNULL((sc.GWeight),0) GWeight,sc.RefNo,sc.Cones,sc.LotNo,PackingLineItemId from trn.POLotReference po
left join dbo.ItemScanChild sc on sc.PackingId = po.Id
)POLR ON POLR.PackingLineItemId=PLI.PackingLineItemId								
WHERE  PLI.PackingId ='" + packingId + "' ORDER BY MMA.StandardName";
                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public DataTable getGroupFinishedStocksReport(string Loc, string FromDate, string ToDate)
        {
            try
            {
                string loc = "";
                string tempDate = "";
                string tempCurrentDate = "";
                if (Loc == "All")
                {
                    loc = "";
                }
                else
                {
                    loc = " AND R.ToStorageLocId = '" + Loc + "' ";
                }
                if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
                {
                    tempDate = " AND convert(Date,S.AddedDate) between '" + FromDate + "' AND '" + ToDate + @"' ";
                    tempCurrentDate = " AND convert(Date,S.AddedDate) between '" + FromDate + "' AND '" + DateTime.Now.ToString() + @"' ";
                }
                else if (string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
                {
                    tempDate = " AND convert(Date,S.AddedDate) <= '" + ToDate + @"' ";
                    tempCurrentDate = " AND convert(Date,S.AddedDate) <= '" + DateTime.Now.ToString() + @"' ";
                }
                else
                {
                    tempDate = " ";
                    tempCurrentDate = " ";
                }
                var str = @"select x.StandardName,x.ProductCode,x.POId,x.LotNo,sum(x.Bags) Bags,x.BagSize BagSize,sum(x.NtWt) NtWt,sum(x.GtWt) GtWt,sum(x.ActualBags) ActualBags,sum(x.ActualNtWt) ActualNtWt,sum(x.ActualGtWt) ActualGtWt,x.ProdDetails 
                            FROM (
                             Select M.StandardName , S.ProductCode, S.POId,  S.LotNo, Count(S.RefNo) as Bags, S.NetWeight as BagSize , Sum(S.NetWeight) as NtWt, Sum(S.GWeight) as GtWt,
                              0 ActualBags,0 ActualNtWt, 0 ActualGtWt,
							(Select Stuff((
                            Select ' / ' + pla.ShortName + ' - ' + pla.AttributeValue
                            FROM dbo.ProductLibraryAttribute pla
                            WHERE pla.ProductLibraryId = P.Id
                            FOR XML PATH('')
                            ) , 1, 2, '')) as ProdDetails
                            FROM ItemScanChild S 
							LEFT JOIN MST.MaterialMovementMaster MMM ON MMM.Id = S.LocMasterId
							LEFT JOIN HKP.MaterialMovementPurpose MMP ON MMP.Id = MMM.PurposeId
                            LEFT JOIN ProductLibrary P ON P.Code = S.ProductCode 
                            LEFT JOIN MST.MaterialMasterArticle M ON M.Id = P.ArticleId 
                            LEFT JOIN MST.MaterialMovementMaster R ON R.ID = S.LocMasterId 
                            WHERE S.Booked = 'False' AND isnull(cast(MMP.IsInventoryOut as int),0) <> 1
							" + tempDate + @"
                            AND M.StandardName IS NOT NULL AND S.SalesId IS NULL 
                            GROUP BY  M.StandardName , S.LotNo, S.NetWeight , P.Id, S.ProductCode, S.POId
                           
							UNION ALL

							Select M.StandardName , S.ProductCode, S.POId,  S.LotNo, 0 Bags, S.NetWeight as BagSize , 0 NtWt, 0 GtWt,
                             Count(S.RefNo) as ActualBags,Sum(S.NetWeight) as ActualNtWt, Sum(S.GWeight) as ActualGtWt,
							(Select Stuff((
                            Select ' / ' + pla.ShortName + ' - ' + pla.AttributeValue
                            FROM dbo.ProductLibraryAttribute pla
                            WHERE pla.ProductLibraryId = P.Id
                            FOR XML PATH('')
                            ) , 1, 2, '')) as ProdDetails
                            FROM ItemScanChild S 
							LEFT JOIN MST.MaterialMovementMaster MMM ON MMM.Id = S.LocMasterId
							LEFT JOIN HKP.MaterialMovementPurpose MMP ON MMP.Id = MMM.PurposeId
                            LEFT JOIN ProductLibrary P ON P.Code = S.ProductCode 
                            LEFT JOIN MST.MaterialMasterArticle M ON M.Id = P.ArticleId 
                            LEFT JOIN MST.MaterialMovementMaster R ON R.ID = S.LocMasterId 
                            WHERE S.Booked = 'False' AND isnull(cast(MMP.IsInventoryOut as int),0) <> 1
                             " + tempCurrentDate + @"
                            AND M.StandardName IS NOT NULL AND S.SalesId IS NULL 
                            GROUP BY  M.StandardName , S.LotNo, S.NetWeight , P.Id, S.ProductCode, S.POId
                            
							) x
							GROUP BY x.StandardName,x.ProductCode,x.POId,x.LotNo,x.ProdDetails ,x.BagSize
							ORDER BY X.StandardName , X.LotNo DESC
							";
                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public void GetFinishedGoodsPackingReportData(string fromDate, string toDate, string PurposeId, out DataTable dtOrder)
        {
            try
            {
                string purposeId = "'" + PurposeId.Replace(",", "','") + "'";//replaced with ""
                var str = @"select distinct MP.UserName AS 'PROD_TYPE',
S.ProductCode, S.POId, S.LotNo, S.RefNo, S.Cones, S.NetWeight, S.GWeight, S.PackedBy, 
S.Shade, S.AddedBy, FORMAT (ISM.WorkDate, 'MM/dd/yyyy ') as WorkDate, S.AddedDate, M.StandardName Article, R.FromLocation, R.ToLocation,R.EntityId 
FROM ItemScanChild S 
LEFT JOIN ItemScan ISM ON ISM.Id = S.MasterId
LEFT JOIN ProductLibrary P ON P.Code = S.ProductCode 
LEFT JOIN MST.MaterialMasterArticle M ON M.Id = P.ArticleId 
LEFT JOIN MST.MaterialMovementMaster R ON R.ID = S.LocMasterId
LEFT JOIN HKP.MaterialMovementPurpose MP ON MP.Id=R.PurposeId
WHERE R.PurposeId IN(" + purposeId + ") AND ISM.WorkDate between '" + fromDate + @"' and '" + toDate + @"'
union all
select distinct MP.UserName AS 'PROD_TYPE',
S.ProductCode, S.POId, S.LotNo, S.RefNo, S.Cones, S.NetWeight, S.GWeight, S.PackedBy, 
S.Shade, S.AddedBy, FORMAT (ISM.WorkDate, 'MM/dd/yyyy ') as WorkDate, S.AddedDate, M.StandardName Article, R.FromLocation, R.ToLocation,R.EntityId 
FROM ItemScanChildHistory S 
LEFT JOIN ItemScan ISM ON ISM.Id = S.MasterId
LEFT JOIN ProductLibrary P ON P.Code = S.ProductCode 
LEFT JOIN MST.MaterialMasterArticle M ON M.Id = P.ArticleId 
LEFT JOIN MST.MaterialMovementMaster R ON R.ID = S.LocMasterId
LEFT JOIN HKP.MaterialMovementPurpose MP ON MP.Id=R.PurposeId
WHERE R.PurposeId IN(" + purposeId + ") AND ISM.WorkDate between '" + fromDate + @"' and '" + toDate + @"'
";
                dtOrder = _sqlRepository.GetDataTable(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void GetFinishedGoodsPackingData(string fromDate, string toDate, string PurposeId, out DataTable dtOrder)
        {
            try
            {
                string purposeId = "'" + PurposeId.Replace(",", "','") + "'";//replaced with ""
                var str = @"Select SUM(A.NetWeight)Quantity,A.POId ProductionOrderId,A.LotNo LotNumber,A.EntityId,A.WorkDate ProductionDate,A.ShiftId,A.Grade,A.ProcessId from(
select distinct MP.UserName AS 'PROD_TYPE',
S.ProductCode, S.POId, S.LotNo, S.RefNo, S.Cones, S.NetWeight, S.GWeight, S.PackedBy, 
S.Shade, S.AddedBy, FORMAT (ISM.WorkDate, 'MM/dd/yyyy ') as WorkDate, S.AddedDate, M.StandardName Article, R.FromLocation, R.ToLocation,R.EntityId,ISM.Id,ISM.ShiftId,ISM.Grade,MP.ProcessId  
FROM ItemScanChild S 
LEFT JOIN ItemScan ISM ON ISM.Id = S.MasterId
LEFT JOIN ProductLibrary P ON P.Code = S.ProductCode 
LEFT JOIN MST.MaterialMasterArticle M ON M.Id = P.ArticleId 
LEFT JOIN MST.MaterialMovementMaster R ON R.ID = S.LocMasterId
LEFT JOIN HKP.MaterialMovementPurpose MP ON MP.Id=R.PurposeId
WHERE R.PurposeId IN(" + purposeId + ") AND ISM.WorkDate between '" + fromDate + @"' and '" + toDate + @"'
union all
select distinct MP.UserName AS 'PROD_TYPE',
S.ProductCode, S.POId, S.LotNo, S.RefNo, S.Cones, S.NetWeight, S.GWeight, S.PackedBy, 
S.Shade, S.AddedBy, FORMAT (ISM.WorkDate, 'MM/dd/yyyy ') as WorkDate, S.AddedDate, M.StandardName Article, R.FromLocation, R.ToLocation,R.EntityId,ISM.Id,ISM.ShiftId,ISM.Grade,MP.ProcessId  
FROM ItemScanChildHistory S 
LEFT JOIN ItemScan ISM ON ISM.Id = S.MasterId
LEFT JOIN ProductLibrary P ON P.Code = S.ProductCode 
LEFT JOIN MST.MaterialMasterArticle M ON M.Id = P.ArticleId 
LEFT JOIN MST.MaterialMovementMaster R ON R.ID = S.LocMasterId
LEFT JOIN HKP.MaterialMovementPurpose MP ON MP.Id=R.PurposeId
WHERE R.PurposeId IN(" + purposeId + ") AND ISM.WorkDate between '" + fromDate + @"' and '" + toDate + @"'
)A
Group By A.POId,A.LotNo,A.EntityId,A.WorkDate,A.ShiftId,A.Grade,A.ProcessId
";
                dtOrder = _sqlRepository.GetDataTable(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void GetItemScanChildData(string fromDate, string toDate, string PurposeId, out DataSet dtOrder)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string purposeId = "'" + PurposeId.Replace(",", "','") + "'";//replaced with ""
                string sql = @"Select * from ItemScanChild Where Id IN(
select S.Id FROM ItemScanChild S 
LEFT JOIN ItemScan ISM ON ISM.Id = S.MasterId
LEFT JOIN ProductLibrary P ON P.Code = S.ProductCode 
LEFT JOIN MST.MaterialMasterArticle M ON M.Id = P.ArticleId 
LEFT JOIN MST.MaterialMovementMaster R ON R.ID = S.LocMasterId
LEFT JOIN HKP.MaterialMovementPurpose MP ON MP.Id=R.PurposeId
WHERE R.PurposeId IN (" + purposeId + ") AND ISM.WorkDate between '" + fromDate + @"' and '" + toDate + @"')";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dtOrder, false, false, "", "1");
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public DataTable getAllFinishedStocksReport(string Loc, string ToDate, string FromDate)
        {
            try
            {
                string loc = "";
                if (Loc == "All")
                {
                    loc = "";
                }
                else
                {
                    loc = "AND R.ToStorageLocId = '" + Loc + "'";
                }
                var str = @"Select M.StandardName ,  S.LotNo, S.RefNo as Cartons, S.NetWeight as NtWt, S.GWeight as GtWt
                            from ItemScanChild S 
                            LEFT JOIN ProductLibrary P ON P.Code = S.ProductCode 
                            LEFT JOIN MST.MaterialMasterArticle M ON M.Id = P.ArticleId 
                            LEFT JOIN MST.MaterialMovementMaster R ON R.ID = S.LocMasterId 
                            WHERE s.booked = 'False' AND R.ToLocation <> 'JOB WORK LOCATION' AND R.ToLocation <> 'DyeHouse' AND R.ToLocation <> 'PACKING'  " + loc + @"
                            AND S.AddedDate between '" + FromDate + @"' and '" + ToDate + @"'";
                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ProductionSummary", out sID);
            return sID;
        }

        public void SaveScandataToBooking(string fromDate, string toDate, string PurposeId, IdentityParameter identity)
        {
            try
            {
                DataTable data = null;
                DataSet dsProductionSummary = null;
                DataSet dsIsInventory = null;
                DataSet dsItemScanChild = null;
                GetFinishedGoodsPackingData(fromDate, toDate, PurposeId, out data);
                GetItemScanChildData(fromDate, toDate, PurposeId, out dsItemScanChild);
                bool IsInventory = false;
                string psId = "";
                #region ProductionSummary
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                if (data.Rows.Count > 0)
                {
                    for (int i = 0; i < data.Rows.Count; i++)
                    {
                        string pssql = @"SELECT * FROM TRN.ProductionSummary WHERE ProductionDate between '" + Convert.ToDateTime(data.Rows[i]["ProductionDate"]).ToString("dd-MMM-yyyy") + @"' AND '" + Convert.ToDateTime(data.Rows[i]["ProductionDate"]).ToString("dd-MMM-yyyy") + @"' AND ProductionOrderId='" + data.Rows[i]["ProductionOrderId"].ToString() + @"' AND EntityId='" + data.Rows[i]["EntityId"].ToString() + @"' AND ProcessId='" + data.Rows[i]["ProcessId"].ToString() + @"' AND LotNumber='" + data.Rows[i]["LotNumber"].ToString() + @"'";
                        con.OpenDataSetThroughAdapter(pssql, out dsProductionSummary, false, "1");

                        string invsql = "Select  IsInventory from TRN.ProductionOrderProcessSet where ProductionOrderId='" + data.Rows[i]["ProductionOrderId"].ToString() + @"' AND ProcessId='" + data.Rows[i]["ProcessId"].ToString() + "'";
                        con.OpenDataSetThroughAdapter(invsql, out dsIsInventory, false, "1");
                        if (dsIsInventory.Tables[0].Rows.Count > 0)
                        {
                            IsInventory = Convert.ToBoolean(dsIsInventory.Tables[0].Rows[0]["IsInventory"]);
                        }

                        dsProductionSummary.Tables[0].DefaultView.RowFilter = "ProductionDate='" + data.Rows[i]["ProductionDate"].ToString() + "' AND ProductionOrderId = '" + data.Rows[i]["ProductionOrderId"] + "' AND EntityId = '" + data.Rows[i]["EntityId"].ToString() + "' AND ProcessId = '" + data.Rows[i]["ProcessId"].ToString() + "'AND LotNumber = '" + data.Rows[i]["LotNumber"].ToString() + "'";

                        if (dsProductionSummary.Tables[0].DefaultView.Count > 0)
                        {
                            //edit
                            DataRow dr = dsProductionSummary.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();
                            psId = dr["Id"].ToString();
                            dr["ScanQty"] = data.Rows[i]["Quantity"].ToString();
                            dr["Quantity"] = data.Rows[i]["Quantity"].ToString();
                            dr["IsInventory"] = IsInventory;
                            dr["SourceType"] = "Scan";
                            dr["UpdatedBy"] = identity.UpdatedBy;
                            dr["UpdatedDate"] = DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.UpdatedFromIP;

                            dr.EndEdit();
                        }
                        else
                        {
                            bplib.clsGenID objGenID = new bplib.clsGenID();
                            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ProductionSummary", out string sID);

                            DataRow drProductionSummary = dsProductionSummary.Tables[0].NewRow();
                            drProductionSummary["Id"] = "PS" + sID;
                            psId = "PS" + sID;
                            drProductionSummary["PlantId"] = identity.PlantId;
                            drProductionSummary["EntityId"] = data.Rows[i]["EntityId"].ToString();
                            drProductionSummary["ProcessId"] = data.Rows[i]["ProcessId"].ToString();
                            drProductionSummary["ProductionDate"] = data.Rows[i]["ProductionDate"].ToString();
                            drProductionSummary["ScanQty"] = data.Rows[i]["Quantity"].ToString();
                            drProductionSummary["Quantity"] = data.Rows[i]["Quantity"].ToString();
                            drProductionSummary["ProductionOrderId"] = data.Rows[i]["ProductionOrderId"].ToString();
                            drProductionSummary["ProductionShiftId"] = data.Rows[i]["ShiftId"].ToString();
                            drProductionSummary["ProductionGrade"] = data.Rows[i]["Grade"].ToString();
                            drProductionSummary["LotNumber"] = data.Rows[i]["LotNumber"].ToString();
                            drProductionSummary["IsInventory"] = IsInventory;
                            drProductionSummary["SourceType"] = "Scan";
                            drProductionSummary["AddedBy"] = identity.AddedBy;
                            drProductionSummary["AddedDate"] = DateTime.Now;
                            drProductionSummary["AddedFromIP"] = identity.AddedFromIP;

                            dsProductionSummary.Tables[0].Rows.Add(drProductionSummary);
                        }


                        #region ItemScanChild
                        if (dsItemScanChild.Tables[0].Rows.Count > 0)
                        {
                            for (int j = 0; j < dsItemScanChild.Tables[0].Rows.Count; j++)
                            {
                                dsItemScanChild.Tables[0].DefaultView.RowFilter = "Id='" + dsItemScanChild.Tables[0].Rows[j]["Id"].ToString() + "' AND POId = '" + data.Rows[i]["ProductionOrderId"] + "' AND LotNo = '" + data.Rows[i]["LotNumber"] + "'";

                                if (dsItemScanChild.Tables[0].DefaultView.Count > 0)
                                {
                                    //edit
                                    DataRow drsc = dsItemScanChild.Tables[0].DefaultView[0].Row;
                                    drsc.BeginEdit();

                                    drsc["ProductionSummaryId"] = psId;
                                    drsc["UpdatedBy"] = identity.UpdatedBy;
                                    drsc["UpdatedDate"] = DateTime.Now.ToString();
                                    drsc.EndEdit();
                                }
                            }
                        }
                        #endregion



                    }
                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsProductionSummary, dsItemScanChild);


                }




                #endregion
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void XAddNewRow(DataTable dt, Dictionary<string, object> sourceData)
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
    }

}



