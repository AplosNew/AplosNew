#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Organizations;
using Library.Security.Core;
using Library.Service.Organizations;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Organizations.Controllers
{
    public class ManpowerBudgetController : BaseController
    {
        #region Constructor

        private readonly IManpowerBudgetService _manpowerBudgetService;
        private readonly IManpowerBudgetJobDescriptionService _manpowerBudgetJobDescriptionService;
        private readonly IManpowerBudgetResponsiblePersonService _manpowerBudgetResponsiblePersonService;
        private readonly IOrganizationReportService _organizationReportService;
        ISqlRepository _sqlRepository;

        public ManpowerBudgetController(
            IManpowerBudgetService manpowerBudgetService
            , IManpowerBudgetJobDescriptionService manpowerBudgetJobDescriptionService
            , IManpowerBudgetResponsiblePersonService manpowerBudgetResponsiblePersonService
            , IOrganizationReportService organizationReportService
            )
        {
            _manpowerBudgetService = manpowerBudgetService;
            _organizationReportService = organizationReportService;
            _manpowerBudgetJobDescriptionService = manpowerBudgetJobDescriptionService;
            _manpowerBudgetResponsiblePersonService = manpowerBudgetResponsiblePersonService;
            _sqlRepository = new SqlRepository();
        }

        #endregion Constructor

        #region ManpowerBudgetResponsiblePerson

        #region BudgetMaster

        [HttpGet, Authorize]
        public ActionResult BudgetMaster()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult BudgetMasterResponsiblePerson(GridParameter parameters, string manpowerBudgetId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_manpowerBudgetResponsiblePersonService.QueryBudgetMaster(parameters, identity.CompanyGroupId, manpowerBudgetId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult SaveBudgetMaster(ManpowerBudgetResponsiblePerson entity)
        {
            _manpowerBudgetResponsiblePersonService.SaveBudgetMaster(entity);
            return Json(new { Message = AplosMessage.Success });
        }

        #endregion BudgetMaster

        #region BudgetMasterActivity

        [HttpGet, Authorize]
        public ActionResult BudgetMasterActivity()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult BudgetMasterActivityResponsiblePerson(GridParameter parameters, string manpowerBudgetId, string budgetMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_manpowerBudgetResponsiblePersonService.QueryBudgetMasterActivity(parameters, identity.CompanyGroupId, manpowerBudgetId, budgetMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult SaveBudgetMasterActivity(ManpowerBudgetResponsiblePerson entity)
        {
            _manpowerBudgetResponsiblePersonService.SaveBudgetMasterActivity(entity);
            return Json(new { Message = AplosMessage.Success });
        }

        #endregion BudgetMasterActivity

        #endregion ManpowerBudgetResponsiblePerson

        [HttpGet, Authorize]
        public JsonResult GetManpowerBudgetById(string id)
        {
            return Json(_manpowerBudgetService.GetManpowerBudgetById(id), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetManpowerBudgetRelationChainById(string companyId, string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_manpowerBudgetService.GetManpowerBudgetRelationChainById(identity.CompanyGroupId, companyId, id), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCboCodeList()
        {
            return Json(new SelectList(_manpowerBudgetService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboList(string companyId)
        {
            return Json(new SelectList(_manpowerBudgetService.GetCbo(companyId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboByCompanyAndPlant(string companyId, string plantId)
        {
            return Json(_manpowerBudgetService.GetCbo(companyId, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetManpowerBudget(string manPowerBudgetMasterId)
        {
            return Json(_manpowerBudgetService.GetManpowerBudget(manPowerBudgetMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult Aplos()
        {
            // For testing.
            //var test = _manpowerBudgetService.GetManpowerBudgetByIdNew("CG20181", "C20181", "20188", "1253");
            return View();
        }

        [HttpPost]
        public JsonResult Create(ManpowerBudget manpowerBudget
            , IEnumerable<ManpowerBudgetJobDescription> manpowerBudgetJobDescription
            , IEnumerable<ManpowerBudgetDetail> manpowerBudgetDetailList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            manpowerBudget.CompanyGroupId = identity.CompanyGroupId;
            _manpowerBudgetService.Insert(manpowerBudget, manpowerBudgetJobDescription, manpowerBudgetDetailList);
            return Json(new { ManPowerBudgetMaster = manpowerBudget, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(ManpowerBudget manpowerBudget
            , IEnumerable<ManpowerBudgetJobDescription> manpowerBudgetJobDescription
            , IEnumerable<ManpowerBudgetDetail> manpowerBudgetDetailList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            manpowerBudget.CompanyGroupId = identity.CompanyGroupId;
            _manpowerBudgetService.Update(manpowerBudget, manpowerBudgetJobDescription, manpowerBudgetDetailList);
            return Json(new { ManPowerBudgetMaster = manpowerBudget, Message = AplosMessage.Updated });
        }

        #region Allowance

        [HttpGet, Authorize]
        public ActionResult Allowance()
        {
            return View();
        }

        [HttpPost]
        public JsonResult CreateAllowance(ManpowerBudgetAllowance allowance, decimal rate)
        {
            allowance.MinimumSalary = allowance.MinimumSalary * rate;
            allowance.MaximumSalary = allowance.MaximumSalary * rate;
            allowance.SkillAllowance = allowance.SkillAllowance * rate;
            allowance.ResponsibilityAllowance = allowance.ResponsibilityAllowance * rate;
            _manpowerBudgetService.InsertAllowance(allowance);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult EditAllowance(ManpowerBudgetAllowance allowance, decimal rate)
        {
            allowance.MinimumSalary = allowance.MinimumSalary * rate;
            allowance.MaximumSalary = allowance.MaximumSalary * rate;
            allowance.SkillAllowance = allowance.SkillAllowance * rate;
            allowance.ResponsibilityAllowance = allowance.ResponsibilityAllowance * rate;
            _manpowerBudgetService.UpdateAllowance(allowance);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpGet]
        public JsonResult QueryAllowance(GridParameter parameters, string manpowerBudgetId)
        {
            return Json(_manpowerBudgetService.QueryAllowance(manpowerBudgetId, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAllowance(string id)
        {
            return Json(_manpowerBudgetService.GetAllowance(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteAllowance(string id)
        {
            _manpowerBudgetService.DeleteAllowance(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion Allowance

        #region Detail

        [HttpPost]
        public JsonResult CreateDetail(ManpowerBudgetDetail manpowerBudgetDetail)
        {
            _manpowerBudgetService.InsertDetail(manpowerBudgetDetail);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult EditDetail(ManpowerBudgetDetail manpowerBudgetDetail)
        {
            _manpowerBudgetService.UpdateDetail(manpowerBudgetDetail);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpGet]
        public JsonResult QueryDetail(GridParameter parameters, string manpowerBudgetId)
        {
            return Json(_manpowerBudgetService.QueryDetail(manpowerBudgetId, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetDetail(string id)
        {
            return Json(_manpowerBudgetService.GetDetail(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteDetail(string id)
        {
            _manpowerBudgetService.DeleteDetail(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion Detail

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _manpowerBudgetService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_manpowerBudgetService.Query(parameters, identity.CompanyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// use : Recruitment Planning, Work Center
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="plantId"></param>
        /// <returns></returns>
        [HttpGet, Authorize]
        public ActionResult GetListByPlant(GridParameter parameters, string plantId)
        {
            return Json(_manpowerBudgetService.QueryByPlant(parameters, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListByEntity(GridParameter parameters, string entityId)
        {
            return Json(_manpowerBudgetService.QueryByEntity(parameters, entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetManpowerBudgetList(GridParameter parameters, string manpowerBudgetId)
        {
            return Json(_manpowerBudgetJobDescriptionService.Query(parameters, manpowerBudgetId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult SearchManpowerBudget(GridParameter parameters, string companyId)
        {
            return Json(_manpowerBudgetService.SearchManpowerBudget(parameters, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetForResponsiblePerson(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_manpowerBudgetService.GetForResponsiblePersonByCompanyGroup(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetForResponsiblePersonByCompany(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_manpowerBudgetService.GetForResponsiblePersonByCompany(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult ManpowerBudgetReport(string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "Manpower Budget Report.xlsx";
            var workbook = _organizationReportService.GetManpowerBudget(identity.CompanyGroupId, companyId);
            workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

        [HttpGet, Authorize]
        public ActionResult GetAttendanceGroup()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;                
                return Json(GetAttdnGroup(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        public IEnumerable<object> GetAttdnGroup()
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"select Id,UserName  from AttendanceGroup";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        [HttpGet, Authorize]
        public ActionResult GetCostCenterCbo(string CompanyId, string EntityId)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"Select CC.Id AS [Value],CC.UserName AS [Text] from [ORG].[EntityCostCenter] EC
                        LEFT JOIN [ORG].[CostCenter] CC ON CC.Id=EC.CostCenterId
                        WHERE EC.CompanyId='" + CompanyId + "' AND EC.EntityId='" + EntityId + "'";

                return Json(_sqlRepository.GetDataCollection(strSQL), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        [HttpPost, Authorize]
        public JsonResult CreateAdditional(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from MST.ManpowerBudgetAdditionalPlan where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";


                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ManpowerBudgetAdditionalPlan", out _Id);

                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);


                return Json(new { Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateKPIResponsible(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [MST].[ManpowerBudgetKPIResponsible] where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";


                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ManpowerBudgetKPIResponsible", out _Id);

                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);


                return Json(new { Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedAdditionalPlanData(string masterId)
        {
            JsonResult json = Json(GetSavedAdditionalPlanDataByMaster(masterId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedKPIResponsibleData(string masterId)
        {
            JsonResult json = Json(GetSavedKPIResponsibleDataByMaster(masterId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        public IEnumerable<object> GetSavedAdditionalPlanDataByMaster(string masterId)
        {
            try
            {
                string CmdText = @"SELECT A.*,FORMAT(A.FromDate,'dd-MMM-yyyy')FD,FORMAT(A.ToDate,'dd-MMM-yyyy')TD FROM [MST].[ManpowerBudgetAdditionalPlan] A WHERE A.ManpowerBudgetId='" + masterId + "'";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSavedKPIResponsibleDataByMaster(string masterId)
        {
            try
            {
                string CmdText = @"SELECT A.*,FORMAT(A.EffectiveDate,'dd-MMM-yyyy') as EDate,R.EmployeeName as ResponsiblePerson, R.EmployeeName as TeamLeader FROM [MST].[ManpowerBudgetKPIResponsible] A 
left outer join EmployeeInformation R ON R.SystemId=A.ResponsiblePersonId
left outer join EmployeeInformation T ON T.SystemId=A.TeamLeaderId
WHERE A.ManpowerBudgetId='" + masterId + "'";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }


            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }

        [Authorize, HttpPost]
        public ActionResult GetEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT EI.SystemId as SystemId, MB.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=MB.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=p.DepartmentId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=p.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=p.SubSectionId
                            WHERE EI.EmployeeStatus='Active'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetTeamLeader()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT EI.SystemId as SystemId, MB.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=MB.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=p.DepartmentId
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=p.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=p.SubSectionId
                            WHERE EI.EmployeeStatus='Active'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }
    }
}