#region Using

using Library.Core;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Productions.SalesOrderInvoice;
using Library.Service.Core;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion Using

namespace Library.Service.Invoices
{
    public class SalesOrderInvoicePackingListService : Service<SalesOrderInvoicePackingList>, ISalesOrderInvoicePackingListService
    {
        #region Constructor

        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISalesOrderInvoiceDetailService _soipd;
        private readonly ISqlRepository _sqlRepository;

        public SalesOrderInvoicePackingListService(
            IRepositoryAsync<SalesOrderInvoicePackingList> salesOrderMasterRepository,
            IPKGeneratorService pkGeneratorService,
            ISalesOrderInvoiceDetailService id,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(salesOrderMasterRepository, unitOfWork, pkGeneratorService)
        {
            _soipd = id;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public string GetPK()
        {
            return "IP" + _pkGeneratorService.GetAutoNumber(nameof(SalesOrderInvoicePackingList), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public IEnumerable<SalesOrderInvoicePackingList> GetDetailList(string SalesOrderInvoiceMasterId)
        {
            return from m in Query(m => m.SalesOrderInvoiceMasterId == SalesOrderInvoiceMasterId) select m;
            //select new { Text = m.FileName, Value = m.Id };
        }

        public void InitInvoicePackingList(SalesOrderInvoicePackingList from_ui, out SalesOrderInvoicePackingList from_db)
        {
            from_db = null;
            from_db = Find(from_ui.Id);

            if (from_db == null || from_db.Id == null)
            {
                from_db = new SalesOrderInvoicePackingList
                {
                    ModelState = ModelState.Added
                };
                AuditService.Log(from_db);
                from_db.Id = GetPK();//set pk
            }
            else
            {
                from_db.ModelState = ModelState.Modified;
                AuditService.Log(from_db);
            }
            from_db.SalesOrderInvoiceMasterId = from_ui.SalesOrderInvoiceMasterId;
            from_db.SalesOrderPackingListMasterId = from_ui.SalesOrderPackingListMasterId;
        }

        public IEnumerable<object> GetInvoicePackingListHead(string invoicemasterid)
        {
            var sql = @"

                                select      ml.Id,
                                            m.Id SalesOrderPackingListMasterId,
											a.Amount,q.Qty,
                                            c.UserName Customer,
                                            m.PartyId,
                                            m.PackingListNo,
                                            m.PackingDate PackingDateId,
                                            Replace(CONVERT(VARCHAR(11), m.PackingDate, 106), ' ', '-') PackingDate
                                            from
	                                        [TRN].[SalesOrderInvoicePackingList] ml
	                                        left outer join [TRN].[SalesOrderPackingListMaster] m on m.Id=ml.SalesOrderPackingListMasterId
											left outer join (
                                                                select      sum(Qty*Rate) Amount,SalesOrderInvoicePackingListId,SalesOrderInvoiceMasterId
                                                                from        trn.SalesOrderInvoiceDetail
																group by    SalesOrderInvoicePackingListId,SalesOrderInvoiceMasterId
															) a
																on a.SalesOrderInvoicePackingListId=ml.id
																and ml.SalesOrderInvoiceMasterId=a.SalesOrderInvoiceMasterId
                                            left outer join (
                                                                select      sum(Qty) Qty,SalesOrderInvoicePackingListId,SalesOrderInvoiceMasterId
                                                                from        trn.SalesOrderInvoiceDetail
																group by    SalesOrderInvoicePackingListId,SalesOrderInvoiceMasterId
															) q
																on q.SalesOrderInvoicePackingListId=ml.id
																and ml.SalesOrderInvoiceMasterId=q.SalesOrderInvoiceMasterId

                                            left outer join hkp.Party c on c.Id=m.PartyId
                                where       ml.SalesOrderInvoiceMasterId='" + invoicemasterid + @"'
                                order by  m.PackingDate desc,m.PartyId,m.PackingListNo";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public void DelInvoicePackingList(string id, out SalesOrderInvoicePackingList from_db)
        {
            from_db = null;
            from_db = Find(id);

            if (from_db.Id == null || from_db.Id == "")
            {
                throw new Exception("No Row found against Id: [" + id + "]");
            }
            else
            {
                from_db.ModelState = ModelState.Deleted;
            }
        }

        public void DelInvoiceDetail(string ipid, out IEnumerable<SalesOrderInvoiceDetail> from_db)
        {
            from_db = null;
            from_db = _soipd.GetDetailList(ipid);
            foreach (var ui in from_db)
            {
                var db = from_db.FirstOrDefault(a => a.Id == ui.Id);
                if (db != null)
                {
                    db.ModelState = ModelState.Deleted;
                }
            }//foreach
        }
    }
}