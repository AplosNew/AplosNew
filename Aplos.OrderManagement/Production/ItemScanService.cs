using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Library.Data.Sql;
using OTSBD;

namespace Library.Service.EmployeeServices
{
    public class ItemScanService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public ItemScanService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }
  
     
        public IEnumerable<object> FromLoc(string Entity,string Purpose)
        {
            try
            {
                var _sql = @"select distinct m.FromLocation as Text
                from mst.MaterialMovementMaster m
                where PurposeId='"+Purpose+"' and EntityId='"+Entity+"'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
      
        public IEnumerable<object> ToLoc(string Entity,string Purpose,string FromLoc)
        {
            try
            {
                var sql = @"select distinct m.ToLocation as Text,m.Id as Value
                from mst.MaterialMovementMaster m
                where PurposeId='" + Purpose+"' and EntityId='"+Entity+"' and FromLocation='"+FromLoc+"'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetPurpose(string Entity)
        {
            try
            {
                var _sql = @"select distinct PurposeId as Value,mp.UserName as Text 
                from mst.MaterialMovementMaster m
                left join hkp.MaterialMovementPurpose mp on mp.Id=m.PurposeId
                where m.EntityId='"+ Entity+"'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string SaveHeader(IEnumerable<ItemScanData> DataToSave)
        {
            try
            {
                DataSet dsMaster;
           
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<ItemScanData> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from dbo.ItemScan where Id='" + items[0].Id +"'", out dsMaster, false, "1");

                foreach (ItemScanData item in DataToSave)
                {

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                       
                        bplib.clsGenID id = new bplib.clsGenID();
                        id.GenIDYearly(DateTime.Now.ToShortDateString(), "Item Scan", out string NewId);



                        dr["Id"] = NewId;
                        dr["WorkDate"] = item.WorkDate;
                        dr["Time"] = item.Time;
                        dr["ShiftId"] = item.ShiftId;
                        dr["Grade"] = item.Grade;
                        dr["LocMasterId"] = item.LocMasterId;                       
                        dr["PurposeId"] = item.PurposeId;
                        dr["Remarks"] = item.Remarks;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = DateTime.Now.ToString();
                        dsMaster.Tables[0].Rows.Add(dr);


                    }
                    else
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                      
                        dr["WorkDate"] = item.WorkDate;
                        dr["Time"] = item.Time;
                        dr["ShiftId"] = item.ShiftId;
                        dr["Grade"] = item.Grade;
                        dr["LocMasterId"] = item.LocMasterId;
                        dr["PurposeId"] = item.PurposeId;
                        dr["Remarks"] = item.Remarks;
                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedDate"] = DateTime.Now.ToString();

                        dr.EndEdit();
                    }

                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                string MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                return MasterId;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public IEnumerable<object> GetShiftMaster(string PlantId)
        {
            try
            {
                var _sql = @"SELECT distinct SystemID as Value,UserName AS Text FROM [dbo].[ShiftDefination] where isnull(PlantID,'')='" + PlantId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string Create(string MId, IEnumerable<ItemScanChildData> DataToSave)
        {
            try
            {
                string inventory = "";
                string Booked, IsDespatch, ToLocation, FLoc = "";bool Inventchk=false ;
                decimal counter = 0,filter=0;
                DataSet dsMaster;
                string TableName = "dbo.ItemScanChild";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";

                var items = DataToSave.ToList();

              
                string PackedBy = "''";
                string RefNo = "''";
                string LocId =  items[0].LocMasterId;
                string User = items[0].AddedBy;  /// Can delete later on
                foreach (ItemScanChildData item in DataToSave)
                {
                    PackedBy += ",'" + item.PackedBy + "'";
                    RefNo += ",'" + item.RefNo + "'";
                }

                

                var sqly = @"select SystemId as EmpId,EmployeeCode from dbo.EmployeeInformation where EmployeeCode IN(" + PackedBy + ")";
                var EmpId = _sqlRepository.GetDataTable(sqly);


                // Check repeat Rows 
                var sql = @"select * from dbo.ItemScanChild where RefNo IN(" + RefNo + @")";
                con.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                // For History
                var sqlx = @"select * from dbo.ItemScanChildHistory where 1=2";
                con.OpenDataSetThroughAdapter(sqlx, out DataSet DsHistory, false, "1");

                // Inventory Check
                var _sql = @"select Inventorycheck from mst.MaterialMovementMaster where Id ='" + LocId + "'";
                var Location = _sqlRepository.GetDataTable(_sql);
                Inventchk =bplib.clsWebLib.GetBoolData(Location.Rows[0]["Inventorycheck"].ToString());

                DataTable ToLocList = new DataTable();
                if (Inventchk == true)
                {
                    var _sqlx = @"select FromLocation from mst.MaterialMovementMaster where Id='" + LocId + "'";
                    var fromloc = _sqlRepository.GetDataTable(_sqlx);
                    FLoc = fromloc.Rows[0]["FromLocation"].ToString();

                    _sqlx = @"select LocMasterId,ToLocation,Booked,RefNo,IsDespatch from 
                    dbo.ItemScanChild sc
                    left join mst.MaterialMovementMaster m on m.Id=sc.LocMasterId
                    where sc.RefNo IN(" + RefNo + ")";

                    ToLocList = _sqlRepository.GetDataTable(_sqlx);
                    if(ToLocList.Rows.Count >0)
                    {
                        filter = 1;
                    }
                }

                int Index = 0;
                string _Id = ""; string _Idx = "";
                foreach (ItemScanChildData item in DataToSave)
                {
                    Index++;
                    object Emp = DBNull.Value;
                    // Get EmpId
                    EmpId.DefaultView.RowFilter = "EmployeeCode='" + item.PackedBy + "'";
                    if (EmpId.DefaultView.Count > 0)
                    {
                        Emp = EmpId.DefaultView[0]["EmpId"].ToString();
                    }
                    counter = 0;
                    if (filter == 1)
                    {
                        ToLocList.DefaultView.RowFilter = "RefNo='" + item.RefNo + "'";
                        if (ToLocList.DefaultView.Count > 0)
                        {
                            ToLocation = ToLocList.DefaultView[0]["ToLocation"].ToString();
                            Booked = ToLocList.DefaultView[0]["Booked"].ToString();
                            IsDespatch = ToLocList.DefaultView[0]["IsDespatch"].ToString();

                            if (ToLocation == FLoc && Booked != "True" && IsDespatch != "True")
                            {
                                counter = 1;
                            }

                        }
                    }

                    dsMaster.Tables[0].DefaultView.RowFilter = @"RefNo='" + item.RefNo + "' ";
                   
                    if (dsMaster.Tables[0].DefaultView.Count == 0 && Inventchk != true)
                    {
                      
                        DataRow dr = dsMaster.Tables[0].NewRow();
                        if (_Id == "")
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                        }
                        dr["Id"] = "SC" + _Id + "-" + Index;
                        dr["MasterId"] = MId;
                        dr["NetWeight"] = item.NetWeight;
                        dr["GWeight"] = item.GWeight;
                        dr["PackedBy"] = Emp;
                        dr["Shade"] = item.Shade;
                        dr["AddedBy"] = User;
                        dr["AddedDate"] = DateTime.Now.ToString();
                        dr["ProductCode"] = item.ProductCode;
                        dr["POId"] = item.POId;
                        dr["LotNo"] = item.LotNo;
                        dr["RefNo"] = item.RefNo;
                        dr["Cones"] = item.Cones;
                        dr["LocMasterId"] = item.LocMasterId;
                        dr["Booked"] = 0;
                        dr["IsDespatch"] = 0;
                        dr["PackingId"] = DBNull.Value;
                        dsMaster.Tables[0].Rows.Add(dr);

                    }

                    else if (Inventchk == true && counter != 1 && filter == 1)
                    {
                        inventory = inventory+item.RefNo+" ";
                       
                    }

                    else if(Inventchk == true && counter !=1 && filter !=1)
                    {
                        inventory = inventory + item.RefNo+" ";

                    }                   
                    else
                    {                       

                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                        if (item.LocMasterId.ToUpper() != dr["LocMasterId"].ToString().ToUpper())
                        {

                            if (Inventchk == true && counter == 1 && filter==1)
                            {
                                
                                DataRow drx = DsHistory.Tables[0].NewRow();
                                if (_Idx == "")
                                {
                                    bplib.clsGenID genid = new bplib.clsGenID();
                                    genid.GenID("ItemScanChildHistory", out _Idx);
                                }
                                drx["Id"] = _Idx + "-" + Index;
                                drx["MasterId"] = dr["MasterId"].ToString();
                                drx["NetWeight"] = dr["NetWeight"].ToString();
                                drx["GWeight"] = dr["GWeight"].ToString();
                                drx["PackedBy"] = dr["PackedBy"].ToString();
                                drx["Shade"] = dr["Shade"].ToString();
                                drx["AddedBy"] = dr["AddedBy"].ToString();
                                drx["AddedDate"] = dr["AddedDate"].ToString();
                                drx["ProductCode"] = dr["ProductCode"].ToString();
                                drx["POId"] = dr["POId"].ToString();
                                drx["LotNo"] = dr["LotNo"].ToString();
                                drx["RefNo"] = dr["RefNo"].ToString();
                                drx["Cones"] = dr["Cones"].ToString();
                                drx["LocMasterId"] = dr["LocMasterId"].ToString();
                                drx["Booked"] = bplib.clsWebLib.GetBoolData(dr["Booked"].ToString());
                                drx["IsDespatch"] = bplib.clsWebLib.GetBoolData(dr["IsDespatch"].ToString());
                                drx["PackingId"] = dr["PackingId"].ToString();
                                DsHistory.Tables[0].Rows.Add(drx);



                                dr.BeginEdit();
                                dr["MasterId"] = MId;
                                dr["UpdatedBy"] = User;
                                dr["UpdatedDate"] = DateTime.Now.ToString();
                                dr["LocMasterId"] = item.LocMasterId;
                                dr.EndEdit();

                            }
                        }

                    }

                }

               

               
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, DsHistory);

                if(inventory !="")
                {
                    return "Inventory not Found of these cartons:- "+inventory;
                }

                return "true";

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        /// <summary>
        ///  For Booking & Dispatch
        /// </summary>
      
        public IEnumerable<object> GetCust()
        {
            try
            {
                var _sql = @"select distinct p.UserName as Text,pa.CustomerId as Value from 
                    trn.Packing pa left join hkp.Party p 
                    on pa.CustomerId=p.Id
					where InactiveDate>=CAST(GETDATE() AS Date) ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetPackingId(string Cust,string User)
        {
            try
            {
                var _sql = @"select distinct p.PackingId as Value from trn.Packing p
		     left join EmployeeInformation e on e.SystemId=p.ByWhom
		     left join [SEC].[User] u on u.EmployeeId=e.SystemId
		     where p.CustomerId = '"+Cust+"' and u.UserId='"+User+ "' " +
             "and InactiveDate>=CAST(GETDATE() AS Date) "; 
             
            return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetSO(string PId)
        {
            try
            {
                var _sql = @"select distinct pl.SOId,pl.PackingLineItemId from trn.PackingLineItem pl
                left join trn.Packing p ON pl.PackingId=p.PackingId
                where pl.PackingId='" + PId+ "' and InactiveDate>=CAST(GETDATE() AS Date) ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetProdCode(string PL)
        {
            try
            {
                var sql = @"select distinct p.ProductCode as Value from trn.POLotReference P
                left join trn.PackingLineItem pl ON pl.PackingLineItemId=p.PackingLineItemId
                where p.PackingLineItemId='"+PL+"'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
      
        public IEnumerable<object> GetPO(string PL,string Prod)
        {
            try
            {
                var sql = @"select distinct p.PONo as Value from trn.POLotReference P
                left join trn.PackingLineItem pl ON pl.PackingLineItemId=p.PackingLineItemId
                where p.PackingLineItemId='"+PL+"' and p.ProductCode='"+Prod+"'";
                
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public IEnumerable<object> GetLotId(string PL, string Prod,string PO)
        {
            try
            {
                var sql = @"select distinct p.LotNo ,p.Id as SystemId,p.PlanQty from trn.POLotReference P
                left join trn.PackingLineItem pl ON pl.PackingLineItemId=p.PackingLineItemId
                where p.PackingLineItemId='" + PL+"' and p.ProductCode='"+Prod+"' and p.PONo='"+PO+ "'and p.Status='Active'";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetBookedQty(string Lot, string Prod, string PO,string Pqty,string PoLotRefernceId)
        {
            try
            {
                decimal PlanQty = Convert.ToDecimal(Pqty);
                var sql = @"select '" + PlanQty + "'-req.BookedQty as AvailQty from(select isnull(Floor(Sum(Netweight)),0) as BookedQty " +
                    "from dbo.ItemScanChild where ProductCode='"+Prod+"'and POId='"+PO+ "'and PackingId='"+PoLotRefernceId+"' " +
                    "and LotNo='" + Lot+"' and Booked ='1' ) as req";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public string CreateDispatch(IEnumerable<ItemScanChildData> DataToSave)
        {
            try
            {

                if (DataToSave.Count() == 0)
                {
                    return "";
                }

                string RefNo = "''";
                foreach (ItemScanChildData item in DataToSave)
                {
                    RefNo += ",'" + item.RefNo + "'";
                }

                var items=DataToSave.ToList();

                string date= DateTime.Now.ToString();
               
                var sql = @"Update dbo.ItemScanChild Set UpdatedBy='"+items[0].UpdatedBy+ "' ,PackingId ='" + items[0].PackingId+"',Booked=1 " +
                        "where RefNo IN("+RefNo+@") and Booked=0 AND IsDespatch=0";
                
                ConnectionManager.DAL.ConManager objCone = null;
                objCone = new ConnectionManager.DAL.ConManager("1");
                objCone.OpenConnection("1");
                objCone.BeginTransaction();

                objCone.ExecuteNonQueryWrapper(sql, true, "1");
                objCone.CommitTransaction();

                return "true";

              
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        
        public static void SaveLog(string Message, string Cartons, string User)
        {

            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter("select * from PackingLog where 1=2", out DataSet dsPacking, false, false, "", "1");

            DataRow dr = dsPacking.Tables[0].NewRow();
            dr["ScheduleMessage"] = Message;
            dr["UserName"] = User;
            dr["CartonNo"] = Cartons;
            dr["AddedDate"] = DateTime.Now.ToString();
            dsPacking.Tables[0].Rows.Add(dr);


            SaveDataSets(dsPacking);
        }
       
        private static void SaveDataSets(params DataSet[] dsRef)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                int i = 0;

                objCon.SaveDataSetThroughAdapter(ref dsRef[i], true, "1");
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                try
                {
                    if (IsTransactionStarted)
                    {
                        objCon.RollBack();
                    }
                    objCon.CloseConnection();
                }
                catch (Exception exp)
                {
                    throw ex;
                }
            }
            finally
            {
                objCon = null;
            }
        }//End Function


    }

    public class ItemScanData
    {
        public string AddedBy { get; set; }
        public string Id { get; set; }
        public DateTime AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string PurposeId { get; set; }
        public string Remarks { get; set; }
        public DateTime WorkDate { get; set; }
        public DateTime Time { get; set; }
        public string Grade { get; set; }
        public string LocMasterId { get; set; }
        public string ShiftId { get; set; }

    }

   public class ItemScanChildData
    {
        public string AddedBy { get; set; }
        public string Id { get; set; }
        public DateTime AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string ProductCode { get; set; }
        public string POId { get; set; }
        public string NetWeight { get; set; }
        public string GWeight { get; set; }
        public string LotNo { get; set; }
        public string LocMasterId { get; set; }
        public string Cones { get; set; }
        public string Shade { get; set; }
        public string RefNo { get; set; }
        public string PackedBy { get; set; }
        public string Booked { get; set; }
        public string IsDespatch { get; set; }
        public string PackingId { get; set; }             
    }

}
  