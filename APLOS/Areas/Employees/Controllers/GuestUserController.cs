#region Using

using Aplos.Controllers;
using Aplos.HumanResource;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Service.Employees;
using Library.Service.Helpers;
using Newtonsoft.Json;
using OTSBD;
using System;
using System.Data;
using System.IO;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Employees.Controllers
{
    public class GuestUserController : BaseController
    {
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IEmployeeProfileService _employeeProfileService;
        EmployeeProfile employeeProfile = new EmployeeProfile();
        public GuestUserController(
              IEmployeeProfileService employeeProfileService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork
            )
        {
            _employeeProfileService = employeeProfileService;
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
        }

        #endregion Constructor


        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
       


        [HttpGet, Authorize]
        public ActionResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            JsonResult json = Json(employeeProfile.GetGuestEmployee(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }


        [HttpGet, Authorize]
        public ActionResult GetGusetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
           string CmdText = @"SELECT MB.Id,E.SystemId EmployeeId,E.EmployeeCode,E.GroupID,E.DivisionId,E.DepartmentId,E.SectionId,E.SubSectionId,E.DesignationGroupId,E.DesignationSystemID,E.BudgetCode,E.PositionID
	                        ,E.CardNumber,E.Salutation,E.FirstName,E.MiddleName,E.LastName,E.EmployeeName,E.NickName,E.EmpPicPath,FORMAT(E.DOB,'dd-MMM-yyyy') DOB ,E.GenderID,E.GivenDesignationId	,E.LegalDesignationId
	                        ,E.EmailId,E.EmpType,D.UserName Division,DPT.UserName Department, S.UserName Section, SS.UserName SubSection,DG.UserName GivenDesignation,LDG.UserName Designation,E.IsAccessible,MB.PIN
                        FROM HKP.EmployeeMobileAppsAuthorization MB  
						LEFT JOIN EmployeeInformation E ON MB.EmployeeId = E.SystemId
                        LEFT JOIN ORG.Division D ON D.Id = E.DivisionId
                        LEFT JOIN ORG.Department DPT ON DPT.Id = E.DepartmentId
                        LEFT JOIN ORG.Section S ON S.Id = E.SectionId
                        LEFT JOIN ORG.SubSection SS ON SS.Id = E.SubSectionId
                        LEFT JOIN HKP.Designation DG ON DG.Id = E.GivenDesignationId
                        LEFT JOIN HKP.LegalDesignation LDG ON LDG.Id = E.LegalDesignationId
                        WHERE E.GroupID = '" + identity.CompanyGroupId + "' AND EmpType='Guest'";
            return Json(_sqlRepository.GetDataCollection(CmdText), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult GetSectionCboByDepartment(string deptID)
        {
            var sql = @"SELECT Id,UserName FROM ORG.Section  Order By UserName";
            return Json(_sqlRepository.GetCombo(sql, "Id", "UserName"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet,Authorize]
        public JsonResult GetSubSectionCboBySection(string secID)
        {
            var sql = @"SELECT Id,UserName FROM ORG.SubSection Order By UserName";
            return Json(_sqlRepository.GetCombo(sql, "Id", "UserName"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAllLegalDesignationCbo(string companyGroupId)
        {
            try
            {
                var sql = @"SELECT A.Id AS [Value], A.UserName AS [Text], B.DesignationGroupId FROM [HKP].[LegalDesignation] A
                            LEFT OUTER JOIN (SELECT * FROM [MST].[DesignationMasterLegalDesignation]) DL ON A.Id=DL.LegalDesignationId
                            LEFT OUTER JOIN (SELECT * FROM [MST].[DesignationMaster] where CompanyGroupId='" + companyGroupId + @"')B ON DL.DesignationMasterId = B.Id
                            Order By A.UserName";

                return Json(_sqlRepository.GetDataCollection(sql, null),JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult Create(FormCollection form)
        {
            var pre = form["employeeInformation"];
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            var employee = JsonConvert.DeserializeObject<EmployeeInformation>(pre, settings);
            

            SaveData(employee, out string Id);
            employee.SystemId = Id;
            var file = Request.Files["file"];
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);
                if (extension.ToLower() == ".jpg" || extension.ToLower() == ".png")
                {
                    employee.EmpPicPath = extension;
                    if (!string.IsNullOrEmpty(employee.EmpPicPath))
                        employee.EmpPicPath = Id + employee.EmpPicPath;
                }
                else
                    throw new CustomException(Resources.ImageUploadError);
            }

            if (file != null)
            {
                var path = Path.Combine(ResourcesPathReader.GetEmployeeDestinationPicPath()/*Server.MapPath("~" + new AppSettingsReader().GetValue(UrlResources.EmployeeImage, typeof(string)).ToString())*/, employee.EmpPicPath);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    file.SaveAs(path);
                }
                else
                {
                    file.SaveAs(path);
                }
            }
            return Json(new { EmployeeInformation = employee, Message = AplosMessage.Success });
        }

        private string GetPadding(string iv)
        {
            while (iv.Length < bplib.clsWebLib.EMP_BASIC_PK_PAD)
            {
                iv = "0" + iv;
            }
            return iv;
        }

        private string GetPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;

            //bplib.clsGenID objGenID = null;
            //objGenID = new bplib.clsGenID();
            //objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "EMP_BASIC", out idFromDB);
           
           
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "EMP_BASIC", out idFromDB);
            string syspad = GetPadding(idFromDB);
            sID = DateTime.Now.ToString("yy") + syspad;
            //sID = idFromDB.Trim();
            return sID;

        }

        private string GetPinPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "EMP_PIN", out idFromDB);

            sID = idFromDB.Trim();
            return sID;

        }

        private void SaveData(EmployeeInformation data, out string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string strEmpSystemID = string.Empty;
           
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string sql = "SELECT * FROM [dbo].[EmployeeInformation] WHERE SystemId='" + data.SystemId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");
               
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    strEmpSystemID = GetPK();

                    dr["SystemId"] = strEmpSystemID;
                    dr["GroupID"] = identity.CompanyGroupId;
                    dr["EmployeeName"] = data.EmployeeName;
                    dr["NickName"] = data.NickName;
                    dr["EmpPicPath"] = strEmpSystemID + ".jpg";
                    dr["EmailId"] = data.EmailId;
                    dr["GenderID"] = data.GenderID;
                    dr["DivisionId"] = data.DivisionID;
                    dr["DepartmentId"] = data.DepartmentID;
                    dr["SectionId"] = data.SectionID;
                    dr["SubSectionId"] = data.SubSectionID;
                    dr["LegalDesignationId"] = data.LegalDesignationId;
                    dr["EmployeeStatus"] = "Active";
                    dr["EmpType"] = "Guest";
                    dr["TentativeExpiryDate"] = data.TentativeExpiryDate;
                    dr["IsConfirmed"] = false;
                    dr["ExcludeOT"] = false;
                    dr["EmployeeCodeNumeric"] = 0;
                    dr["isLeaveOnDOC"] = false;

                    dr["AddedBy"] = identity.Name;
                    dr["DateAdded"] = DateTime.Now;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();
                    dr["EmployeeName"] = data.EmployeeName;
                    dr["NickName"] = data.NickName;
                    dr["EmpPicPath"] = data.SystemId + ".jpg";
                    dr["EmailId"] = data.EmailId;
                    dr["GenderID"] = data.GenderID;
                    dr["DivisionId"] = data.DivisionID;
                    dr["DepartmentId"] = data.DepartmentID;
                    dr["SectionId"] = data.SectionID;
                    dr["SubSectionId"] = data.SubSectionID;
                    dr["LegalDesignationId"] = data.LegalDesignationId;
                    dr["TentativeExpiryDate"] = data.TentativeExpiryDate;
                    dr["IsConfirmed"] = false;
                    dr["ExcludeOT"] = false;
                    dr["isLeaveOnDOC"] = false;
                    dr["EmployeeCodeNumeric"] = 0;

                    dr["UpdatedBy"] = identity.Name;
                    dr["DateUpdated"] = System.DateTime.Now.ToString();

                    dr.EndEdit();
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);

                SaveEmployeePIN(dsMaster.Tables[0].Rows[0]["SystemId"].ToString());
                Id = dsMaster.Tables[0].Rows[0]["SystemId"].ToString();
            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }

        private void SaveEmployeePIN(string employeeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string strEmpSystemID = string.Empty;

            ConnectionManager.DAL.ConManager objCon;
            try
            {

                string sql = "SELECT * FROM [HKP].[EmployeeMobileAppsAuthorization] WHERE EmployeeId='" + employeeId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");


                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();


                    dr["Id"] = GetPinPK();
                    dr["EmployeeId"] = employeeId;
                    dr["PIN"] = new Random().Next(111111, 999999).ToString();
                    dr["IsSalaryStructure"] = false;
                    dr["IsPaySlip"] = false;
                    dr["IsMonthlyAttendance"] = false;
                    dr["IsDailyAttendanceNotification"] = false;
                    dr["IsSalaryProcessConfirmationNotification"] = false;
                    dr["IsSalaryDisbursementNotification"] = false;
                    dr["IsIncrementNotification"] = false;
                    dr["IsPromotionNotification"] = false;
                    dr["IsLeaveNotification"] = false;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["AddedDate"] = DateTime.Now;


                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();
                    dr["IsSalaryStructure"] = false;
                    dr["IsPaySlip"] = false;
                    dr["IsMonthlyAttendance"] = false;
                    dr["IsDailyAttendanceNotification"] = false;
                    dr["IsSalaryProcessConfirmationNotification"] = false;
                    dr["IsSalaryDisbursementNotification"] = false;
                    dr["IsIncrementNotification"] = false;
                    dr["IsPromotionNotification"] = false;
                    dr["IsLeaveNotification"] = false;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr["UpdatedDate"] = DateTime.Now.ToString();

                    dr.EndEdit();
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }


        public ActionResult Delete(string id)
        {
            _employeeProfileService.Delete(id);
            return Json(new {  Message = AplosMessage.Deleted });
        }
    }
}