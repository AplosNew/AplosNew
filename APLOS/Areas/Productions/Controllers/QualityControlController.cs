#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Service.Productions;
using Library.Core;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using System.Collections.Generic;
using Library.Model.Productions.ProductionBooking;
using Library.Data.Sql;
using Library.OrderManagement.Production;
using System;
using System.Data;
using Library.Security.Core;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.IO;
using System.Linq;
using Library.Model.Enums;
using System.Drawing;
using Library.Service.Systems;
using Library.Service.HumanResources;
using Library.HumanResource.NewAttendanceProcess;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class QualityControlController : BaseController
    {
        private readonly IAttendanceManagementService _AttendanceManagementService;
        ProductionSummaryData _productionSummaryData = new ProductionSummaryData();
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;
        #region Constructor
        /// <summary>   The ProductionSummaryService service. </summary>
        private readonly IProductionSummaryService _ProductionSummaryService;

        public QualityControlController(IProductionSummaryService ProductionSummaryService, IAttendanceManagementService AttendanceManagementService, ISqlRepository sqlRepository, IPKGeneratorService pkGeneratorService)
        {
            _AttendanceManagementService = AttendanceManagementService;
            _ProductionSummaryService = ProductionSummaryService;
            _sqlRepository = sqlRepository;
            _pkGeneratorService = pkGeneratorService;
        }
        #endregion

        #region -- Pages


        public ActionResult Aplos()
        {
            return View();
        }
        public ActionResult AplosWC()
        {
            return View();
        }
        //public ActionResult AplosSFG()
        //{
        //    return View();
        //}

        public ActionResult AplosInOut()
        {
            return View();
        }


        public ActionResult Reject()
        {
            return View();
        }

        public ActionResult Report()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [Authorize, HttpPost]
        public ActionResult GetGIEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select distinct EI.SystemId,EI.EmployeeName, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    ,EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection  from 
TRN.QualityIssueControl QIC
left join dbo.EmployeeInformation EI on EI.SystemId=QIC.QGIEmployeeId
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=mb.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=p.DepartmentId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=p.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=p.SubSectionId
where EI.EmployeeStatus='Active' and EI.EmployeeCode is not null and QIC.QGIEmployeeId is not null 
and QIC.QCId is null";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetPIEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select distinct EI.SystemId,EI.EmployeeName, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    ,EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection  from 
TRN.QualityPlanControl QPC
left join dbo.EmployeeInformation EI on EI.SystemId=QPC.QPEmployeeId
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=mb.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=p.DepartmentId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=p.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=p.SubSectionId
where EI.EmployeeStatus='Active' and EI.EmployeeCode is not null and QPC.QPEmployeeId is not null 
and QPC.QCId is null";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetProcessIssueList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,UserName as Text from HKP.Process where Active=1";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetDepartment()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select D.Id DepartmentId,D.Code,D.Sequence,D.ShortName,D.StandardName
						                ,D.UserName DepartmentName,D.Description,D.Remarks 
						                from ORG.Department D";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetPositionCode()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select P.Id,P.Code,P.UserName Position,P.Activity,
DEP.UserName AS Department,S.UserName as Section,SS.UserName as SubSection
from ORG.Position P	
LEFT JOIN ORG.Department AS DEP ON DEP.Id=P.DepartmentId
LEFT OUTER JOIN ORG.Section S ON S.Id=P.SectionId
LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=P.SubSectionId
left outer join MST.DesignationMaster DM ON DM.DesignationId=P.DesignationId
where P.Active = 1";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public decimal GetItemAutoSequence()
        {
            try
            {
                DataTable dt = _sqlRepository.GetDataTable("SELECT isnull(Max(SNO),0) AS SNO FROM [MST].[QualityIssueItem]");
                if (dt.Rows.Count > 0)
                    return (decimal)clsStaticInfo.dbl(dt.Rows[0]["SNO"].ToString()) + 1;

                return 1;
            }
            catch (Exception ex)
            {
                return 1.00M;
            }
        }

        [Authorize, HttpGet]
        public decimal GetGradeAutoSequence()
        {
            try
            {
                DataTable dt = _sqlRepository.GetDataTable("SELECT isnull(Max(SNO),0) AS SNO FROM [MST].[QualityGradeDetails]");
                if (dt.Rows.Count > 0)
                    return (decimal)clsStaticInfo.dbl(dt.Rows[0]["SNO"].ToString()) + 1;

                return 1;
            }
            catch (Exception ex)
            {
                return 1.00M;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetIssueReasonList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,IssueName as Text from [MST].[QualityIssueDetails]";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadIssueDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select ID.*,(select P.UserName from HKP.Process P where P.Id=ID.ProcessId) as Process,
(select D.UserName from ORG.Department D where D.Id=ID.DepartmentId) as Department,
(select P.Code from ORG.Position P where P.Id=ID.PositionCodeId) as PositionCode
from [MST].[QualityIssueDetails] ID";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadIssueDetailsEditData(string IssueId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select ID.*,(select P.UserName from HKP.Process P where P.Id=ID.ProcessId) as Process,
(select D.UserName from ORG.Department D where D.Id=ID.DepartmentId) as Department,
(select P.Code from ORG.Position P where P.Id=ID.PositionCodeId) as PositionCode
from [MST].[QualityIssueDetails] ID where ID.Id='" + IssueId + @"'";
            return Json(new { Issue = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetUOM()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select UM.Id UOMId, UM.Code,UM.StandardName, UM.UserName UOM from scs.UnitOfMeasurement UM where UM.Active = 1";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetParameter()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select PM.Id ParameterId, PM.Code,PM.StandardName, PM.UserName Parameter from HKP.ParameterMaster PM where PM.IsActive=1";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult createIssue(Dictionary<string, object> IssueData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityIssueDetails] where IssueName='" + IssueData["IssueName"] + "' and ProcessId='" + IssueData["ProcessId"] + "'", out DataSet dsItemDetailsIssueNameValidation, false, "1");

                DataSet dsIssueDetails;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityIssueDetails] where Id='" + IssueData["Id"] + "'", out dsIssueDetails, false, "1");
                string _Id = "";

                #region data update
                if (dsIssueDetails.Tables[0].Rows.Count == 0)
                {
                    if (dsItemDetailsIssueNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Issue Name Already Exist.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("QualityIssueDetails", out _Id);
                        _Id = "QID" + _Id;
                        IssueData["Id"] = _Id;
                        AddNewRow(dsIssueDetails.Tables[0], IssueData);
                    }
                }
                else
                {
                    _Id = IssueData["Id"].ToString();
                    EditRow(dsIssueDetails.Tables[0].Rows[0], IssueData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsIssueDetails);

                return Json(new { Error = false, Data = IssueData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public ActionResult IssueDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [MST].[QualityIssueDetails] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult LoadReasonDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select *,(select ID.IssueName from [MST].[QualityIssueDetails] ID where ID.Id=RD.IssueId) as IssueName from [MST].[QualityReasonDetails] RD";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadReasonDetailsEditData(string ReasonId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select *,(select ID.IssueName from [MST].[QualityIssueDetails] ID where ID.Id=RD.IssueId) as IssueName from [MST].[QualityReasonDetails] RD where RD.Id='" + ReasonId + @"'";
            return Json(new { Reason = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult createReason(Dictionary<string, object> ReasonData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityReasonDetails] where ReasonName='" + ReasonData["ReasonName"] + "' and IssueId='" + ReasonData["IssueId"] + "'", out DataSet dsItemDetailsReasonNameValidation, false, "1");

                DataSet dsReasonDetails;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityReasonDetails] where Id='" + ReasonData["Id"] + "'", out dsReasonDetails, false, "1");
                string _Id = "";

                #region data update
                if (dsReasonDetails.Tables[0].Rows.Count == 0)
                {
                    if (dsItemDetailsReasonNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Reason Name Already Exist.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("QualityReasonDetails", out _Id);
                        _Id = "QRD" + _Id;
                        ReasonData["Id"] = _Id;
                        AddNewRow(dsReasonDetails.Tables[0], ReasonData);
                    }
                }
                else
                {
                    _Id = ReasonData["Id"].ToString();
                    EditRow(dsReasonDetails.Tables[0].Rows[0], ReasonData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsReasonDetails);

                return Json(new { Error = false, Data = ReasonData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public ActionResult ReasonDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [MST].[QualityReasonDetails] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult LoadTimeIssueDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select ID.Id as Value,ID.IssueName as Text from [MST].[QualityIssueDetails] ID";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadIssueItemIssueDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select ID.Id as Value,ID.IssueName as Text from [MST].[QualityIssueDetails] ID";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadTimeDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select *,(select ID.IssueName from [MST].[QualityIssueDetails] ID where ID.Id=TD.IssueId) as IssueName,format(TD.FromTime,'hh:mm tt') as FTime,format(TD.ToTime,'hh:mm tt') as TTime from [MST].[QualityTimeDetails] TD";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadTimeDetailsEditData(string TimeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select *,(select ID.IssueName from [MST].[QualityIssueDetails] ID where ID.Id=TD.IssueId) as IssueName,format(TD.FromTime,'hh:mm tt') as FTime,format(TD.ToTime,'hh:mm tt') as TTime from [MST].[QualityTimeDetails] TD where TD.Id='" + TimeId + @"'";
            return Json(new { Time = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult createTime(Dictionary<string, object> TimeData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityTimeDetails] where PeriodName='" + TimeData["PeriodName"] + "'", out DataSet dsTimeDetailsPeriodNameValidation, false, "1");

                DataSet dsTimeDetails;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityTimeDetails] where Id='" + TimeData["Id"] + "'", out dsTimeDetails, false, "1");
                string _Id = "";

                #region data update
                if (dsTimeDetails.Tables[0].Rows.Count == 0)
                {
                    if (dsTimeDetailsPeriodNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Period Name Already Exist.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("WCProcessTimeDetails", out _Id);
                        _Id = "QTD" + _Id;
                        TimeData["Id"] = _Id;
                        AddNewRow(dsTimeDetails.Tables[0], TimeData);
                    }
                }
                else
                {
                    _Id = TimeData["Id"].ToString();
                    EditRow(dsTimeDetails.Tables[0].Rows[0], TimeData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsTimeDetails);

                return Json(new { Error = false, Data = TimeData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public ActionResult TimeDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [MST].[QualityTimeDetails] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult LoadIssueItemDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select IID.*,(select P.IssueName from [MST].[QualityIssueDetails]  P where P.Id=IID.IssueId) as IssueName,
(select U.UserName from SCS.UnitOfMeasurement U where U.Id=IID.UOMId) as UOM,
(select P.Code from ORG.Position P where P.Id=IID.PositionCodeId) as PositionCode,
(select PM.UserName from HKP.ParameterMaster PM where PM.Id=IID.ParameterId) as Parameter
from [MST].[QualityIssueItem] IID";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadIssueItemDetailsEditData(string ItemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select IID.*,(select P.IssueName from [MST].[QualityIssueDetails]  P where P.Id=IID.IssueId) as IssueName,
(select U.UserName from SCS.UnitOfMeasurement U where U.Id=IID.UOMId) as UOM,
(select P.Code from ORG.Position P where P.Id=IID.PositionCodeId) as PositionCode,
(select PM.UserName from HKP.ParameterMaster PM where PM.Id=IID.ParameterId) as Parameter
from [MST].[QualityIssueItem] IID where IID.Id='" + ItemId + @"'";
            return Json(new { IssueItem = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult createIssueItem(Dictionary<string, object> IssueItemData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityIssueItem] where ItemName='" + IssueItemData["ItemName"] + "'", out DataSet dsItemDetailsIssueItemNameValidation, false, "1");

                DataSet dsIssueDetails;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityIssueItem] where Id='" + IssueItemData["Id"] + "'", out dsIssueDetails, false, "1");
                string _Id = "";

                #region data update
                if (dsIssueDetails.Tables[0].Rows.Count == 0)
                {
                    if (dsItemDetailsIssueItemNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Issue Item Already Exist.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("QualityIssueItem", out _Id);
                        _Id = "QII" + _Id;
                        IssueItemData["Id"] = _Id;
                        AddNewRow(dsIssueDetails.Tables[0], IssueItemData);
                    }
                }
                else
                {
                    _Id = IssueItemData["Id"].ToString();
                    EditRow(dsIssueDetails.Tables[0].Rows[0], IssueItemData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsIssueDetails);

                return Json(new { Error = false, Data = IssueItemData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public ActionResult IssueItemDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [MST].[QualityIssueItem] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult LoadGradeDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from [MST].[QualityGradeDetails]";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadGradeDetailsEditData(string GradeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select * from [MST].[QualityGradeDetails] where Id='" + GradeId + @"'";
            return Json(new { Grade = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult createGrade(Dictionary<string, object> GradeData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityGradeDetails] where GradeName='" + GradeData["GradeName"] + "'", out DataSet dsGradeDetailsGradeNameValidation, false, "1");

                DataSet dsGradeDetails;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[QualityGradeDetails] where Id='" + GradeData["Id"] + "'", out dsGradeDetails, false, "1");
                string _Id = "";

                #region data update
                if (dsGradeDetails.Tables[0].Rows.Count == 0)
                {
                    if (dsGradeDetailsGradeNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Grade Name Already Exist.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("QualityGradeDetails", out _Id);
                        _Id = "QGD" + _Id;
                        GradeData["Id"] = _Id;
                        AddNewRow(dsGradeDetails.Tables[0], GradeData);
                    }
                }
                else
                {
                    _Id = GradeData["Id"].ToString();
                    EditRow(dsGradeDetails.Tables[0].Rows[0], GradeData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsGradeDetails);

                return Json(new { Error = false, Data = GradeData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public ActionResult GradeDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [MST].[QualityGradeDetails] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult GetGradeGridList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select QGD.Id as Value,QGD.GradeName as Text from [MST].[QualityGradeDetails] QGD";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetActionToBeTakenGridList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select Id as Value,ActionToBeTakenName as Text from [MST].[QualityActionToBeTakenDetails]";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetWorkCenterList(string IssueId, string EntityId, string ProcessId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select QMW.WorkCenterMasterId as Value, WCM.UserName as Text from MST.QualityManagementWorkCenter QMW
left join scs.WorkCenterMaster WCM on WCM.Id=QMW.WorkCenterMasterId
where QMW.QMID ='" + IssueId + "' and WCM.EntityId='" + EntityId + "' and WCM.ProcessId='" + ProcessId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetQPEmployeeList(string IssueId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select EI.EmployeeName  from EmployeeInformation EI where EmployeeStatus='Active' 
and  PositionID in (select PositionCodeId from MST.QualityIssueDetails where Id='" + IssueId + "')";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult createReasonValue(List<Dictionary<string, object>> ProductionReasonData)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[TRN].[ProductionReasonValue]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                if (ProductionReasonData != null)
                {
                    foreach (var item in ProductionReasonData)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "PRV" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
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

        [Authorize, HttpPost]
        public ActionResult GetEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT EI.SystemId as SystemId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=mb.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=p.DepartmentId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=p.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=p.SubSectionId
                            WHERE EI.EmployeeStatus='Active' and EI.EmployeeCode is not null";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetQPEmployee(string IssueId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT EI.SystemId as SystemId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=mb.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=p.DepartmentId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=p.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=p.SubSectionId
                            WHERE EI.EmployeeStatus='Active' and  EI.PositionID in (select PositionCodeId from MST.QualityManagementPositionCode where QMID='" + IssueId + "')";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult GetSalesOrder(string entityid, string processId, string ProductionOrderId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT DISTINCT mo.MasterOrderNo
	                                ,ISNULL(so.Id,'') SOId
                                    ,ISNULL(moi.Id,'') MOIId
									,Format(so.DeliveryDate,'dd-MMM-yyyy') as DeliveryDate
									,PM.Code as ProductCode
	                                ,SO.CustomerPOId
	                                ,CPO.PONumber
	                                ,mm.Id MaterialMasterId
	                                ,mm.UserName MaterialMaster
	                                ,ISNULL(mma.StandardName, '') Article
	                                ,b.UserName Customer
	                                ,mo.TotalQty MOQty
	                                ,ISNULL(u.UserName, '') UOM
	                                ,moi.ExtraOrderPercentage [ExtraP]
	                                ,moi.OrderWastagePercentage [WastageP]
	                                ,ISNULL(mma.Id, '') ArticleId
	                                ,mmc.CharCount
	                                ,ISNULL(POD.ProductionOrderId, '') POId
	                                ,B.UserName Buyer
	                                ,PM.UserName AS ProductMasterName
	                                ,CEILING(SO.PlannedQty) PlannedQty
	                               	,CEILING(ISNULL(PRS.TotalProductionQty,0)) TotalProductionQty
	                                ,CEILING(ISNULL((SO.PlannedQty - ISNULL(PRS.TotalProductionQty,0)),0)) RemainingQty
                                    ,SO.Description,MO.BuyerReferenceNo BuyerOrder,MO.OwnReferenceNo OwnOrder,moi.BuyerReferenceNo BuyerItem,moi.OwnReferenceNo OwnItem
                                FROM TRN.ProductionOrderDetail POD
                               LEFT JOIN (
	                                SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
		                                ,s.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description,s.DeliveryDate
	                                FROM trn.SalesOrder AS s
	                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
	                                GROUP BY S.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description,s.DeliveryDate
	                                ) so ON POD.SalesOrderId = SO.Id
                                LEFT JOIN TRN.[MasterOrderItem] moi ON moi.id = so.MasterOrderItemId
                                LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
                                LEFT JOIN (SELECT SUM(PS.Quantity) TotalProductionQty,PS.SalesOrderId,PS.ProcessId
	                                FROM [TRN].[ProductionSummary] PS WHERE PS.ProcessId = '" + processId + @"' GROUP BY PS.SalesOrderId,PS.ProcessId
	                                ) AS PRS ON PRS.SalesOrderId = SO.Id AND PRS.ProcessId = '" + processId + @"'
                                LEFT JOIN HKP.Party b ON b.id = mo.PartyId
                                LEFT JOIN SCS.UnitOfMeasurement u ON u.id = mo.TotalQtyUOMId
                                LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
                                LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
                                LEFT JOIN (SELECT COUNT(Id) CharCount, MaterialMasterId	FROM [MST].[MaterialMasterCharacteristics] GROUP BY MaterialMasterId
	                                ) mmc ON mmc.MaterialMasterId = mm.id
                                LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
                                LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
                                LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
                                LEFT JOIN (SELECT PS.UserName, PO.Id ProductionOrderId FROM [HKP].[ProductionStatus] PS
	                                INNER JOIN TRN.ProductionOrder PO ON PO.ProductionStatusId = PS.Id
	                                ) OS ON OS.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN TRN.ProductionOrder PO ON PO.Id = POD.ProductionOrderId
                                LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id = PO.ProductionStatusId
                                LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
                                LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = SO.CustomerPOId
                                WHERE PO.EntityId = '" + entityid + @"'	AND PS.UserName in ('Running','To Close')	AND POSP.ProcessId = '" + processId + "' AND PO.Id='" + ProductionOrderId + "'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetMasterOrderItem(string entityid, string processId, string ProductionOrderId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT DISTINCT mo.MasterOrderNo,so.MasterOrderItemId
	                                ,ISNULL(so.Id,'') SOId
                                    ,PM.Code as ProductCode
	                                ,SO.CustomerPOId
	                                ,CPO.PONumber
	                                ,mm.Id MaterialMasterId
	                                ,mm.UserName MaterialMaster
	                                ,ISNULL(mma.StandardName, '') Article
	                                ,b.UserName Customer
	                                ,mo.TotalQty MOQty
	                                ,ISNULL(u.UserName, '') UOM
	                                ,moi.ExtraOrderPercentage [ExtraP]
	                                ,moi.OrderWastagePercentage [WastageP]
	                                ,ISNULL(mma.Id, '') ArticleId
	                                ,mmc.CharCount
	                                ,ISNULL(POD.ProductionOrderId, '') POId
	                                ,B.UserName Buyer
	                                ,PM.UserName AS ProductMasterName
	                                ,CEILING(SO.PlannedQty) PlannedQty
	                               	,CEILING(ISNULL(PRS.TotalProductionQty,0)) TotalProductionQty
	                                ,CEILING(ISNULL((SO.PlannedQty - ISNULL(PRS.TotalProductionQty,0)),0)) RemainingQty
                                    ,SO.Description,MO.BuyerReferenceNo BuyerOrder,MO.OwnReferenceNo OwnOrder,moi.BuyerReferenceNo BuyerItem,moi.OwnReferenceNo OwnItem
                                FROM TRN.ProductionOrderDetail POD
                               LEFT JOIN (
	                                SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
		                                ,s.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                FROM trn.SalesOrder AS s
	                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
	                                GROUP BY S.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                ) so ON POD.SalesOrderId = SO.Id
                                LEFT JOIN TRN.[MasterOrderItem] moi ON moi.id = so.MasterOrderItemId
                                LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
                                LEFT JOIN (SELECT SUM(PS.Quantity) TotalProductionQty,PS.SalesOrderId,PS.ProcessId
	                                FROM [TRN].[ProductionSummary] PS WHERE PS.ProcessId = '" + processId + @"' GROUP BY PS.SalesOrderId,PS.ProcessId
	                                ) AS PRS ON PRS.SalesOrderId = SO.Id AND PRS.ProcessId = '" + processId + @"'
                                LEFT JOIN HKP.Party b ON b.id = mo.PartyId
                                LEFT JOIN SCS.UnitOfMeasurement u ON u.id = mo.TotalQtyUOMId
                                LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
                                LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
                                LEFT JOIN (SELECT COUNT(Id) CharCount, MaterialMasterId	FROM [MST].[MaterialMasterCharacteristics] GROUP BY MaterialMasterId
	                                ) mmc ON mmc.MaterialMasterId = mm.id
                                LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
                                LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
                                LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
                                LEFT JOIN (SELECT PS.UserName, PO.Id ProductionOrderId FROM [HKP].[ProductionStatus] PS
	                                INNER JOIN TRN.ProductionOrder PO ON PO.ProductionStatusId = PS.Id
	                                ) OS ON OS.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN TRN.ProductionOrder PO ON PO.Id = POD.ProductionOrderId
                                LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id = PO.ProductionStatusId
                                LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
                                LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = SO.CustomerPOId
                                WHERE PO.EntityId = '" + entityid + @"'	AND PS.UserName in ('Running','To Close') AND POSP.ProcessId = '" + processId + "' AND PO.Id='" + ProductionOrderId + "'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetProductCode(string entityid, string processId, string ProductionOrderId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT PO.Id POId,PS.UserName ProductionStatus, PO.RequiredTimeUnit, Qty,FORMAT(LSD,'dd-MMM-yyyy') LSD,PD.MOIId
								   ,FORMAT(CommitmentDate,'dd-MMM-yyyy') CommitmentDate, PD.Product, PD.ProductCategory,PD.Buyer,PD.Customer,PD.ProductCode 
                                   ,ISNULL(PD.BuyerOrder,'') BuyerOrder,ISNULL(PD.OwnOrder,'') OwnOrder,ISNULL(PD.BuyerItem,'') BuyerItem,ISNULL(PD.OwnItem,'') OwnItem,PD.Description,PD.PONumber,PD.MaterialMasterId,PD.MaterialMaster,PD.ArticleId,PD.Article
								   FROM TRN.ProductionOrder PO 
								   LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
								   LEFT JOIN 
								   (select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory,PM.Code as ProductCode
								   ,MM.Id MaterialMasterId,mm.UserName MaterialMaster,MMA.Id ArticleId,ISNULL(mma.StandardName, '') Article,ISNULL(moi.Id,'') MOIId
								   ,Buyer=  REPLACE(REPLACE(
										            STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                            ,'&amp;','&'), 'amp;', '')	
								,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                        ,'&amp;','&'), 'amp;', '')	
                                ,BuyerOrder = REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								                		,'&amp;','&'), 'amp;', '')
                                ,OwnOrder =REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									                	,'&amp;','&'), 'amp;', '')
							 ,BuyerItem=REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                ,'&amp;','&'), 'amp;', '')	                                                
                              ,OwnItem=REPLACE(REPLACE(
										STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	 
                               ,PONumber=REPLACE(REPLACE(
										 STUFF((select distinct ','+CPO.PONumber from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
                            , Description=REPLACE(REPLACE(
										 STUFF((select distinct ','+XSO.Description from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                        
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
								   FROM TRN.SalesOrder SO
							       LEFT JOIN  TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
								   LEFT JOIN TRN.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                   LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                                   LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
								   LEFT JOIN TRN.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
								   LEFT JOIN [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                   LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
								   ) PD ON PD.ProductionOrderId=PO.Id
								   WHERE PO.EntityId='" + entityid + "' AND PS.UserName in ('Running','To Close')  AND PO.Id='" + ProductionOrderId + "'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetProdQtyValidate(string Processid, string POId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select ISNULL((CASE WHEN ISNULL(PPS.Qty,0)=0 THEN ISNULL(PQ.Qty,PO.PlannedQty) ELSE PO.PlannedQty*PPS.Qty/100 END)-ISNULL(CEILING(PRS.TotalProductionQty), 0),0) RemainingQty from trn.ProductionOrder PO
LEFT JOIN TRN.ProductionOrderProcessSet PPS ON PPS.ProductionOrderID = PO.Id AND PPS.ProcessId = '" + Processid + @"'
                        LEFT JOIN ProductionOrderSchedulingParametersType1 PQ ON PQ.ProductionOrderID = PO.Id
						 LEFT JOIN
                            (SELECT SUM(PS.Quantity) TotalProductionQty, PS.ProductionOrderId
                            FROM [TRN].[ProductionSummary] PS WHERE PS.ProcessId = '" + Processid + @"'  GROUP BY PS.ProductionOrderId
                            ) AS PRS ON PRS.ProductionOrderId = PO.Id 
where PO.ID= '" + POId + "'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetIsProductionHourOpen()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_productionSummaryData.GetIsProductionHourOpen(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProductionBookingPeriodCbo()
        {
            return Json(_productionSummaryData.GetProductionBookingPeriodCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetShiftList(string processId)
        {
            return Json(_productionSummaryData.GetShiftList(processId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetIssueList(string processId)
        {
            return Json(_productionSummaryData.GetIssueList(processId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetQualityIssueList(string processId)
        {
            return Json(_productionSummaryData.GetQualityIssueList(processId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetQualityWorkCenterList(string IssueId, string EntityId, string ProcessId)
        {
            return Json(_productionSummaryData.GetQualityWorkCenterList(IssueId, EntityId, ProcessId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetPOCompleteIssueList()
        {
            return Json(_productionSummaryData.GetPOCompleteIssueList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetPOList(string IssueId)
        {
            return Json(_productionSummaryData.GetPOList(IssueId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetPOCompleteList(string IssueId)
        {
            return Json(_productionSummaryData.GetPOList(IssueId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetPeriodList(string IssueId)
        {
            return Json(_productionSummaryData.GetPeriodList(IssueId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetQualityPeriodList(string IssueId)
        {
            return Json(_productionSummaryData.GetQualityPeriodList(IssueId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetIssueType(string IssueId)
        {
            return Json(_productionSummaryData.GetIssueType(IssueId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetQBookingLevel(string ProcessId, string EntityId, string POId)
        {
            string sql = @"SELECT UserLotNo AS Text, UserLotNo As Value FROM [dbo].[ProductionOrderLotControl]  PPS
where PPS.ProductionOrderID = '" + POId + "' AND PPS.ProcessId = '" + ProcessId + @"'";
            var lot= _sqlRepository.GetDataCollection(sql);
            var pbl = _productionSummaryData.GetQBookingLevel(ProcessId, EntityId, POId);
            //return Json(pbl, lot,JsonRequestBehavior.AllowGet);
            return Json(new { pbl, lot }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetChkInterval(string IssueId)
        {
            return Json(_productionSummaryData.GetChkInterval(IssueId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAllShiftList()
        {
            return Json(_productionSummaryData.GetAllShiftList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCharacteristicsValueCbo(string soid)
        {
            return Json(_ProductionSummaryService.GetCharacteristicsValueCbo(soid), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCharacteristicsValueByPrOCbo(string soid)
        {
            return Json(_ProductionSummaryService.GetCharacteristicsValueByPrOCbo(soid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetLotNumberCbo(string SalesOrderId, string ProductionOrderId, string ProcessId, string productionLevel)
        {
            return Json(_productionSummaryData.GetLotNumberCbo(SalesOrderId, ProductionOrderId, ProcessId, productionLevel), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetWCProcessCbo(string processid, string entityId, string shiftId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_ProductionSummaryService.GetCbo(identity.PlantId, processid, entityId, identity.CompanyId, shiftId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetToWCProcessCbo(string processid, string entityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_ProductionSummaryService.GetToWCCbo(identity.PlantId, processid, entityId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetWCProcessCboNew(string processid, string entityId, string productionDate, string shiftId, string ProductionInChargeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_ProductionSummaryService.GetCboWC(identity.PlantId, processid, entityId, productionDate, shiftId, ProductionInChargeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetWCProcessCboPIC(string processid, string entityId, string productionDate, string shiftId, string ProductionInChargeId, string IssueId, string PeriodId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_ProductionSummaryService.GetCboWCPIC(identity.PlantId, processid, entityId, productionDate, shiftId, ProductionInChargeId, IssueId, PeriodId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetIssueCboQIC(string processid, string entityId, string productionDate, string shiftId, string ProductionInChargeId, string IssueId, string PeriodId, string PId, string POItemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_ProductionSummaryService.GetCboIssueQIC(identity.PlantId, processid, entityId, productionDate, shiftId, ProductionInChargeId, IssueId, PeriodId, PId, POItemId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetPOWiseData(string processid, string entityId, string POId, string Date, string POStatus, string CustomerId, string IssueId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_ProductionSummaryService.GetPOWiseData(processid, entityId, POId, Date, POStatus, CustomerId, IssueId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult LoadQCComplete(string IssueId, string todate, string fromDate, string POId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_ProductionSummaryService.GetQCComplete(IssueId, todate, fromDate, POId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

        [HttpGet, Authorize]
        public JsonResult LoadQCSummary(string IssueId, string todate, string fromDate, string POId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_ProductionSummaryService.GetQCSummary(IssueId, todate, fromDate, POId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult LoadQualityPlan(string POIssueDate, string ResponsiblePersonId)
        {
            return Json(_productionSummaryData.GetQualityPlan(POIssueDate, ResponsiblePersonId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult LoadGeneralIssue(string ResponsiblePersonId)
        {
            return Json(_productionSummaryData.GetGeneralIssue(ResponsiblePersonId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult createQP(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[TRN].[QualityPlanControl]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [TRN].[QualityPlanControl] where QCId is null");
                conC.CommitTransaction();

                objCon = new ConnectionManager.DAL.ConManager("1");

                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and QCId is null", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[TRN].[QualityPlanControl]", out _Id);
                            item["Id"] = "P" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
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

        [Authorize, HttpPost]
        public ActionResult UpdateQP(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[TRN].[QualityPlanControl]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and QCId is null", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[TRN].[QualityPlanControl]", out _Id);
                            item["Id"] = "P" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
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

        [Authorize, HttpPost]
        public ActionResult createGI(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[TRN].[QualityIssueControl]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and QCId is null", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[TRN].[QualityIssueControl]", out _Id);
                            item["Id"] = "G" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
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

        [Authorize, HttpPost]
        public ActionResult UpdateGI(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[TRN].[QualityIssueControl]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and QCId is null", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            //genid.GenID(TableName, out _Id);
                            genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[TRN].[QualityIssueControl]", out _Id);
                            item["Id"] = "G" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        Id = dsProdBooked.Tables[0].Rows[0]["Id"].ToString();
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Id = Id, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetBookingLevel(string FromId, string ToId)
        {
            return Json(_productionSummaryData.GetBookingLevel(FromId, ToId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetSFGMovementFromCbo(string entity)
        {
            return Json(_productionSummaryData.GetSFGMovementFromCbo(entity), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetSFGMovementToCbo(string FromId, string flag, string EntityId)
        {
            return Json(_productionSummaryData.GetSFGMovementToCbo(FromId, flag, EntityId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetShiftGroupCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_ProductionSummaryService.GetShiftGroupCbo(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCharInfoByPrO(string masterid, string workdate, string mmid, string soid, string artid, string CharCount, string CharacteristicsValueId)
        {
            return Json(_ProductionSummaryService.GetCharInfoByPrO(masterid, workdate, mmid, soid, artid, CharCount, CharacteristicsValueId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCharInfo(string masterid, string workdate, string mmid, string soid, string artid, string CharCount, string CharacteristicsValueId)
        {
            return Json(_ProductionSummaryService.GetCharInfo(masterid, workdate, mmid, soid, artid, CharCount, CharacteristicsValueId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetMentorAndRespPersonByWCM(string wcmId)
        {
            return Json(_ProductionSummaryService.GetMentorAndRespPersonByWCM(wcmId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetChar1Info(string masterid, string soid)
        {
            return Json(_ProductionSummaryService.GetChar1Info(masterid, soid), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetChar1InfobyPrO(string masterid, string soid)
        {
            return Json(_ProductionSummaryService.GetChar1InfobyPrO(masterid, soid), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetLineItemGrid(string entityid, string processid, string workdate, string shiftid, string wcid, string ProductionLevel)
        {
            return Json(_productionSummaryData.GetLineItemGrid(entityid, processid, workdate, shiftid, wcid, ProductionLevel), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetLineItemGridInOut(string entityid, string processid, string workdate, string shiftid, string wcid, string ProductionLevel)
        {
            return Json(_productionSummaryData.GetLineItemGrid(entityid, processid, workdate, shiftid, wcid, ProductionLevel), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetLineItemGridSFG(string entityid, string processid, string workdate, string shiftid, string wcid, string ProductionLevel, string status)
        {

            return Json(_productionSummaryData.GetLineItemGridSFG(entityid, processid, workdate, shiftid, wcid, ProductionLevel, status), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSFGWIPQty(string EntityId, string processId, string workCenterMasterId, string salesOrderId, string productionOrderId, string status, bool IsCrossAllowed)
        {
            return Json(_productionSummaryData.GetSFGWIPQty(EntityId, processId, workCenterMasterId, salesOrderId, productionOrderId, status, IsCrossAllowed), JsonRequestBehavior.AllowGet);
        }



        [HttpGet, Authorize]
        public ActionResult GetEntityProcessOrderTotalQty(string EntityId, string processId, string salesOrderId, string productionOrderId, string status)
        {
            return Json(_productionSummaryData.GetEntityProcessOrderTotalQty(EntityId, processId, salesOrderId, productionOrderId, status), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetProcessParaData(string processId, string masterId, string ProductionOrderId)
        {
            return Json(_productionSummaryData.GetProcessParaData(processId, masterId, ProductionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetDetentionParaData(string DetentionId, string processId, string masterId)
        {
            return Json(_productionSummaryData.GetDetentionParaData(DetentionId, processId, masterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetProcessData(string entityId)
        {
            return Json(_productionSummaryData.GetProcessData(entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetProcessDetentionData(string processId, string entityId, string productionDate, string shiftId, string workcenter, string ProductionSummaryId)
        {
            try
            {
                string sql = "";
                string DetentionTypeListsql = "";
                string DetentionListsql = "";
                sql = @"
SELECT CAST (CASE WHEN MMT.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,MMT.Sequence,MMT.Id, MMT.EntityId, MMT.DetentionId, MMT.DetentionTypeId, MMT.ProcessId, MMT.DepartmentId, MMT.ShiftId, MMT.ResponsiblePersonId as ResponsiblePersonId, 
MMT.Remark, MMT.AddedBy, MMT.AddedDate, MMT.AddedFromIP, MMT.UpdatedBy, MMT.UpdatedDate, MMT.UpdatedFromIP
,E.UserName Entity,D.UserName DepartmentName,DM.DetentionUserName Detention,FORMAT(MMT.Date,'dd-MMM-yyyy')[Date],P.UserName Process
										,format(MMT.FromTime,'hh:mm tt') as FromTime,format(MMT.ToTime,'hh:mm tt') as ToTime,MMT.Minute as [Minute],SD.UserName Shift,
										EI.EmployeeName ResponsiblePerson,EI.EmployeeCode ResponsiblePersonCode,MMT.Remark,MMT.WorkCenterId,WC.UserName as WorkCenter
			                            from MachineMasterTransaction MMT
			                            left join ORG.Entity E on E.Id=MMT.EntityId
										left join ORG.Department D on D.Id=MMT.DepartmentId
										left join DetentionMaster DM on DM.Id=MMT.DetentionId
										left join HKP.Process P on P.Id=MMT.ProcessId
										left join ShiftDefination SD on SD.SystemID=MMT.ShiftId
										left Join SCS.WorkCenterMaster WC on WC.id=MMT.WorkCenterId
										left join EmployeeInformation EI on EI.SystemId=MMT.ResponsiblePersonId
                where MMT.EntityId = '" + entityId + "' and MMT.ProcessId = '" + processId + "'  and MMT.Date = '" + productionDate + "' and MMT.ShiftId = '" + shiftId + "' and MMT.WorkCenterId = '" + workcenter + "' and MMT.ProductionSummaryId = '" + ProductionSummaryId + "'";


                //return _sqlRepository.GetDataCollection(sql, null);

                DetentionTypeListsql = @"select DT.UserName As Text, DT.Id As Value from MachineMasterTransaction MMT 
                                         left outer join hkp.DetentionType DT ON DT.id=MMT.DetentionTypeId";

                DetentionListsql = @"Select DM.DetentionUserName As Text, DM.Id As Value,DM.IsAssetApplicable,DM.IsWorkCenterApplicable from MachineMasterTransaction MMT 
                                     left outer join  DetentionMaster DM ON DM.id=MMT.DetentionId";

                List<Dictionary<string, object>> MainList = _sqlRepository.GetDataCollection(sql);
                List<Dictionary<string, object>> detentiontypelist = _sqlRepository.GetDataCollection(DetentionTypeListsql);
                List<Dictionary<string, object>> detentionList = _sqlRepository.GetDataCollection(DetentionListsql);
                for (int i = 0; i < MainList.Count; i++)
                {
                    try
                    {
                        //List<Dictionary<string, object>> k = detentiontypelist.ToList();
                        List<Dictionary<string, object>> k = detentiontypelist.Where(ee => clsStaticInfo.nullrecorder(ee["Value"]) == clsStaticInfo.nullrecorder(MainList[i]["DetentionTypeId"])).ToList();
                        MainList[i]["DetentionTypeList"] = k;

                    }
                    catch (Exception)
                    {

                    }

                    try
                    {
                        List<Dictionary<string, object>> m = detentionList.Where(ee => clsStaticInfo.nullrecorder(ee["Value"]) == clsStaticInfo.nullrecorder(MainList[i]["DetentionId"])).ToList();


                        MainList[i]["DetentionList"] = m;
                    }
                    catch (Exception)
                    {

                    }


                }
                return Json(MainList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            //return Json(_productionSummaryData.GetProcessDetentionData(processId, entityId, productionDate, shiftId, workcenter), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTotalProductionQty(string wcid, string workdate)
        {
            return Json(_ProductionSummaryService.GetTotalProductionQty(wcid, workdate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTotalSOQty(string salesOrderId, string processId)
        {
            return Json(_ProductionSummaryService.GetTotalQty(salesOrderId, processId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTotalSO(string POId, string salesOrderId, string processId)
        {
            return Json(_ProductionSummaryService.GetTotalSOQty(POId, salesOrderId, processId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTotalMOIQty(string POId, string MasterOrderItemId, string processId)
        {
            return Json(_ProductionSummaryService.GetTotalMOIQty(POId, MasterOrderItemId, processId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTotalPCQty(string POId, string MasterOrderItemId, string processId)
        {
            return Json(_ProductionSummaryService.GetTotalPCQty(POId, MasterOrderItemId, processId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTotalPOQty(string productionOrderId, string processId)
        {
            return Json(_productionSummaryData.GetTotalPOQty(productionOrderId, processId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPOQty(string productionOrderId, string processId)
        {
            return Json(_productionSummaryData.GetPOQty(productionOrderId, processId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetItemsData(string entityid, string workCenterMasterId, string productionLevel, string processId, string ProductionOrderId)
        {
            return Json(_productionSummaryData.GetItemsData(entityid, workCenterMasterId, productionLevel, processId, ProductionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetProductionOrderData(string entityid, string workCenterMasterId, string productionLevel, string processId, string status)
        {
            return Json(_productionSummaryData.GetProductionOrderData(entityid, workCenterMasterId, productionLevel, processId, status), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetProductionOrderDataList(string entityid, string workCenterMasterId, string productionLevel, string processId, bool ToCloseAllowed)
        {
            return Json(_productionSummaryData.GetProductionOrderDataList(entityid, workCenterMasterId, productionLevel, processId, ToCloseAllowed), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetProductionOrderDataListWC(string entityid, string workCenterMasterId, string productionLevel, string processId, bool ToCloseAllowed)
        {
            return Json(_productionSummaryData.GetProductionOrderDataListWC(entityid, workCenterMasterId, productionLevel, processId, ToCloseAllowed), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetQualityProductionOrderList(string entityid, string productionLevel, string processId, bool ToCloseAllowed)
        {
            return Json(_productionSummaryData.GetQualityProductionOrderList(entityid, productionLevel, processId, ToCloseAllowed), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetQualityCompletePOList(string IssueId)
        {
            return Json(_productionSummaryData.GetQualityCompletePOList(IssueId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetSFGSOItem(string entityid, string workCenterMasterId, string productionLevel, string processId, string status, bool IsFirst, string ProductionOrderId)
        {
            return Json(_productionSummaryData.GetSFGSOItem(entityid, workCenterMasterId, productionLevel, processId, status, IsFirst, ProductionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult createQC(Dictionary<string, object> QualityControlData, string QualityPlanId, string PlanType, string EntryLevel)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from MST.QualityManagementParameterItem where QMID='" + QualityControlData["IssueId"] + "'", out DataSet dsItemIssueValidation, false, "1");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsQualityControlData;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[QualityControl] where Id='" + QualityControlData["Id"] + "'", out dsQualityControlData, false, "1");
                string _Id = "", Id = string.Empty;

                #region data update
                if (dsQualityControlData.Tables[0].Rows.Count == 0)
                {
                    if (dsItemIssueValidation.Tables[0].Rows.Count == 0)
                    {
                        throw new Exception("Items are not exists for selected Issue.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[TRN].[QualityControl]", out _Id);
                        QualityControlData["Id"] = _Id;
                        QualityControlData["QualityPlanId"] = QualityPlanId;
                        QualityControlData["PlanType"] = PlanType;
                        QualityControlData["EntryLevel"] = EntryLevel;
                        QualityControlData["PlantId"] = identity.PlantId;
                        AddNewRow(dsQualityControlData.Tables[0], QualityControlData);
                        ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                        conC.BeginTransaction();
                        if (PlanType == "GeneralIssue")
                        {
                            conC.executeQuery("Update TRN.QualityIssueControl set QCId='" + QualityControlData["Id"] + "' where Id='" + QualityPlanId + @"'");
                        }
                        if (PlanType == "POIssue")
                        {
                            conC.executeQuery("Update TRN.QualityPlanControl set QCId='" + QualityControlData["Id"] + "' where Id='" + QualityPlanId + @"'");
                        }
                        conC.CommitTransaction();
                    }
                }
                else
                {
                    _Id = QualityControlData["Id"].ToString();
                    QualityControlData["PlantId"] = identity.PlantId;
                    EditRow(dsQualityControlData.Tables[0].Rows[0], QualityControlData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsQualityControlData);

                return Json(new { Error = true, Data = QualityControlData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public ActionResult UpdateQIC(List<Dictionary<string, object>> DataList, string PId)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked, dsChildId, dsGradeApplicable;
            string TableName = "[TRN].[QualityControlDetails]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    objCon.OpenDataSetThroughAdapter("select count(Id) + 1 as QCDId from TRN.QualityControlDetails where QCId='" + PId + "'", out dsChildId, false, "1");
                    int QCDId = Convert.ToInt32(dsChildId.Tables[0].Rows[0]["QCDId"].ToString());
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and QCId='" + PId + "'", out dsProdBooked, false, "1");
                        objCon.OpenDataSetThroughAdapter("select ActionApplicable from [MST].[QualityGradeDetails] where Id='" + item["GradeId"] + "'", out dsGradeApplicable, false, "1");

                        DataView dv = new DataView(dsProdBooked.Tables[0]);
                        if (clsStaticInfo.GetBoolData(dsGradeApplicable.Tables[0].Rows[0]["ActionApplicable"].ToString()) == true && item["ActionToBeTaken"] == null)
                        {
                            throw new Exception("Please enter ActionToBeTaken and proceed.");
                        }
                        else if (clsStaticInfo.GetBoolData(dsGradeApplicable.Tables[0].Rows[0]["ActionApplicable"].ToString()) == true && item["ResponsiblePerson"] == null)
                        {
                            throw new Exception("Please enter Responsible Person and proceed.");
                        }
                        else
                        {
                            if (dv.Count == 0)
                            {
                                //bplib.clsGenID genid = new bplib.clsGenID();
                                item["Id"] = PId + "-" + QCDId++;
                                item["QCID"] = PId;
                                AddNewRow(dsProdBooked.Tables[0], item);
                            }
                            else
                            {
                                DataRow drpb = dv[0].Row;
                                EditRow(drpb, item);
                            }
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

        [HttpPost]
        public JsonResult createRepeatQC(Dictionary<string, object> QualityControlData, List<Dictionary<string, object>> DataList, string QualityPlanId, string PlanType)
        {
            string TableName = "[TRN].[QualityControlDetails]";
            string contId = string.Empty;
            string _QId = "", _QCId = "";
            DataSet dsQualityControlData, dsProdBooked, dsChildId;

            try
            {
                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from MST.QualityIssueItem where IssueId='" + QualityControlData["IssueId"] + "'", out DataSet dsItemIssueValidation, false, "1");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[QualityControl] where Id='" + QualityControlData["Id"] + "'", out dsQualityControlData, false, "1");
                string _Id = "", Id = string.Empty;

                #region data update
                if (dsQualityControlData.Tables[0].Rows.Count == 0)
                {
                    if (dsItemIssueValidation.Tables[0].Rows.Count == 0)
                    {
                        throw new Exception("Items are not exists for selected Issue.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        //genid.GenID("[TRN].[QualityControl]", out _Id);
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[TRN].[QualityControl]", out _Id);
                        QualityControlData["Id"] = _Id;
                        QualityControlData["QualityPlanId"] = QualityPlanId;
                        QualityControlData["PlanType"] = PlanType;
                        QualityControlData["PlantId"] = identity.PlantId;
                        _QCId = QualityControlData["Id"].ToString();
                        QualityControlData["RepeatEntry"] = "Repeat";
                        AddNewRow(dsQualityControlData.Tables[0], QualityControlData);
                        ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                        conC.BeginTransaction();
                        //if (PlanType == "GeneralIssue")
                        //{
                        //    conC.executeQuery("Update TRN.QualityIssueControl set QCId='" + QualityControlData["Id"] + "',RepeatEntry='Repeat' where Id='" + QualityPlanId + @"'");
                        //}
                        //if (PlanType == "POIssue")
                        //{
                        //    conC.executeQuery("Update TRN.QualityPlanControl set QCId='" + QualityControlData["Id"] + "',RepeatEntry='Repeat' where Id='" + QualityPlanId + @"'");
                        //}
                        conC.CommitTransaction();
                    }
                }
                else
                {
                    _Id = QualityControlData["Id"].ToString();
                    _QCId = QualityControlData["Id"].ToString();
                    QualityControlData["PlantId"] = identity.PlantId;
                    EditRow(dsQualityControlData.Tables[0].Rows[0], QualityControlData);
                }
                #endregion data update

                #region  
                conRack.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  QCID='" + _QCId + "'", out dsProdBooked, false, "1");
                conRack.OpenDataSetThroughAdapter("select count(Id) + 1 as QCDId from TRN.QualityControlDetails where QCId='" + _QCId + "'", out dsChildId, false, "1");
                int QCDId = Convert.ToInt32(dsChildId.Tables[0].Rows[0]["QCDId"].ToString());
                if (DataList != null)
                {

                    foreach (var item in DataList)
                    {
                        DataView dv = new DataView(dsProdBooked.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        item["QCID"] = _QCId;
                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            //genid.GenID(TableName, out _QId);
                            item["Id"] = _QCId + "-" + QCDId++;
                            item["QCID"] = _QCId;
                            item["RepeatEntry"] = "Repeat";
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                    }
                }

                #endregion


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsQualityControlData, dsProdBooked);

                return Json(new { Error = true, Data = QualityControlData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public JsonResult create(Dictionary<string, object> QualityControlDetailsData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsQualityControlDetailsData, dsChildId, dsGradeApplicable;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select ActionApplicable from [MST].[QualityGradeDetails] where Id='" + QualityControlDetailsData["GradeId"] + "'", out dsGradeApplicable, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[QualityControlDetails] where Id='" + QualityControlDetailsData["Id"] + "'", out dsQualityControlDetailsData, false, "1");
                conRack.OpenDataSetThroughAdapter("select count(Id) + 1 as QCDId from TRN.QualityControlDetails where QCId='" + QualityControlDetailsData["QCId"] + "'", out dsChildId, false, "1");
                string _Id = "", Id = string.Empty;

                #region data update
                if (dsQualityControlDetailsData.Tables[0].Rows.Count == 0)
                {
                    if (clsStaticInfo.GetBoolData(dsGradeApplicable.Tables[0].Rows[0]["ActionApplicable"].ToString()) == true && QualityControlDetailsData["ActionToBeTaken"] == null)
                    {
                        throw new Exception("Please enter ActionToBeTaken and proceed.");
                    }
                    else if (clsStaticInfo.GetBoolData(dsGradeApplicable.Tables[0].Rows[0]["ActionApplicable"].ToString()) == true && QualityControlDetailsData["ResponsiblePerson"] == null)
                    {
                        throw new Exception("Please enter Responsible Person and proceed.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        //genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[TRN].[QualityControlDetails]", out _Id);
                        QualityControlDetailsData["Id"] = QualityControlDetailsData["QCId"] + "-" + dsChildId.Tables[0].Rows[0]["QCDId"].ToString();
                        QualityControlDetailsData["PlantId"] = identity.PlantId;
                        AddNewRow(dsQualityControlDetailsData.Tables[0], QualityControlDetailsData);
                    }
                }
                else
                {
                    _Id = QualityControlDetailsData["Id"].ToString();
                    QualityControlDetailsData["PlantId"] = identity.PlantId;
                    EditRow(dsQualityControlDetailsData.Tables[0].Rows[0], QualityControlDetailsData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsQualityControlDetailsData);

                return Json(new { Error = false, Data = QualityControlDetailsData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpGet, Authorize]
        public ActionResult GetQualityPOIssueJobCardReportView(string PlannedId, string IssueId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _AttendanceManagementService.GetPOIssueJobCardReports(identity.Name, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, PlannedId, IssueId);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Job Card Report";
                return RenderReportAsPdf(workbook, reportFileName);

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetQualityGeneralIssueJobCardReportView(string PlannedId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _AttendanceManagementService.GetGeneralIssueJobCardReports(identity.Name, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, PlannedId);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Job Card Report";
                return RenderReportAsPdf(workbook, reportFileName);

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetQualityUpdateIssueJobCardReportView(string PlannedId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _AttendanceManagementService.GetUpdateIssueJobCardReports(identity.Name, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, PlannedId);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Job Card Report";
                return RenderReportAsPdf(workbook, reportFileName);

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
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
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
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
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }

        [HttpPost]
        public JsonResult CreateInOut(ProductionSummary ps, IEnumerable<ProductionSummaryDetail> psd)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ps.PlantId = identity.PlantId;
            _ProductionSummaryService.SaveInOutMaster(ps, psd, identity.CompanyGroupId);
            return Json(new { ProductionSummary = ps, Message = AplosMessage.Success });
        }

        [HttpGet, Authorize]
        public ActionResult GetWIPQtyForValidation(string Id, string EntityId, string processId, string workCenterMasterId, string salesOrderId, string productionOrderId, string status, bool IsCrossAllowed)
        {
            return Json(_productionSummaryData.GetWIPQtyForValidation(Id, EntityId, processId, workCenterMasterId, salesOrderId, productionOrderId, status, IsCrossAllowed), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSFGTotalQty(string salesOrderId, string processId, string status)
        {
            return Json(_productionSummaryData.GetSFGTotalQty(salesOrderId, processId, status), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetSFGTotalPOQty(string productionOrderId, string processId, string status)
        {
            return Json(_productionSummaryData.GetSFGTotalPOQty(productionOrderId, processId, status), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult CreateSFG(ProductionSummary ps, IEnumerable<ProductionSummaryDetail> psd, string level, string productionOrderId, string salesOrderId, string processId, string status, bool IsFirst, bool IsCrossAllowed)
        {

            try
            {
                if (IsFirst == true && status == "INVENTORY")
                {
                    if (level == "ProductionOrder")
                    {
                        var allData = _productionSummaryData.GetTotalSFGPOQty(ps.Id, productionOrderId, processId, status);
                        if (allData != null)
                        {

                            int TotalSalesOrderQty = Convert.ToInt32(allData["PlannedQty"].ToString());
                            int RemainingQty = Convert.ToInt32(allData["RemainingQty"].ToString());
                            int TotalProductionQty = Convert.ToInt32(allData["TotalProductionQty"].ToString());

                            if (RemainingQty < 0)
                            {
                                throw new Exception("Order Quantity dosen't available.");
                            }

                            if (TotalSalesOrderQty < (TotalProductionQty + ps.Quantity))
                            {
                                throw new Exception("Produced Quantity should less than Order Quantity.");
                            }

                        }

                    }
                    else
                    {
                        var allData = _productionSummaryData.GetTotalSOSFGQty(ps.Id, salesOrderId, processId, status);
                        int TotalSalesOrderQty = Convert.ToInt32(allData["PlannedQty"].ToString());
                        int RemainingQty = Convert.ToInt32(allData["RemainingQty"].ToString());
                        int TotalProductionQty = Convert.ToInt32(allData["TotalProductionQty"].ToString());

                        if (RemainingQty < 0)
                        {
                            throw new Exception("Order Quantity dosen't available.");
                        }

                        if (TotalSalesOrderQty < (TotalProductionQty + ps.Quantity))
                        {
                            throw new Exception("Produced Quantity should less than Sales Order Quantity.");
                        }
                    }
                }

                if (IsFirst == false)
                {
                    if (status == "INVENTORY")
                    {
                        processId = ps.FromSFGInventoryId;
                    }
                    else
                    {
                        processId = ps.ProcessId;
                    }
                    var wipData = _productionSummaryData.GetWIPQtyValidation(ps.Id, ps.EntityId, processId, ps.WorkCenterMasterId, salesOrderId, productionOrderId, status, IsCrossAllowed);

                    if (wipData != null)
                    {
                        decimal InQ = Convert.ToDecimal(wipData["InQuantity"].ToString());
                        decimal OutQ = Convert.ToDecimal(wipData["OutQuantity"].ToString());

                        if (InQ - (OutQ + ps.Quantity) < 0)
                        {
                            throw new Exception("Total out quantity is greater than total in quantity.");
                        }
                    }

                }

                //if (IsFirst == true && status == "INVENTORY")
                //{
                //    var wipData = _productionSummaryData.GetWIPQtyValidation(ps.Id, ps.EntityId, processId, ps.WorkCenterMasterId, salesOrderId, productionOrderId, status, IsCrossAllowed);

                //    if (wipData != null)
                //    {
                //        decimal InQ = Convert.ToDecimal(wipData["InQuantity"].ToString());
                //        decimal OutQ = Convert.ToDecimal(wipData["OutQuantity"].ToString());

                //        if (InQ - (OutQ + ps.Quantity) < 0)
                //        {
                //            throw new Exception("Total out quantity is greater than total in quantity.");
                //        }
                //    }

                //}




                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ps.PlantId = identity.PlantId;

                _ProductionSummaryService.SaveInOutMaster(ps, psd, identity.CompanyGroupId);
                return Json(new { ProductionSummary = ps, Message = AplosMessage.Success });
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost, Authorize]
        public JsonResult createDetail(string psid, IEnumerable<ProductionSummaryDetail> psd)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _ProductionSummaryService.SaveDetail(psid, psd);
            return Json(new { ProductionSummaryDetail = psd, Message = AplosMessage.Success });
        }
        [HttpPost, Authorize]
        public JsonResult createSecondDetail(IEnumerable<ProductionSummaryDetail> psd, ProductionSummary productionSummary)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _ProductionSummaryService.SaveSecondDetail(psd, productionSummary, identity.CompanyGroupId, identity.PlantId);
            return Json(new { ProductionSummaryDetail = psd, Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _ProductionSummaryService.DeleteDetail(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public ActionResult DeleteMasterWC(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();

                conC.BeginTransaction();
                conC.executeQuery("delete from TRN.QualityControlDetails where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost]
        public ActionResult DeleteInOut(string id)
        {
            _ProductionSummaryService.DeleteDetail(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public ActionResult DeleteSFG(string id)
        {
            _ProductionSummaryService.DeleteDetail(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetInWC(string FDUD, string PlantId, string EntityId, string WorkCenterMasterId, string ProcessId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            return Json(new Library.Planning.PlanningType1.ProductionDashboard().GetWIPInWC(FDUD, EntityId, WorkCenterMasterId, ProcessId, _date), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetOutWC(string FDUD, string PlantId, string EntityId, string WorkCenterMasterId, string ProcessId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            return Json(new Library.Planning.PlanningType1.ProductionDashboard().GetOutWC(FDUD, EntityId, WorkCenterMasterId, ProcessId, _date), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult Calculate(IEnumerable<OpenHeadModelNew> OpenHeadNew)
        {
            DataTable dtValue = new DataTable();
            dtValue.TableName = "TempTable";
            dtValue.Columns.Add("ProductionBookingParameterId");
            dtValue.Columns.Add("Amount");
            string sFormulaResult = null;

            DataSet dsOpenHead = Library.Service.Helpers.DataTableExtensions.ToDataSet<OpenHeadModelNew>(OpenHeadNew);
            for (int i = 0; i < dsOpenHead.Tables[0].Rows.Count; i++)
            {
                if (i == 0)
                {
                    DataRow dtValueRow = dtValue.NewRow();

                    dtValueRow["ProductionBookingParameterId"] = dsOpenHead.Tables[0].Rows[i]["ProductionBookingParameterId"].ToString().Trim();
                    dtValueRow["Amount"] = dsOpenHead.Tables[0].Rows[i]["Value"].ToString().Trim();

                    dtValue.Rows.Add(dtValueRow);
                }
                else if (i > 0 && string.IsNullOrEmpty(dsOpenHead.Tables[0].Rows[i]["FormulaId"].ToString()))
                {
                    DataRow dtValueRow = dtValue.NewRow();

                    dtValueRow["ProductionBookingParameterId"] = dsOpenHead.Tables[0].Rows[i]["ProductionBookingParameterId"].ToString().Trim();
                    dtValueRow["Amount"] = dsOpenHead.Tables[0].Rows[i]["Value"].ToString().Trim();

                    dtValue.Rows.Add(dtValueRow);
                }

                if (!string.IsNullOrEmpty(dsOpenHead.Tables[0].Rows[i]["FormulaId"].ToString()))
                {
                    _productionSummaryData.ReLoadFormulaWithValue(dsOpenHead.Tables[0].Rows[i]["FormulaId"].ToString(), ref dtValue, out string _formulaValue);
                    sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString("#####");

                    DataRow dtValueRow = dtValue.NewRow();

                    dtValueRow["ProductionBookingParameterId"] = dsOpenHead.Tables[0].Rows[i]["ProductionBookingParameterId"].ToString().Trim();

                    if (sFormulaResult == "" || sFormulaResult == "∞")
                    {
                        dtValueRow["Amount"] = 0;
                    }
                    else
                    {
                        dtValueRow["Amount"] = sFormulaResult;
                    }

                    dtValue.Rows.Add(dtValueRow);

                    DataView dv = new DataView(dsOpenHead.Tables[0]);
                    dv.RowFilter = "ProductionBookingParameterId='" + dsOpenHead.Tables[0].Rows[i]["ProductionBookingParameterId"].ToString() + "'";
                    if (dv.Count > 0)
                    {
                        DataRow drmo = dv[0].Row;

                        drmo.BeginEdit();
                        if (sFormulaResult == "" || sFormulaResult == "∞" || sFormulaResult == "NaN")
                        {
                            drmo["Value"] = 0;
                        }
                        else
                        {
                            drmo["Value"] = sFormulaResult;
                        }
                        drmo.EndEdit();

                    }


                }


            }


            List<Dictionary<string, object>> NewData = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(dsOpenHead.Tables[0]);
            return Json(new { NewData, Message = AplosMessage.Success });
        }

        [Authorize]
        public JsonResult CalculateDetention(IEnumerable<OpenHeadModelNew> OpenHeadNew)
        {
            DataTable dtValue = new DataTable();
            dtValue.TableName = "TempTable";
            dtValue.Columns.Add("DetentionMasterMachineParameterId");
            dtValue.Columns.Add("Amount");
            string sFormulaResult = null;

            DataSet dsOpenHead = Library.Service.Helpers.DataTableExtensions.ToDataSet<OpenHeadModelNew>(OpenHeadNew);
            for (int i = 0; i < dsOpenHead.Tables[0].Rows.Count; i++)
            {
                if (i == 0)
                {
                    DataRow dtValueRow = dtValue.NewRow();

                    dtValueRow["DetentionMasterMachineParameterId"] = dsOpenHead.Tables[0].Rows[i]["DetentionMasterMachineParameterId"].ToString().Trim();
                    dtValueRow["Amount"] = dsOpenHead.Tables[0].Rows[i]["Value"].ToString().Trim();

                    dtValue.Rows.Add(dtValueRow);
                }
                else if (i > 0 && string.IsNullOrEmpty(dsOpenHead.Tables[0].Rows[i]["FormulaId"].ToString()))
                {
                    DataRow dtValueRow = dtValue.NewRow();

                    dtValueRow["DetentionMasterMachineParameterId"] = dsOpenHead.Tables[0].Rows[i]["DetentionMasterMachineParameterId"].ToString().Trim();
                    dtValueRow["Amount"] = dsOpenHead.Tables[0].Rows[i]["Value"].ToString().Trim();

                    dtValue.Rows.Add(dtValueRow);
                }

                if (!string.IsNullOrEmpty(dsOpenHead.Tables[0].Rows[i]["FormulaId"].ToString()))
                {
                    _productionSummaryData.ReLoadDetentionFormulaWithValue(dsOpenHead.Tables[0].Rows[i]["FormulaId"].ToString(), ref dtValue, out string _formulaValue);
                    sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString("#####");

                    DataRow dtValueRow = dtValue.NewRow();

                    dtValueRow["DetentionMasterMachineParameterId"] = dsOpenHead.Tables[0].Rows[i]["DetentionMasterMachineParameterId"].ToString().Trim();

                    if (sFormulaResult == "" || sFormulaResult == "∞")
                    {
                        dtValueRow["Amount"] = 0;
                    }
                    else
                    {
                        dtValueRow["Amount"] = sFormulaResult;
                    }

                    dtValue.Rows.Add(dtValueRow);

                    DataView dv = new DataView(dsOpenHead.Tables[0]);
                    dv.RowFilter = "DetentionMasterMachineParameterId='" + dsOpenHead.Tables[0].Rows[i]["DetentionMasterMachineParameterId"].ToString() + "'";
                    if (dv.Count > 0)
                    {
                        DataRow drmo = dv[0].Row;

                        drmo.BeginEdit();
                        if (sFormulaResult == "" || sFormulaResult == "∞" || sFormulaResult == "NaN")
                        {
                            drmo["Value"] = 0;
                        }
                        else
                        {
                            drmo["Value"] = sFormulaResult;
                        }
                        drmo.EndEdit();

                    }


                }


            }


            List<Dictionary<string, object>> NewData = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(dsOpenHead.Tables[0]);
            return Json(new { NewData, Message = AplosMessage.Success });
        }


        #endregion

        #region Production Report with parameter
        [HttpPost, Authorize]
        public ActionResult StockRegisterData(string ToDate, string FromDate, string EntityId, string ShiftId, string ProcessId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                List<Dictionary<string, object>> NewData = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(GetStockRegisterData(FromDate, ToDate, EntityId, ShiftId, ProcessId));
                var jsondata = Json(new { NewData, Message = AplosMessage.Success });
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetStockRegisterData(string FromDate, string ToDate, string EntityId, string ShiftId, string ProcessId)
        {
            try
            {
                var str = @"SELECT e2.UserName Entity,P.UserName Process,PSQ.Sequence ProcessSequence,FORMAT(PS.ProductionDate,'dd-MMM-yyyy')ProductionDate
										,CSG.UserName [Shift],WCM.UserName WorkCenterMaster,PS.ProductionOrderId,PS.LotNumber,E.EmployeeName ResponsiblePerson,E.EmployeeName Mentor
										,PS.Remarks,'' MaterialMaster,''Article,''BuyerRefrence,''Productcode,PS.AddedBy,FORMAT(PS.AddedDate,'dd-MMM-yyyy')AddedDate,PS.UpdatedBy,FORMAT(PS.UpdatedDate,'dd-MMM-yyyy')UpdateDate
										FROM [TRN].[ProductionSummary] PS
										LEFT JOIN ORG.Entity AS e2 ON e2.Id = PS.EntityId
										LEFT JOIN HKP.Process P ON P.Id=PS.ProcessId
										LEFT JOIN [dbo].[ProcessAndInventorySequence] PSQ ON PSQ.ProcessId = P.Id
										LEFT JOIN EmployeeInformation E ON E.SystemId=PS.ResponsiblePersonId
										LEFT JOIN EmployeeInformation M ON M.SystemId=PS.MentorId
										LEFT JOIN SCS.WorkCenterMaster WCM ON WCM.Id=PS.WorkCenterMasterId
                                        LEFT JOIN dbo.ShiftDefination csg ON csg.SystemId=pp.ProductionShiftId
										Where
										PS.EntityId='" + EntityId + @"' and
										
										PS.ProductionDate between '" + FromDate + "' AND '" + ToDate + "'";
                return _sqlRepository.GetDataTable(str);

            }
            catch (Exception e)
            {
                throw e;
            }
        }


        [HttpPost, Authorize]
        public ActionResult StockRegisterReport(string ToDate, string FromDate, string SlNo)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string fileName = "";
                fileName = CreateStockRegisterReportSheet(identity.CompanyId, identity.PlantId, FromDate, ToDate, SlNo, "Stock Register Report " + FromDate + " To " + ToDate + "");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string CreateStockRegisterReportSheet(string CompanyId, string PlantId, string FromDate, string ToDate, string SlNo, string SheetName)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;

            var data = GetStockRegisterReportData(CompanyId, PlantId, FromDate, ToDate, SlNo, true);

            var sheet = workbook.Worksheets[0];

            #region sheet1
            sheet.Name = "PurchaseRegisterGRNWise";

            int ROW = 7;
            int endCol = 1;
            int COL = 1;

            //sheet.Range[ROW, COL].Text = "From - "+FromDate+" , To - "+ToDate;
            //sheet.Range[ROW, COL].ColumnWidth = 13;
            //sheet.Range[ROW, COL].CellStyle.Font.Size = 12;
            //sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
            //sheet.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //ROW += 2;

            #region Grid Headers

            report.SetHeaderText(ref sheet, ROW, COL, "Entity", 25, ExcelHAlign.HAlignLeft);
            int ColEntity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Process", 18, ExcelHAlign.HAlignLeft);
            int ColProcess = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Process sequence", 18, ExcelHAlign.HAlignLeft);
            int ColProcessSequence = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Date", 13, ExcelHAlign.HAlignLeft);
            int ColDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Shift", 15, ExcelHAlign.HAlignLeft);
            int ColShift = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Work Center", 18, ExcelHAlign.HAlignLeft);
            int ColWorkCenter = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Po No.", 10, ExcelHAlign.HAlignLeft);
            int ColPoNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Lot No.", 10, ExcelHAlign.HAlignLeft);
            int ColLotNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Production Work Station", 12, ExcelHAlign.HAlignLeft);
            int ColProductionWorkStation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Responsible Person", 12, ExcelHAlign.HAlignLeft);
            int ColResponsiblePerson = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Mentor", 10, ExcelHAlign.HAlignLeft);
            int ColMentor = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Production", 11, ExcelHAlign.HAlignLeft);
            int ColProduction = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Parameter 1", 20, ExcelHAlign.HAlignLeft);
            int ColPeramiter1 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Parameter 2", 12, ExcelHAlign.HAlignLeft);
            int ColPeramiter2 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Parameter 3", 13, ExcelHAlign.HAlignLeft);
            int ColPeramiter3 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Parameter 4", 12, ExcelHAlign.HAlignLeft);
            int ColPeramiter4 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 13, ExcelHAlign.HAlignRight);
            //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            int ColRemarks = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Material Master", 15, ExcelHAlign.HAlignRight);
            int ColMaterialMaster = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Article", 16, ExcelHAlign.HAlignRight);
            int ColArticle = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Buyer Refrence", 13, ExcelHAlign.HAlignRight);
            int ColBuyerRefrence = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Product Code", 13, ExcelHAlign.HAlignRight);
            int ColProductCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Add By", 10, ExcelHAlign.HAlignRight);
            int ColAddBy = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Add Date", 13, ExcelHAlign.HAlignRight);
            int ColAddDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Updated By", 16, ExcelHAlign.HAlignRight);
            int ColUpdatedBy = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Updated Time", 10, ExcelHAlign.HAlignRight);
            int ColUpdatedTime = COL;

            endCol = COL;
            #endregion Headers


            sheet.Range[ROW, 1, ROW, COL].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            ROW++;
            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {
                sheet[ROW, ColEntity].Text = data.Rows[i]["PartyName"].ToString();
                sheet[ROW, ColProcess].Text = data.Rows[i]["InvoicingPartyPlant"].ToString();
                sheet[ROW, ColProcessSequence].Text = data.Rows[i]["DeliveryPartyPlant"].ToString();
                sheet[ROW, ColDate].Text = data.Rows[i]["PartyCode"].ToString();
                sheet[ROW, ColShift].Text = data.Rows[i]["GSTINNo"].ToString();
                sheet[ROW, ColWorkCenter].Text = data.Rows[i]["Employee"].ToString();
                sheet[ROW, ColPoNo].Text = data.Rows[i]["GRNNo"].ToString();
                sheet[ROW, ColLotNo].Text = data.Rows[i]["GRNEntryDate"].ToString();
                sheet[ROW, ColProductionWorkStation].Text = data.Rows[i]["VoucherNo"].ToString();
                sheet[ROW, ColResponsiblePerson].Text = data.Rows[i]["PostingDate"].ToString();
                sheet[ROW, ColMentor].Text = data.Rows[i]["DocRefNo"].ToString();
                sheet[ROW, ColProduction].Text = data.Rows[i]["DocRefDate"].ToString();
                sheet[ROW, ColPeramiter1].Text = data.Rows[i]["GrnDocDateDifference"].ToString();
                sheet[ROW, ColPeramiter2].Text = data.Rows[i]["GateEntryNo"].ToString();
                sheet[ROW, ColPeramiter3].Text = data.Rows[i]["GateName"].ToString();
                sheet[ROW, ColPeramiter4].Text = data.Rows[i]["CurrencyName"].ToString();
                sheet[ROW, ColRemarks].Text = data.Rows[i]["PartyGroup"].ToString();
                sheet[ROW, ColMaterialMaster].Text = data.Rows[i]["PartyCategory"].ToString();
                sheet[ROW, ColArticle].Text = data.Rows[i]["PartySubCategory"].ToString();
                sheet[ROW, ColBuyerRefrence].Text = data.Rows[i]["PartyType"].ToString();
                sheet[ROW, ColProductCode].Text = data.Rows[i]["PartyAccountGroup"].ToString();
                sheet[ROW, ColAddBy].Text = data.Rows[i]["PartyAccountGroup"].ToString();
                sheet[ROW, ColAddDate].Text = data.Rows[i]["PartyAccountGroup"].ToString();
                sheet[ROW, ColUpdatedBy].Text = data.Rows[i]["PartyAccountGroup"].ToString();
                sheet[ROW, ColUpdatedTime].Text = data.Rows[i]["PartyAccountGroup"].ToString();


                sheet.Range[ROW, ColEntity, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, ColEntity, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
            }

            //ROW++;

            //if (FromDate != "" && ToDate != "")
            //{


            //	report.SetText(ref sheet, ROW, Convert.ToInt32(ColMaterialTranAmount) - 1, "Total");
            //	sheet.Range[ROW, Convert.ToInt32(ColMaterialTranAmount) - 1].CellStyle.Font.Bold = true;
            //	//sheet.Range[1, ROW, Convert.ToInt32(ColMaterialTranAmount) - 1, ROW].Merge();
            //	object sumObject;

            //	//sumObject = data.Compute("Sum(MaterialTranAmount)", "");
            //	//sheet.Range[ROW, Convert.ToInt32(ColMaterialTranAmount)].CellStyle.Font.Bold = true;
            //	//report.SetText(ref sheet, ROW, Convert.ToInt32(ColMaterialTranAmount), Convert.ToDouble(sumObject).ToString("0.##"));
            //	//sheet.Range[ROW, Convert.ToInt32(ColMaterialTranAmount)].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //	//sheet.Range[ROW, Convert.ToInt32(ColMaterialTranAmount)].VerticalAlignment = ExcelVAlign.VAlignTop;

            //	sumObject = data.Compute("Sum(TotalMaterialBaseAmount)", "");
            //	sheet.Range[ROW, Convert.ToInt32(ColTotalMaterialBaseAmount)].CellStyle.Font.Bold = true;
            //	report.SetText(ref sheet, ROW, Convert.ToInt32(ColTotalMaterialBaseAmount), Convert.ToDouble(sumObject).ToString("0.##"));
            //	sheet.Range[ROW, Convert.ToInt32(ColTotalMaterialBaseAmount)].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //	sheet.Range[ROW, Convert.ToInt32(ColTotalMaterialBaseAmount)].VerticalAlignment = ExcelVAlign.VAlignTop;

            //	sumObject = data.Compute("Sum(Payment)", "");
            //	sheet.Range[ROW, Convert.ToInt32(ColPayment)].CellStyle.Font.Bold = true;
            //	report.SetText(ref sheet, ROW, Convert.ToInt32(ColPayment), Convert.ToDouble(sumObject).ToString("0.##"));
            //	sheet.Range[ROW, Convert.ToInt32(ColPayment)].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //	sheet.Range[ROW, Convert.ToInt32(ColPayment)].VerticalAlignment = ExcelVAlign.VAlignTop;

            //	sumObject = data.Compute("Sum(Balance)", "");
            //	sheet.Range[ROW, Convert.ToInt32(ColBalance)].CellStyle.Font.Bold = true;
            //	report.SetText(ref sheet, ROW, Convert.ToInt32(ColBalance), Convert.ToDouble(sumObject).ToString("0.##"));
            //	sheet.Range[ROW, Convert.ToInt32(ColBalance)].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //	sheet.Range[ROW, Convert.ToInt32(ColBalance)].VerticalAlignment = ExcelVAlign.VAlignTop;

            //}

            endRow = ROW - 1;
            endRow = ROW - 1;

            #endregion sheet



            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.UsedRange.CellStyle.Font.Size = 8;



            ReportUtility reportUtility = new ReportUtility();
            //reportUtility.CompanyHeader(ref sheet, endCol, "Purchase Report Register GRN Wise", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);


            sheet.Name = SheetName;
            sheet.UsedRange.WrapText = true;
            sheet.IsGridLinesVisible = false;
            report.PlantHeader(ref sheet, COL, SheetName, PlantId);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);

            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
            workbook.Version = ExcelVersion.Excel2016;

            workbook.SaveAs(filePath);
            workbook.Close();
            excelEngine.Dispose();
            return filePath;

        }

        public DataTable GetStockRegisterReportData(string CompanyId, string PlantId, string FromDate, string ToDate, string GRNNo, bool isreport)
        {
            try
            {
                var str = @"SELECT   IR.Id GRNNo,REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNEntryDate,
							IR.GateEntryNo,p.UserName AS PartyName,P.Code PartyCode,isnull(PP.GSTIN,'') GSTINNo
						   ,ROUND(Isnull(IRD.TotalMaterialTranAmount*IR.ToCurrencyRate,0),2) MaterialTranAmount
						   ,ROUND(Isnull(IRD.TotalTaxAmount,0),2)+ROUND(Isnull(IRD.ChargesTaxTranAmount,0),2) TotalTaxAmount
						   ,ROUND(Isnull(IRD.TotalMaterialTranAmount*IR.ToCurrencyRate,0)+Isnull(IRD.TotalTaxAmount,0),2)+ROUND(Isnull(IRD.ChargesTaxTranAmount,0),2) TotalMaterialBaseAmount
						   ,SUM(ROUND(ISNULL(I.WrittenOffAmount*I.CompanyCurrencyRate,0),4)) as Payment
						   ,( ROUND(Isnull(IRD.TotalMaterialTranAmount*IR.ToCurrencyRate,0)+Isnull(IRD.TotalTaxAmount,0)+Isnull(IRD.ChargesTaxTranAmount,0),2))-(SUM(ROUND(ISNULL(I.WrittenOffAmount*I.CompanyCurrencyRate,0),4))) as Balance
						   ,VoucherNo=CASE WHEN IR.EmployeeId <> '' Then V1.VoucherNo else V.VoucherNo END
						   ,PostingDate= CASE WHEN IR.EmployeeId <> '' Then REPLACE(CONVERT(CHAR(11), ep.PostingDate, 106),' ','-')   else REPLACE(CONVERT(CHAR(11), I.PostingDate, 106),' ','-')  END 
						   ,IR.DocRefNo,CU.Code CurrencyName,IR.PartyType
						   ,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,PAG.UserName PartyAccountGroup

							--new add
							,'' DocRefDate,'' GrnDocDateDifference,'' GateName,'' InvoicingPartyPlant,'' DeliveryPartyPlant,'' Employee
					from [TRN].[InventoryReceive] AS IR
					left jOIN (select InventoryReceiveId,Sum(TransactionQty)TransactionQty,Sum(MaterialTranAmount)MaterialTranAmount
						,Sum(TotalMaterialTranAmount)TotalMaterialTranAmount,Sum(TotalMaterialBooksCurrencyAmount)TotalMaterialBooksCurrencyAmount
						,SUM(TotalTaxAmount) TotalTaxAmount,sum(ChargesTaxTranAmount) ChargesTaxTranAmount
						FROM [TRN].[InventoryReceiveDetail]
					group by InventoryReceiveId ) AS IRD ON IR.Id=IRD.InventoryReceiveId 
					left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
					LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId 
					LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
					LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
					LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
					LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Vendor' AND cp.PlantId=IR.PlantId
					LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Vendor'
					LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
					LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
					LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
                    left JOIN trn.Invoice as I ON I.InventoryReceiveId=IR.Id					
					left join trn.Voucher V on V.Id=I.VoucherId
                    left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
					left join trn.Voucher V1 on V1.Id=ep.VoucherId
						
					where  IR.PlantId='" + PlantId + @"' AND convert(Date,IR.GRNDate) BETWEEN  '" + FromDate + @"' AND '" + ToDate + @"' 
                    AND IR.GRNType IN('GRNBYPO','GRN','EMPGRN')

					group by IR.GRNDate,IR.Id,IR.GateEntryNo,p.UserName,P.Code,PP.GSTIN,IRD.TotalMaterialTranAmount,IRD.TotalMaterialBooksCurrencyAmount,IRD.TotalTaxAmount,IRD.ChargesTaxTranAmount
					,MaterialTranAmount,IR.EmployeeId,IR.EmployeeId,V.VoucherNo,V1.VoucherNo,ep.PostingDate,I.PostingDate,IR.DocRefNo,CU.Code,IR.PartyType,PAG.UserName
					,PC.UserName,PSC.UserName,PG.UserName,IR.ToCurrencyRate";

                if (isreport)
                {

                    var newsql = "select * from(" + str + ") y where y.GRNNo in (" + GRNNo + @")";
                    return _sqlRepository.GetDataTable(newsql);

                }
                else
                {
                    str += "";
                    return _sqlRepository.GetDataTable(str);
                }


            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpGet, Authorize]
        public ActionResult getFilters()
        {
            return Json(filters(), JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<object> filters()
        {
            try
            {
                var sql = @"SELECT * FROM ( SELECT  
                                        isnull(e.Id,'') AS EntityId,isnull(e.UserName,'') Entity,
										pln.Id PlantId,Pln.UserName Plant,
                                        isnull(ps.Id,'') AS ProductionStatusId, isnull(ps.UserName,'') AS ProductionStatus
										,PO.Id ProductionOrderId
                                      , ResponsiblePersonId=STUFF((select distinct ','+XMO.ResponsiblePersonId from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join dbo.EmployeeInformation XEmp on XEmp.SystemId=XMO.ResponsiblePersonId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
	                                         , ResponsiblePerson=STUFF((select distinct ','+XEmp.EmployeeName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join dbo.EmployeeInformation XEmp on XEmp.SystemId=XMO.ResponsiblePersonId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                   , Buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

														 SOStatusId=STUFF((select distinct ','+XB.Id from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].OrderStatus XB on XB.Id=XSO.OrderStatusId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),


													
																 MOStatusId=STUFF((select distinct ','+XB.Id from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].OrderStatus XB on XB.Id=XMO.OrderStatusId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

		
													 BuyerId=STUFF((select distinct ','+XB.Id from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

																
                                                    CustomerId=STUFF((select distinct ','+XP.Id from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),   
                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')                                                 


                                        from trn.ProductionOrder PO
				                                inner join ProductionOrderSchedulingParametersType1 T1 on t1.ProductionOrderID=po.Id
												
				                              
				                                left outer join org.Entity E on e.Id=PO.EntityID
				                             
				                                left outer join org.Plant PLN on pln.Id=E.PlantId
				                                LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId

                              WHERE  PO.ProductionStatusId<>'Closed'
                                ) AS KK	";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpPost, Authorize]
        //public ActionResult GetOrderReport(Dictionary<string, string> parameters, string fromDate, string toDate, string dateType)
        public ActionResult GetProcessWiseOrderReport(string fromDate, string toDate, string EntityId, string ProcessId)
        {
            try
            {
                string fileName = "";
                // fileName = OrderReport(parameters, fromDate, toDate, dateType, "OrderReport");
                fileName = ProcessWiseOrderReport(fromDate, toDate, "OrderReport", EntityId, ProcessId);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string ProcessWiseOrderReport(string fromDate, string toDate, string SheetName, string EntityId, string ProcessId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {

                string Entity = "'" + EntityId.Replace(",", "','") + "'";//replaced with ""
                string processId = "'" + ProcessId.Replace(",", "','") + "'";//replaced with ""

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);
                workbook.Worksheets[1].Name = "Data";
                sheet = workbook.Worksheets[1];
                DataTable dtOrder;
                _productionSummaryData.ProductionOrderReportSQL(fromDate, toDate, Entity, processId, out dtOrder);

                int ROW = 6; int COL = 1;

                #region columns
                sheet[ROW, COL].Text = "Id";
                sheet[ROW, COL].ColumnWidth = 16;
                int colId = COL;
                COL++;
                sheet[ROW, COL].Text = "Plant";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPlant = COL;
                COL++;
                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEntity = COL;
                COL++;
                sheet[ROW, COL].Text = "EntityID";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEntityID = COL;
                COL++;
                sheet[ROW, COL].Text = "WorkCenterMasterId";
                sheet[ROW, COL].ColumnWidth = 16;
                int colWorkCenterMasterId = COL;
                COL++;
                sheet[ROW, COL].Text = "ProductionOrderID";
                sheet[ROW, COL].ColumnWidth = 22;
                int colProductionOrderID = COL;
                COL++;
                sheet[ROW, COL].Text = "WorkCenter";
                sheet[ROW, COL].ColumnWidth = 12;
                int colWorkCenter = COL;
                COL++;
                sheet[ROW, COL].Text = "ActualDate";
                sheet[ROW, COL].ColumnWidth = 12;
                int colActualDate = COL;
                COL++;
                sheet[ROW, COL].Text = "ActualQty";
                sheet[ROW, COL].ColumnWidth = 16;
                int colActualQty = COL;
                COL++;
                sheet[ROW, COL].Text = "ActualCM";
                sheet[ROW, COL].ColumnWidth = 16;
                int colActualCM = COL;
                COL++;
                sheet[ROW, COL].Text = "SAM";
                sheet[ROW, COL].ColumnWidth = 12;
                int colSAM = COL;
                COL++;
                sheet[ROW, COL].Text = "Process";
                sheet[ROW, COL].ColumnWidth = 12;
                int colProcess = COL;
                COL++;
                sheet[ROW, COL].Text = "ToProcess";
                sheet[ROW, COL].ColumnWidth = 12;
                int colToProcess = COL;
                COL++;
                sheet[ROW, COL].Text = "ToWorkCenter";
                sheet[ROW, COL].ColumnWidth = 12;
                int colToWorkCenter = COL;
                COL++;
                sheet[ROW, COL].Text = "Material";
                sheet[ROW, COL].ColumnWidth = 30;
                int colMaterial = COL;
                COL++;
                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 30;
                int colArticle = COL;
                COL++;
                sheet[ROW, COL].Text = "Product";
                sheet[ROW, COL].ColumnWidth = 12;
                int colProduct = COL;
                COL++;
                sheet[ROW, COL].Text = "ProductCategory";
                sheet[ROW, COL].ColumnWidth = 6;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colProductCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "SnapshotDate";
                sheet[ROW, COL].ColumnWidth = 6;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colSnapshotDate = COL;
                COL++;
                sheet[ROW, COL].Text = "PlanQty";
                sheet[ROW, COL].ColumnWidth = 10;
                int colPlanQty = COL;
                COL++;
                sheet[ROW, COL].Text = "PlanCM";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPlanCM = COL;
                COL++;
                sheet[ROW, COL].Text = "CM";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colCM = COL;
                COL++;
                sheet[ROW, COL].Text = "ProductionShift";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colProductionShift = COL;
                COL++;
                sheet[ROW, COL].Text = "SalesOrderIdBooking";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderIdBooking = COL;
                COL++;
                sheet[ROW, COL].Text = "SalesOrderDescBooking";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderDescBooking = COL;
                COL++;
                sheet[ROW, COL].Text = "StandardWorkingHours";
                sheet[ROW, COL].ColumnWidth = 16;
                int colStandardWorkingHours = COL;
                COL++;
                sheet[ROW, COL].Text = "StandardWorkStations";
                sheet[ROW, COL].ColumnWidth = 16;
                int colStandardWorkStations = COL;
                COL++;
                sheet[ROW, COL].Text = "DailyFixedCost";
                sheet[ROW, COL].ColumnWidth = 16;
                int colDailyFixedCost = COL;
                COL++;
                sheet[ROW, COL].Text = "VariableCostPerHour";
                sheet[ROW, COL].ColumnWidth = 16;
                int colVariableCostPerHour = COL;
                COL++;
                sheet[ROW, COL].Text = "WorkingHours";
                sheet[ROW, COL].ColumnWidth = 16;
                int colWorkingHours = COL;
                COL++;
                sheet[ROW, COL].Text = "isBuildUp";
                sheet[ROW, COL].ColumnWidth = 12;
                int colisBuildUp = COL;
                COL++;
                sheet[ROW, COL].Text = "LineTargetPerDay";
                sheet[ROW, COL].ColumnWidth = 12;
                int colLineTargetPerDay = COL;
                COL++;
                sheet[ROW, COL].Text = "PlanTargetPerHour";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPlanTargetPerHour = COL;
                COL++;
                sheet[ROW, COL].Text = "PlanWorkingHoursPerDay";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPlanWorkingHoursPerDay = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer";
                sheet[ROW, COL].ColumnWidth = 12;
                int colbuyer = COL;
                COL++;
                sheet[ROW, COL].Text = "SalesOrderIds";
                sheet[ROW, COL].ColumnWidth = 12;
                int colSalesOrderIds = COL;
                COL++;
                sheet[ROW, COL].Text = "SalesOrderDesc";
                sheet[ROW, COL].ColumnWidth = 50;
                int colSalesOrderDesc = COL;
                COL++;
                sheet[ROW, COL].Text = "MasterOrderNo";
                sheet[ROW, COL].ColumnWidth = 12;
                int colMasterOrderNo = COL;
                COL++;


                sheet[ROW, COL].Text = "BuyerOrderNo";
                sheet[ROW, COL].ColumnWidth = 25;
                int colBuyerOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "OwnOrderNo";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOwnOrderNo = COL;
                COL++;

                sheet[ROW, COL].Text = "StyleNo";
                sheet[ROW, COL].ColumnWidth = 12;
                int colStyleNo = COL;
                COL++;
                sheet[ROW, COL].Text = "OwnStyleNo";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colOwnStyleNo = COL;
                COL++;

                sheet[ROW, COL].Text = "NoOfWorkStation";
                sheet[ROW, COL].ColumnWidth = 12;
                int colNoOfWorkStation = COL;
                COL++;
                sheet[ROW, COL].Text = "PlanHours";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPlanHours = COL;
                COL++;
                sheet[ROW, COL].Text = "ProductionHours";
                sheet[ROW, COL].ColumnWidth = 12;
                int colProductionHours = COL;
                COL++;
                sheet[ROW, COL].Text = "PlanMinutes";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPlanMinutes = COL;
                COL++;
                sheet[ROW, COL].Text = "PlanEfficiency";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPlanEfficiency = COL;
                COL++;
                sheet[ROW, COL].Text = "ActualMinutes";
                sheet[ROW, COL].ColumnWidth = 12;
                int colActualMinutes = COL;
                COL++;
                sheet[ROW, COL].Text = "ActualEfficiency";
                sheet[ROW, COL].ColumnWidth = 12;
                int colActualEfficiency = COL;


                #endregion columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;

                for (int i = 0; i < dtOrder.Rows.Count; i++)
                {
                    sheet[ROW, colId].Text = dtOrder.Rows[i]["Id"].ToString();
                    sheet[ROW, colPlant].Text = dtOrder.Rows[i]["Plant"].ToString();
                    sheet[ROW, colEntity].Text = dtOrder.Rows[i]["Entity"].ToString();
                    sheet[ROW, colEntityID].Text = dtOrder.Rows[i]["EntityID"].ToString();
                    sheet[ROW, colWorkCenterMasterId].Text = dtOrder.Rows[i]["WorkCenterMasterId"].ToString();
                    sheet[ROW, colProductionOrderID].Text = dtOrder.Rows[i]["ProductionOrderID"].ToString();
                    sheet[ROW, colWorkCenter].Text = dtOrder.Rows[i]["WorkCenter"].ToString();
                    sheet[ROW, colActualDate].Text = dtOrder.Rows[i]["ActualDate"].ToString();
                    sheet[ROW, colActualQty].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["ActualQty"].ToString());
                    sheet[ROW, colActualCM].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["ActualCM"].ToString());
                    sheet[ROW, colToProcess].Text = dtOrder.Rows[i]["ToProcess"].ToString();
                    sheet[ROW, colProcess].Text = dtOrder.Rows[i]["Process"].ToString();
                    sheet[ROW, colToWorkCenter].Text = dtOrder.Rows[i]["ToWorkCenter"].ToString();
                    sheet[ROW, colMaterial].Text = dtOrder.Rows[i]["Material"].ToString();
                    sheet[ROW, colArticle].Text = dtOrder.Rows[i]["Article"].ToString();
                    sheet[ROW, colProduct].Text = dtOrder.Rows[i]["Product"].ToString();
                    sheet[ROW, colProductCategory].Text = dtOrder.Rows[i]["ProductCategory"].ToString();
                    sheet[ROW, colSnapshotDate].Text = dtOrder.Rows[i]["SnapshotDate"].ToString();
                    sheet[ROW, colPlanQty].Text = dtOrder.Rows[i]["PlanQty"].ToString();
                    sheet[ROW, colPlanCM].Text = dtOrder.Rows[i]["PlanCM"].ToString();
                    sheet[ROW, colCM].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["CM"].ToString());
                    sheet[ROW, colProductionShift].Text = dtOrder.Rows[i]["ProductionShift"].ToString();
                    sheet[ROW, colSalesOrderIdBooking].Text = dtOrder.Rows[i]["SalesOrderIdBooking"].ToString();
                    sheet[ROW, colSalesOrderDescBooking].Text = dtOrder.Rows[i]["SalesOrderDescBooking"].ToString();
                    sheet[ROW, colStandardWorkingHours].Text = dtOrder.Rows[i]["StandardWorkingHours"].ToString();
                    sheet[ROW, colStandardWorkStations].Text = dtOrder.Rows[i]["StandardWorkStations"].ToString();
                    sheet[ROW, colDailyFixedCost].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["DailyFixedCost"].ToString());
                    sheet[ROW, colVariableCostPerHour].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["VariableCostPerHour"].ToString());
                    sheet[ROW, colWorkingHours].Text = dtOrder.Rows[i]["WorkingHours"].ToString();
                    sheet[ROW, colisBuildUp].Text = dtOrder.Rows[i]["isBuildUp"].ToString();
                    sheet[ROW, colLineTargetPerDay].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["LineTargetPerDay"].ToString());
                    sheet[ROW, colPlanTargetPerHour].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["PlanTargetPerHour"].ToString());
                    sheet[ROW, colPlanWorkingHoursPerDay].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["PlanWorkingHoursPerDay"].ToString());
                    sheet[ROW, colbuyer].Text = dtOrder.Rows[i]["buyer"].ToString();
                    sheet[ROW, colSalesOrderIds].Text = dtOrder.Rows[i]["SalesOrderIds"].ToString();
                    sheet[ROW, colSalesOrderDesc].Text = dtOrder.Rows[i]["SalesOrderDesc"].ToString();
                    sheet[ROW, colMasterOrderNo].Text = dtOrder.Rows[i]["MasterOrderNo"].ToString();
                    sheet[ROW, colBuyerOrderNo].Text = dtOrder.Rows[i]["BuyerOrderNo"].ToString();
                    sheet[ROW, colOwnOrderNo].Text = dtOrder.Rows[i]["OwnOrderNo"].ToString();
                    sheet[ROW, colStyleNo].Text = dtOrder.Rows[i]["StyleNo"].ToString();
                    sheet[ROW, colOwnStyleNo].Text = dtOrder.Rows[i]["OwnStyleNo"].ToString();
                    sheet[ROW, colNoOfWorkStation].Text = dtOrder.Rows[i]["NoOfWorkStation"].ToString();
                    sheet[ROW, colPlanHours].Text = dtOrder.Rows[i]["PlanHours"].ToString();
                    sheet[ROW, colProductionHours].Text = dtOrder.Rows[i]["ProductionHours"].ToString();
                    sheet[ROW, colPlanMinutes].Text = dtOrder.Rows[i]["PlanMinutes"].ToString();
                    sheet[ROW, colPlanEfficiency].Text = dtOrder.Rows[i]["PlanEfficiency"].ToString();
                    sheet[ROW, colActualMinutes].Text = dtOrder.Rows[i]["ActualMinutes"].ToString();
                    sheet[ROW, colActualEfficiency].Text = dtOrder.Rows[i]["ActualEfficiency"].ToString();
                    //sheet[ROW, colParameter].Text = dtOrder.Rows[i]["Parameter"].ToString();
                    //sheet[ROW, colParameterValue].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["ParameterValue"].ToString());



                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }
                IListObject table = sheet.ListObjects.Create("Table1", sheet.Range[6, 1, ROW, endCol]);
                table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Order Report Process Wise", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


                //#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;



                #region Pivot
                //  DataTable dtDistinctParameter = dtOrder.DefaultView.ToTable(true, "Parameter");

                string fPath = fPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + "OrderTempReport" + identity.UserId + ".xlsx";

                workbook.SaveAs(fPath);
                workbook = application.Workbooks.Open(fPath);
                try { System.IO.File.Delete(fPath); } catch (Exception) { }

                workbook.Worksheets[0].Name = "Process Wise";

                IWorksheet pivotSheet = workbook.Worksheets[0];
                IPivotCache cache = workbook.PivotCaches.Add(workbook.Worksheets[1][startRow - 1, 1, ROW - 1, endCol]);
                IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet["A6"], cache);

                pivotTable.Fields[colProcess - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colEntity - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colActualDate - 1].Axis = PivotAxisTypes.Column;


                IPivotField field = pivotTable.Fields[colActualQty - 1];
                field.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);
                pivotTable.DataFields.Add(field, "ActualQty", PivotSubtotalTypes.Sum);

                for (int i = 0; i < pivotTable.Fields.Count; i++)
                {
                    if (i == colProcess - 1 || i == colEntity - 1 || i == colActualDate - 1)
                        pivotTable.Fields[i].Subtotals = PivotSubtotalTypes.None;
                }

                pivotTable.ShowRowGrand = false;
                pivotTable.ShowDrillIndicators = false;
                pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                pivotTable.Options.NullString = "";
                pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;

                sheet = workbook.Worksheets[0];
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Poduction Order Process Wise Report", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;
                workbook.Worksheets[0].UsedRange["A7"].FreezePanes();


                #endregion Buyer Summary
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        [HttpPost, Authorize]
        public ActionResult GetPerametreWiseOrderReport(string fromDate, string toDate, string EntityId, string ProcessId, string ShiftId)
        {
            try
            {
                string fileName = "";
                fileName = PerametreWiseOrderReport(fromDate, toDate, "OrderReport", EntityId, ProcessId, ShiftId);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string PerametreWiseOrderReport(string fromDate, string toDate, string SheetName, string EntityId, string ProcessId, string ShiftId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {

                string Entity = "'" + EntityId.Replace(",", "','") + "'";//replaced with ""

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);
                workbook.Worksheets[1].Name = "Data";
                sheet = workbook.Worksheets[1];
                DataTable dtOrder;
                _productionSummaryData.ProductionOrderParameterReportSQL(fromDate, toDate, Entity, ProcessId, ShiftId, out dtOrder);

                int ROW = 6; int COL = 1;

                #region columns
                sheet[ROW, COL].Text = "Id";
                sheet[ROW, COL].ColumnWidth = 16;
                int colId = COL;
                COL++;
                sheet[ROW, COL].Text = "Plant";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPlant = COL;
                COL++;
                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEntity = COL;
                COL++;
                sheet[ROW, COL].Text = "EntityID";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEntityID = COL;
                COL++;
                sheet[ROW, COL].Text = "WorkCenterMasterId";
                sheet[ROW, COL].ColumnWidth = 16;
                int colWorkCenterMasterId = COL;
                COL++;
                sheet[ROW, COL].Text = "ProductionOrderID";
                sheet[ROW, COL].ColumnWidth = 22;
                int colProductionOrderID = COL;
                COL++;
                sheet[ROW, COL].Text = "WorkCenter";
                sheet[ROW, COL].ColumnWidth = 12;
                int colWorkCenter = COL;
                COL++;
                sheet[ROW, COL].Text = "ActualDate";
                sheet[ROW, COL].ColumnWidth = 12;
                int colActualDate = COL;
                COL++;
                sheet[ROW, COL].Text = "ActualQty";
                sheet[ROW, COL].ColumnWidth = 16;
                int colActualQty = COL;
                COL++;
                sheet[ROW, COL].Text = "ActualCM";
                sheet[ROW, COL].ColumnWidth = 16;
                int colActualCM = COL;
                COL++;
                sheet[ROW, COL].Text = "SAM";
                sheet[ROW, COL].ColumnWidth = 12;
                int colSAM = COL;
                COL++;
                sheet[ROW, COL].Text = "Process";
                sheet[ROW, COL].ColumnWidth = 12;
                int colProcess = COL;
                COL++;
                sheet[ROW, COL].Text = "ToProcess";
                sheet[ROW, COL].ColumnWidth = 12;
                int colToProcess = COL;
                COL++;
                sheet[ROW, COL].Text = "ToWorkCenter";
                sheet[ROW, COL].ColumnWidth = 12;
                int colToWorkCenter = COL;
                COL++;
                sheet[ROW, COL].Text = "Material";
                sheet[ROW, COL].ColumnWidth = 35;
                int colMaterial = COL;
                COL++;
                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 40;
                int colArticle = COL;
                COL++;
                sheet[ROW, COL].Text = "Product Code";
                sheet[ROW, COL].ColumnWidth = 12;
                int colProductCode = COL;
                COL++;
                sheet[ROW, COL].Text = "Entry Id";
                sheet[ROW, COL].ColumnWidth = 12;
                int colEntryId = COL;
                COL++;
                sheet[ROW, COL].Text = "Entry By";
                sheet[ROW, COL].ColumnWidth = 12;
                int colEntryBy = COL;
                COL++;
                sheet[ROW, COL].Text = "UOM";
                sheet[ROW, COL].ColumnWidth = 12;
                int colUOM = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                int colProductionQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Remarks";
                sheet[ROW, COL].ColumnWidth = 12;
                int colRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "Product";
                sheet[ROW, COL].ColumnWidth = 12;
                int colProduct = COL;
                COL++;
                sheet[ROW, COL].Text = "ProductCategory";
                sheet[ROW, COL].ColumnWidth = 6;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colProductCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "SnapshotDate";
                sheet[ROW, COL].ColumnWidth = 6;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colSnapshotDate = COL;
                COL++;
                sheet[ROW, COL].Text = "PlanQty";
                sheet[ROW, COL].ColumnWidth = 10;
                int colPlanQty = COL;
                COL++;
                sheet[ROW, COL].Text = "PlanCM";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPlanCM = COL;
                COL++;
                sheet[ROW, COL].Text = "CM";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colCM = COL;
                COL++;
                sheet[ROW, COL].Text = "ProductionShift";
                sheet[ROW, COL].ColumnWidth = 15;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colProductionShift = COL;
                COL++;
                sheet[ROW, COL].Text = "Shift Working Min";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colShiftWorkingMin = COL;
                COL++;
                sheet[ROW, COL].Text = "Detention In Min";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colDetentionInMin = COL;
                COL++;
                sheet[ROW, COL].Text = "Utilization";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colUtilization = COL;
                COL++;
                sheet[ROW, COL].Text = "SalesOrderIdBooking";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderIdBooking = COL;
                COL++;
                sheet[ROW, COL].Text = "SalesOrderDescBooking";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderDescBooking = COL;
                COL++;
                sheet[ROW, COL].Text = "StandardWorkingHours";
                sheet[ROW, COL].ColumnWidth = 16;
                int colStandardWorkingHours = COL;
                COL++;
                sheet[ROW, COL].Text = "StandardWorkStations";
                sheet[ROW, COL].ColumnWidth = 16;
                int colStandardWorkStations = COL;
                COL++;
                sheet[ROW, COL].Text = "DailyFixedCost";
                sheet[ROW, COL].ColumnWidth = 16;
                int colDailyFixedCost = COL;
                COL++;
                sheet[ROW, COL].Text = "VariableCostPerHour";
                sheet[ROW, COL].ColumnWidth = 16;
                int colVariableCostPerHour = COL;
                COL++;
                sheet[ROW, COL].Text = "WorkingHours";
                sheet[ROW, COL].ColumnWidth = 16;
                int colWorkingHours = COL;
                COL++;
                sheet[ROW, COL].Text = "isBuildUp";
                sheet[ROW, COL].ColumnWidth = 12;
                int colisBuildUp = COL;
                COL++;
                sheet[ROW, COL].Text = "LineTargetPerDay";
                sheet[ROW, COL].ColumnWidth = 12;
                int colLineTargetPerDay = COL;
                COL++;
                sheet[ROW, COL].Text = "PlanTargetPerHour";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPlanTargetPerHour = COL;
                COL++;
                sheet[ROW, COL].Text = "PlanWorkingHoursPerDay";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPlanWorkingHoursPerDay = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer";
                sheet[ROW, COL].ColumnWidth = 12;
                int colbuyer = COL;
                COL++;
                sheet[ROW, COL].Text = "SalesOrderIds";
                sheet[ROW, COL].ColumnWidth = 12;
                int colSalesOrderIds = COL;
                COL++;
                sheet[ROW, COL].Text = "SalesOrderDesc";
                sheet[ROW, COL].ColumnWidth = 40;
                int colSalesOrderDesc = COL;
                COL++;
                sheet[ROW, COL].Text = "MasterOrderNo";
                sheet[ROW, COL].ColumnWidth = 12;
                int colMasterOrderNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Buyer Order No";
                sheet[ROW, COL].ColumnWidth = 12;
                int colBuyerOrderNo = COL;
                COL++;

                sheet[ROW, COL].Text = "PO Ref No";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPORefNo = COL;
                COL++;
                sheet[ROW, COL].Text = "OwnOrderNo";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOwnOrderNo = COL;
                COL++;

                sheet[ROW, COL].Text = "StyleNo";
                sheet[ROW, COL].ColumnWidth = 12;
                int colStyleNo = COL;
                COL++;
                sheet[ROW, COL].Text = "OwnStyleNo";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colOwnStyleNo = COL;
                COL++;

                sheet[ROW, COL].Text = "NoOfWorkStation";
                sheet[ROW, COL].ColumnWidth = 12;
                int colNoOfWorkStation = COL;
                COL++;
                sheet[ROW, COL].Text = "PlanHours";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPlanHours = COL;
                COL++;
                sheet[ROW, COL].Text = "ProductionHours";
                sheet[ROW, COL].ColumnWidth = 12;
                int colProductionHours = COL;
                COL++;
                sheet[ROW, COL].Text = "PlanMinutes";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPlanMinutes = COL;
                COL++;
                sheet[ROW, COL].Text = "PlanEfficiency";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPlanEfficiency = COL;
                COL++;
                sheet[ROW, COL].Text = "ActualMinutes";
                sheet[ROW, COL].ColumnWidth = 12;
                int colActualMinutes = COL;
                COL++;
                sheet[ROW, COL].Text = "ActualEfficiency";
                sheet[ROW, COL].ColumnWidth = 12;
                int colActualEfficiency = COL;
                COL++;
                sheet[ROW, COL].Text = "Sequence";
                sheet[ROW, COL].ColumnWidth = 12;
                int colSequence = COL;
                COL++;
                sheet[ROW, COL].Text = "Parameter";
                sheet[ROW, COL].ColumnWidth = 12;
                int colParameter = COL;
                COL++;
                sheet[ROW, COL].Text = "Parameter Value";
                sheet[ROW, COL].ColumnWidth = 12;
                int colParameterValue = COL;



                #endregion columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;

                for (int i = 0; i < dtOrder.Rows.Count; i++)
                {
                    sheet[ROW, colId].Text = dtOrder.Rows[i]["Id"].ToString();
                    sheet[ROW, colPlant].Text = dtOrder.Rows[i]["Plant"].ToString();
                    sheet[ROW, colEntity].Text = dtOrder.Rows[i]["Entity"].ToString();
                    sheet[ROW, colEntityID].Text = dtOrder.Rows[i]["EntityID"].ToString();
                    sheet[ROW, colWorkCenterMasterId].Text = dtOrder.Rows[i]["WorkCenterMasterId"].ToString();
                    sheet[ROW, colProductionOrderID].Text = dtOrder.Rows[i]["ProductionOrderID"].ToString();
                    sheet[ROW, colWorkCenter].Text = dtOrder.Rows[i]["WorkCenter"].ToString();
                    sheet[ROW, colActualDate].Text = dtOrder.Rows[i]["ActualDate"].ToString();
                    sheet[ROW, colActualQty].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["ActualQty"].ToString());
                    sheet[ROW, colActualCM].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["ActualCM"].ToString());
                    sheet[ROW, colToProcess].Text = dtOrder.Rows[i]["ToProcess"].ToString();
                    sheet[ROW, colProcess].Text = dtOrder.Rows[i]["Process"].ToString();
                    sheet[ROW, colToWorkCenter].Text = dtOrder.Rows[i]["ToWorkCenter"].ToString();
                    sheet[ROW, colMaterial].Text = dtOrder.Rows[i]["Material"].ToString();
                    sheet[ROW, colArticle].Text = dtOrder.Rows[i]["Article"].ToString();
                    sheet[ROW, colProductCode].Text = dtOrder.Rows[i]["ProductCode"].ToString();
                    sheet[ROW, colProduct].Text = dtOrder.Rows[i]["Product"].ToString();
                    sheet[ROW, colEntryId].Text = dtOrder.Rows[i]["Id"].ToString();
                    sheet[ROW, colEntryBy].Text = dtOrder.Rows[i]["EntryBy"].ToString();
                    sheet[ROW, colUOM].Text = dtOrder.Rows[i]["UOM"].ToString();
                    sheet[ROW, colProductionQty].Text = dtOrder.Rows[i]["ProductionQty"].ToString();
                    sheet[ROW, colRemarks].Text = dtOrder.Rows[i]["Remarks"].ToString();
                    sheet[ROW, colProductCategory].Text = dtOrder.Rows[i]["ProductCategory"].ToString();
                    sheet[ROW, colSnapshotDate].Text = dtOrder.Rows[i]["SnapshotDate"].ToString();
                    sheet[ROW, colPlanQty].Text = dtOrder.Rows[i]["PlanQty"].ToString();
                    sheet[ROW, colPlanCM].Text = dtOrder.Rows[i]["PlanCM"].ToString();
                    sheet[ROW, colCM].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["CM"].ToString());
                    sheet[ROW, colProductionShift].Text = dtOrder.Rows[i]["ProductionShift"].ToString();
                    sheet[ROW, colShiftWorkingMin].Text = dtOrder.Rows[i]["ShiftWorkingMin"].ToString();
                    sheet[ROW, colDetentionInMin].Text = dtOrder.Rows[i]["DetentionInMin"].ToString();
                    sheet[ROW, colUtilization].Text = dtOrder.Rows[i]["Utilization"].ToString();
                    sheet[ROW, colSalesOrderIdBooking].Text = dtOrder.Rows[i]["SalesOrderIdBooking"].ToString();
                    sheet[ROW, colSalesOrderDescBooking].Text = dtOrder.Rows[i]["SalesOrderDescBooking"].ToString();
                    sheet[ROW, colStandardWorkingHours].Text = dtOrder.Rows[i]["StandardWorkingHours"].ToString();
                    sheet[ROW, colStandardWorkStations].Text = dtOrder.Rows[i]["StandardWorkStations"].ToString();
                    sheet[ROW, colDailyFixedCost].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["DailyFixedCost"].ToString());
                    sheet[ROW, colVariableCostPerHour].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["VariableCostPerHour"].ToString());
                    sheet[ROW, colWorkingHours].Text = dtOrder.Rows[i]["WorkingHours"].ToString();
                    sheet[ROW, colisBuildUp].Text = dtOrder.Rows[i]["isBuildUp"].ToString();
                    sheet[ROW, colLineTargetPerDay].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["LineTargetPerDay"].ToString());
                    sheet[ROW, colPlanTargetPerHour].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["PlanTargetPerHour"].ToString());
                    sheet[ROW, colPlanWorkingHoursPerDay].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["PlanWorkingHoursPerDay"].ToString());
                    sheet[ROW, colbuyer].Text = dtOrder.Rows[i]["buyer"].ToString();
                    sheet[ROW, colSalesOrderIds].Text = dtOrder.Rows[i]["SalesOrderIds"].ToString();
                    sheet[ROW, colSalesOrderDesc].Text = dtOrder.Rows[i]["SalesOrderDesc"].ToString();
                    sheet[ROW, colMasterOrderNo].Text = dtOrder.Rows[i]["MasterOrderNo"].ToString();
                    sheet[ROW, colBuyerOrderNo].Text = dtOrder.Rows[i]["BuyerOrderNo"].ToString();
                    sheet[ROW, colPORefNo].Text = dtOrder.Rows[i]["PORefNo"].ToString();
                    sheet[ROW, colOwnOrderNo].Text = dtOrder.Rows[i]["OwnOrderNo"].ToString();
                    sheet[ROW, colStyleNo].Text = dtOrder.Rows[i]["StyleNo"].ToString();
                    sheet[ROW, colOwnStyleNo].Text = dtOrder.Rows[i]["OwnStyleNo"].ToString();
                    sheet[ROW, colNoOfWorkStation].Text = dtOrder.Rows[i]["NoOfWorkStation"].ToString();
                    sheet[ROW, colPlanHours].Text = dtOrder.Rows[i]["PlanHours"].ToString();
                    sheet[ROW, colProductionHours].Text = dtOrder.Rows[i]["ProductionHours"].ToString();
                    sheet[ROW, colPlanMinutes].Text = dtOrder.Rows[i]["PlanMinutes"].ToString();
                    sheet[ROW, colPlanEfficiency].Text = dtOrder.Rows[i]["PlanEfficiency"].ToString();
                    sheet[ROW, colActualMinutes].Text = dtOrder.Rows[i]["ActualMinutes"].ToString();
                    sheet[ROW, colActualEfficiency].Text = dtOrder.Rows[i]["ActualEfficiency"].ToString();
                    sheet[ROW, colParameter].Text = dtOrder.Rows[i]["Parameter"].ToString();
                    sheet[ROW, colSequence].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["Sequence"].ToString());
                    sheet[ROW, colParameterValue].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["ParameterValue"].ToString());



                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }
                IListObject table = sheet.ListObjects.Create("Table1", sheet.Range[6, 1, ROW, endCol]);
                table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Order Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


                //#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;



                #region Pivot

                string fPath = fPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + "OrderTempReport" + identity.UserId + ".xlsx";

                workbook.SaveAs(fPath);
                workbook = application.Workbooks.Open(fPath);
                try { System.IO.File.Delete(fPath); } catch (Exception) { }

                workbook.Worksheets[0].Name = "Perameter Wise";

                IWorksheet pivotSheet = workbook.Worksheets[0];
                IPivotCache cache = workbook.PivotCaches.Add(workbook.Worksheets[1][startRow - 1, 1, ROW - 1, endCol]);
                IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet["A6"], cache);

                pivotTable.Fields[colActualDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProcess - 1].Axis = PivotAxisTypes.Data;
                pivotTable.Fields[colEntity - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colWorkCenter - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProductionShift - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colDetentionInMin - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colUtilization - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colbuyer - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colBuyerOrderNo - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colPORefNo - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProductCode - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colMaterial - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colArticle - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colEntryId - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colEntryBy - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colUOM - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProductionQty - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colRemarks - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSequence - 1].Axis = PivotAxisTypes.Column;
                //pivotTable.Fields[colSequence - 1].
                pivotTable.Fields[colParameter - 1].Axis = PivotAxisTypes.Column;



                IPivotField field = pivotTable.Fields[colParameterValue - 1];
                field.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);
                pivotTable.DataFields.Add(field, "ParameterValue", PivotSubtotalTypes.Sum);

                for (int i = 0; i < pivotTable.Fields.Count; i++)
                {
                    if (i == colActualDate - 1 || i == colProcess - 1 || i == colEntity - 1 || i == colWorkCenter - 1 || i == colProductionShift - 1 || i == colDetentionInMin - 1
                        || i == colUtilization - 1 || i == colbuyer - 1 || i == colBuyerOrderNo - 1 || i == colPORefNo - 1 || i == colProductCode - 1 || i == colMaterial - 1
                        || i == colArticle - 1 || i == colEntryId - 1 || i == colEntryBy - 1 || i == colUOM - 1 || i == colProductionQty - 1 || i == colRemarks - 1 || i == colSequence - 1 || i == colParameter - 1)
                        pivotTable.Fields[i].Subtotals = PivotSubtotalTypes.None;
                }


                pivotTable.ShowRowGrand = false;
                pivotTable.ShowDrillIndicators = false;
                pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                pivotTable.Options.NullString = "";
                pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;

                sheet = workbook.Worksheets[0];
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Poduction Order Parameter Wise Report", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;
                workbook.Worksheets[0].UsedRange["A7"].FreezePanes();


                #endregion Buyer Summary
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
  
}