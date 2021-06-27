using Library.Model.Employees;
using Library.Data;
using Library.Service.Employees;

using System;
using System.Web.Mvc;
using System.Linq;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using OTSBD;
using System.Data;
using System.Collections.Generic;
using Newtonsoft.Json;
using LV;

namespace Aplos.Areas.Payrolls.Controllers
{

    public class LeaveEncashmentEntryController : BaseController
    {
        #region Constructor
        /// <summary>   The separationTypeService service. </summary>

        private readonly ISqlRepository _sqlRepository;
        public LeaveEncashmentEntryController(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        #endregion

        #region Aplos
        public ActionResult Aplos()
        {
            return View();
        }
        public ActionResult MultipleLeaveEncashment()
        {
            return View();
        }

        public ActionResult WithinYearLeaveEncashment()
        {
            return View();
        }
        public ActionResult SpecificDateLeaveEncashment()
        {
            return View();
        }
        #endregion

        #region -- Operations


        [HttpGet, Authorize]
        public ActionResult LoadEmployeelist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = string.Empty;
            try
            {
                sql = @"SELECT  EI.SystemId
                         ,EI.EmployeeCode
                         ,EI.EmployeeName
                         , FORMAT(EI.DOB,'dd-MMM-yyyy') DOB
                         , FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                         , FORMAT(EI.DOS,'dd-MMM-yyyy') DOS
                         , DG.UserName LegalDesignation
                         , DP.UserName Department
                         , PMB.Code,PR.UserName PositionName
                         , E.UserName EntityName
                        
                         FROM dbo.Employeeinformation EI
                         LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id							 
                         LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id							 
                         LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                         LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                         LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                         LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id                       
                         LEFT JOIN HKP.LegalDesignation  DG on DG.Id=EI.LegalDesignationId
                         LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId				
                         WHERE --EI.SystemId IN (SELECT EmployeeId FROM TRN.Resignation WHERE ApprovalStatus='Approved' ) AND EI.SystemId NOT IN (SELECT EmpSystemId FROM EmployeeFinalSettlement ) AND
                         EI.PlantId='" + identity.PlantId + @"' ORDER BY CONVERT(INT, ei.EmployeeCode) ";

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
        public ActionResult LoadYearlyCalendar()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = string.Empty;
            try
            {
                sql = @"select * from YearlyCalendar where  PlantId='" + identity.PlantId + @"'";

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
        public ActionResult LoadLeaveEncashmentTypes()
        {
          
            string sql = string.Empty;
            try
            {
                sql = @"SELECT * FROM LeaveEncashmentTypes";
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
        public ActionResult GetLeaveEncashmentData(string EmpSystemId, string YearNo,string EffectiveDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsLeaveEncashment ob = new clsLeaveEncashment();
            LeaveEncashmentViewModel data = ob.GetLeaveEncashmentData(EmpSystemId, EffectiveDate, YearNo, identity.PlantId);   
            return Json(new {LeaveInfo=data}, JsonRequestBehavior.AllowGet);
        }





        [HttpGet, Authorize]
        public ActionResult GetMultipleLeaveEncashmentData( string YearNo, string EffectiveDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsLeaveEncashment ob = new clsLeaveEncashment();
            List< MultipleLeaveEncashmentViewModel> data = ob.GetMultipleLeaveEncashmentData( EffectiveDate, YearNo, identity.PlantId);
            List<MultipleLeaveEncashmentViewModel> dataNew = data.Where(x=>x.Days>0).OrderBy(x => x.EmployeeCode).ToList();
            //return Json(new { LeaveInfo = data }, JsonRequestBehavior.AllowGet);


            JsonResult json = Json(new { LeaveInfo = data }, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet]
        public ActionResult GetSevedMultipleLeaveEncashmentData(string YearId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = string.Empty;
            try
            {
                sql = @"SELECT  [CheckBoxSelect] = Convert(bit, 'False'), 
                             E.SystemId, e.EmployeeCode,e.EmployeeName,t.UserName LeaveType ,FORMAT(e.DOJ,'dd-MMM-yyyy') DOJ
                             , EC.UserName EmpCategoryName  
                            ,ld.UserName Designation
                            ,U.UserName Unit 
                            ,Dv.UserName Division
                            ,Dp.UserName Department
                            ,Se.UserName Section 
                            ,SB.UserName SubSection 
                            ,L.UserName Line
                            ,BroughtForward=isnull(s.BroughtForward,0)+isnull(s.CarryForwardOpeningBalance,0)
                            ,s.DaysCanBeSanctioned
                            ,s.CurrentYearAllocation
                            ,s.IsYearlyProcessed
                            ,LeaveDaysAllowed=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)+isnull(s.CarryForwardOpeningBalance,0)
                            ,isnull(kk.LeaveDuration,0) AvailedLeave
                            --,Balance=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)
                               ---------------Is Brought Forward Add to balance -----------------------------------------------------------                                       
							,Balance=CASE WHEN t.LeaveType='Earn' THEN  
								CASE WHEN
								-----------------------------------DOJorDOC start -----------------------------------------------------------
															CASE WHEN ltd.LvAvailedOnDOJ=1 THEN                            										 
                            																	 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  e.DOJ ) END
																	   WHEN  ltd.LvAvailedOnDOC=1 THEN 										   
										   														 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	e.DOC  )
										   													END
																   END
							---------------------------------------DOJorDOC start  end-------------------------------------------------------
	
								> GETDATE() then 
									  isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0)------No
									ELSE  isnull(s.BroughtForward,0)+isnull(s.CarryForwardOpeningBalance,0)+isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0) END---Yes
							ELSE  isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0) END  ---No

                            ,av.LeaveEncashmentDayNo Days,ltd.PolicyName
                            from TRN.EmployeeLeaveSummary S 
                            INNER JOIN LeaveType t on s.LeaveTypeId=t.Id AND t.LeaveType='Earn'
                            INNER JOIN EmployeeInformation e on e.SystemId=s.EmployeeId
                            LEFT JOIN (
									select 
									tt.UserName LeaveType,t.EmpSystemID,t.LTSystemID, sum(isnull(d.LeaveDuration,0)) LeaveDuration
									from 
									LeaveTransaction t 
									left join 
							          (--detail
                                    select SUM(LeaveDuration) LeaveDuration, LvTrnsSystemID from LeaveTransactionDetails 
                                    where IsAvailed=1
                                    and WorkDate between
                                    (select FromDate from YearlyCalendar where Id=" + YearId + @" and PlantId='" + identity.PlantId + @"')
                                    and (select ToDate from YearlyCalendar where Id= " + YearId + @" and PlantId='" + identity.PlantId + @"')
                                    group by LvTrnsSystemID
                                    )--detail 
                                    d on t.SystemID=d.LvTrnsSystemID

									left join LeaveType tt on tt.id=t.LTSystemID
									where t.IsApproved=1  
									group by tt.UserName ,t.EmpSystemID,t.LTSystemID
                            ) kk on kk.LTSystemID=s.LeaveTypeId and kk.EmpSystemID=s.EmployeeId
                             left outer join (
                            	--***********LV**********************
                            	
								SELECT DC.LeavePolicyMasterId,lpm.PolicyName ,e.SystemId EmpId,d.*
																				FROM 
																				EmployeeInformation e
																				LEFT join MST.DesignationMaster DM ON e.GivenDesignationId=dm.DesignationId
																				LEFT JOIN SCS.DesignationMasterConfiguration DC 
																							ON DM.Id=DC.DesignationMasterId AND dc.plantid=e.plantid
																				LEFT JOIN LeavePolicyDetail d ON d.LPMSystemID=dc.LeavePolicyMasterId
																				LEFT JOIN LeavePolicyMaster AS lpm  ON lpm.SystemID=dc.LeavePolicyMasterId	
																where dc.plantid='" + identity.PlantId + @"'
											--*******************LV***********************
							) ltd on ltd.LTSystemID = t.Id AND ltd.EmpId=e.SystemId
                          
                             LEFT JOIN 
                            (
                            	SELECT EmpSystemId,YearlyCalendarId ,SUM(Days) LeaveEncashmentDayNo FROM LeaveEncashmentTransaction WHERE  YearlyCalendarId='" + YearId + @"' GROUP BY EmpSystemId,YearlyCalendarId
                            ) as 
                            av ON av.EmpSystemId=s.EmployeeId
                            LEFT JOIN HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
                            LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                            LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                            LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id
                            LEFT JOIN ORG.Section Se ON E.SectionID = Se.Id
                            LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                            LEFT JOIN ORG.Line L ON E.LineID = L.Id
                            LEFT JOIN HKP.Designation D ON E.DesignationSystemID = D.Id
                            LEFT JOIN HKP.LegalDesignation AS ld  ON E.LegalDesignationId = ld.Id
							LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
							LEFT JOIN MST.ManpowerBudget mb ON mb.Id=e.BudgetCode
                            where s.CalanderYearId=(select id from YearlyCalendar where Id=" + YearId + @" and PlantId='" + identity.PlantId + @"') 
                            AND s.EmployeeId IN (SELECT SystemId FROM EmployeeInformation WHERE EmployeeStatus='Active' and PlantId='" + identity.PlantId + @"' )
                            AND s.EmployeeId  IN (SELECT EmpSystemId  FROM LeaveEncashmentTransaction WHERE YearlyCalendarId='" + YearId + @"' AND LeaveEncashmentType='Year End Leave Encashment' and PlantId='" + identity.PlantId + @"' )
                            ORDER BY  e.EmployeeCodePreFix,e.EmployeeCodeNumeric";

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
        [HttpPost]
        public JsonResult SaveMultipleLeaveEncashment(List<MultipleLeaveEncashmentViewModelNew> leaveEncashment,string YearlyCalendarId,string EncashmentDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ConnectionManager.DAL.ConManager objCon;
            DataSet dsleaveEncashment;
            try
            {
                string sql = @"SELECT * FROM LeaveEncashmentTransaction 
                                            WHERE 
                                            ---PlantId='" + identity.PlantId + @"' AND
                                            EmpSystemId='" + leaveEncashment + @"' AND EncashmentDate='" + Convert.ToDateTime(EncashmentDate).ToString("dd-MMM-yyyy") + @"' 
                                            AND LeaveEncashmentType='Year End Leave Encashment' AND YearlyCalendarId='" + YearlyCalendarId + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsleaveEncashment, false, "1");

                DataView dvleaveEncashment = new DataView(dsleaveEncashment.Tables[0]);

                for (int i = 0; i < leaveEncashment.Count; i++)
                {
                    dvleaveEncashment.RowFilter = "EmpSystemId='" + leaveEncashment[i].EmpSystemId + @"' AND YearlyCalendarId='"+ YearlyCalendarId + "'  AND EncashmentDate='" + Convert.ToDateTime(EncashmentDate).ToString("dd-MMM-yyyy") + @"'  AND LeaveEncashmentType='Year End Leave Encashment'";

                    if (dvleaveEncashment.Count == 0)
                    {
                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "MultipleaveEncashment", out sID);
                        DataRow dr = dsleaveEncashment.Tables[0].NewRow();
                        dr["Id"] = "LE" + sID;
                        dr["PlantId"] = identity.PlantId.ToString();
                        dr["EmpSystemId"] = leaveEncashment[i].EmpSystemId;
                        dr["LeaveTypeSystemId"] = leaveEncashment[i].LeaveTypeId;
                        dr["LeaveEncashmentType"] = "Year End Leave Encashment";
                        dr["EncashmentDate"] = EncashmentDate;  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                        dr["Days"] = leaveEncashment[i].Days;
                        //dr["Rest"] = leaveEncashment.Balance-(leaveEncashment.AvailedEncashment+ leaveEncashment.Days);
                        dr["Rate"] = leaveEncashment[i].Rate;
                        dr["PaymentMode"] = leaveEncashment[i].PaymentMode;
                        dr["BasicAmmount"] = leaveEncashment[i].BasicAmmount;
                        dr["GrossAmmount"] = leaveEncashment[i].GrossAmmount;
                        //dr["GivenDesignationId"] = leaveEncashment[i].GivenDesignationId;
                        dr["BankSystemID"] = leaveEncashment[i].BankSystemID;
                        dr["BankBranchId"] = leaveEncashment[i].BankBranchId;
                        dr["BankAccNo"] = leaveEncashment[i].BankAccNo;
                        //dr["SalaryPercentage"] = leaveEncashment[i].SalaryPercentage;
                        //dr["BudgetCode"] = leaveEncashment[i].BudgetCode;
                        //dr["EmployeeCategoryId"] = leaveEncashment[i].EmployeeCategoryId;
                        dr["LegalDesignationId"] = leaveEncashment[i].LegalDesignationId;

                        dr["BroughtForward"] = leaveEncashment[i].BroughtForward;
                        dr["DaysCanBeSanctioned"] = leaveEncashment[i].DaysCanBeSanctioned;
                        dr["AvailedLeave"] = leaveEncashment[i].AvailedLeave;
                        dr["CarryForward"] = leaveEncashment[i].CarryForward;
                        //dr["Balance"] = leaveEncashment[i].Balance - leaveEncashment[i].NewYearEndEncash;

                        dr["Isdisburse"] = true;
                        dr["YearlyCalendarId"] = YearlyCalendarId;
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dsleaveEncashment.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        DataRow dr = dvleaveEncashment[0].Row;
                        dr.BeginEdit();
                        dr["PlantId"] = identity.PlantId.ToString();
                        dr["EmpSystemId"] = leaveEncashment[i].EmpSystemId;
                        dr["LeaveTypeSystemId"] = leaveEncashment[i].LeaveTypeId;
                        dr["LeaveEncashmentType"] = "Year End Leave Encashment";
                        dr["EncashmentDate"] = leaveEncashment[i].EncashmentDate;  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                        dr["Days"] = leaveEncashment[i].Days;
                        //dr["Rest"] = leaveEncashment.Balance - (leaveEncashment.AvailedEncashment + leaveEncashment.Days);
                        dr["Rate"] = leaveEncashment[i].Rate;
                        dr["PaymentMode"] = leaveEncashment[i].PaymentMode;
                        dr["BasicAmmount"] = leaveEncashment[i].BasicAmmount;
                        dr["GrossAmmount"] = leaveEncashment[i].GrossAmmount;
                        //dr["GivenDesignationId"] = leaveEncashment[i].GivenDesignationId;
                        dr["BankSystemID"] = leaveEncashment[i].BankSystemID;
                        dr["BankBranchId"] = leaveEncashment[i].BankBranchId;
                        dr["BankAccNo"] = leaveEncashment[i].BankAccNo;
                        //dr["SalaryPercentage"] = leaveEncashment[i].SalaryPercentage;
                        //dr["BudgetCode"] = leaveEncashment[i].BudgetCode;
                        //dr["EmployeeCategoryId"] = leaveEncashment[i].EmployeeCategoryId;
                        dr["LegalDesignationId"] = leaveEncashment[i].LegalDesignationId;

                        dr["BroughtForward"] = leaveEncashment[i].BroughtForward;
                        dr["DaysCanBeSanctioned"] = leaveEncashment[i].DaysCanBeSanctioned;
                        dr["AvailedLeave"] = leaveEncashment[i].AvailedLeave;
                        dr["CarryForward"] = leaveEncashment[i].CarryForward;
                        //dr["Balance"] = leaveEncashment[i].Balance - leaveEncashment[i].NewYearEndEncash;

                        dr["Isdisburse"] = true;
                        dr["YearlyCalendarId"] = YearlyCalendarId;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();

                    }
                    dvleaveEncashment.RowFilter = null;
                }
              



                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsleaveEncashment);
            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult SaveLeaveEncashment(LeaveEncashmentViewModel leaveEncashment)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ConnectionManager.DAL.ConManager objCon;
            DataSet dsleaveEncashment;
            try
            {
                string sql = @"SELECT * FROM LeaveEncashmentTransaction 
                                            WHERE ---PlantId='" + identity.PlantId + @"' AND
                                            EmpSystemId='" + leaveEncashment.EmpSystemId + @"' AND EncashmentDate='" + Convert.ToDateTime(leaveEncashment.EncashmentDate).ToString("dd-MMM-yyyy") + @"' 
                                            AND LeaveEncashmentType='"+leaveEncashment.LeaveEncashmentType+ @"' AND YearlyCalendarId='"+ leaveEncashment.YearlyCalendarId + @"'"; 

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsleaveEncashment, false, "1");



                DataView dvleaveEncashment = new DataView(dsleaveEncashment.Tables[0]);
                dvleaveEncashment.RowFilter = "EmpSystemId='" + leaveEncashment.EmpSystemId + @"' AND Id=''";

                if (dvleaveEncashment.Count == 0)
                {
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "leaveEncashment", out sID);
                    DataRow dr = dsleaveEncashment.Tables[0].NewRow();
                    dr["Id"] = "LE" + sID;
                    dr["PlantId"] = identity.PlantId.ToString();
                    dr["EmpSystemId"] = leaveEncashment.EmpSystemId;                    
                    dr["LeaveEncashmentType"] = leaveEncashment.LeaveEncashmentType;
                    dr["EncashmentDate"] = leaveEncashment.EncashmentDate;  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                    dr["Days"] = leaveEncashment.Days;
                    //dr["Rest"] = leaveEncashment.Balance-(leaveEncashment.AvailedEncashment+ leaveEncashment.Days);
                    dr["Rate"] = leaveEncashment.Rate;
                    dr["Isdisburse"] = leaveEncashment.Isdisburse;
                    dr["YearlyCalendarId"] = leaveEncashment.YearlyCalendarId;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsleaveEncashment.Tables[0].Rows.Add(dr);

                }
                else
                {
                    DataRow dr = dvleaveEncashment[0].Row;
                    dr.BeginEdit();
                    dr["PlantId"] = identity.PlantId.ToString();
                    dr["EmpSystemId"] = leaveEncashment.EmpSystemId;
                    dr["LeaveEncashmentType"] = leaveEncashment.LeaveEncashmentType;
                    dr["EncashmentDate"] = leaveEncashment.EncashmentDate;  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                    dr["Days"] = leaveEncashment.Days;
                    //dr["Rest"] = leaveEncashment.Balance - (leaveEncashment.AvailedEncashment + leaveEncashment.Days);
                    dr["Rate"] = leaveEncashment.Rate;
                    dr["Isdisburse"] = leaveEncashment.Isdisburse;
                    dr["YearlyCalendarId"] = leaveEncashment.YearlyCalendarId;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();

                }
                dvleaveEncashment.RowFilter = null;



                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsleaveEncashment);
            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Success });
        }
        [HttpPost]
        public JsonResult DeleteLeaveEncashment(string leaveEncashmentId,string EmpSystemId, string EncashmentDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ConnectionManager.DAL.ConManager objCon;
            DataSet dsleaveEncashment;
            try
            {
                string sql = @"Delete FROM LeaveEncashmentTransaction 
                                            WHERE ---PlantId='" + identity.PlantId + @"' AND
                                            EmpSystemId='" + EmpSystemId + @"' AND EncashmentDate='" + Convert.ToDateTime(EncashmentDate).ToString("dd-MMM-yyyy") + @"' 
                                            AND Id='" + leaveEncashmentId + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsleaveEncashment, false, "1");



              
            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult GetLeaveEncashmentlist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT EI.EmployeeCode,EI.EmployeeName
                            ,FORMAT(LET.EncashmentDate,'dd-MMM-yyyy') EncashmentDate
                            ,LET.LeaveEncashmentType, LET.YearlyCalendarId
                            ,LET.LeaveEncashmentType, LET.EmpSystemId, LET.Days, LET.Isdisburse, LET.Rate,LET.Id
                            FROM LeaveEncashmentTransaction AS LET 
                            INNER JOIN EmployeeInformation AS ei ON ei.SystemId = LET.EmpSystemId
                            WHERE ei.PlantId='" + identity.PlantId + @"'
                            ORDER BY  CONVERT(DATETIME,LET.EncashmentDate) DESC";

            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }









        #endregion

        #region with-in-year-leave-encashment

        [HttpGet, Authorize]
        public ActionResult GetWithInYearLeaveEncashmentData(string FromDate)
        {
            string YearNo = string.Empty;
            DataSet dsYearCalander = null;
          
            ConnectionManager.DAL.ConManager objCon;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsLeaveEncashment ob = new clsLeaveEncashment();
            List<MultipleLeaveEncashmentViewModel> data = ob.GetWithInYearLeaveEncashmentData(FromDate,  identity.PlantId);
            List<MultipleLeaveEncashmentViewModel> dataNew = data.Where(x => x.Days > 0).OrderBy(x => x.EmployeeCode).ToList();
            if (data.Count>0)
            {
                string sql1 = @"SELECT YearNo FROM YearlyCalendar WHERE id='" + data[0].YearlyCalendarId + @"' AND PlantId='" + identity.PlantId + @"'"; 
                                           

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql1, out dsYearCalander, false, "1");

                if (dsYearCalander.Tables[0].Rows.Count > 0)
                {
                    YearNo = dsYearCalander.Tables[0].Rows[0]["YearNo"].ToString();
                }
            }

            
            //return Json(new { LeaveInfo = data }, JsonRequestBehavior.AllowGet);


            JsonResult json = Json(new { LeaveInfo = data, YearNo }, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }
        [HttpPost]
        public JsonResult SaveWithInYearLeaveEncashment(List<MultipleLeaveEncashmentViewModelNew> leaveEncashment,  string EncashmentDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string YearlyCalendarId = string.Empty;
            string YearlyCalendarYearNo = string.Empty;
            DataSet  dsYearCalander = null;
            DataSet dsOldSummary = null;
            DataView dvSaveSummaryOld = null;
            string empids = string.Empty;

            DataSet dsleaveEncashment;
            ConnectionManager.DAL.ConManager objCon;
            try
            {



              





                string sql1 = @"select * from YearlyCalendar where '" + Convert.ToDateTime(EncashmentDate).ToString("dd-MMM-yyyy") + @"' BETWEEN FromDate AND ToDate  
                                            AND PlantId='" + identity.PlantId + @"' ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql1, out dsYearCalander, false, "1");

                if (dsYearCalander.Tables[0].Rows.Count > 0)
                {
                    YearlyCalendarId = dsYearCalander.Tables[0].Rows[0]["Id"].ToString();
                    YearlyCalendarYearNo = dsYearCalander.Tables[0].Rows[0]["YearNo"].ToString();
                }


                string sqlsummary = @"SELECT * FROM TRN.EmployeeLeaveSummary WHERE  CalanderYearId='" + YearlyCalendarId + @"' AND LeaveTypeId IN (SELECT Id FROM  LeaveType WHERE LeaveType='Earn' )  
                                            AND PlantId='" + identity.PlantId + @"' ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlsummary, out dsOldSummary, false, "1");
                dvSaveSummaryOld = new DataView(dsOldSummary.Tables[0]);




                string sql = @"SELECT * FROM LeaveEncashmentTransaction 
                                            WHERE ---PlantId='" + identity.PlantId + @"' AND
                                            EmpSystemId='" + leaveEncashment + @"' AND EncashmentDate='" + Convert.ToDateTime(EncashmentDate).ToString("dd-MMM-yyyy") + @"' 
                                            AND LeaveEncashmentType='Encashment Within Year' AND YearlyCalendarId='" + YearlyCalendarId + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsleaveEncashment, false, "1");


               

                DataView dvleaveEncashment = new DataView(dsleaveEncashment.Tables[0]);

                for (int i = 0; i < leaveEncashment.Count; i++)
                {
                    dvleaveEncashment.RowFilter = "EmpSystemId='" + leaveEncashment[i].EmpSystemId + @"' AND YearlyCalendarId='" + YearlyCalendarId + "'  AND EncashmentDate='" + Convert.ToDateTime(EncashmentDate).ToString("dd-MMM-yyyy") + @"'  AND LeaveEncashmentType='Encashment Within Year'";

                    if (dvleaveEncashment.Count == 0)
                    {
                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "MultipleaveEncashment", out sID);
                        DataRow dr = dsleaveEncashment.Tables[0].NewRow();
                        dr["Id"] = "LE" + sID;
                        dr["PlantId"] = identity.PlantId.ToString();
                        dr["EmpSystemId"] = leaveEncashment[i].EmpSystemId;
                        dr["LeaveEncashmentType"] = "Encashment Within Year";
                        dr["EncashmentDate"] = EncashmentDate;  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                        dr["Days"] = leaveEncashment[i].Days;
                        dr["LeaveTypeSystemId"] = leaveEncashment[i].LeaveTypeId;
                        //dr["Rest"] = leaveEncashment.Balance-(leaveEncashment.AvailedEncashment+ leaveEncashment.Days);
                        dr["Rate"] = leaveEncashment[i].Rate;
                        dr["PaymentMode"] = leaveEncashment[i].PaymentMode;
                        if (!string.IsNullOrEmpty(leaveEncashment[i].BasicAmmount))
                        {
                            dr["BasicAmmount"] = leaveEncashment[i].BasicAmmount;
                        }
                        else
                        {
                            dr["BasicAmmount"] = 0;
                        }

                        if (!string.IsNullOrEmpty(leaveEncashment[i].GrossAmmount))
                        {
                            dr["GrossAmmount"] = leaveEncashment[i].GrossAmmount;
                        }
                        else
                        {
                            dr["GrossAmmount"] = 0;
                        }
                        
                        //dr["GivenDesignationId"] = leaveEncashment[i].GivenDesignationId;
                        dr["BankSystemID"] = leaveEncashment[i].BankSystemID;
                        dr["BankBranchId"] = leaveEncashment[i].BankBranchId;
                        dr["BankAccNo"] = leaveEncashment[i].BankAccNo;
                        //dr["SalaryPercentage"] = leaveEncashment[i].SalaryPercentage;
                        //dr["BudgetCode"] = leaveEncashment[i].BudgetCode;
                        //dr["EmployeeCategoryId"] = leaveEncashment[i].EmployeeCategoryId;
                        dr["LegalDesignationId"] = leaveEncashment[i].LegalDesignationId;

                        dr["BroughtForward"] = leaveEncashment[i].BroughtForward;
                        dr["DaysCanBeSanctioned"] = leaveEncashment[i].DaysCanBeSanctioned;
                        dr["AvailedLeave"] = leaveEncashment[i].AvailedLeave;
                        dr["CarryForward"] = leaveEncashment[i].NewBroughtForward;
                        //dr["Balance"] = leaveEncashment[i].Balance - leaveEncashment[i].NewYearEndEncash;

                        dr["Isdisburse"] = true;
                        dr["YearlyCalendarId"] = YearlyCalendarId;
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dsleaveEncashment.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        DataRow dr = dvleaveEncashment[0].Row;
                        dr.BeginEdit();
                        dr["PlantId"] = identity.PlantId.ToString();
                        dr["EmpSystemId"] = leaveEncashment[i].EmpSystemId;
                        dr["LeaveEncashmentType"] = "Encashment Within Year";
                        dr["EncashmentDate"] = leaveEncashment[i].EncashmentDate;  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                        dr["Days"] = leaveEncashment[i].Days;
                        //dr["Rest"] = leaveEncashment.Balance - (leaveEncashment.AvailedEncashment + leaveEncashment.Days);
                        dr["Rate"] = leaveEncashment[i].Rate;
                        dr["PaymentMode"] = leaveEncashment[i].PaymentMode;
                        //dr["BasicAmmount"] = leaveEncashment[i].BasicAmmount;
                        //dr["GrossAmmount"] = leaveEncashment[i].GrossAmmount;
                        if (!string.IsNullOrEmpty(leaveEncashment[i].BasicAmmount))
                        {
                            dr["BasicAmmount"] = leaveEncashment[i].BasicAmmount;
                        }
                        else
                        {
                            dr["BasicAmmount"] = 0;
                        }

                        if (!string.IsNullOrEmpty(leaveEncashment[i].GrossAmmount))
                        {
                            dr["GrossAmmount"] = leaveEncashment[i].GrossAmmount;
                        }
                        else
                        {
                            dr["GrossAmmount"] = 0;
                        }
                        //dr["GivenDesignationId"] = leaveEncashment[i].GivenDesignationId;
                        dr["BankSystemID"] = leaveEncashment[i].BankSystemID;
                        dr["BankBranchId"] = leaveEncashment[i].BankBranchId;
                        dr["BankAccNo"] = leaveEncashment[i].BankAccNo;
                        //dr["SalaryPercentage"] = leaveEncashment[i].SalaryPercentage;
                        //dr["BudgetCode"] = leaveEncashment[i].BudgetCode;
                        //dr["EmployeeCategoryId"] = leaveEncashment[i].EmployeeCategoryId;
                        dr["LegalDesignationId"] = leaveEncashment[i].LegalDesignationId;


                        dr["BroughtForward"] = leaveEncashment[i].BroughtForward;
                        dr["DaysCanBeSanctioned"] = leaveEncashment[i].DaysCanBeSanctioned;
                        dr["AvailedLeave"] = leaveEncashment[i].AvailedLeave;
                        dr["CarryForward"] = leaveEncashment[i].NewBroughtForward;
                        //dr["Balance"] = leaveEncashment[i].Balance - leaveEncashment[i].NewYearEndEncash;

                        dr["Isdisburse"] = true;
                        dr["YearlyCalendarId"] = YearlyCalendarId;
                        dr["LeaveTypeSystemId"] = leaveEncashment[i].LeaveTypeId;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();

                    }
                    dvleaveEncashment.RowFilter = null;

                   
                    //Old year insert or update
                    
                    dvSaveSummaryOld.RowFilter = "EmployeeId='" + leaveEncashment[i].EmpSystemId + "' and LeaveTypeId='" + leaveEncashment[i].LeaveTypeId + "' and CalanderYearId='" + YearlyCalendarId + "' AND IsEncashed=0";
                    if (dvSaveSummaryOld.Count > 0)
                    {

                    
                        DataRow  drSaveSummaryOld = dvSaveSummaryOld[0].Row;
                        drSaveSummaryOld.BeginEdit();
                        drSaveSummaryOld["CarryForward"] = leaveEncashment[i].NewBroughtForward;
                        drSaveSummaryOld["YearEndEncash"] = leaveEncashment[i].NewYearEndEncash;
                        drSaveSummaryOld["YearEndLapse"] = leaveEncashment[i].NewYearEndLapse; 
                        //drSaveSummaryOld["BroughtForward"] = leaveEncashment[i].NewBroughtForward;
                        drSaveSummaryOld["EncashedInbetween"] = leaveEncashment[i].NewYearEndEncash;
                        drSaveSummaryOld["IsEncashed"] = true;
                        drSaveSummaryOld["UpdatedFromIP"] = "::1";
                        drSaveSummaryOld["UpdatedDate"] = System.DateTime.Now;
                        drSaveSummaryOld["UpdatedBy"] = "Schedule";
                        drSaveSummaryOld.EndEdit();
                    }

                    dvSaveSummaryOld.RowFilter = null;

                    if (empids == "")
                        empids = "'" + leaveEncashment[i].EmployeeCode.ToString() + "'";
                    else
                        empids = empids + ",'" + leaveEncashment[i].EmployeeCode.ToString() + "'";


                }




                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsleaveEncashment, dsOldSummary);


                GenericLeaveProcess ob = new GenericLeaveProcess();
                ob.ExecuteProcess(identity.PlantId, YearlyCalendarYearNo, empids);


            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Success });
        }


        [HttpGet]
        public ActionResult GetSevedWithInYearLeaveEncashmentData(string ToDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(ToDate))
            {
                ToDate = DateTime.Now.ToString("dd-MMM-yyyy");
            }   
            string YearlyCalendarId = string.Empty;
            DataSet dsYearCalander = null;

            DataSet dsleaveEncashment;
            ConnectionManager.DAL.ConManager objCon;

            string sql1 = @"select * from YearlyCalendar where '" + Convert.ToDateTime(ToDate).ToString("dd-MMM-yyyy") + @"' BETWEEN FromDate AND ToDate  
                                            AND PlantId='" + identity.PlantId + @"' ";

            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(sql1, out dsYearCalander, false, "1");

            if (dsYearCalander.Tables[0].Rows.Count > 0)
            {
                YearlyCalendarId = dsYearCalander.Tables[0].Rows[0]["Id"].ToString();
            }


            string sql = string.Empty;
            try
            {
                sql = @"SELECT  [CheckBoxSelect] = Convert(bit, 'False'), 
                             E.SystemId, e.EmployeeCode,e.EmployeeName,t.UserName LeaveType ,FORMAT(e.DOJ,'dd-MMM-yyyy') DOJ
                            , EC.UserName EmpCategoryName  
                            ,ld.UserName Designation
                            ,U.UserName Unit 
                            ,Dv.UserName Division
                            ,Dp.UserName Department
                            ,Se.UserName Section 
                            ,SB.UserName SubSection 
                            ,L.UserName Line
                            
                            ,S.BroughtForward
                            ,s.DaysCanBeSanctioned
                            
                          
							,Balance=ISNULL(s.BroughtForward,0)+ISNULL(s.DaysCanBeSanctioned,0)-ISNULL(s.AvailedLeave,0)-ISNULL(s.Days,0)-ISNULL(s.YearEndLapse,0)
                            ,S.Days
                            ,FORMAT(DATEFROMPARTS( YEAR(GETDATE()),CONVERT(INT,ltd.EncashmentSpecificMonth),CONVERT(INT,ltd.EncashmentSpecificDay)),'dd-MMM-yyyy') EncashmentSpecificDate
                            ,ltd.PolicyName
                            from LeaveEncashmentTransaction S 
                            INNER JOIN LeaveType t on s.LeaveTypeSystemId=t.Id AND t.LeaveType='Earn'
                            INNER JOIN EmployeeInformation e on e.SystemId=s.empsystemId
                          
                            LEFT JOIN HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
                            LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                            LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                            LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id
                            LEFT JOIN ORG.Section Se ON E.SectionID = Se.Id
                            LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                            LEFT JOIN ORG.Line L ON E.LineID = L.Id
                            LEFT JOIN HKP.Designation D ON E.DesignationSystemID = D.Id
                            LEFT JOIN HKP.LegalDesignation AS ld  ON E.LegalDesignationId = ld.Id
							LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
							LEFT JOIN MST.ManpowerBudget mb ON mb.Id=e.BudgetCode
							
							
							left outer join (
                            	--***********LV**********************
                            	
								SELECT DC.LeavePolicyMasterId ,lpm.PolicyName ,e.SystemId EmpId,d.*
																				FROM 
																				EmployeeInformation e
																				LEFT join MST.DesignationMaster DM ON e.GivenDesignationId=dm.DesignationId
																				LEFT JOIN SCS.DesignationMasterConfiguration DC 
																							ON DM.Id=DC.DesignationMasterId AND dc.plantid=e.plantid
																				LEFT JOIN LeavePolicyDetail d ON d.LPMSystemID=dc.LeavePolicyMasterId
                                                                                LEFT JOIN LeavePolicyMaster AS lpm  ON lpm.SystemID=dc.LeavePolicyMasterId
																where dc.plantid='" + identity.PlantId + @"'
											--*******************LV***********************
							) ltd on ltd.LTSystemID = t.Id AND ltd.EmpId=e.SystemId
                            where s.YearlyCalendarId =(select id from YearlyCalendar where Id=" + YearlyCalendarId + @" and PlantId='" + identity.PlantId + @"') 
                            AND s.EmpSystemId  IN (SELECT SystemId FROM EmployeeInformation WHERE EmployeeStatus='Active' and PlantId='" + identity.PlantId + @"' )
                            AND s.LeaveEncashmentType='Encashment Within Year' 
                            ORDER BY  e.EmployeeCodePreFix,e.EmployeeCodeNumeric ";

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


        [HttpGet]
        public ActionResult xGetSevedWithInYearLeaveEncashmentData(string ToDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(ToDate))
            {
                ToDate = DateTime.Now.ToString("dd-MMM-yyyy");
            }
            string YearlyCalendarId = string.Empty;
            DataSet dsYearCalander = null;

            DataSet dsleaveEncashment;
            ConnectionManager.DAL.ConManager objCon;

            string sql1 = @"select * from YearlyCalendar where '" + Convert.ToDateTime(ToDate).ToString("dd-MMM-yyyy") + @"' BETWEEN FromDate AND ToDate  
                                            AND PlantId='" + identity.PlantId + @"' ";

            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(sql1, out dsYearCalander, false, "1");

            if (dsYearCalander.Tables[0].Rows.Count > 0)
            {
                YearlyCalendarId = dsYearCalander.Tables[0].Rows[0]["Id"].ToString();
            }


            string sql = string.Empty;
            try
            {
                sql = @"SELECT  [CheckBoxSelect] = Convert(bit, 'False'), 
                             E.SystemId, e.EmployeeCode,e.EmployeeName,t.UserName LeaveType ,FORMAT(e.DOJ,'dd-MMM-yyyy') DOJ
                             , EC.UserName EmpCategoryName  
                            ,ld.UserName Designation
                            ,U.UserName Unit 
                            ,Dv.UserName Division
                            ,Dp.UserName Department
                            ,Se.UserName Section 
                            ,SB.UserName SubSection 
                            ,L.UserName Line
                            ,BroughtForward=isnull(s.BroughtForward,0)+isnull(s.CarryForwardOpeningBalance,0)
                            ,s.DaysCanBeSanctioned
                            ,s.CurrentYearAllocation
                            ,s.IsYearlyProcessed
                            ,LeaveDaysAllowed=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)+isnull(s.CarryForwardOpeningBalance,0)
                            ,isnull(kk.LeaveDuration,0) AvailedLeave
                            --,Balance=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)
                           -----------------------------------Is Brought Forward Add to balance -----------------------------------------------------------                                       
							,Balance=CASE WHEN t.LeaveType='Earn' THEN  
								CASE WHEN
								-----------------------------------DOJorDOC start -----------------------------------------------------------
															CASE WHEN ltd.LvAvailedOnDOJ=1 THEN                            										 
                            																	 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  e.DOJ ) END
																	   WHEN  ltd.LvAvailedOnDOC=1 THEN 										   
										   														 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	e.DOC  )
										   													END
																   END
							---------------------------------------DOJorDOC start  end-------------------------------------------------------
	
								> GETDATE() then 
									  isnull(s.BroughtForward,0)+isnull(s.CarryForwardOpeningBalance,0)+isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0)------No
									ELSE  isnull(s.BroughtForward,0)+isnull(s.CarryForwardOpeningBalance,0)+isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0) END---Yes
							ELSE  isnull(s.BroughtForward,0)+isnull(s.CarryForwardOpeningBalance,0)+isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0) END  ---No

                            ,av.LeaveEncashmentDayNo Days,ltd.PolicyName
                            from TRN.EmployeeLeaveSummary S 
                            INNER JOIN LeaveType t on s.LeaveTypeId=t.Id AND t.LeaveType='Earn'
                            INNER JOIN EmployeeInformation e on e.SystemId=s.EmployeeId
                            LEFT JOIN (
									select 
									tt.UserName LeaveType,t.EmpSystemID,t.LTSystemID, sum(isnull(d.LeaveDuration,0)) LeaveDuration
									from 
									LeaveTransaction t 
									left join 
							          (--detail
                                    select SUM(LeaveDuration) LeaveDuration, LvTrnsSystemID from LeaveTransactionDetails 
                                    where IsAvailed=1
                                    and WorkDate between
                                    (select FromDate from YearlyCalendar where Id=" + YearlyCalendarId + @" and PlantId='" + identity.PlantId + @"')
                                    and (select ToDate from YearlyCalendar where Id= " + YearlyCalendarId + @" and PlantId='" + identity.PlantId + @"')
                                    group by LvTrnsSystemID
                                    )--detail 
                                    d on t.SystemID=d.LvTrnsSystemID

									left join LeaveType tt on tt.id=t.LTSystemID
									where t.IsApproved=1  
									group by tt.UserName ,t.EmpSystemID,t.LTSystemID
                            ) kk on kk.LTSystemID=s.LeaveTypeId and kk.EmpSystemID=s.EmployeeId
                             left outer join (
                            	--***********LV**********************
                            	
								SELECT DC.LeavePolicyMasterId,lpm.PolicyName ,e.SystemId EmpId,d.*
																				FROM 
																				EmployeeInformation e
																				LEFT join MST.DesignationMaster DM ON e.GivenDesignationId=dm.DesignationId
																				LEFT JOIN SCS.DesignationMasterConfiguration DC 
																							ON DM.Id=DC.DesignationMasterId AND dc.plantid=e.plantid
																				LEFT JOIN LeavePolicyDetail d ON d.LPMSystemID=dc.LeavePolicyMasterId
																				LEFT JOIN LeavePolicyMaster AS lpm  ON lpm.SystemID=dc.LeavePolicyMasterId			
																where dc.plantid='" + identity.PlantId + @"'
											--*******************LV***********************
							) ltd on ltd.LTSystemID = t.Id AND ltd.EmpId=e.SystemId
                          
                             LEFT JOIN 
                            (
                            	SELECT EmpSystemId,YearlyCalendarId ,SUM(Days) LeaveEncashmentDayNo FROM LeaveEncashmentTransaction WHERE  YearlyCalendarId='" + YearlyCalendarId + @"' GROUP BY EmpSystemId,YearlyCalendarId
                            ) as 
                            av ON av.EmpSystemId=s.EmployeeId
                            LEFT JOIN HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
                            LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                            LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                            LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id
                            LEFT JOIN ORG.Section Se ON E.SectionID = Se.Id
                            LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                            LEFT JOIN ORG.Line L ON E.LineID = L.Id
                            LEFT JOIN HKP.Designation D ON E.DesignationSystemID = D.Id
                            LEFT JOIN HKP.LegalDesignation AS ld  ON E.LegalDesignationId = ld.Id
							LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
							LEFT JOIN MST.ManpowerBudget mb ON mb.Id=e.BudgetCode
                            where s.CalanderYearId=(select id from YearlyCalendar where Id=" + YearlyCalendarId + @" and PlantId='" + identity.PlantId + @"') 
                            AND s.EmployeeId IN (SELECT SystemId FROM EmployeeInformation WHERE EmployeeStatus='Active' and PlantId='" + identity.PlantId + @"' )
                            AND s.EmployeeId  IN (SELECT EmpSystemId  FROM LeaveEncashmentTransaction WHERE YearlyCalendarId='" + YearlyCalendarId + @"' AND LeaveEncashmentType='Encashment Within Year' and PlantId='" + identity.PlantId + @"' )
                            ORDER BY  e.EmployeeCodePreFix,e.EmployeeCodeNumeric";

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
        #endregion
        #region Specific-date-leave-encashment
        [HttpGet, Authorize]
        public ActionResult GetSpecificDateLeaveEncashmentData()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsLeaveEncashment ob = new clsLeaveEncashment();
            List<MultipleLeaveEncashmentViewModel> data = ob.GetSpecificDateLeaveEncashmentData( identity.PlantId);
            List<MultipleLeaveEncashmentViewModel> dataNew = data.Where(x => x.Days > 0).OrderBy(x => x.EmployeeCode).ToList();
            //return Json(new { LeaveInfo = data }, JsonRequestBehavior.AllowGet);

            string SpecificDate = string.Empty;
            DataSet dsSpecificDate = null;          
            ConnectionManager.DAL.ConManager objCon;

            string sql1 = @"SELECT FORMAT( DATEFROMPARTS( YEAR(GETDATE()),CONVERT(INT,ltd.EncashmentSpecificMonth),CONVERT(INT,ltd.EncashmentSpecificDay)),'dd-MMM-yyyy') EncashmentSpecificDate FROM LeavePolicyDetail AS ltd
                            WHERE ltd.EncashmentBasis='EncashmentDate' AND ltd.PlantID='" + identity.PlantId + @"' ORDER BY DATEFROMPARTS( YEAR(GETDATE()),CONVERT(INT,ltd.EncashmentSpecificMonth),CONVERT(INT,ltd.EncashmentSpecificDay))
