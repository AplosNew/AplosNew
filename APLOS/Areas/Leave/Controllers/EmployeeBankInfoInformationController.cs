using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Enums;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Leave.Controllers
{
    public class EmployeeBankInfoInformationController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private readonly IAttendanceManagementService _AttendanceManagementService;
        private DataSet dsRef;

        public EmployeeBankInfoInformationController(
              IMaternityLeavePolicyService LeavePolicyService,
               IAttendanceManagementService AttendanceManagementService,
            ISqlRepository sqlRepository
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _AttendanceManagementService = AttendanceManagementService;
            _sqlRepository = sqlRepository;
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

        [HttpGet, Authorize]
        public ActionResult GetList(string EmpSystemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" select RowID,EmpSystemID,BankSystemID,BankBranchId,BankAccNo,SalaryPercentage,IsApproved,format(ApprovedDateTime,'dd-MMM-yyyy hh:mm tt')as ApprovedDateTime
                             from [dbo].[EmployeeBankInfo] where EmpSystemID='" + EmpSystemId + @"'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Save(EmployeeBankInfo BankInfoInformation)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            SaveBankInfo(BankInfoInformation);
            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }

        public void SaveBankInfo(EmployeeBankInfo BankInfoInformation)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster, dsEmp;
            try
            {
                string sql = "SELECT * FROM [dbo].[EmployeeBankInfo] WHERE RowID='" + BankInfoInformation.RowID + "' ";
                string empsql = "SELECT * FROM [dbo].[EmployeeInformation] WHERE SystemId='" + BankInfoInformation.EmpSystemID + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                objCon.OpenDataSetThroughAdapter(empsql, out dsEmp, false, "1");
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    //string sID = string.Empty;
                    //bplib.clsGenID objGenID = new bplib.clsGenID();
                    //objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "EmployeeBankInfo", out sID);
                    //dr["RowID"] = sID;
                    dr["EmpSystemID"] = BankInfoInformation.EmpSystemID;
                    dr["BankSystemID"] = BankInfoInformation.BankSystemID;
                    dr["BankBranchId"] = BankInfoInformation.BankBranchId;
                    dr["BankAccNo"] = BankInfoInformation.BankAccNo;
                    dr["SalaryPercentage"] = BankInfoInformation.SalaryPercentage;
                    dr["IsApproved"] = BankInfoInformation.IsApproved;
                    dr["ApprovedDateTime"] = DBNull.Value;

                    dr["AddedBy"] = identity.Name;
                    dr["DateAdded"] = DateTime.Now;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    dr["EmpSystemID"] = BankInfoInformation.EmpSystemID;
                    dr["BankSystemID"] = BankInfoInformation.BankSystemID;
                    dr["BankBranchId"] = BankInfoInformation.BankBranchId;
                    dr["BankAccNo"] = BankInfoInformation.BankAccNo;
                    dr["SalaryPercentage"] = BankInfoInformation.SalaryPercentage;
                    dr["IsApproved"] = BankInfoInformation.IsApproved;
                    dr["ApprovedDateTime"] = BankInfoInformation.ApprovedDateTime;

                    dr["UpdatedBy"] = identity.Name;
                    dr["DateUpdated"] = System.DateTime.Now.ToString();

                    dr.EndEdit();
                }

                DataView dv = new DataView(dsEmp.Tables[0]);
                dv.RowFilter = "SystemId='" + BankInfoInformation.EmpSystemID + "'";
                if (dv.Count > 0)
                {
                    DataRow drmo = dv[0].Row;

                    drmo.BeginEdit();

                    drmo["PaymentMode"] = BankInfoInformation.PaymentMode;
                    drmo["UpdatedBy"] = identity.Name;
                    drmo["DateUpdated"] = DateTime.Now.ToString();

                    drmo.EndEdit();

                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsEmp);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public ActionResult Delete(string Id)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"Delete FROM [dbo].[EmployeeBankInfo] WHERE RowID='" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" select b.UserName,b.Id BankSystemID,bb.UserName as BankBranch,bb.Id BankBranchId from [HKP].[Bank] b
inner join  [HKP].[BankBranch] bb on bb.BankId=b.Id ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion -- Operations  
    }

    public class EmployeeBankInfo : BaseModel
    {
        #region Scalar Properties            
        public int RowID { get; set; }
        public string EmpSystemID { get; set; }
        public string BankSystemID { get; set; }
        public string BankBranchId { get; set; }
        public string BankAccNo { get; set; }
        public decimal SalaryPercentage { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? ApprovedDateTime { get; set; }
        public string PaymentMode { get; set; }

        #endregion Scalar Properties

        #region Audit Properties
        [NeverUpdate]
        public string ApprovedBy { get; set; }
        [NeverUpdate]
        public string AddedBy { get; set; }
        [NeverUpdate]
        public DateTime? DateAdded { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }
        #endregion Audit Properties
    }

}