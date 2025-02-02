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
    public class EmployeePlantTransferNewController : BaseController
    {



        private readonly ISqlRepository _sqlRepository;



        private readonly IEmployeeInformationService _employeeInformationService;
        private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly EmployeePromotionNewService _employeePromotionService;
        private readonly IRecruitmentSelectionService _preRecruitmentEmployee;
        public EmployeePlantTransferNewController(
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


        #region -- Pages

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
								  ,ld.UserName LegalDesignation,LSGD.LegalDesignationId
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
                              LEFT JOIN  HKP.LegalDesignation AS ld ON ld.Id=LSGD.LegalDesignationId----


                              LEFT JOIN mst.DesignationMasterLegalDesignation AS dmld ON dmld.LegalDesignationId=LSGD.LegalDesignationId-------
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


        [HttpPost,Authorize]
        public ActionResult SaveData(EmployeePlantTransferModel data, EmployeePlantTransferModelDetails olddata, EmployeePlantTransferModelDetails newdata, BudgetCodeModel bc)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            clsStaticInfo obj = new clsStaticInfo();
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

                
                #endregion

                #region EmployeeInformation 
                string sql = @" UPDATE EmployeeInformation SET PlantId = '" + data.PlantId + @"'
                                , JobLocationID='" + data.JobLocationId + @"'
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


                obj.SaveDataSets(dsMaster, dsEmployeePlantTransferHistory);

                //update raw table
                string u_AttdnRawData = @"update AttdnRawData set plantid='" + data.PlantId + @"'  where pDate >='" + data.EffectiveDate + "'  and LogDownLoadNum ='" + data.EmpSystemId + @"'";
                ExecuteRawSQL(u_AttdnRawData);

                ////update OtProcessDay limit Table
                //string u_DayLimit = @"update OTProcessDayLimit set DateUpdated=GETDATE(),PlantID='" + data.PlantId + @"' where WorkDate>='" + data.EffectiveDate + "' and EmpSystemID='" + data.EmpSystemId + @"'";
                //ExecuteRawSQL(u_DayLimit);

                #region Salary Tables
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

                #endregion

                #region Updation in APD
                DataSet ShiftdataSource;
                ShiftData(out ShiftdataSource, data.ShiftId); // Shift Data Columns
                if (ShiftdataSource.Tables[0].Rows.Count > 0)
                {
                    string Shiftdurn = ShiftdataSource.Tables[0].Rows[0][@"ShiftDuration"].ToString();
                    string Shifthalfdaydurn = ShiftdataSource.Tables[0].Rows[0][@"HalfDayDuration"].ToString();
                    string FullDayDuration = ShiftdataSource.Tables[0].Rows[0][@"FullDayDuration"].ToString();
                    string ShortDuration = ShiftdataSource.Tables[0].Rows[0][@"ShortDuration"].ToString();
                    string HoursWithoutOT = ShiftdataSource.Tables[0].Rows[0][@"HoursWithoutOT"].ToString();

                    string u_APD = @"update AttdnProcessData set PlantID='" + data.PlantId + @"',ShiftSystemID='" + data.ShiftId + @"',
                    ShiftFullDayDuration='" + FullDayDuration + @"',DateUpdated=GETDATE(),UpdatedBy='PlantTransfer', ShiftDuration='" + Shiftdurn + @"',ShiftShortDuration='" + ShortDuration + @"',
                    ShiftHoursWithoutOT='" + HoursWithoutOT + @"',ShiftHalfDayDuration='" + Shifthalfdaydurn + @"' where EmpSystemID='"+data.EmpSystemId+@"'
                    and WorkDate>='"+data.EffectiveDate+"'";


                    ExecuteRawSQL(u_APD);
                }
                #endregion

                
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }
      
        public void ShiftData(out DataSet ds, string ShiftId)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select ShiftDuration,HalfDayDuration,FullDayDuration,ShortDuration,HoursWithoutOT
                from ShiftDefination where SystemID='" + ShiftId+"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
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
                catch (Exception exp)
                {
                    throw exp;
                }
            }
            finally
            {

                objCon = null;
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
                    throw exp;
                }
            }
            finally
            {
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
       
        [HttpGet, Authorize]
        public ActionResult GetBudgetCodeList(GridParameter parameters, string PlantId)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_preRecruitmentEmployee.GetBudgetCodeList(parameters, PlantId), JsonRequestBehavior.AllowGet);
        }
    }
   
}
 