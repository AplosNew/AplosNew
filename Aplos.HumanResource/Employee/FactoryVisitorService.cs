using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Library.Data.Sql;
using OTSBD;
using bplib;
using Library.HumanResource.NewAttendanceProcess;
using Library.Service.Setups;

namespace Library.HumanResource.Employee
{
    #region Visitor Process Functions

    public class FactoryVisitorService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
        private readonly IMailSenderService _mailSenderService;

        public FactoryVisitorService(IMailSenderService mailSenderService)
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
            _mailSenderService = mailSenderService;

        }
        public string SaveEmployeeVisit(IEnumerable<VisitorModel> DataToSave)
        {
            try
            {
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<VisitorModel> items = DataToSave.ToList();

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
                        dr["CardNo"] = item.CardNo;
                        dr["NoOfPerson"] = item.NoOfPerson;
                        dr["MobileNo"] = item.MobileNo;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = DateTime.Now.ToString();
                        dr["AddedFromIP"] = item.AddedFromIP;
                        if(item.param=="In")
                        {
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

                #region Email Sender Service

                if (!string.IsNullOrEmpty(items[0].ToMeet))
                {
                    string mailMessage = "";
                    DataTable dtEmpInfo = _sqlRepository.GetDataTable(@"select * from EmployeeInformation where SystemId = '" + items[0].ToMeet + @"'");
                    string EmpName = dtEmpInfo.Rows[0]["EmployeeName"].ToString();
                    string EmailId = dtEmpInfo.Rows[0]["EmailId"].ToString();

                   
                    mailMessage = @"Dear " + EmpName + "<br> <br> <br>" +
                       " You have a Entry of a Visitor "+ items[0].VisitorName +" for the Purpose of "+items[0].Purpose+ 
                       ". Please go to the App to View the List." +
                       "<br> <br> <br>" +
                       "Thank you";

                    _mailSenderService.SendVisitorEntryAlertMail(items[0].ToMeet, mailMessage, EmailId,
                        EmpName, items[0].VisitorName);
                }
                #endregion


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
				l.UserName as Designation
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
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                if (DataToSave.Count() == 0)
                {
                    return "No Data Found";
                }
                var items = DataToSave.ToList();
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

    #endregion

    #region Vehicle Requistion Module Functions

    public class VehicleRequistionModel
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
        public string Id { get; set; }
        public string Reason { get; set; }
        public string Purpose { get; set; }
        public string Remarks { get; set; }
        public DateTime Date { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
        public string ApprovingAuthority { get; set; }
        public string ToLoc { get; set; }
        public string FromLoc { get; set; }
        public string EmployeeId { get; set; }
        public string IsApproved { get; set; }
      
        #endregion
    }
    public class VehicleRequistionService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
        private readonly IMailSenderService _mailSenderService;

        public VehicleRequistionService(IMailSenderService mailSenderService)
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
            _mailSenderService = mailSenderService;

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

        public IEnumerable<object> GetApprovingAuthList()
        {
            try
            {
                var sql = @"select ei.EmployeeName as Text,ei.SystemId as Value
                from ServicesApprovingAuthority s left join EmpServiceType e on e.Id=s.ServiceId
                left join EmployeeInformation ei on ei.SystemId=s.EmpId
                where e.Service='Transport'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string SaveData(IEnumerable<VehicleRequistionModel> DataToSave)
        {
            try
            {
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<VehicleRequistionModel> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from dbo.VehicleRequistionData where 1=2", out dsMaster, false, "1");

                foreach (VehicleRequistionModel item in DataToSave)
                {

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();
                        clsGenID genid = new clsGenID();
                        genid.GenID("VehicleRequistion", out string _Id);

                        dr["Id"] = "VR" + _Id;
                        dr["Date"] = DateTime.Now.ToString("dd-MMM-yyyy");
                        dr["EmployeeId"] = item.EmployeeId;
                        dr["FromLoc"] = item.FromLoc;
                        dr["ToLoc"] = item.ToLoc;
                        dr["FromTime"] = item.FromTime;
                        dr["ToTime"] = item.ToTime;
                        dr["Purpose"] = item.Purpose;
                        dr["Remarks"] = item.Remarks;
                        dr["Reason"] = item.Reason;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = DateTime.Now.ToString();
                        dr["AddedFromIP"] = item.AddedFromIP;
                        dr["IsApproved"] = false;
                        dr["ApprovingAuthority"] = item.ApprovingAuthority;
                    
                        dsMaster.Tables[0].Rows.Add(dr);
                    }

                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                string MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();


                #region Email Sender Service

                if (!string.IsNullOrEmpty(items[0].ApprovingAuthority))
                {
                    string mailMessage = "";
                    DataTable dtApprovalEmpInfo = _sqlRepository.GetDataTable(@"select * from EmployeeInformation where SystemId = '" + items[0].ApprovingAuthority + @"'");
                    string ApprovalEmpName = dtApprovalEmpInfo.Rows[0]["EmployeeName"].ToString();
                    string RespEmailId = dtApprovalEmpInfo.Rows[0]["EmailId"].ToString();

                    DataTable dtEmpInfo = _sqlRepository.GetDataTable(@"select * from EmployeeInformation where SystemId = '" + items[0].EmployeeId + @"'");
                    string EmpName = dtEmpInfo.Rows[0]["EmployeeName"].ToString();
                    string EmpCode= dtEmpInfo.Rows[0]["EmployeeCode"].ToString();

                    mailMessage = @"Dear " + ApprovalEmpName + "<br> <br> <br>" +
                       " You have a Vehicle Requistion Approval request of " + EmpName + "(" + dtEmpInfo.Rows[0]["EmployeeCode"].ToString() + ") For " + items[0].FromTime + " To " + items[0].ToTime +
                       ". Please go to the App for Approving." +
                       "<br> <br> <br>" +
                       "Thank you";

                    _mailSenderService.SendVehicleRequistionApproveMail(items[0].ApprovingAuthority, mailMessage, RespEmailId,
                        ApprovalEmpName, items[0].EmployeeId, EmpName, EmpCode);
                }
                #endregion


                return MasterId;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

    }

    #endregion
}
