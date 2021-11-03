#region

using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.FixedAssets;
using Library.Model.Systems;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion

namespace Library.Service.FixedAssets
{
    public class FixedAssetMasterVendorReconGLService : Service<FixedAssetMasterVendorReconGL>, IFixedAssetMasterVendorReconGLService
    {
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<FixedAssetMasterVendorReconGL> _comgroupdesingationgroupRepository;

        public FixedAssetMasterVendorReconGLService(
            IRepositoryAsync<FixedAssetMasterVendorReconGL> comgroupdesingationgroupRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(comgroupdesingationgroupRepository, unitOfWork, pkGeneratorService)
        {
            _comgroupdesingationgroupRepository = comgroupdesingationgroupRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion

        public void InsertOrUpdate(IEnumerable<FixedAssetMasterGL> masterlist, IEnumerable<FixedAssetMasterVendorReconGL> childListUI)
        {
            try
            {
                // Check(entity);
                var pk = GetMaxNumber();
                IEnumerable<FixedAssetMasterVendorReconGL> childListDb = GetChildList(masterlist);

                foreach (var m in masterlist)
                {
                    var db_c = childListDb.Where(a => a.FixedAssetMasterGLId == m.Id);
                    // OutChild(childListUI, out childListDb);
                    foreach (var item in childListUI)
                    {
                        var ob_c = db_c.Where(a => a.PartyAccountGroupId == item.PartyAccountGroupId).FirstOrDefault();
                        var temp = item.Copy<FixedAssetMasterVendorReconGL>();
                        if (ob_c == null || string.IsNullOrEmpty(ob_c.Id))
                        {
                            pk.MaxNumber++;
                            temp.FixedAssetMasterGLId = m.Id;
                            temp.Id = pk.MaxNumber.ToString();
                            temp.FixedAssetMasterId = m.FixedAssetMasterId;
                            InsertGraph(temp);
                        }
                        else
                        {
                            //log
                            ob_c.VendorReconGLId = (string.IsNullOrEmpty(item.VendorReconGLId) ? ob_c.VendorReconGLId : item.VendorReconGLId);
                            ob_c.VendorReconBudgetMasterId = (string.IsNullOrEmpty(item.VendorReconBudgetMasterId) ? ob_c.VendorReconBudgetMasterId : item.VendorReconBudgetMasterId);
                            ob_c.VendorReconActivityId = (string.IsNullOrEmpty(item.VendorReconActivityId) ? ob_c.VendorReconActivityId : item.VendorReconActivityId);
                            UpdateGraph(ob_c);
                        }
                    }//child
                }//parent
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
            finally
            {
            }
        }

        private IEnumerable<FixedAssetMasterVendorReconGL> GetChildList(IEnumerable<FixedAssetMasterGL> masterList)
        {
            try
            {
                var masterId = "";
                foreach (var item in masterList)
                {
                    if (masterId == "")
                    {
                        masterId += "'" + item.Id + "'";
                    }
                    else
                    {
                        masterId += ",'" + item.Id + "'";
                    }
                }
                string _sql = @" SELECT * FROM [HKP].[FixedAssetMasterVendorReconGL]
                                WHERE FixedAssetMasterGLId In(" + masterId + ");";
                return _comgroupdesingationgroupRepository.SqlQuery<FixedAssetMasterVendorReconGL>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void OutChild(IEnumerable<FixedAssetMasterVendorReconGL> from_ui, out List<FixedAssetMasterVendorReconGL> from_db)
        {
            from_db = null;
            try
            {                                                                                                                                         //List<OperationTimeCaptureMaster> fromdblist = ieList.ToList();
                foreach (var ui in from_ui)
                {
                    var db = from_db.Where(a => a.FixedAssetMasterGLId == ui.FixedAssetMasterGLId).FirstOrDefault();
                    if (db == null)//new
                    {
                        db = ui;
                    }//edit
                }//foreach
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(FixedAssetMasterVendorReconGL), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        private PKGenerator GetMaxNumber()
        {
            return base.GetMaxNumber(nameof(FixedAssetMasterVendorReconGL), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void DeleteGraph(string masterId)
        {
            try
            {
                var data = Query(r => r.FixedAssetMasterGLId == masterId).Select().ToList();
                if (data != null)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        base.DeleteGraph(data[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public FixedAssetMasterVendorReconGL FindbyFKId(string key)
        {
            return Query(m => m.FixedAssetMasterGLId == key).Select().FirstOrDefault();
        }
    }
}