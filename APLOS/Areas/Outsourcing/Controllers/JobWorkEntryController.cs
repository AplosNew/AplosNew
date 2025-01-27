using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Security.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.Outsourcing.Controllers
{
    public class JobWorkEntryController : BaseController
    {
        #region Constructor
        private readonly SqlRepository _sqlRepository;
        public JobWorkEntryController(SqlRepository Repository)
        {
            _sqlRepository = Repository;
        }
        #endregion
        #region Pages
        // GET: IE/JobWorkItem
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion
        #region Code
        [HttpGet, Authorize]
        public JsonResult GetAllData()
        {
            string sql = "";
            sql = @"select * from trn.JobWorkEntry order by Sequence Desc";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetSelectedData(string Id)
        {
            string sql = "";
            sql = @"select * from trn.JobWorkEntry
                    WHERE Id='" + Id + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult DeleteSelectedData(string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id.ToString()))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("DELETE FROM [HKP].[JobWorkItem] WHERE Id='" + Id.ToString() + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobWorkEntry", out sID);
            return sID;
        }

        [HttpPost, Authorize]
        public ActionResult SaveData(Dictionary<string, object> saveData)
        {
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            DataSet dsDuplicate = new DataSet();
            if (saveData["Id"]==null)
            {
                con.getDataSet("SELECT * FROM trn.JobWorkEntry WHERE GateEntryNo = '" + saveData["GateEntryNo"] + "'", out dsDuplicate);
                if (dsDuplicate.Tables[0].Rows.Count > 0)
                    return Json(new { Error = true, Message = "Gate Entry No. Already Exists..!" }, JsonRequestBehavior.AllowGet);
            }
           

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID _genId = new bplib.clsGenID();
                string _Message = "";
                string Id = "";
                con.getDataSet("SELECT * FROM trn.JobWorkEntry WHERE Id='" + saveData["Id"] + "'", out DataSet dsOut);
                if (dsOut.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsOut.Tables[0].NewRow();
                    _genId.GenID("trn.JobWorkEntry", out Id);
                 //   Id = "JWI-" + Id;
                    dr["Id"] = "JWE" + GetPK();
                    dr["Sequence"] = saveData["Sequence"];
                    dr["CustomerInvoiceNo"] = saveData["CustomerInvoiceNo"];
                    dr["CustomerInvoiceDate"] = saveData["CustomerInvoiceDate"];
                    dr["CustomerName"] = saveData["CustomerName"];
                    dr["Article"] = saveData["Article"];
                    dr["Lot"] = saveData["Lot"];
                    dr["Shade"] = saveData["Shade"];
                    dr["GateEntryNo"] = saveData["GateEntryNo"];
                    dr["Quantity"] = saveData["Quantity"];
                    dr["MaterialValue"] = saveData["MaterialValue"];
                    dr["TaxValue"] = saveData["TaxValue"];
                    dr["TotalValue"] = saveData["TotalValue"];
                    dr["GateEntryNo"] = saveData["GateEntryNo"];
                    dr["Remarks"] = saveData["Remarks"];
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsOut.Tables[0].Rows.Add(dr);

                    _Message = "Data Save Successfully..!";

                }
                else
                {
                    DataRow dr = dsOut.Tables[0].DefaultView[0].Row;
                    //Id = dr["Id"].ToString();
                    dr.BeginEdit();

                    dr["Sequence"] = saveData["Sequence"];
                    dr["CustomerInvoiceNo"] = saveData["CustomerInvoiceNo"];
                    dr["CustomerInvoiceDate"] = saveData["CustomerInvoiceDate"];
                    dr["CustomerName"] = saveData["CustomerName"];
                    dr["Article"] = saveData["Article"];
                    dr["Lot"] = saveData["Lot"];
                    dr["Shade"] = saveData["Shade"];
                    dr["GateEntryNo"] = saveData["GateEntryNo"];
                    dr["Quantity"] = saveData["Quantity"];
                    dr["MaterialValue"] = saveData["MaterialValue"];
                    dr["TaxValue"] = saveData["TaxValue"];
                    dr["TotalValue"] = saveData["TotalValue"];
                    dr["GateEntryNo"] = saveData["GateEntryNo"];
                    dr["Remarks"] = saveData["Remarks"];
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();

                    _Message = "Data Updated Successfully..!";
                }

                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsOut);

                return Json(new { Error = false, Message = _Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetUOMList()
        {
            try
            {
                string sql = "";
                sql = @"SELECT Id,UserName FROM [SCS].[UnitOfMeasurement] WHERE Active = 1 ORDER BY UserName";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetSequenceNumber()
        {
            try
            {
                string sql = "";
                sql = @"SELECT  isnull(Max(Sequence),0)+1 AS Sequence FROM trn.JobWorkEntry";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult getMatbaseUOM(string MatId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select mm.Id, mm.Code, mm.UserName as MaterialName,mc.UserName as MaterialCategory, mgm.UserName as MaterialGroupMaster,mm.BaseUOMId, buom.UserName as BaseUOM
                                      from MST.MaterialMaster mm left join MST.MaterialGroupMaster mgm on mm.MaterialGroupMasterId=mgm.Id
									  left join SCS.UnitOfMeasurement buom on buom.Id=mm.BaseUOMId
									  left join HKP.MaterialCategory mc on mc.Id=mm.MaterialCategoryId
                                      WHERE mm.CompanyGroupId='" + identity.CompanyGroupId + @"'
                                      AND mm.Id='"+ MatId + @"' order by mm.Code ";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult LoadAllMaterialMstDetails(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select GE.Id GateEntryNo , format(GE.GateEntryTime,'dd-MMM-yyyy') GateEntryDate , GE.PackageQty Quantity , PT.StandardName Party , GE.Remarks   from trn.GateEntry GE
left join hkp.Party PT on PT.Id = GE.PartyId 
order by GE.GateEntryTime desc --where Convert(date,GE.GateEntryTime) between DATEADD(day, -2, CAST(GETDATE() AS date)) and GETDATE() ";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult LoadResponsiblePersonDetails(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS Id, EMP.EmployeeStatus,
                        EMP.EmployeeName,EMP.EmployeeCode AS Code,
                        EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName DepartmentName,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
    
                        WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and emp.CompanyId='" + identity.CompanyId + @"' and emp.EmployeeStatus='Active' and EMP.EmpType='Local'
                   AND isnull(Emp.SystemID,'') not in (select isnull(ResponsiblePersonId,'') from HKP.JobWorkItem where Id='" + Id + @"')
                  order by EMP.EmployeeCode";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        #endregion
    }
}