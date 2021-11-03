#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.OrderManagement.ShipmentControl;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class OrderControlController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public OrderControlController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        ShipmentControl control = new ShipmentControl();

        
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetControlTypeCbo()
        {
            try
            {
                return Json(control.GetControlTypeCbo(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        [HttpGet, Authorize]
        public JsonResult GetSalesOrderData(int day,int lagdays, string level)
        {
            try
            {
                return Json(control.GetSalesOrderData(day, lagdays, level), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        [HttpGet, Authorize]
        public JsonResult GetProductoinOrderData(int day, int lagdays, string level)
        {
            try
            {
                return Json(control.GetProductoinOrderData(day, lagdays, level), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        [HttpGet, Authorize]
        public JsonResult GetRemarksByMaster(string OrderControlId)
        {
            try
            {
                return Json(control.GetRemarksByMaster(OrderControlId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        [HttpPost, Authorize]
        public JsonResult CreateMaster(List<ShipmentControlModel> data)
        {
            try
            {
                CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                IdentityParameter para = new IdentityParameter
                {
                    AddedBy = identity.Name,
                    AddedDate = DateTime.Now,
                    AddedFromIP = identity.IPAddress,
                    UpdatedBy = identity.Name,
                    UpdatedDate = DateTime.Now,
                    UpdatedFromIP = identity.IPAddress
                };

                control.SaveMasterData(data,para);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }

        [HttpPost]
        public JsonResult Create(ShipmentControlModel data, OrderControlRemarks child)
        {
            try
            {
                CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                data.AddedBy = identity.Name;
                data.AddedDate = DateTime.Now;
                data.AddedFromIP = identity.IPAddress;
                data.UpdatedBy = identity.Name;
                data.UpdatedDate = DateTime.Now;
                data.UpdatedFromIP = identity.IPAddress;

                control.SaveData(data, child);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            control.DeleteData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

       
    }

   

}