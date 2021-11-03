#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Inventory;
using Library.Model.OrderManagements;
using Library.Model.Productions;
using Library.Model.Products;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Products
{
    public class PurchaseOrderGroupMasterService : Service<PurchaseOrderGroup>, IPurchaseOrderGroupMasterService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<PurchaseOrderGroup> _purchaseOrderGroupMaster;
        private readonly IRepositoryAsync<PurchaseOrderGroupDetails> _purchaseOrderGroupDetails;
        private readonly IUnitOfWork _unitOfWork;

        public PurchaseOrderGroupMasterService(
            IRepositoryAsync<PurchaseOrderGroup> purchaseOrderGroupMaster
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IRepositoryAsync<PurchaseOrderGroupDetails> purchaseOrderGroupDetails
            ) : base(purchaseOrderGroupMaster, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
            _purchaseOrderGroupMaster = purchaseOrderGroupMaster;
            _purchaseOrderGroupDetails = purchaseOrderGroupDetails;

        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(PurchaseOrderGroup), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        private void Check(PurchaseOrderGroup entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName);
            CheckUniqueColumn(UniqueColumnName.StandardName, entity.StandardName, r => r.Id != entity.Id && r.StandardName == entity.StandardName);
            CheckUniqueColumn(UniqueColumnName.ShortName, entity.ShortName, r => r.Id != entity.Id && r.ShortName == entity.ShortName);
       
        }


        public override void Insert(PurchaseOrderGroup entity)
        {
            try
            {
                Check(entity);
                

                entity.Id = GetPK();
                AuditService.AddedLog(entity);
                entity.ModelState = ModelState.Added;
                _purchaseOrderGroupMaster.Insert(entity);
                _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public override void Update(PurchaseOrderGroup entity)
        {
            try
            {
                
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                entity.CompanyGroupId = identity.CompanyGroupId;
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public void DeletePOG(string id)
        {
            try
            {
                var detail = Convert.ToBoolean(_purchaseOrderGroupMaster.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM [TRN].[PurchaseOrderGroupDetails] WHERE Id='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
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




        public decimal GetAutoSequence()
        {
            try
            {
                return base.Query().Select().Max(r => r.Sequence + 1);
            }
            catch
            {
                return 1.00M;
            }
        }


        public IEnumerable<object> GetPurchaseOrderGroupGridData()
        {
            try
            {
                var sql = @"SELECT       POG.Id
	                                    ,POG.CompanyGroupId
	                                    ,POG.Sequence
	                                    ,POG.Code 
	                                    ,POG.UserName 
	                                    ,POG.ShortName
	                                    ,POG.StandardName
	                                    ,POG.UserName As PartyName
                                        ,POG.ResponsiblePersonName
	                                    ,POG.Description
	                                    ,POG.Remarks
	                                    ,POG.Active
	                                    ,POG.AddedBy
                                       FROM TRN.PurchaseOrderGroup POG";


                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

     


        public IEnumerable<object> GetAllPurchaseOrderGroupDetails(string Id)//string ReqDetailId
        {
            try
            {
                //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var _sql = @"select
                                 POGD.Id
                                 ,MGM.UserName As MateralMasterGroupName
                                 ,MM.Id AS MaterialMasterId
                                ,MM.UserName as MaterialMasterName
                                ,POGD.ArticleId
                                --,POGD.PartyPreference
                                ,EI.FirstName  ResponsiblePerson 
                               -- ,EIC.EmployeeName  EmployeeCode 
	                            ,ART.StandardName
	                           -- ,Pr.UserName As PartyName
	                            ,POGD.FirstCharacteristicsId
	                            ,FC.UserName AS FirstCharacteristics
	                            ,POGD.FirstCharacteristicsValueId
	                            ,FCV.UserName AS FirstCharacteristicsValue
	                            ,POGD.SecondCharacteristicsId
	                            ,SC.UserName AS SecondCharacteristics
	                            ,POGD.SecondCharacteristicsValueId
	                            ,SCV.UserName AS SecondCharacteristicsValue
	                            ,POGD.ThirdCharacteristicsId
	                            ,TC.UserName AS ThirdCharacteristics
	                            ,POGD.ThirdCharacteristicsValueId
	                            ,TCV.UserName AS ThirdCharacteristicsValue
                             FROM 
                            TRn.PurchaseOrderGroupDetails POGD
                            Left JOIn TRn.PurchaseOrderGroup POG ON POG.Id=POGD.PurchaseOrderGroupId
                            Left JOin mst.MaterialMaster MM ON MM.Id=POGD.MaterialMasterId
                            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                            LEFT JOIN MST.MaterialMasterArticle  ART ON ART.Id= POGD.ArticleId
                            LEFT JOIN HKP.Characteristics AS FC ON POGD.FirstCharacteristicsId = FC.Id
                            LEFT JOIN HKP.Characteristics AS SC ON POGD.SecondCharacteristicsId = SC.Id
                            LEFT JOIN HKP.Characteristics AS TC ON POGD.ThirdCharacteristicsId = TC.Id
                            LEFT JOIN HKP.CharacteristicsValue AS FCV ON POGD.FirstCharacteristicsValueId = FCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS SCV ON POGD.SecondCharacteristicsValueId = SCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS TCV ON POGD.ThirdCharacteristicsValueId = TCV.Id
                            LEFT JOIN EmployeeInformation AS EIC ON EIC.SystemId=POGD.EmployeeCode
                            LEFT JOIN EmployeeInformation AS EI ON EI.SystemId=POGD.ResponsiblePerson
                            --LEFT Join [HKP].[Party] As Pr ON POGD.PartyId=Pr.Id
	                 
                           Where POG.Id ='" + Id + "' ";
                return _sqlRepository.GetDataCollection(_sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }






        public IEnumerable<object> GetAllPOGVendor(string Id)//string ReqDetailId
        {
            try
            {
                //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var _sql = @"Select 

                                 POGV.Id
                                ,POGV.PartyPreference
                                ,Pr.UserName As PartyName
                                from trn.POGVendor As POGV
                                Left Join TRN.PurchaseOrderGroup POG ON  POG.Id=POGV.PurchaseOrderGroupId
                                 LEFT Join [HKP].[Party] As Pr ON Pr.Id=POGV.PartyId
                                                           Where POG.Id ='" + Id + "' ";
                return _sqlRepository.GetDataCollection(_sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }







        public IEnumerable<object> GetAllReqdata1()
        {
            throw new NotImplementedException();
        }


        public object SqlQuery<T>(string v)
        {
            throw new NotImplementedException();
        }



        public void UpdateMaterial(IEnumerable<PurchaseOrderGroupDetails> entity, IEnumerable<PurchaseOrderTax> receiveTaxList)
        {
            try
            {
                

                if (entity.IsNotNull())
                {
                    // var currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId='{entity.InventoryReceiveDetailId}'").First();
                    foreach (var item1 in entity)
                    {
                        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                        var ip = identity.IPAddress;
                        var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                        var UpdatedBy = identity.Name;
                        var ReqDetailId = item1.Id;
                       
                        var _sql = "UPDATE [TRN].[PurchaseOrderGroupDetails] SET [TransactionQty] =  '" + Convert.ToDecimal(item1.TransactionQty) + "',[EstimatedRate] = '" + Convert.ToDecimal(item1.EstimatedRate) + "',[TotalAmount] = '" + Convert.ToDecimal(item1.TotalAmount) + "',[UpdatedBy] = '" + identity.UserId + "',[UpdatedDate] = '" + Convert.ToDateTime(DateTime.Now) + "',[UpdatedFromIP] = '" + identity.IPAddress + "' where id = '" + ReqDetailId + "'";
                        _sqlRepository.ExecuteSqlCommand(_sql);
                    }
                }
               
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public void DeleteReqDetails(string id)
        {
            try
            {
                //var detail = Convert.ToBoolean(_inventoryReceiveRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM TRN.MaterialRequsitionDetails WHERE Id='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                ////var service = Convert.ToBoolean(_inventoryReceiveRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM TRN.InventoryService WHERE InventoryReceiveId='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                //if (!detail)
                //{

                var data = _purchaseOrderGroupDetails.Find(id);
                if (data.IsNull()) throw new CustomException(ServiceResources.RecordNoLonger);
                _purchaseOrderGroupDetails.Delete(data.Id);
                _unitOfWork.SaveChanges();
                //}
                //else throw new CustomException("Please delete first line item.");
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }


        

        

        public decimal GetToCurrencyRate(string currencyId, string baseCurrencyId, DateTime docDate, string companyId)
        {
            try
            {
                decimal toCurrencyRate = 0;
                if (currencyId != baseCurrencyId)
                {
                    var sql = @"SELECT ISNULL((SELECT TOP(1) ISNULL(A.ToCurrencyBankSelling,0) FROM SCS.ExchangeRate AS A WHERE
                                            FromCurrencyCode='" + currencyId + "'   AND A.CompanyId='" + companyId + "' ORDER BY CAST(FromDate AS DATE) DESC), 0)";
                    toCurrencyRate = _purchaseOrderGroupDetails.SqlQuery<decimal>(sql).First();
                }
                return toCurrencyRate;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                 ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }




        public IEnumerable<object> GetVendorCbo(string partyId, string Id)
        {
            try
            {
                var sql = @"Select 

                                POGV.Id
                                ,Pr.UserName As PartyName

                                from trn.POGVendor As POGV
                                Left Join TRN.PurchaseOrderGroup POG ON  POG.Id=POGV.PurchaseOrderGroupId
                                 LEFT Join [HKP].[Party] As Pr ON Pr.Id=POGV.PartyId
                                 Where POG.Id ='" + Id + "' ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }



        #region Purchase Order Group Report




        public IWorkbook CreatePurchaseOrderGroupReportSheet(string companyId, string plantId)
        {
            try
            {
                var excelEngine = new ExcelEngine();
                var report = new Helpers.ReportUtility();
                var workbook = report.GetWorkbook(ref excelEngine, 2);
                var sheet1 = workbook.Worksheets[0];
                var sheet2 = workbook.Worksheets[1];
                var Head = "Purchase Order Group " ;
                CreatePurchaseOrderGroupReportSheet(ref sheet1, ref sheet2, report, Head, "Summary", companyId, plantId);
                workbook.Version = ExcelVersion.Excel2016;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }



        private void CreatePurchaseOrderGroupReportSheet(ref IWorksheet sheet1, ref IWorksheet sheet2, ReportUtility report, string sheet1Name, string sheet2Name, string companyId, string plantId)
        {
                                var cmdText = @"SELECT 	                        CG.UserName AS CompanyGroup		                    ,C.UserName AS Company		                    ,Plt.UserName AS Plant	                        ,POG.Sequence	                        ,POG.Code 	                        ,POG.UserName AS POGroupName	                        ,POG.ShortName	                        ,POG.StandardName		                    ,MT.UserName MaterialType		                    ,MGM.UserName AS MaterialGroupMasterName				                    ,MM.UserName MaterialMasterName						                    , ART.StandardName ArticleName		                    ,IsAsset=CASE WHEN MM.IsAsset=0 then 'No' else 'Yes' END								                    , ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue								                    , ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue								                    , ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 	  		                    ,p.UserName As PartyName                            ,POG.ResponsiblePersonName	                        ,POG.Description	                        ,POG.Remarks	  	                        ,POG.AddedBy AS PreparedBy	                        ,IsActive=CASE WHEN POG.Active=1 THEN 'Yes' Else 'No' END	                      FROM TRN.PurchaseOrderGroupDetails As POGD                    LEFT JOIN TRN.PurchaseOrderGroup As POG ON POG.Id=POGD.PurchaseOrderGroupId                    LEFT JOIN MST.MaterialMaster AS MM ON POGD.MaterialMasterId = MM.Id                    LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id                    LEFT JOIN MST.MaterialMasterArticle AS ART ON POGD.ArticleId = ART.Id                    LEFT JOIN HKP.Characteristics AS FC ON POGD.FirstCharacteristicsId = FC.Id                    LEFT JOIN HKP.Characteristics AS SC ON POGD.SecondCharacteristicsId = SC.Id                    LEFT JOIN HKP.Characteristics AS TC ON POGD.ThirdCharacteristicsId = TC.Id                    LEFT JOIN HKP.CharacteristicsValue AS FCV ON POGD.FirstCharacteristicsValueId = FCV.Id                    LEFT JOIN HKP.CharacteristicsValue AS SCV ON POGD.SecondCharacteristicsValueId = SCV.Id                    LEFT JOIN HKP.CharacteristicsValue AS TCV ON POGD.ThirdCharacteristicsValueId = TCV.Id                    LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id                    LEFT JOIN trn.POGVendor As POGV ON POGV.PurchaseOrderGroupId=POG.id                    LEFT JOIN hkp.Party AS P On P.Id=POGV.PartyId                    LEFT JOIN org.CompanyGroup AS CG on CG.id=POG.CompanyGroupId                    LEFT JOIN org.Company AS C on C.id=POG.CompanyId                    LEFT JOIN org.Plant AS Plt on Plt.id=POG.PlantId                    where POG.Active=1";
            var inventoryMaterialList = _sqlRepository.GetDataTable(cmdText);
            var plantName = new DataView(_sqlRepository.GetDataTable(@"SELECT UserName from org.Plant WHERE Id='" + plantId + "'")).ToTable(true, "UserName").Rows[0]["UserName"].ToString();
            //var cmdSText = @"SELECT A.Id, A.InventoryReceiveId, A.ServiceMasterId, B.UserName AS ServiceMasterName, A.Amount, A.TotalTaxAmount
            //                FROM [TRN].[InventoryService] AS A JOIN [HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id WHERE A.InventoryReceiveId='" + inventoryReceiveId + "'";
            //var inventoryServiceList = _sqlRepository.GetDataTable(cmdSText);
            //var empId = inventoryMaterialList.Rows[0]["EmployeeId"].ToString();

            if (inventoryMaterialList.Rows.Count == 0)
                throw new Exception("No Data Found !!!");

            var _row = 5;
            //if (!string.IsNullOrEmpty(empId))
            //{
            //    report.SetMasterHeaderText(ref sheet1, _row, 1, "Employee Code");
            //    report.SetMasterHeaderText(ref sheet2, _row, 1, "Employee Code");
            //    report.SetText(ref sheet1, _row, 2, inventoryMaterialList.Rows[0]["EmployeeCode"].ToString());
            //    report.SetText(ref sheet2, _row, 2, inventoryMaterialList.Rows[0]["EmployeeCode"].ToString());
            //    sheet1.Range[_row, 2, _row, 2].Merge();
            //    sheet2.Range[_row, 2, _row, 3].Merge();
            //    _row++;
            //}
            //report.SetMasterHeaderText(ref sheet1, _row, 1, "Vendor");
            //report.SetMasterHeaderText(ref sheet2, _row, 1, "Vendor");
            //report.SetText(ref sheet1, _row, 2, inventoryMaterialList.Rows[0]["Vendor"].ToString());
            //report.SetText(ref sheet2, _row, 2, inventoryMaterialList.Rows[0]["Vendor"].ToString());
            //sheet1.Range[_row, 2, _row, 2].Merge();
            //sheet2.Range[_row, 2, _row, 3].Merge();
            //_row++;

            //report.SetMasterHeaderText(ref sheet1, _row, 1, "Invoicing By");
            //report.SetMasterHeaderText(ref sheet2, _row, 1, "Invoicing By");
            //report.SetText(ref sheet1, _row, 2, inventoryMaterialList.Rows[0]["InvoicingBy"].ToString());
            //report.SetText(ref sheet2, _row, 2, inventoryMaterialList.Rows[0]["InvoicingBy"].ToString());
            //sheet1.Range[_row, 2, _row, 2].Merge();
            //sheet2.Range[_row, 2, _row, 3].Merge();
            //_row++;

            //report.SetMasterHeaderText(ref sheet1, _row, 1, "Delivery By");
            //report.SetMasterHeaderText(ref sheet2, _row, 1, "Delivery By");
            //report.SetText(ref sheet1, _row, 2, inventoryMaterialList.Rows[0]["DeliveryBy"].ToString());
            //report.SetText(ref sheet2, _row, 2, inventoryMaterialList.Rows[0]["DeliveryBy"].ToString());
            //sheet1.Range[_row, 2, _row, 2].Merge();
            //sheet2.Range[_row, 2, _row, 3].Merge();
            //_row++;

            //report.SetMasterHeaderText(ref sheet1, _row, 1, "Vendor Doc. RefNo");
            //report.SetMasterHeaderText(ref sheet2, _row, 1, "Vendor Doc. RefNo");
            //report.SetText(ref sheet1, _row, 2, inventoryMaterialList.Rows[0]["DocRefNo"].ToString());
            //report.SetText(ref sheet2, _row, 2, inventoryMaterialList.Rows[0]["DocRefNo"].ToString());
            //sheet1.Range[_row, 2, _row, 2].Merge();
            //sheet2.Range[_row, 2, _row, 3].Merge();
            //_row++;

            //report.SetMasterHeaderText(ref sheet1, _row, 1, "Gate Entry No");
            //report.SetMasterHeaderText(ref sheet2, _row, 1, "Gate Entry No");
            //report.SetText(ref sheet1, _row, 2, inventoryMaterialList.Rows[0]["GateEntryNo"].ToString());
            //report.SetText(ref sheet2, _row, 2, inventoryMaterialList.Rows[0]["GateEntryNo"].ToString());
            //sheet1.Range[_row, 2, _row, 2].Merge();
            //sheet2.Range[_row, 2, _row, 3].Merge();
            //_row++;

            //report.SetMasterHeaderText(ref sheet1, _row, 1, "GRN No");
            //report.SetMasterHeaderText(ref sheet2, _row, 1, "GRN No");
            //report.SetText(ref sheet1, _row, 2, inventoryMaterialList.Rows[0]["GRNNo"].ToString());
            //report.SetText(ref sheet2, _row, 2, inventoryMaterialList.Rows[0]["GRNNo"].ToString());
            //sheet1.Range[_row, 2, _row, 2].Merge();
            //sheet2.Range[_row, 2, _row, 3].Merge();
            //_row++;

            //report.SetMasterHeaderText(ref sheet1, _row, 1, "Storage Location");
            //report.SetMasterHeaderText(ref sheet2, _row, 1, "Storage Location");
            //report.SetText(ref sheet1, _row, 2, inventoryMaterialList.Rows[0]["StorageLocation"].ToString());
            //report.SetText(ref sheet2, _row, 2, inventoryMaterialList.Rows[0]["StorageLocation"].ToString());
            //sheet1.Range[_row, 2, _row, 2].Merge();
            //sheet2.Range[_row, 2, _row, 3].Merge();
            //_row++;
            var _rowL = _row;
            var row = _row + 1;
            //var _rowR = 5;
            //if (!string.IsNullOrEmpty(empId))
            //{
            //    report.SetMasterHeaderText(ref sheet1, _rowR, 3, "Employee Name");
            //    report.SetMasterHeaderText(ref sheet2, _rowR, 4, "Employee Name");
            //    report.SetText(ref sheet1, _rowR, 5, inventoryMaterialList.Rows[0]["EmployeeName"].ToString());
            //    report.SetText(ref sheet2, _rowR, 5, inventoryMaterialList.Rows[0]["EmployeeName"].ToString());
            //    sheet1.Range[_rowR, 4, _rowR, 5].Merge();
            //    sheet2.Range[_rowR, 5, _rowR, 8].Merge();
            //    _rowR++;
            //}

            //report.SetMasterHeaderText(ref sheet1, _rowR, 3, "Invoice No");
            //report.SetMasterHeaderText(ref sheet2, _rowR, 4, "Invoice No");
            //report.SetText(ref sheet1, _rowR, 5, inventoryMaterialList.Rows[0]["InvoiceNo"].ToString());
            //report.SetText(ref sheet2, _rowR, 5, inventoryMaterialList.Rows[0]["InvoiceNo"].ToString());
            //sheet1.Range[_rowR, 4, _rowR, 5].Merge();
            //sheet2.Range[_rowR, 5, _rowR, 8].Merge();
            //_rowR++;

            //report.SetMasterHeaderText(ref sheet1, _rowR, 3, "Creditable");
            //report.SetMasterHeaderText(ref sheet2, _rowR, 4, "Creditable");
            //report.SetText(ref sheet1, _rowR, 5, inventoryMaterialList.Rows[0]["Creditable"].ToString());
            //report.SetText(ref sheet2, _rowR, 5, inventoryMaterialList.Rows[0]["Creditable"].ToString());
            //sheet1.Range[_rowR, 4, _rowR, 5].Merge();
            //sheet2.Range[_rowR, 5, _rowR, 8].Merge();
            //_rowR++;


            //report.SetMasterHeaderText(ref sheet1, _rowR, 3, "A/C Group");
            //report.SetMasterHeaderText(ref sheet2, _rowR, 4, "A/C Group");
            //report.SetText(ref sheet1, _rowR, 5, inventoryMaterialList.Rows[0]["PartyAccountGroupName"].ToString());
            //report.SetText(ref sheet2, _rowR, 5, inventoryMaterialList.Rows[0]["PartyAccountGroupName"].ToString());
            //sheet1.Range[_rowR, 4, _rowR, 5].Merge();
            //sheet2.Range[_rowR, 5, _rowR, 8].Merge();
            //_rowR++;

            //report.SetMasterHeaderText(ref sheet1, _rowR, 3, "Invoicing By Address");
            //report.SetMasterHeaderText(ref sheet2, _rowR, 4, "Invoicing By Address");
            //report.SetText(ref sheet1, _rowR, 5, inventoryMaterialList.Rows[0]["InvoicingByAddress"].ToString());
            //report.SetText(ref sheet2, _rowR, 5, inventoryMaterialList.Rows[0]["InvoicingByAddress"].ToString());
            //sheet1.Range[_rowR, 4, _rowR, 5].Merge();
            //sheet2.Range[_rowR, 5, _rowR, 8].Merge();
            //_rowR++;

            //report.SetMasterHeaderText(ref sheet1, _rowR, 3, "Delivery By Address");
            //report.SetMasterHeaderText(ref sheet2, _rowR, 4, "Delivery By Address");
            //report.SetText(ref sheet1, _rowR, 5, inventoryMaterialList.Rows[0]["DeliveryByAddress"].ToString());
            //report.SetText(ref sheet2, _rowR, 5, inventoryMaterialList.Rows[0]["DeliveryByAddress"].ToString());
            //sheet1.Range[_rowR, 4, _rowR, 5].Merge();
            //sheet2.Range[_rowR, 5, _rowR, 8].Merge();
            //_rowR++;

            //report.SetMasterHeaderText(ref sheet1, _rowR, 3, "Doc Date");
            //report.SetMasterHeaderText(ref sheet2, _rowR, 4, "Doc Date");
            //report.SetText(ref sheet1, _rowR, 5, inventoryMaterialList.Rows[0]["DocDate"].ToString());
            //report.SetText(ref sheet2, _rowR, 5, inventoryMaterialList.Rows[0]["DocDate"].ToString());
            //sheet1.Range[_rowR, 4, _rowR, 5].Merge();
            //sheet2.Range[_rowR, 5, _rowR, 8].Merge();
            //_rowR++;

            //report.SetMasterHeaderText(ref sheet1, _rowR, 3, "Entry Date");
            //report.SetMasterHeaderText(ref sheet2, _rowR, 4, "Entry Date");
            //report.SetText(ref sheet1, _rowR, 5, inventoryMaterialList.Rows[0]["EntryDate"].ToString());
            //report.SetText(ref sheet2, _rowR, 5, inventoryMaterialList.Rows[0]["EntryDate"].ToString());
            //sheet1.Range[_rowR, 4, _rowR, 5].Merge();
            //sheet2.Range[_rowR, 5, _rowR, 8].Merge();

            //var _rowRN = 5;

            //report.SetMasterHeaderText(ref sheet1, _rowRN, 6, "GRND Date");
            //report.SetMasterHeaderText(ref sheet2, _rowRN, 9, "GRND Date");
            //report.SetText(ref sheet1, _rowRN, 7, inventoryMaterialList.Rows[0]["GRNDate"].ToString());
            //report.SetText(ref sheet2, _rowRN, 10, inventoryMaterialList.Rows[0]["GRNDate"].ToString());
            //_rowRN++;

            //report.SetMasterHeaderText(ref sheet1, _rowRN, 6, "Invoice Date");
            //report.SetMasterHeaderText(ref sheet2, _rowRN, 9, "Invoice Date");
            //report.SetText(ref sheet1, _rowRN, 7, inventoryMaterialList.Rows[0]["InvoiceDate"].ToString());
            //report.SetText(ref sheet2, _rowRN, 10, inventoryMaterialList.Rows[0]["InvoiceDate"].ToString());
            //_rowRN++;

            //report.SetMasterHeaderText(ref sheet1, _rowRN, 6, "Currency");
            //report.SetMasterHeaderText(ref sheet2, _rowRN, 9, "Currency");
            //report.SetText(ref sheet1, _rowRN, 7, inventoryMaterialList.Rows[0]["CurrencyName"].ToString());
            //report.SetText(ref sheet2, _rowRN, 10, inventoryMaterialList.Rows[0]["CurrencyName"].ToString());
            //_rowRN++;

            //report.SetMasterHeaderText(ref sheet1, _rowRN, 6, "Total Amount (BC)");
            //report.SetMasterHeaderText(ref sheet2, _rowRN, 9, "Total Amount (BC)");

            //report.SetText(ref sheet1, _rowRN, 7, Convert.ToDouble(inventoryMaterialList.Rows[0]["BaseAmountTotal"].ToString()), ExcelHAlign.HAlignLeft);
            //report.SetText(ref sheet2, _rowRN, 10, Convert.ToDouble(inventoryMaterialList.Rows[0]["BaseAmountTotal"].ToString()), ExcelHAlign.HAlignLeft);
            //_rowRN++;

            //report.SetMasterHeaderText(ref sheet1, _rowRN, 6, "Total Tax (BC)");
            //report.SetMasterHeaderText(ref sheet2, _rowRN, 9, "Total Tax (BC)");

            //report.SetText(ref sheet1, _rowRN, 7, Convert.ToDouble(inventoryMaterialList.Rows[0]["BaseTaxAmountTotal"].ToString()), ExcelHAlign.HAlignLeft);
            //report.SetText(ref sheet2, _rowRN, 10, Convert.ToDouble(inventoryMaterialList.Rows[0]["BaseTaxAmountTotal"].ToString()), ExcelHAlign.HAlignLeft);
            //_rowRN++;

            //report.SetMasterHeaderText(ref sheet1, _rowRN, 6, "Total Charges (BC)");
            //report.SetMasterHeaderText(ref sheet2, _rowRN, 9, "Total Charges (BC)");

            //report.SetText(ref sheet1, _rowRN, 7, Convert.ToDouble(inventoryMaterialList.Rows[0]["ChargesAmountTotal"].ToString()), ExcelHAlign.HAlignLeft);
            //report.SetText(ref sheet2, _rowRN, 10, Convert.ToDouble(inventoryMaterialList.Rows[0]["ChargesAmountTotal"].ToString()), ExcelHAlign.HAlignLeft);
            //_rowRN++;

            //report.SetMasterHeaderText(ref sheet1, _rowRN, 6, "Total Charges Tax(BC)");
            //report.SetMasterHeaderText(ref sheet2, _rowRN, 9, "Total Charges Tax(BC)");

            //report.SetText(ref sheet1, _rowRN, 7, Convert.ToDouble(inventoryMaterialList.Rows[0]["TotalSvcTaxAmount"].ToString()), ExcelHAlign.HAlignLeft);
            //report.SetText(ref sheet2, _rowRN, 10, Convert.ToDouble(inventoryMaterialList.Rows[0]["TotalSvcTaxAmount"].ToString()), ExcelHAlign.HAlignLeft);
            //_rowRN++;

            //report.SetMasterHeaderText(ref sheet1, _rowRN, 6, "Gross Total");
            //report.SetMasterHeaderText(ref sheet2, _rowRN, 9, "Gross Total");

            //report.SetText(ref sheet1, _rowRN, 7, Convert.ToDouble(inventoryMaterialList.Rows[0]["GrossTotal"].ToString()), ExcelHAlign.HAlignLeft);
            //report.SetText(ref sheet2, _rowRN, 10, Convert.ToDouble(inventoryMaterialList.Rows[0]["GrossTotal"].ToString()), ExcelHAlign.HAlignLeft);
            //_rowRN++;

            var sheet1headreColIndex = 1;
            //var sheet2headreColIndex = 1;
            _rowL += 1;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Company Group");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Company");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Plant");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Sequence");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Code");
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "PO Group Name");
            sheet1headreColIndex++;
            
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Short Name");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Standard Name");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Type");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Group ");
            sheet1headreColIndex++; 
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material ");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article ");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "PartyName");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Responsible Person Name");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Description");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Remarks");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Prepared By");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "IsActive");
            sheet1headreColIndex++;
           
       



            var Row_Total_Start = _rowL + 1;
            for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
            {
                _rowL++;
                report.SetText(ref sheet1, _rowL, 1, inventoryMaterialList.Rows[n]["CompanyGroup"].ToString());
                report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["Company"].ToString());
                report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["Plant"].ToString());
                report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["Sequence"].ToString());
                report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["Code"].ToString());
                report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["POGroupName"].ToString());
                report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["ShortName"].ToString());
                report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["StandardName"].ToString());
                report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["MaterialType"].ToString());
                report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["MaterialGroupMasterName"].ToString());
                report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
                report.SetText(ref sheet1, _rowL, 12, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
                report.SetText(ref sheet1, _rowL, 13, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, _rowL, 14, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, _rowL, 15, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, _rowL, 16, inventoryMaterialList.Rows[n]["PartyName"].ToString());
                report.SetText(ref sheet1, _rowL, 17, inventoryMaterialList.Rows[n]["ResponsiblePersonName"].ToString());
                report.SetText(ref sheet1, _rowL, 18, inventoryMaterialList.Rows[n]["Description"].ToString());
                report.SetText(ref sheet1, _rowL, 19, inventoryMaterialList.Rows[n]["Remarks"].ToString());
                report.SetText(ref sheet1, _rowL, 20, inventoryMaterialList.Rows[n]["PreparedBy"].ToString());
                report.SetText(ref sheet1, _rowL, 21, inventoryMaterialList.Rows[n]["IsActive"].ToString());
                
            }

            //#region sumCalc

            //_rowL++;
            //sheet1.Range[_rowL, 1, _rowL, 5].Merge();
            //sheet2.Range[_rowL, 1, _rowL, 5].Merge();
            //report.SetText(ref sheet1, _rowL, 1, "Total :", true);
            //report.SetText(ref sheet2, _rowL, 1, "Total :", true);

            //var totalCountNeed = 5;
            //var sumdrcrCol = 6;
            //for (int i = 1; i <= totalCountNeed; i++)
            //{
            //    if (i < 3)
            //    {
            //        sheet1.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
            //        sheet1.Range[_rowL, sumdrcrCol].NumberFormat = report.NumberFormatDecimalTwo();
            //        sheet1.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
            //        sheet1.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);
            //    }

            //    sheet2.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
            //    sheet2.Range[_rowL, sumdrcrCol].NumberFormat = report.NumberFormatDecimalTwo();
            //    sheet2.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
            //    sheet2.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);


            //    sumdrcrCol++;
            //}
            //#endregion sumCalc

            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            //sheet2.Range[(row), 1, _rowL, sheet2headreColIndex].BorderAround(ExcelLineStyle.Hair);


            _rowL++;
            //if (inventoryServiceList.Rows.Count != 0)
            //{
            //    _rowL++;
            //    var serviceHeadreColIndex = 1;
            //    report.SetHeaderText(ref sheet1, _rowL, serviceHeadreColIndex, "Service", 32);
            //    serviceHeadreColIndex++;
            //    report.SetHeaderText(ref sheet1, _rowL, serviceHeadreColIndex, "Amount (TRN)", 32, ExcelHAlign.HAlignRight);
            //    serviceHeadreColIndex++;
            //    report.SetHeaderText(ref sheet1, _rowL, serviceHeadreColIndex, "Total Tax", 26, ExcelHAlign.HAlignRight);


            //    for (int n = 0; n < inventoryServiceList.Rows.Count; n++)
            //    {
            //        _rowL++;
            //        report.SetText(ref sheet1, _rowL, 1, inventoryServiceList.Rows[n]["ServiceMasterName"].ToString());
            //        report.SetText(ref sheet1, _rowL, 2, Convert.ToDouble(inventoryServiceList.Rows[n]["Amount"].ToString()));
            //        report.SetText(ref sheet1, _rowL, 3, Convert.ToDouble(inventoryServiceList.Rows[n]["TotalTaxAmount"].ToString()));
            //    }

            //}
            //#region sum

            //_rowL++;
            //report.SetText(ref sheet1, _rowL, 1, "Total :", true);

            //var loopCount = 2;
            //var colNo = 2;
            //for (int i = 1; i <= loopCount; i++)
            //{
            //    sheet1.Range[_rowL, colNo].Formula = "=SUM(" + report.GetColumnNameForXls(colNo) + Row_Total_Start + ":" + report.GetColumnNameForXls(colNo) + (_rowL - 1) + ")";
            //    sheet1.Range[_rowL, colNo].NumberFormat = report.NumberFormatDecimalTwo();
            //    sheet1.Range[_rowL, colNo].CellStyle.Font.Bold = true;
            //    sheet1.Range[_rowL, colNo].BorderAround(ExcelLineStyle.Hair);
            //    colNo++;
            //}
            //#endregion sumCalc

            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);

            //#region Signature

            //_rowL = _rowL + 4;
            //sheet1.Range[_rowL, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            //sheet1.Range[_rowL, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            //sheet1.Range[_rowL, 6].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

            //report.SetText(ref sheet1, _rowL, 1, "Prepared By", true);
            //report.SetText(ref sheet1, _rowL, 3, "Checked By", true);
            //report.SetText(ref sheet1, _rowL, 6, "Authorized By", true);

            //sheet2.Range[_rowL, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            //sheet2.Range[_rowL, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            //sheet2.Range[_rowL, 6].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

            //report.SetText(ref sheet2, _rowL, 1, "Prepared By", true);
            //report.SetText(ref sheet2, _rowL, 3, "Checked By", true);
            //report.SetText(ref sheet2, _rowL, 6, "Authorized By", true);

            //#endregion Signature

            sheet1.Name = sheet1Name;
            sheet1.UsedRange.WrapText = true;
            sheet1.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyPlantHeader(ref sheet1, sheet1headreColIndex, sheet1Name, companyId, plantName, null);
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