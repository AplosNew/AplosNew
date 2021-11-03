using Library.Data.Sql;
using Library.Model.Attendances;
using Library.Model.Organizations;
using Library.Service.Attendances;
using Library.Service.Biometrics;
using Library.Service.MobileAPI;
using Library.Service.Modules;
using Library.Service.Organizations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Mvc;

namespace Aplos.Controllers
{
    public class MobileController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMobileService _AccessControlService;
        public MobileController(IMobileService AccessControlService, ISqlRepository R)
        {
            _AccessControlService = AccessControlService;
            _sqlRepository = R;
        }

        #endregion Constructor


        [HttpGet]
        public List<OperationMasterData> SearchAllOperationInformation(string strkey, string CompanyGroupId)
        {
            return _AccessControlService.SearchOperationMasterData(strkey, CompanyGroupId);
        }
        [HttpGet]
        public JsonResult SearchAllOperationInformationByPlant(string strKey, string CompanyGroupId, string PlantId)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            List<OperationMasterData> DataList = new List<OperationMasterData>();
            System.Data.DataSet dsRef;
            try
            {
                if (string.IsNullOrEmpty(strKey))
                    strKey = "1=1";
                else
                    strKey = "isnull(code,'')+isnull(username,'') like '%" + strKey + "%'";


                DataTable dt = _sqlRepository.GetDataTable(@"select * from scs.PlantConfig where PlantId='" + PlantId + @"'");
                strSql = @"select * from (select * from mst.OperationMaster where CompanyGroupId='" + CompanyGroupId + "') AS K where " + strKey;
                if (dt.Rows.Count > 0)
                {

                    if (dt.Rows[0]["Operation"].ToString().ToUpper() == "OPERATION VARIATION")
                    {
                        strSql = @"select * from (
                                SELECT ov.Id,ov.Code,concat(ov.UserName,'(',o.UserName,')') AS UserName FROM mst.OperationVariation AS ov
                                JOIN mst.Operation AS o ON o.Id=ov.OperationId) AS K where " + strKey;

                    }
                }

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");

                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    DataList.Add(new OperationMasterData
                    {
                        ID = dsRef.Tables[0].Rows[i]["Id"].ToString(),
                        Code = dsRef.Tables[0].Rows[i]["Code"].ToString(),
                        UserName = dsRef.Tables[0].Rows[i]["UserName"].ToString(),

                    });
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
            return Json(DataList, JsonRequestBehavior.AllowGet);
           
        }
        [HttpGet]
        public string saveEmployeeOperation(string EmployeeID, string OperationId, string PlantId)
        {
            ConnectionManager.DAL.ConManager objCon;
            System.Data.DataSet dsRef;
            try
            {


                DataTable dt = _sqlRepository.GetDataTable(@"select * from scs.PlantConfig where PlantId='" + PlantId + @"'");
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("Select * from EmployeeInformation Where SystemId='"+ EmployeeID + "'", out dsRef, false, false, "", "1");

                DataRow dr = dsRef.Tables[0].Rows[0];
                dr.BeginEdit();
                dr["OperationMasterID"] = bplib.clsWebLib.RetValidLen(OperationId);
                dr["OperationVariationId"] =DBNull.Value;
                if (dt.Rows.Count > 0)
                {

                    if (dt.Rows[0]["Operation"].ToString().ToUpper() == "OPERATION VARIATION")
                    {
                        dr["OperationVariationId"] = bplib.clsWebLib.RetValidLen(OperationId);
                        dr["OperationMasterID"] = DBNull.Value;
                    }
                }
                dr.EndEdit();

                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsRef);

               
            }
            catch (System.Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                objCon = null;
            }
            return "Data Updated Successfully";
        }

        [HttpGet]
        public List<OperationMasterData> GetOperationMasterData(string id)
        {
            return _AccessControlService.GetOperationMasterData(id);
        }
        [HttpGet]
        public JsonResult GetPlant(string companyid)
        {
            string sql = @"SELECT * FROM org.Plant AS p WHERE p.CompanyId='" + companyid + "' AND p.[Active]=1";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetEntity(string companyid)
        {
            string sql = @"SELECT e.Id, e.PlantId,p.UserName AS PlantName, e.UserName AS EntityName
                              FROM org.Entity AS e 
                            INNER JOIN org.Plant AS p ON p.id=e.PlantId
                            WHERE e.[Active]=1 AND e.IsProduction=1
                            and e.CompanyId='" + companyid + "' order by e.UserName";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

    }


}
