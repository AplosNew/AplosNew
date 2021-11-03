#region Using

using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Materials;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Reflection;

#endregion Using

namespace Library.Service.Materials
{
    public class FabricRollMasterDefectService : Service<FabricRollMasterDefect>, IFabricRollMasterDefectService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;

        public FabricRollMasterDefectService(
            IRepositoryAsync<FabricRollMasterDefect> FabricRollMasterDefectRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(FabricRollMasterDefectRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(FabricRollMasterDefect), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Insert(FabricRollMasterDefect entity)
        {
            try
            {
                entity.Id = GetPK();
                InsertGraph(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }
        public void DeleteGraph(string id)
        {
                if (string.IsNullOrEmpty(id))
                    throw new CustomException("No defect found to delete");
                var entity = Find(id);
                base.Delete(entity);
               
        }
        public IEnumerable<FabricRollMasterDefect> QueryList(string value)
        {
            try
            {
                string _sql = @"SELECT
                                    f.Id,
									f.[MaterialMasterId]
									,a.BlanketLengthBeforeWash SettingBlanketLengthBeforeWash
									,a.BlanketWidthBeforeWash SettingBlanketWidthBeforeWash
                                    ,f.RollNo
                                    ,f.HasDefectShade
                                    ,f.SpecialShadeType
                                    ,f.ShrinkagePercentageWidth
                                    ,f.Shade
                                    ,f.Width
                                    ,f.VendorRollNo
                                    ,f.VendorLotNo
                                    ,f.VendorQty
                                    ,f.VendorWidth
                                    ,f.PlantId
                                    ,f.QualityPass
                                    ,f.IsBlanketCutApplicable
                                    ,f.BlanketLengthBeforeWash
                                    ,f.BlanketWidthBeforeWash
                                    ,f.BlanketLengthAfterWash
                                    ,f.BlanketWidthAfterWash
									,P.IsAfterWashShrinkageOnActualFR
									,M.UserName MaterialMasterName
                                    FROM
                                    TRN.FabricRollMasterDefect f
									left outer join mst.FabricRollManagementSettings a on a.MaterialMasterId=f.MaterialMasterId
									LEFT OUTER JOIN SCS.PlantConfig P ON F.PlantId=P.PlantId
									LEFT OUTER JOIN MST.MaterialMaster M ON F.MaterialMasterId=M.Id
                                    WHERE RollNo='" + value + "'";
                var a = _sqlRepository.GetModelCollection<FabricRollMasterDefect>(_sql, null);
                return a;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}