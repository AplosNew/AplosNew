#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Products;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Products
{
    public class PlantWiseGateService : Service<PlantWiseGate>, IPlantWiseGateService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public PlantWiseGateService(
            IRepositoryAsync<PlantWiseGate> PlantWiseGateRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork) :
            base(PlantWiseGateRepository, unitOfWork, pkGeneratorService)
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
            catch
            {
                return 1.00M;
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(PlantWiseGate), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private DataSet CheckPreFix(string preFix, string id)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT PG.PreFix,P.UserName Plant from PlantWiseGate PG
                          LEFT JOIN ORG.Plant P ON P.Id=PG.PlantId
                          WHERE PG.PreFix='" + preFix + "' and PG.Id<>'" + id + "'"
            };
            return _sqlRepository.GetGridData(parameters).Source;
           
        }

        private void Check(PlantWiseGate entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code && r.PlantId == entity.PlantId);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName && r.PlantId == entity.PlantId);
        }

        public override void Insert(PlantWiseGate entity)
        {
            try
            {
                var preFix = CheckPreFix(entity.PreFix, entity.Id);
                if (preFix.Tables[0].Rows.Count>0)
                {
                    throw new Exception("PreFix "+entity.PreFix+" has already used in "+ preFix.Tables[0].Rows[0]["Plant"] + "");
                }
                Check(entity);
                entity.Id = GetPK();
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public override void Update(PlantWiseGate entity)
        {
            try
            {
                var preFix = CheckPreFix(entity.PreFix, entity.Id);
                if (preFix.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("PreFix: " + entity.PreFix + " has already used in Plant: " + preFix.Tables[0].Rows[0]["Plant"] + "");
                }
                Check(entity);
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT * FROM [dbo].[PlantWiseGate] Where PlantId='"+ plantId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetGateData(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT G.*, 0 AS Flag, P.UserName Plant FROM [dbo].[PlantWiseGate] G
                                     JOIN ORG.Plant P ON P.Id = G.PlantId";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetUserGateList(string userId)
        {
            try
            {
                var sql = @"SELECT UG.Id, UG.UserId, UG.PlantGateId, PG.Code, PG.[Sequence], PG.ShortName, PG.StandardName, PG.UserName AS GateName,P.UserName Plant
                            FROM [SEC].[UserPlantGate] AS UG
                            JOIN dbo.PlantWiseGate AS PG ON UG.PlantGateId=PG.Id
							JOIN ORG.Plant P ON P.Id=PG.PlantId
                            WHERE UG.UserId='" + userId + "' ORDER BY PG.UserName";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Securities.ToString()));
            }
        }

        public IEnumerable<object> GetCbo(string plantId)
        {
            try
            {
                return from m in base.Query(r=>r.PlantId==plantId).Select().OrderBy(r => r.UserName)
                       select new { Text = m.UserName, Value = m.Id };
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