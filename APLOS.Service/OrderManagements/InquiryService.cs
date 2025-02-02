#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.OrderManagements;
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

namespace Library.Service.OrderManagements
{
    public class InquiryService : Service<Inquiry>, IInquiryService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<CommitmentInquiry> _commitmentInquiryRepository;
        private readonly IRepositoryAsync<ProductInquiry> _productInquiryRepository;
        private readonly IRepositoryAsync<ProductInquiryDetail> _productInquiryDetailRepository;

        public InquiryService(
            IRepositoryAsync<Inquiry> inquiryRepository,
            IPKGeneratorService pkGeneratorService,
            IRepositoryAsync<CommitmentInquiry> commitmentInquiryRepository,
            IRepositoryAsync<ProductInquiry> productInquiryRepository,
            IRepositoryAsync<ProductInquiryDetail> productInquiryDetailRepository,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(inquiryRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _commitmentInquiryRepository = commitmentInquiryRepository;
            _productInquiryRepository = productInquiryRepository;
            _productInquiryDetailRepository = productInquiryDetailRepository;
        }

        #endregion Constructor

        public GridModel Query(GridParameter parameters, string entityId, string employeeId)
        {
            try
            {
                parameters.CmdText = @"	SELECT IQ.*,REI.EmployeeName ResponsiblePerson,EI.EmployeeName,B.UserName BuyerName FROM [TRN].[Inquiry] IQ
	                                    LEFT JOIN MST.BuyerMaster BM ON IQ.BuyerMasterId=BM.Id
	                                    LEFT JOIN HKP.Buyer B ON BM.BuyerId=B.Id
	                                    LEFT JOIN dbo.EmployeeInformation EI ON IQ.EmployeeId= EI.SystemId
	                                    LEFT JOIN dbo.EmployeeInformation REI ON IQ.ResponsiblePersonId= REI.SystemId
                                        WHERE IQ.EntityId='" + entityId + "' and IQ.EmployeeId IN (select u.EmployeeId from sec.UserSalesGroup s join sec.[User] u on s.UserId= u.Id  where u.EmployeeId='" + employeeId + "')";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public IEnumerable<object> QueryForCommitmentInquiry(string inquiryId)
        {
            try
            {
                string _sql = @"SELECT CI.*,M.UserName FinishedGoods,p.UserName ProcessName,SP.UserName SubProcessName,B.UserName BuyerName,CM.LSD FROM [TRN].[CommitmentInquiry] CI
                                LEFT JOIN TRN.Commitment CM ON CI.CommitmentId=CM.Id
                                LEFT JOIN MST.MaterialMaster M ON  CM.MaterialMasterId= M.Id
                                LEFT JOIN [HKP].[Process] P ON CM.ProcessId = P.Id
                                LEFT JOIN [HKP].[SubProcess] SP ON CM.SubProcessId= SP.Id
								LEFT JOIN MST.BuyerMaster BM ON CM.BuyerMasterId=BM.Id
                                LEFT JOIN HKP.Buyer B ON BM.BuyerId =B.Id WHERE CI.InquiryId='" + inquiryId + "'";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public IEnumerable<object> CheckUserSalesGroup()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @"select S.SalesGroupId from sec.UserSalesGroup s
                                inner join sec.[User] u on s.UserId= u.Id
                                where u.EmployeeId IN(select distinct u.EmployeeId from sec.UserSalesGroup s
                                inner join sec.[User] u on s.UserId= u.Id
                                where u.EmployeeId='" + identity.EmployeeId + "')";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public IEnumerable<object> QueryForProductInquiry(string inquiryId)
        {
            try
            {
                string _sql = @"SELECT PI.*,M.UserName FinishedGoods,MG.UserName MaterialGroupName,PM.UserName ProductMasterName
								FROM [TRN].[ProductInquiry] PI
                                LEFT JOIN MST.MaterialMaster M ON PI.MaterialMasterId=M.Id
                                LEFT JOIN MST.MaterialGroupMaster MG ON M.MaterialGroupMasterId=MG.Id
                                LEFT JOIN TRN.ProductDefinition PD ON PD.MaterialMasterId=PI.MaterialMasterId
                                LEFT JOIN MST.ProductMaster PM ON PD.ProductMasterId = PM.Id WHERE PI.InquiryId='" + inquiryId + "'";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public GridModel QueryForIsPreCostingInquiry(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT PI.Id,PI.MaterialMasterId,PI.MaterialMasterArticleId,PI.InquiryId,PI.EntityId,PI.IsDevelopment,PI.IsPreCosting,IQ.BuyerId,M.UserName FinishedGoods,MG.UserName MaterialGroupName,PM.UserName ProductMasterName
								FROM [TRN].[ProductInquiry] PI
								LEFT JOIN TRN.Inquiry IQ ON PI.InquiryId=IQ.Id
                                LEFT JOIN MST.MaterialMaster M ON PI.MaterialMasterId=M.Id
                                LEFT JOIN MST.MaterialGroupMaster MG ON M.MaterialGroupMasterId=MG.Id
                                LEFT JOIN TRN.ProductDefinition PD ON PD.MaterialMasterId=PI.MaterialMasterId
                                LEFT JOIN MST.ProductMaster PM ON PD.ProductMasterId = PM.Id
								WHERE PI.IsPreCosting=1 and PI.MaterialMasterId not in(SELECT MaterialMasterId FROM [TRN].[PreCosting] WHERE IsInquiryLinked=1)";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public GridModel GetProductInquiryWithEntity(GridParameter parameters, string entityId)
        {
            try
            {
                parameters.CmdText = @"SELECT MM.Id,MM.UserName FinishedGoods,MG.UserName MaterialGroupName,PM.UserName ProductMasterName
                             FROM TRN.ProductDefinition PD
                             JOIN MST.MaterialMaster MM ON MM.Id= PD.MaterialMasterId
                             left join mst.MaterialGroupMaster MG ON MM.MaterialGroupMasterId = MG.Id
                             left join mst.ProductMaster PM ON PD.ProductMasterId=PM.Id
                             left JOIN ORG.Entity E ON MM.CompanyGroupId=E.CompanyGroupId
                            WHERE E.Id='" + entityId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetIntermediateItemWithEntity(GridParameter parameters, string entityId)
        {
            try
            {
                parameters.CmdText = @"SELECT IE.Id, I.UserName IntermediateItemName,I.StandardName,I.Code
                              FROM [HKP].[IntermediateItemEntity] IE
                              LEFT JOIN HKP.IntermediateItem I ON IE.IntermediateItemId= I.Id
                              WHERE EntityId='" + entityId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetProductProcessGroupWithNotId(GridParameter parameters, string processProductionGroupId)
        {
            try
            {
                parameters.CmdText = @" SELECT * FROM HKP.ProductionProcessGroup WHERE Id NOT IN('"+processProductionGroupId+"')";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> QueryForProductInquiryDetailList(string productInquiryId)
        {
            try
            {
                string _sql = @"SELECT PD.*,II.Code,II.UserName ProductionProcessGroupName,II.StandardName
                                            ,EntityOrVendorName=
                                         CASE ISNULL(PD.InternalEntityId,'')
                                                  WHEN '' THEN ''
                                                  ELSE EWG.UserName
                                                  END
                                         + CASE ISNULL(PD.VendorId,'')
                                                  WHEN '' THEN ''
                                                  ELSE PRT.UserName
                                                  END
                                  FROM [ODYSSEYPOP].[TRN].[ProductInquiryDetail] PD
                                  LEFT JOIN HKP.ProductionProcessGroup II ON PD.ProductionProcessGroupId=II.Id
                                LEFT OUTER JOIN ORG.Entity AS EWG ON PD.InternalEntityId=EWG.Id
                                LEFT OUTER JOIN HKP.Party AS PRT ON PD.VendorId=PRT.Id
                              WHERE PD.ProductInquiryId='" + productInquiryId + "'";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private string GetPK()
        {
            return base.GetAutoNumber(nameof(Inquiry), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public void InsertAndUpdate(Inquiry entity)
        {
            var flag = false;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sg = CheckUserSalesGroup();
                if (sg.Count() == 0)
                {
                    throw new CustomException("No sales group found.");
                }
                _unitOfWork.BeginTransaction();
                flag = true;
                string pkId = GetPK();
                if (string.IsNullOrEmpty(entity.Id))
                {
                    entity.Id = pkId;
                    entity.EmployeeId = identity.EmployeeId;
                    base.InsertGraph(entity);
                }
                else
                {
                    base.UpdateGraph(entity);
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void InsertCommitmentInquiry(IEnumerable<CommitmentInquiry> entities)
        {
            var flag = false;
            try
            {
                if (entities == null)
                    throw new CustomException("Please insert commitment inquiry.");
                CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sg = CheckUserSalesGroup();
                if (sg.Count() == 0)
                {
                    throw new CustomException("No sales group found.");
                }
                _unitOfWork.BeginTransaction();
                flag = true;
                var pk = base.GetMaxNumber(nameof(CommitmentInquiry), PKGeneratorEnum.Auto, identity.CompanyGroupId, DateTime.Now);
                foreach (var item in entities)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        pk.MaxNumber++;
                        item.Id = pk.MaxNumber.ToString();
                        item.ModelState = ModelState.Added;
                        AuditService.Log(item);
                    }
                    else
                    {
                        item.ModelState = ModelState.Modified;
                        AuditService.Log(item);
                    }
                    _commitmentInquiryRepository.InsertOrUpdateGraph(item);
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException cx)
            {
                throw cx;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name.ToString(), null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void InsertProductInquiry(IEnumerable<ProductInquiry> entities)
        {
            var flag = false;
            try
            {
                if (entities == null)
                    throw new CustomException("Please insert legal designation");
                CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _unitOfWork.BeginTransaction();
                flag = true;
                var pk = base.GetMaxNumber(nameof(ProductInquiry), PKGeneratorEnum.Auto, identity.CompanyGroupId, DateTime.Now);
                foreach (var item in entities)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        pk.MaxNumber++;
                        item.Id = pk.MaxNumber.ToString();
                        item.ModelState = ModelState.Added;
                        AuditService.Log(item);
                    }
                    else
                    {
                        item.ModelState = ModelState.Modified;
                        AuditService.Log(item);
                    }
                    _productInquiryRepository.InsertOrUpdateGraph(item);
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name.ToString(), null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void InsertProductInquiryDetail(IEnumerable<ProductInquiryDetail> entities)
        {
            var flag = false;
            try
            {
                if (entities == null)
                    throw new CustomException("Please insert legal designation");
                CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _unitOfWork.BeginTransaction();
                flag = true;
                var pk = base.GetMaxNumber(nameof(ProductInquiryDetail), PKGeneratorEnum.Auto, identity.CompanyGroupId, DateTime.Now);
                foreach (var item in entities)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        pk.MaxNumber++;
                        item.Id = pk.MaxNumber.ToString();
                        item.ModelState = ModelState.Added;
                        AuditService.Log(item);
                    }
                    else
                    {
                        item.ModelState = ModelState.Modified;
                        AuditService.Log(item);
                    }
                    _productInquiryDetailRepository.InsertOrUpdateGraph(item);
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException cx)
            {
                throw cx;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name.ToString(), null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteGraph(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new CustomException(string.Format(ResourcesCore.IsNull, "Inquiry Id"));
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var data = Find(id);
                if (data != null)
                {
                    IEnumerable<CommitmentInquiry> commitmentInquiry = _commitmentInquiryRepository.Query(r => r.InquiryId == data.Id).Select();
                    if (commitmentInquiry != null)
                    {
                        _commitmentInquiryRepository.ExecuteSqlCommand("DELETE FROM TRN.CommitmentInquiry Where InquiryId='" + data.Id + "'");
                    }
                    IEnumerable<ProductInquiry> productInquiry = _productInquiryRepository.Query(r => r.InquiryId == id).Select();
                    foreach (var item in productInquiry)
                    {
                        _productInquiryRepository.ExecuteSqlCommand("DELETE FROM TRN.ProductInquiryDetail Where ProductInquiryId='" + item.Id + "'");
                    }
                    if (productInquiry != null)
                    {
                        _productInquiryRepository.ExecuteSqlCommand("DELETE FROM TRN.ProductInquiry Where InquiryId='" + id + "'");
                    }
                    base.DeleteGraph(data);
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public GridModel EntityWithInternal(GridParameter parameters, string companyGroupId, string productionProcessGroupId)
        {
            try
            {
                parameters.CmdText = @"SELECT rd.Id,C.UserName AS Company, rd.UserName AS [Name],DivisionId
								, (SELECT UserName FROM  [ORG].[Division] WHERE Id=rd.DivisionId) AS [Division], UnitId,
								 (SELECT UserName FROM  [ORG].[Unit] WHERE Id=rd.UnitId) AS [Unit]
								FROM [ORG].[Entity] as rd
								LEFT OUTER JOIN ORG.Company AS C ON rd.CompanyId=C.Id
								 WHERE rd.Archive=0  AND rd.CompanyGroupId='" + companyGroupId + "'  AND rd.Id  in (Select EntityId From [HKP].[ProductionProcessGroupEntity] WHERE ProductionProcessGroupId='" + productionProcessGroupId + "') ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetEntityCboWithProductionProcessGroup(string productionProcessGroupId)
        {
            try
            {
                string _sql = @"SELECT distinct E.Id Value, E.UserName Text FROM
							[HKP].[ProductionProcessGroupEntity] PG
						LEFT JOIN ORG.Entity E ON PG.EntityId=E.Id where PG.ProductionProcessGroupId='" + productionProcessGroupId + "'";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public IEnumerable<object> GetActivityWithBuyerMasterCbo(string buyerMasterId)
        {
            try
            {
                CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var _sql = @"SELECT O.Id Value,O.UserName Text FROM MST.BuyerMasterDetail BD
                              LEFT JOIN SCS.OrderActivity O ON BD.BuyerActivityId=O.Id
                              WHERE BD.BuyerMasterId='"+ buyerMasterId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public GridModel QueryForResponsible(GridParameter parameters,string entityId,string buyerMasterId,string buyerActivityId)
        {
            try
            {
                parameters.CmdText = @"SELECT EI.SystemId, C.UserName AS CompanyName, MB.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName,   EI.EmployeeName, REPLACE(CONVERT(CHAR(11), EI.DOB, 106),' ','-') AS DateOfBirth,'' AS Flag FROM dbo.EmployeeInformation AS EI 
                                    LEFT OUTER JOIN ORG.Company AS C ON EI.CompanyId=C.Id  WHERE EI.SystemId=(select EmployeeOneId from mst.BuyerMasterDetail  where EntityId="+ entityId + @" and BuyerMasterId="+buyerMasterId+@" and BuyerActivityId="+buyerActivityId+@")
                                    UNION SELECT EI.SystemId, C.UserName AS CompanyName, MB.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName,   EI.EmployeeName, REPLACE(CONVERT(CHAR(11), EI.DOB, 106),' ','-') AS DateOfBirth,'' AS Flag FROM dbo.EmployeeInformation AS EI 
                                    LEFT OUTER JOIN ORG.Company AS C ON EI.CompanyId=C.Id  WHERE EI.SystemId=(select EmployeeTwoId from mst.BuyerMasterDetail  where EntityId=" + entityId + @" and BuyerMasterId=" + buyerMasterId + @" and BuyerActivityId=" + buyerActivityId + @")
                                    UNION SELECT EI.SystemId, C.UserName AS CompanyName, MB.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName,   EI.EmployeeName, REPLACE(CONVERT(CHAR(11), EI.DOB, 106),' ','-') AS DateOfBirth,'' AS Flag FROM dbo.EmployeeInformation AS EI 
                                    LEFT OUTER JOIN ORG.Company AS C ON EI.CompanyId=C.Id  WHERE EI.SystemId=(select EmployeeThreeId from mst.BuyerMasterDetail  where EntityId=" + entityId + @" and BuyerMasterId=" + buyerMasterId + @" and BuyerActivityId=" + buyerActivityId + @")
                                    UNION SELECT EI.SystemId, C.UserName AS CompanyName, MB.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName,   EI.EmployeeName, REPLACE(CONVERT(CHAR(11), EI.DOB, 106),' ','-') AS DateOfBirth,'' AS Flag FROM dbo.EmployeeInformation AS EI 
                                    LEFT OUTER JOIN ORG.Company AS C ON EI.CompanyId=C.Id  WHERE EI.SystemId=(select EmployeeFourId from mst.BuyerMasterDetail  where EntityId=" + entityId + @" and BuyerMasterId=" + buyerMasterId + @" and BuyerActivityId=" + buyerActivityId + @")
                                    UNION SELECT EI.SystemId, C.UserName AS CompanyName, MB.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName,   EI.EmployeeName, REPLACE(CONVERT(CHAR(11), EI.DOB, 106),' ','-') AS DateOfBirth,'' AS Flag FROM dbo.EmployeeInformation AS EI 
                                    LEFT OUTER JOIN ORG.Company AS C ON EI.CompanyId=C.Id  WHERE EI.SystemId=(select EmployeeFiveId from mst.BuyerMasterDetail  where EntityId=" + entityId + @" and BuyerMasterId=" + buyerMasterId + @" and BuyerActivityId=" + buyerActivityId + @") ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
    }
}