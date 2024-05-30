using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.HumanResource.Leave;
using Library.HumanResource.NewAttendanceProcess;
using Library.Model.Biometrics;
using Library.Model.HumanResources;
using Library.Service.Biometrics;
using Library.Service.Enums;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using Library.Service.Setups;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Leave.Controllers
{
    public class FirstAuthEmpLeaveApprovalController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private readonly ILeaveTransectionService _leaveTransactionService;
        private readonly ILeaveTransactionNewService _leaveTransactionNewService;

        private readonly IMailSenderService _mailSenderService;

        public FirstAuthEmpLeaveApprovalController(
              IMaternityLeavePolicyService LeavePolicyService,
              ILeaveTransactionNewService leaveTransactionNewService,
               ISqlRepository sqlRepository,
               ILeaveTransectionService leaveTransactionService
             , IMailSenderService mailSenderService

            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _sqlRepository = sqlRepository;
            _leaveTransactionService = leaveTransactionService;
            _leaveTransactionNewService = leaveTransactionNewService;
            _mailSenderService = mailSenderService;
            
        }

        #endregion Constructor

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        #region All Grid 


        private void LoadGrdAvailedLvDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;



            DataSet dsLocalMst = null;
            DataSet dsLocalchd = null;

            DataSet dsLoadGrd = null;
            DataView dvLoadGrd = null;

            string strLvTrnsSystemID = "";
            bool IsControlAdmin = Convert.ToBoolean(Session["ca"]);
            bool IsSysAdmin = Convert.ToBoolean(Session["sa"]);
            var CompanyId = (string)Session["COMPANY_ID"];
            var employeeId = (string)Session["employeeId"];

            try
            {
                clsLeaveApproval objLvTrsEmpWise;
                objLvTrsEmpWise = new clsLeaveApproval();

                //objLvTrsEmpWise.GetSysIdWiseEmpBasicInfoInformationForLeave((string)Session["LOGIN_GROUP_ID"].ToString().Trim(), ddlPlant.SelectedValue.ToString().Trim(), IsControlAdmin, IsSysAdmin, employeeId, CompanyId, out dsLocalMst);

                objLvTrsEmpWise.GetSysIdWiseEmpBasicInfoInformationForLeave(identity.CompanyGroupId, identity.PlantId, identity.IsControlAdmin, identity.IsSysAdmin, employeeId, CompanyId, out dsLocalMst);


                dvLoadGrd = new DataView();
                //dsLocalMst.Tables[0].TableName = "TbMaster";
                dvLoadGrd.Table = dsLocalMst.Tables[0];

                dsLoadGrd = new DataSet();
                dsLoadGrd.Tables.Add(dvLoadGrd.ToTable());



                //if (dsLoadGrd.Tables[0].Rows.Count > 0)
                //{
                //    dgLvTransDtl.DataSource = dsLoadGrd.Tables[0];
                //    dgLvTransDtl.DataBind();
                //    dgLvTransDtl.Visible = true;
                //}
                //else
                //{
                //    dgLvTransDtl.DataSource = null;
                //    dgLvTransDtl.DataBind();
                //    dgLvTransDtl.Visible = false;
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        }//End Function


        [HttpGet,Authorize]
        public ActionResult GetGrdAvailedLvDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsLeaveApproval objLvTrsEmpWise;
            objLvTrsEmpWise = new clsLeaveApproval(_sqlRepository);
           // IEnumerable<object> GetEmpBasicInfoInformationForLeaveMA(string companyGroupId, string plantId, bool isControlAdmin, bool isSysAdmin, string employeeId, string companyId, string FirstApprovingAuthority)
            var data = objLvTrsEmpWise.GetEmpBasicInfoInformationForLeaveMA(identity.CompanyGroupId, identity.PlantId, identity.IsControlAdmin, identity.IsSysAdmin, identity.EmployeeId, identity.CompanyId,identity.EmployeeId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetEmpLeaveBalance(string EmpsystemId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsLeaveApproval objLvTrsEmpWise;
            objLvTrsEmpWise = new clsLeaveApproval(_sqlRepository);
            List<Dictionary<string, object>> data = (List<Dictionary<string, object>>)objLvTrsEmpWise.GetYearlyCalendarInfoCmb(identity.CompanyGroupId, identity.PlantId);
            string calanderYearId = data[0]["Id"].ToString();
            //return Json(_leaveTransactionService.LoadGrdAllocatedLvDetails(identity.CompanyGroupId, identity.PlantId, EmpsystemId, calanderYearId), JsonRequestBehavior.AllowGet);
            return Json(_leaveTransactionNewService.LoadGrdAllocatedLvDetailsNew(identity.CompanyGroupId, identity.PlantId, EmpsystemId, calanderYearId), JsonRequestBehavior.AllowGet);

        }


        public void PlantLockCheck(string FDate, string TDate, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string From = Convert.ToDateTime(FDate).ToString("dd-MMM-yyyy");
                string To = Convert.ToDateTime(TDate).ToString("dd-MMM-yyyy");

                var sql = @"select * from PlantWiseAttendanceLock where PlantId='" + Plant + @"'
                and LockedDate between '" + From + "' and '" + To + "' and IsActive='1'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost,Authorize]
        public ActionResult SaveLeaveApproval(List<LeaveVM> LeaveData)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                foreach (LeaveVM item in LeaveData)
                {
                    DateTime Ftd = Convert.ToDateTime(item.FromDate);
                    DateTime Tld = Convert.ToDateTime(item.ToDate);
                    DataSet PlantLock;
                    PlantLockCheck(Ftd.ToString(), Tld.ToString(), out PlantLock, identity.PlantId);
                    string pl = "";
                    if (PlantLock.Tables[0].Rows.Count > 0)
                    {
                        for (var i = 0; i < PlantLock.Tables[0].Rows.Count; i++)
                        {
                            pl = pl + " " + PlantLock.Tables[0].Rows[i]["LockedDate"].ToString() + ", ";
                        }

                        throw new Exception("The Plant is Locked for - " + pl);
                    } 
                }

                string trnIdList = "(' '";

                foreach (LeaveVM item in LeaveData)
                {
                    
                    trnIdList += ",'" + item.LvTransSystemID + "'";

                    if (!string.IsNullOrEmpty(item.EmployeeID))
                    {
                        string responsiblePersonName = "";
                        string responsiblePersonId = "";
                        string responsiplePersonEmail = "";
                        string mailMessage = "";
                        DataTable dtEmpInfo = _sqlRepository.GetDataTable(@"SELECT * FROM EmployeeInformation WHERE SystemId = '" + item.EmployeeID + @"'");

                        var dtFmDate = Convert.ToDateTime(item.FromDate);
                        var dtToDate = Convert.ToDateTime(item.ToDate);

                        TimeSpan difference = dtToDate - dtFmDate;
                        var leaveDays = Convert.ToInt32(difference.Days + 1);
                        //item.Le = LeaveStatus.Canceled.ToString();

                        responsiblePersonName = dtEmpInfo.Rows[0]["EmployeeName"].ToString();
                        responsiblePersonId = dtEmpInfo.Rows[0]["SystemId"].ToString();
                        responsiplePersonEmail = dtEmpInfo.Rows[0]["EmailId"].ToString();


                        string dt = "";
                        dt = item.ToDate != null ? item.ToDate : "";

                        mailMessage = @"Dear " + responsiblePersonName + "<br> <br> <br>" +
                                            " Your leave request has been Accepted for " + leaveDays + " Day(s),  Dated From " + item.FromDate + " To " + dt +
                                            ". If any discrepancy, Please contact to concern HOD." +
                                            "<br> <br> <br>" +
                                            "Thank you";

                        _mailSenderService.SendFirstLeaveApproveRequestMail(responsiblePersonId, identity.PlantId, mailMessage, responsiplePersonEmail, responsiblePersonName, item.EmployeeID, dtEmpInfo.Rows[0]["EmployeeName"].ToString(), dtEmpInfo.Rows[0]["EmployeeCode"].ToString());
                    }
                }
                trnIdList += ")";


                ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                connection.BeginTransaction();
                string strSql = @"Update LeaveTransaction set FirstApprovingStatus = 1,FirstApprovingDate = '" + DateTime.Now + "' where SystemID IN " + trnIdList + "";
                connection.executeQuery(strSql);
              
                connection.CommitTransaction();


                

                //SaveLeave(LeaveData);

                //SaveSandwich(LeaveData);
                return Json(new {Error=false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new {Error=true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        void SaveLeave(List<LeaveVM> LeaveData)
        {
            try
            {
                clsLeaveApproval objLvTrsEmpWise;
                objLvTrsEmpWise = new clsLeaveApproval(_sqlRepository);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                foreach (LeaveVM item in LeaveData)
                {
                    string _sql_AttdnManual = @"select e.EmployeeCode,format(d.workdate,'dd-MMM-yyyy') wd from AttdnManualData AS  d
                                        inner join EmployeeInformation e on e.systemid=d.EmpSystemID
                                        where d.EmpSystemID  in (" + item.EmployeeID + @") and
                                        d.WorkDate BETWEEN '" + Convert.ToDateTime(item.FromDate).ToString("dd-MMM-yyyy") + @"' AND '" + Convert.ToDateTime(item.ToDate).ToString("dd-MMM-yyyy") + @"' AND d.DayStatus IS NOT NULL AND  d.DayStatus<>'HDP'";
                    DataTable dtLeave = _sqlRepository.GetDataTable(_sql_AttdnManual);
                    if (dtLeave.Rows.Count > 0)
                    {
                        string msg = string.Empty;
                        foreach (DataRow item2 in dtLeave.Rows)
                        {
                            if (msg == "")
                                msg = "'" + item2["EmployeeCode"].ToString() + "' on (" + item2["wd"].ToString() + @")";
                            else
                                msg += ", '" + item2["EmployeeCode"].ToString() + "' on (" + item2["wd"].ToString() + @")";
                        }

                        throw new Exception("Manual attendance for the following employees must be deleted..." + msg);
                    }
                }

                foreach (LeaveVM item in LeaveData)
                {
                    LeaveCustomPara obj = new LeaveCustomPara();
                    obj.EmpSystemId = item.EmployeeID;
                    obj.FromDate = Convert.ToDateTime(item.FromDate);
                    obj.ToDate = Convert.ToDateTime(item.ToDate);
                    obj.LvTransSystemID = item.LvTransSystemID;
                    obj.LTSystemID = item.LTSystemID;
                    obj.CalanderYearID = item.CalanderYearID;

                    obj.PlantId = identity.PlantId;
                    obj.CompanyId = identity.CompanyId;
                    obj.GroupId = identity.CompanyGroupId;
                    obj.UserId = identity.Name;
                    obj.EmpSystemId = item.EmployeeID;

                    objLvTrsEmpWise.SaveData(obj);
                    

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            ///MasterId = obj.SaveMasterAndDetailForLeavePolicy(LeaveData);
            //return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }
        void SaveSandwich(List<LeaveVM> LeaveData)
        {
            string _FromDate = string.Empty;
            string _ToDate = string.Empty;
            string _empids = "''";
            DateTime _minDate = DateTime.Now;
            DateTime _maxDate = DateTime.Now;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //clsLeaveSandwich obj = new clsLeaveSandwich(identity.CompanyGroupId, identity.PlantId, identity.Name);
                foreach (LeaveVM item in LeaveData)
                {
                    if(_empids=="''")
                    {
                        _maxDate = Convert.ToDateTime(item.ToDate);
                    }
                    //-------------------------------------------------------------
                    _empids +=",'"+ item.EmployeeID+"'";
                    if(_minDate>Convert.ToDateTime(item.FromDate))
                    {
                        _minDate = Convert.ToDateTime(item.FromDate);
                    }

                    if (_maxDate < Convert.ToDateTime(item.ToDate))
                    {
                        _maxDate = Convert.ToDateTime(item.ToDate);
                    }
                }//foreach
                if(_empids=="''")
                {
                    _empids = string.Empty;
                }
                _FromDate = _minDate.ToString("dd-MMM-yyyy");
                _ToDate = _maxDate.ToString("dd-MMM-yyyy");
                //obj.ProcessSandwich(_FromDate, _ToDate, DateTime.Now.ToString("dd-MMM-yyyy"), _empids);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost,Authorize]
        public ActionResult SaveLeaveReject(List<LeaveVM> LeaveData,string CancelationReason)
        {
            clsLeaveApproval objLvTrsEmpWise;
            objLvTrsEmpWise = new clsLeaveApproval(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            foreach (LeaveVM item in LeaveData)
            {
                LeaveCustomPara obj = new LeaveCustomPara();
                obj.EmpSystemId = item.EmployeeID;
                obj.FromDate = Convert.ToDateTime(item.FromDate);
                obj.ToDate = Convert.ToDateTime(item.ToDate);
                obj.LvTransSystemID = item.LvTransSystemID;
                obj.LTSystemID = item.LTSystemID;
                obj.CalanderYearID = item.CalanderYearID;
                obj.CancelationReason = CancelationReason;


                obj.PlantId = identity.PlantId;
                obj.CompanyId = identity.CompanyId;
                obj.GroupId = identity.CompanyGroupId;
                obj.UserId = identity.Name;
                obj.EmpSystemId = item.EmployeeID;
                objLvTrsEmpWise.Reject(obj);

                if (!string.IsNullOrEmpty(item.EmployeeID))
                {
                    string responsiblePersonName = "";
                    string responsiblePersonId = "";
                    string responsiplePersonEmail = "";
                    string mailMessage = "";
                    DataTable dtEmpInfo = _sqlRepository.GetDataTable(@"SELECT * FROM EmployeeInformation WHERE SystemId = '" + item.EmployeeID + @"'");

                    var dtFmDate = Convert.ToDateTime(item.FromDate);
                    var dtToDate = Convert.ToDateTime(item.ToDate);

                    TimeSpan difference = dtToDate - dtFmDate;
                    var leaveDays = Convert.ToInt32(difference.Days + 1);
                    //item.Le = LeaveStatus.Canceled.ToString();

                    responsiblePersonName = dtEmpInfo.Rows[0]["EmployeeName"].ToString();
                    responsiblePersonId = dtEmpInfo.Rows[0]["SystemId"].ToString();
                    responsiplePersonEmail = dtEmpInfo.Rows[0]["EmailId"].ToString();
                    

                    string dt = "";
                        dt = item.ToDate != null ? item.ToDate : "";

                        mailMessage = @"Dear " + responsiblePersonName + "<br> <br> <br>" +
                                            " Your leave request has been rejected for " + leaveDays + " Day(s),  Dated From " + item.FromDate + " To " + dt +
                                            ". Please contact to concern HOD." +
                                            "<br> <br> <br>" +
                                            "Thank you";
                    
                    _mailSenderService.SendFirstLeaveApproveRequestMail(responsiblePersonId, identity.PlantId, mailMessage, responsiplePersonEmail, responsiblePersonName, item.EmployeeID, dtEmpInfo.Rows[0]["EmployeeName"].ToString(), dtEmpInfo.Rows[0]["EmployeeCode"].ToString());
                }




            }





            //MasterId = obj.SaveMasterAndDetailForLeavePolicy(LeaveData);
            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }












        #endregion





        #endregion -- Operations  
    }
}