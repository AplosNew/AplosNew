#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Setups;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Setups;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.SchedulingServices.Setups
{
    public class MailReceiverService : Service<MailReceiver>, IMailReceiverService
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<MailReceiverDetail> _mailReceiverDetailRepository;
        private readonly IRepositoryAsync<MailReceiverServiceMapping> _mailReceiverMappingRepository;

        public MailReceiverService(
              IRepositoryAsync<MailReceiver> repository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IRepositoryAsync<MailReceiverDetail> mailReceiverDetailRepository
            , IRepositoryAsync<MailReceiverServiceMapping> mailReceiverMappingRepository
            ) : base(repository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
            _mailReceiverDetailRepository = mailReceiverDetailRepository;
            _mailReceiverMappingRepository = mailReceiverMappingRepository;
        }

        #endregion Constructor

        public void Insert(MailReceiver entity, IEnumerable<MailReceiverDetail> mailReceiverDetailList)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                CheckUnique(entity);
                entity.Id = GetAutoId();
                entity.CompanyGroupId = identity.CompanyGroupId;
                InsertGraph(entity);

                foreach (var mailReceiverDetail in mailReceiverDetailList)
                {
                    mailReceiverDetail.AddedBy = entity.AddedBy;
                    mailReceiverDetail.AddedDate = entity.AddedDate;
                    mailReceiverDetail.AddedFromIP = entity.AddedFromIP;
                    mailReceiverDetail.MailReceiverId = entity.Id;
                    _mailReceiverDetailRepository.Insert(mailReceiverDetail);
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private string GetAutoId()
        {
            return GetAutoNumber("MailReceiver", PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void Update(MailReceiver entity, IEnumerable<MailReceiverDetail> mailReceiverDetailList)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                CheckUnique(entity);
                var dbList = _mailReceiverDetailRepository.Query(t => t.MailReceiverId == entity.Id).Select().ToList();
                var receiverDetailList = mailReceiverDetailList.ToList();
                foreach (var item in receiverDetailList)
                {
                    if (item.Id == 0)
                    {
                        item.AddedBy = entity.AddedBy;
                        item.AddedDate = DateTime.Now;
                        item.AddedFromIP = entity.AddedFromIP;
                        item.MailReceiverId = entity.Id;

                        _mailReceiverDetailRepository.Insert(item);
                    }
                    else _mailReceiverDetailRepository.Update(item);
                }
               
                UpdateGraph(entity);
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        
        public void DeleteDetail(int Id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var data = _mailReceiverDetailRepository.Query(r => r.Id == Id).Select().FirstOrDefault();
                _mailReceiverDetailRepository.Delete(data);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteGraph(string masterId)
        {
            if (string.IsNullOrEmpty(masterId))
                throw new CustomException(string.Format(ResourcesCore.IsNull, "Mail Recipient Data Id"));
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var data = Find(masterId);
                if (data != null)
                {
                    var childData = _mailReceiverDetailRepository.Query(t => t.MailReceiverId == masterId).Select().ToList();
                    var childData2 = _mailReceiverMappingRepository.Query(t => t.MailReceiverId == masterId).Select().ToList();
					
                    if (childData != null && childData2 != null)
                    {
						_mailReceiverMappingRepository.ExecuteSqlCommand("DELETE FROM SCS.MailReceiverServiceMapping Where MailReceiverId='" + data.Id + "'");
						_mailReceiverDetailRepository.ExecuteSqlCommand("DELETE FROM SCS.MailReceiverDetail Where MailReceiverId='" + data.Id + "'");
                        _mailReceiverDetailRepository.ExecuteSqlCommand("DELETE FROM ACS.MailLog Where MailReceiverId='" + data.Id + "'");
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

        private void CheckUnique(MailReceiver entity)
        {
            CheckUniqueColumn(UniqueColumnName.Name, entity.Name, r => r.Id != entity.Id && r.Name == entity.Name);
        }

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT * FROM [SCS].[MailReceiver] WHERE MailReceipientType = 'Normal'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public GridModel AdminQuery(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = $"SELECT * FROM [{DbSchema.SystemConfigurationAndSetup}].[MailReceiver] WHERE MailReceipientType = 'Admin'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public IEnumerable<object> GetTaggingUser(string mailReceiverId)
        {
            try
            {
                var sql = @"SELECT A.Id, A.MailReceiverId, A.UserId,B.UserId AS UserName,A.SourceType as SourceType, B.EmployeeId,
							--ISNULL(B.FullName, A.FullName) AS FullName,
						--ISNULL(B.Email, A.Email) AS Email,
							A.FullName,A.Email,
							A.MailType, B.Active
							, A.MailType FROM SCS.MailReceiverDetail AS A LEFT JOIN SEC.[User] AS B ON A.UserId=B.Id WHERE A.MailReceiverId ='" + mailReceiverId + "';";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public IEnumerable<object> GetCbo(string companyGroupId)
        {
            try
            {
                return from m in base.Query(r => r.CompanyGroupId == companyGroupId).Select().OrderBy(r => r.Name)
                       select new { Text = m.Name, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #region--Mail Receiver Service Mapping

        public void InsertMailReceiverMapping(MailReceiverServiceMapping entity)
        {
            try
            {
                
                entity.CompanyId = !string.IsNullOrEmpty(entity.PlantId) ? GetCompanyByPlant(entity.PlantId) : null;
                AuditService.AddedLog(entity);
                _mailReceiverMappingRepository.Insert(entity);
                _unitOfWork.SaveChanges();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public void UpdateMailReceiverMapping(MailReceiverServiceMapping entity)
        {
            try
            {
                entity.CompanyId = !string.IsNullOrEmpty(entity.PlantId) ? GetCompanyByPlant(entity.PlantId) : null;
                AuditService.UpdatedLog(entity);
                _mailReceiverMappingRepository.Update(entity);
                _unitOfWork.SaveChanges();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public GridModel QueryMailReceiverMapping(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT a.*,b.Name as MailReceiverName,plant.UserName PlantName FROM SCS.MailReceiverServiceMapping as a
                            INNER JOIN SCS.[MailReceiver] as b on a.MailReceiverId = b.Id
							LEFT JOIN ORG.Plant as plant on plant.Id = a.PlantId ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public void DeleteMapping(string id)
        {
            _mailReceiverMappingRepository.Delete(Convert.ToInt32(id));
            _unitOfWork.SaveChanges();
        }

        public string GetCompanyByPlant(string id)
        {
            try
            {
                var sql = @"SELECT CompanyId FROM ORG.Plant WHERE Id='" + id + "'";
                return _mailReceiverDetailRepository.SqlQuery<string>(sql).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        #endregion

        public IEnumerable<object> GetAdminCcUser()
        {
            try
            {
                var sql = @"SELECT * FROM  SCS.MailReceiverDetail MRD
												INNER JOIN SCS.MailReceiver MR ON MR.Id = MRD.MailReceiverId
										WHERE MailType = 'cc' AND MR.MailReceipientType = 'Admin'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public IEnumerable<object> GetAdminBccUser()
        {
            try
            {
                var sql = @"SELECT * FROM  SCS.MailReceiverDetail MRD
											INNER JOIN SCS.MailReceiver MR ON MR.Id = MRD.MailReceiverId
										where MailType = 'Bcc' AND MR.MailReceipientType = 'Admin'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }
    }
}