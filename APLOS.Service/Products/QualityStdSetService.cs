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
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Products
{
    public class QualityStdSetService : Service<QualityStdSet>, IQualityStdSetService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<PurchaseOrderGroup> _purchaseOrderGroupMaster;
        private readonly IRepositoryAsync<QualityStdSet> _qualityStdSet;
        private readonly IRepositoryAsync<PurchaseOrderGroupDetails> _purchaseOrderGroupDetails;
        private readonly IUnitOfWork _unitOfWork;

        public QualityStdSetService(
            IRepositoryAsync<PurchaseOrderGroup> purchaseOrderGroupMaster
             ,IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IRepositoryAsync<PurchaseOrderGroupDetails> purchaseOrderGroupDetails
             , IRepositoryAsync<QualityStdSet> qualityStdSet
            ) : base(qualityStdSet, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
            _purchaseOrderGroupMaster = purchaseOrderGroupMaster;
            _purchaseOrderGroupDetails = purchaseOrderGroupDetails;
            _qualityStdSet = qualityStdSet;

        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(QualityStdSet), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        private void Check(QualityStdSet entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName);
            CheckUniqueColumn(UniqueColumnName.StandardName, entity.StandardName, r => r.Id != entity.Id && r.StandardName == entity.StandardName);
            CheckUniqueColumn(UniqueColumnName.ShortName, entity.ShortName, r => r.Id != entity.Id && r.ShortName == entity.ShortName);

        }


        public override void Insert(QualityStdSet entity)
        {
            try
            {
               // Check(entity);


                entity.Id = GetPK();
                AuditService.AddedLog(entity);
                entity.ModelState = ModelState.Added;
                _qualityStdSet.Insert(entity);
                _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public override void Update(QualityStdSet entity)
        {
            try
            {
                
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //entity.CompanyGroupId = identity.CompanyGroupId;
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public void DeleteQStd1(string id)
        {
            try
            {
                var detail = Convert.ToBoolean(_qualityStdSet.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM [TRN].[QualityStdSet] WHERE Id='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
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


        public void DeleteQStd(string id)
        {
            try
            {
                //var detail = Convert.ToBoolean(_inventoryReceiveRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM TRN.MaterialRequsitionDetails WHERE Id='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                ////var service = Convert.ToBoolean(_inventoryReceiveRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM TRN.InventoryService WHERE InventoryReceiveId='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                //if (!detail)
                //{

                var data = _qualityStdSet.Find(id);
                if (data.IsNull()) throw new CustomException(ServiceResources.RecordNoLonger);
                _qualityStdSet.Delete(data.Id);
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

        public IEnumerable<object> GetQualityStdSetGridData()
        {
            try
            {
                var sql = @"SELECT      
                                  QSS. Id
                                  ,QSS.SiNo
                                  ,QSS.CompanyGroupId
                                  ,QSS.CompanyId
                                  ,QSS.PlantId
                                  ,QSS.Code
                                  ,QSS.ShortName
                                  ,QSS.UserName
                                  ,QSS.StandardName
                                  ,QSS.Sequence
                                  ,QSS.Description
                                  ,QSS.Remarks
                                  ,QSS.QualityCategory
                                  ,QSS.[Parameter]
                                  ,QSS.[UnitOfMeasurement]
                                  ,QSS.MaxValue
                                  ,QSS.MainValue
                                  ,QSS.AddedBy
                                  ,QSS.AddedDate
                                  ,QSS.AddedFromIP
                                  ,QSS.UpdatedBy
                                  ,QSS.UpdatedDate
                                  ,QSS.UpdatedFromIp
                                  FROM TRN.QualityStdSet QSS";


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

                var data = _qualityStdSet.Find(id);
                if (data.IsNull()) throw new CustomException(ServiceResources.RecordNoLonger);
                _qualityStdSet.Delete(data.Id);
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
                    toCurrencyRate = _qualityStdSet.SqlQuery<decimal>(sql).First();
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

    }
}