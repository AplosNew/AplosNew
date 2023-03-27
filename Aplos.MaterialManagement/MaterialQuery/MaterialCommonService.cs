using bplib;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Inventory;
using Library.ViewModel.Materials;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Aplos.MaterialManagement.MaterialQuery
{
    public class MaterialCommonService
    {
        private readonly ISqlRepository _sqlRepository;
        public MaterialCommonService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }

        public IEnumerable<POMaterial> GetInventoryMaterialByUpToSku(InventoryMaterialViewModel entity)
        {
            string materialmaster = "";
            string articleId = "";
            string firstCharacteristicsId = "";
            string firstCharacteristicsValueId = "";
            //string secondCharacteristicsId = "";
            //string secondCharacteristicsValueId = "";
            //string thirdCharacteristicsId = entity.ThirdCharacteristicsId;
            //string ThirdCharacteristicsValueId = entity.ThirdCharacteristicsValueId;

            if (string.IsNullOrEmpty(articleId)) { articleId = "AND  ArticleId=NULL"; } else { articleId = "AND  ArticleId = '" + entity.ArticleId + @"'"; }
            if (string.IsNullOrEmpty(firstCharacteristicsId)) { firstCharacteristicsId = "AND  FirstCharacteristicsId=NULL"; } else { firstCharacteristicsId = "AND  FirstCharacteristicsId = '" + entity.ArticleId + @"'"; }
            if (string.IsNullOrEmpty(firstCharacteristicsValueId)) { firstCharacteristicsValueId = "AND  FirstCharacteristicsValueId=NULL"; } else { firstCharacteristicsValueId = "AND  FirstCharacteristicsValueId = '" + entity.FirstCharacteristicsValueId + @"'"; }

            var sql = @"SELECT Top(1) * FROM TRN.InventoryMaterial WHERE MaterialMasterId='" + entity.MaterialMasterId + @"' " + articleId + " " + firstCharacteristicsId + " " + firstCharacteristicsValueId + @"
                                AND  SecondCharacteristicsId =ISNULL( '" + entity.SecondCharacteristicsId + @"',NULL) AND  SecondCharacteristicsValueId =ISNULL( '" + entity.SecondCharacteristicsValueId + @"',NULL)
                                AND  ThirdCharacteristicsId =ISNULL( '" + entity.ThirdCharacteristicsId + @"',NULL) AND  ThirdCharacteristicsValueId =ISNULL( '" + entity.ThirdCharacteristicsValueId + @"',NULL)
                                AND  CompanyId = '" + entity.CompanyId + @"' AND  PlantId ='" + entity.PlantId + @"'";
            return _sqlRepository.GetModelCollection<POMaterial>(sql);
        }

        public IEnumerable<object> GetALLUOMCbo()
        {
            var sql = @"SELECT u.Id Value,u.UserName [Text],mu.Id MaterialMasterId,1 BaseUOMFactor FROM scs.[UnitOfMeasurement] u  
                                    LEFT OUTER JOIN [MST].[MaterialMaster]  
                                    mu ON u.Id=mu.BaseUOMId 
                                    WHERE mu.Id IS NOT NULL  
									UNION ALL
									SELECT u.Id Value,u.UserName [Text],mu.MaterialMasterId,mu.BaseUOMFactor FROM scs.[UnitOfMeasurement] u  
                                    LEFT OUTER JOIN [MST].[MaterialMasterAlternativeUOM]  
                                    mu ON u.Id=mu.AlternativeUOMId 
                                    WHERE mu.MaterialMasterId IS NOT NULL  ";
             return _sqlRepository.GetDataCollection(sql);
        }
        public Dictionary<string, object> GetCompanyParty(string companyId, string plantId, string partyId, string partyType)
        {
            var sql = @"select TOP(1) * from hkp.CompanyParty where CompanyId='" + companyId + "'  and PartyId='" + partyId + "' and PartyType='" + partyType + @"'";
            var partyPlantTemp = _sqlRepository.GetData(sql);

            if (null == sql || partyPlantTemp.Count == 0)
                throw new CustomException("Plant party mapping not found.");
            return partyPlantTemp;
        }
        public void AddNewRow<T>(DataTable dt, T Data)
        {
            Dictionary<string, object> sourceData = Data.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(prop => prop.Name, prop => prop.GetValue(Data, null));
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            dt.Rows.Add(dr);
        }
        public void EditRow<T>(DataRow dr, T Data)
        {
            Dictionary<string, object> sourceData = Data.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(prop => prop.Name, prop => prop.GetValue(Data, null));

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            dr.BeginEdit();
            foreach (var item in sourceData.Keys)
            {
                try
                {
                    if (item.ToUpper() == "ID")
                        continue;

                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["UpdatedDate"] = DateTime.Now.ToString();
            dr.EndEdit();
        }

        public void AddNewRowD(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow(); foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress; dt.Rows.Add(dr);
        }

        public void EditRowD(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit(); foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }
        public string MakePK(string masterId, int currentId, int padLeft)
        {
            return masterId + currentId.ToString().PadLeft(padLeft, '0');
        }
        public void GenerateIDYearly(string strEntryDate, string strFieldName, out string strID)
        {
            ConnectionManager.DAL.ConManager objCoManager;

            string strSql = "";
            DataSet dsLocal = null;
            DataTable dtLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;
            // System.Text.StringBuilder SB = null;
            decimal MaxNumber = 0;

            try
            {
                //by Monir
                strEntryDate = clsWebLib.AppDateConvert(strEntryDate, "MM/dd/yyyy", clsWebLib.getUserDateFormat()).ToShortDateString();
                //strEntryDate = bplib.clsWebLib.AppDateConvert(strEntryDate, bplib.clsWebLib.getUserDateFormat(), "MM/dd/yyyy").ToString("MM/dd/yyyy");
                //strSql = "SELECT [Field], [Dates], [LastNumber], Year(Dates) as YearNo FROM Signature WHERE Field ='" + strFieldName.Trim() + "' and Year(Dates) = '" + Convert.ToDateTime(strEntryDate).Year.ToString() + "'";
                string period = Convert.ToDateTime(strEntryDate).Year.ToString();
                strSql = "SELECT Id, Period, FieldName, MaxNumber,UpdatedDate FROM ACS.PKGenerator WHERE FieldName ='" + strFieldName.Trim() + @"'  AND Period='" + period + "'";

                objCoManager = new ConnectionManager.DAL.ConManager("1");
                objCoManager.OpenDataSetThroughAdapter(strSql, out dsLocal, false, false, "", "1");


                dtLocal = dsLocal.Tables[0];
                dvLocal = new DataView(dtLocal);
                //dvLocal.Table = dtLocal;
                dvLocal.RowFilter = "FieldName ='" + strFieldName.Trim() + "' AND Period = '" + period + "'";
                if (dvLocal.Count == 0)
                {// Add data
                    drLocal = dtLocal.NewRow();
                    drLocal["FieldName"] = clsWebLib.RetValidLen(strFieldName, 100);
                    drLocal["Period"] = DateTime.Now.Year;
                    drLocal["MaxNumber"] = 1;
                    drLocal["UpdatedDate"] = DateTime.Now;
                    MaxNumber = 1;
                    dtLocal.Rows.Add(drLocal);
                }
                else if (dvLocal.Count == 1)
                {
                    drLocal = dvLocal[0].Row;

                    MaxNumber = Convert.ToDecimal(clsWebLib.GetNumData(("" + drLocal["MaxNumber"].ToString())));
                    MaxNumber = MaxNumber + 1;

                    drLocal.BeginEdit();
                    drLocal["FieldName"] = clsWebLib.RetValidLen(strFieldName, 100);
                    drLocal["Period"] = DateTime.Now.Year;
                    drLocal["MaxNumber"] = MaxNumber;
                    drLocal["UpdatedDate"] = DateTime.Now;
                    drLocal.EndEdit();
                }

                objCoManager.SaveDataSetThroughAdapter(ref dsLocal, false, "1");
                strID = /*strID + "-" +*/ (int)MaxNumber + "";
                strID = DateTime.Now.ToString("yyyy") + strID;

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                dtLocal = null;
                dvLocal = null;
                drLocal = null;
            }
        }
    }
}
