#region using

using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.FixedAssets;
using Library.Service.Core;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion using

namespace Library.Service.FixedAssets
{
    public class FixedAssetRegisterSkuValueService : Service<FixedAssetRegisterSkuValue>, IFixedAssetRegisterSkuValueService
    {
        #region Constructor

        private readonly IRepositoryAsync<FixedAssetRegisterSkuValue> _assetItemValueRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;

        public FixedAssetRegisterSkuValueService(
            IRepositoryAsync<FixedAssetRegisterSkuValue> assetItemValueRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(assetItemValueRepository, unitOfWork, pkGeneratorService)
        {
            _assetItemValueRepository = assetItemValueRepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InsertOrUpdateGraph(IEnumerable<FixedAssetRegisterSkuValue> entity, string assetItemId, string fixedAssetRegisterId)
        {
            if (entity != null)
            {
                var pk = GetMaxNumber(nameof(FixedAssetRegisterSkuValue), PKGeneratorEnum.Auto, null, DateTime.Now);
                foreach (var item in entity)
                {
                    var temp = item.Copy<FixedAssetRegisterSkuValue>();
                    if (string.IsNullOrEmpty(temp.Id))
                    {
                        pk.MaxNumber++;
                        temp.Id = pk.MaxNumber.ToString();
                        temp.FixedAssetRegisterId = fixedAssetRegisterId;
                        temp.AssetItemId = assetItemId;
                        InsertGraph(temp);
                    }
                    else if (!string.IsNullOrEmpty(temp.Id))
                    {
                        base.DeleteGraph(temp);
                    }
                    else
                    {
                        UpdateGraph(temp);
                    }
                }
            }
        }

        public void DeleteGraph(string masterId)
        {
            var savedList = Query(r => r.FixedAssetRegisterId == masterId).Select();
            if (savedList != null)
            {
                foreach (var item in savedList.ToList())
                {
                    base.DeleteGraph(item);
                }
            }
        }
    }
}