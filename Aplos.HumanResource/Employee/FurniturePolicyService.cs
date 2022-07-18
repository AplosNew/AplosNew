using Library.Data.Sql;
using Library.Service.Extension;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.Employee
{
   public class FurniturePolicyService
    {
        private readonly SqlRepository _sqlRepository;
        public FurniturePolicyService()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> getFurnitureMaster()
        {
            try
            {
                var sql = @"select distinct UserName as Text from HKP.furnitureMaster";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getDesignationMaster()
        {
            try
            {
                var sql = @"select DISTINCT d.UserName as Text from MST.DesignationMaster d ORDER BY Text";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getFurnitureGridView(string username)
        {
            try
            {
                var sql = @"select fm.* from HKP.furnitureMaster fm where fm.UserName = '"+ username + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getDesignationGridView(string username)
        {
            try
            {
                var sql = @"select dm.*, dg.UserName as DesignationGroup, dsg.UserName as Designation, ec.UserName as EmployeeCategory from MST.DesignationMaster dm
left join HKP.Designation dg on dg.Id = dm.DesignationId
left join HKP.DesignationGroup dsg on dsg.Id = dm.DesignationGroupId 
left join HKP.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
 where dm.UserName = '" + username + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public string save(Dictionary<string, object> data)
        //{
        //    try
        //    {
        //        string TableName1 = "HKP.FurnitureMaster";
        //        string TableName2 = "HKP.FurnitureMaster";
        //        DataSet dsMaster;
        //        ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                
        //        con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

        //        string _Id = "";

        //        #region data update
              
        //            bplib.clsGenID genid = new bplib.clsGenID();
        //            genid.GenID(TableName1, out _Id);

        //            data["Id"] = "FM" + _Id;
        //            AddNewRow(dsMaster.Tables[0], data);
               
        //        #endregion data update

        //        clsStaticInfo _info = new clsStaticInfo();
        //        _info.SaveDataSets(dsMaster);

        //        return "Success";

        //    }
        //    catch (Exception ex)
        //    {

        //        return ex.Message;

        //    }
        //}

        //private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

        //    DataRow dr = dt.NewRow();

        //    foreach (var item in sourceData.Keys)
        //    {
        //        try
        //        {
        //            dr[item] = sourceData[item];
        //        }
        //        catch (Exception)
        //        {
        //        }
        //    }
        //    dr["AddedBy"] = identity.Name;
        //    dr["AddedDate"] = System.DateTime.Now.ToString();
        //    dr["AddedFromIP"] = identity.IPAddress;
        //    //dr["UpdatedBy"] = identity.Name;
        //    //dr["UpdatedDate"] = System.DateTime.Now.ToString();
        //    //dr["UpdatedFromIP"] = identity.IPAddress;

        //    dt.Rows.Add(dr);
        //}
        //private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    dr.BeginEdit();

        //    foreach (var item in sourceData.Keys)
        //    {
        //        try
        //        {
        //            dr[item] = sourceData[item];
        //        }
        //        catch (Exception)
        //        {
        //        }
        //    }
        //    dr["UpdatedBy"] = identity.Name;
        //    dr["UpdatedDate"] = System.DateTime.Now.ToString();
        //    dr["UpdatedFromIP"] = identity.IPAddress;
        //    dr.EndEdit();
        //}
    }
}
