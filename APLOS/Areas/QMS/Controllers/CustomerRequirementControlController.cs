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
    public class CustomerRequirementControlController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        public CustomerRequirementControlController(ISqlRepository R)
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
        public ActionResult LoadCustomerRequirementControl(string ParameterStatus)
        {
            string FilterData = string.Empty;
          
            if (ParameterStatus == "Completed")
            {
                FilterData = " and MOI.CustomerParameterId is not null";
            }
            if (ParameterStatus == "Pending")
            {
                FilterData = " and MOI.CustomerParameterId is null";
            }
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string
                sql = @"select XP.PartyType CustomerType,XP.UserName Customer,
MOI.Id LineItemNo,MM.UserName Material,MA.StandardName Article,MOI.BuyerReferenceNo,MOI.OwnReferenceNo,
PL.Code ProductLibraryCode,PL.UserName ProductLibrary,MOI.ProductionGrouping,POD.ProductionOrderId PONo,MOI.TotalQty ItemQty,
MOI.Remark,PS.UserName POStatus
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
where SO.OrderStatusId in ('Active','Toship','ToClose')" + FilterData + @"";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        #endregion -- Operations
    }
}