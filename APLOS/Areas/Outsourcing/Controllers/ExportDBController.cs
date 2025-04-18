using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Security.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.Outsourcing.Controllers
{
    public class ExportDBController : BaseController
    {
        #region Constructor
        private readonly SqlRepository _sqlRepository;
        public ExportDBController(SqlRepository Repository)
        {
            _sqlRepository = Repository;
        }
        #endregion
        #region Pages
        // GET: IE/JobWorkItem
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion
        #region Code
        [HttpGet, Authorize]
        public JsonResult GetAllData()
        {
            string sql = "";
            sql = @"select (Select Id from [TRN].[ExportDB]  where InvoiceNo = Sa.Id) Id , Sa.Id InvoiceNo ,Format( Sa.InvoiceDate , 'dd-MM-yyyy') InvoiceDate
                    ,case when(Select InvoiceNo from [TRN].[ExportDB]  where InvoiceNo = Sa.Id) = Sa.Id then (Select SBNumber from [TRN].[ExportDB]  where InvoiceNo = Sa.Id) else (select ShippingBillNo from [dbo].[PostSalesInvoice] where SalesId = SA.Id) end SBNumber
                    ,case when(Select InvoiceNo from [TRN].[ExportDB]  where InvoiceNo = Sa.Id) = Sa.Id then (Select Format( SBDate , 'dd-MM-yyyy') from [TRN].[ExportDB]  where InvoiceNo = Sa.Id) else (select Format( ShippingBillDate , 'dd-MM-yyyy') from [dbo].[PostSalesInvoice] where SalesId = SA.Id) end SBDate
                    ,case when(Select InvoiceNo from [TRN].[ExportDB]  where InvoiceNo = Sa.Id) = Sa.Id then (Select PortCode from [TRN].[ExportDB]  where InvoiceNo = Sa.Id) else (select PortCode from [dbo].[PostSalesInvoice] where SalesId = SA.Id) end PortCode
                    ,case when(Select InvoiceNo from [TRN].[ExportDB]  where InvoiceNo = Sa.Id) = Sa.Id then (Select InvoiceValue from [TRN].[ExportDB]  where InvoiceNo = Sa.Id) else (select Convert(decimal(10,2) , SUM(BaseAmount)) from [TRN].[SalesMaterial] where SalesId = SA.Id Group By SalesId) end InvoiceValue
                    ,(Select EXRate from [TRN].[ExportDB]  where InvoiceNo = Sa.Id) EXRate
                    ,(Select FOBValueInr from [TRN].[ExportDB]  where InvoiceNo = Sa.Id) FOBValueInr
                    ,(Select RODTEP from [TRN].[ExportDB]  where InvoiceNo = Sa.Id) RODTEP
                    ,(Select DBKValue from [TRN].[ExportDB]  where InvoiceNo = Sa.Id) DBKValue
					,case when(Select InvoiceNo from [TRN].[ExportDB]  where InvoiceNo = Sa.Id) = Sa.Id then (Select IGSTAmount from [TRN].[ExportDB]  where InvoiceNo = Sa.Id) else (select Convert(decimal(10,2) , SUM(BooksCurrencyTaxAmount)) from [TRN].[SalesMaterial] where SalesId = SA.Id Group By SalesId) end IGSTAmount
                    ,(Select CommPercentage from [TRN].[ExportDB]  where InvoiceNo = Sa.Id) CommPercentage
                    ,(Select CommAmount from [TRN].[ExportDB]  where InvoiceNo = Sa.Id) CommAmount
                    ,(Select InsuranceAmount from [TRN].[ExportDB]  where InvoiceNo = Sa.Id) InsuranceAmount
                    ,(Select Incoterms from [TRN].[ExportDB]  where InvoiceNo = Sa.Id) Incoterms
					,(Select CommDoller from [TRN].[ExportDB]  where InvoiceNo = Sa.Id) CommDoller
					,(Select FOBDoller from [TRN].[ExportDB]  where InvoiceNo = Sa.Id) FOBDoller
					,(Select InsuranceDoller from [TRN].[ExportDB]  where InvoiceNo = Sa.Id) InsuranceDoller
					,(Select Fright from [TRN].[ExportDB]  where InvoiceNo = Sa.Id) Fright
					,(Select FrightDoller from [TRN].[ExportDB]  where InvoiceNo = Sa.Id) FrightDoller
					,(Select UserName from HKP.Party where Id = Sa.PartyId ) Customer
                    from trn.Sales Sa
                    where  Sa.InvoiceDate >= '2023-04-01' and  Sa.CurrencyId = 12  order by Sa.InvoiceDate Desc ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetSelectedData(string Id)
        {
            string sql = "";
            sql = @"Select  
 Case when (Select InvoiceNo from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) = Sa.Id  then (Select InvoiceNo from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) else Sa.Id end InvoiceNo
,Case when (Select InvoiceNo from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) = Sa.Id  then (Select Format(InvoiceDate,'dd-MM-yyyy') from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) else Format(Sa.InvoiceDate,'dd-MM-yyyy') end InvoiceDate
, Case when (Select InvoiceNo from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) = Sa.Id  then (Select Id from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) else null end Id
, Case when (Select InvoiceNo from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) = Sa.Id  then (Select SBNumber from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) else (select ShippingBillNo from [dbo].[PostSalesInvoice] where SalesId = SA.Id) end SBNumber
,Case when (Select InvoiceNo from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) = Sa.Id  then (Select Format(SBDate,'dd-MM-yyyy') from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) else (select Format( ShippingBillDate , 'dd-MM-yyyy') from [dbo].[PostSalesInvoice] where SalesId = SA.Id) end SBDate
, Case when (Select InvoiceNo from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) = Sa.Id  then (Select PortCode from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) else (select PortCode from [dbo].[PostSalesInvoice] where SalesId = SA.Id) end PortCode
, Case when (Select InvoiceNo from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) = Sa.Id  then (Select InvoiceValue from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) else (select Convert(decimal(10,2) , SUM(BaseAmount)) from [TRN].[SalesMaterial] where SalesId = SA.Id Group By SalesId) end InvoiceValue
, Case when (Select InvoiceNo from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) = Sa.Id  then (Select EXRate from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) else null end EXRate
, Case when (Select InvoiceNo from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) = Sa.Id  then (Select FOBValueInr from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) else null end FOBValueInr
, Case when (Select InvoiceNo from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) = Sa.Id  then (Select RODTEP from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) else null end RODTEP
, Case when (Select InvoiceNo from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) = Sa.Id  then (Select DBKValue from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) else null end DBKValue
, Case when (Select InvoiceNo from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) = Sa.Id  then (Select IGSTAmount from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) else (select Convert(decimal(10,2) , SUM(BooksCurrencyTaxAmount)) from [TRN].[SalesMaterial] where SalesId = SA.Id Group By SalesId) end IGSTAmount
, Case when (Select InvoiceNo from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) = Sa.Id  then (Select CommPercentage from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) else null end CommPercentage
, Case when (Select InvoiceNo from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) = Sa.Id  then (Select CommAmount from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) else null end CommAmount
, Case when (Select InvoiceNo from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) = Sa.Id  then (Select InsuranceAmount from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) else null end InsuranceAmount
, Case when (Select InvoiceNo from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) = Sa.Id  then (Select Incoterms from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) else null end Incoterms
, Case when (Select InvoiceNo from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) = Sa.Id  then (Select CommDoller from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) else null end CommDoller
, Case when (Select InvoiceNo from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) = Sa.Id  then (Select FOBDoller from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) else null end FOBDoller
, Case when (Select InvoiceNo from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) = Sa.Id  then (Select InsuranceDoller from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) else null end InsuranceDoller
, Case when (Select InvoiceNo from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) = Sa.Id  then (Select Fright from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) else null end Fright
, Case when (Select InvoiceNo from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) = Sa.Id  then (Select FrightDoller from [TRN].[ExportDB] EDB where Sa.Id = EDB.InvoiceNo) else null end FrightDoller
,PT.UserName Customer
from TRN.Sales Sa
                        left join [dbo].[PostSalesInvoice] PSI on PSI.SalesId = Sa.Id
						left join HKP.Party PT on PT.Id = Sa.PartyId
                        Where Sa.InvoiceDate >= '2023-04-01'  and Sa.CurrencyId = 12 and Sa.Id = '" + Id + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public ActionResult DeleteSelectedData(string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id.ToString()))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("DELETE FROM [HKP].[JobWorkItem] WHERE Id='" + Id.ToString() + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobWorkEntry", out sID);
            return sID;
        }

        [HttpPost, Authorize]
        public ActionResult SaveData(Dictionary<string, object> saveData)
        {
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            DataSet dsDuplicate = new DataSet();
            /*if (saveData["Id"]==null)
            {
                con.getDataSet("SELECT * FROM trn.JobWorkEntry WHERE GateEntryNo = '" + saveData["GateEntryNo"] + "'", out dsDuplicate);
                if (dsDuplicate.Tables[0].Rows.Count > 0)
                    return Json(new { Error = true, Message = "Gate Entry No. Already Exists..!" }, JsonRequestBehavior.AllowGet);
            }
           */

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID _genId = new bplib.clsGenID();
                string _Message = "";
                string Id = "";
                con.getDataSet("SELECT * FROM trn.ExportDB WHERE Id='" + saveData["Id"] + "'", out DataSet dsOut);
                if (dsOut.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsOut.Tables[0].NewRow();
                    _genId.GenID("trn.ExportDB", out Id);
                 //   Id = "JWI-" + Id;
                    dr["Id"] = "EDB" + Id;
                    dr["InvoiceNo"] = saveData["InvoiceNo"];
                    dr["InvoiceDate"] = saveData["InvoiceDate"];
                    dr["SBNumber"] = saveData["SBNumber"];
                    dr["SBDate"] = saveData["SBDate"];
                    dr["PortCode"] = saveData["PortCode"];
                    dr["InvoiceValue"] = saveData["InvoiceValue"];
                    dr["EXRate"] = saveData["EXRate"];
                    dr["FOBValueInr"] = saveData["FOBValueInr"];
                    dr["RODTEP"] = saveData["RODTEP"];
                    dr["DBKValue"] = saveData["DBKValue"];
                    dr["IGSTAmount"] = saveData["IGSTAmount"];
                    dr["CommPercentage"] = saveData["CommPercentage"];
                    dr["CommAmount"] = saveData["CommAmount"];
                    dr["InsuranceAmount"] = saveData["InsuranceAmount"];
                    dr["Incoterms"] = saveData["Incoterms"];
                    dr["CommDoller"] = saveData["CommDoller"];
                    dr["FOBDoller"] = saveData["FOBDoller"];
                    dr["InsuranceDoller"] = saveData["InsuranceDoller"];
                    dr["Fright"] = saveData["Fright"];
                    dr["FrightDoller"] = saveData["FrightDoller"];
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsOut.Tables[0].Rows.Add(dr);

                    _Message = "Data Save Successfully..!";

                }
                else
                {
                    DataRow dr = dsOut.Tables[0].DefaultView[0].Row;
                    //Id = dr["Id"].ToString();
                    dr.BeginEdit();

                    dr["InvoiceNo"] = saveData["InvoiceNo"];
                    dr["InvoiceDate"] = saveData["InvoiceDate"];
                    dr["SBNumber"] = saveData["SBNumber"];
                    dr["SBDate"] = saveData["SBDate"];
                    dr["PortCode"] = saveData["PortCode"];
                    dr["InvoiceValue"] = saveData["InvoiceValue"];
                    dr["EXRate"] = saveData["EXRate"];
                    dr["FOBValueInr"] = saveData["FOBValueInr"];
                    dr["RODTEP"] = saveData["RODTEP"];
                    dr["DBKValue"] = saveData["DBKValue"];
                    dr["IGSTAmount"] = saveData["IGSTAmount"];
                    dr["CommPercentage"] = saveData["CommPercentage"];
                    dr["CommAmount"] = saveData["CommAmount"];
                    dr["InsuranceAmount"] = saveData["InsuranceAmount"];
                    dr["Incoterms"] = saveData["Incoterms"];
                    dr["CommDoller"] = saveData["CommDoller"];
                    dr["FOBDoller"] = saveData["FOBDoller"];
                    dr["InsuranceDoller"] = saveData["InsuranceDoller"];
                    dr["Fright"] = saveData["Fright"];
                    dr["FrightDoller"] = saveData["FrightDoller"];
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();

                    _Message = "Data Updated Successfully..!";
                }

                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsOut);

                return Json(new { Error = false, Message = _Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetUOMList()
        {
            try
            {
                string sql = "";
                sql = @"SELECT Id,UserName FROM [SCS].[UnitOfMeasurement] WHERE Active = 1 ORDER BY UserName";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetSequenceNumber()
        {
            try
            {
                string sql = "";
                sql = @"SELECT  isnull(Max(Sequence),0)+1 AS Sequence FROM trn.JobWorkEntry";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult getMatbaseUOM(string MatId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select mm.Id, mm.Code, mm.UserName as MaterialName,mc.UserName as MaterialCategory, mgm.UserName as MaterialGroupMaster,mm.BaseUOMId, buom.UserName as BaseUOM
                                      from MST.MaterialMaster mm left join MST.MaterialGroupMaster mgm on mm.MaterialGroupMasterId=mgm.Id
									  left join SCS.UnitOfMeasurement buom on buom.Id=mm.BaseUOMId
									  left join HKP.MaterialCategory mc on mc.Id=mm.MaterialCategoryId
                                      WHERE mm.CompanyGroupId='" + identity.CompanyGroupId + @"'
                                      AND mm.Id='"+ MatId + @"' order by mm.Code ";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult LoadAllMaterialMstDetails(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select GE.Id GateEntryNo , format(GE.GateEntryTime,'dd-MMM-yyyy') GateEntryDate , GE.PackageQty Quantity , PT.StandardName Party , GE.Remarks , JWE.Id   from trn.GateEntry GE
left join hkp.Party PT on PT.Id = GE.PartyId 
left join trn.JobWorkEntry JWE on JWE.GateEntryNo = GE.Id
where JWE.Id is null and Convert(date,GE.GateEntryTime) between '01-Feb-2025' and GETDATE()  
order by GE.GateEntryTime desc  ";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult LoadResponsiblePersonDetails(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS Id, EMP.EmployeeStatus,
                        EMP.EmployeeName,EMP.EmployeeCode AS Code,
                        EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName DepartmentName,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
    
                        WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and emp.CompanyId='" + identity.CompanyId + @"' and emp.EmployeeStatus='Active' and EMP.EmpType='Local'
                   AND isnull(Emp.SystemID,'') not in (select isnull(ResponsiblePersonId,'') from HKP.JobWorkItem where Id='" + Id + @"')
                  order by EMP.EmployeeCode";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        #endregion
    }
}