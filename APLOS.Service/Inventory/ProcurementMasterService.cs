using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Inventory;
using Library.Model.Taxations;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using Library.ViewModel.OrderManagements;
using OTSBD;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace Library.Service.Inventory
{
    public class ProcurementMasterService : Service<ProcurementMaster>, IProcurementMasterService

    {

        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<ProcurementMaster> _procurementMasterRepository;
        private readonly IRepositoryAsync<ProcurementMasterDetail> _materialRequsitionDetailsRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ProcurementMasterService(
            IRepositoryAsync<ProcurementMaster> procurementMasterRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IRepositoryAsync<ProcurementMasterDetail> materialRequsitionDetailsRepository
            ) : base(procurementMasterRepository, unitOfWork, pkGeneratorService)
        {
            _procurementMasterRepository = procurementMasterRepository;
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
            _materialRequsitionDetailsRepository = materialRequsitionDetailsRepository;
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(ProcurementMaster), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        //private void Check(ProcurementMaster entity)
        //{
        //    CheckUniqueColumn(UniqueColumnName.Code, entity.MaterialType, r => r.Id != entity.Id && r.MaterialType == entity.MaterialType);
        //    CheckUniqueColumn(UniqueColumnName.UserName, entity.MaterialMasterId, r => r.Id != entity.Id && r.MaterialMasterId == entity.MaterialMasterId);
  
        //}
        public override void Insert(ProcurementMaster entity)
        {
            try
            {
                var MaterialMId = _procurementMasterRepository.Query(r => r.MaterialMasterId == entity.MaterialMasterId).Select().FirstOrDefault();
                //string _sql = "Select MaterialMasterId from [TRN].[ProcurementMaster] Where MaterialMasterId='" + entity.MaterialMasterId+"'";
                //var res= _procurementMasterRepository.ExecuteSqlCommand(_sql);
                //if(res==)

                //var MaterialMasterId = _procurementMasterRepository.SqlQuery<int>($"Select Id from [TRN].[ProcurementMaster] Where MaterialMasterId='{entity.MaterialMasterId}'").First();
                if (MaterialMId == null)
                {

                    entity.Id = GetPK();
                    base.Insert(entity);

                }
                else
                {
                    throw new CustomException("Material Already Exists");
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
                
            }
        }

        public void DeleteReq(string id)
        {
            try
            {
                var detail = Convert.ToBoolean(_procurementMasterRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM [TRN].[ProcurementMasterDetail] WHERE ProcurementMasterId='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                if (!detail)
                {
                    var data = base.Find(id);
                    if (data.IsNull()) throw new CustomException(ServiceResources.RecordNoLonger);
                    base.Delete(data);
                    _unitOfWork.SaveChanges();
                }
                else throw new CustomException("Please delete first line item.");
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }




        

        private void UpdateGraph(ProcurementMasterDetail receiveDetail)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> GetDataByProcurementMasterId()
        {
            try
            {
                var sql = @"SELECT 
                                   PM.Id 
                                   ,PM.PositionCode
                                  , PM.CompanyGroupId
                                  , PM.CompanyId
                                  , PM.PlantId
                                  , PM.EntityId
                                  , PM.ProcurementFrequency
                                  , PM.ProcurementDays
                                  ,PM.Remarks
                                  , PM.CostReductionCategory
                                  , PM.MaterialMasterId
                                  , PM.ArticleId
                                  , PM.ArticleCriticality
                                  , PM.FirstCharacteristicsId
                                  , PM.FirstCharacteristicsValueId
                                  , PM.SecondCharacteristicsId
                                  , PM.SecondCharacteristicsValueId
                                  , PM.ThirdCharacteristicsId
                                  , PM.ThirdCharacteristicsValueId
                                  , PM.MinStockLevel
                                  , PM.MaxStockLevel
                                  , PM.CostingPercentage
                                  , PM.ProcurementPercentage
                                  , PM.QualityApprovalReq
                                  , PM.QualityApprovedBy
                                  , PM.PossitionCodeForApproval
                                  ,PM.QualityStdSet
                                  , PM.SupplierQualityReportReq
                                  , PM.RequisitionType
                                  , PM.PriceApproval
                                  , PM.POGroupId
                                  , PM.Imported
                                  , PM.ImportedCurrencyId
                                  , PM.ImportedBaseRate
                                  , PM.ImportedTgtLandedRate
                                  , PM.ImportProcurementLedTimeDays
                                  , PM.ImportedMinimumOrderQty
                                  , PM.ImportedArticleLifeDays
                                  , PM.Local
                                  , PM.LocalCurrencyId
                                  , PM.LocalBaseRate
                                  , PM.LocalTgtLandedRate
                                  , PM.LocalProcurementLedTimeDays
                                  , PM.LocalMinimumOrderQty
                                  , PM.LocalArticleLifeDays
                                  , PM.AutoPoGeneration
                                  , PM.PoGenerationDay
                                  , PM.LastProcurementRate
                                  , PM.MinimumProcurementRate
                                  , PM.MaximumProcurementRate
	                              ,EN.UserName AS Entityname
	                              ,MM.UserName AS MaterialMasterName
                                    , MGM.UserName AS MaterialGroupMasterName
                                  ,MMA.StandardName As ArticleName,MM.UserName  AS BaseUoM,PM.ProcurementsPlanDay
                                          FROM [TRN].[ProcurementMaster] PM
                                        Left JOIn [ORG].CompanyGroup CG On CG.Id=PM.CompanyGroupId
                                        Left JOIn [ORG].Company COM On COM.Id=PM.CompanyId
                                        Left JOIn [ORG].Plant  Pl On PM.Id=PM.PlantId
                                        Left JOIn [ORG].[Entity] EN On EN.Id =PM.EntityId
                                        Left JOIn  [MST].[MaterialMaster] MM On MM.Id =PM.MaterialMasterId
                                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                                        Left JOIn [MST].[MaterialMasterArticle] MMA On MMA.Id =PM.ArticleId
                                        Left JOIn [TRN].FirstCharacteristics  FC On FC.Id=PM.FirstCharacteristicsId
                                        Left JOIn [HKP].[CharacteristicsValue] FCV On FCV.Id=PM.FirstCharacteristicsValueId
                                        Left JOIn [TRN].[SecondCharacteristics] SC On SC.Id=PM.SecondCharacteristicsId
                                        Left JOIn [HKP].[CharacteristicsValue] SCSV On SCSV.Id=PM.SecondCharacteristicsValueId
                                        Left JOIn [TRN].[ThirdCharacteristics] TC On TC.Id=PM.ThirdCharacteristicsId
                                        Left JOIn [HKP].[CharacteristicsValue] TCSV On PM.Id=PM.ThirdCharacteristicsValueId
                                        Left JOIn [TRN].PurchaseOrderGroup PO On PM.Id=PM.POGroupId
                                        Left JOIn  [SCS].[Currency] CRN On PM.Id=PM.POGroupId
                                        Left JOIn [TRN].PurchaseOrderGroup POG On PM.Id=PM.ImportedCurrencyId
                                        Left JOIn [SCS].[Currency] CRNI On PM.Id=PM.LocalCurrencyId
                                        JOIN [SCS].[UnitOfMeasurement] AS TUoM ON TUoM.Id= MM.BaseUOMId
                                        ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public object SqlQuery<T>(string v)
        {
            throw new NotImplementedException();
        }

        public object GetAutoSequence()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> GetMaterialTypeCbo()
        {
            try
            {

                var sql = @"Select Code +' - '+UserName As Text, Id As Value from [HKP].[MaterialType]";    
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }




        public IEnumerable<object> GetQualityStdCbo()
        {
            try
            {

                var sql = @"Select Id +' - '+UserName As Text, Id As Value from [TRN].[QualityStdSet]";
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        #region ProcureMentMaster Report 




        public IWorkbook CreateProcurementMasterReportSheet(string companyId, string plantId, string fromDate, string toDate, string Qty, string Amount)
        {
            try
            {
                var excelEngine = new ExcelEngine();
                var report = new Helpers.ReportUtility();
                var workbook = report.GetWorkbook(ref excelEngine, 2);
                var sheet1 = workbook.Worksheets[0];
                var sheet2 = workbook.Worksheets[1];
                var Head = "ProcureMent Master" ;
                CreateProcurementMasterReportSheet(ref sheet1, ref sheet2, report, Head, "Summary", companyId, plantId, fromDate, toDate, Qty, Amount);
                workbook.Version = ExcelVersion.Excel2016;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }



        private void CreateProcurementMasterReportSheet(ref IWorksheet sheet1, ref IWorksheet sheet2, ReportUtility report, string sheet1Name, string sheet2Name, string companyId, string plantId, string fromDate, string toDate, string Qty, string Amount)
        {
          

            var cmdText = @"SELECT 
                             PM.Id
	                        ,CG.UserName CompanyGroupName
	                        ,COM.UserName CompanyName
	                        ,Pl.UserName PlantName
	                        ,MM.UserName AS MaterialMasterName
	                        ,MMA.StandardName AS ArticleName
	                        ,MGM.UserName AS MaterialGroupMasterName
	                        ,MM.UserName AS BaseUoM
	                        ,PM.ProcurementFrequency
	                        ,PM.ProcurementDays
	                        ,PM.RequisitionType
	                        ,PM.MinStockLevel
	                        ,PM.MaxStockLevel
	                        ,PM.SupplierQualityReportReq
	                        ,PM.CostReductionCategory
	                        ,PM.ArticleCriticality
	                        ,PM.QualityStdSet
	                        ,PM.ProcurementsPlanDay
	                        ,PM.ProcurementPercentage
	                        ,PM.CostingPercentage
	                        ,PM.PositionCode
	                        ,PM.QualityApprovalReq
	                        ,PM.PriceApproval
	                        ,PM.Remarks
	                        ,POG.UserName ImportedCurrency
	                        ,PM.ImportedBaseRate
	                        ,PM.ImportedTgtLandedRate
	                        ,PM.ImportProcurementLedTimeDays
	                        ,PM.ImportedMinimumOrderQty
	                        ,PM.ImportedArticleLifeDays
	                        ,CRNI.Name LocalCurrency
	                        ,PM.LocalBaseRate
	                        ,PM.LocalTgtLandedRate
	                        ,PM.LocalProcurementLedTimeDays
	                        ,PM.LocalMinimumOrderQty
	                        ,PM.LocalArticleLifeDays
	                        ,p.UserName PartyName
	                        ,PMD.PartyBaseRate
	                        ,PMD.PartyPreference
	                     --,PM.ProcurementsPlanDay
                        --,FC.UserName FirstCharacteristics
                        --,FCV.UserName FirstCharacteristicsValue
                        --,SC.UserName SecondCharacteristics
                        -- ,SCSV.UserName SecondCharacteristicsValue
                        --,TC.UserName ThirdCharacteristics
                        --,TCSV.UserName ThirdCharacteristicsValue
                        --,PM.QualityApprovedBy
                        --,PM.PossitionCodeForApproval
                        --,PO.UserName POGroupName
                        --,PM.Imported
                        -- ,PM.LOCAL
                        --,PM.AutoPoGeneration
                        -- ,PM.PoGenerationDay
                        --,PM.LastProcurementRate
                        --,PM.MinimumProcurementRate
                        --,PM.MaximumProcurementRate
                        FROM [TRN].[ProcurementMaster] PM
                        LEFT JOIN [TRN].[ProcurementMasterDetail] PMD ON PMD.ProcurementMasterId = PM.Id
                        LEFT JOIN HKP.Party AS P ON P.Id = PMD.PartyId
                        LEFT JOIN [ORG].CompanyGroup CG ON CG.Id = PM.CompanyGroupId
                        LEFT JOIN [ORG].Company COM ON COM.Id = PM.CompanyId
                        LEFT JOIN [ORG].Plant Pl ON Pl.Id = PM.PlantId
                        LEFT JOIN [MST].[MaterialMaster] MM ON MM.Id = PM.MaterialMasterId
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                        LEFT JOIN [MST].[MaterialMasterArticle] MMA ON MMA.Id = PM.ArticleId
                        -- LEFT JOIN HKP.Characteristics AS FC ON PM.FirstCharacteristicsId = FC.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] FCV ON FCV.Id = PM.FirstCharacteristicsValueId
                        --  LEFT JOIN HKP.Characteristics AS SC ON PM.SecondCharacteristicsId = SC.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] SCSV ON SCSV.Id = PM.SecondCharacteristicsValueId
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON PM.ThirdCharacteristicsValueId = TCV.Id
                        --  LEFT JOIN HKP.Characteristics AS TC ON PM.ThirdCharacteristicsId = SC.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] TCSV ON PM.Id = PM.ThirdCharacteristicsValueId
                        LEFT JOIN [TRN].PurchaseOrderGroup PO ON PM.Id = PM.POGroupId
                        LEFT JOIN [SCS].[Currency] CRN ON PM.Id = PM.POGroupId
                        LEFT JOIN [TRN].PurchaseOrderGroup POG ON PM.Id = PM.ImportedCurrencyId
                        LEFT JOIN [SCS].[Currency] CRNI ON CRNI.Id = PM.LocalCurrencyId
                        JOIN [SCS].[UnitOfMeasurement] AS TUoM ON TUoM.Id = MM.BaseUOMId ";

            var inventoryMaterialList = _sqlRepository.GetDataTable(cmdText);
            var plantName = new DataView(_sqlRepository.GetDataTable(@"SELECT UserName from org.Plant WHERE Id='" + plantId + "'")).ToTable(true, "UserName").Rows[0]["UserName"].ToString();
        

            if (inventoryMaterialList.Rows.Count == 0)
                throw new Exception("No Data Found !!!");

            var _row = 5;
            
          
            var _rowL = _row;
            var row = _row + 1;


            var sheet1headreColIndex = 1;
            //var sheet2headreColIndex = 1;
            _rowL += 1;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Procurement Master Id");
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Company Group Name");
            sheet1headreColIndex++;


            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Company Name");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Plant Name");
            sheet1headreColIndex++;


            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Master Name");
            sheet1headreColIndex++;




            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article Name");
            sheet1headreColIndex++;
          
           
        


            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material GroupMaster Name");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Base UoM");
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Procurement Frequency");
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Procurement Days");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Requisition Type");
            sheet1headreColIndex++;




            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Minimum Stock Level");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Maximum Stock Level");
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Supplier Quality Report Req");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Cost Reduction Category");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article Criticality");
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Quality Standard Set");
            sheet1headreColIndex++;

         
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Procurements Plan Day");
            sheet1headreColIndex++;
        
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Procurement (%)");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Costing (%)");
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Position Code");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Quality Approval Req");
            sheet1headreColIndex++;





            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Price Approval");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Remarks");
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Imported Currency");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Imported Base Rate");
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Imported Tgt Landed Rate");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Import Procurement Led Time Days");
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Imported Minimum Order Qty");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Imported Article Life Days");
            sheet1headreColIndex++;

          

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Local Currency");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Local Base Rate");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Local Tgt Landed Rate");
            sheet1headreColIndex++;



            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Local Procurement Led Time Days");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Local Minimum Order Qty");
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Local Article Life Days");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Party Name");
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Party Base Rate");
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Party Preference");
            //sheet1headreColIndex++;
            

            var Row_Total_Start = _rowL + 1;
            for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
            {
                _rowL++;
                report.SetText(ref sheet1, _rowL, 1, inventoryMaterialList.Rows[n]["Id"].ToString());
                report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["CompanyGroupName"].ToString());
                report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["CompanyName"].ToString());
                report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["PlantName"].ToString());
                report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
                report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
                report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["MaterialGroupMasterName"].ToString());
                report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["BaseUoM"].ToString());
                report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["ProcurementFrequency"].ToString());
                report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["ProcurementDays"].ToString());
                report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["RequisitionType"].ToString());
                report.SetText(ref sheet1, _rowL, 12, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MinStockLevel"].ToString()));
                report.SetText(ref sheet1, _rowL, 13, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MaxStockLevel"].ToString()));
                report.SetText(ref sheet1, _rowL, 14, inventoryMaterialList.Rows[n]["SupplierQualityReportReq"].ToString());
                report.SetText(ref sheet1, _rowL, 15, inventoryMaterialList.Rows[n]["CostReductionCategory"].ToString());
                report.SetText(ref sheet1, _rowL, 16, inventoryMaterialList.Rows[n]["ArticleCriticality"].ToString());
                report.SetText(ref sheet1, _rowL, 17, inventoryMaterialList.Rows[n]["QualityStdSet"].ToString());
                report.SetText(ref sheet1, _rowL, 18, inventoryMaterialList.Rows[n]["ProcurementsPlanDay"].ToString());
                report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ProcurementPercentage"].ToString()));
                report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["CostingPercentage"].ToString()));
                report.SetText(ref sheet1, _rowL, 21, inventoryMaterialList.Rows[n]["PositionCode"].ToString());
                report.SetText(ref sheet1, _rowL, 22, inventoryMaterialList.Rows[n]["QualityApprovalReq"].ToString());
                report.SetText(ref sheet1, _rowL, 23, inventoryMaterialList.Rows[n]["PriceApproval"].ToString());
                report.SetText(ref sheet1, _rowL, 24, inventoryMaterialList.Rows[n]["Remarks"].ToString());
                report.SetText(ref sheet1, _rowL, 25, inventoryMaterialList.Rows[n]["ImportedCurrency"].ToString());
                report.SetText(ref sheet1, _rowL, 26, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ImportedBaseRate"].ToString()));
                report.SetText(ref sheet1, _rowL, 27, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ImportedTgtLandedRate"].ToString()));
                report.SetText(ref sheet1, _rowL, 28, inventoryMaterialList.Rows[n]["ImportProcurementLedTimeDays"].ToString());
                report.SetText(ref sheet1, _rowL, 29, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ImportedMinimumOrderQty"].ToString()));
                report.SetText(ref sheet1, _rowL, 30, inventoryMaterialList.Rows[n]["ImportedArticleLifeDays"].ToString());
                report.SetText(ref sheet1, _rowL, 31, inventoryMaterialList.Rows[n]["LocalCurrency"].ToString());
                report.SetText(ref sheet1, _rowL, 32, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["LocalBaseRate"].ToString()));
                report.SetText(ref sheet1, _rowL, 33, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["LocalTgtLandedRate"].ToString()));
                report.SetText(ref sheet1, _rowL, 34, inventoryMaterialList.Rows[n]["LocalProcurementLedTimeDays"].ToString());
                report.SetText(ref sheet1, _rowL, 35, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["LocalMinimumOrderQty"].ToString()));
                report.SetText(ref sheet1, _rowL, 36, inventoryMaterialList.Rows[n]["LocalArticleLifeDays"].ToString());
                report.SetText(ref sheet1, _rowL, 37, inventoryMaterialList.Rows[n]["PartyName"].ToString());
                report.SetText(ref sheet1, _rowL, 38, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PartyBaseRate"].ToString()));
                report.SetText(ref sheet1, _rowL, 39, inventoryMaterialList.Rows[n]["PartyPreference"].ToString());
           


           

            }


            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            //  sheet2.Range[(row), 1, _rowL, sheet2headreColIndex].BorderAround(ExcelLineStyle.Hair);


            _rowL++;
           

            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);

           

            sheet1.Name = sheet1Name;
            sheet1.UsedRange.WrapText = true;
            sheet1.UsedRange.CellStyle.Font.Size = 8;
            report.PlantHeader(ref sheet1, sheet1headreColIndex, sheet1Name, plantId);
            report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);

            //sheet2.Name = sheet2Name;
            //sheet2.UsedRange.WrapText = true;
            //sheet2.UsedRange.CellStyle.Font.Size = 8;
            //report.CompanyPlantHeader(ref sheet2, sheet2headreColIndex, sheet2Name, companyId, plantName, null);
            //report.PageSetup(ref sheet2, 5, ExcelPageOrientation.Landscape);
        }

        #endregion Material Stock Ledeger 

    }
}