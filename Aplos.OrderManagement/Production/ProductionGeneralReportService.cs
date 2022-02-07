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
    public class ProductionGeneralReportService
    {
        private readonly SqlRepository _sqlRepository;

        #region Constructor
        public ProductionGeneralReportService()
        {
            _sqlRepository = new SqlRepository();

        }
        #endregion Constructor

        #region ProcessWise
        

        #region masterOperations

        public IEnumerable<object> getProcess()
        {
            try
            {
                // var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var str = "Select Id as Value , UserName as Text from hkp.process order by UserName asc";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> getFilters()
        {
            try
            {
                var str = @"
                            Select p.Id as CustomerId,p.UserName as Customer, mo.BuyerReferenceNo as BuyerRef, mo.OwnReferenceNo as OwnRef , mo.Id as MOId,mo.MasterOrderNo as MO ,
                            moi.Id as LineItem , So.Id as SO,prs.Id as PRStatId,prs.UserName as PRStatus, po.planningStatus as PRStatusNA , so.OrderStatusId as SOOrderId,os.UserName as SOStatus ,
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

        public IEnumerable<object> getMasterGrid(Dictionary<string, object> filters, string ProcessId)
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


                var str = @"Select dd.Customer ,dd.PRStatus,dd.ProductionOrderId , dd.BuyerRef , dd.OwnRef, dd.LineItem,isnull(dd.ProductCode,'') as ProductCode, Sum(dd.OrderQty) as OrderQty , Sum(dd.PlanQty) as PlanQty , Sum(dd.ProducedQty) as ProdQty , abs(Sum(Case when dd.ShortExcess<0 then dd.ShortExcess else 0 end)) as ToProduce ,Sum(Case when dd.ShortExcess>0 then dd.ShortExcess else 0 end) as ExcessProduce 
                        from 
                        (
                        Select pps.Customer , pps.LineItem,pl.Code as ProductCode,ps.SalesOrderId , pps.BuyerRef , pps.OwnRef , ps.ProductionOrderId, prs.UserName as PRStatus  , ps.ProcessId , ps.PlantId , c.UserName as Charac, cv.Id as CharVId ,cv.UserName as CharV , cs.UserName as Char2c , cvs.Id as Char2VId ,cvs.UserName as Char2V , pps.CharValF , pps.CharValS , pps.OrderQty , pps.PlanQty , Sum(psd.Qty) as ProducedQty , (Sum(psd.Qty) -  pps.PlanQty  ) as ShortExcess 
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
                        Select p.Id as CusId,p.UserName as Customer,moi.Id as LineItem,So.Id , mo.BuyerReferenceNo as BuyerRef, mo.OwnReferenceNo as OwnRef,cv.Id as FirstId ,cv.UserName as CharValF , cvs.Id as SecId ,cvs.UserName as CharValS , sc.Qty as OrderQty 
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
						where  p.Id in ("+filters["CustomerId"]+ @")  and  mo.BuyerReferenceNo in (" + filters["BuyerRef"] + @") and mo.OwnReferenceNo in (" + filters["OwnRef"] + @") and mo.MasterOrderNo in (" + filters["MOId"] + @") and moi.Id in (" + filters["LineItem"] + @") and so.Id in (" + filters["SO"] + @") and so.OrderStatusId in (" + filters["SOOrderId"] + @")
                        )
                        as pps on pps.Id  = ps.SalesOrderId and pps.FirstId = cv.Id and pps.SecId = cvs.Id
                        left join trn.ProductionOrder po on po.Id = ps.ProductionOrderId
                        left join hkp.ProductionStatus prs on prs.Id = po.ProductionStatusId
                        left join dbo.ProductLibrary pl on pl.Id = ps.ProductLibraryId
                        where  cv.Id is not null and cvs.Id is not null and ps.ProcessId  = '"+ ProcessId + @"'
						and ps.SalesOrderId in (" + filters["SO"] + @") and po.ProductionStatusId in (" + filters["PRStatId"] + @")
						 "+pc+@"
                        group by ps.SalesOrderId , ps.ProductionOrderId , ps.ProcessId , ps.PlantId ,
                        c.UserName , cv.UserName , cs.UserName , cvs.UserName ,cv.Id,cvs.Id, pps.CharValF , pps.CharValS , 
                        pps.OrderQty , pps.PlanQty ,  prs.UserName ,pps.Customer , pps.LineItem , pl.Code, pps.BuyerRef , pps.OwnRef 

                        ) as dd
                        group by dd.ProductionOrderId , dd.LineItem , dd.Customer , dd.PRStatus,dd.ProductCode, dd.BuyerRef , dd.OwnRef
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
        public IEnumerable<object> masterDetail(string PRId , string Col , Dictionary<string, object> filters , string ProcessId)
        {
            try
            {
                var str = "";
                string pc = "";
                if (filters["PSLibId"].ToString() == "'','null'")
                {
                    pc = "";
                }
                else
                {
                    pc = "and ps.ProductLibraryId in (" + filters["PSLibId"] + ")";
                }
                if (Col == "Excess Produce")
                {
                    str = @"Select dd.* from
                            (Select pps.Customer , pps.LineItem,pl.Code as ProductCode,ps.SalesOrderId ,pps.BuyerRef,pps.OwnRef , ps.ProductionOrderId, prs.UserName as PRStatus  , ps.ProcessId ,
                            ps.PlantId , c.UserName as Charac, cv.Id as CharVId ,cv.UserName as CharV , cs.UserName as Char2c , cvs.Id as Char2VId ,
                            cvs.UserName as Char2V , pps.CharValF , pps.CharValS , pps.OrderQty , pps.PlanQty , Sum(psd.Qty) as ProducedQty ,
                            (Sum(psd.Qty) -  pps.PlanQty  ) as ShortExcess
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
                            Select p.UserName as Customer,moi.Id as LineItem,mo.BuyerReferenceNo as BuyerRef, mo.OwnReferenceNo as OwnRef,So.Id , cv.Id as FirstId ,cv.UserName as CharValF , cvs.Id as SecId ,cvs.UserName as CharValS , sc.Qty as OrderQty 
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
                            where  p.Id in (" + filters["CustomerId"] + @")  and  mo.BuyerReferenceNo in (" + filters["BuyerRef"] + @") and mo.OwnReferenceNo in (" + filters["OwnRef"] + @") and mo.MasterOrderNo in (" + filters["MOId"] + @") and moi.Id in (" + filters["LineItem"] + @") and so.Id in (" + filters["SO"] + @") and so.OrderStatusId in (" + filters["SOOrderId"] + @")
                        )
                        as pps on pps.Id  = ps.SalesOrderId and pps.FirstId = cv.Id and pps.SecId = cvs.Id
                        left join trn.ProductionOrder po on po.Id = ps.ProductionOrderId
                        left join hkp.ProductionStatus prs on prs.Id = po.ProductionStatusId
                        left join dbo.ProductLibrary pl on pl.Id = ps.ProductLibraryId
                        where  cv.Id is not null and cvs.Id is not null and ps.ProductionOrderId = '" + PRId + @"'  and ps.ProcessId = '" + ProcessId + @"'
						and ps.SalesOrderId in (" + filters["SO"] + @") and po.ProductionStatusId in (" + filters["PRStatId"] + @")
						 " + pc + @"
                            group by ps.SalesOrderId , ps.ProductionOrderId , ps.ProcessId , ps.PlantId , c.UserName , cv.UserName , cs.UserName ,
                            cvs.UserName ,cv.Id,cvs.Id, pps.CharValF , pps.CharValS , pps.OrderQty , pps.PlanQty ,  prs.UserName ,pps.Customer , 
                            pps.LineItem , pl.Code, pps.BuyerRef, pps.OwnRef
                           ) as dd
						   where ShortExcess>0
                            ";

                    
                }
                else if (Col == "To Produce")
                {
                    str = @"Select dd.* from
                            (Select pps.Customer , pps.LineItem,pl.Code as ProductCode,ps.SalesOrderId , ps.ProductionOrderId, prs.UserName as PRStatus  ,pps.BuyerRef,pps.OwnRef , 
                            ps.ProcessId , ps.PlantId , c.UserName as Charac, cv.Id as CharVId ,cv.UserName as CharV , cs.UserName as Char2c ,
                            cvs.Id as Char2VId ,cvs.UserName as Char2V , pps.CharValF , pps.CharValS , pps.OrderQty , pps.PlanQty , Sum(psd.Qty) as ProducedQty ,
                            (Sum(psd.Qty) -  pps.PlanQty  ) as ShortExcess
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
                            Select p.UserName as Customer,moi.Id as LineItem,mo.BuyerReferenceNo as BuyerRef, mo.OwnReferenceNo as OwnRef,So.Id , cv.Id as FirstId ,cv.UserName as CharValF , cvs.Id as SecId ,cvs.UserName as CharValS , sc.Qty as OrderQty 
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
                            where  p.Id in (" + filters["CustomerId"] + @")  and  mo.BuyerReferenceNo in (" + filters["BuyerRef"] + @") and mo.OwnReferenceNo in (" + filters["OwnRef"] + @") and mo.MasterOrderNo in (" + filters["MOId"] + @") and moi.Id in (" + filters["LineItem"] + @") and so.Id in (" + filters["SO"] + @") and so.OrderStatusId in (" + filters["SOOrderId"] + @")
                        )
                        as pps on pps.Id  = ps.SalesOrderId and pps.FirstId = cv.Id and pps.SecId = cvs.Id
                        left join trn.ProductionOrder po on po.Id = ps.ProductionOrderId
                        left join hkp.ProductionStatus prs on prs.Id = po.ProductionStatusId
                        left join dbo.ProductLibrary pl on pl.Id = ps.ProductLibraryId
                        where  cv.Id is not null and cvs.Id is not null and ps.ProductionOrderId = '" + PRId + @"' and ps.ProcessId = '" + ProcessId + @"'
						and ps.SalesOrderId in (" + filters["SO"] + @") and po.ProductionStatusId in (" + filters["PRStatId"] + @")
						 " + pc + @"
                            group by ps.SalesOrderId , ps.ProductionOrderId , ps.ProcessId , ps.PlantId , c.UserName , cv.UserName ,
                            cs.UserName , cvs.UserName ,cv.Id,cvs.Id, pps.CharValF , pps.CharValS , pps.OrderQty , pps.PlanQty ,  prs.UserName ,
                            pps.Customer , pps.LineItem , pl.Code, pps.BuyerRef, pps.OwnRef
                           ) as dd
						   where ShortExcess<0
                            ";
                    
                }
                else
                {
                    str = @"Select pps.Customer , pps.LineItem,pl.Code as ProductCode,ps.SalesOrderId , ps.ProductionOrderId, prs.UserName as PRStatus  , ps.ProcessId ,pps.BuyerRef,pps.OwnRef ,
                            ps.PlantId , c.UserName as Charac, cv.Id as CharVId ,cv.UserName as CharV , cs.UserName as Char2c , cvs.Id as Char2VId ,
                            cvs.UserName as Char2V , pps.CharValF , pps.CharValS , pps.OrderQty , pps.PlanQty , Sum(psd.Qty) as ProducedQty ,
                            (Sum(psd.Qty) -  pps.PlanQty  ) as ShortExcess
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
                            Select p.UserName as Customer,moi.Id as LineItem,mo.BuyerReferenceNo as BuyerRef, mo.OwnReferenceNo as OwnRef,So.Id , cv.Id as FirstId ,cv.UserName as CharValF , cvs.Id as SecId ,cvs.UserName as CharValS , sc.Qty as OrderQty 
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
                            where  p.Id in (" + filters["CustomerId"] + @")  and  mo.BuyerReferenceNo in (" + filters["BuyerRef"] + @") and mo.OwnReferenceNo in (" + filters["OwnRef"] + @") and mo.MasterOrderNo in (" + filters["MOId"] + @") and moi.Id in (" + filters["LineItem"] + @") and so.Id in (" + filters["SO"] + @") and so.OrderStatusId in (" + filters["SOOrderId"] + @")
                        )
                        as pps on pps.Id  = ps.SalesOrderId and pps.FirstId = cv.Id and pps.SecId = cvs.Id
                        left join trn.ProductionOrder po on po.Id = ps.ProductionOrderId
                        left join hkp.ProductionStatus prs on prs.Id = po.ProductionStatusId
                        left join dbo.ProductLibrary pl on pl.Id = ps.ProductLibraryId
                        where  cv.Id is not null and cvs.Id is not null  and ps.ProductionOrderId = '" + PRId + @"' and ps.ProcessId = '" + ProcessId + @"'
						and ps.SalesOrderId in (" + filters["SO"] + @") and po.ProductionStatusId in (" + filters["PRStatId"] + @")
						 " + pc + @"
                            group by ps.SalesOrderId , ps.ProductionOrderId , ps.ProcessId , ps.PlantId , c.UserName , cv.UserName ,
                            cs.UserName , cvs.UserName ,cv.Id,cvs.Id, pps.CharValF , pps.CharValS , pps.OrderQty , pps.PlanQty ,  prs.UserName ,
                            pps.Customer , pps.LineItem , pl.Code, pps.BuyerRef, pps.OwnRef
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

        #region Report
        public DataTable getReports(string PRId , Dictionary<string, object> filters , string ProcessId)
        {
            try
            {
                string pc = "";
                if (filters["PSLibId"].ToString() == "'','null'")
                {
                    pc = "";
                }
                else
                {
                    pc = "and ps.ProductLibraryId in (" + filters["PSLibId"] + ")";
                }
                var str = @"Select dd.* , abs((Case when dd.ShortExcess<0 then dd.ShortExcess else 0 end)) as ToProduce ,
                            (Case when dd.ShortExcess>0 then dd.ShortExcess else 0 end) as ExcessProduce  ,
                           FLOOR((dd.ProducedQty/dd.OrderQty)*100) as Percents
                            from
                            (Select ps.ProductionOrderId, pps.Customer as Buyer, pps.MasterOrderNo ,pps.BuyerRef, pps.OwnRef ,pps.LineItem,pl.Code as ProductCode,ps.SalesOrderId ,  prs.UserName as PRStatus  , ps.ProcessId , ps.PlantId , c.UserName as Charac, cv.Id as CharVId ,cv.UserName as CharV , cs.UserName as Char2c , cvs.Id as Char2VId ,cvs.UserName as Char2V , pps.CharValF , pps.CharValS , pps.OrderQty , pps.PlanQty , Sum(psd.Qty) as ProducedQty , (Sum(psd.Qty) -  pps.PlanQty  ) as ShortExcess
                            --, c.Id as cc , fc.CharacteristicsId , cs.Id , sc.CharacteristicsId
                            from trn.ProductionSummary ps
                            right join trn.ProductionSummaryDetail psd on psd.ProductionSummaryId = ps.Id
                            left join hkp.Characteristics c on c.Id = psd.Characteristics1Id
                            left join hkp.CharacteristicsValue cv on cv.Id = psd.Characteristics1ValueId
                            left join hkp.Characteristics cs on cs.Id = psd.Characteristics2Id
                            left join hkp.CharacteristicsValue cvs on cvs.Id = psd.Characteristics2ValueId
                           
                            left join 
                            (
                            Select p.UserName as Customer,mo.MasterOrderNo,mo.BuyerReferenceNo as BuyerRef, mo.OwnReferenceNo as OwnRef,moi.Id as LineItem,So.Id , cv.Id as FirstId ,cv.UserName as CharValF , cvs.Id as SecId ,cvs.UserName as CharValS , sc.Qty as OrderQty 
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
                            where  p.Id in (" + filters["CustomerId"] + @")  and  mo.BuyerReferenceNo in (" + filters["BuyerRef"] + @") and mo.OwnReferenceNo in (" + filters["OwnRef"] + @") and mo.MasterOrderNo in (" + filters["MOId"] + @") and moi.Id in (" + filters["LineItem"] + @") and so.Id in (" + filters["SO"] + @") and so.OrderStatusId in (" + filters["SOOrderId"] + @")
                        )
                        as pps on pps.Id  = ps.SalesOrderId and pps.FirstId = cv.Id and pps.SecId = cvs.Id
                        left join trn.ProductionOrder po on po.Id = ps.ProductionOrderId
                        left join hkp.ProductionStatus prs on prs.Id = po.ProductionStatusId
                        left join dbo.ProductLibrary pl on pl.Id = ps.ProductLibraryId
                        where  cv.Id is not null and cvs.Id is not null and ps.ProductionOrderId = '" + PRId + @"' and ps.ProcessId = '" + ProcessId + @"'
						and ps.SalesOrderId in (" + filters["SO"] + @") and po.ProductionStatusId in (" + filters["PRStatId"] + @")
						 " + pc + @"
                            group by ps.SalesOrderId , ps.ProductionOrderId , ps.ProcessId , ps.PlantId , c.UserName , cv.UserName , cs.UserName , cvs.UserName ,cv.Id,cvs.Id, pps.CharValF , pps.CharValS , pps.OrderQty , pps.PlanQty ,  prs.UserName ,pps.Customer , pps.LineItem , pl.Code , pps.BuyerRef , pps.OwnRef , pps.MasterOrderNo
                           ) as dd";
                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion Report

        #endregion ProcessWise

        #region POWise

        #region GetOperations

        public IEnumerable<object> getPo()
        {
            try
            {
                var str = @"Select distinct po.Id as Text 
                            from trn.ProductionSummary ps
                            left join trn.ProductionOrder po on po.ID = ps.ProductionOrderId";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion GetOperations

        #region MasterGrid

        public IEnumerable<object> generate(string PO)
        {
            try
            {

                #region Query
                //var CStr = @"Select base.*,
                //             isnull(prod.Produced,0) as ProducedQty  
                //                                        , STUFF((
                //                                        Select distinct ','+ so.Id
                //                                        from trn.ProductionOrder pos
                //                                        left join trn.ProductionOrderDetail pod on pod.ProductionOrderId = pos.Id
                //                                        left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                //                                        where pos.Id = base.ProductionId	
                //                                        FOR XML PATH('')
                //                                        ),1,1,'') as SOId 
                //                                        , STUFF((
                //                                        Select distinct ','+ b.UserName 
                //                                        from trn.ProductionOrder pos
                //                                        left join trn.ProductionOrderDetail pod on pod.ProductionOrderId = pos.Id
                //                                        left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                //                                        left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                //                                        left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                //                                        left join hkp.Buyer b on b.Id = mo.BuyerId
                //                                        where pos.Id = base.ProductionId
                //                                        FOR XML PATH('')
                //                                        ),1,1,'') as Buyer 
                //                                        , STUFF((
                //                                        Select distinct ','+ mo.BuyerReferenceNo 
                //                                        from trn.ProductionOrder pos
                //                                        left join trn.ProductionOrderDetail pod on pod.ProductionOrderId = pos.Id
                //                                        left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                //                                        left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                //                                        left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                //                                        where pos.Id = base.ProductionId
                //                                        FOR XML PATH('')
                //                                        ),1,1,'') as BuyRef 
                //                                        , STUFF((
                //                                        Select distinct ','+ mo.OwnReferenceNo 
                //                                        from trn.ProductionOrder pos
                //                                        left join trn.ProductionOrderDetail pod on pod.ProductionOrderId = pos.Id
                //                                        left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                //                                        left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                //                                        left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                //                                        where pos.Id = base.ProductionId
                //                                        FOR XML PATH('')
                //                                        ),1,1,'') as OwnRef 
                //            from
                //            (
                //            Select po.Id as ProductionId , po.EntityId , pps.ProcessId , p.UserName as Process, pps.Sequence 
                //            , ept.ProductionBookingLevel
                //                                        ,fc.CharacteristicsId , fc.CharacteristicsValueId , c.UserName as Chars , cv.UserName as CharV , Sum(fc.Qty) as OrderQty 
                //                                        , Sum(Ceiling( fc.Qty/(1-(mo.ExtraOrderPercentage+mo.OrderWastagePercentage)/100))) as PlanQty  
                //            from trn.ProductionOrder po
                //            left join trn.ProductionOrderProcessSet pps on pps.ProductionOrderId = po.Id 
                //            left join hkp.Process p on p.Id = pps.ProcessId
                //            left join hkp.EntityProcessTag ept on ept.EntityId = po.EntityId and ept.ProcessId = pps.ProcessId
                //            left join trn.ProductionOrderDetail pod on pod.ProductionOrderId = po.Id
                //            left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                //            left join trn.FirstCharacteristics fc on fc.SalesOrderId = so.Id
                //            left join hkp.Characteristics c on c.Id = fc.CharacteristicsId
                //            left join hkp.CharacteristicsValue cv on cv.Id = fc.CharacteristicsValueId
                //            left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                //            left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                //            where po.Id = '215'
                //            group by  po.Id , po.EntityId , pps.ProcessId , p.UserName, pps.Sequence 
                //            , ept.ProductionBookingLevel , c.UserName , cv.UserName,fc.CharacteristicsId , fc.CharacteristicsValueId 
                //            ) as base
                //            left join 
                //            (
                //            Select ps.ProcessId , ps.ProductionOrderId , psd.Characteristics1Id , psd.Characteristics1ValueId ,  c.UserName as Chars , cv.UserName as CharsV , Sum(psd.Qty) as Produced
                //            from trn.ProductionSummary ps
                //            left join trn.ProductionSummaryDetail psd on psd.ProductionSummaryId = ps.Id
                //            left join hkp.Characteristics c on c.Id = psd.Characteristics1Id
                //            left join hkp.CharacteristicsValue cv on cv.Id = psd.Characteristics1ValueId
                //            where ps.ProductionOrderId = '215' and psd.Characteristics1Id is not null
                //            group by ps.ProcessId , ps.ProductionOrderId , psd.Characteristics1Id , psd.Characteristics1ValueId ,  c.UserName , cv.UserName 
                //            ) 
                //            as prod on prod.ProductionOrderId = base.ProductionId and prod.ProcessId = base.ProcessId and prod.Characteristics1Id = base.CharacteristicsId and prod.Characteristics1ValueId = base.CharacteristicsValueId
                //            order by base.CharV,base.Sequence";
                #endregion Query


                var str = @"Select  po.Id as ProductionId , po.EntityId , pps.ProcessId , p.UserName as Process, pps.Sequence , ept.ProductionBookingLevel
                            , c.UserName as Chars , cv.UserName as CharV , fc.Qty as OrderQty 
                            , Ceiling( fc.Qty/(1-(mo.ExtraOrderPercentage+mo.OrderWastagePercentage)/100)) as PlanQty  , isnull(prod.Produced,0) as ProducedQty  
                            , STUFF((
                            Select distinct ','+ so.Id
                            from trn.ProductionOrder pos
                            left join trn.ProductionOrderDetail pod on pod.ProductionOrderId = pos.Id
                            left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                            where pos.Id = po.Id
                            FOR XML PATH('')
                            ),1,1,'') as SOId 
                            , STUFF((
                            Select distinct ','+ b.UserName 
                            from trn.ProductionOrder pos
                            left join trn.ProductionOrderDetail pod on pod.ProductionOrderId = pos.Id
                            left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                            left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                            left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                            left join hkp.Buyer b on b.Id = mo.BuyerId
                            where pos.Id = po.Id
                            FOR XML PATH('')
                            ),1,1,'') as Buyer 
                            , STUFF((
                            Select distinct ','+ mo.BuyerReferenceNo 
                            from trn.ProductionOrder pos
                            left join trn.ProductionOrderDetail pod on pod.ProductionOrderId = pos.Id
                            left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                            left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                            left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                            where pos.Id = po.Id
                            FOR XML PATH('')
                            ),1,1,'') as BuyRef 
                            , STUFF((
                            Select distinct ','+ mo.OwnReferenceNo 
                            from trn.ProductionOrder pos
                            left join trn.ProductionOrderDetail pod on pod.ProductionOrderId = pos.Id
                            left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                            left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                            left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                            where pos.Id = po.Id
                            FOR XML PATH('')
                            ),1,1,'') as OwnRef 
                            from trn.ProductionOrder po
                            left join trn.ProductionOrderProcessSet pps on pps.ProductionOrderId = po.Id 
                            left join hkp.Process p on p.Id = pps.ProcessId
                            left join hkp.EntityProcessTag ept on ept.EntityId = po.EntityId and ept.ProcessId = pps.ProcessId
                            left join trn.ProductionOrderDetail pod on pod.ProductionOrderId = po.Id
                            left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                            left join trn.FirstCharacteristics fc on fc.SalesOrderId = so.Id
                            left join hkp.Characteristics c on c.Id = fc.CharacteristicsId
                            left join hkp.CharacteristicsValue cv on cv.Id = fc.CharacteristicsValueId
                            left join 
                            (
                            Select ps.ProcessId , ps.ProductionOrderId , psd.Characteristics1Id , psd.Characteristics1ValueId ,  c.UserName as Chars , cv.UserName as CharsV , Sum(psd.Qty) as Produced
                            from trn.ProductionSummary ps
                            left join trn.ProductionSummaryDetail psd on psd.ProductionSummaryId = ps.Id
                            left join hkp.Characteristics c on c.Id = psd.Characteristics1Id
                            left join hkp.CharacteristicsValue cv on cv.Id = psd.Characteristics1ValueId
                            where ps.ProductionOrderId = '"+PO+@"' and psd.Characteristics1Id is not null
                            group by ps.ProcessId , ps.ProductionOrderId , psd.Characteristics1Id , psd.Characteristics1ValueId ,  c.UserName , cv.UserName 
                            ) 
                            as prod on prod.ProductionOrderId = po.Id and prod.ProcessId = pps.ProcessId and prod.Characteristics1Id = fc.CharacteristicsId and prod.Characteristics1ValueId = fc.CharacteristicsValueId
                            left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                            left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                            where po.id ='"+PO+@"' 
                            order by cv.Id,pps.Sequence";

                var proclist = @"Select pps.ProductionOrderId,pps.ProcessId , pps.Sequence , p.UserName from trn.ProductionOrderProcessSet pps 
                                    left join hkp.Process p on p.Id = pps.ProcessId
                                    where ProductionOrderId = '"+PO+@"' order by Sequence";

                DataTable dtFirst =  _sqlRepository.GetDataTable(str);
                DataTable dtProcess = _sqlRepository.GetDataTable(proclist);

                DataTable ff = new DataTable();
                ff.Columns.Add("PO", typeof(string));
                ff.Columns.Add("SO", typeof(string));
                ff.Columns.Add("Buyer", typeof(string));
                ff.Columns.Add("BuyerRef", typeof(string));
                ff.Columns.Add("OwnRef", typeof(string));
                ff.Columns.Add("SKU1", typeof(string));
                ff.Columns.Add("OrderQty", typeof(double));
                ff.Columns.Add("PlanQty", typeof(double));

                for(int i = 0; i < dtProcess.Rows.Count; i++)
                {
                    string s = dtProcess.Rows[i]["UserName"].ToString() + "Qty";
                    ff.Columns.Add(s , typeof(string));
                }

                for (int i = 1; i < dtProcess.Rows.Count; i++)
                {
                    string s = dtProcess.Rows[i]["UserName"].ToString() + "WIP";
                    ff.Columns.Add(s, typeof(string));
                }


                DataRow dr = ff.NewRow();
                if (dtFirst.Rows.Count > 0)
                {
                    dr["PO"] = dtFirst.Rows[0]["ProductionId"].ToString();
                    dr["SO"] = dtFirst.Rows[0]["SOId"].ToString();
                    dr["Buyer"] = dtFirst.Rows[0]["Buyer"].ToString();
                    dr["BuyerRef"] = dtFirst.Rows[0]["BuyRef"].ToString();
                    dr["OwnRef"] = dtFirst.Rows[0]["OwnRef"].ToString();
                    dr["OwnRef"] = dtFirst.Rows[0]["OwnRef"].ToString();
                    dr["SKU1"] = dtFirst.Rows[0]["CharV"].ToString();
                    dr["OrderQty"] = clsStaticInfo.dbl(dtFirst.Rows[0]["OrderQty"].ToString());
                    dr["PlanQty"] = clsStaticInfo.dbl(dtFirst.Rows[0]["PlanQty"].ToString());
                }

                if (dtFirst.Rows.Count > 0)
                {
                    for (int i = 0; i < dtFirst.Rows.Count; i++)
                    {
                        if(dr["SKU1"].ToString() != dtFirst.Rows[i]["CharV"].ToString())
                        {
                            ff.Rows.Add(dr);
                            dr = ff.NewRow();
                            dr["SKU1"] = dtFirst.Rows[i]["CharV"].ToString();
                            dr["OrderQty"] = clsStaticInfo.dbl(dtFirst.Rows[0]["OrderQty"].ToString());
                            dr["PlanQty"] = clsStaticInfo.dbl(dtFirst.Rows[0]["PlanQty"].ToString());
                        }
                        //else
                        //{
                            string pQty = dtFirst.Rows[i]["Process"].ToString() + "Qty";
                            string pWip = "";
                            if( clsStaticInfo.dbl(dtFirst.Rows[i]["Sequence"].ToString()) != 1 )
                            {
                                pWip = dtFirst.Rows[i]["Process"].ToString() + "WIP";
                            }

                            dr[pQty] = clsStaticInfo.dbl(dtFirst.Rows[i]["ProducedQty"].ToString());
                            if (pWip != "")
                            {
                                dr[pWip] = clsStaticInfo.dbl(dtFirst.Rows[i - 1]["ProducedQty"].ToString()) - clsStaticInfo.dbl(dtFirst.Rows[i]["ProducedQty"].ToString());
                            }

                        //}

                    }

                    ff.Rows.Add(dr);
                }


                return Library.Service.Helpers.DataTableExtensions.DataTableToJson(ff);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable generateReport(string PO , out List<string> DynCols)
        {
            try
            {

                #region Query
                //var CStr = @"Select base.*,
                //             isnull(prod.Produced,0) as ProducedQty  
                //                                        , STUFF((
                //                                        Select distinct ','+ so.Id
                //                                        from trn.ProductionOrder pos
                //                                        left join trn.ProductionOrderDetail pod on pod.ProductionOrderId = pos.Id
                //                                        left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                //                                        where pos.Id = base.ProductionId	
                //                                        FOR XML PATH('')
                //                                        ),1,1,'') as SOId 
                //                                        , STUFF((
                //                                        Select distinct ','+ b.UserName 
                //                                        from trn.ProductionOrder pos
                //                                        left join trn.ProductionOrderDetail pod on pod.ProductionOrderId = pos.Id
                //                                        left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                //                                        left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                //                                        left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                //                                        left join hkp.Buyer b on b.Id = mo.BuyerId
                //                                        where pos.Id = base.ProductionId
                //                                        FOR XML PATH('')
                //                                        ),1,1,'') as Buyer 
                //                                        , STUFF((
                //                                        Select distinct ','+ mo.BuyerReferenceNo 
                //                                        from trn.ProductionOrder pos
                //                                        left join trn.ProductionOrderDetail pod on pod.ProductionOrderId = pos.Id
                //                                        left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                //                                        left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                //                                        left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                //                                        where pos.Id = base.ProductionId
                //                                        FOR XML PATH('')
                //                                        ),1,1,'') as BuyRef 
                //                                        , STUFF((
                //                                        Select distinct ','+ mo.OwnReferenceNo 
                //                                        from trn.ProductionOrder pos
                //                                        left join trn.ProductionOrderDetail pod on pod.ProductionOrderId = pos.Id
                //                                        left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                //                                        left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                //                                        left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                //                                        where pos.Id = base.ProductionId
                //                                        FOR XML PATH('')
                //                                        ),1,1,'') as OwnRef 
                //            from
                //            (
                //            Select po.Id as ProductionId , po.EntityId , pps.ProcessId , p.UserName as Process, pps.Sequence 
                //            , ept.ProductionBookingLevel
                //                                        ,fc.CharacteristicsId , fc.CharacteristicsValueId , c.UserName as Chars , cv.UserName as CharV , Sum(fc.Qty) as OrderQty 
                //                                        , Sum(Ceiling( fc.Qty/(1-(mo.ExtraOrderPercentage+mo.OrderWastagePercentage)/100))) as PlanQty  
                //            from trn.ProductionOrder po
                //            left join trn.ProductionOrderProcessSet pps on pps.ProductionOrderId = po.Id 
                //            left join hkp.Process p on p.Id = pps.ProcessId
                //            left join hkp.EntityProcessTag ept on ept.EntityId = po.EntityId and ept.ProcessId = pps.ProcessId
                //            left join trn.ProductionOrderDetail pod on pod.ProductionOrderId = po.Id
                //            left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                //            left join trn.FirstCharacteristics fc on fc.SalesOrderId = so.Id
                //            left join hkp.Characteristics c on c.Id = fc.CharacteristicsId
                //            left join hkp.CharacteristicsValue cv on cv.Id = fc.CharacteristicsValueId
                //            left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                //            left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                //            where po.Id = '215'
                //            group by  po.Id , po.EntityId , pps.ProcessId , p.UserName, pps.Sequence 
                //            , ept.ProductionBookingLevel , c.UserName , cv.UserName,fc.CharacteristicsId , fc.CharacteristicsValueId 
                //            ) as base
                //            left join 
                //            (
                //            Select ps.ProcessId , ps.ProductionOrderId , psd.Characteristics1Id , psd.Characteristics1ValueId ,  c.UserName as Chars , cv.UserName as CharsV , Sum(psd.Qty) as Produced
                //            from trn.ProductionSummary ps
                //            left join trn.ProductionSummaryDetail psd on psd.ProductionSummaryId = ps.Id
                //            left join hkp.Characteristics c on c.Id = psd.Characteristics1Id
                //            left join hkp.CharacteristicsValue cv on cv.Id = psd.Characteristics1ValueId
                //            where ps.ProductionOrderId = '215' and psd.Characteristics1Id is not null
                //            group by ps.ProcessId , ps.ProductionOrderId , psd.Characteristics1Id , psd.Characteristics1ValueId ,  c.UserName , cv.UserName 
                //            ) 
                //            as prod on prod.ProductionOrderId = base.ProductionId and prod.ProcessId = base.ProcessId and prod.Characteristics1Id = base.CharacteristicsId and prod.Characteristics1ValueId = base.CharacteristicsValueId
                //            order by base.CharV,base.Sequence";
                #endregion Query


                var str = @"Select  po.Id as ProductionId , po.EntityId , pps.ProcessId , p.UserName as Process, pps.Sequence , ept.ProductionBookingLevel
                            , c.UserName as Chars , cv.UserName as CharV , fc.Qty as OrderQty 
                            , Ceiling( fc.Qty/(1-(mo.ExtraOrderPercentage+mo.OrderWastagePercentage)/100)) as PlanQty  , isnull(prod.Produced,0) as ProducedQty  
                            , STUFF((
                            Select distinct ','+ so.Id
                            from trn.ProductionOrder pos
                            left join trn.ProductionOrderDetail pod on pod.ProductionOrderId = pos.Id
                            left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                            where pos.Id = po.Id
                            FOR XML PATH('')
                            ),1,1,'') as SOId 
                            , STUFF((
                            Select distinct ','+ b.UserName 
                            from trn.ProductionOrder pos
                            left join trn.ProductionOrderDetail pod on pod.ProductionOrderId = pos.Id
                            left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                            left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                            left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                            left join hkp.Buyer b on b.Id = mo.BuyerId
                            where pos.Id = po.Id
                            FOR XML PATH('')
                            ),1,1,'') as Buyer 
                            , STUFF((
                            Select distinct ','+ mo.BuyerReferenceNo 
                            from trn.ProductionOrder pos
                            left join trn.ProductionOrderDetail pod on pod.ProductionOrderId = pos.Id
                            left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                            left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                            left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                            where pos.Id = po.Id
                            FOR XML PATH('')
                            ),1,1,'') as BuyRef 
                            , STUFF((
                            Select distinct ','+ mo.OwnReferenceNo 
                            from trn.ProductionOrder pos
                            left join trn.ProductionOrderDetail pod on pod.ProductionOrderId = pos.Id
                            left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                            left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                            left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                            where pos.Id = po.Id
                            FOR XML PATH('')
                            ),1,1,'') as OwnRef 
                            from trn.ProductionOrder po
                            left join trn.ProductionOrderProcessSet pps on pps.ProductionOrderId = po.Id 
                            left join hkp.Process p on p.Id = pps.ProcessId
                            left join hkp.EntityProcessTag ept on ept.EntityId = po.EntityId and ept.ProcessId = pps.ProcessId
                            left join trn.ProductionOrderDetail pod on pod.ProductionOrderId = po.Id
                            left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                            left join trn.FirstCharacteristics fc on fc.SalesOrderId = so.Id
                            left join hkp.Characteristics c on c.Id = fc.CharacteristicsId
                            left join hkp.CharacteristicsValue cv on cv.Id = fc.CharacteristicsValueId
                            left join 
                            (
                            Select ps.ProcessId , ps.ProductionOrderId , psd.Characteristics1Id , psd.Characteristics1ValueId ,  c.UserName as Chars , cv.UserName as CharsV , Sum(psd.Qty) as Produced
                            from trn.ProductionSummary ps
                            left join trn.ProductionSummaryDetail psd on psd.ProductionSummaryId = ps.Id
                            left join hkp.Characteristics c on c.Id = psd.Characteristics1Id
                            left join hkp.CharacteristicsValue cv on cv.Id = psd.Characteristics1ValueId
                            where ps.ProductionOrderId = '" + PO + @"' and psd.Characteristics1Id is not null
                            group by ps.ProcessId , ps.ProductionOrderId , psd.Characteristics1Id , psd.Characteristics1ValueId ,  c.UserName , cv.UserName 
                            ) 
                            as prod on prod.ProductionOrderId = po.Id and prod.ProcessId = pps.ProcessId and prod.Characteristics1Id = fc.CharacteristicsId and prod.Characteristics1ValueId = fc.CharacteristicsValueId
                            left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                            left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                            where po.id ='" + PO + @"' 
                            order by cv.Id,pps.Sequence";

                var proclist = @"Select pps.ProductionOrderId,pps.ProcessId , pps.Sequence , p.UserName from trn.ProductionOrderProcessSet pps 
                                    left join hkp.Process p on p.Id = pps.ProcessId
                                    where ProductionOrderId = '" + PO + @"' order by Sequence";

                DataTable dtFirst = _sqlRepository.GetDataTable(str);
                DataTable dtProcess = _sqlRepository.GetDataTable(proclist);

                List<string> Cols = new List<string>();

                DataTable ff = new DataTable();
                ff.Columns.Add("PO", typeof(string));
                ff.Columns.Add("SO", typeof(string));
                ff.Columns.Add("Buyer", typeof(string));
                ff.Columns.Add("BuyerRef", typeof(string));
                ff.Columns.Add("OwnRef", typeof(string));
                ff.Columns.Add("SKU1", typeof(string));
                ff.Columns.Add("OrderQty", typeof(double));
                ff.Columns.Add("PlanQty", typeof(double));

                for (int i = 0; i < dtProcess.Rows.Count; i++)
                {
                    string s = dtProcess.Rows[i]["UserName"].ToString() + "Qty";
                    Cols.Add(s);
                    ff.Columns.Add(s, typeof(string));
                }

                for (int i = 1; i < dtProcess.Rows.Count; i++)
                {
                    string s = dtProcess.Rows[i]["UserName"].ToString() + "WIP";
                    Cols.Add(s);
                    ff.Columns.Add(s, typeof(string));
                }


                DataRow dr = ff.NewRow();
                if (dtFirst.Rows.Count > 0)
                {
                    dr["PO"] = dtFirst.Rows[0]["ProductionId"].ToString();
                    dr["SO"] = dtFirst.Rows[0]["SOId"].ToString();
                    dr["Buyer"] = dtFirst.Rows[0]["Buyer"].ToString();
                    dr["BuyerRef"] = dtFirst.Rows[0]["BuyRef"].ToString();
                    dr["OwnRef"] = dtFirst.Rows[0]["OwnRef"].ToString();
                    dr["SKU1"] = dtFirst.Rows[0]["CharV"].ToString();
                    dr["OrderQty"] = clsStaticInfo.dbl(dtFirst.Rows[0]["OrderQty"].ToString());
                    dr["PlanQty"] = clsStaticInfo.dbl(dtFirst.Rows[0]["PlanQty"].ToString());
                }

                if (dtFirst.Rows.Count > 0)
                {
                    for (int i = 0; i < dtFirst.Rows.Count; i++)
                    {
                        if (dr["SKU1"].ToString() != dtFirst.Rows[i]["CharV"].ToString())
                        {
                            ff.Rows.Add(dr);
                            dr = ff.NewRow();
                            dr["SKU1"] = dtFirst.Rows[i]["CharV"].ToString();
                            dr["OrderQty"] = clsStaticInfo.dbl(dtFirst.Rows[0]["OrderQty"].ToString());
                            dr["PlanQty"] = clsStaticInfo.dbl(dtFirst.Rows[0]["PlanQty"].ToString());
                        }
                        //else
                        //{
                        string pQty = dtFirst.Rows[i]["Process"].ToString() + "Qty";
                        string pWip = "";
                        if (clsStaticInfo.dbl(dtFirst.Rows[i]["Sequence"].ToString()) != 1)
                        {
                            pWip = dtFirst.Rows[i]["Process"].ToString() + "WIP";
                        }

                        dr[pQty] = clsStaticInfo.dbl(dtFirst.Rows[i]["ProducedQty"].ToString());
                        if (pWip != "")
                        {
                            dr[pWip] = clsStaticInfo.dbl(dtFirst.Rows[i - 1]["ProducedQty"].ToString()) - clsStaticInfo.dbl(dtFirst.Rows[i]["ProducedQty"].ToString());
                        }

                        //}

                    }

                    ff.Rows.Add(dr);
                }


                DynCols = Cols;

                return ff;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion MasterGrid

        #endregion POWise
    }
}
