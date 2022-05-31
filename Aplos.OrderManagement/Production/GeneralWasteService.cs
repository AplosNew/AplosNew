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
    public class GeneralWasteService
    {
        private readonly SqlRepository _sqlRepository;

        #region Constructor
        public GeneralWasteService()
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion Constructor

        

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

        public IEnumerable<object> getEntity()
        {
            try
            {
                var str = @"Select e.Id as EntityId, e.UserName as EntityName , p.UserName as Plant, c.UserName as Company from org.Entity e
                                left join org.Plant p on p.Id = e.PlantId
                                left join org.Company c on c.Id = p.CompanyId
                        ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getView(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                //var budC = @"Select ei.BudgetCode  as Id
                //                from SEC.[USER] u 
                //                left join dbo.EmployeeInformation ei on ei.SystemId = u.EmployeeId
                //                where ei.BudgetCode is not null --and u.Id = '"+identity.UserId+@"'  ";
                //DataTable tb = _sqlRepository.GetDataTable(budC);

                string str = "";
                
                //if (tb.Rows.Count == 0)
                //{
                //    throw new Exception("You are not an Employee with a Budget Code!");
                //}
                

                //else
                //{
                    //string Bdc = tb.Rows[0]["Id"].ToString();
        //            str = @"Select wm.Id ,wm.Sequence, wm.ProcessId ,p.UserName as process, wm.ItemName, wm.Category ,
        //                        wm.SubCategory , wm.code , wm.UOMId ,uom.UserName as Uom ,wbd.BudgetId , ept.EntityId,
								//mb.Code as BudgetCode , e.UserName as EntityName
        //                        from dbo.WasteMaster wm
        //                        left join dbo.WasteBudgetDetail wbd on wbd.WasteMasterId = wm.Id
        //                        left join hkp.EntityProcessTag ept on ept.ProcessId = wm.ProcessId
        //                        left join hkp.Process p on p.Id = wm.ProcessId
        //                        left join scs.UnitOfMeasurement uom on uom.Id = wm.UOMId
								//left join org.Entity e on e.Id = ept.EntityId
								//left join mst.ManpowerBudget mb on mb.Id = wbd.BudgetId
        //                        where ept.EntityId = '" + Id+"'";

                str = @"Select wm.Id ,wm.Sequence, wm.ProcessId ,p.UserName as process, wm.ItemName, wm.Category ,
                                wm.SubCategory , wm.code , wm.UOMId ,uom.UserName as Uom , ept.EntityId, e.UserName as EntityName
                                from dbo.WasteMaster wm
                                --left join dbo.WasteBudgetDetail wbd on wbd.WasteMasterId = wm.Id
                                left join hkp.EntityProcessTag ept on ept.ProcessId = wm.ProcessId
                                left join hkp.Process p on p.Id = wm.ProcessId
                                left join scs.UnitOfMeasurement uom on uom.Id = wm.UOMId
								left join org.Entity e on e.Id = ept.EntityId
								--left join mst.ManpowerBudget mb on mb.Id = wbd.BudgetId
                                where ept.EntityId = '" + Id + "'";

                  //}
                  DataTable dt =  _sqlRepository.GetDataTable(str);
                dt.Columns.Add("Quantity", typeof(double));
                dt.Columns.Add("Remarks", typeof(string));

                for(int i = 0; i< dt.Rows.Count;i++)
                {
                    dt.Rows[i]["Quantity"] = 0;
                    dt.Rows[i]["Remarks"] ="";
                }

                return Service.Helpers.DataTableExtensions.DataTableToJson(dt);
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
				string sql = @"select wm.* , uom.UserName as UOM
                                from dbo.WasteMaster wm
                                
                                left join scs.UnitOfMeasurement uom on uom.Id = wm.UOMId 
                                order by wm.Sequence asc";

				return _sqlRepository.GetDataCollection(sql);
			}
			catch(Exception e)
            {
                throw e;
            }
        }

        public List<Dictionary<string, object>> Create(List<Dictionary<string, object>> Data , string Date, string LocationId)
        {
            try
            {
                //Master Table - Wastw-Transaction
                string TableName = "dbo.WasteTransactionData";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where 1=2  ", out dsMaster, false, "1");
                
                string _Id = "";

                #region data Upload
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                for (int i = 0; i < Data.Count; i++)
                {

                    if( clsStaticInfo.dbl(Data[i]["Quantity"].ToString()) > 0.0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("dbo.WasteTransactionData", out _Id);
                        dr["Id"] = "WT"+DateTime.Now.Year.ToString()+ '-' + _Id;
                        dr["WasteMasterId"] = Data[i]["Id"].ToString();
                        dr["EntityId"] = Data[i]["EntityId"].ToString();
                        dr["WasteLocationId"] = LocationId;
                        dr["Date"] = Convert.ToDateTime(Date);
                        dr["Quantity"] = Data[i]["Quantity"].ToString();
                        dr["Remarks"] = Data[i]["Remarks"].ToString();
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    
                }
                #endregion data Upload

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster );

                return Data;

            }
            catch (Exception ex)
            {

                throw ex;

            }
        }

        //public void Delete(string id)
        //{
        //    try
        //    {
        //        string TableName = "dbo.WasteMaster";

        //        if (string.IsNullOrEmpty(id))
        //            throw new Exception("Select entry first");

        //        ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
        //        conC.BeginTransaction();
        //        conC.executeQuery("delete from dbo.WasteBudgetDetail where WasteMasterId ='" + id + "'");
        //        conC.CommitTransaction();


        //        ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
        //        con.BeginTransaction();
        //        con.executeQuery("delete from " + TableName + " where id='" + id + "'");
        //        con.CommitTransaction();

        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;

        //    }
        //}

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
