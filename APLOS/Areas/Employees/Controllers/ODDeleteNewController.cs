using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.HumanResource.NewAttendanceProcess;
using Library.Service.Biometrics;
using System;
using System.Data;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Employees.Controllers
{
    public class ODDeleteNewController : BaseController
    {
        #region Constructor

        private readonly ILeaveTransectionService _leaveTransactionService;
        
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;


        public ODDeleteNewController(

              ILeaveTransectionService leaveTransactionService
              ,ISqlRepository sqlRepository
            ,IUnitOfWork U
            )
        {
            _leaveTransactionService = leaveTransactionService;
            _sqlRepository = sqlRepository;
            _unitOfWork = U;
        }

        #endregion Constructor

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        #endregion -- Pages

        #region -- Operations

        [HttpGet,Authorize]
        public JsonResult Query(string PlantId,string EmpSystemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select eod.Id,eod.EmpSystemId,FORMAT(eod.FromDate,'dd-MMM-yyyy') FromDate,FORMAT(eod.ToDate,'dd-MMM-yyyy') ToDate,
                            eod.IsApproved, ei.EmployeeName,ei.EmployeeCode,ei.PlantId
                           from EmployeeInformation ei
                           left join dbo.EmployeeOnDuty eod on ei.SystemId= eod.EmpSystemId
                           where ei.PlantId='" + PlantId + @"' and eod.EmpSystemId='" + EmpSystemId + @"'
                           order by eod.FromDate desc";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult Delete(string id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
             
                string sql = @"select * from dbo.EmployeeOnDuty WHERE Id='" + id + "'";
                DataTable dt = _sqlRepository.GetDataTable(sql);


                string RowsEdited = "''";

                _leaveTransactionService.ExecuteSqlCommand(@"DELETE FROM  dbo.EmployeeOnDutyDetails WHERE OnDutyId='" + id + "'");
                _leaveTransactionService.ExecuteSqlCommand(@"DELETE FROM  dbo.EmployeeOnDuty WHERE Id='" + id + "'");

                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                DateTime FromDate = Convert.ToDateTime(dt.Rows[0]["FromDate"].ToString());
                DateTime ToDate = Convert.ToDateTime(dt.Rows[0]["ToDate"].ToString());
                string EmpId = dt.Rows[0]["EmpSystemId"].ToString();

                DataSet PlantLock;
                PlantLockCheck(FromDate.ToString(), ToDate.ToString(), out PlantLock, identity.PlantId);
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
                var sqlx = @"select * from AttdnProcessData where WorkDate between '"+FromDate.ToString()+"' and '"+ToDate.ToString()+"' and EmpSystemID='"+EmpId+"'";
                objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                while (FromDate <= ToDate)
                {
                    string newformat = Convert.ToDateTime(FromDate).ToString("yyyyMMdd");
                    dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                    if (dsRef.Tables[0].DefaultView.Count > 0)
                    {
                        string ExistingManualDay = bplib.clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"ManualDayStatus"]).ToString();

                        if (ExistingManualDay=="OD")
                        {
                            RowsEdited = RowsEdited + ",'" + newformat + EmpId + "'";
                            DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();
                            dr["ManualFlag"] = true;
                            dr["ManualByWhom"] = identity.Name;
                            dr["ManualEntryTime"] = Convert.ToDateTime(DateTime.Now);
                            dr["IsOD"] = 0;
                            dr["ManualDayStatus"] = DBNull.Value;
                            dr["IsManualDayStatus"] = false;
                            // Mandatory Nullifying
                            dr["OTComfirmBy"] = DBNull.Value;
                            dr["DateOTComfirm"] = DBNull.Value;
                            dr["IsOTComfirm"] = false;
                            dr["LockedBy"] = DBNull.Value;
                            dr["LockedDate"] = DBNull.Value; 
                            dr["isLock"] = false;

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
                    }

                    FromDate = FromDate.AddDays(1);
                }
                SaveDataSets(dsRef);

                NewAttendanceProcessService ap = new NewAttendanceProcessService();
                ap.ManualScheduler(identity.PlantId, RowsEdited);

                return Json(new { Message = AplosMessage.Deleted });
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }

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
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                        if (dsRef[i].Tables.Count > 0)
                            objCon.SaveDataSetThroughAdapter(ref dsRef[i], true, "1");
                    i++;
                }
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
                    throw exp;
                }
                throw ex;
            }
            finally
            {
                objCon = null;
            }
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




        #endregion -- Operations
    }
}