using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.General.Conversions
{
    public class CurrencyConversions
    {
        SqlRepository _sqlRepository;
        string TableName = "";
        public CurrencyConversions(string ExchangeRateTableName)
        {
            _sqlRepository = new SqlRepository();
            TableName = ExchangeRateTableName;

        }

        public void SaveConversion(string TransactionId, List<Dictionary<string, object>> data)
        {
            DataSet dsLocal;
            try
            {

                if (data == null)
                    return;

                ConnectionManager.DAL.ConManager objCon;
                string sql = "select * from " + TableName + " where TransactionId='" + TransactionId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsLocal, false, "1");


                while (dsLocal.Tables[0].DefaultView.Count > 0)
                    dsLocal.Tables[0].DefaultView[0].Delete();

                for (int i = 0; i < data.Count; i++)
                {
                    DataRow dr = dsLocal.Tables[0].NewRow();

                    dr["TransactionId"] = TransactionId;
                    dr["FromCurrencyId"] = bplib.clsWebLib.RetValidLen(data[i]["FromCurrencyId"].ToString());
                    dr["ToCurrencyId"] = bplib.clsWebLib.RetValidLen(data[i]["ToCurrencyId"].ToString());
                    dr["ExchangeRate"] = OTSBD.clsStaticInfo.dbl(bplib.clsWebLib.RetValidLen(data[i]["ToUnit"].ToString()));
                    dsLocal.Tables[0].Rows.Add(dr);
                }


                OTSBD.clsStaticInfo info = new OTSBD.clsStaticInfo();
                info.SaveDataSets(dsLocal);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<Dictionary<string, object>> GetExchangeRates(string TransactionId, string CompanyId)
        {

            return _sqlRepository.GetDataCollection(@"SELECT 1 AS FromUnit, c.Id AS FromCurrencyId, c.Code AS FromCurrencyCode,bk.Id AS ToCurrencyId,bk.Code AS ToCurrencyCode,
                                        CASE WHEN isnull(ex.ExchangeRate,0)=0 THEN CASE WHEN c.Id=com.BaseCurrencyId THEN 1 ELSE 0 END  ELSE ex.ExchangeRate END  AS ToUnit
                                          FROM scs.CurrencyTransaction AS ct
                                        INNER JOIN scs.Currency AS c ON c.Id=ct.CurrencyId
                                        LEFT JOIN org.Company AS com ON com.id=ct.CompanyId
                                        LEFT JOIN scs.Currency AS bk ON bk.id=com.BaseCurrencyId
                                        LEFT JOIN " + TableName + @" EX ON ex.FromCurrencyId=c.Id AND ex.TransactionId='" + TransactionId + @"' 
                                        WHERE ct.[Active]=1 AND ct.CompanyId='" + CompanyId + @"'
                                        ORDER BY c.Sequence");
        }


        public List<Dictionary<string, object>> GetReportBaseCurrency(string PlantId)
        {

            string sql = @"SELECT c.* FROM ReportExchangeRates EX
                    JOIN scs.Currency AS c ON c.Id=ex.FromCurrencyId WHERE FromCurrencyId=ToCurrencyId AND PlantId='" + PlantId + "'";

            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetAllTransactionCurrency(string PlantId)
        {

            string sql = @"SELECT c.* FROM scs.CurrencyTransaction EX
                        JOIN scs.Currency AS c ON c.Id=ex.CurrencyId 
                        WHERE Ex.CompanyId=(SELECT top 1 p.CompanyId FROM org.Plant AS p WHERE p.Id='" + PlantId + "')";

            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetRelativeCurrencyMatrix(string PlantId, string BaseCurrencyId)
        {

            string sql = @"SELECT 1 AS FromFactor,cf.Id AS FromCurrencyId,cf.Code AS FromCurrency,
                            ct.Id AS ToCurrencyId,Ct.Code AS ToCurrency,r.ExchangeRate
                              FROM scs.CurrencyTransaction TR
                            LEFT JOIN ReportExchangeRates AS r ON tr.CurrencyId=r.ToCurrencyId AND r.PlantId='" + PlantId + @"' AND r.FromCurrencyId='" + BaseCurrencyId + @"'
                            LEFT JOIN scs.Currency AS cF ON cf.Id=isnull(r.FromCurrencyId,'" + BaseCurrencyId + @"')
                            LEFT JOIN scs.Currency AS cT ON cT.Id=TR.CurrencyId

                            WHERE tr.CompanyId=(SELECT top 1 p.CompanyId FROM org.Plant AS p WHERE p.Id='" + PlantId + @"') AND tr.CurrencyId<>'" + BaseCurrencyId + @"'
                            ";

            sql = @"SELECT 1 AS FromFactor,cf.Id AS FromCurrencyId,cf.Code AS FromCurrency,
                        ct.Id AS ToCurrencyId,Ct.Code AS ToCurrency,r.ExchangeRate
                        FROM scs.CurrencyTransaction TR
                        LEFT JOIN ReportExchangeRates AS r ON tr.CurrencyId=r.FromCurrencyId AND r.PlantId='" + PlantId + @"' AND r.ToCurrencyId='" + BaseCurrencyId + @"'
                        LEFT JOIN scs.Currency AS cT ON cT.Id=isnull(r.ToCurrencyId,'" + BaseCurrencyId + @"')
                        LEFT JOIN scs.Currency AS cF ON cF.Id=TR.CurrencyId

                        WHERE tr.CompanyId=(SELECT top 1 p.CompanyId FROM org.Plant AS p WHERE p.Id='" + PlantId + @"') AND tr.CurrencyId<>'" + BaseCurrencyId + @"'";

            return _sqlRepository.GetDataCollection(sql);
        }
        public void SaveReportCurrencyConversion(string PlantId, string BaseCurrencyId, List<Dictionary<string, object>> data)
        {
            DataSet dsLocal;
            try
            {

                if (data == null)
                    return;

                ConnectionManager.DAL.ConManager objCon;
                string sql = "select * from ReportExchangeRates where PlantId='" + PlantId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsLocal, false, "1");


                while (dsLocal.Tables[0].DefaultView.Count > 0)
                    dsLocal.Tables[0].DefaultView[0].Delete();

                DataRow dr = dsLocal.Tables[0].NewRow();

                dr["PlantId"] = PlantId;
                dr["FromCurrencyId"] =BaseCurrencyId;
                dr["ToCurrencyId"] = BaseCurrencyId;
                dr["ExchangeRate"] = 1;
                dsLocal.Tables[0].Rows.Add(dr);

                for (int i = 0; i < data.Count; i++)
                {
                     dr = dsLocal.Tables[0].NewRow();

                    dr["PlantId"] = PlantId;
                    dr["FromCurrencyId"] = bplib.clsWebLib.RetValidLen(data[i]["FromCurrencyId"].ToString());
                    dr["ToCurrencyId"] = bplib.clsWebLib.RetValidLen(data[i]["ToCurrencyId"].ToString());
                    dr["ExchangeRate"] = OTSBD.clsStaticInfo.dbl(bplib.clsWebLib.RetValidLen(data[i]["ExchangeRate"].ToString()));
                    dsLocal.Tables[0].Rows.Add(dr);
                }


                OTSBD.clsStaticInfo info = new OTSBD.clsStaticInfo();
                info.SaveDataSets(dsLocal);
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
