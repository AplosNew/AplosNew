#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Setups;
using Library.Service.Core;
using Library.Service.Setups;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
    public class LocalLanguageController : BaseController
    {
        #region Constructor

        private readonly ILocalLanguageService _localLanguageService;
        private readonly ISalaryHeadService _salaryHeadService;

        public LocalLanguageController(ILocalLanguageService LocalLanguageService, ISalaryHeadService salaryHeadService)
        {
            _localLanguageService = LocalLanguageService;
            _salaryHeadService = salaryHeadService;
        }

        #endregion Constructor

        /// <summary>
        /// This list for line local language
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="lineId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetLineLanguageList(GridParameter parameters, string lineId)
        {
            return Json(_localLanguageService.QueryByLine(parameters, lineId), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// This list for subSection local language
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="subSectionId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetSubSectionLanguageList(GridParameter parameters, string subSectionId)
        {
            return Json(_localLanguageService.QueryBySubSection(parameters, subSectionId), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// This list for section local language
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="sectionId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetSectionLanguageList(GridParameter parameters, string sectionId)
        {
            return Json(_localLanguageService.QueryBySection(parameters, sectionId), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// This list for subDivision local language
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="subDivisionId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetSubDivisionLanguageList(GridParameter parameters, string subDivisionId)
        {
            return Json(_localLanguageService.QueryBySubDivision(parameters, subDivisionId), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// This list for division local language
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="divisionId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetDivisionLanguageList(GridParameter parameters, string divisionId)
        {
            return Json(_localLanguageService.QueryByDivision(parameters, divisionId), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// This list for unit local language
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="unitId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetUnitLanguageList(GridParameter parameters, string unitId)
        {
            return Json(_localLanguageService.QueryByUnit(parameters, unitId), JsonRequestBehavior.AllowGet);
        }

		/// <summary>
		/// This list for plant local language
		/// </summary>
		/// <param name="parameters"></param>
		/// <param name="plantId"></param>
		/// <returns></returns>
		[HttpGet]
        public JsonResult GetPlantLanguageList(GridParameter parameters, string plantId)
        {
            return Json(_localLanguageService.QueryByPlant(parameters, plantId), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// This list for company local language
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetCompanyLanguageList(GridParameter parameters, string companyId)
        {
            return Json(_localLanguageService.QueryByCompany(parameters, companyId), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// This list for company group local language
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="companyGroupId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetCompanyGroupLanguageList(GridParameter parameters, string companyGroupId)
        {
            return Json(_localLanguageService.QueryByCompanyGroup(parameters, companyGroupId), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// This list for department local language
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="departmentId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetDepartmentLanguageList(GridParameter parameters, string departmentId)
        {
            return Json(_localLanguageService.QueryByDepartment(parameters, departmentId), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// This list for designation local language
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="designationId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetDesignationLanguageList(GridParameter parameters, string designationId)
        {
            return Json(_localLanguageService.QueryByDesignation(parameters, designationId), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// This list for designation group local language
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="designationGroupId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetDesignationGroupLanguageList(GridParameter parameters, string designationGroupId)
        {
            return Json(_localLanguageService.QueryByDesignationGroup(parameters, designationGroupId), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// This list for legal designation local language
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="designationGroupId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetLegalDesignationLanguageList(GridParameter parameters, string legalDesignationId)
        {
            return Json(_localLanguageService.QueryByLegalDesignation(parameters, legalDesignationId), JsonRequestBehavior.AllowGet);
        }

		/// <summary>
		/// This list for continent local language
		/// </summary>
		/// <param name="parameters"></param>
		/// <param name="continentId"></param>
		/// <returns></returns>
		[HttpGet]
		public JsonResult GetContinentLanguageList(GridParameter parameters, string continentId)
		{
			return Json(_localLanguageService.QueryByContinent(parameters, continentId), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// This list for country local language
		/// </summary>
		/// <param name="parameters"></param>
		/// <param name="countryId"></param>
		/// <returns></returns>
		[HttpGet]
		public JsonResult GetCountryLanguageList(GridParameter parameters, string countryId)
		{
			return Json(_localLanguageService.QueryByCountry(parameters, countryId), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// This list for state local language
		/// </summary>
		/// <param name="parameters"></param>
		/// <param name="stateId"></param>
		/// <returns></returns>
		[HttpGet]
		public JsonResult GetStateLanguageList(GridParameter parameters, string stateId)
		{
			return Json(_localLanguageService.QueryByState(parameters, stateId), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// This list for district local language
		/// </summary>
		/// <param name="parameters"></param>
		/// <param name="districtId"></param>
		/// <returns></returns>
		[HttpGet]
		public JsonResult GetDistrictLanguageList(GridParameter parameters, string districtd)
		{
			return Json(_localLanguageService.QueryByDistrict(parameters, districtd), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// This list for city local language
		/// </summary>
		/// <param name="parameters"></param>
		/// <param name="cityId"></param>
		/// <returns></returns>
		[HttpGet]
		public JsonResult GetCityLanguageList(GridParameter parameters, string cityId)
		{
			return Json(_localLanguageService.QueryByCity(parameters, cityId), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// This list for area local language
		/// </summary>
		/// <param name="parameters"></param>
		/// <param name="areaId"></param>
		/// <returns></returns>
		[HttpGet]
		public JsonResult GetAreaLanguageList(GridParameter parameters, string areaId)
		{
			return Json(_localLanguageService.QueryByArea(parameters, areaId), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// This list for police station local language
		/// </summary>
		/// <param name="parameters"></param>
		/// <param name="postOfficeId"></param>
		/// <returns></returns>
		[HttpGet]
		public JsonResult GetPoliceStationLanguageList(GridParameter parameters, string policeStationId)
		{
			return Json(_localLanguageService.QueryByPoliceStation(parameters, policeStationId), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// This list for post office local language
		/// </summary>
		/// <param name="parameters"></param>
		/// <param name="postOfficeId"></param>
		/// <returns></returns>
		[HttpGet]
		public JsonResult GetPostOfficeLanguageList(GridParameter parameters, string postOfficeId)
		{
			return Json(_localLanguageService.QueryByPostOffice(parameters, postOfficeId), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// This list for civil status local language
		/// </summary>
		/// <param name="parameters"></param>
		/// <param name="civilStatusId"></param>
		/// <returns></returns>
		[HttpGet]
		public JsonResult GetCivilStatusLanguageList(GridParameter parameters, string civilStatusId)
		{
			return Json(_localLanguageService.QueryByCivilStatus(parameters, civilStatusId), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// This list for religion local language
		/// </summary>
		/// <param name="parameters"></param>
		/// <param name="religionId"></param>
		/// <returns></returns>
		[HttpGet]
		public JsonResult GetReligionLanguageList(GridParameter parameters, string religionId)
		{
			return Json(_localLanguageService.QueryByReligion(parameters, religionId), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// This list for qualification label info local language
		/// </summary>
		/// <param name="parameters"></param>
		/// <param name="qualificationLabelInfoId"></param>
		/// <returns></returns>
		[HttpGet]
		public JsonResult GetQualificationLabelInfoLanguageList(GridParameter parameters, string qualificationLabelInfoId)
		{
			return Json(_localLanguageService.QueryByQualificationLabelInfo(parameters, qualificationLabelInfoId), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// This list for employee category local language
		/// </summary>
		/// <param name="parameters"></param>
		/// <param name="employeeCategoryId"></param>
		/// <returns></returns>
		[HttpGet]
		public JsonResult GetEmployeeCategoryLanguageList(GridParameter parameters, string employeeCategoryId)
		{
			return Json(_localLanguageService.QueryByEmployeeCategory(parameters, employeeCategoryId), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// This list for relationship local language
		/// </summary>
		/// <param name="parameters"></param>
		/// <param name="relationshipId"></param>
		/// <returns></returns>
		[HttpGet]
		public JsonResult GetRelationshipLanguageList(GridParameter parameters, string relationshipId)
		{
			return Json(_localLanguageService.QueryByRelationship(parameters, relationshipId), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// This list for profession local language
		/// </summary>
		/// <param name="parameters"></param>
		/// <param name="professionId"></param>
		/// <returns></returns>
		[HttpGet]
		public JsonResult GetProfessionLanguageList(GridParameter parameters, string professionId)
		{
			return Json(_localLanguageService.QueryByProfession(parameters, professionId), JsonRequestBehavior.AllowGet);
		}

		#region Label

		[HttpGet, Authorize]
        public ActionResult Label()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetLabelList(GridParameter parameters,string languageId)
        {
            return Json(_localLanguageService.QueryLabel(parameters, languageId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateLabel(LocalLanguage localLanguage)
        {
            _localLanguageService.Insert(localLanguage);
            return Json(new { LocalLanguage = localLanguage, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult EditLabel(LocalLanguage localLanguage)
        {
            _localLanguageService.Update(localLanguage);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult DeleteLabel(string id)
        {
            _localLanguageService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion Label

        #region Salary Head

        [HttpGet, Authorize]
        public ActionResult Aplos()
        {
            return View();
        }


        [HttpGet]
        public ActionResult GetSalaryHeadList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var data = _salaryHeadService.GetSalaryHeadQuery();
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult GetLeaveTypeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var data = _salaryHeadService.GetLeaveTypeQuery();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetLanguageTypeList(string LanguageId,string flag)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var data = _salaryHeadService.GetSalaryHeadQueryWithLocalLanguage(LanguageId, flag);
            return Json(data, JsonRequestBehavior.AllowGet);  
        }
     
        [HttpGet]
        public ActionResult GetLeaveList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_salaryHeadService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetSalaryHeadLanguageList(GridParameter parameters,string salaryHeadId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_localLanguageService.QueryBySalaryHead(parameters, salaryHeadId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult CreateSalaryHead(IEnumerable<LocalLanguage> localLanguage )
        {
            
            _localLanguageService.SaveLocalLanguage(localLanguage);
            return Json(new { LocalLanguage = localLanguage, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult EditSalaryHead(LocalLanguage localLanguage)
        {
            
            _localLanguageService.Update(localLanguage);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult DeleteSalaryHead(string id)
        {
            _localLanguageService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion Salary Head
    }
}