#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Library.HumanResource.NewOTProcess;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class ProcessWiseProductionBookingController : Controller
    {
        ProcessWiseProductionBookingService pwp = new ProcessWiseProductionBookingService();
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public ProcessWiseProductionBookingController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpPost]
        public ActionResult getEntity()
        {
            try{
                return Json(pwp.getEntity(), JsonRequestBehavior.AllowGet);

            }
            catch(Exception ex){throw ex;}
        }

        [Authorize, HttpPost]
        public ActionResult getDepartment()
        {
            try {
                return Json(pwp.getDepartment(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { throw ex;}
        }

        [Authorize, HttpPost]
        public ActionResult getShift()
        {
           try{
                return Json(pwp.getShift(), JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex){throw ex;}
        }


        [Authorize, HttpPost]
        public ActionResult getMachine()
        {
            try{
                return Json(pwp.getMachine(), JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex){throw ex;}
        }

       
        [Authorize, HttpPost]
        public ActionResult getProcess(string entityId)
        {
            try
            {
                return Json(pwp.getProcess(entityId), JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex){throw ex;}
        }

        [Authorize, HttpPost]
        public ActionResult getEmployee()
        {
           try{
                return Json(pwp.getEmployee(), JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex){throw ex;}
        }

        [Authorize, HttpPost]
        public ActionResult getArticle()
        {
            try
            {
                return Json(pwp.getArticle(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { throw ex; }
        }

        [HttpPost]
        public JsonResult Save(Dictionary<string, object> data, string responsiblepersonId)
        {
           try{
                var datas = pwp.Save(data, responsiblepersonId);
                return Json(new { Error = false, Data = datas, Message = AplosMessage.Updated });
            }
            catch(Exception ex){throw ex;}

        }

       
        
        public JsonResult SaveChild(List<Dictionary<string, object>> workcenterlist, string headerId)
        {
           try{
                var data = pwp.SaveChild(workcenterlist, headerId);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch(Exception ex){throw ex;}
        }

        [Authorize, HttpGet]
        public JsonResult GetMachineMasterTransaction()
        {
           try{
                return Json(pwp.GetMachineMasterTransaction(), JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex){throw ex;}
        }
        //Omar End
        [HttpPost, Authorize]
        public JsonResult GetWCCbo(string processId, string entityId)
        {

            try {
                return Json(pwp.GetWCCbo(processId, entityId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { throw ex; }

        }


        public ActionResult Delete(string id)
        {
           try{
                var data = pwp.Delete(id);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch(Exception ex){throw ex;}
        }


       
        #region commented
        //[Authorize, HttpGet]
        //public JsonResult GetEmployeeListByWhom(GridParameter parameters, string plantId, string partyAccountGroupId, string partyId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    if (string.IsNullOrEmpty(plantId))
        //    {
        //        plantId = identity.PlantId;
        //    }
        //    return Json(GetEmployeeListByWhom(parameters, identity.CompanyId, plantId, partyAccountGroupId, partyId), JsonRequestBehavior.AllowGet);
        //}

        //public GridModel GetEmployeeListByWhom(GridParameter parameters, string companyId, string plantId, string partyAccountGroupId, string partyId)
        //{
        //    try
        //    {
        //        parameters.CmdText = @"SELECT EI.SystemId, MB.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
        //                            , EI.EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [Designation], MB.EntityId
        //                            , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType
        //                    FROM dbo.EmployeeInformation AS EI
        //                    LEFT JOIN HKP.Designation AS DEG ON DEG.Id=EI.GivenDesignationID
        //                    LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId
        //                    LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
        //                    LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
        //                    WHERE EI.CompanyId='" + companyId + "' AND EI.PlantId='" + plantId + "' AND EI.EmployeeStatus='Active'";

        //        return _sqlRepository.GetGridData(parameters);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
        //    }
        //}

        #endregion commented

    }


}
