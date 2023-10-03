using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Model.Calendars;
using Library.Model.Currencies;
using Library.Model.Employees;
using Library.Model.FixedAssets;
using Library.Model.Invoices;
using Library.Model.Organizations;
using Library.Model.Vouchers;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using Library.ViewModel.Vouchers;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Library.Accounting.Accounts
{
    public class FiscalYearCloseService
    {
        private readonly ISqlRepository _sqlRepository;
        public FiscalYearCloseService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }
        
        public void InsertFiscalYearClose(FiscalYearClose fiscalYearCloseVM)
        {
            try
            {
                var fiscalYearClose = new FiscalYearClose
                {
                   
                    CompanyGroupId = fiscalYearCloseVM.CompanyGroupId,
                    CompanyId = fiscalYearCloseVM.CompanyId,
                    PlantId = fiscalYearCloseVM.PlantId,
                    FiscalYearId = fiscalYearCloseVM.FiscalYearId
                   
                };
                InsertFiscalYearCloseData(fiscalYearClose, out DataSet _fiscalYearCloseData);

                clsStaticInfo objApp = new clsStaticInfo();
                objApp.SaveDataSets(_fiscalYearCloseData);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public FiscalYearClose InsertFiscalYearCloseData(FiscalYearClose fiscalYearClose, out DataSet dsData)
        {
            AccountsCommonService _accountsCommonService = new AccountsCommonService(_sqlRepository);
            
           
            if (!string.IsNullOrEmpty(fiscalYearClose.FiscalYearId))
            {
                DataTable Qry = _sqlRepository.GetDataTable("select * from [SCS].[FiscalYearClose] where FiscalYearId='" + fiscalYearClose.FiscalYearId + "' AND CompanyId='" + fiscalYearClose.CompanyId + "' AND PlantId='" + fiscalYearClose.PlantId + "' AND Id<>''");
                if (Qry.Rows.Count > 0)
                    throw new Exception("Data already exists!!!");

            }
            fiscalYearClose.Id = _accountsCommonService.GetAutoNumber(nameof(FiscalYearClose), PKGeneratorEnum.Yearly, null, DateTime.Now);
          
            if (string.IsNullOrEmpty(fiscalYearClose.AddedBy))
                AuditService.AddedLog(fiscalYearClose);

            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            con.getDataSet("Select * from [SCS].[FiscalYearClose] where 1=2", out dsData);

            AddNewRow<FiscalYearClose>(dsData.Tables[0], fiscalYearClose);

            return fiscalYearClose;
        }
        private void AddNewRow<T>(DataTable dt, T Data)
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
        private void EditRow<T>(DataRow dr, T Data)
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
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();

        }
        private void EditRow(DataSet ds)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (ds.Tables[0].Rows.Count > 0)
            {
                DataRow dr = ds.Tables[0].DefaultView[0].Row;

                dr.BeginEdit();
                dr["UpdatedBy"] = identity.Name;
                dr["UpdatedDate"] = DateTime.Now.ToString();
                dr["UpdatedFromIP"] = identity.IPAddress;
                dr.EndEdit();
            }
            clsStaticInfo obj = new clsStaticInfo();
            obj.SaveDataSets(ds);

        }

        public GridModel GetFiscalYearCloseList(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT FYC.Id,FY.Id Sequence,FY.FiscalYearName,C.UserName CompanyName,P.UserName PlantName
                                FROM [SCS].[FiscalYearClose] As FYC
                                LEFT JOIN [SCS].[FiscalYear] AS FY  ON FY.Id=FYC.FiscalYearId
                                LEFT JOIN  [ORG].[Company] AS C  ON C.Id=FYC.CompanyId
                                LEFT JOIN [ORG].[Plant] AS P  ON P.Id=FYC.PlantId";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        #region
        public List<Dictionary<string, object>> GetFiscalYearClosePostedList(string column, string value, string companyId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            var sql = @"select top 100 * from (SELECT V.Id,V.VoucherNo,V.DocRefNo,FORMAT(V.PostingDate, 'dd-MMM-yyyy') PostingDate
				,FY.FiscalYearName,C.UserName CompanyName,P.UserName PlantName,FYC.AdjustmentAmount Amount
				FROM TRN.Voucher V 
				INNER JOIN [SCS].[FiscalYearClose] FYC ON FYC.VoucherId=V.Id
				LEFT JOIN [SCS].[FiscalYear] AS FY  ON FY.Id=FYC.FiscalYearId
				LEFT JOIN  [ORG].[Company] AS C  ON C.Id=FYC.CompanyId
                LEFT JOIN [ORG].[Plant] AS P  ON P.Id=FYC.PlantId
                WHERE V.CompanyId='" + companyId + @"' AND V.Archive=0 
                ) AS TEMP WHERE " + strkey + " order by PostingDate DESC   ";
            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetFiscalYearCloseListForPosting()
        {
            string sql = @"SELECT FYC.Id,FY.Id Sequence,FY.FiscalYearName,C.UserName CompanyName,P.UserName PlantName,'' Amount
                                FROM [SCS].[FiscalYearClose] As FYC
                                LEFT JOIN [SCS].[FiscalYear] AS FY  ON FY.Id=FYC.FiscalYearId
                                LEFT JOIN  [ORG].[Company] AS C  ON C.Id=FYC.CompanyId
                                LEFT JOIN [ORG].[Plant] AS P  ON P.Id=FYC.PlantId
                                Where FYC.VoucherId IS NULL";
            return _sqlRepository.GetDataCollection(sql);
        }
        #endregion
    }
}
