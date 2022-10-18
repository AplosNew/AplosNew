using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace Library.MaterialManagement.Material
{
   public class IssueControlService
    {
        private readonly SqlRepository _sqlRepository;
        public IssueControlService()
        {
            _sqlRepository = new SqlRepository();
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
                string sql = "";
                if (MaterialTypeId == null)
                {
                    sql = @"select mgm.Id as Value, mgm.UserName as Text from mst.MaterialGroupMaster mgm
                               left join hkp.MaterialType mt on mt.Id = mgm.MaterialTypeId
                               where mt.Active = '1' order by Text ASC";
                }
                else
                {
                    sql = @"select mgm.Id as Value, mgm.UserName as Text from mst.MaterialGroupMaster mgm
                               left join hkp.MaterialType mt on mt.Id = mgm.MaterialTypeId
                               where mt.Id = '" + MaterialTypeId + "' and mt.Active = '1' order by Text ASC";
                }
                 

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
                string sql = "";
                if (materialgroupid == null)
                {
                    sql = @"select mm.Id as Value, mm.UserName as Text from mst.MaterialMaster mm
                               left join mst.MaterialGroupMaster mg on mg.Id = mm.MaterialGroupMasterId
                               where mm.Active = '1' ORDER BY Text ASC";
                }
                else
                {
                    sql = @"select mm.Id as Value, mm.UserName as Text from mst.MaterialMaster mm
                               left join mst.MaterialGroupMaster mg on mg.Id = mm.MaterialGroupMasterId
                               where  mg.Id = '" + materialgroupid + "' and mm.Active = '1' ORDER BY Text ASC";
                }
                

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getMaterialArticleId(string materialTypeId, string materialGroupMasterId, string materialMasterId, string storagelevel)
        {
            try
            {
                var sql = "";

                if(storagelevel == "Material" && materialTypeId == null && materialGroupMasterId == null && materialMasterId == null)
                {
                    sql = @"select mt.UserName as MaterialType, mgm.username as MaterialgroupName, mm.username as MaterialMaster, 
                            mma.standardname as ArticleName, mm.Id as MaterialMasterId,
                            mt.Id as MaterialTypeId, mgm.Id as MaterialGroupMasterId, bah.Id
							from mst.MaterialMasterArticle mma
							left join MST.MaterialMaster mm on mm.Id = mma.MaterialMasterId
							left join mst.MaterialGroupMaster mgm on mgm.Id = mm.MaterialGroupMasterId	
							left join hkp.materialtype mt on mt.Id =  mgm.materialtypeid                                                       
							left join trn.BinAllocationHead bah on bah.MaterialMasterId = mm.Id
                           -- where mm.Id not in (SELECT M.MaterialMasterId FROM TRN.MaterialAlocation M) 
                            --and mt.Id = '" + materialTypeId + "' and mgm.Id = '" + materialGroupMasterId + "' and mm.Id = '" + materialMasterId + "'";
                }
                else if (storagelevel == "Material")
                {
                    sql = @"select distinct mma.standardname as ArticleName, mt.UserName as MaterialType, mgm.username as MaterialgroupName, mm.username as MaterialMaster, 
                             mm.Id as MaterialMasterId,
                            mt.Id as MaterialTypeId, mgm.Id as MaterialGroupMasterId
							from mst.MaterialMasterArticle mma
							left join MST.MaterialMaster mm on mm.Id = mma.MaterialMasterId
							left join mst.MaterialGroupMaster mgm on mgm.Id = mm.MaterialGroupMasterId	
							left join hkp.materialtype mt on mt.Id =  mgm.materialtypeid                                                       
							left join trn.BinAllocationHead bah on bah.MaterialMasterId = mm.Id
                            where --mm.Id not in (SELECT M.MaterialMasterId FROM TRN.MaterialAlocation M) and
                             mt.Id = '" + materialTypeId + "' and mgm.Id = '" + materialGroupMasterId + "' and mm.Id = '" + materialMasterId + "'";
                }
                else
                {
                    if (storagelevel == "Article" && materialTypeId == null && materialGroupMasterId == null && materialMasterId == null)
                    {
                        sql = @"select mt.UserName as MaterialType, mgm.username as MaterialgroupName, mm.username as MaterialMaster, 
                            mma.standardname as ArticleName, mm.Id as MaterialMasterId,
                            mt.Id as MaterialTypeId, mgm.Id as MaterialGroupMasterId, bah.Id
							from mst.MaterialMasterArticle mma
							left join MST.MaterialMaster mm on mm.Id = mma.MaterialMasterId
							left join mst.MaterialGroupMaster mgm on mgm.Id = mm.MaterialGroupMasterId	
							left join hkp.materialtype mt on mt.Id =  mgm.materialtypeid                                                       
							left join trn.BinAllocationHead bah on bah.MaterialMasterId = mm.Id
                           -- where mm.Id not in (SELECT M.MaterialMasterId FROM TRN.MaterialAlocation M) 
                            --and mt.Id = '" + materialTypeId + "' and mgm.Id = '" + materialGroupMasterId + "' and mm.Id = '" + materialMasterId + "'";
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
                            where --mma.Id NOT in (SELECT M.MaterialMasterArticleId FROM TRN.MaterialAlocation M)  and
                            mt.Id = '" + materialTypeId + "' and mgm.Id = '" + materialGroupMasterId + "' and mm.Id = '" + materialMasterId + "'";
                    }
                }
                


                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
