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
    public class ServiceGroupPartyAccountGroupGLService : Service<ServiceGroupPartyAccountGroupGL>, IServiceGroupPartyAccountGroupGLService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public ServiceGroupPartyAccountGroupGLService(
            IRepositoryAsync<ServiceGroupPartyAccountGroupGL> comgroupdesingationgroupRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(comgroupdesingationgroupRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InsertOrUpdate(IEnumerable<ServiceGroupGL> masterlist, IEnumerable<ServiceGroupPartyAccountGroupGL> childListUI)
        {
            try
            {
                // Check(entity);
                var pk = GetMaxNumber();
                IEnumerable<ServiceGroupPartyAccountGroupGL> childListDb = GetChildList(masterlist);

                foreach (var m in masterlist)
                {
                    var db_c = childListDb.Where(a => a.ServiceGroupGLId == m.Id);
                    // OutChild(childListUI, out childListDb);
                    foreach (var item in childListUI)
                    {
                        var ob_c = db_c.Where(a => a.PartyAccountGroupId == item.PartyAccountGroupId).FirstOrDefault();
                        var temp = item.Copy<ServiceGroupPartyAccountGroupGL>();
                        if (ob_c == null || string.IsNullOrEmpty(ob_c.Id))
                        {
                            pk.MaxNumber++;
                            temp.ServiceGroupGLId = m.Id;
                            temp.Id = pk.MaxNumber.ToString();
                            temp.ServiceGroupId = m.ServiceGroupId;
                            InsertGraph(temp);
                        }
                        else
                        {
                            //log
                            ob_c.GLGeneralInfoId = (string.IsNullOrEmpty(item.GLGeneralInfoId) ? ob_c.GLGeneralInfoId : item.GLGeneralInfoId);
                            ob_c.BudgetMasterId = (string.IsNullOrEmpty(item.BudgetMasterId) ? ob_c.BudgetMasterId : item.BudgetMasterId);
                            ob_c.ActivityId = (string.IsNullOrEmpty(item.ActivityId) ? ob_c.ActivityId : item.ActivityId);
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
            //try
            //{
            //    // Check(entity);
            //    var pk = GetPK();
            //    var count = 0;
            //    //List<FAAccountDeterminateWiseVendorRecon> ob = new List<FAAccountDeterminateWiseVendorRecon>();
            //    //DeleteGraph(master.Id);
            //    //db
            //    IEnumerable<ServiceGroupPartyAccountGroupGL> childListDb = GetChildList(masterlist);

            //    foreach (var m in masterlist)
            //    {
            //        var db_c = childListDb.Where(a => a.ServiceGroupGLId == m.Id);
            //        // OutChild(childListUI, out childListDb);
            //        foreach (var item in childListUI)
            //        {
            //            var ob_c = db_c.Where(a => a.PartyAccountGroupId == item.PartyAccountGroupId).FirstOrDefault();
            //            var temp = item.Copy<ServiceGroupPartyAccountGroupGL>();
            //            if (ob_c == null || string.IsNullOrEmpty(ob_c.Id))
            //            {
            //                //ob_c = new ServiceGroupPartyAccountGroupGL();
            //                count++;
            //                //log
            //                temp.Id = pk + "." + count;
            //                temp.ServiceGroupGLId = m.Id;
            //                temp.ServiceGroupId = m.ServiceGroupId;
            //                InsertGraph(temp);
            //            }
            //            else
            //            {
            //                //log
            //                ob_c.VendorReconGLId = (string.IsNullOrEmpty(item.VendorReconGLId) ? ob_c.VendorReconGLId : item.VendorReconGLId);
            //                ob_c.VendorReconBudgetMasterId = item.VendorReconBudgetMasterId;
            //                ob_c.VendorReconActivityId = item.VendorReconActivityId;
            //                UpdateGraph(ob_c);
            //            }
            //        }//child
            //    }//parent
            //}
            //catch (Exception ex)
            //{
            //    throw new CustomException(ex.Message, ex,
            //    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
            //    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            //}
            //finally
            //{
            //}
        }

        private IEnumerable<ServiceGroupPartyAccountGroupGL> GetChildList(IEnumerable<ServiceGroupGL> masterList)
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
                string _sql = @"
                                SELECT * FROM [HKP].[ServiceGroupPartyAccountGroupGL]
                                WHERE ServiceGroupGLId In(" + masterId + ");";
                return _sqlRepository.GetModelCollection<ServiceGroupPartyAccountGroupGL>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void OutChild(IEnumerable<ServiceGroupPartyAccountGroupGL> from_ui, out List<ServiceGroupPartyAccountGroupGL> from_db)
        {
            from_db = null;
            try
            {                                                                                                                                         //List<OperationTimeCaptureMaster> fromdblist = ieList.ToList();
                foreach (var ui in from_ui)
                {
                    var db = from_db.Where(a => a.ServiceGroupGLId == ui.ServiceGroupGLId).FirstOrDefault();
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
            return GetAutoNumber(nameof(ServiceGroupPartyAccountGroupGL), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        private PKGenerator GetMaxNumber()
        {
            return base.GetMaxNumber(nameof(ServiceGroupPartyAccountGroupGL), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void DeleteGraph(string masterId)
        {
            try
            {
                var data = Query(r => r.ServiceGroupGLId == masterId).Select().ToList();
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

        public ServiceGroupPartyAccountGroupGL FindbyFKId(string key)
        {
            return Query(m => m.ServiceGroupGLId == key).Select().FirstOrDefault();
        }
    }
}