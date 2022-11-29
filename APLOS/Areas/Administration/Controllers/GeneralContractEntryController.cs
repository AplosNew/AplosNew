using Aplos.Controllers;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Library.Service.Administration.Contract;
using Aplos.Properties;

namespace Aplos.Areas.Administration.Controllers
{
    public class GeneralContractEntryController : BaseController
    {
        private readonly SqlRepository _sqlRepository;
        ContractEntryService ce = new ContractEntryService();
        public GeneralContractEntryController()
        {
            _sqlRepository = new SqlRepository();
        }
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public ActionResult GetContract()
        {
            try
            {
                var sql = @"select Id Value, UserName Text from MST.GeneralContract";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult GetEntity()
        {
            try
            {
                var sql = @"select Id Value, UserName Text from ORG.Entity where Active = 1";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult GetAllContractItem(string headerId)
        {
            try
            {
                var sql = @"select CID.*, CID.ContractMasterId, GCIM.UserName from MST.ContractItemDetail CID
                            left join MST.GeneralContract GC on GC.Id = CID.GeneralContractId
                            left join HKP.GeneralContractItemMaster GCIM on GCIM.Id = CID.ContractMasterId
                            where GC.Id = '" + headerId + "'";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult GetAllCheckById(string headerId)
        {
            try
            {
                var sql = @"select GCC.Id, GCC.isCheck, GCC.SystemId,ei.EmployeeName, ei.EmployeeId , FORMAT(ei.DOJ, 'dd-MMM-yyyy') as DOJ, x.UserName as category,
FORMAT(ei.DOB, 'dd-MMM-yyyy') as DOB ,ei.EmployeeCode, DP.UserName as Department ,
LDSG.StandardName as Designation, SC.UserName as Section, GDSG.UserName LegalDesignation,
SBC.UserName as SubSection
from MST.GeneralContractCheckBy GCC
left join  MST.GeneralContract GC on GC.Id = GCC.GeneralContractId
left join EmployeeInformation EI on EI.SystemId = GCC.SystemId
LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = ei.BudgetCode
LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
left join ORG.Entity UN on UN.Id = MBGT.EntityId
left join ORG.Department DP on DP.ID = POS.DepartmentId
left join ORG.Section SC on SC.Id = POS.SectionId
left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=ei.DesignationGroupId
LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=ei.LegalDesignationId
left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
left join hkp.EmployeeCategory x on x.Id=dm.EmployeeCategoryId
where GC.Id = '" + headerId + "'";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult GetList()
        {
            try
            {
                var sql = @"select GCE.*, GC.UserName GeneralContract, E.UserName Entity, EI.EmployeeName from TRN.GeneralContractEntry GCE
left join ORG.Entity E on E.Id = GCE.EntityId
left join MST.GeneralContract GC on GC.Id = GCE.GeneralContractId
left join EmployeeInformation EI on EI.SystemId = GCE.CheckBySystemId 
order by GCE.Date DESC";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult GetChildList(string headerId)
        {
            try
            {
                var sql = @"select CIE.*, GCI.UserName from TRN.ContractItemEntry CIE
left join TRN.GeneralContractEntry GCE on GCE.Id = CIE.GeneralContractEntryId
left join HKP.GeneralContractItemMaster GCI on GCI.Id = CIE.ContractMasterId
where CIE.GeneralContractEntryId = '" + headerId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #region SAVE
        [HttpPost]
        public ActionResult Save(Dictionary<string, object> data, List<Dictionary<string, object>> contractItemDetail)
        {
            try
            {
                return Json(new { Error = false, Data = ce.Save(data, contractItemDetail), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Update(Dictionary<string, object> data, List<Dictionary<string, object>> contractItemDetail)
        {
            try
            {
                return Json(new { Error = false, Data = ce.Save(data, contractItemDetail), Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion SAVE
    }
}