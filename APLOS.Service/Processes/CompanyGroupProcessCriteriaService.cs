#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Processes;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Processes
{
    public class CompanyGroupProcessCriteriaService : Service<CompanyGroupProcessCriteria>, ICompanyGroupProcessCriteriaService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public CompanyGroupProcessCriteriaService(
            IRepositoryAsync<CompanyGroupProcessCriteria> companyGroupProcessCriteriaRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(companyGroupProcessCriteriaRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT SC.[Sequence]
		                                            ,SC.Code
		                                            ,SC.ShortName
		                                            ,SC.StandardName
		                                            ,SC.UserName
		                                            ,SC.[Description]
		                                            ,SC.Remarks
		                                            ,SC.Active
		                                            ,SC.Id
                                            FROM [" + DbSchema.HKP + @"].[" + DbTable.CompanyGroupProcessCriteria + @"] AS CGSC
                                            INNER JOIN [" + DbSchema.HKP + @"].[" + DbTable.ProcessCriteria + @"] AS SC ON SC.Id=CGSC.ProcessCriteriaId
                                            WHERE CGSC.CompanyGroupId='" + identity.CompanyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public IEnumerable<object> GetCbo(string companyGroupId)
        {
            try
            {
                return from m in base.Query(r => r.CompanyGroupId == companyGroupId && r.Active).Include(r => r.ProcessCriteria).Select().OrderBy(r => r.ProcessCriteria.UserName)
                       select new { Text = m.ProcessCriteria.UserName, Value = m.ProcessCriteriaId };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public IEnumerable<ComboModel> GetCbo()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var _sql = @"SELECT p.Id [Value], p.UserName [Text] FROM .[HKP].[ProcessCriteria] p
                                  left outer join (select * from [HKP].[CompanyGroupProcessCriteria] where CompanyGroupId='" + identity.CompanyGroupId + "') g  on g.ProcessCriteriaId=p.Id";
                return _sqlRepository.GetCombo(_sql, "Value", "Text");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<ComboModel> GetWeightUomCbo(string materialMasterId)
        {
            try
            {
                var _sql = @"Select mmauom.AlternativeUOMId [Value], uom.UserName [Text], 0 IsBase From [MST].[MaterialMasterAlternativeUOM] mmauom
                                Left Outer Join [SCS].[UnitOfMeasurement] uom on uom.Id=mmauom.AlternativeUOMId
                                Where mmauom.MaterialMasterId='" + materialMasterId + @"'
                                union
                                Select mm.BaseUOMId [Value], uom.UserName [Text], 1 IsBase From [MST].[MaterialMaster] mm
                                Left Outer Join [SCS].[UnitOfMeasurement] uom on uom.Id=mm.BaseUOMId
                                Where mm.Id='" + materialMasterId + "'";
                return _sqlRepository.GetCombo(_sql, "Value", "Text");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(CompanyGroupProcessCriteria), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public override void InsertGraph(CompanyGroupProcessCriteria entity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                entity.Id = GetPK();
                entity.CompanyGroupId = identity.CompanyGroupId;
                base.InsertGraph(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public void UpdateGraph(string buyerCategoryId, bool active)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var data_Db = base.Query(r => r.CompanyGroupId == identity.CompanyGroupId && r.ProcessCriteriaId == buyerCategoryId).Select().FirstOrDefault();
            if (data_Db != null)
            {
                data_Db.Active = active;
                data_Db.ModelState = ModelState.Modified;
                AuditService.Log(data_Db);
                base.UpdateGraph(data_Db);
            }
        }

        public void DeleteGraph(string buyerCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var data_Db = base.Query(r => r.CompanyGroupId == identity.CompanyGroupId && r.ProcessCriteriaId == buyerCategoryId).Select().FirstOrDefault();
            if (data_Db != null)
            {
                base.DeleteGraph(data_Db);
            }
        }
    }
}