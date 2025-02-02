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

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class ProductIntegrityAnalysisController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        public ProductIntegrityAnalysisController(ISqlRepository R)
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


        [Authorize, HttpGet]
        public JsonResult GetFromDateList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select top 1 format(AddedDate,'dd-MMM-yyyy') FromDate from TRN.MasterOrderItem order by AddedDate asc";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCustomerList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select distinct XP.UserName as Text,XP.Id as Value from TRN.MasterOrderItem XMOI
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult LoadProductIntegrityAnalysis(string CustomerInfo, string todate, string fromdate, string AnalysisType)
        {

            string Customer = string.Empty;
            string OrderAnalysis = string.Empty;
            if (CustomerInfo == null)
            {
                Customer = "";
            }
            else
            {
                Customer = " and XP.Id='" + CustomerInfo + "'";
            }
            if (AnalysisType == null)
            {
                OrderAnalysis = "";
            }
            else
            {
                OrderAnalysis = " and PIA.AnalysisType='" + AnalysisType + "'";
            }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select distinct Xp.UserName as Customer,XMOI.Id as LineItemId,mm.userName AS Material,ma.StandardName AS Article,
PL.Code as ProductCode,STUFF((select distinct ','+ ShortName + '-' +AttributeValue from ProductLibraryAttribute where Active=1 and ProductLibraryId=PL.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '') as	ProductCodeDetails,
XMOI.BuyerReferenceNo as CustRef,
XMOI.OwnReferenceNo as OwnRef,
XMOI.TotalQty as ItemQty,pc.UserName as	ProductCategory,PM.UserName as Product,
STUFF((select distinct ','+FORMAT(sox.DeliveryDate,'dd-MMM-yyyy') from trn.SalesOrder sox where sox.MasterOrderItemId=XMOI.Id  
								                                for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '') as	 	[1stDeliveryDate],
																Xmo.Id as MasterOrderId,
STUFF((select distinct ','+sox.Id from trn.SalesOrder sox where sox.MasterOrderItemId=XMOI.Id  
								                                for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '') as	SONos,OS.UserName as LineItemStatus,
reverse(stuff(reverse((select AnalysisType + ',' from TRN.ProductIntegrityAnalysis P where P.LineItemId=PIA.LineItemId for xml path(''))), 1, 1, '')) AnalysisType
from TRN.MasterOrderItem XMOI
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=XMOI.ArticleId
left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
left outer join ProductLibrary PL ON PL.Id=XMOI.ProductLibraryId
left outer join TRN.ProductIntegrityAnalysis PIA ON PIA.LineItemId=XMOI.Id
left outer join hkp.OrderStatus OS ON OS.Id=XMOI.OrderStatusId
 where XMOI.AddedDate  between '" + fromdate + "' and '" + todate + "'" + Customer + @"" + OrderAnalysis + @"";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult LoadOrderAnalysis(string AnalysisType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select distinct Xp.UserName as Customer,XMOI.Id as LineItemId,mm.userName AS Material,ma.StandardName AS Article,
PL.Code as ProductCode,STUFF((select distinct ','+ ShortName + '-' +AttributeValue from ProductLibraryAttribute where Active=1 and ProductLibraryId=PL.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '') as	ProductCodeDetails,
XMOI.BuyerReferenceNo as CustRef,
XMOI.OwnReferenceNo as OwnRef,
XMOI.TotalQty as ItemQty,pc.UserName as	ProductCategory,PM.UserName as Product,
STUFF((select distinct ','+FORMAT(sox.DeliveryDate,'dd-MMM-yyyy') from trn.SalesOrder sox where sox.MasterOrderItemId=XMOI.Id  
								                                for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '') as	 	[1stDeliveryDate],
																Xmo.Id as MasterOrderId,
STUFF((select distinct ','+sox.Id from trn.SalesOrder sox where sox.MasterOrderItemId=XMOI.Id  
								                                for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '') as	SONos,OS.UserName as LineItemStatus,PIA.AnalysisType
from TRN.MasterOrderItem XMOI
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=XMOI.ArticleId
left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
left outer join ProductLibrary PL ON PL.Id=XMOI.ProductLibraryId
left outer join TRN.ProductIntegrityAnalysis PIA ON PIA.LineItemId=XMOI.Id
left outer join hkp.OrderStatus OS ON OS.Id=XMOI.OrderStatusId
 where PIA.AnalysisType='"+ AnalysisType + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult LoadProductIntegrityAnalysisByLineItem(string LineItemId, string AnalysisType)
        {
            string OrderAnalysis = string.Empty;
            if (AnalysisType == null)
            {
                OrderAnalysis = "";
            }
            else
            {
                OrderAnalysis = " and PIA.AnalysisType='" + AnalysisType + "'";
            }
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select distinct Xp.UserName as Customer,XMOI.Id as LineItemId,mm.userName AS Material,ma.StandardName AS Article,
PL.Code as ProductCode,STUFF((select distinct ','+ ShortName + '-' +AttributeValue from ProductLibraryAttribute where Active=1 and ProductLibraryId=PL.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '') as	ProductCodeDetails,
XMOI.BuyerReferenceNo as CustRef,
XMOI.OwnReferenceNo as OwnRef,
XMOI.TotalQty as ItemQty,pc.UserName as	ProductCategory,PM.UserName as Product,
STUFF((select distinct ','+FORMAT(sox.DeliveryDate,'dd-MMM-yyyy') from trn.SalesOrder sox where sox.MasterOrderItemId=XMOI.Id  
								                                for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '') as	 	[1stDeliveryDate],
																Xmo.Id as MasterOrderId,
STUFF((select distinct ','+sox.Id from trn.SalesOrder sox where sox.MasterOrderItemId=XMOI.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '') as	SONos,
PIA.Id,PIA.AnalysisMasterId,PIA.AnalysisType,PIA.Remarks	
from TRN.MasterOrderItem XMOI
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=XMOI.ArticleId
left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
left outer join ProductLibrary PL ON PL.Id=XMOI.ProductLibraryId
left outer join [TRN].[ProductIntegrityAnalysis] PIA ON PIA.LineItemId=XMOI.Id
 where XMOI.Id = '" + LineItemId + "'" + OrderAnalysis + @"";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult LoadOrderAnalysisByLineItem(string LineItemId, string AnalysisType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select distinct Xp.UserName as Customer,XMOI.Id as LineItemId,mm.userName AS Material,ma.StandardName AS Article,
PL.Code as ProductCode,STUFF((select distinct ','+ ShortName + '-' +AttributeValue from ProductLibraryAttribute where Active=1 and ProductLibraryId=PL.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '') as	ProductCodeDetails,
XMOI.BuyerReferenceNo as CustRef,
XMOI.OwnReferenceNo as OwnRef,
XMOI.TotalQty as ItemQty,pc.UserName as	ProductCategory,PM.UserName as Product,
STUFF((select distinct ','+FORMAT(sox.DeliveryDate,'dd-MMM-yyyy') from trn.SalesOrder sox where sox.MasterOrderItemId=XMOI.Id  
								                                for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '') as	 	[1stDeliveryDate],
																Xmo.Id as MasterOrderId,
STUFF((select distinct ','+sox.Id from trn.SalesOrder sox where sox.MasterOrderItemId=XMOI.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '') as	SONos,
PIA.Id,PIA.AnalysisMasterId,PIA.AnalysisType,PIA.Remarks	
from TRN.MasterOrderItem XMOI
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=XMOI.ArticleId
left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
left outer join ProductLibrary PL ON PL.Id=XMOI.ProductLibraryId
left outer join [TRN].[ProductIntegrityAnalysis] PIA ON PIA.LineItemId=XMOI.Id
 where PIA.AnalysisType='"+ AnalysisType + "' and XMOI.Id = '" + LineItemId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAnalysisNameList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select PAM.UserName as Text,PAM.Id as Value from [MST].[ProductIntegrityAnalysisMaster] PAM";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> AnalysisHeaderData, string LineItemId)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[ProductIntegrityAnalysis] where AnalysisType='" + AnalysisHeaderData["AnalysisType"] + "' and LineItemId='" + LineItemId + "'", out DataSet dsProductIntegrityAnalysisATValidation, false, "1");
                DataSet dsProductIntegrityAnalysis;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[ProductIntegrityAnalysis] where Id='" + AnalysisHeaderData["Id"] + "'", out dsProductIntegrityAnalysis, false, "1");
                string _Id = "";

                #region data update
                if (dsProductIntegrityAnalysis.Tables[0].Rows.Count == 0)
                {
                    if (dsProductIntegrityAnalysisATValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Analysis Type is already mapped for this LineItem.");
                    }
                    else
                    {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("ProductIntegrityAnalysis", out _Id);
                    _Id = "PAH" + _Id;
                    AnalysisHeaderData["Id"] = _Id;
                    AnalysisHeaderData["LineItemId"] = LineItemId;
                    AddNewRow(dsProductIntegrityAnalysis.Tables[0], AnalysisHeaderData);
                    }
                }
                else
                {
                    _Id = AnalysisHeaderData["Id"].ToString();
                    AnalysisHeaderData["LineItemId"] = LineItemId;
                    EditRow(dsProductIntegrityAnalysis.Tables[0].Rows[0], AnalysisHeaderData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsProductIntegrityAnalysis);

                return Json(new { Error = false, Data = AnalysisHeaderData, Message = AplosMessage.Insert });
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

        [Authorize, HttpGet]
        public ActionResult LoadItemDetails(string ProductId, string Pid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT PAI.Id as ItemId,PAI.SNO,PAI.Category,PAI.ItemName,PAI.CriticalLevel,PAI.Remarks,(select UserName from HKP.Process where Active=1 and Id=ProcessId) as Process,
(select UserName from scs.UnitOfMeasurement where Active=1 and Id=UOMId) as UOM,PAI.AttachmentApplicable,PAID.Id,PAID.IsPending,PAID.Applicable,isnull(PAID.Value,0) as Value,PAID.ActionToBeTaken,PAID.AnalysisRemarks,E.EmployeeName as ResponsiblePerson,PAID.ResponsiblePersonId as ResponsiblePersonId,PAID.FileName
FROM [TRN].[ProductIntegrityAnalysisItem] PAI
left Join [TRN].[ProductIntegrityAnalysisItemDetails] PAID ON  PAID.ItemId=PAI.Id and PAID.PIAId='" + Pid + @"'
left Join dbo.EmployeeInformation E ON E.SystemId=PAID.ResponsiblePersonId 
where PAI.PIAMID ='" + ProductId + "' order by SNO";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT EI.SystemId as SystemId, MB.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=MB.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=P.DepartmentId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=P.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=P.SubSectionId
                            WHERE EI.EmployeeStatus='Active'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetPresentyNames(string PId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT PN.Id,CAST (CASE WHEN PN.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,EI.SystemId as PresentyNameId, MB.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=MB.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=p.DepartmentId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=p.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=p.SubSectionId
                            LEFT OUTER JOIN TRN.PIAPresentyNames PN ON PN.PresentyNameId=EI.SystemId and PIAId='" + PId + @"'
                            WHERE EI.EmployeeStatus='Active'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult GetAnalysisItemValueList(string ItemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"select convert(varchar(50),PPD.PredefineValue) As Text,PPD.Id as Value,PPD.ItemId  from ProductItemParameterDetails  PPD where PPD.ItemId='"+ ItemId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult CreatePresentyNames(List<Dictionary<string, object>> DataList, string PId)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[TRN].[PIAPresentyNames]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);
                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "APN" + _Id;
                            item["PIAId"] = PId;
                            AddNewRow(dsProdBooked.Tables[0], item);

                        }
                        else
                        {
                            item["PIAId"] = PId;
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                    return Json(new { Message = AplosMessage.Insert });
                }
                else
                {
                    throw new CustomException("Please select atleast one record and proceed!");
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [Authorize, HttpPost]
        public ActionResult CreateAnalysisItem(List<Dictionary<string, object>> DataList, string PId)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[TRN].[ProductIntegrityAnalysisItemDetails]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);
                        if (dv.Count == 0)
                        {
                                bplib.clsGenID genid = new bplib.clsGenID();
                                genid.GenID(TableName, out _Id);
                                item["Id"] = "PAD" + _Id;
                                item["PIAId"] = PId;
                                AddNewRow(dsProdBooked.Tables[0], item);
                            
                        }
                        else
                        {
                                item["PIAId"] = PId;
                                DataRow drpb = dv[0].Row;
                                EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                    return Json(new { Message = AplosMessage.Insert });
                }
                else
                {
                    throw new CustomException("Please select atleast one record and proceed!");
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [Authorize, HttpPost]
        public ActionResult SaveDefault(IEnumerable<System.Web.HttpPostedFileBase> UploadDefault, string UploadDefault_data)
        {
            try
            {
                UploadDefault_data = UploadDefault_data.Replace("\"", "");
                if (string.IsNullOrEmpty(UploadDefault_data))
                    throw new Exception("Save the order first");

                foreach (var file in UploadDefault)
                {

                    var fileName = Path.GetFileName(UploadDefault_data + new FileInfo(file.FileName).Extension);
                    var fileN = file.FileName;
                    var destinationPath = Path.Combine(ResourcesPathReader.GetPAIDocumentPath(), fileName);

                    var directory = ResourcesPathReader.GetPAIDocumentPath();
                    var path = Path.Combine(directory);

                    if (System.IO.Directory.Exists(ResourcesPathReader.GetPAIDocumentPath()) == false)
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(ResourcesPathReader.GetPAIDocumentPath());
                        }
                        catch (Exception)
                        {

                        }
                    }


                    ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                    string sql = "SELECT * FROM [TRN].[ProductIntegrityAnalysisItemDetails] WHERE Id='" + UploadDefault_data + "'";
                    DataSet dsLocal = null;
                    connection.BeginTransaction();
                    connection.getDataSet(sql, out dsLocal);
                    connection.CommitTransaction();
                    var FN = dsLocal.Tables[0].Rows[0]["FileName"].ToString();
                    if (fileN != FN)
                        if (System.IO.File.Exists(path + UploadDefault_data + Path.GetExtension(FN)))
                            System.IO.File.Delete(path + UploadDefault_data + Path.GetExtension(FN));

                    if (dsLocal.Tables[0].Rows.Count > 0)
                    {
                        dsLocal.Tables[0].Rows[0].BeginEdit();

                        dsLocal.Tables[0].Rows[0]["FileName"] = fileN;

                        dsLocal.Tables[0].Rows[0].EndEdit();

                        file.SaveAs(destinationPath);
                        clsStaticInfo info = new clsStaticInfo();
                        info.SaveDataSets(dsLocal);
                    }
                }
                return Content("");
            }
            catch (Exception ex)
            {
                HttpResponse Response = System.Web.HttpContext.Current.Response;
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = 204;
                Response.Status = "204 No Content";
                Response.StatusDescription = ex.Message;
                Response.End();

                return Content("");
            }

        }
      
        #endregion -- Operations
    }
}