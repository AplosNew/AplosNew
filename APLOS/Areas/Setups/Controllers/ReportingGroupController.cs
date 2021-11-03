#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Security.Core;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Organizations;
using Library.Service.Setups;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
    public class ReportingGroupController : BaseController
    {
        string TableName = "dbo.ReportingGroup";
        string DetailsTableName = "dbo.BudgetCodeTagWithReportGroup ";

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IRecruitmentSelectionService _preRecruitmentEmployee;
        private readonly IManpowerBudgetService _manpowerBudgetService;
        public ReportingGroupController(ISqlRepository R, IRecruitmentSelectionService preRecruitmentEmployee,
             IManpowerBudgetService manpowerBudgetService)
        {
            _sqlRepository = R;            
            _manpowerBudgetService = manpowerBudgetService;
            _preRecruitmentEmployee = preRecruitmentEmployee;
        }

        #endregion Constructor
           
        public ActionResult Aplos()
        {
            return View();
        }


        #region --Master Save--
        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName + ""), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from " + TableName + " where Id = '" + Id + "' ");


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost,Authorize]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT * FROM " + TableName + ") AS TEMP WHERE " + strkey + " order by sequence";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same user name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "RG" + _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = data, Sequence = GetSequence(), Message = AplosMessage.Updated });

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
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();
                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
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
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;
            return 1;
        }


        [HttpGet]
        public ActionResult GetReportingGroupList(GridParameter parameters)
        {

            parameters.CmdText = @"SELECT * ,'' AS Flag FROM dbo.ReportingGroup";

            return Json(_sqlRepository.GetGridData(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetuserReportGroupList(string userId)
        {
            var sql = @"SELECT UG.Id, UG.UserId, UG.ReportingGroupId, PG.Code, PG.[Sequence], PG.ShortName, PG.StandardName, PG.UserName
                            FROM [SEC].[UserReportGroup] AS UG
                            JOIN [dbo].[ReportingGroup] AS PG ON UG.ReportingGroupId=PG.Id
                            WHERE UG.UserId='" + userId + "' ORDER BY PG.UserName";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region --Budget Code--

        [HttpGet, Authorize]
        public ActionResult GetBudgetCodeList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(GetBudgetCode(parameters), JsonRequestBehavior.AllowGet);
        }
        public GridModel GetBudgetCode (GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"select m.Id Code,e.UserName EntityName,p.UserName PositionName,l.UserName Line,
                                        u.UserName UnitName,d.UserName Designation,ISNULL(dpt.UserName,dept.UserName)Department,
                                        ISNULL(sec.UserName,sect.UserName)Section,ISNULL(ssec.UserName,ssect.UserName)SubSection
                                        FROM [MST].[ManpowerBudget] m
                                        left join ORG.Entity e on e.Id=m.EntityId
                                        left join ORG.Position p on p.Id= m.PositionId
                                        left join ORG.Unit u on u.Id=e.UnitId
                                        left join HKP.Designation d on d.Id=p.DesignationId
                                        left join ORG.Department dpt on dpt.Id = p.DepartmentId
                                        left join ORG.Department dept on dept.Id = e.DepartmentId
                                        left join ORG.Section sec on sec.Id = p.SectionId
                                        left join ORG.Section sect on sec.Id = e.SectionId
                                        left join ORG.SubSection ssec on ssec.Id = p.SubSectionId
                                        left join ORG.SubSection ssect on ssec.Id = e.SubSectionId
                                        left join ORG.Line l on l.Id=m.LineId";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        [HttpPost,Authorize]
        public JsonResult SaveDetails(Dictionary<string, object> Details)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + DetailsTableName + " where Id='" + Details["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    Details["Id"] = "BCRG" + _Id;
                    AddNew(dsMaster.Tables[0], Details);
                }
                else
                {
                    _Id = Details["Id"].ToString();
                    EditR(dsMaster.Tables[0].Rows[0], Details);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = Details, Sequence = GetSequence(), Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
        [HttpPost,Authorize]
        public ActionResult DeleteDetails(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + DetailsTableName + " where id='" + id + "'");
                con.CommitTransaction();
                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        private void AddNew(DataTable dt, Dictionary<string, object> sourceData)
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
        private void EditR(DataRow dr, Dictionary<string, object> sourceData)
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
        [HttpPost, Authorize]
        public ActionResult GetDetailsList(string Mid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select bct.Id,bct.ReportGroupId,m.Id Code,e.UserName EntityName,p.UserName PositionName,l.UserName Line,
                                u.UserName UnitName,d.UserName Designation,ISNULL(dpt.UserName,dept.UserName)Department,
                                ISNULL(sec.UserName,sect.UserName)Section,ISNULL(ssec.UserName,ssect.UserName)SubSection
                                FROM [MST].[ManpowerBudget] m
                                left join ORG.Entity e on e.Id=m.EntityId
                                left join ORG.Position p on p.Id= m.PositionId
                                left join ORG.Unit u on u.Id=e.UnitId
                                left join HKP.Designation d on d.Id=p.DesignationId
                                left join ORG.Department dpt on dpt.Id = p.DepartmentId
                                left join ORG.Department dept on dept.Id = e.DepartmentId
                                left join ORG.Section sec on sec.Id = p.SectionId
                                left join ORG.Section sect on sec.Id = e.SectionId
                                left join ORG.SubSection ssec on ssec.Id = p.SubSectionId
                                left join ORG.SubSection ssect on ssec.Id = e.SubSectionId
                                left join ORG.Line l on l.Id=m.LineId
                                left join BudgetCodeTagWithReportGroup bct on bct.ManpowerBudgetId = m.Id
                                WHERE bct.ReportGroupId = '" + Mid+"'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}