using Aplos.Helpers;
using HRService;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Modules;
using Library.Service.Organizations;
using Library.Service.Securites;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using HRService;
using System.Web.Http;

namespace Aplos.Controllers
{
     public class ApopAPIHRController : ApiController
    {
        private readonly SqlRepository _sqlRepository;
        public ApopAPIHRController()
        {

            _sqlRepository = new SqlRepository();
        }
        public List<CompanyList> getCompanyList()
        {
            clsDataContext clsData = new clsDataContext();

            clsData.getCompanyList(out List<CompanyList> CompanyData);
            return CompanyData;
        }
    }
}