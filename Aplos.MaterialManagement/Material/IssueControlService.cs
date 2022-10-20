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
                    sql = @"select distinct  mt.UserName as MaterialType, mgm.username as MaterialgroupName, mm.username as MaterialMaster, 
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
                        sql = @"select distinct mt.UserName as MaterialType, mgm.username as MaterialgroupName, mm.username as MaterialMaster, 
                            mma.standardname as ArticleName, mma.Id as MaterialMasterArticleId, mm.Id as MaterialMasterId,
                            mt.Id as MaterialTypeId, mgm.Id as MaterialGroupMasterId -- bah.Id
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
        public List<Dictionary<string, object>> SaveChild(List<Dictionary<string, object>> data, Dictionary<string, object> itemApplicableData, string headerId, string materiallevel)
        {
            try
            {
                string TableNameHead = "TRN.IssueControlChild";

                DataSet dsMaster;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where IssueControlHeadId='" + headerId + "'", out dsMaster, false, "1");
                string _Id = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                #region Medicine HEAD
                for (int i = 0; i < data.Count; i++)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    if (materiallevel == "Material")
                    {
                        dr["Id"] = headerId + '-' + i.ToString();
                        dr["IssueControlHeadId"] = headerId;
                        dr["MaterialMasterId"] = data[i]["MaterialMasterId"];
                        //dr["MaterialMasterArticleId"] = data[i]["MaterialMasterArticleId"];
                        dr["MachineApplicable"] = data[i]["MachineApplicable"];
                        dr["WorkcenterApplicable"] = data[i]["WorkcenterApplicable"];
                        dr["OrderLevel"] = data[i]["StorageLevel"];
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        dr["Id"] = headerId + '-' + i.ToString();
                        dr["IssueControlHeadId"] = headerId;
                        //dr["MaterialMasterId"] = data[i]["MaterialMasterId"];
                        dr["MaterialMasterArticleId"] = data[i]["MaterialMasterArticleId"];
                        dr["MachineApplicable"] = data[i]["MachineApplicable"];
                        dr["WorkcenterApplicable"] = data[i]["WorkcenterApplicable"];
                        dr["OrderLevel"] = data[i]["StorageLevel"];
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                }
                #endregion Medicine HEAD

                #region ItemAplicable
                string ItemApplicableTable = "TRN.IssueControlItemApplicable";
                DataSet dsItemApplicable;

                con.OpenDataSetThroughAdapter("select * from " + ItemApplicableTable + " where IssueControlHeadId='" + headerId + "'", out dsItemApplicable, false, "1");
                if (dsItemApplicable.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsItemApplicable.Tables[0].NewRow();
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableNameHead, out _Id);

                    itemApplicableData["Id"] = "IA" + _Id;
                    itemApplicableData["IssueControlHeadId"] = headerId;
                    AddNewRow(dsItemApplicable.Tables[0], itemApplicableData);
                }




                #endregion ItemAplicable


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsItemApplicable);

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
                var str = @"Select Id Vaue, EnumName Text  from dbo.DefineEnum";

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
