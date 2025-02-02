using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Attendances;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.Mvc;
using Library.HumanResource.NewAttendanceProcess;

namespace Aplos.Areas.Attendances.Controllers
{
    public class MonthlyAttendanceInformationNewController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
      
        public MonthlyAttendanceInformationNewController(
              ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages

        
        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult MonthlyInfoAll()
        {
            return View();
        }

        public ActionResult MonthlyInfoDateRange()
        {
            return View();
        }

        #endregion -- Pages


        #region -- Operations
        [HttpGet, Authorize]
        public ActionResult XlsDepWiseAttnRptView(string Month, string Year, string DayStatus, bool withColor, string[] empParameters, bool includeCurrentDate, bool isActive, bool isSeperated, bool isMaternity)
        {

            //var paramValues = HttpContext.Current.Request.Params.GetValues("listOfIds");
            Dictionary<string, string> empParameters1 = new Dictionary<string, string>();
            if (empParameters.Length > 0)
            {
                if (!string.IsNullOrEmpty(empParameters[0].ToString()))
                {
                    empParameters1.Add("EmpSystemId", empParameters[0].ToString());
                }

            }


            try
            {


                NewAttdnMonthlySummaryService app = new NewAttdnMonthlySummaryService();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var workbook = app.XlsMonthlyAttendanceSummaryReport(identity.CompanyId, identity.PlantId, Month, Year, identity.Name, DayStatus, empParameters1, withColor, includeCurrentDate, false, isActive, isSeperated, isMaternity);



                return RenderReportAsPdf(workbook, "MonthlyAttdnInfo");
             }

            catch (Exception ex)
            {
                throw ex;
            }

        }





        [HttpPost, Authorize]
        public ActionResult XlsDepWiseAttnRpt(string Month, string Year, string DayStatus, Dictionary<string, string> empParameters, bool withColor, bool includeCurrentDate, bool withSummary, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {
                NewAttdnMonthlySummaryService app = new NewAttdnMonthlySummaryService();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = "MonthlyAttdnInfo" + DateTime.Now.ToString("yyMMdd") + ".xlsx";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;
                var workbook = app.XlsMonthlyAttendanceSummaryReport(identity.CompanyId, identity.PlantId, Month, Year, identity.Name, DayStatus, empParameters, withColor, includeCurrentDate, withSummary, isActive, isSeperated, isMaternity);

                workbook.Version = ExcelVersion.Excel2016;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);


            }

            catch (Exception ex)
            {
                throw ex;
            }

        }
        
