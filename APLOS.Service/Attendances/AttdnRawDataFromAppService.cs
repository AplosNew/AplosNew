#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Attendances;
using Library.Service.Core;
using Library.Service.Systems;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

#endregion Using

namespace Library.Service.Attendances
{
    public class AttdnRawDataFromAppService : Service<AttdnRawDataFromApp>, IAttdnRawDataFromAppService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pk;

        public AttdnRawDataFromAppService(
             IRepositoryAsync<AttdnRawDataFromApp> attdnRawDataFromAppRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(attdnRawDataFromAppRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _pk = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return _pk.GetAutoNumber(nameof(AttdnRawDataFromApp), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void SaveAttdnRawDataFromApp_backup(AttdnRawDataFromApp entity)
        {

            var flag = false;
            try
            {
                string _date = string.Empty;

                if (string.IsNullOrEmpty(entity.EmployeeId))
                {
                    throw new CustomException("Please Input EmployeeId");
                }
                //if ( string.IsNullOrEmpty(entity.PDate.ToString()))
                //    { 
                //    throw new CustomException("Please Input Punch Date");
                //}
                if (string.IsNullOrEmpty(entity.InTime.ToString()) && string.IsNullOrEmpty(entity.OutTime.ToString()))
                {
                    throw new CustomException("Please Input Intime/Outtime");
                }
                else
                {
                    if (string.IsNullOrEmpty(entity.InTimeUI) == false)
                    {
                        _date = Convert.ToDateTime(entity.InTimeUI).ToString("dd-MMM-yyyy");
                    }
                    else
                    {
                        _date = Convert.ToDateTime(entity.OutTimeUI).ToString("dd-MMM-yyyy");
                    }
                }
                _unitOfWork.BeginTransaction();
                flag = true;

                var _date2 = Convert.ToDateTime(_date);
                var empAndDate = Query(t => t.EmployeeId == entity.EmployeeId && t.PDate == _date2).Select().ToList().FirstOrDefault();


                if (empAndDate == null || string.IsNullOrEmpty(empAndDate.Id))
                {
                    entity.Id = "RA" + GetPK();
                    entity.AddedBy = entity.EmployeeId;
                    entity.AddedDate = DateTime.Now;
                    entity.IsProcessed = false;
                    entity.IsLocked = false;
                    //AuditService.AddedLog(entity);
                    if (entity.InTimeUI == null || string.IsNullOrEmpty(entity.InTimeUI))
                    {
                        entity.InTime = null;
                    }
                    else
                    {
                        var intime_ = entity.InTimeUI;
                        //var _intime = Convert.ToDateTime(intime_).ToString("dd-MMM-yyyy hh:mm:ss");
                        entity.InTime = Convert.ToDateTime(intime_);
                        entity.PDate = _date2;// Convert.ToDateTime(Convert.ToDateTime(intime_).ToString("dd-MMM-yyyy"));

                    }

                    if (entity.OutTimeUI == null || string.IsNullOrEmpty(entity.OutTimeUI))
                    {
                        entity.OutTime = null;
                    }
                    else
                    {
                        //entity.OutTime = Convert.ToDateTime(entity.OutTimeUI);
                        entity.OutTime = Convert.ToDateTime(entity.OutTimeUI);
                        entity.PDate = _date2;// Convert.ToDateTime(Convert.ToDateTime(entity.OutTimeUI).ToString("dd-MMM-yyyy"));
                    }

                    Insert(entity);
                }
                else
                {
                    var dbData = Find(empAndDate.Id);
                    if (!dbData.IsLocked)
                    {
                        //if (dbData.PDate != null && string.IsNullOrEmpty(dbData.PDate.ToString())==false)
                        //{
                        //    dbData.PDate = entity.PDate;
                        //}

                        if (entity.InTimeUI != null && string.IsNullOrEmpty(entity.InTimeUI) == false)
                        {
                            //dbData.InTime = Convert.ToDateTime(entity.InTimeUI);
                            dbData.InTime = Convert.ToDateTime(entity.InTimeUI);
                        }

                        if (entity.OutTimeUI != null && string.IsNullOrEmpty(entity.OutTimeUI) == false)
                        {
                            //dbData.OutTime = Convert.ToDateTime(entity.OutTimeUI);
                            dbData.OutTime = Convert.ToDateTime(entity.OutTimeUI);
                        }

                        if (entity.Remarks != null && string.IsNullOrEmpty(entity.Remarks.ToString()) == false)
                        {
                            dbData.Remarks = entity.Remarks;
                        }

                        dbData.IsProcessed = false;


                        dbData.UpdatedBy = entity.EmployeeId;
                        dbData.UpdatedDate = DateTime.Now;
                        Update(dbData);
                    }
                }


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
        public void SaveAttdnRawDataFromApp(AttdnRawDataFromApp entity)
        {

            try
            {
                string _date = string.Empty;

                if (string.IsNullOrEmpty(entity.EmployeeId))
                {
                    throw new CustomException("Please Input EmployeeId");
                }
                //if ( string.IsNullOrEmpty(entity.PDate.ToString()))
                //    { 
                //    throw new CustomException("Please Input Punch Date");
                //}
                if (string.IsNullOrEmpty(entity.InTime.ToString()) && string.IsNullOrEmpty(entity.OutTime.ToString()))
                {
                    throw new CustomException("Please Input Intime/Outtime");
                }
                else
                {
                    if (string.IsNullOrEmpty(entity.InTimeUI) == false)
                    {
                        _date = Convert.ToDateTime(System.DateTime.Now).ToString("dd-MMM-yyyy");
                    }
                    else
                    {
                        _date = Convert.ToDateTime(System.DateTime.Now).ToString("dd-MMM-yyyy");
                    }
                }


                DateTime _date2 = Convert.ToDateTime(_date);

                DataSet dsRef;
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                string strSql = @"select * from AttdnRawDataFromApp where EmployeeId='" + entity.EmployeeId + "' and PDate='" + _date2.ToString("dd-MMM-yyyy") + "'";
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");

                if (dsRef.Tables[0].Rows.Count > 0)
                {

                    if (string.IsNullOrEmpty(entity.InTimeUI) == false)
                    {
                        if (bplib.clsWebLib.GetBoolData(dsRef.Tables[0].Rows[0]["isApprovedIN"].ToString()) == true)
                            throw new Exception("In data has been approved");

                        if (dsRef.Tables[0].Rows[0]["InTime"].ToString() != "")
                            throw new Exception("You have already submitted In data");
                    }

                    if (string.IsNullOrEmpty(entity.OutTimeUI) == false)
                    {
                        if (bplib.clsWebLib.GetBoolData(dsRef.Tables[0].Rows[0]["isApprovedOUT"].ToString()) == true)
                            throw new Exception("Out data has been approved");


                    }


                }


                if (dsRef.Tables[0].Rows.Count == 0)
                {
                    string NewId = "";
                    bplib.clsGenID id = new bplib.clsGenID();
                    id.GenIDYearly(System.DateTime.Now.ToShortDateString(), "MANUAL ATTENDANCE", out NewId);

                    DataRow dr = dsRef.Tables[0].NewRow();

                    dr["Id"] = NewId;
                    dr["PlantId"] = entity.PlantId;
                    dr["EmployeeId"] = entity.EmployeeId;
                    dr["PDate"] = _date2;



                    if (string.IsNullOrEmpty(entity.InTimeUI) == false)
                    {

                        dr["InTime"] = System.DateTime.Now.ToString();
                        dr["Latitude"] = entity.Latitude;
                        dr["Longitude"] = entity.Longitude;
                        dr["INLocationDesc"] = entity.INLocationDesc;
                        dr["Remarks"] = entity.Remarks;
                    }

                    if (string.IsNullOrEmpty(entity.OutTimeUI) == false)
                    {
                        dr["OutTime"] = System.DateTime.Now.ToString();
                        dr["LatitudeOUT"] = entity.LatitudeOUT;
                        dr["LongitudeOUT"] = entity.LongitudeOUT;
                        dr["OutLocationDesc"] = entity.OutLocationDesc;
                        dr["RemarksOUT"] = entity.RemarksOUT;

                    }


                    dr["AddedBy"] = entity.EmployeeId;
                    dr["AddedDate"] = DateTime.Now.ToString();
                    dr["UpdatedBy"] = entity.EmployeeId;
                    dr["UpdatedDate"] = DateTime.Now.ToString();

                    //       IsProcessed       IsLocked SourceFlag   OutLocationDesc isApprovedIN ApprovedByIN    ApprovalDateIN isApprovedOUT   ApprovedByOUT ApprovalDateOUT RemarksOUT LatitudeOUT LongitudeOUT



                    dsRef.Tables[0].Rows.Add(dr);


                }
                else
                {

                    DataRow dr = dsRef.Tables[0].Rows[0];
                    dr.BeginEdit();


                    if (string.IsNullOrEmpty(entity.InTimeUI) == false)
                    {

                        dr["InTime"] = System.DateTime.Now.ToString();
                        dr["Latitude"] = entity.Latitude;
                        dr["Longitude"] = entity.Longitude;
                        dr["INLocationDesc"] = entity.INLocationDesc;
                        dr["Remarks"] = entity.Remarks;
                    }

                    if (string.IsNullOrEmpty(entity.OutTimeUI) == false)
                    {
                        dr["OutTime"] = System.DateTime.Now.ToString();
                        dr["LatitudeOUT"] = entity.LatitudeOUT;
                        dr["LongitudeOUT"] = entity.LongitudeOUT;
                        dr["OutLocationDesc"] = entity.OutLocationDesc;
                        dr["RemarksOUT"] = entity.RemarksOUT;

                    }


                    dr["UpdatedBy"] = entity.EmployeeId;
                    dr["UpdatedDate"] = DateTime.Now.ToString();

                    dr.EndEdit();


                }

                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsRef);


            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }
        }

        public IEnumerable<object> GetAttnd(string EmpId)
        {
            try
            {
                var sql = @"SELECT TOP 45 apd.EmpSystemID, apd.WorkDate,sd.ShiftDefinationName,apd.InTime,
                            apd.OutTime, apd.DayStatus
                            FROM AttdnProcessData apd 
                            left outer join ShiftDefination sd ON sd.SystemID=apd.ShiftSystemID
                            WHERE apd.EmpSystemID='" + EmpId + "' order by WorkDate desc";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public string SaveData(IEnumerable<AttdnRawDataFromApp> DataToSave)
        {

            try
            {
                List<AttdnRawDataFromApp> items = DataToSave.ToList();

                string _date = string.Empty;

                if (string.IsNullOrEmpty(items[0].InTimeUI) == false)
                {
                    _date = Convert.ToDateTime(DateTime.Now).ToString("dd-MMM-yyyy");
                }
                else
                {
                    _date = Convert.ToDateTime(DateTime.Now).ToString("dd-MMM-yyyy");
                }

                DateTime _date2 = Convert.ToDateTime(_date);

                DataSet dsRef,dsMaster;
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                string strSql = @"select * from AttdnRawDataFromApp where EmployeeId='" + items[0].EmployeeId + "' and PDate='" + _date2.ToString("dd-MMM-yyyy") + "'";
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");

                // APD Manual Entry
                string Sql = @"select * from AttdnProcessData where EmpSystemID='" + items[0].EmployeeId + "' and WorkDate='" + _date2.ToString("dd-MMM-yyyy") + "'";
                objCon.OpenDataSetThroughAdapter(Sql, out dsMaster, false, "1");


                if (dsRef.Tables[0].Rows.Count > 0)
                {
                    // Validation Section
                    if (string.IsNullOrEmpty(items[0].InTimeUI) == false)
                    {
                        if (bplib.clsWebLib.GetBoolData(dsRef.Tables[0].Rows[0]["isApprovedIN"].ToString()) == true)
                            return "In data has been approved";


                        if (dsRef.Tables[0].Rows[0]["InTime"].ToString() != "")
                            return "You have already submitted In data";
                    }


                    if (string.IsNullOrEmpty(items[0].OutTimeUI) == false)
                    {
                        if (bplib.clsWebLib.GetBoolData(dsRef.Tables[0].Rows[0]["isApprovedOUT"].ToString()) == true)
                            return "Out data has been approved";

                    }


                }


                if (dsRef.Tables[0].Rows.Count == 0)
                {
                    string NewId = "";
                    bplib.clsGenID id = new bplib.clsGenID();
                    id.GenIDYearly(DateTime.Now.ToShortDateString(), "MANUAL ATTENDANCE", out NewId);

                    DataRow dr = dsRef.Tables[0].NewRow();


                    dr["Id"] = NewId;
                    dr["PlantId"] = items[0].PlantId;
                    dr["EmployeeId"] = items[0].EmployeeId;
                    dr["PDate"] = _date2;


                    if (string.IsNullOrEmpty(items[0].InTimeUI) == false)
                    {
                        dr["InTime"] = DateTime.Now.ToString();
                        dr["Latitude"] = items[0].Latitude;
                        dr["Longitude"] = items[0].Longitude;
                        dr["INLocationDesc"] = items[0].INLocationDesc;
                        dr["Remarks"] = items[0].Remarks;
                    }



                    if (string.IsNullOrEmpty(items[0].OutTimeUI) == false)
                    {
                        dr["OutTime"] = System.DateTime.Now.ToString();
                        dr["LatitudeOUT"] = items[0].LatitudeOUT;
                        dr["LongitudeOUT"] = items[0].LongitudeOUT;
                        dr["OutLocationDesc"] = items[0].OutLocationDesc;
                        dr["RemarksOUT"] = items[0].RemarksOUT;



                    }
                    if (string.IsNullOrEmpty(items[0].AttndType) == false)
                    {
                        dr["AttndType"] = items[0].AttndType;

                    }
                    dr["AddedBy"] = items[0].EmployeeId;
                    dr["AddedDate"] = DateTime.Now.ToString();
                    dr["UpdatedBy"] = items[0].EmployeeId;
                    dr["UpdatedDate"] = DateTime.Now.ToString();

                    dsRef.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsRef.Tables[0].Rows[0];
                    dr.BeginEdit();

                    if (string.IsNullOrEmpty(items[0].InTimeUI) == false)
                    {
                        dr["InTime"] = DateTime.Now.ToString();
                        dr["Latitude"] = items[0].Latitude;
                        dr["Longitude"] = items[0].Longitude;
                        dr["INLocationDesc"] = items[0].INLocationDesc;
                        dr["Remarks"] = items[0].Remarks;
                    }

                    if (string.IsNullOrEmpty(items[0].OutTimeUI) == false)
                    {
                        dr["OutTime"] = DateTime.Now.ToString();
                        dr["LatitudeOUT"] = items[0].LatitudeOUT;
                        dr["LongitudeOUT"] = items[0].LongitudeOUT;
                        dr["OutLocationDesc"] = items[0].OutLocationDesc;
                        dr["RemarksOUT"] = items[0].RemarksOUT;

                    }

                    if (string.IsNullOrEmpty(items[0].AttndType) == false)
                    {
                        dr["AttndType"] = items[0].AttndType;

                    }
                   
                    dr["UpdatedBy"] = items[0].EmployeeId;
                    dr["UpdatedDate"] = DateTime.Now.ToString();

                    dr.EndEdit();

                }

                // Entry in APD 
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    DataRow drx = dsMaster.Tables[0].Rows[0];
                    drx.BeginEdit();

                    if (string.IsNullOrEmpty(items[0].InTimeUI) == false)
                    {
                        drx["ManualInTime"] = DateTime.Now.ToString();                        
                        drx["OriginalManualInTime"] = DateTime.Now.ToString();
                        drx["IsManualInTime"] = true;
                        drx["InTime"]= DateTime.Now.ToString();
                    }

                    if (string.IsNullOrEmpty(items[0].OutTimeUI) == false)
                    {
                        drx["OutTime"] = DateTime.Now.ToString();                        
                        drx["ManualOutTime"] = DateTime.Now.ToString();
                        drx["OriginalManualOutTime"] = DateTime.Now.ToString();
                        drx["IsManualOutTime"] = true;

                    }
                    drx["DataSource"] = "MobileAppEntry";
                    drx["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                    drx.EndEdit();

                }


                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsRef,dsMaster);

                string MasterId = dsRef.Tables[0].Rows[0]["Id"].ToString();
                return MasterId;



            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            finally
            {



            }
        }
    }
}