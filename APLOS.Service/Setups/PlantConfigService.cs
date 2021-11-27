#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Setups;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion Using

namespace Library.Service.Setups
{
    public class PlantConfigService : Service<PlantConfig>, IPlantConfigService
    {
        private readonly string PlantConfigTable = " " + DbSchema.SystemConfigurationAndSetup + ".[PlantConfig] ";
        private readonly string PlantTable = " " + DbSchema.Organizations + ".[Plant] ";
        private readonly string ProcessTable = " " + DbSchema.HKP + ".[" + DbTable.Process + "] ";
        private readonly string CompanyGroupTable = " " + DbSchema.Organizations + ".[CompanyGroup] ";
        private readonly string CompanyTable = " " + DbSchema.Organizations + ".[Company] ";

        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<PlantConfig> _plantConfigRepository;
        private readonly IPrdOrdSettingService _PrdOrdSettingService;

        public PlantConfigService(
            IRepositoryAsync<PlantConfig> plantConfigRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IPrdOrdSettingService PrdOrdSettingService) : base(plantConfigRepository, unitOfWork, pkGeneratorService)
        {
            _plantConfigRepository = plantConfigRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _PrdOrdSettingService = PrdOrdSettingService;
        }

        #endregion Constructor

        public GridModel GetMasterSearchData(GridParameter parameters)
        {
            parameters.CmdText = @"SELECT pc.Id, pc.BuyerApplicable
                          , pc.PlantId, p2.UserName AS PlantName
                          , pc.CompanyGroupId, cg.UserName AS CompanyGroupName
                          , pc.CompanyId, c.UserName AS CompanyName,PC.FabRollPrefix,pc.MachineBudgetLevel
                    FROM SCS.PlantConfig pc
                    LEFT JOIN ORG.Plant p2 ON pc.PlantId = p2.Id
                    LEFT JOIN ORG.CompanyGroup cg ON pc.CompanyGroupId=cg.Id
                    LEFT JOIN ORG.Company c ON pc.CompanyId=c.Id";
            return _sqlRepository.GetGridData(parameters);
        }

        public GridModel GetPlantList(string CompanyId)
        {
            var sql = @"SELECT p.Id AS [Value], p.CompanyGroupId, p.CompanyId, p.AddressMasterId, p.[Sequence],
                            p.Code, p.ShortName, p.StandardName, p.UserName AS [Text], p.VATResistrationNo, p.Description,
                            p.Remarks, p.Active, p.Archive
                            FROM " + PlantTable + @"  as p
                            WHERE p.CompanyId='" + CompanyId + @"'
                            AND p.Active = 1 AND p.Archive = 0";

            return _sqlRepository.GetGridData(new GridParameter { CmdText = sql });
        }

        public GridModel GetProcessList()
        {
            var sql = @"SELECT p.Id AS [Value], p.UserName AS [Text],
                            p.Active, p.Archive
                            FROM " + ProcessTable + @"  as p
                            WHERE p.Active = 1 AND p.Archive = 0";
            return _sqlRepository.GetGridData(new GridParameter { CmdText = sql });
        }

        public GridModel Query(GridParameter parameters)
        {
            parameters.CmdText = $"SELECT * FROM {DbSchema.SystemConfigurationAndSetup}.[PlantConfig]";
            return _sqlRepository.GetGridData(parameters);
        }

