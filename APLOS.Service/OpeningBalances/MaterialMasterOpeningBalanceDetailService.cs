#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.OpeningBalances;
using Library.Model.Systems;
using Library.Model.Vouchers;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using Library.Service.Vouchers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.OpeningBalances
{
    public class MaterialMasterOpeningBalanceDetailService : Service<MaterialMasterOpeningBalanceDetail>, IMaterialMasterOpeningBalanceDetailService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IVoucherService _voucherService;
        private readonly IRepositoryAsync<VoucherDetail> _voucherDetailRepository;
        private readonly IRepositoryAsync<VoucherDetailCurrency> _voucherDetailCurrencyService;

        public MaterialMasterOpeningBalanceDetailService(
            IRepositoryAsync<MaterialMasterOpeningBalanceDetail> FixedAssetClassRepository,
            IPKGeneratorService pkGeneratorService
            , IVoucherService voucherService
            , IRepositoryAsync<VoucherDetail> voucherDetailRepository
            , IRepositoryAsync<VoucherDetailCurrency> voucherDetailCurrencyService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(FixedAssetClassRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _voucherService = voucherService;
            _voucherDetailRepository = voucherDetailRepository;
            _voucherDetailCurrencyService = voucherDetailCurrencyService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public GridModel Query(GridParameter parameters, string companyId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT FAM.username AS FixedAssetMasterName,
                                        GL1.username AS AccDepreciation,
                                        GL.username  AS AssetGLName,
                                        FOB.*
                                         FROM   TRN.MaterialMasterOpeningBalance AS FOB
                                        LEFT JOIN mst.fixedassetmaster AS FAM
                                               ON FAM.id = FOB.fixedassetmasterid
                                        LEFT JOIN hkp.glgeneralinfo AS GL
                                               ON GL.id = FOB.fixedassetglid
                                        LEFT JOIN hkp.glgeneralinfo AS GL1
                                               ON GL1.id = FOB.accdepreciationglid
                                         WHERE  FOB.ispark = 1
                                        AND FOB.companyId = '" + companyId + @"'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public GridModel GetFixedAssetOpeningBalanceById(GridParameter parameters, string id, string companyId)
        {
            try
            {
                parameters.CmdText = @"SELECT FAM.username AS FixedAssetMasterName,
                                        GL1.username AS AccDepreciation,
                                        GL.username  AS AssetGLName,
                                        FOB.*
                                         FROM   TRN.MaterialMasterOpeningBalance AS FOB
                                        LEFT JOIN mst.fixedassetmaster AS FAM
                                               ON FAM.id = FOB.fixedassetmasterid
                                        LEFT JOIN hkp.glgeneralinfo AS GL
                                               ON GL.id = FOB.fixedassetglid
                                        LEFT JOIN hkp.glgeneralinfo AS GL1
                                               ON GL1.id = FOB.accdepreciationglid
                                         WHERE  FOB.ispark = 1
                                        AND FOB.companyId = '" + companyId + @"' AND FOB.Id='" + id + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        private string GetVoucherNo(string companyId)
        {
            return "REG" + "-" + GetAutoNumber("Voucher" + companyId, PKGeneratorEnum.Daily, null, DateTime.Now);
        }

        public void Post(IEnumerable<MaterialMasterOpeningBalanceDetail> entities)
        {
            var flag = false;
            try
            {
                // Check(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var pk = GetMaxNumber();
                var voucherlist = new List<Voucher>();
                var voucherPk = base.GetMaxNumber("Voucher", PKGeneratorEnum.Auto, null, DateTime.Now);
                var voucherDetaillist = new List<VoucherDetail>();
                var voucherDetailPk = base.GetMaxNumber("VoucherDetail", PKGeneratorEnum.Auto, null, DateTime.Now);
                var voucherDetailCurrencylist = new List<VoucherDetailCurrency>();
                var voucherDetailCurrencyPk = base.GetMaxNumber("VoucherDetailCurrency", PKGeneratorEnum.Auto, null, DateTime.Now);
                foreach (var item in entities)
                {
                    var voucher = new Voucher();
                    pk.MaxNumber++;
                    voucherPk.MaxNumber++;
                    if (!string.IsNullOrEmpty(item.Id))
                    {
                        UpdateGraph(item);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            item.Id = pk.MaxNumber.ToString();
                            InsertGraph(item);
                        }
                    }

                    #region ****************Voucher************

                    voucher.Id = voucherPk.MaxNumber.ToString();
                    voucher.VoucherNo = GetVoucherNo(identity.CompanyId);
                    voucher.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + "-" + voucher.Id;
                    voucher.CompanyGroupId = identity.CompanyGroupId;
                    voucher.VoucherDate = DateTime.Now;
                    voucher.DocDate = DateTime.Now;
                    voucher.PostingDate = DateTime.Now;
                    voucher.VoucherTypeId = "1";
                    voucher.Narration = "Fixed Asset Opening Balance";
                    voucherlist.Add(voucher);

                    #endregion ****************Voucher************

                    #region ************VoucherDetail*************

                    var voucherDetail = new VoucherDetail();
                    string voucherdetailId1 = null;
                    string voucherdetailId2 = null;
                    voucherDetailPk.MaxNumber++;
                    voucherDetail.Id = voucherDetailPk.MaxNumber.ToString();
                    voucherdetailId1 = voucherDetail.Id;
                    voucherDetail.VoucherId = voucher.Id;
                    voucherDetail.GLGeneralInfoId = item.AssetGLId;
                    voucherDetail.Narration = "Fixed Asset Opening Balance for Fixed Asset GL";

                    voucherDetaillist.Add(voucherDetail);

                    if (!string.IsNullOrEmpty(item.AccumulatedDepreciationGLId))
                    {
                        voucherDetail = new VoucherDetail();
                        voucherDetailPk.MaxNumber++;
                        voucherDetail.Id = voucherDetailPk.MaxNumber.ToString();
                        voucherdetailId2 = voucherDetail.Id;
                        voucherDetail.VoucherId = voucher.Id;
                        voucherDetail.GLGeneralInfoId = item.AccumulatedDepreciationGLId;
                        voucherDetail.Narration = "Fixed Asset Opening Balance for Accumulated Depriciation GL";

                        voucherDetaillist.Add(voucherDetail);
                    }

                    #endregion ************VoucherDetail*************

                    #region *************VoucherDetailCurrency*************

                    var voucherDetailCurrency = new VoucherDetailCurrency();

                    #region *******FixedAssetGL Currency*********

                    #region *******BaseCurrency**********

                    voucherDetailCurrencyPk.MaxNumber++;
                    voucherDetailCurrency.Id = voucherDetailCurrencyPk.MaxNumber.ToString();
                    voucherDetailCurrency.VoucherId = voucher.Id;
                    voucherDetailCurrency.VoucherDetailId = voucherdetailId1;
                    voucherDetailCurrencylist.Add(voucherDetailCurrency);

                    #endregion *******BaseCurrency**********

                    #region *******GroupCurrency**********

                    voucherDetailCurrency = new VoucherDetailCurrency();
                    voucherDetailCurrencyPk.MaxNumber++;
                    voucherDetailCurrency.Id = voucherDetailCurrencyPk.MaxNumber.ToString();
                    voucherDetailCurrency.VoucherId = voucher.Id;
                    voucherDetailCurrency.VoucherDetailId = voucherdetailId1;
                    voucherDetailCurrencylist.Add(voucherDetailCurrency);

                    #endregion *******GroupCurrency**********

                    #region *******HardCurrency**********

                    voucherDetailCurrency = new VoucherDetailCurrency();
                    voucherDetailCurrencyPk.MaxNumber++;
                    voucherDetailCurrency.Id = voucherDetailCurrencyPk.MaxNumber.ToString();
                    voucherDetailCurrency.VoucherId = voucher.Id;
                    voucherDetailCurrency.VoucherDetailId = voucherdetailId1;
                    voucherDetailCurrencylist.Add(voucherDetailCurrency);

                    #endregion *******HardCurrency**********

                    #endregion *******FixedAssetGL Currency*********

                    #region *******Accumulate Depreciation GL Currency*********

                    if (!string.IsNullOrEmpty(item.AccumulatedDepreciationGLId))
                    {
                        voucherDetailCurrency = new VoucherDetailCurrency();

                        #region *******BaseCurrency**********

                        voucherDetailCurrencyPk.MaxNumber++;
                        voucherDetailCurrency.Id = voucherDetailCurrencyPk.MaxNumber.ToString();
                        voucherDetailCurrency.VoucherId = voucher.Id;
                        voucherDetailCurrency.VoucherDetailId = voucherdetailId2;
                        voucherDetailCurrencylist.Add(voucherDetailCurrency);

                        #endregion *******BaseCurrency**********

                        #region *******GroupCurrency**********

                        voucherDetailCurrency = new VoucherDetailCurrency();
                        voucherDetailCurrencyPk.MaxNumber++;
                        voucherDetailCurrency.Id = voucherDetailCurrencyPk.MaxNumber.ToString();
                        voucherDetailCurrency.VoucherId = voucher.Id;
                        voucherDetailCurrency.VoucherDetailId = voucherdetailId2;
                        voucherDetailCurrencylist.Add(voucherDetailCurrency);

                        #endregion *******GroupCurrency**********

                        #region *******HardCurrency**********

                        voucherDetailCurrency = new VoucherDetailCurrency();
                        voucherDetailCurrencyPk.MaxNumber++;
                        voucherDetailCurrency.Id = voucherDetailCurrencyPk.MaxNumber.ToString();
                        voucherDetailCurrency.VoucherId = voucher.Id;
                        voucherDetailCurrency.VoucherDetailId = voucherdetailId2;
                        voucherDetailCurrency.ParallelCurrencyId = item.CurrencyId;
                        voucherDetailCurrency.FromCurrencyId = item.CurrencyId;
                        voucherDetailCurrencylist.Add(voucherDetailCurrency);

                        #endregion *******HardCurrency**********
                    }

                    #endregion *******Accumulate Depreciation GL Currency*********

                    #endregion *************VoucherDetailCurrency*************
                }
                // TODO: Shamim Khan changes this.
                //_voucherService.InsertGraphRange(voucherlist);
                _voucherDetailRepository.InsertGraphRange(voucherDetaillist);
                _voucherDetailCurrencyService.InsertGraphRange(voucherDetailCurrencylist);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void Park(IEnumerable<MaterialMasterOpeningBalanceDetail> entity)
        {
            var flag = false;
            try
            {
                // Check(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                var pk = GetMaxNumber();
                foreach (var item in entity)
                {
                    pk.MaxNumber++;
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        item.Id = pk.MaxNumber.ToString();
                        InsertGraph(item);
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private PKGenerator GetMaxNumber()
        {
            return base.GetMaxNumber("MaterialMasterOpeningBalance", PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Update(MaterialMasterOpeningBalanceDetail entity)
        {
            var flag = false;
            try
            {
                // Check(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                // If department row inacitve
                UpdateGraph(entity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteGraph(string id)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new CustomException(string.Format(ResourcesCore.IsNull, "FixedAssetGLCurrency Id"));

                _unitOfWork.BeginTransaction();
                flag = true;
                var entity = Find(id);
                // If section row inactive
                base.DeleteGraph(entity);
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
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name,
                    MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Organization.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
    }
}