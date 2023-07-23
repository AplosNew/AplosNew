using System;
using System.Collections.Generic;
using System.Linq;
using Library.Data.Sql;
using System.Data;
using OTSBD;
using Library.Crosscutting.Security;
using System.Threading;

namespace Library.Service.Productions
{

    //    public class PackingData
    //    {
    //        SqlRepository _sqlRepository;
    //        ConnectionManager.clsConnectionManager ConManager;
    //        public PackingData()
    //        {
    //            _sqlRepository = new SqlRepository();
    //            ConManager = new ConnectionManager.clsConnectionManager();
    //        }

    //        public IEnumerable<object> GetEntity()
    //        {
    //            try
    //            {
    //                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
    //                var _sql = @"select UserName as Text,Id as Value from org.Entity where CompanyId='" + identity.CompanyId + "'" +
    //                    " and PlantId='" + identity.PlantId + "'";
    //                return _sqlRepository.GetDataCollection(_sql, null);
    //            }
    //            catch (Exception)
    //            {
    //                throw;
    //            }

    //        }

    //        public IEnumerable<object> GetData(string column, string value, string Entity)
    //        {
    //            try
    //            {
    //                string strkey = "1=1";
    //                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
    //                {
    //                    if (column == "PO")
    //                    {
    //                        strkey = "po.Id like '%" + value + "%'";
    //                    }
    //                    else if (column == "Masterorder")
    //                    {
    //                        strkey = "mo.Id like '%" + value + "%'";
    //                    }
    //                    else if (column == "Customer")
    //                    {
    //                        strkey = "p.UserName like '%" + value + "%'";
    //                    }
    //                    else if (column == "Productcode")
    //                    {
    //                        strkey = "MOI.OwnReferenceNo like '%" + value + "%'";
    //                    }
    //                }
    //                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

    //                var _sql = @"SELECT so.Id AS SalesOrderId,p.UserName AS Customer,b.UserName AS Buyer,mo.MasterOrderNo,mm.UserName AS Material,
    //                           OC.UserName AS OrderCategory,os.UserName AS OrderStatus,OC1.UserName AS SOCategory,
    //						   os1.UserName AS SOStatus,    MA.StandardName AS Article,MA.Code as ArticleCode,                   
    //                            pc.UserName AS ProductCategory,  pm.UserName AS Product,
    //							moi.BuyerReferenceNo,MOI.OwnReferenceNo,mo.BuyerReferenceNo
    //							 AS BuyerOrderNo,MO.OwnReferenceNo AS OwnOrderNo,
    //                            pod.ProductionOrderId,FORMAT(so.DeliveryDate,'dd-MMM-yyyy') as DeliveryDate,
    //							Format(so.CommitmentDate,'dd-MMM-yyyy') as CommitmentDate,Floor(so.Qty) AS SOQty
    //                            ,uom.UserName AS UOM,cur.Code AS Currency,trkp.UserName AS Plant,trke.UserName AS Entity
    //                            ,PLN.PRPlanQty,Pack.PackedQty
    //                              FROM trn.MasterOrder MO

    //						    left outer join trn.MasterOrderItem MOI on moi.MasterOrderId=mo.Id
    //                            left join trn.SalesOrder SO on so.MasterOrderItemId=moi.Id
    //                            LEFT OUTER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
    //                            JOIN ProductionOrderSchedulingParametersType1 AS SED ON sed.ProductionOrderID=pod.ProductionOrderId
    //                            LEFT OUTER JOIN trn.ProductionOrder AS po ON po.Id=pod.ProductionOrderId
    //                        	LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = mo.PlantId
    //                            LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = mo.EntityId


    //                         LEFT JOIN (SELECT ps.ProductionOrderID,FLOOR(SUM(ps.Quantity)) AS  PRPlanQty
    //                                         FROM ProductionPlanningType1 AS ps 
    //                                       GROUP BY ps.ProductionOrderID) AS PLN ON PLN.ProductionOrderID=pod.ProductionOrderId


    //						  left join (SELECT sc.POId,sc.ProductCode,SUM(CAST(sc.NetWeight as decimal(18,2))) as PackedQty from dbo.ItemScanChild as sc
    //						  group by sc.POId,sc.ProductCode
    //						  )as Pack on Pack.POId=po.Id


    //                            left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
    //                            LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
    //                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
    //                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
    //                            left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId

    //                            left outer join [HKP].[Party] p on P.Id=MO.PartyId
    //                            LEFT JOIN [HKP].[CompanyParty] AS COMP ON COMP.PartyId=P.Id AND COMP.PartyType='Customer' AND (TRKP.Id=COMP.PlantId OR isnull(COMP.PlantId,'')='')

    //                            left outer join [HKP].[Buyer] B on b.id=mo.BuyerId
    //                            left outer join [HKP].[OrderCategory] OC on oc.id=mo.OrderCategoryId
    //                            left outer join [HKP].[OrderStatus] OS on OS.id=mo.OrderStatusId
    //                            left outer join [HKP].[OrderCategory] OC1 on oc1.id=so.OrderCategoryId
    //                            left outer join [HKP].[OrderStatus] OS1 on OS1.id=so.OrderStatusId
    //                            left outer join [TRN].[CustomerPO] CPO ON CPO.Id=so.CustomerPOId
    //                        	LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=MO.TotalQtyUOMId
    //							LEFT OUTER JOIN scs.Currency AS cur ON cur.Id=mo.CurrencyId

    //                            WHERE " + strkey + " AND os.Id='Active' AND mo.PlantId='" + identity.PlantId + @"' AND  mo.EntityId IN (" + Entity + @") 
    //            ORDER BY trkp.UserName,trke.UserName,p.UserName, b.UserName,convert(date,so.DeliveryDate),SO.ID
    //               ";
    //                return _sqlRepository.GetDataCollection(_sql, null);
    //            }
    //            catch (Exception)
    //            {
    //                throw;
    //            }

    //        }
    //    }

    //    public class MovementItemsData
    //    {
    //        SqlRepository _sqlRepository;
    //        ConnectionManager.clsConnectionManager ConManager;
    //        string TableName = "dbo.MovementItems";

    //        public MovementItemsData()
    //        {
    //            _sqlRepository = new SqlRepository();
    //            ConManager = new ConnectionManager.clsConnectionManager();
    //        }

    //        public IEnumerable<object> Get(string Id)
    //        {
    //            try
    //            {
    //                var _sql = @"select * from dbo.MovementItems where Id ='" + Id + "'";
    //                return _sqlRepository.GetDataCollection(_sql, null);
    //            }
    //            catch (Exception)
    //            {
    //                throw;
    //            }

    //        }

    //        public IEnumerable<object> GetList(string column, string value)
    //        {
    //            try
    //            {
    //                string strkey = "1=1";
    //                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
    //                    strkey = column + " like '%" + value + "%'";

    //                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
    //                string sql = @"select top 100 * from(select distinct est.*, uom.Username as UOM 
    //                from dbo.MovementItems est left join SCS.UnitOfMeasurement uom on est.UOMId = uom.Id) 
    //                AS TEMP WHERE " + strkey + " order by Item";

    //                return _sqlRepository.GetDataCollection(sql, null);
    //            }
    //            catch (Exception)
    //            {
    //                throw;
    //            }

    //        }

    //        public void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
    //        {
    //            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
    //            DataRow dr = dt.NewRow();

    //            foreach (var item in sourceData.Keys)
    //            {
    //                try
    //                {
    //                    dr[item] = sourceData[item];
    //                }
    //                catch (Exception)
    //                {
    //                }
    //            }
    //            dr["AddedBy"] = identity.Name;
    //            dr["AddedDate"] = System.DateTime.Now.ToString();
    //            dr["AddedFromIP"] = identity.IPAddress;
    //            dr["UpdatedBy"] = identity.Name;
    //            dr["UpdatedDate"] = System.DateTime.Now.ToString();
    //            dr["UpdatedFromIP"] = identity.IPAddress;

    //            dt.Rows.Add(dr);
    //        }

    //        public void EditRow(DataRow dr, Dictionary<string, object> sourceData)
    //        {
    //            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
    //            dr.BeginEdit();

    //            foreach (var item in sourceData.Keys)
    //            {
    //                try
    //                {
    //                    dr[item] = sourceData[item];
    //                }
    //                catch (Exception)
    //                {
    //                }
    //            }
    //            dr["UpdatedBy"] = identity.Name;
    //            dr["UpdatedDate"] = System.DateTime.Now.ToString();
    //            dr["UpdatedFromIP"] = identity.IPAddress;
    //            dr.EndEdit();
    //        }

    //        public void Create(Dictionary<string, object> data)
    //        {
    //            try
    //            {

    //                DataSet dsMaster;
    //                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
    //                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Item='" + data["Item"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
    //                if (dsMaster.Tables[0].Rows.Count > 0)
    //                    throw new Exception("Same Item already exists!!!");


    //                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

    //                string _Id = "";

    //                #region data update
    //                if (dsMaster.Tables[0].Rows.Count == 0)
    //                {
    //                    bplib.clsGenID genid = new bplib.clsGenID();
    //                    genid.GenID(TableName, out _Id);

    //                    data["Id"] = "MI" + _Id;
    //                    AddNewRow(dsMaster.Tables[0], data);
    //                }
    //                else
    //                {
    //                    _Id = data["Id"].ToString();
    //                    EditRow(dsMaster.Tables[0].Rows[0], data);
    //                }
    //                #endregion data update

    //                clsStaticInfo _info = new clsStaticInfo();
    //                _info.SaveDataSets(dsMaster);

    //            }
    //            catch (Exception ex)
    //            {
    //                throw ex;
    //            }
    //        }

