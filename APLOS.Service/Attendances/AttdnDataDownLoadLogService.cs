#region Using

using Library.Core;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Attendances;
using Library.Service.Core;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion Using

namespace Library.Service.Attendances
{
    public class AttdnDataDownLoadLogService : Service<AttdnDataDownLoadLog>, IAttdnDataDownLoadLogService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pk;

        public AttdnDataDownLoadLogService(
            IRepositoryAsync<AttdnDataDownLoadLog> attdnDataDownLoadLogRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(attdnDataDownLoadLogRepository, unitOfWork, pkGeneratorService)
        {
            _pk = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public IEnumerable<object> AttendanceLogMaxDate(string sPlantId)
        {
            try
            {
                var _sql = @"SELECT DS.Id DevSystemId
                                    , DS.MachineId DeviceId
                                    , DL.PDate
                                    , MAX(DL.PTime) PTime FROM mst.AccessControllerList DS
                                      LEFT JOIN dbo.AttdnDataDownLoadLog DL ON DS.Id = DL.DevSystemId
                                        WHERE DS.PlantId = '" + sPlantId + @"'
                                        GROUP BY DS.Id, DS.MachineId, DL.PDate
                                        ORDER By DS.MachineId, DL.PDate";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetPK()
        {
            return _pk.GetAutoNumber(nameof(AttdnDataDownLoadLog), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private IEnumerable<AttdnDataDownLoadLog> LoadAttdnDataDownLoadLog(string sPlantid)
        {
            try
            {
                var _sql = @"SELECT * FROM AttdnDataDownLoadLog WHERE PlantID = '" + sPlantid + @"' ";
                return _sqlRepository.GetModelCollection<AttdnDataDownLoadLog>(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void InitData(string plantid, List<AttdnDataDownLoadLog> from_ui, out List<AttdnDataDownLoadLog> from_db)
        {
            from_db = null;
            try
            {
                var _pks = GetPK();
                from_db = LoadAttdnDataDownLoadLog(plantid).ToList<AttdnDataDownLoadLog>();
                var _count = 0;
                foreach (var ui in from_ui)
                {
                    //dvAttnLog.RowFilter = "DevSystemID = '" + sDevSystemID + "' AND PDate = '" + DownLdDate + "' AND PTime = '" + MaxTimeInLog + "'";
                    var dtTime = Convert.ToDateTime(ui.PTime);
                    var db = from_db.Where(a => a.DevSystemId == ui.DevSystemId && a.PDate == ui.PDate && a.PTime == ui.PTime).FirstOrDefault();
                    if (db == null)
                    {
                        _count++;
                        db = new AttdnDataDownLoadLog
                        {
                            AddedBy = ui.AddedBy,
                            DateAdded = DateTime.Now,
                            DevSystemId = ui.DevSystemId,
                            Id = "AL" + _pks + "-" + _count,
                            PDate = ui.PDate,
                            DownLoadRemarks = ui.DownLoadRemarks,
                            PTime = ui.PTime,
                            PlantId = plantid,
                            DateUpdated = DateTime.Now,
                            UpdatedBy = ui.UpdatedBy,
                            ModelState = ModelState.Added
                        };
                        from_db.Add(db);
                    }
                    else
                    {
                        //db.AddedBy = ui.AddedBy;
                        //db.DeviceId = ui.DeviceId;
                        //db.DevSystemId = ui.DevSystemId;
                        //db.GroupId = ui.GroupId;
                        //db.LogDownLoadNum = ui.LogDownLoadNum;
                        //db.PDate = ui.PDate;
                        //db.PlantId = ui.PlantId;
                        //db.ProcessedFlag = ui.ProcessedFlag;
                        //db.PTime = ui.PTime;
                        //db.PType = ui.PType;
                        ////db.RowId = ui.RowId;
                        //db.DateUpdated = DateTime.Now;
                        //db.UpdatedBy = ui.UpdatedBy;
                        //db.ModelState = ModelState.Modified;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SaveAttdnDataDownLoadLog(string plantid, List<AttdnDataDownLoadLog> fromui)
        {
            List<AttdnDataDownLoadLog> from_db = null;
            var flag = false;
            try
            {
                InitData(plantid, fromui, out from_db);
                foreach (var item in from_db)
                {
                    InsertOrUpdateGraph(item);
                }
                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
    }
}