using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
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
                string sql = @"Select ms.Id as Value ms.UserName as Text from MST.StorageBinMaster sb
                               left join HKP.MaterialStorage ms on ms.Id = sb.StorageLocation order by Text ASC";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getStorageSubLocation()
        {
            try
            {
                string sql = @"Select distinct sb.StorageSubLocation as Text from MST.StorageBinMaster sb";
                               

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

        public IEnumerable<object> getAccessType()
        {
            try
            {
                string sql = @"Select distinct sb.AccessType as Text from MST.StorageBinMaster sb";

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
    }
}
