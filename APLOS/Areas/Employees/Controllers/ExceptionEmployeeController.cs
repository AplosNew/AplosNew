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
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Employees.Controllers
{
    public class ExceptionEmployeeController : BaseController
    {
        #region Constructor

        private readonly ILeaveTransectionService _leaveTransactionService;
        private readonly IRestService _restService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRestDetailsService _restDetailsService;

        public ExceptionEmployeeController(

              ILeaveTransectionService leaveTransactionService
              , ISqlRepository sqlRepository
            , IRestService restService
             , IRestDetailsService restDetailsService
            , IUnitOfWork U
            )
        {
            _leaveTransactionService = leaveTransactionService;
            _restService = restService;
            _sqlRepository = sqlRepository;
            _unitOfWork = U;
            _restDetailsService = restDetailsService;
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
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_restService.Query(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetRestDetailsData(string restId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_restService.GetRestDetailsData(restId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAllEmployeeList(GridParameter parameters, string sectionId, string subSectionId, string departmentId, bool isOTEntitle, string AttendanceRestDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_restService.GetAllEmployeeForEx(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, sectionId, subSectionId, departmentId, isOTEntitle, AttendanceRestDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetExceptionEmployeeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"Select 0 CheckBoxSelect, EI.SystemID,EI.EmployeeCode
                                , EI.EmployeeName,FORMAT(EI.DOJ, 'dd-MMM-yyyy')   DOJ, FORMAT(EI.DOS, 'dd-MMM-yyyy') DOS
                                ,DG.UserName GivenDesignation
                                , DP.UserName Department
                                 , PR.UserName PositionName
                                  , DSG.UserName Designation
                                   , PR.DesignationId
                                ,PG.StandardName PayRollGroupName
                                , PG.Id PayRollGroupId
                                 , ld.UserName LegalDesignation, Section.UserName Section,FORMAT(EE.EffectiveDate,'dd-MMM-yyyy')EffectiveDate
                                From ExceptionEmployee EE 
                                LEFT JOIN EmployeeInformation EI ON EE.EmpSystemId = EI.SystemId
                                LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode = PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                                LEFT JOIN HKP.Designation DSG ON PR.DesignationId = DSG.Id
                                LEFT JOIN HKP.Designation DG on DG.Id = EI.GivenDesignationId
                                LEFT JOIN ORG.Department DP on DP.Id = PR.DepartmentId
                                LEFT JOIN HKP.LegalDesignation ld on ld.Id = EI.LegalDesignationId
                                LEFT JOIN MST.payrollgroupmaster PM on PM.EmployeeId = EI.SystemId
                                LEFT JOIN hkp.payrollgroup PG on PG.Id = PM.PayRollGroupId
                                LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId = EI.GivenDesignationId
                                LEFT JOIN HKP.DesignationGroup DeG ON DeG.Id = DM.DesignationGroupId
                                LEFT JOIN[ORG].[Section] ON Section.Id = PR.SectionId
                                 WHERE EI.PlantId='" + identity.PlantId + @"' ORDER BY EE.AddedDate DESC";

            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Create(IEnumerable<ExceptionEmployeeModel> empList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            List<string> ExcEmployeeList = new List<string>();

            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;

            try
            {
                string sql = @"SELECT * FROM [dbo].[ExceptionEmployee] WHERE PlantId='" + identity.PlantId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");

                if (empList != null)
                {
                    foreach (var item in empList)
                    {
                        DataView dvExceptionEmployeeList = new DataView(dsExceptionEmployeeList.Tables[0]);
                        dvExceptionEmployeeList.RowFilter = "EmpSystemId='" + item.EmpSystemId.ToString() + "' AND PlantId='" + identity.PlantId + "'";
                        if (dvExceptionEmployeeList.Count == 0)
                        {
                            string sID = string.Empty;
                            bplib.clsGenID objGenID = new bplib.clsGenID();
                            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ExceptionEmployee", out sID);
                            DataRow dr = dsExceptionEmployeeList.Tables[0].NewRow();
                            dr["Id"] = "EX" + sID;
                            dr["EmpSystemId"] = item.EmpSystemId.ToString();
                            dr["PlantId"] = identity.PlantId;
                            dr["IsActive"] = true;
                            dr["IsForever"] = true;
                            dr["WorkDate"] = System.DateTime.Now.ToString();
                            dr["ExpirationDate"] = System.DateTime.Now.ToString();
                            dr["EffectiveDate"] = item.EffectiveDate;
                            dr["ExceptionCategory"] = "Salary Process";
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dsExceptionEmployeeList.Tables[0].Rows.Add(dr);

                        }
                        else
                        {
                            //edit
                            DataRow dr = dvExceptionEmployeeList[0].Row;

                            dr.BeginEdit();
                            dr["PlantId"] = identity.PlantId;
                            dr["EmpSystemId"] = item.EmpSystemId.ToString();
                            dr["IsActive"] = true;
                            dr["IsForever"] = true;
                            dr["WorkDate"] = System.DateTime.Now.ToString();
                            dr["ExpirationDate"] = System.DateTime.Now.ToString();
                            dr["EffectiveDate"] = item.EffectiveDate;
                            dr["ExceptionCategory"] = "Salary Process";
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dr.EndEdit();

                        }
                        dvExceptionEmployeeList.RowFilter = null;
                    }

                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsExceptionEmployeeList);
            }
            catch (Exception ex)
            {

                throw (ex);
            }


            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            // _restService.Insert(rest, identity.PlantId, restDetails);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult Delete(string EmpId)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //SELECT * FROM [dbo].[ExceptionEmployee] WHERE PlantId='' AND EmpSystemId=''
                string sql = @"Delete FROM [dbo].[ExceptionEmployee] WHERE  EmpSystemId='" + EmpId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");

            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpPost, Authorize]
        public ActionResult DeleteDetail(string id)
        {
            _restDetailsService.DeleteDetail(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations

        public class ExceptionEmployeeModel : BaseModel
        {
            public string Id { get; set; }
            public string EmpSystemId { get; set; }
            public string ExceptionCategor { get; set; }
            public string PlantId { get; set; }
            public bool IsActive { get; set; }
            public bool IsForever { get; set; }
            public string WorkDate { get; set; }
            public DateTime ExpirationDate { get; set; }
            public DateTime? EffectiveDate { get; set; }
            public string Region { get; set; }

            public string AddedBy { get; set; }

            [NeverUpdate]
            public DateTime AddedDate { get; set; }

            [NeverUpdate]
            public string AddedFromIP { get; set; }

            public string UpdatedBy { get; set; }

            public DateTime? UpdatedDate { get; set; }

            public string UpdatedFromIP { get; set; }

        }

    }
}