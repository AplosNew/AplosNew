using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Enums;
using Library.Service.Logs;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;

namespace Library.MaterialManagement.Material
{
   public class StorageBinAllocationService
    {
        private readonly SqlRepository _sqlRepository;
        public StorageBinAllocationService()
        {
            _sqlRepository = new SqlRepository();
        }

        #region All Get and select function
        public IEnumerable<object> getStorageLevel()
        {
            try
            {
                string sql = @"select distinct sbm.CapacityValue as Text from MST.StorageBinMaster sbm ORDER BY Text ASC";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getMaterialType()
        {
            try
            {
                string sql = @"select Id as Value, UserName as Text from HKP.MaterialType where Active = '1' order by Text ASC";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getMaterialGroup(string MaterialTypeId)
        {
            try
            {
                string sql = @"select mgm.Id as Value, mgm.UserName as Text from mst.MaterialGroupMaster mgm
                               left join hkp.MaterialType mt on mt.Id = mgm.MaterialTypeId
                               where mt.Id = '"+ MaterialTypeId + "' and mt.Active = '1' order by Text ASC";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getMaterial(string materialgroupid)
        {
            try
            {
                string sql = @"select mm.Id as Value, mm.UserName as Text from mst.MaterialMaster mm
                               left join mst.MaterialGroupMaster mg on mg.Id = mm.MaterialGroupMasterId
                               where  mg.Id = '"+ materialgroupid + "' and mm.Active = '1' ORDER BY Text ASC";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getStorageLocation()
        {
            try
            {
                string sql = @"Select ms.Id as Value,  ms.UserName as Text from  HKP.MaterialStorage ms  order by Text ASC";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> GetBinAllocationHead(string column, string value)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"
                        select top 100 * from (select BAH.Id,BAH.UserName ,SBM.UserName MaterialStorage,MT.UserName MaterialType,MGM.UserName MaterialGroup,MM.UserName MaterialName,BAH.MaterialMasterId,BAH.StorageLocationId,BAH.AccessType 
	                    FROM TRN.BinAllocationHead BAH 
	                    LEFT JOIN HKP.MaterialStorage SBM ON SBM.Id=BAH.StorageLocationId
	                    LEFT JOIN HKP.MaterialType MT ON MT.Id=BAH.MaterialTypeId
	                    LEFT JOIN MST.MaterialMaster MM ON MM.Id=BAH.MaterialMasterId
	                    LEFT JOIN MST.MaterialGroupMaster MGM ON MGM.Id=BAH.MaterialGroupMasterId) AS TEMP WHERE " + strkey + " ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetBinAllocationByMaterialId(string materialMasterId, string materialStorageId)
        {
            try
            {
                var sql = @"
                        select BAH.Id,BAH.UserName ,SBM.UserName StorageBinMaster,MS.UserName StorageLocation,MT.UserName MaterialType,MGM.UserName MaterialGroup
	                    ,MM.UserName MaterialName,BAH.MaterialMasterId,BAH.StorageLocationId,BA.StorageBinMasterId,BAH.AccessType ,0 [check],NULL PurchaseOrderDetailId
	                    FROM TRN.BinAllocationHead BAH 
	                    LEFT JOIN TRN.BinAllocation BA ON BA.BinAllocationHeadId=BAH.Id
	                    LEFT JOIN MST.StorageBinMaster SBM ON SBM.Id=ba.StorageBinMasterId
	                    LEFT JOIN HKP.MaterialStorage MS ON MS.Id=BAH.StorageLocationId
	                    LEFT JOIN HKP.MaterialType MT ON MT.Id=BAH.MaterialTypeId
	                    LEFT JOIN MST.MaterialMaster MM ON MM.Id=BAH.MaterialMasterId
	                    LEFT JOIN MST.MaterialGroupMaster MGM ON MGM.Id=BAH.MaterialGroupMasterId
	                    where BAH.MaterialMasterId='" + materialMasterId + "' AND BAH.StorageLocationId='"+ materialStorageId + @"' ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> getStorageSubLocation(string storageLocationId)
        {
            try
            {
                string sql = @"Select distinct sb.StorageSubLocation as Text from MST.StorageBinMaster sb
							   where sb.StorageLocation = '" + storageLocationId + "'";
                               

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getMaterialArticle(string materialmasterId)
        {
            try
            {
                string sql = @"select mma.Id as Value, mma.StandardName as Text
                                from mst.MaterialGroupMaster mgm
                                left join hkp.materialtype mt
                                on mgm.materialtypeid = mt.id
                                left join mst.MaterialMaster mm
                                on mm.MaterialGroupMasterId = mgm.Id
                                left join mst.MaterialMasterArticle mma
                                on mma.materialmasterid = mm.id
                                where mma.materialmasterid = '"+ materialmasterId + "' and mm.Active = '1' order by Text ASC";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getAccessType(string storagesublocation)
        {
            try
            {
                string sql = @"Select distinct sb.AccessType as Text from MST.StorageBinMaster sb where sb.StorageSubLocation = '"+ storagesublocation + "'";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> Query(string materialMasterId)
        {
            try
            {
                var _sql = @"SELECT MMA.Id, MMA.MaterialMasterId, MMA.Code, MMA.ShortName, MMA.StandardName, MMA.RPM, MMA.MachineAllowance, MMA.StitchCodeId,MMA.MachineMasterId,MM.UserName MachineMaster
		                    FROM MST.MaterialMasterArticle MMA
                           LEFT JOIN [MST].[MachineMaster] MM ON MM.Id=MMA.MachineMasterId
                            WHERE MaterialMasterId='" + materialMasterId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> viewBinHead(string materialType, string materialGroup, string material, string materialArticle)
        {
            try
            {
                var sql = "";
                #region commented
                /* if (materialArticle == null)
                 {
                     sql = @"select bah.Id, bah.UserName as BinHeader, mt.UserName as MaterialType, mgm.username as MaterialgroupName, mm.username as MaterialMaster, 
                             mma.standardname as ArticleName, mma.Id as MaterialMasterArticleId, mm.Id as MaterialMasterId
                             from TRN.BinAllocationHead bah
                             left join mst.MaterialGroupMaster mgm
                             on mgm.Id = bah.MaterialGroupMasterId
                             left join hkp.materialtype mt
                             on mgm.materialtypeid = mt.id
                             left join mst.MaterialMaster mm
                             on mm.MaterialGroupMasterId = mgm.Id
                             left join mst.MaterialMasterArticle mma
                             on mma.materialmasterid = mm.id
                             where mt.Id = '" + materialType + "' and mgm.Id = '" + materialGroup + "' and mm.Id = '" + material + "'";
                     return _sqlRepository.GetDataCollection(sql);
                 }
                 else
                 {
                     sql = @"select bah.Id, bah.UserName as BinHeader, mt.UserName as MaterialType, mgm.username as MaterialgroupName, mm.username as MaterialMaster, 
                             mma.standardname as ArticleName, mma.Id as MaterialMasterArticleId, mm.Id as MaterialMasterId
                             from TRN.BinAllocationHead bah
                             left join mst.MaterialGroupMaster mgm
                             on mgm.Id = bah.MaterialGroupMasterId
                             left join hkp.materialtype mt
                             on mgm.materialtypeid = mt.id
                             left join mst.MaterialMaster mm
                             on mm.MaterialGroupMasterId = mgm.Id
                             left join mst.MaterialMasterArticle mma
                             on mma.materialmasterid = mm.id
                             where mt.Id = '" + materialType + "' and mgm.Id = '" + materialGroup + "' and mm.Id = '" + material + "' and mma.Id = '" + materialArticle + "'";
                     return _sqlRepository.GetDataCollection(sql);
                 }*/
                #endregion commented

                    sql = @"select bah.Id, bah.UserName as BinHeader, mt.UserName as MaterialType, mgm.username as MaterialgroupName, mm.username as MaterialMaster, 
                                mma.standardname as ArticleName, mma.Id as MaterialMasterArticleId, mm.Id as MaterialMasterId
                                from TRN.BinAllocationHead bah
							    left join mst.MaterialGroupMaster mgm
							    on mgm.Id = bah.MaterialGroupMasterId
                                left join hkp.materialtype mt
                                on mgm.materialtypeid = mt.id
                                left join mst.MaterialMaster mm
                                on mm.MaterialGroupMasterId = mgm.Id
                                left join mst.MaterialMasterArticle mma
                                on mma.materialmasterid = mm.id
                                where mt.Id = '" + materialType + "' and mgm.Id = '" + materialGroup + "' and mm.Id = '" + material + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> viewBinAllocation(string storagelocation, string storagesublocation, string AccessType)
        {
            try
            {
                #region commented
                /* var sql = @"SELECT sb.Id as Id, sb.UserName, ms.Id as StorageLocationId, ms.UserName as StorageLocation, e.SystemId as ResponsiblePersonId, e.EmployeeName as EmployeeName, sb.StorageSubLocation,
                             sb.AreaRackCode, sb.ColumnNo, sb.RowNo, sb.BinCode, sb.BinReference, sb.UserName, sb.CapacityValue,
                             sb.AccessType, sb.UserLocationType, sb.Remarks
                             FROM MST.StorageBinMaster sb
                             left join hkp.MaterialStorage ms on ms.Id = sb.StorageLocation
                             left join dbo.EmployeeInformation e on e.SystemId = sb.ResponsiblePersonId
                             WHERE sb.Id = '" + storagelocation + "' and sb.StorageSubLocation = '"+ storagesublocation + "'";*/
                #endregion commented
                var sql = @"SELECT DISTINCT  ms.UserName as StorageLocation, e.EmployeeName as EmployeeName, sb.StorageSubLocation,
                            sb.AreaRackCode, sb.ColumnNo, sb.RowNo, sb.BinCode, sb.BinReference, sb.UserName, sb.CapacityValue,
                            sb.AccessType, sb.UserLocationType, sb.Remarks, sb.Id
                            FROM TRN.BinAllocationHead	bah	
							left join MST.StorageBinMaster sb on sb.Id = bah.StorageBinMasterId
                            left join hkp.MaterialStorage ms on ms.Id = sb.StorageLocation
                            left join dbo.EmployeeInformation e on e.SystemId = sb.ResponsiblePersonId
							
							left join mst.MaterialGroupMaster mgm on mgm.Id = bah.MaterialGroupMasterId
							left join hkp.materialtype mt on mt.Id = mgm.MaterialTypeId
							left join mst.MaterialMaster mm on mm.MaterialGroupMasterId = mgm.Id
                        WHERE sb.Id = '" + storagelocation + "' and sb.StorageSubLocation = '" + storagesublocation + "' and sb.AccessType = '"+ AccessType + "'";
               
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> selectIDs(string materialType, string materialGroup, string material, string storagelevel)
        {
            try
            {
                var sql = "";
                if (storagelevel == "Material")
                {
                    sql = @"select mt.UserName as MaterialType, mgm.username as MaterialgroupName, mm.username as MaterialMaster, 
                            mma.standardname as ArticleName, mm.Id as MaterialMasterId,
                            mt.Id as MaterialTypeId, mgm.Id as MaterialGroupMasterId, bah.Id
							from mst.MaterialMasterArticle mma
							left join MST.MaterialMaster mm on mm.Id = mma.MaterialMasterId
							left join mst.MaterialGroupMaster mgm on mgm.Id = mm.MaterialGroupMasterId	
							left join hkp.materialtype mt on mt.Id =  mgm.materialtypeid                                                       
							left join trn.BinAllocationHead bah on bah.MaterialMasterId = mm.Id
                            where --mm.Id not in (SELECT M.MaterialMasterId FROM TRN.MaterialAlocation M) and
                            mt.Id = '" + materialType + "' and mgm.Id = '" + materialGroup + "' --or mm.Id = '" + material + "'";
                }
                else if (storagelevel == "Article")
                {
                    sql = @"select mt.UserName as MaterialType, mgm.username as MaterialgroupName, mm.username as MaterialMaster, 
                            mma.standardname as ArticleName, mma.Id as MaterialMasterArticleId, mm.Id as MaterialMasterId,
                            mt.Id as MaterialTypeId, mgm.Id as MaterialGroupMasterId, bah.Id
							from mst.MaterialMasterArticle mma
							left join MST.MaterialMaster mm on mm.Id = mma.MaterialMasterId
							left join mst.MaterialGroupMaster mgm on mgm.Id = mm.MaterialGroupMasterId	
							left join hkp.materialtype mt on mt.Id =  mgm.materialtypeid                                                       
							left join trn.BinAllocationHead bah on bah.MaterialMasterId = mm.Id
                            where  mt.Id = '" + materialType + "' and mgm.Id = '" + materialGroup + "' and mm.Id = '" + material + "' and bah.MaterialMasterId is null";
                }
                
                        
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> selectBinIDs(string storagelocation, string storagesublocation, string AccessType)
        {
            try
            {
                var sql = @"SELECT   sb.AreaRackCode, sb.ColumnNo, sb.RowNo, sb.BinCode, sb.BinReference, sb.CapacityValue,
                            sb.AccessType, sb.UserLocationType, sb.Remarks, sb.Id as StorageBinMasterId,BAH.StorageLocationId, ba.Id
                            FROM MST.StorageBinMaster sb
							LEFT JOIN TRN.BinAllocation BA ON BA.StorageBinMasterId=sb.Id
							left join trn.BinAllocationHead BAH on BAH.Id = BA.BinAllocationHeadId
                           WHERE   sb.StorageLocation = '" + storagelocation + "'  and sb.AccessType = '" + AccessType + "' AND BA.Id IS NULL AND SB.StorageSubLocation='"+ storagesublocation + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getMaterialAllocation(string Id)
        {
            try
            {
                var sql = @"select ma.* from TRN.MaterialAlocation ma
                            left join TRN.BinAllcationHead bah on bah.Id = ma.BinAllocationHeadId
                            where bah.Id = '"+Id+"'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        #endregion All Get and select function 

       

        #region All Save Function
        public Dictionary<string, object> Save(Dictionary<string, object> datas)
        {

            try
            {
                //Master Table - PMSMaster
                string TableName = "TRN.BinAllocationHead";
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                // Validate Unique User Name
                //con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id <> '" + datas["Id"] + "' and UserName='" + datas["UserName"].ToString() + "'", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //{
                //    throw new Exception("Same UserName is already there!!");
                //}

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + datas["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    datas["Id"] = "BA" + _Id;

                    AddNewRow(dsMaster.Tables[0], datas);
                }
                else
                {
                    _Id = datas["Id"].ToString();


                    EditRow(dsMaster.Tables[0].Rows[0], datas);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return datas;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #region Material Allocation
        // MATERIAL ALLOCATION SAVE FUNCTION
        public List<Dictionary<string, object>> SaveMaterialAllocation(List<Dictionary<string, object>> material, string headerId, string storagelevel)
        {
            try
            {
                //Master Table - PMSMaster
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string TableName = "TRN.MaterialAlocation";
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("SELECT * FROM TRN.MaterialAlocation WHERE BinAllocationHeaderId ='" + headerId + "'", out dsMaster, false, "1");


                #region data Master update
                int count = 0;
               
                    foreach (var item in material)
                    {
                        count++;
                        DataView dv = new DataView(dsMaster.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";
                    if (storagelevel == "Material")
                    {


                        if (dv.Count == 0)
                        {
                            item["Id"] = headerId + "-" + count;
                            item["BinAllocationHeaderId"] = headerId;
                            item["MaterialmaterArticleId"] = null;
                            AddNewRow(dsMaster.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                    else if(storagelevel == "Article")
                    {
                        if (dv.Count == 0)
                        {
                            item["Id"] = headerId + "-" + count;
                            item["BinAllocationHeaderId"] = headerId;
                            
                            AddNewRow(dsMaster.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                    }
                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster);
               
                #endregion data update

                return material;
            }
            catch (Exception ex) 
            {
                throw ex;
            }
        }
        #endregion Material Allocation
        // BIN ALLOCATION SAVE FUNCTION
        #region Bin Allocation
        public List<Dictionary<string, object>> SaveBinAllocation(List<Dictionary<string, object>> BinHead, string headerId, string MaterialId)
        {
            try
            {
                //Master Table - PMSMaster
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string TableName = "TRN.BinAllocation";
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("SELECT * FROM TRN.BinAllocation WHERE BinAllocationHeadId ='" + headerId + "'", out dsMaster, false, "1");


                #region data Master update
                int count = 0;

                foreach (var item in BinHead)
                {
                    count++;
                    DataView dv = new DataView(dsMaster.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = headerId + "-" + count;
                        item["BinAllocationHeadId"] = headerId ;
                        item["MaterialMasterId"] = MaterialId;

                        AddNewRow(dsMaster.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, item);
                    }
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                #endregion data update

                return BinHead;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Bin Allocation
        #endregion All Save Function

        #region Add and set New Row
        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
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
            dr["AddedDate"] = DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            //dr["UpdatedBy"] = identity.Name;
            //dr["UpdatedDate"] = DateTime.Now.ToString();
            //dr["UpdatedFromIP"] = identity.IPAddress;

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
            dr["UpdatedDate"] = DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }
        #endregion Add and set New Row
    }
}
