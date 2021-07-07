using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
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

namespace Aplos.Areas.Accounts.Controllers
{
    public class VoucherParkController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        public VoucherParkController(
             IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        public ActionResult Aplos()
        {
            return View("~/Areas/Accounts/Views/VoucherPark/Aplos.cshtml");
        }


        [HttpPost, Authorize]
        public ActionResult getVoucherDataList( string voucherNo)
        {
            AccountsCommonService accountsCommonService = new AccountsCommonService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { DATA = accountsCommonService.getVoucherDataList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId,voucherNo), Error = false }, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public JsonResult Create(string entityId, string departmentId, IEnumerable<FixedAssetRegister> entityFixedAssetList)
        //{
        //    var flag = false;

        //    try
        //    {
        //        string entityFixedAssetList1 = "";

        //        foreach (var item in entityFixedAssetList)
        //        {
        //            if (string.IsNullOrEmpty(entityFixedAssetList1))
        //            {
        //                entityFixedAssetList1 += "'','" + item.Id+"'";
        //            }
        //            else
        //            {
        //                entityFixedAssetList1 += ",'" + item.Id + "'";
        //            }

        //        }
        //        _unitOfWork.BeginTransaction();
        //        flag = true;
        //        var vendorAdWr = new System.Text.StringBuilder();
        //        var vendorAdWrsql = "";

        //            vendorAdWrsql = @"update  TRN.FixedAssetRegister set EntityId='"+ entityId + "',DepartmentId='"+ departmentId + @"' where Id in ("+ entityFixedAssetList1 + @")";
        //            vendorAdWr.Append(vendorAdWrsql);
        //        _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
        //        _unitOfWork.SaveChanges();
        //        flag = false;
        //        _unitOfWork.Commit();



        //        return Json(new { Error = false, Message = AplosMessage.Updated });

        //    }
        //    catch (Exception ex)
        //    {

        //        return Json(new { Error = true, Message = ex.Message });

        //    }
        //}




    }
}