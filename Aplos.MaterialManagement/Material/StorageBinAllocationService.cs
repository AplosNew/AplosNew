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

        public Dictionary<string, object> Save(Dictionary<string, object> datas)
        {
            try
            {
                //Master Table - PMSMaster
                string TableName = "MST.StorageBinMaster";
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                // Validate Unique User Name
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id <> '" + datas["Id"] + "' and UserName='" + datas["UserName"].ToString() + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same UserName is already there!!");
                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + datas["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    datas["Id"] = "SBA" + _Id;
                    
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
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = DateTime.Now.ToString();
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
            dr["UpdatedDate"] = DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }


    }
}
