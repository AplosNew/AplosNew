#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.HumanResources;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.HumanResources
{
    public class EmployeeDisciplinaryActionService : Service<EmployeeDisciplinaryAction>, IEmployeeDisciplinaryActionService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;

        public EmployeeDisciplinaryActionService(
            IRepositoryAsync<EmployeeDisciplinaryAction> EmployeeDisciplinaryActionRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(EmployeeDisciplinaryActionRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _pkGeneratorService = pkGeneratorService;
        }

        #endregion Constructor


        private string GetPK()
        {
            return GetAutoNumber(nameof(EmployeeDisciplinaryAction), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Insert(EmployeeDisciplinaryAction entity)
        {
            try
            {
                entity.Id = "EDA-" + GetPK();
               
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public override void Update(EmployeeDisciplinaryAction entity)
        {
            try
            {
           
             base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> Query(string EmpId)
        {
            try
            {
                 var CmdText = @"SELECT EDA.Id	
                                ,EDA.Id	CaseNo
                                ,EDA.EmpSystemId	
                                ,EDA.DisciplinaryActionCategoryId	
                                ,EDA.Description	
                                ---,EDA.EntryDate	
                                ,EDA.ActionType	
                                ,EDA.IsDACompleted	
                                ,EDA.DACompeletedBy	
                                ,EDA.DACompeletionDate	
                                ,EDA.DACompeletionRemark
                                ,E.EmployeeCode,E.EmployeeName
                                ,DAC.UserName DisciplinaryActionCategory
                                ,DPT.UserName Department
                                ,SEC.UserName Section
                                ,SSEC.UserName SubSection
                                ,DEG.UserName Designation
                                ,GDEG.UserName GivenDesignation
                                                                 ,REPLACE(CONVERT(varchar(11), E.DOJ, 106), ' ', '-') AS DOJ

								                                 ,SUBSTRING(
			                                (
				                                SELECT ','+ DAC.LetterName  AS [text()]
				                                FROM  [dbo].[EmployeeDisciplinaryActionDetails] DAD 				
				                                LEFT JOIN [dbo].[DisciplinaryActionSettingChild] DAC on DAC.Id = DAD.LetterFormetId
				                                WHERE DAD.EmployeeDisciplinaryActionId = EDA.Id			
				
				                                ORDER BY DAD.LetterFormetId
				                                FOR XML PATH ('')
			                                ), 2, 1000) [Letters],Format(EDA.EntryDate,'dd-MMM-yyyy') EntryDate,Format(dadm.NextLetterDueDate,'dd-MMM-yyyy') NextLetterDueDate,Format(dadm.LetterIssueDate,'dd-MMM-yyyy') LetterIssueDate,EDA.Id EmployeeDisciplinaryActionId
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
			                                         where das.DisciplinaryActionCategoryId=EDA.DisciplinaryActionCategoryId--'DACM2010'
			                                         and das.IsActive = 1 
			                                         and Sequence=(select Sequence+1 from DisciplinaryActionSettingDetails where id=dadm.DisciplinaryActionSettingDetailsId--'DASD2057'
			                                         )
		                                        )





                                                                 FROM HKP.EmployeeDisciplinaryAction EDA
                                                                 LEFT JOIN HKP.DisciplinaryActionCategory DAC on DAC.Id = EDA.DisciplinaryActionCategoryId

								                                  LEFT JOIN 
		                                (		
                                        select m.EmpSystemId,d.EmployeeDisciplinaryActionId,Max(ds.Sequence) Sequence,ds.DisciplinaryActionCategoryId
		                                from  [dbo].[EmployeeDisciplinaryActionDetails] d
		                                inner join HKP.EmployeeDisciplinaryAction m on m.Id=d.EmployeeDisciplinaryActionId
		                                left join  DisciplinaryActionSettingDetails ds on ds.Id=d.DisciplinaryActionSettingDetailsId
		                                group by m.EmpSystemId,d.EmployeeDisciplinaryActionId,ds.DisciplinaryActionCategoryId		
		                                ) dadm2 on   dadm2.EmployeeDisciplinaryActionId=EDA.Id  
		                                left join  DisciplinaryActionSettingDetails ds2 on ds2.DisciplinaryActionCategoryId=dadm2.DisciplinaryActionCategoryId and ds2.Sequence=dadm2.Sequence		
	                                    left join  [dbo].[EmployeeDisciplinaryActionDetails] dadm  on 	 dadm.DisciplinaryActionSettingDetailsId=ds2.id and dadm.EmployeeDisciplinaryActionId=dadm2.EmployeeDisciplinaryActionId

                                                                 LEFT JOIN EmployeeInformation E on E.SystemId = EDA.EmpSystemId
                                                                 LEFT JOIN MST.ManpowerBudget MB ON MB.Id = E.BudgetCode
                                                                 LEFT JOIN ORG.Entity EN ON EN.Id = MB.EntityId
                                                                 LEFT JOIN ORG.Position PS on PS.Id = MB.PositionId
                                                                 LEFT JOIN[ORG].[Division] DIV ON DIV.Id=PS.DivisionId
                                                                 LEFT JOIN[ORG].[SubDivision] SDIV ON SDIV.Id=PS.SubdivisionID
                                                                 LEFT JOIN[ORG].[Department] DPT ON DPT.Id=PS.DepartmentId
                                                                 LEFT JOIN[ORG].[Section] SEC ON SEC.Id=PS.SectionId
                                                                 LEFT JOIN[ORG].[SubSection] SSEC ON SSEC.Id=PS.SubSectionId
                                                                 LEFT JOIN[HKP].[Designation] DEG ON DEG.Id=PS.DesignationID
                                                                 LEFT JOIN[HKP].[Designation] GDEG ON GDEG.Id=E.GivenDesignationId 
                                                                Where EDA.EmpSystemId = '" + EmpId+ @"' AND ISNULL(ActionType,'') NOT IN ('TBS')
                                                                Order By EDA.EntryDate DESC";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public GridModel QueryActionCount(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.sort = "TotalAction";
                parameters.order = "DESC";
                parameters.CmdText = @"	Select COUNT(EmpSystemId) TotalAction, EmpSystemId 
                                        ,E.EmployeeCode
                                        ,E.EmployeeName
                                        ,DPT.UserName Department
                                        ,SEC.UserName Section
                                        ,SSEC.UserName SubSection
                                        ,DEG.UserName Designation
                                        ,GDEG.UserName GivenDesignation ,E.EmpPicPath 
                                        ,REPLACE(CONVERT(varchar(11), E.DOJ, 106), ' ', '-') AS DOJ
                                        FROM HKP.EmployeeDisciplinaryAction EA
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
                                        where  ISNULL(ActionType,'') NOT IN ('TBS') AND E.PlantId='"+ plantId + @"'
                                        Group By EA.EmpSystemId,E.EmployeeCode,E.EmployeeName,DPT.UserName 
                                        ,SEC.UserName 
                                        ,SSEC.UserName
                                        ,DEG.UserName 
                                        ,GDEG.UserName,E.EmpPicPath,DOJ";
               return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
       
       
    }
}