";

            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(sql1, out dsSpecificDate, false, "1");

            if (dsSpecificDate.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < dsSpecificDate.Tables[0].Rows.Count; i++)
                {
                    if (string.IsNullOrEmpty(SpecificDate))
                    {
                        SpecificDate = "Encashment Specific Date is : " + dsSpecificDate.Tables[0].Rows[i]["EncashmentSpecificDate"].ToString();
                    }
                    else
                    {
                        SpecificDate += ", "+dsSpecificDate.Tables[0].Rows[i]["EncashmentSpecificDate"].ToString();
                    }
                }
                
            }


            JsonResult json = Json(new { LeaveInfo = data, SpecificDate }, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }
        [HttpPost]
        public JsonResult SaveSpecificDateLeaveEncashment(List<MultipleLeaveEncashmentViewModelNew> leaveEncashment)
        {


            //var settings = new JsonSerializerSettings
            //{
            //    NullValueHandling = NullValueHandling.Ignore,
            //    MissingMemberHandling = MissingMemberHandling.Ignore
            //};
            //List<MultipleLeaveEncashmentViewModelNew> leaveEncashment = JsonConvert.DeserializeObject<List<MultipleLeaveEncashmentViewModelNew>>(leaveEncashmentdata, settings);





            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string YearlyCalendarId = string.Empty;
            DataSet dsYearCalander = null;
            DataSet dsOldSummary = null;
            DataView dvSaveSummaryOld = null;

            DataSet dsleaveEncashment;
            ConnectionManager.DAL.ConManager objCon;
            try
            {









                //string sql1 = @"select * from YearlyCalendar where '" + Convert.ToDateTime(EncashmentDate).ToString("dd-MMM-yyyy") + @"' BETWEEN FromDate AND ToDate  
                //                            AND PlantId='" + identity.PlantId + @"' ";

                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(sql1, out dsYearCalander, false, "1");

                //if (dsYearCalander.Tables[0].Rows.Count > 0)
                //{
                //    YearlyCalendarId = dsYearCalander.Tables[0].Rows[0]["Id"].ToString();
                //}


                string sqlsummary = @"SELECT * FROM TRN.EmployeeLeaveSummary WHERE  LeaveTypeId IN (SELECT Id FROM  LeaveType WHERE LeaveType='Earn' )  
                                            AND PlantId='" + identity.PlantId + @"' ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlsummary, out dsOldSummary, false, "1");
                dvSaveSummaryOld = new DataView(dsOldSummary.Tables[0]);




                string sql = @"SELECT * FROM LeaveEncashmentTransaction 
                                            WHERE PlantId='" + identity.PlantId + @"'                                            
                                            AND LeaveEncashmentType='Specific Date Leave Encashment' ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsleaveEncashment, false, "1");




                DataView dvleaveEncashment = new DataView(dsleaveEncashment.Tables[0]);

                for (int i = 0; i < leaveEncashment.Count; i++)
                {
                    dvleaveEncashment.RowFilter = "EmpSystemId='" + leaveEncashment[i].EmpSystemId + @"' AND YearlyCalendarId='" + leaveEncashment[i].YearlyCalendarId   + @"'  AND LeaveEncashmentType='Specific Date Leave Encashment'";

                    if (dvleaveEncashment.Count == 0)
                    {
                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "MultipleaveEncashment", out sID);
                        DataRow dr = dsleaveEncashment.Tables[0].NewRow();
                        dr["Id"] = "LE" + sID;
                        dr["PlantId"] = identity.PlantId.ToString();
                        dr["EmpSystemId"] = leaveEncashment[i].EmpSystemId;
                        dr["LeaveEncashmentType"] = "Specific Date Leave Encashment";
                        dr["EncashmentDate"] = System.DateTime.Now.ToString("dd-MMM-yyyy");  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                        dr["Days"] = leaveEncashment[i].Days;
                        dr["LeaveTypeSystemId"] = leaveEncashment[i].LeaveTypeId;
                        //dr["Rest"] = leaveEncashment.Balance-(leaveEncashment.AvailedEncashment+ leaveEncashment.Days);
                        dr["Rate"] = leaveEncashment[i].Rate;





                        dr["PaymentMode"] = leaveEncashment[i].PaymentMode;
                        dr["BasicAmmount"] = leaveEncashment[i].BasicAmmount;
                        dr["GrossAmmount"] = leaveEncashment[i].GrossAmmount;
                        //dr["GivenDesignationId"] = leaveEncashment[i].GivenDesignationId;
                        dr["BankSystemID"] = leaveEncashment[i].BankSystemID;
                        dr["BankBranchId"] = leaveEncashment[i].BankBranchId;
                        dr["BankAccNo"] = leaveEncashment[i].BankAccNo;
                        //dr["SalaryPercentage"] = leaveEncashment[i].SalaryPercentage;
                        //dr["BudgetCode"] = leaveEncashment[i].BudgetCode;
                        //dr["EmployeeCategoryId"] = leaveEncashment[i].EmployeeCategoryId;
                        dr["LegalDesignationId"] = leaveEncashment[i].LegalDesignationId;



                        dr["BroughtForward"] = leaveEncashment[i].BroughtForward;
                        dr["DaysCanBeSanctioned"] = leaveEncashment[i].DaysCanBeSanctioned;
                        dr["AvailedLeave"] = leaveEncashment[i].AvailedLeave;
                        dr["CarryForward"] = leaveEncashment[i].NewBroughtForward;
                        //dr["Balance"] = leaveEncashment[i].Balance - leaveEncashment[i].NewYearEndEncash;



                        dr["Isdisburse"] = true;
                        dr["YearlyCalendarId"] = leaveEncashment[i].YearlyCalendarId;
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dsleaveEncashment.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        DataRow dr = dvleaveEncashment[0].Row;
                        dr.BeginEdit();
                        dr["PlantId"] = identity.PlantId.ToString();
                        dr["EmpSystemId"] = leaveEncashment[i].EmpSystemId;
                        dr["LeaveEncashmentType"] = "Specific Date Leave Encashment";
                        dr["EncashmentDate"] = System.DateTime.Now.ToString("dd-MMM-yyyy");  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                        dr["Days"] = leaveEncashment[i].Days;
                        //dr["Rest"] = leaveEncashment.Balance - (leaveEncashment.AvailedEncashment + leaveEncashment.Days);
                        dr["Rate"] = leaveEncashment[i].Rate;
                        dr["PaymentMode"] = leaveEncashment[i].PaymentMode;
                        dr["BasicAmmount"] = leaveEncashment[i].BasicAmmount;
                        dr["GrossAmmount"] = leaveEncashment[i].GrossAmmount;
                        //dr["GivenDesignationId"] = leaveEncashment[i].GivenDesignationId;
                        dr["BankSystemID"] = leaveEncashment[i].BankSystemID;
                        dr["BankBranchId"] = leaveEncashment[i].BankBranchId;
                        dr["BankAccNo"] = leaveEncashment[i].BankAccNo;
                        //dr["SalaryPercentage"] = leaveEncashment[i].SalaryPercentage;
                        //dr["BudgetCode"] = leaveEncashment[i].BudgetCode;
                        //dr["EmployeeCategoryId"] = leaveEncashment[i].EmployeeCategoryId;
                        dr["LegalDesignationId"] = leaveEncashment[i].LegalDesignationId;


                        dr["BroughtForward"] = leaveEncashment[i].BroughtForward;
                        dr["DaysCanBeSanctioned"] = leaveEncashment[i].DaysCanBeSanctioned;
                        dr["AvailedLeave"] = leaveEncashment[i].AvailedLeave;
                        dr["CarryForward"] = leaveEncashment[i].NewBroughtForward; ;
                        //dr["Balance"] = leaveEncashment[i].Balance- leaveEncashment[i].NewYearEndEncash;

                        dr["Isdisburse"] = true;
                        dr["YearlyCalendarId"] = leaveEncashment[i].YearlyCalendarId;
                        dr["LeaveTypeSystemId"] = leaveEncashment[i].LeaveTypeId;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();

                    }
                    dvleaveEncashment.RowFilter = null;


                    //Old year insert or update

                    dvSaveSummaryOld.RowFilter = "EmployeeId='" + leaveEncashment[i].EmpSystemId + "' and LeaveTypeId='" + leaveEncashment[i].LeaveTypeId + "' and CalanderYearId='" + leaveEncashment[i].YearlyCalendarId + "' AND IsEncashed=0";
                    if (dvSaveSummaryOld.Count > 0)
                    {


                        DataRow drSaveSummaryOld = dvSaveSummaryOld[0].Row;
                        drSaveSummaryOld.BeginEdit();
                        drSaveSummaryOld["CarryForward"] = leaveEncashment[i].NewBroughtForward;
                        drSaveSummaryOld["YearEndEncash"] = leaveEncashment[i].NewYearEndEncash;
                        drSaveSummaryOld["YearEndLapse"] = leaveEncashment[i].NewYearEndLapse;
                        //drSaveSummaryOld["BroughtForward"] = leaveEncashment[i].NewBroughtForward;
                        drSaveSummaryOld["EncashedInbetween"] = leaveEncashment[i].NewYearEndEncash;
                        drSaveSummaryOld["IsEncashed"] = true;
                        drSaveSummaryOld["UpdatedFromIP"] = "::1";
                        drSaveSummaryOld["UpdatedDate"] = System.DateTime.Now;
                        drSaveSummaryOld["UpdatedBy"] = "Schedule";
                        drSaveSummaryOld.EndEdit();
                    }

                    dvSaveSummaryOld.RowFilter = null;
                }




                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsleaveEncashment, dsOldSummary);





            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Success });
        }
        [HttpGet]
        public ActionResult GetSevedSpecificDateLeaveEncashmentData()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
          
             string   ToDate = DateTime.Now.ToString("dd-MMM-yyyy");
           
            string YearlyCalendarId = string.Empty;
            DataSet dsYearCalander = null;

            DataSet dsleaveEncashment;
            ConnectionManager.DAL.ConManager objCon;

            string sql1 = @"select * from YearlyCalendar where '" + Convert.ToDateTime(ToDate).ToString("dd-MMM-yyyy") + @"' BETWEEN FromDate AND ToDate  
                                            AND PlantId='" + identity.PlantId + @"' ";

            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(sql1, out dsYearCalander, false, "1");

            if (dsYearCalander.Tables[0].Rows.Count > 0)
            {
                YearlyCalendarId = dsYearCalander.Tables[0].Rows[0]["Id"].ToString();
            }


            string sql = string.Empty;
            try
            {
                sql = @"SELECT  [CheckBoxSelect] = Convert(bit, 'False'), 
                             E.SystemId, e.EmployeeCode,e.EmployeeName,t.UserName LeaveType ,FORMAT(e.DOJ,'dd-MMM-yyyy') DOJ
                            , EC.UserName EmpCategoryName  
                            ,ld.UserName Designation
                            ,U.UserName Unit 
                            ,Dv.UserName Division
                            ,Dp.UserName Department
                            ,Se.UserName Section 
                            ,SB.UserName SubSection 
                            ,L.UserName Line
                            
                            ,S.BroughtForward
                            ,s.DaysCanBeSanctioned
                            
                          
							,Balance=ISNULL(s.BroughtForward,0)+ISNULL(s.DaysCanBeSanctioned,0)-ISNULL(s.AvailedLeave,0)-ISNULL(s.Days,0)-ISNULL(s.YearEndLapse,0)
                            ,S.Days
                            ,FORMAT(DATEFROMPARTS( YEAR(GETDATE()),CONVERT(INT,ltd.EncashmentSpecificMonth),CONVERT(INT,ltd.EncashmentSpecificDay)),'dd-MMM-yyyy') EncashmentSpecificDate
                            ,ltd.PolicyName
                            from LeaveEncashmentTransaction S 
                            INNER JOIN LeaveType t on s.LeaveTypeSystemId=t.Id AND t.LeaveType='Earn'
                            INNER JOIN EmployeeInformation e on e.SystemId=s.empsystemId
                          
                            LEFT JOIN HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
                            LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                            LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                            LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id
                            LEFT JOIN ORG.Section Se ON E.SectionID = Se.Id
                            LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                            LEFT JOIN ORG.Line L ON E.LineID = L.Id
                            LEFT JOIN HKP.Designation D ON E.DesignationSystemID = D.Id
                            LEFT JOIN HKP.LegalDesignation AS ld  ON E.LegalDesignationId = ld.Id
							LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
							LEFT JOIN MST.ManpowerBudget mb ON mb.Id=e.BudgetCode
							
							
							left outer join (
                            	--***********LV**********************
                            	
								SELECT DC.LeavePolicyMasterId ,lpm.PolicyName ,e.SystemId EmpId,d.*
																				FROM 
																				EmployeeInformation e
																				LEFT join MST.DesignationMaster DM ON e.GivenDesignationId=dm.DesignationId
																				LEFT JOIN SCS.DesignationMasterConfiguration DC 
																							ON DM.Id=DC.DesignationMasterId AND dc.plantid=e.plantid
																				LEFT JOIN LeavePolicyDetail d ON d.LPMSystemID=dc.LeavePolicyMasterId
                                                                                LEFT JOIN LeavePolicyMaster AS lpm  ON lpm.SystemID=dc.LeavePolicyMasterId
																where dc.plantid='" + identity.PlantId + @"'
											--*******************LV***********************
							) ltd on ltd.LTSystemID = t.Id AND ltd.EmpId=e.SystemId
                            where s.YearlyCalendarId =(select id from YearlyCalendar where Id=" + YearlyCalendarId + @" and PlantId='" + identity.PlantId + @"') 
                            AND s.EmpSystemId  IN (SELECT SystemId FROM EmployeeInformation WHERE EmployeeStatus='Active' and PlantId='" + identity.PlantId + @"' )
                            AND s.LeaveEncashmentType='Specific Date Leave Encashment' 
                            ORDER BY  e.EmployeeCodePreFix,e.EmployeeCodeNumeric  ";

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
        [HttpGet]
        public ActionResult xGetSevedSpecificDateLeaveEncashmentData()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string ToDate = DateTime.Now.ToString("dd-MMM-yyyy");

            string YearlyCalendarId = string.Empty;
            DataSet dsYearCalander = null;

            DataSet dsleaveEncashment;
            ConnectionManager.DAL.ConManager objCon;

            string sql1 = @"select * from YearlyCalendar where '" + Convert.ToDateTime(ToDate).ToString("dd-MMM-yyyy") + @"' BETWEEN FromDate AND ToDate  
                                            AND PlantId='" + identity.PlantId + @"' ";

            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(sql1, out dsYearCalander, false, "1");

            if (dsYearCalander.Tables[0].Rows.Count > 0)
            {
                YearlyCalendarId = dsYearCalander.Tables[0].Rows[0]["Id"].ToString();
            }


            string sql = string.Empty;
            try
            {
                sql = @"SELECT  [CheckBoxSelect] = Convert(bit, 'False'), 
                             E.SystemId, e.EmployeeCode,e.EmployeeName,t.UserName LeaveType ,FORMAT(e.DOJ,'dd-MMM-yyyy') DOJ
                            , EC.UserName EmpCategoryName  
                            ,ld.UserName Designation
                            ,U.UserName Unit 
                            ,Dv.UserName Division
                            ,Dp.UserName Department
                            ,Se.UserName Section 
                            ,SB.UserName SubSection 
                            ,L.UserName Line
                            --,BroughtForward=isnull(s.BroughtForward,0)+isnull(s.CarryForwardOpeningBalance,0)
                            ,BroughtForward=CASE WHEN s.IsEncashed =1 THEN ISNULL(s.CarryForward, 0)+ISNULL(s.EncashedInbetween, 0) ELSE ISNULL(s.BroughtForward, 0)+isnull(s.CarryForwardOpeningBalance,0) END
                            ,s.DaysCanBeSanctioned
                            ,s.CurrentYearAllocation
                            ,s.IsYearlyProcessed
                            ,LeaveDaysAllowed=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)+isnull(s.CarryForwardOpeningBalance,0)
                            ,isnull(kk.LeaveDuration,0) AvailedLeave
                            --,Balance=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)
                           -----------------------------------Is Brought Forward Add to balance -----------------------------------------------------------                                       
							,Balance=CASE WHEN t.LeaveType='Earn' THEN  
								CASE WHEN
								-----------------------------------DOJorDOC start -----------------------------------------------------------
															CASE WHEN ltd.LvAvailedOnDOJ=1 THEN                            										 
                            																	 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  e.DOJ ) END
																	   WHEN  ltd.LvAvailedOnDOC=1 THEN 										   
										   														 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	e.DOC  )
										   													END
																   END
							---------------------------------------DOJorDOC start  end-------------------------------------------------------
	
								> GETDATE() then 
									  (CASE WHEN s.IsEncashed =1 THEN ISNULL(s.CarryForward, 0)+ISNULL(s.EncashedInbetween, 0) ELSE ISNULL(s.BroughtForward, 0)+isnull(s.CarryForwardOpeningBalance,0) END)+isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0)------No
									ELSE  (CASE WHEN s.IsEncashed =1 THEN ISNULL(s.CarryForward, 0)+ISNULL(s.EncashedInbetween, 0) ELSE ISNULL(s.BroughtForward, 0)+isnull(s.CarryForwardOpeningBalance,0) END)+isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0) END---Yes
							ELSE  (CASE WHEN s.IsEncashed =1 THEN ISNULL(s.CarryForward, 0)+ISNULL(s.EncashedInbetween, 0) ELSE ISNULL(s.BroughtForward, 0)+isnull(s.CarryForwardOpeningBalance,0) END)+isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0) END  ---No

                            ,av.LeaveEncashmentDayNo Days,FORMAT(DATEFROMPARTS( YEAR(GETDATE()),CONVERT(INT,ltd.EncashmentSpecificMonth),CONVERT(INT,ltd.EncashmentSpecificDay)),'dd-MMM-yyyy') EncashmentSpecificDate,ltd.PolicyName
                            from TRN.EmployeeLeaveSummary S 
                            INNER JOIN LeaveType t on s.LeaveTypeId=t.Id AND t.LeaveType='Earn'
                            INNER JOIN EmployeeInformation e on e.SystemId=s.EmployeeId
                            LEFT JOIN (
									select 
									tt.UserName LeaveType,t.EmpSystemID,t.LTSystemID, sum(isnull(d.LeaveDuration,0)) LeaveDuration
									from 
									LeaveTransaction t 
									left join 
							          (--detail
                                    select SUM(LeaveDuration) LeaveDuration, LvTrnsSystemID from LeaveTransactionDetails 
                                    where IsAvailed=1
                                    and WorkDate between
                                    (select FromDate from YearlyCalendar where Id=" + YearlyCalendarId + @" and PlantId='" + identity.PlantId + @"')
                                    and (select ToDate from YearlyCalendar where Id= " + YearlyCalendarId + @" and PlantId='" + identity.PlantId + @"')
                                    group by LvTrnsSystemID
                                    )--detail 
                                    d on t.SystemID=d.LvTrnsSystemID

									left join LeaveType tt on tt.id=t.LTSystemID
									where t.IsApproved=1  
									group by tt.UserName ,t.EmpSystemID,t.LTSystemID
                            ) kk on kk.LTSystemID=s.LeaveTypeId and kk.EmpSystemID=s.EmployeeId
                             left outer join (
                            	--***********LV**********************
                            	
								SELECT DC.LeavePolicyMasterId ,lpm.PolicyName ,e.SystemId EmpId,d.*
																				FROM 
																				EmployeeInformation e
																				LEFT join MST.DesignationMaster DM ON e.GivenDesignationId=dm.DesignationId
																				LEFT JOIN SCS.DesignationMasterConfiguration DC 
																							ON DM.Id=DC.DesignationMasterId AND dc.plantid=e.plantid
																				LEFT JOIN LeavePolicyDetail d ON d.LPMSystemID=dc.LeavePolicyMasterId
                                                                                LEFT JOIN LeavePolicyMaster AS lpm  ON lpm.SystemID=dc.LeavePolicyMasterId
																where dc.plantid='" + identity.PlantId + @"'
											--*******************LV***********************
							) ltd on ltd.LTSystemID = t.Id AND ltd.EmpId=e.SystemId
                          
                             LEFT JOIN 
                            (
                            	SELECT EmpSystemId,YearlyCalendarId ,SUM(Days) LeaveEncashmentDayNo FROM LeaveEncashmentTransaction WHERE  YearlyCalendarId='" + YearlyCalendarId + @"' GROUP BY EmpSystemId,YearlyCalendarId
                            ) as 
                            av ON av.EmpSystemId=s.EmployeeId
                            LEFT JOIN HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
                            LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                            LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                            LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id
                            LEFT JOIN ORG.Section Se ON E.SectionID = Se.Id
                            LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                            LEFT JOIN ORG.Line L ON E.LineID = L.Id
                            LEFT JOIN HKP.Designation D ON E.DesignationSystemID = D.Id
                            LEFT JOIN HKP.LegalDesignation AS ld  ON E.LegalDesignationId = ld.Id
							LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
							LEFT JOIN MST.ManpowerBudget mb ON mb.Id=e.BudgetCode
                            where s.CalanderYearId=(select id from YearlyCalendar where Id=" + YearlyCalendarId + @" and PlantId='" + identity.PlantId + @"') 
                            AND s.EmployeeId IN (SELECT SystemId FROM EmployeeInformation WHERE EmployeeStatus='Active' and PlantId='" + identity.PlantId + @"' )
                            AND s.EmployeeId  IN (SELECT EmpSystemId  FROM LeaveEncashmentTransaction WHERE YearlyCalendarId='" + YearlyCalendarId + @"' AND LeaveEncashmentType='Specific Date Leave Encashment' and PlantId='" + identity.PlantId + @"' )
                            ORDER BY  e.EmployeeCodePreFix,e.EmployeeCodeNumeric";

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
        #endregion

    }

    public class SalaryInfoModel
    {
        public string HeadName { get; set; }
        public decimal HeadValue { get; set; }
    }


}