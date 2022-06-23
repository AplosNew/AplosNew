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
                string sql = @"select mt.UserName as MaterialType, mgm.username as MaterialgroupName, mm.username as MaterialMaster, mma.standardname as ArticleName
                                from mst.MaterialGroupMaster mgm
                                left join hkp.materialtype mt
                                on mgm.materialtypeid = mt.id
                                left join mst.MaterialMaster mm
                                on mm.MaterialGroupMasterId = mgm.Id
                                left join mst.MaterialMasterArticle mma
                                on mma.materialmasterid = mm.id
                                where mm.Active = '1'";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getMaterialGroup()
        {
            try
            {
                string sql = @"select mt.UserName as MaterialType, mgm.username as MaterialgroupName, mm.username as MaterialMaster, mma.standardname as ArticleName
                                from mst.MaterialGroupMaster mgm
                                left join hkp.materialtype mt
                                on mgm.materialtypeid = mt.id
                                left join mst.MaterialMaster mm
                                on mm.MaterialGroupMasterId = mgm.Id
                                left join mst.MaterialMasterArticle mma
                                on mma.materialmasterid = mm.id
                                where mm.Active = '1'";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getMaterial()
        {
            try
            {
                string sql = @"select mt.UserName as MaterialType, mgm.username as MaterialgroupName, mm.username as MaterialMaster, mma.standardname as ArticleName
                                from mst.MaterialGroupMaster mgm
                                left join hkp.materialtype mt
                                on mgm.materialtypeid = mt.id
                                left join mst.MaterialMaster mm
                                on mm.MaterialGroupMasterId = mgm.Id
                                left join mst.MaterialMasterArticle mma
                                on mma.materialmasterid = mm.id
                                where mm.Active = '1'";

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
                string sql = @"select mt.UserName as MaterialType, mgm.username as MaterialgroupName, mm.username as MaterialMaster, mma.standardname as ArticleName
                                from mst.MaterialGroupMaster mgm
                                left join hkp.materialtype mt
                                on mgm.materialtypeid = mt.id
                                left join mst.MaterialMaster mm
                                on mm.MaterialGroupMasterId = mgm.Id
                                left join mst.MaterialMasterArticle mma
                                on mma.materialmasterid = mm.id
                                where mm.Active = '1'";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getMaterialArticle()
        {
            try
            {
                string sql = @"select mt.UserName as MaterialType, mgm.username as MaterialgroupName, mm.username as MaterialMaster, mma.standardname as ArticleName
                                from mst.MaterialGroupMaster mgm
                                left join hkp.materialtype mt
                                on mgm.materialtypeid = mt.id
                                left join mst.MaterialMaster mm
                                on mm.MaterialGroupMasterId = mgm.Id
                                left join mst.MaterialMasterArticle mma
                                on mma.materialmasterid = mm.id
                                where mm.Active = '1'";

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
                string sql = @"";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
