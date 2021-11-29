using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
#region Using

using Library.Service.Enums;
using Library.Service.Logs;

#endregion Using

namespace Library.OrderManagement.Production
{
	public class WasteMasterService
	{
		private readonly SqlRepository _sqlRepository;

		#region Constructor
		public WasteMasterService()
		{
			_sqlRepository = new SqlRepository();
		}
		#endregion Constructor

        public IEnumerable<object> getCompany ()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var str = "Select Id as Value , UserName as Text from Org.Company where CompanyGroupId = '" + identity.CompanyGroupId + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getPlants (string cmp)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var str = "Select Id as Value , UserName as Text from Org.Plant where CompanyId = '"+cmp+"'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getEntity (string PlantId)
        {
            try
            {
                var str = "Select Id as Value , UserName as Text from Org.Entity where PlantId = '" + PlantId + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getUOM ()
        {
            try
            {
                var str = "Select Id as Value , UserName as Text from scs.UnitOfMeasurement ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IEnumerable<object> getBudgetId(string EId)
        {
            try
            {
                var str = @"Select mb.Id , mb.Code , p.UserName as Position , c.UserName as Company, e.UserName as Entity from mst.ManpowerBudget mb
                            left join org.Position p on p.Id = mb.PositionId
                            left join org.Company c on c.Id = mb.CompanyId
                            left join org.Entity e on e.Id = mb.EntityId
                            where e.Id = '"+EId+"' ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

		public IEnumerable<object> Get( string Id)
        {
			try
            {
				var str = @"select wm.* , mb.Code as BudgetCode,  e.UserName as Entity, p.UserName as Plant , c.UserName as Company,c.id as CompanyId , p.Id as PlantId 
                            from
                            dbo.WasteMaster wm
                            left join org.Entity e on e.Id = wm.EntityId
                            left join org.Plant p on p.Id = e.PlantId
                            left join org.Company c on c.Id = p.CompanyId
                            left join mst.ManpowerBudget mb on mb.Id = wm.BudgetId
                            where wm.Id = '" + Id + "' ";
				return _sqlRepository.GetDataCollection(str);
            }
			catch(Exception e)
            {
				throw e;
            }
        }

		public IEnumerable<object> GetList(string strkey)
        {
			try
            {
				string sql = @"select wm.* , p.Id as PlantId , c.Id as CompanyId , e.UserName as Entity ,p.UserName as Plant , c.UserName as Company , mb.Code as BudgetCode , uom.UserName as UOM 
                                from dbo.WasteMaster wm
                                left join org.Entity e on e.Id = wm.EntityId
                                left join org.Plant p on p.Id = e.PlantId
                                left join org.Company c on c.Id = p.CompanyId
                                left join mst.ManpowerBudget mb on mb.Id = wm.BudgetId
                                left join scs.UnitOfMeasurement uom on uom.Id = wm.UOMId 
                                order by wm.Sequence asc";

				return _sqlRepository.GetDataCollection(sql);
			}
			catch(Exception e)
            {
				throw e;
            }
        }

		public Dictionary<string, object> Create(Dictionary<string, object> data)
        {
            try
            {
                string TableName = "dbo.WasteMaster";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where ItemName='" + data["ItemName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Item Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), TableName, out _Id);

                    data["Id"] = "WM" + _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return data;

            }
            catch (Exception ex)
            {

                throw ex;

            }
        }

        public void Delete (string id)
        {
            try
            {
                string TableName = "dbo.WasteMaster";

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

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
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
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
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }

    }
}
