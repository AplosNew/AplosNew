using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.HumanResource.Leave;
using Library.HumanResource.NewAttendanceProcess;
using Library.Model.HumanResources;
using Library.Security.Core;
using Library.Service.Biometrics;
using Library.Service.Enums;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class EmployeeLeaveApprovalNewController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private readonly ILeaveTransectionService _leaveTransactionService;
        public EmployeeLeaveApprovalNewController(
              IMaternityLeavePolicyService LeavePolicyService,
               ISqlRepository sqlRepository,
               ILeaveTransectionService leaveTransactionService
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _sqlRepository = sqlRepository;
            _leaveTransactionService = leaveTransactionService;
        }

        #endregion Constructor

        #region -- Pages
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


        [HttpGet, Authorize]
        public ActionResult GetGrdAvailedLvDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsLeaveApproval objLvTrsEmpWise;
            objLvTrsEmpWise = new clsLeaveApproval(_sqlRepository);
            var data = objLvTrsEmpWise.GetEmpBasicInfoInformationForLeave(identity.CompanyGroupId, identity.PlantId, identity.IsControlAdmin, identity.IsSysAdmin, identity.EmployeeId, identity.CompanyId);
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
            return Json(_leaveTransactionService.LoadGrdAllocatedLvDetails(identity.CompanyGroupId, identity.PlantId, EmpsystemId, calanderYearId), JsonRequestBehavior.AllowGet);
        }




        [HttpPost]
        public ActionResult SaveLeaveApproval(List<LeaveVM> LeaveData)
        {
            try
            {
                string strSql = "";
                strSql = @"select * from LeaveTransaction  where SystemID = '' and FirstApprovingStatus = 1";
                DataTable dtLTransactionFA = null;

                DataTable dtLTransaction = null;
                string trnIdList = "(' '";

                foreach (LeaveVM item in LeaveData)
                {
                    dtLTransaction = null;

                    trnIdList += ",'" + item.LvTransSystemID + "'";
                }
                trnIdList += ")";

                strSql = @"select * from LeaveTransaction  where SystemID in " + trnIdList + @" AND FirstApprovingStatus = 0";
                dtLTransaction = _sqlRepository.GetDataTable(strSql);


                string empIdList = "";

                if (dtLTransaction.Rows.Count > 0)
                {
                    empIdList = "(' '";
                    for (int i = 0; i < dtLTransaction.Rows.Count; i++)
                    {
                        empIdList += ",'" + dtLTransaction.Rows[i]["EmpSystemID"].ToString() + "'";

                    }
                    empIdList += ")";

                    strSql = "select * from EmployeeInformation where SystemId IN " + empIdList + "";

                    DataTable dtEmpInfo = _sqlRepository.GetDataTable(strSql);

                    string errorMessage = "First authority approval is pending of ";

                    if (dtEmpInfo.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtEmpInfo.Rows.Count; i++)
                        {
                            errorMessage += dtEmpInfo.Rows[i]["EmployeeName"].ToString() + "(" + dtEmpInfo.Rows[i]["EmployeeCode"].ToString() + ")";

                            dtLTransaction.DefaultView.RowFilter = "EmpSystemID = " + dtEmpInfo.Rows[i]["SystemId"].ToString() + "";

                            for (int k = 0; k < dtLTransaction.DefaultView.Count; k++)
                            {
                                if (k > 0)
                                {
                                    errorMessage += " and ";
                                }
                                if (Convert.ToDouble(dtLTransaction.DefaultView[k]["LeaveDays"]) >= 1)
                                {
                                 
                                    errorMessage += " (from " + Convert.ToDateTime(dtLTransaction.DefaultView[k]["FromDate"]).ToString("dd-MMM-yyyy") + " to " + Convert.ToDateTime(dtLTransaction.DefaultView[k]["ToDate"]).ToString("dd-MMM-yyyy") + " )";

                                }
                                else
                                {
                                    errorMessage += " (from " + Convert.ToDateTime(dtLTransaction.DefaultView[k]["FromDate"]).ToString("dd-MMM-yyyy") + ") for half day";
                                }
                            }
                        }
                        errorMessage += ".";
                        throw new Exception(errorMessage);
                    }
                }



                SaveLeave(LeaveData);

                //SaveSandwich(LeaveData);
                return Json(new { Error = false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        void SaveLeave(List<LeaveVM> LeaveData)
        {
            try
            {
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


                //************New Leave Work Code


                string RowsEdited = "''";

                //Getting the Employees Data from the APD Table
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

                    ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                    var sqlx = @"select * from AttdnProcessData where WorkDate between '" + Convert.ToDateTime(item.FromDate) + "' and '" + Convert.ToDateTime(item.ToDate) + @"' 
                            and EmpSystemID ='" + item.EmployeeID + "' ";

                    objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                    

                    //Getting the Leave Code
                    var strCode = "Select Code from dbo.LeaveType where Id = '" + item.LTSystemID + "'";
                    DataTable ddt = _sqlRepository.GetDataTable(strCode);
                    string LeaveCode = ddt.Rows[0]["Code"].ToString();


                   

                    while (Ftd <= Tld)
                    {
                        string newformat = Convert.ToDateTime(Ftd).ToString("yyyyMMdd");
                        if (Ftd <= DateTime.Now.Date)
                        {
                            // If Item is noBnefit is  1  then LeaveCode + WOB (MLWOB)

                            if (item.isNoBenefit == true && (string)bplib.clsWebLib.RetValidLen(item.MPolicyId) != ""  )
                            {
                                LeaveCode = LeaveCode + "WOB";
                            }

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + item.EmployeeID + "' ";
                            RowsEdited = RowsEdited + ",'" + newformat + item.EmployeeID + "'";
                            DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();
                            dr["LeaveStatus"] = LeaveCode;
                            dr["LTSystemID"] = item.LTSystemID;
                            dr["UpdatedBy"] = "Schedule";
                            dr["ManualEntryTime"] = Convert.ToDateTime(DateTime.Now);
                            dr["LockedDate"] = DBNull.Value;
                            dr["ManualByWhom"] = identity.Name;
                            dr["LockedBy"] = DBNull.Value;
                            dr["ManualFlag"] = true;
                            dr["isLock"] = false;
                            dr["OTComfirmBy"] = DBNull.Value;
                            dr["DateOTComfirm"] = DBNull.Value;
                            dr["IsOTComfirm"] = false;

                            #region OT Columns Nullified

                            dr["TargetOT"] = DBNull.Value;
                            dr["PlanOT"] = DBNull.Value;
                            dr["AppliedOTLimit"] = DBNull.Value;
                            dr["AllowedOTLimit"] = DBNull.Value;
                            dr["StandardOT"] = DBNull.Value;
                            dr["AdditionalOt"] = DBNull.Value;

                            #endregion
                            dr.EndEdit();
                        }

                        Ftd = Ftd.AddDays(1);
                    }

                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsRef);

                    var sqls = @"UPDATE LeaveTransactionDetails SET IsAvailed = 1,LeaveStatus = '"+LeaveCode+@"',UpdatedBy = '"+identity.Name+@"',DateUpdated = '"+DateTime.Now+@"'
                                    where LvTrnsSystemID = '"+item.LvTransSystemID+"'";

                    var sqlss = @"UPDATE LeaveTransaction SET IsApproved = 1,UpdatedBy = '" + identity.Name + @"',DateUpdated = '" + DateTime.Now + @"'
                                    where SystemID = '" + item.LvTransSystemID + "'";

                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenConnection("1");
                    objCon.BeginTransaction();
                    objCon.ExecuteNonQueryWrapper(sqls, true, "1");
                    objCon.ExecuteNonQueryWrapper(sqlss, true, "1");
                    objCon.CommitTransaction();
                }

                

                NewAttendanceProcessService ap = new NewAttendanceProcessService();
                ap.ManualScheduler(identity.PlantId, RowsEdited);

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
                    if (_empids == "''")
                    {
                        _maxDate = Convert.ToDateTime(item.ToDate);
                    }
                    //-------------------------------------------------------------
                    _empids += ",'" + item.EmployeeID + "'";
                    if (_minDate > Convert.ToDateTime(item.FromDate))
                    {
                        _minDate = Convert.ToDateTime(item.FromDate);
                    }

                    if (_maxDate < Convert.ToDateTime(item.ToDate))
                    {
                        _maxDate = Convert.ToDateTime(item.ToDate);
                    }
                }//foreach
                if (_empids == "''")
                {
                    _empids = string.Empty;
                }
                _FromDate = _minDate.ToString("dd-MMM-yyyy");
                _ToDate = _maxDate.ToString("dd-MMM-yyyy");
              // obj.ProcessSandwich(_FromDate, _ToDate, DateTime.Now.ToString("dd-MMM-yyyy"), _empids);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult SaveLeaveReject(List<LeaveVM> LeaveData, string CancelationReason)
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
            }

            //MasterId = obj.SaveMasterAndDetailForLeavePolicy(LeaveData);
            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
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

        #endregion





        #endregion -- Operations  
    }
}