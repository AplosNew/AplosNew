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
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.OrderManagements
{
    public class SampleOrderPartnerFunctionService : Service<SampleOrderPartnerFunction>, ISampleOrderPartnerFunctionService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public SampleOrderPartnerFunctionService(
            IRepositoryAsync<SampleOrderPartnerFunction> partnerFunctionRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(partnerFunctionRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(SampleOrderPartnerFunction), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public IEnumerable<object> Query(string masterId)
        {
            try
            {
                string _sql = @"SELECT SPF.Id
                                     , SPF.PartnerFunctionId
                                     , SPF.SampleOrderId
                                     , PF.UserName
                                     , AssignmentType
                                     , AccountType
                                     , Cus.UserName Customer
                                     , SPF.CustomerId
                                     , 1 AS IsSelectedID
                                FROM TRN.SampleOrderPartnerFunction SPF
                                LEFT OUTER JOIN HKP.PartnerFunction PF ON PF.Id = SPF.PartnerFunctionId
                                LEFT OUTER JOIN HKP.Party Cus ON Cus.Id = SPF.CustomerId
                                Where SPF.SampleOrderId='" + masterId + "'";

                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetCustomerBySPF(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.order = "asc";
                parameters.sort = "PartyName";
                parameters.CmdText = @"SELECT DISTINCT(A.PartyId) AS PartyId, P.Code AS PartyCode, P.UserName AS PartyName, P.Id, P.Code, P.UserName
									 FROM TRN.SampleOrder AS A
									 LEFT JOIN [ORG].[Plant] AS PL ON A.PlantId=PL.Id
									 LEFT JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
									 LEFT JOIN [TRN].SamplePackingList AS SPL ON SPL.PartyId=P.Id
									 WHERE P.Active=1 AND P.Archive=0 AND PL.CompanyId='" + identity.CompanyId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertOrUpdateGraph(string masterId, IEnumerable<SampleOrderPartnerFunction> partnerFunctions)
        {
            try
            {
                if (partnerFunctions != null)
                {
                    var pk = GetMaxNumber(nameof(SampleOrderPartnerFunction), PKGeneratorEnum.Auto, null, DateTime.Now);
                    foreach (var item in partnerFunctions)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            pk.MaxNumber++;
                            item.Id = pk.MaxNumber.ToString();
                            item.SampleOrderId = masterId;
                            InsertGraph(item);
                        }
                        else
                        {
                            UpdateGraph(item);
                        }
                    }
                }
                var dbList = base.Query(t => t.SampleOrderId == masterId).Select().AsEnumerable();
                if (dbList.Count() > 0)
                {
                    if (partnerFunctions == null || partnerFunctions.Count() == 0)
                    {
                        foreach (var item in dbList)
                        {
                            base.DeleteGraph(item);
                        }
                    }
                    else
                    {
                        foreach (var item in dbList)
                        {
                            if (!partnerFunctions.Any(t => t.Id == item.Id))
                            {
                                base.DeleteGraph(item);
                            }
                        }
                    }
                }
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public void DeleteGraph(string masterId)
        {
            var dbList = base.Query(t => t.SampleOrderId == masterId).Select().AsEnumerable();
            if (dbList != null)
            {
                foreach (var item in dbList)
                {
                    base.DeleteGraph(item);
                }
            }
        }
    }
}