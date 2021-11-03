#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Payrolls;
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

namespace Library.Service.Setups
{
    public class PlantSalaryHeadSequenceService : Service<PlantSalaryHeadSequence>, IPlantSalaryHeadSequenceService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public PlantSalaryHeadSequenceService(
            IRepositoryAsync<PlantSalaryHeadSequence> plantSalaryHeadSequenceRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository) : base(plantSalaryHeadSequenceRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InsertORUpdate(IEnumerable<PlantSalaryHeadSequence> entities)
        {
            var flag = false;
            try
            {
                if (entities == null)
                    throw new CustomException("Please insert legal designation");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _unitOfWork.BeginTransaction();
                flag = true;
                var pk = GetMaxNumber(nameof(PlantSalaryHeadSequence), PKGeneratorEnum.Auto, identity.CompanyGroupId, DateTime.Now);
                foreach (var item in entities)
                {
                    pk.MaxNumber++;
                    item.Id = pk.MaxNumber.ToString();
                    item.CompanyGroupId = identity.CompanyGroupId;
                    InsertGraph(item);
                }
                var plantId = entities.First().PlantId;
                var dbList = GetDBList(plantId);
                if (dbList != null && dbList.Count() > 0)
                {
                    if (entities == null)
                    {
                        foreach (var item in dbList)
                        {
                            base.Delete(item);
                        }
                    }
                    else
                    {
                        foreach (var item in dbList)
                        {
                            if (!entities.Any(t => t.Id == item.Id && t.PlantId == item.PlantId && t.SalaryHeadId == item.SalaryHeadId))
                            {
                                base.Delete(item);
                            }
                        }
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public override void Update(PlantSalaryHeadSequence entity)
        {
            try
            {
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public void Delete(string Id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                base.Delete(Id);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        private IEnumerable<PlantSalaryHeadSequence> GetDBList(string plantId)
        {
            var _sql = @" SELECT * FROM [MST].[PlantSalaryHeadSequence] WHERE PlantId='" + plantId + "'";
            return _sqlRepository.GetModelCollection<PlantSalaryHeadSequence>(_sql);
        }

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT * FROM ORG.PlantSalaryHeadSequence ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetSalaryHead(string companyGroupId)
        {
            var _sql = @"SELECT * FROM [dbo].[SalaryHead] AS S
                        WHERE GroupID='" + companyGroupId + "' ORDER BY S.SalaryHead";
            return _sqlRepository.GetDataCollection(_sql);
        }

        public IEnumerable<object> QueryGraph(string plantId, string companyGroupId)
        {
            var _sql = @"SELECT B.SalaryHead,B.Description,B.HeadCategory,A.* FROM [MST].[PlantSalaryHeadSequence] AS A
                                LEFT OUTER JOIN [dbo].[SalaryHead] AS B ON A.SalaryHeadId=B.SalaryHeadID
                                WHERE A.PlantId='" + plantId + "' AND A.CompanyGroupId='" + companyGroupId + "' ORDER BY A.Sequence";
            return _sqlRepository.GetDataCollection(_sql);
        }
    }
}