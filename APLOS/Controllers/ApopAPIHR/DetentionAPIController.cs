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
        
        clsDataContext clsData = new clsDataContext();
        public DetentionAPIController()
        {

            
        }


        

        public List<WorkCenterList> GetWorkCenter(string processid)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.getWorkcenter(out List<WorkCenterList> workcenterlst, processid);
            return workcenterlst;
        }
        public List<DepartmentList> GetDepartment(string detentionid)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.getDepartment(out List<DepartmentList> DepartmentList, detentionid);
            return DepartmentList;
        }

        public List<DetentionTypeList> GetDetentionTypes()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.getDetentionType(out List<DetentionTypeList> detentionTypeIdLst);
            return detentionTypeIdLst;
        }

        public List<DetentionResponsiblePersonList> GetDetentionResponsible(string detentiontypeid)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetDetentionResponsible(out List<DetentionResponsiblePersonList> detResPList, detentiontypeid);
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
        public List<GetDetentionLog> GetDetentionLogDetail()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetDetentionLogDetail(out List<GetDetentionLog> detentionLoggridlist);
            return detentionLoggridlist;
        }

        [HttpPost]
        public string PostGetDetentionLogGrid([FromBody] IEnumerable<CreateDetentionList> DataToSave)
        {
            try
            {
                string Id = clsData.PostCreateDetention(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        public List<Process> GetProcess()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetProcess(out List<Process> Processlist);
            return Processlist;
        }

    }
}
