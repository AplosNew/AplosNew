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
    public class ResidenceStatusLocationController : Controller
    {
        ResidenceStatusLocationService rsl = new ResidenceStatusLocationService();
        private readonly ISqlRepository _sqlRepository;
        public ResidenceStatusLocationController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        public ActionResult Aplos()
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
        public JsonResult getemployeeDelete()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(rsl.getemployeeDelete(identity.PlantId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public ActionResult getPlant(string ResidenceGroupId)
        {
            return Json(rsl.getPlant(ResidenceGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getLocation(string PlantId, string ResidenceGroupId)
        {
            return Json(rsl.getLocation(PlantId, ResidenceGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getResidenceGroup()
        {
            return Json(rsl.getResidenceGroup(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getServiceType(string PlantId, string ResidenceGroupId)
        {
            return Json(rsl.getServiceType(PlantId, ResidenceGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getResidenceSubCategory(string PlantId, string ResidenceGroupId)
        {
            return Json(rsl.getResidenceSubCategory(PlantId, ResidenceGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getBlock(string PlantId, string ResidenceGroupId)
        {
            return Json(rsl.getBlock(PlantId, ResidenceGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getRoom(string PlantId, string ResidenceGroupId)
        {
            return Json(rsl.getRoom(PlantId, ResidenceGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getEmployeeType(string PlantId, string ResidenceGroupId)
        {
            return Json(rsl.getEmployeeType(PlantId, ResidenceGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getResidenceNumber(string PlantId, string ResidenceGroupId)
        {
            return Json(rsl.getResidenceNumber(PlantId, ResidenceGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getFloor(string PlantId, string ResidenceGroupId)
        {
            return Json(rsl.getFloor(PlantId, ResidenceGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getResidentType(string PlantId, string ResidenceGroupId)
        {
            return Json(rsl.getResidentType(PlantId, ResidenceGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getAssetName(string PlantId, string ResidenceGroupId)
        {
            return Json(rsl.getAssetName(PlantId, ResidenceGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getVacancy(string PlantId, string ResidenceGroupId)
        {
            return Json(rsl.getVacancy(PlantId, ResidenceGroupId), JsonRequestBehavior.AllowGet);
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
        public ActionResult view(Dictionary<string,string> parameters)
        {
            return Json(rsl.view(parameters), JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost, Authorize]
        public ActionResult PopupEmployeeView(string fromDate, string toDate, string EmployeeCategorySystemID)
        {
            return Json(rsl.PopupEmployeeView(fromDate, toDate, EmployeeCategorySystemID), JsonRequestBehavior.AllowGet);
        }

        #region Save Operations
        [HttpPost]
        public JsonResult residenceStatusSave(List<Dictionary<string, object>> EmployeeList, string ResidenceMasterId)
        {

            try
            {
                rsl.Save(EmployeeList, ResidenceMasterId);
                return Json(new { Data = EmployeeList, Message = AplosMessage.Insert });
                //return Json(new { Error = "No", Data = rsl.Save( EmployeeList, ResidenceMasterId), Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = "Yes", Msg = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult getData()
        {
            try
            {
                return Json(rsl.getData(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

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
        public ActionResult getSelectedEmployees(List<Dictionary<string, object>> EmpList)
        {
            try
            {
                return Json(rsl.getSelectedEmployees(EmpList), JsonRequestBehavior.AllowGet);
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
    }
}