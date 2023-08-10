#region Using

using Aplos.Controllers;
using Aplos.MaterialManagement.MaterialQuery;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.HumanResource.Payroll.Allowance;
using Library.Model.Setups;
using Library.OrderManagement.Sales;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Setups;
using Newtonsoft.Json;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Attendances.Controllers
{
    public class GoodWorkController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly AccountVoucherReportService _accountVoucherReportService;
        clsSales clsSales = new clsSales();
        public GoodWorkController(ISqlRepository R, AccountVoucherReportService accountVoucherReportService)
        {
            _sqlRepository = R;
            _accountVoucherReportService = accountVoucherReportService;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }
        public ActionResult PCAAC()
        {
            return View();
        }
        //Load Employee
        [HttpGet]
        public ActionResult LoadEmployeelist(string empCategory, string department, string section, string subSection, string designation, string userGroup)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = string.Empty;
            var ec = ""; var dep = ""; var sec = ""; var subsec = ""; var des = ""; var userGr = "";
            if (empCategory != "null")
            {
                ec = "and EC.Id in ('" + empCategory + @"')";
            }
            if (department != "null")
            {
                dep = "and DP.Id in ('" + department + @"')";
            }
            if (section != "null")
            {
                sec = "and EI.SectionId in ('" + section + @"')";
            }
            if (subSection != "null")
            {
                subsec = "and EI.SubSectionId in ('" + subSection + @"')";
            }
            if (designation != "null")
            {
                des = "and EI.GivenDesignationId in ('" + designation + @"')";
            }
            if (userGroup != "null")
            {
                userGr = "and PR.GoodWorkPositionCodeId in ('" + userGroup + @"')";
            }
            try
            {
                sql = @"SELECT '' Id,0 CheckBoxSelect, EI.SystemId
                         ,EI.EmployeeCode
                         ,EI.EmployeeName,ei.EmployeeCodePreFix,ei.EmployeeCodeNumeric
                         , FORMAT(EI.DOB,'dd-MMM-yyyy') DOB
                         , FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                         , FORMAT(EI.DOS,'dd-MMM-yyyy') DOS
                         , DG.UserName LegalDesignation
                         ,S.UserName Section,SS.UserName SubSection, DP.UserName Department
                         , PMB.Code,PR.UserName PositionName
                         ,EI.EmployeeStatus
						 ,OTTitle = case when EI.ExcludeOT=0 then 'Yes' else 'No' end
                         FROM dbo.Employeeinformation EI
                         LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id							 
                         LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id							 
                         LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                         LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                         LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                         LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id                       
                         LEFT JOIN HKP.LegalDesignation  DG on DG.Id=EI.LegalDesignationId
                         LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId	
                         LEFT join MST.DesignationMaster DM on DM.DesignationId=EI.GivenDesignationId
						 LEFT join HKP.EmployeeCategory EC on EC.Id=DM.EmployeeCategoryId
                         LEFT JOIN ORG.Section S ON S.Id=EI.SectionId
                         LEFT JOIN ORG.SubSection SS ON SS.Id=EI.SubSectionId
                         WHERE  EI.PlantId='" + identity.PlantId + @"'  " + ec + @"  " + dep + @"  " + sec + @"   " + subsec + @"   " + des + @" " + userGr + @"
                         ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            var data = _sqlRepository.GetDataCollection(sql);
            JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeCategoryList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsDiscreteAllowanceReport ep = new clsDiscreteAllowanceReport();
                return Json(ep.GetEmployeeCategoryList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        //Load Employee

        //Good Work

        public ActionResult DeleteChildUrl(string Id)
        {
            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from dbo.GoodWorkDetail where Id='" + Id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult CreateGoodWork(Dictionary<string, object> data, List<Dictionary<string, object>> goodWorkDetail)
        {
            try
            {
                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                DataSet dsMaster;
                DataSet dsDetail;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from GoodWork where Id='" + data["Id"] + "'", out dsMaster, false, "1");


                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].DefaultView.Count == 0)
                {
                    if (_Id == "")
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("GoodWork", out _Id);
                    }
                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    data["Id"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                    EditRow(dsMaster.Tables[0].DefaultView[0].Row, data);
                }

                #endregion data update

                #region Good Work Detail

                string _MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                con.OpenDataSetThroughAdapter("select * from GoodWorkDetail where GoodWorkId='" + _MasterId + "'", out dsDetail, false, "1");
                int ccount = 0;
                if (goodWorkDetail != null)
                {
                    foreach (var item in goodWorkDetail)
                    {
                        DataView dv = new DataView(dsDetail.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";
                        if (dv.Count == 0)
                        {
                            ccount++;
                            string detailid = materialCommonService.MakePK(_MasterId, ccount, 2);
                            item["Id"] = detailid;
                            item["goodWorkId"] = _MasterId;
                            item["EmpSystemId"] = item["SystemId"];
                            item["FromTime"] = item["FromTime"];
                            item["ToTime"] = item["ToTime"];
                            item["Purpose"] = item["Purpose"];
                            item["PurposeCategory"] = item["PurposeCategory"];
                            item["ApprovedById"] = item["ApprovedById"];
                            item["Minute"] = item["CalculatedTime"];
                            item["Remarks"] = item["Remarks"];

                            materialCommonService.AddNewRowD(dsDetail.Tables[0], item);
                        }
                        if (dv.Count > 0)
                        {
                            ccount++;
                            string detailid = materialCommonService.MakePK(_MasterId, ccount, 2);
                            DataRow drmo = dv[0].Row;
                            drmo.BeginEdit();
                            drmo["Id"] = detailid;
                            drmo["goodWorkId"] = _MasterId;
                            drmo["EmpSystemId"] = item["SystemId"];
                            drmo["FromTime"] = item["FromTime"];
                            drmo["ToTime"] = item["ToTime"];
                            drmo["Purpose"] = item["Purpose"];
                            drmo["PurposeCategory"] = item["PurposeCategory"];
                            drmo["ApprovedById"] = item["ApprovedById"];
                            drmo["Minute"] = item["CalculatedTime"];
                            drmo["Remarks"] = item["Remarks"];
                            drmo.EndEdit();

                        }
                    }
                }

                #endregion Good Work Detail
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsDetail);

                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult Delete(string Id)
        {
            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from dbo.GoodWorkDetail where GoodWorkId='" + Id + "'");
                con.executeQuery("delete from dbo.GoodWork where Id='" + Id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetGoodWorkList()
        {
            string sql = @"select GW.Id,format(GW.WorkDate,'dd-MMM-yyyy') WorkDate,S.UserName Shift,GW.Remarks
                                    from GoodWork GW
                                    left join ShiftDefination S on S.SystemId=GW.ShiftId";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetMinute(GoodWorkTransaction data)
        {
            var ts = data.ToTime.Subtract(data.FromTime);
            return Json(ts.TotalMinutes, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAllActiveEmployeeData()
        {
            JsonResult json = Json(clsSales.GetAllGoodWorkEmployeeData(), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }
        public ActionResult GetGoodWorkDetailCenter(string goodWorkId)
        {
            string str = @"select GWD.Id,EI.SystemId,EI.EmployeeCode,EI.EmployeeName
							,format(GWD.FromTime,'hh:m') FromTime,format(GWD.ToTime,'hh:m') ToTime,GWD.Minute CalculatedTime
							,GWD.Purpose,GWD.PurposeCategory,EmI.SystemId ApprovedById,EmI.EmployeeCode ApprovedByCode
                            ,EmI.EmployeeName ApprovedByName,GWD.[Minute],GWD.Remarks
							,S.UserName Section,SS.UserName SubSection,DEPT.UserName Department
                            from GoodworkDetail GWD 
                            left join EmployeeInformation EI on EI.SystemId=GWD.EmpSystemId
                            left join EmployeeInformation EmI on EmI.SystemId=GWD.ApprovedById
							LEFT JOIN ORG.Section S ON S.Id=EI.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=EI.SubSectionId
                            LEFT JOIN ORG.Department DEPT ON EI.DepartmentId=DEPT.Id
                            where GWD.GoodWorkId in ('" + goodWorkId + "')";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getUserGroupData()
        {
            try
            {
                string strSQL = @"select distinct UserReportGroup, Id from org.position";
                return Json(_sqlRepository.GetDataCollection(strSQL), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public class GoodWorkTransaction
        {
            #region Scalar Properties

            public DateTime FromTime { get; set; }
            public DateTime ToTime { get; set; }
            public int Minute { get; set; }

            #endregion Scalar Properties

            #region Audit Properties

            /// <summary>
            ///This is  AddedBy.Who add data keep track by AddedBy.
            /// </summary>
            [NeverUpdate]
            public string AddedBy { get; set; }

            /// <summary>
            ///This is  AddedDate.Added date keep track by AddedDate.
            /// </summary>
            [NeverUpdate]
            public DateTime AddedDate { get; set; }

            /// <summary>
            /// Record insert by user from IP address.
            /// </summary>
            [NeverUpdate]
            public string AddedFromIP { get; set; }

            /// <summary>
            /// Record updated user name.
            /// </summary>
            public string UpdatedBy { get; set; }

            /// <summary>
            /// Record updated by user date and time.
            /// </summary>
            public DateTime? UpdatedDate { get; set; }

            /// <summary>
            /// Record updated by user IP address.
            /// </summary>
            public string UpdatedFromIP { get; set; }

            #endregion Audit Properties
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

        #region Payable Creation and Worker Advance

        [HttpGet, Authorize]
        public ActionResult GetWorkerAdvanceList()
        {
            string sql = @"select *,FORMAT(FromDate,'dd-MMM-yy')FromDate,FORMAT(ToDate,'dd-MMM-yy')ToDate from [dbo].[WorkerAdvance]";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetWorkerAdvanceDetailCenter(string workAdvanceId)
        {
            string str = @"select ei.SystemId EmpSystemId,ei.EmployeeCode,ei.EmployeeName,s.UserName UserName,ss.UserName SubSection,wad.Id
							,wa.Id workAdvanceId,d.UserName Department,wad.PayDays,wad.Amount,x.[Basic]
                            from [dbo].[WorkerAdvanceDetail] wad
                            left join [dbo].[WorkerAdvance] wa on wa.Id=wad.WorkerAdvanceId
                            left join EmployeeInformation ei on ei.SystemId=wad.EmpSystemId
                            left join org.Section AS s ON s.Id=ei.SectionId
                            left join org.SubSection AS ss ON ss.Id=ei.SubSectionId
                            left join org.Department d on d.Id=ei.DepartmentId
                            LEFT JOIN SalaryInfoDefineMaster SIDM ON SIDM.EmpInfoSystemID = ei.SystemId
                            LEFT JOIN (SELECT SID.DefineAmount Basic,SH.SalaryHead,SID.SalaryID
                                      FROM SalaryInfoDefine SID 
                                      LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SID.SalaryHeadID
                                      WHERE SH.HeadCategory='Basic')x ON x.SalaryID = SIDM.SystemID
                            where wad.WorkerAdvanceId in ('" + workAdvanceId + "')";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult CreateWorkerAdvance(Dictionary<string, object> data, List<Dictionary<string, object>> workerAdvanceDetail)
        {
            try
            {
                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                DataSet dsMaster;
                DataSet dsDetail;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [dbo].[WorkerAdvance] where Id='" + data["Id"] + "'", out dsMaster, false, "1");


                string _Id = "";

                #region data update Worker Advance
                if (dsMaster.Tables[0].DefaultView.Count == 0)
                {
                    if (_Id == "")
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("WorkerAdvance", out _Id);
                    }
                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    data["Id"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                    EditRow(dsMaster.Tables[0].DefaultView[0].Row, data);
                }

                #endregion data update  Worker Advance

                #region  Worker Advance Detail

                string _MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                con.OpenDataSetThroughAdapter("select * from [dbo].[WorkerAdvanceDetail] where  WorkerAdvanceId='" + _MasterId + "'", out dsDetail, false, "1");
                int ccount = 0;
                if (workerAdvanceDetail != null)
                {
                    foreach (var item in workerAdvanceDetail)
                    {
                        DataView dv = new DataView(dsDetail.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";
                        if (dv.Count == 0)
                        {
                            ccount++;
                            string detailId = materialCommonService.MakePK(_MasterId, ccount, 2);

                            item["Id"] = detailId;
                            item["WorkerAdvanceId"] = _MasterId;
                            item["EmpSystemId"] = item["SystemId"];
                            item["PayDays"] = item["PayDays"];
                            //item["Amount"] = item["Amount"];

                            materialCommonService.AddNewRowD(dsDetail.Tables[0], item);
                        }
                        if (dv.Count > 0)
                        {
                            ccount++;
                            string detailid = materialCommonService.MakePK(_MasterId, ccount, 2);
                            DataRow drmo = dv[0].Row;
                            drmo.BeginEdit();
                            
                            drmo["WorkerAdvanceId"] = _MasterId;
                            drmo["EmpSystemId"] = item["EmpSystemId"];
                            drmo["PayDays"] = item["PayDays"];
                            drmo["Amount"] = item["Amount"];

                            drmo.EndEdit();
                        }
                    }
                }

                #endregion  Worker Advance Detail
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsDetail);

                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


        [HttpGet]
        public ActionResult LoadPCAACEmployeelist()
        {
            string sql = string.Empty;
            try
            {
                sql = @"SELECT '' Id,0 CheckBoxSelect, EI.SystemId
                         ,EI.EmployeeCode
                         ,EI.EmployeeName,ei.EmployeeCodePreFix,ei.EmployeeCodeNumeric
                         , FORMAT(EI.DOB,'dd-MMM-yyyy') DOB
                         , FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                         , FORMAT(EI.DOS,'dd-MMM-yyyy') DOS
                         , DG.UserName LegalDesignation
                         ,S.UserName Section,SS.UserName SubSection, DP.UserName Department
                         , PMB.Code,PR.UserName PositionName
                         ,EI.EmployeeStatus
						 ,OTTitle = case when EI.ExcludeOT=0 then 'Yes' else 'No' END
						 ,x.DefineAmount Basic,x.SalaryHead
                         FROM dbo.Employeeinformation EI
                         LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id							 
                         LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id							 
                         LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                         LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                         LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                         LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id                       
                         LEFT JOIN HKP.LegalDesignation  DG on DG.Id=EI.LegalDesignationId
                         LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId	
                         LEFT join MST.DesignationMaster DM on DM.DesignationId=EI.GivenDesignationId
						 LEFT join HKP.EmployeeCategory EC on EC.Id=DM.EmployeeCategoryId
                         LEFT JOIN ORG.Section S ON S.Id=EI.SectionId
                         LEFT JOIN ORG.SubSection SS ON SS.Id=EI.SubSectionId
                         LEFT JOIN SalaryInfoDefineMaster SIDM ON SIDM.EmpInfoSystemID = EI.SystemId
                         LEFT JOIN (SELECT SID.DefineAmount,SH.SalaryHead,SID.SalaryID
                                      FROM SalaryInfoDefine SID 
                         LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SID.SalaryHeadID
                                    WHERE SH.HeadCategory='Basic')x ON x.SalaryID = SIDM.SystemID
                         
                         where EI.employeeCode<>''
                         ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            var data = _sqlRepository.GetDataCollection(sql);
            JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        public class WorkerAdvanceTransaction
        {
            #region Scalar Properties
            public int Percentage { get; set; }

            #endregion Scalar Properties

            #region Audit Properties

            /// <summary>
            ///This is  AddedBy.Who add data keep track by AddedBy.
            /// </summary>
            [NeverUpdate]
            public string AddedBy { get; set; }

            /// <summary>
            ///This is  AddedDate.Added date keep track by AddedDate.
            /// </summary>
            [NeverUpdate]
            public DateTime AddedDate { get; set; }

            /// <summary>
            /// Record insert by user from IP address.
            /// </summary>
            [NeverUpdate]
            public string AddedFromIP { get; set; }

            /// <summary>
            /// Record updated user name.
            /// </summary>
            public string UpdatedBy { get; set; }

            /// <summary>
            /// Record updated by user date and time.
            /// </summary>
            public DateTime? UpdatedDate { get; set; }

            /// <summary>
            /// Record updated by user IP address.
            /// </summary>
            public string UpdatedFromIP { get; set; }

            #endregion Audit Properties
        }
        public class WorkerAdvanceDetailTransaction
        {
            #region Scalar Properties
            public double Basic { get; set; }
            public int PayDays { get; set; }

            #endregion Scalar Properties

            #region Audit Properties

            /// <summary>
            ///This is  AddedBy.Who add data keep track by AddedBy.
            /// </summary>
            [NeverUpdate]
            public string AddedBy { get; set; }

            /// <summary>
            ///This is  AddedDate.Added date keep track by AddedDate.
            /// </summary>
            [NeverUpdate]
            public DateTime AddedDate { get; set; }

            /// <summary>
            /// Record insert by user from IP address.
            /// </summary>
            [NeverUpdate]
            public string AddedFromIP { get; set; }

            /// <summary>
            /// Record updated user name.
            /// </summary>
            public string UpdatedBy { get; set; }

            /// <summary>
            /// Record updated by user date and time.
            /// </summary>
            public DateTime? UpdatedDate { get; set; }

            /// <summary>
            /// Record updated by user IP address.
            /// </summary>
            public string UpdatedFromIP { get; set; }

            #endregion Audit Properties
        }

        #endregion Payable Creation and Worker Advance
    }
}