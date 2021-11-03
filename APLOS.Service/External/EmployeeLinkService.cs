#region Using

using Library.Core;
using Library.Crosscutting;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.External;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.External
{
    public class EmployeeLinkService : Service<EmployeeLink>, IEmployeeLinkService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IEmployeeService _employeeService;

        public EmployeeLinkService(
              IRepositoryAsync<EmployeeLink> baseRepository
            , IEmployeeService employeeService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(baseRepository, unitOfWork)
        {
            _employeeService = employeeService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region Operation

        private Dictionary<string, object> GetDoainDataByCompanyGroup(string companyGroupId)
        {
            try
            {
                var sql = @"SELECT Host,Port,MailingUserName,MailingPassword,IsSSL FROM CompanyGroup WHERE Id='" + companyGroupId + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void EmployeeLinkSend(EmployeeLink entity, IEnumerable<Employee> employeeList)
        {
            try
            {
                if (employeeList == null || employeeList.Count() == 0)
                    throw new CustomException("Recipent can not be null.............!");
                var empIds = employeeList.Select(t => t.Id).ToString();
                var dbData = _employeeService.Query(t => empIds.Contains(t.Id)).Select().ToList();

                var domainAddress = GetDoainDataByCompanyGroup(entity.CompanyGroupId);
                string host;
                int port;
                string mailUser;
                string mailPassword = "";
                var isSSL = false;

                if (!string.IsNullOrEmpty(domainAddress["Host"].ToString()) &&
                    (domainAddress["Port"].ToString()).ToInt() != 0 &&
                    !string.IsNullOrEmpty(domainAddress["MailingUserName"].ToString()) &&
                    !string.IsNullOrEmpty(domainAddress["MailingPassword"].ToString()))
                {
                    host = domainAddress["Host"].ToString();
                    port = (domainAddress["Port"].ToString()).ToInt();
                    mailUser = domainAddress["MailingUserName"].ToString();
                    mailPassword = domainAddress["MailingPassword"].ToString();
                    isSSL = domainAddress["IsSSL"].ToString().ToBoolean();
                }
                else
                    throw new CustomException("This 'company group' has no web address..................!");
                var msg = "";
                var cc = entity.CC ?? "";
                foreach (var item in employeeList)
                {
                    msg = "";
                    msg = entity.Message + "<br />" + entity.Url + "/Employee?id=" + item.Id;
                    EmailSender em = new EmailSender(host, port, mailUser, mailPassword, isSSL);
                    //em.Send(item.Email, cc, entity.Subject, msg, null, (entity.SenderName + " <" + entity.SenderEmail + ">"));
                    em.Send(entity.SenderName + " <" + entity.SenderEmail + ">", item.Email, cc, entity.Subject, entity.Message);
                }
                foreach (var item in dbData)
                {
                    item.TimesSend += 1;
                    _employeeService.UpdateGraph(item);
                }
                _unitOfWork.SaveChanges();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion Operation
    }
}