    //        public void Delete(string id)
    //        {
    //            try
    //            {
    //                if (string.IsNullOrEmpty(id))
    //                    throw new Exception("Select entry first");

    //                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
    //                con.BeginTransaction();
    //                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
    //                con.CommitTransaction();

    //            }
    //            catch (Exception ex)
    //            {
    //                throw ex;
    //            }

    //        }
    //    }

    //    public class MovementMasterData
    //    {
    //        SqlRepository _sqlRepository;
    //        ConnectionManager.clsConnectionManager ConManager;

    //        public MovementMasterData()
    //        {
    //            _sqlRepository = new SqlRepository();
    //            ConManager = new ConnectionManager.clsConnectionManager();
    //        }

    //        public IEnumerable<object> GetItem()
    //        {
    //            try
    //            {
    //                var _sql = @"select distinct Id as Value,Item as Text from dbo.MovementItems";
    //                return _sqlRepository.GetDataCollection(_sql, null);
    //            }
    //            catch (Exception)
    //            {
    //                throw;
    //            }

    //        }


    //        public IEnumerable<object> GetList(string column, string value)
    //        {
    //            try
    //            {
    //                string strkey = "1=1";
    //                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
    //                {
    //                    strkey = column + " like '%" + value + "%'";
    //                }

    //                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

    //                var sql = @"select top 100 * from (SELECT m.Id,m.FromLocation ,m.ToLocation,m.FromStorageLocId,m.ToStorageLocId,ms.UserName as FromStorageLoc, mss.UserName as ToStorageLoc,
    //                m.Inventorycheck,c.Id as CompanyId,
    //                c.UserName as Company,p.Id as PlantId,P.UserName as Plant,m.EntityId,mp.Username as MovementCategory,mp.id as PurposeId,
    //                e.UserName as Entity,m.AddedDate,
    //                m.ItemId,mi.Item from mst.MaterialMovementMaster m 
    //                left join org.Entity e on e.Id=m.EntityId
    //                left join dbo.MovementItems mi on mi.Id=m.ItemId
    //				left join org.Plant p on p.Id=e.PlantId 
    //                left join HKP.MaterialMovementPurpose mp on mp.Id = m.PurposeId
    //				left join org.Company c on c.Id=p.CompanyId
    //				left join hkp.MaterialStorage ms on ms.Id=m.FromStorageLocId
    //				left join hkp.MaterialStorage mss on mss.Id=m.ToStorageLocId) as Temp where " + strkey + " order by Temp.AddedDate desc";

    //                return _sqlRepository.GetDataCollection(sql, null);
    //            }
    //            catch (Exception)
    //            {
    //                throw;
    //            }

    //        }

    //        public DataTable GetListRpt(string column, string value)
    //        {
    //            try
    //            {
    //                string strkey = "1=1";
    //                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
    //                {
    //                    strkey = column + " like '%" + value + "%'";
    //                }

    //                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

    //                var sql = @"select top 100 * from (SELECT m.Id,m.FromLocation ,m.ToLocation,m.FromStorageLocId,m.ToStorageLocId,ms.UserName as FromStorageLoc,
    //                mss.UserName as ToStorageLoc,
    //                m.Inventorycheck,c.Id as CompanyId,
    //                c.UserName as Company,p.Id as PlantId,P.UserName as Plant,m.EntityId,mp.Username as MovementCategory,mp.id as PurposeId,
    //                e.UserName as Entity,m.AddedDate,
    //                m.ItemId,mi.Item from mst.MaterialMovementMaster m 
    //                left join org.Entity e on e.Id=m.EntityId
    //                left join dbo.MovementItems mi on mi.Id=m.ItemId
    //				left join org.Plant p on p.Id=e.PlantId 
    //                left join HKP.MaterialMovementPurpose mp on mp.Id = m.PurposeId
    //				left join org.Company c on c.Id=p.CompanyId
    //				left join hkp.MaterialStorage ms on ms.Id=m.FromStorageLocId
    //				left join hkp.MaterialStorage mss on mss.Id=m.ToStorageLocId) as Temp where " + strkey + " order by Temp.AddedDate desc";

    //                return _sqlRepository.GetDataTable(sql);
    //            }
    //            catch (Exception)
    //            {
    //                throw;
    //            }

    //        }

    //        public IEnumerable<object> GetStorageLoc(string PlantId, string CompId)
    //        {
    //            try
    //            {
    //                var _sql = @"SELECT UserName AS Text,Id as Value 
    //                FROM HKP.MaterialStorage where PlantId='" + PlantId + "' and CompanyId='" + CompId + "'";
    //                return _sqlRepository.GetDataCollection(_sql, null);
    //            }
    //            catch (Exception)
    //            {
    //                throw;
    //            }

    //        }

    //        public IEnumerable<object> getPurposeCategory()
    //        {
    //            try
    //            {
    //                var _sql = @"SELECT UserName AS Text,Id as Value 
    //                FROM HKP.MaterialMovementPurpose ";
    //                return _sqlRepository.GetDataCollection(_sql, null);
    //            }
    //            catch (Exception)
    //            {
    //                throw;
    //            }

    //        }


    //        public IEnumerable<object> LoadAll(string Id)
    //        {
    //            try
    //            {
    //                var _sql = @"select m.Id,m.FromLocation,m.ToLocation,m.Inventorycheck,m.FromStorageLocId,ms.UserName as FromStorageLoc,m.ToStorageLocId,mss.UserName as ToStorageLoc,mp.Username as MovementCategory, mp.Id as PurposeId,
    //                c.Id as CompanyId,c.UserName as Company,p.Id as PlantId,
    //                P.UserName as Plant,m.EntityId,
    //                e.UserName as Entity, m.ItemId,mi.Item from mst.MaterialMovementMaster m 
    //                left join org.Entity e on e.Id=m.EntityId
    //                left join dbo.MovementItems mi on mi.Id=m.ItemId
    //				left join org.Plant p on p.Id=e.PlantId 
    //				left join org.Company c on c.Id=p.CompanyId
    //                left join HKP.MaterialMovementPurpose mp on mp.Id = m.PurposeId
    //				left join hkp.MaterialStorage ms on ms.Id=m.FromStorageLocId
    //                left join hkp.MaterialStorage mss on mss.Id=m.ToStorageLocId
    //                where m.Id='" + Id + "'";

    //                return _sqlRepository.GetDataCollection(_sql, null);
    //            }
    //            catch (Exception)
    //            {
    //                throw;
    //            }

    //        }



    //        public void saveData(MovementMasterList masterdata)
    //        {
    //            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

    //            DataSet dsMaster;
    //            try
    //            {
    //                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
    //                con.OpenDataSetThroughAdapter("select * from MST.MaterialMovementMaster where FromLocation='" + masterdata.FromLocation + "' AND EntityId='" + masterdata.EntityId + "'AND ItemId='" + masterdata.ItemId + "'AND ToLocation='" + masterdata.ToLocation + "' AND PurposeId='" + masterdata.PurposeId + "' AND Id<>'" + masterdata.Id + "'", out dsMaster, false, "1");
    //                if (dsMaster.Tables[0].Rows.Count > 0)
    //                    throw new Exception("Same Data already exists!!!");

    //                con.OpenDataSetThroughAdapter("select * from MST.MaterialMovementMaster where Id='" + masterdata.Id + "'", out dsMaster, false, "1");




    //                string _Id = "";
    //                string MasterID = "";

    //                if (dsMaster.Tables[0].Rows.Count == 0)
    //                {
    //                    bplib.clsGenID genid = new bplib.clsGenID();
    //                    genid.GenID("MaterialMovementMaster", out _Id);

    //                    MasterID = "MM" + _Id;
    //                    DataRow dr = dsMaster.Tables[0].NewRow();
    //                    dr["Id"] = "MM" + _Id;
    //                    dr["FromLocation"] = masterdata.FromLocation;
    //                    dr["ToLocation"] = masterdata.ToLocation;
    //                    if (masterdata.FromStorageLocId == "")
    //                    {
    //                        dr["FromStorageLocId"] = DBNull.Value;
    //                    }
    //                    else
    //                    {
    //                        dr["FromStorageLocId"] = masterdata.FromStorageLocId;
    //                    }
    //                    if (masterdata.ToStorageLocId == "")
    //                    {
    //                        dr["ToStorageLocId"] = DBNull.Value;
    //                    }
    //                    else
    //                    {
    //                        dr["ToStorageLocId"] = masterdata.ToStorageLocId;
    //                    }
    //                    dr["ItemId"] = masterdata.ItemId;
    //                    dr["EntityId"] = masterdata.EntityId;
    //                    dr["EntityId"] = masterdata.EntityId;
    //                    dr["Tagtype"] = "-";
    //                    dr["Inventorycheck"] = masterdata.Inventorycheck;
    //                    dr["PurposeId"] = masterdata.PurposeId;
    //                    dr["AddedBy"] = identity.Name;
    //                    dr["AddedDate"] = DateTime.Now.ToString();
    //                    dr["UpdatedBy"] = identity.Name;
    //                    dr["UpdatedDate"] = DateTime.Now.ToString();

    //                    dsMaster.Tables[0].Rows.Add(dr);

    //                }
    //                else
    //                {
    //                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
    //                    MasterID = dr["Id"].ToString();
    //                    dr.BeginEdit();

    //                    dr["FromLocation"] = masterdata.FromLocation;
    //                    dr["ToLocation"] = masterdata.ToLocation;
    //                    dr["FromStorageLocId"] = masterdata.FromStorageLocId;
    //                    dr["ToStorageLocId"] = masterdata.ToStorageLocId;
    //                    dr["ItemId"] = masterdata.ItemId;
    //                    dr["EntityId"] = masterdata.EntityId;
    //                    dr["Tagtype"] = "-";
    //                    dr["PurposeId"] = masterdata.PurposeId;
    //                    dr["Inventorycheck"] = masterdata.Inventorycheck;

