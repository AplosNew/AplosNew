#region Using

using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.External;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.External
{
    public class DocumentActivityService : Service<DocumentActivity>, IDocumentActivityService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<DocumentActivity> _documentActivityRepository;

        public DocumentActivityService(
              IRepositoryAsync<DocumentActivity> documentActivityRepository
            , IUnitOfWork unitOfWork
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            ) : base(documentActivityRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _documentActivityRepository = documentActivityRepository;
        }

        #endregion Constructor

        #region Operation

        private void CheckDuplicateFile(string activityId, string fileName)
        {
            try
            {
                string sql = @"IF EXISTS(SELECT 1 FROM(
                                 SELECT [FileName] AS CheckingColumn FROM dbo.DocumentActivity WHERE ActivityId='" + activityId + @"'
                                 ) A WHERE CheckingColumn = '" + fileName + "') SELECT 1 ELSE SELECT 0 RETURN";
                var data = Convert.ToBoolean(_documentActivityRepository.SqlQuery<int>(sql).Single());
                if (data)
                    throw new CustomException("This file is already exists!!!");
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Designation.ToString()));
            }
        }

        public Dictionary<string, object> GetDocFile(string id)
        {
            try
            {
                var sql = @"Select FileId, FileName From [dbo].[DocumentActivity]  Where Id='" + id + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetDocumentList(string activityId)
        {
            try
            {
                var sql = @"SELECT D.*,ACT.Name AS Activity, EMP.Name PreparedByInCaseOfOtherName
                                       FROM dbo.DocumentActivity D
                                       LEFT OUTER JOIN dbo.ActivityEmp ACT ON ACT.Id=D.ActivityId
                                       LEFT OUTER JOIN dbo.Employee EMP ON D.PreparedByInCaseOfOther=EMP.Id
                                       Where D.ActivityId='" + activityId + @"'
                                       Order By ACT.Name ";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        #endregion Operation
    }
}