using System;
using System.Collections.Generic;
using Library.Data.Sql;
using System.Data;
using OTSBD;
using Library.Crosscutting.Security;
using System.Threading;


namespace Library.OrderManagement.Production
{
    public class StocksAgeingReportService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
        public StocksAgeingReportService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        public IEnumerable<object> getData()
        {
            try
            {
                var str = @"Select ProductCategory,ProductSubCategory,ProductCode, PordDertails as ProdDetails ,POId,LotNo,Material , Article, Customers , CustomerType ,
                            Case when D15>0 then D15 else null end as D15
                            , Case when D15T30>0 then D15T30 else null end as D15T30
                            , Case when D30T60>0 then D30T60 else null end as D30T60
                            , Case when D60T90>0 then D60T90 else null end as D60T90
                            , Case when D90T120>0 then D90T120 else null end as D90T120
                            , Case when D120T150>0 then D120T150 else null end as D120T150
                            , Case when D150T180>0 then D150T180 else null end as D150T180
                            , Case when D180T360>0 then D180T360 else null end as D180T360
                            , Case when DG360>0 then DG360 else null end as DG360
                            from
                            (Select tt.ProductCategory, tt.ProductSubCategory,tt.ProductCode , tt.PordDertails, tt.POId , tt.LotNo , tt.Material , tt.Article , tt.Customers,tt.CustomerType  , sum(case when Interval<15 then NetWeight else 0 end) as D15
                            ,sum(case when Interval>=15 and Interval<30 then NetWeight else 0 end) as D15T30
                            ,sum(case when Interval>=30 and Interval<60 then NetWeight else 0 end) as D30T60
                            ,sum(case when Interval>=60 and Interval<90 then NetWeight else 0 end) as D60T90
                            ,sum(case when Interval>=90 and Interval<120 then NetWeight else 0 end) as D90T120
                            ,sum(case when Interval>=120 and Interval<150 then NetWeight else 0 end) as D120T150
                            ,sum(case when Interval>=150 and Interval<180 then NetWeight else 0 end) as D150T180
                            ,sum(case when Interval>=180 and Interval<360 then NetWeight else 0 end) as D180T360
                            ,sum(case when Interval>=360 then NetWeight else 0 end) as DG360
                                from
                                (select distinct pcc.UserName as ProductCategory , pscc.UserName as ProductSubCategory,
                                S.ProductCode, S.POId, S.LotNo,
                                S.RefNo, S.Cones, S.NetWeight, S.GWeight, S.PackedBy,
                                S.Shade, S.AddedBy, S.AddedDate, ma.UserName as Material , M.StandardName as Article, R.FromLocation, R.ToLocation , cus.ProductLibraryId ,(Select Stuff((
                                Select ' / ' + pla.ShortName + ' - ' + pla.AttributeValue
                                from dbo.ProductLibraryAttribute pla
                                where pla.ProductLibraryId = cus.ProductLibraryId
                                for XML PATH('')
                                ) , 1, 2, '')) as PordDertails , format(sc.WorkDate,'dd-MMM-yyyy') as WorkDate
                                , DATEDIFF(DAY, sc.WorkDate, GETDATE()) as Interval
                                , STUFF((
                                    Select distinct ','+ cuss.UserName
                                   from trn.MasterOrder mos
                                        left join trn.MasterOrderItem mois on mois.MasterOrderId = mos.Id
                                        left join trn.SalesOrder sos on sos.MasterOrderItemId = mois.id
                                        left join trn.ProductionOrderDetail pods on pods.SalesOrderId = sos.Id
                                        left join trn.ProductionOrder pos on pos.Id = pods.ProductionOrderId
                                        left join hkp.party cuss on cuss.Id = mos.PartyId
                                        where pos.Id = S.POId  
                                        FOR XML PATH('')
                                ),1,1,'') as Customers
                                , STUFF((
                                    Select distinct ','+ PAG.StandardName
                                    from trn.MasterOrder mos
                                        left join trn.MasterOrderItem mois on mois.MasterOrderId = mos.Id
                                        left join trn.SalesOrder sos on sos.MasterOrderItemId = mois.id
                                        left join trn.ProductionOrderDetail pods on pods.SalesOrderId = sos.Id
                                        left join trn.ProductionOrder pos on pos.Id = pods.ProductionOrderId
                                        left join hkp.party cuss on cuss.Id = mos.PartyId
										left join hkp.CompanyParty CP ON CP.PartyId=cuss.Id and CP.PartyType='Customer'
										left join hkp.PartyAccountGroup PAG ON PAG.Id=cp.PartyAccountGroupId
                                        where pos.Id = S.POId 
                                        FOR XML PATH('')
                                ),1,1,'') as CustomerType
                                from ItemScanChild S
                                LEFT JOIN MST.MaterialMovementMaster R ON R.ID = S.LocMasterId
								LEFT JOIN HKP.MaterialMovementPurpose MMP ON MMP.Id = R.PurposeId
                                LEFT JOIN ProductLibrary P ON P.Code = S.ProductCode
                                LEFT JOIN MST.MaterialMasterArticle M ON M.Id = P.ArticleId
                                left join mst.MaterialMaster ma on ma.Id = M.MaterialMasterId
                                left join  dbo.ItemScan sc on sc.Id = S.MasterId
                                left join trn.ProductDefinition pd on pd.MaterialMasterId = ma.Id
                                left join mst.ProductMaster pm on pm.Id = pd.ProductMasterId
                                left join hkp.ProductCategory pcc on pcc.Id = pm.ProductCategoryId
                                left join hkp.ProductSubCategory pscc on pscc.Id = pm.ProductSubCategoryId
                                left join
                                (
                                Select distinct mo.Id as MasterId , mo.PartyId , po.Id as POIdd , moi.ProductLibraryId , pl.Code
                                from trn.MasterOrder mo
                                left join trn.MasterOrderItem moi on moi.MasterOrderId = mo.Id
                                left join trn.SalesOrder so on so.MasterOrderItemId = moi.id
                                left join trn.ProductionOrderDetail pod on pod.SalesOrderId = so.Id
                                left join trn.ProductionOrder po on po.Id = pod.ProductionOrderId
                                left join ProductLibrary pl on pl.Id = moi.ProductLibraryId
                                where moi.ProductLibraryId is not null 
                                )
                                as cus on cus.Code = S.ProductCode and cus.POIdd = S.POId
                                WHERE s.booked = 'False' AND ISNULL(CAST(MMP.IsInventoryOut as int),0) <> 1
                                --AND R.ToLocation NOT IN ('JOB WORK LOCATION','DyeHouse','PACKING','JW Sale-Dye')
                                group by S.ProductCode, S.POId, S.LotNo,
                                S.RefNo, S.Cones, S.NetWeight, S.GWeight, S.PackedBy,
                                S.Shade, S.AddedBy, S.AddedDate, M.StandardName, R.FromLocation, R.ToLocation , cus.ProductLibraryId, sc.WorkDate , ma.UserName
                                , pcc.UserName, pscc.UserName 
                                )  as TT
                                group by tt.ProductCode ,tt.PordDertails, tt.POId , tt.LotNo , tt.Material , tt.Article , tt.Customers,tt.ProductCategory, tt.ProductSubCategory, tt.CustomerType) as dd";
                return _sqlRepository.GetDataCollection(str);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
         
    }

    public class FGInventoryStockReportService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
        public FGInventoryStockReportService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        public DataTable getStocksReport(string ToDate, string FromDate)
        {
            try
            {
                //var str = @"Select ProductCategory, ProductSubCategory , Material , Article , ProductCode , POId , LotNo , Opening , Produce , Retrn , Dispatch , Issue , 
                //            ( Opening + Produce - Retrn - Dispatch - Issue ) as Closing
                //            from
                //            (
                //            Select pcc.UserName as ProductCategory , pscc.UserName as ProductSubCategory,ma.UserName as Material , M.StandardName as Article,S.ProductCode, S.POId, S.LotNo
                //            , Sum(Case When purp.UserName = 'PACKING' or purp.UserName= 'RE-PACK' then S.NetWeight else 0 end) as Produce
                //            , Sum(Case When purp.UserName = 'ISSUE' then S.NetWeight else 0 end) as Issue
                //            , Sum(Case When S.IsDespatch = 1 then S.NetWeight else 0 end) as Dispatch
                //            , Sum(Case When purp.UserName = 'RETURN' then S.NetWeight else 0 end) as Retrn
                //            , isnull(opens.Opening , 0) as Opening
                //            from ItemScanChild S
                //            LEFT JOIN MST.MaterialMovementMaster R ON R.ID = S.LocMasterId
                //            LEFT JOIN ProductLibrary P ON P.Code = S.ProductCode
                //            LEFT JOIN MST.MaterialMasterArticle M ON M.Id = P.ArticleId
                //            left join mst.MaterialMaster ma on ma.Id = M.MaterialMasterId
                //            left join  dbo.ItemScan sc on sc.Id = S.MasterId
                //            left join trn.ProductDefinition pd on pd.MaterialMasterId = ma.Id
                //            left join mst.ProductMaster pm on pm.Id = pd.ProductMasterId
                //            left join hkp.ProductCategory pcc on pcc.Id = pm.ProductCategoryId
                //            left join hkp.ProductSubCategory pscc on pscc.Id = pm.ProductSubCategoryId
                //            left join hkp.MaterialMovementPurpose purp on purp.Id = sc.PurposeId
                //            left join 
                //            (
                //            Select  dd.ProductCode , dd.POId,dd.LotNo , (dd.Produce - dd.Issue - dd.Retrn - dd.Dispatch) as Opening
                //            from
                //            (
                //            Select sc.ProductCode , sc.POId,sc.LotNo, Sum(Case When purp.UserName = 'PACKING' or purp.UserName= 'RE-PACK' then Sc.NetWeight else 0 end) as Produce
                //            , Sum(Case When purp.UserName = 'ISSUE' then Sc.NetWeight else 0 end) as Issue
                //            , Sum(Case When Sc.IsDespatch = 1 then Sc.NetWeight else 0 end) as Dispatch
                //            , Sum(Case When purp.UserName = 'RETURN' then Sc.NetWeight else 0 end) as Retrn
                //            from dbo.ItemScanChild sc
                //            left join ItemScan s on s.Id = sc.MasterId
                //            left join hkp.MaterialMovementPurpose purp on purp.Id = s.PurposeId
                //            where s.WorkDate < '"+FromDate+@"'
                //            group by  sc.ProductCode , sc.POId,sc.LotNo

                //            ) as dd
                //            ) as opens on opens.ProductCode = S.ProductCode and opens.POId = S.POId and opens.LotNo = S.LotNo
                //            where sc.WorkDate between '"+FromDate+@"' and '"+ToDate+@"' 
                //            --and  R.ToLocation<> 'JOB WORK LOCATION' AND R.ToLocation<> 'DyeHouse' AND R.ToLocation<> 'PACKING'
                //            group by pcc.UserName , pscc.UserName , ma.UserName , M.StandardName , S.ProductCode , S.POId, S.LotNo , opens.Opening
                //            ) as
                //            final";

                var str = @"Select ProductCategory,ProductSubCategory,ProductCode, POId,LotNo,Material , Article, Customers , Case when D15>0 then D15 else null end as D15
                            , Case when D15T30>0 then D15T30 else null end as D15T30
                            , Case when D30T60>0 then D30T60 else null end as D30T60
                            , Case when D60T90>0 then D60T90 else null end as D60T90
                            , Case when D90T120>0 then D90T120 else null end as D90T120
                            , Case when D120T150>0 then D120T150 else null end as D120T150
                            , Case when D150T180>0 then D150T180 else null end as D150T180
                            , Case when D180T360>0 then D180T360 else null end as D180T360
                            , Case when DG360>0 then DG360 else null end as DG360
                            from
                            (Select tt.ProductCategory, tt.ProductSubCategory,tt.ProductCode , tt.POId , tt.LotNo , tt.Material , tt.Article , tt.Customers  , sum(case when Interval<15 then NetWeight else 0 end) as D15
                            ,sum(case when Interval>=15 and Interval<30 then NetWeight else 0 end) as D15T30
                            ,sum(case when Interval>=30 and Interval<60 then NetWeight else 0 end) as D30T60
                            ,sum(case when Interval>=60 and Interval<90 then NetWeight else 0 end) as D60T90
                            ,sum(case when Interval>=90 and Interval<120 then NetWeight else 0 end) as D90T120
                            ,sum(case when Interval>=120 and Interval<150 then NetWeight else 0 end) as D120T150
                            ,sum(case when Interval>=150 and Interval<180 then NetWeight else 0 end) as D150T180
                            ,sum(case when Interval>=180 and Interval<360 then NetWeight else 0 end) as D180T360
                            ,sum(case when Interval>=360 then NetWeight else 0 end) as DG360
                                from

                                (select distinct pcc.UserName as ProductCategory , pscc.UserName as ProductSubCategory,
								S.ProductCode, S.POId, S.LotNo,
                                S.RefNo, S.Cones, S.NetWeight, S.GWeight, S.PackedBy,
                                S.Shade, S.AddedBy, S.AddedDate, ma.UserName as Material , M.StandardName as Article, R.FromLocation, R.ToLocation , cus.ProductLibraryId , format(sc.WorkDate,'dd-MMM-yyyy') as WorkDate
                                , DATEDIFF(DAY, sc.WorkDate, GETDATE()) as Interval
                                , STUFF((
                                    Select distinct ','+ cuss.UserName
                                    from trn.MasterOrder mos

                                        left join trn.MasterOrderItem mois on mois.MasterOrderId = mos.Id

                                        left join trn.SalesOrder sos on sos.MasterOrderItemId = mois.id

                                        left join trn.ProductionOrderDetail pods on pods.SalesOrderId = sos.Id

                                        left join trn.ProductionOrder pos on pos.Id = pods.ProductionOrderId

                                        left join ProductLibrary pls on pls.Id = mois.ProductLibraryId

                                        left join hkp.party cuss on cuss.Id = mos.PartyId

                                        where pos.Id = S.POId and mois.ProductLibraryId = cus.ProductLibraryId and pls.Code = S.ProductCode --and cuss.Id in ('202017389')

                                        FOR XML PATH('')
                                ),1,1,'') as Customers
                                from ItemScanChild S
                                LEFT JOIN MST.MaterialMovementMaster R ON R.ID = S.LocMasterId
                                LEFT JOIN ProductLibrary P ON P.Code = S.ProductCode
                                LEFT JOIN MST.MaterialMasterArticle M ON M.Id = P.ArticleId
                                left join mst.MaterialMaster ma on ma.Id = M.MaterialMasterId
                                left join  dbo.ItemScan sc on sc.Id = S.MasterId
								left join trn.ProductDefinition pd on pd.MaterialMasterId = ma.Id
								left join mst.ProductMaster pm on pm.Id = pd.ProductMasterId
								left join hkp.ProductCategory pcc on pcc.Id = pm.ProductCategoryId
								left join hkp.ProductSubCategory pscc on pscc.Id = pm.ProductSubCategoryId
                                left join
                                (
                                Select distinct mo.Id as MasterId , mo.PartyId , po.Id as POIdd , moi.ProductLibraryId , pl.Code
                                from trn.MasterOrder mo
                                left join trn.MasterOrderItem moi on moi.MasterOrderId = mo.Id
                                left join trn.SalesOrder so on so.MasterOrderItemId = moi.id
                                left join trn.ProductionOrderDetail pod on pod.SalesOrderId = so.Id
                                left join trn.ProductionOrder po on po.Id = pod.ProductionOrderId
                                left join ProductLibrary pl on pl.Id = moi.ProductLibraryId
                                where moi.ProductLibraryId is not null 
                                )
                                as cus on cus.Code = S.ProductCode and cus.POIdd = S.POId
                                WHERE s.booked = 'False' AND R.ToLocation<> 'JOB WORK LOCATION'
                                AND R.ToLocation<> 'DyeHouse' AND R.ToLocation<> 'PACKING' and R.ToLocation<> 'JW Sale-Dye' --and sc.WorkDate between FromDate ToDate
                                group by S.ProductCode, S.POId, S.LotNo,
                                S.RefNo, S.Cones, S.NetWeight, S.GWeight, S.PackedBy,
                                S.Shade, S.AddedBy, S.AddedDate, M.StandardName, R.FromLocation, R.ToLocation , cus.ProductLibraryId, sc.WorkDate , ma.UserName
								, pcc.UserName, pscc.UserName 
                                )  as TT
                                group by tt.ProductCode , tt.POId , tt.LotNo , tt.Material , tt.Article , tt.Customers,tt.ProductCategory , tt.ProductSubCategory) as dd
								order by POId DESC
                ";
                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }
    }

    public class StocksAdjustmentService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
          
        public StocksAdjustmentService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        public IEnumerable<object> getCurrentList()
        {
            try
            {
                var str = @"Select * , format(WorkDate , 'dd-MMM-yyyy') as DDate from dbo.StocksAdjustment";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetSampAdjReport()
        {
            try
            {
                var str = @"Select ProductCode , LotNo , WorkDate , Qty from dbo.StocksAdjustment where 1 = 2";
                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveFileList(List<Dictionary<string, object>> data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
               
                string TableName = "dbo.StocksAdjustment";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where 1 = 2", out dsMaster, false, "1");

                string _Id = "";
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    int indexa = 0;
                    for (int i = 0; i < data.Count; i++)
                    {
                        Dictionary<string, object> jj = data[i];
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);
                        indexa++;
                        jj["Id"] = _Id;

                        AddNewRow(dsMaster.Tables[0], jj);
                    }


                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();

            dt.Rows.Add(dr);
        }
    }
}



