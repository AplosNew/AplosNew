using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Text.RegularExpressions;
using System.Linq;

namespace Library.OrderManagement.Production
{
    public class ProductiveAllowanceRateSetupService
    {
        private readonly SqlRepository _sqlRepository;

        #region Constructor
        public ProductiveAllowanceRateSetupService()
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion Constructor

        #region ProductiveAllowance


        #region AllGetOperations

        public IEnumerable<object> getProcess()
        {
            try
            {
                var str = @"Select Id as Value , UserName as Text from hkp.Process";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getEntity()
        {
            try
            {
                var str = @"Select Id as Value , UserName as Text from org.Entity";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getMasterData()
        {
            try
            {
                var str = @"Select Id, UserName, EffectiveDate, Remarks,
                            STUFF((
                            SELECT ',' + p.UserName

                            FROM dbo.ProducedMinAllowanceProcess pp 
                            left join hkp.Process p on p.Id = pp.ProcessId
                            where pp.HeaderId = pm.Id
                            FOR XML PATH('')

                            ),1,1,'') AS Processes,
                            STUFF((
                            SELECT ',' + e.UserName

                            FROM dbo.ProducedMinAllowanceEntity pe 
                            left join org.Entity e on e.Id = pe.EntityId
                            where pe.HeaderId = pm.Id
                            FOR XML PATH('')

                            ),1,1,'') AS Entity
                            from dbo.ProducedMinAllowanceHeader pm
                            ";

                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getPaChildList(string Id)
        {
            try
            {
                var str = @"Select pc.*,  sk.UserName as SkillCategory from dbo.ProducedMinAllowanceChild pc
                            left join hkp.SkillCategory sk on sk.Id = pc.SkillCategoryId
                             where pc.headerId ='" + Id+ "' order by pc.SkillCategoryId asc";

                DataTable dtChild = _sqlRepository.GetDataTable(str);
                

                if(dtChild.Rows.Count>0)
                {
                    return Library.Service.Helpers.DataTableExtensions.DataTableToJson(dtChild);
                }
                else
                {
                    var sql = @"Select Id , UserName from hkp.SkillCategory";
                    DataTable dtSkill = _sqlRepository.GetDataTable(sql);

                    for (int i = 0; i < dtSkill.Rows.Count; i++)
                    {
                        for (int j = 1; j <= 6; j++)
                        {
                            DataRow dr = dtChild.NewRow();
                            dr["Id"] = null;
                            dr["HeaderId"] = Id;
                            dr["SkillCategory"] = dtSkill.Rows[i]["UserName"].ToString();
                            dr["SkillCategoryId"] = dtSkill.Rows[i]["Id"].ToString();
                            dr["OperationSequence"] = j;
                            dr["SkillAllowance"] = 0.0;
                            dr["AdditionOperationAllowance"] = 0.0;
                            dtChild.Rows.Add(dr);
                        }
                    }
                }

                return Library.Service.Helpers.DataTableExtensions.DataTableToJson(dtChild);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion AllGetOperations


        #region SaveOps

        public Dictionary<string, object> saveHeaderPa(Dictionary<string, object> headerData, List<string> process, List<string> entity)
        {
            try
            {

               
                //Master Table - Wastw-Transaction
                string TableName = "dbo.ProducedMinAllowanceHeader";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                //if( cl headerData["Id"])

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id <> '" + headerData["Id"] + "' and UserName='"+headerData["UserName"].ToString()+"'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same UserName is already there!!");
                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id = '"+headerData["Id"]+"'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master Upload
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID genid = new bplib.clsGenID();

                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    dr["UserName"] = headerData["UserName"].ToString();
                    dr["EffectiveDate"] =Convert.ToDateTime(headerData["EffectiveDate"].ToString());
                    dr["Remarks"] = headerData["Remarks"].ToString();
                    dr.EndEdit();
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    genid.GenID("dbo.ProducedMinAllowanceHeader", out _Id);
                    headerData["Id"] = _Id;
                    dr["Id"] = _Id;
                    dr["UserName"] = headerData["UserName"].ToString();
                    dr["EffectiveDate"] = Convert.ToDateTime(headerData["EffectiveDate"].ToString());
                    dr["Remarks"] = headerData["Remarks"].ToString();
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);
                }



                #endregion data Master Upload

                #region ProcessChild

                DataSet dsProcessChild;
                ConnectionManager.DAL.ConManager conn = new ConnectionManager.DAL.ConManager("1");
                conn.OpenDataSetThroughAdapter("select * from dbo.ProducedMinAllowanceProcess where HeaderId ='"+headerData["Id"].ToString()+"'", out dsProcessChild, false, "1");

                while (dsProcessChild.Tables[0].DefaultView.Count > 0)
                {
                    dsProcessChild.Tables[0].DefaultView[0].Delete();
                }

                for (int i = 0; i < process.Count; i++)
                {
                    DataRow dr = dsProcessChild.Tables[0].NewRow();
                    dr["Id"] = headerData["Id"].ToString() + i.ToString();
                    dr["HeaderId"] = headerData["Id"].ToString();
                    dr["ProcessId"] = process[i].ToString();
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsProcessChild.Tables[0].Rows.Add(dr);
                }

                #endregion ProcessChild

                #region EntityChild

                DataSet dsEntityChild;
                ConnectionManager.DAL.ConManager cone = new ConnectionManager.DAL.ConManager("1");
                cone.OpenDataSetThroughAdapter("select * from dbo.ProducedMinAllowanceEntity where HeaderId ='" + headerData["Id"].ToString() + "'", out dsEntityChild, false, "1");

                while (dsEntityChild.Tables[0].DefaultView.Count > 0)
                {
                    dsEntityChild.Tables[0].DefaultView[0].Delete();
                }

                for (int i = 0; i < entity.Count; i++)
                {
                    DataRow dr = dsEntityChild.Tables[0].NewRow();
                    dr["Id"] = headerData["Id"].ToString() + i.ToString();
                    dr["HeaderId"] = headerData["Id"].ToString();
                    dr["EntityId"] = entity[i].ToString();
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsEntityChild.Tables[0].Rows.Add(dr);
                }


                #endregion EntityChild

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster , dsProcessChild , dsEntityChild);

                return headerData;

            }
            catch (Exception ex)
            {

                throw ex;

            }
        }


        public List<Dictionary<string, object>> saveChildPa(List<Dictionary<string, object>> childData , string headerId)
        {
            try
            {
                //Master Table - Wastw-Transaction
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string TableName = "dbo.ProducedMinAllowanceChild";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where headerId = '" + headerId + "'", out dsMaster, false, "1");

                while (dsMaster.Tables[0].DefaultView.Count > 0)
                {
                    dsMaster.Tables[0].DefaultView[0].Delete();
                }

                #region data Upload
                for (int i = 0; i< childData.Count; i++)
                {
                    var jj = childData[i];
                    jj["Id"] = headerId + i;
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = headerId + i;
                    dr["HeaderId"] = headerId;
                    dr["SkilLCategoryId"] = jj["SkillCategoryId"].ToString();
                    dr["OperationSequence"] = jj["OperationSequence"].ToString();
                    dr["SkillAllowance"] = clsStaticInfo.dbl(jj["SkillAllowance"].ToString());
                    dr["AdditionOperationAllowance"] = clsStaticInfo.dbl(jj["AdditionOperationAllowance"].ToString());

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);
                }



                #endregion data Upload


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return childData;

            }
            catch (Exception ex)
            {

                throw ex;

            }
        }
        #endregion Save

        #endregion ProductiveAllowance

        //--------------------------- Rate Setup

        #region RateSetup
        #region Get All Rate setup
        public IEnumerable<object> getRsMasterData()
        {
            try
            {
                var str = @"Select Id, UserName, EffectiveDate, Remarks,
                            STUFF((
                            SELECT ',' + p.UserName

                            FROM dbo.IncentiveRateSetupProcess irp 
                            left join hkp.Process p on p.Id = irp.ProcessId
                            where irp.HeaderId = pm.Id
                            FOR XML PATH('')

                            ),1,1,'') AS Processes,
                            STUFF((
                            SELECT ',' + e.UserName

                            FROM dbo.IncentiveRateSetupEntity ire 
                            left join org.Entity e on e.Id = ire.EntityId
                            where ire.HeaderId = pm.Id
                            FOR XML PATH('')

                            ),1,1,'') AS Entity
                            from dbo.IncentiveRateSetupHeader pm
                            ";

                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Get RS Child List
        public IEnumerable<object> getRsChildList(string Id)
        {
            try
            {
                var str = @"Select irc.* from dbo.IncentiveRateSetupChild irc where headerId ='" + Id + "'";
                            
                             

                DataTable dtChild = _sqlRepository.GetDataTable(str);


                if (dtChild.Rows.Count > 0)
                {
                   
                       return Library.Service.Helpers.DataTableExtensions.DataTableToJson(dtChild);
                    
                }
                else
                {
                    for (int i = 0; i < 10; i++)
                    {
                        DataRow dr = dtChild.NewRow();
                        dr["Id"] = null;
                        dr["HeaderId"] = Id;
                        dr["Effeciency"] = 0;
                        dr["EffeciencyRate"] = 0 ;
                        dr["Remarks"] = null ;
                        dtChild.Rows.Add(dr);
                    }
                }

                return Library.Service.Helpers.DataTableExtensions.DataTableToJson(dtChild);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion Get All Rate setup
        #region SaveOps
        
        public Dictionary<string, object> saveHeaderRs(Dictionary<string, object> headerData, List<string> process, List<string> entity)
        {
            try
            {
                //Master Table - Wastw-Transaction
                string TableName = "dbo.IncentiveRateSetupHeader";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                //if( cl headerData["Id"])

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id <> '" + headerData["Id"] + "' and UserName='" + headerData["UserName"].ToString() + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same UserName is already there!!");
                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id = '" + headerData["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master Upload
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID genid = new bplib.clsGenID();

                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    dr["UserName"] = headerData["UserName"].ToString();
                    dr["EffectiveDate"] = Convert.ToDateTime(headerData["EffectiveDate"].ToString());
                    dr["Remarks"] = headerData["Remarks"].ToString();
                    dr.EndEdit();
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    genid.GenID("dbo.IncentiveRateSetupHeader", out _Id);
                    headerData["Id"] = _Id;
                    dr["Id"] = _Id;
                    dr["UserName"] = headerData["UserName"].ToString();
                    dr["EffectiveDate"] = Convert.ToDateTime(headerData["EffectiveDate"].ToString());
                    dr["Remarks"] = headerData["Remarks"].ToString();
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);
                }



                #endregion data Master Upload

                #region ProcessChild

                DataSet dsProcessChild;
                ConnectionManager.DAL.ConManager conn = new ConnectionManager.DAL.ConManager("1");
                conn.OpenDataSetThroughAdapter("select * from dbo.IncentiveRateSetupProcess where HeaderId ='" + headerData["Id"].ToString() + "'", out dsProcessChild, false, "1");

                while (dsProcessChild.Tables[0].DefaultView.Count > 0)
                {
                    dsProcessChild.Tables[0].DefaultView[0].Delete();
                }

                for (int i = 0; i < process.Count; i++)
                {
                    DataRow dr = dsProcessChild.Tables[0].NewRow();
                    dr["Id"] = headerData["Id"].ToString() + i.ToString();
                    dr["HeaderId"] = headerData["Id"].ToString();
                    dr["ProcessId"] = process[i].ToString();
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsProcessChild.Tables[0].Rows.Add(dr);
                }

                #endregion ProcessChild

                #region EntityChild

                DataSet dsEntityChild;
                ConnectionManager.DAL.ConManager cone = new ConnectionManager.DAL.ConManager("1");
                cone.OpenDataSetThroughAdapter("select * from dbo.IncentiveRateSetupEntity where HeaderId ='" + headerData["Id"].ToString() + "'", out dsEntityChild, false, "1");

                while (dsEntityChild.Tables[0].DefaultView.Count > 0)
                {
                    dsEntityChild.Tables[0].DefaultView[0].Delete();
                }

                for (int i = 0; i < entity.Count; i++)
                {
                    DataRow dr = dsEntityChild.Tables[0].NewRow();
                    dr["Id"] = headerData["Id"].ToString() + i.ToString();
                    dr["HeaderId"] = headerData["Id"].ToString();
                    dr["EntityId"] = entity[i].ToString();
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsEntityChild.Tables[0].Rows.Add(dr);
                }


                #endregion EntityChild

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsProcessChild, dsEntityChild);

                return headerData;

            }
            catch (Exception ex)
            {

                throw ex;

            }
        }

        // -------------------------Save Child RS
        public List<Dictionary<string, object>> saveChildRs(List<Dictionary<string, object>> childData, string headerId)
        {
            try
            {
                //Master Table - Wastw-Transaction
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string TableName = "dbo.IncentiveRateSetupChild";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where headerId = '" + headerId + "'", out dsMaster, false, "1");

                while (dsMaster.Tables[0].DefaultView.Count > 0)
                {
                    dsMaster.Tables[0].DefaultView[0].Delete();
                }

                #region data Upload
                for (int i = 0; i < childData.Count; i++)
                {
                    var jj = childData[i];
                    if((int)(jj["Effeciency"]) > 0)
                    {
                        jj["Id"] = headerId + i;
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        dr["Id"] = headerId + i;
                        dr["HeaderId"] = headerId;
                        dr["Effeciency"] = jj["Effeciency"];
                        dr["EffeciencyRate"] = jj["EffeciencyRate"];
                        dr["Remarks"] = jj["Remarks"];

                    
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        
                    }
                }



                #endregion data Upload


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return childData;

            }
            catch (Exception ex)
            {

                throw ex;

            }
        }
        #endregion Save

        #endregion RateSetup


    }


}
