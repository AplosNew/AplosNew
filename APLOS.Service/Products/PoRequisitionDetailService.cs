#region Using

using Library.Core;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Products
{
    public class PoRequisitionDetailService : Service<PoRequisitionDetail>, IPoRequisitionDetailService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<PoRequisitionDetail> _poRequisitionDetailService; 
        //private readonly IRepositoryAsync<PurchaseOrderGroupDetails> _purchaseOrderGroupDetails;
        private readonly IUnitOfWork _unitOfWork;

        public PoRequisitionDetailService(
            IRepositoryAsync<PoRequisitionDetail> poRequisitionDetailService 
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IRepositoryAsync<PurchaseOrderGroupDetails> purchaseOrderGroupDetails
            ) : base(poRequisitionDetailService, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
            _poRequisitionDetailService = poRequisitionDetailService;
            

        }




        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(PoRequisitionDetail), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }
        public override void Insert(PoRequisitionDetail entity)
        {
            try
            {
                entity.Id = GetPK();
                base.Insert(entity);
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
            //try
            //{
            //    var detail = Convert.ToBoolean(_purchaseOrderGroupMaster.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM [TRN].[PurchaseOrderDetail] WHERE Id='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
            //    if (!detail)
            //    {
            //        var data = base.Find(id);
            //        if (data.IsNull()) throw new CustomException(ServiceResources.RecordNoLonger);
            //        base.Delete(data);
            //        _unitOfWork.SaveChanges();
            //    }
            //    else throw new CustomException("Please delete first line item.");
            //}
            //catch (Exception ex)
            //{
            //    throw new CustomException(ex.Message, ex,
            //        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
            //        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            //}
        }

        //private static void ResetCurrencyRate(PurchaseOrder entity)
        //{
        //    if (string.IsNullOrEmpty(entity.ToCurrencyRate.ToString()))
        //    {
        //        if (entity.BaseCurrencyId != entity.CurrencyId)
        //            throw new CustomException("Please input currency rate.");
        //        else
        //            entity.ToCurrencyRate = 1;
        //    }
        //    else if (entity.ToCurrencyRate == 0)
        //    {
        //        if (entity.BaseCurrencyId != entity.CurrencyId)
        //            throw new CustomException("Please input currency rate.");
        //    }
        //    else
        //    {
        //        if (entity.BaseCurrencyId == entity.CurrencyId)
        //            entity.ToCurrencyRate = 1;
        //    }
        //}



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
	                                    ,POG.Description
	                                    ,POG.Remarks
	                                    ,POG.Active
	                                    ,POG.AddedBy
                                       FROM TRN.PurchaseOrderGroup POG   ";


                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }






        public IEnumerable<object> GetAllPurchaseOrderGroupDetails()//string ReqDetailId
        {
            try
            {
                //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var _sql = @"SELECT IM.Id
                        --,IM.Id AS MaterialReqqusitionMasterId
                         ,IM.MaterialReqqusitionMasterId AS Id
                         ,IR.Id MaterialReqqusitionMasterId
                        , MGM.UserName AS MaterialGroupName
                        , IM.MaterialMasterId, MM.UserName AS MaterialName
                        , IM.ArticleId, ART.StandardName
                        , IM.FirstCharacteristicsId, FC.UserName AS SKU1
                        , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                        , IM.SecondCharacteristicsId, SC.UserName AS SKU2
                        , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                        , IM.ThirdCharacteristicsId, TC.UserName AS SKU3
                        , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
                        , ROUND(IM.TransactionQty,2) TransactionQty
                        , IM.TransactionUoMId
                        , TUoM.UserName AS TransactionUoM
                        , ROUND(IM.EstimatedRate,2) EstimatedRate 
                        , CU.Code AS CurrencyName
                        , ROUND((IM.TransactionQty * IM.EstimatedRate),2) AS TotalAmount   
                        ,IM.MaterialDetail
                        ,Replace(CONVERT(VARCHAR(11), IM.DeliveryDate, 106), ' ', '-') DeliveryDate
                        ,Act.Id As Activity
                        ,Act.UserName As ActivityName
                        ,IM.BudgetType
                        ,IM.Reason
                        ,IM.Remarks
                        ,IM.FutureReqApp
                        --,BudgetMasterId
                        --,GLGeneralInfoId
                        FROM TRN.MaterialRequsitionDetails AS IM
                        LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IM.TransactionUoMId=TUoM.Id
                        LEFT JOIN [TRN].[MaterialRequsitionMaster] AS IR ON IM.MaterialReqqusitionMasterId=IR.Id
                        LEFT JOIN [SCS].[Currency] AS CU ON IM.CurrencyId=CU.Id 
                        LEFT JOIN [HKP].[Activity] As Act On ACT.Id=IM.ActivityId
                        --JOIN [HKP].Budget
                        --JOIN [HKP].Gl
                       --WHERE IM.MaterialReqqusitionMasterId
                       ";
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



       
        public IEnumerable<object> GetReqMaster(string Id)
        {
            try
            {
                var sql = @"Select 
                            MRM.Id
                            ,MRM.RequisitionDate
                            ,MRM.RequisitionType
                            ,MRM.RequirmentType
                            ,MRM.QualityApprovalResponsiblePersonId
                            ,EI1.EmployeeName AS ResponsiblePersonName
                            ,MRM.NeedSpecialAppId
                            ,EI.EmployeeName AS EmployeeName
                            ,E.UserName EntityName
                            ,MRM.EntityId
                            ,MRM.ReasonWhyItIsNotPlanEarlier
                            ,MRM.AddedBy
                            ,MRM.AddedDate
                            ,MRM.AddedFromIP
                            ,MRM.UpdatedBy
                            ,MRM.UpdatedDate
                            ,MRM.UpdatedFromIP
                            ,MRM.RequisitionDate
                            ,MRM.Remarks
                            ,MRM.CheckedBy
                            ,MRM.CheckedByStatus
                            ,MRM.AuthorizedBy
                            ,MRM.AuthorizedByStatus
                            ,MRM.IsApproved
                            ,A.UserName ActivityName
                            ,MM.UserName MaterialName
                            ,MRD.TransactionQty
                            ,MRD.EstimatedRate
                            ,MRD.TotalAmount
                            FROM [TRN].[MaterialRequsitionMaster] MRM
                            Left Join [TRN].[MaterialRequsitionDetails] MRD On MRD.MaterialReqqusitionMasterId=MRM.Id
                            Left Join org.Entity E on E.Id=MRM.EntityId
                            LEFT JOin HKp.Activity A On A.Id=MRD.ActivityId
                            Left Join MST.MaterialMaster MM on MM.Id=MRD.MaterialMasterId 
                            LEFT JOIN dbo.EmployeeInformation EI On EI.SystemId=MRM.NeedSpecialAppId
                            LEFT JOIN dbo.EmployeeInformation EI1 On EI1.SystemId=MRM.QualityApprovalResponsiblePersonId
                                                where MRM.Id='" + Id + @"'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }


        public object GetAutoSequence()
        {
            throw new NotImplementedException();
        }

    }
}