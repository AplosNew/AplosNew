#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Productions.SalesOrderInvoice;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Invoices
{
    public class SalesOrderInvoiceMasterService : Service<SalesOrderInvoiceMaster>, ISalesOrderInvoiceMasterService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISalesOrderInvoiceDetailService _ds;
        private readonly ISalesOrderInvoicePackingListService _ip;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<SalesOrderInvoiceMaster> _salesOrderMasterRepository;

        public SalesOrderInvoiceMasterService(
            IRepositoryAsync<SalesOrderInvoiceMaster> salesOrderMasterRepository,
            IPKGeneratorService pkGeneratorService,
            ISalesOrderInvoiceDetailService ds,
            ISalesOrderInvoicePackingListService ip,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(salesOrderMasterRepository, unitOfWork, pkGeneratorService)
        {
            _salesOrderMasterRepository = salesOrderMasterRepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _ds = ds;
            _ip = ip;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public string GetPK()
        {
            return "IM" + _pkGeneratorService.GetAutoNumber(nameof(SalesOrderInvoiceMaster), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void SaveMaster(SalesOrderInvoiceMaster uimaster, out string MasterID)
        {
            SalesOrderInvoiceMaster localMaster = null;
            var flag = false;
            MasterID = "";
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //Validation
                //Master

                InitMaster(uimaster, out localMaster);

                AuditService.Log(localMaster);
                InsertOrUpdateGraph(localMaster);
                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                MasterID = localMaster.Id;
                //ui_prmaster = localMaster;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                uimaster.AddedBy, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private void DelMaster(string id, out SalesOrderInvoiceMaster from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
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
            catch (Exception)
            {
                throw;
            }
        }

        private void DelDetailList(IEnumerable<SalesOrderInvoiceDetail> idlist, ref IEnumerable<SalesOrderInvoicePackingList> iplist)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                // from_db = _recipesubprocessservice.GetDetailList(id);
                foreach (var ip in iplist)
                {
                    var db_ip = iplist.FirstOrDefault(a => a.Id == ip.Id);
                    if (db_ip != null)
                    {
                        db_ip.ModelState = ModelState.Deleted;
                        //get child as its properties
                        var dbc = idlist.Where(a => a.SalesOrderInvoicePackingListId == db_ip.Id).ToList();
                        if (dbc.Count() > 0)
                        {
                            foreach (var d in idlist)
                            {
                                d.ModelState = ModelState.Deleted;
                            }
                            db_ip.SalesOrderInvoiceDetailList = dbc;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DeleteMaster(string masterid)
        {
            SalesOrderInvoiceMaster from_db = null;
            IEnumerable<SalesOrderInvoicePackingList> iplist = null;
            IEnumerable<SalesOrderInvoiceDetail> idlist = null;

            var flag = false;
            try
            {
                //master
                DelMaster(masterid, out from_db);
                iplist = _ip.GetDetailList(masterid);
                idlist = _ds.GetDetailListByInvoiceMaster(masterid);
                DelDetailList(idlist, ref iplist);

                from_db.SalesOrderInvoicePackingList = iplist.ToList();

                _unitOfWork.BeginTransaction();
                flag = true;
                Delete(from_db);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        // delete invoice packing list and detail and update invoice master amount
        public void DeleteDetailSingle(string InvoiceMasterId, string InvoicePackingListId)
        {
            SalesOrderInvoiceMaster master = null; ;
            IEnumerable<SalesOrderInvoicePackingList> ip_list = null;
            IEnumerable<SalesOrderInvoiceDetail> detaillist = null;
            const decimal _Amount = 0;
            var flag = false;
            try
            {
                ip_list = _ip.GetDetailList(InvoiceMasterId);
                _ip.DelInvoiceDetail(InvoicePackingListId, out detaillist);
                foreach (var item in ip_list)
                {
                    if (item.Id == InvoicePackingListId)
                    {
                        item.ModelState = ModelState.Deleted;
                        item.SalesOrderInvoiceDetailList = detaillist.ToList();
                    }
                }
                InitMasterAmount(InvoiceMasterId, InvoicePackingListId, _Amount, out master);
                master.SalesOrderInvoicePackingList = ip_list.ToList();
                _salesOrderMasterRepository.InsertOrUpdateGraph(master);
                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private DataSet GetPaymentTermInfo(string PaymentTermId)
        {
            GridParameter parameters = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"select m.Id,m.BaseLineDate,m.IsCustomer,m.IsEmployee
                                    ,m.IsVendor,m.PaymentModeId,m.UserName
                                    ,d.Percentage,d.NoOfDay,d.Sequence
                                    from mst.PaymentTerm m
                                    left outer join
				                                    (
					                                    select c.* from mst.PaymentTermDetail c
					                                    where c.Sequence=
						                                    (
							                                    select MAX(Sequence)
							                                    from  mst.PaymentTermDetail
							                                    where PaymentTermId='" + PaymentTermId + @"'
						                                    )

				                                    )d on m.Id=d.PaymentTermId
				                                    where m.Id='" + PaymentTermId + "'  and IsCustomer=1";
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void SetDueDateInfo(SalesOrderInvoiceMaster soim, out DateTime BaseOnDueDate, out DateTime ActualDueDate, out DateTime RevisedDueDate, out int BaseNoOfDays)
        {
            DataSet ds = null;
            BaseOnDueDate = DateTime.MinValue;
            ActualDueDate = DateTime.MinValue;
            RevisedDueDate = DateTime.MinValue;
            BaseNoOfDays = 0;
            try
            {
                ds = GetPaymentTermInfo(soim.PaymentTermId);
                if (ds.Tables[0].Rows.Count == 0)
                {
                    throw new Exception("No 'Payment Term' Info found for [" + soim.PaymentTermId + "]");
                }

                //BaseOnDueDate
                if (ds.Tables[0].Rows[0]["BaseLineDate"].ToString() == "default")
                {
                    BaseOnDueDate = DateTime.Now;
                }
                else if (ds.Tables[0].Rows[0]["BaseLineDate"].ToString() == "documentdate")
                {
                    BaseOnDueDate = soim.InvoiceDate;
                }
                else
                {
                    throw new Exception("'Base Line Date' of 'Payment Term' [" + ds.Tables[0].Rows[0]["UserName"] + "] can not be [" + ds.Tables[0].Rows[0]["BaseLineDate"] + "] for invoice");
                }
                //ActualDueDate
                //RevisedDueDate
                BaseNoOfDays = Convert.ToInt32(ds.Tables[0].Rows[0]["NoOfDay"].ToString());
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void InitMaster(SalesOrderInvoiceMaster from_ui, out SalesOrderInvoiceMaster from_db)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            from_db = null;
            DateTime BaseOnDueDate = DateTime.MinValue;
            DateTime ActualDueDate = DateTime.MinValue;
            DateTime RevisedDueDate = DateTime.MinValue;
            var BaseNoOfDays = 0;
            try
            {
                SetDueDateInfo(from_ui, out BaseOnDueDate, out ActualDueDate, out RevisedDueDate, out BaseNoOfDays);
                from_db = Find(from_ui.Id);

                if (from_db == null || from_db.Id == null)
                {
                    from_db = new SalesOrderInvoiceMaster
                    {
                        ModelState = ModelState.Added,
                        Id = GetPK(),//set pk

                        PlantId = from_ui.PlantId,
                        EntityId = from_ui.EntityId,
                        CompanyGroupId = identity.CompanyGroupId,
                        CompanyId = identity.CompanyId
                    };
                }
                else
                {
                    from_db.ModelState = ModelState.Modified;
                }
                from_db.CurrencyId = from_ui.CurrencyId;
                from_db.PaymentTermId = from_ui.PaymentTermId;
                from_db.CustomerId = from_ui.CustomerId;
                from_db.InvoiceDate = from_ui.InvoiceDate;
                from_db.InvoiceNo = (string.IsNullOrEmpty(from_ui.InvoiceNo) ? from_db.Id : from_ui.InvoiceNo);
                //  from_db.InvoiceValue = 0;
                from_db.SalesGroupId = from_ui.SalesGroupId;
                from_db.SalesOrganizationId = from_ui.SalesOrganizationId;
                from_db.SalesTypeId = from_ui.SalesTypeId;

                from_db.BaseOnDueDate = from_ui.BaseOnDueDate;
                from_db.ActualDueDate = from_ui.ActualDueDate.AddDays(from_ui.BaseNoOfDays);
                from_db.RevisedDueDate = from_db.ActualDueDate;
                from_db.BaseNoOfDays = from_ui.BaseNoOfDays;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void InitMasterAmount(string InvoiceMasterId, string InvoicePackingListMasterId, decimal Amount, out SalesOrderInvoiceMaster from_db)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            from_db = null;
            decimal _amount = 0;
            try
            {
                from_db = Find(InvoiceMasterId);

                IEnumerable<SalesOrderInvoiceDetail> d = _ds.SalesOrderInvoiceDetail(InvoiceMasterId, InvoicePackingListMasterId);
                foreach (var item in d)
                {
                    _amount += item.Qty * item.Rate;
                }

                if (from_db != null)
                {
                    from_db.ModelState = ModelState.Modified;
                    from_db.InvoiceValue = _amount + Amount;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetFileInfo(GridParameter parameters, string entityid)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"
                            SELECT pom.Id,pom.FileNo
	                            ,pom.[Description]
	                            ,p.UserName Customer
								,OQ.Qty OrderQty
								,PQ.PQty PackedQty
								,OQ.Qty-PQ.PQty BalanceQty
	                            ,u.UserName Entity,b.UserName Buyer,pom.SPT,pom.Cm,c.Code CmCurrency
	                            ,pom.CustomerId,pom.EntityId ,pom.PlantId,pom.BuyerId
                                ,oc.UserName OrderCategory
                                ,pom.OrderGrade,pom.OrderStatus,pl.UserName Plant

                            FROM  TRN.[SalesOrderMaster]  pom
                            LEFT JOIN  HKP.[Party]  p ON p.Id = pom.CustomerId
                            LEFT JOIN org.Entity u ON u.Id = pom.EntityId
                            LEFT JOIN  ORG.[Plant]  pl ON pl.Id = pom.PlantId
                            LEFT OUTER JOIN  HKP.[Buyer]  b ON pom.BuyerId = b.Id
                            LEFT OUTER JOIN  SCS.[Currency]  c ON pom.CmCurrencyId = c.Id
                            LEFT OUTER JOIN  HKP.[OrderCategory]  oc ON pom.OrderCategoryId = oc.Id
							left outer join (select mm.SalesOrderMasterId,sum(isnull(mm.Qty,0)) Qty FROM  TRN.[SalesOrderMaterialMaster] mm
												group by mm.SalesOrderMasterId) OQ on OQ.SalesOrderMasterId=pom.Id
							left outer join
							(
							select
									m.SalesOrderMasterId,
									sum(isnull(d.Qty,0)) PQty
									from [TRN].[SalesOrderPackingListMaster] m
									left outer join [TRN].[SalesOrderPackingListDetail] d on d.SalesOrderPackingListMasterId=m.Id
									group by m.SalesOrderMasterId
							) PQ on PQ.SalesOrderMasterId=pom.Id
                            WHERE pom.CompanyId = '" + identity.CompanyId + @"' AND pom.Archive = 0 and pom.OrderStatus<>'Closed'
                            AND pom.EntityId ='" + entityid + @"'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetMasterList(GridParameter parameters, string plantid, string entityId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"
                            select
                                m.Id,
                                m.InvoiceDate InvoiceDateId,
                                Replace(CONVERT(VARCHAR(11), m.InvoiceDate, 106), ' ', '-') InvoiceDate,
                                m.InvoiceNo,
                                m.InvoiceValue,
                                c.Code Currency,
                                p.UserName CustomerName,
                                e.UserName Entity,
                                so.UserName SalesOrganization,
                                sg.UserName SalesGroup,
                                st.UserName SalesType,
                                m.CurrencyId,
                                m.CustomerId,
                                m.PlantId,
                                m.SalesGroupId,
                                m.SalesOrganizationId,m.EntityId,
                                m.SalesTypeId,m.PaymentTermId,pt.UserName PaymentTerm
                                from [TRN].[SalesOrderInvoiceMaster] m
                                left outer join hkp.SalesType st on st.Id=m.SalesTypeId
                                left outer join org.SalesGroup sg on sg.Id=m.SalesGroupId
                                left outer join org.SalesOrganisation so on so.Id=m.SalesOrganizationId
                                left outer join hkp.Party p on p.Id=m.CustomerId
                                left outer join scs.Currency c on c.Id=m.CurrencyId
                                left outer join org.Entity e on e.Id=m.EntityId
                                left outer join mst.paymentTerm pt on pt.Id=m.PaymentTermId
                            where m.PlantId ='" + plantid + @"'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetInvoiceMaster(string id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"
                            select
                                m.Id,
                                m.InvoiceDate InvoiceDateId,
                                Replace(CONVERT(VARCHAR(11), m.InvoiceDate, 106), ' ', '-') InvoiceDate,
                                m.InvoiceNo,
                                m.InvoiceValue,
                                c.Code Currency,
                                p.UserName Customer,
                                so.UserName SalesOrganization,
                                sg.UserName SalesGroup,
                                st.UserName SalesType,
                                m.CurrencyId,
                                m.CustomerId,
                                m.PlantId,
                                m.SalesGroupId,
                                m.SalesOrganizationId,
                                m.SalesTypeId
                                from [TRN].[SalesOrderInvoiceMaster] m
                                left outer join hkp.SalesType st on st.Id=m.SalesTypeId
                                left outer join org.SalesGroup sg on sg.Id=m.SalesGroupId
                                left outer join org.SalesOrganisation so on so.Id=m.SalesOrganizationId
                                left outer join hkp.Party p on p.Id=m.CustomerId
                                left outer join scs.Currency c on c.Id=m.CurrencyId
                            where m.Id ='" + id + @"'";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetSalesType()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select s.Id [Value],s.UserName [Text] from hkp.SalesType s
		                left join HKP.CompanyGroupSalesType CG ON S.Id=CG.SalesTypeId
		                WHERE CG.CompanyGroupId='" + identity.CompanyGroupId + "' order by s.UserName";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetBaseLineDateSetting(string PaymentTermId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select
                                top(1)
                                m.Id,m.BaseLineDate,d.Id,d.Sequence,d.NoOfDay
                                from [MST].[PaymentTerm] m
                                left outer join [MST].[PaymentTermDetail] d on d.PaymentTermId=m.Id
                                where m.Id='" + PaymentTermId + @"'
                                order by d.[Sequence] desc";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetPackingList(string packmasterid)
        {
            GridParameter parameters = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"select  * from [TRN].[SalesOrderPackingListMaster] pm where pm.Id ='" + packmasterid + "'";
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void IsPackingListDeleted(string packmasterid)
        {
            try
            {
                var invoice = string.Empty;
                DataSet ds = GetPackingList(packmasterid);
                if (ds.Tables[0].Rows.Count == 0)
                {
                    throw new Exception("PackingList [" + ds.Tables[0].Rows[0]["PackingListNo"] + "] has already been deleted...");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SaveDetailList(string InvoiceMasterId, SalesOrderInvoicePackingList ui_pl, SalesOrderInvoiceDetail ui_dl)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var flag = false;
            SalesOrderInvoiceMaster master = null;
            SalesOrderInvoicePackingList pl = null;
            SalesOrderInvoiceDetail d_list = null;
            decimal _Amount = 0;
            try
            {
                //Validation
                IsPackingListDeleted(ui_pl.SalesOrderPackingListMasterId);
                //Detail Material
                _ip.InitInvoicePackingList(ui_pl, out pl);
                _ds.InitDetail(pl.Id, ui_dl, out d_list);
                //current packlist qty
                _Amount += d_list.Qty * d_list.Rate;

                InitMasterAmount(InvoiceMasterId, pl.Id, _Amount, out master);
                //////-----------------------------------------------
                _salesOrderMasterRepository.InsertOrUpdateGraph(master);
                _ip.InsertOrUpdateGraph(pl);
                _ds.InsertOrUpdateGraph(d_list);
                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                // ui_pmaterial = localDetailList;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
    }
}