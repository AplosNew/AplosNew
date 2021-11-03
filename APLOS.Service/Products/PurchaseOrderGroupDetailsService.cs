#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.OrderManagements;
using Library.Model.Productions;
using Library.Model.Products;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using Library.ViewModel.OrderManagements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Products
{
    public class PurchaseOrderGroupDetailsService : Service<PurchaseOrderGroupDetails>, IPurchaseOrderGroupDetailsService
    {
        #region Constructor

        private readonly IRepositoryAsync<PurchaseOrderGroupDetails> _receiveDetailRepository;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PurchaseOrderGroupDetailsService(
            IRepositoryAsync<PurchaseOrderGroupDetails> receiveDetailRepository
            ,IRepositoryAsync<PurchaseOrderGroupDetails> materialRequsitionMaster
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IRepositoryAsync<PurchaseOrderGroupDetails> materialRequsitionDetailsRepository
            ) : base(materialRequsitionMaster, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
            _receiveDetailRepository = receiveDetailRepository;
          
        }

        public string CompanyGroupId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Id => throw new NotImplementedException();




        #endregion Constructor



        public void InsertOrUpdateGraph(IEnumerable<PurchaseOrderGroupDetailsViewModel> entity, string id,string Gname)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var flag = false;
            try
            {
               

                //var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderGroupDetails] WHERE PurchaseOrderGroupId='{id}'").First();


                var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',Id)+1,len(Id)) AS INT)), 0) Id FROM [TRN].[PurchaseOrderGroupDetails]   WHERE PurchaseOrderGroupId='{id}'").First();

                _unitOfWork.BeginTransaction();
                flag = true;
                foreach (var item in entity)
                {

                    //var GroupId = _receiveDetailRepository.SqlQuery<int>($"SELECT PurchaseOrderGroupId FROM [TRN].[PurchaseOrderGroupDetails] WHERE MaterialMasterId='{item.MaterialMasterId}'").First();
                    //var GroupName = _receiveDetailRepository.SqlQuery<string>($"select isnull(B.UserName,'') UserName from trn.PurchaseOrderGroupDetails a 	inner join trn.PurchaseOrderGroup B on a.PurchaseOrderGroupId=B.id WHERE a.MaterialMasterId='{item.MaterialMasterId}'").First();

                    ////var POGId = _receiveDetailRepository.Query(r => r.MaterialMasterId == item.MaterialMasterId).Select().FirstOrDefault();
                    //if (GroupName != null || GroupName !="")
                    //{
                    //   // string sql= "Select A.UserName from [TRN].PurchaseOrderGroup a JOIN[TRN].[PurchaseOrderGroupDetails] b ON a.Id = b.PurchaseOrderGroupId where b.MaterialMasterId = '"+ item.MaterialMasterId + "'";
                    //    //var name=_receiveDetailRepository.ExecuteSqlCommand(sql);
                    //    throw new CustomException("Material Already Added in this Group="+ GroupName);
                    //}
                    
                    // Insert in receive detail
                    if (string.IsNullOrEmpty(item.Id))
                {
                    var NewId = id + "-";
                    //var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 1) AS INT)), 0) Id FROM [TRN].[MaterialRequsitionDetails] WHERE MaterialReqqusitionMasterId='{entity.MaterialReqqusitionMasterId}'").First();

                    
                    currentId++;
                        var receiveDetail = new PurchaseOrderGroupDetails
                        {

                            Id = NewId + currentId, //MakePK(NewId + currentId, 0,0),
                            CompanyGroupId = identity.CompanyGroupId,
                            CompanyId= identity.CompanyGroupId,
                            PlantId= identity.PlantId,
                            PurchaseOrderGroupId = id,
                            MaterialMasterId = item.MaterialMasterId,
                            ArticleId = item.ArticleId,
                            FirstCharacteristicsId = item.FirstCharacteristicsId,
                            FirstCharacteristicsValueId = item.FirstCharacteristicsValueId,
                            SecondCharacteristicsId = item.SecondCharacteristicsId,
                            SecondCharacteristicsValueId = item.SecondCharacteristicsValueId,
                            ThirdCharacteristicsValueId = item.ThirdCharacteristicsValueId,
                            ResponsiblePerson = item.ResponsiblePerson,
                            EmployeeCode = item.EmployeeCode,
                            //PartyId = item.PartyId,
                            //PartyPreference = item.PartyPreference


                        };
                        InsertGraph(receiveDetail);

                    }

                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            //catch (CustomException)
            //{
            //    throw;
            //}
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                 ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }

        }

        public void InsertOrUpdateGraphEdit(PurchaseOrderGroupDetails entity)
        {


            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;

                // Insert in receive detail
                if (!string.IsNullOrEmpty(entity.Id))
                {
                    
                    var receiveDetail = new PurchaseOrderGroupDetails
                    {

                        Id = entity.Id,
                        CompanyGroupId = entity.CompanyGroupId,
                        //MaterialReqqusitionMasterId = entity.MaterialReqqusitionMasterId,
                        //ActivityId = entity.ActivityId,
                        MaterialMasterId = entity.MaterialMasterId,
                        ArticleId = entity.ArticleId,
                        FirstCharacteristicsId = entity.FirstCharacteristicsId,
                        FirstCharacteristicsValueId = entity.FirstCharacteristicsValueId,
                        SecondCharacteristicsId = entity.SecondCharacteristicsId,
                        SecondCharacteristicsValueId = entity.SecondCharacteristicsValueId,
                        ThirdCharacteristicsId = entity.ThirdCharacteristicsId,
                        ThirdCharacteristicsValueId = entity.ThirdCharacteristicsValueId,
                        TransactionQty = Convert.ToDecimal(entity.TransactionQty),
                        EstimatedRate = Convert.ToDecimal(entity.EstimatedRate),
                        TotalAmount = Convert.ToDecimal(entity.TotalAmount),
                        
                    };
                    UpdateGraph(receiveDetail);
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
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
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }

        }

        public void DeletePOGDetails(string id)
        {
            try
            {                

                var data = _receiveDetailRepository.Find(id);
                if (data.IsNull()) throw new CustomException(ServiceResources.RecordNoLonger);
                _receiveDetailRepository.Delete(data.Id);
                _unitOfWork.SaveChanges();
               
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetMaterialGridData()
        {
            try
            {
                var sql = @"SELECT top 6000 
                            MT.UserName MaterialType
                            ,MGM.UserName AS MaterialGroupMasterName
                            ,MM.Id MaterialMasterId 
                            ,MM.UserName MaterialMasterName
                            ,0 Active
                            ,ART.Id ArticleId
                            , ART.StandardName ArticleName
                            --, IM.FirstCharacteristicsId
                            --, FC.UserName AS FirstCharacteristics
                           -- , IM.FirstCharacteristicsValueId
                            , ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
                            --, IM.SecondCharacteristicsId
                            --, SC.UserName AS SecondCharacteristics
                            --, IM.SecondCharacteristicsValueId
                            , ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
                            --, IM.ThirdCharacteristicsId
                            --, TC.UserName AS ThirdCharacteristics
                            --, IM.ThirdCharacteristicsValueId
                            , ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 

                            from MST.MaterialMaster AS MM                           
                            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                            LEFT JOIN MST.MaterialMasterArticle AS ART ON MM.Id=ART.MaterialMasterId
							LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id	
                            --LEFT JOIN HKP.Characteristics AS FC ON MM.FirstCharacteristicsId=FC.Id
                            --LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                            --LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                            LEFT JOIN HKP.CharacteristicsValue AS FCV ON MM.Id=FCV.MaterialMasterId
                            LEFT JOIN HKP.CharacteristicsValue AS SCV ON MM.Id=SCV.MaterialMasterId
                            LEFT JOIN HKP.CharacteristicsValue AS TCV ON MM.Id=TCV.MaterialMasterId 
                            where mm.Active=1 and MM.id not in(select MaterialMasterId from trn.PurchaseOrderGroupDetails)";






                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }



    }
}