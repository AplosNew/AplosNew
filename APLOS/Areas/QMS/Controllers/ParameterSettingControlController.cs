using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Materials;
using Library.Security.Core;
using Library.Service.Helpers;
using Library.Service.Materials;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.QMS.Controllers
{
    public class ParameterSettingControlController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        public ParameterSettingControlController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult LoadParameterSettingControl(string FromDate, string ToDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string
                sql = @"select PAG.UserName CustomerType,XP.UserName Customer,
MOI.Id LineItemNo,MM.UserName Material,MA.StandardName Article,MOI.BuyerReferenceNo,MOI.OwnReferenceNo,
PL.Code ProductLibraryCode,PL.UserName ProductLibrary,MOI.ProductionGrouping,POD.ProductionOrderId PONo,MOI.TotalQty ItemQty,
MOI.Remark,PS.UserName POStatus,CUP.Id,CUP.EmployeeId,CUP.ApprovedById,CUP.CriticalLevel,CUP.Remarks,MO.Id MasterOrderNo
from TRN.SalesOrder SO
left join TRN.MasterOrderItem MOI on MOI.Id=SO.MasterOrderItemId
left join MST.MaterialMaster MM on MM.Id=MOI.MaterialMasterId 
left join [MST].[MaterialMasterArticle] MA ON MA.Id=MOI.ArticleId
left join ProductLibrary PL on PL.Id=MOI.ProductLibraryId
left join trn.ProductionOrderDetail POD on POD.SalesOrderId=SO.Id
left join trn.ProductionOrder PO on PO.Id=POD.ProductionOrderId
left join hkp.ProductionStatus PS on PS.Id=PO.ProductionStatusId
left join Trn.MasterOrder MO on MO.Id=MOI.MasterOrderId
left join [HKP].[Party] Xp on XP.Id=MO.PartyId
left join hkp.CompanyParty CP on CP.PartyId=XP.Id and CP.PartyType='Customer'
left join hkp.PartyAccountGroup PAG on PAG.Id=CP.PartyAccountGroupId
left join TRN.CustomerUpdateParameter CUP on CUP.Id=MOI.CustomerParameterId
where SO.OrderStatusId in ('Active','Toship','ToClose') and MOI.ParameterSettingId is null
and format(MOI.AddedDate,'dd-MMM-yyyy') between '"+ FromDate +"' and '"+ ToDate +"'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCRPCbo(string MasterId, string LineItemNo)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string
                sql = @"select RD.Id,SD.Id SId,QCD.Id ParameterId,QC.Id TransactionHeaderId,QMM.UserName Issue,QC.IssueId, QCD.ItemId,
PM.UserName ParameterName,QPM.UOMId,UOM.UserName UOM,'" + MasterId + @"' UCPId,RD.MinRequirement,RD.MaxRequirement,SD.MinStandard,SD.MaxStandard MaxStandard,EI.EmployeeName ResponsiblePerson,RD.Remarks,RD.CriticalLevel,
(select ArticleId from TRN.MasterOrderItem where CustomerParameterId='" + MasterId + @"' and Id='" + LineItemNo + @"') as ArticleId
from TRN.QualityControlDetails QCD
left join TRN.QualityControl QC on QC.Id=QCD.QCId
left join MST.QualityManagementMaster QMM on QMM.Id= QC.IssueId
left join MST.QualityManagementParameterItem QPM on QPM.Id=QCD.ItemId
left join hkp.ParameterMaster PM on PM.Id=QPM.ParameterId
left join SCS.UnitOfMeasurement UOM on UOM.Id=QPM.UOMId
left join TRN.UCPRequirementDetails RD on RD.UCPId='" + MasterId + @"' and RD.ParameterId=QCD.Id
left join TRN.UCPMaxMinStandardDetails SD on SD.ArticleId=(select ArticleId from TRN.MasterOrderItem where CustomerParameterId='" + MasterId + @"' and Id='"+ LineItemNo + @"') and SD.ParameterId=QCD.Id
left join EmployeeInformation EI on EI.SystemId=RD.ResponsiblePersonId
where ItemId in (select Id from MST.QualityManagementParameterItem where CustomerParameter = 1)";
             return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult createUCPRequirement(Dictionary<string, object> UCPRequirementDetailsData)
        {
            try
            {
                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsUCPRequirementDetailsData, dsUCPStandardDetailsData, dsChildId;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[UCPRequirementDetails] where Id='" + UCPRequirementDetailsData["Id"] + "'", out dsUCPRequirementDetailsData, false, "1");
                conRack.OpenDataSetThroughAdapter("select count(Id) + 1 as UCPId from TRN.UCPRequirementDetails where UCPId='" + UCPRequirementDetailsData["UCPId"] + "'", out dsChildId, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[UCPMaxMinStandardDetails] where Id='" + UCPRequirementDetailsData["SId"] + "'", out dsUCPStandardDetailsData, false, "1");
                

                string _Id = "", _SId = "", Id = string.Empty;

                #region data update
                if (dsUCPRequirementDetailsData.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    UCPRequirementDetailsData["Id"] = UCPRequirementDetailsData["UCPId"] + "-" + dsChildId.Tables[0].Rows[0]["UCPId"].ToString();
                    AddNewRow(dsUCPRequirementDetailsData.Tables[0], UCPRequirementDetailsData);

                }
                else
                {
                    _Id = UCPRequirementDetailsData["Id"].ToString();
                    EditRow(dsUCPRequirementDetailsData.Tables[0].Rows[0], UCPRequirementDetailsData);
                }

                if (dsUCPStandardDetailsData.Tables[0].Rows.Count == 0)
                {
                    
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[TRN].[UCPMaxMinStandardDetails]", out _SId);
                        UCPRequirementDetailsData["Id"] = _SId;
                        AddNewRow(dsUCPStandardDetailsData.Tables[0], UCPRequirementDetailsData);
                       
                }
                else
                {
                    _SId = UCPRequirementDetailsData["SId"].ToString();
                    UCPRequirementDetailsData["Id"] = _SId;
                    EditRow(dsUCPStandardDetailsData.Tables[0].Rows[0], UCPRequirementDetailsData);
                }

                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsUCPRequirementDetailsData, dsUCPStandardDetailsData);

                return Json(new { Error = false, Data = UCPRequirementDetailsData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [Authorize, HttpGet]
        public JsonResult GetParameterResponsiblePersonLists()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,(select EmployeeName from  employeeinformation where SystemId=ResponsiblePersonId) as Text from MST.ProcessParameterResponsiblePerson";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetParameterApprovalPersonLists()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,(select EmployeeName from  employeeinformation where SystemId=ResponsiblePersonId) as Text from [MST].[ProcessParameterApprovalResponsiblePerson]";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult createCustomerUpdatePara(Dictionary<string, object> CustomerUpdateParaData,string ApprovalStatus)
        {
            try
            {
                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[CustomerUpdateParameter] where LineItemNo='" + CustomerUpdateParaData["LineItemNo"] + "'", out DataSet dsCustomerUpdateParaItemLineNoValidation, false, "1");

                DataSet dsCustomerUpdatePara;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[CustomerUpdateParameter] where Id='" + CustomerUpdateParaData["Id"] + "'", out dsCustomerUpdatePara, false, "1");
                string _Id = "";

                #region data update
                if (CustomerUpdateParaData["LineItemNo"] == null)
                {
                    throw new Exception("LineItemNo is required");
                }
                else
                {
                    if (CustomerUpdateParaData["EmployeeId"] == null)
                    {
                        throw new Exception("Employee is required");
                    }
                    else
                    {
                        if (dsCustomerUpdatePara.Tables[0].Rows.Count == 0)
                        {
                            if (dsCustomerUpdateParaItemLineNoValidation.Tables[0].Rows.Count > 0)
                            {
                                throw new Exception("LineItemNo Name Already Exist.");
                            }
                            else
                            {
                                bplib.clsGenID genid = new bplib.clsGenID();
                                genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[TRN].[CustomerUpdateParameter]", out _Id);
                                CustomerUpdateParaData["Id"] = _Id;
                                CustomerUpdateParaData["ApprovalStatus"] = ApprovalStatus;
                                AddNewRow(dsCustomerUpdatePara.Tables[0], CustomerUpdateParaData);
                                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                                conC.BeginTransaction();
                                conC.executeQuery("Update TRN.MasterOrderItem set CustomerParameterId='" + CustomerUpdateParaData["Id"] + "' where Id='" + CustomerUpdateParaData["LineItemNo"] + @"'");
                                conC.CommitTransaction();
                            }
                        }
                        else
                        {
                            _Id = CustomerUpdateParaData["Id"].ToString();
                            CustomerUpdateParaData["ApprovalStatus"] = ApprovalStatus;
                            EditRow(dsCustomerUpdatePara.Tables[0].Rows[0], CustomerUpdateParaData);
                        }
                    }
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsCustomerUpdatePara);

                return Json(new { Error = false, Data = CustomerUpdateParaData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [Authorize, HttpPost]
        public ActionResult GetEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT EI.SystemId as SystemId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=mb.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=p.DepartmentId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=p.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=p.SubSectionId
                            WHERE EI.EmployeeStatus='Active' and EI.EmployeeCode is not null";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }
        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();
            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();
            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }
        #endregion -- Operations
    }
}