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

        public IEnumerable<object> GetItemApplicable()
        {
            try
            {
                var SQL = @"select IC.*, ISNULL(DE.UserName, 'Bulk Packing') Category from TRN.IssueControlItemApplicable IC
                        left join dbo.DefineEnum DE on DE.Id = IC.OrderLevel";
                return _sqlRepository.GetDataCollection(SQL);
            }
            catch (Exception ex)
            {
                throw ex;
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

        public IEnumerable<object> GetMaterialAndArticle(string materialTypeId, string materialGroupMasterId, string materialMasterId, string storagelevel)
        {
            try
            {
                var sql = "";
                var tempsql = "";

                if (storagelevel == "Material" && !string.IsNullOrEmpty(materialTypeId) && string.IsNullOrEmpty(materialGroupMasterId) && string.IsNullOrEmpty(materialMasterId))
                {
                    tempsql = "MM.MaterialMasterTypeId = '" + materialTypeId + "'";
                }
                else if (storagelevel == "Material" && !string.IsNullOrEmpty(materialTypeId) && !string.IsNullOrEmpty(materialGroupMasterId) && string.IsNullOrEmpty(materialMasterId))
                {
                    tempsql = "MM.MaterialMasterTypeId = '" + materialTypeId + "' AND MM.MaterialGroupMasterId='" + materialGroupMasterId + "'";
                }
                else if (storagelevel == "Material" && !string.IsNullOrEmpty(materialTypeId) && !string.IsNullOrEmpty(materialGroupMasterId) && !string.IsNullOrEmpty(materialMasterId))
                {
                    tempsql = "MM.MaterialMasterTypeId = '" + materialTypeId + "' AND MM.MaterialGroupMasterId='" + materialGroupMasterId + "' AND MM.Id='" + materialMasterId + "'";
                }
                else if (storagelevel == "Material" && string.IsNullOrEmpty(materialTypeId) && !string.IsNullOrEmpty(materialGroupMasterId) && !string.IsNullOrEmpty(materialMasterId))
                {
                    tempsql = " MM.MaterialGroupMasterId='" + materialGroupMasterId + "' AND MM.Id='" + materialMasterId + "'";
                }
                else if (storagelevel == "Material" && string.IsNullOrEmpty(materialTypeId) && !string.IsNullOrEmpty(materialGroupMasterId) && string.IsNullOrEmpty(materialMasterId))
                {
                    tempsql = " MM.MaterialGroupMasterId='" + materialGroupMasterId + "' ";
                }
                if (storagelevel == "Material")
                {
                    sql = @"SELECT MM.Id MaterialMasterId,MM.MaterialMasterTypeId,MM.UserName MaterialName,MM.IsMachineApplicable,MM.IsWorkCenterApplicable,MM.OrderLevel 
                            FROM MST.MaterialMaster MM
                            WHERE " + tempsql + "";
                }
                else
                {
                    sql = @"SELECT MMA.MaterialMasterId,MM.MaterialMasterTypeId,MM.UserName MaterialName,MMA.IsMachineApplicable,MMA.IsWorkCenterApplicable,MMA.OrderLevel 
                           FROM  MST.MaterialMasterArticle MMA
                           LEFT JOIN MST.MaterialMaster MM ON MM.Id=MMA.MaterialMasterId
                           WHERE " + tempsql + "";
                }
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region SAVE
        public Dictionary<string, object> Save(Dictionary<string, object> data)
        {
            try
            {
                string TableNameHead = "TRN.IssueControlHeader";

                DataSet dsMaster;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                string _Id = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                #region Medicine HEAD
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableNameHead, out _Id);

                    data["Id"] = "IC" + _Id;

                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();

                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion Medicine POLICY HEAD


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Save 

        #region Save Child
        //Need to delete or update
        public string SaveItemApplicable(bool machineApplicable, bool worckcenterApplicable, int orderlevel, string headerId)
        {
            try
            {
                string TableNameHead = "TRN.IssueControlItemApplicable";

                DataSet dsMaster;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where IssueControlHeadId='" + headerId + "'", out dsMaster, false, "1");
                string _Id = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                #region Medicine HEAD
                DataRow dr = dsMaster.Tables[0].NewRow();
                if (dsMaster.Tables[0].Rows.Count == 0)
                {

                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableNameHead, out _Id);

                    dr["Id"] = "IA" + _Id;
                    dr["IssueControlHeadId"] = headerId;
                    dr["MachineApplicable"] = machineApplicable;
                    dr["WorkCenterApplicable"] = worckcenterApplicable;
                    dr["OrderLevel"] = orderlevel;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsMaster.Tables[0].Rows.Add(dr);
                }

                #endregion Medicine HEAD

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return dr.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<Dictionary<string, object>> UpdateMaterialMasterForIssueControl(List<Dictionary<string, object>> data, string materiallevel, string materialIds)
        {
            try
            {
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from MST.MaterialMaster where Id in (" + materialIds + ")", out dsMaster, false, "1");
                if (materiallevel == "Material")
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "Id='" + data[i]["MaterialMasterId"].ToString() + "'";
                        if (dsMaster.Tables[0].DefaultView.Count > 0)
                        {
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();
                            dr["Id"] = data[i]["MaterialMasterId"];
                            dr["IsMachineApplicable"] = data[i]["IsMachineApplicable"];
                            dr["IsWorkCenterApplicable"] = data[i]["IsWorkCenterApplicable"];
                            dr["OrderLevel"] = data[i]["OrderLevel"];
                            //dsMaster.Tables[0].Rows.Add(dr);
                            dr.EndEdit();
                        }
                    }
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion SAVE Child

        #region CREATE AND EDIT DEFAULT COLUMN
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
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;

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
        #endregion CREATE AND EDIT DEFAULT COLUMN

        #region GET
        public IEnumerable<object> GetIssue()
        {
            try
            {
                var str = @"Select * from TRN.IssueControlHeader";

                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetEnum()
        {
            try
            {
                var str = @"Select Id Value, UserName Text  from dbo.DefineEnum";

                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion GET
    }


}
