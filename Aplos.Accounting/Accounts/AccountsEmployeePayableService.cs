using ConnectionManager;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Enums;
using Library.Service.Logs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.Accounting.Accounts
{
    public class AccountsEmployeePayableService
	{
        private readonly ISqlRepository _sqlRepository;
        public AccountsEmployeePayableService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }
        public List<Dictionary<string, object>> GetCbo(string companyGroupId, string companyId)
        {
            try
            {
                var sql = @"SELECT ETT.Id AS EmployeeTransactionTypeId, ETT.UserName AS EmployeeTransactionTypeName
                            , EGL.AdvanceGLId, EGL.AdvanceGLCode, EGL.AdvanceGLName
                            , EGL.AdvanceBudgetMasterId, EGL.AdvanceBudgetCode, EGL.AdvanceBudgetName
                            , EGL.AdvanceActivityId, EGL.AdvanceActivityCode, EGL.AdvanceActivityName
                            , EGL.PayableGLId, EGL.PayableGLCode, EGL.PayableGLName
                            , EGL.PayableBudgetMasterId, EGL.PayableBudgetCode, EGL.PayableBudgetName
                            , EGL.PayableActivityId, EGL.PayableActivityCode, EGL.PayableActivityName
                            , ETT.AdvanceType
                            FROM [HKP].[EmployeeTransactionType] ETT
                            LEFT JOIN(
	                            SELECT ETTGL.EmployeeTransactionTypeId, ETTGL.AdvanceGLId, AGGI.AccountCode AS AdvanceGLCode, AGGI.UserName AS AdvanceGLName
	                            , ETTGL.AdvanceBudgetMasterId, AB.Code AS AdvanceBudgetCode, AB.UserName AS AdvanceBudgetName
	                            , ETTGL.AdvanceActivityId, AA.Code AS AdvanceActivityCode, AA.UserName AS AdvanceActivityName
	                            , ETTGL.PayableGLId, PGGI.AccountCode AS PayableGLCode, PGGI.UserName AS PayableGLName
	                            , ETTGL.PayableBudgetMasterId, PB.Code AS PayableBudgetCode, PB.UserName AS PayableBudgetName
	                            , ETTGL.PayableActivityId, PA.Code AS PayableActivityCode, PA.UserName AS PayableActivityName
	                            FROM [HKP].[EmployeeTransactionTypeGL] AS ETTGL
	                            LEFT JOIN [HKP].[GLGeneralInfo] AS AGGI ON AGGI.Id=ETTGL.AdvanceGLId
	                            LEFT JOIN [MST].[BudgetMaster] AS ABM ON ABM.Id=ETTGL.AdvanceBudgetMasterId
	                            LEFT JOIN [HKP].[Budget] AS AB ON AB.Id=ABM.BudgetId
	                            LEFT JOIN [HKP].[Activity] AS AA ON AA.Id=ETTGL.AdvanceActivityId
	                            LEFT JOIN [HKP].[GLGeneralInfo] AS PGGI ON PGGI.Id=ETTGL.PayableGLId
	                            LEFT JOIN [MST].[BudgetMaster] AS PBM ON PBM.Id=ETTGL.PayableBudgetMasterId
	                            LEFT JOIN [HKP].[Budget] AS PB ON PB.Id=PBM.BudgetId
	                            LEFT JOIN [HKP].[Activity] AS PA ON PA.Id=ETTGL.PayableActivityId
	                            LEFT JOIN [ORG].[Company] AS C ON C.COAId=ETTGL.COAId
	                            WHERE C.Id='" + companyId + @"'
                            )AS EGL ON EGL.EmployeeTransactionTypeId=ETT.Id
                            WHERE ETT.Active=1 AND ETT.CompanyGroupId='" + companyGroupId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public List<Dictionary<string, object>> GetCboAdvanceSalary(string companyGroupId, string companyId)
        {
            try
            {
                var sql = @"SELECT ETT.Id AS EmployeeTransactionTypeId, ETT.UserName AS EmployeeTransactionTypeName
                            , EGL.AdvanceGLId, EGL.AdvanceGLCode, EGL.AdvanceGLName
                            , EGL.AdvanceBudgetMasterId, EGL.AdvanceBudgetCode, EGL.AdvanceBudgetName
                            , EGL.AdvanceActivityId, EGL.AdvanceActivityCode, EGL.AdvanceActivityName, EGL.AdvanceBudgetMasterActivityId
                            , EGL.PayableGLId, EGL.PayableGLCode, EGL.PayableGLName
                            , EGL.PayableBudgetMasterId, EGL.PayableBudgetCode, EGL.PayableBudgetName
                            , EGL.PayableActivityId, EGL.PayableActivityCode, EGL.PayableActivityName
                            , ETT.AdvanceType
                            FROM [HKP].[EmployeeTransactionType] ETT
                            LEFT JOIN(
	                            SELECT ETTGL.EmployeeTransactionTypeId, ETTGL.AdvanceGLId, AGGI.AccountCode AS AdvanceGLCode, AGGI.UserName AS AdvanceGLName
	                            , ETTGL.AdvanceBudgetMasterId, AB.Code AS AdvanceBudgetCode, AB.UserName AS AdvanceBudgetName
	                            , ETTGL.AdvanceActivityId, AA.Code AS AdvanceActivityCode, AA.UserName AS AdvanceActivityName,BMA.Id AdvanceBudgetMasterActivityId
	                            , ETTGL.PayableGLId, PGGI.AccountCode AS PayableGLCode, PGGI.UserName AS PayableGLName
	                            , ETTGL.PayableBudgetMasterId, PB.Code AS PayableBudgetCode, PB.UserName AS PayableBudgetName
	                            , ETTGL.PayableActivityId, PA.Code AS PayableActivityCode, PA.UserName AS PayableActivityName
	                            FROM [HKP].[EmployeeTransactionTypeGL] AS ETTGL
	                            LEFT JOIN [HKP].[GLGeneralInfo] AS AGGI ON AGGI.Id=ETTGL.AdvanceGLId
	                            LEFT JOIN [MST].[BudgetMaster] AS ABM ON ABM.Id=ETTGL.AdvanceBudgetMasterId
	                            LEFT JOIN [HKP].[Budget] AS AB ON AB.Id=ABM.BudgetId
	                            LEFT JOIN [HKP].[Activity] AS AA ON AA.Id=ETTGL.AdvanceActivityId
	                            LEFT JOIN [MST].[BudgetMasterActivity] BMA ON BMA.ActivityId=ETTGL.AdvanceActivityId AND BMA.BudgetMasterId=ETTGL.AdvanceBudgetMasterId
	                            LEFT JOIN [HKP].[GLGeneralInfo] AS PGGI ON PGGI.Id=ETTGL.PayableGLId
	                            LEFT JOIN [MST].[BudgetMaster] AS PBM ON PBM.Id=ETTGL.PayableBudgetMasterId
	                            LEFT JOIN [HKP].[Budget] AS PB ON PB.Id=PBM.BudgetId
	                            LEFT JOIN [HKP].[Activity] AS PA ON PA.Id=ETTGL.PayableActivityId
	                            LEFT JOIN [ORG].[Company] AS C ON C.COAId=ETTGL.COAId
	                            WHERE C.Id='" + companyId + @"'
                            )AS EGL ON EGL.EmployeeTransactionTypeId=ETT.Id
                            WHERE ETT.Active=1 AND ETT.CompanyGroupId='" + companyGroupId + @"' AND ETT.AdvanceType='Salary' ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public List<Dictionary<string, object>> GetEmpTrnTypeByAdvanceType(string companyGroupId, string companyId,string advanceType)
        {
            try
            {
                var sql = @"SELECT ETT.Id AS EmployeeTransactionTypeId, ETT.UserName AS EmployeeTransactionTypeName
                            , EGL.AdvanceGLId, EGL.AdvanceGLCode, EGL.AdvanceGLName
                            , EGL.AdvanceBudgetMasterId, EGL.AdvanceBudgetCode, EGL.AdvanceBudgetName
                            , EGL.AdvanceActivityId, EGL.AdvanceActivityCode, EGL.AdvanceActivityName
                            , EGL.PayableGLId, EGL.PayableGLCode, EGL.PayableGLName
                            , EGL.PayableBudgetMasterId, EGL.PayableBudgetCode, EGL.PayableBudgetName
                            , EGL.PayableActivityId, EGL.PayableActivityCode, EGL.PayableActivityName
                            , ETT.AdvanceType
                            FROM [HKP].[EmployeeTransactionType] ETT
                            LEFT JOIN(
	                            SELECT ETTGL.EmployeeTransactionTypeId, ETTGL.AdvanceGLId, AGGI.AccountCode AS AdvanceGLCode, AGGI.UserName AS AdvanceGLName
	                            , ETTGL.AdvanceBudgetMasterId, AB.Code AS AdvanceBudgetCode, AB.UserName AS AdvanceBudgetName
	                            , ETTGL.AdvanceActivityId, AA.Code AS AdvanceActivityCode, AA.UserName AS AdvanceActivityName
	                            , ETTGL.PayableGLId, PGGI.AccountCode AS PayableGLCode, PGGI.UserName AS PayableGLName
	                            , ETTGL.PayableBudgetMasterId, PB.Code AS PayableBudgetCode, PB.UserName AS PayableBudgetName
	                            , ETTGL.PayableActivityId, PA.Code AS PayableActivityCode, PA.UserName AS PayableActivityName
	                            FROM [HKP].[EmployeeTransactionTypeGL] AS ETTGL
	                            LEFT JOIN [HKP].[GLGeneralInfo] AS AGGI ON AGGI.Id=ETTGL.AdvanceGLId
	                            LEFT JOIN [MST].[BudgetMaster] AS ABM ON ABM.Id=ETTGL.AdvanceBudgetMasterId
	                            LEFT JOIN [HKP].[Budget] AS AB ON AB.Id=ABM.BudgetId
	                            LEFT JOIN [HKP].[Activity] AS AA ON AA.Id=ETTGL.AdvanceActivityId
	                            LEFT JOIN [HKP].[GLGeneralInfo] AS PGGI ON PGGI.Id=ETTGL.PayableGLId
	                            LEFT JOIN [MST].[BudgetMaster] AS PBM ON PBM.Id=ETTGL.PayableBudgetMasterId
	                            LEFT JOIN [HKP].[Budget] AS PB ON PB.Id=PBM.BudgetId
	                            LEFT JOIN [HKP].[Activity] AS PA ON PA.Id=ETTGL.PayableActivityId
	                            LEFT JOIN [ORG].[Company] AS C ON C.COAId=ETTGL.COAId
	                            WHERE C.Id='" + companyId + @"'
                            )AS EGL ON EGL.EmployeeTransactionTypeId=ETT.Id
                            WHERE ETT.Active=1 AND  ETT.AdvanceType='"+ advanceType + "' AND ETT.CompanyGroupId='" + companyGroupId + "' ";
                return _sqlRepository.GetDataCollection(sql);
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
