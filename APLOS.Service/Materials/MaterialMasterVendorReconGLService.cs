using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Materials;
using Library.Model.Systems;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Library.Service.Materials
{
    public class MaterialMasterVendorReconGLService : Service<MaterialMasterVendorReconGL>, IMaterialMasterVendorReconGLService
    {
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<MaterialMasterVendorReconGL> _comgroupdesingationgroupRepository;

        public MaterialMasterVendorReconGLService(
            IRepositoryAsync<MaterialMasterVendorReconGL> comgroupdesingationgroupRepository,
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

        public void InsertOrUpdate(IEnumerable<MaterialMasterGL> masterlist, IEnumerable<MaterialMasterVendorReconGL> childListUI)
        {
            try
            {
                // Check(entity);
                var pk = GetMaxNumber();
                IEnumerable<MaterialMasterVendorReconGL> childListDb = GetChildList(masterlist);

                foreach (var m in masterlist)
                {
                    var db_c = childListDb.Where(a => a.MaterialMasterGLId == m.Id);
                    // OutChild(childListUI, out childListDb);
                    foreach (var item in childListUI)
                    {
                        var ob_c = db_c.Where(a => a.PartyAccountGroupId == item.PartyAccountGroupId).FirstOrDefault();
                        var temp = item.Copy<MaterialMasterVendorReconGL>();
                        if (ob_c == null || string.IsNullOrEmpty(ob_c.Id))
                        {
                            pk.MaxNumber++;
                            temp.MaterialMasterGLId = m.Id;
                            temp.Id = pk.MaxNumber.ToString();
                            temp.MaterialMasterId = m.MaterialMasterId;
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
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
            finally
            {
            }
        }

        //public void InsertOrUpdate(FixedAssetGL master, IEnumerable<FixedAssetVendorReconGL> entities)
        //{
        //    try
        //    {
        //        // Check(entity);
        //        var pk = GetPK();
        //        var count = 0;
        //        //List<FAAccountDeterminateWiseVendorRecon> ob = new List<FAAccountDeterminateWiseVendorRecon>();
        //        //DeleteGraph(master.Id);
        //        foreach (var item in entities)
        //        {
        //            if (!string.IsNullOrEmpty(item.VendorReconGLId))
        //            {
        //                count++;
        //                var temp = item.Copy<FixedAssetVendorReconGL>();
        //                temp.Id = pk + "." + count;
        //                temp.FixedAssetGLId = master.Id;
        //                temp.FixedAssetMasterId = master.FixedAssetMasterId;
        //                base.InsertGraph(temp);
        //            }
        //            else
        //            {
        //                base.UpdateGraph(item);

        //            }
        //            //_fAAccountDeterminateWiseVendorReconService.DeleteWithFK(item.FixedAssetGLId);
        //            //ob.Add(new FAAccountDeterminateWiseVendorRecon() { FixedAssetGLId=item.FixedAssetGLId,FAADVendorReconGLId=item.Id});
        //            //_fAAccountDeterminateWiseVendorReconService.InsertOrUpdate(ob);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
        //    }
        //    finally
        //    {
        //    }
        //}
        private IEnumerable<MaterialMasterVendorReconGL> GetChildList(IEnumerable<MaterialMasterGL> masterList)
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
                string _sql = @" SELECT * FROM [HKP].[MaterialMasterVendorReconGL]
                                WHERE MaterialMasterGLId In(" + masterId + ");";
                return _comgroupdesingationgroupRepository.SqlQuery<MaterialMasterVendorReconGL>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void OutChild(IEnumerable<MaterialMasterVendorReconGL> from_ui, out List<MaterialMasterVendorReconGL> from_db)
        {
            from_db = null;
            try
            {                                                                                                                                         //List<OperationTimeCaptureMaster> fromdblist = ieList.ToList();
                foreach (var ui in from_ui)
                {
                    var db = from_db.Where(a => a.MaterialMasterGLId == ui.MaterialMasterGLId).FirstOrDefault();
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
            return GetAutoNumber(nameof(MaterialMasterVendorReconGL), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        private PKGenerator GetMaxNumber()
        {
            return base.GetMaxNumber(nameof(MaterialMasterVendorReconGL), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void DeleteGraph(string masterId)
        {
            try
            {
                var data = Query(r => r.MaterialMasterGLId == masterId).Select().ToList();
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

        public MaterialMasterVendorReconGL FindbyFKId(string key)
        {
            return Query(m => m.MaterialMasterGLId == key).Select().FirstOrDefault();
        }
    }
}