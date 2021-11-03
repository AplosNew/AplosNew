#region Using
using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using System;
using System.Data;
using OTSBD;
using clsAttendance;
using System.Collections.Generic;

#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class BonusPolicyController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IStoppageService _stoppageService;
        public BonusPolicyController(
              IStoppageService stoppageService,
              ISqlRepository sqlRepository
            )
        {
            _stoppageService = stoppageService;
            _sqlRepository = sqlRepository;
        }
        #endregion

        #region -- Pages
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [HttpGet, Authorize]
        public ActionResult GetSalaryHeadListeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" select SalaryHeadID as Id,SalaryHead as UserName from [dbo].[SalaryHead]";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select M.SystemID AS MID,M.PolicyName,M.BonusDescription,M.DefaultPolicy, M.GroupID,M.EntitleFrm,M.ServiceLengthType
                            from [dbo].[BonusPolicyMaster] as M";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPlantBonusPolicy(string plantID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT IsSelectPolicy = Case WHEN p.BonusPolicyID IS NULL THEN Convert(bit, 'False')
                            ELSE Convert(bit, 'True') END, b.SystemID BonusPolicyID, b.PolicyName,p.ID 
                            FROM BonusPolicyMaster b
                            LEFT JOIN BonusPolicyPlantWise p ON p.BonusPolicyID = b.SystemID  
                            and p.PlantId = '" + plantID + "'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPlantBonusPolicyy(string PolicyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select IsSelectPolicy = case when pw.PlantId is null then CONVERT(bit, 'False') else Convert(bit, 'True') END
                            ,p.UserName PlantName,m.SystemID BonusPolicyID,p.Id PlantId,pw.ID,c.UserName CompanyName
                            from ORG.Plant p
                            left join BonusPolicyPlantWise pw on pw.PlantId=p.Id and pw.BonusPolicyID= '" + PolicyId + @"'
                            left join BonusPolicyMaster m on m.SystemID=pw.BonusPolicyID
                            left join ORG.Company c on c.Id= p.CompanyId";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetPlant()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select  p.Id PlantId,p.UserName PlantName, c.Id CompanyId,c.UserName as CompanyName
                         from ORG.Plant p
                         left join [ORG].[Company] c on c.Id = p.CompanyId ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetDetailsList(string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" select D.SystemID,M.SystemID as MID,D.DisbursementType,D.BPMSystemID,M.EntitleFrm --,D.EmpCategorySysID
                            ,D.MinServLen,D.MaxServLen,D.IsFixed,D.IsPercentage,D.IsProportionate
                            ,D.FixedAmount,D.PerctSalaryHeadID,D.BonusPercentage,D.DivisionFactor,D.MinBonusAmt,M.ServiceLengthType
                            from [dbo].[BonusPolicyMaster] M
                            LEFT JOIN [dbo].[BonusPolicyDetail] D on d.BPMSystemID=m.SystemID
                            WHERE D.BPMSystemID='" + MasterId + @"'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveM(BonusPolicyMaster Master)
        {
            try
            {
                string MasterId = string.Empty;
                MasterId = SaveMaster(Master);
                return Json(new { MasterId, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost, Authorize]
        public ActionResult Save(List<Dictionary<string, object>> Details, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                string DetailsId = string.Empty;
                string sql = "SELECT * FROM [dbo].[BonusPolicyDetail] WHERE BPMSystemID='" + MasterId + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                while (dsMaster.Tables[0].DefaultView.Count > 0)
                {
                    dsMaster.Tables[0].DefaultView[0].Delete();
                }

                for (int i = 0; i < Details.Count; i++)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[BonusPolicyDetail]", out sID);
                    DetailsId = "BPD" + sID;
                    dr["SystemID"] = DetailsId;
                    dr["BPMSystemID"] = MasterId;
                    //dr["EmpCategorySysID"] = Details[i]["EmpCategorySysID"];
                    dr["MinServLen"] = clsStaticInfo.dbl(Details[i]["MinServLen"]);
                    dr["MaxServLen"] = clsStaticInfo.dbl(Details[i]["MaxServLen"]);
                    dr["FixedAmount"] = clsStaticInfo.dbl(Details[i]["FixedAmount"]);

                    if (Details[i]["PerctSalaryHeadID"] == null)
                    {
                        dr["PerctSalaryHeadID"] = DBNull.Value;
                    }
                    else
                    {
                        dr["PerctSalaryHeadID"] = Details[i]["PerctSalaryHeadID"].ToString();
                    }
                    
                    dr["BonusPercentage"] = clsStaticInfo.dbl(Details[i]["BonusPercentage"]);
                    dr["DivisionFactor"] = clsStaticInfo.dbl(Details[i]["DivisionFactor"]);
                    dr["MinBonusAmt"] = clsStaticInfo.dbl(Details[i]["MinBonusAmt"]);

                    if (Details[i]["DisbursementType"].ToString() == "Fixed")
                    {
                        dr["IsFixed"] = true;
                    }
                    else
                    {
                        dr["IsFixed"] = false;
                    }
                    if (Details[i]["DisbursementType"].ToString() == "Percentage")
                    {
                        dr["IsPercentage"] = true;
                    }
                    else
                    {
                        dr["IsPercentage"] = false;
                    }
                    if (Details[i]["DisbursementType"].ToString() == "Proportionate")
                    {
                        dr["IsProportionate"] = true;
                    }
                    else
                    {
                        dr["IsProportionate"] = false;
                    }
                    dr["DisbursementType"] = /*"Fixed"*/ Details[i]["DisbursementType"].ToString();
                    dr["AddedBy"] = identity.Name;
                    dr["DateAdded"] = DateTime.Now;

                    dsMaster.Tables[0].Rows.Add(dr);

                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost, Authorize]
        public ActionResult SaveBP(List<BonusPolicyPlantWise> BP)
        {
            try
            {
                SaveBPlant(BP);
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost, Authorize]
        public string SaveMaster(BonusPolicyMaster Master)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                string Id = string.Empty;

                string sql = "SELECT * FROM [dbo].[BonusPolicyMaster] WHERE SystemID='" + Master.MID + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[BonusPolicyMaster]", out sID);
                    Id = "BPM" + sID;
                    dr["SystemID"] = Id;
                    dr["PolicyName"] = Master.PolicyName;
                    dr["BonusDescription"] = Master.BonusDescription;
                    dr["GroupID"] = identity.CompanyGroupId;
                    //dr["PlantID"] = Master.PlantID;
                    dr["DefaultPolicy"] = Master.DefaultPolicy;
                    dr["EntitleFrm"] = Master.EntitleFrm;
                    dr["ServiceLengthType"] = Master.ServiceLengthType;

                    dr["AddedBy"] = identity.Name;
                    dr["DateAdded"] = DateTime.Now;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    Id = dr["SystemID"].ToString();

                    dr["PolicyName"] = Master.PolicyName;
                    dr["BonusDescription"] = Master.BonusDescription;
                    dr["DefaultPolicy"] = Master.DefaultPolicy;
                    dr["EntitleFrm"] = Master.EntitleFrm;
                    dr["ServiceLengthType"] = Master.ServiceLengthType;

                    dr["UpdatedBy"] = identity.Name;
                    dr["DateUpdated"] = System.DateTime.Now.ToString();

                    dr.EndEdit();
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                return Id;
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveBPlant(List<BonusPolicyPlantWise> BP)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                DataTable dtBp = null;
                DataSet dsBp = null;
                DataView dvBp = null;
                DataRow drBp = null;
                string BPId = string.Empty;
                string sql = "SELECT * FROM [dbo].[BonusPolicyPlantWise] where PlantId= '" + BP[0].PlantId+@"' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsBp, false, "1");

                bplib.clsGenID objGenID = null;
                objGenID = new bplib.clsGenID();
                objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "Bonus_POLICY_P", out BPId);
                int count = 0;

                for (int i = dsBp.Tables[0].Rows.Count - 1; i >= 0; i--)
                {
                    string PlantID = dsBp.Tables[0].Rows[i]["PlantId"].ToString();
                    foreach (var item in BP)
                    {
                        if (item.PlantId == PlantID && item.IsSelectPolicy == false)
                        {
                            DataView dv = new DataView(dsBp.Tables[0]);
                            dv.RowFilter = "ID='" + item.ID + "'";
                            if (dv.Count > 0)
                            {
                                Delete(item.ID);
                            }
                        }
                    }
                }


                objCon.OpenDataSetThroughAdapter(sql, out dsBp, false, "1");

                foreach (var item in BP)
                {

                    if (item.IsSelectPolicy == true)
                    {
                        dvBp = new DataView(dsBp.Tables[0]);
                        //dvBp.Table = ;
                        dvBp.RowFilter = " BonusPolicyID='" + item.BonusPolicyID + "' and plantID='" + item.PlantId + "' ";

                        if (dvBp.Count == 0)
                        {
                            count++;
                            string pk = "B_P_P" + BPId + "_" + count;
                            drBp = dsBp.Tables[0].NewRow();
                            drBp["ID"] = pk;
                            drBp["BonusPolicyID"] = item.BonusPolicyID;
                            drBp["PlantId"] = item.PlantId;

                            drBp["AddedBy"] = identity.Name;
                            drBp["AddedDate"] = DateTime.Now;
                            drBp["AddedFromIP"] = identity.IPAddress;

                            dsBp.Tables[0].Rows.Add(drBp);
                        }

                    }
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsBp);
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void Delete(string ID)
        {
            try
            {
                if (string.IsNullOrEmpty(ID))
                {
                    throw new Exception("Select Id first");
                }
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from [dbo].[BonusPolicyPlantWise] where ID ='" + ID + "'");

                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpPost]
        public ActionResult DeleteM(string SystemID)
        {
            DataSet dsMaster;
            DataSet dsChild;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string sql = "SELECT * FROM [dbo].[BonusPolicyPlantWise] WHERE BonusPolicyId='" + SystemID + "' ";
                string sql1 = "SELECT * FROM  [dbo].[BonusPolicyDetail] WHERE BPMSystemID='" + SystemID + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql1, out dsChild, false, "1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    Exception ex = new Exception("This Policy tagged with Plant...");
                    throw (ex);
                }
                if (dsChild.Tables[0].Rows.Count > 0)
                {
                    Exception ex = new Exception("Delete Details First..");
                    throw (ex);
                }
                //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from [dbo].[BonusPolicyDetail] where BPMSystemID='" + SystemID + "'");
                con.executeQuery("delete from [dbo].[BonusPolicyMaster] where SystemID='" + SystemID + "'");
                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult DeleteDetails(string DetailsId)
        {
            string strDetailsSQL;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                strDetailsSQL = "DELETE FROM  [dbo].[BonusPolicyDetail] WHERE BPMSystemID='" + DetailsId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strDetailsSQL, out dsExceptionEmployeeList, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }
        public class BonusPolicyMaster : BaseModel
        {
            #region Scalar Properties            
            public string SystemID { get; set; }
            public string MID { get; set; }
            public string PolicyName { get; set; }
            public string BonusDescription { get; set; }
            public string EntitleFrm { get; set; }
            public string ServiceLengthType { get; set; }
            public bool DefaultPolicy { get; set; }
            //public string GroupID { get; set; }
            //public string PlantID { get; set; }

            #endregion Scalar Properties

            #region Audit Properties
            [NeverUpdate]
            public string AddedBy { get; set; }
            [NeverUpdate]
            public DateTime? DateAdded { get; set; }

            public string UpdatedBy { get; set; }
            public DateTime? DateUpdated { get; set; }

            #endregion Audit Properties
        }
        public class BonusPolicyPlantWise : BaseModel
        {
            #region Scalar Properties            
            public string ID { get; set; }
            public string BonusPolicyID { get; set; }
            public string PlantId { get; set; }
            public bool IsSelectPolicy { get; set; }

            #endregion Scalar Properties

            #region Audit Properties
            [NeverUpdate]
            public string AddedBy { get; set; }
            public string AddedFromIP { get; set; }
            [NeverUpdate]
            public DateTime? AddedDate { get; set; }

            public string UpdatedBy { get; set; }
            public string UpdatedFromIP { get; set; }
            public DateTime? UpdatedDate { get; set; }

            #endregion Audit Properties
        }

        public class BonusPolicyDetail : BaseModel
        {
            #region Scalar Properties  
            public string ID { get; set; }
            public string SystemID { get; set; }
            public string BPMSystemID { get; set; }
            public string EmpCategorySysID { get; set; }
            public int MinServLen { get; set; }
            public int MaxServLen { get; set; }
            public string DisbursementType { get; set; }
            public string FixedAmount { get; set; }
            public string PerctSalaryHeadID { get; set; }
            public decimal BonusPercentage { get; set; }
            public decimal DivisionFactor { get; set; }
            public string MID { get; set; }
            public int MinBonusAmt { get; set; }
            public bool IsFixed { get; set; }
            public bool IsPercentage { get; set; }
            public bool IsProportionate { get; set; }

            #endregion Scalar Properties


            #region Audit Properties
            [NeverUpdate]
            public string AddedBy { get; set; }
            [NeverUpdate]
            public DateTime? DateAdded { get; set; }

            public string UpdatedBy { get; set; }
            public DateTime? DateUpdated { get; set; }

            #endregion Audit Properties

        }

        #endregion
    }
}