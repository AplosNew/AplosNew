using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Biometrics;
using Library.Model.HumanResources;
using Library.Service.Biometrics;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.HumanResources;
using System;
using System.Data;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Employees.Controllers
{
    public class ODDeleteController : BaseController
    {
        #region Constructor

        private readonly ILeaveTransectionService _leaveTransactionService;
        
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;


        public ODDeleteController(

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

        [Authorize]
        public ActionResult ODDelete()
        {
            return View();
        }
        #endregion -- Pages

        #region -- Operations

        [HttpGet]
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

        public ActionResult Delete(string id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsAttendance.AttendanceProcessAplos obj = new AttendanceProcessAplos();

                string sql = @"select * from dbo.EmployeeOnDuty WHERE Id='" + id + "'";
                DataTable dt = _sqlRepository.GetDataTable(sql);
                

                _leaveTransactionService.ExecuteSqlCommand(@"DELETE FROM  dbo.EmployeeOnDutyDetails WHERE OnDutyId='" + id + "'");
                _leaveTransactionService.ExecuteSqlCommand(@"DELETE FROM  dbo.EmployeeOnDuty WHERE Id='" + id + "'");

                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                DateTime FromDate = Convert.ToDateTime(dt.Rows[0]["FromDate"].ToString());
                DateTime ToDate = Convert.ToDateTime(dt.Rows[0]["ToDate"].ToString());
                while (FromDate <= ToDate)
                {

                    obj.SaveTotal(identity.PlantId, FromDate.ToString("dd-MMM-yyyy"), dt.Rows[0]["EmpSystemID"].ToString(), true);
                    FromDate = FromDate.AddDays(1);
                }

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


        #endregion -- Operations
    }
}