        [HttpGet, Authorize]
        public ActionResult GetEmployeeInformation(string EffectiveDate, string criteria)
        {
            string sql = string.Empty;
            try
            {
                EffectiveDate = DateTime.Now.ToString();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsShiftInfo ob = new clsShiftInfo(_sqlRepository);
                var data = ob.GetEmpInfo(identity.CompanyGroupId, identity.PlantId, EffectiveDate, criteria);

                return Json(new { EmpInfo = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

      
       [HttpGet, Authorize]
        public ActionResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT Id,YearNo FROM YearlyCalendar WHERE PlantId='" + identity.PlantId + "'  ORDER BY YearNo DESC ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);

        }

        [HttpPost, Authorize]
        public ActionResult XlsDepWiseAttnRptDateRange(string FromDate, string ToDate, string DayStatus, string employeeStatus, Dictionary<string, string> empParameters, bool withColor, bool includeCurrentDate, bool withSummary, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {
                NewAttdnMonthlyDateRangeSummaryService appx = new NewAttdnMonthlyDateRangeSummaryService();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var fileName = "MonthlyAttdnInfo" + DateTime.Now.ToString("yyMMdd") + ".xlsx";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;
                var workbook = appx.XlsMonthlyAttendanceSummaryReportDateRange(identity.CompanyId, identity.PlantId, FromDate, ToDate, identity.Name, DayStatus, empParameters, withColor, includeCurrentDate, withSummary, isActive, isSeperated, isMaternity);

                workbook.Version = ExcelVersion.Excel2016;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);


            }

            catch (Exception ex)
            {
                throw ex;
            }

        }

        class OTReport
        {
            public decimal TotalOTHr { get; set; }
            public string EmployeeCode { get; set; }
            public DateTime workdate { get; set; }

        }

        [HttpPost, Authorize]
        public ActionResult GetEmpInfoDateRang(string fromDate, string toDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //var month = Convert.ToDateTime(toDate).AddMonths(1);
            //var Ld = month.AddDays(-1);
            var wcPayrollGroup = "";
            var wcSalaryProcess = "";
            var salaryProcessJoin = "";
            var salaryProcessColumn = "";
            var strDOJ = "";
            string param = "";
            string salaryProcessFlag = "";
            string wcEmpStatus = "";
            wcEmpStatus = " Where (1=0 ";

            if (isActive == true && isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " Where (1=1 ";
            }
            else
            {
                if (isActive == true)
                {
                    wcEmpStatus += " OR CurrentMonthEmployeeStatus ='Regular'";
                }
                if (isSeperated == true)
                {
                    wcEmpStatus += " OR CurrentMonthEmployeeStatus ='Separated'";
                }

            }
            wcEmpStatus += ")";

            param = "E.GroupID='" + identity.CompanyGroupId + "' AND E.CompanyId='" + identity.CompanyId + "' AND E.PlantId='" + identity.PlantId + "'";

            var cmdText = @"SELECT * fROM (  SELECT   dISTINCT        [CheckBoxSelect] = Convert(bit, 'False'),
                                     isnull(e.SystemId,'') EmpSystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId                                     
                                    ,ISNULL(e.EmployeeCode,'') EmployeeCode
                                    ,ISNULL(e.EmployeeName,'') EmployeeName								
                                    ,ISNULL(mpb.EntityId,'') EntityId
									,ISNULL(mpb.PositionId,'') PositionId     
                                    ,ISNULL(e.EmployeeCurrentStatus,'') EmployeeCurrentStatus	
                                    ,isnull(ld.UserName,'') Designation                                       
									,ISNULL(Department.UserName,'') Department 
									,ISNULL(Division.UserName,'') Division 
									,ISNULL(EmpC.UserName,'') EmployeeCategory
									,ISNULL(Plant.UserName,'') Plant 
									,ISNULL(Section.UserName,'') Section 
									,ISNULL(SubSection.UserName,'') SubSection 
									,ISNULL(Unit.UserName,'') Unit 
                                    ,ISNULL(Line.UserName,'') Line
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    , CASE WHEN MONTH(DOS) =  MONTH('" + fromDate + @"')  AND YEAR(DOS) = YEAR('" + fromDate + @"') then 'Separated' else 'Regular' end CurrentMonthEmployeeStatus
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
                                    
                                    
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(e.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName

                                    FROM EmployeeInformation e                                   
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId
                                    LEFT JOIN [ORG].[Line] ON Line.Id = mpb.LineId

                                    
						LEFT JOIN [HKP].[LegalDesignation] as Ld on Ld.Id=E.LegalDesignationId
			LEFT JOIN [MST].DesignationMasterLegalDesignation LDM ON LDM.LegalDesignationId=E.LegalDesignationId
			LEFT JOIN [MST].DesignationMaster DesM ON DesM.Id = LDM.DesignationMasterId
            LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
									Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                    " + salaryProcessJoin + @"
                                    Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
									left join [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									left join [HKP].[Bank] bb on bb.Id = ebi.BankSystemID

                                     WHERE " + param + @" " + strDOJ + @"
                                            " + wcPayrollGroup + @"  " + wcSalaryProcess + @"  
                                                    
                                        AND
									(E.DOS IS NULL OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + fromDate + @"')
                                    and e.DOJ <= '" + toDate + @"'
                                    ) 
                                     ) DD " + wcEmpStatus + @" ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";


            var jsondata = Json(_sqlRepository.GetDataCollection(cmdText), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }


        [HttpPost, Authorize]
        public ActionResult GetEmpInfo(string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity,string PlantId)
        {
            string Plant = string.Empty;
            
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            if (PlantId == "" || PlantId == null)
            {
                Plant = identity.PlantId;
            }
            else
            {
                Plant = "'" + PlantId.Replace(",", "','") + "'";//replaced with ""
            }

            var month = Convert.ToDateTime(effectiveDate).AddMonths(1);
            var Ld = month.AddDays(-1);
            var wcPayrollGroup = "";
            var wcSalaryProcess = "";
            var salaryProcessJoin = "";
            var salaryProcessColumn = "";
            var strDOJ = "";
            string param = "";
            string salaryProcessFlag = "";
            string wcEmpStatus = "";
            wcEmpStatus = " Where (1=0 ";

            if (isActive == true && isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " Where (1=1 ";
            }
            else
            {
                if (isActive == true)
                {
                    wcEmpStatus += " OR CurrentMonthEmployeeStatus ='Regular'";
                }
                if (isSeperated == true)
                {
                    wcEmpStatus += " OR CurrentMonthEmployeeStatus ='Separated'";
                }

            }
            wcEmpStatus += ")";

            param = "E.GroupID='" + identity.CompanyGroupId + "' AND E.CompanyId='" + identity.CompanyId + "' AND E.PlantId in (" + Plant + ")";

            var cmdText = @"SELECT * fROM (  SELECT   dISTINCT        [CheckBoxSelect] = Convert(bit, 'False'),
                                     isnull(e.SystemId,'') EmpSystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId                                     
                                    ,ISNULL(e.EmployeeCode,'') EmployeeCode
                                    ,ISNULL(e.EmployeeName,'') EmployeeName								
                                    ,ISNULL(mpb.EntityId,'') EntityId
									,ISNULL(mpb.PositionId,'') PositionId     
                                    ,ISNULL(e.EmployeeCurrentStatus,'') EmployeeCurrentStatus	
                                    ,isnull(ld.UserName,'') Designation                                       
									,ISNULL(Department.UserName,'') Department 
									,ISNULL(Division.UserName,'') Division 
									,ISNULL(EmpC.UserName,'') EmployeeCategory
									,ISNULL(Plant.UserName,'') Plant 
									,ISNULL(Section.UserName,'') Section 
									,ISNULL(SubSection.UserName,'') SubSection 
									,ISNULL(Unit.UserName,'') Unit 
                                    ,ISNULL(Line.UserName,'') Line
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    , CASE WHEN MONTH(DOS) =  MONTH('" + effectiveDate + @"')  AND YEAR(DOS) = YEAR('" + effectiveDate + @"') then 'Separated' else 'Regular' end CurrentMonthEmployeeStatus
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
                                    
                                    
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(e.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName

                                    FROM EmployeeInformation e
                                   
									LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    

                                    LEFT OUTER JOIN (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									,dg.UserName GivenDesignationGroup--,srm.SalaryRuleName
									FROM mst.DesignationMaster dm
									LEFT OUTER JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
		                          
									) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
									AND egdsgg.EmployeeCategoryId=e.EmployeeCategorySystemID
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId
                                    LEFT JOIN [ORG].[Line] ON Line.Id = mpb.LineId

                                    
						LEFT JOIN [HKP].[LegalDesignation] as Ld on Ld.Id=E.LegalDesignationId
			LEFT JOIN [MST].DesignationMasterLegalDesignation LDM ON LDM.LegalDesignationId=E.LegalDesignationId
			LEFT JOIN [MST].DesignationMaster DesM ON DesM.Id = LDM.DesignationMasterId
            LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
									Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                    " + salaryProcessJoin + @"
                                    Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
									left join [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									left join [HKP].[Bank] bb on bb.Id = ebi.BankSystemID

                                     WHERE " + param + @" " + strDOJ + @"
                                            " + wcPayrollGroup + @"  " + wcSalaryProcess + @"  
                                                    
                                        AND
									(E.DOS IS NULL OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + effectiveDate + @"')
                                    AND e.DOJ <= '" + Ld.ToString("dd-MMM-yyyy") + @"'
                                    ) 
                                     ) DD " + wcEmpStatus + @" ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";


            var jsondata = Json(_sqlRepository.GetDataCollection(cmdText), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }


       [HttpGet, Authorize]
        public JsonResult GetPlantList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var str = @"select Id PlantId,UserName PlantName  from ORG.PLANT where CompanyId='" + identity.CompanyId + "'";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        #endregion -- Operations  
    }

    
}