    //                    dr["UpdatedBy"] = identity.Name;
    //                    dr["UpdatedDate"] = DateTime.Now.ToString();

    //                    dr.EndEdit();

    //                }




    //                clsStaticInfo obj1 = new clsStaticInfo();
    //                obj1.SaveDataSets(dsMaster);//, dsEmployee);
    //            }
    //            catch (Exception ex)
    //            {

    //                throw (ex);
    //            }
    //        }

    //        public void deleteemployee(MovementMasterList masterdata, string employeedata)
    //        {
    //            DataSet dsMaster;
    //            string sql = " SELECT * FROM dbo.MovementResponsiblePerson WHERE MasterId='" + masterdata.Id + "' AND EmpSystemId='" + employeedata + "'";
    //            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
    //            objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

    //            while (dsMaster.Tables[0].DefaultView.Count > 0)
    //                dsMaster.Tables[0].DefaultView[0].Delete();

    //            clsStaticInfo clsStatic = new clsStaticInfo();
    //            clsStatic.SaveDataSets(dsMaster);

    //        }

    //        public void delete(string id)
    //        {
    //            ConnectionManager.DAL.ConManager objCon;

    //            objCon = new ConnectionManager.DAL.ConManager("1");
    //            objCon.BeginTransaction();
    //            objCon.ExecuteNonQueryWrapper("delete FROM mst.MaterialMovementMaster where Id='" + id + "'", true, "1");

    //            objCon.CommitTransaction();

    //        }

    //    }

    //    public class MovementMasterList
    //    {
    //        public string Id { get; set; } = "";
    //        public string PurposeId { get; set; } = "";
    //        public string FromLocation { get; set; } = "";
    //        public string ToLocation { get; set; } = "";
    //        public string ItemId { get; set; } = "";
    //        public string EntityId { get; set; } = "";
    //        public string FromStorageLocId { get; set; } = "";
    //        public string ToStorageLocId { get; set; } = "";
    //        public string Tagtype { get; set; } = "";
    //        public string Inventorycheck { get; set; } = "";
    //    }

    //    public class WeighingScaleData
    //    {
    //        SqlRepository _sqlRepository;
    //        ConnectionManager.clsConnectionManager ConManager;

    //        public WeighingScaleData()
    //        {
    //            _sqlRepository = new SqlRepository();
    //            ConManager = new ConnectionManager.clsConnectionManager();

    //        }

    //        public IEnumerable<object> GetStatus()
    //        {
    //            try
    //            {
    //                var _sql = @"select distinct Id as Value,UserName as Text from
    //                hkp.ProductionStatus";
    //                return _sqlRepository.GetDataCollection(_sql, null);
    //            }
    //            catch (Exception)
    //            {
    //                throw;
    //            }

    //        }

    //        public IEnumerable<object> GetData(out List<string> ExtraColumns, string Status, string purp)
    //        {
    //            try
    //            {
    //                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

    //                var _sql = @"SELECT distinct pl.Code as ProductCode,(Case when po.Id is null then p.Id else Po.id end) as PO,sc.LotNo,
    //                ps.UserName as OrderStatus,moi.MasterOrderId as MasterOrderNo,
    //                so.Id as SO,pl.Remarks,ma.Code as MaterialCode,ma.UserName as Material,ma.Id as MaterialId,PM.UserName as Product , prod.UserName as Prod,PM.Id as ProductId,
    //                pl.Id as ProductLibId,so.Qty as SOQty,Format(so.DeliveryDate,'dd-MMM-yyyy')DeliveryDate, par.UserName as Customer,par.Id as CustomerId,
    //                si.UserName as Attribute,pa.AttributeValue              
    //                FROM DBO.ItemScanChild sc
    //				left join trn.ProductionOrder po on sc.Poid = po.Id
    //				full outer join dbo.ProductLibrary pl on pl.Code=sc.ProductCode
    //                left join dbo.ProductLibraryAttribute pa on pa.ProductLibraryId=pl.Id
    //				full outer join trn.MasterOrderItem moi on moi.ProductLibraryId=pl.Id
    //                left join trn.MasterOrder mo on mo.Id=moi.MasterOrderId              
    //                left join trn.SalesOrder so on so.MasterOrderItemId = moi.Id
    //				left join trn.ProductionOrderDetail pod on pod.SalesOrderId = so.Id
    //                left join trn.ProductionOrder p on p.Id=pod.ProductionOrderId
    //                 left join hkp.ProductionStatus ps on ps.Id=p.ProductionStatusId
    //                 left join dbo.ItemScan s on s.Id=sc.MasterId
    //                 left join mst.MaterialMaster ma on ma.Id=moi.MaterialMasterId
    //                 left join trn.ProductDefinition AS pd ON pd.MaterialMasterId=ma.Id
    //                 left join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId     
    //                left join hkp.Party par on par.Id=mo.PartyId
    //                left join dbo.ScanItem si on si.Id=pa.ScanItemId     
    //                left join hkp.Product prod on prod.Id = pm.ProductId
    //                where p.ProductionStatusId='" + Status + "' and p.PlantId='" + identity.PlantId + "' and pa.ScanItemId !='null' and p.Id !='null' and pl.Code !='null'";
    //                DataTable dt = _sqlRepository.GetDataTable(_sql);


    //                DataTable dtfinal = dt.Clone();
    //                DataTable dtattb = dt.DefaultView.ToTable(true, "Attribute");
    //                for (int i = 0; i < dtattb.Rows.Count; i++)
    //                {
    //                    dtfinal.Columns.Add(dtattb.Rows[i]["Attribute"].ToString());
    //                }

    //                string ProdId = "", POxId = "";
    //                DataRow dr = null;
    //                ExtraColumns = new List<string>();
    //                List<string> stringtxt = new List<string>();

    //                if (purp == "For User")
    //                {

    //                    for (int i = 0; i < dt.Rows.Count; i++)
    //                    {
    //                        if (dt.Rows[i]["ProductCode"].ToString() != ProdId || dt.Rows[i]["PO"].ToString() != POxId)
    //                        {
    //                            dr = dtfinal.NewRow();
    //                            dr["ProductCode"] = dt.Rows[i]["ProductCode"];
    //                            dr["PO"] = dt.Rows[i]["PO"];
    //                            dr["LotNo"] = dt.Rows[i]["LotNo"];
    //                            dr["Material"] = dt.Rows[i]["Material"];
    //                            dr["MaterialCode"] = dt.Rows[i]["MaterialCode"];
    //                            dr["MaterialId"] = dt.Rows[i]["MaterialId"];
    //                            dr["Product"] = dt.Rows[i]["Product"];
    //                            dr["ProductId"] = dt.Rows[i]["ProductId"];
    //                            dr["DeliveryDate"] = dt.Rows[i]["DeliveryDate"];
    //                            dr["Customer"] = dt.Rows[i]["Customer"];
    //                            dr["CustomerId"] = dt.Rows[i]["CustomerId"];
    //                            dr["OrderStatus"] = dt.Rows[i]["OrderStatus"];
    //                            dr["SO"] = dt.Rows[i]["SO"];
    //                            dr["SOQty"] = dt.Rows[i]["SOQty"];
    //                            dr["MasterOrderNo"] = dt.Rows[i]["MasterOrderNo"];
    //                            dr["Remarks"] = dt.Rows[i]["Remarks"];
    //                            dr["ProductLibId"] = dt.Rows[i]["ProductLibId"];
    //                            int j = i;
    //                            while (dr["ProductCode"].ToString() == dt.Rows[j]["ProductCode"].ToString()
    //                                && dr["PO"].ToString() == dt.Rows[j]["PO"].ToString()
    //                                )
    //                            {
    //                                string kk = dt.Rows[j]["Attribute"].ToString();
    //                                dr[kk] = dt.Rows[j]["AttributeValue"].ToString();
    //                                j++;

    //                                stringtxt.Add(kk.ToString());

    //                                ExtraColumns = stringtxt.Distinct().ToList();

    //                                if (j == dt.Rows.Count)
    //                                {
    //                                    break;
    //                                }
    //                            }


    //                            dtfinal.Rows.Add(dr);

    //                        }

    //                        ProdId = dt.Rows[i]["ProductCode"].ToString();
    //                        POxId = dt.Rows[i]["PO"].ToString();

    //                    }
    //                }
    //                else
    //                {
    //                    for (int i = 0; i < dt.Rows.Count; i++)
    //                    {
    //                        if (dt.Rows[i]["ProductCode"].ToString() != ProdId && dt.Rows[i]["PO"].ToString() != POxId)
    //                        {
    //                            dr = dtfinal.NewRow();
    //                            dr["ProductCode"] = dt.Rows[i]["ProductCode"];
    //                            dr["PO"] = dt.Rows[i]["PO"];
    //                            dr["LotNo"] = dt.Rows[i]["LotNo"];
    //                            dr["Material"] = dt.Rows[i]["Material"];
    //                            dr["MaterialCode"] = dt.Rows[i]["MaterialCode"];
    //                            dr["MaterialId"] = dt.Rows[i]["MaterialId"];
    //                            dr["Product"] = dt.Rows[i]["Product"];
    //                            dr["ProductId"] = dt.Rows[i]["ProductId"];
    //                            dr["DeliveryDate"] = dt.Rows[i]["DeliveryDate"];
    //                            dr["Customer"] = dt.Rows[i]["Customer"];
    //                            dr["CustomerId"] = dt.Rows[i]["CustomerId"];
    //                            dr["OrderStatus"] = dt.Rows[i]["OrderStatus"];
    //                            dr["SO"] = dt.Rows[i]["SO"];
    //                            dr["SOQty"] = dt.Rows[i]["SOQty"];
    //                            dr["MasterOrderNo"] = dt.Rows[i]["MasterOrderNo"];
    //                            dr["Remarks"] = dt.Rows[i]["Remarks"];
    //                            dr["ProductLibId"] = dt.Rows[i]["ProductLibId"];
    //                            int j = i;
    //                            while (dr["ProductCode"].ToString() == dt.Rows[j]["ProductCode"].ToString()
    //                                && dr["PO"].ToString() == dt.Rows[j]["PO"].ToString()
    //                                )
    //                            {
    //                                string kk = dt.Rows[j]["Attribute"].ToString();
    //                                dr[kk] = dt.Rows[j]["AttributeValue"].ToString();
    //                                j++;

