#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.Organizations;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Organizations;
using Library.Service.Systems;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Syncfusion.XlsIO;
using System.Data;
using OTSBD;
using Library.Service.Helpers;

#endregion Using

namespace Library.Service.Employees
{
    public class SalaryHeadGLService : Service<SalaryHeadGL>, ISalaryHeadGLService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IManpowerBudgetService _manpowerBudget;

        public SalaryHeadGLService(
            IRepositoryAsync<SalaryHeadGL> loanTypeTakenGLRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IManpowerBudgetService manpowerBudget
            ) : base(loanTypeTakenGLRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _manpowerBudget = manpowerBudget;
        }

        #endregion Constructor

        public IEnumerable<object> GetSalaryHeadGl(string loanTypeTakenId)
        {
            try
            {
                string _sql = @"SELECT LTTG.LiabilityGLId AS GLGeneralInfoId, GLGI.UserName AS GL, LTTG.LiabilityBudgetId AS BudgetId, LTTG.LiabilityActivityId AS ActivityId
                                FROM HKP.SalaryHeadGL AS LTTG
                                INNER JOIN HKP.GLGeneralInfo AS GLGI ON LTTG.LiabilityGLId=GLGI.Id
                                WHERE SalaryHeadId='" + loanTypeTakenId + "'";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetSalaryHead(string plantid, string manPowerBudgetId)
        {
            try
            {
                var st = "";
                if (manPowerBudgetId != "undefined")
                {
                    st = manPowerBudgetId;
                }
                string _sql = @"SELECT Distinct SG.Id
                                ,SG.CompanyId
                                ,SG.ManpowerBudgetId
                                ,SG.SalaryHeadId
                                ,SG.GLGeneralInfoId
                                ,SG.BudgetId
                                ,SG.ActivityId
                                ,SG.PreGLId
                                ,SG.PreBudgetId
                                ,SG.PreActivityId
                                ,S.SalaryHeadID
                                ,S.SalaryHead
                                ,S.Description
                                ,S.HeadType
								,ADGL.AccountCode +' - ' + ADGL.UserName ReconAssetGLInfo
								,PAGL.AccountCode +' - ' + PAGL.UserName DepreciationGLInfo
                                FROM SalaryHead S
                                LEFT OUTER JOIN MST.PlantSalaryHeadSequence PS ON S.SalaryHeadID=PS.SalaryHeadId
                                LEFT OUTER JOIN (select * from MST.SalaryHeadGL where ManPowerBudgetId in('" + st + @"')) SG ON S.SalaryHeadID=SG.SalaryHeadId
                                 LEFT OUTER JOIN HKP.GLGeneralInfo AS ADGL ON ADGL.Id=SG.GLGeneralInfoId
                                LEFT OUTER JOIN HKP.GLGeneralInfo AS PAGL ON PAGL.Id=SG.PreGLId
                                WHERE PS.PlantId='20171'
                                ORDER BY S.HeadType DESC , S.SalaryHead";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IEnumerable<object> GetSalaryHeadGL(string plantid, string salaryHeadId)
        {
            try
            {
                string _sql = @"SELECT Distinct SG.Id
                                ,SG.CompanyId
                                ,SG.SalaryHeadId
                                ,SG.DirectGLId
                                ,SG.DirectBudgetMasterId
                                ,SG.DirectActivityId
                                ,SG.InDirectGLId
                                ,SG.InDirectBudgetMasterId
                                ,SG.InDirectActivityId
                                ,S.SalaryHeadID
                                ,S.SalaryHead
                                ,S.Description
                                ,S.HeadType
								,ADGL.AccountCode +' - ' + ADGL.UserName ReconAssetGLInfo
								,PAGL.AccountCode +' - ' + PAGL.UserName DepreciationGLInfo
                                FROM SalaryHead S
                                LEFT  JOIN MST.PlantSalaryHeadSequence PS ON S.SalaryHeadID=PS.SalaryHeadId
                                LEFT  JOIN  MST.SalaryHeadGL   SG ON S.SalaryHeadID=SG.SalaryHeadId
                                 LEFT  JOIN HKP.GLGeneralInfo AS ADGL ON ADGL.Id=SG.DirectGLId
                                LEFT  JOIN HKP.GLGeneralInfo AS PAGL ON PAGL.Id=SG.InDirectGLId
                                WHERE PS.PlantId='" + plantid+"' and S.SalaryHeadId='"+salaryHeadId+@"'
                                ORDER BY S.HeadType DESC , S.SalaryHead";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IEnumerable<object> GetSalaryHeadData()
        {
            try
            {
                string _sql = @"SELECT *
                                FROM SalaryHead S
                                WHERE SalaryHead not in ('CTC','Gross','Net Pay','Total Gross')
                                ORDER BY S.HeadType DESC , S.SalaryHead";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetManPowerBudgetList(GridParameter parameters, string companyId)
        {
            try
            {
                parameters.CmdText = @"SELECT  PMB.Id
                                    ,PMB.Code
                                    ,PMB.EntityId
                                    ,ERD.Code AS EntityCode
		                            ,ERD.UserName AS Entity
                                    , (SELECT UserName FROM  [HKP].[EmployeeGroup] WHERE Id=ERD.EmployeeGroupId) AS [EmployeeGroup], P.UserName AS [Plant], (SELECT UserName FROM  [ORG].[Unit] WHERE Id=ERD.UnitId) AS [Unit], (SELECT UserName FROM  [ORG].[Division] WHERE Id=ERD.DivisionId) AS [Division], PMB.PositionId, PRD.Code AS PositionCode, PRD.UserName AS Position, (SELECT UserName FROM [ORG].[Department] WHERE Id=PRD.DepartmentId) AS [Department], (SELECT UserName FROM [ORG].[Section] WHERE Id=PRD.SectionId) AS [Section], (SELECT UserName FROM  [ORG].[Line] WHERE Id=PMB.LineId) AS [Line], (SELECT UserName FROM  [dbo].[ShiftDefination] WHERE SystemID=PMB.ShiftDefinationId) AS [ShiftDefination], (SELECT UserName FROM [HKP].[Designation] WHERE Id=PRD.DesignationId) AS [Designation]
                                    ,PMB.EmploymentType
                                    ,PMB.IsOTEntitled
                                    ,PRD.DesignationId
                                    ,PMB.CompanyId
                            FROM [MST].[ManpowerBudget] AS PMB
                            INNER JOIN [ORG].[Entity] AS ERD ON PMB.EntityId=ERD.Id
                            LEFT JOIN [ORG].[Plant] AS P ON P.Id=ERD.PlantId
                            INNER JOIN [ORG].[Position] AS PRD ON PMB.PositionId=PRD.Id
							LEFT OUTER JOIN MST.SalaryHeadGL S ON PMB.Id=S.ManpowerBudgetId
                            WHERE PMB.Archive=0 AND PMB.CompanyId='" + companyId + "' AND PMB.Id NOT IN (SELECT ManpowerBudgetId FROM MST.SalaryHeadGL)";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetManPowerBudgetSavedList(GridParameter parameters, string plantid)
        {
            try
            {
                parameters.CmdText = @"SELECT distinct b.Id,sg.ManpowerBudgetId,b.Code,b.EmploymentType
							,  (SELECT UserName FROM  [HKP].[EmployeeGroup] WHERE Id=ERD.EmployeeGroupId) AS [EmployeeGroup], P.UserName AS [Plant]
							, (SELECT UserName FROM  [ORG].[Unit] WHERE Id=ERD.UnitId) AS [Unit]
							, (SELECT UserName FROM  [ORG].[Division] WHERE Id=ERD.DivisionId) AS [Division], b.PositionId, p.Code AS PositionCode, p.UserName AS Position
							, (SELECT UserName FROM [ORG].[Department] WHERE Id=p.DepartmentId) AS [Department]
							, (SELECT UserName FROM [ORG].[Section] WHERE Id=p.SectionId) AS [Section]
							, (SELECT UserName FROM  [ORG].[Line] WHERE Id=b.LineId) AS [Line]
							, (SELECT UserName FROM  [dbo].[ShiftDefination] WHERE SystemID=b.ShiftDefinationId) AS [ShiftDefination]
							, (SELECT UserName FROM [HKP].[Designation] WHERE Id=p.DesignationId) AS [Designation]
                                FROM  MST.SalaryHeadGL SG
								left outer join mst.ManpowerBudget b on b.Id=sg.ManpowerBudgetId
								INNER JOIN [ORG].[Entity] AS ERD ON b.EntityId=ERD.Id
								left outer join org.Position p on b.PositionId=p.Id
                                WHERE sg.PlantId='" + plantid + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetBudgetListWithGL(GridParameter parameters, string id)
        {
            try
            {
                parameters.CmdText = @"SELECT B.Id,B.Code ,B.UserName,B.StandardName FROM HKP.Budget AS B
                                LEFT OUTER JOIN [MST].[BudgetMaster] AS BM ON B.Id=BM.BudgetId
                                WHERE BM.GLGeneralInfoId='" + id + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetActivityListWithBudget(GridParameter parameters, string id)
        {
            try
            {
                parameters.CmdText = @"SELECT Distinct B.Id,B.Code ,B.UserName,B.StandardName  FROM HKP.Activity AS A
                                LEFT OUTER JOIN [MST].[BudgetActivity] AS BA ON A.Id= BA.ActivityId
                                LEFT OUTER JOIN [MST].[BudgetMaster] AS BM ON BA.BudgetMasterId=BM.Id
                                LEFT OUTER JOIN HKP.Budget AS B ON BM.BudgetId = B.Id
                                WHERE B.Id='" + id + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertOrUpdate(IEnumerable<SalaryHeadGL> entities)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var pk = GetMaxNumber(nameof(SalaryHeadGL), PKGeneratorEnum.Auto, null, DateTime.Now);
                foreach (var item in entities)
                {
                    var data = Query(r => r.SalaryHeadId == item.SalaryHeadId).Select().FirstOrDefault();
                    if (data==null)
                    {
                        pk.MaxNumber++;
                        item.Id = pk.MaxNumber.ToString();
                        AuditService.Log(item);
                        item.ModelState = ModelState.Added;
                        InsertGraph(item);


                    }
                    


                    else
                    {
                        item.ModelState = ModelState.Modified;
                        UpdateGraph(item);
                    }
                }
               
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
       
        public GridModel GetAllList(GridParameter parameters, string coaId)
        {
            try
            {
                parameters.CmdText = @"SELECT
                     F.Id
                    ,ETT.Id AS SalaryHeadId
                    ,ETT.UserName AS SalaryHeadName
                    ,ETT.Code
                    ,ETT.Description
                    ,F.COAId
                    ,F.UserName 'COAName'
                    ,F.LiabilityGLId
					,F.ExpensesGLId
                    ,F.ExpensesGLCode
                    ,F.ExpensesGLText
					,F.LiabilityGLText
					,F.LiabilityGLCode
					,F.ExpensesBudgetName
					,F.ExpensesActivityName
					,F.LiabilityBudgetName
					,F.LiabilityActivityName
					,F.ExpensesBudgetId
					,F.ExpensesActivityId
					,F.LiabilityBudgetId
					,F.LiabilityActivityId
					FROM HKP.SalaryHead  AS ETT
						LEFT OUTER JOIN (select
        			            ETTGL.Id,ETTGL.SalaryHeadId
        			            ,C.Id AS COAId,C.UserName
								,ETTGL.LiabilityGLId,ETTGL.ExpensesGLId
								,ETTGL.ExpensesBudgetId,ETTGL.ExpensesActivityId
								,ETTGL.LiabilityBudgetId,ETTGL.LiabilityActivityId
        			            ,ADGL.UserName AS  LiabilityGLText,ADGL.AccountCode AS LiabilityGLCode
        			            ,PAGL.UserName AS  ExpensesGLText,PAGL.AccountCode AS ExpensesGLCode
								,AB.UserName AS ExpensesBudgetName
								,AA.UserName AS ExpensesActivityName
								,PB.UserName AS LiabilityBudgetName
								,PA.UserName AS LiabilityActivityName
        			            from HKP.COA c
        			            LEFT OUTER JOIN HKP.SalaryHeadGL AS ETTGL ON ETTGL.COAId=c.Id
        			            LEFT OUTER JOIN HKP.GLGeneralInfo AS ADGL ON ADGL.Id=ETTGL.LiabilityGLId
        			            LEFT OUTER JOIN HKP.GLGeneralInfo AS PAGL ON PAGL.Id=ETTGL.ExpensesGLId
								LEFT OUTER JOIN HKP.Budget AS AB ON ETTGL.ExpensesBudgetId = AB.Id
								LEFT OUTER JOIN HKP.Activity AS AA ON ETTGL.ExpensesActivityId = AA.Id
								LEFT OUTER JOIN HKP.Budget AS PB ON ETTGL.LiabilityBudgetId = PB.Id
								LEFT OUTER JOIN HKP.Activity AS PA ON ETTGL.LiabilityActivityId = PA.Id
					            where isnull(c.Id,'') ='" + coaId + "' ) AS F ON F.SalaryHeadId = ETT.Id ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetAssingList(GridParameter parameters, string coaId)
        {
            try
            {
                string coaStr = " ";
                if (coaId != "null")
                    coaStr += "where isnull(c.Id,'') ='" + coaId + @"'";
                parameters.CmdText = @"SELECT
                     F.Id
                    ,ETT.Id AS SalaryHeadId
                    ,ETT.UserName AS SalaryHeadName
                    ,ETT.Code
                    ,ETT.Description
                    ,F.COAId
                    ,F.UserName 'COAName'
                    ,F.LiabilityGLId
					,F.ExpensesGLId
                    ,F.ExpensesGLCode
                    ,F.ExpensesGLText
					,F.LiabilityGLText
					,F.LiabilityGLCode
					,F.ExpensesBudgetName
					,F.ExpensesActivityName
					,F.LiabilityBudgetName
					,F.LiabilityActivityName
					,F.ExpensesBudgetId
					,F.ExpensesActivityId
					,F.LiabilityBudgetId
					,F.LiabilityActivityId
					FROM HKP.SalaryHead  AS ETT
						LEFT OUTER JOIN (select
        			            ETTGL.Id, ETTGL.SalaryHeadId
        			            ,C.Id AS COAId, C.UserName
								,ETTGL.LiabilityGLId, ETTGL.ExpensesGLId
								,ETTGL.ExpensesBudgetId,ETTGL.ExpensesActivityId
								,ETTGL.LiabilityBudgetId,ETTGL.LiabilityActivityId
        			            ,ADGL.UserName AS  LiabilityGLText,ADGL.AccountCode AS LiabilityGLCode
        			            ,PAGL.UserName AS  ExpensesGLText,PAGL.AccountCode AS ExpensesGLCode
								,AB.UserName AS ExpensesBudgetName
								,AA.UserName AS ExpensesActivityName
								,PB.UserName AS LiabilityBudgetName
								,PA.UserName AS LiabilityActivityName
        			            from HKP.COA c
        			            LEFT OUTER JOIN [HKP].SalaryHeadGL AS ETTGL ON ETTGL.COAId=c.Id
        			            LEFT OUTER JOIN [HKP].GLGeneralInfo AS ADGL ON ADGL.Id=ETTGL.LiabilityGLId
        			            LEFT OUTER JOIN [HKP].GLGeneralInfo AS PAGL ON PAGL.Id=ETTGL.ExpensesGLId
								LEFT OUTER JOIN [HKP].Budget AS AB ON ETTGL.ExpensesBudgetId = AB.Id
								LEFT OUTER JOIN [HKP].Activity AS AA ON ETTGL.ExpensesActivityId = AA.Id
								LEFT OUTER JOIN [HKP].Budget AS PB ON ETTGL.LiabilityBudgetId = PB.Id
								LEFT OUTER JOIN [HKP].Activity AS PA ON ETTGL.LiabilityActivityId = PA.Id
					" + coaStr + @"
        			)AS F ON F.SalaryHeadId = ETT.Id
                     WHERE  F.LiabilityGLId <> '' AND F.ExpensesGLId <> ''";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<object> CoaInfo(string companyId)
        {
            try
            {
                string _sql = @"SELECT CO.COAId,C.UserName AS CoaName FROM ORG.Company CO
                                LEFT Outer Join HKP.COA C ON CO.COAId =C.Id WHERE CO.Id='" + companyId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetNotAssingList(GridParameter parameters, string coaId)
        {
            try
            {
                string coaStr = " ";
                if (coaId != "null")
                    coaStr += "where isnull(c.Id,'') ='" + coaId + @"'";
                parameters.CmdText = @"SELECT
                     F.Id
                    ,ETT.Id AS SalaryHeadId
                    ,ETT.UserName AS SalaryHeadName
                    ,ETT.Code
                    ,ETT.Description
                    ,F.COAId
                    ,F.UserName 'COAName'
                    ,F.LiabilityGLId
					,F.ExpensesGLId
                    ,F.ExpensesGLCode
                    ,F.ExpensesGLText
					,F.LiabilityGLText
					,F.LiabilityGLCode
					,F.ExpensesBudgetName
					,F.ExpensesActivityName
					,F.LiabilityBudgetName
					,F.LiabilityActivityName
					,F.ExpensesBudgetId
					,F.ExpensesActivityId
					,F.LiabilityBudgetId
					,F.LiabilityActivityId
					FROM HKP.SalaryHead  AS ETT
						LEFT OUTER JOIN (select
        			            ETTGL.Id,ETTGL.SalaryHeadId
        			            ,C.Id AS COAId,C.UserName
								,ETTGL.LiabilityGLId,ETTGL.ExpensesGLId
								,ETTGL.ExpensesBudgetId,ETTGL.ExpensesActivityId
								,ETTGL.LiabilityBudgetId,ETTGL.LiabilityActivityId
        			            ,ADGL.UserName AS  LiabilityGLText,ADGL.AccountCode AS LiabilityGLCode
        			            ,PAGL.UserName AS  ExpensesGLText,PAGL.AccountCode AS ExpensesGLCode
								,AB.UserName AS ExpensesBudgetName
								,AA.UserName AS ExpensesActivityName
								,PB.UserName AS LiabilityBudgetName
								,PA.UserName AS LiabilityActivityName
        			            from HKP.COA c
        			            LEFT OUTER JOIN HKP.SalaryHeadGL AS ETTGL ON ETTGL.COAId=c.Id
        			            LEFT OUTER JOIN HKP.GLGeneralInfo AS ADGL ON ADGL.Id=ETTGL.LiabilityGLId
        			            LEFT OUTER JOIN HKP.GLGeneralInfo AS PAGL ON PAGL.Id=ETTGL.ExpensesGLId
								LEFT OUTER JOIN HKP.Budget AS AB ON ETTGL.ExpensesBudgetId = AB.Id
								LEFT OUTER JOIN HKP.Activity AS AA ON ETTGL.ExpensesActivityId = AA.Id
								LEFT OUTER JOIN HKP.Budget AS PB ON ETTGL.LiabilityBudgetId = PB.Id
								LEFT OUTER JOIN HKP.Activity AS PA ON ETTGL.LiabilityActivityId = PA.Id
					" + coaStr + @"
        			)AS F ON F.SalaryHeadId = ETT.Id
                                        WHERE (isnull(F.LiabilityGLId,'')= '' OR isnull(F.ExpensesGLId,'')= '') ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetSearchWithCombine(GridParameter parameters, string coaId)
        {
            try
            {
                parameters.CmdText = @"SELECT distinct SH.SalaryHeadID,SH.SalaryHead,SH.HeadType,SH.HeadCategory,SGL.DrDirectGLId,SGL.DrDirectBudgetMasterId,SGL.DrDirectActivityId
                            ,SGL.DrInDirectGLId,SGL.DrInDirectBudgetMasterId,SGL.DrInDirectActivityId
                            ,DGL.AccountCode+' - '+DGL.UserName DirectGLName,DB.UserName DirectBudgetName,DA.UserName DirectActivityName 
                            ,IGL.AccountCode+' - '+IGL.UserName InDirectGLName,IB.UserName InDirectBudgetName,IA.UserName InDirectActivityName 
							,SGL.CrDirectGLId,SGL.CrDirectBudgetMasterId,SGL.CrDirectActivityId
                            ,SGL.CrInDirectGLId,SGL.CrInDirectBudgetMasterId,SGL.CrInDirectActivityId
                            ,CDGL.AccountCode+' - '+CDGL.UserName CrDirectGLName,CDB.UserName CrDirectBudgetName,CDA.UserName CrDirectActivityName 
                            ,CIGL.AccountCode+' - '+CIGL.UserName CrInDirectGLName,CIB.UserName CrInDirectBudgetName,CIA.UserName CrInDirectActivityName 
                            ,0 Flag,SGL.Id,SGL.SalaryPayableGroup
	                        ,SGL.DrDirectOtherGLCode,SGL.DrDirectOtherGL ,SGL.CrDirectOtherGLCode,SGL.CrDirectOtherGL
							,SGL.DrInDirectOtherGLCode,SGL.DrInDirectOtherGL ,SGL.CrInDirectOtherGLCode,SGL.CrInDirectOtherGL

                            FROM dbo.SalaryHead SH
                            LEFT JOIN MST.SalaryHeadGL SGL  ON SH.SalaryHeadID=SGL.SalaryHeadId
                            LEFT JOIN HKP.GLGeneralInfo DGL ON DGL.Id=SGL.DrDirectGLId
                            LEFT JOIN MST.BudgetMaster DBM ON DBM.Id=SGL.DrDirectBudgetMasterId
                            LEFT JOIN HKP.Budget DB ON DB.Id=DBM.BudgetId
                            LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=SGL.DrInDirectGLId
                            LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=SGL.DrInDirectBudgetMasterId
                            LEFT JOIN HKP.Budget IB ON IB.Id=IBM.BudgetId
                            LEFT JOIN HKP.Activity DA ON DA.Id=SGL.DrDirectActivityId
                            LEFT JOIN HKP.Activity IA ON IA.Id=SGL.DrInDirectActivityId

							LEFT JOIN HKP.GLGeneralInfo CDGL ON CDGL.Id=SGL.CrDirectGLId
                            LEFT JOIN MST.BudgetMaster CDBM ON CDBM.Id=SGL.CrDirectBudgetMasterId
                            LEFT JOIN HKP.Budget CDB ON CDB.Id=CDBM.BudgetId
                            LEFT JOIN HKP.GLGeneralInfo CIGL ON CIGL.Id=SGL.CrInDirectGLId
                            LEFT JOIN MST.BudgetMaster CIBM ON CIBM.Id=SGL.CrInDirectBudgetMasterId
                            LEFT JOIN HKP.Budget CIB ON CIB.Id=CIBM.BudgetId
                            LEFT JOIN HKP.Activity CDA ON CDA.Id=SGL.CrDirectActivityId
                            LEFT JOIN HKP.Activity CIA ON CIA.Id=SGL.CrInDirectActivityId
                            Where ISNULL(SH.HeadCategory,'') not in ('CTC','Gross','Total Gross')";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public GridModel GetSearchWithCombineSalaryHead(GridParameter parameters, string coaId)
        {
            try
            {
                parameters.CmdText = @"SELECT distinct SH.SalaryHeadID,SH.SalaryHead,SH.HeadType,SH.HeadCategory
                                        FROM dbo.SalaryHead SH
                                        Where ISNULL(SH.HeadCategory,'') not in ('CTC','Gross','Total Gross')";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public List<Dictionary<string, object>> GetSalaryHeadGLCombine(string coaId)
        {
            try
            {
                var sql = @"SELECT distinct SH.SalaryHeadID,SH.SalaryHead,SH.HeadType,SH.HeadCategory,SGL.DrDirectGLId,SGL.DrDirectBudgetMasterId,SGL.DrDirectActivityId
                            ,SGL.DrInDirectGLId,SGL.DrInDirectBudgetMasterId,SGL.DrInDirectActivityId
                            ,DGL.AccountCode+' - '+DGL.UserName DirectGLName,DB.UserName DirectBudgetName,DA.UserName DirectActivityName 
                            ,IGL.AccountCode+' - '+IGL.UserName InDirectGLName,IB.UserName InDirectBudgetName,IA.UserName InDirectActivityName 
							,SGL.CrDirectGLId,SGL.CrDirectBudgetMasterId,SGL.CrDirectActivityId
                            ,SGL.CrInDirectGLId,SGL.CrInDirectBudgetMasterId,SGL.CrInDirectActivityId
                            ,CDGL.AccountCode+' - '+CDGL.UserName CrDirectGLName,CDB.UserName CrDirectBudgetName,CDA.UserName CrDirectActivityName 
                            ,CIGL.AccountCode+' - '+CIGL.UserName CrInDirectGLName,CIB.UserName CrInDirectBudgetName,CIA.UserName CrInDirectActivityName 
                            ,0 Flag,SGL.Id,SGL.SalaryPayableGroup,NULL HType
                            FROM dbo.SalaryHead SH
                            LEFT JOIN MST.SalaryHeadGL SGL  ON SH.SalaryHeadID=SGL.SalaryHeadId
                            LEFT JOIN HKP.GLGeneralInfo DGL ON DGL.Id=SGL.DrDirectGLId
                            LEFT JOIN MST.BudgetMaster DBM ON DBM.Id=SGL.DrDirectBudgetMasterId
                            LEFT JOIN HKP.Budget DB ON DB.Id=DBM.BudgetId
                            LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=SGL.DrInDirectGLId
                            LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=SGL.DrInDirectBudgetMasterId
                            LEFT JOIN HKP.Budget IB ON IB.Id=IBM.BudgetId
                            LEFT JOIN HKP.Activity DA ON DA.Id=SGL.DrDirectActivityId
                            LEFT JOIN HKP.Activity IA ON IA.Id=SGL.DrInDirectActivityId

							LEFT JOIN HKP.GLGeneralInfo CDGL ON CDGL.Id=SGL.CrDirectGLId
                            LEFT JOIN MST.BudgetMaster CDBM ON CDBM.Id=SGL.CrDirectBudgetMasterId
                            LEFT JOIN HKP.Budget CDB ON CDB.Id=CDBM.BudgetId
                            LEFT JOIN HKP.GLGeneralInfo CIGL ON CIGL.Id=SGL.CrInDirectGLId
                            LEFT JOIN MST.BudgetMaster CIBM ON CIBM.Id=SGL.CrInDirectBudgetMasterId
                            LEFT JOIN HKP.Budget CIB ON CIB.Id=CIBM.BudgetId
                            LEFT JOIN HKP.Activity CDA ON CDA.Id=SGL.CrDirectActivityId
                            LEFT JOIN HKP.Activity CIA ON CIA.Id=SGL.CrInDirectActivityId
                            Where ISNULL(SH.HeadCategory,'') not in ('CTC','Gross','Total Gross')
                            
                                ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public GridModel GetSearchWithCombineWithAssing(GridParameter parameters, string coaId)
        {
            try
            {
                parameters.CmdText = @"SELECT FAD.Id
                                ,FAM.Id AS FixedAssetMasterId
                                ,FAM.UserName AS FixedAssetMasterName
                                ,FAD.AccumulatedDepreciationGLId
                                ,FAD.AssetUnderConstructionGLId
                                ,FAD.DepreciationGLId
                                ,FAD.DownPaymentGLId
                                ,FAD.ClearingAccountGLId
                                ,FAD.GainOnSaleOfAssetGLId
                                ,FAD.LossOnSaleOfAssetGLId
                                ,FAD.LossOnDisposalAssetGLId
                                ,FAD.LessValueAssetGLId
                                ,C.UserName
                                ,GLGI2.AccountCode + ' - ' + GLGI2.UserName AS AccDepreciationGLInfo
                                ,GLGI3.AccountCode + ' - ' + GLGI3.UserName AS DepreciationGLInfo
                                ,GLGI4.AccountCode + ' - ' + GLGI4.UserName AS AUCGLInfo
                                ,GLGI5.AccountCode + ' - ' + GLGI5.UserName AS DownPaymentGLInfo
                                ,GLGI6.AccountCode + ' - ' + GLGI6.UserName AS ClearingAccountGLInfo
                                ,GLGI7.AccountCode + ' - ' + GLGI7.UserName AS GainOnSaleOfAssetGLInfo
                                ,GLGI8.AccountCode + ' - ' + GLGI8.UserName AS LossOnSaleOfAssetGLInfo
                                ,GLGI9.AccountCode + ' - ' + GLGI9.UserName AS LossOnDisposalAssetGLInfo
                                ,GLGI10.AccountCode + ' - ' + GLGI10.UserName AS LessValueAssetGLInfo
                                ,FAD.AccumulatedDepreciationBudgetMasterId
                                ,FAD.AccumulatedDepreciationActivityId
                                ,ADBudget.UserName AS   AccumulatedDepreciationBudgetName
                                ,ADActivity.UserName AS AccumulatedDepreciationActivityName
                                ,FAD.DepreciationBudgetMasterId,FAD.DepreciationActivityId
                                ,DEPBudget.UserName AS   DepreciationBudgetName
                                ,DEPActivity.UserName AS DepreciationActivityName
                                ,FAD.AssetUnderConstructionBudgetMasterId
                                ,FAD.AssetUnderConstructionActivityId
                                ,AUCBudget.UserName AS   AssetUnderConstructionBudgetName
                                ,AUCActivity.UserName AS AssetUnderConstructionActivityName
                                ,FAD.DownPaymentBudgetMasterId
                                ,FAD.DownPaymentActivityId
                                ,DPBudget.UserName AS   DownPaymentBudgetName
                                ,DPActivity.UserName AS DownPaymentActivityName
                                ,FAD.ClearingAccountBudgetMasterId
                                ,FAD.ClearingAccountActivityId
                                ,CABudget.UserName AS   ClearingAccountBudgetName
                                ,CAActivity.UserName AS ClearingAccountActivityName
                                ,FAD.GainOnSaleOfAssetBudgetMasterId
                                ,FAD.GainOnSaleOfAssetActivityId
                                ,GOSBudget.UserName AS   GainOnSaleOfAssetBudgetName
                                ,GOSActivity.UserName AS GainOnSaleOfAssetActivityName
                                ,FAD.LossOnSaleOfAssetBudgetMasterId
                                ,FAD.LossOnSaleOfAssetActivityId
                                ,LOSBudget.UserName AS   LossOnSaleOfAssetBudgetName
                                ,LOSActivity.UserName AS LossOnSaleOfAssetActivityName
                                ,FAD.LossOnDisposalAssetBudgetMasterId
                                ,FAD.LossOnDisposalAssetActivityId
                                ,LODBudget.UserName AS   LossOnDisposalAssetBudgetName
                                ,LODActivity.UserName AS LossOnDisposalAssetActivityName
								,FAD.LessValueAssetBudgetMasterId
								,FAD.LessValueAssetActivityId
                                ,LEVBudget.UserName AS   LessValueAssetBudgetName
                                ,LEActivity.UserName AS LessValueAssetActivityName
                                FROM MST.FixedAssetMaster As FAM
                                LEFT JOIN HKP.FixedAssetMasterGL AS FAD  ON FAD.FixedAssetMasterId=FAM.Id
                                LEFT JOIN(SELECT Id, UserName from HKP.COA where isnull(Id,'') ='" + coaId + @"') C ON FAD.COAId=C.Id
                                                                LEFT JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=FAD.AccumulatedDepreciationGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI3 ON GLGI3.Id=FAD.DepreciationGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=FAD.AssetUnderConstructionGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI5 ON GLGI5.Id=FAD.DownPaymentGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI6 ON GLGI6.Id=FAD.ClearingAccountGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI7 ON GLGI7.Id=FAD.GainOnSaleOfAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI8 ON GLGI8.Id=FAD.LossOnSaleOfAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI9 ON GLGI9.Id=FAD.LossOnDisposalAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI10 ON GLGI10.Id=FAD.LessValueAssetGLId
                                LEFT JOIN MST.BudgetMaster AS ADBudgetM ON FAD.AccumulatedDepreciationBudgetMasterId = ADBudgetM.Id
                                LEFT JOIN HKP.Budget AS ADBudget ON ADBudgetM.BudgetId = ADBudget.Id
                                LEFT JOIN HKP.Activity AS ADActivity ON FAD.AccumulatedDepreciationActivityId = ADActivity.Id
								LEFT JOIN MST.BudgetMaster AS DEPBudgetM ON FAD.DepreciationBudgetMasterId = DEPBudgetM.Id
                                LEFT JOIN HKP.Budget AS   DEPBudget ON     DEPBudget.Id =   DEPBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS DEPActivity ON FAD.DepreciationActivityId = DEPActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   AUCBudgetM ON   FAD.AssetUnderConstructionBudgetMasterId =   AUCBudgetM.Id
                                LEFT JOIN HKP.Budget AS   AUCBudget ON   AUCBudget.Id =   AUCBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS AUCActivity ON FAD.AssetUnderConstructionActivityId = AUCActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   DPBudgetM ON   FAD.DownPaymentBudgetMasterId =   DPBudgetM.Id
                                LEFT JOIN HKP.Budget AS   DPBudget ON   DPBudget.Id =   DPBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS DPActivity ON FAD.DownPaymentActivityId = DPActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   CABudgetM ON   FAD.ClearingAccountBudgetMasterId =   CABudgetM.Id
                                LEFT JOIN HKP.Budget AS   CABudget ON   CABudget.Id =   CABudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS CAActivity ON FAD.ClearingAccountActivityId = CAActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   GOSBudgetM ON   FAD.GainOnSaleOfAssetBudgetMasterId = GOSBudgetM.Id
                                LEFT JOIN HKP.Budget AS   GOSBudget ON   GOSBudget.Id =   GOSBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS GOSActivity ON FAD.GainOnSaleOfAssetActivityId = GOSActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   LOSBudgetM ON   FAD.LossOnSaleOfAssetBudgetMasterId =   LOSBudgetM.Id
                                LEFT JOIN HKP.Budget AS   LOSBudget ON   LOSBudget.Id =   LOSBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS LOSActivity ON FAD.LossOnSaleOfAssetActivityId = LOSActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   LODBudgetM ON   FAD.LossOnDisposalAssetBudgetMasterId =   LODBudgetM.Id
                                LEFT JOIN HKP.Budget AS   LODBudget ON   LODBudget.Id =   LODBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS LODActivity ON FAD.LossOnDisposalAssetActivityId = LODActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   LEVBudgetM ON   FAD.LessValueAssetBudgetMasterId =   LEVBudgetM.Id
								LEFT JOIN HKP.Budget AS   LEVBudget ON   LEVBudget.Id =   LEVBudgetM.BudgetId
								LEFT JOIN HKP.Activity AS LEActivity ON FAD.LessValueAssetActivityId = LEActivity.Id
                                WHERE   FAD.AccumulatedDepreciationGLId <> '' AND FAD.DepreciationGLId <> ''
                                AND FAD.DownPaymentGLId <> '' AND FAD.ClearingAccountGLId <> ''
                                AND FAD.GainOnSaleOfAssetGLId <> '' AND FAD.LossOnSaleOfAssetGLId <> ''
                                AND FAD.LossOnDisposalAssetGLId <> ''";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public GridModel GetSearchWithCombineWithNotAssing(GridParameter parameters, string coaId)
        {
            try
            {
                parameters.CmdText = @"SELECT FAD.Id
                                ,FAM.Id AS FixedAssetMasterId
                                ,FAM.UserName AS FixedAssetMasterName
                                ,FAD.AccumulatedDepreciationGLId
                                ,FAD.AssetUnderConstructionGLId
                                ,FAD.DepreciationGLId
                                ,FAD.DownPaymentGLId
                                ,FAD.ClearingAccountGLId
                                ,FAD.GainOnSaleOfAssetGLId
                                ,FAD.LossOnSaleOfAssetGLId
                                ,FAD.LossOnDisposalAssetGLId
                                ,FAD.LessValueAssetGLId
                                ,C.UserName
                                ,GLGI2.AccountCode + ' - ' + GLGI2.UserName AS AccDepreciationGLInfo
                                ,GLGI3.AccountCode + ' - ' + GLGI3.UserName AS DepreciationGLInfo
                                ,GLGI4.AccountCode + ' - ' + GLGI4.UserName AS AUCGLInfo
                                ,GLGI5.AccountCode + ' - ' + GLGI5.UserName AS DownPaymentGLInfo
                                ,GLGI6.AccountCode + ' - ' + GLGI6.UserName AS ClearingAccountGLInfo
                                ,GLGI7.AccountCode + ' - ' + GLGI7.UserName AS GainOnSaleOfAssetGLInfo
                                ,GLGI8.AccountCode + ' - ' + GLGI8.UserName AS LossOnSaleOfAssetGLInfo
                                ,GLGI9.AccountCode + ' - ' + GLGI9.UserName AS LossOnDisposalAssetGLInfo
                                ,GLGI10.AccountCode + ' - ' + GLGI10.UserName AS LessValueAssetGLInfo
                                ,FAD.AccumulatedDepreciationBudgetMasterId
                                ,FAD.AccumulatedDepreciationActivityId
                                ,ADBudget.UserName AS   AccumulatedDepreciationBudgetName
                                ,ADActivity.UserName AS AccumulatedDepreciationActivityName
                                ,FAD.DepreciationBudgetMasterId,FAD.DepreciationActivityId
                                ,DEPBudget.UserName AS   DepreciationBudgetName
                                ,DEPActivity.UserName AS DepreciationActivityName
                                ,FAD.AssetUnderConstructionBudgetMasterId
                                ,FAD.AssetUnderConstructionActivityId
                                ,AUCBudget.UserName AS   AssetUnderConstructionBudgetName
                                ,AUCActivity.UserName AS AssetUnderConstructionActivityName
                                ,FAD.DownPaymentBudgetMasterId
                                ,FAD.DownPaymentActivityId
                                ,DPBudget.UserName AS   DownPaymentBudgetName
                                ,DPActivity.UserName AS DownPaymentActivityName
                                ,FAD.ClearingAccountBudgetMasterId
                                ,FAD.ClearingAccountActivityId
                                ,CABudget.UserName AS   ClearingAccountBudgetName
                                ,CAActivity.UserName AS ClearingAccountActivityName
                                ,FAD.GainOnSaleOfAssetBudgetMasterId
                                ,FAD.GainOnSaleOfAssetActivityId
                                ,GOSBudget.UserName AS   GainOnSaleOfAssetBudgetName
                                ,GOSActivity.UserName AS GainOnSaleOfAssetActivityName
                                ,FAD.LossOnSaleOfAssetBudgetMasterId
                                ,FAD.LossOnSaleOfAssetActivityId
                                ,LOSBudget.UserName AS   LossOnSaleOfAssetBudgetName
                                ,LOSActivity.UserName AS LossOnSaleOfAssetActivityName
                                ,FAD.LossOnDisposalAssetBudgetMasterId
                                ,FAD.LossOnDisposalAssetActivityId
                                ,LODBudget.UserName AS   LossOnDisposalAssetBudgetName
                                ,LODActivity.UserName AS LossOnDisposalAssetActivityName
								,FAD.LessValueAssetBudgetMasterId
								,FAD.LessValueAssetActivityId
                                ,LEVBudget.UserName AS   LessValueAssetBudgetName
                                ,LEActivity.UserName AS LessValueAssetActivityName
                                FROM MST.FixedAssetMaster As FAM
                                LEFT JOIN HKP.FixedAssetMasterGL AS FAD  ON FAD.FixedAssetMasterId=FAM.Id
                                LEFT JOIN(SELECT Id, UserName from HKP.COA where isnull(Id,'') ='" + coaId + @"') C ON FAD.COAId=C.Id
                                                                LEFT JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=FAD.AccumulatedDepreciationGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI3 ON GLGI3.Id=FAD.DepreciationGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=FAD.AssetUnderConstructionGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI5 ON GLGI5.Id=FAD.DownPaymentGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI6 ON GLGI6.Id=FAD.ClearingAccountGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI7 ON GLGI7.Id=FAD.GainOnSaleOfAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI8 ON GLGI8.Id=FAD.LossOnSaleOfAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI9 ON GLGI9.Id=FAD.LossOnDisposalAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI10 ON GLGI10.Id=FAD.LessValueAssetGLId
                                LEFT JOIN MST.BudgetMaster AS ADBudgetM ON FAD.AccumulatedDepreciationBudgetMasterId = ADBudgetM.Id
                                LEFT JOIN HKP.Budget AS ADBudget ON ADBudgetM.BudgetId = ADBudget.Id
                                LEFT JOIN HKP.Activity AS ADActivity ON FAD.AccumulatedDepreciationActivityId = ADActivity.Id
								LEFT JOIN MST.BudgetMaster AS DEPBudgetM ON FAD.DepreciationBudgetMasterId = DEPBudgetM.Id
                                LEFT JOIN HKP.Budget AS   DEPBudget ON     DEPBudget.Id =   DEPBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS DEPActivity ON FAD.DepreciationActivityId = DEPActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   AUCBudgetM ON   FAD.AssetUnderConstructionBudgetMasterId =   AUCBudgetM.Id
                                LEFT JOIN HKP.Budget AS   AUCBudget ON   AUCBudget.Id =   AUCBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS AUCActivity ON FAD.AssetUnderConstructionActivityId = AUCActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   DPBudgetM ON   FAD.DownPaymentBudgetMasterId =   DPBudgetM.Id
                                LEFT JOIN HKP.Budget AS   DPBudget ON   DPBudget.Id =   DPBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS DPActivity ON FAD.DownPaymentActivityId = DPActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   CABudgetM ON   FAD.ClearingAccountBudgetMasterId =   CABudgetM.Id
                                LEFT JOIN HKP.Budget AS   CABudget ON   CABudget.Id =   CABudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS CAActivity ON FAD.ClearingAccountActivityId = CAActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   GOSBudgetM ON   FAD.GainOnSaleOfAssetBudgetMasterId = GOSBudgetM.Id
                                LEFT JOIN HKP.Budget AS   GOSBudget ON   GOSBudget.Id =   GOSBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS GOSActivity ON FAD.GainOnSaleOfAssetActivityId = GOSActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   LOSBudgetM ON   FAD.LossOnSaleOfAssetBudgetMasterId =   LOSBudgetM.Id
                                LEFT JOIN HKP.Budget AS   LOSBudget ON   LOSBudget.Id =   LOSBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS LOSActivity ON FAD.LossOnSaleOfAssetActivityId = LOSActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   LODBudgetM ON   FAD.LossOnDisposalAssetBudgetMasterId =   LODBudgetM.Id
                                LEFT JOIN HKP.Budget AS   LODBudget ON   LODBudget.Id =   LODBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS LODActivity ON FAD.LossOnDisposalAssetActivityId = LODActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   LEVBudgetM ON   FAD.LessValueAssetBudgetMasterId =   LEVBudgetM.Id
								LEFT JOIN HKP.Budget AS   LEVBudget ON   LEVBudget.Id =   LEVBudgetM.BudgetId
								LEFT JOIN HKP.Activity AS LEActivity ON FAD.LessValueAssetActivityId = LEActivity.Id
                               WHERE ISNULL(FAD.AccumulatedDepreciationGLId, '') = ''
                            OR ISNULL(FAD.DepreciationGLId, '') = ''OR ISNULL(FAD.AssetUnderConstructionGLId, '') = ''
                            OR ISNULL(FAD.DownPaymentGLId, '') = '' OR ISNULL(FAD.ClearingAccountGLId, '') = ''
							OR ISNULL(FAD.GainOnSaleOfAssetGLId, '') = '' OR ISNULL(FAD.LossOnSaleOfAssetGLId, '') = ''
							OR ISNULL(FAD.LossOnDisposalAssetGLId, '') = '' ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        #region Salary head Gl report

        //Report service level
        private DataTable GetAutoMailReportData(/*string companyGroupId, string companyId, string plantId*/)
        {
            var sql = @"SELECT distinct SH.SalaryHeadID,SH.SalaryHead,SH.HeadType,SH.HeadCategory,SGL.DrDirectGLId,SGL.DrDirectBudgetMasterId,SGL.DrDirectActivityId
                            ,SGL.DrInDirectGLId,SGL.DrInDirectBudgetMasterId,SGL.DrInDirectActivityId
                            ,DGL.AccountCode+' - '+DGL.UserName DirectGLName,DB.UserName DirectBudgetName,DA.UserName DirectActivityName 
                            ,IGL.AccountCode+' - '+IGL.UserName InDirectGLName,IB.UserName InDirectBudgetName,IA.UserName InDirectActivityName 
							,SGL.CrDirectGLId,SGL.CrDirectBudgetMasterId,SGL.CrDirectActivityId
                            ,SGL.CrInDirectGLId,SGL.CrInDirectBudgetMasterId,SGL.CrInDirectActivityId
                            ,CDGL.AccountCode+' - '+CDGL.UserName CrDirectGLName,CDB.UserName CrDirectBudgetName,CDA.UserName CrDirectActivityName 
                            ,CIGL.AccountCode+' - '+CIGL.UserName CrInDirectGLName,CIB.UserName CrInDirectBudgetName,CIA.UserName CrInDirectActivityName 
                            ,0 Flag,SGL.Id,SGL.SalaryPayableGroup
                            FROM dbo.SalaryHead SH
                            LEFT JOIN MST.SalaryHeadGL SGL  ON SH.SalaryHeadID=SGL.SalaryHeadId
                            LEFT JOIN HKP.GLGeneralInfo DGL ON DGL.Id=SGL.DrDirectGLId
                            LEFT JOIN MST.BudgetMaster DBM ON DBM.Id=SGL.DrDirectBudgetMasterId
                            LEFT JOIN HKP.Budget DB ON DB.Id=DBM.BudgetId
                            LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=SGL.DrInDirectGLId
                            LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=SGL.DrInDirectBudgetMasterId
                            LEFT JOIN HKP.Budget IB ON IB.Id=IBM.BudgetId
                            LEFT JOIN HKP.Activity DA ON DA.Id=SGL.DrDirectActivityId
                            LEFT JOIN HKP.Activity IA ON IA.Id=SGL.DrInDirectActivityId

							LEFT JOIN HKP.GLGeneralInfo CDGL ON CDGL.Id=SGL.CrDirectGLId
                            LEFT JOIN MST.BudgetMaster CDBM ON CDBM.Id=SGL.CrDirectBudgetMasterId
                            LEFT JOIN HKP.Budget CDB ON CDB.Id=CDBM.BudgetId
                            LEFT JOIN HKP.GLGeneralInfo CIGL ON CIGL.Id=SGL.CrInDirectGLId
                            LEFT JOIN MST.BudgetMaster CIBM ON CIBM.Id=SGL.CrInDirectBudgetMasterId
                            LEFT JOIN HKP.Budget CIB ON CIB.Id=CIBM.BudgetId
                            LEFT JOIN HKP.Activity CDA ON CDA.Id=SGL.CrDirectActivityId
                            LEFT JOIN HKP.Activity CIA ON CIA.Id=SGL.CrInDirectActivityId
                            Where ISNULL(SH.HeadCategory,'') not in ('CTC','Gross','Total Gross') ";

            return _sqlRepository.GetDataTable(sql);
        }


  

        public IWorkbook GetSalaryHeadGlReport(/*string CompanyGroupId, string CompanyId, string PlantId*/)  
        {
            // var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            DataTable dtAutoMailReportList = GetAutoMailReportData(/*CompanyGroupId, CompanyId, PlantId*/);

            //DataTable dtCompanyCurrency = _sqlRepository.GetDataTable(@"select CR.* from org.Company c
            //                                            inner join scs.Currency CR ON CR.Id=c.BaseCurrencyId
            //                                            where C.Id='" + CompanyId + "'");

            if (dtAutoMailReportList.Rows.Count == 0)
                throw new Exception("No data found");

            worksheet.Name = "SalaryHeadGLList";

            int COL = 1; int ROW = 5;
            int startCol = COL;

            worksheet[ROW, COL].Text = "SL. No";
            int colSLNO = COL;
            worksheet[ROW, COL].ColumnWidth = 5;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            //worksheet[ROW, COL].Text = "Salary Head Id";
            //int colPartyPlantName = COL;
            //worksheet[ROW, COL].ColumnWidth = 15;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            ////worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //COL++;

            worksheet[ROW, COL].Text = "Salary Head";
            int colSalaryHead = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Head Type";
            int colHeadType = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Head Category";
            int colHeadCategory = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

   
            //worksheet[ROW, COL].Text = "DrDirect GLId";
            //int colDrDirectGlId = COL;
            //worksheet[ROW, COL].ColumnWidth = 10;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //COL++;

            //worksheet[ROW, COL].Text = "DrInDirect GLId";
            //int colDrInDirectGLId = COL;
            //worksheet[ROW, COL].ColumnWidth = 10;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //COL++;

            
            //worksheet[ROW, COL].Text = "DrInDirectBudgetMasterId";
            //int colDrInDirectBudgetMasterId = COL;
            //worksheet[ROW, COL].ColumnWidth = 15;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            //worksheet[ROW, COL].Text = "DrInDirectActivityId";
            //int colDrInDirectActivityId = COL;
            //worksheet[ROW, COL].ColumnWidth = 12;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //COL++;

            worksheet[ROW, COL].Text = "Direct GLName";
            int colDirectGLName = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "DirectBudgetName";
            int colDirectBudgetName = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;
            worksheet[ROW, COL].Text = "DirectActivityName";
            int colDirectActivityName = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "CrDirectGLName";
            int colCrDirectGLName = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "CrDirectBudgetName";
            int colCrDirectBudgetName = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "CrDirectActivityName";
            int colCrDirectActivityName = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;



            worksheet[ROW, COL].Text = "InDirectGLName";
            int colInDirectGLName = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            //worksheet[ROW, COL].Text = "Payable " + '(' + dtCompanyCurrency.Rows[0]["Code"].ToString() + ')';
            //int colBooksPayable = COL;
            //worksheet[ROW, COL].ColumnWidth = 15;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //COL++;
            //InDirectBudgetName

            worksheet[ROW, COL].Text = "InDirectBudgetName";
            int colInDirectBudgetName = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "InDirectActivityName";
            int colInDirectActivityName = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

   
            //worksheet[ROW, COL].Text = "CrDirectGLId";
            //int colCrDirectGLId = COL;
            //worksheet[ROW, COL].ColumnWidth = 10;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;


            //worksheet[ROW, COL].Text = "CrDirectBudgetMasterId";
            //int colCrDirectBudgetMasterId = COL;
            //worksheet[ROW, COL].ColumnWidth = 15;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;


            //worksheet[ROW, COL].Text = "CrDirectActivityId";
            //int colCrDirectActivityId = COL;
            //worksheet[ROW, COL].ColumnWidth = 12;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            //worksheet[ROW, COL].Text = "CrInDirectGLId";
            //int colCrInDirectGLId = COL;
            //worksheet[ROW, COL].ColumnWidth = 10;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            //worksheet[ROW, COL].Text = "CrInDirectBudgetMasterId";
            //int colCrInDirectBudgetMasterId = COL;
            //worksheet[ROW, COL].ColumnWidth = 10;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            //worksheet[ROW, COL].Text = "CrInDirectActivityId";
            //int colCrInDirectActivityId = COL;
            //worksheet[ROW, COL].ColumnWidth = 10;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

       


            worksheet[ROW, COL].Text = "CrInDirectGLName";
            int colCrInDirectGLName = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;
            worksheet[ROW, COL].Text = "CrInDirectBudgetName";
            int colCrInDirectBudgetName = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;
            worksheet[ROW, COL].Text = "CrInDirectActivityName";
            int colCrInDirectActivityName = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;
            worksheet[ROW, COL].Text = "SalaryPayableGroup";
            int colSalaryPayableGroup = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;



            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            // sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Black;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            ROW++;

            for (int i = 0; i < dtAutoMailReportList.Rows.Count; i++)
            {
                worksheet[ROW, colSLNO].Number = (i + 1);
                //worksheet[ROW, colPartyPlantName].Text = dtAutoMailReportList.Rows[i]["SalaryHeadID"].ToString();

                worksheet[ROW, colSalaryHead].Text = dtAutoMailReportList.Rows[i]["SalaryHead"].ToString();
                worksheet[ROW, colHeadType].Text = dtAutoMailReportList.Rows[i]["HeadType"].ToString();
                worksheet[ROW, colHeadCategory].Text = dtAutoMailReportList.Rows[i]["HeadCategory"].ToString();

                //worksheet[ROW,  colDrDirectGlId].Text = dtAutoMailReportList.Rows[i]["DrDirectGLId"].ToString();
                //worksheet[ROW, colDrInDirectGLId].Text = dtAutoMailReportList.Rows[i]["DrInDirectGLId"].ToString();
                //worksheet[ROW, colDrInDirectBudgetMasterId].Text = dtAutoMailReportList.Rows[i]["DrInDirectBudgetMasterId"].ToString();
                //worksheet[ROW, colDrInDirectActivityId].Text = dtAutoMailReportList.Rows[i]["DrInDirectActivityId"].ToString();
                worksheet[ROW, colDirectGLName].Text = dtAutoMailReportList.Rows[i]["DirectGLName"].ToString();

                worksheet[ROW, colDirectBudgetName].Text = dtAutoMailReportList.Rows[i]["DirectBudgetName"].ToString();
                worksheet[ROW, colDirectActivityName].Text = dtAutoMailReportList.Rows[i]["DirectActivityName"].ToString();
                worksheet[ROW, colInDirectGLName].Text = dtAutoMailReportList.Rows[i]["InDirectGLName"].ToString();
                worksheet[ROW, colInDirectBudgetName].Text = dtAutoMailReportList.Rows[i]["InDirectBudgetName"].ToString();
                worksheet[ROW, colInDirectActivityName].Text = dtAutoMailReportList.Rows[i]["InDirectActivityName"].ToString();

                //worksheet[ROW, colCrDirectGLId].Text = dtAutoMailReportList.Rows[i]["CrDirectGLId"].ToString();
                //worksheet[ROW, colCrDirectBudgetMasterId].Text = dtAutoMailReportList.Rows[i]["CrDirectBudgetMasterId"].ToString();
                //worksheet[ROW, colCrInDirectActivityId].Text = dtAutoMailReportList.Rows[i]["CrDirectActivityId"].ToString();
                //worksheet[ROW, colCrInDirectGLId].Text = dtAutoMailReportList.Rows[i]["CrInDirectGLId"].ToString();
                //worksheet[ROW, colCrInDirectBudgetMasterId].Text = dtAutoMailReportList.Rows[i]["CrInDirectBudgetMasterId"].ToString();

                //worksheet[ROW, colCrInDirectActivityId].Text = dtAutoMailReportList.Rows[i]["CrInDirectActivityId"].ToString();

                worksheet[ROW, colCrDirectGLName].Text = dtAutoMailReportList.Rows[i]["CrDirectGLName"].ToString();
                worksheet[ROW, colCrDirectBudgetName].Text = dtAutoMailReportList.Rows[i]["CrDirectBudgetName"].ToString();
                worksheet[ROW, colCrDirectActivityName].Text = dtAutoMailReportList.Rows[i]["CrDirectActivityName"].ToString();
                worksheet[ROW, colCrDirectGLName].Text = dtAutoMailReportList.Rows[i]["CrInDirectGLName"].ToString();
                worksheet[ROW, colCrInDirectBudgetName].Text = dtAutoMailReportList.Rows[i]["CrInDirectBudgetName"].ToString();
                worksheet[ROW, colCrInDirectActivityName].Text = dtAutoMailReportList.Rows[i]["CrInDirectActivityName"].ToString();
                worksheet[ROW, colSalaryPayableGroup].Text = dtAutoMailReportList.Rows[i]["SalaryPayableGroup"].ToString();
 



                //worksheet[ROW, colDocDate].DateTime = Convert.ToDateTime(dtAutoMailReportList.Rows[i]["DocDate"].ToString());
                //worksheet[ROW, colDocDate].NumberFormat = "dd-MMM-yyyy";
                // worksheet.Range[ROW, colDocDate].NumberFormat = "hh:mm AM/PM";
                //sheet1.Range[xlsRow, iInTime].NumberFormat = "hh:mm AM/PM";
                //sheet1.Range[xlsRow, iInTime].DateTime = Convert.ToDateTime(dvBioDvAC[i]["InTimeShow"].ToString());

                //worksheet[ROW, colVoucherDate].DateTime = Convert.ToDateTime(dtAutoMailReportList.Rows[i]["EntryDate"].ToString());
                //worksheet[ROW, colVoucherDate].NumberFormat = "dd-MMM-yyyy";
                //worksheet[ROW, colPostingDate].DateTime = Convert.ToDateTime(dtAutoMailReportList.Rows[i]["PostingDate"].ToString());
                //worksheet[ROW, colPostingDate].NumberFormat = "dd-MMM-yyyy";

                //worksheet[ROW, colPayable].Number = clsStaticInfo.dbl(dtAutoMailReportList.Rows[i]["Payable"].ToString());
                //worksheet[ROW, colPayable].NumberFormat = clsStaticInfo.NumberFormat(2);


                //if (dtAutoMailReportList.Rows[i]["GRNDate"].ToString() != "")
                //{
                //    worksheet[ROW, colGRNDate].DateTime = Convert.ToDateTime(dtAutoMailReportList.Rows[i]["GRNDate"].ToString());
                //    worksheet[ROW, colGRNDate].NumberFormat = "dd-MMM-yyyy";
                //}
                //else
                //{
                //    worksheet[ROW, colGRNDate].Text = dtAutoMailReportList.Rows[i]["GRNDate"].ToString();

                //}

                //worksheet[ROW, colCurrencyCode].Text = dtAutoMailReportList.Rows[i]["CurrencyCode"].ToString();


                //worksheet[ROW, colType].Text = dtAutoMailReportList.Rows[i]["Type"].ToString();
                //worksheet[ROW, colBooksPayable].Number = clsStaticInfo.dbl(dtAutoMailReportList.Rows[i]["PayableBooks"].ToString());
                //worksheet[ROW, colBooksPayable].NumberFormat = clsStaticInfo.NumberFormat(2);

                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;



            ReportUtility reportUtility = new ReportUtility();

            worksheet[3, 1].Text = "Salary Head GL List";

            // reportUtility.PlantHeader(ref worksheet, endCol, "Last 10 Days Creation Payable List"/*, PlantId*/);
            //reportUtility.CompanyPlantHeader(ref worksheet, endCol, "Party Payment Status Summary",/* identity.CompanyId, identity.PlantName,*/ "");
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            // worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            #region Freeze Panes

            worksheet.IsDisplayZeros = false;
            worksheet.UsedRange["A6"].FreezePanes();
            worksheet.FirstVisibleColumn = 1;
            worksheet.FirstVisibleRow = 6;

            #endregion Freeze Panes



            return workbook;
        }
        #endregion

    }
}