using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.PlantWiseHRMS
{
   public class clsPlantWiseHRMSSetting
    {
        string TableName = "dbo.PlantWiseHRMSSetting";
        ISqlRepository _sqlRepository;
        public clsPlantWiseHRMSSetting()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> GetModPlant(string CompanyId)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"select  
                            p.Id Value
                            ,case when pw.SystemID is null then  p.UserName +' (TBD)' else  p.UserName+' (Done)' end Text
                            from ORG.Plant p 
                            left join PlantWiseHRMSSetting pw on p.Id = pw.PlantID
                            where p.CompanyId = '" + CompanyId + "' ";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function
        public IEnumerable<object> GetList(string CompanyId)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"select  p.CompanyId , p.Id PlantID, pw.*,
                            case when pw.SystemID is null then  p.UserName +' (TBD)' else  p.UserName+' (Done)' end PlantName
                            from ORG.Plant p 
                            left join PlantWiseHRMSSetting pw on p.Id = pw.PlantID
                            where p.CompanyId = '" + CompanyId + "' ";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public IEnumerable<object> GetPlantList(string PlantID)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"select pw.*, p.CompanyId from PlantWiseHRMSSetting pw
                            left join ORG.Plant p on p.Id = pw.PlantID
                            where pw.PlantID =  '" + PlantID + "' ";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function
        public void Save(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                //con.OpenDataSetThroughAdapter("select * from " + TableName + " where PlantID='" + data["PlantID"] + "' AND  SystemID<>'" + data["SystemID"] + "'", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //    throw new Exception("Same Setting already exists!!!");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where SystemID='" + data["SystemID"] + "'", out dsMaster, false, "1");
                string _Id = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["SystemID"] = "PW_HR-" + _Id;
                    data["AttdnProcBase"] = "EnrollmentID";
                    data["GroupID"] = identity.CompanyGroupId;
                    data["AddedBy"] = identity.Name;
                    data["DateAdded"] = System.DateTime.Now.ToString();
                    data["UpdatedBy"] = identity.Name;
                    data["DateUpdated"] = System.DateTime.Now.ToString();
                    data["RoundDayINFinalSettlement"] = false;
                    data["IsTransportGroupMandatory"] = false;
                    data["IsResidenceGroupMandatory"] = false;
                    if (Convert.ToBoolean(data["IsOTOverHalfDay"]) == true)
                    {
                        data["IsOTOverHalfDay"] = false;
                    }
                    else
                    {
                        data["IsOTOverHalfDay"] = true;
                    }
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["SystemID"].ToString();
                    data["UpdatedBy"] = identity.Name;
                    data["DateUpdated"] = System.DateTime.Now.ToString();
                    data["IsTransportGroupMandatory"] = false;
                    data["IsResidenceGroupMandatory"] = false;
                    data["RoundDayINFinalSettlement"] = false;
                    if (Convert.ToBoolean(data["IsOTOverHalfDay"]) == true)
                    {
                        data["IsOTOverHalfDay"] = false;
                    }
                    else
                    {
                        data["IsOTOverHalfDay"] = true;
                    }
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

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();
            
            foreach (var item in sourceData.Keys)
            {
                try
                {
                    if (item != "PlantName")
                    {
                        if (item != "CompanyId")
                        {
                            dr[item] = sourceData[item];
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            
            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();
            dr["GroupID"] = identity.CompanyGroupId;
            dr["UpdatedBy"] = identity.Name;
            dr["DateUpdated"] = System.DateTime.Now.ToString();
            foreach (var item in sourceData.Keys)
            {
            
                try
                {
                    if (item != "PlantName")
                    {
                        if (item != "CompanyId")
                        {
                            dr[item] = sourceData[item]; 
                        }
                    }



                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            
            dr.EndEdit();
        }

    }
}