    //                                stringtxt.Add(kk.ToString());

    //                                ExtraColumns = stringtxt.Distinct().ToList();

    //                                if (j == dt.Rows.Count)
    //                                {
    //                                    break;
    //                                }
    //                            }


    //                            dtfinal.Rows.Add(dr);

    //                        }

    //                        ProdId = dt.Rows[i]["ProductCode"].ToString();
    //                        POxId = dt.Rows[i]["PO"].ToString();

    //                    }
    //                }

    //                return Helpers.DataTableExtensions.DataTableToJson(dtfinal);
    //            }
    //            catch (Exception)
    //            {
    //                throw;
    //            }
    //        }

    //        public DataTable GetReportData(out List<string> ExtraColumns, string Status, string SO, string ProductCode, string PO, string Product, string Material, string LotNo, string MaterialCode, string Customer, string MasterOrderNo, string purp)
    //        {
    //            try
    //            {
    //                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

    //                var sql = @"SELECT req.* from (select distinct pl.Code as ProductCode,
    //                (Case when po.Id is null then p.Id else Po.id end) as PO,sc.LotNo,
    //                ps.UserName as OrderStatus,moi.MasterOrderId as MasterOrderNo,
    //                so.Id as SO,pl.Remarks,ma.Code as MaterialCode,ma.UserName as Material,ma.Id as MaterialId,PM.UserName as Product , prod.UserName as Prod,PM.Id as ProductId,
    //                pl.Id as ProductLibId,so.Qty as SOQty,Format(so.DeliveryDate,'dd-MMM-yyyy')DeliveryDate, par.UserName as Customer,par.Id as CustomerId,
    //                si.UserName as Attribute,pa.AttributeValue,si.Id as ScanItemId,ps.Id as ProductionStatusId,             
    //                (Case when po.Id is null then p.PlantId else Po.PlantId end) as Plant				                
    //                FROM DBO.ItemScanChild sc
    //                left join trn.ProductionOrder po on sc.Poid = po.Id
    //				full outer join dbo.ProductLibrary pl on pl.Code=sc.ProductCode
    //                left join dbo.ProductLibraryAttribute pa on pa.ProductLibraryId=pl.Id
    //				full outer join trn.MasterOrderItem moi on moi.ProductLibraryId=pl.Id
    //                left join trn.MasterOrder mo on mo.Id=moi.MasterOrderId              
    //                left join trn.SalesOrder so on so.MasterOrderItemId = moi.Id
    //				left join trn.ProductionOrderDetail pod on pod.SalesOrderId = so.Id
    //                left join trn.ProductionOrder p on p.Id=pod.ProductionOrderId
    //                left join hkp.ProductionStatus ps on ps.Id=p.ProductionStatusId
    //                left join dbo.ItemScan s on s.Id=sc.MasterId
    //                left join mst.MaterialMaster ma on ma.Id=moi.MaterialMasterId
    //                left join trn.ProductDefinition AS pd ON pd.MaterialMasterId=ma.Id
    //                left join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId     
    //                left join hkp.Party par on par.Id=mo.PartyId
    //                left join dbo.ScanItem si on si.Id=pa.ScanItemId     
    //                left join hkp.Product prod on prod.Id = pm.ProductId) as req
    //                where ProductionStatusId='" + Status + "' and Plant='" + identity.PlantId + @"' and isnull(PO ,'') IN(" + PO + @") 
    //                and isnull(LotNo,'') IN(" + LotNo + @")
    //                and isnull(MaterialCode ,'') IN(" + MaterialCode + @") and isnull(SO,'') IN(" + SO + @")
    //                and isnull(ProductCode ,'') IN(" + ProductCode + @") and isnull(ProductId,'') IN(" + Product + @")
    //                and isnull(MaterialId,'') IN(" + Material + @") and isnull(MasterOrderNo,'') IN(" + MasterOrderNo + @")
    //                and isnull(CustomerId ,'') IN(" + Customer + @") and ScanItemId !='null'
    //				and PO !='null' and ProductCode !='null'";


    //                DataTable dt = _sqlRepository.GetDataTable(sql);

    //                DataTable dtfinal = dt.Clone();
    //                DataTable dtattb = dt.DefaultView.ToTable(true, "Attribute");
    //                for (int i = 0; i < dtattb.Rows.Count; i++)
    //                {
    //                    dtfinal.Columns.Add(dtattb.Rows[i]["Attribute"].ToString());
    //                }

    //                string ProdId = "", POxId = "";
    //                DataRow dr = null;
    //                ExtraColumns = new List<string>();
    //                List<string> stringtxt = new List<string>();
    //                if (purp == "For User")
    //                {

    //                    for (int i = 0; i < dt.Rows.Count; i++)
    //                    {
    //                        if (dt.Rows[i]["ProductCode"].ToString() != ProdId || dt.Rows[i]["PO"].ToString() != POxId)
    //                        {
    //                            dr = dtfinal.NewRow();
    //                            dr["ProductCode"] = dt.Rows[i]["ProductCode"];
    //                            dr["PO"] = dt.Rows[i]["PO"];
    //                            dr["LotNo"] = dt.Rows[i]["LotNo"];
    //                            dr["Material"] = dt.Rows[i]["Material"];
    //                            dr["MaterialCode"] = dt.Rows[i]["MaterialCode"];
    //                            dr["Product"] = dt.Rows[i]["Product"];
    //                            dr["DeliveryDate"] = dt.Rows[i]["DeliveryDate"];
    //                            dr["Customer"] = dt.Rows[i]["Customer"];
    //                            dr["OrderStatus"] = dt.Rows[i]["OrderStatus"];
    //                            dr["SOQty"] = dt.Rows[i]["SOQty"];
    //                            dr["SO"] = dt.Rows[i]["SO"];
    //                            dr["MasterOrderNo"] = dt.Rows[i]["MasterOrderNo"];
    //                            dr["Remarks"] = dt.Rows[i]["Remarks"];
    //                            int j = i;
    //                            while (dr["ProductCode"].ToString() == dt.Rows[j]["ProductCode"].ToString()
    //                               && dr["PO"].ToString() == dt.Rows[j]["PO"].ToString()
    //                               )
    //                            {
    //                                string kk = dt.Rows[j]["Attribute"].ToString();
    //                                dr[kk] = dt.Rows[j]["AttributeValue"].ToString();
    //                                j++;

    //                                stringtxt.Add(kk.ToString());

    //                                ExtraColumns = stringtxt.Distinct().ToList();

    //                                if (j == dt.Rows.Count)
    //                                {
    //                                    break;
    //                                }
    //                            }


    //                            dtfinal.Rows.Add(dr);

    //                        }

    //                        ProdId = dt.Rows[i]["ProductCode"].ToString();
    //                        POxId = dt.Rows[i]["PO"].ToString();

    //                    }
    //                }
    //                else
    //                {
    //                    for (int i = 0; i < dt.Rows.Count; i++)
    //                    {
    //                        if (dt.Rows[i]["ProductCode"].ToString() != ProdId && dt.Rows[i]["PO"].ToString() != POxId)
    //                        {
    //                            dr = dtfinal.NewRow();
    //                            dr["ProductCode"] = dt.Rows[i]["ProductCode"];
    //                            dr["PO"] = dt.Rows[i]["PO"];
    //                            dr["OrderStatus"] = dt.Rows[i]["OrderStatus"];

    //                            int j = i;
    //                            while (dr["ProductCode"].ToString() == dt.Rows[j]["ProductCode"].ToString()
    //                                && dr["PO"].ToString() == dt.Rows[j]["PO"].ToString()
    //                                )
    //                            {
    //                                string kk = dt.Rows[j]["Attribute"].ToString();
    //                                dr[kk] = dt.Rows[j]["AttributeValue"].ToString();
    //                                j++;

    //                                stringtxt.Add(kk.ToString());

    //                                ExtraColumns = stringtxt.Distinct().ToList();

    //                                if (j == dt.Rows.Count)
    //                                {
    //                                    break;
    //                                }
    //                            }


    //                            dtfinal.Rows.Add(dr);

    //                        }

    //                        ProdId = dt.Rows[i]["ProductCode"].ToString();
    //                        POxId = dt.Rows[i]["PO"].ToString();

    //                    }
    //                }

    //                return dtfinal;

    //            }
    //            catch (Exception ex)
    //            {
    //                throw ex;
    //            }
    //        }

    //    }

    //    public class MovementScanData
    //    {
    //        SqlRepository _sqlRepository;
    //        ConnectionManager.clsConnectionManager ConManager;

    //        public MovementScanData()
    //        {
    //            _sqlRepository = new SqlRepository();
    //            ConManager = new ConnectionManager.clsConnectionManager();

    //        }

    //        public IEnumerable<object> GetEntity()
    //        {
    //            try
    //            {
    //                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
    //                var _sql = @"select UserName as Text,Id as Value from org.Entity where CompanyId='" + identity.CompanyId + "'" +
    //                    " and PlantId='" + identity.PlantId + "'";
    //                return _sqlRepository.GetDataCollection(_sql, null);
    //            }
    //            catch (Exception)
    //            {
    //                throw;
    //            }

