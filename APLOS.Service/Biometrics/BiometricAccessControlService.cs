using Library.Core;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Attendances;
using Library.Service.Attendances;
using Library.Service.Core;
using Library.Service.Systems;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.Biometrics
{
    public class BiometricAccessControlService : Service<AccessControllerList>, IBiometricAccessControlService
    {
        #region Constructor
        private readonly ISignatureService _signatrueService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<AccessControllerList> _r;

        public BiometricAccessControlService(
            IRepositoryAsync<AccessControllerList> PreRecruitmentEmpReferenceRepository
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

        public IEnumerable<AccessControllerList> GetBiometricDeviceAsAccessController(string PlantID)
        {

            try
            {
                var _sql = @"SELECT * FROM MST.AccessControllerList";// WHERE (PlantID = '" + PlantID + @"') ORDER BY MachineID";

                return _sqlRepository.GetModelCollection<AccessControllerList>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<AccessControllerEmployeeTag> GetAccCrlRegInfoDeviceWiseForEmp(string PlantID, string strDeviceSystemID)
        {
            try
            {
                var _sql = @"SELECT * FROM AccessControllerEmployeeTag
                           WHERE plantid = '" + PlantID + @"' AND DeviceSystemID = '" + strDeviceSystemID + @"'";
                return _sqlRepository.GetModelCollection<AccessControllerEmployeeTag>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IEnumerable<AccessControllerEmployeeTag> GetAccCrlRegInfoEmployeeWise(string EmployeeId)
        {
            try
            {
                var _sql = @"SELECT * FROM AccessControllerEmployeeTag T
                           WHERE T.EmpInfoSystemID = '" + EmployeeId + @"' AND T.RegisterStatus = 'Registered'";
                return _sqlRepository.GetModelCollection<AccessControllerEmployeeTag>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IEnumerable<AccessControllerEmployeeTag> GetAccCrlRegInfoDeviceWiseForEmpAndDevice(string EmployeeId, string strDeviceSystemID)
        {
            try
            {
                var _sql = @"SELECT * FROM AccessControllerEmployeeTag
                           WHERE  DeviceSystemID = '" + strDeviceSystemID + @"'
                            AND EmpInfoSystemID IN (SELECT SystemID FROM EmployeeInformation where SystemID IN (" + EmployeeId + "))";
                return _sqlRepository.GetModelCollection<AccessControllerEmployeeTag>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IEnumerable<AccessControllerEmployeeTag> GetAccCrlRegInfoDeviceWiseForEmp(string PlantID, string strDeviceSystemID, string empIds)
        {
            try
            {
                var _sql = @"SELECT * FROM AccessControllerEmployeeTag
                           WHERE PlantID = '" + PlantID + @"' AND DeviceSystemID = '" + strDeviceSystemID + @"'
                            AND EmpInfoSystemID IN (SELECT SystemID FROM EmployeeInformation where SystemID IN (" + empIds + "))";
                return _sqlRepository.GetModelCollection<AccessControllerEmployeeTag>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<EmployeeInfomationForAccessControl> SearchEmployeeInformationForDevice(string PlantID, string DeviceSystemID)
        {
            try
            {
                string strSql = @"SELECT DISTINCT P.SystemID, P.EmployeeName, P.EmployeeCode,  REPLACE(CONVERT(VARCHAR(11), P.DOB, 113),' ','-') DOB,
                            CASE WHEN ISNULL(fp1.FPTemplate,'')<>'' THEN 'YES' ELSE 'NO' END AS LeftFP,
                            CASE WHEN ISNULL(fp2.FPTemplate,'')<>'' THEN 'YES' ELSE 'NO' END AS RightFP,
                             REPLACE(CONVERT(VARCHAR(11), P.DOJ, 113),' ','-') DOJ, p.EmployeeStatus,
                            P.CardNumber,  CASE WHEN ISNULL(AR.EmpInfoSystemID,'')<>'' THEN 'Registered' else '' end as DeviceStatus,
                          
                            Dpt.UserName AS Department, S.UserName Section, SS.UserName AS SubSection, Desg.UserName AS Designation
                             FROM EmployeeInformation P
                             LEFT JOIN ORG.Department Dpt ON P.DepartmentId = Dpt.id
                             LEFT JOIN ORG.Section S ON P.SectionId = S.id
                             LEFT JOIN ORG.SubSection SS ON P.SubSectionId = SS.id
                             LEFT JOIN HKP.Designation Desg ON P.DesignationSystemID = Desg.Id
							  left join EmployeeFPInformation FP1 ON fp1.id=(SELECT TOP 1 id FROM EmployeeFPInformation FP1 WHERE fp1.EmpSystemId=p.SystemId and isnull(fp1.IsLeft,0)=1) AND fp1.EmpSystemId=p.SystemId 
							 left join EmployeeFPInformation FP2 ON fp2.id=(SELECT TOP 1 id FROM EmployeeFPInformation FP2 WHERE fp2.EmpSystemId=p.SystemId and isnull(fp2.IsLeft,0)=0) AND  fp2.EmpSystemId=p.SystemId 

                            LEFT JOIN AccessControllerEmployeeTag AR ON AR.EmpInfoSystemID=p.SystemID AND AR.DeviceSystemID='" + DeviceSystemID + @"'
                            WHERE (P.PlantID = '" + PlantID + @"') AND isnull(P.EmployeeStatus,'')='Active'
                            ORDER BY P.SystemID";


                return _sqlRepository.GetModelCollection<EmployeeInfomationForAccessControl>(strSql, null);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public List<EmployeeInfomationForAccessControl> SearchRegisteredEmployeeInformation(string PlantID, string DeviceSystemID)
        {
            try
            {
                string strSql = @"SELECT DISTINCT P.SystemID, P.EmployeeName, P.EmployeeCode,  REPLACE(CONVERT(VARCHAR(11), P.DOB, 113),' ','-') DOB, 

                            CASE WHEN ISNULL(fp1.FPTemplate,'')<>'' THEN 'YES' ELSE 'NO' END AS LeftFP,
                            CASE WHEN ISNULL(fp2.FPTemplate,'')<>'' THEN 'YES' ELSE 'NO' END AS RightFP,
                             REPLACE(CONVERT(VARCHAR(11), P.DOJ, 113),' ','-') DOJ, p.EmployeeStatus,
                            P.CardNumber,  CASE WHEN ISNULL(AR.EmpInfoSystemID,'')<>'' THEN 'Registered' else '' end as DeviceStatus,
                            Dpt.UserName AS Department, S.UserName Section, SS.UserName AS SubSection, Desg.UserName AS Designation,
                            P.EmployeeCurrentStatus 
                             FROM EmployeeInformation P
                             LEFT JOIN ORG.Department Dpt ON P.DepartmentId = Dpt.id
                             LEFT JOIN ORG.Section S ON P.SectionId = S.id
                             LEFT JOIN ORG.SubSection SS ON P.SubSectionId = SS.id
                             LEFT JOIN HKP.Designation Desg ON P.DesignationSystemID = Desg.Id
						  left join EmployeeFPInformation FP1 ON fp1.id=(SELECT TOP 1 id FROM EmployeeFPInformation FP1 WHERE fp1.EmpSystemId=p.SystemId and isnull(fp1.IsLeft,0)=1) AND fp1.EmpSystemId=p.SystemId 
							 left join EmployeeFPInformation FP2 ON fp2.id=(SELECT TOP 1 id FROM EmployeeFPInformation FP2 WHERE fp2.EmpSystemId=p.SystemId and isnull(fp2.IsLeft,0)=0) AND  fp2.EmpSystemId=p.SystemId 

                            INNER JOIN AccessControllerEmployeeTag AR ON AR.EmpInfoSystemID=p.SystemID AND AR.DeviceSystemID='" + DeviceSystemID + @"'
                            WHERE (P.PlantID = '" + PlantID + @"')
                            ORDER BY P.SystemID";

                var x = _sqlRepository.GetModelCollection<EmployeeInfomationForAccessControl>(strSql, null);
                return x;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public List<EmployeeInfomationForAccessControl> GetAllSelectedEmployeesToDelete(string emplIDList)
        {
            string strSql = @"SELECT DISTINCT P.SystemID, P.EmployeeName, P.EmployeeCode,  REPLACE(CONVERT(VARCHAR(11), P.DOB, 113),' ','-') DOB, 

                            CASE WHEN ISNULL(fp1.FPTemplate,'')<>'' THEN 'YES' ELSE 'NO' END AS LeftFP,
                            CASE WHEN ISNULL(fp2.FPTemplate,'')<>'' THEN 'YES' ELSE 'NO' END AS RightFP,
                             REPLACE(CONVERT(VARCHAR(11), P.DOJ, 113),' ','-') DOJ, p.EmployeeStatus,
                            P.CardNumber,  '' as DeviceStatus,
                            Dpt.UserName AS Department, S.UserName Section, SS.UserName AS SubSection, Desg.UserName AS Designation
                             FROM EmployeeInformation P
                             LEFT JOIN ORG.Department Dpt ON P.DepartmentId = Dpt.id
                             LEFT JOIN ORG.Section S ON P.SectionId = S.id
                             LEFT JOIN ORG.SubSection SS ON P.SubSectionId = SS.id
                             LEFT JOIN HKP.Designation Desg ON P.DesignationSystemID = Desg.Id
							  left join EmployeeFPInformation FP1 ON fp1.id=(SELECT TOP 1 id FROM EmployeeFPInformation FP1 WHERE fp1.EmpSystemId=p.SystemId and isnull(fp1.IsLeft,0)=1) AND fp1.EmpSystemId=p.SystemId 
							 left join EmployeeFPInformation FP2 ON fp2.id=(SELECT TOP 1 id FROM EmployeeFPInformation FP2 WHERE fp2.EmpSystemId=p.SystemId and isnull(fp2.IsLeft,0)=0) AND  fp2.EmpSystemId=p.SystemId 

                            
                            WHERE P.SystemID IN (" + emplIDList + @")
                            ORDER BY P.SystemID";


            return _sqlRepository.GetModelCollection<EmployeeInfomationForAccessControl>(strSql, null);
        }
        public List<EmployeeInfomationForAccessControl> GetAllRegisteredEmployeeList(string deviceSystemID)
        {
            string strSql = @"SELECT DISTINCT P.SystemID, P.EmployeeName, P.EmployeeCode,  REPLACE(CONVERT(VARCHAR(11), P.DOB, 113),' ','-') DOB,

                            CASE WHEN ISNULL(fp1.FPTemplate,'')<>'' THEN 'YES' ELSE 'NO' END AS LeftFP,
                            CASE WHEN ISNULL(fp2.FPTemplate,'')<>'' THEN 'YES' ELSE 'NO' END AS RightFP,
                             REPLACE(CONVERT(VARCHAR(11), P.DOJ, 113),' ','-') DOJ, p.EmployeeStatus,
                            P.CardNumber,  CASE WHEN ISNULL(AR.EmpInfoSystemID,'')<>'' THEN 'Registered' else '' end as DeviceStatus,
                            Dpt.UserName AS Department, S.UserName Section, SS.UserName AS SubSection, Desg.UserName AS Designation
                             FROM EmployeeInformation P
                             LEFT JOIN ORG.Department Dpt ON P.DepartmentId = Dpt.id
                             LEFT JOIN ORG.Section S ON P.SectionId = S.id
                             LEFT JOIN ORG.SubSection SS ON P.SubSectionId = SS.id
                             LEFT JOIN HKP.Designation Desg ON P.DesignationSystemID = Desg.Id
						  left join EmployeeFPInformation FP1 ON fp1.id=(SELECT TOP 1 id FROM EmployeeFPInformation FP1 WHERE fp1.EmpSystemId=p.SystemId and isnull(fp1.IsLeft,0)=1) AND fp1.EmpSystemId=p.SystemId 
							 left join EmployeeFPInformation FP2 ON fp2.id=(SELECT TOP 1 id FROM EmployeeFPInformation FP2 WHERE fp2.EmpSystemId=p.SystemId and isnull(fp2.IsLeft,0)=0) AND  fp2.EmpSystemId=p.SystemId 

                            LEFT JOIN AccessControllerEmployeeTag AR ON AR.EmpInfoSystemID=p.SystemID
                     
                            WHERE AR.DeviceSystemID='" + deviceSystemID + @"'
                            ORDER BY p.SystemID";

            return _sqlRepository.GetModelCollection<EmployeeInfomationForAccessControl>(strSql, null);
        }
        public List<EmployeeInfomationForAccessControl> GetEmployeeInfoByEmployeeListForUpload(string deviceSystemID, string SystemIDs, string plantId)
        {
            string strSql = @"SELECT P.SystemID, P.EmployeeName,p.EmpPicPath, P.EmployeeCode,  REPLACE(CONVERT(VARCHAR(11), P.DOB, 113),' ','-') DOB, 

                            CASE WHEN ISNULL(fp1.FPTemplate,'')<>'' THEN 'YES' ELSE 'NO' END AS LeftFP,
                            CASE WHEN ISNULL(fp2.FPTemplate,'')<>'' THEN 'YES' ELSE 'NO' END AS RightFP,
                             REPLACE(CONVERT(VARCHAR(11), P.DOJ, 113),' ','-') DOJ, p.EmployeeStatus,
                            P.CardNumber,  CASE WHEN ISNULL(AR.EmpInfoSystemID,'')<>'' THEN 'Registered' else '' end as DeviceStatus,
                            fp1.FPTemplate AS LeftFPTemplate,fp2.FPTemplate AS RightFPTemplate,
 
                            Dpt.UserName AS Department, S.UserName Section, SS.UserName AS SubSection, Desg.UserName AS Designation
                             FROM EmployeeInformation P
                             LEFT JOIN ORG.Department Dpt ON P.DepartmentId = Dpt.id
                             LEFT JOIN ORG.Section S ON P.SectionId = S.id
                             LEFT JOIN ORG.SubSection SS ON P.SubSectionId = SS.id
                             LEFT JOIN HKP.Designation Desg ON P.DesignationSystemID = Desg.Id
						  left join EmployeeFPInformation FP1 ON fp1.id=(SELECT TOP 1 id FROM EmployeeFPInformation FP1 WHERE fp1.EmpSystemId=p.SystemId and isnull(fp1.IsLeft,0)=1) AND fp1.EmpSystemId=p.SystemId 
							 left join EmployeeFPInformation FP2 ON fp2.id=(SELECT TOP 1 id FROM EmployeeFPInformation FP2 WHERE fp2.EmpSystemId=p.SystemId and isnull(fp2.IsLeft,0)=0) AND  fp2.EmpSystemId=p.SystemId 

                            LEFT JOIN AccessControllerEmployeeTag AR ON AR.EmpInfoSystemID=p.SystemID AND AR.DeviceSystemID='" + deviceSystemID + @"'
                            WHERE P.EmployeeCode IN (" + SystemIDs + @")  AND p.PlantId='" + plantId + @"'
                            ORDER BY P.SystemID";

            List<EmployeeInfomationForAccessControl> list = _sqlRepository.GetModelCollection<EmployeeInfomationForAccessControl>(strSql, null);
            foreach (var item in list)
            {
                item.ImageName = item.EmpPicPath;
                item.EmpPicPath = "POPResources/EmployeeProfiles/EmpPic/" + item.EmpPicPath;
            }

            return list;
        }

        public void DeleteDataSetsForEmp(IEnumerable<AccessControllerEmployeeTag> DataToDelete)
        {


            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                foreach (var item in DataToDelete)
                {
                    objCon.ExecuteNonQueryWrapper("Delete FROM AccessControllerEmployeeTag WHERE id='" + item.Id + "'", true, "1");
                }

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

        }//end of function
        public void SaveAdminInfo(Dictionary<string, object> data)
        {
            try
            {

                string strSql = @"SELECT * FROM mst.AccessControllerList AS acl WHERE acl.Id='" + data["Id"].ToString() + "'";

                DataSet dsRef = null;
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");

                if (dsRef.Tables[0].Rows.Count > 0)
                {
                    dsRef.Tables[0].Rows[0].BeginEdit();

                    dsRef.Tables[0].Rows[0]["AdminEnrollID"] = data["AdminEnrollID"].ToString();
                    dsRef.Tables[0].Rows[0]["AdminPassword"] = data["AdminPassword"].ToString();
                    dsRef.Tables[0].Rows[0]["AdminProxiCard"] = data["AdminProxiCard"].ToString();


                    dsRef.Tables[0].Rows[0].EndEdit();
                }
                clsStaticInfo info = new clsStaticInfo();
                info.SaveData(ref dsRef);
            }
            catch (Exception)
            {

                throw;
            }


        }
        public void SaveDataSetsForEmp(IEnumerable<AccessControllerEmployeeTag> DataToSave)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            DataSet dsRef;
            try
            {
                // string plantID, string DeviceID,


                if (DataToSave.Count() == 0)
                    return;

                List<AccessControllerEmployeeTag> items = DataToSave.ToList();



                string strSql = @"
                                SELECT * FROM AccessControllerEmployeeTag WHERE DeviceSystemID IN(
	
	                                 SELECT TOP 1  id FROM MST.AccessControllerList
                                                           WHERE plantid = '" + items[0].PlantID + "' AND (MachineIP ='" + items[0].DeviceIP + "' OR  id = '" + items[0].DeviceSystemID + @"')
                                )";


                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");



                strSql = @" SELECT TOP 1  * FROM MST.AccessControllerList
                                                           WHERE plantid = '" + items[0].PlantID + "' AND (MachineIP ='" + items[0].DeviceIP + "' OR  id = '" + items[0].DeviceSystemID + @"')";

                DataSet dsDevice;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsDevice, false, "1");
                if (dsDevice.Tables[0].Rows.Count == 0)
                    return;


                strSql = @"SELECT SystemID from EmployeeInformation where EmployeeStatus='ACTIVE' and plantID='" + items[0].PlantID + @"'";
                DataSet dsSystemEmployeeList;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsSystemEmployeeList, false, "1");

                string id = "";
                int index = 0;
                foreach (AccessControllerEmployeeTag item in DataToSave)
                {
                    index++;


                    //checking whether the employeeSystem id exists in the system or not
                    dsSystemEmployeeList.Tables[0].DefaultView.RowFilter = "SystemID='" + item.EmpInfoSystemID + "'";
                    if (dsSystemEmployeeList.Tables[0].DefaultView.Count == 0)
                        continue;

                    dsRef.Tables[0].DefaultView.RowFilter = "EmpInfoSystemID='" + item.EmpInfoSystemID + "'";
                    if (dsRef.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = dsRef.Tables[0].NewRow();

                        if (id == "")
                        {
                            bplib.clsGenID gen = new bplib.clsGenID();
                            gen.GenID(System.DateTime.Now.ToShortDateString(), "EMPLOYEE DEVICE TAG", out id);
                        }

                        dr["Id"] = "TAG-" + id.ToString() + index.ToString();
                        dr["GroupID"] = item.GroupID;
                        dr["PlantID"] = item.PlantID;
                        dr["EmpInfoSystemID"] = item.EmpInfoSystemID;
                        dr["DeviceSystemID"] = dsDevice.Tables[0].Rows[0]["id"].ToString();
                        dr["RegisterStatus"] = item.RegisterStatus;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = System.DateTime.Now.ToString();

                        dsRef.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dsRef.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["GroupID"] = item.GroupID;
                        dr["PlantID"] = item.PlantID;
                        dr["EmpInfoSystemID"] = item.EmpInfoSystemID;
                        dr["DeviceSystemID"] = dsDevice.Tables[0].Rows[0]["id"].ToString();
                        dr["RegisterStatus"] = item.RegisterStatus;
                        //dr["AddedBy"] = item.AddedBy;
                        //dr["AddedDate"] = System.DateTime.Now.ToString();

                        dr.EndEdit();
                    }
                }


                clsStaticInfo obs = new clsStaticInfo();
                obs.SaveDataSets(dsRef);
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
        public void SaveDataSetsForSingleEmp(IEnumerable<AccessControllerEmployeeTag> DataToSave)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            DataSet dsRef;
            try
            {
                // string plantID, string DeviceID,


                if (DataToSave.Count() == 0)
                    return;

                List<AccessControllerEmployeeTag> items = DataToSave.ToList();



                string strSql = @"
                                SELECT * FROM AccessControllerEmployeeTag WHERE  EmpInfoSystemID = '" + items[0].EmpInfoSystemID + "' AND   DeviceSystemID = '" + items[0].DeviceSystemID + @"'
                                ";


                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");



                strSql = @" SELECT TOP 1  * FROM MST.AccessControllerList
                                                           WHERE plantid = '" + items[0].PlantID + "' AND (MachineIP ='" + items[0].DeviceIP + "' OR  id = '" + items[0].DeviceSystemID + @"')";

                DataSet dsDevice;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsDevice, false, "1");
                if (dsDevice.Tables[0].Rows.Count == 0)
                    return;


                strSql = @"SELECT SystemID from EmployeeInformation where SystemId='" + items[0].EmpInfoSystemID + @"'";
                DataSet dsSystemEmployeeList;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsSystemEmployeeList, false, "1");

                string id = "";
                int index = 0;
                foreach (AccessControllerEmployeeTag item in DataToSave)
                {
                    index++;


                    //checking whether the employeeSystem id exists in the system or not
                    dsSystemEmployeeList.Tables[0].DefaultView.RowFilter = "SystemID='" + item.EmpInfoSystemID + "'";
                    if (dsSystemEmployeeList.Tables[0].DefaultView.Count == 0)
                        continue;

                    dsRef.Tables[0].DefaultView.RowFilter = "EmpInfoSystemID='" + item.EmpInfoSystemID + "'";
                    if (dsRef.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = dsRef.Tables[0].NewRow();

                        if (id == "")
                        {
                            bplib.clsGenID gen = new bplib.clsGenID();
                            gen.GenID(System.DateTime.Now.ToShortDateString(), "EMPLOYEE DEVICE TAG", out id);
                        }

                        dr["Id"] = "TAG-" + id.ToString() + index.ToString();
                        dr["GroupID"] = item.GroupID;
                        dr["PlantID"] = item.PlantID;
                        dr["EmpInfoSystemID"] = item.EmpInfoSystemID;
                        dr["DeviceSystemID"] = dsDevice.Tables[0].Rows[0]["id"].ToString();
                        dr["RegisterStatus"] = item.RegisterStatus;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = System.DateTime.Now.ToString();

                        dsRef.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dsRef.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["GroupID"] = item.GroupID;
                        dr["PlantID"] = item.PlantID;
                        dr["EmpInfoSystemID"] = item.EmpInfoSystemID;
                        dr["DeviceSystemID"] = dsDevice.Tables[0].Rows[0]["id"].ToString();
                        dr["RegisterStatus"] = item.RegisterStatus;
                        //dr["AddedBy"] = item.AddedBy;
                        //dr["AddedDate"] = System.DateTime.Now.ToString();

                        dr.EndEdit();
                    }
                }


                clsStaticInfo obs = new clsStaticInfo();
                obs.SaveDataSets(dsRef);
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

        public List<EmployeeInfomationForAccessControl> SearchAllEmployeeInformation(string strkey, string PlantID)
        {
            if (string.IsNullOrEmpty(strkey))
                strkey = "1=1";
            else
                strkey = "isnull(SystemID,'')+isnull(EmployeeCode,'')+isnull(EmployeeName,'') Like '%" + strkey + @"%'";

            string strSql = @"SELECT TOP 100 * FROM (SELECT DISTINCT CONVERT(VARCHAR(20), P.SystemID) SystemID,
                                            isnull(P.OperationMasterID,P.OperationVariationId) AS OperationMasterID, P.EmployeeName, P.EmployeeCode,p.EmpPicPath,  REPLACE(CONVERT(VARCHAR(11), P.DOB, 113),' ','-') DOB, 
                                                REPLACE(CONVERT(VARCHAR(11), P.DOJ, 113),' ','-') DOJ, p.EmployeeStatus,PC.Operation AS OperationOrVariation,
                                            P.CardNumber, ''  as DeviceStatus,P.PlantId,ISNULL(OM.Code,OV.Code) AS OperationCode,
                                            ISNULL(om.UserName,CASE WHEN ISNULL(ov.UserName,'')<>'' THEN concat(ov.UserName,'(',o.UserName,')') ELSE '' END) AS Operation,
                                            '' AS LeftFP,
                                            '' AS RightFP,
                                            Dpt.UserName AS Department, S.UserName Section, SS.UserName AS SubSection, Desg.UserName AS Designation
                                                FROM EmployeeInformation P
                                                LEFT JOIN ORG.Department Dpt ON P.DepartmentId = Dpt.id
                                                LEFT JOIN ORG.Section S ON P.SectionId = S.id
                                                LEFT JOIN ORG.SubSection SS ON P.SubSectionId = SS.id
                                                LEFT JOIN HKP.Designation Desg ON P.DesignationSystemID = Desg.Id
	                                            left join scs.PlantConfig PC on PC.PlantId=P.PlantId

	                                            left join Mst.OperationMaster OM ON OM.Id=P.OperationMasterId
	                                            LEFT JOIN mst.OperationVariation AS ov ON ov.Id=p.OperationVariationId
	                                            LEFT JOIN mst.Operation AS o ON o.Id=ov.OperationId
							
                            WHERE (P.PlantID = '" + PlantID + @"') and p.EmployeeStatus='Active') as K where " + strkey + @" " +
                            " ORDER BY SystemID";


            List<EmployeeInfomationForAccessControl> list = _sqlRepository.GetModelCollection<EmployeeInfomationForAccessControl>(strSql, null);
            foreach (var item in list)
            {
                item.ImageName = item.EmpPicPath;
                item.EmpPicPath = "POPResources/EmployeeProfiles/EmpPic/" + item.EmpPicPath;
            }

            return list;
        }
        public List<EmployeeInfomationForAccessControl> GetSingleEmployeeInformation(string employeeid, string PlantID)
        {


            string strSql = @"SELECT DISTINCT CONVERT(VARCHAR(20), P.SystemID) SystemID,
                            isnull(P.OperationMasterID,P.OperationVariationId) AS OperationMasterID, P.EmployeeName, P.EmployeeCode,p.EmpPicPath,  REPLACE(CONVERT(VARCHAR(11), P.DOB, 113),' ','-') DOB, 
                                REPLACE(CONVERT(VARCHAR(11), P.DOJ, 113),' ','-') DOJ, p.EmployeeStatus,PC.Operation AS OperationOrVariation,
                            P.CardNumber, ''  as DeviceStatus,P.PlantId,ISNULL(OM.Code,OV.Code) AS OperationCode,
                            ISNULL(om.UserName,CASE WHEN ISNULL(ov.UserName,'')<>'' THEN concat(ov.UserName,'(',o.UserName,')') ELSE '' END) AS Operation,
                            '' AS LeftFP,
                            '' AS RightFP,
                            Dpt.UserName AS Department, S.UserName Section, SS.UserName AS SubSection, Desg.UserName AS Designation
                                FROM EmployeeInformation P
                                LEFT JOIN ORG.Department Dpt ON P.DepartmentId = Dpt.id
                                LEFT JOIN ORG.Section S ON P.SectionId = S.id
                                LEFT JOIN ORG.SubSection SS ON P.SubSectionId = SS.id
                                LEFT JOIN HKP.Designation Desg ON P.DesignationSystemID = Desg.Id
	                            left join scs.PlantConfig PC on PC.PlantId=P.PlantId

	                            left join Mst.OperationMaster OM ON OM.Id=P.OperationMasterId
	                            LEFT JOIN mst.OperationVariation AS ov ON ov.Id=p.OperationVariationId
	                            LEFT JOIN mst.Operation AS o ON o.Id=ov.OperationId
                            WHERE (P.PlantID = '" + PlantID + @"') and p.SystemID='" + employeeid + "'";


            List<EmployeeInfomationForAccessControl> list = _sqlRepository.GetModelCollection<EmployeeInfomationForAccessControl>(strSql, null);
            foreach (var item in list)
            {
                item.ImageName = item.EmpPicPath;
                item.EmpPicPath = "POPResources/EmployeeProfiles/EmpPic/" + item.EmpPicPath;
            }

            return list;
        }

        public void ClearDeviceLog(string plantID, string deviceIP)
        {

            ConnectionManager.DAL.ConManager objCon = null;
            try
            {

                string sql = @"Delete FROM  AccessControllerEmployeeTag WHERE DeviceSystemID IN 
                            (SELECT MST.AccessControllerList.Id
                               FROM MST.AccessControllerList WHERE MachineIP='" + deviceIP.ToString().Trim() + "')";

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

        public List<FPInformation> GetAllSelectedEmployeesFP(string emplIDList)
        {
            string strSql = "";
            try
            {
                if (emplIDList.Contains(",") == true)
                {
                    strSql = @"SELECT EmpSystemid,FPTemplate,NULL AS FPImage,FingerName,IsLeft FROM EmployeeFPInformation AS ef
                               INNER JOIN EmployeeInformation AS ei ON ei.SystemId=ef.EmpSystemId
                                WHERE ei.EmployeeCode IN (" + emplIDList + @")
                                ORDER BY ef.EmpSystemId";

                }
                else
                {
                    strSql = @"SELECT EmpSystemid,FPTemplate,FPImage,FingerName,IsLeft FROM EmployeeFPInformation AS ef
                               INNER JOIN EmployeeInformation AS ei ON ei.SystemId=ef.EmpSystemId
                                WHERE ei.EmployeeCode IN (" + emplIDList + @")
                                ORDER BY ef.EmpSystemId";

                }


            }
            catch (Exception)
            {


            }





            var c = _sqlRepository.GetModelCollection<FPInformation>(strSql, null);
            return c;


        }
    }


    public class FPInformation : BaseModel
    {
        public string EmpSystemid { get; set; } = "";
        public string FPTemplate { get; set; } = "";
        public byte[] FPImage { get; set; }
        public string FingerName { get; set; } = "";
        public bool IsLeft { get; set; } = false;

    }

    public class EmployeeInfomationForAccessControl : BaseModel
    {
        public string SystemID { get; set; } = "";
        public string EmployeeName { get; set; } = "";
        public string EmployeeCode { get; set; } = "";
        public string DOB { get; set; } = "";
        public string DOJ { get; set; } = "";
        public string EmployeeStatus { get; set; } = "";
        public string CardNumber { get; set; } = "";
        public string DeviceStatus { get; set; } = "";
        public string LeftFP { get; set; } = "";
        public string RightFP { get; set; } = "";
        public string LeftFPTemplate { get; set; } = "";
        public string RightFPTemplate { get; set; } = "";
        public string Department { get; set; } = "";
        public string Section { get; set; } = "";
        public string SubSection { get; set; } = "";
        public string Designation { get; set; } = "";
        public string EmpPicPath { get; set; } = "";
        public string ImageName { get; set; } = "";
        public string EmployeeCurrentStatus { get; set; } = "";
        public string OperationMasterID { get; set; } = "";
        public string PlantId { get; set; } = "";
        public string OperationOrVariation { get; set; } = "";
        public string Operation { get; set; } = "";
        public string OperationCode { get; set; } = "";
    }
}
