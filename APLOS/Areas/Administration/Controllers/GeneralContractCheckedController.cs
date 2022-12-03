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
    public class GeneralContractCheckedController : BaseController
    {
        GeneralContractCheckService gc = new GeneralContractCheckService();
       private readonly SqlRepository _sqlRepository ;
        #region CONSTRUCTOR
        public GeneralContractCheckedController()
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion CONSTRUCTOR

        #region Page
        public ActionResult Aplos()
        {
            return View();
        }

        
        #endregion Page

        #region GETFUNCTION
        public ActionResult GetUncheckedData()
        {
            try
            {
                var sql = @"select  GCE.Id, FORMAT([GCE].[Date], 'dd-MMM-yyyy')[Date], E.UserName Entity, EI.EmployeeName, GC.UserName Contract
from TRN.GeneralContractEntry GCE
LEFT JOIN ORG.Entity E on E.Id = GCE.EntityId
LEFT JOIN MST.GeneralContract GC ON GC.Id = GCE.GeneralContractId
left join EmployeeInformation EI on EI.SystemId = GCE.CheckBySystemId
where GCE.CheckedByStatus is null";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult GetcheckedData()
        {
            try
            {
                var sql = @"select  GCE.Id, FORMAT([GCE].[Date], 'dd-MMM-yyyy')[Date], E.UserName Entity, EI.EmployeeName, GC.UserName Contract,
GCE.CheckedByStatus, GCE.CheckedReason
from TRN.GeneralContractEntry GCE
LEFT JOIN ORG.Entity E on E.Id = GCE.EntityId
LEFT JOIN MST.GeneralContract GC ON GC.Id = GCE.GeneralContractId
left join EmployeeInformation EI on EI.SystemId = GCE.ApprovedById
where GCE.CheckedByStatus = 'Checked' and GCE.ApprovedStatus is null";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult GetAllCheckBy()
        {
            try
            {
                var sql = @"select EI.SystemId Value, EI.EmployeeName Text
                            from MST.GeneralContractApproveBy GCA
                            left join  MST.GeneralContract GC on GC.Id = GCA.GeneralContractId
                            left join EmployeeInformation EI on EI.SystemId = GCA.SystemId";



                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Authorize, HttpGet]
        public ActionResult GetChildList()
        {
            try
            {
                var sql = @"select CIE.*, GCI.UserName from TRN.ContractItemEntry CIE
left join TRN.GeneralContractEntry GCE on GCE.Id = CIE.GeneralContractEntryId
left join HKP.GeneralContractItemMaster GCI on GCI.Id = CIE.ContractMasterId";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion GETFUNCTION

        #region SAVE
        [HttpPost]
        public ActionResult GeneralContractChecked (string headerId, string CheckedStataus, string AuthorizedById, string CheckedReason)
        {
            try
            {
                gc.GeneralContractChecked(headerId, CheckedStataus, AuthorizedById, CheckedReason);
                return Json(new { Message = "General Contract  Checked " + AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion SAVE
    }


}