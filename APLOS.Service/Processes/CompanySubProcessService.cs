#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Processes;
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

namespace Library.Service.Processes
{
    public class CompanySubProcessService : Service<CompanySubProcess>, ICompanySubProcessService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IProcessService _subProcessService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<CompanySubProcess> _companySubProcessRepository;

        public CompanySubProcessService(
            IRepositoryAsync<CompanySubProcess> companySubProcessRepository,
            IProcessService subProcessService,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(companySubProcessRepository, unitOfWork, pkGeneratorService)
        {
            _companySubProcessRepository = companySubProcessRepository;
            _subProcessService = subProcessService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void Insert(IEnumerable<CompanySubProcess> companySubProcess, string[] ids)
        {
            var flag = false;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string i = GetPK();
                var count = 0;
                string companyId = companySubProcess.First().CompanyId;
                var data = base.Query(r => r.CompanyId == companyId).Select();
                foreach (var item in companySubProcess)
                {
                    if (item.Id.StartsWith("new"))
                    {
                        count++;
                        item.Id = null;
                        item.Id = i + "-" + count;
                        item.CompanyGroupId = identity.CompanyGroupId;
                        InsertGraph(item);
                    }
                }
                foreach (var item in ids)
                {
                    base.DeleteGraph(item);
                }

                _unitOfWork.BeginTransaction();
                flag = true;
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(CompanySubProcess), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public GridModel Query(GridParameter parameters, string companyId, string processId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            parameters.CmdText = @"SELECT
		                                CSP.Id
		                                ,CSP.CompanyGroupId
		                                ,CSP.CompanyId
		                                ,CSP.SubProcessId
		                                ,CSP.ProcessId
                                        ,CSP.Archive
		                                ,SP.Code
		                                ,SP.UserName AS SubProcessName
		                                ,SPC.UserName AS SubProcessCategoryName
                                FROM MST.[CompanySubProcess] AS CSP
                                LEFT OUTER JOIN HKP.[SubProcess] AS SP ON CSP.SubProcessId=SP.Id
                                LEFT OUTER JOIN HKP.[SubProcessCategory] AS SPC ON SP.SubProcessCategoryId=SPC.Id
                                WHERE CSP.CompanyGroupId='" + identity.CompanyGroupId + "' AND CSP.CompanyId='" + companyId + "' AND CSP.Archive=0 AND CSP.ProcessId='" + processId + "' ";
            return _sqlRepository.GetGridData(parameters);
        }

        public GridModel Query(GridParameter parameters, string companyId, string processId, string[] subProcessIds)
        {
            var subProcess = "";
            if (subProcessIds.Length > 0)
                subProcess = string.Join(",", subProcessIds.Select(item => "'" + item + "'"));
            else
                subProcess = "' '";
            parameters.order = "asc";
            parameters.sort = "UserName";
            parameters.CmdText = @"Select csp.Id,
                                          sp.ProcessId,
                                          csp.SubProcessId,
                                          --csp.CompanyGroupId,
                                          --csp.CompanyId,
                                          sp.Code,
                                          sp.UserName,
                                          sp.Remarks
                                   FROM   [" + DbSchema.Masters + @"].[" + DbTable.CompanySubProcess + @"] csp
                                          LEFT OUTER JOIN [" + DbSchema.HKP + @"].[" + DbTable.SubProcess + @"] sp on csp.SubProcessId = sp.Id
                                   WHERE  csp.CompanyId ='" + companyId + "' AND sp.ProcessId ='" + processId + "' AND sp.Id NOT IN (" + subProcess + ") AND csp.Archive = 0 ";
            return _sqlRepository.GetGridData(parameters);
        }

        public void DeleteGraph(string id)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new CustomException(string.Format(ResourcesCore.IsNull, "CompanySubProcess Id"));
                _unitOfWork.BeginTransaction();
                flag = true;
                CompanySubProcess companySubProcess = Find(id);
                base.DeleteGraph(companySubProcess);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private bool CheckIdUse(string companyId, string[] processIds)
        {
            try
            {
                var process = "";
                if (processIds.Length > 0)
                    process = string.Join(",", processIds.Select(item => "'" + item + "'"));
                else
                    process = "' '";
                string sql = @"IF EXISTS(SELECT 1 FROM(
                                SELECT A.CheckingColumn1,B.CheckingColumn2 FROM
                                (SELECT Id,CompanyId AS CheckingColumn1 FROM HKP.SubProcessSet) AS A LEFT OUTER JOIN
                                (SELECT SubProcessSetId,SubProcessId AS CheckingColumn2 FROM HKP.SubProcessSetDetail ) AS B ON A.Id=B.SubProcessSetId
                               ) AA WHERE CheckingColumn1 ='" + companyId + "' AND CheckingColumn2 IN (" + process + ")) SELECT 1 ELSE SELECT 0 RETURN";
                return Convert.ToBoolean(_companySubProcessRepository.SqlQuery<int>(sql).Single());
            }
            catch
            {
                throw;
            }
        }

        public IEnumerable<ComboModel> GetCbo(string ProcessId, string companyId)
        {
            try
            {
                
                string _sql = @"select s.Id,s.UserName from hkp.SubProcess s
                                left join (select * from mst.CompanySubProcess where CompanyId='" + companyId + @"') cs on cs.SubProcessId= s.Id
                                where s.ProcessId = '" + ProcessId + @"'
                                order by s.UserName ";
                return _sqlRepository.GetCombo(_sql, "Id", "UserName");
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}