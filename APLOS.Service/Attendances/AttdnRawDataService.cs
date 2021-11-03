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
    public class AttdnRawDataService : Service<AttdnRawData>, IAttdnRawDataService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pk;

        public AttdnRawDataService(
             IRepositoryAsync<AttdnRawData> attdnRawDataRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(attdnRawDataRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _pk = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return _pk.GetAutoNumber(nameof(AttdnRawData), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public IEnumerable<object> AttendanceProximityInfo(string sPlantID, string sAttnDate)
        {
            try
            {
                var _sql = @"SELECT SystemID
                                        ,SystemID EnrollID
                                        ,EmployeeName EnrollName
                                        ,CardNumber
                                        ,PlantID
                                         FROM
                                            (
                                                SELECT * FROM EmployeeInformation WHERE
                                                JobLocationID IN(SELECT SystemID FROM[dbo].[JobLocation] WHERE PlantID = '" + sPlantID + @"')
                                                and SystemID IN( SELECT DISTINCT EmpSystemID FROM ftEmployeeJobLocationDateWise(" + sAttnDate + @", " + sAttnDate + @", '" + sPlantID + @"')
                                                            WHERE JobLcSystemID IN( SELECT SystemID FROM[dbo].[JobLocation]
                                                                                        WHERE PlantID = '" + sPlantID + @"'
                                                                                   )
                                                        )
                                            ) AS E WHERE(DOS > " + sAttnDate + @" OR DOS IS NULL)";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private IEnumerable<AttdnRawData> LoadAttdnRawData(string sPlantid, string sDevSystemID, string sMinDate, string sMaxDate)//TBT
        {
            var _sql = @"SELECT * FROM AttdnRawData
                            WHERE PlantID = '" + sPlantid + @"' AND DevSystemID = '" + sDevSystemID + @"'
                                  AND PDate BETWEEN '" + sMinDate + @"' AND '" + sMaxDate + @"'";

            return _sqlRepository.GetModelCollection<AttdnRawData>(_sql, null);
        }

        private void InitData(string plantid, string deviceid, string sMinDate, string sMaxDate, string groupid, List<AttdnRawData> from_ui, out List<AttdnRawData> from_db)
        {
            from_db = null;
            try
            {
                var _pks = GetPK();
                from_db = LoadAttdnRawData(plantid, deviceid, sMinDate, sMaxDate).ToList<AttdnRawData>();
                var _count = 0;
                foreach (var ui in from_ui)
                {
                    //dvAttnRawData.RowFilter = "LogDownLoadNum = '" + sLogDownLoadNum + "' AND PDate = '" + sDate + "' AND PTime >= '" + dtTime.AddSeconds(-10) + "' AND PTime <= '" + dtTime + "'";
                    var dtTime = Convert.ToDateTime(ui.PTime);
                    var db = from_db.Where(a => a.LogDownLoadNum == ui.LogDownLoadNum && a.PDate == ui.PDate && a.PTime >= dtTime.AddSeconds(-10) && a.PTime <= dtTime).FirstOrDefault();
                    if (db == null)
                    {
                        _count++;
                        db = new AttdnRawData
                        {
                            AddedBy = ui.AddedBy,
                            DateAdded = DateTime.Now,
                            DeviceId = ui.DeviceId,
                            DevSystemId = ui.DevSystemId,
                            GroupId = groupid,
                            Id = "R" + _pks + "-" + _count,
                            LogDownLoadNum = ui.LogDownLoadNum,
                            PDate = ui.PDate,
                            PlantId = ui.PlantId,
                            ProcessedFlag = ui.ProcessedFlag,
                            PTime = ui.PTime,
                            PType = ui.PType,
                            //db.RowId = ui.RowId;
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
                }//foreach
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SaveAttdnRawData(string plantid, string deviceid, string sMinDate, string sMaxDate, string groupid, List<AttdnRawData> fromui)
        {
            List<AttdnRawData> from_db = null;
            var flag = false;
            try
            {
                InitData(plantid, deviceid, sMinDate, sMaxDate, groupid, fromui, out from_db);
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
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
    }
}