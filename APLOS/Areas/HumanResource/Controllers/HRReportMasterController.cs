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

namespace Aplos.Areas.HumanResource.Controllers
{
    public class HRReportMasterController : Controller
    {
        private readonly SqlRepository _sqlRepository;
        #region Constructor
        public HRReportMasterController()
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }



        public ActionResult GetMaster(string Id)
        {
            try
            {
                var sql = @"select * from HKP.HRReportMaster where Id = '" + Id + "' ";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public ActionResult GetUserGroup(string id)
        {
            try
            {
                var sql = @"select isSelected=CAST (CASE WHEN UG.Id IS NULL THEN 0 ELSE 1 END AS bit), GM.Id UserGroupId, GM.UserGroup, GM.UserSubGroup, UG.Id, ug.HRReportMasterChildId, ug.Grade, ug.AddedBy, UG.AddedFromIP, UG.AddedDate, UG.UpdatedBy, UG.UpdatedFromIP, UG.UpdatedDate
                           from HKP.HRReportGroupMaster GM
                            outer apply (select * from  [TRN].[HRReportMasterBudgetUserGroup] where UserGroupId=GM.Id AND  HRReportMasterChildId = '" + id + @"') UG";


                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public ActionResult GetUserSubGroup(string userId)
        {
            try
            {
                var sql = @"select Id Value, UserSubGroup Text from HKP.HRReportGroupMaster where Id = '" + userId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public ActionResult GetGrade(string userId)
        {
            try
            {
                var sql = @"select Id Value, Grade Text from HKP.HRReportGroupMaster where Id = '" + userId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public ActionResult GetList()
        {
            try
            {
                string sql = @"select HRM.* from HKP.HRReportMaster HRM

order by Sequence
";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public ActionResult GetEntity()
        {
            var entityQry = @"select Id Value, UserName Text from org.Entity where Id !=111 ";
            return Json(_sqlRepository.GetDataCollection(entityQry), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ViewAllBudgetCode()
        {
            string bgtQuery = @"select BGT.Id, E.UserName Entity, D.UserName Division, DT.UserName Department, S.UserName Section, SS.UserName SubSection
                            , DSG.UserName Designation, A.UserName Activity,SDF.UserName [Shift], P.Code PositionCode
                            , P.UserName Position ,BGT.Code BudgetCode, BGT.Id ManpowerBudgetId 
                            from  MST.ManpowerBudget BGT 
                            left join ORG.Entity E on E.Id = BGT.EntityId
                            left join MST.BudgetMasterActivity BMA on BGT.ROBudgetCode = BMA.BudgetMasterId
                            left join HKP.Activity A on BMA.ActivityId = A.Id
                            left join dbo.ShiftDefination SDF on BGT.ShiftDefinationId = SDF.SystemID
                            left join ORG.Position P on BGT.PositionId = P.Id
                            left join ORG.Division D on P.DivisionId  = D.Id
                            left join ORG.Department DT on P.DepartmentId = DT.Id
                            left join ORG.Section S on P.SectionId = S.Id
                            left join ORG.SubSection SS on P.SubSectionId = SS.Id
                            left join ORG.Division DSN on P.DivisionId = DSN.Id
                            left join HKP.Designation DSG ON P.DesignationId = DSG.Id
                            --left join [TRN].[HRReportMasterChild] HMC on HMC.ManpowerBudgetId = BGT.Id
                            where BGT.Active = 1";

            return Json(_sqlRepository.GetDataCollection(bgtQuery), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBudgetCode(string EntityId, string id)
        {
            string Entity = "'" + EntityId.Replace(",", "','") + "'";//replaced with ""
            string whereClause = "";
            var bgtQuery = "";

            //if(id == null || id == "")
            //{
            //    whereClause = $"where BGT.EntityId in ({Entity})";
            //}
            //else if(EntityId == "NaN,undefined,undefined,undefined,undefined,undefined,undefined,undefined,undefined,undefined" && (id != null || id != ""))
            //{
            //    whereClause = $"where HMC.HRReportMasterId = '{id}'"; 
            //}
            //else if((EntityId != "NaN,undefined,undefined,undefined,undefined,undefined,undefined,undefined,undefined,undefined") && (id != null || id != ""))
            //{
            //    whereClause = $"where BGT.EntityId = ({Entity}) or HMC.HRReportMasterId = '{id}'";
            //}



            bgtQuery = @"select BGT.Id--, HMC.Active 
, E.UserName Entity, D.UserName Division, DT.UserName Department, S.UserName Section, SS.UserName SubSection
, DSG.UserName Designation, A.UserName Activity,SDF.UserName [Shift], P.Code PositionCode
, P.UserName Position ,BGT.Code BudgetCode, BGT.Id ManpowerBudgetId --, isSelected=CAST (CASE WHEN HMC.Id IS NULL THEN 0 ELSE 1 END AS bit)
from  MST.ManpowerBudget BGT 
left join ORG.Entity E on E.Id = BGT.EntityId
left join MST.BudgetMasterActivity BMA on BGT.ROBudgetCode = BMA.BudgetMasterId
left join HKP.Activity A on BMA.ActivityId = A.Id
left join dbo.ShiftDefination SDF on BGT.ShiftDefinationId = SDF.SystemID
left join ORG.Position P on BGT.PositionId = P.Id
left join ORG.Division D on P.DivisionId  = D.Id
left join ORG.Department DT on P.DepartmentId = DT.Id
left join ORG.Section S on P.SectionId = S.Id
left join ORG.SubSection SS on P.SubSectionId = SS.Id
left join ORG.Division DSN on P.DivisionId = DSN.Id
left join HKP.Designation DSG ON P.DesignationId = DSG.Id
--left join [TRN].[HRReportMasterChild] HMC on HMC.ManpowerBudgetId = BGT.Id
where BGT.EntityId in (" + Entity + @") and BGT.Active = 1 
--and BGT.Id not in(select ManpowerBudgetId from [TRN].[HRReportMasterChild] where HMC.HRReportMasterId = '" + id + @"')
";

            return Json(_sqlRepository.GetDataCollection(bgtQuery), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetAllSavedBudgetCode(string id)
        {
            string bgtQuery = @"select HMC.Id ,E.UserName Entity, HMC.Active ,D.UserName Division, DT.UserName Department, S.UserName Section, SS.UserName SubSection
, DSG.UserName Designation, A.UserName Activity,SDF.UserName [Shift], P.Code PositionCode
, P.UserName Position ,BGT.Code BudgetCode, BGT.Id ManpowerBudgetId, isSelected = HMC.Active, HMC.Active
from  [TRN].[HRReportMasterChild] HMC
full join MST.ManpowerBudget BGT on BGT.Id = HMC.ManpowerBudgetId
left join ORG.Entity E on E.Id = BGT.EntityId
left join MST.BudgetMasterActivity BMA on BGT.ROBudgetCode = BMA.BudgetMasterId
left join HKP.Activity A on BMA.ActivityId = A.Id
left join dbo.ShiftDefination SDF on BGT.ShiftDefinationId = SDF.SystemID
left join ORG.Position P on BGT.PositionId = P.Id
left join ORG.Division D on P.DivisionId  = D.Id
left join ORG.Department DT on P.DepartmentId = DT.Id
left join ORG.Section S on P.SectionId = S.Id
left join ORG.SubSection SS on P.SubSectionId = SS.Id
left join ORG.Division DSN on P.DivisionId = DSN.Id
left join HKP.Designation DSG ON P.DesignationId = DSG.Id
where HMC.HRReportMasterId = '" + id + @"' and HMC.Active = 1
order by HMC.ManpowerBudgetId DESC";

            return Json(_sqlRepository.GetDataCollection(bgtQuery), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetEmployee(string column, string value, string headerid)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                var str = @"select top 500 * from (select ''Id, ei.SystemId, ei.EmployeeName, ei.EmployeeId , FORMAT(ei.DOJ, 'dd-MMM-yyyy') as DOJ, x.UserName as category,
                            FORMAT(ei.DOB, 'dd-MMM-yyyy') as DOB ,ei.EmployeeCode, DP.UserName as Department ,
                            LDSG.StandardName as Designation, SC.UserName as Section,
                            SBC.UserName as SubSection --, isSelected=CAST (CASE WHEN HRP.Id IS NULL THEN 0 ELSE 1 END AS bit)
							from dbo.EmployeeInformation ei
                            LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = ei.BudgetCode
                            LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                            LEFT JOIN(Select SUM(TotalNumber)TotalNumber,ManpowerBudgetId,Id from MST.ManpowerBudgetDetail Group BY ManpowerBudgetId,Id) AS mbd ON mbd.ManpowerBudgetId=MBGT.Id
AND mbd.Id =(Select top(1) Id from MST.ManpowerBudgetDetail Where ManpowerBudgetId=MBGT.Id order by EffectiveDate desc)
                            left join ORG.Entity UN on UN.Id = MBGT.EntityId
                            left join ORG.Department DP on DP.ID = POS.DepartmentId
                            left join ORG.Section SC on SC.Id = POS.SectionId
                            left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                            LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=ei.DesignationGroupId
                            LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                            LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=ei.LegalDesignationId
                            left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                            left join hkp.EmployeeCategory x on x.Id=dm.EmployeeCategoryId
                            left join ShiftDefination sd on sd.systemid = mbgt.shiftdefinationid
                            left join SalaryRuleMaster SRM on srm.systemid = ei.salaryrulemastersystemid
                            left join ResidenceGroup RG on RG.Id = ei.ResidenceGroupId
                            left join TransportGroup TG on TG.Id = ei.TransportGroupId 
							--left join TRN.HRReportMasterResponsiblePerson HRP on HRP.EmpSystemId = ei.SystemId
                            where ei.EmployeeStatus = 'Active' and ei.SystemId not in (select EmpSystemId from TRN.HRReportMasterResponsiblePerson where HRReportMasterId = '" + headerid + "')) AS TEMP WHERE " + strkey + " ORDER BY EmployeeCode";

                var json = Json(_sqlRepository.GetDataCollection(str, null), JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult GetSavedResponsiblePerson(string headerId)
        {
            try
            {
                var str = @"select HRP.Id, HRP.HRReportMasterId ,HRP.Active 
  , ei.SystemId, ei.EmployeeName, ei.EmployeeId , FORMAT(ei.DOJ, 'dd-MMM-yyyy') as DOJ, x.UserName as category,
                            FORMAT(ei.DOB, 'dd-MMM-yyyy') as DOB ,ei.EmployeeCode, DP.UserName as Department ,
                            LDSG.StandardName as Designation, SC.UserName as Section,
                            SBC.UserName as SubSection , isSelected=CAST (CASE WHEN HRP.Id IS NULL THEN 0 ELSE 1 END AS bit)
							from (select * from TRN.HRReportMasterResponsiblePerson where HRReportMasterId = '" + headerId + @"' and Active = 1) HRP
							left JOIN dbo.EmployeeInformation ei on ei.SystemId = HRP.EmpSystemId
							left join HKP.HRReportMaster HPM on HPM.Id = HRP.HRReportMasterId
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
                            left join ShiftDefination sd on sd.systemid = mbgt.shiftdefinationid
                            left join SalaryRuleMaster SRM on srm.systemid = ei.salaryrulemastersystemid
                            left join ResidenceGroup RG on RG.Id = ei.ResidenceGroupId
                            left join TransportGroup TG on TG.Id = ei.TransportGroupId          
                            where ei.EmployeeStatus = 'Active'";
                return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public ActionResult Save(Dictionary<string, object> datas)
        {
            try
            {

                string TableName = "HKP.HRReportMaster";
                DataSet dsMaster;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");



                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + datas["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    datas["Id"] = _Id;

                    AddNewRow(dsMaster.Tables[0], datas);
                }
                else
                {
                    _Id = datas["Id"].ToString();

                    EditRow(dsMaster.Tables[0].Rows[0], datas);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Data = datas, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public void Delete(string id)
        {
            try
            {
                string TableName = "HKP.HRReportMaster";

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

            }
            catch (Exception ex)
            {

                throw ex;

            }
        }

        #region AddEdit
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
            dr["AddedDate"] = DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

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
            dr["UpdatedDate"] = DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }

        public double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM HKP.HRReportMaster");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }
        #endregion AddEdit

        public void SaveData(Dictionary<string, object> chkBgtList, string headerId, out string contId, List<Dictionary<string, object>> usergroup)
        {
            string TableName = "TRN.HRReportMasterChild";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;

            DataSet dsMaster, dsChild;


            try
            {

                string sql = "SELECT * FROM [TRN].[HRReportMasterChild] WHERE Id='" + chkBgtList["Id"] + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                string id = string.Empty;

                string _Id = "";
                string _UserGroupId = string.Empty;

                if (dsMaster.Tables[0].Rows.Count == 0)
                {

                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    chkBgtList["Id"] = _Id;
                    chkBgtList["HRReportMasterId"] = headerId;
                    chkBgtList["Active"] = chkBgtList["isSelected"];


                    AddNewRow(dsMaster.Tables[0], chkBgtList);

                }
                else
                {
                    chkBgtList["Active"] = 0;
                    EditRow(dsMaster.Tables[0].Rows[0], chkBgtList);
                }

                contId = dsMaster.Tables[0].Rows[0]["Id"].ToString();



                objCon.OpenDataSetThroughAdapter("select * from TRN.HRReportMasterBudgetUserGroup  where HRReportMasterChildId = '" + contId + "'", out dsChild, false, "1");
                foreach (var item in usergroup)
                {
                    DataView dv = new DataView(dsChild.Tables[0]);

                    dv.RowFilter = "Id='" + item["Id"] + "'";
                    if (dv.Count > 0)
                    {
                        DataRow dr = dv[0].Row;
                        dr.BeginEdit();
                        dr["Grade"] = item["Grade"];

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();

                    }
                    else
                    {
                        DataRow dr = dsChild.Tables[0].NewRow();
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("TRN.HRReportMasterBudgetUserGroup", out _UserGroupId);

                        dr["Id"] = _UserGroupId;
                        dr["HRReportMasterChildId"] = contId;
                        dr["UserGroupId"] = item["UserGroupId"];
                        dr["Grade"] = item["Grade"];
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dsChild.Tables[0].Rows.Add(dr);
                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsChild);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> chkBgtList, string headerId, List<Dictionary<string, object>> usergroup)
        {
            try
            {
                SaveData(chkBgtList, headerId, out string contractId, usergroup);


                return Json(new { Id = contractId, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }

        public ActionResult DeleteBudgetCode(List<Dictionary<string, object>> groupId, string bgtId)
        {
            try
            {
                string BudgetCodeTable = "TRN.HRReportMasterChild";
                string GroupTable = "TRN.HRReportMasterBudgetUserGroup";
                var userGroupId = "";

                foreach (var item in groupId)
                {
                    if (userGroupId == "")
                        userGroupId = "'" + item["Id"] + "'";
                    else
                        userGroupId = userGroupId + ",'" + item["Id"] + "'";

                }

                if (string.IsNullOrEmpty(userGroupId))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                //con.executeQuery($"delete from  {GroupTable} where id in ('{userGroupId})'");
                con.executeQuery("delete from " + GroupTable + " where id in (" + userGroupId + ")");
                con.executeQuery("delete from " + BudgetCodeTable + " where id='" + bgtId + "'");
                con.CommitTransaction();
                return Json(new { Id = bgtId, x = groupId, Message = AplosMessage.Deleted });

            }
            catch (Exception ex)
            {

                throw ex;

            }
        }

        public ActionResult Save_One_or_MultipleResPers(List<Dictionary<string, object>> chkRespersonList, string headerId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string TableNameChildA = "TRN.HRReportMasterResponsiblePerson";

                DataSet dsChildA;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                string _Id = "";
                #region CHILD 1
                con.OpenDataSetThroughAdapter("select * from " + TableNameChildA + " where HRReportMasterId='" + headerId + "'", out dsChildA, false, "1");

                //for (int i = 0; i < chkBgtList.Count; i++)
                foreach (var item in chkRespersonList)
                {
                    DataView dv = new DataView(dsChildA.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";
                    if (dv.Count > 0)
                    {

                        bplib.clsGenID genid = new bplib.clsGenID();


                        DataRow dr = dv[0].Row;
                        dr.BeginEdit();

                        dr["Active"] = false;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableNameChildA, out _Id);

                        DataRow dr = dsChildA.Tables[0].NewRow();
                        dr["Id"] = _Id;
                        dr["HRReportMasterId"] = headerId;
                        dr["EmpSystemId"] = item["SystemId"];
                        dr["Active"] = item["isSelected"];
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dsChildA.Tables[0].Rows.Add(dr);
                    }


                }
                #endregion CHILD 1

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsChildA);

                return Json(new { Data = chkRespersonList, headerId, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        #region 2nd Tab

        public ActionResult GetListB()
        {
            try
            {
                string sql = @"select HRM.* from [HKP].[HRReportGroupMaster] HRM";


                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public ActionResult SaveB(Dictionary<string, object> datas)
        {
            try
            {

                string TableName = "HKP.HRReportGroupMaster";
                DataSet dsMaster;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserGroup='" + datas["UserGroup"] + "' AND  Id<>'" + datas["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Group Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserSubGroup='" + datas["UserSubGroup"] + "' AND  Id<>'" + datas["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Sub Group Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + datas["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    datas["Id"] = _Id;

                    AddNewRow(dsMaster.Tables[0], datas);
                }
                else
                {
                    _Id = datas["Id"].ToString();

                    EditRow(dsMaster.Tables[0].Rows[0], datas);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Data = datas, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public void DeleteB(string id)
        {
            try
            {
                string TableName = "HKP.HRReportGroupMaster";

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

            }
            catch (Exception ex)
            {

                throw ex;

            }
        }

        public JsonResult DeleteResponsiblePerson(List<Dictionary<string, object>> data)
        {
            try
            {
                var id = "";
                foreach (var item in data)
                {
                    if (id == "")
                        id = "'" + item["Id"] + "'";
                    else
                        id = id + ",'" + item["Id"] + "'";
                }
                string TableName = "TRN.HRReportMasterResponsiblePerson";

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery($"delete from {TableName} where id in ({id})");
                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;

            }
        }

        #endregion 2nd Tab

    }


}