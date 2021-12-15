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
    public class GeneralDataMasterService
    {
        private readonly SqlRepository _sqlRepository;

        #region Constructor
        public GeneralDataMasterService()
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion Constructor

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
                string TableName = "dbo.GeneralDataMaster";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), TableName, out _Id);

                    data["Id"] = "GD" + _Id;
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
                string TableName = "dbo.GeneralDataMaster";

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

    public class GeneralDataOperationsService
    {
        private readonly SqlRepository _sqlRepository;

        #region Constructor
        public GeneralDataOperationsService()
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion Constructor

        public DataTable getGeneralDataUploadFile()
        {
            try
            {
                var str = @"Select * from dbo.GeneralDataUpload";
                return _sqlRepository.GetDataTable(str);
            }
            catch( Exception ex )
            {
                throw ex;
            }
        }

        public DataTable getGeneralMasterFile()
        {
            try
            {
                var str = @"Select * from dbo.GeneralDataMaster";
                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable getEmployeesAll()
        {
            try
            {
                var str = @"Select SystemId, EmployeeCode from dbo.EmployeeInformation
                           ";

                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public List<Dictionary<string, object>> SaveFileList(List<Dictionary<string, object>> data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                
                string TableName = "dbo.GeneralDataUpload";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where 1 = 2", out dsMaster, false, "1");

                string Tid = "0";
                int Num = 0;

                DataTable TIdTable = _sqlRepository.GetDataTable("Select top 1 TransactionId from dbo.GeneralDataUpload order by AddedDate desc");
                if(TIdTable.Rows.Count>0)
                {
                     Tid = TIdTable.Rows[0]["TransactionId"].ToString();

                    Regex re = new Regex(@"([a-zA-Z]+)(\d+)");
                    Match result = re.Match(TIdTable.Rows[0]["TransactionId"].ToString());

                    string alphaPart = result.Groups[1].Value;
                    string numberPart = result.Groups[2].Value;
                    int numberParts = int.Parse(result.Groups[2].Value)+1;
                    Num = numberParts;
                }


                string _Id = "";
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        Dictionary<string, object> jj = data[i];
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);
                        
                        jj["Id"] = _Id;
                        jj["TransactionId"] = "TId" + Num;
                        jj["TransactionDate"] = Convert.ToDateTime(jj["TransactionDate"]);
                        AddNewRow(dsMaster.Tables[0], jj);
                    }


                }

               
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return data;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
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
            dr["AddedDate"] = DateTime.Now;
            dr["AddedFromIP"] = identity.IPAddress;
           

            dt.Rows.Add(dr);
        }
    }


}
