#region Using

using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Documents;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Employees
{
    public class ComplianceDocumentPositonCodeService : Service<ComplianceDocumentPositonCode>, IComplianceDocumentPositonCodeService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ComplianceDocumentPositonCodeService(
            IRepositoryAsync<ComplianceDocumentPositonCode> ComplianceDocumentPositonCodeRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork) :
            base(ComplianceDocumentPositonCodeRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InsertOrUpdate(IEnumerable<ComplianceDocumentPositonCode> entities, string masterId)
        {
            try
            {
                if (entities != null)
                {
                    var count = 0;

                    foreach (var item in entities)
                    {
                        count++;
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            item.Id = masterId + "-" + count;
                            item.ComplianceDocumentId = masterId;
                            InsertGraph(item);
                        }
                        else
                        {
                            UpdateGraph(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
            finally
            {
            }
        }

        public IEnumerable<object> Query(string complianceDocumentId)
        {
            try
            {
                var sql = @"
SELECT PO.UserName,PO.PositionCode,PO.Division,PO.Department,PO.Section,PO.Designation,CDPC.* FROM [HKP].[ComplianceDocumentPositonCode] AS CDPC
	LEFT OUTER JOIN (SELECT rd.Id
, rd.UserName
, DivisionId
, (SELECT UserName FROM  [ORG].[Division] WHERE Id=rd.DivisionId) AS [Division]
, DepartmentId
, (SELECT UserName FROM  [ORG].[Department] WHERE Id=rd.DepartmentId) AS [Department]
, SectionId
, (SELECT UserName FROM  [ORG].[Section] WHERE Id=rd.SectionId) AS [Section]
, D.UserName AS Designation,rd.Code As PositionCode FROM  [ORG].[Position] as rd INNER JOIN [HKP].[Designation] AS D ON D.Id=rd.DesignationId) AS PO ON CDPC.PositionId= PO.Id
                            WHERE CDPC.ComplianceDocumentId='" + complianceDocumentId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void DeleteWithMaster(string Id)
        {
            try
            {
                var data = base.Query(r => r.ComplianceDocumentId == Id).Select().ToList();
                if (data != null)
                {
                    for (int i = 0; i < data.Count(); i++)
                    {
                        DeleteGraph(data[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }
    }
}