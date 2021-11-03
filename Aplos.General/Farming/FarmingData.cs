using System;
using System.Collections.Generic;
using System.Linq;
using Library.Data.Sql;
using System.Data;
using OTSBD;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using Library.Data.UnitOfWorks;

namespace Library.General.Farming
{
    public class FarmingModel
    {
        #region Scalar Properties
        public string Id { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? Time { get; set; }
        public DateTime? ValidationDate { get; set; }
        public string LocationId { get; set; }
        public string CropPlanningId { get; set; }
        public string CustomerId { get; set; }
        public string IsConfirmed { get; set; }
        public string IsApproved { get; set; }
        public string IsPayment { get; set; }
        public string VoucherId { get; set; }
        public string IsVoucher { get; set; }
        public DateTime? VoucherDate { get; set; }

        #endregion Scalar Properties 

        #region Audit Properties

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedFromIP { get; set; }

        public string ConfirmationBy { get; set; }
        public DateTime? ConfirmationDate { get; set; }
        public string ConfirmationIP { get; set; }

        public string ApprovalBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string ApprovalIP { get; set; }

        public string PaymentBy { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string PaymentIP { get; set; }



        #endregion Audit Properties

    }

    public class FarmingChildModel
    {
        #region Scalar Properties
        public string Id { get; set; }
        public string PurchaseBookingSodaMasterId { get; set; }
        public string CropPlanningChildId { get; set; }
        public string Quantity { get; set; }
        public string Rate { get; set; }
        public string Remarks { get; set; }
        public string TargetRate { get; set; }

        public string ConfirmationQuantity { get; set; }
        public string ConfirmationRate { get; set; }
        public string ApprovedQuantity { get; set; }
        public string ApprovedRate { get; set; }
        public string PaymentQuantity { get; set; }
        public string PaymentRate { get; set; }

        #endregion Scalar Properties 

        #region Audit Properties

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties

    }


    public class FarmingData
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public FarmingData()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        public IEnumerable<object> getLocations()
        {
            try
            {
                var str = @"Select distinct Id, UserName as Location 
                            from HKP.CropRateLocation";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getIcsMasterId()
        {
            try
            {
                var str = @"Select distinct Id , Name from MST.ICSMaster";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getFarmer(string IcsId)
        {
            try
            {
                var str = @"Select distinct FarmerName , MST.FarmerMaster.Id as FarmerId
                            from MST.ICSMaster
                            join MST.FarmerMasterPlot
                            on MST.ICSMaster.Id = MST.FarmerMasterPlot.ICSMasterId
                            join MST.FarmerMaster
                            on MST.FarmerMasterPlot.FarmerMasterId = MST.FarmerMaster.Id
                            where MST.ICSMaster.Id = '" + IcsId + @"'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getCropPlanning(string IcsId)
        {
            try
            {
                var str = @"Select distinct cp.Id as CropPlanningId, cp.UserName as CropPlanningName
                         from TRN.CropPlanning cp where cp.ICSMasterID = '" + IcsId + @"'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetFirstPageInfo()
        {
            try
            {
                var str = @"select distinct pbs.Id,pbs.AddedBy,pbs.CropPlanningId,pbs.ValidationDate,crl.UserName as Location,cp.UserName as CropPlanning,p.UserName as Customer,ics.Name as IcsMaster,ics.Id as ICSMasterID,fm.Id as FarmerId,fm.FarmerName,fm.FarmerFatherHusbandName,fm.Id as FarmerRegId,fm.FarmerRegistrationID as FarmerRegistration,DATEDIFF(day,pbs.Date,GETDATE()) as BookDays,kk.BookingStatus                                                          
                              from TRN.PurchaseBookingSoda pbs
                              left join HKP.CropRateLocation crl on crl.Id=pbs.LocationId
                             left join TRN.CropPlanning cp on cp.Id=pbs.CropPlanningId
                            left join HKP.Party p on p.Id=pbs.CustomerId
                             left join MST.ICSMaster ics on ics.Id=cp.ICSMasterID
                             left join TRN.CropPlanning on cp.Id=pbs.CropPlanningId
                             left join TRN.CropPlanningChild cpc on cpc.CropPlanningMasterId=pbs.CropPlanningId
                            left join MST.FarmerMaster fm on fm.Id=cpc.FarmerId
                           left join (select BookingStatus='Booked',PurchaseBookingSodaMasterId FROM TRN.PurchaseBookingSodaChild) kk on kk.PurchaseBookingSodaMasterId=pbs.Id
                              where pbs.IsConfirmed=0";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getTargetRate(string locationId, string cropId, string cropTypeId)
        {
            try
            {
                var str = @"Select TargetRate
                            from MST.DailyCroprate
                            where LocationId = '" + locationId + @"' AND CropId = '" + cropId + @"' AND CropTypeId = '" + cropTypeId + @"'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getCropDetails(string cropPlanningId)
        {
            try
            {
                var str = @"Select MST.CropMaster.UserName as CropName, MST.CropMaster.Id as CropId , HKP.CropType.Id as CropTypeId, HKP.CropType.UserName as CropTypeName,
                            TRN.CropPlanning.Id as CropPlanningId , TRN.CropPlanning.UserName as CropPlanningName
                            from MST.CropMaster
                            join TRN.CropPlanningChild 
                            on TRN.CropPlanningChild.CropId = MST.CropMaster.Id
                            join TRN.CropPlanning
                            on TRN.CropPlanning.Id = TRN.CropPlanningChild.CropPlanningMasterId
                            join HKP.CropType
                            on HKP.CropType.Id = TRN.CropPlanningChild.CropTypeId
                            where TRN.CropPlanning.Id = '" + cropPlanningId + @"'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getCropAndBalance(string cropId, string cropPlanningMasterId, string cropTypeId)
        {
            try
            {
                var str = @"Select TRN.CropPlanningChild.Id as CropPlanningChildId , TRN.CropPlanningChild.PlanQuantity as BalanceToBook
                            from TRN.CropPlanningChild
                            where CropId = '" + cropId + @"' AND CropPlanningMasterId = '" + cropPlanningMasterId + @"' AND CropTypeId = '" + cropTypeId + @"'";

                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public IEnumerable<object> getCustomers()
        {
            try
            {
                var str = @"Select distinct Id as [Value], Username as [Text]
                            from HKP.Party";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string Create(IEnumerable<FarmingModel> DataToSave)
        {

            try
            {
                DataSet dsMaster;
                string TableName = "TRN.PurchaseBookingSoda";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                if (DataToSave.Count() == 0)
                    return "";

                List<FarmingModel> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + items[0].Id + "'", out dsMaster, false, "1");

                string _Id = "";


                foreach (FarmingModel item in DataToSave)
                {
                    if (dsMaster.Tables[0].Rows.Count == 0 && items[0].Id == null)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);

                        dr["Id"] = "PBS" + _Id;
                        dr["LocationId"] = item.LocationId;
                        dr["Date"] = item.Date;
                        dr["Time"] = item.Time;
                        dr["CustomerId"] = item.CustomerId;
                        dr["CropPlanningId"] = item.CropPlanningId;
                        dr["ValidationDate"] = item.ValidationDate;
                        dr["CustomerId"] = item.CustomerId;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = item.AddedFromIP;
                        dr["IsApproved"] = item.IsApproved;
                        dr["IsConfirmed"] = item.IsConfirmed;
                        dr["IsPayment"] = item.IsPayment;
                        dr["IsVoucher"] = item.IsVoucher;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                string MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                return MasterId;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }


        public string CreateChild(IEnumerable<FarmingChildModel> DataToSave)
        {

            try
            {
                DataSet dsMaster;
                string TableName = "TRN.PurchaseBookingSodaChild";


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                if (DataToSave.Count() == 0)
                    return "";

                List<FarmingChildModel> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + items[0].Id + "'", out dsMaster, false, "1");

                string _Id = "";

                foreach (FarmingChildModel item in DataToSave)
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + item.Id + "'", out dsMaster, false, "1");

                    if (dsMaster.Tables[0].Rows.Count == 0 && items[0].Id == null)
                    {

                        DataRow dr = dsMaster.Tables[0].NewRow();

                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);

                        dr["Id"] = _Id;
                        dr["PurchaseBookingSodaMasterId"] = item.PurchaseBookingSodaMasterId;
                        dr["Quantity"] = item.Quantity;
                        dr["Rate"] = item.Rate;
                        dr["TargetRate"] = item.TargetRate;
                        dr["Remarks"] = item.Remarks;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = System.DateTime.Now;
                        dr["AddedFromIP"] = item.AddedFromIP;
                        dr["CropPlanningChildId"] = item.CropPlanningChildId;
                        dr["ConfirmationQuantity"] = item.ConfirmationQuantity;
                        dr["ConfirmationRate"] = item.ConfirmationRate;
                        dr["ApprovedQuantity"] = item.ApprovedQuantity;
                        dr["ApprovedRate"] = item.ApprovedRate;
                        dr["PaymentQuantity"] = item.PaymentQuantity;
                        dr["PaymentRate"] = item.PaymentRate;
                        dsMaster.Tables[0].Rows.Add(dr);


                    }
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                string MId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                return MId;

            }

            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


        public decimal getBalanceToPurchase(string cropPlanningChildId, string purchaseBookingSodaMasterId)
        {
            try
            {
                var str = @"Select PlanQuantity as TotalBalance from TRN.CropPlanningChild where Id = '" + cropPlanningChildId + @"'";

                var k = _sqlRepository.GetDataCollection(str);
                var j = Convert.ToDecimal(k[0].ElementAt(0).Value.ToString());

                var str1 = @"Select SUM(ConfirmationQuantity ) as Confirm  from TRN.PurchaseBookingSodaChild where CropPlanningChildId = '" + cropPlanningChildId + @"' AND PurchaseBookingSodaMasterId = '" + purchaseBookingSodaMasterId + @"'";
                var k1 = _sqlRepository.GetDataCollection(str1);
                var j1 = Convert.ToDecimal(k1[0].ElementAt(0).Value.ToString());
                var Diff = j - j1;
                return Diff;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getChildData(string cropPlanningId, string sodaBookingId)
        {
            try
            {
                var sql = @"select distinct cpc.Id as CPCId,kk.TotalQuantity,CQ.TotalConfirmedQuantity,cm.Id as CropId,cm.UserName as Crop,
                            ct.UserName as CropType,ct.Id as CropTypeId,
                            cpc.PlanQuantity,cpc.CropPlanningMasterId AS CropPlanningId,dcr.Id as DailyCropRateId,dcr.TargetRate as TargetRate,
                             BalanceBook = cpc.PlanQuantity - (isnull(kk.TotalQuantity, '0')),BalancePurchase = cpc.PlanQuantity - (isnull(CQ.TotalConfirmedQuantity, '0'))
                                                    from TRN.PurchaseBookingSodaChild pbsc
                                                    full
                                                    join TRN.CropPlanningChild cpc on cpc.Id = pbsc.CropPlanningChildId

                                               full join MST.CropMaster cm on cm.Id = cpc.CropId

                                               full  join HKP.CropType ct on ct.Id = cpc.CropTypeId
                                                inner
                                                  join MST.DailyCroprate dcr on dcr.CropId = cpc.CropId and
                                                    dcr.CropTypeId = cpc.CropTypeId
                                                    left join(
                                                    select SUM(quantity) as TotalQuantity,CropPlanningChildId
                                                    FROM TRN.PurchaseBookingSodaChild
                                                    group by CropPlanningChildId
                                                    ) kk on kk.CropPlanningChildId = cpc.id
                                                    left join(
                                                    select SUM(ConfirmationQuantity) as TotalConfirmedQuantity,CropPlanningChildId FROM
                                                    TRN.PurchaseBookingSodaChild group by CropPlanningChildId
                                                    ) CQ on CQ.CropPlanningChildId = cpc.id
                                                    where cpc.CropPlanningMasterId = '" + cropPlanningId + @"' or pbsc.PurchaseBookingSodaMasterId = '" + sodaBookingId + @"'";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        ///----------------------------------------------------------------------------------------------------------------------------------------------////

        ///// THE API's FOR THE FARMING DASHBOARD

        //The api for the Drop Down Models

        //Crop Type

        public IEnumerable<object> getCropType()
        {
            try
            {
                var str = @"Select Id , StandardName , UserName
                            from HKP.CropType 
                            ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Crop Category
        public IEnumerable<object> getCropCategory()
        {
            try
            {
                var str = @"Select Id , StandardName , UserName
                            from HKP.CropCategory 
                            ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Crop Sub Category
        public IEnumerable<object> getCropSubCategory()
        {
            try
            {
                var str = @"Select Id , StandardName , UserName
                            from HKP.CropSubCategory 
                            ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Land Category
        public IEnumerable<object> getLand()
        {
            try
            {
                var str = @"Select Id , StandardName , UserName
                            from HKP.LandCategory 
                            ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Crop
        public IEnumerable<object> getCrop()
        {
            try
            {
                var str = @"Select Id , StandardName , UserName
                            from MST.CropMaster 
                            ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //End of the Drop Down Model API

        /*public IEnumerable<object> getInitialData() //The Total Farmers and The Total ICS Numbers in a Group
        {
            try
            {
                var str = @"Select MST.ICSMaster.[Group] as ICSGroup, Count(MST.ICSMaster.Id) as NumberOfIcs,Count(Distinct MST.FarmerMasterPlot.FarmerMasterId) as TotalFarmers
                            from Mst.ICSMaster
                            left join MST.FarmerMasterPlot
                            on MST.ICSMaster.Id = MST.FarmerMasterPlot.ICSMasterId
                            group by MST.ICSMaster.[Group]";
                return _sqlRepository.GetDataCollection(str);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }*/

        public IEnumerable<object> getInitialData()
        {
            try
            {
                var str = @";With CTE_table
                            as
                            (Select distinct FarmerMasterId , ICSMasterId
                            from MST.FarmerMasterPlot 
                            ),
                            Planned as
                            (
                            Select distinct  CropPlanningMasterId, FarmerId ,   SUM(CropArea) as PlannedArea
                            from TRN.CropPlanningChild
                            group by CropPlanningMasterId , FarmerId
                            )
                            Select MST.ICSMaster.[Group] as ICSGroup, Count(distinct MST.ICSMaster.Id) as numberICS ,
							sum(case fm.Active when 1 then 1 else 0 end) as Active, 
							sum(case when fm.Active<>1 then 1 else 0 end) as Inactive,
                             COALESCE(SUM(fm.TotalArea),0) as TotalArea, COALESCE(SUM(pa.PlannedArea),0) as PlannedArea
                            from MST.ICSMaster
                            left join CTE_table as tb
                            on tb.ICSMasterId = MST.ICSMaster.Id
                            left join MST.FarmerMaster fm
                            on tb.FarmerMasterId = fm.Id 
                            left join Planned as pa
                            on pa.FarmerId = tb.FarmerMasterId
                            group by MST.ICSMaster.[Group]";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getDrillData(string icsGroup)
        {
            try
            {
                var str = @";With CTE_table
                            as
                            (Select distinct FarmerMasterId , ICSMasterId
                            from MST.FarmerMasterPlot 
                            ),
                            Planned as
                            (
                            Select distinct  CropPlanningMasterId, FarmerId ,   SUM(CropArea) as PlannedArea
                            from TRN.CropPlanningChild
                            group by CropPlanningMasterId , FarmerId
                            )
                            Select MST.ICSMaster.[Id] as ICSGroup,
							sum(case fm.Active when 1 then 1 else 0 end) as Active, 
							sum(case when fm.Active<>1 then 1 else 0 end) as Inactive,
                             COALESCE(SUM(fm.TotalArea),0) as TotalArea, COALESCE(SUM(pa.PlannedArea),0) as PlannedArea
                            from MST.ICSMaster
                            left join CTE_table as tb
                            on tb.ICSMasterId = MST.ICSMaster.Id
                            left join MST.FarmerMaster fm
                            on tb.FarmerMasterId = fm.Id 
                            left join Planned as pa
                            on pa.FarmerId = tb.FarmerMasterId
							where MST.ICSMaster.[Group] = '" + icsGroup + @"' 
                            group by MST.ICSMaster.[Id]";
                return _sqlRepository.GetDataCollection(str);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public IEnumerable<object> getFilterData(string landId, string cropId, string cropTypeId, string cropCategoryId, string cropSubCategoryId)
        {
            try
            {
                if (landId == null)
                {
                    landId = "NULL";
                }
                else
                {
                    landId = "'" + landId + "'";
                }

                if (cropId == null)
                {
                    cropId = "NULL";
                }
                else
                {
                    cropId = "'" + cropId + "'";
                }

                if (cropTypeId == null)
                {
                    cropTypeId = "NULL";
                }
                else
                {
                    cropTypeId = "'" + cropTypeId + "'";
                }
                if (cropCategoryId == null)
                {
                    cropCategoryId = "NULL";
                }
                else
                {
                    cropCategoryId = "'" + cropCategoryId + "'";
                }
                if (cropSubCategoryId == null)
                {
                    cropSubCategoryId = "NULL";
                }
                else
                {
                    cropSubCategoryId = "'" + cropSubCategoryId + "'";
                }


                var str = @"
                              DECLARE @landId char(20) = " + landId + @"
                                DECLARE @cropId char(20) = " + cropId + @"	
                                DECLARE @cropTypeId char(20) = " + cropTypeId + @"
                                DECLARE @cropCategoryId char(20) = " + cropCategoryId + @"
                                DECLARE @cropSubCategoryId char(20) = " + cropSubCategoryId + @"
                                ; With mainTable as 
                                (
	                                select ics.[Id] , fm.Active , SUM(cpc.CropArea) as PlannedArea , fm.TotalArea as TotalArea  
                                                                                 from MST.FarmerMaster fm left join dbo.EmployeeInformation EI on fm.ResponsiblePersonId=EI.SystemId
												                                 left join SCS.State s on fm.StateId=s.Id
												                                  left join SCS.Country c on c.Id=s.CountryId
												                                 left join SCS.District d on fm.DistrictId=d.Id
												                                 left join HKP.Taluk t on fm.TalukaId=t.Id
												                                 left join HKP.Village v on fm.VillageId=v.Id
												                                 left join SCS.UnitOfMeasurement uom on uom.Id=fm.UOMId
												                                 left join MST.FarmerMasterPlot fmp on fmp.FarmerMasterId=fm.Id
												                                 left join HKP.LandCategory lc on lc.Id=fmp.PlotStatus
												                                 left join MST.ICSMaster ics on ics.Id=fmp.ICSMasterId
												                                 left join TRN.CropPlanning cp on cp.ICSMasterID = fmp.ICSMasterId
												                                 left join TRN.CropPlanningChild cpc on cpc.CropPlanningMasterId=cp.Id and cpc.FarmerPlotId=fmp.Id
                                                                                 left join MST.CropMaster cm on cm.Id = cpc.CropId
												                                 left join HKP.CropCategory cc on cc.Id = cm.CropCategoryId
												                                 left join HKP.CropSubCategory csc on csc.Id = cm.CropSubCategoryId
												                                 WHERE lc.Id =IIF(@landId  IS NULL, lc.Id,@landId ) AND
												                                 cpc.CropId = IIF( @cropId IS NULL , cpc.CropId, @cropId) AND
												                                 cpc.CropTypeId = IIF(@cropTypeId IS NULL , cpc.CropTypeId,@cropTypeId) AND
												                                 cm.CropCategoryId = IIF(@cropCategoryId IS NULL , cm.CropCategoryId, @cropCategoryId) AND
												                                 cm.CropSubCategoryId = IIF(@cropSubCategoryId IS NULL , cm.CropSubCategoryId, @cropSubCategoryId)

												                                 group by ics.[Id] , cpc.CropArea , fm.TotalArea , fm.Active
 												                                 )
	                                Select distinct  icm.[Group] , sum(case mt.Active when 1 then 1 else 0 end) as Active, 
                                sum(case when mt.Active<>1 then 1 else 0 end) as Inactive , Coalesce(SUM(mt.TotalArea),0) as TotalArea , Coalesce(SUM(mt.PlannedArea),0) as PlannedArea
	                                from MST.ICSMaster icm
	                                left join mainTable mt
	                                on icm.[Id]= mt.[Id]
									group by icm.[Group]
	
                                ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public IEnumerable<object> getFilterDrillData(string landId, string cropId, string cropTypeId, string cropCategoryId, string cropSubCategoryId, string icsGroup)
        {
            try
            {

                if (landId == null)
                {
                    landId = "NULL";
                }
                else
                {
                    landId = "'" + landId + "'";
                }

                if (cropId == null)
                {
                    cropId = "NULL";
                }
                else
                {
                    cropId = "'" + cropId + "'";
                }

                if (cropTypeId == null)
                {
                    cropTypeId = "NULL";
                }
                else
                {
                    cropTypeId = "'" + cropTypeId + "'";
                }
                if (cropCategoryId == null)
                {
                    cropCategoryId = "NULL";
                }
                else
                {
                    cropCategoryId = "'" + cropCategoryId + "'";
                }
                if (cropSubCategoryId == null)
                {
                    cropSubCategoryId = "NULL";
                }
                else
                {
                    cropSubCategoryId = "'" + cropSubCategoryId + "'";
                }

                var str = @"			  DECLARE @landId char(20) = " + landId + @"
                                DECLARE @cropId char(20) = " + cropId + @"	
                                DECLARE @cropTypeId char(20) = " + cropTypeId + @"
                                DECLARE @cropCategoryId char(20) = " + cropCategoryId + @"
                                DECLARE @cropSubCategoryId char(20) = " + cropSubCategoryId + @"
                                ; With mainTable as 
                                (
	                                select ics.[Id] , fm.Active , SUM(cpc.CropArea) as PlannedArea , fm.TotalArea as TotalArea  
                                                                                 from MST.FarmerMaster fm left join dbo.EmployeeInformation EI on fm.ResponsiblePersonId=EI.SystemId
												                                 left join SCS.State s on fm.StateId=s.Id
												                                  left join SCS.Country c on c.Id=s.CountryId
												                                 left join SCS.District d on fm.DistrictId=d.Id
												                                 left join HKP.Taluk t on fm.TalukaId=t.Id
												                                 left join HKP.Village v on fm.VillageId=v.Id
												                                 left join SCS.UnitOfMeasurement uom on uom.Id=fm.UOMId
												                                 left join MST.FarmerMasterPlot fmp on fmp.FarmerMasterId=fm.Id
												                                 left join HKP.LandCategory lc on lc.Id=fmp.PlotStatus
												                                 left join MST.ICSMaster ics on ics.Id=fmp.ICSMasterId
												                                 left join TRN.CropPlanning cp on cp.ICSMasterID = fmp.ICSMasterId
												                                 left join TRN.CropPlanningChild cpc on cpc.CropPlanningMasterId=cp.Id and cpc.FarmerPlotId=fmp.Id
                                                                                 left join MST.CropMaster cm on cm.Id = cpc.CropId
												                                 left join HKP.CropCategory cc on cc.Id = cm.CropCategoryId
												                                 left join HKP.CropSubCategory csc on csc.Id = cm.CropSubCategoryId
												                                 WHERE lc.Id =IIF(@landId  IS NULL, lc.Id,@landId ) AND
												                                 cpc.CropId = IIF( @cropId IS NULL , cpc.CropId, @cropId) AND
												                                 cpc.CropTypeId = IIF(@cropTypeId IS NULL , cpc.CropTypeId,@cropTypeId) AND
												                                 cm.CropCategoryId = IIF(@cropCategoryId IS NULL , cm.CropCategoryId, @cropCategoryId) AND
												                                 cm.CropSubCategoryId = IIF(@cropSubCategoryId IS NULL , cm.CropSubCategoryId, @cropSubCategoryId)

												                                 group by ics.[Id] , cpc.CropArea , fm.TotalArea , fm.Active
 												                                 )
	                                Select distinct  icm.Id , sum(case mt.Active when 1 then 1 else 0 end) as Active, 
                                sum(case when mt.Active<>1 then 1 else 0 end) as Inactive , Coalesce(SUM(mt.TotalArea),0) as TotalArea , Coalesce(SUM(mt.PlannedArea),0) as PlannedArea
	                                from MST.ICSMaster icm
	                                left join mainTable mt
	                                on icm.[Id]= mt.[Id]
									 where icm.[Group] = '" + icsGroup + @"'
									group by icm.Id";
                return _sqlRepository.GetDataCollection(str);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /*
         //The Total Planned Area By ICS Groups
         public IEnumerable<object> getInitialPlannedArea()
         {
             try
             {
                 var str = @"Select  MST.ICSMaster.[Group] as IcsGroup , COALESCE(SUM(TRN.CropPlanningChild.CropArea),0) as PlannedArea 
                             from mst.ICSMaster
                             left join TRN.CropPlanning
                             on TRN.CropPlanning.ICSMasterID = mst.ICSMaster.Id
                             left join TRN.CropPlanningChild
                             on TRN.CropPlanning.Id = TRN.CropPlanningChild.CropPlanningMasterId
                             group by MST.ICSMaster.[Group] 
                             order by MST.ICSMaster.[Group] ASC";
                 return _sqlRepository.GetDataCollection(str);
             }
             catch( Exception e)
             {
                 throw e;
             }
         }


         //The Total Area By ICS Groups
         public IEnumerable<object> getInitialTotalArea()
         {
             try
             {
                 var str = @"Select  MST.ICSMaster.[Group] ,COALESCE(SUM(MST.FarmerMasterPlot.PlotArea),0) as TotalArea   
                             from MST.ICSMaster
                             left join MST.FarmerMasterPlot
                             on MST.FarmerMasterPlot.ICSMasterId = MST.ICSMaster.Id
                             group by MST.ICSMaster.[Group] 
                             order by MST.ICSMaster.[Group] ASC";
                 return _sqlRepository.GetDataCollection(str);
             }
             catch (Exception e)
             {
                 throw e;
             }
         }

         //For the Nos of Farmers in each ICS in a selected group
         public IEnumerable<object> getGroupFarmers(string icsGroup)
         {
             try
             {
                 var str = @"Select MST.ICSMaster.Id as icsId, Count(Distinct MST.FarmerMasterPlot.FarmerMasterId) as TotalFarmers
                             from MST.ICSMaster
                             left join MST.FarmerMasterPlot
                             on MST.FarmerMasterPlot.ICSMasterId = MST.ICSMaster.Id
                             where MST.ICSMaster.[Group] = '"+ icsGroup+@"'
                             group by MST.ICSMaster.Id
                             order by MST.ICSMaster.Id ASC";
                 return _sqlRepository.GetDataCollection(str);
             }
             catch (Exception ex)
             {
                 throw ex;
             }
         }

         //For the Planned Area in each ICS in a selected Group
         public IEnumerable<object> getGroupPlannedArea(string icsGroup)
         {
             try
             {
                 var str = @"Select MST.ICSMaster.Id , COALESCE(SUM(TRN.CropPlanningChild.CropArea) , 0) as PlannedArea
                             from MST.ICSMaster
                             left join TRN.CropPlanning
                             on TRN.CropPlanning.ICSMasterID = MST.ICSMaster.Id
                             left join TRN.CropPlanningChild
                             on TRN.CropPlanningChild.CropPlanningMasterId = TRN.CropPlanning.Id
                             where MST.ICSMaster.[Group] = '"+icsGroup+@"'
                             group by MST.ICSMaster.Id
                             order by MST.ICSMaster.Id ASC";
                 return _sqlRepository.GetDataCollection(str);
             }
             catch( Exception ex)
             {
                 throw ex;
             }
         }

         //For the Total Area in each ICS in a selected Group
         public IEnumerable<object> getGroupTotalArea(string icsGroup)
         {
             try
             {
                 var str = @"Select MST.ICSMaster.Id , COALESCE(SUM(MST.FarmerMasterPlot.PlotArea),0) as TotalArea
                             from MST.ICSMaster
                             left join MST.FarmerMasterPlot
                             on MST.FarmerMasterPlot.ICSMasterId = MST.ICSMaster.Id
                             where MST.ICSMaster.[Group] = '"+icsGroup+@"'
                             group by MST.ICSMaster.Id
                             order by MST.ICSMaster.Id ASC";
                 return _sqlRepository.GetDataCollection(str);
             }
             catch (Exception ex)
             {
                 throw ex;
             }
         }*/

        //For the Farmer Modal getting Farmers Group Wise
        /* public IEnumerable<object> getFarmersGroupWise(string icsGroup)
         {
             try
             {
                 var str = @"Select Distinct fm.Id as FarmerId , fm.FarmerName as FarmerName , fm.FarmerRegistrationID as RegistrationId, fm.FarmerRegistrationDate as RegistrationDate
                             from MST.ICSMaster ic
                             join MST.FarmerMasterPlot fmp
                             on fmp.ICSMasterId = ic.Id
                             left join MST.FarmerMaster fm
                             on fm.Id = fmp.FarmerMasterId
                             where ic.[Group] = '" + icsGroup +@"'";
                 return _sqlRepository.GetDataCollection(str);
             }
             catch (Exception ex)
             {
                 throw ex;
             }
         }

         //For the Farmer Modal getting Farmers ICS Wise
         public IEnumerable<object> getFarmersIcsWise(string ics)
         {
             try
             {
                 var str = @"Select Distinct fm.Id as FarmerId, fm.FarmerName as FarmerName, fm.FarmerRegistrationID as RegistrationId, fm.FarmerRegistrationDate as RegistrationDate
                             from MST.FarmerMasterPlot fmp
                             left join MST.FarmerMaster fm
                             on fm.Id = fmp.FarmerMasterId
                             where fmp.ICSMasterId = '"+ics+@"'";
                 return _sqlRepository.GetDataCollection(str);
             }
             catch(Exception ex)
             {
                 throw ex;
             }
         }*/

        public IEnumerable<object> getActiveFarmers(string landId, string cropId, string cropTypeId, string cropCategoryId, string cropSubCategoryId, string column, string groups)
        {
            try
            {

                if (landId == null)
                {
                    landId = "NULL";
                }
                else
                {
                    landId = "'" + landId + "'";
                }

                if (cropId == null)
                {
                    cropId = "NULL";
                }
                else
                {
                    cropId = "'" + cropId + "'";
                }

                if (cropTypeId == null)
                {
                    cropTypeId = "NULL";
                }
                else
                {
                    cropTypeId = "'" + cropTypeId + "'";
                }
                if (cropCategoryId == null)
                {
                    cropCategoryId = "NULL";
                }
                else
                {
                    cropCategoryId = "'" + cropCategoryId + "'";
                }
                if (cropSubCategoryId == null)
                {
                    cropSubCategoryId = "NULL";
                }
                else
                {
                    cropSubCategoryId = "'" + cropSubCategoryId + "'";
                }
                var str = @"DECLARE @landId char(20) = " + landId + @"
                                DECLARE @cropId char(20) = " + cropId + @"	
                                DECLARE @cropTypeId char(20) = " + cropTypeId + @"
                                DECLARE @cropCategoryId char(20) = " + cropCategoryId + @"
                                DECLARE @cropSubCategoryId char(20) = " + cropSubCategoryId + @"
                            select distinct Row_Number() Over (order by fm.Id) as Snum ,fm.Id as FarmerId, fm.FarmerRegistrationID as TracenetId, fm.FarmerName as FarmerName, 
                                                 fm.FarmerFatherHusbandName as FarmerFatherName, fm.TotalArea as TotalArea, Count(fmp.FarmerMasterId) as TotalPlots , fm.FarmerRegistrationDate as RegistrationDate
                                                 from MST.FarmerMaster fm left join dbo.EmployeeInformation EI on fm.ResponsiblePersonId=EI.SystemId
												 left join SCS.State s on fm.StateId=s.Id
												 left join SCS.Country c on c.Id=s.CountryId
												 left join SCS.District d on fm.DistrictId=d.Id
												 left join HKP.Taluk t on fm.TalukaId=t.Id
												 left join HKP.Village v on fm.VillageId=v.Id
												 left join SCS.UnitOfMeasurement uom on uom.Id=fm.UOMId
												 left join MST.FarmerMasterPlot fmp on fmp.FarmerMasterId=fm.Id
												 left join HKP.LandCategory lc on lc.Id=fmp.PlotStatus
												 left join MST.ICSMaster ics on ics.Id=fmp.ICSMasterId
												 left join TRN.CropPlanning cp on cp.ICSMasterID = fmp.ICSMasterId
												 left join TRN.CropPlanningChild cpc on cpc.CropPlanningMasterId=cp.Id and cpc.FarmerPlotId=fmp.Id
												 left join MST.CropMaster cm on cm.Id = cpc.CropId
												 WHERE  ics.[" + column + @"] = '" + groups + @"' AND fm.Active = '1' AND
												 lc.Id =IIF(@landId  IS NULL, lc.Id,@landId ) AND
												 cpc.CropId = IIF( @cropId IS NULL , cpc.CropId, @cropId) AND
												 cpc.CropTypeId = IIF(@cropTypeId IS NULL , cpc.CropTypeId,@cropTypeId) AND
												 cm.CropCategoryId = IIF(@cropCategoryId IS NULL , cm.CropCategoryId, @cropCategoryId) AND
												 cm.CropSubCategoryId = IIF(@cropSubCategoryId IS NULL , cm.CropSubCategoryId, @cropSubCategoryId)
												 group by fm.Id , fm.FarmerRegistrationID , fm.FarmerName , fm.FarmerFatherHusbandName , fm.TotalArea , fm.FarmerRegistrationDate";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getInactiveFarmers(string landId, string cropId, string cropTypeId, string cropCategoryId, string cropSubCategoryId, string column, string groups)
        {
            try
            {

                if (landId == null)
                {
                    landId = "NULL";
                }
                else
                {
                    landId = "'" + landId + "'";
                }

                if (cropId == null)
                {
                    cropId = "NULL";
                }
                else
                {
                    cropId = "'" + cropId + "'";
                }

                if (cropTypeId == null)
                {
                    cropTypeId = "NULL";
                }
                else
                {
                    cropTypeId = "'" + cropTypeId + "'";
                }
                if (cropCategoryId == null)
                {
                    cropCategoryId = "NULL";
                }
                else
                {
                    cropCategoryId = "'" + cropCategoryId + "'";
                }
                if (cropSubCategoryId == null)
                {
                    cropSubCategoryId = "NULL";
                }
                else
                {
                    cropSubCategoryId = "'" + cropSubCategoryId + "'";
                }
                var str = @"DECLARE @landId char(20) = " + landId + @"
                                DECLARE @cropId char(20) = " + cropId + @"	
                                DECLARE @cropTypeId char(20) = " + cropTypeId + @"
                                DECLARE @cropCategoryId char(20) = " + cropCategoryId + @"
                                DECLARE @cropSubCategoryId char(20) = " + cropSubCategoryId + @"
                            select distinct Row_Number() Over (order by fm.Id) as Snum ,fm.Id as FarmerId, fm.FarmerRegistrationID as TracenetId, fm.FarmerName as FarmerName, 
                                                 fm.FarmerFatherHusbandName as FarmerFatherName, fm.TotalArea as TotalArea, Count(fmp.FarmerMasterId) as TotalPlots , fm.FarmerRegistrationDate as RegistrationDate
                                                 from MST.FarmerMaster fm left join dbo.EmployeeInformation EI on fm.ResponsiblePersonId=EI.SystemId
												 left join SCS.State s on fm.StateId=s.Id
												 left join SCS.Country c on c.Id=s.CountryId
												 left join SCS.District d on fm.DistrictId=d.Id
												 left join HKP.Taluk t on fm.TalukaId=t.Id
												 left join HKP.Village v on fm.VillageId=v.Id
												 left join SCS.UnitOfMeasurement uom on uom.Id=fm.UOMId
												 left join MST.FarmerMasterPlot fmp on fmp.FarmerMasterId=fm.Id
												 left join HKP.LandCategory lc on lc.Id=fmp.PlotStatus
												 left join MST.ICSMaster ics on ics.Id=fmp.ICSMasterId
												 left join TRN.CropPlanning cp on cp.ICSMasterID = fmp.ICSMasterId
												 left join TRN.CropPlanningChild cpc on cpc.CropPlanningMasterId=cp.Id and cpc.FarmerPlotId=fmp.Id
												 left join MST.CropMaster cm on cm.Id = cpc.CropId
												 WHERE  ics.[" + column + @"] = '" + groups + @"' AND fm.Active = '0' AND
												 lc.Id =IIF(@landId  IS NULL, lc.Id,@landId ) AND
												 cpc.CropId = IIF( @cropId IS NULL , cpc.CropId, @cropId) AND
												 cpc.CropTypeId = IIF(@cropTypeId IS NULL , cpc.CropTypeId,@cropTypeId) AND
												 cm.CropCategoryId = IIF(@cropCategoryId IS NULL , cm.CropCategoryId, @cropCategoryId) AND
												 cm.CropSubCategoryId = IIF(@cropSubCategoryId IS NULL , cm.CropSubCategoryId, @cropSubCategoryId)
												 group by fm.Id , fm.FarmerRegistrationID , fm.FarmerName , fm.FarmerFatherHusbandName , fm.TotalArea , fm.FarmerRegistrationDate";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getTotalArea(string landId, string cropId, string cropTypeId, string cropCategoryId, string cropSubCategoryId, string column, string groups)
        {
            try
            {
                if (landId == null)
                {
                    landId = "NULL";
                }
                else
                {
                    landId = "'" + landId + "'";
                }

                if (cropId == null)
                {
                    cropId = "NULL";
                }
                else
                {
                    cropId = "'" + cropId + "'";
                }

                if (cropTypeId == null)
                {
                    cropTypeId = "NULL";
                }
                else
                {
                    cropTypeId = "'" + cropTypeId + "'";
                }
                if (cropCategoryId == null)
                {
                    cropCategoryId = "NULL";
                }
                else
                {
                    cropCategoryId = "'" + cropCategoryId + "'";
                }
                if (cropSubCategoryId == null)
                {
                    cropSubCategoryId = "NULL";
                }
                else
                {
                    cropSubCategoryId = "'" + cropSubCategoryId + "'";
                }
                var str = @"DECLARE @landId char(20) = " + landId + @"
                                DECLARE @cropId char(20) = " + cropId + @"	
                                DECLARE @cropTypeId char(20) = " + cropTypeId + @"
                                DECLARE @cropCategoryId char(20) = " + cropCategoryId + @"
                                DECLARE @cropSubCategoryId char(20) = " + cropSubCategoryId + @"
                            select distinct Row_Number() Over (order by fm.Id) as Snum ,fm.Id as FarmerId, fm.FarmerRegistrationID as TracenetId, fm.FarmerName as FarmerName, 
                                                 fm.FarmerFatherHusbandName as FarmerFatherName, fm.TotalArea as TotalArea, Count(fmp.FarmerMasterId) as TotalPlots , fm.FarmerRegistrationDate as RegistrationDate
                                                 from MST.FarmerMaster fm left join dbo.EmployeeInformation EI on fm.ResponsiblePersonId=EI.SystemId
												 left join SCS.State s on fm.StateId=s.Id
												 left join SCS.Country c on c.Id=s.CountryId
												 left join SCS.District d on fm.DistrictId=d.Id
												 left join HKP.Taluk t on fm.TalukaId=t.Id
												 left join HKP.Village v on fm.VillageId=v.Id
												 left join SCS.UnitOfMeasurement uom on uom.Id=fm.UOMId
												 left join MST.FarmerMasterPlot fmp on fmp.FarmerMasterId=fm.Id
												 left join HKP.LandCategory lc on lc.Id=fmp.PlotStatus
												 left join MST.ICSMaster ics on ics.Id=fmp.ICSMasterId
												 left join TRN.CropPlanning cp on cp.ICSMasterID = fmp.ICSMasterId
												 left join TRN.CropPlanningChild cpc on cpc.CropPlanningMasterId=cp.Id and cpc.FarmerPlotId=fmp.Id
												 left join MST.CropMaster cm on cm.Id = cpc.CropId
												 WHERE  ics.[" + column + @"] = '" + groups + @"'  AND
												 lc.Id =IIF(@landId  IS NULL, lc.Id,@landId ) AND
												 cpc.CropId = IIF( @cropId IS NULL , cpc.CropId, @cropId) AND
												 cpc.CropTypeId = IIF(@cropTypeId IS NULL , cpc.CropTypeId,@cropTypeId) AND
												 cm.CropCategoryId = IIF(@cropCategoryId IS NULL , cm.CropCategoryId, @cropCategoryId) AND
												 cm.CropSubCategoryId = IIF(@cropSubCategoryId IS NULL , cm.CropSubCategoryId, @cropSubCategoryId)
												 group by fm.Id , fm.FarmerRegistrationID , fm.FarmerName , fm.FarmerFatherHusbandName , fm.TotalArea , fm.FarmerRegistrationDate";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getPlannedArea(string landId, string cropId, string cropTypeId, string cropCategoryId, string cropSubCategoryId, string column, string groups)
        {
            try
            {
                if (landId == null)
                {
                    landId = "NULL";
                }
                else
                {
                    landId = "'" + landId + "'";
                }

                if (cropId == null)
                {
                    cropId = "NULL";
                }
                else
                {
                    cropId = "'" + cropId + "'";
                }

                if (cropTypeId == null)
                {
                    cropTypeId = "NULL";
                }
                else
                {
                    cropTypeId = "'" + cropTypeId + "'";
                }
                if (cropCategoryId == null)
                {
                    cropCategoryId = "NULL";
                }
                else
                {
                    cropCategoryId = "'" + cropCategoryId + "'";
                }
                if (cropSubCategoryId == null)
                {
                    cropSubCategoryId = "NULL";
                }
                else
                {
                    cropSubCategoryId = "'" + cropSubCategoryId + "'";
                }
                var str = @"DECLARE @landId char(20) = " + landId + @"
                                DECLARE @cropId char(20) = " + cropId + @"	
                                DECLARE @cropTypeId char(20) = " + cropTypeId + @"
                                DECLARE @cropCategoryId char(20) = " + cropCategoryId + @"
                                DECLARE @cropSubCategoryId char(20) = " + cropSubCategoryId + @"
                                
	                            select distinct Row_Number() Over (order by fm.Id) as Snum ,fm.Id as FarmerId, fm.FarmerRegistrationID as TracenetId, fm.FarmerName as FarmerName, 
                                fm.FarmerFatherHusbandName as FarmerFatherName,Coalesce(SUM(cpc.CropArea),0) as PlannedArea, Count(fmp.FarmerMasterId) as TotalPlots , fm.FarmerRegistrationDate as RegistrationDate  
                                                                                 from MST.FarmerMaster fm left join dbo.EmployeeInformation EI on fm.ResponsiblePersonId=EI.SystemId
												                                 left join SCS.State s on fm.StateId=s.Id
												                                  left join SCS.Country c on c.Id=s.CountryId
												                                 left join SCS.District d on fm.DistrictId=d.Id
												                                 left join HKP.Taluk t on fm.TalukaId=t.Id
												                                 left join HKP.Village v on fm.VillageId=v.Id
												                                 left join SCS.UnitOfMeasurement uom on uom.Id=fm.UOMId
												                                 left join MST.FarmerMasterPlot fmp on fmp.FarmerMasterId=fm.Id
												                                 left join HKP.LandCategory lc on lc.Id=fmp.PlotStatus
												                                 left join MST.ICSMaster ics on ics.Id=fmp.ICSMasterId
												                                 left join TRN.CropPlanning cp on cp.ICSMasterID = fmp.ICSMasterId
												                                 left join TRN.CropPlanningChild cpc on cpc.CropPlanningMasterId=cp.Id and cpc.FarmerPlotId=fmp.Id
                                                                                 left join MST.CropMaster cm on cm.Id = cpc.CropId
												                                 left join HKP.CropCategory cc on cc.Id = cm.CropCategoryId
												                                 left join HKP.CropSubCategory csc on csc.Id = cm.CropSubCategoryId
												                                 WHERE  ics.[" + column + @"] = '" + groups + @"'  AND
                                                                                 lc.Id =IIF(@landId  IS NULL, lc.Id,@landId ) AND
												                                 cpc.CropId = IIF( @cropId IS NULL , cpc.CropId, @cropId) AND
												                                 cpc.CropTypeId = IIF(@cropTypeId IS NULL , cpc.CropTypeId,@cropTypeId) AND
												                                 cm.CropCategoryId = IIF(@cropCategoryId IS NULL , cm.CropCategoryId, @cropCategoryId) AND
												                                 cm.CropSubCategoryId = IIF(@cropSubCategoryId IS NULL , cm.CropSubCategoryId, @cropSubCategoryId)

												                                 group by fm.Id , fm.FarmerRegistrationID , fm.FarmerName , fm.FarmerFatherHusbandName , fm.TotalArea , fm.FarmerRegistrationDate
 									";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// -- THe Download Parts 
        /// 
        private void SetHeaderTextTop(ref IWorksheet sheet, int row, int col, string txt, int width, ExcelHAlign al)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].HorizontalAlignment = al;

        }
        public IWorkbook GetFarmerMasterReportWorkSheet(string FarmerMasterPrintId)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];
            //var sheet1 = workbook.Worksheets[1];
            //var sheet2 = workbook.Worksheets[2];

            sheet.Name = "FarmerMaster";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            DataTable data = GetFarmerMasterReportDataByFarmerMasterId(FarmerMasterPrintId);
            if (data.Rows.Count > 0)
            {
                int ColFarmerNameHeader = 1;
                int ColFarmerNameEnd;
                int ColFarmerFatherHusbandNameHeader;
                int ColFarmerFatherHusbandNameEnd;
                int ColFarmerFatherHusbandName;
                int ColFarmerRegistrationIDHeader;
                int ColFarmerRegistrationIDEnd;
                int ColFarmerRegistrationIDName;
                int ColAddressHeader = 1;
                int ColAddressEnd;


                SetHeaderTextTop(ref sheet, ROW, ColFarmerNameHeader, "Farmer Name", 12, ExcelHAlign.HAlignLeft);
                ColFarmerNameHeader++;
                ColFarmerNameEnd = ColFarmerNameHeader + 1;
                sheet.Range[ROW, ColFarmerNameHeader, ROW, ColFarmerNameEnd].Text = data.Rows[0]["FarmerName"].ToString();
                sheet.Range[ROW, ColFarmerNameHeader, ROW, ColFarmerNameEnd].Merge();
                sheet.Range[ROW, ColFarmerNameHeader, ROW, ColFarmerNameEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColFarmerNameHeader, ROW, ColFarmerNameEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColFarmerNameEnd++;

                ColFarmerFatherHusbandNameHeader = ColFarmerNameEnd;
                SetHeaderTextTop(ref sheet, ROW, ColFarmerFatherHusbandNameHeader, "Farmer Father/ Husband Name", 20, ExcelHAlign.HAlignLeft);
                ColFarmerFatherHusbandNameHeader++;
                ColFarmerFatherHusbandNameEnd = ColFarmerFatherHusbandNameHeader + 1;
                ColFarmerFatherHusbandName = ColFarmerFatherHusbandNameHeader;
                sheet.Range[ROW, ColFarmerFatherHusbandName, ROW, ColFarmerFatherHusbandNameEnd].Text = data.Rows[0]["FarmerFatherHusbandName"].ToString();
                sheet.Range[ROW, ColFarmerFatherHusbandName, ROW, ColFarmerFatherHusbandNameEnd].Merge();
                sheet.Range[ROW, ColFarmerFatherHusbandName, ROW, ColFarmerFatherHusbandNameEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColFarmerFatherHusbandName, ROW, ColFarmerFatherHusbandNameEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //            ROW++;
                ColFarmerFatherHusbandNameEnd++;

                ColFarmerRegistrationIDHeader = ColFarmerFatherHusbandNameEnd;
                SetHeaderTextTop(ref sheet, ROW, ColFarmerRegistrationIDHeader, "Farmer Registration ID", 20, ExcelHAlign.HAlignLeft);
                ColFarmerRegistrationIDHeader++;
                ColFarmerRegistrationIDEnd = ColFarmerRegistrationIDHeader + 1;
                ColFarmerRegistrationIDName = ColFarmerRegistrationIDHeader;
                sheet.Range[ROW, ColFarmerRegistrationIDName, ROW, ColFarmerRegistrationIDEnd].Text = data.Rows[0]["FarmerRegistrationID"].ToString();
                sheet.Range[ROW, ColFarmerRegistrationIDName, ROW, ColFarmerRegistrationIDEnd].Merge();
                sheet.Range[ROW, ColFarmerRegistrationIDName, ROW, ColFarmerRegistrationIDEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColFarmerRegistrationIDName, ROW, ColFarmerRegistrationIDEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;

                ROW++;


                SetHeaderTextTop(ref sheet, ROW, ColAddressHeader, "Address", 12, ExcelHAlign.HAlignLeft);
                ColAddressHeader++;
                ColAddressEnd = ColAddressHeader + 1;
                int ColAddress = ColAddressHeader;
                sheet.Range[ROW, ColAddressHeader, ROW, ColAddressEnd].Text = data.Rows[0]["Address1"].ToString();
                sheet.Range[ROW, ColAddressHeader, ROW, ColAddressEnd].Merge();
                sheet.Range[ROW, ColAddressHeader, ROW, ColAddressEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColAddressHeader, ROW, ColAddressEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColAddressEnd++;

                SetHeaderTextTop(ref sheet, ROW, ColAddressEnd, "Gender", 20, ExcelHAlign.HAlignLeft);
                ColAddressEnd++;
                int ColGender = ColAddressEnd;
                int ColGenderEnd = ColAddressEnd + 1;
                sheet.Range[ROW, ColGender, ROW, ColGenderEnd].Text = data.Rows[0]["Gender"].ToString();
                sheet.Range[ROW, ColGender, ROW, ColGenderEnd].Merge();
                sheet.Range[ROW, ColGender, ROW, ColGenderEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColGender, ROW, ColGenderEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //  ROW++;
                ColGenderEnd++;

                SetHeaderTextTop(ref sheet, ROW, ColGenderEnd, "Total Area", 20, ExcelHAlign.HAlignLeft);
                ColGenderEnd++;
                int ColTotalArea = ColGenderEnd;
                int ColTotalAreaEnd = ColGenderEnd + 1;
                sheet.Range[ROW, ColTotalArea, ROW, ColTotalAreaEnd].Text = data.Rows[0]["TotalArea"].ToString();
                sheet.Range[ROW, ColTotalArea, ROW, ColTotalAreaEnd].Merge();
                sheet.Range[ROW, ColTotalArea, ROW, ColTotalAreaEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColTotalArea, ROW, ColTotalAreaEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //   ROW++;
                ColTotalAreaEnd++;

                SetHeaderTextTop(ref sheet, ROW, ColTotalAreaEnd, "UOM", 20, ExcelHAlign.HAlignLeft);
                ColTotalAreaEnd++;
                int ColUOM = ColTotalAreaEnd;
                int ColUOMEnd = ColTotalAreaEnd + 1;
                sheet.Range[ROW, ColUOM, ROW, ColUOMEnd].Text = data.Rows[0]["UOM"].ToString();
                sheet.Range[ROW, ColUOM, ROW, ColUOMEnd].Merge();
                sheet.Range[ROW, ColUOM, ROW, ColUOMEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColUOM, ROW, ColUOMEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;

            }

            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Plot Name/ No", 12, ExcelHAlign.HAlignLeft);
            int ColPlotNameNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Plot Status", 8, ExcelHAlign.HAlignLeft);
            int ColFmpPlotStatus = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Plot Area", 8, ExcelHAlign.HAlignRight);
            int ColPlotArea = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Survey", 15, ExcelHAlign.HAlignLeft);
            int ColSurvey = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ICS Master", 15, ExcelHAlign.HAlignLeft);
            int ColfmpICSMaster = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Inspection Date", 20, ExcelHAlign.HAlignLeft);
            int ColInspectionDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Approval Date", 11, ExcelHAlign.HAlignLeft);
            int ColApprovalDate = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Renewal Period", 11, ExcelHAlign.HAlignRight);
            int ColRenewalPeriod = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 10, ExcelHAlign.HAlignRight);
            int ColfmpRemarks = COL;
            ROW++;
            endCol = COL;
            #endregion Headers




            string PlotNameAndNo = "";
            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {

                if (PlotNameAndNo != data.Rows[i]["PlotNameNo"].ToString())
                {

                    if (RowIndex < ROW)
                    {
                        sheet.Range[RowIndex, ColPlotNameNo, ROW - 1, ColPlotNameNo].Merge();
                        sheet.Range[RowIndex, ColPlotNameNo, ROW - 1, ColPlotNameNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[RowIndex, ColPlotNameNo, ROW - 1, ColPlotNameNo].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    }
                    RowIndex = ROW;
                }

                //sheet[ROW, ColAVGTotalTime].Number = clsStaticInfo.dbl(data.Rows[i]["AvgAllotedTime"].ToString());
                //sheet[ROW, ColAVGTotalTime].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColPlotArea].Number = clsStaticInfo.dbl(data.Rows[i]["PlotArea"].ToString());
                sheet[ROW, ColPlotNameNo].Text = data.Rows[i]["PlotNameNo"].ToString();
                sheet[ROW, ColFmpPlotStatus].Text = data.Rows[i]["FmpPlotStatus"].ToString();
                sheet[ROW, ColSurvey].Text = data.Rows[i]["Survey"].ToString();
                sheet[ROW, ColfmpICSMaster].Text = data.Rows[i]["fmpICSMaster"].ToString();
                sheet[ROW, ColInspectionDate].Text = data.Rows[i]["InspectionDate"].ToString();

                sheet[ROW, ColApprovalDate].Text = data.Rows[i]["ApprovalDate"].ToString();
                sheet[ROW, ColRenewalPeriod].Text = data.Rows[i]["RenewalPeriod"].ToString();


                sheet[ROW, ColfmpRemarks].Text = data.Rows[i]["fmpRemarks"].ToString();

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                PlotNameAndNo = data.Rows[i]["PlotNameNo"].ToString();

                ROW++;
            }

            endRow = ROW - 1;

            if (RowIndex < ROW - 1)
            {
                sheet.Range[RowIndex, ColPlotNameNo, ROW - 1, ColPlotNameNo].Merge();
                sheet.Range[RowIndex, ColPlotNameNo, ROW - 1, ColPlotNameNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColPlotNameNo, ROW - 1, ColPlotNameNo].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            }



            // var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.000";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            //report.CompanyPlantHeader(ref sheet, endCol, "Farmer Master", identity.CompanyId, identity.PlantName, null);
            //report.CompanyPlantHeader(ref sheet, endCol, "Farmer Master", "Odyssey", "Hyderbad", null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }

        private DataTable GetFarmerMasterReportDataByFarmerMasterId(string FarmerMasterPrintId)
        {
            var sql = @"select fm.*,c.Id as CountryId,c.UserName as Country,s.UserName as State,d.UserName as District,t.UserName as Taluk,v.UserName as Villages,EI.EmployeeStatus,EI.EmployeeCode,EI.EmployeeName as ResponsiblePerson, uom.UserName as UOM
                                                 ,fmp.Id as FmpId,fmp.FarmerMasterId,fmp.PlotNameNo,fmp.PlotArea,fmp.Survey,fmp.Latitude,fmp.Longitude,fmp.PlotStatus,fmp.ICSMasterId,fmp.FarmerRegistrationID as fmpFarmerRegistrationID
												 ,fmp.FarmerRegistrationDate as fmpFarmerRegistrationDate,fmp.InspectionDate,fmp.ApprovalDate,fmp.RenewalPeriod,fmp.FileName, fmp.Remarks as fmpRemarks, fmp.Active as fmpActive
												 ,lc.UserName as FmpPlotStatus, ics.Name as fmpICSMaster 
                                                 from MST.FarmerMaster fm left join dbo.EmployeeInformation EI on fm.ResponsiblePersonId=EI.SystemId
												 left join SCS.State s on fm.StateId=s.Id
												  left join SCS.Country c on c.Id=s.CountryId
												 left join SCS.District d on fm.DistrictId=d.Id
												 left join HKP.Taluk t on fm.TalukaId=t.Id
												 left join HKP.Village v on fm.VillageId=v.Id
												 left join SCS.UnitOfMeasurement uom on uom.Id=fm.UOMId
												 left join MST.FarmerMasterPlot fmp on fmp.FarmerMasterId=fm.Id
												 left join HKP.LandCategory lc on lc.Id=fmp.PlotStatus
												 left join MST.ICSMaster ics on ics.Id=fmp.ICSMasterId
                                                 where fm.Id = '" + FarmerMasterPrintId + "'";

            return _sqlRepository.GetDataTable(sql);
        }

        private DataTable getActiveFarmersReport(string landId, string cropId, string cropTypeId, string cropCategoryId, string cropSubCategoryId, string column, string groups)
        {
            try
            {

                if (landId == null || landId == "null")
                {
                    landId = "NULL";
                }
                else
                {
                    landId = "'" + landId + "'";
                }

                if (cropId == null || cropId == "null")
                {
                    cropId = "NULL";
                }
                else
                {
                    cropId = "'" + cropId + "'";
                }

                if (cropTypeId == null || cropTypeId == "null")
                {
                    cropTypeId = "NULL";
                }
                else
                {
                    cropTypeId = "'" + cropTypeId + "'";
                }
                if (cropCategoryId == null || cropCategoryId == "null")
                {
                    cropCategoryId = "NULL";
                }
                else
                {
                    cropCategoryId = "'" + cropCategoryId + "'";
                }
                if (cropSubCategoryId == null || cropSubCategoryId == "null")
                {
                    cropSubCategoryId = "NULL";
                }
                else
                {
                    cropSubCategoryId = "'" + cropSubCategoryId + "'";
                }
                var str = @"DECLARE @landId char(20) = " + landId + @"
                                DECLARE @cropId char(20) = " + cropId + @"	
                                DECLARE @cropTypeId char(20) = " + cropTypeId + @"
                                DECLARE @cropCategoryId char(20) = " + cropCategoryId + @"
                                DECLARE @cropSubCategoryId char(20) = " + cropSubCategoryId + @"
                                select distinct Row_Number() Over (order by fm.Id) as Snum ,fm.Id as FarmerId, fm.FarmerRegistrationID as TracenetId, fm.FarmerName as FarmerName, 
                                fm.FarmerFatherHusbandName as FarmerFatherName, fm.TotalArea as TotalArea, Count(fmp.FarmerMasterId) as TotalPlots , fm.FarmerRegistrationDate as RegistrationDate
                                                 from MST.FarmerMaster fm left join dbo.EmployeeInformation EI on fm.ResponsiblePersonId=EI.SystemId
												 left join SCS.State s on fm.StateId=s.Id
												 left join SCS.Country c on c.Id=s.CountryId
												 left join SCS.District d on fm.DistrictId=d.Id
												 left join HKP.Taluk t on fm.TalukaId=t.Id
												 left join HKP.Village v on fm.VillageId=v.Id
												 left join SCS.UnitOfMeasurement uom on uom.Id=fm.UOMId
												 left join MST.FarmerMasterPlot fmp on fmp.FarmerMasterId=fm.Id
												 left join HKP.LandCategory lc on lc.Id=fmp.PlotStatus
												 left join MST.ICSMaster ics on ics.Id=fmp.ICSMasterId
												 left join TRN.CropPlanning cp on cp.ICSMasterID = fmp.ICSMasterId
												 left join TRN.CropPlanningChild cpc on cpc.CropPlanningMasterId=cp.Id and cpc.FarmerPlotId=fmp.Id
												 left join MST.CropMaster cm on cm.Id = cpc.CropId
												  WHERE  ics.[" + column + @"] = '" + groups + @"' AND fm.Active = '1' AND
												 lc.Id =IIF(@landId  IS NULL, lc.Id,@landId ) AND
												 cpc.CropId = IIF( @cropId IS NULL , cpc.CropId, @cropId) AND
												 cpc.CropTypeId = IIF(@cropTypeId IS NULL , cpc.CropTypeId,@cropTypeId) AND
												 cm.CropCategoryId = IIF(@cropCategoryId IS NULL , cm.CropCategoryId, @cropCategoryId) AND
												 cm.CropSubCategoryId = IIF(@cropSubCategoryId IS NULL , cm.CropSubCategoryId, @cropSubCategoryId)
												 group by fm.Id , fm.FarmerRegistrationID , fm.FarmerName , fm.FarmerFatherHusbandName , fm.TotalArea , fm.FarmerRegistrationDate";
                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IWorkbook GetActiveFarmersReport(string landId, string cropId, string cropTypeId, string cropCategoryId, string cropSubCategoryId, string column, string groups)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];
            //var sheet1 = workbook.Worksheets[1];
            //var sheet2 = workbook.Worksheets[2];

            sheet.Name = "Active Farmer Report";


            int ROW = 2;
            int endCol = 1;
            int COL = 1;


            DataTable data = getActiveFarmersReport(landId, cropId, cropTypeId, cropCategoryId, cropSubCategoryId, column, groups);


            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "S. No.", 12, ExcelHAlign.HAlignLeft);
            int ColSnum = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Farmer ID", 8, ExcelHAlign.HAlignLeft);
            int ColFarmerId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Tracenet ID", 8, ExcelHAlign.HAlignRight);
            int ColTracenetId = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Farmer Name", 15, ExcelHAlign.HAlignLeft);
            int ColFarmerName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Farmer Father Name", 15, ExcelHAlign.HAlignLeft);
            int ColFarmerFatherName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Total Area", 20, ExcelHAlign.HAlignLeft);
            int ColTotalArea = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Total Plots", 11, ExcelHAlign.HAlignLeft);
            int ColTotalPlots = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Registration Date", 11, ExcelHAlign.HAlignRight);
            int ColRegistrationDate = COL;
            COL++;

            ROW++;
            endCol = COL;
            #endregion Headers




            string ColSum = "";
            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {

                if (ColSum != data.Rows[i]["Snum"].ToString())
                {

                    if (RowIndex < ROW)
                    {
                        sheet.Range[RowIndex, ColSnum, ROW - 1, ColSnum].Merge();
                        sheet.Range[RowIndex, ColSnum, ROW - 1, ColSnum].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[RowIndex, ColSnum, ROW - 1, ColSnum].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    }
                    RowIndex = ROW;
                }
                //sheet[ROW, ColAVGTotalTime].Number = clsStaticInfo.dbl(data.Rows[i]["AvgAllotedTime"].ToString());
                //sheet[ROW, ColAVGTotalTime].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColSnum].Number = clsStaticInfo.dbl(data.Rows[i]["Snum"].ToString());
                sheet[ROW, ColFarmerId].Text = data.Rows[i]["FarmerId"].ToString();
                sheet[ROW, ColTracenetId].Text = data.Rows[i]["TracenetId"].ToString();
                sheet[ROW, ColFarmerName].Text = data.Rows[i]["FarmerName"].ToString();
                sheet[ROW, ColFarmerFatherName].Text = data.Rows[i]["FarmerFatherName"].ToString();
                sheet[ROW, ColTotalArea].Text = data.Rows[i]["TotalArea"].ToString();

                sheet[ROW, ColTotalPlots].Text = data.Rows[i]["TotalPlots"].ToString();
                sheet[ROW, ColRegistrationDate].Text = data.Rows[i]["RegistrationDate"].ToString();




                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                //  PlotNameAndNo = data.Rows[i]["PlotNameNo"].ToString();

                ROW++;
            }

            endRow = ROW - 1;

            if (RowIndex < ROW - 1)
            {
                sheet.Range[RowIndex, ColSnum, ROW - 1, ColSnum].Merge();
                sheet.Range[RowIndex, ColSnum, ROW - 1, ColSnum].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColSnum, ROW - 1, ColSnum].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            }



            // var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.000";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            //report.CompanyPlantHeader(ref sheet, endCol, "Farmer Master", identity.CompanyId, identity.PlantName, null);
            //report.CompanyPlantHeader(ref sheet, endCol, "Farmer Master", "Odyssey", "Hyderbad", null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }


        private DataTable getInactiveFarmersReport(string landId, string cropId, string cropTypeId, string cropCategoryId, string cropSubCategoryId, string column, string groups)
        {
            try
            {

                if (landId == null || landId == "null")
                {
                    landId = "NULL";
                }
                else
                {
                    landId = "'" + landId + "'";
                }

                if (cropId == null || cropId == "null")
                {
                    cropId = "NULL";
                }
                else
                {
                    cropId = "'" + cropId + "'";
                }

                if (cropTypeId == null || cropTypeId == "null")
                {
                    cropTypeId = "NULL";
                }
                else
                {
                    cropTypeId = "'" + cropTypeId + "'";
                }
                if (cropCategoryId == null || cropCategoryId == "null")
                {
                    cropCategoryId = "NULL";
                }
                else
                {
                    cropCategoryId = "'" + cropCategoryId + "'";
                }
                if (cropSubCategoryId == null || cropSubCategoryId == "null")
                {
                    cropSubCategoryId = "NULL";
                }
                else
                {
                    cropSubCategoryId = "'" + cropSubCategoryId + "'";
                }
                var str = @"DECLARE @landId char(20) = " + landId + @"
                                DECLARE @cropId char(20) = " + cropId + @"	
                                DECLARE @cropTypeId char(20) = " + cropTypeId + @"
                                DECLARE @cropCategoryId char(20) = " + cropCategoryId + @"
                                DECLARE @cropSubCategoryId char(20) = " + cropSubCategoryId + @"
                                select distinct Row_Number() Over (order by fm.Id) as Snum ,fm.Id as FarmerId, fm.FarmerRegistrationID as TracenetId, fm.FarmerName as FarmerName, 
                                fm.FarmerFatherHusbandName as FarmerFatherName, fm.TotalArea as TotalArea, Count(fmp.FarmerMasterId) as TotalPlots , fm.FarmerRegistrationDate as RegistrationDate
                                                 from MST.FarmerMaster fm left join dbo.EmployeeInformation EI on fm.ResponsiblePersonId=EI.SystemId
												 left join SCS.State s on fm.StateId=s.Id
												 left join SCS.Country c on c.Id=s.CountryId
												 left join SCS.District d on fm.DistrictId=d.Id
												 left join HKP.Taluk t on fm.TalukaId=t.Id
												 left join HKP.Village v on fm.VillageId=v.Id
												 left join SCS.UnitOfMeasurement uom on uom.Id=fm.UOMId
												 left join MST.FarmerMasterPlot fmp on fmp.FarmerMasterId=fm.Id
												 left join HKP.LandCategory lc on lc.Id=fmp.PlotStatus
												 left join MST.ICSMaster ics on ics.Id=fmp.ICSMasterId
												 left join TRN.CropPlanning cp on cp.ICSMasterID = fmp.ICSMasterId
												 left join TRN.CropPlanningChild cpc on cpc.CropPlanningMasterId=cp.Id and cpc.FarmerPlotId=fmp.Id
												 left join MST.CropMaster cm on cm.Id = cpc.CropId
												  WHERE  ics.[" + column + @"] = '" + groups + @"' AND fm.Active = '0' AND
												 lc.Id =IIF(@landId  IS NULL, lc.Id,@landId ) AND
												 cpc.CropId = IIF( @cropId IS NULL , cpc.CropId, @cropId) AND
												 cpc.CropTypeId = IIF(@cropTypeId IS NULL , cpc.CropTypeId,@cropTypeId) AND
												 cm.CropCategoryId = IIF(@cropCategoryId IS NULL , cm.CropCategoryId, @cropCategoryId) AND
												 cm.CropSubCategoryId = IIF(@cropSubCategoryId IS NULL , cm.CropSubCategoryId, @cropSubCategoryId)
												 group by fm.Id , fm.FarmerRegistrationID , fm.FarmerName , fm.FarmerFatherHusbandName , fm.TotalArea , fm.FarmerRegistrationDate";
                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }







        public IWorkbook GetInactiveFarmersReport(string landId, string cropId, string cropTypeId, string cropCategoryId, string cropSubCategoryId, string column, string groups)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];
            //var sheet1 = workbook.Worksheets[1];
            //var sheet2 = workbook.Worksheets[2];

            sheet.Name = "Inactive Farmer Report";


            int ROW = 2;
            int endCol = 1;
            int COL = 1;


            DataTable data = getInactiveFarmersReport(landId, cropId, cropTypeId, cropCategoryId, cropSubCategoryId, column, groups);


            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "S. No.", 12, ExcelHAlign.HAlignLeft);
            int ColSnum = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Farmer ID", 8, ExcelHAlign.HAlignLeft);
            int ColFarmerId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Tracenet ID", 8, ExcelHAlign.HAlignRight);
            int ColTracenetId = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Farmer Name", 15, ExcelHAlign.HAlignLeft);
            int ColFarmerName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Farmer Father Name", 15, ExcelHAlign.HAlignLeft);
            int ColFarmerFatherName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Total Area", 20, ExcelHAlign.HAlignLeft);
            int ColTotalArea = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Total Plots", 11, ExcelHAlign.HAlignLeft);
            int ColTotalPlots = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Registration Date", 11, ExcelHAlign.HAlignRight);
            int ColRegistrationDate = COL;
            COL++;

            ROW++;
            endCol = COL;
            #endregion Headers




            string ColSum = "";
            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {

                if (ColSum != data.Rows[i]["Snum"].ToString())
                {

                    if (RowIndex < ROW)
                    {
                        sheet.Range[RowIndex, ColSnum, ROW - 1, ColSnum].Merge();
                        sheet.Range[RowIndex, ColSnum, ROW - 1, ColSnum].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[RowIndex, ColSnum, ROW - 1, ColSnum].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    }
                    RowIndex = ROW;
                }
                //sheet[ROW, ColAVGTotalTime].Number = clsStaticInfo.dbl(data.Rows[i]["AvgAllotedTime"].ToString());
                //sheet[ROW, ColAVGTotalTime].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColSnum].Number = clsStaticInfo.dbl(data.Rows[i]["Snum"].ToString());
                sheet[ROW, ColFarmerId].Text = data.Rows[i]["FarmerId"].ToString();
                sheet[ROW, ColTracenetId].Text = data.Rows[i]["TracenetId"].ToString();
                sheet[ROW, ColFarmerName].Text = data.Rows[i]["FarmerName"].ToString();
                sheet[ROW, ColFarmerFatherName].Text = data.Rows[i]["FarmerFatherName"].ToString();
                sheet[ROW, ColTotalArea].Text = data.Rows[i]["TotalArea"].ToString();

                sheet[ROW, ColTotalPlots].Text = data.Rows[i]["TotalPlots"].ToString();
                sheet[ROW, ColRegistrationDate].Text = data.Rows[i]["RegistrationDate"].ToString();




                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                //  PlotNameAndNo = data.Rows[i]["PlotNameNo"].ToString();

                ROW++;
            }

            endRow = ROW - 1;

            if (RowIndex < ROW - 1)
            {
                sheet.Range[RowIndex, ColSnum, ROW - 1, ColSnum].Merge();
                sheet.Range[RowIndex, ColSnum, ROW - 1, ColSnum].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColSnum, ROW - 1, ColSnum].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            }



            // var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.000";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            //report.CompanyPlantHeader(ref sheet, endCol, "Farmer Master", identity.CompanyId, identity.PlantName, null);
            //report.CompanyPlantHeader(ref sheet, endCol, "Farmer Master", "Odyssey", "Hyderbad", null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }




        private DataTable getTotalAreaReport(string landId, string cropId, string cropTypeId, string cropCategoryId, string cropSubCategoryId, string column, string groups)
        {
            try
            {

                if (landId == null || landId == "null")
                {
                    landId = "NULL";
                }
                else
                {
                    landId = "'" + landId + "'";
                }

                if (cropId == null || cropId == "null")
                {
                    cropId = "NULL";
                }
                else
                {
                    cropId = "'" + cropId + "'";
                }

                if (cropTypeId == null || cropTypeId == "null")
                {
                    cropTypeId = "NULL";
                }
                else
                {
                    cropTypeId = "'" + cropTypeId + "'";
                }
                if (cropCategoryId == null || cropCategoryId == "null")
                {
                    cropCategoryId = "NULL";
                }
                else
                {
                    cropCategoryId = "'" + cropCategoryId + "'";
                }
                if (cropSubCategoryId == null || cropSubCategoryId == "null")
                {
                    cropSubCategoryId = "NULL";
                }
                else
                {
                    cropSubCategoryId = "'" + cropSubCategoryId + "'";
                }
                var str = @"DECLARE @landId char(20) = " + landId + @"
                                DECLARE @cropId char(20) = " + cropId + @"	
                                DECLARE @cropTypeId char(20) = " + cropTypeId + @"
                                DECLARE @cropCategoryId char(20) = " + cropCategoryId + @"
                                DECLARE @cropSubCategoryId char(20) = " + cropSubCategoryId + @"
                            select distinct Row_Number() Over (order by fm.Id) as Snum ,fm.Id as FarmerId, fm.FarmerRegistrationID as TracenetId, fm.FarmerName as FarmerName, 
                                                 fm.FarmerFatherHusbandName as FarmerFatherName, fm.TotalArea as TotalArea, Count(fmp.FarmerMasterId) as TotalPlots , fm.FarmerRegistrationDate as RegistrationDate
                                                 from MST.FarmerMaster fm left join dbo.EmployeeInformation EI on fm.ResponsiblePersonId=EI.SystemId
												 left join SCS.State s on fm.StateId=s.Id
												 left join SCS.Country c on c.Id=s.CountryId
												 left join SCS.District d on fm.DistrictId=d.Id
												 left join HKP.Taluk t on fm.TalukaId=t.Id
												 left join HKP.Village v on fm.VillageId=v.Id
												 left join SCS.UnitOfMeasurement uom on uom.Id=fm.UOMId
												 left join MST.FarmerMasterPlot fmp on fmp.FarmerMasterId=fm.Id
												 left join HKP.LandCategory lc on lc.Id=fmp.PlotStatus
												 left join MST.ICSMaster ics on ics.Id=fmp.ICSMasterId
												 left join TRN.CropPlanning cp on cp.ICSMasterID = fmp.ICSMasterId
												 left join TRN.CropPlanningChild cpc on cpc.CropPlanningMasterId=cp.Id and cpc.FarmerPlotId=fmp.Id
												 left join MST.CropMaster cm on cm.Id = cpc.CropId
												 WHERE  ics.[" + column + @"] = '" + groups + @"'  AND
												 lc.Id =IIF(@landId  IS NULL, lc.Id,@landId ) AND
												 cpc.CropId = IIF( @cropId IS NULL , cpc.CropId, @cropId) AND
												 cpc.CropTypeId = IIF(@cropTypeId IS NULL , cpc.CropTypeId,@cropTypeId) AND
												 cm.CropCategoryId = IIF(@cropCategoryId IS NULL , cm.CropCategoryId, @cropCategoryId) AND
												 cm.CropSubCategoryId = IIF(@cropSubCategoryId IS NULL , cm.CropSubCategoryId, @cropSubCategoryId)
												 group by fm.Id , fm.FarmerRegistrationID , fm.FarmerName , fm.FarmerFatherHusbandName , fm.TotalArea , fm.FarmerRegistrationDate";
                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IWorkbook GetTotalAreaReport(string landId, string cropId, string cropTypeId, string cropCategoryId, string cropSubCategoryId, string column, string groups)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];
            //var sheet1 = workbook.Worksheets[1];
            //var sheet2 = workbook.Worksheets[2];

            sheet.Name = "Total Area Report";


            int ROW = 2;
            int endCol = 1;
            int COL = 1;


            DataTable data = getTotalAreaReport(landId, cropId, cropTypeId, cropCategoryId, cropSubCategoryId, column, groups);


            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "S. No.", 12, ExcelHAlign.HAlignLeft);
            int ColSnum = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Farmer ID", 8, ExcelHAlign.HAlignLeft);
            int ColFarmerId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Tracenet ID", 8, ExcelHAlign.HAlignRight);
            int ColTracenetId = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Farmer Name", 15, ExcelHAlign.HAlignLeft);
            int ColFarmerName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Farmer Father Name", 15, ExcelHAlign.HAlignLeft);
            int ColFarmerFatherName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Total Area", 20, ExcelHAlign.HAlignLeft);
            int ColTotalArea = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Total Plots", 11, ExcelHAlign.HAlignLeft);
            int ColTotalPlots = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Registration Date", 11, ExcelHAlign.HAlignRight);
            int ColRegistrationDate = COL;
            COL++;

            ROW++;
            endCol = COL;
            #endregion Headers




            string ColSum = "";
            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {

                if (ColSum != data.Rows[i]["Snum"].ToString())
                {

                    if (RowIndex < ROW)
                    {
                        sheet.Range[RowIndex, ColSnum, ROW - 1, ColSnum].Merge();
                        sheet.Range[RowIndex, ColSnum, ROW - 1, ColSnum].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[RowIndex, ColSnum, ROW - 1, ColSnum].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    }
                    RowIndex = ROW;
                }
                //sheet[ROW, ColAVGTotalTime].Number = clsStaticInfo.dbl(data.Rows[i]["AvgAllotedTime"].ToString());
                //sheet[ROW, ColAVGTotalTime].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColSnum].Number = clsStaticInfo.dbl(data.Rows[i]["Snum"].ToString());
                sheet[ROW, ColFarmerId].Text = data.Rows[i]["FarmerId"].ToString();
                sheet[ROW, ColTracenetId].Text = data.Rows[i]["TracenetId"].ToString();
                sheet[ROW, ColFarmerName].Text = data.Rows[i]["FarmerName"].ToString();
                sheet[ROW, ColFarmerFatherName].Text = data.Rows[i]["FarmerFatherName"].ToString();
                sheet[ROW, ColTotalArea].Text = data.Rows[i]["TotalArea"].ToString();

                sheet[ROW, ColTotalPlots].Text = data.Rows[i]["TotalPlots"].ToString();
                sheet[ROW, ColRegistrationDate].Text = data.Rows[i]["RegistrationDate"].ToString();




                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                //  PlotNameAndNo = data.Rows[i]["PlotNameNo"].ToString();

                ROW++;
            }

            endRow = ROW - 1;

            if (RowIndex < ROW - 1)
            {
                sheet.Range[RowIndex, ColSnum, ROW - 1, ColSnum].Merge();
                sheet.Range[RowIndex, ColSnum, ROW - 1, ColSnum].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColSnum, ROW - 1, ColSnum].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            }



            // var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.000";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            //report.CompanyPlantHeader(ref sheet, endCol, "Farmer Master", identity.CompanyId, identity.PlantName, null);
            //report.CompanyPlantHeader(ref sheet, endCol, "Farmer Master", "Odyssey", "Hyderbad", null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }


        private DataTable getPlannedAreaReport(string landId, string cropId, string cropTypeId, string cropCategoryId, string cropSubCategoryId, string column, string groups)
        {
            try
            {

                if (landId == null || landId == "null")
                {
                    landId = "NULL";
                }
                else
                {
                    landId = "'" + landId + "'";
                }

                if (cropId == null || cropId == "null")
                {
                    cropId = "NULL";
                }
                else
                {
                    cropId = "'" + cropId + "'";
                }

                if (cropTypeId == null || cropTypeId == "null")
                {
                    cropTypeId = "NULL";
                }
                else
                {
                    cropTypeId = "'" + cropTypeId + "'";
                }
                if (cropCategoryId == null || cropCategoryId == "null")
                {
                    cropCategoryId = "NULL";
                }
                else
                {
                    cropCategoryId = "'" + cropCategoryId + "'";
                }
                if (cropSubCategoryId == null || cropSubCategoryId == "null")
                {
                    cropSubCategoryId = "NULL";
                }
                else
                {
                    cropSubCategoryId = "'" + cropSubCategoryId + "'";
                }
                var str = @"DECLARE @landId char(20) = " + landId + @"
                                DECLARE @cropId char(20) = " + cropId + @"	
                                DECLARE @cropTypeId char(20) = " + cropTypeId + @"
                                DECLARE @cropCategoryId char(20) = " + cropCategoryId + @"
                                DECLARE @cropSubCategoryId char(20) = " + cropSubCategoryId + @"
                                
	                            select distinct Row_Number() Over (order by fm.Id) as Snum ,fm.Id as FarmerId, fm.FarmerRegistrationID as TracenetId, fm.FarmerName as FarmerName, 
                                fm.FarmerFatherHusbandName as FarmerFatherName,Coalesce(SUM(cpc.CropArea),0) as PlannedArea, Count(fmp.FarmerMasterId) as TotalPlots , fm.FarmerRegistrationDate as RegistrationDate  
                                                                                 from MST.FarmerMaster fm left join dbo.EmployeeInformation EI on fm.ResponsiblePersonId=EI.SystemId
												                                 left join SCS.State s on fm.StateId=s.Id
												                                  left join SCS.Country c on c.Id=s.CountryId
												                                 left join SCS.District d on fm.DistrictId=d.Id
												                                 left join HKP.Taluk t on fm.TalukaId=t.Id
												                                 left join HKP.Village v on fm.VillageId=v.Id
												                                 left join SCS.UnitOfMeasurement uom on uom.Id=fm.UOMId
												                                 left join MST.FarmerMasterPlot fmp on fmp.FarmerMasterId=fm.Id
												                                 left join HKP.LandCategory lc on lc.Id=fmp.PlotStatus
												                                 left join MST.ICSMaster ics on ics.Id=fmp.ICSMasterId
												                                 left join TRN.CropPlanning cp on cp.ICSMasterID = fmp.ICSMasterId
												                                 left join TRN.CropPlanningChild cpc on cpc.CropPlanningMasterId=cp.Id and cpc.FarmerPlotId=fmp.Id
                                                                                 left join MST.CropMaster cm on cm.Id = cpc.CropId
												                                 left join HKP.CropCategory cc on cc.Id = cm.CropCategoryId
												                                 left join HKP.CropSubCategory csc on csc.Id = cm.CropSubCategoryId
												                                 WHERE  ics.[" + column + @"] = '" + groups + @"'  AND
                                                                                 lc.Id =IIF(@landId  IS NULL, lc.Id,@landId ) AND
												                                 cpc.CropId = IIF( @cropId IS NULL , cpc.CropId, @cropId) AND
												                                 cpc.CropTypeId = IIF(@cropTypeId IS NULL , cpc.CropTypeId,@cropTypeId) AND
												                                 cm.CropCategoryId = IIF(@cropCategoryId IS NULL , cm.CropCategoryId, @cropCategoryId) AND
												                                 cm.CropSubCategoryId = IIF(@cropSubCategoryId IS NULL , cm.CropSubCategoryId, @cropSubCategoryId)

												                                 group by fm.Id , fm.FarmerRegistrationID , fm.FarmerName , fm.FarmerFatherHusbandName , fm.TotalArea , fm.FarmerRegistrationDate
 									";
                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IWorkbook GetPlannedAreaReport(string landId, string cropId, string cropTypeId, string cropCategoryId, string cropSubCategoryId, string column, string groups)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];
            //var sheet1 = workbook.Worksheets[1];
            //var sheet2 = workbook.Worksheets[2];

            sheet.Name = "Planned Area Report";


            int ROW = 2;
            int endCol = 1;
            int COL = 1;


            DataTable data = getPlannedAreaReport(landId, cropId, cropTypeId, cropCategoryId, cropSubCategoryId, column, groups);


            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "S. No.", 12, ExcelHAlign.HAlignLeft);
            int ColSnum = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Farmer ID", 8, ExcelHAlign.HAlignLeft);
            int ColFarmerId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Tracenet ID", 8, ExcelHAlign.HAlignRight);
            int ColTracenetId = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Farmer Name", 15, ExcelHAlign.HAlignLeft);
            int ColFarmerName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Farmer Father Name", 15, ExcelHAlign.HAlignLeft);
            int ColFarmerFatherName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Plan Area", 20, ExcelHAlign.HAlignLeft);
            int ColPlanlArea = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Total Plots", 11, ExcelHAlign.HAlignLeft);
            int ColTotalPlots = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Registration Date", 11, ExcelHAlign.HAlignRight);
            int ColRegistrationDate = COL;
            COL++;

            ROW++;
            endCol = COL;
            #endregion Headers




            string ColSum = "";
            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {

                if (ColSum != data.Rows[i]["Snum"].ToString())
                {

                    if (RowIndex < ROW)
                    {
                        sheet.Range[RowIndex, ColSnum, ROW - 1, ColSnum].Merge();
                        sheet.Range[RowIndex, ColSnum, ROW - 1, ColSnum].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[RowIndex, ColSnum, ROW - 1, ColSnum].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    }
                    RowIndex = ROW;
                }
                //sheet[ROW, ColAVGTotalTime].Number = clsStaticInfo.dbl(data.Rows[i]["AvgAllotedTime"].ToString());
                //sheet[ROW, ColAVGTotalTime].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColSnum].Number = clsStaticInfo.dbl(data.Rows[i]["Snum"].ToString());
                sheet[ROW, ColFarmerId].Text = data.Rows[i]["FarmerId"].ToString();
                sheet[ROW, ColTracenetId].Text = data.Rows[i]["TracenetId"].ToString();
                sheet[ROW, ColFarmerName].Text = data.Rows[i]["FarmerName"].ToString();
                sheet[ROW, ColFarmerFatherName].Text = data.Rows[i]["FarmerFatherName"].ToString();
                sheet[ROW, ColPlanlArea].Text = data.Rows[i]["PlannedArea"].ToString();

                sheet[ROW, ColTotalPlots].Text = data.Rows[i]["TotalPlots"].ToString();
                sheet[ROW, ColRegistrationDate].Text = data.Rows[i]["RegistrationDate"].ToString();




                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                //  PlotNameAndNo = data.Rows[i]["PlotNameNo"].ToString();

                ROW++;
            }

            endRow = ROW - 1;

            if (RowIndex < ROW - 1)
            {
                sheet.Range[RowIndex, ColSnum, ROW - 1, ColSnum].Merge();
                sheet.Range[RowIndex, ColSnum, ROW - 1, ColSnum].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColSnum, ROW - 1, ColSnum].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            }



            // var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.000";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            //report.CompanyPlantHeader(ref sheet, endCol, "Farmer Master", identity.CompanyId, identity.PlantName, null);
            //report.CompanyPlantHeader(ref sheet, endCol, "Farmer Master", "Odyssey", "Hyderbad", null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }
    }
}
