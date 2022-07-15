using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.HumanResource.NewAttendanceProcess;


namespace Aplos.Areas.HumanResource.Controllers
{
    public class ResidenceStatusAllocationController : Controller
    {
        ResidenceStatusLocationService rsl = new ResidenceStatusLocationService();
        private readonly ISqlRepository _sqlRepository;
        public ResidenceStatusAllocationController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        public ActionResult Aplos()
        {
            return View();
        }
        public ActionResult Report()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult getResidenceFilters()
        {
            try
            {
                var sql = @"select RM.Id ResidenceMasterId,RG.Id ResidenceGroupId,RG.UserName ResidenceGroup,P.Id PlantId,P.UserName Plant,RM.[Location],EC.Id EmployeeTypeId,EC.UserName EmployeeType
									,EST.[Service] ServiceType,RM.Rooms,RM.[Block],RM.ResidenceSubCategory,RM.[Floor],RM.ResidentType,RM.ResidenceNumber,RM.AssetName
									,VacancyStatus = 'Occupied'

									from ResidenceMaster RM
									left join ResidenceGroup RG on RG.Id=RM.ResidenceGroupId 
									left join ORG.Plant P on P.Id=RM.PlantId
									left join HKP.EmployeeCategory EC on EC.Id=RM.EmployeeCategoryId
									left join EmpServiceType EST on EST.Id=RM.EmpServiceTypeId

                union all
                select RM.Id ResidenceMasterId,RG.Id ResidenceGroupId,RG.UserName ResidenceGroup,P.Id PlantId,P.UserName Plant,RM.[Location],EC.Id EmployeeTypeId,EC.UserName EmployeeType
									,EST.[Service] ServiceType,RM.Rooms,RM.[Block],RM.ResidenceSubCategory,RM.[Floor],RM.ResidentType,RM.ResidenceNumber,RM.AssetName
									,VacancyStatus = 'All'

									from ResidenceMaster RM
									left join ResidenceGroup RG on RG.Id=RM.ResidenceGroupId 
									left join ORG.Plant P on P.Id=RM.PlantId
									left join HKP.EmployeeCategory EC on EC.Id=RM.EmployeeCategoryId
									left join EmpServiceType EST on EST.Id=RM.EmpServiceTypeId";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpGet, Authorize]
        public JsonResult getemployeeDataList(string plantId)
        {
            return Json(rsl.getemployeeDataList(plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult viewUnallocation(string PlantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(rsl.getviewUnallocation(PlantId), JsonRequestBehavior.AllowGet);
        }



        [Authorize, HttpPost]
        public ActionResult getAllEmployee(string EmpCategoryId)
        {
            try
            {
                var jsondata = Json(rsl.getAllEmployee(EmpCategoryId), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult getEmployeeCategory()
        {
            try
            {
                return Json(rsl.getEmployeeCategory(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetViewData(Dictionary<string,string> parameters)
        {
            return Json(rsl.GetViewData(parameters), JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost, Authorize]
        public ActionResult PopupEmployeeView(string fromDate, string toDate, string EmployeeCategorySystemID)
        {
            return Json(rsl.PopupEmployeeView(fromDate, toDate, EmployeeCategorySystemID), JsonRequestBehavior.AllowGet);
        }

        #region Save Operations
        [HttpPost]
        public JsonResult residenceStatusSave(List<Dictionary<string, object>> EmployeeList)
        {

            try
            {
                rsl.Save(EmployeeList);
                return Json(new { Data = EmployeeList, Message = AplosMessage.Insert });
                //return Json(new { Error = "No", Data = rsl.Save( EmployeeList, ResidenceMasterId), Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = "Yes", Msg = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        //[HttpPost]
        //public ActionResult getData()
        //{
        //    try
        //    {
        //        return Json(rsl.getData(), JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        [HttpPost]
        public ActionResult getEmployee(string PlantId, string ResidenceGroupId, string EmployeeCategoryId)
        {
            try
            {
                return Json(rsl.getEmployee(PlantId, ResidenceGroupId, EmployeeCategoryId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

       
        [HttpPost]
        public ActionResult getResidenceStatusLocation(string EmployeeId, string ResidenceMasterId)
        {
            try
            {
                return Json(rsl.getResidenceStatusLocation(EmployeeId, ResidenceMasterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /* [HttpPost]
         public JsonResult Delete(string id)
         {
             try
             {
                 rsl.delete(id);
                 return Json(new { Message = "Data deleted successfully", Error = false }, JsonRequestBehavior.AllowGet);
             }
             catch (Exception ex)
             {
                 return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
             }

         }*/

        #endregion Save Operations

        [HttpGet, Authorize]
        public ActionResult getResidenceReportFilters()
        
        {
            try
            {
                var sql = @"select ei.SystemId EmployeeId,DE.UserName Designation,ei.EmployeeName,S.UserName Section,SS.UserName SubSection,D.UserName Department
                            ,RG.UserName ResidenceGroup,RM.Id ResidenceId,RM.ResidenceNumber,RM.[Block],RM.ResidentType,RM.ResidenceSubCategory
							,E.UserName Entity

							from dbo.ResidenceAllocatedEmployees rae
                            left join dbo.EmployeeInformation ei on ei.SystemId = rae.EmployeeSystemId 
                            left join HKP.Designation DE on DE.Id=ei.DesignationSystemID
                            left join dbo.ResidenceMaster RM on RM.Id = rae.ResidenceId
                            left join dbo.ResidenceGroup RG on RG.Id = RM.ResidenceGroupId
                            left join org.Section S on S.Id = ei.SectionId
                            left join org.SubSection SS on SS.Id = ei.SubSectionId
                            left join org.Department D on D.Id = ei.DepartmentId
							left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
                            left join org.Entity E on E.Id =MPB.EntityId";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}