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
    public class SpecialIssueControlUpdateController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        public SpecialIssueControlUpdateController(ISqlRepository R)
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
        public JsonResult GetShiftList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"select SystemID as Value,UserName as Text from ShiftDefination where IsActive=1";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult GetIssue()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"SELECT SIC.Id,TC.UserName as Category,TSC.UserName as SubCategory,SIC.SpecialIssueName,SIC.SpecialIssueDetails,SIC.Remarks,
format(SIC.TargetDate,'dd-MMM-yyyy') as TDate,MonitoringPeriod as MonitoringPeriods,
(select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=SIC.ResponsiblePersonId) as ResponsiblePerson
 FROM TRN.SpecialIssueControl SIC
 left join HKP.TaskCategory TC On TC.Id=SIC.Category
 left join HKP.TaskSubCategory TSC On TSC.Id=SIC.SubCategory where IssueStatus<>'Close'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT EI.SystemId as SystemId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
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
        public ActionResult LoadIssueControlUpdateList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT *,format(ICU.Date,'dd-MMM-yyyy') as [IssueUpdateDate],
(select SD.UserName from ShiftDefination SD where SD.SystemID=ICU.Shift) as [ShiftName],
(select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=ICU.ShiftInchargeId) as ShiftInCharge,
(select SIC.SpecialIssueName from TRN.SpecialIssueControl SIC where SIC.Id=ICU.IssueId) as Issue,
format(ICU.Time,'hh:mm tt') as IssueTime
 FROM TRN.SpecialIssueControlUpdate ICU";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadICUEditData(string ICUId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @" SELECT *,format(ICU.Date,'dd-MMM-yyyy') as [IssueUpdateDate],
(select SD.UserName from ShiftDefination SD where SD.SystemID=ICU.Shift) as [ShiftName],
(select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=ICU.ShiftInchargeId) as ShiftInCharge,
(select SIC.SpecialIssueName from TRN.SpecialIssueControl SIC where SIC.Id=ICU.IssueId) as Issue,
format(ICU.Time,'hh:mm tt') as IssueTime
 FROM TRN.SpecialIssueControlUpdate ICU where ICU.Id='" + ICUId + @"'";
            return Json(new { issueupdate = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult Create(Dictionary<string, object> IssueUpdateData)
        {
            try
            {
                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SpecialIssueControlUpdate] where Id<>'" + IssueUpdateData["Id"] + "'", out DataSet dsSpecialIssueControlUpdateValidation, false, "1");

                DataSet dsSpecialIssueControlUpdate;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SpecialIssueControlUpdate] where Id='" + IssueUpdateData["Id"] + "'", out dsSpecialIssueControlUpdate, false, "1");
                string _Id = "";

                #region data update
                if (dsSpecialIssueControlUpdate.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("SpecialIssueControlUpdate", out _Id);
                    _Id = "ICU" + _Id;
                    IssueUpdateData["Id"] = _Id;
                    AddNewRow(dsSpecialIssueControlUpdate.Tables[0], IssueUpdateData);
                }
                else
                {
                    _Id = IssueUpdateData["Id"].ToString();
                    EditRow(dsSpecialIssueControlUpdate.Tables[0].Rows[0], IssueUpdateData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsSpecialIssueControlUpdate);

                return Json(new { Error = false, Data = IssueUpdateData, Message = AplosMessage.Insert });

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

        [Authorize, HttpGet]
        public ActionResult LoadIssueItemDetailsList(string IssueId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"Select SII.Id as SICItemId,
 SII.SpecialIssueItem,SII.Actiontaken,
 (select EmployeeName from EmployeeInformation where SystemId=SII.ActiontakenById) as ActiontakenBy,
 SII.SampleSize,'' Value,'' Remarks,'' ConfidenceLevel,'' Id,'' ICUId  from TRN.SpecialIssueItem SII
 where SII.SpecialIssueControlId='" + IssueId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult CreateItem(List<Dictionary<string, object>> DataList, string Pid)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[TRN].[SpecialIssueUpdateItem]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "'", out dsProdBooked, false, "1");
                        objCon.OpenDataSetThroughAdapter("SELECT * from " + TableName + "  where SICItemId='" + item["SICItemId"] + "' and Value < '" + item["Value"] + "'", out DataSet dsSpecialIssueUpdateItemValidation, false, "1");
                        objCon.OpenDataSetThroughAdapter("SELECT * from TRN.SpecialIssueItem  where Id= '" + item["SICItemId"] + "' and SampleSize<" + item["Value"] + "", out DataSet dsSampleSizeValidation, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            if (dsSampleSizeValidation.Tables[0].Rows.Count > 0)
                            {
                                
                                objCon.BeginTransaction();
                                objCon.executeQuery("delete from TRN.SpecialIssueUpdateItem where ICUId = '" + Pid + @"'");
                                objCon.executeQuery("delete from TRN.SpecialIssueControlUpdate where Id ='" + Pid + @"'");
                                objCon.CommitTransaction();
                                throw new Exception("Value should not be exceed more than Sample Size");
                            }
                            else
                            {
                                if (dsSpecialIssueUpdateItemValidation.Tables[0].Rows.Count > 0)
                                {

                                    if (item["Remarks"].ToString() == "")
                                    {
                                        objCon.BeginTransaction();
                                        objCon.executeQuery("delete from TRN.SpecialIssueUpdateItem where ICUId = '" + Pid + @"'");
                                        objCon.executeQuery("delete from TRN.SpecialIssueControlUpdate where Id ='" + Pid + @"'");
                                        objCon.CommitTransaction();
                                        throw new Exception("Please add remarks and proceed");
                                    }
                                    else
                                    {
                                        bplib.clsGenID genid = new bplib.clsGenID();
                                        genid.GenID(TableName, out _Id);
                                        item["Id"] = "SIUI" + _Id;
                                        item["ICUId"] = Pid;
                                        AddNewRow(dsProdBooked.Tables[0], item);

                                    }

                                }
                                else
                                {
                                    bplib.clsGenID genid = new bplib.clsGenID();
                                    genid.GenID(TableName, out _Id);
                                    item["Id"] = "SIUI" + _Id;
                                    item["ICUId"] = Pid;
                                    AddNewRow(dsProdBooked.Tables[0], item);

                                }
                            }
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                            clsStaticInfo obj = new clsStaticInfo();
                            obj.SaveDataSets(dsProdBooked);
                        }
                    }
                    return Json(new { Message = AplosMessage.Insert });

                }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        #endregion -- Operations
    }
}