#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.IE;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.IE
{
    public class SkillGroupingService : Service<SkillGrouping>, ISkillGroupingService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public SkillGroupingService(
            IRepositoryAsync<SkillGrouping> SizeGroupRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork) :
            base(SizeGroupRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public decimal GetAutoSequence()
        {
            try
            {
                return base.Query().Select().Max(r => r.Sequence + 1);
            }
            catch (Exception)
            {
                return 1.00M;
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(SkillGrouping), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void Check(SkillGrouping entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName);
        }

        public override void Insert(SkillGrouping entity)
        {
            try
            {
                Check(entity);
                entity.Id ="SG"+ GetPK();
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                //throw new CustomException(ex.Message, ex,
                //Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                //ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
                throw ex;
            }
        }

        public override void Update(SkillGrouping entity)
        {
            try
            {
                Check(entity);
                base.Update(entity);
            }
            catch (Exception ex)
            {
                //throw new CustomException(ex.Message, ex,
                //Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                //ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
                throw ex;

            }
        }

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = $"SELECT * FROM [HKP].[SizeGroup]";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetCbo()
        {
            try
            {

                try
                {

                    var sql = @"Select Code +' - '+UserName As Text, Id As Value from [SCS].[LegalSalaryGrade]";
                    return _sqlRepository.GetDataCollection(sql);

                }
                catch (Exception ex)
                {
                    throw new CustomException(ex.Message, ex,
                        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }




        #region start for Skill grouping


         public IEnumerable<object> GetSkillgrouping()
         {
            try
            {
                var sql = @"SELECT
                      SG.Id
                    , SG.CompanyGroupId
                    , sg.Sequence
                    , sg.Code
                    , sg.ShortName
                    , sg.UserName
                    ,sg.StandardName 
                    , sg.Description
                    , sg.Remarks
                    , sg.LegalSalaryGradeId
                    , LSG.UserName AS LegalSalaryG
                    , sg.Grouping
                    , sg.DesignationCategory
                    , sg.StandardSalary
                    , sg.active
                    FROM scs.SkillGrouping SG
                    LEFT JOIN [SCS].[LegalSalaryGrade] LSG ON LSG.Id= SG.LegalSalaryGradeId Order by sg.Sequence DESC ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }



        public IEnumerable<object> GetDataBySkillGroupingId(string id)
        {
            try
            {
                var sql = @"SELECT  
                            SG.Id
                            ,SG.CompanyGroupId
                            ,sg.Sequence
                            ,sg.Code
                            ,sg.ShortName
                            ,sg.UserName
                            ,sg.StandardName 
                            ,sg.Description
                            ,sg.Remarks
                            ,sg.LegalSalaryGradeId
                            ,LSG.UserName LegalSalaryGrade
                            ,sg.Grouping
                            ,sg.DesignationCategory
                            ,sg.StandardSalary
                            ,sg.Active
                            FROM scs.SkillGrouping SG
                            LEFT JOIN [SCS].[LegalSalaryGrade] LSG ON LSG.Id=SG.LegalSalaryGradeId
                            WHERE SG.Id='" + id + "' Order by sg.Sequence DESC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetDataByMachineMasterId(string id)
        {
            throw new NotImplementedException();
        }

        //void ISkillGroupingService.Check(SkillGrouping model)
        //{
        //    throw new NotImplementedException();
        //}


        #endregion
    }
}