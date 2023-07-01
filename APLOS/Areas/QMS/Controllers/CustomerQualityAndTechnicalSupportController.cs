using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.QMS.Controllers
{
    public class CustomerQualityAndTechnicalSupportController : Controller
    {
        private readonly SqlRepository _sqlRepository;

        public CustomerQualityAndTechnicalSupportController()
        {
            _sqlRepository = new SqlRepository();
        }

        public ActionResult Aplos()
        {
            return View();
        }

        public JsonResult GetArticle(string salesId)
        {
            try
            {
                string sql = @"select distinct MMA.Id Value, MMA.StandardName Text from MST.MaterialMasterArticle MMA
left join TRN.SalesMaterial SM on SM.ArticleId = MMA.Id
left join TRN.Sales S on S.Id = SM.SalesId and S.AddedDate between dateadd(year,datediff(year,0,getdate()),0)
and  dateadd(day,-1,dateadd(year,datediff(year,-1,getdate()),0))
where S.PartyId = '"+ salesId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public JsonResult GetInvoiceNumber(string articleId)
        {
            try
            {
                string sql = @"select * from TRN.SalesMaterial SM
left join TRN.Sales on Sales.Id  = SM.SalesId
where SM.ArticleId = '"+ articleId + "' ";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}