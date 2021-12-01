using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Linq;

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

        public IEnumerable<object> getEntity(string PlantId)
        {
            try
            {
                var str = "Select Id as Value , UserName as Text from Org.Entity where PlantId = '" + PlantId + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getUOM()
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
                            where e.Id = '" + EId + "' ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> Get(string Id)
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
            catch (Exception e)
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

        public void Delete(string id)
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

    public class WasteTransactionService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public WasteTransactionService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        public IEnumerable<object> GetBudgetInfo(string UserId)
        {
            try
            {
                var sql = @"select distinct u.Id as Value,u.UserId as Text,u.EmployeeId,
                b.Id as BudgetId from [SEC].[User] u 
                left join EmployeeInformation e on e.SystemId=u.EmployeeId
                left join mst.ManpowerBudget b on b.Id=e.BudgetCode
                where UserId='"+UserId+"'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetItemName(string Entity,string BudgetId)
        {
            try
            {
                var sql = @"select Id as MasterId,ItemName from WasteMaster where EntityId='"+Entity+ "'" +
                    " and BudgetId='"+BudgetId+"'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string SaveData(IEnumerable<WasteTransactionModel> DataToSave)
        {
            try
            {
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<WasteTransactionModel> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from dbo.WasteTransactionData where 1=2", out dsMaster, false, "1");

                foreach (WasteTransactionModel item in DataToSave)
                {

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();


                        bplib.clsGenID id = new bplib.clsGenID();
                        id.GenIDYearly(DateTime.Now.ToShortDateString(), "WasteTransactionData", out string NewId);

                        dr["Id"] = "WT"+ NewId;
                        dr["Date"] = item.Date;
                        dr["EntityId"] = item.EntityId;
                        dr["WasteMasterId"] = item.WasteMasterId;
                        dr["Quantity"] = item.Quantity;
                        dr["Remarks"] = item.Remarks;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = DateTime.Now.ToString();
                        dr["AddedFromIP"] = item.AddedFromIP;

                        dsMaster.Tables[0].Rows.Add(dr);


                    }                   

                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                string MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                if(MasterId.Contains("WT"))
                {
                    return "true";
                }
                return "false";

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


    }


    public class WasteTransactionModel
    {
        public string Id { get; set; }
        public string WasteMasterId { get; set; }
        public string EntityId { get; set; }
        public string Remarks { get; set; }
        public string Quantity { get; set; }
        public DateTime Date { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedFromIP { get; set; }
      
    }



    public class WasteTransactionReportService
    {
        SqlRepository _sqlRepository;

        public WasteTransactionReportService()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> getEntity()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var str = "Select Id as Value , UserName as Text from Org.Entity where PlantId = '" + identity.PlantId + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getData(string EntityId , string ToDate , string FromDate)
        {
            try
            {
                var str = @"Select wtd.Id as WTDId ,format(wtd.Date , 'dd-MMM-yyyy' ) as Dates, wm.ItemName,wm.Category,wm.SubCategory,wtd.Quantity , wtd.AddedBy
                            from dbo.WasteTransactionData wtd
                            left join dbo.WasteMaster wm on wm.Id = wtd.WasteMasterId
                            where wtd.Date between '"+FromDate+"' and '"+ToDate+"' and wm.EntityId = '"+EntityId+"'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        //public IEnumerable<object> getClickedData(string Id)
        //{

        //}

    }
}
