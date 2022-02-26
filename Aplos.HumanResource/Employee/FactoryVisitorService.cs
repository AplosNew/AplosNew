using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Library.Data.Sql;
using OTSBD;
using bplib;

namespace Library.HumanResource.Employee
{
    public class FactoryVisitorService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public FactoryVisitorService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }
        
        #region API's 
        public string SaveEmployeeVisit(IEnumerable<VisitorModel> DataToSave)
        {
            try
            {
                DataSet dsMaster,dsCard;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<VisitorModel> items = DataToSave.ToList();

                string checkCard = clsWebLib.RetValidLen(items[0].CardNo).ToString();

                if (checkCard != "")
                {
                    con.OpenDataSetThroughAdapter("select distinct CardNo from visitorservicedata where InDone = '1'  and OutDone = '0'", out dsCard, false, "1");
                    if (dsCard.Tables[0].Rows.Count > 0)
                    {
                        for (int j = 0; j < dsCard.Tables[0].Rows.Count; j++)
                        {
                            if ((dsCard.Tables[0].Rows[j][@"CardNo"]).ToString() == checkCard)
                            {
                                return " Please Enter Valid CardNo.Already in Use ...";
                            }
                        }
                    }
                }

                con.OpenDataSetThroughAdapter("select * from dbo.VisitorServiceData where 1=2", out dsMaster, false, "1");

                foreach (VisitorModel item in DataToSave)
                {

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();
                        clsGenID genid = new clsGenID();
                        genid.GenID("VisitorService", out string _Id);
                     
                        dr["Id"] = "VSD"+ _Id;
                        dr["VisitorCategory"] = item.VisitorCategory;
                        dr["VisitorType"] = item.VisitorType;
                        dr["VisitorName"] = item.VisitorName;
                        dr["ToMeet"] = item.ToMeet;
                        dr["Purpose"] = item.Purpose;
                        dr["Remarks"] = item.Remarks;
                        dr["OutDone"] = false;
                        dr["NoOfPerson"] = item.NoOfPerson;
                        dr["MobileNo"] = item.MobileNo;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = DateTime.Now.ToString();
                        dr["AddedFromIP"] = item.AddedFromIP;
                        if(item.param=="In")
                        {
                            dr["CardNo"] = item.CardNo;
                            dr["VehicleNo"] = item.VehicleNo;
                            dr["VisitorLocation"] = item.VisitorLocation;
                            dr["InDate"] = DateTime.Now.ToString("dd-MMM-yyyy");
                            dr["InTime"] = DateTime.Now;
                            dr["InDone"] = true;
                        }
                        else
                        {
                            dr["ExpectedDate"] = item.ExpectedDate;
                            dr["ExpectedTime"] = item.ExpectedTime;
                            dr["InDone"] = false;                           
                        }
                        dsMaster.Tables[0].Rows.Add(dr);
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
        public IEnumerable<object> GetTodayMineList(string EmpId)
        {
            try
            {
                var sql = @"select Id,CardNo,ExpectedDate,format(ExpectedTime,'hh:mm tt')ExpectedTime,
                VisitorCategory,
                VisitorName,VisitorType,Purpose
                from VisitorServiceData where ToMeet='"+EmpId+@"'
                and ExpectedDate=CONVERT(date,GETDATE()) and InDone='0' and OutDone='0'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetExpectedInList()
        {
            try
            {
                var sql = @"select d.Id,CardNo,ExpectedDate,ExpectedTime,VisitorCategory,
                VisitorName,VisitorType,Purpose,e.EmployeeName as ToMeet,p.UserName as Department,
				l.UserName as Designation,d.VisitorLocation
                from VisitorServiceData d
				left join EmployeeInformation e on e.SystemId=d.ToMeet
				left join org.Department p on p.Id=e.DepartmentId
				left join hkp.LegalDesignation l on l.Id=e.LegalDesignationId                     
				where ExpectedDate=CONVERT(date,GETDATE()) and InDone='0' 
                and OutDone='0'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetExpectedOutList()
        {
            try
            {
                var sqlx = @"select d.Id,CardNo,ExpectedDate,ExpectedTime,VisitorCategory,d.VisitorLocation,
                VisitorName,VisitorType,Purpose,e.EmployeeName as ToMeet,p.UserName as Department,
				l.UserName as Designation,format(d.InDate,'dd-MMM-yyyy')InDate,format(d.InTime,'hh:mm tt')InTime
                from VisitorServiceData d
				left join EmployeeInformation e on e.SystemId=d.ToMeet
				left join org.Department p on p.Id=e.DepartmentId
				left join hkp.LegalDesignation l on l.Id=e.LegalDesignationId                     
				where InDone='1' and OutDone='0'";
                return _sqlRepository.GetDataCollection(sqlx, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string SaveInOutTime(IEnumerable<VisitorModel> DataToSave)
        {
            try
            {
                DataSet dsMaster, dsCard;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                if (DataToSave.Count() == 0)
                {
                    return "No Data Found";
                }
                var items = DataToSave.ToList();

                string checkCard = clsWebLib.RetValidLen(items[0].CardNo).ToString();

                if (checkCard != "")
                {
                    con.OpenDataSetThroughAdapter("select distinct CardNo from visitorservicedata where InDone = '1'  and OutDone = '0'", out dsCard, false, "1");
                    if (dsCard.Tables[0].Rows.Count > 0)
                    {
                        for (int j = 0; j < dsCard.Tables[0].Rows.Count; j++)
                        {
                            if ((dsCard.Tables[0].Rows[j][@"CardNo"]).ToString() == checkCard)
                            {
                                return " Please Enter Valid CardNo.Already in Use ...";
                            }
                        }
                    }
                }
                con.OpenDataSetThroughAdapter("select * from dbo.VisitorServiceData where Id='"+items[0].Id+"'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                   
                    foreach (VisitorModel item in DataToSave)
                    {
                   
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                       
                        if (item.param == "In")
                        {
                            dr["CardNo"] = item.CardNo;
                            dr["Purpose"] = item.Purpose;
                            dr["Remarks"] = item.Remarks;
                            dr["NoOfPerson"] = item.NoOfPerson;
                            dr["MobileNo"] = item.MobileNo;
                            dr["VehicleNo"] = item.VehicleNo;
                            dr["VisitorLocation"] = item.VisitorLocation;
                            dr["OutDone"] = false;
                            dr["InDone"] = true;
                            dr["InDate"] = Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy"));
                            dr["InTime"] = DateTime.Now;
                        }
                        else
                        {
                            dr["OutDate"] = Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy"));
                            dr["OutTime"] = DateTime.Now;
                            dr["OutDone"] = true;
                            dr["InDone"] = true;
                        }
                        dr["UpdatedBy"] = item.AddedBy;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = item.AddedFromIP;
                        dr.EndEdit();
                    }
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

        #endregion

        #region Report Functions
        public IEnumerable<object> GetVisitorList(string InDone,string OutDone,string FromDate,string ToDate )
        {
            try
            {
                #region InDone OutDone Value Region
               
                if (InDone=="True")
                {
                    InDone = "1";
                }
                else
                {
                    InDone = "0";
                }
                if (OutDone == "True")
                {
                    OutDone = "1";
                }
                else
                {
                    OutDone = "0";
                }
                #endregion

                TimeSpan ts = Convert.ToDateTime(ToDate).Subtract(Convert.ToDateTime(FromDate));
                if (ts.Days >= 0)
                {
                    var sql = @"select v.Id,v.CardNo,v.VisitorCategory,
                    v.VisitorType,v.VisitorName,v.NoOfPerson,v.MobileNo,
                    e.EmployeeName as ToMeet,v.Purpose,format(v.InDate,'dd-MMM-yyyy')InDate,
                    format(v.InTime,'hh:mm tt')InTime,
                    format(v.OutDate,'dd-MMM-yyyy')OutDate,format(v.OutTime,'hh:mm tt')OutTime,
                    v.AddedBy,v.VisitorLocation,
                    v.VehicleNo
                    from visitorservicedata v left join EmployeeInformation e on e.SystemId=v.ToMeet
                    WHERE InDone='"+InDone+@"' and OutDone='"+OutDone+@"'
                    and InDate between '"+FromDate+"' and '"+ToDate+"'";
                    
                    return _sqlRepository.GetDataCollection(sql, null);
                
                }
                else
                {
                    throw new Exception("Please choose a valid Date !!");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetReportData(string InDone, string OutDone, string FromDate, string ToDate,string Id)
        {
            try
            {
                #region InDone OutDone Value Region

                if (InDone == "True")
                {
                    InDone = "1";
                }
                else
                {
                    InDone = "0";
                }
                if (OutDone == "True")
                {
                    OutDone = "1";
                }
                else
                {
                    OutDone = "0";
                }
                #endregion
               
                var sql = @"select v.Id,v.CardNo,v.VisitorCategory,
                    v.VisitorType,v.VisitorName,v.NoOfPerson,v.MobileNo,
                    e.EmployeeName as ToMeet,v.Purpose,format(v.InDate,'dd-MMM-yyyy')InDate,
                    format(v.InTime,'hh:mm tt')InTime,
                    format(v.OutDate,'dd-MMM-yyyy')OutDate,format(v.OutTime,'hh:mm tt')OutTime,
                    v.AddedBy,v.VisitorLocation,
                    v.VehicleNo,
                    Duration=isnull(Case when (InDone='1' and OutDone='1') then
					(select datediff(hour,Intime,Outtime))
					end,'0')
                    from visitorservicedata v left join EmployeeInformation e on e.SystemId=v.ToMeet
                    WHERE InDone='" + InDone + @"' and OutDone='" + OutDone + @"'
                    and InDate between '" + FromDate + "' and '" + ToDate + "' and isnull(v.Id ,'') IN(" + Id + @") ";

                    return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
     
        #endregion
    }

    public class VisitorModel
    {       
        #region Fixed Fields
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedFromIP { get; set; }
        #endregion

        #region Other Fields
        public string param { get; set; }
        public string Id { get; set; }
        public string CardNo { get; set; }
        public string Purpose { get; set; }
        public string Remarks { get; set; }
        public DateTime ExpectedDate { get; set; }
        public DateTime ExpectedTime { get; set; }
        public DateTime InDate { get; set; }
        public DateTime InTime { get; set; }
        public DateTime OutDate { get; set; }
        public DateTime OutTime { get; set; }
        public string VisitorType { get; set; }
        public string VehicleNo { get; set; }
        public string VisitorCategory { get; set; }
        public string VisitorName { get; set; }
        public string MobileNo { get; set; }
        public string ToMeet { get; set; }
        public decimal NoOfPerson { get; set; }
        public string InDone { get; set; }
        public string OutDone { get; set; }
        public string VisitorLocation { get; set; }
        #endregion
    }

    public class VehicleRequistionService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public VehicleRequistionService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }
        public IEnumerable<object> GetToLocation(string Id)
        {
            try
            {
                var sql = @"select Id as Value,UserName as Text from 
                hkp.transportservicelocations 
                where Id <>'"+Id+"'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetFromLocation()
        {
            try
            {
                var sql = @"select Id as Value,UserName as Text from
                hkp.transportservicelocations";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
      
    }

}
  