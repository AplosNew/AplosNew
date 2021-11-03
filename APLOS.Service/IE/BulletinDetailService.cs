using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.IE;
using Library.Service.Core;
using Library.Service.Machines;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Library.Service.IEnumerable
{
    public class BulletinDetailService : Service<BulletinDetail>, IBulletinDetailService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<BulletinDetail> _bulletinedetailRepository;

        public BulletinDetailService(
            IRepositoryAsync<BulletinDetail> bulletinedetailRepository,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IOperationService operationService
            , IPKGeneratorService pkGeneratorService) :
            base(bulletinedetailRepository, unitOfWork, pkGeneratorService)
        {
            _bulletinedetailRepository = bulletinedetailRepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public GridModel GetSearchData(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                parameters.CmdText = @"SELECT	m.Id
		                            , m.AllotedManpower
		                            , m.AllotedWorkstation
		                            , m.Manpowertype
		                            , m.Remark
		                            , m.Sequence
		                            , m.MachineExecutiontype
		                            , z.Code [Zone]
		                            , c.Code [Component]
		                            , dg.Code [DesignationGroup]
		                            , op.Code [Operation]
		                            , mt.Code [MachineType]
		                            , m.ZoneId
		                            , m.RequiredManpower
		                            , m.OperationTargetPerHour
		                            , m.ComponentId
		                            , m.DesignationgroupId
		                            , m.MachineTypeId
		                            , m.OperationId
		                            , m.IsLastOperation

		                            FROM [TRN].[BulletinDetail] AS m left outer join
		                            [HKP].[FGZone] z  ON z.Id=m.ZoneId left outer join
		                            [HKP].[FGComponent] c  ON c.Id=m.ComponentId left outer join
		                            [HKP].[DesignationGroup] dg  ON dg.Id=m.DesignationgroupId  left outer join
		                            [MST].[Operation] op ON op.Id=m.OperationId  left outer join
		                            [MST].[MaterialMasterMachineProcess]  mt ON mt.Id=m.MachineTypeId
		                            WHERE   m.Companygroupid='" + identity.CompanyGroupId + "'";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string GetPK()
        {
            return "BD" + _pkGeneratorService.GetAutoNumber(nameof(BulletinDetail), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public IEnumerable<BulletinDetail> GetDetailList(string MasterId)
        {
            try
            {
                var _sql = "select * from [TRN].[BulletinDetail] where BulletinMasterId='" + MasterId + "' ";
                return _bulletinedetailRepository.SqlQuery<BulletinDetail>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetList(string companyGroupId, string masterId, string processId)
        {
            try
            {
                var _sql = @"SELECT M.Id, M.CompanyGroupId, M.CompanyId, M.BulletinMasterId
                            , M.ZoneId, M.ComponentId, M.OperationActionId
                            , M.Manpowertype, M.OperationId, M.MaterialMasterArticleId, M.MachineExecutiontype
                            , M.UserDefinedSPT, M.AllotedWorkstation, M.AllotedManpower, M.Sequence, M.IsPrintable
                            , M.IsDirect, M.IsLastOperation, M.Remark
                            , OP.UserName [OperationDescription], OA.UserName OperationActionName
                            , OT.UserName [OperationType], ART.StandardName AS Machine, MM.UserName AS AssetItem
                            , FAC.UserName AS FixedAssetCategory, FASC.UserName AS FixedAssetSubCategory, FAM.AssetType
                            , Z.UserName AS [Zone], C.UserName AS Component, M.ProcessId
                    FROM [TRN].[BulletinDetail] AS M
                    LEFT JOIN [HKP].[FGZone] Z  ON Z.Id=M.ZoneId
                    LEFT JOIN [HKP].[FGComponent] C  ON C.Id=M.ComponentId
                    LEFT JOIN [MST].[Operation] OP ON OP.Id=M.OperationId
                    LEFT JOIN [HKP].[OperationType] AS OT ON OP.OperationTypeId = OT.Id
                    LEFT JOIN [HKP].[OperationCategory] AS OC ON OP.OperationCategoryId = OC.Id
                    LEFT JOIN [HKP].[OperationAction] AS OA ON M.OperationActionId=OA.Id
                    LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON M.MaterialMasterArticleId=ART.Id
                    LEFT JOIN [MST].[MaterialMaster] AS MM ON ART.MaterialMasterId=MM.Id
                    LEFT JOIN [MST].[FixedAssetMaster] AS FAM ON MM.AssetMasterId = FAM.Id
                    LEFT JOIN [HKP].[FixedAssetCategory] AS FAC ON FAM.FixedAssetCategoryId = FAC.Id
                    LEFT JOIN [HKP].[FixedAssetSubCategory] AS FASC ON FAM.FixedAssetCategoryId = FASC.Id
                    WHERE M.Companygroupid='" + companyGroupId + "' and M.BulletinMasterId= '" + masterId + "' AND M.ProcessId='" + processId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<BulletinDetail> GetBulletinDetailList(string MasterId)
        {
            throw new NotImplementedException();
        }
    }
}