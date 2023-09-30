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
    public class OrderWiseQualityReportController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        public OrderWiseQualityReportController(ISqlRepository R)
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
        public ActionResult LoadOrderWiseQualityReport()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string
                sql = @"select (case when QCD.Id is null then 'Pending' else 'Completed' end) QualityStatus,format(QC.AddedDate,'dd-MMM-yyyy') Date,MOI.Id MOLineItemNo,PS.UserName POStatus,POD.ProductionOrderId PONo,
QC.LotNumber,MA.StandardName Article,XP.UserName Customer,Reverse(stuff(Reverse((select OWC.Grade +', ' from MST.OrderWiseQualityComment OWC																			
where OWC.MOLineItemNo=MOI.Id and OWC.PONo=PO.Id and OWC.LotNo=QC.LotNumber for xml PATH(''))),1,2,'')) Grade,
Reverse(stuff(Reverse((select format(OWC.AddedDate,'dd-MMM-yyyy') + '-' + OWC.Comment +', ' from MST.OrderWiseQualityComment OWC																			
where OWC.MOLineItemNo=MOI.Id and OWC.PONo=PO.Id and OWC.LotNo=QC.LotNumber for xml PATH(''))),1,2,'')) CommentDetails
from TRN.SalesOrder SO
left join TRN.MasterOrderItem MOI on MOI.Id=SO.MasterOrderItemId
left join [MST].[MaterialMasterArticle] MA ON MA.Id=MOI.ArticleId
left join trn.ProductionOrderDetail POD on POD.SalesOrderId=SO.Id
left join trn.ProductionOrder PO on PO.Id=POD.ProductionOrderId
left join hkp.ProductionStatus PS on PS.Id=PO.ProductionStatusId
left join Trn.MasterOrder MO on MO.Id=MOI.MasterOrderId
left join [HKP].[Party] Xp on XP.Id=MO.PartyId
left join TRN.QualityControl QC on QC.ProductionOrderId=PO.Id
left join TRN.QualityControlDetails QCD on QCD.QCId=QC.Id and QCD.ItemId in (select Id from MST.QualityManagementParameterItem where CustomerParameter=1)
where SO.OrderStatusId in ('Active','Toship','ToClose') and PS.UserName in ('Running','To Close') order by QC.AddedDate desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getCommentEntryData(string MOLineItemNo, string PONo, string LotNo)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select Id,isnull(MOLineItemNo,'" + MOLineItemNo + "') MOLineItemNo,isnull(PONo,'" + PONo + "') PONo,isnull(LotNo,'" + LotNo + "') LotNo,Comment,ByWhomId," +
                "(select EmployeeName from EmployeeInformation Where SystemId=(select AuthorizedResPersonId from [HKP].[QualityManagementAuthorizedPerson] where Id=ByWhomId)) ByWhom,Grade " +
                "from [MST].[OrderWiseQualityComment] where MOLineItemNo='"+ MOLineItemNo + "' and PONo='"+ PONo + "' and LotNo='"+ LotNo + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetByWhomLists()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,(select EmployeeName from  employeeinformation where SystemId=AuthorizedResPersonId) as Text from [HKP].[QualityManagementAuthorizedPerson]";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult createComments(Dictionary<string, object> CommentsData, string MOItem, string POId, string LotNumber)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[OrderWiseQualityComment] where Comment='" + CommentsData["Comment"] + "'", out DataSet dsOrderWiseQualityCommentValidation, false, "1");

                DataSet dsOrderWiseQualityComment;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[OrderWiseQualityComment] where Id='" + CommentsData["Id"] + "'", out dsOrderWiseQualityComment, false, "1");
                string _Id = "";

                #region data update
                if (dsOrderWiseQualityComment.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("OrderWiseQualityComment", out _Id);
                    _Id = "OWC" + _Id;
                    CommentsData["Id"] = _Id;
                    CommentsData["MOLineItemNo"] = MOItem;
                    CommentsData["PONo"] = POId;
                    CommentsData["LotNo"] = LotNumber;
                    AddNewRow(dsOrderWiseQualityComment.Tables[0], CommentsData);
                }
                else
                {
                    _Id = CommentsData["Id"].ToString();
                    EditRow(dsOrderWiseQualityComment.Tables[0].Rows[0], CommentsData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsOrderWiseQualityComment);

                return Json(new { Error = false, Data = CommentsData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [Authorize, HttpGet]
        public ActionResult getCommentData(string MOId, string POId, string LotNo)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select OWC.*,(select EmployeeName from EmployeeInformation Where SystemId=(select AuthorizedResPersonId from [HKP].[QualityManagementAuthorizedPerson] where Id=ByWhomId)) ByWhom from [MST].[OrderWiseQualityComment] OWC
where OWC.MOLineItemNo ='" + MOId + "' and OWC.PONo='" + POId + "' and OWC.LotNo='" + LotNo + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadCommentEntryEditData(string CommentId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select OWC.*,(select EmployeeName from EmployeeInformation Where SystemId=(select AuthorizedResPersonId from [HKP].[QualityManagementAuthorizedPerson] where Id=ByWhomId)) ByWhom from [MST].[OrderWiseQualityComment] OWC where Id='" + CommentId + @"'";
            return Json(new { comment = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CommentsDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [MST].[OrderWiseQualityComment] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
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