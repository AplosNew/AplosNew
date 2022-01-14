using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Linq;

namespace Library.OrderManagement.Production
{
    public class ProductionReportService
    {
        private readonly SqlRepository _sqlRepository;

        #region Constructor
        public ProductionReportService()
        {
            _sqlRepository = new SqlRepository();

        }
        #endregion Constructor

        #region masterOperations
        public IEnumerable<object> getFilters()
        {
            try
            {
                var str = @"
                            Select p.Id as CustomerId,p.UserName as Customer, mo.BuyerReferenceNo as BuyerRef, mo.OwnReferenceNo as OwnRef , mo.Id as MOId,mo.MasterOrderNo as MO ,
                            moi.Id as LineItem , So.Id as SO,prs.UserName as PRStatusNA, po.planningStatus as PRStatus , so.OrderStatusId as SOOrderId,os.UserName as SOStatus ,
                            pl.Code as ProductCode ,moi.ProductLibraryId as MOILibId , ps.ProductLibraryId as PSLibId , ps.ProcessId , pc.UserName as Process
                            from trn.ProductionSummary ps
                            left join trn.ProductionOrder po on po.Id = ps.ProductionOrderId
                            left join trn.ProductionOrderDetail pod on pod.ProductionOrderId = po.Id
                            left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                            left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                            left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                            left join dbo.ProductLibrary pl on pl.Id = moi.ProductLibraryId
                            left join hkp.Party p on p.Id = mo.PartyId
                            left join hkp.ProductionStatus prs on prs.Id = po.ProductionStatusId
                            left join hkp.OrderStatus os on os.Id = so.OrderStatusId
                            left join hkp.Process pc on pc.Id = ps.ProcessId";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public IEnumerable<object> getMasterGrid(Dictionary<string, object> filters)
        {
            try
            {

                string pc = "";
                if(filters["PSLibId"].ToString() == "'','null'")
                {
                    pc = "";
                }
                else
                {
                    pc = "and ps.ProductLibraryId in ("+ filters["PSLibId"] + ")";
                }


                var str = @"Select dd.Customer ,dd.PRStatus,dd.ProductionOrderId , dd.LineItem,isnull(dd.ProductCode,'') as ProductCode, Sum(dd.OrderQty) as OrderQty , Sum(dd.PlanQty) as PlanQty , Sum(dd.ProducedQty) as ProdQty , abs(Sum(Case when dd.ShortExcess<0 then dd.ShortExcess else 0 end)) as ToProduce ,Sum(Case when dd.ShortExcess>0 then dd.ShortExcess else 0 end) as ExcessProduce 
                        from 
                        (
                        Select pps.Customer , pps.LineItem,pl.Code as ProductCode,ps.SalesOrderId , ps.ProductionOrderId, prs.UserName as PRStatus  , ps.ProcessId , ps.PlantId , c.UserName as Charac, cv.Id as CharVId ,cv.UserName as CharV , cs.UserName as Char2c , cvs.Id as Char2VId ,cvs.UserName as Char2V , pps.CharValF , pps.CharValS , pps.OrderQty , pps.PlanQty , Sum(psd.Qty) as ProducedQty , (Sum(psd.Qty) -  pps.PlanQty  ) as ShortExcess
                        --, c.Id as cc , fc.CharacteristicsId , cs.Id , sc.CharacteristicsId
                        from trn.ProductionSummary ps
                        right join trn.ProductionSummaryDetail psd on psd.ProductionSummaryId = ps.Id
                        left join hkp.Characteristics c on c.Id = psd.Characteristics1Id
                        left join hkp.CharacteristicsValue cv on cv.Id = psd.Characteristics1ValueId
                        left join hkp.Characteristics cs on cs.Id = psd.Characteristics2Id
                        left join hkp.CharacteristicsValue cvs on cvs.Id = psd.Characteristics2ValueId
                        --From SO SKU Level
                        --left join trn.SalesOrder so on so.Id = ps.SalesOrderId
                        --left join trn.FirstCharacteristics fc on fc.SalesOrderId = so.Id and psd.Characteristics1Id = fc.CharacteristicsId
                        --left join trn.SecondCharacteristics sc on sc.FirstCharacteristicsId = fc.Id and psd.Characteristics2Id = sc.CharacteristicsId
                        left join 
                        (
                        Select p.UserName as Customer,moi.Id as LineItem,So.Id , cv.Id as FirstId ,cv.UserName as CharValF , cvs.Id as SecId ,cvs.UserName as CharValS , sc.Qty as OrderQty 
                        ,Ceiling( sc.Qty/(1-(mo.ExtraOrderPercentage+mo.OrderWastagePercentage)/100)) as PlanQty
                        from trn.SalesOrder so
                        left join trn.FirstCharacteristics fc on fc.SalesOrderId = so.Id
                        left join trn.SecondCharacteristics sc on sc.FirstCharacteristicsId = fc.Id
                        left join hkp.Characteristics c on c.Id = fc.CharacteristicsId
                        left join hkp.CharacteristicsValue cv on cv.Id = fc.CharacteristicsValueId
                        left join hkp.Characteristics cs on cs.Id = sc.CharacteristicsId
                        left join hkp.CharacteristicsValue cvs on cvs.Id = sc.CharacteristicsValueId
                        left join trn.MasterOrderItem moi on moi.Id = So.MasterOrderItemId
                        left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                        left join hkp.Party p on p.Id = mo.PartyId
                        )
                        as pps on pps.Id  = ps.SalesOrderId and pps.FirstId = cv.Id and pps.SecId = cvs.Id
                        left join trn.ProductionOrder po on po.Id = ps.ProductionOrderId
                        left join hkp.ProductionStatus prs on prs.Id = po.ProductionStatusId
                        left join dbo.ProductLibrary pl on pl.Id = ps.ProductLibraryId
                        where  cv.Id is not null and cvs.Id is not null 
                        group by ps.SalesOrderId , ps.ProductionOrderId , ps.ProcessId , ps.PlantId , c.UserName , cv.UserName , cs.UserName , cvs.UserName ,cv.Id,cvs.Id, pps.CharValF , pps.CharValS , pps.OrderQty , pps.PlanQty ,  prs.UserName ,pps.Customer , pps.LineItem , pl.Code

                        ) as dd
                        group by dd.ProductionOrderId , dd.LineItem , dd.Customer , dd.PRStatus,dd.ProductCode
                            ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion masterOperations

        #region masterDetail
        public IEnumerable<object> masterDetail(string PRId , string Col)
        {
            try
            {
                var str = "";

                if(Col == "Excess Produce")
                {
                    str = @"Select dd.* from
                            (Select pps.Customer , pps.LineItem,pl.Code as ProductCode,ps.SalesOrderId , ps.ProductionOrderId, prs.UserName as PRStatus  , ps.ProcessId , ps.PlantId , c.UserName as Charac, cv.Id as CharVId ,cv.UserName as CharV , cs.UserName as Char2c , cvs.Id as Char2VId ,cvs.UserName as Char2V , pps.CharValF , pps.CharValS , pps.OrderQty , pps.PlanQty , Sum(psd.Qty) as ProducedQty , (Sum(psd.Qty) -  pps.PlanQty  ) as ShortExcess
                            --, c.Id as cc , fc.CharacteristicsId , cs.Id , sc.CharacteristicsId
                            from trn.ProductionSummary ps
                            right join trn.ProductionSummaryDetail psd on psd.ProductionSummaryId = ps.Id
                            left join hkp.Characteristics c on c.Id = psd.Characteristics1Id
                            left join hkp.CharacteristicsValue cv on cv.Id = psd.Characteristics1ValueId
                            left join hkp.Characteristics cs on cs.Id = psd.Characteristics2Id
                            left join hkp.CharacteristicsValue cvs on cvs.Id = psd.Characteristics2ValueId
                            --From SO SKU Level
                            --left join trn.SalesOrder so on so.Id = ps.SalesOrderId
                            --left join trn.FirstCharacteristics fc on fc.SalesOrderId = so.Id and psd.Characteristics1Id = fc.CharacteristicsId
                            --left join trn.SecondCharacteristics sc on sc.FirstCharacteristicsId = fc.Id and psd.Characteristics2Id = sc.CharacteristicsId
                            left join 
                            (
                            Select p.UserName as Customer,moi.Id as LineItem,So.Id , cv.Id as FirstId ,cv.UserName as CharValF , cvs.Id as SecId ,cvs.UserName as CharValS , sc.Qty as OrderQty 
                            ,Ceiling( sc.Qty/(1-(mo.ExtraOrderPercentage+mo.OrderWastagePercentage)/100)) as PlanQty
                            from trn.SalesOrder so
                            left join trn.FirstCharacteristics fc on fc.SalesOrderId = so.Id
                            left join trn.SecondCharacteristics sc on sc.FirstCharacteristicsId = fc.Id
                            left join hkp.Characteristics c on c.Id = fc.CharacteristicsId
                            left join hkp.CharacteristicsValue cv on cv.Id = fc.CharacteristicsValueId
                            left join hkp.Characteristics cs on cs.Id = sc.CharacteristicsId
                            left join hkp.CharacteristicsValue cvs on cvs.Id = sc.CharacteristicsValueId
                            left join trn.MasterOrderItem moi on moi.Id = So.MasterOrderItemId
                            left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                            left join hkp.Party p on p.Id = mo.PartyId
                            )
                            as pps on pps.Id  = ps.SalesOrderId and pps.FirstId = cv.Id and pps.SecId = cvs.Id
                            left join trn.ProductionOrder po on po.Id = ps.ProductionOrderId
                            left join hkp.ProductionStatus prs on prs.Id = po.ProductionStatusId
                            left join dbo.ProductLibrary pl on pl.Id = ps.ProductLibraryId
                            where  cv.Id is not null and cvs.Id is not null and ps.ProductionOrderId = '"+ PRId + @"'
                            group by ps.SalesOrderId , ps.ProductionOrderId , ps.ProcessId , ps.PlantId , c.UserName , cv.UserName , cs.UserName , cvs.UserName ,cv.Id,cvs.Id, pps.CharValF , pps.CharValS , pps.OrderQty , pps.PlanQty ,  prs.UserName ,pps.Customer , pps.LineItem , pl.Code
                           ) as dd
						   where ShortExcess>0
                            ";

                    
                }
                else if (Col == "To Produce")
                {
                    str = @"Select dd.* from
                            (Select pps.Customer , pps.LineItem,pl.Code as ProductCode,ps.SalesOrderId , ps.ProductionOrderId, prs.UserName as PRStatus  , ps.ProcessId , ps.PlantId , c.UserName as Charac, cv.Id as CharVId ,cv.UserName as CharV , cs.UserName as Char2c , cvs.Id as Char2VId ,cvs.UserName as Char2V , pps.CharValF , pps.CharValS , pps.OrderQty , pps.PlanQty , Sum(psd.Qty) as ProducedQty , (Sum(psd.Qty) -  pps.PlanQty  ) as ShortExcess
                            --, c.Id as cc , fc.CharacteristicsId , cs.Id , sc.CharacteristicsId
                            from trn.ProductionSummary ps
                            right join trn.ProductionSummaryDetail psd on psd.ProductionSummaryId = ps.Id
                            left join hkp.Characteristics c on c.Id = psd.Characteristics1Id
                            left join hkp.CharacteristicsValue cv on cv.Id = psd.Characteristics1ValueId
                            left join hkp.Characteristics cs on cs.Id = psd.Characteristics2Id
                            left join hkp.CharacteristicsValue cvs on cvs.Id = psd.Characteristics2ValueId
                            --From SO SKU Level
                            --left join trn.SalesOrder so on so.Id = ps.SalesOrderId
                            --left join trn.FirstCharacteristics fc on fc.SalesOrderId = so.Id and psd.Characteristics1Id = fc.CharacteristicsId
                            --left join trn.SecondCharacteristics sc on sc.FirstCharacteristicsId = fc.Id and psd.Characteristics2Id = sc.CharacteristicsId
                            left join 
                            (
                            Select p.UserName as Customer,moi.Id as LineItem,So.Id , cv.Id as FirstId ,cv.UserName as CharValF , cvs.Id as SecId ,cvs.UserName as CharValS , sc.Qty as OrderQty 
                            ,Ceiling( sc.Qty/(1-(mo.ExtraOrderPercentage+mo.OrderWastagePercentage)/100)) as PlanQty
                            from trn.SalesOrder so
                            left join trn.FirstCharacteristics fc on fc.SalesOrderId = so.Id
                            left join trn.SecondCharacteristics sc on sc.FirstCharacteristicsId = fc.Id
                            left join hkp.Characteristics c on c.Id = fc.CharacteristicsId
                            left join hkp.CharacteristicsValue cv on cv.Id = fc.CharacteristicsValueId
                            left join hkp.Characteristics cs on cs.Id = sc.CharacteristicsId
                            left join hkp.CharacteristicsValue cvs on cvs.Id = sc.CharacteristicsValueId
                            left join trn.MasterOrderItem moi on moi.Id = So.MasterOrderItemId
                            left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                            left join hkp.Party p on p.Id = mo.PartyId
                            )
                            as pps on pps.Id  = ps.SalesOrderId and pps.FirstId = cv.Id and pps.SecId = cvs.Id
                            left join trn.ProductionOrder po on po.Id = ps.ProductionOrderId
                            left join hkp.ProductionStatus prs on prs.Id = po.ProductionStatusId
                            left join dbo.ProductLibrary pl on pl.Id = ps.ProductLibraryId
                            where  cv.Id is not null and cvs.Id is not null and ps.ProductionOrderId = '"+ PRId + @"'
                            group by ps.SalesOrderId , ps.ProductionOrderId , ps.ProcessId , ps.PlantId , c.UserName , cv.UserName , cs.UserName , cvs.UserName ,cv.Id,cvs.Id, pps.CharValF , pps.CharValS , pps.OrderQty , pps.PlanQty ,  prs.UserName ,pps.Customer , pps.LineItem , pl.Code
                           ) as dd
						   where ShortExcess<0
                            ";
                    
                }
                else
                {
                    str = @"Select pps.Customer , pps.LineItem,pl.Code as ProductCode,ps.SalesOrderId , ps.ProductionOrderId, prs.UserName as PRStatus  , ps.ProcessId , ps.PlantId , c.UserName as Charac, cv.Id as CharVId ,cv.UserName as CharV , cs.UserName as Char2c , cvs.Id as Char2VId ,cvs.UserName as Char2V , pps.CharValF , pps.CharValS , pps.OrderQty , pps.PlanQty , Sum(psd.Qty) as ProducedQty , (Sum(psd.Qty) -  pps.PlanQty  ) as ShortExcess
                            --, c.Id as cc , fc.CharacteristicsId , cs.Id , sc.CharacteristicsId
                            from trn.ProductionSummary ps
                            right join trn.ProductionSummaryDetail psd on psd.ProductionSummaryId = ps.Id
                            left join hkp.Characteristics c on c.Id = psd.Characteristics1Id
                            left join hkp.CharacteristicsValue cv on cv.Id = psd.Characteristics1ValueId
                            left join hkp.Characteristics cs on cs.Id = psd.Characteristics2Id
                            left join hkp.CharacteristicsValue cvs on cvs.Id = psd.Characteristics2ValueId
                            --From SO SKU Level
                            --left join trn.SalesOrder so on so.Id = ps.SalesOrderId
                            --left join trn.FirstCharacteristics fc on fc.SalesOrderId = so.Id and psd.Characteristics1Id = fc.CharacteristicsId
                            --left join trn.SecondCharacteristics sc on sc.FirstCharacteristicsId = fc.Id and psd.Characteristics2Id = sc.CharacteristicsId
                            left join 
                            (
                            Select p.UserName as Customer,moi.Id as LineItem,So.Id , cv.Id as FirstId ,cv.UserName as CharValF , cvs.Id as SecId ,cvs.UserName as CharValS , sc.Qty as OrderQty 
                            ,Ceiling( sc.Qty/(1-(mo.ExtraOrderPercentage+mo.OrderWastagePercentage)/100)) as PlanQty
                            from trn.SalesOrder so
                            left join trn.FirstCharacteristics fc on fc.SalesOrderId = so.Id
                            left join trn.SecondCharacteristics sc on sc.FirstCharacteristicsId = fc.Id
                            left join hkp.Characteristics c on c.Id = fc.CharacteristicsId
                            left join hkp.CharacteristicsValue cv on cv.Id = fc.CharacteristicsValueId
                            left join hkp.Characteristics cs on cs.Id = sc.CharacteristicsId
                            left join hkp.CharacteristicsValue cvs on cvs.Id = sc.CharacteristicsValueId
                            left join trn.MasterOrderItem moi on moi.Id = So.MasterOrderItemId
                            left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                            left join hkp.Party p on p.Id = mo.PartyId
                            )
                            as pps on pps.Id  = ps.SalesOrderId and pps.FirstId = cv.Id and pps.SecId = cvs.Id
                            left join trn.ProductionOrder po on po.Id = ps.ProductionOrderId
                            left join hkp.ProductionStatus prs on prs.Id = po.ProductionStatusId
                            left join dbo.ProductLibrary pl on pl.Id = ps.ProductLibraryId
                            where  cv.Id is not null and cvs.Id is not null and ps.ProductionOrderId = '" + PRId + @"'
                            group by ps.SalesOrderId , ps.ProductionOrderId , ps.ProcessId , ps.PlantId , c.UserName , cv.UserName , cs.UserName , cvs.UserName ,cv.Id,cvs.Id, pps.CharValF , pps.CharValS , pps.OrderQty , pps.PlanQty ,  prs.UserName ,pps.Customer , pps.LineItem , pl.Code
                            ";
                }

                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion masterDetail



    }
}
