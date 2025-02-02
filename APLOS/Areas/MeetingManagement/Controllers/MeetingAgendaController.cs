#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Setups;
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
    public class MeetingAgendaController : BaseController
    {


        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public MeetingAgendaController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM MeetingAgenda"), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (select MA.* ,FORMAT(MA.Date,'dd-MMM-yyyy') TDate,EI.EmployeeName MeetingOrganizedBy,EI.EmployeeCode MeetingOrganizedByCode
			                                   ,EID.EmployeeName ChairedBy,EID.EmployeeCode ChairedByCode
                                                from MeetingAgenda MA
                                                left join EmployeeInformation EI on EI.SystemId=MA.MeetingOrganizedById
                                                left join EmployeeInformation EID on EID.SystemId=MA.ChairedById) AS TEMP WHERE " + strkey;



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data, List<Dictionary<string, object>> MeetingData)
        {
            try
            {
                DataSet dsMeeting, dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from MeetingAgenda where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                string _Id = "";
                string MasterId = string.Empty;
                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "MeetingAgenda", out _Id);

                    data["Id"] = _Id;
                    MasterId = data["Id"].ToString();
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    MasterId = _Id;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                string id = "";
                for (int i = 0; i < MeetingData.Count; i++)
                {
                    if (id == "")
                    {
                        id = "'" + MeetingData[i]["Id"] + "'";
                    }
                    else
                    {
                        id += ",'" + MeetingData[i]["Id"] + "'";
                    }
                }

                con.OpenDataSetThroughAdapter("select * from MeetingAgendaItem where Id in (" + id + ")", out dsMeeting, false, "1");

                string MeetingId = "";
                for (int i = 0; i < MeetingData.Count; i++)
                {

                    dsMeeting.Tables[0].DefaultView.RowFilter = "Id='" + MeetingData[i]["Id"] + @"'";
                    if (dsMeeting.Tables[0].DefaultView.Count > 0)
                    {
                        //edit
                        DataRow dr = dsMeeting.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["MeetingAgendaId"] = MasterId;
                        dr["MeetingItemHeaderId"] = MeetingData[i]["Id"];
                        dr.EndEdit();
                    }
                    else
                    {
                        //addnew
                        
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID("MeetingAgendaItem", out MeetingId);
                        
                        DataRow dr = dsMeeting.Tables[0].NewRow();

                        dr["Id"] = "M-" + MeetingId + "-" + (i + 1);
                        dr["MeetingAgendaId"] = MasterId;
                        dr["MeetingItemHeaderId"] = MeetingData[i]["Id"];
                        dr["MeetingTypeId"] = MeetingData[i]["MeetingTypeId"];
                        dr["IssueStatus"] = MeetingData[i]["IssueStatus"];
                        dr["IssueCritically"] = MeetingData[i]["IssueCritically"];
                        dr["DepartmentId"] = MeetingData[i]["DepartmentId"];
                        dr["AttendeeId"] = MeetingData[i]["AttendeeId"];

                        dsMeeting.Tables[0].Rows.Add(dr);

                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsMeeting);
                return Json(new { Error = false, Message = AplosMessage.Updated, Id= _Id });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult Delete(string id)
        {
            string strUSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                //talkingSQL = "delete from dbo.MeetingTalkingPoint Where MeetingItemHeaderId='" + id + "'";
                //suggestionSQL = "delete from dbo.MeetingSuggestion Where MeetingItemHeaderId='" + id + "'";
                //actionSQL = "delete from dbo.MeetingActionablePoints Where MeetingItemHeaderId='" + id + "'";
                //meetingSQL = "delete from dbo.MeetingDecision Where MeetingItemHeaderId='" + id + "'";
                strUSQL = "delete dbo.MeetingAgenda Where Id='" + id + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                //objCon.ExecuteNonQueryWrapper(talkingSQL, true, "1");
                //objCon.ExecuteNonQueryWrapper(suggestionSQL, true, "1");
                //objCon.ExecuteNonQueryWrapper(actionSQL, true, "1");
                //objCon.ExecuteNonQueryWrapper(meetingSQL, true, "1");
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
                parameters.CmdText = @"SELECT EI.SystemId, MB.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [Designation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN HKP.Designation AS DEG ON DEG.Id=EI.GivenDesignationID
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

        [HttpGet, Authorize]
        public ActionResult getFilters()
        {
            try
            {
                var sql = @"SELECT * FROM (select MT.Id MeetingTypeId,MT.UserName MeetingType,MIH.IssueStatus,MIH.IssueCritically,D.Id DepartmentId,D.UserName Department,EI.SystemId AttendeeId,EI.EmployeeName Attendee
			                                    
                                                from MeetingItemHeader MIH
                                                left join EmployeeInformation EI on EI.SystemId=MIH.ByWhomId
												left join ORG.Department D on D.Id=MIH.DepartmentId
                                                left join MeetingType MT on MT.Id=MIH.MeetingTypeId) AS KK	";
               
                //return _sqlRepository.GetDataCollection(sql);
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        [HttpPost, Authorize]
        public ActionResult GetMeetingInformation(Dictionary<string, string> parameters, string toDate, string fromDate)
        {
            try
            {
                    string sql = @"Select cast(0 as bit) Active,MIH.*,D.UserName Department,MT.UserName MeetingType,EI.SystemId AttendeeId,EI.EmployeeName Attendee 
                                from MeetingItemHeader MIH
                                left join MeetingType MT on MT.Id=MIH.MeetingTypeId
                                left join ORG.Department D on D.Id=MIH.DepartmentId
                                left join EmployeeInformation EI on EI.SystemId=MIH.ByWhomId

                                where MeetingTypeId in (" + parameters["MeetingTypeId"] + @") and IssueStatus in (" + parameters["IssueStatus"] + @") 
                                and IssueCritically in (" + parameters["IssueCritically"] + @") and MIH.DepartmentId in (" + parameters["DepartmentId"] + @") 
                                and ByWhomId in (" + parameters["AttendeeId"] + @") and Format(MIH.AddedDate,'dd-MMM-yyyy')  between '" + fromDate + "' and '" + toDate + "'";


                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                }
            catch (Exception e)
            {
                throw e;
            }
        }
        
        [HttpGet, Authorize]
        public ActionResult GetDateInformation()
        {
            try
            {
                var sql = @"Select FORMAT(MIN(AddedDate),'dd-MMM-yyyy') FromDate,FORMAT(MAX(AddedDate),'dd-MMM-yyyy') ToDate
                            from dbo.MeetingItemHeader";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}