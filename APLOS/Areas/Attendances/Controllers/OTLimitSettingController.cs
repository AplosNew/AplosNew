#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using System.Web.Mvc;
using Library.Service.Biometrics;
using System.Collections.Generic;
using Library.Model.Biometrics;
using Library.Service.Attendances;
using Library.Model.Attendances;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using OTSBD;
using System.Web.Script.Serialization;
using System;
using clsAttendance;
using Library.Data.Sql;
using Library.Service.HumanResources;
using Library.Data.UnitOfWorks;
using System.Linq;
using Aplos.Areas.Payrolls.Controllers;
#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class OTLimitSettingController : BaseController
    {
        #region Constructor

        private readonly ILeaveTransectionService _leaveTransactionService;
        private readonly IRestService _restService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRestDetailsService _restDetailsService;

        public OTLimitSettingController(

              ILeaveTransectionService leaveTransactionService
              , ISqlRepository sqlRepository
            , IRestService restService
             , IRestDetailsService restDetailsService
            , IUnitOfWork U
            )
        {
            _leaveTransactionService = leaveTransactionService;
            _restService = restService;
            _sqlRepository = sqlRepository;
            _unitOfWork = U;
            _restDetailsService = restDetailsService;
        }

        #endregion Constructor

      
        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }



        #endregion -- Pages

        #region -- Operations

        
        [HttpPost]
        public ActionResult GetOTLimitSettingList(string PlantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT otl.*,p.CompanyGroupId FROM OTLimitSetting otl 
                            LEFT JOIN ORG.Plant p on p.Id=otl.PlantID
                            where PlantId= '" + PlantId + "'";
            return Json(_sqlRepository.GetModelCollection<OTLimitSettingModel>(sql), JsonRequestBehavior.AllowGet);
        }

        
        [HttpPost]
        public JsonResult Create(OTLimitSettingModel entity)
        {
            try
            {
                SaveData(entity);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

        private string GetPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string Id = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "OTLimitSetting", out idFromDB);
            Id = "L" + idFromDB;
            sID = Id.Trim();
            return sID;

        }

        //private DataSet CheckCode(string Code, string id)
        //{
        //    GridParameter parameters;
        //    parameters = new GridParameter
        //    {
        //        ExportType = "DATASET",
        //        CmdText = @"SELECT Code FROM [HKP].[AllowanceDaily] WHERE Code='" + Code + "' and Id <>'" + id + "'"
        //    };
        //    return _sqlRepository.GetGridData(parameters).Source;

        //}
        private DataSet CheckWeekName(string Week, string id, string PlantId)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT Week FROM OTLimitSetting WHERE Week='" + Week + "' and Id <>'" + id + "' AND PlantId='" + PlantId + @"'"
            };
            return _sqlRepository.GetGridData(parameters).Source;

        }
        private DataSet CheckUserName(string UserName, string id,string PlantId)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT UserName FROM OTLimitSetting WHERE UserName='" + UserName + "' and Id <>'" + id + "' AND PlantId='"+ PlantId + @"'"
            };
            return _sqlRepository.GetGridData(parameters).Source;

        }
        private void SaveData(OTLimitSettingModel data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var UserName = CheckUserName(data.UserName, data.Id, data.PlantID);
            if (UserName.Tables[0].Rows.Count > 0)
            {
                throw new Exception("UserName already exist.");
            }


            var WeekName = CheckWeekName(data.Week, data.Id, data.PlantID);
            if (WeekName.Tables[0].Rows.Count > 0)
            {
                throw new Exception("Week already exist.");
            }


            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                string sql = "SELECT * FROM OTLimitSetting WHERE ID='" + data.Id + "' AND PlantId='" + data.PlantID + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = GetPK();
                   
                    dr["MinOTLimitParDay"] = data.MinOTLimitParDay;
                    dr["MaxOTLimitParDay"] = data.MaxOTLimitParDay;

                    //dr["MinOTLimitParWeek"] = data.MinOTLimitParWeek;
                    dr["MaxOTLimitParWeek"] = data.MaxOTLimitParWeek;

                    dr["MaxWeekOffOTLimitParDay"] = data.MaxWeekOffOTLimitParDay;
                    dr["MaxHolidayOTLimitParDay"] = data.MaxHolidayOTLimitParDay;

                    dr["OTReductionFactor"] = data.OTReductionFactor;

                    dr["UserName"] = data.UserName;
                    dr["Description"] = data.Description;
                  
                    dr["Week"] = data.Week;
                    //dr["ToDay"] = data.ToDay;
                    dr["Active"] = data.Active;
                    dr["PlantID"] = data.PlantID;
                
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();
                    dr["MinOTLimitParDay"] = data.MinOTLimitParDay;
                    dr["MaxOTLimitParDay"] = data.MaxOTLimitParDay;


                    dr["MaxWeekOffOTLimitParDay"] = data.MaxWeekOffOTLimitParDay;
                    dr["MaxHolidayOTLimitParDay"] = data.MaxHolidayOTLimitParDay;

                    //dr["MinOTLimitParWeek"] = data.MinOTLimitParWeek;
                    dr["MaxOTLimitParWeek"] = data.MaxOTLimitParWeek;

                    dr["OTReductionFactor"] = data.OTReductionFactor;

                    dr["UserName"] = data.UserName;
                    dr["Description"] = data.Description;                 
                    dr["Week"] = data.Week;
                    //dr["ToDay"] = data.ToDay;
                    dr["Active"] = data.Active;
                    dr["PlantID"] = data.PlantID;                  
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dr.EndEdit();
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            DeleteData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM OTLimitSetting WHERE Id = '" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        [HttpGet,Authorize]
        public ActionResult GetEditData(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT s.*,c.Id CompanyIds FROM OTLimitSetting s
                        left join ORG.Plant p on p.Id=s.PlantID
                        left join ORG.Company c on c.Id=p.CompanyId
                        where s.Id='" + Id + "'";
            return Json(_sqlRepository.GetModelCollection<OTLimitSettingModel>(sql), JsonRequestBehavior.AllowGet);
        }
        #endregion





    }
    public class OTLimitSettingModel: BaseModel
    {
        public string Id { get; set; }
        public string CompanyIds { get; set; }
        public string PlantID { get; set; }
        public string Week { get; set; }
       
        public string UserName { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; } = false;
        public decimal MinOTLimitParDay { get; set; } = 0;
        public decimal MaxOTLimitParDay { get; set; } = 0;
        //public decimal MinOTLimitParWeek { get; set; } = 0;
        public decimal MaxOTLimitParWeek { get; set; } = 0;
        public decimal OTReductionFactor { get; set; } = 0;
        public decimal MaxWeekOffOTLimitParDay { get; set; } = 0;
        public decimal MaxHolidayOTLimitParDay { get; set; } = 0;
        public string PlantName { get; set; }
        //public string Week { get; set; }
    }

}