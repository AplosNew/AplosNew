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
using System.Net;
using System.Net.Http;
using Library.MaterialManagement.Material;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;

namespace Aplos.Controllers.ApopAPIHR
{
    public class DetentionAPIController : ApiController
    {
        private readonly SqlRepository _sqlRepository;
        
        public DetentionAPIController()
        {

            _sqlRepository = new SqlRepository();
        }

        
        public List<WorkCenterList> GetWorkCenter()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.getWorkcenter(out List<WorkCenterList> workcenterlst);
            return workcenterlst;
        }

        public List<DetentionTypeList> GetDetentionTypes()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.getDetentionType(out List<DetentionTypeList> detentionTypeIdLst);
            return detentionTypeIdLst;
        }

        public List<DetentionResponsiblePersonList> GetDetentionResponsible()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetDetentionResponsible(out List<DetentionResponsiblePersonList> detResPList);
            return detResPList;
        }

        public List<DetentionIssueByNo> GetIssueByNo(string EmployeeId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetIssueByNo(out List<DetentionIssueByNo> detentionIssueByNo, EmployeeId);
            return detentionIssueByNo;
        }

        public List<DetentionLogGridList> GetDetentionLogGrid()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetDetentionLogGrid(out List<DetentionLogGridList> detentionLoggridlist);
            return detentionLoggridlist;
        }

        public List<MachineMasterList> GetMachineMastersAsset()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetMachineMasterAsset(out List<MachineMasterList> machinemstrlst);
            return machinemstrlst;
        }

        [HttpPost]
        public string Create([FromBody] IEnumerable<CreateDetentionList> DataToSave, string By, string Ip, string updatedBy, string updatedFrom)
        {
            try
            {
                clsDataContext clsData = new clsDataContext();
                string Id = clsData.CreateDetentionLog(DataToSave, By, Ip, updatedBy, updatedFrom);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }
    }
}
