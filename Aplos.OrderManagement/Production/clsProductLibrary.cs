using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace Library.OrderManagement.Production
{
    public class clsProductLibrary
    {
        SqlRepository _sqlRepository = null;

        public clsProductLibrary()
        {
            _sqlRepository = new SqlRepository();

        }

        public IEnumerable<object> GetCostingMasterTemplate()
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"select qcm.*, p.UserName as Customer, pm.UserName as ProductMaster 
							,pc.UserName as ProductCategory,CUR.Code AS Currency,ct.UserName AS CostingTypeName
							,psc.UserName as ProductSubCategory
                             ,pm.CostingType
							from CostingMasterTemplate qcm 
							left join [HKP].[Party] p ON p.Id = qcm.CustomerId
                            left join scs.Currency CUR on CUR.Id=qcm.CurrencyId
                            left join [MST].[ProductMaster] pm ON pm.Id = qcm.ProductMasterId
							left join [HKP].[ProductCategory] as pc on pc.Id = pm.ProductCategoryId
							left join [HKP].[ProductSubCategory] as psc on psc.Id = pm.ProductSubCategoryId
							LEFT JOIN CostingTypes AS ct ON ct.CostingType=pm.CostingType";

                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        public IEnumerable<object> GetRecipeGlobalMasterList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"SELECT BRM.Id, BRM.CompanyGroupId, BRM.CompanyId, BRM.EntityId, BRM.ProcessId, PR.UserName AS Process,
                                BRM.Code, BRM.UserName as 'Name',BRM.Description,E.UserName Entity FROM [TRN].[RecipeGlobalMaster] AS BRM
                                LEFT JOIN [HKP].[Process] AS PR ON BRM.ProcessId=PR.Id
                                LEFT JOIN [ORG].[Entity] AS E ON BRM.EntityId=E.Id
                                Where BRM.CompanyGroupId='" + identity.CompanyGroupId + "' AND BRM.CompanyId='" + identity.CompanyId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetProductLibraryAttribute(string masterId)
        {

            string sql = @"SELECT PA.*,SI.UserName ScanItem,U.Code UoM FROM [dbo].[ProductLibraryAttribute] PA
                            LEFT JOIN dbo.ScanItem SI ON SI.Id=PA.ScanItemId
                            LEFT JOIN SCS.UnitOfMeasurement U ON U.Id=PA.UoMId Where PA.ProductLibraryId='" + masterId + "'";

            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = "";

            sql = @"select top 100 * from (SELECT PL.*,MM.UserName MaterialMaster, RGM.UserName Recipe, MMA.StandardName Article, PM.UserName AS ProductMasterName, CT.UserName AS CostingMasterTemplate 
                        FROM [dbo].[ProductLibrary] PL
                        LEFT JOIN MST.[MaterialMaster] MM ON MM.Id = PL.MaterialMasterId
                        LEFT JOIN [TRN].[RecipeGlobalMaster] RGM ON RGM.Id = PL.RecipeId
                        LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id = PL.ArticleId
                        LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
                        LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
                        LEFT JOIN dbo.CostingMasterTemplate AS CT ON CT.Id=PL.CostingMasterTemplateId
                        WHERE PL.CompanyGroupId='" + identity.CompanyGroupId + "') AS TEMP WHERE " + strkey + " ORDER BY AddedDate DESC";
            return _sqlRepository.GetDataCollection(sql);
        }

        public void DeleteProductLibrary(string id)
        {
            string strSQL, strDCSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strDCSQL = "DELETE FROM [dbo].[ProductLibraryAttribute] Where ProductLibraryId ='" + id + "'";
                strSQL = "DELETE FROM [dbo].[ProductLibrary] WHERE Id = '" + id + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                objCon.ExecuteNonQueryWrapper(strDCSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        public void DeleteProductLibraryAttributeData(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {


                strSQL = "DELETE FROM [dbo].[ProductLibraryAttribute] Where Id ='" + id + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        public string ProductLibrarySql(string IDs)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"SELECT PL.*,MM.UserName MaterialMaster, RGM.UserName Recipe, MMA.StandardName Article, 
PM.UserName AS ProductMasterName,PLA.UserName Attribute,PLA.AttributeValue
FROM [dbo].[ProductLibrary] PL
LEFT JOIN MST.[MaterialMaster] MM ON MM.Id = PL.MaterialMasterId
LEFT JOIN [TRN].[RecipeGlobalMaster] RGM ON RGM.Id = PL.RecipeId
LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id = PL.ArticleId
LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
LEFT JOIN [dbo].[ProductLibraryAttribute] PLA ON PLA.ProductLibraryId=PL.Id
WHERE PL.CompanyGroupId='" + identity.CompanyGroupId + @"' --and ISnull( PL.Id,'') in(" + IDs + @")
ORDER BY PL.Sequence  ";

        }


    }
}
