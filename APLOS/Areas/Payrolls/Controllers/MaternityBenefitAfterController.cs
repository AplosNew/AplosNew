
using System;
using System.Web.Mvc;
using Aplos.Controllers;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using Library.Service.Payrolls.SalaryProcess;
using System.Collections.Generic;
using OTSBD;
using System.Data;
using Aplos.Properties;

namespace Aplos.Areas.Payrolls.Controllers
{

    public class MaternityBenefitAfterController : BaseController
    {
        #region Constructor
        /// <summary>   The separationTypeService service. </summary>
       
        private readonly ISqlRepository _sqlRepository;
        public MaternityBenefitAfterController( ISqlRepository sqlRepository)
        {          
            _sqlRepository = sqlRepository;
        }
        #endregion

        #region Aplos
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations


        [HttpGet, Authorize]
        public ActionResult GetSeparationTypelist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT Id,UserName FROM hkp.[SeparationType] ORDER BY Sequence";

            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult CalculateSalary(string empid,string empLeaveId)
        {
            var data = _sqlRepository.GetDataCollection("");

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Save(MaternityBenefitMaster master)
        {
            if (master.IsPaidBefore == true && master.BeforePaymentDate == null)
            {
                Exception ex = new Exception("please select before date......");
                throw (ex);
            }
            if (master.IsPaidAfter == true && master.AfterPaymentDate == null)
            {
                Exception ex = new Exception("please select after date......");
                throw (ex);
            }

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsMaternityBenefit obj = new clsMaternityBenefit();
            master.AddedBy = identity.UserId;
            master.AddedFromIP = identity.IPAddress;
            master.PlantId = identity.PlantId;
            master.UpdatedBy = identity.Name;
            obj.SaveMasterAndDetailForAfter(master);            
            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult ShowInfo(string empid, string LeaveTransactionId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsMaternityBenefit obj = new clsMaternityBenefit(_sqlRepository);
            var data = obj.ShowSalaryInfoAfter(empid, LeaveTransactionId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult LevalValue(string empid, string LeaveTransactionId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsMaternityBenefit obj = new clsMaternityBenefit(_sqlRepository);
            var data = obj.GetLavelValue(empid, LeaveTransactionId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        
        [HttpGet]
        public ActionResult LoadEmployeelist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = string.Empty;
            try
            {
                 sql = @"select EI.SystemId
                            ,EI.EmployeeCode
                            ,EI.EmployeeName
                            , FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                            , DG.UserName GivenDesignation
                            , DP.UserName Department
                            , DSG.UserName LegalDesignation
		                    ,s.UserName Section
		                    ,ss.UserName Subsection
		                    ,ll.UserName Line
		                    ,format(t.fromdate,'dd-MMM-yyyy') LeaveStartDate ,t.LeaveDays
                            ,t.SystemID LeaveTransactionId,EI.SystemId EmpSystemId
                            ,mp.IsBefore,mp.BeforePercentage,mp.IsAfter,mp.AfterPercentage,mp.ChildNo 
                            , FORMAT(t.todate,'dd-MMM-yyyy') LeaveEndDate
                            , FORMAT(t.ExpectedDelivaryDate,'dd-MMM-yyyy') EDD
                    from [dbo].[MaternityBenefitMaster] mbm
                    inner join dbo.Employeeinformation EI on EI.SystemId=mbm.EmpSystemId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = EI.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                    LEFT JOIN HKP.LegalDesignation DSG ON ei.LegalDesignationId=DSG.Id
                    LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                    LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId							
                    LEFT JOIN org.Section s ON s.id=PR.SectionId
                    LEFT JOIN org.SubSection ss ON ss.Id=PR.SubSectionId
                    left join org.Line ll on ll.id=MB.LineId
                    left join (select * FROM LeaveTransaction where LTSystemID in (select id from LeaveType where LeaveType='Maternity')) t on t.EmpSystemID=ei.SystemId and t.SystemID=mbm.LeaveTransactionId  and t.PlantID='" + identity.PlantId+@"'
                    left join mst.MaternityLeavePolicy mp on mp.id=t.MaternityLeavePolicyId		
                    WHERE (IsPaidAfter=0 or IsPaidBefore=0)";

                var data = _sqlRepository.GetDataCollection(sql);
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
                  
            }
        }

        string GetMonthList(string LeaveStartDate)
        {
            string WC = " Where ";
            try
            {
                var v = Convert.ToDateTime(LeaveStartDate).AddMonths(3);
                WC += "(YearNo="+v.ToString("yyyy")+" and MonthNo="+v.ToString("MM")+")";
                var v2 = Convert.ToDateTime(v).AddMonths(1);
                WC += " or (YearNo=" + v2.ToString("yyyy") + " and MonthNo=" + v2.ToString("MM") + ")";
                var v3 = Convert.ToDateTime(v2).AddMonths(1);
                WC += " or (YearNo=" + v3.ToString("yyyy") + " and MonthNo=" + v3.ToString("MM") + ")";
                return WC;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetEmpSalary(string EmpSystemId,string LeaveStartDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = string.Empty;
            try
            {
                string WC = string.Empty;//(YearNo=2019 and MonthNo=8) or (YearNo=2019 and MonthNo=9) or (YearNo=2019 and MonthNo=10)
                WC = GetMonthList(LeaveStartDate);
                sql = @"select m.MonthNo,m.YearNo
                            ,DateName( month , DateAdd( month , m.MonthNo , -1 )) [MonthName]
                            ,h.SalaryHead,c.EntryAmount StructureAmount,c.DisbusmentAmount EarnedAmount
                            --,att.TotalProcDate,att.TotalAbsent,att.TotalHoliDay,att.TotalLWP,att.TotalWeekOff
                            ,ActualWorkingDays=att.TotalProcDate-att.TotalAbsent-att.TotalHoliDay-att.TotalLWP-att.TotalWeekOff
                            ,b.BonusAmount,b.EffectiveDate
                            from SalaryProcChild c
                            inner join (select * from SalaryHead where HeadCategory in( 'Gross')) h on h.SalaryHeadID=c.SalaryHeadID
                            left join SalaryProcMaster m on m.SystemID=c.SlrProcMstSystemID
                            left join SalaryProceAttdnData att on att.SlrProcMstSystemID=m.SystemID and att.EmpSystemID='" + EmpSystemId + @"'                           
                            left join (select c.*,m.EffectiveDate from BonusPaymentActual c
                            left join [BonusPaymentActualMaster] m on m.SystemID=c.BnsMstSystemID
                            )b on b.EmpSystemID='" + EmpSystemId + @"' and month(b.EffectiveDate)=m.MonthNo
                            where c.SlrProcMstSystemID in (
                            select SystemID from SalaryProcMaster "+WC+@"
                            )
                            and c.EmpInfoSystemID='" + EmpSystemId + @"'";

                var data = _sqlRepository.GetDataCollection(sql);
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSingleEmployeeInfo(string EmpSystemId, string LeaveStartDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = string.Empty;
            try
            {
                string WC = string.Empty;//(YearNo=2019 and MonthNo=8) or (YearNo=2019 and MonthNo=9) or (YearNo=2019 and MonthNo=10)
                WC = GetMonthList(LeaveStartDate);
                sql = @"
                                declare @emp varchar(30)
                                set @emp='"+ EmpSystemId + @"'
                                SELECT  EI.SystemId
                                        ,EI.EmployeeCode
                                        ,EI.EmployeeName
                                        , FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                                        , FORMAT(EI.DOB,'dd-MMM-yyyy') DOB
                                        , DG.UserName GivenDesignation
                                        , DP.UserName Department
                                        , DSG.UserName LegalDesignation
		                                ,s.UserName Section
		                                ,ss.UserName Subsection
		                                ,ll.UserName Line
		                                ,format(t.fromdate,'dd-MMM-yyyy') LeaveStartDate
		                                ,t.LeaveDays
		                                ,mp.IsBefore,mp.BeforePercentage,mp.IsAfter,mp.AfterPercentage
		                                ,x.TotalDays,x.TotalEarn,Rate=isnull(x.TotalEarn,0)/isnull(x.TotalDays,0)
		                                ,TotlaEarning=(isnull(x.TotalEarn,0)/isnull(x.TotalDays,0))*t.LeaveDays
		                                ,BeforePercentageAmount=((isnull(x.TotalEarn,0)/isnull(x.TotalDays,0))*t.LeaveDays)*mp.BeforePercentage/100
		                                ,AfterPercentageAmount=((isnull(x.TotalEarn,0)/isnull(x.TotalDays,0))*t.LeaveDays)*mp.AfterPercentage/100

                                        FROM dbo.Employeeinformation EI
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = EI.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                                        LEFT JOIN HKP.LegalDesignation DSG ON ei.LegalDesignationId=DSG.Id
                                        LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                                        LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId							
                                        LEFT JOIN org.Section s ON s.id=PR.SectionId
                                        LEFT JOIN org.SubSection ss ON ss.Id=PR.SubSectionId
		                                left join org.Line ll on ll.id=ei.LineId
		                                left join (select * FROM LeaveTransaction where LTSystemID in (select id from LeaveType where LeaveType='Maternity')) t on t.EmpSystemID=ei.SystemId
		                                left join mst.MaternityLeavePolicy mp on mp.id=t.MaternityLeavePolicyId
		                                left join
		                                (--x
		                                select c.EmpInfoSystemID,sum(c.DisbusmentAmount+isnull(b.BonusAmount,0)) TotalEarn
					                                ,sum(att.TotalProcDate-att.TotalAbsent-att.TotalHoliDay-att.TotalLWP-att.TotalWeekOff) TotalDays
					                                from SalaryProcChild c
					                                inner join (select * from SalaryHead where HeadCategory in( 'Gross')) h on h.SalaryHeadID=c.SalaryHeadID
					                                left join SalaryProcMaster m on m.SystemID=c.SlrProcMstSystemID
					                                left join SalaryProceAttdnData att on att.SlrProcMstSystemID=m.SystemID and att.EmpSystemID=@emp
					                                left join (select c.*,m.EffectiveDate from BonusPaymentActual c
					                                left join [BonusPaymentActualMaster] m on m.SystemID=c.BnsMstSystemID
					                                )b on b.EmpSystemID=@emp and month(b.EffectiveDate)=m.MonthNo
					                                where c.SlrProcMstSystemID in (
					                                select SystemID from SalaryProcMaster " + WC+@"
					                                )
					                                and c.EmpInfoSystemID=@emp
					                                group by c.EmpInfoSystemID
		                                )--x 
		                                x on x.EmpInfoSystemID=ei.SystemId
		                                where ei.SystemId =@emp";

                var data = _sqlRepository.GetDataCollection(sql);
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }
        }
        [HttpGet, Authorize]
        public ActionResult GetEmployeeLeaveInfo(string EmpSystemId, string LeavePK)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = string.Empty;
            try
            {
               // string WC = string.Empty;//(YearNo=2019 and MonthNo=8) or (YearNo=2019 and MonthNo=9) or (YearNo=2019 and MonthNo=10)
               // WC = GetMonthList(LeaveStartDate);
                sql = @"
                                declare @emp varchar(30)
                                set @emp='" + EmpSystemId + @"'
                                SELECT  EI.SystemId
                                        ,EI.EmployeeCode
                                        ,EI.EmployeeName
                                        , FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                                        , FORMAT(EI.DOB,'dd-MMM-yyyy') DOB
                                        , DG.UserName GivenDesignation
                                        , DP.UserName Department
                                        , DSG.UserName LegalDesignation
		                                ,s.UserName Section
		                                ,ss.UserName Subsection
		                                ,ll.UserName Line
		                                ,format(t.fromdate,'dd-MMM-yyyy') LeaveStartDate
		                                ,t.LeaveDays
		                                ,mp.IsBefore,mp.BeforePercentage,mp.IsAfter,mp.AfterPercentage
		                                ,x.TotalDays,x.TotalEarn,Rate=isnull(x.TotalEarn,0)/isnull(x.TotalDays,0)
		                                ,TotlaEarning=(isnull(x.TotalEarn,0)/isnull(x.TotalDays,0))*t.LeaveDays
		                                ,BeforePercentageAmount=((isnull(x.TotalEarn,0)/isnull(x.TotalDays,0))*t.LeaveDays)*mp.BeforePercentage/100
		                                ,AfterPercentageAmount=((isnull(x.TotalEarn,0)/isnull(x.TotalDays,0))*t.LeaveDays)*mp.AfterPercentage/100

                                        FROM dbo.Employeeinformation EI
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                                        LEFT JOIN HKP.LegalDesignation DSG ON ei.LegalDesignationId=DSG.Id
                                        LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                                        LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId							
                                        LEFT JOIN org.Section s ON s.id=PR.SectionId
                                        LEFT JOIN org.SubSection ss ON ss.Id=PR.SubSectionId
		                                left join org.Line ll on ll.id=MB.LineId
		                                left join (select * FROM LeaveTransaction where SystemID ='" + LeavePK + @"') t on t.EmpSystemID=ei.SystemId
		                                left join mst.MaternityLeavePolicy mp on mp.id=t.MaternityLeavePolicyId		                                
		                                where ei.SystemId =@emp";

                var data = _sqlRepository.GetDataCollection(sql);
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }
        }
        
        bool IsEligible(string pit,string pot,string pt,string shiftInTime,string ProcessDate)
        {
            bool _isEligible = false;
            try
            {
                var _punchInTime = Convert.ToDateTime(pit);
                var _punchOutTime = Convert.ToDateTime(pot);
                var _policyTime = Convert.ToDateTime(pt);
                var _shiftintime = Convert.ToDateTime(shiftInTime);
                string policyDateTime = _punchInTime.ToString("dd-MMM-yyyy") + " " + _policyTime.ToString("HH:mm");
                string shiftDateTime = _punchInTime.ToString("dd-MMM-yyyy") + " " + _shiftintime.ToString("HH:mm");

                if (Convert.ToDateTime(shiftDateTime) > Convert.ToDateTime(policyDateTime))//night shift so add one day
                {
                    string _fpdt = Convert.ToDateTime(ProcessDate).AddDays(1) + " " + _policyTime.ToString("HH:mm");
                    //if(_fpdt between pi and po)
                    //{
                    //    _isEligible = true;
                    //}
                }
                else //same date
                {
                    string _fpdt = ProcessDate + " " + _policyTime.ToString("HH:mm");
                    //if (_fpdt between pi and po)
                    //{
                    //    _isEligible = true;
                    //}
                }
                return _isEligible;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetDataForEdit(string Id)
        {
            
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string SeparationTypesql = @"SELECT * FROM hkp.[SeparationType] WHERE Id='" + Id + "'";
            string SeparationTypeDetailssql = @"SELECT * FROM [SeparationTypeDetails] WHERE SeparationTypeId='"+ Id + "'";

            var SeparationType = _sqlRepository.GetDataCollection(SeparationTypesql);
            var SeparationTypeDetails = _sqlRepository.GetDataCollection(SeparationTypeDetailssql);

            return Json(new { SeparationType, SeparationTypeDetails }, JsonRequestBehavior.AllowGet);
        }

       

        [HttpGet, Authorize]
        public ActionResult xGetSeparationTypelist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT Id
                                , Sequence
                                , Code
                                , ShortName
                                , StandardName
                                , UserName
                                , [Description]
                                , Remarks
                                , FormulaDes	
                                , FormulaDesID	
                                , PlantID	
                                , IsGratuityApplicable
                                , IsActive      
                                 FROM [HKP].[SeparationType]
                                 WHERE PlantID='" + identity.PlantId + @"'";


            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmploymentTypelist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT UserName  FROM EmploymentTypeEnum  ";


            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT max(Sequence)+1 Sequence FROM hkp.[SeparationType] WHERE PlantID='"+ identity .PlantId+ "'";          

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        #endregion
    }

   

}