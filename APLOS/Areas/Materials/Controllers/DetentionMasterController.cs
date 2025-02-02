using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Materials;
using Library.Security.Core;
using Library.Service.Materials;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.Materials.Controllers
{
    public class DetentionMasterController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        public DetentionMasterController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations
        public JsonResult StorageSql()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_sqlRepository.GetDataCollection("select s.Id,s.PlantId,s.UserName as Storage from HKP.MaterialStorage s where s.plantId = '" + identity.PlantId + @"'"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetList(string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM dbo.Rack where PlantId='" + plantId + "' order by sequence";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public JsonResult Create(Dictionary<string, object> DetentionData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from DetentionMaster where Id<>'" + DetentionData["Id"] + "'", out DataSet dsDetentionMasterValidation, false, "1");

                //if (dsDetentionMaster.Tables[0].Rows.Count>0)
                //{
                //    throw new Exception("Code Already Exist.");
                //}

                DataSet dsDetentionMaster;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from DetentionMaster where Id='" + DetentionData["Id"] + "'", out dsDetentionMaster, false, "1");
                string _Id = "";

                #region data update
                if (dsDetentionMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("DetentionMaster", out _Id);
                    _Id = "DM" + _Id;
                    DetentionData["Id"] = _Id;
                    AddNewRow(dsDetentionMaster.Tables[0], DetentionData);
                }
                else
                {
                    _Id = DetentionData["Id"].ToString();
                    EditRow(dsDetentionMaster.Tables[0].Rows[0], DetentionData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsDetentionMaster);

                return Json(new { Error = false, Data = DetentionData, Sequence = GetSequence(), Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
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
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM  dbo.Rack ");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;
            return 1;
        }
        [Authorize, HttpPost]
        public ActionResult getProcess(string DetentionMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select DMP.Id,P.Sequence,P.Code,P.ShortName,P.StandardName,P.Id ProcessId,P.UserName Process
			                            from DetentionMasterProcess DMP
			                            left join HKP.Process P on P.Id=DMP.ProcessId
										where DMP.DetentionMasterId='" + DetentionMasterId + @"'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult getDepartment(string DetentionMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select dmd.Id,d.Sequence,d.Code,d.ShortName,d.StandardName,d.Id DepartmentId,d.UserName Department
			                            from DetentionMasterDepartment AS dmd
			                            left join org.Department AS d ON d.Id=dmd.DepartmentId
										where dmd.DetentionMasterId='" + DetentionMasterId + @"'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult getMachine(string DetentionMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select dmm.Id,mm.Sequence,mm.Code,mm.ShortName,mm.StandardName,mm.Id MachineMasterId,mm.UserName Machine
			                            from DetentionMasterMachine AS dmm
			                            left join mst.MachineMaster AS mm ON mm.Id=dmm.MachineMasterId
										where dmm.DetentionMasterId='" + DetentionMasterId + @"'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult getResponsible(string DetentionMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select DMR.Id,E.EmployeeCode,E.EmployeeName,DEP.UserName AS Department,S.UserName as Section,
  SS.UserName as SubSection,DEG.UserName AS [LegalDesignation],E.SystemId ResponsibleMasterId,DMR.DetentionMasterId
  from DetentionMasterResponsible AS DMR
			                left join EmployeeInformation AS E ON E.SystemId=DMR.ResponsibleMasterId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
							LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=E.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=PR.DepartmentId
							LEFT OUTER JOIN ORG.Section S ON S.Id=PR.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
							where DMR.DetentionMasterId='" + DetentionMasterId + @"'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult ProcessDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from DetentionMasterProcess where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;

            }
        }

        [Authorize, HttpPost]
        public ActionResult DepartmentDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from DetentionMasterDepartment where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpPost]
        public ActionResult MachineDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from DetentionMasterMachine where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpPost]
        public ActionResult ResponsibleDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from DetentionMasterResponsible where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult LoadDetentionList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT *,CASE IsAvoidable WHEN 1 THEN 'Yes' ELSE 'No' END Avoidable,(select EmployeeName from EmployeeInformation where SystemId=InChargePersonId) as InChargePerson,
                            (select UserName from [HKP].[DetentionType] where Id=DetentionTypeId) as DetentionType
                            FROM DetentionMaster";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadDepartmentList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT *,convert(bit,0) AS chk FROM [ORG].[Department]";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadMachineList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT *,convert(bit,0) AS chk FROM mst.MachineMaster";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadResponsibleList(string DetentionId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT distinct convert(bit,0) AS chk,DMR.Id,DMR.ResponsibleMasterId,EI.SystemId,EI.EmployeeCode,EI.EmployeeName,DEP.UserName AS Department,S.UserName as Section,
                            SS.UserName as SubSection,DEG.UserName AS [LegalDesignation]
                            FROM dbo.EmployeeInformation AS EI
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = ei.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                            LEFT JOIN DetentionMasterResponsible DMR ON DMR.ResponsibleMasterId=EI.SystemId and DetentionMasterId='" + DetentionId + @"'
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=pr.DepartmentId
							LEFT OUTER JOIN ORG.Section S ON S.Id=pr.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=pr.SubSectionId
                            WHERE EI.EmployeeStatus='Active' and EI.EmployeeCode is not null and pr.DepartmentId in (select distinct DepartmentId from DetentionMasterDepartment)";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadEditData(string DetentionID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select *,(select EmployeeName from EmployeeInformation where SystemId=InChargePersonId) as InChargePerson
                            FROM DetentionMaster where Id='" + DetentionID + @"'";
            return Json(new { detention = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        //[HttpGet, Authorize]
        //public JsonResult GetList(GridParameter parameters, string processId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_processService.Query(parameters, identity.CompanyGroupId, new JavaScriptSerializer().Deserialize<string[]>(processId)), JsonRequestBehavior.AllowGet);
        //}
        //public ActionResult Delete(string RackID)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    string Deletesql = @"delete from Bin where RackId ='" + RackID + @"'";
        //    string Deletesql1 = @"delete from Rack where Id='" + RackID + @"'";
        //    return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        //   // return Json(new { rack = _sqlRepository.GetDataCollection(Deletesql1, null), bin = _sqlRepository.GetDataCollection(Deletesql, null) }, JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        public JsonResult Delete(string Id)
        {
            DeleteData(Id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteData(string RackID)
        {
            string strSQL, strCSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strCSQL = @"delete from Bin where RackId ='" + RackID + @"'";
                strSQL = @"delete from Rack where Id='" + RackID + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strCSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        [HttpPost]
        public JsonResult CreateProcess(List<Dictionary<string, object>> data, string DetentionMasterId)
        {
            try
            {
                SaveData(data, DetentionMasterId);

                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }
        private void SaveData(List<Dictionary<string, object>> data, string DetentionMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMasterOrder;
            string id = string.Empty;
            try
            {
                string mosql = "SELECT * FROM DetentionMasterProcess WHERE DetentionMasterId ='" + DetentionMasterId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(mosql, out dsMasterOrder, false, "1");

                string cId = string.Empty;
                string DetentionMasterProcessId = "";


                foreach (var item in data)
                {

                    DataView dv = new DataView(dsMasterOrder.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("DetentionMasterProcess", out DetentionMasterProcessId);

                        item["Id"] = "DMP-" + DetentionMasterProcessId + "-" + (1);
                        item["DetentionMasterId"] = DetentionMasterId;
                        item["ProcessId"] = item["ProcessId"];

                        AddNewRow(dsMasterOrder.Tables[0], item);
                    }

                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMasterOrder);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        #endregion -- Operations
        [HttpPost]
        public JsonResult CreateDepartment(List<Dictionary<string, object>> data, string DetentionMasterId)
        {
            try
            {
                SaveDepartmentData(data, DetentionMasterId);

                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }
        private void SaveDepartmentData(List<Dictionary<string, object>> data, string DetentionMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMasterOrder;
            string id = string.Empty;
            try
            {
                string mosql = "SELECT * FROM DetentionMasterDepartment WHERE DetentionMasterId ='" + DetentionMasterId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(mosql, out dsMasterOrder, false, "1");

                string cId = string.Empty;
                string DetentionMasterDepartmentId = "";


                foreach (var item in data)
                {

                    DataView dv = new DataView(dsMasterOrder.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("DetentionMasterDepartment", out DetentionMasterDepartmentId);

                        item["Id"] = "DMD-" + DetentionMasterDepartmentId + "-" + (1);
                        item["DetentionMasterId"] = DetentionMasterId;
                        item["DepartmentId"] = item["DepartmentId"];

                        AddNewRow(dsMasterOrder.Tables[0], item);
                    }

                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMasterOrder);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        [HttpPost]
        public JsonResult CreateMachine(List<Dictionary<string, object>> data, string DetentionMasterId)
        {
            try
            {
                SaveMachineData(data, DetentionMasterId);

                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }

        [HttpPost]
        public JsonResult CreateResponsible(List<Dictionary<string, object>> data, string DetentionMasterId)
        {
            try
            {
                SaveResponsibleData(data, DetentionMasterId);

                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }
        private void SaveMachineData(List<Dictionary<string, object>> data, string DetentionMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMasterOrder;
            string id = string.Empty;
            try
            {
                string mosql = "SELECT * FROM DetentionMasterMachine WHERE DetentionMasterId ='" + DetentionMasterId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(mosql, out dsMasterOrder, false, "1");

                string cId = string.Empty;
                string DetentionMasterMachineId = "";

                foreach (var item in data)
                {
                    DataView dv = new DataView(dsMasterOrder.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("DetentionMasterMachine", out DetentionMasterMachineId);

                        item["Id"] = "DMM-" + DetentionMasterMachineId + "-" + (1);
                        item["DetentionMasterId"] = DetentionMasterId;
                        item["MachineMasterId"] = item["MachineMasterId"];

                        AddNewRow(dsMasterOrder.Tables[0], item);
                    }

                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMasterOrder);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void SaveResponsibleData(List<Dictionary<string, object>> data, string DetentionMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMasterOrder;
            string id = string.Empty;
            try
            {
                string mosql = "SELECT * FROM DetentionMasterResponsible WHERE DetentionMasterId ='" + DetentionMasterId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(mosql, out dsMasterOrder, false, "1");

                string cId = string.Empty;
                string DetentionMasterResponsibleId = "";

                foreach (var item in data)
                {
                    DataView dv = new DataView(dsMasterOrder.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("DetentionMasterResponsible", out DetentionMasterResponsibleId);

                        item["Id"] = "DMR-" + DetentionMasterResponsibleId + "-" + (1);
                        item["DetentionMasterId"] = DetentionMasterId;
                        item["ResponsibleMasterId"] = item["ResponsibleMasterId"];

                        AddNewRow(dsMasterOrder.Tables[0], item);
                    }

                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMasterOrder);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        [Authorize, HttpGet]
        public JsonResult GetEmployeeListInChargePerson(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT EI.SystemId, mb.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=MB.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=p.DepartmentId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=p.SectionId
							LEFT OUTER JOIN ORG.Section SS ON SS.Id=p.SubSectionId
                            WHERE EI.EmployeeStatus='Active'";

                return Json(_sqlRepository.GetGridData(parameters), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetDetentionTypeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"Select  UserName As Text, Id As Value from [HKP].[DetentionType]";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateProcessParameter(Dictionary<string, object> data, IEnumerable<DetentionMasterMachineParameterFormulaDetail> details)
        {
            try
            {
                SaveCostingSOTemplateData(data, details);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        private void SaveCostingSOTemplateData(Dictionary<string, object> data, IEnumerable<DetentionMasterMachineParameterFormulaDetail> details)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                if (data != null)
                {
                    string _Id = "";

                    DataSet dsMaster, dsDestination = null;
                    DataRow drF;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                    con.OpenDataSetThroughAdapter("select * from DetentionMasterMachineParameter where UserName='" + data["UserName"] + "'  AND  Id<>'" + data["Id"] + "' AND DetentionMasterId='" + data["DetentionMasterId"] + "'", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                        throw new Exception("UserName already exists!!!");


                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.DetentionMasterMachineParameter WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.FormulaDetail Where DetentionMasterMachineParameterId='" + data["Id"] + "'", out dsDestination, false, "1");

                    if (data["EntryState"].ToString() == "Entry")
                    {
                        data["Formula"] = DBNull.Value;
                        data["FormulaId"] = DBNull.Value;


                        while (dsDestination.Tables[0].DefaultView.Count > 0)
                            dsDestination.Tables[0].DefaultView[0].Delete();
                    }


                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(DetentionMasterMachineParameter), out _Id);

                        data["Id"] = _Id;
                        AddNewRow(dsMaster.Tables[0], data);
                    }
                    else
                    {
                        _Id = data["Id"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], data);
                    }

                    string Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                    #region NoticePeriodFormulaDetail 

                    if (data["EntryState"].ToString() == "Calculate")
                    {
                        while (dsDestination.Tables[0].DefaultView.Count > 0)
                            dsDestination.Tables[0].DefaultView[0].Delete();
                        int count = 0;
                        if (details != null)
                        {

                            foreach (var item in details)
                            {
                                drF = dsDestination.Tables[0].NewRow();
                                count++;
                                string pk = _Id + "_" + count;
                                drF["Id"] = pk;
                                drF["DetentionMasterMachineParameterId"] = _Id;
                                drF["Sequence"] = item.Sequence;
                                drF["DetentionMasterMachineParameterHeadId"] = item.DetentionMasterMachineParameterHeadId;
                                drF["Component"] = item.Component;

                                dsDestination.Tables[0].Rows.Add(drF);
                            }

                        }
                    }
                    #endregion NoticePeriodFormulaDetail 

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster, dsDestination);


                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        public class DetentionMasterMachineParameter
        {
            public string Id { get; set; }
            public string FormulaDes { get; set; }
            public string FormulaDesID { get; set; }
            public string AddedBy { get; set; }
            public DateTime AddedDate { get; set; }
            public string AddedFromIP { get; set; }
            public string UpdatedBy { get; set; }
            public DateTime UpdatedDate { get; set; }
            public string UpdatedFromIP { get; set; }

        }

        public class DetentionMasterMachineParameterFormulaDetail
        {
            public string Id { get; set; }
            public decimal Sequence { get; set; }
            public string Component { get; set; }
            public string DetentionMasterMachineParameterId { get; set; }
            public string DetentionMasterMachineParameterHeadId { get; set; }

        }
    }
}