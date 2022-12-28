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

        /*public IEnumerable<object> getCompany ()
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
        }*/

        public IEnumerable<object> getProcess ()
        {
            try
            {
               // var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var str = "Select Id as Value , UserName as Text from hkp.process";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetWasteType()
        {
            var str = "Select Id as Value , UserName as Text from HKP.WasteType";
            return _sqlRepository.GetDataCollection(str);
        }
       /* public IEnumerable<object> getEntity(string PlantId)
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
        }*/

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

        public IEnumerable<object> getBudgetId()
        {
            try
            {
                var str = @"Select mb.Id , mb.Code , p.UserName as Position , c.UserName as Company, pp.UserName as Plant ,e.UserName as Entity from mst.ManpowerBudget mb
                            left join org.Position p on p.Id = mb.PositionId
                            left join org.Company c on c.Id = mb.CompanyId
                            left join org.Entity e on e.Id = mb.EntityId
							left join org.Plant pp on pp.Id = e.PlantId
                        ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetMaster(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var str = @"select * from dbo.WasteMaster where Id = '"+Id+"' ";
				return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> GetChild(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var str = @"select BudgetId from dbo.WasteBudgetDetail where WasteMasterId = '" + Id + "' ";
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
				string sql = @"select wm.* , uom.UserName as UOM, WT.UserName WasteType
                                from dbo.WasteMaster wm
                                
                                left join scs.UnitOfMeasurement uom on uom.Id = wm.UOMId
                                left join HKP.WasteType WT on WT.Id = wm.WasteTypeId
                                order by wm.Sequence asc";

				return _sqlRepository.GetDataCollection(sql);
			}
			catch(Exception e)
            {
                throw e;
            }
        }

        public Dictionary<string, object> Create(Dictionary<string, object> data, List<string> budgets)
        {
            try
            {
                //Master Table - WasteMaster
                string TableName = "dbo.WasteMaster";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where ItemName='" + data["ItemName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Item Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                 #region data Master update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "WM" + _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                // Child table - WasteBudgetDetail
               
                DataSet dsChild;
                ConnectionManager.DAL.ConManager conC = new ConnectionManager.DAL.ConManager("1");
                conC.OpenDataSetThroughAdapter("select * from dbo.WasteBudgetDetail where WastemasterId = '"+ data["Id"].ToString() + "'", out dsChild, false, "1");

                while (dsChild.Tables[0].DefaultView.Count > 0)
                {
                    dsChild.Tables[0].DefaultView[0].Delete();
                }

                string _IdC = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                #region data Child update
                
                    for(int i = 0; i < budgets.Count; i++)
                    {
                        DataRow dr = dsChild.Tables[0].NewRow();
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("dbo.WasteBudgetDetail", out _IdC);
                        dr["Id"] = "WBD" + _IdC;
                        dr["WasteMasterId"] = data["Id"].ToString();
                        dr["BudgetId"] = budgets[i].ToString();
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dsChild.Tables[0].Rows.Add(dr);
                    }
                
                #endregion data update




                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster , dsChild);

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

                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from dbo.WasteBudgetDetail where WasteMasterId ='" + id + "'");
                conC.CommitTransaction();


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
                where UserId='" + UserId + "'";
                return _sqlRepository.GetDataCollection(sql, null);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetItemName(string BudgetId)
        {
            try
            {
                var sql = @"select w.ItemName,w.Id as MasterId from WasteMaster w 
                left join WasteBudgetDetail wb on wb.WasteMasterId=w.Id
                where wb.BudgetId='"+BudgetId+"'";
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
                var str = @"Select wtd.Id as WTDId ,format(wtd.Date , 'dd-MMM-yyyy' ) as Dates, wm.ItemName,wm.Category,wm.SubCategory,wtd.Quantity
                           , wtd.AddedBy , e.UserName as EntityName
                            from dbo.WasteTransactionData wtd
                            left join dbo.WasteMaster wm on wm.Id = wtd.WasteMasterId
                            left join org.Entity e on e.Id=wtd.EntityId
                             where wtd.Date between '" + FromDate + "' and '" + ToDate + "' and wtd.EntityId = '" + EntityId+"'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getClickedData(string Id)
        {
            try
            {
                var str = @"Select wtd.Id as WTDId ,format(wtd.Date , 'dd-MMM-yyyy' ) as Dates, wm.ItemName,wm.Category,wm.SubCategory,wtd.Quantity , wtd.AddedBy
                            from dbo.WasteTransactionData wtd
                            left join dbo.WasteMaster wm on wm.Id = wtd.WasteMasterId
                            where wtd.Id = '"+Id+"'";

                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string, object> saveQuantity(Dictionary<string, object> data)
        {
            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery(@"Update dbo.WasteTransactionData

                            Set Quantity = "+clsStaticInfo.dbl(data["Quantity"].ToString())+@"

                            where Id = '"+data["WTDId"]+"'");
                con.CommitTransaction();
                return data;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public DataTable getGroupWasteReport(string EntityId, string FromDate, string ToDate)
        {
            try
            {
                var str = @"Select wtd.Id as WTDId ,format(wtd.Date , 'dd-MMM-yyyy' ) as Dates, wm.ItemName,wm.Category,wm.SubCategory,wtd.Quantity , wtd.Remarks
                           , wtd.AddedBy , e.UserName as EntityName
                            from dbo.WasteTransactionData wtd
                            left join dbo.WasteMaster wm on wm.Id = wtd.WasteMasterId
                            left join org.Entity e on e.Id=wtd.EntityId
                             where wtd.Date between '" + FromDate + "' and '" + ToDate + "' and wtd.EntityId = '" + EntityId + "'";
                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
