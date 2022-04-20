using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Library.Data.Sql;
using OTSBD;
using Library.Service.EmployeeServices;
using bplib;
using Newtonsoft.Json;
using Library.Core;

namespace Library.HumanResource.NewAttendanceProcess
{
    public class AttdnRawDataUploadService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public AttdnRawDataUploadService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }
      
        public string SaveDataWithEmpId(List<AttdnRawData> DataToSave)
        {
            try
            {
                if (DataToSave.Count() == 0)
                    return "Either Data not in Correct Format or Missing....";

                List<AttdnRawData> items = DataToSave.ToList();

                DataSet dsRef,dsPlant,Device;
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                string strSql = @"select * from dbo.AttdnRawData where 1=2";
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
                
                string EmpId = "''";
                foreach (AttdnRawData item in DataToSave)
                {
                    EmpId += ",'" + item.LogDownLoadNum + "'";                    
                }

                string Sql = @"select * from EmployeeInformation where SystemId IN("+EmpId+")";
                objCon.OpenDataSetThroughAdapter(Sql, out dsPlant, false, "1");
               
                var sqlx = @"select top 1 * from mst.AccessControllerList";
                objCon.OpenDataSetThroughAdapter(sqlx, out Device, false, "1");
                var DeviceSystemId = clsWebLib.RetValidLen(Device.Tables[0].Rows[0][@"Id"]).ToString();

                foreach (AttdnRawData item in DataToSave)
                {

                    if (clsWebLib.RetValidLen(item.LogDownLoadNum).ToString() != "" &&
                        clsWebLib.RetValidLen(item.PTime).ToString() != "")
                    {
                        dsPlant.Tables[0].DefaultView.RowFilter = @"SystemId='" + item.LogDownLoadNum + "'";
                        if (dsPlant.Tables[0].DefaultView.Count > 0)
                        {
                            string PlantId = clsWebLib.RetValidLen(dsPlant.Tables[0].DefaultView[0][@"PlantId"]).ToString();
                            string GpId = clsWebLib.RetValidLen(dsPlant.Tables[0].DefaultView[0][@"GroupID"]).ToString();


                            DataRow drx = dsRef.Tables[0].NewRow();

                            clsGenID genid = new clsGenID();
                            genid.GenID("AttdnRawData", out string _Idx);

                            drx["Id"] = "ARD" + _Idx;
                            drx["DeviceID"] = DBNull.Value;
                            drx["DevSystemID"] = DeviceSystemId;
                            drx["LogDownLoadNum"] = item.LogDownLoadNum;
                            drx["PlantID"] = PlantId;
                            drx["GroupID"] = GpId;
                            drx["PDate"] =Convert.ToDateTime(item.PTime).ToString("dd-MMM-yyyy");
                            drx["PTime"] =Convert.ToDateTime(item.PTime);
                            drx["PType"] = clsWebLib.RetValidLen(item.PType);
                            drx["AddedBy"] = "API";
                            drx["DateAdded"] = DateTime.Now;
                            drx["FlagSetByProcess"] = DBNull.Value;
                            drx["ProcessedFlag"] = false;
                            dsRef.Tables[0].Rows.Add(drx);

                        }
                    }
                }
                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsRef);
                var Counter = dsRef.Tables[0].Rows.Count;
                if (Counter <= 1)
                {
                    return Counter.ToString() + " Row Uploaded... ";
                }
                else
                {
                    return Counter.ToString() + " Rows Uploaded... ";
                }
                
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public string SaveSingleDataWithEmpId(AttdnRawData DataToSave)
        {
            if(DataToSave==null)
            {
                return "Data not Found";
            }

            List<AttdnRawData> data = new List<AttdnRawData>();
            data.Add(DataToSave);
            return SaveDataWithEmpId(data);
                 
        }
        public string SaveDataWithCardNumber(List<AttdnRawData> DataToSave)
        {
            try
            {
                if (DataToSave.Count() == 0)
                    return "Either Data not in Correct Format or Missing....";

                List<AttdnRawData> items = DataToSave.ToList();

                DataSet dsRef, dsPlant, Device;
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                string strSql = @"select * from dbo.AttdnRawData where 1=2";
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");

                string CardNo = "''";
                foreach (AttdnRawData item in DataToSave)
                {
                    CardNo += ",'" + item.LogDownLoadNum + "'";
                }

                string Sql = @"select * from EmployeeInformation where CardNumber IN(" + CardNo + ")";
                objCon.OpenDataSetThroughAdapter(Sql, out dsPlant, false, "1");

                var sqlx = @"select top 1 * from mst.AccessControllerList";
                objCon.OpenDataSetThroughAdapter(sqlx, out Device, false, "1");
                var DeviceSystemId = clsWebLib.RetValidLen(Device.Tables[0].Rows[0][@"Id"]).ToString();

                foreach (AttdnRawData item in DataToSave)
                {

                    if (clsWebLib.RetValidLen(item.LogDownLoadNum).ToString() != "" &&
                        clsWebLib.RetValidLen(item.PTime).ToString() != "")
                    {
                        dsPlant.Tables[0].DefaultView.RowFilter = @"CardNumber='" + item.LogDownLoadNum + "'";
                        if (dsPlant.Tables[0].DefaultView.Count > 0)
                        {
                            string PlantId = clsWebLib.RetValidLen(dsPlant.Tables[0].DefaultView[0][@"PlantId"]).ToString();
                            string GpId = clsWebLib.RetValidLen(dsPlant.Tables[0].DefaultView[0][@"GroupID"]).ToString();
                            string EmpId= clsWebLib.RetValidLen(dsPlant.Tables[0].DefaultView[0][@"SystemId"]).ToString();

                            DataRow drx = dsRef.Tables[0].NewRow();

                            clsGenID genid = new clsGenID();
                            genid.GenID("AttdnRawData", out string _Idx);

                            drx["Id"] = "ARD" + _Idx;
                            drx["DeviceID"] = DBNull.Value;
                            drx["DevSystemID"] = DeviceSystemId;
                            drx["LogDownLoadNum"] = EmpId;
                            drx["PlantID"] = PlantId;
                            drx["GroupID"] = GpId;
                            drx["PDate"] = Convert.ToDateTime(item.PTime).ToString("dd-MMM-yyyy");
                            drx["PTime"] = Convert.ToDateTime(item.PTime);
                            drx["PType"] = clsWebLib.RetValidLen(item.PType);
                            drx["AddedBy"] = "API";
                            drx["DateAdded"] = DateTime.Now;
                            drx["FlagSetByProcess"] = DBNull.Value;
                            drx["ProcessedFlag"] = false;
                            dsRef.Tables[0].Rows.Add(drx);

                        }
                    }
                }
                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsRef);
                var Counter = dsRef.Tables[0].Rows.Count;
                if (Counter <= 1)
                {
                    return Counter.ToString() + " Row Uploaded... ";
                }
                else
                {
                    return Counter.ToString() + " Rows Uploaded... ";
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

    }

    public class AttdnRawData 
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string DeviceId { get; set; }
        public string DevSystemId { get; set; }
        public string LogDownLoadNum { get; set; }
        public string PDate { get; set; }
        public string PTime { get; set; }
        public string PType { get; set; }
        public string ProcessedFlag { get; set; }
        public string FlagSetByProcess { get; set; }
        public string GroupID { get; set; }
        public string PlantID { get; set; }

        #endregion Navigation Properties
    }
    
    public class ServiceScanModel
    {
        #region Scalar Properties
        public string Service { get; set; }
        public string Category { get; set; }
        public string Id { get; set; }
        public string EmployeeId { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string PTime { get; set; }
        public string EmployeeServiceCategoryId { get; set; }
        public string Quantity { get; set; }
        public string Amount { get; set; }
        public string BillOtherReferenceNo { get; set; }
        public string Particulars { get; set; }
        #endregion Scalar Properties

        #region Audit Properties

        [NeverUpdate]
        public string AddedBy { get; set; }
        [NeverUpdate]
        public DateTime AddedDate { get; set; }
        [NeverUpdate]
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties
    }

    public class EmpServiceDataScanService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public EmpServiceDataScanService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        public string SaveData(List<ServiceScanModel> DataToSave)
        {
            try
            {
                if (DataToSave.Count() == 0)
                    return "Either Data not in Correct Format or Missing....";

                List<ServiceScanModel> items = DataToSave.ToList();

                DataSet dsRef, dsEmpShift, dsCategory;
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                string strSql = @"select * from dbo.empservicedata where 1=2";
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");

                string EmpId = "''";
                foreach (ServiceScanModel item in DataToSave)
                {
                    EmpId += ",'" + item.EmployeeId + "'";
                }

                var sql = @"select e.SystemId, e.EmployeeCode,e.EmployeeName,
                mb.Code as BudgetCode,mb.Id as BudgetId,
                mb.ShiftDefinationId as BudgetedShift
                from EmployeeInformation e left join mst.ManpowerBudget mb on mb.Id=e.BudgetCode
                where e.SystemId in ("+EmpId+@")
                and mb.ShiftDefinationId is not null";
                objCon.OpenDataSetThroughAdapter(sql, out dsEmpShift, false, "1");

                var sqlx = @"select et.Id as ServiceId,ec.Id as CategoryId from
                EmpServiceCategory ec
                left join EmpServiceType et on et.Id=ec.EmpServiceTypeId
                where et.Service='"+DataToSave[0].Service+"' and ec.Category='"+ DataToSave[0].Category + "'";
                objCon.OpenDataSetThroughAdapter(sqlx, out dsCategory, false, "1");
               
                string CategoryId = "";
                if (dsCategory.Tables[0].Rows.Count > 0)
                {
                    CategoryId = clsWebLib.RetValidLen(dsCategory.Tables[0].Rows[0][@"CategoryId"]).ToString();
                }
                

                if (CategoryId =="")
                {
                    return "Please Enter Valid Service Type and Category....";
                }
               
                foreach (ServiceScanModel item in DataToSave)
                {

                    if (clsWebLib.RetValidLen(item.EmployeeId).ToString() != "" &&
                        clsWebLib.RetValidLen(item.Service).ToString() != "" && clsWebLib.RetValidLen(item.Category).ToString() != "")
                    {
                        dsEmpShift.Tables[0].DefaultView.RowFilter = @"SystemId='" + item.EmployeeId + "'";
                        if (dsEmpShift.Tables[0].DefaultView.Count > 0)
                        {
                            string ShiftId = clsWebLib.RetValidLen(dsEmpShift.Tables[0].DefaultView[0][@"BudgetedShift"]).ToString();
                        
                            DataRow dr = dsRef.Tables[0].NewRow();

                            clsGenID genid = new clsGenID();
                            genid.GenID("dbo.EmpServiceData", out string _Idx);

                            dr["Id"] = "ED" + _Idx;
                            dr["EmployeeId"] = item.EmployeeId;
                            dr["Date"] = Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy"));
                            dr["Time"] = Convert.ToDateTime(item.PTime.ToString());
                            dr["ShiftId"] = ShiftId;
                            dr["EmployeeServiceCategoryId"] = CategoryId;
                            dr["Chargeable"] = 1;
                            dr["IsProcessed"] = false;
                            dr["From"] = 0;
                            dr["To"] = 0;
                            dr["Quantity"] = 1;
                            dr["Particulars"] = DBNull.Value;
                            dr["BillOtherReferenceNo"] = DBNull.Value;
                            dr["Amount"] = 0;

                            dr["AddedBy"] = "API";
                            dr["AddedDate"] = DateTime.Now.ToString();
                            dr["AddedFromIP"] = "1";

                            dsRef.Tables[0].Rows.Add(dr);

                        }
                    }
                }
                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsRef);
                var Counter = dsRef.Tables[0].Rows.Count;
                if (Counter <= 1)
                {
                    return Counter.ToString() + " Row Uploaded... ";
                }
                else
                {
                    return Counter.ToString() + " Rows Uploaded... ";
                }

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public string ShopData(List<ServiceScanModel> DataToSave)
        {
            try
            {
                if (DataToSave.Count() == 0)
                    return "Either Data not in Correct Format or Missing....";

                List<ServiceScanModel> items = DataToSave.ToList();

                DataSet dsRef, dsEmpShift, dsCategory;
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                string strSql = @"select * from dbo.empservicedata where 1=2";
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");

                string EmpId = "''";
                foreach (ServiceScanModel item in DataToSave)
                {
                    EmpId += ",'" + item.EmployeeId + "'";
                }

                var sql = @"select e.SystemId, e.EmployeeCode,e.EmployeeName,
                mb.Code as BudgetCode,mb.Id as BudgetId,
                mb.ShiftDefinationId as BudgetedShift
                from EmployeeInformation e left join mst.ManpowerBudget mb on mb.Id=e.BudgetCode
                where e.SystemId in (" + EmpId + @")
                and mb.ShiftDefinationId is not null";
                objCon.OpenDataSetThroughAdapter(sql, out dsEmpShift, false, "1");

                var sqlx = @"select et.Id as ServiceId,ec.Id as CategoryId from
                EmpServiceCategory ec
                left join EmpServiceType et on et.Id=ec.EmpServiceTypeId
                where et.Service='Shop'";
                objCon.OpenDataSetThroughAdapter(sqlx, out dsCategory, false, "1");
               
                string CategoryId = "";
                if (dsCategory.Tables[0].Rows.Count > 0)
                {
                    CategoryId = clsWebLib.RetValidLen(dsCategory.Tables[0].Rows[0][@"CategoryId"]).ToString();
                }
                if (CategoryId == "")
                {
                    return "Please Configure Service First....";
                }

                foreach (ServiceScanModel item in DataToSave)
                {

                    if (clsWebLib.RetValidLen(item.EmployeeId).ToString() != "" && clsWebLib.RetValidLen(item.BillOtherReferenceNo).ToString() != ""
                       && clsWebLib.RetValidLen(item.Amount).ToString() != "" && clsWebLib.RetValidLen(CategoryId).ToString() != "")
                    {
                        dsEmpShift.Tables[0].DefaultView.RowFilter = @"SystemId='" + item.EmployeeId + "'";
                        if (dsEmpShift.Tables[0].DefaultView.Count > 0)
                        {
                            string ShiftId = clsWebLib.RetValidLen(dsEmpShift.Tables[0].DefaultView[0][@"BudgetedShift"]).ToString();

                            DataRow dr = dsRef.Tables[0].NewRow();

                            clsGenID genid = new clsGenID();
                            genid.GenID("dbo.EmpServiceData", out string _Idx);

                            dr["Id"] = "ED" + _Idx;
                            dr["EmployeeId"] = item.EmployeeId;
                            dr["Date"] = Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy"));
                            dr["Time"] = DateTime.Now.ToString();
                            dr["ShiftId"] = ShiftId;
                            dr["EmployeeServiceCategoryId"] = CategoryId;
                            dr["Chargeable"] = 1;
                            dr["IsProcessed"] = false;
                            dr["From"] = 0;
                            dr["To"] = 0;
                            dr["Quantity"] = 0;
                            dr["Particulars"] = item.Particulars;
                            dr["BillOtherReferenceNo"] = item.BillOtherReferenceNo;
                            dr["Amount"] = item.Amount;

                            dr["AddedBy"] = "API";
                            dr["AddedDate"] = DateTime.Now.ToString();
                            dr["AddedFromIP"] = "1";

                            dsRef.Tables[0].Rows.Add(dr);

                        }
                    }
                }
                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsRef);
                var Counter = dsRef.Tables[0].Rows.Count;
                if (Counter <= 1)
                {
                    return Counter.ToString() + " Row Uploaded... ";
                }
                else
                {
                    return Counter.ToString() + " Rows Uploaded... ";
                }

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

    }

}

