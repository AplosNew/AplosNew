#region Using

using Library.Data;
using Library.Data.Sql;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Securites;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web.Http;

#endregion Using

namespace Aplos.Controllers
{
    public class SecurityController : ApiController
    {
        #region Constructor

        private readonly IUserService _userService;
        private readonly ISqlRepository _sqlRepository;
        public SecurityController(IUserService userService, ISqlRepository sqlRepository)
        {
            _userService = userService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public IHttpActionResult GetLogin(string authenticationToken, string groupId, string companyId, string appId, string userId, string password)
        {
            var result = _userService.Login(authenticationToken, groupId, companyId, null, appId, userId, password);
            return Ok(result);
            //if (result["Status"].ToString() == "Success")
            //    return Ok(result);
            //else
            //    throw new Exception(result["ErrorText"].ToString());
        }

        public IHttpActionResult Get(string authenticationToken, string groupId)
        {
            try
            {
                if (!string.IsNullOrEmpty(authenticationToken) && !string.IsNullOrEmpty(groupId))
                {
                    _userService.CheckAuthenticationTokenWithCompanyGroup(authenticationToken, groupId);
                    return Ok(HttpStatusCode.OK);
                }
                else
                    throw new Exception("Secured Client Configuration Error!");
            }
            catch (Exception ex)
            {
                //var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                //{
                //    ReasonPhrase = ex.Message
                //};
                //throw new HttpResponseException(resp);
                throw ex;
            }
        }

       
        public IEnumerable<object> GetPlantList(string groupId, string companyId, string userId, bool flag)
        {
            try
            {
                var sql = "";
                if (flag)
                    sql = @"SELECT P.CompanyId, P.Id AS PlantId, P.Code, P.UserName AS PlantName FROM [ORG].[Plant] AS P WHERE P.CompanyGroupId='" + groupId + "' AND P.CompanyId='" + companyId + "' AND P.Active=1 AND P.Archive=0";
                else
                    sql = @"SELECT A.CompanyId, A.PlantId, P.Code, P.UserName AS PlantName FROM [SEC].[UserAccessPlant] AS A
                            JOIN [ORG].[Plant] AS P ON A.PlantId=P.Id WHERE A.Active=1 AND A.CompanyGroupId='" + groupId + "' AND A.UserId='" + userId + "'";


                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Module.ToString()));
            }
        }
        public HttpResponseMessage Error(string error)
        {
            return Request.CreateResponse(HttpStatusCode.OK, error);
        }

        


    }
}