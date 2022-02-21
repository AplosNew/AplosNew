using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Library.Data.Sql;
using OTSBD;
using bplib;
using Library.HumanResource.NewAttendanceProcess;

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
                var sqlx = @"select d.Id,CardNo,ExpectedDate,ExpectedTime,VisitorCategory,
                VisitorName,VisitorType,Purpose,e.EmployeeName as ToMeet,p.UserName as Department,
				l.UserName as Designation
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

        #endregion
    }

}
  