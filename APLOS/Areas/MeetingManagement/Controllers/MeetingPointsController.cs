#region Using

using Aplos.Controllers;
using Aplos.Helpers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Attendances;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Setups;
using Library.ViewModel.Organizations;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.MeetingManagement.Controllers
{
    public class MeetingPointsController : BaseController
    {


        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public MeetingPointsController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM MeetingItemHeader"), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (select MP.*,D.UserName Department,MT.UserName MeetingType,EI.EmployeeName ByWhomName,EI.EmployeeCode ByWhomCode
			
			from MeetingItemHeader MP
			left join ORG.Department D on D.Id=MP.DepartmentId
			left join MeetingType MT on MT.Id=MP.MeetingTypeId
			left join EmployeeInformation EI on EI.SystemId=MP.ByWhomId) AS TEMP WHERE " + strkey;



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from MeetingItemHeader where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "MeetingItemHeader", out _Id);

                    data["Id"] = _Id;
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

                return Json(new { Error = false, Message = AplosMessage.Updated, Id = _Id });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public ActionResult Delete(string id)
        {
            string strUSQL, talkingSQL, suggestionSQL, actionSQL, meetingSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                talkingSQL = "delete from dbo.MeetingTalkingPoint Where MeetingItemHeaderId='" + id + "'";
                suggestionSQL = "delete from dbo.MeetingSuggestion Where MeetingItemHeaderId='" + id + "'";
                actionSQL = "delete from dbo.MeetingActionablePoints Where MeetingItemHeaderId='" + id + "'";
                meetingSQL = "delete from dbo.MeetingDecision Where MeetingItemHeaderId='" + id + "'";
                strUSQL = "delete dbo.MeetingItemHeader Where Id='" + id + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(talkingSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(suggestionSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(actionSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(meetingSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strUSQL, true, "1");
                objCon.CommitTransaction();

                return Json(new { Message = AplosMessage.Deleted });
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
            //dr["UpdatedBy"] = identity.Name;
            //dr["UpdatedDate"] = System.DateTime.Now.ToString();
            //dr["UpdatedFromIP"] = identity.IPAddress;

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


        [Authorize, HttpGet]
        public JsonResult GetEmployeeListByWhom(GridParameter parameters, string plantId, string partyAccountGroupId, string partyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(plantId))
            {
                plantId = identity.PlantId;
            }
            return Json(GetEmployeeListByWhom(parameters, identity.CompanyId, plantId, partyAccountGroupId, partyId), JsonRequestBehavior.AllowGet);
        }

        public GridModel GetEmployeeListByWhom(GridParameter parameters, string companyId, string plantId, string partyAccountGroupId, string partyId)
        {
            try
            {
                parameters.CmdText = @"SELECT EI.SystemId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [Designation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN HKP.Designation AS DEG ON DEG.Id=EI.DesignationSystemID
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            WHERE EI.CompanyId='" + companyId + "' AND EI.PlantId='" + plantId + "' AND EI.EmployeeStatus='Active'";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #region upload Production Bulletin picture
        [HttpPost, Authorize]

        public ActionResult SaveMeetingPointsDefault(IEnumerable<HttpPostedFileBase> UploadDefault, string UploadDefault_data)
        {
            try
            {
                UploadDefault_data = UploadDefault_data.Replace("\\", "");

                DataTable AdditionalData = CustomJsonResult.ToDataTable(UploadDefault_data);


                foreach (var file in UploadDefault)
                {

                    //var fileName = Path.GetFileName(UploadDefault_data + new FileInfo(file.FileName).Extension);
                    var fileName = Path.GetFileName(AdditionalData.Rows[0]["Id"].ToString() + new FileInfo(file.FileName).Extension);
                    var fileN = file.FileName;
                    var destinationPath = Path.Combine(ResourcesPathReader.GetMeetingPointsImagePath(), fileName);

                    var directory = ResourcesPathReader.GetMeetingPointsImagePath();
                    var path = Path.Combine(directory);

                    if (System.IO.Directory.Exists(ResourcesPathReader.GetMeetingPointsImagePath()) == false)
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(ResourcesPathReader.GetMeetingPointsImagePath());
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }


                    ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                    string sql = "select * from MeetingItemHeader where id='" + AdditionalData.Rows[0]["Id"].ToString() + "'";
                    DataSet dsLocal = null;
                    connection.BeginTransaction();
                    connection.getDataSet(sql, out dsLocal);
                    connection.CommitTransaction();
                    var FN = dsLocal.Tables[0].Rows[0]["Attachment"].ToString();
                    if (fileN != FN)
                        if (System.IO.File.Exists(path + AdditionalData.Rows[0]["Id"].ToString() + Path.GetExtension(FN)))
                            System.IO.File.Delete(path + AdditionalData.Rows[0]["Id"].ToString() + Path.GetExtension(FN));
                    if (dsLocal.Tables[0].Rows.Count > 0)
                    {
                        dsLocal.Tables[0].Rows[0].BeginEdit();

                        dsLocal.Tables[0].Rows[0]["Attachment"] = fileN;

                        dsLocal.Tables[0].Rows[0].EndEdit();

                        file.SaveAs(destinationPath);
                        clsStaticInfo info = new clsStaticInfo();
                        info.SaveDataSets(dsLocal);



                    }
                }
                return Content("");
            }
            catch (Exception ex)
            {
                HttpResponse Response = System.Web.HttpContext.Current.Response;
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = 204;
                Response.Status = "204 No Content";
                Response.StatusDescription = ex.Message;
                Response.End();

                return Content("");
            }

        }
        [HttpPost, Authorize]
        public ActionResult GetFileInfo(string Id)
        {

            try
            {
                return Json(_sqlRepository.GetDataCollection("SELECT Attachment FROM MeetingItemHeader WHERE Id ='" + Id + "' "), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        #endregion upload product picture


        [HttpPost, Authorize]
        public JsonResult CreateTalkingPoint(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from MeetingTalkingPoint where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "MeetingTalkingPoint", out _Id);

                    data["Id"] = _Id;

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


                return Json(new { Error = false, Message = AplosMessage.Updated, Id = _Id });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateSuggestionsRecommendation(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from MeetingSuggestion where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "MeetingSuggestion", out _Id);

                    data["Id"] = _Id;
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


                return Json(new { Error = false, Message = AplosMessage.Updated, Id = _Id });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateActionablePoints(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from MeetingActionablePoints where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "MeetingActionablePoints", out _Id);

                    data["Id"] = _Id;
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


                return Json(new { Error = false, Message = AplosMessage.Updated, Id = _Id });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }


        [HttpPost, Authorize]
        public JsonResult CreateMeetingDecision(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from MeetingDecision where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "MeetingDecision", out _Id);

                    data["Id"] = _Id;
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


                return Json(new { Error = false, Message = AplosMessage.Updated, Id = _Id });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateExpectedPerson(List<Dictionary<string, object>> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from MeetingExpectedPerson", out dsMaster, false, "1");

                string _Id = "";

                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsMaster.Tables[0]);
                        dv.RowFilter = "ExpectedPersonId='" + item["ExpectedPersonId"] + "'";

                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "MeetingExpectedPerson", out _Id);

                        if (dv.Count == 0)
                        {
                            item["Id"] = _Id;
                            AddNewRow(dsMaster.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);


                return Json(new { Error = false, Message = AplosMessage.Updated, Id = _Id });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost, Authorize]
        public ActionResult GetTalkingPointList(string column, string value, string meetingItemHeaderId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (select MTP.*,EI.EmployeeName ByWhomName,EI.EmployeeCode ByWhomCode 
                                                from MeetingTalkingPoint MTP
                                                left join EmployeeInformation EI on EI.SystemId=MTP.ByWhomId
                                                WHERE MeetingItemHeaderId = '" + meetingItemHeaderId + @"'
                                                ) AS TEMP WHERE " + strkey;

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetSuggestionsRecommendationList(string column, string value, string meetingItemHeaderId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (select MS.*,EI.EmployeeName ByWhomName,EI.EmployeeCode ByWhomCode
                                                from MeetingSuggestion MS
                                                left join EmployeeInformation EI on EI.SystemId=MS.ByWhomId
                                                WHERE MeetingItemHeaderId = '" + meetingItemHeaderId + @"'
                                                ) AS TEMP WHERE " + strkey;

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public ActionResult GetActionablePointsList(string column, string value, string meetingItemHeaderId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (select MAP.*,FORMAT(MAP.TargetDate,'dd-MMM-yyyy') TDate,EI.EmployeeName ByWhomName,EI.EmployeeCode ByWhomCode
                                                from MeetingActionablePoints MAP
                                                left join EmployeeInformation EI on EI.SystemId=MAP.ByWhomId
                                                WHERE MeetingItemHeaderId = '" + meetingItemHeaderId + @"'
                                                ) AS TEMP WHERE " + strkey;

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetMeetingDecisionList(string column, string value, string meetingItemHeaderId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (select MD.*,EI.EmployeeName ByWhomName,EI.EmployeeCode ByWhomCode
                                                from MeetingDecision MD
                                                left join EmployeeInformation EI on EI.SystemId=MD.ByWhomId
                                                WHERE MeetingItemHeaderId = '" + meetingItemHeaderId + @"'
                                                ) AS TEMP WHERE " + strkey;

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetExpectedPersonList(string column, string value, string meetingItemHeaderId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (select MEP.Id,MEP.MeetingItemHeaderId,MEP.ExpectedPersonId,EI.EmployeeName ExpectedPerson,EI.*,D.UserName Designation,Dep.UserName Department,Div.UserName Division
                                                    ,EC.UserName EmployeeCategory,S.UserName Section,SS.UserName SubSection,L.UserName Line
                                                    from MeetingExpectedPerson MEP
                                                    left join EmployeeInformation EI on EI.SystemId=MEP.ExpectedPersonId
left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
							left join ORG.Position P on P.Id=MPB.PositionID
                                                    left join HKP.Designation D on D.Id=P.DesignationID
                                                    left join ORG.Department Dep on Dep.Id=p.DepartmentId
                                                    left join ORG.Division Div on Div.Id=p.DivisionId
                                                    LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
                            LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
                                                    left join ORG.Section S on S.Id=p.SectionId
                                                    left join ORG.SubSection SS on SS.Id=p.SubSectionId
                                                    left join ORG.Line L on L.Id=MPB.LineId
                                                    WHERE MeetingItemHeaderId = '" + meetingItemHeaderId + @"'
                                                ) AS TEMP WHERE " + strkey;

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult DeleteTalkingPointData(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select File first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from MeetingTalkingPoint where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult DeleteSuggestionsRecommendationData(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select File first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from MeetingSuggestion where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult DeleteActionablePointData(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select File first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from MeetingActionablePoints where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult DeleteMeetingDecisionData(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select File first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from MeetingDecision where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult DeleteExpectedPersonData(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select File first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from MeetingExpectedPerson where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet, Authorize]
        public ActionResult GetEmployeeInformation(string criteria)
        {
            string sql = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var data = GetEmpInfo(identity.CompanyGroupId, identity.PlantId, criteria);

                //return Json(new { LeaveInfo = data }, JsonRequestBehavior.AllowGet);

                JsonResult LeaveInfo = Json(data, JsonRequestBehavior.AllowGet);
                LeaveInfo.MaxJsonLength = int.MaxValue;
                return LeaveInfo;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        public List<Dictionary<string, object>> GetEmpInfo(string companyGroupId, string plantId, string criteria)
        {

            try
            {

                var cmdText = @"SELECT * FROM (  SELECT DISTINCT [CheckBoxSelect] = Convert(bit, 'false'),
                                     isnull(e.SystemId,'') SystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId                                     
                                    , EmployeeCode
                                    ,ISNULL(e.EmployeeName,'') EmployeeName								
                                    ,ISNULL(mpb.EntityId,'') EntityId
									,ISNULL(mpb.PositionId,'') PositionId                                     
                                    ,isnull(ISNULL(egdsg.UserName,ld.UserName),'') Designation                                       
									,ISNULL(Department.UserName,'') Department 
									,ISNULL(Division.UserName,'') Division 
									,ISNULL(EmpC.UserName,'') EmployeeCategory
									,ISNULL(Plant.UserName,'') Plant 
									,ISNULL(Section.UserName,'') Section 
									,ISNULL(SubSection.UserName,'') SubSection 
									,ISNULL(Unit.UserName,'') Unit 
                                    ,ISNULL(eL.UserName,'') Line
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus , e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                    ,E.PlantId
								  
                                    FROM EmployeeInformation e

                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                    LEFT OUTER JOIN ORG.Department edept on edept.id=e.DepartmentId
                                    LEFT OUTER JOIN ORG.Line eL on eL.id=mpb.LineId
                                    LEFT OUTER JOIN ORG.Division ediv on ediv.id=e.DivisionId
                                    LEFT OUTER JOIN ORG.SubDivision esdiv on esdiv.id=e.SubDivisionId
                                    LEFT OUTER JOIN ORG.Section es on es.id=PO.SectionId
                                    LEFT OUTER JOIN ORG.SubSection ess on ess.id=PO.SubSectionId
                                    LEFT OUTER JOIN ORG.Plant ep on ep.id=e.PlantId
                                    LEFT OUTER JOIN ORG.Unit eu on eu.id=e.UnitId
                                    LEFT OUTER JOIN HKP.Designation edsg on edsg.id=e.DesignationSystemID
                                    LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
									LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=e.LegalDesignationId
                                    LEFT OUTER JOIN (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									,dg.UserName GivenDesignationGroup--,srm.SalaryRuleName
									FROM mst.DesignationMaster dm
									LEFT OUTER JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
									) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
									AND egdsgg.EmployeeCategoryId=e.EmployeeCategorySystemID
                                  
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId
                                    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.LegalDesignationId
                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId			                                       
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId

                                  WHERE e.plantId='" + plantId + @"' and e.GroupID='" + companyGroupId + @"' and e.EmployeeStatus='Active'
                                     ) DD ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";

                return _sqlRepository.GetDataCollection(cmdText);

            }
            catch (Exception)
            {
                throw;
            }
        }//end of function

    }
}