        public void SaveMaster(PlantConfig from_ui, out string masterID, IEnumerable<PrdOrdSetting> prdOrdSetting)
        {
            var flag = false;
            masterID = "";
            try
            {
                PlantConfig from_db = null;
                from_db = GetMaster(from_ui.Id).FirstOrDefault();

                if (from_db == null)
                {
                    from_db = new PlantConfig
                    {
                        ModelState = ModelState.Added,
                        Id = GetPK(),
                        FabRollPrefix = from_ui.FabRollPrefix,
                        BuyerApplicable = from_ui.BuyerApplicable,
                        PlantId = from_ui.PlantId,
                        CompanyId = from_ui.CompanyId,
                        CompanyGroupId = from_ui.CompanyGroupId,
                        BlanketDefaultLength=from_ui.BlanketDefaultLength,
                        BlanketDefaultWidth = from_ui.BlanketDefaultWidth,
                        IsBlanketDefaultLengthValuesChangeable = from_ui.IsBlanketDefaultLengthValuesChangeable,
                        IsBlanketDefaultWidthValuesChangeable = from_ui.IsBlanketDefaultWidthValuesChangeable,
                        IsAfterWashShrinkageOnActual = from_ui.IsAfterWashShrinkageOnActual,
                        WeekendforProductionOrder = from_ui.WeekendforProductionOrder,
                        Operation = from_ui.Operation,
                        OperationInProductionBookingWillBeCapturebyBulletin = from_ui.OperationInProductionBookingWillBeCapturebyBulletin,
                        MachineBudgetLevel = from_ui.MachineBudgetLevel,
                        IsMachineChangeableinBulletinTemplate = from_ui.IsMachineChangeableinBulletinTemplate,
                        IsProductionHourOpen = from_ui.IsProductionHourOpen
                    };
                }
                else
                {
                    from_db.ModelState = ModelState.Modified;

                    from_db.FabRollPrefix = from_ui.FabRollPrefix;
                    from_db.BuyerApplicable = from_ui.BuyerApplicable;
                    from_db.PlantId = from_ui.PlantId;
                    from_db.CompanyId = from_ui.CompanyId;
                    from_db.CompanyGroupId = from_ui.CompanyGroupId;
                    from_db.BlanketDefaultLength = from_ui.BlanketDefaultLength;
                    from_db.BlanketDefaultWidth = from_ui.BlanketDefaultWidth;
                    from_db.IsBlanketDefaultLengthValuesChangeable = from_ui.IsBlanketDefaultLengthValuesChangeable;
                    from_db.IsBlanketDefaultWidthValuesChangeable = from_ui.IsBlanketDefaultWidthValuesChangeable;
                    from_db.IsAfterWashShrinkageOnActual = from_ui.IsAfterWashShrinkageOnActual;
                    from_db.WeekendforProductionOrder = from_ui.WeekendforProductionOrder;
                    from_db.Operation = from_ui.Operation;
                    from_db.OperationInProductionBookingWillBeCapturebyBulletin = from_ui.OperationInProductionBookingWillBeCapturebyBulletin;
                    from_db.MachineBudgetLevel = from_ui.MachineBudgetLevel;
                    from_db.IsMachineChangeableinBulletinTemplate = from_ui.IsMachineChangeableinBulletinTemplate;
                    from_db.IsProductionHourOpen = from_ui.IsProductionHourOpen;
                }
                AuditService.Log(from_db);
                InsertOrUpdateGraph(from_db);

                _PrdOrdSettingService.InsertOrUpdateGraph(prdOrdSetting);

                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                masterID = from_db.Id;
            }
            catch (Exception) { throw; }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public IEnumerable<PlantConfig> GetMaster(string Id)
        {
            var _sql = "SELECT * FROM " + PlantConfigTable + " WHERE Id='" + Id + "'";
            return _sqlRepository.GetModelCollection<PlantConfig>(_sql);
        }

        public IEnumerable<object> GetPlantConfigByPlant(string PlantId)
        {
            var _sql = @"SELECT PlantId, BuyerApplicable, FabRollPrefix FROM " + PlantConfigTable + " WHERE PlantId='" + PlantId + "'";
            return _sqlRepository.GetDataCollection(_sql);
        }

        public IEnumerable<object> GetMasterDataById(string MasterId)
        {
            var _sql = @"SELECT pc.Id,pc.BuyerApplicable
                                  , pc.PlantId, p2.UserName AS PlantName
                                  , pc.CompanyGroupId, cg.UserName AS CompanyGroupName
                                  , pc.CompanyId, c.UserName AS CompanyName
                                  ,BlanketDefaultLength,BlanketDefaultWidth,IsBlanketDefaultLengthValuesChangeable,IsBlanketDefaultWidthValuesChangeable
                                  ,IsAfterWashShrinkageOnActual,PC.FabRollPrefix,PC.IsProductionOrderCreatedAfterConfirmationOfSO,PC.WeekendforProductionOrder,PC.Operation,PC.OperationInProductionBookingWillBeCapturebyBulletin,pc.MachineBudgetLevel
                                  ,PC.IsMachineChangeableinBulletinTemplate,pc.IsProductionHourOpen
                            FROM SCS.PlantConfig pc
                            LEFT JOIN ORG.Plant p2 ON pc.PlantId = p2.Id
                            LEFT JOIN ORG.CompanyGroup cg ON pc.CompanyGroupId=cg.Id
                            LEFT JOIN ORG.Company c ON pc.CompanyId=c.Id
                            WHERE pc.Id='" + MasterId + @"' ";
            return _sqlRepository.GetDataCollection(_sql);
        }

        public IEnumerable<object> GetPlantWiseDuplicateData(string Id, string CompanyGroupId, string CompanyId, string PlantId)
        {
            var _sql = @"SELECT pc.Id, pc.BuyerApplicable,
                            pc.PlantId, p2.UserName AS PlantName,
                            pc.CompanyGroupId, cg.UserName AS CompanyGroupName,
                            pc.CompanyId, c.UserName AS CompanyName
                            FROM " + PlantConfigTable + @" pc
                            LEFT JOIN " + PlantTable + @" p2 ON pc.PlantId = p2.Id
                            LEFT JOIN " + CompanyGroupTable + @" cg ON pc.CompanyGroupId=cg.Id
                            LEFT JOIN " + CompanyTable + @" c ON pc.CompanyId=c.Id
                            WHERE pc.CompanyGroupId='" + CompanyGroupId + @"'
                            AND pc.CompanyId='" + CompanyId + @"'
                            AND pc.PlantId='" + PlantId + @"'
                            AND pc.Id<>'" + Id + @"'";
            return _sqlRepository.GetDataCollection(_sql);
        }

        public override void Insert(PlantConfig entity)
        {
            try
            {
                entity.Id = GetPK();
                base.Insert(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        private string GetPK()
        {
            return "PC" + GetAutoNumber(nameof(PlantConfig), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public IEnumerable<object> GetCboList()
        {
            return from m in base.Query().Select().OrderBy(r => r.PlantId)
                   select new { Text = m.FabRollPrefix, Value = m.Id };
        }

        public PlantConfig GetPlantConfig(string PK)
        {
            var _sql = "select * from " + PlantConfigTable + " where Id='" + PK + "'";
            return _plantConfigRepository.SelectQuery(_sql).FirstOrDefault();
        }

        public PlantConfig GetPlantConfigByPlantId(string plantId)
        {
            var _sql = "select * from SCS.PlantConfig where PlantId='" + plantId + "'";
            return _plantConfigRepository.SelectQuery(_sql, null).FirstOrDefault();
        }

        public void DeleteMaster(string masterid)
        {
            var flag = false;
            try
            {
                //master
                DelMaster(masterid, out PlantConfig from_db);
                Delete(from_db);

                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private void DelMaster(string id, out PlantConfig from_db)
        {
            from_db = null;
            from_db = GetPlantConfig(id);
            if (from_db.Id == null || from_db.Id == "")
            {
                throw new Exception("No Row found against Id: [" + id + "]");
            }
            from_db.ModelState = ModelState.Deleted;
        }
    }
}