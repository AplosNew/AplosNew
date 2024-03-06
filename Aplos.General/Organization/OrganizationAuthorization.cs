using Library.Crosscutting.Security;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.General.Organization
{

    public class OrganizationAuthorization
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
      
        public OrganizationAuthorization()
        {

            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
           
        }

        #region Entity Specific
        public List<Dictionary<string, object>> GetEntityByUser(string PlantId, string UserId, bool IsSysAdmin)
        {
            try
            {

                string sql = @"";
                if (IsSysAdmin)
                {
                    sql = @"SELECT distinct E.*,Flag=CAST(0 AS bit) FROM [ORG].[Entity] E
                            LEFT JOIN dbo.EntityConfig ECC ON ECC.EntityId=E.Id
                            WHERE E.PlantId='" + PlantId + @"' AND ECC.IsProductionEntity=1 AND E.[Active]=1 ORDER BY E.Code";
                    return _sqlRepository.GetDataCollection(sql);
                }

                sql = @"SELECT distinct e2.*,Flag=CAST(0 AS bit) FROM [SEC].[UserEntity] E
                        LEFT JOIN org.Entity AS e2 ON e2.Id=e.EntityId
                        LEFT JOIN dbo.EntityConfig ECC ON ECC.EntityId=E2.Id
                        WHERE E.UserId='" + UserId + @"' AND e.PlantId='" + PlantId + "' AND ECC.IsProductionEntity=1 AND E2.[Active]=1 ORDER BY E2.Code";
                return _sqlRepository.GetDataCollection(sql);


                throw new Exception("No entity configurations was found in the system for the current user");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        #endregion Entity Specific

        public List<Dictionary<string, object>> GetPlanStatus()
        {
            try
            {
                string sql = @"";
                sql = @"select * from HKP.ProductionStatus";
                    return _sqlRepository.GetDataCollection(sql); 
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        #region Process Specific

        public List<Dictionary<string, object>> GetEntityProcessCbo(bool IsSysAdmin, string userId, string entityId)
        {
            if (IsSysAdmin)
            {
                string _sql = @"SELECT DISTINCT P.Id AS [Value], P.UserName AS [Text],EP.ProductionBookingLevel FROM HKP.EntityProcessTag AS EP
                            JOIN HKP.Process AS P ON EP.ProcessId=P.Id WHERE EP.EntityId='" + entityId + "'";
                return _sqlRepository.GetDataCollection(_sql);
            }
            else
            {
                string _sql = @"SELECT P.Id AS [Value], P.UserName AS [Text],EPT.ProductionBookingLevel FROM HKP.EntityProcessTag EPT
						        INNER JOIN HKP.Process AS P ON P.Id=EPT.ProcessId
						        INNER JOIN [SEC].[UserProcess] UP ON UP.ProcessId=P.Id
						        WHERE EPT.EntityId='" + entityId + @"' AND UP.UserId='" + userId + "'";
                return _sqlRepository.GetDataCollection(_sql);
            }
        }
        public List<Dictionary<string, object>> GetProcessForMultipleEntitiesCbo(string entityIds)
        {
            
                string _sql = @"SELECT DISTINCT P.Id AS [Value], P.UserName AS [Text],EP.ProductionBookingLevel FROM HKP.EntityProcessTag AS EP
                            JOIN HKP.Process AS P ON EP.ProcessId=P.Id WHERE EP.EntityId IN (" + entityIds + ")";
                return _sqlRepository.GetDataCollection(_sql);
           
        }
        #endregion

        #region Plant Specific
        #endregion Plant Specific

        #region Company Specific
        #endregion Company Specific

        #region Group Specific
        #endregion Group Specific
    }
}
