using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Attendances;
using Library.Model.HumanResources;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Library.Service.HumanResources
{
    public class RestDetailsService : Service<AttendanceRestDetail>, IRestDetailsService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<AttdnProcessData> _attdnProcessDataRepository;

        public RestDetailsService(
            IRepositoryAsync<AttendanceRestDetail> restDetailsRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork
            , IRepositoryAsync<AttdnProcessData> attdnProcessDataRepository) : base(restDetailsRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _attdnProcessDataRepository = attdnProcessDataRepository;
        }

        #endregion Constructor

        public void InsertOrUpdateGraph(IEnumerable<AttendanceRestDetail> restDetailsList, string plantId, string restId, out List<AttendanceRestDetail> restDetailsDb_list)
        {
            try
            {
                

                restDetailsDb_list = base.Query(r => r.AttendanceRestId == restId).Select().ToList<AttendanceRestDetail>();
                if (restDetailsDb_list == null)
                {
                    restDetailsDb_list = new List<AttendanceRestDetail>();
                }

                var pk = GetAutoNumber(nameof(AttendanceRestDetail), PKGeneratorEnum.Auto, null, DateTime.Now);
                var count = 0;
                foreach (AttendanceRestDetail restDetails in restDetailsList)
                {

                    
                    var restDetailsDb = restDetailsDb_list.FirstOrDefault(r => r.EmpSystemId == restDetails.EmpSystemId && r.AttendanceRestId == restId);
                    if (restDetailsDb == null || string.IsNullOrEmpty(restDetailsDb.Id))
                    {
                        count++;
                        restDetailsDb = new AttendanceRestDetail
                        {
                            Id = "RD-" + pk + "-" + count,
                            AttendanceRestId = restId,
                            PlantId = plantId,
                            EmpSystemId = restDetails.EmpSystemId,
                            ModelState = ModelState.Added
                        };
                        AuditService.AddedLog(restDetailsDb);
                        restDetailsDb_list.Add(restDetailsDb);
                    }
                    else
                    {
                        restDetailsDb.EmpSystemId = restDetails.EmpSystemId;
                        restDetailsDb.ModelState = ModelState.Modified;
                        AuditService.UpdatedLog(restDetailsDb);
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
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void DeleteDetail(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new CustomException("Attendance Rest Detail id is not found...");

            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var data = Find(id);
                if (data != null)
                {
                    var updateData = _attdnProcessDataRepository.Query(t => t.AttendanceRestDetailId == id).Select().FirstOrDefault();
                    if (updateData!=null)
                    {
                        updateData.AttendanceRestDetailId = null;
                        updateData.DayStatus = updateData.DayStatusInTimeOnly;
                        updateData.OTHr = updateData.OTIntime + updateData.OTOuttime;
                        _attdnProcessDataRepository.Update(updateData);
                    }
                    base.Delete(data);
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

       
    }
}