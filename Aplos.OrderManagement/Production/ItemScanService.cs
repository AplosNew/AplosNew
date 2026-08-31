using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Library.Data.Sql;
using OTSBD;
using bplib;

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


        public IEnumerable<object> FromLoc(string Entity, string Purpose)
        {
            try
            {
                var _sql = @"select distinct m.FromLocation as Text
                from mst.MaterialMovementMaster m
                where PurposeId='" + Purpose + "' and EntityId='" + Entity + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> ToLoc(string Entity, string Purpose, string FromLoc)
        {
            try
            {
                var sql = @"select distinct m.ToLocation as Text,m.Id as Value
                from mst.MaterialMovementMaster m
                where PurposeId='" + Purpose + "' and EntityId='" + Entity + "' and FromLocation='" + FromLoc + "'";
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
                where m.EntityId='" + Entity + "'and mp.Active='1'";
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

                con.OpenDataSetThroughAdapter("select * from dbo.ItemScan where Id='" + items[0].Id + "'", out dsMaster, false, "1");

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



        /* public string Create(string MId, IEnumerable<ItemScanChildData> DataToSave)
         {
             try
             {
                 string processId = "";
                 string inventory = "";
                 string Booked, IsDespatch, ToLocation, FLoc = ""; bool Inventchk = false;
                 decimal counter = 0, filter = 0;
                 DataSet dsMaster;
                 string TableName = "dbo.ItemScanChild";

                 ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                 if (DataToSave.Count() == 0)
                     return "";

                 var items = DataToSave.ToList();


                 string PackedBy = "''";
                 string RefNo = "''";
                 string LocId = items[0].LocMasterId;
                 string User = items[0].AddedBy;  /// Can delete later on
                 foreach (ItemScanChildData item in DataToSave)
                 {
                     PackedBy += ",'" + item.PackedBy + "'";
                     RefNo += ",'" + item.RefNo + "'";
                 }



                 var sqly = @"select SystemId as EmpId,EmployeeCode from dbo.EmployeeInformation where EmployeeCode IN(" + PackedBy + ")";
                 var EmpId = _sqlRepository.GetDataTable(sqly);

                 //getscandata
                 var sqlscan = @"Select WorkDate,ShiftId,Grade,PurposeId,LocMasterId from dbo.ItemScan Where Id='" + MId + "'";
                 DataTable dtScan = _sqlRepository.GetDataTable(sqlscan);

                 DateTime WorkDate = Convert.ToDateTime(dtScan.Rows[0]["WorkDate"].ToString());
                 string ShiftId = dtScan.Rows[0]["ShiftId"].ToString();
                 string Grade = dtScan.Rows[0]["Grade"].ToString();
                 string PurposeId = dtScan.Rows[0]["PurposeId"].ToString();

                 //getProcess&Entity
                 var sqlProcess = @"SELECT ProcessId FROM HKP.MaterialMovementPurpose where Id ='" + PurposeId + "'";
                 DataTable dtProcess = _sqlRepository.GetDataTable(sqlProcess);
                 processId = dtProcess.Rows[0]["ProcessId"].ToString();


                 // Check repeat Rows 
                 var sql = @"select * from dbo.ItemScanChild where RefNo IN(" + RefNo + @")";
                 con.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                 // For History
                 var sqlx = @"select * from dbo.ItemScanChildHistory where 1=2";
                 con.OpenDataSetThroughAdapter(sqlx, out DataSet DsHistory, false, "1");



                 // Inventory Check
                 var _sql = @"select Inventorycheck,EntityId from mst.MaterialMovementMaster where Id ='" + LocId + "'";
                 var Location = _sqlRepository.GetDataTable(_sql);
                 Inventchk = bplib.clsWebLib.GetBoolData(Location.Rows[0]["Inventorycheck"].ToString());
                 string entityId = Location.Rows[0]["EntityId"].ToString();

                 string esql = "select PlantId from ORG.Entity Where Id='" + entityId + "'";
                 DataTable dtPlant = _sqlRepository.GetDataTable(esql);
                 string PlantId = dtPlant.Rows[0]["PlantId"].ToString();

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
                     if (ToLocList.Rows.Count > 0)
                     {
                         filter = 1;
                     }
                 }

                 int Index = 0;
                 decimal netWeight = 0;
                 string POId = string.Empty;
                 string lotNo = string.Empty;
                 string _Id = ""; string _Idx = "";
                 foreach (ItemScanChildData item in DataToSave)
                 {
                     //netWeight += Convert.ToDecimal(item.NetWeight);
                     //POId = item.POId;
                     //lotNo = item.LotNo;

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
                             clsGenID genid = new clsGenID();
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
                         dr["IsReturn"] = 0;
                         dr["PackingId"] = DBNull.Value;
                         dr["ReturnNetWeight"] = item.ReturnNetWeight;
                         dsMaster.Tables[0].Rows.Add(dr);

                     }

                     else if (Inventchk == true && counter != 1 && filter == 1)
                     {
                         inventory = inventory + item.RefNo + " ";

                     }

                     else if (Inventchk == true && counter != 1 && filter != 1)
                     {
                         inventory = inventory + item.RefNo + " ";
                     }
                     else
                     {

                         DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                         if (item.LocMasterId.ToUpper() != dr["LocMasterId"].ToString().ToUpper())
                         {

                             if (Inventchk == true && counter == 1 && filter == 1)
                             {

                                 DataRow drx = DsHistory.Tables[0].NewRow();
                                 if (_Idx == "")
                                 {
                                     clsGenID genid = new clsGenID();
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
                                 string BookingDate = bplib.clsWebLib.RetValidLen(dr["BookedDate"]).ToString();
                                 if (BookingDate != "")
                                 {
                                     drx["BookedDate"] = BookingDate;
                                 }
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



                 if (inventory != "")
                 {
                     return "Inventory not Found of these cartons:- " + inventory;
                 }

                 return "true";

             }
             catch (Exception ex)
             {
                 return ex.ToString();
             }
         }*/

        public string Create(string MId, IEnumerable<ItemScanChildData> DataToSave)
        {
            try
            {
                if (DataToSave == null || !DataToSave.Any())
                    return "";

                var items = DataToSave.ToList();

                ConnectionManager.DAL.ConManager con =
                    new ConnectionManager.DAL.ConManager("1");

                string TableName = "dbo.ItemScanChild";

                DataSet dsMaster;

                // Load existing data structure
                var sql = @"SELECT * FROM dbo.ItemScanChild WHERE 1 = 2";

                con.OpenDataSetThroughAdapter(
                    sql,
                    out dsMaster,
                    false,
                    "1"
                );

                string User = items[0].AddedBy;

                int Index = 0;
                string _Id = "";

                foreach (ItemScanChildData item in items)
                {
                    Index++;

                    // Check RefNo already exists
                    string checkSql = @"
                SELECT *
                FROM dbo.ItemScanChild
                WHERE RefNo = '" + item.RefNo.Replace("'", "''") + @"'
            ";

                    DataTable existingData = _sqlRepository.GetDataTable(checkSql);

                    // =====================================================
                    // REFNO EXISTS -> UPDATE
                    // =====================================================
                    if (existingData.Rows.Count > 0)
                    {
                        string updateSql = @"
                    UPDATE dbo.ItemScanChild
                    SET 
                        LocMasterId = '" + item.LocMasterId.Replace("'", "''") + @"',
                        UpdatedBy = '" + User.Replace("'", "''") + @"',
                        UpdatedDate = GETDATE()
                    WHERE RefNo = '" + item.RefNo.Replace("'", "''") + @"'
                ";

                        _sqlRepository.ExecuteNonQuery(updateSql);
                    }
                    // =====================================================
                    // REFNO DOES NOT EXIST -> INSERT
                    // =====================================================
                    else
                    {
                        if (_Id == "")
                        {
                            clsGenID genid = new clsGenID();
                            genid.GenID(TableName, out _Id);
                        }

                        DataRow dr = dsMaster.Tables[0].NewRow();

                        dr["Id"] = "SC" + _Id + "-" + Index;
                        dr["MasterId"] = MId;
                        dr["NetWeight"] = DBNull.Value;
                        dr["GWeight"] = DBNull.Value;
                        dr["PackedBy"] = DBNull.Value;
                        dr["Shade"] = DBNull.Value;
                        dr["AddedBy"] = User;
                        dr["AddedDate"] = DateTime.Now;
                        dr["ProductCode"] = DBNull.Value;
                        dr["POId"] = DBNull.Value;
                        dr["LotNo"] = DBNull.Value;
                        dr["RefNo"] = item.RefNo;
                        dr["Cones"] = DBNull.Value;
                        dr["LocMasterId"] = item.LocMasterId;
                        dr["Booked"] = 0;
                        dr["IsDespatch"] = 0;
                        dr["IsReturn"] = 0;
                        dr["PackingId"] = DBNull.Value;
                        dr["ReturnNetWeight"] = DBNull.Value;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                }

                // Save only newly inserted records
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster);
                }

                return "true";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public string CreateSummaryData(string MId, IEnumerable<ItemScanChildData> DataToSave)
        {
            try
            {
                string processId = "";
                string inventory = "";
                string Booked, IsDespatch, ToLocation, FLoc = ""; bool Inventchk = false;
                decimal counter = 0, filter = 0;
                DataSet dsMaster;
                string TableName = "dbo.ItemScanChild";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";

                var items = DataToSave.ToList();


                string PackedBy = "''";
                string RefNo = "''";
                string LocId = items[0].LocMasterId;
                string User = items[0].AddedBy;  /// Can delete later on
                foreach (ItemScanChildData item in DataToSave)
                {
                    PackedBy += ",'" + item.PackedBy + "'";
                    RefNo += ",'" + item.RefNo + "'";
                }



                var sqly = @"select SystemId as EmpId,EmployeeCode from dbo.EmployeeInformation where EmployeeCode IN(" + PackedBy + ")";
                var EmpId = _sqlRepository.GetDataTable(sqly);

                //getscandata
                var sqlscan = @"Select WorkDate,ShiftId,Grade,PurposeId,LocMasterId from dbo.ItemScan Where Id='" + MId + "'";
                DataTable dtScan = _sqlRepository.GetDataTable(sqlscan);

                DateTime WorkDate = Convert.ToDateTime(dtScan.Rows[0]["WorkDate"].ToString());
                string ShiftId = dtScan.Rows[0]["ShiftId"].ToString();
                string Grade = dtScan.Rows[0]["Grade"].ToString();
                string PurposeId = dtScan.Rows[0]["PurposeId"].ToString();

                //getProcess&Entity
                var sqlProcess = @"SELECT ProcessId FROM HKP.MaterialMovementPurpose where Id ='" + PurposeId + "'";
                DataTable dtProcess = _sqlRepository.GetDataTable(sqlProcess);
                processId = dtProcess.Rows[0]["ProcessId"].ToString();


                // Check repeat Rows 
                var sql = @"select * from dbo.ItemScanChild where RefNo IN(" + RefNo + @")";
                con.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                // For History
                var sqlx = @"select * from dbo.ItemScanChildHistory where 1=2";
                con.OpenDataSetThroughAdapter(sqlx, out DataSet DsHistory, false, "1");

                // For ProductionSummary
                string sqlPS = @"SELECT * FROM TRN.ProductionSummary where 1=2";
                con.OpenDataSetThroughAdapter(sqlPS, out DataSet dsProductionSummary, false, "1");

                // Inventory Check
                var _sql = @"select Inventorycheck,EntityId from mst.MaterialMovementMaster where Id ='" + LocId + "'";
                var Location = _sqlRepository.GetDataTable(_sql);
                Inventchk = bplib.clsWebLib.GetBoolData(Location.Rows[0]["Inventorycheck"].ToString());
                string entityId = Location.Rows[0]["EntityId"].ToString();

                string esql = "select PlantId from ORG.Entity Where Id='" + entityId + "'";
                DataTable dtPlant = _sqlRepository.GetDataTable(esql);
                string PlantId = dtPlant.Rows[0]["PlantId"].ToString();

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
                    if (ToLocList.Rows.Count > 0)
                    {
                        filter = 1;
                    }
                }

                int Index = 0;
                decimal netWeight = 0;
                string POId = string.Empty;
                string lotNo = string.Empty;
                string _Id = ""; string _Idx = "";
                foreach (ItemScanChildData item in DataToSave)
                {
                    netWeight += Convert.ToDecimal(item.NetWeight);
                    POId = item.POId;
                    lotNo = item.LotNo;



                }




                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, DsHistory, dsProductionSummary);

                #region ProductionSummary


                if (!string.IsNullOrEmpty(processId))
                {
                    //netWeight = 0;
                    //for (int j = 0; j < dsMaster.Tables[0].Rows.Count; j++)
                    //{
                    //    netWeight += Convert.ToDecimal(dsMaster.Tables[0].Rows[j]["NetWeight"]);
                    //    lotNo = dsMaster.Tables[0].Rows[j]["LotNo"].ToString();
                    //    POId = dsMaster.Tables[0].Rows[j]["POId"].ToString();
                    //}


                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ProductionSummary", out string sID);
                    DataRow drProductionSummary = dsProductionSummary.Tables[0].NewRow();
                    drProductionSummary["Id"] = "PS" + sID;
                    drProductionSummary["PlantId"] = PlantId;
                    drProductionSummary["EntityId"] = entityId;
                    drProductionSummary["ProcessId"] = processId;
                    drProductionSummary["ProductionDate"] = WorkDate;
                    drProductionSummary["Quantity"] = netWeight;
                    drProductionSummary["ProductionOrderId"] = POId;
                    drProductionSummary["ProductionShiftId"] = ShiftId;
                    drProductionSummary["ProductionGrade"] = Grade;
                    drProductionSummary["LotNumber"] = lotNo;

                    drProductionSummary["AddedBy"] = User;
                    drProductionSummary["AddedDate"] = DateTime.Now;
                    drProductionSummary["AddedFromIP"] = "1";

                    dsProductionSummary.Tables[0].Rows.Add(drProductionSummary);
                }
                #endregion

                if (inventory != "")
                {
                    return "Inventory not Found of these cartons:- " + inventory;
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

        public IEnumerable<object> GetPackingId(string Cust, string User)
        {
            try
            {
                var _sql = @"select distinct p.PackingId as Value from trn.Packing p
		     left join EmployeeInformation e on e.SystemId=p.ByWhom
		     left join [SEC].[User] u on u.EmployeeId=e.SystemId
		     where p.CustomerId = '" + Cust + "' and u.UserId='" + User + "' " +
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
                where pl.PackingId='" + PId + "' and InactiveDate>=CAST(GETDATE() AS Date) ";
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
                where p.PackingLineItemId='" + PL + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetPO(string PL, string Prod)
        {
            try
            {
                var sql = @"select distinct p.PONo as Value from trn.POLotReference P
                left join trn.PackingLineItem pl ON pl.PackingLineItemId=p.PackingLineItemId
                where p.PackingLineItemId='" + PL + "' and p.ProductCode='" + Prod + "'";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetLotId(string PL, string Prod, string PO)
        {
            try
            {
                var sql = @"select distinct p.LotNo ,p.Id as SystemId,p.PlanQty from trn.POLotReference P
                left join trn.PackingLineItem pl ON pl.PackingLineItemId=p.PackingLineItemId
                where p.PackingLineItemId='" + PL + "' and p.ProductCode='" + Prod + "' and p.PONo='" + PO + "'and p.Status='Active'";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetBookedQty(string Lot, string Prod, string PO, string Pqty, string PoLotRefernceId)
        {
            try
            {
                decimal PlanQty = Convert.ToDecimal(Pqty);
                var sql = @"select '" + PlanQty + "'-req.BookedQty as AvailQty from(select isnull(Floor(Sum(Netweight)),0) as BookedQty " +
                    "from dbo.ItemScanChild where ProductCode='" + Prod + "'and POId='" + PO + "'and PackingId='" + PoLotRefernceId + "' " +
                    "and LotNo='" + Lot + "' and Booked ='1' ) as req";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetBookedQtyMsg(string PackingId)
        {
            try
            {
                var sql = @"select Count(refno)CartonQty,
                isnull(Floor(Sum(netweight)),0)BookedQty from itemscanchild 
                where PackingId='" + PackingId + "'and Booked=1";
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
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                string ErrorList = "";

                if (DataToSave.Count() == 0)
                {
                    return "No Data Found";
                }

                string RefNo = "''";
                string PckId = "";
                foreach (ItemScanChildData item in DataToSave)
                {
                    RefNo += ",'" + item.RefNo + "'";
                    PckId = item.PackingId;
                }

                var items = DataToSave.ToList();

                var sqlx = @"select * from dbo.ItemScanChild where Booked=0 AND IsDespatch=0 and RefNo IN(" + RefNo + @")";
                con.OpenDataSetThroughAdapter(sqlx, out dsMaster, false, "1");

                double BkQty = 0.0;

                foreach (ItemScanChildData item in DataToSave)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = @"RefNo='" + item.RefNo + "' ";
                    if (dsMaster.Tables[0].DefaultView.Count > 0)
                    {

                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["BookedDate"] = DateTime.Now;
                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["PackingId"] = item.PackingId;
                        dr["Booked"] = true;
                        dr.EndEdit();
                        PckId = item.PackingId;
                        BkQty += clsStaticInfo.dbl(dr["NetWeight"].ToString());

                    }
                    else
                    {
                        ErrorList += item.RefNo + "...";
                    }


                }

                ConnectionManager.DAL.ConManager conn = new ConnectionManager.DAL.ConManager("1");
                DataSet dsPo;

                var sql = @"select * from trn.POLotReference where Id ='" + PckId + "'";
                conn.OpenDataSetThroughAdapter(sql, out dsPo, false, "1");
                if (dsPo.Tables[0].Rows.Count > 0)
                {
                    double poBkQty = clsStaticInfo.dbl(dsPo.Tables[0].Rows[0]["BookQty"].ToString());
                    BkQty += poBkQty;
                    dsPo.Tables[0].Rows[0].BeginEdit();
                    dsPo.Tables[0].Rows[0]["BookQty"] = BkQty;
                    dsPo.Tables[0].Rows[0].EndEdit();
                }
                SaveDataSets(dsMaster);
                SaveDataSets(dsPo);

                if (ErrorList != "")
                {
                    return "Their are issues with these Cartons:- " + ErrorList;
                }

                return "true";


            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        #region Aman SalesReturn
        public string CreateSalesReturn(IEnumerable<ItemScanChildSalesReturn> DataToSave)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                string ErrorList = "";

                if (DataToSave.Count() == 0)
                {
                    return "No Data Found";
                }

                string RefNo = "''";
                string PckId = "";
                foreach (ItemScanChildSalesReturn item in DataToSave)
                {
                    RefNo += ",'" + item.RefNo + "'";
                    PckId = item.PackingId;
                }

                var items = DataToSave.ToList();

                var sqlx = @"select * from dbo.ItemScanChild where SalesReturnId is null and Booked = 1 and IsDespatch = 1  and RefNo IN(" + RefNo + @")";
                con.OpenDataSetThroughAdapter(sqlx, out dsMaster, false, "1");

                double BkQty = 0.0;

                foreach (ItemScanChildSalesReturn item in DataToSave)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = @"RefNo='" + item.RefNo + "' ";
                    if (dsMaster.Tables[0].DefaultView.Count > 0)
                    {

                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["ReturnNetWeight"] = item.ReturnNetWeight;
                       // dr["Booked"] = false;   Booked is now 0 is stop 
                        dr.EndEdit();
                        PckId = item.PackingId;
                        BkQty += clsStaticInfo.dbl(dr["NetWeight"].ToString());

                    }
                    else
                    {
                        ErrorList += item.RefNo + " , ";
                    }


                }

                ConnectionManager.DAL.ConManager conn = new ConnectionManager.DAL.ConManager("1");
                DataSet dsPo;

                var sql = @"select * from trn.POLotReference where Id ='" + PckId + "'";
                conn.OpenDataSetThroughAdapter(sql, out dsPo, false, "1");
                if (dsPo.Tables[0].Rows.Count > 0)
                {
                    double poBkQty = clsStaticInfo.dbl(dsPo.Tables[0].Rows[0]["BookQty"].ToString());
                    BkQty += poBkQty;
                    dsPo.Tables[0].Rows[0].BeginEdit();
                    dsPo.Tables[0].Rows[0]["BookQty"] = BkQty;
                    dsPo.Tables[0].Rows[0].EndEdit();
                }
                SaveDataSets(dsMaster);
                SaveDataSets(dsPo);

                if (ErrorList != "")
                {
                    return "These Cartons are not Sold:- " + ErrorList;
                }

                return "true";


            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        #endregion Aman SalesReturn

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
        public string ReturnNetWeight { get; set; }
        public string IsReturn { get; set; }

    }

    public class ItemScanChildSalesReturn
    {
        public string AddedBy { get; set; }
        public string Id { get; set; }
        public DateTime AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string ProductCode { get; set; }
        public string POId { get; set; }
        public string NetWeight { get; set; }
        public string ReturnNetWeight { get; set; }
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
        public string SalesReturnId { get; set; }
    }

}
