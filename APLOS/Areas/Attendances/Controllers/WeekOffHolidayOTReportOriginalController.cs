using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.HumanResource.Report.OT;
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
using Library.HumanResource.Report.OT;
namespace Aplos.Areas.Attendances.Controllers
{
    public class WeekOffHolidayOTReportOriginalController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMonthlyAttendanceInformation _monthlyAttendanceInformation;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private DataSet dsRef;
        private object workbook;
        private object objRpt;
        private object excelEngine;
        private object application;

        public WeekOffHolidayOTReportOriginalController(
              IMaternityLeavePolicyService LeavePolicyService,
            ISqlRepository sqlRepository,
            IMonthlyAttendanceInformation monthlyAttendanceInformation
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _sqlRepository = sqlRepository;
            _monthlyAttendanceInformation = monthlyAttendanceInformation;
        }

        #endregion Constructor

        #region -- Pages
        
        public ActionResult Aplos()
        {
            return View();
        }
        public ActionResult HolidayOT()
        {
            return View();
        }



        #endregion -- Pages


        #region -- Operations

        //Addition By Sayanto
        [HttpPost, Authorize]
        public ActionResult GetEmpInfo(string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity, string PlantId)
        {
           
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if(PlantId == "" || PlantId == null)
            {
                PlantId = identity.PlantId;
            }
            string Plant = "'" + PlantId.Replace(",", "','") + "'";//replaced with ""
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
									,ISNULL(Plant.UserName,'') Plant ,ISNULL(Plant.Id,'') PlantId 
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
        /// 
       



        [HttpPost, Authorize]
        public ActionResult GetMonthWiseWeekExtraOTReport(string Month, string Year, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity , string PlantId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
               
                if (PlantId == "" || PlantId == null)
                {
                    PlantId = identity.PlantId;
                }
                string Plant = "'" + PlantId.Replace(",", "','") + "'";//replaced with ""

                WeekOFFandHolidayOTOriginal clsWeekOFFOTReport = new WeekOFFandHolidayOTOriginal();
                var fileName = "WeekOFFOT" + DateTime.Now.ToString("yyMMdd") + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;

                //GetWeekOFFExtraOT(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName, string Month, string Year, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
                var workbook = clsWeekOFFOTReport.GetWeekOFFExtraOTCon(identity.Name, identity.CompanyGroupId, identity.CompanyId, Plant, Month, Year,  parameters,  isActive,  isSeperated,  isMaternity);

                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);


            }

            catch (Exception ex)
            {
                throw ex;
            }

        }


        [HttpPost, Authorize]
        public ActionResult GetMonthWiseHolidayExtraOTReport(string Month, string Year, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                WeekOFFandHolidayOTOriginal clsWeekOFFOTReport = new WeekOFFandHolidayOTOriginal();
                var fileName = "HolidayOT" + DateTime.Now.ToString("yyMMdd") + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;

                //GetWeekOFFExtraOT(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName, string Month, string Year, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
                var workbook = clsWeekOFFOTReport.GetholidayExtraOT(identity.Name, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, Month, Year, parameters, isActive, isSeperated, isMaternity);

                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);


            }

            catch (Exception ex)
            {
                throw ex;
            }

        }



        [HttpPost, Authorize]
        public ActionResult GetEmpInfoSalaryPorcessed(string effectiveDate, string month ,string year, bool isActive, bool isSeperated, bool isMaternity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            WeekOFFandHolidayOTOriginal clsWeekOFFOTReport = new WeekOFFandHolidayOTOriginal();
            var jsondata = Json(clsWeekOFFOTReport.GetEmpInfoDateRange(identity.CompanyGroupId, identity.PlantId, month, year,  identity.IsSysAdmin, identity.IsControlAdmin, identity.UserId, isActive, isSeperated, isMaternity), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpGet, Authorize]
        public ActionResult getPlants()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var str = "Select Id as Value, UserName as Text from org.Plant where CompanyId ='"+identity.CompanyId+"'";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT Id,YearNo FROM YearlyCalendar WHERE PlantId='" + identity.PlantId + "'  ORDER BY YearNo DESC ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);

        }
        class OTReport
        {
            public decimal TotalOTHr { get; set; }
            public string EmployeeCode { get; set; }
            public DateTime workdate { get; set; }

        }

       
        


        #endregion -- Operations  
    }

  
}