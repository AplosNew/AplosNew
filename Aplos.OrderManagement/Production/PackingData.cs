using System;
using System.Collections.Generic;
using Library.Data.Sql;
using System.Data;
using OTSBD;
using Library.Crosscutting.Security;
using System.Threading;


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

                            if (Cartons[j]["LotNo"].ToString() == jj["LotNo"].ToString() && Cartons[j]["ProductCode"].ToString() == jj["ProductCode"].ToString() && Cartons[j]["PO"].ToString() == jj["PONo"].ToString())
                            {
                                var sqls = @"Update dbo.ItemScanChild Set PackingId = '" + jj["Id"].ToString() + "' , UpdatedBy = '" + identity.Name + "' , Booked = 1  where RefNo IN(" + Cartons[j]["RefNo"] + @")";

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
                var str = @"Select username  , id 
                            from hkp.Party";
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
                            Select isc.ProductCode , isc.POId , Floor(sum(isc.NetWeight)) as FD from
                            dbo.ItemScanChild isc
                            left join dbo.ItemScan isch on isch.Id = isc.LocMasterId
                            --where isch.WorkDate ='ToDate' 
                            group by ProductCode , POId
                            ) fd on fd.ProductCode = sc.ProductCode and fd.POId = sc.POId
                            left join(
                            Select isc.ProductCode , isc.POId , Floor(sum(isc.NetWeight)) as FP from
                            dbo.ItemScanChild isc 
                            left join dbo.ItemScan isch on isch.Id = isc.LocMasterId
                            --where isch.WorkDate between 'ToDate' and 'FromDate'
                            group by ProductCode , POId
                            ) fp on fp.ProductCode = sc.ProductCode and fp.POId = sc.POId
                            left join(
                            Select isc.ProductCode , isc.POId , Floor(sum(isc.NetWeight)) as UD from
                            dbo.ItemScanChild isc 
                            left join dbo.ItemScan isch on isch.Id = isc.MasterId
                            --where isch.WorkDate <= 'FromDate' 
                            group by ProductCode , POId
                            ) ud on ud.ProductCode = sc.ProductCode and ud.POId = sc.POId
                            left join(
                            Select isc.ProductCode , isc.POId , Floor(sum(isc.NetWeight)) as Despatch from
                            dbo.ItemScanChild isc 
                            left join dbo.ItemScan isch on isch.Id = isc.MasterId
                            where isc.IsDespatch = 1
                            group by ProductCode , POId
                            ) desp on desp.ProductCode = sc.ProductCode and desp.POId = sc.POId
                            left join (
                            Select isc.ProductCode , isc.POId, Floor(sum(isc.NetWeight)) as Ret from
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
                var str = @"Select LotNo , RefNo , NetWeight , GWeight ,ProductCode , POId from dbo.ItemScanChild where LotNo = '" + LotNo + "' and ProductCode = '" + ProductCode + @"' and POId = '" + PO + @"' and Booked = 0 and IsDespatch = 0";
                DataTable dt = _sqlRepository.GetDataTable(str);
                var str1 = @"Select LotNo , RefNo , NetWeight , GWeight ,ProductCode , POId from dbo.ItemScanChild where LotNo = '" + LotNo + "' and ProductCode = '" + ProductCode + @"' and POId = '" + PO + @"' and Booked = 1 and IsDespatch = 0";
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
                var str = @" Select sc.ProductCode , sc.POId as PO, sc.LotNo , isnull(plann.PlanQty,0) as PlannedQty , isnull(StockQty.StockQty,0)  as StockQty , isnull(desp.Despatch,0) as Despatch , isnull(bb.BookQty,0) as BookedQty,
                        (Case when bb.BookQty >plann.PlanQty then (isnull(StockQty.StockQty,0) - isnull(bb.BookQty,0)) else (isnull(StockQty.StockQty,0)  - isnull(plann.PlanQty,0)) end) as Available
                        from
                        dbo.ItemScanChild sc
                        left join trn.POLotReference pol  on pol.Id = sc.PackingId
                        left join(
                        Select isc.ProductCode , isc.POId , Floor(sum(isc.NetWeight)) as StockQty from
                        dbo.ItemScanChild isc 
                        left join dbo.ItemScan isch on isch.Id = isc.MasterId
                        left join trn.POLotReference pol on pol.Id = isc.PackingId
                        where isch.WorkDate between '" + FromDate + @"' and '" + ToDate + @"'
                        and isc.IsDespatch = 0 
                        group by isc.ProductCode , POId
                        ) StockQty on StockQty.ProductCode = sc.ProductCode and StockQty.POId = sc.POId
                        left join(
                        Select isc.ProductCode , isc.POId , isnull(Floor(sum(isc.NetWeight)),0) as Despatch from
                        dbo.ItemScanChild isc 
                        left join dbo.ItemScan isch on isch.Id = isc.MasterId
                        where isc.IsDespatch = 1 and isch.WorkDate between '" + FromDate + @"' and '" + ToDate + @"'
                        group by ProductCode , POId
                        ) desp on desp.ProductCode = sc.ProductCode and desp.POId = sc.POId
                        left join (
                        Select isc.ProductCode , isc.POId ,isc.LotNo, isnull(Floor(sum(isc.NetWeight)),0) as BookQty from
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
                        where sc.ProductCode = '" + productCode + @"'
                        group by sc.ProductCode , sc.POId, sc.LotNo ,StockQty.StockQty,desp.Despatch,bb.BookQty,plann.PlanQty

                        ";

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
                (SELECT distinct so.Id as SO, so.Qty as SoQty, sos.Despatch , (so.Qty - sos.Despatch) as toDespatch,format(so.DeliveryDate,'dd-MMM-yyyy') as DeliveryDate, CAST(so.DeliveryDate as DATE) as Del ,pl.Code  as ProductCode,
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
				 left join hkp.OrderStatus os on os.Id = mo.OrderStatusId
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
				mo.OrderStatusId not in ( 'Closed' , 'Cancelled' , 'Hold') 
				 and pl.Code !='null'
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

        public IEnumerable<object> GetData(string ToDate, string FromDate, string type, string group, string column, string value)
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

                var _sql = @"Select * from 
(Select (Case when Sos.SoQty is null or Scan.Available is null then 'Unassigned' else 'Assigned' end) as Assigned
,(Case when Scan.PC is null then sos.Code else scan.PC end) as ProductCode ,
(Case when Scan.POId is null then sos.PONo else Scan.POId end) as PO , Scan.*
, Sos.*
from 
(
Select distinct sc.ProductCode as PC , sc.POId , sc.LotNo , isnull(plann.PlanQty,0) as PlannedQty , StockQty.StockQty , isnull(desp.Despatch,0) as Despatch , isnull(bb.BookQty,0) as BookedQty,
(Case when bb.BookQty >plann.PlanQty then (StockQty.StockQty - isnull(bb.BookQty,0)) else (StockQty.StockQty - isnull(plann.PlanQty,0)) end) as Available,
isnull(ud.ud,0) as ud , isnull(fd.fd,0) as fd , isnull(fp.fp,0) as fp
from
dbo.ItemScanChild sc
left join trn.POLotReference pol  on pol.Id = sc.PackingId
left join(
Select isc.ProductCode , isc.POId , isc.LotNo ,Floor(sum(isc.NetWeight)) as StockQty from
dbo.ItemScanChild isc 
left join dbo.ItemScan isch on isch.Id = isc.MasterId
left join trn.POLotReference pol on pol.Id = isc.PackingId
where isch.WorkDate between '" + FromDate + @"' and '" + ToDate + @"' 
and isc.IsDespatch = 0 
group by isc.ProductCode , POId , isc.LotNo
) StockQty on StockQty.ProductCode = sc.ProductCode and StockQty.POId = sc.POId and StockQty.LotNo = sc.LotNo
left join(
Select isc.ProductCode , isc.POId , isc.LotNo, isnull(Floor(sum(isc.NetWeight)),0) as Despatch from
dbo.ItemScanChild isc 
left join dbo.ItemScan isch on isch.Id = isc.MasterId
where isc.IsDespatch = 1 and isch.WorkDate between '" + FromDate + @"' and '" + ToDate + @"'
group by ProductCode , POId , isc.LotNo
) desp on desp.ProductCode = sc.ProductCode and desp.POId = sc.POId and desp.LotNo = sc.LotNo
left join (
Select isc.ProductCode , isc.POId ,isc.LotNo, isnull(Floor(sum(isc.NetWeight)),0) as BookQty from
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
Select isc.POId, isc.ProductCode , isc.LotNo ,Floor(sum(isc.netweight)) as ud from dbo.ItemScanChild isc
left join dbo.ItemScan isch on isch.Id = isc.MasterId
where isch.WorkDate <= '" + ToDate + @"'  
group by isc.POId, isc.ProductCode , isc.LotNo
) as ud on ud.ProductCode = sc.ProductCode and ud.POId = sc.POId and ud.LotNo = sc.LotNo
left join (
Select isc.POId, isc.ProductCode , isc.LotNo ,Floor(sum(isc.netweight)) as fd from dbo.ItemScanChild isc
left join dbo.ItemScan isch on isch.Id = isc.MasterId
where isch.WorkDate = '" + FromDate + @"'  
group by isc.POId, isc.ProductCode , isc.LotNo
) as fd on fd.ProductCode = sc.ProductCode and fd.POId = sc.POId and fd.LotNo = sc.LotNo
left join (
Select isc.POId, isc.ProductCode , isc.LotNo ,Floor(sum(isc.netweight)) as fp from dbo.ItemScanChild isc
left join dbo.ItemScan isch on isch.Id = isc.MasterId
where isch.WorkDate between '" + FromDate + @"' and '" + ToDate + @"'
group by isc.POId, isc.ProductCode , isc.LotNo
) as fp on fp.ProductCode = sc.ProductCode and fp.POId = sc.POId and fp.LotNo = sc.LotNo
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
                var str = @"Select PackingId, format(Date,'dd-MMM-yyyy') as AddedDate, format(InactiveDate,'dd-MMM-yyyy') as InActiveDate, DATEDIFF(Day,GETDATE() , InactiveDate) as Active , p.UserName as Customer, ms.UserName as StorageLoc , e.EmployeeName as ByWhom,
                            ei.Employeename as DRespPerson, en.UserName as Entity, pk.Remarks from trn.Packing pk
                            left join hkp.Party p on p.Id = pk.CustomerId
                            left join dbo.EmployeeInformation e on e.SystemId = pk.ByWhom
                            left join dbo.EmployeeInformation ei on ei.SystemId = pk.DispatchResponsiblePersonId
                            left join hkp.MaterialStorage ms on ms.Id = pk.StorageLocId
                            left join org.Entity en on en.Id = pk.EntityId";
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
                var str = @"Select * from trn.POLotReference pol
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
                            pol.LotNo  , pol.ProductCode , pol.PONo , sc.StockQty , sc.NoOfPackages,
                            pol.PlanQty , (Case when po.Id != pol.PONo then po.Id else '' end) as SoPoNo
                            from trn.PackingLineItem pli
                            left join trn.SalesOrder so on so.Id = pli.SOId
                            left join trn.ProductionOrderDetail pod on pod.SalesOrderId = so.Id
                            left join trn.ProductionOrder po on po.Id = pod.ProductionOrderId
                            left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                            left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                            left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
                            left join 
                            (
                            Select Floor(sum(sc.netWeight)) as StockQty , Count(sc.RefNo) as NoOfPackages, sc.ProductCode ,sc.POId , sc.LotNo from
                            dbo.ItemScanChild sc where IsDespatch = 0
                            group by  sc.ProductCode ,sc.POId , sc.LotNo
                            ) as sc on sc.LotNo = pol.LotNo and sc.ProductCode = pol.ProductCode and sc.POId = pol.PONo
                            left join mst.MaterialMaster ma on ma.Id = moi.MaterialMasterId
                            left join mst.MaterialMasterArticle mma ON mma.Id=moi.ArticleId
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

        //        public DataTable getPackageDetailReport1(string PackingId)
        //        {
        //            try
        //            {
        //                var str = @"Select pli.PackingLineItemId , sc.RefNo 
        //from trn.PackingLineItem pli 
        //left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
        //left join dbo.ItemScanChild sc on sc.PackingId = pol.Id
        //where pli.PackingId = '210110' and pol.Status = 'Active'";
        //                return _sqlRepository.GetDataTable(str);
        //            }
        //            catch (Exception e)
        //            {
        //                throw e;
        //            }
        //        }
    }

}



