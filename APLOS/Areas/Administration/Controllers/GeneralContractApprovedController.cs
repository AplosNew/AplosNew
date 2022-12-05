using Aplos.Controllers;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Library.Service.Administration.Contract;
using Aplos.Properties;
using Library.Security.Core;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.Threading;
using Library.Crosscutting.Security;
using System.IO;
using System.Data;

namespace Aplos.Areas.Administration.Controllers
{
    public class GeneralContractApprovedController : BaseController
    {
        GeneralContractCheckService gc = new GeneralContractCheckService();
        private readonly SqlRepository _sqlRepository;
        public GeneralContractApprovedController()
        {
            _sqlRepository = new SqlRepository();
        }

        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult GetcheckedApprovedData()
        {
            try
            {
                var sql = @"select  GCE.Id, FORMAT([GCE].[Date], 'dd-MMM-yyyy')[Date], E.UserName Entity, EI.EmployeeName, GC.UserName Contract,
GCE.CheckedByStatus, GCE.CheckedReason, GCE.ApprovedStatus, GCE.ApprovedReason
from TRN.GeneralContractEntry GCE
LEFT JOIN ORG.Entity E on E.Id = GCE.EntityId
LEFT JOIN MST.GeneralContract GC ON GC.Id = GCE.GeneralContractId
left join EmployeeInformation EI on EI.SystemId = GCE.ApprovedById
where GCE.CheckedByStatus = 'Checked' and GCE.ApprovedStatus = 'Approved'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region SAVE
        [HttpPost]
        public ActionResult GeneralContractAuth(string headerId, string ApprovedStataus, string AuthorizedById, string ApprovedReason)
        {
            try
            {
                gc.GeneralContractAuth(headerId, ApprovedStataus, AuthorizedById, ApprovedReason);
                return Json(new { Message = "General Contract  Approved " + AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion SAVE
    }
}