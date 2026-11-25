#region Using
using Library.Core;
using Library.Model.Machines;
using Aplos.Properties;
using Library.Data;
using Library.Service.Machines;

using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using System.Collections.Generic;
using System.Data;
using OTSBD;
using System;

#endregion

namespace Aplos.Areas.Machines.Controllers
{
    public class MachineandEquipmentHelpDeskController : Controller
    {
        #region -- Constrator
        private readonly ISqlRepository _sqlRepository;

        public MachineandEquipmentHelpDeskController(ISqlRepository R)
        {
            _sqlRepository = R;

        }
        #endregion

        #region -- Pages
        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [HttpPost, Authorize]
        public ActionResult GetMachineandEquipmentHelpDeskDataList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select  * from (SELECT TT.*,E.EmployeeName ResponsiblePerson,SD.UserName MachineCategory FROM dbo.MachineandEquipmentHelpDesk TT
LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=TT.ResponsiblePersonId
LEFT JOIN hkp.MachineCategory SD ON SD.Id=TT.MachineCategoryId) AS TEMP WHERE " + strkey + "";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }


            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                string smg = "";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from dbo.MachineandEquipmentHelpDesk where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("MachineandEquipmentHelpDesk", out _Id);

                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                    smg = AplosMessage.Insert;
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                    smg = AplosMessage.Updated;
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = data, Message = smg });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
        [HttpPost]
        public ActionResult Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from dbo.MachineandEquipmentHelpDesk where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost]
        public ActionResult DeleteResPerson(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from dbo.MachineandEquipmentHelpDeskResponsiblePerson where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost]
        public JsonResult CreateResPerson(List<Dictionary<string, object>> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsEntity;
            try
            {

                #region Entity 
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.MachineandEquipmentHelpDeskResponsiblePerson", out dsEntity, false, "1");
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsEntity.Tables[0]);
                        dv.RowFilter = "Id='" + Convert.ToInt64(item["Id"]) + "' AND EmpSystemId='" + item["EmpSystemId"] + "'";

                        if (dv.Count == 0)
                        {
                            AddNewRow(dsEntity.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsEntity);

                return Json(new { Error = false, Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetResPersonList()
        {
            string CmdText = @"SELECT SSU.*,EI.EmployeeName,EI.CompanyId,EI.PlantId,EI.GroupId,EI.EmployeeCode
                                      ,EI.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                      PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,SS.UserName SubSection
                                      ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,EI.EmpPicPath,EI.EmployeeStatus,C.UserName Company
									  FROM dbo.MachineandEquipmentHelpDeskResponsiblePerson SSU
									  LEFT JOIN dbo.EmployeeInformation EI on ei.SystemId=ssu.EmpSystemId
                                      LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                      LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                      LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                      LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                                      LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                      LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                      LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                      LEFT JOIN ORG.Plant PL ON PL.Id=EI.PlantId
                                      LEFT JOIN ORG.Company C ON C.Id=EI.CompanyId
                                      LEFT JOIN ORG.Line L ON L.Id=PMB.LineId
                                      LEFT JOIN HKP.Designation DEG ON EI.GivenDesignationId=DEG.Id
                                      LEFT JOIN HKP.LegalDesignation LDEG ON EI.LegalDesignationId=LDEG.Id
									  ORDER BY EI.EmployeeStatus";
            var jsondata = Json(_sqlRepository.GetDataCollection(CmdText), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        #endregion
    }
}