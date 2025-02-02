using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Model.Employees;
using Library.Model.External;
using Library.Model.HumanResources;
using Library.Model.Payrolls;
using Library.Service.Attendances;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using OTSBD;
using SetINOUT;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Employees.Controllers
{
    public class CompanyWiseEmployeePlantTransferController : BaseController
    {



        private readonly ISqlRepository _sqlRepository;



        private readonly IEmployeeInformationService _employeeInformationService;
        private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly EmployeePromotionNewService _employeePromotionService;
        private readonly IRecruitmentSelectionService _preRecruitmentEmployee;
        public CompanyWiseEmployeePlantTransferController(
            IEmployeeInformationService employeeInformationService
            , IPreRecruitmentEmployeeService preRecruitmentEmployeeService
            , IEmployeeProfileService employeeProfileService
            , EmployeePromotionNewService employeePromotionService, ISqlRepository sqlRepository, IRecruitmentSelectionService preRecruitmentEmployee
        )
        {
            _employeeInformationService = employeeInformationService;
            _preRecruitmentEmployeeService = preRecruitmentEmployeeService;
            _employeeProfileService = employeeProfileService;
            _employeePromotionService = employeePromotionService;
            _sqlRepository = sqlRepository;
            _preRecruitmentEmployee = preRecruitmentEmployee;
        }



        //private readonly IEmployeeProfileService _employeeProfileService;

        //public EmployeePromotionController(
        //      IEmployeeProfileService employeeProfileService
        //    )
        //{
        //    _employeeProfileService = employeeProfileService;
        //}



        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }




        #endregion -- Pages








        [HttpGet, Authorize]
        public ActionResult LoadEmployeelist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = string.Empty;
            try
            {
                sql = @"SELECT EI.*,Replace(CONVERT(VARCHAR(11), EI.DOB, 106), ' ', '-') DOBs,Replace(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJs                                 
								  ,DG.UserName GivenDesignation, DP.UserName Department, PMB.Code,PR.UserName PositionName,E.UserName EntityName
								  ,DSG.UserName Designation
								  ,PR.DesignationId
								  ,PG.StandardName PayRollGroupName
								  ,PG.Id PayRollGroupId
								  --, ApprovedStatus=case when SM.IsApproved=1 then 'Approved' when SM.IsApproved=0 then 'Un-approved' when SM.IsApproved is null then 'Not Defined' end  
								  , DeG.UserName DesignationGroupName
								  ,PFPolicy=case when dmc.PFPolicyMasterID IS NOT NULL THEN 'Yes' ELSE 'No' END
								  ,ESICPolicy=case when dmc.ESICPolicyMasterID IS NOT NULL THEN 'Yes' ELSE 'No' END
								  ,BnsPolicy=case when dmc.BnsPlcMthRetainID IS NOT NULL THEN 'Yes' ELSE 'No' END
								  ,SalaryRule=case when dmc.SalaryRuleMasterId IS NOT NULL THEN 'Yes' ELSE 'No' END
								  ,ld.UserName LegalDesignation
								  ,ec.UserName EmployeeCategory
								  ,LSG.UserName SalaryGrade
                                  ,PL.UserName PlantName
                                  ,SR.SalaryRuleName
								  ,LP.PolicyName LeavePolicyName
                              FROM dbo.Employeeinformation EI 
                              LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id	
                              LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                              LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                              LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
							  LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
							  LEFT JOIN HKP.Designation DeG on DeG.Id=EI.GivenDesignationId
							  LEFT JOIN  HKP.LegalDesignation AS ld ON ld.Id=ei.LegalDesignationId----
							  
				              LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
							  Left join MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
							  Left Join hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
							  
							  LEFT JOIN mst.DesignationMasterLegalDesignation AS dmld ON dmld.LegalDesignationId=ei.LegalDesignationId---
                              LEFT JOIN MST.DesignationMaster DM ON DM.id=dmld.DesignationMasterId----
                              LEFT JOIN hkp.EmployeeCategory AS ec ON ec.Id=dm.EmployeeCategoryId-----
                              
                              left join [MST].[LegalSalaryGradeDesignation] LSGD ON LSGD.LegalDesignationId = EI.LegalDesignationId and lsgd.PlantId=ei.PlantId
                              LEFT JOIN [SCS].[LegalSalaryGrade] LSG ON LSGD.LegalSalaryGradeId = LSG.Id                             
                              
                              
                              LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId = DM.Id AND dmc.PlantId=ei.PlantId                              
                              LEFT JOIN HKP.DesignationGroup DG ON DG.Id=DM.DesignationGroupId

							  LEFT JOIN SalaryRuleMaster SR on SR.SystemID=dmc.SalaryRuleMasterId
							  left join LeavePolicyMaster LP on LP.SystemID=dmc.LeavePolicyMasterId


                         WHERE EI.EmployeeStatus='Active' AND
                         EI.PlantId='" + identity.PlantId + @"'   ORDER BY  ei.EmployeeCodePreFix,ei.EmployeeCodeNumeric";

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }

            var data = _sqlRepository.GetDataCollection(sql);
            JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;



            //return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult PlantWiseEmployeeDetails(string EmployeeId, string PlantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT EI.SystemId
                                  ,Replace(CONVERT(VARCHAR(11), EI.DOB, 106), ' ', '-') DOBs,Replace(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJs                                 
								  ,DG.UserName GivenDesignation, DP.UserName Department, PMB.Code,PR.UserName PositionName,E.UserName EntityName
								  ,DSG.UserName Designation
								  ,PR.DesignationId
								  ,PG.StandardName PayRollGroupName
								  ,PG.Id PayRollGroupId
								  --, ApprovedStatus=case when SM.IsApproved=1 then 'Approved' when SM.IsApproved=0 then 'Un-approved' when SM.IsApproved is null then 'Not Defined' end  
								  , DeG.UserName DesignationGroupName
								  ,PFPolicy=case when dmc.PFPolicyMasterID IS NOT NULL THEN 'Yes' ELSE 'No' END
								  ,ESICPolicy=case when dmc.ESICPolicyMasterID IS NOT NULL THEN 'Yes' ELSE 'No' END
								  ,BnsPolicy=case when dmc.BnsPlcMthRetainID IS NOT NULL THEN 'Yes' ELSE 'No' END
								  ,SalaryRule=case when dmc.SalaryRuleMasterId IS NOT NULL THEN 'Yes' ELSE 'No' END,dmc.SalaryRuleMasterId
								  ,ld.UserName LegalDesignation,EI.LegalDesignationId
								  ,ec.UserName EmployeeCategory
								  ,LSG.UserName SalaryGrade
                                  ,PL.UserName PlantName
                                  ,SR.SalaryRuleName
								  ,LP.PolicyName LeavePolicyName
                                  ,DM.DesignationId GivenDesignationId
								  ,DM.EmployeeCategoryId EmployeeCategorySystemID
								  ,LSGD.LegalDesignationId
                              FROM dbo.Employeeinformation EI 
                              LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id	
                              LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                              LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                              LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
							  LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
							  LEFT JOIN HKP.Designation DeG on DeG.Id=EI.GivenDesignationId
							 
							  
				              LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId
							  Left join MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
							  Left Join hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
							  
							  ---LEFT JOIN mst.DesignationMasterLegalDesignation AS dmld ON dmld.LegalDesignationId=ei.LegalDesignationId---
                              ---LEFT JOIN MST.DesignationMaster DM ON DM.id=dmld.DesignationMasterId----
                              ---LEFT JOIN hkp.EmployeeCategory AS ec ON ec.Id=dm.EmployeeCategoryId-----
                              
                              left join [MST].[LegalSalaryGradeDesignation] LSGD ON LSGD.LegalDesignationId = EI.LegalDesignationId and lsgd.PlantId='" + PlantId + @"'     
                              LEFT JOIN [SCS].[LegalSalaryGrade] LSG ON LSGD.LegalSalaryGradeId = LSG.Id                             
                              LEFT JOIN  HKP.LegalDesignation AS ld ON ld.Id=EI.LegalDesignationId----


                              LEFT JOIN mst.DesignationMasterLegalDesignation AS dmld ON dmld.LegalDesignationId=EI.LegalDesignationId  -------
                              LEFT JOIN MST.DesignationMaster DM ON DM.id=dmld.DesignationMasterId----
                              LEFT JOIN hkp.EmployeeCategory AS ec ON ec.Id=dm.EmployeeCategoryId-----


                              
                              LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId = DM.Id AND dmc.PlantId='" + PlantId + @"'                              
                              LEFT JOIN HKP.DesignationGroup DG ON DG.Id=DM.DesignationGroupId

							  LEFT JOIN SalaryRuleMaster SR on SR.SystemID=dmc.SalaryRuleMasterId
							  left join LeavePolicyMaster LP on LP.SystemID=dmc.LeavePolicyMasterId


                         WHERE EI.EmployeeStatus='Active' AND
                         EI.PlantId='" + identity.PlantId + @"' AND EI.SystemId='" + EmployeeId + @"'";

            string sqljoblocation = @"select SystemID,JobLocation from JobLocation where PlantID='" + PlantId + @"'";

            string sqlshift = @"Select SystemID, UserName from ShiftDefination where PlantID='" + PlantId + @"'";






            var data = _sqlRepository.GetDataCollection(sql);
            var Joblocation = _sqlRepository.GetDataCollection(sqljoblocation);
            var shift = _sqlRepository.GetDataCollection(sqlshift);



            return Json(new { data, Joblocation, shift }, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public ActionResult SaveData(EmployeePlantTransferModel data, EmployeePlantTransferModelDetails olddata, EmployeePlantTransferModelDetails newdata, BudgetCodeModel bc)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            clsStaticInfo obj = new clsStaticInfo();
            clsAttendance.AttendanceProcessAplos objAttdn = new clsAttendance.AttendanceProcessAplos();
            //DADID = string.Empty;
            try
            {

                #region validation

                if (string.IsNullOrEmpty(data.PlantId))
                {
                    throw new Exception("Select Plant");
                }
                if (string.IsNullOrEmpty(data.JobLocationId))
                {
                    throw new Exception("Select Job Location");
                }
                if (string.IsNullOrEmpty(data.BudgetCodeId))
                {
                    throw new Exception("Select Budget Code");
                }
                string Date = Convert.ToDateTime(data.EffectiveDate).ToString("dd");
                if (Date == "1" || Date == "01")
                {

                }
                else
                {
                    bplib.clsWebLib.Throw("'Effective Date' [" + data.EffectiveDate + "] must be first day of the month...");
                }

                //DataSet dsV = null;
                //string sqlv = "SELECT * FROM SalaryLock WHERE EmpSystemId='" + data.EmpSystemId + @"' and MonthNo=Month(DATEADD(DAY,-1,'" + data.EffectiveDate + @"'))  and YearNo=Year(DATEADD(DAY,-1,'" + data.EffectiveDate + @"')) and IsLocked=1 ";
                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(sqlv, out dsV, false, "1");
                //if (dsV.Tables[0].Rows.Count == 0)
                //{
                //    bplib.clsWebLib.Throw("Plaese lock previous month salary...");
                //}


                #endregion




                #region EmployeeInformation 
                string sql = @" UPDATE EmployeeInformation SET PlantId = '" + data.PlantId + @"'
                                , JobLocationID='" + data.JobLocationId + @"'
                                , CompanyId='" + data.CompanyId + @"'
                                , BudgetCode='" + data.BudgetCodeId + @"' ";

                if (!string.IsNullOrEmpty(newdata.GivenDesignationId))
                {
                    sql += @" , GivenDesignationId='" + newdata.GivenDesignationId + @"' ";
                }
                if (!string.IsNullOrEmpty(newdata.EmployeeCategorySystemID))
                {
                    sql += @" , EmployeeCategorySystemID ='" + newdata.EmployeeCategorySystemID + @"' ";
                }
                if (!string.IsNullOrEmpty(newdata.LegalDesignationId))
                {
                    sql += @" , LegalDesignationId ='" + newdata.LegalDesignationId + @"' ";
                }

                if (!string.IsNullOrEmpty(newdata.SalaryRuleMasterId))
                {
                    sql += @" , SalaryRuleMasterSystemID ='" + newdata.SalaryRuleMasterId + @"' ";
                }





                if (!string.IsNullOrEmpty(bc.DesignationSystemID))
                {
                    sql += @" , DesignationSystemID ='" + bc.DesignationSystemID + @"' ";
                }
                if (!string.IsNullOrEmpty(bc.UnitId))
                {
                    sql += @" , UnitId = '" + bc.UnitId + @"' ";
                }
                if (!string.IsNullOrEmpty(bc.DivisionId))
                {
                    sql += @" , DivisionId = '" + bc.DivisionId + @"' ";
                }
                if (!string.IsNullOrEmpty(bc.DepartmentId))
                {
                    sql += @" , DepartmentId = '" + bc.DepartmentId + @"' ";
                }
                if (!string.IsNullOrEmpty(bc.SectionId))
                {
                    sql += @" , SectionId = '" + bc.SectionId + @"' ";
                }

                if (!string.IsNullOrEmpty(bc.SubSectionId))
                {
                    sql += @" , SubSectionId = '" + bc.SubSectionId + @"' ";
                }

                if (!string.IsNullOrEmpty(bc.SubdivisionID))
                {
                    sql += @" , SubdivisionID = '" + bc.SubdivisionID + @"' ";
                }
                if (!string.IsNullOrEmpty(bc.LineId))
                {
                    sql += @"  , LineId = '" + bc.LineId + @"' ";
                }
                if (!string.IsNullOrEmpty(bc.EmploymentType))
                {
                    sql += @"  , EmploymentType = '" + bc.EmploymentType + @"' ";
                }
                if (!string.IsNullOrEmpty(bc.PositionID))
                {
                    sql += @" , PositionID = '" + bc.PositionID + @"' ";
                }
                if (!string.IsNullOrEmpty(bc.IsDirect))
                {
                    if (Convert.ToBoolean(bc.IsDirect))
                    {
                        sql += @"  , IsDirect = 1 ";
                    }
                    else
                    {
                        sql += @"  , IsDirect = 0 ";
                    }

                }

                sql += @"  WHERE SystemId='" + data.EmpSystemId + @"'";
                ExecuteRawSQL(sql);
                #endregion




                #region JobLocation

                string sqljl = "select * from EmpDateWiseJobLocation where EmpSystemID ='" + data.EmpSystemId + @"' and EffectiveDate>='" + data.EffectiveDate + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqljl, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "EmpDateWiseJobLocationEPT", out sID);
                    dr["SystemID"] = "JLEPT" + sID;

                    dr["EmpSystemID"] = data.EmpSystemId;
                    dr["JobLcSystemID"] = data.JobLocationId;
                    dr["EffectiveDate"] = Convert.ToDateTime(data.EffectiveDate);
                    dr["AddedBy"] = identity.Name;
                    dr["DateAdded"] = DateTime.Now;
                    dr["UpdatedBy"] = identity.Name;
                    dr["DateUpdated"] = DateTime.Now;
                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    dr["EmpSystemID"] = data.EmpSystemId;
                    dr["JobLcSystemID"] = data.JobLocationId;
                    dr["EffectiveDate"] = Convert.ToDateTime(data.EffectiveDate);
                    dr["UpdatedBy"] = identity.Name;
                    dr["DateUpdated"] = DateTime.Now;
                    dr.EndEdit();
                }
                #endregion

                #region log

                string EmployeePlantTransferHistory = string.Empty;
                DataSet dsEmployeePlantTransferHistory = null;
                string sqllog = "select * from [dbo].[EmployeePlantTransferHistory] where SystemId=''";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqllog, out dsEmployeePlantTransferHistory, false, "1");
                if (dsEmployeePlantTransferHistory.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsEmployeePlantTransferHistory.Tables[0].NewRow();
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "EmployeePlantTransferHistory", out sID);
                    EmployeePlantTransferHistory = "EPTL" + sID;
                    dr["SystemID"] = "EPTL" + sID;

                    dr["EmpSystemID"] = data.EmpSystemId;
                    dr["ToPlantId"] = data.PlantId;
                    dr["EffectiveDate"] = Convert.ToDateTime(data.EffectiveDate);
                    dr["ToBudgetCode"] = data.BudgetCodeId;

                    dr["FromPlantId"] = olddata.PlantId;
                    dr["FromLegalDesignationId"] = olddata.LegalDesignationId;
                    dr["FromBudgetCode"] = olddata.BudgetCode;


                    dr["ToLegalDesignationId"] = newdata.LegalDesignationId;


                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = "::";
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = "::";
                    dsEmployeePlantTransferHistory.Tables[0].Rows.Add(dr);
                }
                #endregion

                #region  weekoff

                string sqlweekoffdelete = @"Delete from [dbo].[EmployeeWeekOffByDay] where EmpSystemID='" + data.EmpSystemId + @"' and EffectiveDate>='" + data.EffectiveDate + @"'";
                ExecuteRawSQL(sqlweekoffdelete);


                DataSet dsWeekOff = null;
                string sqlweekoff = "select * from [dbo].[EmployeeWeekOffByDay] where EmpSystemID='" + data.EmpSystemId + @"' and EffectiveDate='" + data.EffectiveDate + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlweekoff, out dsWeekOff, false, "1");
                if (dsWeekOff.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsWeekOff.Tables[0].NewRow();
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "EmployeePlantTransferWeekoff", out sID);
                    dr["SystemID"] = "EPTW" + sID;

                    dr["EmpSystemID"] = data.EmpSystemId;
                    dr["FixSystemID"] = data.ShiftId;

                    dr["EffectiveDate"] = Convert.ToDateTime(data.EffectiveDate);
                    dr["AlignWithCC"] = true;
                    dr["IndividualWeekOff"] = false;
                    //dr["FromPlantId"] = olddata.PlantId;
                    //dr["FromLegalDesignationId"] = olddata.LegalDesignationId;
                    //dr["FromBudgetCode"] = olddata.BudgetCode;


                    //dr["ToLegalDesignationId"] = newdata.LegalDesignationId;


                    dr["AddedBy"] = identity.Name;
                    dr["DateAdded"] = DateTime.Now;

                    dr["UpdatedBy"] = identity.Name;
                    dr["DateUpdated"] = DateTime.Now;

                    dsWeekOff.Tables[0].Rows.Add(dr);
                }
                #endregion

                #region Leave
                DataSet dsEmployeePlantTransferLeave = null;
                string sqlleave = @"select * from TRN.EmployeeLeaveSummary where EmployeeId='" + data.EmpSystemId + @"'
                                   and CalanderYearId = (select top 1 Id from YearlyCalendar where '" + Convert.ToDateTime(data.EffectiveDate).ToString("dd-MMM-yyyy") + @"' between FromDate and ToDate and PlantId = '" + olddata.PlantId + @"')";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlleave, out dsEmployeePlantTransferLeave, false, "1");


                DataSet dsYearlyCalendar = null;
                string sqlYearlyCalendar = @" select top 1 Id from YearlyCalendar where '" + Convert.ToDateTime(data.EffectiveDate).ToString("dd-MMM-yyyy") + @"' between FromDate and ToDate and PlantId = '" + data.PlantId + @"'";


                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlYearlyCalendar, out dsYearlyCalendar, false, "1");
                string CalanderYearId = string.Empty;

                if (dsYearlyCalendar.Tables[0].Rows.Count > 0)
                {
                    CalanderYearId = dsYearlyCalendar.Tables[0].Rows[0]["Id"].ToString();
                }



                if (dsEmployeePlantTransferLeave.Tables[0].Rows.Count > 0)
                {
                    DataSet dsEmpPlantTransferLeaveSummaryBackup = null;
                    string sqlEmpPlantTransferLeaveSummaryBackup = @" select * from EmpPlantTransferLeaveSummaryBackup where Id=''";


                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sqlEmpPlantTransferLeaveSummaryBackup, out dsEmpPlantTransferLeaveSummaryBackup, false, "1");
                    for (int i = 0; i < dsEmployeePlantTransferLeave.Tables[0].Rows.Count; i++)
                    {
                        DataRow dr = dsEmpPlantTransferLeaveSummaryBackup.Tables[0].NewRow();
                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "EmpPlantTransferLeaveSummaryBackup", out sID);
                        dr["Id"] = "LB" + sID;
                        dr["EmployeePlantTransferHistoryId"] = EmployeePlantTransferHistory;
                        dr["EmployeeId"] = dsEmployeePlantTransferLeave.Tables[0].Rows[i]["EmployeeId"];
                        dr["CalanderYearId"] = dsEmployeePlantTransferLeave.Tables[0].Rows[i]["CalanderYearId"];
                        dr["LeaveTypeId"] = dsEmployeePlantTransferLeave.Tables[0].Rows[i]["LeaveTypeId"];
                        dr["CarryForward"] = dsEmployeePlantTransferLeave.Tables[0].Rows[i]["CarryForward"];
                        dr["CurrentYearAllocation"] = dsEmployeePlantTransferLeave.Tables[0].Rows[i]["CurrentYearAllocation"];
                        dr["DaysCanBeSanctioned"] = dsEmployeePlantTransferLeave.Tables[0].Rows[i]["DaysCanBeSanctioned"];
                        dr["CurrentYearAvailedOpeningBalance"] = dsEmployeePlantTransferLeave.Tables[0].Rows[i]["CurrentYearAvailedOpeningBalance"];
                        dr["AppliedDays"] = dsEmployeePlantTransferLeave.Tables[0].Rows[i]["AppliedDays"];
                        dr["AvailedDays"] = dsEmployeePlantTransferLeave.Tables[0].Rows[i]["AvailedDays"];
                        dr["PreviousYearCarryForward"] = dsEmployeePlantTransferLeave.Tables[0].Rows[i]["PreviousYearCarryForward"];
                        dr["CompanyGroupId"] = dsEmployeePlantTransferLeave.Tables[0].Rows[i]["CompanyGroupId"];
                        dr["PlantId"] = dsEmployeePlantTransferLeave.Tables[0].Rows[i]["PlantId"];
                        dr["CurrentYearEarnedDaysOpeningBalance"] = dsEmployeePlantTransferLeave.Tables[0].Rows[i]["CurrentYearEarnedDaysOpeningBalance"];
                        dr["CarryForwardOpeningBalance"] = dsEmployeePlantTransferLeave.Tables[0].Rows[i]["CarryForwardOpeningBalance"];
                        dr["YearEndEncash"] = dsEmployeePlantTransferLeave.Tables[0].Rows[i]["YearEndEncash"];
                        dr["YearEndLapse"] = dsEmployeePlantTransferLeave.Tables[0].Rows[i]["YearEndLapse"];
                        dr["YearEndEncashCumulative"] = dsEmployeePlantTransferLeave.Tables[0].Rows[i]["YearEndEncashCumulative"];
                        dr["YearEndLapseCumulative"] = dsEmployeePlantTransferLeave.Tables[0].Rows[i]["YearEndLapseCumulative"];
                        dr["BroughtForward"] = dsEmployeePlantTransferLeave.Tables[0].Rows[i]["BroughtForward"];
                        dr["IsYearlyProcessed"] = dsEmployeePlantTransferLeave.Tables[0].Rows[i]["IsYearlyProcessed"];
                        dr["CalculatedEarningDays"] = dsEmployeePlantTransferLeave.Tables[0].Rows[i]["CalculatedEarningDays"];
                        dr["CurrentYearAllocationAsPerPolicy"] = dsEmployeePlantTransferLeave.Tables[0].Rows[i]["CurrentYearAllocationAsPerPolicy"];
                        dr["EncashedInbetween"] = dsEmployeePlantTransferLeave.Tables[0].Rows[i]["EncashedInbetween"];
                        dr["IsEncashed"] = dsEmployeePlantTransferLeave.Tables[0].Rows[i]["IsEncashed"];
                        dr["NotEncashedButYearEnded"] = dsEmployeePlantTransferLeave.Tables[0].Rows[i]["NotEncashedButYearEnded"];
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dsEmpPlantTransferLeaveSummaryBackup.Tables[0].Rows.Add(dr);
                    }
                    obj.SaveDataSets(dsEmpPlantTransferLeaveSummaryBackup);
                }








                string u_Leave = @"Update TRN.EmployeeLeaveSummary set CalanderYearId='" + CalanderYearId + @"',PlantId='" + data.PlantId + @"' where EmployeeId='" + data.EmpSystemId + @"'
                                   and CalanderYearId = (select top 1 Id from YearlyCalendar where '" + Convert.ToDateTime(data.EffectiveDate).ToString("dd-MMM-yyyy") + @"' between FromDate and ToDate and PlantId = '" + olddata.PlantId + @"')";

                ExecuteRawSQL(u_Leave);
                #endregion






                obj.SaveDataSets(dsMaster, dsEmployeePlantTransferHistory, dsWeekOff);

                //update raw table and processed data and datewise shift
                string d_EmpDateWise = @"delete from EmpDateWiseShiftAssign  where WorkDate >= '" + data.EffectiveDate + "' and EmpSystemID ='" + data.EmpSystemId + @"'";
                string d_ProcessData = @"delete FROM AttdnProcessData  where WorkDate >='" + data.EffectiveDate + "'  and EmpSystemID ='" + data.EmpSystemId + @"'";
                string u_AttdnRawData = @"update AttdnRawData set plantid='" + data.PlantId + @"'  where pDate >='" + data.EffectiveDate + "'  and LogDownLoadNum ='" + data.EmpSystemId + @"'";
                ExecuteRawSQL(u_AttdnRawData, d_EmpDateWise, d_ProcessData);



                //update salary information
                string u_SalaryInfoDefineMaster = @"UPDATE SalaryInfoDefineMaster SET PlantID='" + data.PlantId + @"',SalaryRuleMasterSystemID='" + newdata.SalaryRuleMasterId + @"' WHERE EmpInfoSystemID='" + data.EmpSystemId + @"' ---and EffectiveDate>='" + data.EffectiveDate + "'";
                //string u_SalaryInfoBackMaster = @"UPDATE SalaryInfoBackMaster SET PlantID='" + data.PlantId + @"',SalaryRuleMasterSystemID='" + newdata.SalaryRuleMasterId + @"' WHERE EmpInfoSystemID='" + data.EmpSystemId + @"' ---and EffectiveDate>='" + data.EffectiveDate + "'";
                string u_SalaryIncrementNextDueDate = @"UPDATE SalaryIncrementNextDueDate SET PlantId='" + data.PlantId + @"' WHERE EmpSystemId='" + data.EmpSystemId + @"' and  EffectiveDate IN (SELECT EffectiveDate from SalaryInfoDefineMaster WHERE EmpInfoSystemID='" + data.EmpSystemId + @"')";

                string u_EmployeeEligibleForSalaryHeadEnum = @" UPDATE EmployeeEligibleForSalaryHeadEnum SET PlantId='" + data.PlantId + @"' WHERE EmpSystemId='" + data.EmpSystemId + @"' and SalaryStructureId IN (
                                           SELECT SystemID FROM SalaryInfoDefineMaster WHERE EmpInfoSystemID='" + data.EmpSystemId + @"' ---and EffectiveDate>='" + data.EffectiveDate + @"'
                                          
                                            )";

                ExecuteRawSQL(u_SalaryInfoDefineMaster);
                //ExecuteRawSQL(u_SalaryInfoBackMaster);
                ExecuteRawSQL(u_SalaryIncrementNextDueDate);
                ExecuteRawSQL(u_EmployeeEligibleForSalaryHeadEnum);

                ShiftAssignEmp ss = new ShiftAssignEmp();
                ss.IsFix = true;
                ss.EffectiveDate = Convert.ToDateTime(data.EffectiveDate);
                ss.PlantId = data.PlantId;
                ss.EmpSystemID = data.EmpSystemId;
                ss.EmpSystemIDs = data.EmpSystemId;
                ss.FixSystemID = data.ShiftId;
                ss.AddedBy = identity.Name;
                ss.UpdatedBy = identity.Name;










                //ShiftAssignEmp 



                SaveDataBulk(ss, true);


                //////
                ///
                #region SetInOut
                clsSetInOut oSetInOut = new clsSetInOut();
                DataSet dsHRsetting = null;
                GetHRsettinng(data.PlantId, out dsHRsetting);

                if (dsHRsetting.Tables[0].Rows.Count > 0)
                {
                    DateTime FromDateR = Convert.ToDateTime(ss.EffectiveDate);
                    DateTime ToDateR = DateTime.Now;
                    string u_AttdnRawFlag = @"update AttdnRawData set PType=NULL  where pDate >='" + data.EffectiveDate + "'  and LogDownLoadNum ='" + data.EmpSystemId + @"'";
                    ExecuteRawSQL(u_AttdnRawFlag);

                    while (FromDateR <= ToDateR)
                    {

                        oSetInOut.SetRawINOUTonShiftAssignment(data.PlantId, identity.CompanyGroupId, FromDateR.ToString("dd-MMM-yyyy"), ss.EmpSystemIDs);
                        FromDateR = FromDateR.AddDays(1);
                    }
                }//hr 
                #endregion




                DateTime FromDateAttdn = Convert.ToDateTime(ss.EffectiveDate);
                DateTime ToDateAttdn = DateTime.Now;
                while (FromDateAttdn <= ToDateAttdn)
                {

                    AttendanceLog.Log.SaveLog(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "\\" + System.Reflection.MethodBase.GetCurrentMethod().Name);
                    objAttdn.SaveTotal(ss.PlantId, FromDateAttdn.ToString("dd-MMM-yyyy"), ss.EmpSystemIDs, false, true);
                    FromDateAttdn = FromDateAttdn.AddDays(1);
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }


            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }

        public void ExecuteRawSQL(string u_RawData, string d_DateWiseShift, string d_ProcessedData)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                objCon.ExecuteNonQueryWrapper(d_DateWiseShift, true, "1");
                objCon.ExecuteNonQueryWrapper(d_ProcessedData, true, "1");
                objCon.ExecuteNonQueryWrapper(u_RawData, true, "1");
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                try
                {
                    if (IsTransactionStarted)
                    {
                        objCon.RollBack();
                    }
                    objCon.CloseConnection();
                }
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End Function

        public void ExecuteRawSQL(string sql1)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                objCon.ExecuteNonQueryWrapper(sql1, true, "1");
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                try
                {
                    if (IsTransactionStarted)
                    {
                        objCon.RollBack();
                    }
                    objCon.CloseConnection();
                }
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End Function


        /////////////////////////////////////////////////////////////////////
        public void SaveDataBulk(ShiftAssignEmp ss, bool CheckBox)
        {
            #region DataSet Declare


            DataSet dsEmployeeInfo = null;

            DataSet dsShiftAssign = null;
            DataTable dtShiftAssign = null;
            DataRow drShiftAssign = null;
            DataView dvShiftAssign = null;

            clsStaticInfo objApp = null;
            clsEmployeeLoad objEmpLoad = null;

            #endregion

            bool DATA_OK = true;

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //clsSetInOut obj = new clsSetInOut();
                objApp = new clsStaticInfo();
                objEmpLoad = new clsEmployeeLoad();
                clsAttendance.AttendanceProcessAplos objAttdn = new clsAttendance.AttendanceProcessAplos();
                TBS.ShiftProcess spr = new TBS.ShiftProcess();
                FixedShiftProcess spf = new FixedShiftProcess();

                #region DataSet

                EmployeeShiftAssign_for_Save(ss.EmpSystemIDs, ss.EffectiveDate.ToString("dd-MMM-yyyy"), out dsShiftAssign);
                dtShiftAssign = dsShiftAssign.Tables[0];
                dvShiftAssign = new DataView();
                dvShiftAssign.Table = dtShiftAssign;

                GetEmployeeInfo(ss.EmpSystemIDs, out dsEmployeeInfo);//need to work

                #endregion DataSet

                //if (DATA_OK == false)//need to work
                //{

                //    #region Validation

                //    #region Shift

                //    if (ss.IsFix == false && ss.IsRoster == false)
                //    {
                //        Exception ex = new Exception("Please select any one Shift type (Fixed/Roster)...");
                //        throw (ex);
                //    }

                //    if ((bplib.clsWebLib.IsDateOK(ss.EffectiveDate.ToString("dd-MMM-yyyy")) == false))
                //    {
                //        Exception ex = new Exception("Please Define Effective Date .... (allowed format is  dd-MMM-yyyy ex: '01-jan-2014')");
                //        throw (ex);
                //    }

                //    if (ss.IsFix == true)//
                //    {
                //        if (ss.FixSystemID == null)
                //        {
                //            Exception ex = new Exception("Please Select Fix Shift...");
                //            throw (ex);
                //        }
                //    }
                //    if (ss.IsRoster == true)
                //    {
                //        if (ss.RosterSystemID.Length == 0)
                //        {
                //            Exception ex = new Exception("Please Select Roster Shift...");
                //            throw (ex);
                //        }
                //        if (ss.RosterStartShiftID.Length == 0)
                //        {
                //            Exception ex = new Exception("Please Select Roster Start Shift...");
                //            throw (ex);
                //        }
                //    }

                //    #endregion Shift

                //    #region valida

                //    #endregion

                //    //for (int i = 0; i < dsEmployeeInfo.Tables[0].Rows.Count; i++)
                //    //{
                //    //    string _empid = dsEmployeeInfo.Tables[0].Rows[i]["SystemID"].ToString();
                //    //    string _EmployeeCode = dsEmployeeInfo.Tables[0].Rows[i]["EmployeeCode"].ToString();
                //    //    string _DOJ = dsEmployeeInfo.Tables[0].Rows[i]["DOJ"].ToString();
                //    //    string _COD = dsEmployeeInfo.Tables[0].Rows[i]["COD"].ToString();
                //    //    string _DOS = dsEmployeeInfo.Tables[0].Rows[i]["DOS"].ToString();
                //    //    string _SAD = dsEmployeeInfo.Tables[0].Rows[i]["SAD"].ToString();
                //    //    string _JLD = dsEmployeeInfo.Tables[0].Rows[i]["JLD"].ToString();

                //    //    if (string.IsNullOrEmpty(_JLD))
                //    //    {
                //    //        throw new Exception("Joblocation not found for Employee [" + _EmployeeCode + "]");
                //    //    }

                //    //    if (string.IsNullOrEmpty(_SAD) == false)
                //    //    {

                //    //        if (Convert.ToDateTime(_SAD) > ss.EffectiveDate)
                //    //        {
                //    //            throw new Exception("Already assigned shift has greater effective Date [" + _SAD + "] for Employee [" + _EmployeeCode + "]");
                //    //        }
                //    //    }

                //    //    if (Convert.ToDateTime(_DOJ) > ss.EffectiveDate)
                //    //    {
                //    //        throw new Exception("EffectiveDate can not be less than DOJ [" + _DOJ + "] for Employee [" + _EmployeeCode + "]");
                //    //    }

                //    //    if (Convert.ToDateTime(_COD) > ss.EffectiveDate)
                //    //    {
                //    //        throw new Exception("EffectiveDate can not be less than 'Cut-off-Date' [" + _COD + "] for Employee [" + _EmployeeCode + "]");
                //    //    }

                //    //    if (Convert.ToDateTime(_JLD) > ss.EffectiveDate)
                //    //    {
                //    //        throw new Exception("EffectiveDate can not be less than 'Joblocation assigned Date' [" + _JLD + "] for Employee [" + _EmployeeCode + "]");
                //    //    }

                //    //    if (string.IsNullOrEmpty(_DOS) == false)
                //    //    {
                //    //        if (Convert.ToDateTime(_DOS) < ss.EffectiveDate)
                //    //        {
                //    //            throw new Exception("Separation Date can not be less than 'EffectiveDate Date' [" + ss.EffectiveDate + "] for Employee [" + _EmployeeCode + "]");
                //    //        }
                //    //    }


                //    //}//for

                //    //lock validation
                //    AttendanceProcessAplos ob = new AttendanceProcessAplos();
                //    ob.LockValidation(ss.PlantId, ss.EffectiveDate.ToString("dd-MMM-yyyy"), DateTime.Now.ToString("dd-MMM-yyyy"), ss.EmpSystemIDs);//need to work

                //    //GetHRsettinng(identity.PlantId, out dsHRsetting);

                //    DATA_OK = true;

                //    #endregion

                //}
                if (DATA_OK == true)
                {


                    DeleteEmpDateWiseShiftAssign(ss.EmpSystemIDs, ss.EffectiveDate.ToString("dd-MMM-yyyy"));

                    #region NEW ID GENERATE

                    string _PK = "";
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "EMP_SHIFT_ASSIGN", out _PK);

                    #endregion End ID Generate

                    #region Employee Shift Assign
                    int _Count = 0;
                    for (int i = 0; i < dsEmployeeInfo.Tables[0].Rows.Count; i++)
                    {
                        string _empid = dsEmployeeInfo.Tables[0].Rows[i]["SystemID"].ToString();
                        string _systemid = "";
                        ShiftAssignEmp s_ob = new ShiftAssignEmp();
                        s_ob.SystemID = _systemid;
                        s_ob.DateAdded = DateTime.Now;
                        s_ob.DateUpdated = DateTime.Now;
                        s_ob.EffectiveDate = ss.EffectiveDate;
                        s_ob.EmpSystemID = _empid;
                        s_ob.FixSystemID = ss.FixSystemID;
                        s_ob.IsFix = ss.IsFix;

                        s_ob.RosterStartShiftID = ss.RosterStartShiftID;
                        s_ob.RosterSystemID = ss.RosterSystemID;
                        s_ob.UpdatedBy = ss.UpdatedBy;
                        s_ob.AddedBy = ss.AddedBy;
                        dvShiftAssign.RowFilter = "EmpSystemID='" + _empid + "' and EffectiveDate='" + ss.EffectiveDate + "'";
                        if (dvShiftAssign.Count > 0)
                        {
                            s_ob.SystemID = dvShiftAssign[0]["SystemID"].ToString();
                        }
                        dvShiftAssign.RowFilter = null;

                        dvShiftAssign.RowFilter = "SystemId='" + s_ob.SystemID + "'";
                        if (dvShiftAssign.Count == 0)
                        {// Add new block
                            _Count++;
                            s_ob.SystemID = "SAPT" + _PK + "_" + _Count;
                            drShiftAssign = dtShiftAssign.NewRow();
                            UpdateEmployeeShiftAssignDataRow("ADDNEW", s_ob, ref drShiftAssign);
                            dtShiftAssign.Rows.Add(drShiftAssign);
                        }
                        else
                        {//edit block                        
                            drShiftAssign = dvShiftAssign[0].Row;
                            drShiftAssign.BeginEdit();
                            UpdateEmployeeShiftAssignDataRow("EDIT", s_ob, ref drShiftAssign);
                            drShiftAssign.EndEdit();
                        }
                        dvShiftAssign.RowFilter = null;
                    }

                    #endregion Employee Shift Assign

                    SaveDataSets(dsShiftAssign);


                    //if (dsHRsetting.Tables[0].Rows.Count > 0)
                    //{
                    //    DateTime FromDateR = Convert.ToDateTime(ss.EffectiveDate);
                    //    DateTime ToDateR = DateTime.Now;
                    //    while (FromDateR <= ToDateR)
                    //    {

                    //        obj.SetRawINOUTonShiftAssignment(identity.PlantId, identity.CompanyGroupId, FromDateR.ToString("dd-MMM-yyyy"), ss.EmpSystemIDs);
                    //        FromDateR = FromDateR.AddDays(1);
                    //    }
                    //}//hr





                    if (CheckBox == true)
                    {
                        //DateTime FromDate = Convert.ToDateTime(ss.EffectiveDate);
                        //DateTime ToDate = DateTime.Now;
                        //while (FromDate <= ToDate)
                        //{

                        //    objAttdn.SaveTotal(ss.PlantId, FromDate.ToString("dd-MMM-yyyy"), ss.EmpSystemIDs, false, true);
                        //    FromDate = FromDate.AddDays(1);
                        //}
                    }

                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objApp = null;
            }
        }//End Function
        public void SaveDataSets(params DataSet[] dsRef)
        {
            //throw new Exception("test");string empid,string WorkDate,
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                //objCon.ExecuteNonQueryWrapper(" update EmpDateWiseShiftAssign set ToReprocess='Yes' where EmpSystemId='" + empid + @"' and WorkDate>='" + WorkDate + @"' ", true, "1");
                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                        if (dsRef[i].Tables.Count > 0)
                            objCon.SaveDataSetThroughAdapter(ref dsRef[i], true, "1");
                    i++;
                }
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                try
                {
                    if (IsTransactionStarted)
                    {
                        objCon.RollBack();
                    }
                    objCon.CloseConnection();
                }
                catch (Exception exp)
                {
                    throw ex;
                }
            }
            finally
            {
                objCon = null;
            }
        }//End Function
        public void EmployeeShiftAssign_for_Save(string empids, string effectivedate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmployeeShiftAssign
                                    WHERE EmpSystemID in (" + empids + @") 
                                            AND EffectiveDate = '" + effectivedate + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function
        public void DeleteEmpDateWiseShiftAssign(string strEmpIDs, string strEffectDate)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("DELETE FROM dbo.EmpDateWiseShiftAssign WHERE EmpSystemID in (" + strEmpIDs + @") AND WorkDate >= '" + strEffectDate + @"' ", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function
        public void GetEmployeeJobLocation(string empSystemIds, string effectiveDate)
        {
            DataSet dsRef = null;
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT JobLcSystemID FROM EmpDateWiseJobLocation WHERE EmpSystemID in (" + empSystemIds + ") AND EffectiveDate<='" + effectiveDate + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");

                if (dsRef.Tables[0].Rows.Count == 0)
                {
                    throw new Exception("This Employee has no job location before Effective date :'" + effectiveDate + "'.");
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function---
        public void GetEmployeeInfo(string empSystemIds, out DataSet dsRef)
        {
            dsRef = null;
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select e.SystemID,e.EmployeeCode
                                --,e.doj,e.dos,c.CutOffDate COD,a.ed SAD,j.ed JLD
                                ,format(e.doj,'dd-MMM-yyyy') DOJ
                                ,format(e.dos,'dd-MMM-yyyy') DOS
                                ,format(c.CutOffDate,'dd-MMM-yyyy') COD
                                ,format(a.ed,'dd-MMM-yyyy') SAD
                                ,format(j.ed,'dd-MMM-yyyy') JLD

                                from EmployeeInformation e
                                left join scs.OpeningBalanceCutOffDate c on c.PlantId=e.PlantId and c.ModuleName='HR'
                                left join (select max(effectivedate) ed,EmpSystemID from EmployeeShiftAssign WHERE IsSingleDayShift=0 group by EmpSystemID) a on a.EmpSystemID=e.SystemId
                                left join (select max(effectivedate) ed,EmpSystemID from EmpDateWiseJobLocation group by EmpSystemID) j on j.EmpSystemID=e.SystemId
                                WHERE e.SystemID in (" + empSystemIds + ") ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function---
        public bool GetMaxDateOfShiftAssign(string strEmpIDs, string strEffectDate, out System.Data.DataSet dsRef)
        {
            //System.Data.DataSet dsRef = null;
            string strSQl;
            bool blnStatus = false;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQl = @"SELECT MAX(WorkDate) MaxWorkDate,EmpSystemID FROM dbo.EmpDateWiseShiftAssign
                            WHERE EmpSystemID in (" + strEmpIDs + @") AND WorkDate >= '" + strEffectDate + @"'
                                Group by EmpSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQl, out dsRef, false, "1");
                if (dsRef.Tables[0].Rows.Count == 0)
                {
                    blnStatus = true;
                }
                else
                {
                    blnStatus = false;
                }
                return blnStatus;
            }
            catch (Exception ex)
            { throw (ex); }
            finally
            {
                objCon = null;
            }
        }//End Function
        public void GetHRsettinng(string plantid, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from PlantWiseHRMSSetting where PlantID='" + plantid + "' and isnull(ShiftBasedPunchFlag,0)=1";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function
        private void UpdateEmployeeShiftAssignDataRow(string OPN_FLAG, ShiftAssignEmp s, ref DataRow drLocal)
        {
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    drLocal["SystemID"] = s.SystemID;
                    drLocal["EmpSystemID"] = s.EmpSystemID;

                    drLocal["AddedBy"] = s.AddedBy;
                    drLocal["DateAdded"] = DateTime.Now;
                }

                drLocal["IsFix"] = s.IsFix;
                drLocal["IsRoster"] = s.IsRoster;
                drLocal["IsSingleDayShift"] = false;
                if (s.IsFix)
                {
                    drLocal["EffectiveDate"] = s.EffectiveDate;
                    drLocal["FixSystemID"] = s.FixSystemID;

                    drLocal["RosterSystemID"] = DBNull.Value;
                    drLocal["RosterStartShiftID"] = DBNull.Value;
                    drLocal["StartFromDay"] = DBNull.Value;
                }
                if (s.IsRoster)
                {
                    drLocal["EffectiveDate"] = s.EffectiveDate;
                    drLocal["RosterSystemID"] = s.RosterSystemID;
                    drLocal["RosterStartShiftID"] = s.RosterStartShiftID;
                    drLocal["StartFromDay"] = 0;

                    drLocal["FixSystemID"] = DBNull.Value;
                }

                drLocal["UpdatedBy"] = "";
                drLocal["DateUpdated"] = DateTime.Now;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function
         //void ShiftProcess(string OPN_FLAG, string a, string s) { }

        [HttpGet, Authorize]
        public ActionResult GetBudgetCodeList(GridParameter parameters, string PlantId)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(GetBudgetCodeListData(parameters, PlantId), JsonRequestBehavior.AllowGet);
        }


        public GridModel GetBudgetCodeListData(GridParameter parameters, string plantId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT PMB.Id, PMB.Code, PMB.EntityId, ERD.UserName AS EntityName, PMB.PositionId, PRD.UserName AS PositionName
                                        ,PRD.Code PositionCode, ERD.Code EntityCode
                                         , PMB.EmploymentType, PMB.IsOTEntitled, PMB.PayrollGroupId, PMB.WorkGroupId, PMB.Deployment, PRD.IsDirect , ERD.PlantId
                                        , (SELECT UserName FROM[ORG].[Plant] WHERE Id = ERD.PlantId) AS[Plant], ERD.DivisionId
                                        , (SELECT UserName FROM[ORG].[Division] WHERE Id = ERD.DivisionId) AS[Division], ERD.UnitId
                                        , (SELECT UserName FROM[ORG].[Unit] WHERE Id = ERD.UnitId) AS[Unit], PRD.DepartmentId
                                        , (SELECT UserName FROM[ORG].[Department] WHERE Id = PRD.DepartmentId) AS[Department]
                                        , PRD.SectionId, (SELECT UserName FROM[ORG].[Section] WHERE Id = PRD.SectionId) AS[Section]
                                        , PRD.SubSectionId, (SELECT UserName FROM[ORG].[SubSection] WHERE Id = PRD.SubSectionId) AS[SubSection]
                                        , PMB.LineId, (SELECT UserName FROM[ORG].[Line] WHERE Id = PMB.LineId) AS[Line] , PMB.ShiftDefinationId
                                        , (SELECT UserName FROM[dbo].[ShiftDefination] WHERE SystemID = PMB.ShiftDefinationId) AS[ShiftDefination] 
                                        , PRD.DesignationId, 
                                         --(SELECT UserName FROM[HKP].[Designation] WHERE Id = PRD.DesignationId) AS[Designation]
										 (SELECT UserName FROM[HKP].[LegalDesignation] WHERE Id = PRD.DesignationId) AS[Designation]
                                         FROM[MST].[ManpowerBudget]
                                                AS PMB
                                        INNER JOIN ORG.Entity AS ERD ON PMB.EntityId=ERD.Id
                                        INNER JOIN ORG.Position AS PRD ON PMB.PositionId = PRD.Id
                                        WHERE PMB.Active= 1 AND PMB.Archive= 0
                                        ---AND ERD.CompanyGroupId= 'CG20181'
                                        ---AND ERD.CompanyId= 'C20181'
                                        AND ERD.PlantId= '" + plantId+@"'
                                        AND ERD.Active= 1 AND ERD.Archive= 0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
    //public class EmployeePlantTransferModel
    //{
    //    public string EmpSystemId { get; set; }
    //    public string PlantId { get; set; }
    //    public string JobLocationId { get; set; }
    //    public string EffectiveDate { get; set; }
    //    public string BudgetCodeId { get; set; }
    //    public string ShiftId { get; set; }
    //}
    //public class EmployeePlantTransferModelDetails
    //{
    //    public string AddedBy { get; set; }
    //    public string AgreedDOJ { get; set; }
    //    public string AnyRelativeWorkedHere { get; set; }
    //    public string AppliedDate { get; set; }
    //    public string ApplyingAsFresher { get; set; }
    //    public string ApprovedBy { get; set; }
    //    public string ApprovedDateTime { get; set; }
    //    public string ApprovedFromIP { get; set; }
    //    public string BankAccNo { get; set; }
    //    public string BankAddedBy { get; set; }
    //    public string BankDateAdded { get; set; }
    //    public string BankDateUpdated { get; set; }
    //    public string BankName { get; set; }
    //    public string BankSystemID { get; set; }
    //    public string BankUpdatedBy { get; set; }
    //    public string BirthdayCelebrationDate { get; set; }
    //    public string BloodGroupID { get; set; }
    //    public string BnsPolicy { get; set; }
    //    public string BudgetCategoryID { get; set; }
    //    public string BudgetCode { get; set; }
    //    public string CardNumber { get; set; }
    //    public string CellPhnNo { get; set; }
    //    public string CitizenID { get; set; }
    //    public string CivilStatusID { get; set; }
    //    public string Code { get; set; }
    //    public string CompanyId { get; set; }
    //    public string DOB { get; set; }
    //    public string DOBs { get; set; }
    //    public string DOC { get; set; }
    //    public string DOCBy { get; set; }
    //    public string DOCDay { get; set; }
    //    public string DOCIsDay { get; set; }
    //    public string DOCIsMonth { get; set; }
    //    public string DOCMonth { get; set; }
    //    public string DOJ { get; set; }
    //    public string DOJs { get; set; }
    //    public string DOS { get; set; }
    //    public string DOSBy { get; set; }
    //    public string DOSDate { get; set; }
    //    public string DateAdded { get; set; }
    //    public string DateUpdated { get; set; }
    //    public string Department { get; set; }
    //    public string DepartmentId { get; set; }
    //    public string Designation { get; set; }
    //    public string DesignationGroupId { get; set; }
    //    public string DesignationGroupName { get; set; }
    //    public string DesignationId { get; set; }
    //    public string DesignationSystemID { get; set; }
    //    public string DivisionId { get; set; }
    //    public string ESICPolicy { get; set; }
    //    public string EmailId { get; set; }
    //    public string EmpPicPath { get; set; }
    //    public string EmpSignature { get; set; }
    //    public string EmpType { get; set; }
    //    public string EmployeeCategory { get; set; }
    //    public string EmployeeCategorySystemID { get; set; }
    //    public string EmployeeCode { get; set; }
    //    public string EmployeeCodeNumeric { get; set; }
    //    public string EmployeeCodePreFix { get; set; }
    //    public string EmployeeCurrentStatus { get; set; }
    //    public string EmployeeCurrentStatusEffectiveDate { get; set; }
    //    public string EmployeeDisciplinaryActionIdForLA { get; set; }
    //    public string EmployeeGroupSystemID { get; set; }
    //    public string EmployeeId { get; set; }
    //    public string EmployeeName { get; set; }
    //    public string EmployeeNameLocal { get; set; }
    //    public string EmployeeStatus { get; set; }
    //    public string EmploymentType { get; set; }
    //    public string EmrCntPer1CellNo { get; set; }
    //    public string EmrCntPer1CellNo2 { get; set; }
    //    public string EmrCntPer1CellNo3 { get; set; }
    //    public string EmrCntPer1Name { get; set; }
    //    public string EmrCntPer2CellNo { get; set; }
    //    public string EmrCntPer2CellNo2 { get; set; }
    //    public string EmrCntPer2CellNo3 { get; set; }
    //    public string EmrCntPer2Name { get; set; }
    //    public string EntityName { get; set; }
    //    public string ExitReason { get; set; }
    //    public string ExitReasonLocal { get; set; }
    //    public string ExtensionNo { get; set; }
    //    public string FatherName { get; set; }
    //    public string FatherNameLocal { get; set; }
    //    public string FirstName { get; set; }
    //    public string GenderID { get; set; }
    //    public string GivenDesignation { get; set; }
    //    public string GivenDesignationId { get; set; }
    //    public string GroupID { get; set; }
    //    public string Height { get; set; }
    //    public string IdentificationMark { get; set; }
    //    public string IsAccessible { get; set; }
    //    public string IsApproved { get; set; }
    //    public string IsAttdnProcBaseOnDeviceData { get; set; }
    //    public string IsConfirmed { get; set; }
    //    public string IsDirect { get; set; }
    //    public string IsImage { get; set; }
    //    public string IsKnownPerson { get; set; }
    //    public string IsSlvDevReg { get; set; }
    //    public string IssueDate { get; set; }
    //    public string JobLocationID { get; set; }
    //    public string LVPolicyMasterSystemID { get; set; }
    //    public string LastName { get; set; }
    //    public string LeavePolicyName { get; set; }
    //    public string LegalDesignation { get; set; }
    //    public string LegalDesignationId { get; set; }
    //    public string LineId { get; set; }
    //    public string LocalIdentificationMark { get; set; }
    //    public string MarriagedayCelebrationDate { get; set; }
    //    public string MiddleName { get; set; }
    //    public string MotherName { get; set; }
    //    public string MotherNameLocal { get; set; }
    //    public string NationalID { get; set; }
    //    public string NickName { get; set; }
    //    public string NoOfChildren { get; set; }
    //    public string NumberOfKnownPerson { get; set; }
    //    public string OperationMasterID { get; set; }
    //    public string OperationVariationId { get; set; }
    //    public string PFPolicy { get; set; }
    //    public string ParmAreaID { get; set; }
    //    public string ParmCityID { get; set; }
    //    public string ParmCountryID { get; set; }
    //    public string ParmDistrictID { get; set; }
    //    public string ParmPostOfficeID { get; set; }
    //    public string ParmStateId { get; set; }
    //    public string ParmThanaID { get; set; }
    //    public string ParmZipCode { get; set; }
    //    public string ParmanentAddress1 { get; set; }
    //    public string ParmanentAddress1Local { get; set; }
    //    public string ParmanentAddress2 { get; set; }
    //    public string ParmanentAddress2Local { get; set; }
    //    public string ParmanentArea { get; set; }
    //    public string PayRollGroupId { get; set; }
    //    public string PayRollGroupName { get; set; }
    //    public string PaymentMode { get; set; }
    //    public string PaymentModeEffectiveDate { get; set; }
    //    public string PlantId { get; set; }
    //    public string PlantName { get; set; }
    //    public string PositionID { get; set; }
    //    public string PositionName { get; set; }
    //    public string PreRecruitmentEmployeeId { get; set; }
    //    public string PresAreaID { get; set; }
    //    public string PresCityID { get; set; }
    //    public string PresCountryID { get; set; }
    //    public string PresDistrictID { get; set; }
    //    public string PresPostOfficeID { get; set; }
    //    public string PresStateId { get; set; }
    //    public string PresThanaID { get; set; }
    //    public string PresZipCode { get; set; }
    //    public string PresentAddress1 { get; set; }
    //    public string PresentAddress1Local { get; set; }
    //    public string PresentAddress2 { get; set; }
    //    public string PresentAddress2Local { get; set; }
    //    public string PresentArea { get; set; }
    //    public string PreviousDesignation { get; set; }
    //    public string PreviousDesignationLocal { get; set; }
    //    public string PreviousEmployeeCode { get; set; }
    //    public string PreviousSalary { get; set; }
    //    public string PreviousServicePeriod { get; set; }
    //    public string PreviouslyWorkedHere { get; set; }
    //    public string ProbationConfirmEntryDate { get; set; }
    //    public string ProximityAddedBy { get; set; }
    //    public string ProximityAddedDate { get; set; }
    //    public string ProximityUpdatedBy { get; set; }
    //    public string ProximityUpdatedDate { get; set; }
    //    public string ReActiveDate { get; set; }
    //    public string RegisterFP { get; set; }
    //    public string RegisterProximate { get; set; }
    //    public string RelationShip { get; set; }
    //    public string RelativeCellNo { get; set; }
    //    public string RelativeSystemId { get; set; }
    //    public string ReligionID { get; set; }
    //    public string SalaryGrade { get; set; }
    //    public string SalaryPercentage { get; set; }
    //    public string SalaryRule { get; set; }
    //    public string SalaryRuleMasterSystemID { get; set; }
    //    public string SalaryRuleMasterId { get; set; }
    //    public string SalaryRuleName { get; set; }
    //    public string Salutation { get; set; }
    //    public string SectionId { get; set; }
    //    public string SpecialReviewAmount { get; set; }
    //    public string SpecialReviewDuration { get; set; }
    //    public string SpouseName { get; set; }
    //    public string SpouseNameLocal { get; set; }
    //    public string SpouseNationalID { get; set; }
    //    public string SpouseOccupation { get; set; }
    //    public string SubSecStrucSystemID { get; set; }
    //    public string SubSectionId { get; set; }
    //    public string SubdivisionID { get; set; }
    //    public string SystemId { get; set; }
    //    public string TIN { get; set; }
    //    public string TelePhnNo { get; set; }
    //    public string TentativeExpiryDate { get; set; }
    //    public string TotalSalary { get; set; }
    //    public string UnitId { get; set; }
    //    public string UpdatedBy { get; set; }
    //    public string Weight { get; set; }

    //}




    //public class BudgetCodeModel
    //{
    //    public string Code { get; set; }
    //    public string Id { get; set; }
    //    public string DesignationSystemID { get; set; }
    //    public string UnitId { get; set; }
    //    public string DivisionId { get; set; }
    //    public string DepartmentId { get; set; }
    //    public string SectionId { get; set; }
    //    public string SubSectionId { get; set; }
    //    public string SubdivisionID { get; set; }
    //    public string LineId { get; set; }
    //    public string EmploymentType { get; set; }
    //    public string PositionID { get; set; }
    //    public string IsDirect { get; set; }
    //}
}
