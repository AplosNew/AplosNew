using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Attendances;
using Library.Service.Enums;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using SetINOUT;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Attendances.Controllers
{
    public class RawDataSetInOutController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private DataSet dsRef;

        public RawDataSetInOutController(
              IMaternityLeavePolicyService LeavePolicyService,
            ISqlRepository sqlRepository
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _sqlRepository = sqlRepository;
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

        [HttpPost]
        public ActionResult Process(string pFromDate)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsSetInOut obj = new clsSetInOut();
                obj.GetHRSetting(identity.PlantId, out DataSet dsPlantSetting);
                if (bplib.clsWebLib.GetBoolData(dsPlantSetting.Tables[0].Rows[0]["ShiftBasedPunchFlag"].ToString()) == false)
                    throw new Exception("Shift based punch flag is not selected for this plant. Cannot proceed");

                DateTime FromDate = Convert.ToDateTime(pFromDate).AddDays(-1);
                DateTime ToDate = Convert.ToDateTime(pFromDate);
                while (FromDate <= ToDate)
                {
                    obj.SetRawINOUT(identity.PlantId, identity.CompanyGroupId, FromDate.ToString("dd-MMM-yyyy"), "");
                    FromDate = FromDate.AddDays(1);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return Json(new { Message = "Data Set completed!!!" }, JsonRequestBehavior.AllowGet);
        }


        #endregion -- Operations  
    }
}