    //        }

    //        public IEnumerable<object> GetFrom(string EntityId, string PurposeId)
    //        {
    //            try
    //            {
    //                var _sql = @"Select distinct FromLocation as Value , FromLocation as Text  from mst.materialmovementMaster where PurposeId = '" + PurposeId + "' and EntityId = '" + EntityId + "' ";
    //                return _sqlRepository.GetDataCollection(_sql, null);
    //            }
    //            catch (Exception)
    //            {
    //                throw;
    //            }

    //        }

    //        public IEnumerable<object> GetTo(string EntityId, string PurposeId, string FromId)
    //        {
    //            try
    //            {
    //                var _sql = @"Select distinct ToLocation as Value,ToLocation as Text from mst.MaterialMovementMaster where PurposeId = '" + PurposeId + "' and EntityId = '" + EntityId + "' and FromLocation = '" + FromId + "'";
    //                return _sqlRepository.GetDataCollection(_sql, null);
    //            }
    //            catch (Exception)
    //            {
    //                throw;
    //            }

    //        }


    //        public IEnumerable<object> getPurposeCategory()
    //        {
    //            try
    //            {
    //                var _sql = @"SELECT UserName AS Text,Id as Value 
    //                FROM HKP.MaterialMovementPurpose ";
    //                return _sqlRepository.GetDataCollection(_sql, null);
    //            }
    //            catch (Exception)
    //            {
    //                throw;
    //            }

    //        }


    //        public IEnumerable<object> GetData(string FromLoc, string ToLoc, string FromDate, string ToDate, string PurposeId, string EntityId)
    //        {
    //            try
    //            {
    //                TimeSpan ts = Convert.ToDateTime(ToDate).Subtract(Convert.ToDateTime(FromDate));
    //                if (ts.Days >= 0)
    //                {
    //                    string strkey = "";
    //                    if (FromLoc != "null" && ToLoc != "null")
    //                    {
    //                        strkey = "(m.FromLocation = '" + FromLoc + "' AND m.ToLocation = '" + ToLoc + "')";
    //                    }
    //                    else if (FromLoc != "null" && ToLoc == "null")
    //                    {
    //                        strkey = "(m.FromLocation = '" + FromLoc + "') ";
    //                    }
    //                    else if (FromLoc == "null" && ToLoc != "null")
    //                    {
    //                        strkey = "(m.ToLocation = '" + ToLoc + "') ";
    //                    }
    //                    else
    //                    {
    //                        strkey = "1=1";
    //                    }

    //                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
    //                    string _sql = @"select s.ProductCode,Format(sc.Time,'hh:mm tt') as Time,Format(sc.WorkDate,'dd-MMM-yyyy') as WorkDate,sc.ShiftId,sh.UserName as Shift,
    //                      sc.Grade,mp.UserName as Purpose,s.Cones,s.GWeight,s.NetWeight,s.LotNo,s.RefNo,s.Shade,
    //                      e.EmployeeName as PackedBy,s.POId as PO,p.ProductionStatusId as OrderStatusId,
    //                      a.StandardName
    //					  as Article,a.Code as ArticleCode,a.Id as ArticleId,ps.UserName
    //                      As OrderStatus,m.FromLocation as FromLoc,m.ToLocation as ToLoc,m.Id as FromId , mp.Id as PurposeId , mp.UserName as Purpose from dbo.ItemScanChild s 
    //                      join dbo.ItemScan sc on sc.Id=s.MasterId
    //                      left join dbo.EmployeeInformation e on e.SystemId=s.PackedBy
    //                      left join dbo.ShiftDefination sh on sc.ShiftId=sh.SystemID
    //                      left join trn.ProductionOrder p on p.Id=s.POId
    //                      join dbo.ProductLibrary pl on pl.Code=s.ProductCode
    //					  LEFT join mst.MaterialMasterArticle a on a.Id=pl.ArticleId
    //					  left join hkp.ProductionStatus ps on ps.Id=p.ProductionStatusId
    //                      left join hkp.MaterialMovementPurpose mp on mp.Id = sc.PurposeId
    //                      join mst.MaterialMovementMaster m on m.Id= sc.LocMasterId

    //                      where (sc.WorkDate between '" + FromDate + "' and '" + ToDate + "') and " + strkey + " and m.EntityId = '" + EntityId + @"' and mp.Id = '" + PurposeId + "' and p.PlantId='" + identity.PlantId + "' order by sc.Time desc";

    //                    return _sqlRepository.GetDataCollection(_sql, null);
    //                }
    //                else
    //                {
    //                    throw new Exception("Please choose a valid Date !!");
    //                }
    //            }
    //            catch (Exception)
    //            {
    //                throw;
    //            }
    //        }

    //        public DataTable GetReportData(string From, string To, string FromLoc, string ToLoc, string EntityId,
    //            string Shade, string ShiftId, string ProductCode, string PO, string Cones, string RefNo, string LotNo,
    //            string PackedBy, string Grade, string OrderStatusId, string Date, string Article, string ArticleCode, string PurposeId)
    //        {
    //            try
    //            {


    //                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
    //                string _sql = @"select s.ProductCode,Format(sc.Time,'hh:mm tt') as Time,Format(sc.WorkDate,'dd-MMM-yyyy') as WorkDate,sc.ShiftId,sh.UserName as Shift,
    //                      sc.Grade,s.Cones,s.GWeight,s.NetWeight,s.LotNo,s.RefNo,s.Shade,
    //                      e.EmployeeName as PackedBy,s.POId as PO,p.ProductionStatusId as OrderStatusId,
    //                      a.StandardName
    //					  as Article,a.Code as ArticleCode,ps.UserName
    //                      As OrderStatus,m.FromLocation as FromLoc,m.ToLocation as ToLoc,m.Id as FromId , mp.Id as PurposeId , mp.UserName as Purpose from dbo.ItemScanChild s 
    //                      join dbo.ItemScan sc on sc.Id=s.MasterId
    //                      left join dbo.EmployeeInformation e on e.SystemId=s.PackedBy
    //                      left join dbo.ShiftDefination sh on sc.ShiftId=sh.SystemID
    //                      left join trn.ProductionOrder p on p.Id=s.POId
    //                      join dbo.ProductLibrary pl on pl.Code=s.ProductCode
    //					  LEFT join mst.MaterialMasterArticle a on a.Id=pl.ArticleId
    //					  left join hkp.ProductionStatus ps on ps.Id=p.ProductionStatusId
    //                      left join hkp.MaterialMovementPurpose mp on mp.Id = sc.PurposeId
    //                      join mst.MaterialMovementMaster m on m.Id= sc.LocMasterId

    //                      where sc.WorkDate " +
    //                    "between '" + From + "' and '" + To + "'  and p.PlantId='" + identity.PlantId + @"'
    //                    and isnull(s.POId ,'') IN(" + PO + @") 
    //                    and isnull(s.LotNo,'') IN(" + LotNo + @") 
    //                    and isnull(s.Cones ,'') IN(" + Cones + @")
    //                    and isnull(s.RefNo,'') IN(" + RefNo + @") 
    //                    and isnull(s.Shade ,'') IN(" + Shade + @") 
    //                    and isnull(sc.Grade,'') IN(" + Grade + @") 
    //                    and isnull(s.ProductCode,'') IN(" + ProductCode + @") 
    //                    and isnull(sc.ShiftId,'') IN(" + ShiftId + @") 
    //                    and isnull(p.ProductionStatusId,'') IN(" + OrderStatusId + @") 
    //                    and isnull(e.EmployeeName,'') IN(" + PackedBy + @")  
    //                    and isnull(sc.WorkDate,'') IN(" + Date + @") 
    //                    and isnull(a.Code,'') IN(" + ArticleCode + @") 
    //                    and isnull(a.Id,'') IN(" + Article + @") 
    //                    and isnull(m.FromLocation,'') IN (" + FromLoc + @")
    //                    and isnull(m.ToLocation,'') IN (" + ToLoc + @")
    //                    and isnull(mp.Id,'') IN (" + PurposeId + @")
    //                    and m.EntityId = '" + EntityId + @"'
    //";

    //                return _sqlRepository.GetDataTable(_sql);
    //            }
    //            catch (Exception)
    //            {
    //                throw;
    //            }
    //        }

    //    }
    //}
    public class MovementItemsData
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
        string TableName = "dbo.MovementItems";

