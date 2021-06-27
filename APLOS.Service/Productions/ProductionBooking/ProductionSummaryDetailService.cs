#region Using
using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Inventory;
using Library.Model.Productions.ProductionBooking;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#endregion Using

namespace Library.Service.Productions
{
    public class ProductionSummaryDetailService : Service<ProductionSummaryDetail>, IProductionSummaryDetailService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        public ProductionSummaryDetailService(
            IRepositoryAsync<ProductionSummaryDetail> ProductionSummaryRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork,
            ISqlRepository sqlRepository
            ) : base(ProductionSummaryRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(ProductionSummaryDetail), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void Save(string psPK, IEnumerable<ProductionSummaryDetail> fromUI)
        {
            var flag = false;
            try
            {
                IEnumerable<ProductionSummaryDetail> fromDB = GetProductionSummaryDetailList(psPK);
                var _pk = GetPK();
                int _count = 0;
                foreach (var ob_ui in fromUI)//if in ui (insert or update)
                {
                    var ob_db = fromDB.Where(r => r.Id == ob_ui.Id).FirstOrDefault();
                    if (ob_db == null)//not found in db
                    {
                        _count++;
                        ob_ui.Id = "PSD" + _pk + "_" + _count;
                        ob_ui.ProductionSummaryId = psPK;
                        ob_ui.ModelState = ModelState.Added;
                        AuditService.AddedLog(ob_ui);
                        base.InsertOrUpdateGraph(ob_ui);
                    }
                    else
                    {
                        ob_db.Qty = ob_ui.Qty;
                        ob_db.ModelState = ModelState.Modified;
                        AuditService.UpdatedLog(ob_db);
                        base.InsertOrUpdateGraph(ob_db);
                    }

                }//for            
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        public void DeleteDetail(string psPK)
        {
            var flag = false;
            try
            {
                IEnumerable<ProductionSummaryDetail> fromDB = GetProductionSummaryDetailList(psPK);
                foreach (var ob_ui in fromDB)//if in ui (insert or update)
                {
                    var ob_db = fromDB.Where(r => r.Id == ob_ui.Id).FirstOrDefault();
                    if (ob_db != null)//not found in db
                    {
                        ob_ui.ModelState = ModelState.Deleted;
                        base.Delete(ob_ui);
                    }
                }//for            
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private IEnumerable<ProductionSummaryDetail> GetProductionSummaryDetailList(string psPK)
        {
            //getbulletindetaillist
            try
            {
                string _sql = "select * from trn.ProductionSummaryDetail where ProductionSummaryId='" + psPK + "'";
                return _sqlRepository.GetModelCollection<ProductionSummaryDetail>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertSecondCharacteristic(IEnumerable<ProductionSummaryDetail> entites, ProductionSummary productionSummary)
        {
            try
            {
                var _pk = GetPK();
                int _count = 0;
                foreach (var item in entites)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        if (item.Qty.ToString() != null && item.Qty != 0)
                        {
                            _count++;
                            item.Id = "PSD" + _pk + "_" + _count;
                            item.ProductionSummaryId = productionSummary.Id;
                            item.ModelState = ModelState.Added;
                            AuditService.AddedLog(item);

                            InsertGraph(item);
                        }
                    }
                    else
                    {

                        if (!string.IsNullOrEmpty(item.Id)  && item.Qty == 0)
                        {
                            item.ModelState = ModelState.Modified;
                            DeleteGraph(item);
                        }
                        else
                        {
                            item.ProductionSummaryId = productionSummary.Id;
                            item.ModelState = ModelState.Modified;
                            AuditService.UpdatedLog(item);

                            UpdateGraph(item);
                        }
                    }
                }
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }



    }
}