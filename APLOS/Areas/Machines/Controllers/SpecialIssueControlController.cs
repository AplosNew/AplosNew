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

namespace Aplos.Areas.Machines.Controllers
{
    public class SpecialIssueControlController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        public SpecialIssueControlController(ISqlRepository R)
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


        [Authorize, HttpGet]
        public JsonResult GetCategoryList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"select Id as Value,UserName as Text from HKP.TaskCategory where Flag='ToDo'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSubCategoryList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"select Id as Value,UserName as Text from HKP.TaskSubCategory where Flag='ToDo'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT EI.SystemId as SystemId, MB.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=MB.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=P.DepartmentId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=P.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=P.SubSectionId
                            WHERE EI.EmployeeStatus='Active'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetActionTakenBy()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT EI.SystemId as SystemId, MB.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=MB.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=P.DepartmentId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=P.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=P.SubSectionId
                            WHERE EI.EmployeeStatus='Active'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadSpecialIssueMasterList()
         {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT *,format(SIC.TargetDate,'dd-MMM-yyyy') as TDate,
(select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=SIC.ResponsiblePersonId) as ResponsiblePerson
 FROM TRN.SpecialIssueControl SIC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult Create(Dictionary<string, object> IssueData)
        {
            try
            {
                    ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                    conRack.OpenDataSetThroughAdapter("select * from [TRN].[SpecialIssueControl] where Id<>'" + IssueData["Id"] + "'", out DataSet dsSpecialIssueControlValidation, false, "1");

                    DataSet dsSpecialIssueControl;

                    conRack = new ConnectionManager.DAL.ConManager("1");
                    conRack.OpenDataSetThroughAdapter("select * from [TRN].[SpecialIssueControl] where Id='" + IssueData["Id"] + "'", out dsSpecialIssueControl, false, "1");
                    string _Id = "";

                    #region data update
                    if (dsSpecialIssueControl.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("SpecialIssueControl", out _Id);
                        _Id = "SIC" + _Id;
                        IssueData["Id"] = _Id;
                        AddNewRow(dsSpecialIssueControl.Tables[0], IssueData);
                    }
                    else
                    {
                        _Id = IssueData["Id"].ToString();
                        EditRow(dsSpecialIssueControl.Tables[0].Rows[0], IssueData);
                    }
                    #endregion data update



                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsSpecialIssueControl);

                    return Json(new { Error = false, Data = IssueData, Message = AplosMessage.Insert });

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

        [Authorize, HttpPost]
        public ActionResult IssueDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                DataSet ItemCount;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SpecialIssueItem] where SpecialIssueControlId='" + id + "'", out ItemCount, false, "1");


                if (ItemCount.Tables[0].Rows.Count == 0)
                {

                    conC.BeginTransaction();
                    conC.executeQuery("delete from TRN.SpecialIssueControl where Id ='" + id + @"'");
                    conC.CommitTransaction();
                }
                else
                {
                    throw new Exception("Transaction are Exists!");
                }
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpPost]
        public ActionResult ItemDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [TRN].[SpecialIssueItem] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult LoadItemDetails(string IssueId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT *,
(select EmployeeName from EmployeeInformation where SystemId=ActiontakenById) as ActiontakenBy
FROM [TRN].[SpecialIssueItem] where SpecialIssueControlId ='" + IssueId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadPeriodDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT *,(select SD.UserName from ShiftDefination SD where SD.SystemID=Shift) as ShiftName,Format(Time,'hh:mm:tt') as Times FROM [MST].[SpecialIssueDefinePeriod]";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadIssueEditData(string IssueID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @" SELECT *,format(SIC.TargetDate,'dd-MMM-yyyy') as TDate,
(select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=SIC.ResponsiblePersonId) as ResponsiblePerson
 FROM [TRN].[SpecialIssueControl] SIC where SIC.Id='" + IssueID + @"'";
            return Json(new { issue = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }
        
        [Authorize, HttpGet]
        public ActionResult LoadItemEditData(string ItemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT *,
(select EmployeeName from EmployeeInformation where SystemId=ActiontakenById) as ActiontakenBy
FROM [TRN].[SpecialIssueItem] where Id='" + ItemId + @"'";
            return Json(new { item = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadPeriodEditData(string PeriodId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT *,(select SD.UserName from ShiftDefination SD where SD.SystemID=Shift) as ShiftName,Format(Time,'hh:mm:tt') as Times FROM [MST].[SpecialIssueDefinePeriod] where Id ='" + PeriodId + @"'";
            return Json(new { Period = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult CreateItem(Dictionary<string, object> ItemData, string Pid)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SpecialIssueItem] where Id<>'" + ItemData["Id"] + "'", out DataSet dsSpecialIssueItemValidation, false, "1");

                DataSet dsSpecialIssueItem;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SpecialIssueItem] where Id='" + ItemData["Id"] + "'", out dsSpecialIssueItem, false, "1");
                string _Id = "";

                #region data update
                if (dsSpecialIssueItem.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("SpecialIssueItem", out _Id);
                    _Id = "SII" + _Id;
                    ItemData["Id"] = _Id;
                    ItemData["SpecialIssueControlId"] = Pid;
                    AddNewRow(dsSpecialIssueItem.Tables[0], ItemData);
                }
                else
                {
                    _Id = ItemData["Id"].ToString();
                    ItemData["SpecialIssueControlId"] = Pid;
                    EditRow(dsSpecialIssueItem.Tables[0].Rows[0], ItemData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsSpecialIssueItem);

                return Json(new { Error = false, Data = ItemData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost, Authorize]
        public JsonResult CreatePeriod(Dictionary<string, object> PeriodData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[SpecialIssueDefinePeriod] where Id<>'" + PeriodData["Id"] + "'", out DataSet dsSpecialIssueDefinePeriodValidation, false, "1");

                DataSet dsSpecialIssueDefinePeriod;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[SpecialIssueDefinePeriod] where Id='" + PeriodData["Id"] + "'", out dsSpecialIssueDefinePeriod, false, "1");
                string _Id = "";

                #region data update
                if (dsSpecialIssueDefinePeriod.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("SpecialIssueDefinePeriod", out _Id);
                    _Id = "SIDP" + _Id;
                    PeriodData["Id"] = _Id;
                    AddNewRow(dsSpecialIssueDefinePeriod.Tables[0], PeriodData);
                }
                else
                {
                    _Id = PeriodData["Id"].ToString();
                    EditRow(dsSpecialIssueDefinePeriod.Tables[0].Rows[0], PeriodData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsSpecialIssueDefinePeriod);

                return Json(new { Error = false, Data = PeriodData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        #endregion -- Operations
    }
}