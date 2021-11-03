using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Payrolls;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Library.Service.Payrolls
{
    public class LoanAdvanceChildService : Service<LoanAdvanceChild>, ILoanAdvanceChildService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public LoanAdvanceChildService(
            IRepositoryAsync<LoanAdvanceChild> loanAdvanceChildRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(loanAdvanceChildRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(LoanAdvanceChild), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        //public void InsertOrUpdateGraph(IEnumerable<LoanAdvanceChild> loanAdvanceChildList, string masterId)
        //{
        //    try
        //    {
        //        if (loanAdvanceChildList != null)
        //        {
        //            var pkGenerator = GetAutoNumber("LoanAdvanceChild", PKGeneratorEnum.Auto, null, DateTime.Now);
        //            var Db_list = Query(r => r.LoanMstSystemID == masterId).Select().ToList();

        //            foreach (var item in Db_list)
        //            {
        //                var db = loanAdvanceChildList.Where(a => a.SystemID == item.SystemID).FirstOrDefault();
        //                if (db == null || string.IsNullOrEmpty(db.SystemID))
        //                {
        //                    //item.ModelState = ModelState.Deleted;
        //                    //AuditService.Log(item);
        //                    Delete(item);
        //                }
        //            }
        //            int count = 0;
        //            foreach (LoanAdvanceChild item in loanAdvanceChildList)
        //            {
        //                count++;
        //                if (Db_list.Any(r => r.SystemID == item.SystemID))
        //                {
        //                    var loanAdvanceChildDb = Db_list.FirstOrDefault(r => r.SystemID == item.SystemID);
        //                    loanAdvanceChildDb.LoanMstSystemID = masterId;
        //                    loanAdvanceChildDb.ModelState = ModelState.Modified;
        //                    AuditService.Log(loanAdvanceChildDb);
        //                    InsertOrUpdateGraph(loanAdvanceChildDb);
        //                }
        //                else
        //                {
        //                    item.SystemID = "LC" + pkGenerator + "-" + count;
        //                    item.LoanMstSystemID = masterId;
        //                    item.ModelState = ModelState.Added;
        //                    AuditService.Log(item);
        //                    InsertOrUpdateGraph(item);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
        //    }
        //}

        //public void InsertOrUpdateGraphOpeningBalance(IEnumerable<LoanAdvanceChild> loanAdvanceChildList, string masterId)
        //{
        //    try
        //    {
        //        if (loanAdvanceChildList != null)
        //        {
        //            var pkGenerator = GetAutoNumber("LoanAdvanceChild", PKGeneratorEnum.Auto, null, DateTime.Now);
        //            var Db_list = Query(r => r.LoanMstSystemID == masterId).Select().ToList();
        //            foreach (var item in loanAdvanceChildList)
        //            {
        //                if (item.SystemID !=null)
        //                {
        //                    base.Delete(item);
        //                }
        //                else
        //                {
        //                    InsertOrUpdateGraph(item);

        //                }
        //            }
        //            foreach (var item in Db_list)
        //            {
        //                var db = loanAdvanceChildList.Where(a => a.SystemID == item.SystemID).FirstOrDefault();
        //                if (db != null || !string.IsNullOrEmpty(db.SystemID))
        //                {
        //                    //item.ModelState = ModelState.Deleted;
        //                    //AuditService.Log(item);
        //                    Delete(item);
        //                }
        //            }
        //            int count = 0;
        //            foreach (LoanAdvanceChild item in loanAdvanceChildList)
        //            {
        //                count++;
        //                if (Db_list.Any(r => r.SystemID == item.SystemID))
        //                {
        //                    var loanAdvanceChildDb = Db_list.FirstOrDefault(r => r.SystemID == item.SystemID);
        //                    loanAdvanceChildDb.LoanMstSystemID = masterId;
        //                    loanAdvanceChildDb.ModelState = ModelState.Modified;
        //                    AuditService.Log(loanAdvanceChildDb);
        //                    InsertOrUpdateGraph(loanAdvanceChildDb);
        //                }
        //                else
        //                {
        //                    item.SystemID = "AO" + pkGenerator + "-" + count;
        //                    item.LoanMstSystemID = masterId;
        //                    item.ModelState = ModelState.Added;
        //                    AuditService.Log(item);
        //                    InsertOrUpdateGraph(item);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
        //    }
        //}

        public void InsertOrUpdateGraph(IEnumerable<LoanAdvanceChild> loanAdvanceChildList, string masterId)
        {
            try
            {
                if (loanAdvanceChildList != null)
                {
                    var pk = GetPK();
                    var count = 0;
                    var dbList = Query(r => r.LoanMstSystemID == masterId).Select().ToList();
                    if (dbList != null && dbList.Count() > 0)
                    {
                        foreach (var item in dbList)
                        {
                            var ui = loanAdvanceChildList.Where(a => a.SystemID == item.SystemID).FirstOrDefault();
                            if (ui == null || ui.SystemID == null)
                            {
                                item.ModelState = ModelState.Deleted;
                                base.DeleteGraph(item);
                            }
                        }
                    }

                    foreach (var item in loanAdvanceChildList)
                    {
                        var db = dbList.Where(a => a.SystemID == item.SystemID).FirstOrDefault();
                        if (db == null || db.SystemID == null)
                        {
                            count++; item.SystemID = "LC" + pk + "_" + count;
                            item.LoanMstSystemID = masterId;
                            item.ModelState = ModelState.Added;
                            InsertGraph(item);
                        }
                        else
                        {
                            item.ModelState = ModelState.Modified;
                            AuditService.Log(item);
                            UpdateGraph(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void InsertOrUpdateGraphOpeningBalance(IEnumerable<LoanAdvanceChild> entities, string masterId)
        {
            try
            {
                if (entities == null)
                    throw new CustomException("Data can't be null.");
                var pk = GetPK();
                var count = 0;
                var dbList = Query(r => r.LoanMstSystemID == masterId).Select().ToList();
                if (dbList != null && dbList.Count() > 0)
                {
                    foreach (var item in dbList)
                    {
                        var ui = entities.Where(a => a.SystemID == item.SystemID).FirstOrDefault();
                        if (ui == null || ui.SystemID == null)
                        {
                            item.ModelState = ModelState.Deleted;
                            base.DeleteGraph(item);
                        }
                    }
                }
                foreach (var item in entities)
                {
                    var db = dbList.Where(a => a.SystemID == item.SystemID).FirstOrDefault();
                    if (db == null || db.SystemID == null)
                    {
                        count++;

                        item.SystemID = "AO" + pk + "-" + count;
                        item.LoanMstSystemID = masterId;
                        item.ModelState = ModelState.Added;
                        AuditService.Log(item);
                        InsertGraph(item);
                    }
                    
                }
            }
            catch (CustomException cx)
            {
                throw cx;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name.ToString(), null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public IEnumerable<object> GetLoanChildByMaster(string loanMstSystemID)
        {
            string sql = @"
						  SELECT SystemID, LoanMstSystemID, MonthNo, YearNo,
                                MonthName = DATENAME(MONTH, MonthNo + '-01-' + YearNo),
                                MonthlyAdjAmount, PaidAmount, BalanceAmount, IsDisbusted, SequenceNo
                            FROM LoanAdvanceChild
                          WHERE LoanMstSystemID = '" + loanMstSystemID + @"'
                          ORDER BY SequenceNo";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetOpeningBalanceChildByMaster(string loanMstSystemID)
        {
            //string sql = @"
            //			  SELECT SystemID, LoanMstSystemID, MonthNo, YearNo,
            //                             MonthName = DATENAME(MONTH, MonthNo + '-01-' + YearNo),
            //                             MonthlyAdjAmount, PaidAmount, BalanceAmount, IsDisbusted, SequenceNo
            //                         FROM LoanAdvanceChild
            //                       WHERE LoanMstSystemID = '" + loanMstSystemID + @"'
            //                       ORDER BY SequenceNo";
            string sql = @"SELECT LC.SystemID, LC.LoanMstSystemID, LC.MonthNo, LC.YearNo,
                                MonthName = DATENAME(MONTH, LC.MonthNo + '-01-' + LC.YearNo),
                                LC.MonthlyAdjAmount, LC.PaidAmount, LC.BalanceAmount, LC.IsDisbusted, LC.SequenceNo,LM.StartDate
                            FROM LoanAdvanceChild LC
							LEFT JOIN dbo.LoanAdvanceMaster LM ON LC.LoanMstSystemID=LM.SystemID
                          WHERE LoanMstSystemID = '" + loanMstSystemID + @"'
                          ORDER BY SequenceNo";
            return _sqlRepository.GetDataCollection(sql, null);
        }
    }
}