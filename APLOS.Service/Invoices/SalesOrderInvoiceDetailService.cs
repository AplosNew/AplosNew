#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Productions.QueryModel;
using Library.Model.Productions.SalesOrderInvoice;
using Library.Service.Core;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

#endregion Using

namespace Library.Service.Invoices
{
    public class SalesOrderInvoiceDetailService : Service<SalesOrderInvoiceDetail>, ISalesOrderInvoiceDetailService
    {
        #region Constructor

        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;

        public SalesOrderInvoiceDetailService(
            IRepositoryAsync<SalesOrderInvoiceDetail> SalesOrderMasterRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository) : base(SalesOrderMasterRepository, unitOfWork, pkGeneratorService)
        {
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public string GetPK()
        {
            return "ID" + _pkGeneratorService.GetAutoNumber(nameof(SalesOrderInvoiceDetail), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public IEnumerable<SalesOrderInvoiceDetail> GetDetailListByInvoiceMaster(string InvoiceMasterId)
        {
            return from m in Query(m => m.SalesOrderInvoiceMasterId == InvoiceMasterId) select m;
        }

        public IEnumerable<SalesOrderInvoiceDetail> GetDetailList(string SalesOrderInvoicePackingListId)
        {
            return from m in Query(m => m.SalesOrderInvoicePackingListId == SalesOrderInvoicePackingListId) select m;
        }

        public IEnumerable<SalesOrderInvoiceDetail> SalesOrderInvoiceDetail(string SalesOrderInvoiceMasterId, string SalesOrderInvoicePackingListId)
        {
            var _sql = @"select * from trn.SalesOrderInvoiceDetail
                                    where SalesOrderInvoiceMasterId = '" + SalesOrderInvoiceMasterId + @"'
                                    and SalesOrderInvoicePackingListId<> '" + SalesOrderInvoicePackingListId + @"'";
            return _sqlRepository.GetModelCollection<SalesOrderInvoiceDetail>(_sql);
        }

        public void InitDetail(string MasterId, SalesOrderInvoiceDetail from_ui, out SalesOrderInvoiceDetail from_db)
        {
            var _count = 0;
            from_db = null;
            from_db = GetDetailList(MasterId).ToList().FirstOrDefault();

            var ui = from_ui;
            if (ui == null || string.IsNullOrEmpty(ui.Id))
            {
                ui.ModelState = ModelState.Deleted;
                AuditService.Log(ui);
            }

            var PK = GetPK();
            if (from_ui != null)
            {
                var db = from_db;
                if (db == null || string.IsNullOrEmpty(db.Id))//new
                {
                    _count += 1;
                    db = new SalesOrderInvoiceDetail
                    {
                        Id = PK + "-" + _count,
                        ModelState = ModelState.Added
                    };
                    AuditService.Log(db);
                    db.Qty = ui.Qty;
                    db.Rate = ui.Rate;
                    db.SalesOrderInvoiceMasterId = ui.SalesOrderInvoiceMasterId;
                    db.SalesOrderPackingListMasterId = ui.SalesOrderPackingListMasterId;
                    db.SalesOrderInvoicePackingListId = MasterId;
                    db.UomId = ui.UomId;
                    db.ArticleId = ui.ArticleId;
                    db.MaterialMasterId = ui.MaterialMasterId;
                    db.SalesOrderPackingListMaterialId = ui.SalesOrderPackingListMaterialId;
                    from_db = db;
                }
                else
                {
                    db.ModelState = ModelState.Modified;
                    AuditService.Log(db);
                    db.Qty = ui.Qty;
                    db.Rate = ui.Rate;
                    db.UomId = ui.UomId;
                    db.SalesOrderInvoiceMasterId = ui.SalesOrderInvoiceMasterId;
                    db.SalesOrderPackingListMasterId = ui.SalesOrderPackingListMasterId;
                    db.SalesOrderInvoicePackingListId = MasterId;
                    db.ArticleId = ui.ArticleId;
                    db.MaterialMasterId = ui.MaterialMasterId;
                }
            }
        }

        public GridModel GetOrderbreakDown(GridParameter parameters, string somid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            parameters.CmdText = @"SELECT pomm.Id SalesOrderMaterialMasterId,pomm.Id OrderBreakNo
	                                ,mm.UserName MaterialMaster
	                                ,pomm.MaterialMasterId
	                                ,po.PoNumber PoNo
	                                ,pomm.DeliveryDate DeliveryDateID
	                                ,Replace(CONVERT(VARCHAR(11), pomm.DeliveryDate, 106), ' ', '-') DeliveryDate
	                                ,pomm.Qty OrderQty
	                                ,PQ.PQty PackedQty
									,pomm.Qty-PQ.PQty BalanceQty
	                                ,pomm.UomId PommUnitId
	                                ,pommu.UserName Uom
	                                ,pom.BuyerId
	                                ,pomm.CustomerPOId

                                FROM (select * from [TRN].[SalesOrderMaster] ) pom

                                left outer JOIN (select * from [TRN].[SalesOrderMaterialMaster] ) pomm
	                            ON pom.Id = pomm.SalesOrderMasterId
								left outer join (select * from trn.CustomerPO ) po on po.Id=pomm.CustomerPOId

                                left outer JOIN MST.MaterialMaster mm ON mm.Id = pomm.MaterialMasterId
                                left outer JOIN SCS.UnitOfMeasurement pommu ON pommu.Id = pomm.UomId

								left outer join
								(
								select
									m.SalesOrderMasterId,d.SalesOrderMaterialMasterId,
									sum(isnull(d.Qty,0)) PQty
									from [TRN].[SalesOrderPackingListMaster] m
									left outer join [TRN].[SalesOrderPackingListDetail] d on d.SalesOrderPackingListMasterId=m.Id
									group by m.SalesOrderMasterId,d.SalesOrderMaterialMasterId
																	) PQ
								on pomm.SalesOrderMasterId=PQ.SalesOrderMasterId and pomm.Id=PQ.SalesOrderMaterialMasterId
                                WHERE
								pom.Id='" + somid + @"'
								and
								 pom.CompanyId='" + identity.CompanyId + @"'
                                ";
            return _sqlRepository.GetGridData(parameters);
        }

        public GridModel GetCharQty(GridParameter parameters, string sommid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            parameters.CmdText = @"
                                select sc1.Id sc1Id,c23.Id SKU
                                ,c1.Alias c1Id
                                ,c2.Alias c2Id
                                ,c3.Alias c3Id
                                ,cv1.[Description] cv1
                                ,cv2.[Description] cv2
                                ,cv3.[Description] cv3
                                ,c23.Qty OQty,0 BQty,0 PQty,0 Qty
                                ,u.UserName Uom
                                ,c23.UomId
                                ,sc1.Characteristics1Id,sc1.CharacteristicsValue1Id
                                ,c23.Characteristics2Id,c23.CharacteristicsValue2Id
                                ,c23.Characteristics3Id,c23.CharacteristicsValue3Id
                                 from
                                [TRN].[SalesOrderCharacteristicsValue1st] sc1
                                left outer join [TRN].[SalesOrderCharacteristicsValue2nd] c23 on sc1.Id=c23.SalesOrderCharacteristicsValue1stId
                                left outer join hkp.Characteristics c1 on c1.Id=sc1.Characteristics1Id
                                left outer join hkp.Characteristics c2 on c2.Id=c23.Characteristics2Id
                                left outer join hkp.Characteristics c3 on c3.Id=c23.Characteristics3Id

                                left outer join hkp.CharacteristicsValue cv1 on cv1.Id=sc1.CharacteristicsValue1Id
                                left outer join hkp.CharacteristicsValue cv2 on cv2.Id=c23.CharacteristicsValue2Id
                                left outer join hkp.CharacteristicsValue cv3 on cv3.Id=c23.CharacteristicsValue3Id

                                left outer join scs.UnitOfMeasurement u on u.Id=c23.UomId

                                where sc1.SalesOrderMaterialMasterId='" + sommid + @"'
								and
								 sc1.CompanyId='" + identity.CompanyId + @"'
                                ";
            return _sqlRepository.GetGridData(parameters);
        }

        public GridModel GetPLDetailSearch(GridParameter parameters, string CustomerId, string plantid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            parameters.CmdText = @"
                               select       case isnull(id.Id,'') when '' then 0 else 1 end IsTakenId,
                                            0 IsSelectedId,
                                            '' SalesOrderInvoiceMasterId,
                                            '' Id,
                                            m.Id SalesOrderPackingListMasterId,
                                            c.UserName Customer,
                                            m.CustomerId,
                                            m.PackingListNo,
                                            m.PackingDate PackingDateId,
                                            Replace(CONVERT(VARCHAR(11), m.PackingDate, 106), ' ', '-') PackingDate,

                                            d.Id SalesOrderPackingListDetailId,
                                            mm.UserName MaterialMaster,
                                            c1.Alias c1,
                                            c2.Alias c2,
                                            c3.Alias c3,

                                            cv1.[Description] cv1,
                                            cv2.[Description] cv2,
                                            cv3.[Description] cv3,

                                            d.Characteristics1Id,
                                            d.Characteristics2Id,
                                            d.Characteristics3Id,

                                            d.CharacteristicsValue1Id,
                                            d.CharacteristicsValue2Id,
                                            d.CharacteristicsValue3Id,
                                            isnull(d.Qty,0) Qty,
                                            isnull(somm.Rate,0) soRate,
                                            isnull(d.Qty,0)*isnull(somm.Rate,0) Amount,
                                            u.UserName Uom,
                                            d.UomId,
                                            isnull(somm.Rate,0) Rate,
                                            som.CmCurrencyId,
                                            cu.Code Currency,
                                            im.InvoiceNo

                                            from [TRN].[SalesOrderPackingListMaster] m
                                            left outer join [TRN].[SalesOrderPackingListDetail] d on d.SalesOrderPackingListMasterId=m.Id
                                            left outer join [TRN].[SalesOrderMaster] som on som.Id=m.SalesOrderMasterId and som.Id=d.SalesOrderMasterId
                                            left outer join [TRN].[SalesOrderMaterialMaster] somm on somm.SalesOrderMasterId=d.SalesOrderMasterId
														                                            and somm.Id=d.SalesOrderMaterialMasterId
														                                            and somm.SalesOrderMasterId=som.Id
                                            left outer join mst.MaterialMaster mm on mm.Id=somm.MaterialMasterId
                                            left outer join hkp.Party c on c.Id=m.CustomerId
                                            left outer join scs.UnitOfMeasurement u on u.Id=d.UomId
                                            left outer join scs.Currency cu on cu.Id=som.CmCurrencyId

                                            left outer join hkp.Characteristics c1 on c1.Id=d.Characteristics1Id
                                            left outer join hkp.Characteristics c2 on c2.Id=d.Characteristics2Id
                                            left outer join hkp.Characteristics c3 on c3.Id=d.Characteristics3Id

                                            left outer join hkp.CharacteristicsValue cv1 on cv1.Id=d.CharacteristicsValue1Id
                                            left outer join hkp.CharacteristicsValue cv2 on cv2.Id=d.CharacteristicsValue2Id
                                            left outer join hkp.CharacteristicsValue cv3 on cv3.Id=d.CharacteristicsValue3Id

                                            left outer join [TRN].[SalesOrderInvoiceDetail] id on id.SalesOrderPackingListDetailId=d.Id
                                            left outer join [TRN].[SalesOrderInvoiceMaster] im on im.Id=id.[SalesOrderInvoiceMasterId]

                                where       m.CustomerId='" + CustomerId + @"'
								and         m.PlantId='" + plantid + @"'
                                ";
            return _sqlRepository.GetGridData(parameters);
        }

        public GridModel GetPLHeadSearch(GridParameter parameters, string EntityId, string CustomerId, string plantid)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                //parameters.CmdText = @"
                //            select       case isnull(id.Id,'') when '' then 0 else 1 end IsTakenId,
                //                        0 IsSelectedId,
                //                        '' SalesOrderInvoiceMasterId,
                //                        '' Id,
                //                        m.Id SalesOrderPackingListMasterId,
                //                        c.UserName Customer,
                //                        m.PartyId,
                //                        m.PackingListNo,
                //                        m.PackingDate PackingDateId,
                //                        Replace(CONVERT(VARCHAR(11), m.PackingDate, 106), ' ', '-') PackingDate,
                //                        isnull(d.Qty,0) Qty,
                //               isnull(iq.iAmount,0) Amount,
                //                        im.InvoiceNo
                //                        from [TRN].[SalesOrderPackingListMaster] m
                //                        left outer join (select sum(k.qty) Qty,k.SalesOrderPackingListMasterId from [TRN].[SalesOrderPackingListMaterial] k
                //               group by k.SalesOrderPackingListMasterId
                //               ) d on d.SalesOrderPackingListMasterId=m.Id

                //                left outer join (select sum(k.Qty*k.Rate) iAmount,k.SalesOrderPackingListMasterId from [TRN].SalesOrderInvoiceDetail k
                //               group by k.SalesOrderPackingListMasterId
                //               ) iq on iq.SalesOrderPackingListMasterId=m.Id

                //                        left outer join hkp.Party c on c.Id=m.PartyId
                //                        left outer join [TRN].[SalesOrderInvoicePackingList] id on id.SalesOrderPackingListMasterId=m.Id
                //                        left outer join [TRN].[SalesOrderInvoiceMaster] im on im.Id=id.[SalesOrderInvoiceMasterId]
                //            where       m.PartyId='" + CustomerId + @"'
                //            and         m.PlantId='" + plantid + @"'
                //            and         m.EntityId='" + EntityId + @"'
                //            and			m.IsEntryCompleted=1";
                parameters.CmdText = @"SELECT  POPL.Id SalesOrderPackingListMasterId,SOMM.Id SalesOrderMaterialMasterId,SOMM.MaterialMasterId,SOMM.ArticleId,SOMM.SalesOrderMasterId,SOMM.UomId ,c.UserName Customer,POPL.PartyId,POPL.PackingListNo,SOPM.Id SalesOrderPackingListMaterialId
                                        ,Replace(CONVERT(VARCHAR(11), POPL.PackingDate, 106), ' ', '-') PackingDate
                                        ,isnull(SOPM.Qty,0) PackingQty
                                        ,CASE  when CV2.Id <>''  then CV2.Qty
                                        when isnull( CV2.Id,'') ='' and CV1.Id <>'' then  CV1.Qty
                                        when isnull( CV2.Id,'') ='' and ISNULL(CV1.Id,'')=''  then SOMM.Qty
                                        end MaterialQty
                                        ,CASE  when CV2.Id <>''  then CV2.Rate
                                        when isnull( CV2.Id,'') ='' and CV1.Id <>'' then  CV1.Rate
                                        when isnull( CV2.Id,'') ='' and ISNULL(CV1.Id,'')=''  then SOMM.Rate
                                        end Rate
                                         FROM TRN.SalesOrderMaterialMaster SOMM
                                         LEFT JOIN TRN.SalesOrderCharacteristicsValue1st CV1 ON SOMM.Id=CV1.MaterialMasterCharacteristicsValue1Id
                                         LEFT JOIN TRN.SalesOrderCharacteristicsValue2nd CV2 ON SOMM.Id=CV2.MaterialMasterCharacteristicsValue2Id AND CV1.Id=CV2.SalesOrderCharacteristicsValue1stId
                                        LEFT JOIN MST.MaterialMaster MM ON SOMM.MaterialMasterId=MM.Id
                                        LEFT JOIN TRN.SalesOrderPackingListMaterial SOPM ON SOMM.Id=SOPM.SalesOrderMaterialMasterId
                                        LEFT JOIN TRN.SalesOrderPackingListMaster POPL ON SOPM.SalesOrderPackingListMasterId=POPL.Id
                                        LEFT JOIN  HKP.Party C on C.Id=POPL.PartyId
                                        WHERE POPL.PlantId='" + plantid + "' AND POPL.EntityId='" + EntityId + "'and POPL.PartyId='" + CustomerId + "' --AND  SOMM.OrderStatus='Confirmed' and SOMM.DeliveryEndDate <>''";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private IEnumerable<MaterialPackedAndInvoiced> Get_Invoiced_Material_Edit(string SalesOrderInvoicePackingListId)
        {
            #region SQL

            var sql = @"select   idid Id ,SalesOrderMasterId,
                                    SalesOrderInvoiceMasterId,SalesOrderInvoicePackingListId,SalesOrderMaterialMasterId,
                             SalesOrderCharacteristicsValue1stId
                             ,SalesOrderCharacteristicsValue2ndId
                             ,Sku
                             ,c1Id
                             ,c2Id
                             ,c3Id
                             ,cv1
                             ,cv2
                             ,cv3
                          	 ,FileNo
							 ,MaterialMasterId
                                     , MaterialMaster
									 ,SubmaterialCode
									 ,Submaterial
									 ,SubMaterialId

							 ,DeliveryDate,DeliveryDateId
							 ,PONumber
							  ,Detail
                             ,OrderQty
							 ,PackedQty
                            ,InvoicedQty
                             ,CurrentQty
							 ,Rate
                             ,Uom
                             ,UomId
                             ,Characteristics1Id
                             ,Characteristics2Id
                             ,Characteristics3Id
                             ,CharacteristicsValue1Id
                             ,CharacteristicsValue2Id
                             ,CharacteristicsValue3Id

                              from
                             (
                            select sc1.Id SalesOrderCharacteristicsValue1stId,im.Id SalesOrderInvoiceMasterId
							,d.SalesOrderInvoicePackingListId
							,d.Id idid
		                            ,c23.Id SalesOrderCharacteristicsValue2ndId
		                            , case isnull(c23.Id,'') when '' then sc1.Id else c23.Id end Sku

                                    ,c1.Alias c1Id
                                    ,c2.Alias c2Id
                                    ,c3.Alias c3Id
                                    ,cv1.[Description] cv1
                                    ,cv2.[Description] cv2
                                    ,cv3.[Description] cv3
		                          	,som.FileNo
									,somm.MaterialMasterId
                                     ,mm.UserName AS MaterialMaster
									 ,sm.Code SubmaterialCode
									 ,sm.StandardName Submaterial
									 ,somm.SubMaterialId,somm.DeliveryDate DeliveryDateId
									,Replace(CONVERT(VARCHAR(11), somm.DeliveryDate, 106), ' ', '-') DeliveryDate
									,cp.PONumber
									, isnull(cv1.[Description],'')+' '+isnull(cv2.[Description],'')+' '+isnull(cv3.[Description],'') Detail

		                           ,OrderQty = CASE
                                              WHEN isnull(c23.Id, '') = ''
                                               THEN (
                                                 CASE
                                                  WHEN isnull(sc1.Id, '') = ''
                                                   THEN isnull(somm.Qty, 0)
                                                  ELSE isnull(sc1.Qty, 0)
                                                  END
                                                 )
                                              ELSE isnull(c23.Qty, 0)
                                              END

								     ,0 PackedQty
									,0 InvoicedQty
		                            ,d.Qty CurrentQty,d.Rate
                                    ,u.UserName Uom,somm.SalesOrderMasterId,somm.Id SalesOrderMaterialMasterId
                                    ,d.UomId
                                    ,sc1.Characteristics1Id,sc1.CharacteristicsValue1Id
                                    ,c23.Characteristics2Id,c23.CharacteristicsValue2Id
                                    ,c23.Characteristics3Id,c23.CharacteristicsValue3Id
		                           -- ,d.SalesOrderMaterialMasterId,d.SalesOrderPackingListMasterId
									--	select * from trn.SalesOrderInvoiceDetail

                                        from
										 [TRN].[SalesOrderInvoiceMaster] im
										 left outer join trn.SalesOrderInvoicePackingList ip on im.Id=ip.SalesOrderInvoiceMasterId
										 left outer join trn.SalesOrderInvoiceDetail d on d.SalesOrderInvoicePackingListId=ip.Id

                                    left outer join trn.SalesOrderMaterialMaster somm on somm.SalesOrderMasterId =d.SalesOrderMasterId
																	and somm.Id=d.SalesOrderMaterialMasterId

									left outer join trn.CustomerPO cp on cp.Id=somm.CustomerPOId
									LEFT JOIN MST.[MaterialMaster] mm ON somm.MaterialMasterId = mm.Id
									left outer join mst.SubMaterial sm on sm.Id=somm.SubMaterialId
									left outer join trn.SalesOrderMaster som on som.Id=somm.SalesOrderMasterId

                                    left outer join (select * from [TRN].[SalesOrderCharacteristicsValue1st]
															                           )sc1 on isnull(d.SalesOrderCharacteristicsValue1stId,'')=isnull(sc1.Id,'')
                                    left outer join [TRN].[SalesOrderCharacteristicsValue2nd] c23
															on isnull(d.SalesOrderCharacteristicsValue2ndId,'')=isnull(c23.Id,'')

                                    left outer join hkp.Characteristics c1 on c1.Id=sc1.Characteristics1Id
                                    left outer join hkp.Characteristics c2 on c2.Id=c23.Characteristics2Id
                                    left outer join hkp.Characteristics c3 on c3.Id=c23.Characteristics3Id

                                    left outer join hkp.CharacteristicsValue cv1 on cv1.Id=sc1.CharacteristicsValue1Id
                                    left outer join hkp.CharacteristicsValue cv2 on cv2.Id=c23.CharacteristicsValue2Id
                                    left outer join hkp.CharacteristicsValue cv3 on cv3.Id=c23.CharacteristicsValue3Id

                                    left outer join scs.UnitOfMeasurement u on u.Id=d.UomId
		                            ) x
									where x.SalesOrderInvoicePackingListId='" + SalesOrderInvoicePackingListId + @"'
                              Order by x.DeliveryDateId,x.MaterialMaster,x.Submaterial,x.Detail

                                ";

            #endregion SQL

            return _sqlRepository.GetModelCollection<MaterialPackedAndInvoiced>(sql, null);
        }

        private IEnumerable<MaterialPackedAndInvoiced> Get_Invoiced_Material_Edit_Qty(string SalesOrderInvoicePackingListId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"
                                select p.SalesOrderMasterId,p.SalesOrderMaterialMasterId
                                    ,p.SalesOrderCharacteristicsValue1stId,p.SalesOrderCharacteristicsValue2ndId
                                    ,isnull(p.PQty,0) PackedQty
                                    ,isnull(i.IQty,0) InvoicedQty
                                    ,isnull(c.PQty,0) CurrentQty
                                    from
                                    (
                                    select distinct
								                                    d.SalesOrderMasterId,d.SalesOrderMaterialMasterId,
								                                    d.SalesOrderCharacteristicsValue1stId,d.SalesOrderCharacteristicsValue2ndId
								                                    from TRN.[SalesOrderInvoiceDetail] d
								                                    where d.SalesOrderInvoicePackingListId='" + SalesOrderInvoicePackingListId + @"'
								    ) mm
								                                    left outer join
                                    (
                                    select
					                                    d.SalesOrderMasterId,d.SalesOrderMaterialMasterId,
					                                    d.SalesOrderCharacteristicsValue1stId,d.SalesOrderCharacteristicsValue2ndId,
					                                    sum(isnull(d.Qty,0)) PQty
					                                    from
					                                    [TRN].[SalesOrderPackingListMaterial] d
					                                    group by d.SalesOrderMasterId,d.SalesOrderMaterialMasterId,
					                                    d.SalesOrderCharacteristicsValue1stId,d.SalesOrderCharacteristicsValue2ndId
					                 ) p on p.SalesOrderMasterId=mm.SalesOrderMasterId
				                                    and p.SalesOrderMaterialMasterId=mm.SalesOrderMaterialMasterId
				                                    and isnull(p.SalesOrderCharacteristicsValue1stId,'')=isnull(mm.SalesOrderCharacteristicsValue1stId,'')
				                                    and isnull(p.SalesOrderCharacteristicsValue2ndId,'')=isnull(mm.SalesOrderCharacteristicsValue2ndId,'')

                                    left outer join
                                    (
                                    select
				                                    d.SalesOrderMasterId,d.SalesOrderMaterialMasterId,
				                                    d.SalesOrderCharacteristicsValue1stId,d.SalesOrderCharacteristicsValue2ndId,
				                                    sum(isnull(d.Qty,0)) IQty
				                                    from
				                                    [TRN].[SalesOrderInvoiceDetail] d where d.SalesOrderInvoicePackingListId<>'" + SalesOrderInvoicePackingListId + @"'
				                                    group by d.SalesOrderMasterId,d.SalesOrderMaterialMasterId,
				                                    d.SalesOrderCharacteristicsValue1stId,d.SalesOrderCharacteristicsValue2ndId
				                    ) i on p.SalesOrderMasterId=i.SalesOrderMasterId
			                                    and p.SalesOrderMaterialMasterId=i.SalesOrderMaterialMasterId
			                                    and isnull(p.SalesOrderCharacteristicsValue1stId,'')=isnull(i.SalesOrderCharacteristicsValue1stId,'')
			                                    and isnull(p.SalesOrderCharacteristicsValue2ndId,'')=isnull(i.SalesOrderCharacteristicsValue2ndId,'')

                                    left outer join
                                    (
                                    select
				                                    d.SalesOrderMasterId,d.SalesOrderMaterialMasterId,
				                                    d.SalesOrderCharacteristicsValue1stId,d.SalesOrderCharacteristicsValue2ndId,
				                                    sum(isnull(d.Qty,0)) PQty
				                                    from
				                                    [TRN].[SalesOrderInvoiceDetail] d where d.SalesOrderInvoicePackingListId='" + SalesOrderInvoicePackingListId + @"'
				                                    group by d.SalesOrderMasterId,d.SalesOrderMaterialMasterId,
				                                    d.SalesOrderCharacteristicsValue1stId,d.SalesOrderCharacteristicsValue2ndId
				                    ) c on p.SalesOrderMasterId=c.SalesOrderMasterId
			                                    and p.SalesOrderMaterialMasterId=c.SalesOrderMaterialMasterId
			                                    and isnull(p.SalesOrderCharacteristicsValue1stId,'')=isnull(c.SalesOrderCharacteristicsValue1stId,'')
			                                    and isnull(p.SalesOrderCharacteristicsValue2ndId,'')=isnull(c.SalesOrderCharacteristicsValue2ndId,'')
								    ";
            // return _sqlRepository.GetModelCollection(sql, null);
            return _sqlRepository.GetModelCollection<MaterialPackedAndInvoiced>(sql, null);
        }

        public IEnumerable<MaterialPackedAndInvoiced> Get_Invoiced_Material_Edit_SetQty(string SalesOrderInvoicePackingListId)
        {
            IEnumerable<MaterialPackedAndInvoiced> mmlist = null;
            IEnumerable<MaterialPackedAndInvoiced> mmQty = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            mmlist = Get_Invoiced_Material_Edit(SalesOrderInvoicePackingListId);
            mmQty = Get_Invoiced_Material_Edit_Qty(SalesOrderInvoicePackingListId);

            //mmlist = _plm.SetQty(mmlist, mmQty);

            return mmlist;
        }
    }
}