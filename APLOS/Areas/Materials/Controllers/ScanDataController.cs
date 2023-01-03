using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aplos.Controllers;
using Library.Data.Sql;
using Library.HumanResource.Parameter;
using Aplos.Properties;
using System.Data;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Security.Core;
using Library.Service.EmployeeServices;

namespace Aplos.Areas.Materials.Controllers
{
    public class ScanDataController : BaseController
    {
        ItemScanService itemScanService = new ItemScanService();
        private readonly SqlRepository _sqlRepository;
        #region Constructor
        public ScanDataController()
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion Constructor

        #region Page
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion Page

        #region GET FUNCTION
        public ActionResult GetItemScanChild()
        {
            try
            {
                var sql = @"Select * from dbo.ItemScanChild where MasterId='2022-12137'";
                return Json(_sqlRepository.GetDataCollection(sql),JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion GET FUNCTION

        #region Save
        public ActionResult Save( List<ItemScanChildData> data)
        {
            try
            {
                
                var Data = itemScanService.CreateSummaryData("2022-12137", data);
               return Json(new { Error = false, Data = Data, Message = AplosMessage.Success});
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
            
            
        }
        #endregion Save
    }
}