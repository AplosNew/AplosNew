using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.FixedAssets;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Model.FixedAsset;
using Library.Model.FixedAssets;
using Library.Model.Inventory;
using Library.Model.Materials;
using Library.Service.Enums;
using Library.Service.FixedAssets;
using Library.Service.Helpers;
using Library.Service.Invoices;
using Library.ViewModel.Materials;
using Library.ViewModel.Vouchers;
using OTSBD;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.FixedAssets.Controllers
{
    public class EntityFixedAssetsRegisterController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        public EntityFixedAssetsRegisterController(
             IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        public ActionResult Aplos()
        {
            return View("~/Areas/FixedAssets/Views/EntityFixedAssetsRegister/Aplos.cshtml");
        }


        //Elastic Search
        [HttpPost, Authorize]
        public ActionResult GetEntityFixedAssetRegisterElasticSearchDataList()
        {
            FixedAssetQueryService fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { DATA = fixedAssetQueryService.GetEntityFixedAssetRegisterElasticSearchDataList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost, Authorize]
        //public ActionResult GetTaskListResult(Dictionary<string, object> Filter, List<Dictionary<string, object>> FilterFields)
        //{
        //    DataTable dtFinal = new DataTable(); ;


        //    GetTNAStatusReportsData(out dtFinal, Filter, FilterFields);

        //    var jsondata = Json(new { MAINDATA = CustomJsonResultService.DataTableToJson(dtFinal) }, JsonRequestBehavior.AllowGet);
        //    jsondata.MaxJsonLength = int.MaxValue;
        //    return jsondata;
        //}
    //    private void GetTNAStatusReportsData(out DataTable dtTna, Dictionary<string, object> Filter, List<Dictionary<string, object>> FilterFields)
    //    {

    //        MasterOrderDataTablesForGrid(Filter, FilterFields, out dtTna);

    //        //dtTna.Columns.Add("EarlyBy", typeof(int));
    //        //dtTna.Columns.Add("LateBy", typeof(int));
    //        //for (int i = 0; i < dtTna.Rows.Count; i++)
    //        //{
    //        //    if (dtTna.Rows[i]["CurrentStatus"].ToString().ToUpper() == "CLOSED" && dtTna.Rows[i]["ClosingDate"].ToString() != "")
    //        //    {
    //        //        try
    //        //        {
    //        //            DateTime dtDueDate = Convert.ToDateTime(dtTna.Rows[i]["DueDate"].ToString());
    //        //            DateTime dtClosingDate = Convert.ToDateTime(dtTna.Rows[i]["ClosingDate"].ToString());
    //        //            if (dtClosingDate < dtDueDate)
    //        //                dtTna.Rows[i]["EarlyBy"] = Math.Abs(clsStaticInfo.dateDiff(dtClosingDate.ToString("dd-MMM-yyyy"), dtDueDate.ToString("dd-MMM-yyyy")));
    //        //            if (dtClosingDate > dtDueDate)
    //        //                dtTna.Rows[i]["LateBy"] = Math.Abs(clsStaticInfo.dateDiff(dtDueDate.ToString("dd-MMM-yyyy"), dtClosingDate.ToString("dd-MMM-yyyy")));
    //        //        }
    //        //        catch (Exception)
    //        //        {


    //        //        }

    //        //    }
    //        //}



    //    }
    //    private void MasterOrderDataTablesForGrid(Dictionary<string, object> Filter, List<Dictionary<string, object>> FilterFields, out DataTable MainData)
    //    {


    //        //string DueDate = "TT.OriginalSequentialEndDate";
    //        //string FilterText = " WHERE 1=1 ";
    //        //if (FilterFields != null)
    //        //{
    //        //    for (int i = 0; i < FilterFields.Count; i++)
    //        //    {
    //        //        FilterText += " AND isnull(RTRIM(LTRIM(" + FilterFields[i]["Key"].ToString() + ")),'') IN (" + FilterFields[i]["Value"].ToString().Replace("' ", "'").Replace("', '", "','").Replace(", ", ",") + ")  ";
    //        //    }

    //        //}
    //        //string TaskTypeFilter = "";
    //        //if (Filter["ReportLevel"].ToString() != "ALL")
    //        //    TaskTypeFilter = "WHERE tao.TaskAppliedOnEnum='" + Filter["ReportLevel"].ToString() + "'";


    //        //if (Filter["ActiveStatus"].ToString() != "All")
    //        //{
    //        //    if (Filter["ActiveStatus"].ToString() == "Closed")
    //        //        TaskTypeFilter += " AND TM.CurrentStatus='" + Filter["ActiveStatus"].ToString() + "'";
    //        //    else
    //        //        TaskTypeFilter += " AND isnull(TM.CurrentStatus,'')<>'Closed'";
    //        //}
    //        //if (Filter["DateSelection"].ToString() != "WITHOUTDATE")
    //        //{
    //        //    if (Filter["DateSelection"].ToString() == "WITHDATE")
    //        //    {
    //        //        if (Filter["ActiveStatus"].ToString() == "Closed")
    //        //            TaskTypeFilter += " AND TM.ClosingDate between '" + Filter["FromDate"].ToString() + "' AND '" + Filter["ToDate"].ToString() + "'";
    //        //        else
    //        //            TaskTypeFilter += " AND " + DueDate + " between '" + Filter["FromDate"].ToString() + "' AND '" + Filter["ToDate"].ToString() + "'";

    //        //    }
    //        //    else if (Filter["DateSelection"].ToString() == "WITHOUTDATE")
    //        //    {
    //        //        if (Filter["ActiveStatus"].ToString() == "Closed")
    //        //            TaskTypeFilter += " AND TM.ClosingDate<='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + "'";
    //        //        else
    //        //            TaskTypeFilter += " AND " + DueDate + "<='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + "'";

    //        //    }
    //        //}


    //        string sql = @"select distinct MM.UserName MaterialMaster,MMA.StandardName Article,FA.UserName AssetMaster,P.UserName Party
    //            , FAR.MaterialMasterId,FAR.MaterialMasterArticleId,FAR.VendorId,FAR.FixedAssetMasterId

    //             ,IsAsset =case when MM.IsAsset =1 then 'Yes' else  'No'  end
				// , Machine=case when MBP.BusinessProcessName ='MachineDefinition' Then 'Yes' else 'No' end 
				// , count(FAR.FixedAssetMasterId) FACount
				// ,sum( ISNULL(FAR.FABaseAmount,0))FABaseAmount
				// ,sum( ISNULL(FAR.ADBaseAmount,0)) ADBaseAmount
				// ,sum( ISNULL(FAR.FABaseAmount,0)- ISNULL(FAR.ADBaseAmount,0)) NetFixedAssetsAmount
				//  ,sum( isnull(sar.SubAssetAmount,0))SubAssetAmount
				//  ,TotalAssetsBaseAmount= sum( ISNULL(FAR.FABaseAmount,0) + (isnull(sar.SubAssetAmount,0) )) 

		  //      from TRN.FixedAssetRegister FAR 
				//JOIN MST.MaterialMaster MM ON MM.Id=FAR.MaterialMasterId
				//JOIN MST.MaterialMasterArticle MMA ON MMA.Id=FAR.MaterialMasterArticleId
				//JOIN MST.FixedAssetMaster FA ON FA.Id=FAR.FixedAssetMasterId
				//LEFT JOIN HKP.Party P ON P.Id=FAR.VendorId

			 //   LEFT JOIN (SELECT MBP.MaterialMasterId,BP.BusinessProcessName FROM [MST].[MaterialMasterBusinessProcess] AS MBP
    //            LEFT JOIN [SCS].[BusinessProcess] AS BP ON MBP.BusinessProcessId = BP.Id
    //            WHERE BP.BusinessProcessName ='MachineDefinition') AS MBP ON MBP.MaterialMasterId=MM.Id


		  //      left join(select sum(Amount) SubAssetAmount,FixedAssetRegisterId from  trn.SubFixedAssetRegister
				//group by FixedAssetRegisterId
				//) sar on sar.FixedAssetRegisterId=FAR.Id


		  //     WHERE FAR.CompanyGroupId='' AND FAR.CompanyId='' AND FAR.PlantId=''  
				//  and FAR.MaterialMasterId in ("++@") AND FAR.MaterialMasterArticleId='' AND FAR.FixedAssetMasterId=''
				//	 and FAR.VendorId='' AND MM.IsAsset='' AND MBP.BusinessProcessName=''

    //           GROUP BY FAR.MaterialMasterId ,MM.UserName ,MMA.StandardName ,FA.UserName,P.UserName 
			 //  ,MM.IsAsset,MBP.BusinessProcessName,FAR.FixedAssetMasterId
			 //   ,FAR.MaterialMasterId,FAR.MaterialMasterArticleId,FAR.VendorId,FAR.FixedAssetMasterId";

    //        MainData = _sqlRepository.GetDataTable(sql);





    //    }


    //    [HttpPost, Authorize]
        public ActionResult GetEntityFixedAssetRegisterDataList(string materialMasterId, string materialMasterArticleId, string fixedAssetMasterId, string vendorId, string isAsset, string machine)
        {
            FixedAssetQueryService fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { DATA = fixedAssetQueryService.GetEntityFixedAssetRegisterDataList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, materialMasterId, materialMasterArticleId, fixedAssetMasterId, vendorId, isAsset, machine), Error = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(string entityId, string departmentId, IEnumerable<FixedAssetRegister> entityFixedAssetList)
        {
            var flag = false;

            try
            {
                string entityFixedAssetList1 = "";

                foreach (var item in entityFixedAssetList)
                {
                    if (string.IsNullOrEmpty(entityFixedAssetList1))
                    {
                        entityFixedAssetList1 += "'','" + item.Id+"'";
                    }
                    else
                    {
                        entityFixedAssetList1 += ",'" + item.Id + "'";
                    }

                }
                _unitOfWork.BeginTransaction();
                flag = true;
                var vendorAdWr = new System.Text.StringBuilder();
                var vendorAdWrsql = "";
                
                    vendorAdWrsql = @"update  TRN.FixedAssetRegister set EntityId='"+ entityId + "',DepartmentId='"+ departmentId + @"' where Id in ("+ entityFixedAssetList1 + @")";
                    vendorAdWr.Append(vendorAdWrsql);
                _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();



                return Json(new { Error = false, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }




    }
}