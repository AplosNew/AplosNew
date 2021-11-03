using Library.Crosscutting.Security;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Biometrics;
using Library.Service.Core;
using Library.Service.Systems;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.Service.Biometrics
{
    public class AttendanceDeviceZoneService : Service<AttendanceDeviceZone>, IAttendanceDeviceZoneService
    {

        #region Constructor
        private readonly ISignatureService _signatrueService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<AttendanceDeviceZone> _r;

        public AttendanceDeviceZoneService(
            IRepositoryAsync<AttendanceDeviceZone> PreRecruitmentEmpReferenceRepository
            , ISignatureService signatrueService
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository) : base(PreRecruitmentEmpReferenceRepository, unitOfWork, pkGeneratorService)
        {
            _signatrueService = signatrueService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;

            _r = PreRecruitmentEmpReferenceRepository;
        }
        #endregion Constructor




        public List<AttendanceDeviceZone> GetAllZone()
        {
            string sql = @"SELECT* FROM hkp.AttendanceDeviceZone";
            return _sqlRepository.GetModelCollection<AttendanceDeviceZone>(sql, null);
        }
        public List<AttendanceDeviceZone> GetSpecificZone(string ID)
        {
            string sql = @"SELECT* FROM hkp.AttendanceDeviceZone where id='" + ID + "";
            return _sqlRepository.GetModelCollection<AttendanceDeviceZone>(sql, null);
        }
        public void Delete(string id)
        {

            ConnectionManager.DAL.ConManager objCon = null;
            try
            {

                string sql = @"Delete FROM hkp.AttendanceDeviceZone where id='" + id + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();


                objCon.ExecuteNonQueryWrapper(sql, true, "1");


                objCon.CommitTransaction();



            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }

        public void Save(AttendanceDeviceZone data)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            DataSet dsRef;
            try
            {
                //validation
                strSql = "SELECT * FROM hkp.AttendanceDeviceZone WHERE id <> '" + data.Id + "' and code='" + data.Code + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
                if (dsRef.Tables[0].Rows.Count > 0)
                    throw new Exception("Same code already exists in the system");





                strSql = "SELECT * FROM hkp.AttendanceDeviceZone WHERE id = '" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");

                DataRow dr;
                if (dsRef.Tables[0].Rows.Count == 0)
                {
                    dr = dsRef.Tables[0].NewRow();
                    updateRow(true, data, dr);
                    dsRef.Tables[0].Rows.Add(dr);
                }
                else
                {

                    dr = dsRef.Tables[0].Rows[0];
                    dr.BeginEdit();
                    updateRow(false, data, dr);
                    dr.EndEdit();
                }

                clsStaticInfo obs = new clsStaticInfo();
                obs.SaveDataSets(dsRef);

            }
            catch (Exception ex)
            {

                throw (ex);

            }
            finally
            {
                objCon = null;
            }


        }
        private void updateRow(bool addnew, AttendanceDeviceZone data, DataRow dr)
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (addnew == true)
            {
                string id = "";
                bplib.clsGenID objGenID = new bplib.clsGenID();
                objGenID.GenID("BIOMETRIC ZONE", out id);
                dr["id"] = "" + id;
                dr["AddedBy"] = identity.UserId;
                dr["AddedDate"] = System.DateTime.Now.ToString();
                dr["AddedFromIP"] = identity.IPAddress;
            }

            dr["Sequence"] = data.Sequence;
            dr["Code"] = data.Code;
            dr["ShortName"] = data.ShortName;
            dr["StandardName"] = data.StandardName;
            dr["UserName"] = data.UserName;
            dr["Description"] = data.Description;
            dr["Remarks"] = data.Remarks;
            dr["Active"] = data.Active;
            dr["Archive"] = data.Archive;

            dr["UpdatedBy"] = identity.UserId;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;



        }

        public List<AttendanceDeviceZone> SearchSpecificZone(string strkey)
        {
            if (strkey == "")
                strkey = "1=1";

            string sql = @"select * from ( SELECT* FROM hkp.AttendanceDeviceZone) AS TEMP where  " + strkey;
            return _sqlRepository.GetModelCollection<AttendanceDeviceZone>(sql, null);
        }
    }
}
