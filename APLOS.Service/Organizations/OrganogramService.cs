using Library.Crosscutting.Security;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Model.Organizations;
using Library.ViewModel.Organizations;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Library.Service.Organizations
{
    public class OrganogramService : IOrganogramService
    {
        private readonly IStructureRelationshipService _structureRelationshipService;
        private readonly IRepositoryAsync<Company> _companyRepository;
        private readonly ISqlRepository _sqlRepository;

        public OrganogramService(
            ISqlRepository sqlRepository
            , IRepositoryAsync<Company> companyRepository
            , IStructureRelationshipService structureRelationshipService)
        {
            _structureRelationshipService = structureRelationshipService;
            _companyRepository = companyRepository;
            _sqlRepository = sqlRepository;
        }

        public OrganoNode GetOrg()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"select E.CompanyGroupId, CG.UserName  AS CompanyGroupName, E.CompanyId, C.UserName AS CompanyName, E.PlantId, P.UserName AS PlantName, E.DivisionId, D.UserName AS DivisionName,
                        E.SubDivisionId, SD.UserName AS SubDivisionName, E.DepartmentId, DP.UserName AS DepartmentName, E.UnitId, U.UserName AS UnitName, E.EmployeeGroupId, EG.UserName AS EmployeeGroupName from [ORG].[Entity] AS E
                        LEFT JOIN [ORG].[CompanyGroup] AS CG ON CG.Id=E.CompanyGroupId
                        LEFT JOIN [ORG].[Company] AS C ON C.Id=E.CompanyId
                        LEFT JOIN [ORG].[Plant] AS P ON P.Id=E.PlantId
                        LEFT JOIN [ORG].[Division] AS D ON D.Id=E.DivisionId
                        LEFT JOIN [ORG].[SubDivision] AS SD ON SD.Id=E.SubDivisionId
                        LEFT JOIN [ORG].[Department] AS DP ON DP.Id=E.DepartmentId
                        LEFT JOIN [ORG].[Unit] AS U ON U.Id=E.UnitId
                        LEFT JOIN [HKP].[EmployeeGroup] AS EG ON EG.Id=E.EmployeeGroupId
                        where E.CompanyGroupId='" + identity.CompanyGroupId + "'";
            var entityList = _companyRepository.SqlQuery<OrganoViewModel>(sql).ToList();
            var companyOrganoNodeList = new List<OrganoNode>();
            var companyList = (from r in entityList
                               select new OrganoViewModel
                               {
                                   CompanyId = r.CompanyId,
                                   CompanyName = r.CompanyName
                               }).Distinct();
            foreach (var item in companyList)
            {
                var companyNode = new OrganoNode { name = item.CompanyName };
                var entityRelationship = _structureRelationshipService.Query(r => r.CompanyId == item.CompanyId && r.RType == RelationshipType.Entity.ToString() && r.Active && !r.Archive)
                                                        .Select().OrderBy(r => r.Sequence);
                var i = 1;
                var entity = entityRelationship.FirstOrDefault(r => r.Sequence == i);
                if (entity != null)
                {
                    if (entity.StandardName == RelationshipHK.EmployeeGroup)
                    {
                        var employeeGroupList = (from r in entityList.Where(r => r.CompanyId == item.CompanyId)
                                                 select new OrganoViewModel
                                                 {
                                                     EmployeeGroupId = r.EmployeeGroupId,
                                                     EmployeeGroupName = r.EmployeeGroupName
                                                 }).Distinct();
                        //companyNode.children = employeeGroupList;
                        i++;
                    }
                }
                if (entity != null)
                {
                    //entity = entityRelationship.FirstOrDefault(r => r.Sequence == i);
                    //if (entity != null)
                    //{
                    //    list = _companyRepository.SqlQuery<OrganoNode>($"SELECT UserName as [name] FROM [{entity.SchemaName}].[{entity.StandardName}]").ToList();
                    //    companyNode.children = list;
                    //    i++;
                    //}
                }

                companyOrganoNodeList.Add(companyNode);
            }
            var data = new OrganoNode(identity.CompanyGroupName, companyOrganoNodeList);
            return data;
        }

        public List<Dictionary<string, object>> GetList(string companyGroupId)
        {
            var sql = @"select CG.Id AS CompanyGroupId, CG.UserName AS CompanyGroupName, C.Id AS CompanyId, C.UserName AS CompanyName, P.Id AS PlantId, P.UserName AS PlantName from [ORG].[Company] AS C
                        left join[ORG].[Plant]
                                AS P ON P.CompanyId=C.Id
                        left join[ORG].[CompanyGroup] AS CG on CG.Id= C.CompanyGroupId
                        where C.CompanyGroupId= '" + companyGroupId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

      
    }
}