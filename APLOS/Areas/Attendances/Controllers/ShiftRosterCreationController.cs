using Library.Model.Employees;
using Library.Data;
using Library.Service.Employees;

using System;
using System.Web.Mvc;
using System.Linq;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using OTSBD;
using System.Data;
using System.Collections.Generic;
using Library.Service.Attendances;
//using TBS;

namespace Aplos.Areas.Attendances.Controllers
{

    public class ShiftRosterCreationController : BaseController
    {
        #region Constructor
        /// <summary>   The separationTypeService service. </summary>

        private readonly ISqlRepository _sqlRepository;
        public ShiftRosterCreationController(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        #endregion



        #region Aplos       
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations


        [HttpGet, Authorize]
        public ActionResult SearchShift()
        {          
            string sql = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsRosterInfo ob = new clsRosterInfo(_sqlRepository);
              var data=  ob.ShiftDefinationSearch(identity.CompanyGroupId, identity.PlantId);
                return Json(new { LeaveInfo = data }, JsonRequestBehavior.AllowGet);
                //return Json(new { LeaveInfo = "Saved" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }
        }

        //SelectedShiftDefination

        [HttpPost]
        public ActionResult Save(ShiftRosterMaster master, List<ShiftRosterDetail> detail)
        {

            string sql = string.Empty;
            try
            {
                string id = string.Empty;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsRosterInfo ob = new clsRosterInfo(_sqlRepository);
                master.GroupID = identity.CompanyGroupId;
                master.PlantID = identity.PlantId;
                master.AddedBy = identity.Name;
                ob.SaveData(master, detail,out id);

                //if (master.RosteringPattern == "ChangeAfterDayLength" && master.EffectiveDate != null)
                //{
                //    ShiftProcess sp = new ShiftProcess();
                //    sp.ProcessRosterSpecific(id, master.EffectiveDate, DateTime.Now, identity.PlantId, identity.CompanyGroupId);
                //}
               

                //return Json(new { LeaveInfo = data }, JsonRequestBehavior.AllowGet);
                return Json(new { Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }
        }

        [HttpGet, Authorize]
        public ActionResult LoadRoster()
        {
            string sql = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsRosterInfo ob = new clsRosterInfo(_sqlRepository);
                var data = ob.RosterLoad(identity.PlantId);
                return Json(new { Roster = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }
        }

        [HttpGet, Authorize]
        public ActionResult LoadRosterChild(string rosterid)
        {
            string sql = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsRosterInfo ob = new clsRosterInfo(_sqlRepository);
                var data = ob.RosterChildLoad(rosterid,identity.PlantId);
                return Json(new { RosterChild = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }
        }

        #endregion
    }
}