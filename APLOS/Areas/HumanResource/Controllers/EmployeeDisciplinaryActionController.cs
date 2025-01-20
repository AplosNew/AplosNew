#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Employees;
using Library.Model.HumanResources;
using Library.Service.Employees;
using Library.Service.Extension;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Syncfusion.DocIO.DLS;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Mvc;
using static Aplos.Areas.HumanResource.Controllers.LongAbsenteeismAssignController;

#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class EmployeeDisciplinaryActionController : BaseController
    {
        #region Constructor
        private readonly IResignationService _ResignationService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IStoppageService _stoppageService;
        private readonly IEmployeeDisciplinaryActionService _EmployeeDisciplinaryActionService;
        public EmployeeDisciplinaryActionController(
              IStoppageService stoppageService,
              ISqlRepository sqlRepository,
              IEmployeeDisciplinaryActionService EmployeeDisciplinaryActionService,
              IResignationService ResignationService
            )
        {
            _stoppageService = stoppageService;
            _sqlRepository = sqlRepository;
            _EmployeeDisciplinaryActionService = EmployeeDisciplinaryActionService;
            _ResignationService = ResignationService;

        }
        #endregion

        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult DisciplinaryActionTransaction()
        {
            return View();
        }
        

        //[AllowAnonymous]
        //public JsonResult GetCbo()
        //{
        //    return Json(new SelectList(_EmployeeDisciplinaryActionService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        //}

        [HttpGet, Authorize]
        public ActionResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select Id,UserName From [HKP].[DisciplinaryActionCategory]";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListCount(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_EmployeeDisciplinaryActionService.QueryActionCount(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetListByEmployee(string EmpSysId)
        {
            return Json(_EmployeeDisciplinaryActionService.Query(EmpSysId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(EmployeeDisciplinaryAction employeeDisciplinaryAction)
        {
            _EmployeeDisciplinaryActionService.Insert(employeeDisciplinaryAction);
            return Json(new { EmployeeDisciplinaryAction = employeeDisciplinaryAction, Message = AplosMessage.Success });
        }
        [HttpPost]
        public JsonResult Edit(EmployeeDisciplinaryAction employeeDisciplinaryAction)
        {
            _EmployeeDisciplinaryActionService.Update(employeeDisciplinaryAction);
            return Json(new { Message = AplosMessage.Updated });
        }


        public ActionResult Delete(string id)
        {
            _EmployeeDisciplinaryActionService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }





        [HttpGet, Authorize]
        public ActionResult GetDueEmployeeDisciplinaryActionsList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT	EA.EmpSystemId ,EA.Id CaseNo 
		,E.EmployeeCode
		,E.EmployeeName
		,DPT.UserName Department
		,SEC.UserName Section
		,SSEC.UserName SubSection
		,DEG.UserName Designation
		,GDEG.UserName GivenDesignation 
		,REPLACE(CONVERT(varchar(11), E.DOJ, 106), ' ', '-') AS DOJ
		,DAC.UserName DisciplinaryActionCategory,EA.DisciplinaryActionCategoryId
		,SUBSTRING(
			(
				SELECT ','+ DAC.LetterName  AS [text()]
				FROM  [dbo].[EmployeeDisciplinaryActionDetails] DAD 				
				LEFT JOIN [dbo].[DisciplinaryActionSettingChild] DAC on DAC.Id = DAD.LetterFormetId
				WHERE DAD.EmployeeDisciplinaryActionId = EA.Id			
				
				ORDER BY DAD.LetterFormetId
				FOR XML PATH ('')
			), 2, 1000) [Letters],Format(EA.EntryDate,'dd-MMM-yyyy') EntryDate,Format(dadm.NextLetterDueDate,'dd-MMM-yyyy') NextLetterDueDate,Format(dadm.LetterIssueDate,'dd-MMM-yyyy') LetterIssueDate,EA.Id EmployeeDisciplinaryActionId
        ,DueStatus =case when GETDATE()>Convert(date, dadm.NextLetterDueDate) then 'OVERDUE' 
							when GETDATE()=Convert(date, dadm.NextLetterDueDate) then 'DUE' 
							when DATEADD(day,7, GETDATE())=Convert(date, dadm.NextLetterDueDate) then 'TOBEDUE' 
					end
        ,dadm.DisciplinaryActionSettingDetailsId DisciplinaryActionSettingDetailsId,dadm.Id EmployeeDisciplinaryActionDetailsId
        ,DueLetter=(
		         SELECT 
			         --DAS.Id
			         --,DAS.DisciplinaryActionCategoryId
			         --,DAS.Sequence
			         --,DAS.LetterIssueDay
			         DAS.Description as UserName
			         From  DisciplinaryActionSettingDetails  DAS
			         where das.DisciplinaryActionCategoryId=EA.DisciplinaryActionCategoryId--'DACM2010'
			         and das.IsActive = 1 
			         and Sequence=(select Sequence+1 from DisciplinaryActionSettingDetails where id=dadm.DisciplinaryActionSettingDetailsId--'DASD2057'
			         )
		        )
		FROM HKP.EmployeeDisciplinaryAction EA
		LEFT JOIN [HKP].[DisciplinaryActionCategory] DAC on DAC.Id=EA.DisciplinaryActionCategoryId
		
        LEFT JOIN 
		(		
        select m.EmpSystemId,d.EmployeeDisciplinaryActionId,Max(ds.Sequence) Sequence,ds.DisciplinaryActionCategoryId
		from  [dbo].[EmployeeDisciplinaryActionDetails] d
		inner join HKP.EmployeeDisciplinaryAction m on m.Id=d.EmployeeDisciplinaryActionId
		left join  DisciplinaryActionSettingDetails ds on ds.Id=d.DisciplinaryActionSettingDetailsId
		group by m.EmpSystemId,d.EmployeeDisciplinaryActionId,ds.DisciplinaryActionCategoryId		
		) dadm2 on   dadm2.EmployeeDisciplinaryActionId=EA.Id  
		inner join  DisciplinaryActionSettingDetails ds2 on ds2.DisciplinaryActionCategoryId=dadm2.DisciplinaryActionCategoryId and ds2.Sequence=dadm2.Sequence		
	    inner join  [dbo].[EmployeeDisciplinaryActionDetails] dadm  on 	 dadm.DisciplinaryActionSettingDetailsId=ds2.id and dadm.EmployeeDisciplinaryActionId=dadm2.EmployeeDisciplinaryActionId

        LEFT JOIN EmployeeInformation E on E.SystemId = EA.EmpSystemId
        LEFT JOIN MST.ManpowerBudget MB ON MB.Id = E.BudgetCode
        LEFT JOIN ORG.Entity EN ON EN.Id = MB.EntityId
        LEFT JOIN ORG.Position PS on PS.Id = MB.PositionId
        LEFT JOIN[ORG].[Division] DIV ON DIV.Id = PS.DivisionId
        LEFT JOIN[ORG].[SubDivision] SDIV ON SDIV.Id = PS.SubdivisionID
        LEFT JOIN[ORG].[Department] DPT ON DPT.Id = PS.DepartmentId
        LEFT JOIN[ORG].[Section] SEC ON SEC.Id = PS.SectionId
        LEFT JOIN[ORG].[SubSection] SSEC ON SSEC.Id = PS.SubSectionId
        LEFT JOIN[HKP].[Designation] DEG ON DEG.Id = PS.DesignationID
        LEFT JOIN[HKP].[Designation] GDEG ON GDEG.Id = E.GivenDesignationId	
        where  ISNULL(ActionType,'') NOT IN ('TBS')  and e.PlantId='" + identity.PlantId + @"' and e.EmployeeStatus='Active' and dadm.NextLetterDueDate is not null
        order by convert(date,dadm.NextLetterDueDate) --desc";
            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetCompletedEmployeeDisciplinaryActionsList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT	EA.EmpSystemId ,EA.Id CaseNo 
		,E.EmployeeCode
		,E.EmployeeName
		,DPT.UserName Department
		,SEC.UserName Section
		,SSEC.UserName SubSection
		,DEG.UserName Designation
		,GDEG.UserName GivenDesignation 
		,REPLACE(CONVERT(varchar(11), E.DOJ, 106), ' ', '-') AS DOJ
		,DAC.UserName DisciplinaryActionCategory,EA.DisciplinaryActionCategoryId
		,SUBSTRING(
			(
				SELECT ','+ DAC.LetterName  AS [text()]
				FROM  [dbo].[EmployeeDisciplinaryActionDetails] DAD 				
				LEFT JOIN [dbo].[DisciplinaryActionSettingChild] DAC on DAC.Id = DAD.LetterFormetId
				WHERE DAD.EmployeeDisciplinaryActionId = EA.Id			
				
				ORDER BY DAD.LetterFormetId
				FOR XML PATH ('')
			), 2, 1000) [Letters],Format(EA.EntryDate,'dd-MMM-yyyy') EntryDate,Format(dadm.NextLetterDueDate,'dd-MMM-yyyy') NextLetterDueDate,Format(dadm.LetterIssueDate,'dd-MMM-yyyy') LetterIssueDate,EA.Id EmployeeDisciplinaryActionId
        ,DueStatus =case when GETDATE()>Convert(date, dadm.NextLetterDueDate) then 'OVERDUE' 
							when GETDATE()=Convert(date, dadm.NextLetterDueDate) then 'DUE' 
							when DATEADD(day,7, GETDATE())=Convert(date, dadm.NextLetterDueDate) then 'TOBEDUE' 
					end
        ,dadm.DisciplinaryActionSettingDetailsId DisciplinaryActionSettingDetailsId,dadm.Id EmployeeDisciplinaryActionDetailsId
        ,DueLetter=(
		         SELECT 
			         --DAS.Id
			         --,DAS.DisciplinaryActionCategoryId
			         --,DAS.Sequence
			         --,DAS.LetterIssueDay
			         DAS.Description as UserName
			         From  DisciplinaryActionSettingDetails  DAS
			         where das.DisciplinaryActionCategoryId=EA.DisciplinaryActionCategoryId--'DACM2010'
			         and das.IsActive = 1 
			         and Sequence=(select Sequence+1 from DisciplinaryActionSettingDetails where id=dadm.DisciplinaryActionSettingDetailsId--'DASD2057'
			         )
		        )
		FROM HKP.EmployeeDisciplinaryAction EA
		LEFT JOIN [HKP].[DisciplinaryActionCategory] DAC on DAC.Id=EA.DisciplinaryActionCategoryId
		
        LEFT JOIN 
		(		
        select m.EmpSystemId,d.EmployeeDisciplinaryActionId,Max(ds.Sequence) Sequence,ds.DisciplinaryActionCategoryId
		from  [dbo].[EmployeeDisciplinaryActionDetails] d
		inner join HKP.EmployeeDisciplinaryAction m on m.Id=d.EmployeeDisciplinaryActionId
		left join  DisciplinaryActionSettingDetails ds on ds.Id=d.DisciplinaryActionSettingDetailsId
		group by m.EmpSystemId,d.EmployeeDisciplinaryActionId,ds.DisciplinaryActionCategoryId		
		) dadm2 on   dadm2.EmployeeDisciplinaryActionId=EA.Id  
		inner join  DisciplinaryActionSettingDetails ds2 on ds2.DisciplinaryActionCategoryId=dadm2.DisciplinaryActionCategoryId and ds2.Sequence=dadm2.Sequence		
	    inner join  [dbo].[EmployeeDisciplinaryActionDetails] dadm  on 	 dadm.DisciplinaryActionSettingDetailsId=ds2.id and dadm.EmployeeDisciplinaryActionId=dadm2.EmployeeDisciplinaryActionId

        LEFT JOIN EmployeeInformation E on E.SystemId = EA.EmpSystemId
        LEFT JOIN MST.ManpowerBudget MB ON MB.Id = E.BudgetCode
        LEFT JOIN ORG.Entity EN ON EN.Id = MB.EntityId
        LEFT JOIN ORG.Position PS on PS.Id = MB.PositionId
        LEFT JOIN[ORG].[Division] DIV ON DIV.Id = PS.DivisionId
        LEFT JOIN[ORG].[SubDivision] SDIV ON SDIV.Id = PS.SubdivisionID
        LEFT JOIN[ORG].[Department] DPT ON DPT.Id = PS.DepartmentId
        LEFT JOIN[ORG].[Section] SEC ON SEC.Id = PS.SectionId
        LEFT JOIN[ORG].[SubSection] SSEC ON SSEC.Id = PS.SubSectionId
        LEFT JOIN[HKP].[Designation] DEG ON DEG.Id = PS.DesignationID
        LEFT JOIN[HKP].[Designation] GDEG ON GDEG.Id = E.GivenDesignationId	
        where  ISNULL(ActionType,'') NOT IN ('TBS')  and e.PlantId='" + identity.PlantId + @"' and e.EmployeeStatus='Active' and dadm.NextLetterDueDate is null";
            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }








        [HttpGet, Authorize]
        public ActionResult GetAllDescription(string DisciplinaryActionCategoryId, string DisciplinaryActionSettingDetailsId, string EmployeeDisciplinaryActionDetailsId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string xsql = @"  select DAS.Id,DAS.DisciplinaryActionCategoryId,DAS.Sequence
							,DAS.LetterIssueDay,DAS.IsSeparable,DAS.Description as UserName
							,Separable=case when DAS.IsSeparable=1 then 'Yes' else 'No' END 
                            From  DisciplinaryActionSettingDetails  DAS
							where das.DisciplinaryActionCategoryId='" + DisciplinaryActionCategoryId + @"' and das.IsActive = 1 order by Sequence asc";




            string sql = @" select DAS.Id
                            ,DAS.DisciplinaryActionCategoryId
                            ,DAS.Sequence
                            ,DAS.LetterIssueDay
                            ,DAS.IsSeparable
                            ,DAS.Description as UserName
                            ,Separable=case when DAS.IsSeparable=1 then 'Yes' else 'No' END 
                            ,EntryDate=(
                            select FORMAT( EDA.EntryDate,'dd-MMM-yyyy') from [dbo].[EmployeeDisciplinaryActionDetails] EDAD
                            left join [HKP].[EmployeeDisciplinaryAction] EDA on EDA.Id=EDAD.EmployeeDisciplinaryActionId
                            Where EDAD.Id='" + EmployeeDisciplinaryActionDetailsId + @"'
                            )

                            ,NextLetterDueDate=(
                            select FORMAT(dateadd(day,Convert(int,


                            (select LetterIssueDay
							From  DisciplinaryActionSettingDetails  where DisciplinaryActionCategoryId='" + DisciplinaryActionCategoryId + @"'
                            and IsActive = 1 
                            and Sequence=(select Sequence+2 from DisciplinaryActionSettingDetails where id='" + DisciplinaryActionSettingDetailsId + @"')
							
							)


                            ),EDAD.NextLetterDueDate),'dd-MMM-yyyy') from [dbo].[EmployeeDisciplinaryActionDetails] EDAD
                            left join [HKP].[EmployeeDisciplinaryAction] EDA on EDA.Id=EDAD.EmployeeDisciplinaryActionId
                            Where EDAD.Id='" + EmployeeDisciplinaryActionDetailsId + @"'
                            )

                            ,LetterIssueDate=(
                            select FORMAT( EDAD.NextLetterDueDate,'dd-MMM-yyyy') from [dbo].[EmployeeDisciplinaryActionDetails] EDAD
                            left join [HKP].[EmployeeDisciplinaryAction] EDA on EDA.Id=EDAD.EmployeeDisciplinaryActionId
                            Where EDAD.Id='" + EmployeeDisciplinaryActionDetailsId + @"'
                            )
                            ,Count=(select Count(Id) from   DisciplinaryActionSettingDetails  where DisciplinaryActionCategoryId='" + DisciplinaryActionCategoryId + @"')
                            From  DisciplinaryActionSettingDetails  DAS
                            where das.DisciplinaryActionCategoryId='" + DisciplinaryActionCategoryId + @"'
                            and das.IsActive = 1 
                            and Sequence=(select Sequence+1 from DisciplinaryActionSettingDetails where id='" + DisciplinaryActionSettingDetailsId + @"')
                            ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetAllDescriptionForDA(string DisciplinaryActionCategoryId, string DisciplinaryActionSettingDetailsId, string EmployeeDisciplinaryActionDetailsId, string EmployeeDisciplinaryActionId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string xsql = @"  select DAS.Id,DAS.DisciplinaryActionCategoryId,DAS.Sequence
							,DAS.LetterIssueDay,DAS.IsSeparable,DAS.Description as UserName
							,Separable=case when DAS.IsSeparable=1 then 'Yes' else 'No' END 
                            From  DisciplinaryActionSettingDetails  DAS
							where das.DisciplinaryActionCategoryId='" + DisciplinaryActionCategoryId + @"' and das.IsActive = 1 order by Sequence asc";
            string sql = string.Empty;
            if (string.IsNullOrEmpty(DisciplinaryActionSettingDetailsId) || DisciplinaryActionSettingDetailsId=="null")
            {
                sql = @"select DAS.Id
                            ,DAS.DisciplinaryActionCategoryId
                            ,DAS.Sequence
                            ,DAS.LetterIssueDay
                            ,DAS.IsSeparable
                            ,DAS.Description as UserName
                            ,Separable=case when DAS.IsSeparable=1 then 'Yes' else 'No' END 
                            ,EntryDate=( select FORMAT( EntryDate,'dd-MMM-yyyy') from  [HKP].[EmployeeDisciplinaryAction]   Where Id='" + EmployeeDisciplinaryActionId + @"' )

                            ,NextLetterDueDate=(
							select 


FORMAT(dateadd(day,Convert(int,
                            (select LetterIssueDay
							From  DisciplinaryActionSettingDetails  where DisciplinaryActionCategoryId='" + DisciplinaryActionCategoryId + @"'
                            and IsActive = 1 
                            and Sequence=2							
							)
                            )
							,
--------------------------------------------------------
							(
							select FORMAT(dateadd(day,Convert(int,
                            (select LetterIssueDay
							From  DisciplinaryActionSettingDetails  where DisciplinaryActionCategoryId='" + DisciplinaryActionCategoryId + @"'
                            and IsActive = 1 
                            and Sequence=1							
							)
                            )
							,							
							EntryDate ),'dd-MMM-yyyy') 							
							from  [HKP].[EmployeeDisciplinaryAction] Where Id='" + EmployeeDisciplinaryActionId + @"'
                            )

---------------------------------------------------------




)


                            ,'dd-MMM-yyyy') 							
							--------from  [HKP].[EmployeeDisciplinaryAction] Where Id='" + EmployeeDisciplinaryActionId + @"'
                            




)



                            ,LetterIssueDate=(
							select FORMAT(dateadd(day,Convert(int,
                            (select LetterIssueDay
							From  DisciplinaryActionSettingDetails  where DisciplinaryActionCategoryId='" + DisciplinaryActionCategoryId + @"'
                            and IsActive = 1 
                            and Sequence=1							
							)
                            )
							,							
							EntryDate ),'dd-MMM-yyyy') 							
							from  [HKP].[EmployeeDisciplinaryAction] Where Id='" + EmployeeDisciplinaryActionId + @"'
                            )












                            ---,LetterIssueDate=(
                            ---select FORMAT( EntryDate,'dd-MMM-yyyy') from [HKP].[EmployeeDisciplinaryAction]   Where Id='" + EmployeeDisciplinaryActionId + @"'
                            ---)
                            ,Count=(select Count(Id) from   DisciplinaryActionSettingDetails  where DisciplinaryActionCategoryId='" + DisciplinaryActionCategoryId + @"')
                            From  DisciplinaryActionSettingDetails  DAS
                            where das.DisciplinaryActionCategoryId='" + DisciplinaryActionCategoryId + @"'
                            and das.IsActive = 1 
                            and Sequence=1 ";
            }
            else
            {




                sql = @" select DAS.Id
                            ,DAS.DisciplinaryActionCategoryId
                            ,DAS.Sequence
                            ,DAS.LetterIssueDay
                            ,DAS.IsSeparable
                            ,DAS.Description as UserName
                            ,Separable=case when DAS.IsSeparable=1 then 'Yes' else 'No' END 
                            ,EntryDate=(
                            select FORMAT( EDA.EntryDate,'dd-MMM-yyyy') from [dbo].[EmployeeDisciplinaryActionDetails] EDAD
                            left join [HKP].[EmployeeDisciplinaryAction] EDA on EDA.Id=EDAD.EmployeeDisciplinaryActionId
                            Where EDAD.Id='" + EmployeeDisciplinaryActionDetailsId + @"'
                            )

                            ,NextLetterDueDate=(
                            select FORMAT(dateadd(day,Convert(int,


                            (select LetterIssueDay
							From  DisciplinaryActionSettingDetails  where DisciplinaryActionCategoryId='" + DisciplinaryActionCategoryId + @"'
                            and IsActive = 1 
                            and Sequence=(select Sequence+2 from DisciplinaryActionSettingDetails where id='" + DisciplinaryActionSettingDetailsId + @"')
							
							)


                            ),EDAD.NextLetterDueDate),'dd-MMM-yyyy') from [dbo].[EmployeeDisciplinaryActionDetails] EDAD
                            left join [HKP].[EmployeeDisciplinaryAction] EDA on EDA.Id=EDAD.EmployeeDisciplinaryActionId
                            Where EDAD.Id='" + EmployeeDisciplinaryActionDetailsId + @"'
                            )

                            ,LetterIssueDate=(
                            select FORMAT( EDAD.NextLetterDueDate,'dd-MMM-yyyy') from [dbo].[EmployeeDisciplinaryActionDetails] EDAD
                            left join [HKP].[EmployeeDisciplinaryAction] EDA on EDA.Id=EDAD.EmployeeDisciplinaryActionId
                            Where EDAD.Id='" + EmployeeDisciplinaryActionDetailsId + @"'
                            )


                            ,Count=(select Count(Id) from   DisciplinaryActionSettingDetails  where DisciplinaryActionCategoryId='" + DisciplinaryActionCategoryId + @"')
                            From  DisciplinaryActionSettingDetails  DAS
                            where das.DisciplinaryActionCategoryId='" + DisciplinaryActionCategoryId + @"'
                            and das.IsActive = 1 
                            and Sequence=(select Sequence+1 from DisciplinaryActionSettingDetails where id='" + DisciplinaryActionSettingDetailsId + @"')
                            ";
            }
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetEmployeeDisciplinaryActionDetailsList(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT  DD.Description ActionName,DAC.LetterName,Format(DAD.NextLetterDueDate,'dd-MMM-yyyy') NextLetterDueDate,Format(DAD.LetterIssueDate,'dd-MMM-yyyy') LetterIssueDate,DAD.Id
                            FROM  [dbo].[EmployeeDisciplinaryActionDetails] DAD 
                            LEFT JOIN DisciplinaryActionSettingDetails DD on DD.Id=DAD.DisciplinaryActionSettingDetailsId
                            LEFT JOIN [dbo].[DisciplinaryActionSettingChild] DAC on DAC.Id = DAD.LetterFormetId
                            WHERE DAD.EmployeeDisciplinaryActionId = '" + Id + @"'
							ORDER BY DD.Sequence DESC ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
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
        [HttpPost]
        public ActionResult Save(LABS longAbsenteeism, DisciplinaryActionDetails disciplinaryActionDetails)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string LBSID = string.Empty;
                string DADID = string.Empty;
                LBSID = SaveLBS(longAbsenteeism);
                SaveLetterInformation(disciplinaryActionDetails, LBSID, out DADID);
                //string sql = @"update EmployeeInformation SET EmployeeDisciplinaryActionIdForLA = '" + LBSID + @"' where SystemId='" + longAbsenteeism.EmpSystemId + @"'";
                string sql = @"update EmployeeInformation SET EmployeeCurrentStatus='TBS',EmployeeCurrentStatusEffectiveDate='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"' ,EmployeeDisciplinaryActionIdForLA = '" + LBSID + @"' where SystemId='" + longAbsenteeism.EmpSystemId + @"'";
                ExecuteRawSQL(sql);

                if (GetDisciplinaryActionSettingDetails(disciplinaryActionDetails.DisciplinaryActionSettingDetailsId))
                {
                    string _id;
                    Resignation reg = new Resignation();

                    reg.CompanyGroupId = identity.CompanyGroupId;
                    reg.CompanyId = identity.CompanyId;
                    reg.PlantId = identity.PlantId;
                    reg.EmployeeId = longAbsenteeism.EmpSystemId;
                    reg.ResignationDate = longAbsenteeism.EntryDate;
                    reg.Reason = "From LA";
                    reg.DisciplinaryActionDetailsId = DADID;
                    //reg.AttachLetter = from_ui.AttachLetter;
                    reg.EffectiveDate = longAbsenteeism.EntryDate;
                    //reg.ApprovedEffectiveDate = from_ui.ApprovedEffectiveDate;
                    //reg.Remarks = from_ui.Remarks;
                    reg.AttachLetter = "NA";
                    reg.AddedBy = identity.Name;
                    reg.AddedFromIP = identity.IPAddress;
                    reg.AddedDate = DateTime.Now;

                    _ResignationService.Save(reg, out _id);
                }

                return Json(new { DADID, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string SaveLBS(LABS longAbsenteeism)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            string LBSID = string.Empty;

            try
            {
                string sql = "SELECT * FROM [HKP].[EmployeeDisciplinaryAction] WHERE EmpSystemId='" + longAbsenteeism.EmpSystemId + @"' AND DisciplinaryActionCategoryId='" + longAbsenteeism.DisciplinaryActionCategoryId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                string Id = string.Empty;

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[HKP].[EmployeeDisciplinaryAction]", out sID);
                    Id = "LA" + sID;
                    dr["Id"] = Id;
                    dr["EmpSystemId"] = longAbsenteeism.EmpSystemId;
                    dr["DisciplinaryActionCategoryId"] = longAbsenteeism.DisciplinaryActionCategoryId;
                    dr["Description"] = longAbsenteeism.Description;
                    dr["EntryDate"] = longAbsenteeism.EntryDate;
                    dr["ActionType"] = "LA";

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {

                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    Id = dr["ID"].ToString();
                    dr["EmpSystemId"] = longAbsenteeism.EmpSystemId;
                    dr["DisciplinaryActionCategoryId"] = longAbsenteeism.DisciplinaryActionCategoryId;
                    dr["Description"] = longAbsenteeism.Description;
                    dr["EntryDate"] = longAbsenteeism.EntryDate;
                    dr["ActionType"] = "LA";

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                return Id;

            }

            catch (Exception ex)
            {
                throw ex;
            }
        }




        [HttpPost,Authorize]
        public ActionResult SaveDAL(LABS longAbsenteeism, DisciplinaryActionDetails disciplinaryActionDetails)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string LBSID = string.Empty;
                string DADID = string.Empty;
                LBSID = SaveDAL(longAbsenteeism);
                SaveLetterInformation(disciplinaryActionDetails, LBSID, out DADID);
                //string sql = @"update EmployeeInformation SET EmployeeDisciplinaryActionIdForLA = '" + LBSID + @"' where SystemId='" + longAbsenteeism.EmpSystemId + @"'";
                string sql = @"update EmployeeInformation SET EmployeeCurrentStatus='TBS',EmployeeCurrentStatusEffectiveDate='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"' ,EmployeeDisciplinaryActionIdForLA = '" + LBSID + @"' where SystemId='" + longAbsenteeism.EmpSystemId + @"'";
                ExecuteRawSQL(sql);

                if (GetDisciplinaryActionSettingDetails(disciplinaryActionDetails.DisciplinaryActionSettingDetailsId))
                {
                    string _id;
                    Resignation reg = new Resignation();

                    reg.CompanyGroupId = identity.CompanyGroupId;
                    reg.CompanyId = identity.CompanyId;
                    reg.PlantId = identity.PlantId;
                    reg.EmployeeId = longAbsenteeism.EmpSystemId;
                    reg.ResignationDate = longAbsenteeism.EntryDate;
                    reg.Reason = "From LA";
                    reg.DisciplinaryActionDetailsId = DADID;
                    //reg.AttachLetter = from_ui.AttachLetter;
                    reg.EffectiveDate = longAbsenteeism.EntryDate;
                    //reg.ApprovedEffectiveDate = from_ui.ApprovedEffectiveDate;
                    //reg.Remarks = from_ui.Remarks;
                    reg.AttachLetter = "NA";
                    reg.AddedBy = identity.Name;
                    reg.AddedFromIP = identity.IPAddress;
                    reg.AddedDate = DateTime.Now;

                    _ResignationService.Save(reg, out _id);
                }

                return Json(new { DADID, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string SaveDAL(LABS longAbsenteeism)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            string LBSID = string.Empty;

            try
            {
                string sql = "SELECT * FROM [HKP].[EmployeeDisciplinaryAction] WHERE EmpSystemId='" + longAbsenteeism.EmpSystemId + @"' AND DisciplinaryActionCategoryId='" + longAbsenteeism.DisciplinaryActionCategoryId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                string Id = string.Empty;

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[HKP].[EmployeeDisciplinaryAction]", out sID);
                    Id = "LA" + sID;
                    dr["Id"] = Id;
                    dr["EmpSystemId"] = longAbsenteeism.EmpSystemId;
                    dr["DisciplinaryActionCategoryId"] = longAbsenteeism.DisciplinaryActionCategoryId;
                    dr["Description"] = longAbsenteeism.Description;
                    dr["EntryDate"] = longAbsenteeism.EntryDate;
                    //dr["ActionType"] = "LA";

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {

                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    Id = dr["ID"].ToString();
                    dr["EmpSystemId"] = longAbsenteeism.EmpSystemId;
                    dr["DisciplinaryActionCategoryId"] = longAbsenteeism.DisciplinaryActionCategoryId;
                    dr["Description"] = longAbsenteeism.Description;
                    dr["EntryDate"] = longAbsenteeism.EntryDate;
                    //dr["ActionType"] = "LA";

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                return Id;

            }

            catch (Exception ex)
            {
                throw ex;
            }
        }





        public bool GetDisciplinaryActionSettingDetails(string Id)
        {

            ConnectionManager.DAL.ConManager objCon;

            try
            {
                bool OutPara = false;
                DataSet dsMaster;
                string sql = "SELECT IsSeparable FROM [dbo].[DisciplinaryActionSettingDetails] where Id='" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    OutPara = Convert.ToBoolean(dsMaster.Tables[0].Rows[0]["IsSeparable"]);
                }


                return OutPara;

            }

            catch (Exception ex)
            {
                throw ex;
            }
        }




        public void SaveLetterInformation(DisciplinaryActionDetails disciplinaryActionDetails, string LBSID, out string DADID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            DADID = string.Empty;
            try
            {
                string sql = "SELECT * FROM EmployeeDisciplinaryActionDetails WHERE Id='" + disciplinaryActionDetails.DADID + @"' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "EmployeeDisciplinaryActionDetails", out sID);
                    dr["Id"] = "DAD" + sID;
                    DADID = dr["Id"].ToString();
                    dr["EmployeeDisciplinaryActionId"] = LBSID;
                    dr["LetterFormetId"] = disciplinaryActionDetails.LettersFormat;
                    if (!string.IsNullOrEmpty(disciplinaryActionDetails.NextLetterDueDate))
                    {
                        dr["NextLetterDueDate"] = disciplinaryActionDetails.NextLetterDueDate;
                    }

                    dr["LetterIssueDate"] = disciplinaryActionDetails.LetterIssueDate;
                    dr["DisciplinaryActionSettingDetailsId"] = disciplinaryActionDetails.DisciplinaryActionSettingDetailsId;
                    dr["DisciplinaryActionCategoryId"] = disciplinaryActionDetails.DisciplinaryActionCategoryId;



                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    DADID = dr["ID"].ToString();

                    dr["EmployeeDisciplinaryActionId"] = LBSID;
                    dr["LetterFormetId"] = disciplinaryActionDetails.LettersFormat;

                    if (!string.IsNullOrEmpty(disciplinaryActionDetails.NextLetterDueDate))
                    {
                        dr["NextLetterDueDate"] = disciplinaryActionDetails.NextLetterDueDate;
                    }
                    dr["LetterIssueDate"] = disciplinaryActionDetails.LetterIssueDate;
                    dr["DisciplinaryActionSettingDetailsId"] = disciplinaryActionDetails.DisciplinaryActionSettingDetailsId;
                    dr["DisciplinaryActionCategoryId"] = disciplinaryActionDetails.DisciplinaryActionCategoryId;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #region Report

        [HttpGet, Authorize]
        public ActionResult DisciplinaryActionLetterInMSWord(string EmployeeDisciplinaryActionDetailsId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;

            ReportUtility oRU = new ReportUtility();
            string File = "";
            string strPath = "";
            var fileName = "";
            string LetterLanguageId = "";
            string EmpSystemId = "";
            string LetterLanguage = "";


            DataSet dsMaster;
            string sql = @"SELECT D.Id
                         ,Format(D.LetterIssueDate, 'dd-MMM-yyyy') LetterIssueDate
                         ,Format(D.NextLetterDueDate, 'dd-MMM-yyyy') NextLetterDueDate
                         ,Format(M.EntryDate, 'dd-MMM-yyyy') EntryDate,DATEDIFF(Day,M.EntryDate,D.LetterIssueDate)+1 NumberOfAbsentDays
                         ,M.EmpSystemId
                         ,M.Id CaseNo ,sd1.Sequence LetterNo
                         ,dasc.LetterLanguage
                         ,DASC.LetterFormat
                         ,Format(D1.LetterIssueDate, 'dd-MMM-yyyy')  OtherLetterIssueDate
                         ,sd.Sequence
                         ,l.UserName LetterLanguageName
                        ,EI.EmployeeCode,EI.EmployeeName,LD.UserName Designation
                        ,ISNULL(LL.Name,Dep.UserName) Department
                         FROM[dbo].[EmployeeDisciplinaryActionDetails] D
                         LEFT JOIN [HKP].[EmployeeDisciplinaryAction] M ON M.Id=D.EmployeeDisciplinaryActionId
                         LEFT JOIN [dbo].[EmployeeDisciplinaryActionDetails] D1 on  M.Id=D1.EmployeeDisciplinaryActionId
                         LEFT JOIN [dbo].[DisciplinaryActionSettingChild] DASC ON D.LetterFormetId=DASC.Id
                         LEFT JOIN [dbo].[DisciplinaryActionSettingDetails] sd on sd.Id=d1.DisciplinaryActionSettingDetailsId
                         LEFT JOIN [dbo].[DisciplinaryActionSettingDetails] sd1 on sd1.Id=d.DisciplinaryActionSettingDetailsId
                         LEFT JOIN SCS.Language l on l.Id=DASC.LetterLanguage
                         left join EmployeeInformation EI on EI.SystemId=M.EmpSystemId
						 left join [HKP].[LegalDesignation] LD on LD.Id=EI.LegalDesignationId
						 left join [ORG].[Department] Dep on Dep.Id=EI.DepartmentId
                        left join [HKP].[LocalLanguage] LL on LL.DepartmentId=Dep.Id
                         WHERE D.Id='" + EmployeeDisciplinaryActionDetailsId + @"' 
                         ORDER BY sd.Sequence ";
            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
            if (dsMaster.Tables[0].Rows.Count > 0)
            {
                LetterLanguageId = dsMaster.Tables[0].Rows[0]["LetterLanguage"].ToString();
                LetterLanguage = dsMaster.Tables[0].Rows[0]["LetterLanguageName"].ToString();
                EmpSystemId = dsMaster.Tables[0].Rows[0]["EmpSystemId"].ToString();
                fileName = dsMaster.Tables[0].Rows[0]["LetterFormat"].ToString();
            }
            else
            {
                Exception ex = new Exception("No data found....");
                throw (ex);
            }

            try
            {





                if (!string.IsNullOrEmpty(fileName))
                {
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), fileName);
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File Not Found");
                    }
                }



                FileInfo DocFile = new FileInfo(strPath);
                if (DocFile.Exists == false)
                {

                    throw new CustomException("File Not Found");
                }

                DataTable dtEmp = GetEmployeeBasicInfoById(EmpSystemId, identity.PlantId, "", LetterLanguageId);

                WordDocument document = new WordDocument(DocFile.FullName);

                TextSelection[] X = document.FindAll(new Regex("{.*?}")).ToArray();
                List<string> allresult = new List<string>();
                for (int i = 0; i < X.Length; i++)
                    allresult.Add(X[i].SelectedText);


                Dictionary<string, int> replaced = new Dictionary<string, int>();

                string value = "";
                for (int i = 0; i < allresult.Count; i++)
                {
                    try
                    {
                        string foundText = allresult[i];

                        if (replaced.ContainsKey(foundText) == false)
                            replaced.Add(foundText, 0);

                        string colName = foundText.Trim().Replace("{", "").Replace("}", "");
                        if (dtEmp.Columns.Contains(colName))
                        {

                            value = dtEmp.Rows[0][dtEmp.Columns[colName].ColumnName].ToString();
                            if (bplib.clsWebLib.IsNumeric(value))
                                replaced[foundText] = document.Replace(foundText, cnDgt(value, LetterLanguage), false, true);
                            else if (bplib.clsWebLib.IsDateOK(value))
                                replaced[foundText] = document.Replace(foundText, GetFormatedDate(value, LetterLanguage), false, true);
                            else
                                replaced[foundText] = document.Replace(foundText, value, false, false);
                        }
                    }
                    catch (Exception)
                    {
                    }

                }


                document.Replace("{Date}", GetFormatedDate(dsMaster.Tables[0].Rows[0]["LetterIssueDate"].ToString(), LetterLanguage), false, true);
                document.Replace("{CaseNo}", dsMaster.Tables[0].Rows[0]["CaseNo"].ToString(), false, true);
                document.Replace("{LetterNo}", dsMaster.Tables[0].Rows[0]["LetterNo"].ToString(), false, true);
                document.Replace("{NumberOfAbsentDays}", cnDgt(dsMaster.Tables[0].Rows[0]["NumberOfAbsentDays"].ToString(), LetterLanguage), false, true);
                document.Replace("{IncidenceDate}", GetFormatedDate(dsMaster.Tables[0].Rows[0]["EntryDate"].ToString(), LetterLanguage), false, true);
                document.Replace("{LetterIssueDate}", GetFormatedDate(dsMaster.Tables[0].Rows[0]["LetterIssueDate"].ToString(), LetterLanguage), false, true);
                document.Replace("{EntryDate}", dsMaster.Tables[0].Rows[0]["EntryDate"].ToString(), false, true);
                document.Replace("{Sequence}", dsMaster.Tables[0].Rows[0]["Sequence"].ToString(), false, true);
                document.Replace("{EmployeeCode}", dsMaster.Tables[0].Rows[0]["EmployeeCode"].ToString(), false, true);
                document.Replace("{Designation}", dsMaster.Tables[0].Rows[0]["Designation"].ToString(), false, true);
                document.Replace("{Department}", dsMaster.Tables[0].Rows[0]["Department"].ToString(), false, true);


                switch (Convert.ToInt32(dsMaster.Tables[0].Rows[0]["LetterNo"].ToString()))
                {
                    //case 1:
                    //    document.Replace("{IncidenceDate}", GetFormatedDate(dsMaster.Tables[0].Rows[0]["LetterIssueDate"].ToString(), LetterLanguageId), false, true);
                    //    break;
                    case 2:
                        document.Replace("{FirstLetterIssueDate}", GetFormatedDate(dsMaster.Tables[0].Rows[0]["OtherLetterIssueDate"].ToString(), LetterLanguage), false, true);
                        break;
                    case 3:
                        document.Replace("{FirstLetterIssueDate}", GetFormatedDate(dsMaster.Tables[0].Rows[0]["OtherLetterIssueDate"].ToString(), LetterLanguage), false, true);
                        document.Replace("{SecondLetterIssueDate}", GetFormatedDate(dsMaster.Tables[0].Rows[1]["OtherLetterIssueDate"].ToString(), LetterLanguage), false, true);
                        break;
                    default:
                        // code block
                        break;
                }




                foreach (string item in replaced.Keys)
                {
                    if (replaced[item] == 0)
                        document.Replace(item, "", false, true);

                }
                //for (int ROW = 0; ROW < dtSalary.Rows.Count; ROW++)
                //{
                //    int isReplaced = 0;

                //    isReplaced = document.Replace("{" + dtSalary.Rows[ROW]["SalaryHead"].ToString() + "}", cnDgt(dtSalary.Rows[ROW]["EntryAmount"].ToString(), tempId), false, false);





                //}





                string fileNames = string.Empty;
                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["EmployeeCode"].ToString()))
                {
                    fileNames = dtEmp.Rows[0]["EmployeeCode"].ToString() + "DALetter.docx";
                }
                else
                {
                    fileNames = "DA.docx";
                }

                document.Save(fileNames, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }






            //DisciplinaryActionLetterInMSWordFun(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, empId, empType, reportType, tempId);




            return View();

        }





        private void DisciplinaryActionLetterInMSWordFun(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)
        {


        }


        public DataTable GetEmployeeBasicInfoById(string employeeId, string plantId, string employeementType, string languageId)
        {
            try
            {

                string sql = @" SELECT TOP 1 EmployeeCode,  
                           TAB3.EmployeeName,TAB3.FatherName,TAB3.MotherName,TAB3.ParmanentAddress, TAB3.PresentAddress,
                            ISNULL(LocalCompanyName, CompanyName) CompanyName,
                            ISNULL(CompanyAddress, CompanyAddress) CompanyAddress,
                            ISNULL(UtilityName, UtilityName) UtilityName,
                            ISNULL(PresentCity, PresentCity) PresentCity,							
                            ISNULL(PresentDistrict, PresentDistrict) PresentDistrict,
                            ISNULL(PresentState, PresentState) PresentState,
                            ISNULL(LPresentCountry, LPermanentCountry) LPresentCountry,
                            ISNULL(FirstName, FirstName) FirstName,
                            ISNULL(LegalDesignationLocal, LegalDesignation) DesignationName,
                            ISNULL(LocalDepartmentName1, Department) Department,
                            ISNULL(UnitLocal, Unit) Unit,
                            ISNULL(DateOfJoin, DateOfJoin) DOJ,
                            ISNULL(DateOfJoin, DateOfJoin) DateOfJoin,
                            ISNULL(DateOfBirth, DateOfBirth) DOB,
                            ISNULL(confirm, confirm) ProbationPeriod,
                            ISNULL(MobileNo, MobileNo) MobileNo,                            
                            ISNULL(SectionName, Section) Section,                          
                            ISNULL(DOC, DOC) DOC,
                            ISNULL(NationalID, NationalID) NationalID,
                            ISNULL(BloodGroup, BloodGroup) BloodGroup,
                            ISNULL(EmployeePic, EmployeePic) EmployeePic,AppliedDate,DateOfBirth,SpouseName,EmploymentTypelocal
							,ProbationerName, fEm,SectionName, LocalDepartmentName1
                            ,ISNULL(GradeLocal, Grade) Grade
                            ,IssueDate,NomineeName,NomineeAddress,NomineeNID,NomineeDOB,NomineeRelation,Gender,IdentificationMark,NomineeAge
                            ,ISNULL(PlantLocal, Plant) PlantName
                            ,ISNULL(LineLocal, Line) Line
                                    FROM(SELECT TAB2.*, AM.Phone, AM.Email, AM.Website, AM.Address1 FROM
                                    --tab2
                                    (SELECT TAB1.*, LAN.StandardName
                                    FROM(SELECT CM.Image CompanyLogo, E.SystemID as EmpSystemID,
                                    CM.UserName CompanyName, AM.Address1 CompanyAddress, E.EmpPicPath EmployeePic, E.EmployeeCode, Convert(varchar, E.DOJ, 105) DOJ,
                                    REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DateOfJoin, BG.UserName BloodGroup, REPLACE(CONVERT(VARCHAR(11), E.DOB, 106), ' ', '-') DateOfBirth
                                    , E.NationalID, E.EmploymentType, D.UserName DesignationName, dm.EmployeeCategoryId, ec.UserName EmployeeCategory, L.UserName Line,
                                    E.EmpSignature CardHolderSignature, P.AuthorizedSignature
                                    , E.CellPhnNo MobileNo, DP.UserName Department, SE.UserName Section, A.[Name] LocalCompanyName, B.[Name] LocalDesignationName, C.[Name] LocalDepartmentName,
                                    N.Name NameLabel
                                    , DN.Name DesignationLabel, DPN.Name DepartmentLabel, LN.Name LineLabel, LET.Name EmploymentTypeLabel, ID.Name IDNoLabel, PT.Name EmploymentTypeName,
                                    DJ.Name DOJLabel, ET.Name EmergencyTellNoLabel, BGP.Name BloodGroupLabel
                                    , E.EmployeeNameLocal, LL.UtilityName, NID.Name NIDLabel, LMB.Name MobileNoLabel,
                                    LD.Name LegalDesignationLocal, SEC.Name SectionName, CAC.[Name] LocalDepartmentName1
                                    , GD.ShortName Grade, LSGA.Name GradeLocal
                                    , Convert(varchar, DATEADD(year, 5, E.DOJ), 105) AS Validity, LNN.Name LineLocal, UN.Username Unit, LUN.[Name] UnitLocal, Convert(varchar, E.DOC, 105) DOC, FORMAT(E.AppliedDate, 'dd-MMM-yyyy') AppliedDate
                                    , PCN.Name LPermanentCountry, PRCN.Name LPresentCountry
                                    , PD.Name PermanentDistrict, PRD.Name PresentDistrict, PST.Name PermanentState, PRST.Name PresentState, PCT.Name PermanentCity, PRCT.Name PresentCity
                                    , CASE WHEN DOCDay = 0 THEN DOCMonth ELSE DOCDay / 30 END AS confirm, PL.LanguageId, PL.Id as 'PlantId', CM.AddressMasterId, E.FirstName, LDN.UserName LegalDesignation, ISNULL(E.SpouseNameLocal, E.SpouseName) SpouseName, ISNULL(LET.Name, E.EmploymentType) EmploymentTypelocal
                                    , LPRL.Name ProbationerName, PT.Name fEm, FORMAT(IssueDate, 'dd-MMM-yyyy') IssueDate,
                                       										
										case when isnull(cg.Id, '') = '' THEN isnull(E.EmployeeNameLocal, E.EmployeeName) ELSE EmployeeName END AS EmployeeName
                                        ,case when isnull(cg.Id, '') = '' THEN isnull(E.FatherNameLocal, E.FatherName) ELSE FatherName END AS FatherName
                                        ,case when isnull(cg.Id, '') = '' THEN isnull(E.MotherNameLocal, E.MotherName) ELSE MotherName END AS MotherName
                                        ,case when isnull(cg.Id, '') = '' THEN isnull(E.ParmanentAddress1Local, E.ParmanentAddress1) ELSE ParmanentAddress1 END AS ParmanentAddress
                                        ,case when isnull(cg.Id, '') = '' THEN isnull(E.PresentAddress1Local, E.PresentAddress1) ELSE PresentAddress1 END AS PresentAddress


                                       ,case when isnull(cg.Id, '') = '' THEN isnull(Case When  GenderID = 'Male' then  LMM.Name else LMF.Name end, E.GenderID) ELSE GenderID END AS Gender
                                        ,case when isnull(cg.Id, '') = '' THEN isnull(E.LocalIdentificationMark, E.IdentificationMark) ELSE IdentificationMark END AS IdentificationMark
                                        ,case when isnull(cg.Id, '') = '' THEN isnull(NomineeInfo.localName, NomineeInfo.Name) ELSE NomineeInfo.Name END AS NomineeName
                                        ,case when isnull(cg.Id, '') = '' THEN isnull(NomineeInfo.AddressLocal, NomineeInfo.Address) ELSE Address END AS NomineeAddress

                                        , NomineeInfo.NationalID NomineeNID, FORMAT(NomineeInfo.DOB, 'dd-MMM-yyyy') NomineeDOB, isnull(cast((DATEDIFF(m, NomineeInfo.DOB, GETDATE()) / 12) as varchar), 0) NomineeAge, LNomR.Name NomineeRelation
                                        , PL.UserName Plant, PLL.Name PlantLocal

                                        from EmployeeInformation E

                                    LEFT JOIN org.CompanyGroup  CG on e.GroupID = cg.Id and CG.LanguageId = '" + languageId + @"'

                                    LEFT JOIN ORG.Company CM ON CM.Id = E.CompanyId
                                    LEFT JOIN MST.AddressMaster AM ON AM.Id = CM.AddressMasterId
                                    LEFT JOIN HKP.BloodGroup BG ON BG.Id = E.BloodGroupID
                                    LEFT JOIN MST.ManpowerBudget bbb ON e.BudgetCode = bbb.Id

                                    LEFT JOIN ORG.Position PS ON PS.Id = bbb.PositionId
                                    LEFT JOIN HKP.Designation D ON D.Id = E.GivenDesignationId
                                    LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId = e.GivenDesignationId
                                    LEFT JOIN  hkp.LegalDesignation LDN ON LDN.Id = E.LegalDesignationId
                                    LEFT JOIN HKP.EmployeeCategory EC ON EC.Id = DM.EmployeeCategoryId
                                    LEFT JOIN ORG.Line L ON L.Id = E.LineId
                                    LEFT JOIN ORG.Unit UN ON UN.Id = E.UnitId

                                    LEFT JOIN[SCS].[PlantSetting] P ON P.PlantId = E.PlantId
                                    LEFT JOIN ORG.Department DP ON DP.Id = PS.DepartmentId
                                    LEFT JOIN org.Section SE ON SE.Id = PS.SectionId

                                    LEFT JOIN ORG.Plant PL ON PL.Id = E.PlantId
                                    left Join MST.PayrollGroupMaster PGM on PGM.EmployeeId = E.EmployeeId
                                    LEFT JOIN EmployeeNomineeInfo NomineeInfo ON NomineeInfo.EmpSystemId = E.SystemId------NomineeInfo
                                    LEFT JOIN(
                                            SELECT LSGD.PlantId, LSGD.LegalDesignationId, LS.ShortName, LSGD.LegalSalaryGradeId from[MST].[LegalSalaryGradeDesignation] LSGD
                                            LEFT JOIN[SCS].[LegalSalaryGrade] LS ON LS.Id = LSGD.LegalSalaryGradeId
                                                ) GD ON GD.PlantId = E.PlantId AND GD.LegalDesignationId = E.LegalDesignationId

                                    LEFT JOIN HKP.LocalLanguage LSGA ON LSGA.LegalSalaryGradeId = GD.LegalSalaryGradeId AND LSGA.LanguageId = '" + languageId + @"'

                                    LEFT JOIN HKP.LocalLanguage A ON A.CompanyId = E.CompanyId AND A.LanguageId = '" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage LL ON LL.CompanyId = E.CompanyId AND LL.LanguageId = '" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage PLL ON PLL.PlantId = E.PlantId AND PLL.LanguageId = '" + languageId + @"'

                                    LEFT JOIN HKP.LocalLanguage B ON B.DesignationId = E.GivenDesignationId AND PL.LanguageId = '" + languageId + @"'

                                    LEFT JOIN HKP.LocalLanguage C ON C.DepartmentId = PS.DepartmentId AND PL.LanguageId = '" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage LD ON LD.LegalDesignationId = E.LegalDesignationId AND PL.LanguageId = '" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage LNN ON LNN.LineId = E.LineId AND PL.LanguageId = '" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage PCN ON PCN.CountryId = E.ParmCountryID AND PL.LanguageId = '" + languageId + @"'

                                    LEFT JOIN HKP.LocalLanguage PRCN ON PRCN.CountryId = E.ParmCountryID AND PL.LanguageId = '" + languageId + @"'

                                    LEFT JOIN HKP.LocalLanguage PD ON PD.DistrictId = E.ParmDistrictID AND PL.LanguageId = '" + languageId + @"'

                                    LEFT JOIN HKP.LocalLanguage PRD ON PRD.DistrictId = E.PresDistrictID AND PL.LanguageId = '" + languageId + @"'

                                    LEFT JOIN HKP.LocalLanguage PST ON PST.StateId = E.ParmStateId AND PL.LanguageId = '" + languageId + @"'

                                    LEFT JOIN HKP.LocalLanguage PRST ON PRST.StateId = E.PresStateId AND PL.LanguageId = '" + languageId + @"'

                                    LEFT JOIN HKP.LocalLanguage PCT ON PCT.CityId = E.ParmCityID AND PL.LanguageId = '" + languageId + @"'

                                    LEFT JOIN HKP.LocalLanguage PRCT ON PRCT.CityId = E.PresCityID AND PL.LanguageId = '" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage LUN ON LUN.UnitId = E.UnitId AND PL.LanguageId = '" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage LNomR ON LNomR.RelationshipId = NomineeInfo.Relation AND PL.LanguageId = '" + languageId + @"'

                                    LEFT JOIN(SELECT LanguageId, Name FROM HKP.LocalLanguage WHERE LabelName = 'Male'and LanguageId = '" + languageId + @"') LMM ON LMM.LanguageId = PL.LanguageId
                                    LEFT JOIN(SELECT LanguageId, Name FROM HKP.LocalLanguage WHERE LabelName = 'Female'and LanguageId = '" + languageId + @"') LMF ON LMF.LanguageId = PL.LanguageId

                                    LEFT JOIN(SELECT LanguageId, Name FROM HKP.LocalLanguage WHERE LabelName = (SELECT EmploymentType FROM dbo.EmployeeInformation where SystemId = '" + employeeId + @"')and LanguageId = '" + languageId + @"') LET ON LET.LanguageId = PL.LanguageId
                                    LEFT JOIN(SELECT LanguageId, Name FROM HKP.LocalLanguage WHERE LabelName = 'Name' and LanguageId = '" + languageId + @"') N ON N.LanguageId = PL.LanguageId
                                    LEFT JOIN(SELECT LanguageId, Name FROM HKP.LocalLanguage wHERE LabelName = 'Designation'and LanguageId = '" + languageId + @"') DN ON DN.LanguageId = PL.LanguageId
                                    LEFT JOIN(SELECT LanguageId, Name FROM HKP.LocalLanguage wHERE LabelName = 'Department'and LanguageId = '" + languageId + @"') DPN ON DPN.LanguageId = PL.LanguageId
                                    LEFT JOIN(SELECT LanguageId, Name FROM HKP.LocalLanguage wHERE LabelName = 'Line'and LanguageId = '" + languageId + @"') LN ON LN.LanguageId = PL.LanguageId

                                    LEFT JOIN(SELECT LanguageId, Name FROM HKP.LocalLanguage WHERE LabelName = 'IDNo'and LanguageId = '" + languageId + @"') ID ON ID.LanguageId = PL.LanguageId

                                    LEFT JOIN(SELECT LanguageId, Name FROM HKP.LocalLanguage WHERE LabelName = 'Permanent'and LanguageId = '" + languageId + @"') PT ON PT.LanguageId = PL.LanguageId

                                    LEFT JOIN(SELECT LanguageId, Name FROM HKP.LocalLanguage WHERE LabelName = 'DOJ'and LanguageId = '" + languageId + @"') DJ ON DJ.LanguageId = PL.LanguageId

                                    LEFT JOIN(SELECT LanguageId, Name FROM HKP.LocalLanguage WHERE LabelName = 'EmergencyTelNo'and LanguageId = '" + languageId + @"') ET ON ET.LanguageId = PL.LanguageId

                                    LEFT JOIN(SELECT LanguageId, Name FROM HKP.LocalLanguage WHERE LabelName = 'BloodGroup'and LanguageId = '" + languageId + @"') BGP ON BGP.LanguageId = PL.LanguageId

                                    LEFT JOIN(SELECT LanguageId, Name FROM HKP.LocalLanguage WHERE LabelName = 'NIDNo'and LanguageId = '" + languageId + @"') NID ON BGP.LanguageId = PL.LanguageId

                                    LEFT JOIN(SELECT LanguageId, Name FROM HKP.LocalLanguage WHERE LabelName = 'Permanent'and LanguageId = '" + languageId + @"') PML ON PML.LanguageId = PL.LanguageId

                                    LEFT JOIN(SELECT LanguageId, Name FROM HKP.LocalLanguage WHERE LabelName = 'Address'and LanguageId = '" + languageId + @"') LA ON LA.LanguageId = PL.LanguageId
                                    LEFT JOIN(SELECT LanguageId, Name FROM HKP.LocalLanguage WHERE LabelName = 'MobileNo'and LanguageId = '" + languageId + @"') LMB ON LMB.LanguageId = PL.LanguageId

                                    LEFT JOIN(SELECT LanguageId, Name FROM HKP.LocalLanguage WHERE LabelName = 'Probationer'and LanguageId = '" + languageId + @"') LPRL ON LPRL.LanguageId = PL.LanguageId

                                    LEFT JOIN(SELECT LanguageId, Name FROM HKP.LocalLanguage WHERE LabelName = 'Permanent' and LanguageId = '" + languageId + @"') PTl ON PTl.LanguageId = PL.LanguageId

                                    LEFT JOIN HKP.LocalLanguage SEC ON SEC.SectionId = PS.SectionId AND PL.LanguageId = SEC.LanguageId  AND PL.LanguageId = '" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage CAC ON CAC.DepartmentId = E.DepartmentId AND PL.LanguageId = CAC.LanguageId  AND PL.LanguageId = '" + languageId + @"'
                                    WHERE E.SystemID = '" + employeeId + @"') TAB1 LEFT JOIN SCS.Language AS LAN ON LAN.Id = TAB1.LanguageId) TAB2 LEFT JOIN MST.AddressMaster AS AM ON AM.Id = TAB2.AddressMasterId) TAB3
                                   ---LEFT JOIN(SELECT * FROM SCS.RptConfigTemplate WHERE Id = ''  and PlantId = '" + plantId + @"') AS RPTM ON TAB3.PlantId = RPTM.PlantId";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }



        public string cnDgt(string input, string lng)
        {
            if (lng == "Bengali")
            {
                return input.Replace('0', '০')
                    .Replace('1', '১')
                    .Replace('2', '২')
                    .Replace('3', '৩')
                    .Replace('4', '৪')
                    .Replace('5', '৫')
                    .Replace('6', '৬')
                    .Replace('7', '৭')
                    .Replace('8', '৮')
                    .Replace('9', '৯');
            }
            else if (lng == "Hindi")
            {
                return input.Replace('0', '०')
                    .Replace('1', '१')
                    .Replace('2', '२')
                    .Replace('3', '३')
                    .Replace('4', '४')
                    .Replace('5', '५')
                    .Replace('6', '६')
                    .Replace('7', '७')
                    .Replace('8', '८')
                    .Replace('9', '९');
            }
            else if (lng == "English")
            {
                return input.Replace('0', '0')
                    .Replace('1', '1')
                    .Replace('2', '2')
                    .Replace('3', '3')
                    .Replace('4', '4')
                    .Replace('5', '5')
                    .Replace('6', '6')
                    .Replace('7', '7')
                    .Replace('8', '8')
                    .Replace('9', '9');
            }
            return input;
        }

        public string ChangeMonth(string input, string lng)
        {
            if (lng == "Bengali")
            {
                return input
                     //.Replace("Jan", "জানুয়ারি")
                     //.Replace("Feb", "ফেব্রুয়ারি")
                     //.Replace("Mar", "মার্চ")
                     //.Replace("Apr", "এপ্রিল")
                     //.Replace("May", "মে")
                     //.Replace("Jun", "জুন")
                     //.Replace("Jul", "জুলাই")
                     //.Replace("Aug", "আগস্ট")
                     //.Replace("Sep", "সেপ্টেম্বর")
                     //.Replace("Oct", "অক্টোবর")
                     //.Replace("Nov", "নভেম্বর")
                     //.Replace("Dec", "ডিসেম্বর");
                     .Replace("Jan", "জানু")
                    .Replace("Feb", "ফেব্রু")
                    .Replace("Mar", "মার্চ")
                    .Replace("Apr", "এপ্রিল")
                    .Replace("May", "মে")
                    .Replace("Jun", "জুন")
                    .Replace("Jul", "জুলাই")
                    .Replace("Aug", "আগস্ট")
                    .Replace("Sep", "সেপ্টে")
                    .Replace("Oct", "অক্টো")
                    .Replace("Nov", "নভে")
                    .Replace("Dec", "ডিসে");
            }
            else if (lng == "Hindi")
            {
                return input
                    .Replace("Jan", "जनवरी")
                    .Replace("Feb", "फरवरी")
                    .Replace("Mar", "मार्च")
                    .Replace("Apr", "अप्रैल")
                    .Replace("May", "मई")
                    .Replace("Jun", "जून")
                    .Replace("Jul", "जुलाई")
                    .Replace("Aug", "अगस्त")
                    .Replace("Sep", "सितम्बर")
                    .Replace("Oct", "अक्तूबर")
                    .Replace("Nov", "नवम्बर")
                    .Replace("Dec", "दिसम्बर");
            }
            return input;
        }

        public string GetFormatedDate(string date, string lng)
        {
            var formateDate = string.Empty;
            var day = cnDgt(date.Substring(0, 2), lng);
            var mon = ChangeMonth(date.Substring(3, 3), lng);
            var year = cnDgt(date.Substring(7, 4), lng);
            return formateDate = day + "-" + mon + "-" + year;
        }
        private DataTable getLanguageId(string username)
        {
            try
            {
                var sql = @"Select Id from SCS.Language where UserName ='" + username.Replace("\r\n", "").Trim() + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable getLanguageName(string Id)
        {
            try
            {
                var sql = @"Select UserName from SCS.Language where Id ='" + Id + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

    }
}