        public MovementItemsData()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        public IEnumerable<object> Get(string Id)
        {
            try
            {
                var _sql = @"select * from dbo.MovementItems where Id ='" + Id + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> GetList(string column, string value)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select top 100 * from(select distinct est.*, uom.Username as UOM 
                from dbo.MovementItems est left join SCS.UnitOfMeasurement uom on est.UOMId = uom.Id) 
                AS TEMP WHERE " + strkey + " order by Item";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

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

        public void EditRow(DataRow dr, Dictionary<string, object> sourceData)
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

        public void Create(Dictionary<string, object> data)
        {
            try
            {

                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Item='" + data["Item"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Item already exists!!!");


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "MI" + _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }

    public class MovementMasterData
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public MovementMasterData()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        public IEnumerable<object> GetItem()
        {
            try
            {
                var _sql = @"select distinct Id as Value,Item as Text from dbo.MovementItems";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> GetEntity(string PlantId, string CompId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var _sql = @"select UserName as Text,Id as Value from org.Entity where CompanyId='" + CompId + "'" +
                    " and PlantId='" + PlantId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }
        public IEnumerable<object> GetList(string column, string value)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                {
                    strkey = column + " like '%" + value + "%'";
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"select top 100 * from (SELECT m.Id,m.FromLocation ,m.ToLocation,m.FromStorageLocId,m.ToStorageLocId,ms.UserName as FromStorageLoc, mss.UserName as ToStorageLoc,
                m.Inventorycheck,c.Id as CompanyId,
                c.UserName as Company,p.Id as PlantId,P.UserName as Plant,m.EntityId,mp.Username as MovementCategory,mp.id as PurposeId,
                e.UserName as Entity,m.AddedDate,
                m.ItemId,mi.Item from mst.MaterialMovementMaster m 
                left join org.Entity e on e.Id=m.EntityId
                left join dbo.MovementItems mi on mi.Id=m.ItemId
				left join org.Plant p on p.Id=e.PlantId 
                left join HKP.MaterialMovementPurpose mp on mp.Id = m.PurposeId
				left join org.Company c on c.Id=p.CompanyId
				left join hkp.MaterialStorage ms on ms.Id=m.FromStorageLocId
				left join hkp.MaterialStorage mss on mss.Id=m.ToStorageLocId) as Temp where " + strkey + " order by Temp.AddedDate desc";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public DataTable GetListRpt(string column, string value)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                {
                    strkey = column + " like '%" + value + "%'";
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"select top 100 * from (SELECT m.Id,m.FromLocation ,m.ToLocation,m.FromStorageLocId,m.ToStorageLocId,ms.UserName as FromStorageLoc,
                mss.UserName as ToStorageLoc,
                m.Inventorycheck,c.Id as CompanyId,
                c.UserName as Company,p.Id as PlantId,P.UserName as Plant,m.EntityId,mp.Username as MovementCategory,mp.id as PurposeId,
                e.UserName as Entity,m.AddedDate,
                m.ItemId,mi.Item from mst.MaterialMovementMaster m 
                left join org.Entity e on e.Id=m.EntityId
                left join dbo.MovementItems mi on mi.Id=m.ItemId
				left join org.Plant p on p.Id=e.PlantId 
                left join HKP.MaterialMovementPurpose mp on mp.Id = m.PurposeId
				left join org.Company c on c.Id=p.CompanyId
				left join hkp.MaterialStorage ms on ms.Id=m.FromStorageLocId
				left join hkp.MaterialStorage mss on mss.Id=m.ToStorageLocId) as Temp where " + strkey + " order by Temp.AddedDate desc";

                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> GetStorageLoc(string PlantId, string CompId)
        {
            try
            {
                var _sql = @"SELECT UserName AS Text,Id as Value 
                FROM HKP.MaterialStorage where PlantId='" + PlantId + "' and CompanyId='" + CompId + "'";
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


        public IEnumerable<object> LoadAll(string Id)
        {
            try
            {
                var _sql = @"select m.Id,m.FromLocation,m.ToLocation,m.Inventorycheck,m.FromStorageLocId,ms.UserName as FromStorageLoc,m.ToStorageLocId,mss.UserName as ToStorageLoc,mp.Username as MovementCategory, mp.Id as PurposeId,
                c.Id as CompanyId,c.UserName as Company,p.Id as PlantId,
                P.UserName as Plant,m.EntityId,
                e.UserName as Entity, m.ItemId,mi.Item from mst.MaterialMovementMaster m 
                left join org.Entity e on e.Id=m.EntityId
                left join dbo.MovementItems mi on mi.Id=m.ItemId
				left join org.Plant p on p.Id=e.PlantId 
				left join org.Company c on c.Id=p.CompanyId
                left join HKP.MaterialMovementPurpose mp on mp.Id = m.PurposeId
				left join hkp.MaterialStorage ms on ms.Id=m.FromStorageLocId
                left join hkp.MaterialStorage mss on mss.Id=m.ToStorageLocId
                where m.Id='" + Id + "'";

                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }



        public void saveData(MovementMasterList masterdata)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            DataSet dsMaster;
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from MST.MaterialMovementMaster where FromLocation='" + masterdata.FromLocation + "' AND EntityId='" + masterdata.EntityId + "'AND ItemId='" + masterdata.ItemId + "'AND ToLocation='" + masterdata.ToLocation + "' AND PurposeId='" + masterdata.PurposeId + "' AND Id<>'" + masterdata.Id + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Data already exists!!!");

                con.OpenDataSetThroughAdapter("select * from MST.MaterialMovementMaster where Id='" + masterdata.Id + "'", out dsMaster, false, "1");




                string _Id = "";
                string MasterID = "";

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("MaterialMovementMaster", out _Id);

                    MasterID = "MM" + _Id;
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = "MM" + _Id;
                    dr["FromLocation"] = masterdata.FromLocation;
                    dr["ToLocation"] = masterdata.ToLocation;
                    if (masterdata.FromStorageLocId == "")
                    {
                        dr["FromStorageLocId"] = DBNull.Value;
                    }
                    else
                    {
                        dr["FromStorageLocId"] = masterdata.FromStorageLocId;
                    }
                    if (masterdata.ToStorageLocId == "")
                    {
                        dr["ToStorageLocId"] = DBNull.Value;
                    }
                    else
                    {
                        dr["ToStorageLocId"] = masterdata.ToStorageLocId;
                    }
                    dr["ItemId"] = masterdata.ItemId;
                    dr["EntityId"] = masterdata.EntityId;
                    dr["EntityId"] = masterdata.EntityId;
                    dr["Tagtype"] = "-";
                    dr["Prefix"] = masterdata.Prefix;
                    dr["StartRefNo"] = masterdata.StartRefNo;
                    dr["Inventorycheck"] = masterdata.Inventorycheck;
                    dr["PurposeId"] = masterdata.PurposeId;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now.ToString();
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();

                    dsMaster.Tables[0].Rows.Add(dr);

                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    MasterID = dr["Id"].ToString();
                    dr.BeginEdit();

                    dr["FromLocation"] = masterdata.FromLocation;
                    dr["ToLocation"] = masterdata.ToLocation;
                    dr["FromStorageLocId"] = masterdata.FromStorageLocId;
                    dr["ToStorageLocId"] = masterdata.ToStorageLocId;
                    dr["ItemId"] = masterdata.ItemId;
                    dr["EntityId"] = masterdata.EntityId;
                    dr["Tagtype"] = "-";
                    dr["PurposeId"] = masterdata.PurposeId;
                    dr["Prefix"] = masterdata.Prefix;
                    dr["StartRefNo"] = masterdata.StartRefNo;
                    dr["Inventorycheck"] = masterdata.Inventorycheck;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();

                    dr.EndEdit();

                }




                clsStaticInfo obj1 = new clsStaticInfo();
                obj1.SaveDataSets(dsMaster);//, dsEmployee);
            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }

        public void deleteemployee(MovementMasterList masterdata, string employeedata)
        {
            DataSet dsMaster;
            string sql = " SELECT * FROM dbo.MovementResponsiblePerson WHERE MasterId='" + masterdata.Id + "' AND EmpSystemId='" + employeedata + "'";
            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

            while (dsMaster.Tables[0].DefaultView.Count > 0)
                dsMaster.Tables[0].DefaultView[0].Delete();

            clsStaticInfo clsStatic = new clsStaticInfo();
            clsStatic.SaveDataSets(dsMaster);

        }

        public void delete(string id)
        {
            ConnectionManager.DAL.ConManager objCon;

            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.BeginTransaction();
            objCon.ExecuteNonQueryWrapper("delete FROM mst.MaterialMovementMaster where Id='" + id + "'", true, "1");

            objCon.CommitTransaction();

        }

    }

    public class MovementMasterList
    {
        public string Id { get; set; } = "";
        public string PurposeId { get; set; } = "";
        public string FromLocation { get; set; } = "";
        public string ToLocation { get; set; } = "";
        public string ItemId { get; set; } = "";
        public string EntityId { get; set; } = "";
        public string FromStorageLocId { get; set; } = "";
        public string ToStorageLocId { get; set; } = "";
        public string Tagtype { get; set; } = "";
        public string Prefix { get; set; } = "";
        public int StartRefNo { get; set; } = 0 ;
        public string Inventorycheck { get; set; } = "";
    }

    public class WeighingScaleData
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public WeighingScaleData()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();

        }

        public IEnumerable<object> GetStatus()
        {
            try
            {
                var _sql = @"select distinct Id as Value,UserName as Text from
                hkp.ProductionStatus";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> GetData(out List<string> ExtraColumns, string Status, string purp)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var _sql = @"SELECT distinct pl.Code as ProductCode,(Case when po.Id is null then p.Id else Po.id end) as PO,sc.LotNo,
                ps.UserName as OrderStatus,moi.MasterOrderId as MasterOrderNo,
                so.Id as SO,pl.Remarks,ma.Code as MaterialCode,ma.UserName as Material,ma.Id as MaterialId,PM.UserName as Product , prod.UserName as Prod,PM.Id as ProductId,
                pl.Id as ProductLibId,so.Qty as SOQty,Format(so.DeliveryDate,'dd-MMM-yyyy')DeliveryDate, par.UserName as Customer,par.Id as CustomerId,
                si.UserName as Attribute,pa.AttributeValue              
                FROM DBO.ItemScanChild sc
				left join trn.ProductionOrder po on sc.Poid = po.Id
				full outer join dbo.ProductLibrary pl on pl.Code=sc.ProductCode
                left join dbo.ProductLibraryAttribute pa on pa.ProductLibraryId=pl.Id
				full outer join trn.MasterOrderItem moi on moi.ProductLibraryId=pl.Id
                left join trn.MasterOrder mo on mo.Id=moi.MasterOrderId              
                left join trn.SalesOrder so on so.MasterOrderItemId = moi.Id
				left join trn.ProductionOrderDetail pod on pod.SalesOrderId = so.Id
                left join trn.ProductionOrder p on p.Id=pod.ProductionOrderId
                 left join hkp.ProductionStatus ps on ps.Id=p.ProductionStatusId
                 left join dbo.ItemScan s on s.Id=sc.MasterId
                 left join mst.MaterialMaster ma on ma.Id=moi.MaterialMasterId
                 left join trn.ProductDefinition AS pd ON pd.MaterialMasterId=ma.Id
                 left join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId     
                left join hkp.Party par on par.Id=mo.PartyId
                left join dbo.ScanItem si on si.Id=pa.ScanItemId     
                left join hkp.Product prod on prod.Id = pm.ProductId
                where p.ProductionStatusId='" + Status + "' and p.PlantId='" + identity.PlantId + "' and pa.ScanItemId !='null' and p.Id !='null' and pl.Code !='null'";
                DataTable dt = _sqlRepository.GetDataTable(_sql);


                DataTable dtfinal = dt.Clone();
                DataTable dtattb = dt.DefaultView.ToTable(true, "Attribute");
                for (int i = 0; i < dtattb.Rows.Count; i++)
                {
                    dtfinal.Columns.Add(dtattb.Rows[i]["Attribute"].ToString());
                }

                string ProdId = "", POxId = "";
                DataRow dr = null;
                ExtraColumns = new List<string>();
                List<string> stringtxt = new List<string>();

                if (purp == "For User")
                {

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i]["ProductCode"].ToString() != ProdId || dt.Rows[i]["PO"].ToString() != POxId)
                        {
                            dr = dtfinal.NewRow();
                            dr["ProductCode"] = dt.Rows[i]["ProductCode"];
                            dr["PO"] = dt.Rows[i]["PO"];
                            dr["LotNo"] = dt.Rows[i]["LotNo"];
                            dr["Material"] = dt.Rows[i]["Material"];
                            dr["MaterialCode"] = dt.Rows[i]["MaterialCode"];
                            dr["MaterialId"] = dt.Rows[i]["MaterialId"];
                            dr["Product"] = dt.Rows[i]["Product"];
                            dr["ProductId"] = dt.Rows[i]["ProductId"];
                            dr["DeliveryDate"] = dt.Rows[i]["DeliveryDate"];
                            dr["Customer"] = dt.Rows[i]["Customer"];
                            dr["CustomerId"] = dt.Rows[i]["CustomerId"];
                            dr["OrderStatus"] = dt.Rows[i]["OrderStatus"];
                            dr["SO"] = dt.Rows[i]["SO"];
                            dr["SOQty"] = dt.Rows[i]["SOQty"];
                            dr["MasterOrderNo"] = dt.Rows[i]["MasterOrderNo"];
                            dr["Remarks"] = dt.Rows[i]["Remarks"];
                            dr["ProductLibId"] = dt.Rows[i]["ProductLibId"];
                            int j = i;
                            while (dr["ProductCode"].ToString() == dt.Rows[j]["ProductCode"].ToString()
                                && dr["PO"].ToString() == dt.Rows[j]["PO"].ToString()
                                )
                            {
                                string kk = dt.Rows[j]["Attribute"].ToString();
                                dr[kk] = dt.Rows[j]["AttributeValue"].ToString();
                                j++;

                                stringtxt.Add(kk.ToString());

                                ExtraColumns = stringtxt.Distinct().ToList();

                                if (j == dt.Rows.Count)
                                {
                                    break;
                                }
                            }


                            dtfinal.Rows.Add(dr);

                        }

                        ProdId = dt.Rows[i]["ProductCode"].ToString();
                        POxId = dt.Rows[i]["PO"].ToString();

                    }
                }
                else
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i]["ProductCode"].ToString() != ProdId && dt.Rows[i]["PO"].ToString() != POxId)
                        {
                            dr = dtfinal.NewRow();
                            dr["ProductCode"] = dt.Rows[i]["ProductCode"];
                            dr["PO"] = dt.Rows[i]["PO"];
                            dr["LotNo"] = dt.Rows[i]["LotNo"];
                            dr["Material"] = dt.Rows[i]["Material"];
                            dr["MaterialCode"] = dt.Rows[i]["MaterialCode"];
                            dr["MaterialId"] = dt.Rows[i]["MaterialId"];
                            dr["Product"] = dt.Rows[i]["Product"];
                            dr["ProductId"] = dt.Rows[i]["ProductId"];
                            dr["DeliveryDate"] = dt.Rows[i]["DeliveryDate"];
                            dr["Customer"] = dt.Rows[i]["Customer"];
                            dr["CustomerId"] = dt.Rows[i]["CustomerId"];
                            dr["OrderStatus"] = dt.Rows[i]["OrderStatus"];
                            dr["SO"] = dt.Rows[i]["SO"];
                            dr["SOQty"] = dt.Rows[i]["SOQty"];
                            dr["MasterOrderNo"] = dt.Rows[i]["MasterOrderNo"];
                            dr["Remarks"] = dt.Rows[i]["Remarks"];
                            dr["ProductLibId"] = dt.Rows[i]["ProductLibId"];
                            int j = i;
                            while (dr["ProductCode"].ToString() == dt.Rows[j]["ProductCode"].ToString()
                                && dr["PO"].ToString() == dt.Rows[j]["PO"].ToString()
                                )
                            {
                                string kk = dt.Rows[j]["Attribute"].ToString();
                                dr[kk] = dt.Rows[j]["AttributeValue"].ToString();
                                j++;

                                stringtxt.Add(kk.ToString());

                                ExtraColumns = stringtxt.Distinct().ToList();

                                if (j == dt.Rows.Count)
                                {
                                    break;
                                }
                            }


                            dtfinal.Rows.Add(dr);

                        }

