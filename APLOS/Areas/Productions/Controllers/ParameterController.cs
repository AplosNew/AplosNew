using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aplos.Controllers;
using Library.Data.Sql;
using Library.HumanResource.Parameter;
using Aplos.Properties;
using System.Data;

namespace Aplos.Areas.Productions.Controllers
{
    public class ParameterController : BaseController
    {
        ParameterService ps = new ParameterService();
        ParameterChild pc = new ParameterChild();
        public ActionResult Aplos()
        {
            return View();
        }
        #region HEADER
        [HttpGet, Authorize]
        public ActionResult GetResponsiblePerson()
        {
            try
            {
                return Json(pc.getResponsiblePerson(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #region Save
        [HttpPost]
        public ActionResult Save(Dictionary<string, object> datas)
        {
            try
            {
                return Json(new { Error = false, Data = pc.Save(datas), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CreateParameter(string headerid, Dictionary<string, object> parameter)
        {
            try
            {
                return Json(new { Error = false, Data = pc.CreateParameter(headerid, parameter), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CreateProductWithParameterSetup(string headerid, List<Dictionary<string, object>> models)
        {
            try
            {
                return Json(new { Error = false, Data = pc.CreateProductWithParameterSetup(headerid, models), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CreateWorkcenterWithParameterSetup(string headerid, List<Dictionary<string, object>> models)
        {
            try
            {
                return Json(new { Error = false, Data = pc.CreateWorkcenterWithParameterSetup(headerid, models), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion  Save

        #endregion HEADER

        #region GetFun

        [HttpGet, Authorize]
        public ActionResult GetProduct()
        {
            return Json(pc.getProduct(), JsonRequestBehavior.AllowGet);
        }        

        [HttpGet, Authorize]
        public ActionResult GetWorkcenter(string paramEntityId)
        {
            return Json(pc.getWorkcenter(paramEntityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetParameterMaster()
        {
            return Json(ps.GetParameter(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetMachineMaster()
        {
            return Json(ps.GetMachineMaster(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetList()
        {
            return Json(pc.GetList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedProduct(string headerid)
        {
            return Json(pc.GetSavedProduct(headerid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedWorkcenter(string headerid)
        {
            return Json(pc.GetSavedWorkcenter(headerid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedParameterChild(string headerid)
        {
            return Json(pc.GetSavedParameterChild(headerid), JsonRequestBehavior.AllowGet);
        }

        #endregion GetFun

        #region Update
        [HttpPost]
        public ActionResult Update(Dictionary<string, object> data)
        {
            try
            {
                return Json(new { Error = false, Data = pc.Update(data), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion Update

        #region DELETE
        public ActionResult RemoveProduct(string productid)
        {
            try
            {

                string ret = pc.RemoveProduct(productid);

                if (ret == "Success")
                {
                    return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Error = true, Message = ret }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }

        public ActionResult RemoveWorkcenter(string workcenterid)
        {
            try
            {

                string ret = pc.RemoveProduct(workcenterid);

                if (ret == "Success")
                {
                    return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Error = true, Message = ret }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }

        public ActionResult RemoveParameterRow(string parameterid)
        {
            try
            {

                string ret = pc.RemoveParameterRow(parameterid);

                if (ret == "Success")
                {
                    return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Error = true, Message = ret }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }
        #endregion DELETE

        #region Entity
        [HttpGet, Authorize]
        public ActionResult GetEntity()
        {
            return Json(pc.GetEntity(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CreateEntityWithParameterSetup(string headerid, List<Dictionary<string, object>> models)
        {
            try
            {
                return Json(new { Error = false, Data = pc.CreateEntityWithParameterSetup(headerid, models), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedEntity(string headerid)
        {
            return Json(pc.GetSavedEntity(headerid), JsonRequestBehavior.AllowGet);
        }

        public ActionResult RemoveEntity(string entityid)
        {
            try
            {

                string ret = pc.RemoveEntityRow(entityid);

                if (ret == "Success")
                {
                    return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Error = true, Message = ret }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }
        public ActionResult GetParameterEntity(string headerid)
        {
            return Json(pc.GetParameterEntity(headerid), JsonRequestBehavior.AllowGet);
        }
        #endregion Entity

        #region Process
        [HttpGet, Authorize]
        public ActionResult GetProcess()
        {
            return Json(pc.GetProcess(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CreateProcessWithParameterSetup(string headerid, List<Dictionary<string, object>> models)
        {
            try
            {
                return Json(new { Error = false, Data = pc.CreateProcessWithParameterSetup(headerid, models), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedProcess(string headerid)
        {
            return Json(pc.GetSavedProcess(headerid), JsonRequestBehavior.AllowGet);
        }

        public ActionResult RemoveProcess(string processid)
        {
            try
            {

                string ret = pc.RemoveProcessRow(processid);

                if (ret == "Success")
                {
                    return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Error = true, Message = ret }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }
        #endregion Process

        #region Machine
        [HttpGet, Authorize]
        public ActionResult GetMachine()
        {
            return Json(pc.GetMachine(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CreateMachineWithParameterSetup(string headerid, List<Dictionary<string, object>> models)
        {
            try
            {
                return Json(new { Error = false, Data = pc.CreateMachineWithParameterSetup(headerid, models), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedMachine(string headerid)
        {
            return Json(pc.GetSavedMachine(headerid), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult RemoveMachine(string machineid)
        {
            try
            {

                string ret = pc.RemoveMachineRow(machineid);

                if (ret == "Success")
                {
                    return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Error = true, Message = ret }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }
        #endregion Machine

        

        #region QualityProcess
        [HttpPost]
        public ActionResult QPSave(Dictionary<string, object> data, string headerId)
        {
            try
            {
                return Json(new { Error = false, Data = pc.SaveQP(data, headerId), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetQualityProcess(string headerid)
        {
            return Json(pc.GetQualityProcess(headerid), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult RemoveQualityPeocess(string Id)
        {
            try
            {

                string ret = pc.RemoveQualityPeocess(Id);

                if (ret == "Success")
                {
                    return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Error = true, Message = ret }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }
        #endregion QualityProcess
    }
}