                        ProdId = dt.Rows[i]["ProductCode"].ToString();
                        POxId = dt.Rows[i]["PO"].ToString();

                    }
                }

                return Service.Helpers.DataTableExtensions.DataTableToJson(dtfinal);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetReportData(out List<string> ExtraColumns, string Status, string SO, string ProductCode, string PO, string Product, string Material, string LotNo, string MaterialCode, string Customer, string MasterOrderNo, string purp)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"SELECT req.* from (select distinct pl.Code as ProductCode,
                (Case when po.Id is null then p.Id else Po.id end) as PO,sc.LotNo,
                ps.UserName as OrderStatus,moi.MasterOrderId as MasterOrderNo,
                so.Id as SO,pl.Remarks,ma.Code as MaterialCode,ma.UserName as Material,ma.Id as MaterialId,PM.UserName as Product , prod.UserName as Prod,PM.Id as ProductId,
                pl.Id as ProductLibId,so.Qty as SOQty,Format(so.DeliveryDate,'dd-MMM-yyyy')DeliveryDate, par.UserName as Customer,par.Id as CustomerId,
                si.UserName as Attribute,pa.AttributeValue,si.Id as ScanItemId,ps.Id as ProductionStatusId,             
                (Case when po.Id is null then p.PlantId else Po.PlantId end) as Plant				                
                FROM DBO.ItemScanChild sc
                left join trn.ProductionOrder po on sc.Poid = po.Id
				full outer join dbo.ProductLibrary pl on pl.Code=sc.ProductCode
                left join dbo.ProductLibraryAttribute pa on pa.ProductLibraryId=pl.Id
				full outer join trn.MasterOrderItem moi on moi.ProductLibraryId=pl.Id
                left join trn.MasterOrder mo on mo.Id=moi.MasterOrderId              
                left join trn.SalesOrder so on so.MasterOrderItemId = moi.Id
				left join trn.ProductionOrderDetail pod on pod.SalesOrderId = so.Id
                left join trn.ProductionOrder p on p.Id=pod.ProductionOrderId
                left join hkp.ProductionStatus ps on ps.Id=p.ProductionStatusId
                left join dbo.ItemScan s on s.Id=sc.MasterId
                left join mst.MaterialMaster ma on ma.Id=moi.MaterialMasterId
                left join trn.ProductDefinition AS pd ON pd.MaterialMasterId=ma.Id
                left join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId     
                left join hkp.Party par on par.Id=mo.PartyId
                left join dbo.ScanItem si on si.Id=pa.ScanItemId     
                left join hkp.Product prod on prod.Id = pm.ProductId) as req
                where ProductionStatusId='" + Status + "' and Plant='" + identity.PlantId + @"' and isnull(PO ,'') IN(" + PO + @") 
                and isnull(LotNo,'') IN(" + LotNo + @")
                and isnull(MaterialCode ,'') IN(" + MaterialCode + @") and isnull(SO,'') IN(" + SO + @")
                and isnull(ProductCode ,'') IN(" + ProductCode + @") and isnull(ProductId,'') IN(" + Product + @")
                and isnull(MaterialId,'') IN(" + Material + @") and isnull(MasterOrderNo,'') IN(" + MasterOrderNo + @")
                and isnull(CustomerId ,'') IN(" + Customer + @") and ScanItemId !='null'
				and PO !='null' and ProductCode !='null'";


                DataTable dt = _sqlRepository.GetDataTable(sql);

                DataTable dtfinal = dt.Clone();
                DataTable dtattb = dt.DefaultView.ToTable(true, "Attribute");
                for (int i = 0; i < dtattb.Rows.Count; i++)
                {
                    dtfinal.Columns.Add(dtattb.Rows[i]["Attribute"].ToString());
                }

                string ProdId = "", POxId = "";
                DataRow dr = null;
                ExtraColumns = new List<string>();
                List<string> stringtxt = new List<string>();
                if (purp == "For User")
                {

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i]["ProductCode"].ToString() != ProdId || dt.Rows[i]["PO"].ToString() != POxId)
                        {
                            dr = dtfinal.NewRow();
                            dr["ProductCode"] = dt.Rows[i]["ProductCode"];
                            dr["PO"] = dt.Rows[i]["PO"];
                            dr["LotNo"] = dt.Rows[i]["LotNo"];
                            dr["Material"] = dt.Rows[i]["Material"];
                            dr["MaterialCode"] = dt.Rows[i]["MaterialCode"];
                            dr["Product"] = dt.Rows[i]["Product"];
                            dr["DeliveryDate"] = dt.Rows[i]["DeliveryDate"];
                            dr["Customer"] = dt.Rows[i]["Customer"];
                            dr["OrderStatus"] = dt.Rows[i]["OrderStatus"];
                            dr["SOQty"] = dt.Rows[i]["SOQty"];
                            dr["SO"] = dt.Rows[i]["SO"];
                            dr["MasterOrderNo"] = dt.Rows[i]["MasterOrderNo"];
                            dr["Remarks"] = dt.Rows[i]["Remarks"];
                            int j = i;
                            while (dr["ProductCode"].ToString() == dt.Rows[j]["ProductCode"].ToString()
                               && dr["PO"].ToString() == dt.Rows[j]["PO"].ToString()
                               )
                            {
                                string kk = dt.Rows[j]["Attribute"].ToString();
                                dr[kk] = dt.Rows[j]["AttributeValue"].ToString();
                                j++;

                                stringtxt.Add(kk.ToString());

                                ExtraColumns = stringtxt.Distinct().ToList();

                                if (j == dt.Rows.Count)
                                {
                                    break;
                                }
                            }


                            dtfinal.Rows.Add(dr);

                        }

                        ProdId = dt.Rows[i]["ProductCode"].ToString();
                        POxId = dt.Rows[i]["PO"].ToString();

                    }
                }
                else
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i]["ProductCode"].ToString() != ProdId && dt.Rows[i]["PO"].ToString() != POxId)
                        {
                            dr = dtfinal.NewRow();
                            dr["ProductCode"] = dt.Rows[i]["ProductCode"];
                            dr["PO"] = dt.Rows[i]["PO"];
                            dr["OrderStatus"] = dt.Rows[i]["OrderStatus"];

                            int j = i;
                            while (dr["ProductCode"].ToString() == dt.Rows[j]["ProductCode"].ToString()
                                && dr["PO"].ToString() == dt.Rows[j]["PO"].ToString()
                                )
                            {
                                string kk = dt.Rows[j]["Attribute"].ToString();
                                dr[kk] = dt.Rows[j]["AttributeValue"].ToString();
                                j++;

                                stringtxt.Add(kk.ToString());

                                ExtraColumns = stringtxt.Distinct().ToList();

                                if (j == dt.Rows.Count)
                                {
                                    break;
                                }
                            }


                            dtfinal.Rows.Add(dr);

                        }

                        ProdId = dt.Rows[i]["ProductCode"].ToString();
                        POxId = dt.Rows[i]["PO"].ToString();

                    }
                }

                return dtfinal;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }

    public class MovementScanData
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public MovementScanData()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();

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


        public IEnumerable<object> GetFrom(string EntityId, string PurposeId)
        {
            try
            {
                var _sql = @"Select distinct FromLocation as Value , FromLocation as Text  from mst.materialmovementMaster where PurposeId = '" + PurposeId + "' and EntityId = '" + EntityId + "' ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> GetTo(string EntityId, string PurposeId, string FromId)
        {
            try
            {
                var _sql = @"Select distinct ToLocation as Value,ToLocation as Text from mst.MaterialMovementMaster where PurposeId = '" + PurposeId + "' and EntityId = '" + EntityId + "' and FromLocation = '" + FromId + "'";
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


        public IEnumerable<object> GetData(string FromLoc, string ToLoc, string FromDate, string ToDate, string PurposeId, string EntityId)
        {
            try
            {
                TimeSpan ts = Convert.ToDateTime(ToDate).Subtract(Convert.ToDateTime(FromDate));
                if (ts.Days >= 0)
                {
                    string strkey = "";
                    if (FromLoc != "null" && ToLoc != "null")
                    {
                        strkey = "(m.FromLocation = '" + FromLoc + "' AND m.ToLocation = '" + ToLoc + "')";
                    }
                    else if (FromLoc != "null" && ToLoc == "null")
                    {
                        strkey = "(m.FromLocation = '" + FromLoc + "') ";
                    }
                    else if (FromLoc == "null" && ToLoc != "null")
                    {
                        strkey = "(m.ToLocation = '" + ToLoc + "') ";
                    }
                    else
                    {
                        strkey = "1=1";
                    }

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    string _sql = @"select s.ProductCode,Format(sc.Time,'hh:mm tt') as Time,Format(sc.WorkDate,'dd-MMM-yyyy') as WorkDate,sc.ShiftId,sh.UserName as Shift,
                      sc.Grade,mp.UserName as Purpose,s.Cones,s.GWeight,s.NetWeight,s.LotNo,s.RefNo,s.Shade,
                      e.EmployeeName as PackedBy,s.POId as PO,p.ProductionStatusId as OrderStatusId,
                      a.StandardName
					  as Article,a.Code as ArticleCode,a.Id as ArticleId,ps.UserName
                      As OrderStatus,m.FromLocation as FromLoc,m.ToLocation as ToLoc,m.Id as FromId , mp.Id as PurposeId , mp.UserName as Purpose from dbo.ItemScanChild s 
                      join dbo.ItemScan sc on sc.Id=s.MasterId
                      left join dbo.EmployeeInformation e on e.SystemId=s.PackedBy
                      left join dbo.ShiftDefination sh on sc.ShiftId=sh.SystemID
                      left join trn.ProductionOrder p on p.Id=s.POId
                      join dbo.ProductLibrary pl on pl.Code=s.ProductCode
					  LEFT join mst.MaterialMasterArticle a on a.Id=pl.ArticleId
					  left join hkp.ProductionStatus ps on ps.Id=p.ProductionStatusId
                      join mst.MaterialMovementMaster m on m.Id= sc.LocMasterId
                      left join hkp.MaterialMovementPurpose mp on mp.Id = m.PurposeId

                      where (sc.WorkDate between '" + FromDate + "' and '" + ToDate + "') and " + strkey + " and m.EntityId = '" + EntityId + @"' and mp.Id = '" + PurposeId + "' and p.PlantId='" + identity.PlantId + "' order by sc.Time desc";

                    return _sqlRepository.GetDataCollection(_sql, null);
                }
                else
                {
                    throw new Exception("Please choose a valid Date !!");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetReportData(string From, string To, string FromLoc, string ToLoc, string EntityId,
            string Shade, string ShiftId, string ProductCode, string PO, string Cones, string RefNo, string LotNo,
            string PackedBy, string Grade, string OrderStatusId, string Date, string Article, string ArticleCode, string PurposeId)
        {
            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @"select s.ProductCode,Format(sc.Time,'hh:mm tt') as Time,Format(sc.WorkDate,'dd-MMM-yyyy') as WorkDate,sc.ShiftId,sh.UserName as Shift,
                      sc.Grade,s.Cones,s.GWeight,s.NetWeight,s.LotNo,s.RefNo,s.Shade,
                      e.EmployeeName as PackedBy,s.POId as PO,p.ProductionStatusId as OrderStatusId,
                      a.StandardName
					  as Article,a.Code as ArticleCode,ps.UserName
                      As OrderStatus,m.FromLocation as FromLoc,m.ToLocation as ToLoc,m.Id as FromId , mp.Id as PurposeId , mp.UserName as Purpose from dbo.ItemScanChild s 
                      join dbo.ItemScan sc on sc.Id=s.MasterId
                      left join dbo.EmployeeInformation e on e.SystemId=s.PackedBy
                      left join dbo.ShiftDefination sh on sc.ShiftId=sh.SystemID
                      left join trn.ProductionOrder p on p.Id=s.POId
                      join dbo.ProductLibrary pl on pl.Code=s.ProductCode
					  LEFT join mst.MaterialMasterArticle a on a.Id=pl.ArticleId
					  left join hkp.ProductionStatus ps on ps.Id=p.ProductionStatusId
                      join mst.MaterialMovementMaster m on m.Id= sc.LocMasterId
                      left join hkp.MaterialMovementPurpose mp on mp.Id = m.PurposeId

                      where sc.WorkDate " +
                    "between '" + From + "' and '" + To + "'  and p.PlantId='" + identity.PlantId + @"'
                    and isnull(s.POId ,'') IN(" + PO + @") 
                    and isnull(s.LotNo,'') IN(" + LotNo + @") 
                    and isnull(s.Cones ,'') IN(" + Cones + @")
                    and isnull(s.RefNo,'') IN(" + RefNo + @") 
                    and isnull(s.Shade ,'') IN(" + Shade + @") 
                    and isnull(sc.Grade,'') IN(" + Grade + @") 
                    and isnull(s.ProductCode,'') IN(" + ProductCode + @") 
                    and isnull(sc.ShiftId,'') IN(" + ShiftId + @") 
                    and isnull(p.ProductionStatusId,'') IN(" + OrderStatusId + @") 
                    and isnull(e.EmployeeName,'') IN(" + PackedBy + @")  
                    and isnull(sc.WorkDate,'') IN(" + Date + @") 
                    and isnull(a.Code,'') IN(" + ArticleCode + @") 
                    and isnull(a.Id,'') IN(" + Article + @") 
                    and isnull(m.FromLocation,'') IN (" + FromLoc + @")
                    and isnull(m.ToLocation,'') IN (" + ToLoc + @")
                    and isnull(mp.Id,'') IN (" + PurposeId + @")
                    and m.EntityId = '" + EntityId + @"'";

                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
