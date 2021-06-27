using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
#region Using
using Syncfusion.DocIO.DLS;
using Library.Service.Enums;
using Library.Service.Logs;
using System.Collections.Specialized;
using System.Linq;
using Syncfusion.XlsIO;
using Library.Service.Helpers;

#endregion Using

namespace Library.MaterialManagement.InventoryManagements
{
	public class InventorySalesService
	{
		private readonly SqlRepository _sqlRepository;

		#region Constructor
		public InventorySalesService() 
		{
			_sqlRepository = new SqlRepository();
		}
		#endregion Constructor

		private string GRNDAddiTaxId()
		{
			string sID = string.Empty;
			bplib.clsGenID objGenID = new bplib.clsGenID();
			objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "InventorySalesAdditionalTax", out sID); 
			return sID;
		}
		public void SaveAdditinalTaxInGRN(string MasterId, List<Dictionary<string, object>> UserSendData, string ToCurrencyRate)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				string sql = "select * from TRN.InventorySalesAdditionalTax where InventorySalesId='" + MasterId + "'";
				ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
				con.OpenDataSetThroughAdapter(sql, out DataSet dsDetail, false, "1");

				for (int i = 0; i < UserSendData.Count; i++)
				{
					dsDetail.Tables[0].DefaultView.RowFilter = "TaxCodeId='" + UserSendData[i]["TaxCodeId"].ToString() + "'";
					if (dsDetail.Tables[0].DefaultView.Count == 0)
					{

						DataRow dr = dsDetail.Tables[0].NewRow();
						dr["Id"] = GRNDAddiTaxId();
						dr["TaxCodeId"] = UserSendData[i]["TaxCodeId"];
						dr["Percentage"] = UserSendData[i]["ValueOfFixed"];
						dr["TaxAmount"] = UserSendData[i]["TaxAmount"];
						dr["AddedBy"] = identity.Name;
						dr["AddedDate"] = System.DateTime.Now.ToString();
						dr["AddedFromIP"] = identity.IPAddress;
						//dr["UpdatedBy"] = "";
						//dr["UpdatedDate"] = "";
						//dr["UpdatedFromIP"] = "";
						dr["InventorySalesId"] = MasterId.ToString();
						dr["TaxCategoryId"] = UserSendData[i]["TaxCategoryId"];
						dr["BooksCurrencyTaxAmount"] = Convert.ToDecimal(Convert.ToDecimal(UserSendData[i]["TaxAmount"]) * Convert.ToDecimal(ToCurrencyRate));
						dsDetail.Tables[0].Rows.Add(dr);
					}
					//else
					//{
					//	DataRow dr = dsDetail.Tables[0].DefaultView[0].Row;
					//	dr.BeginEdit();
					//	dr["ShortageRatePercent"] = UserSendData[i]["ShortageRate"];
					//	dr["ShortageValue"] = UserSendData[i]["ShortageValue"];
					//	dr["RejectRatePercent"] = UserSendData[i]["RejectionRate"];
					//	dr["RejectValue"] = UserSendData[i]["RejectionValue"];
					//	dr["RejectClamPercent"] = UserSendData[i]["RejectionClamRate"];
					//	dr.EndEdit();
					//}
				}


				clsStaticInfo info = new clsStaticInfo();
				info.SaveDataSets(dsDetail);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		public IEnumerable<object> GetAdvanceTaxInfo(string InventoryReceiveId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";
				sql = @"Select a.Id,a.TaxCodeId,a.Percentage ValueOfFixed,a.TaxAmount,a.AddedBy,a.AddedDate,a.AddedFromIP,b.UserName TaxName,InventorySalesId
						from [TRN].[InventorySalesAdditionalTax] a
						left join [mst].[TAXCode] b ON b.Id=a.TaxCodeId where a.InventorySalesId='" + InventoryReceiveId + "'";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}
		public IEnumerable<object> AdditionalTaxDelete(string Id)
		{
			try
			{
				var _sql = @" Delete from [TRN].[InventorySalesAdditionalTax] where Id='" + Id + @"'";
				return _sqlRepository.GetDataCollection(_sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}


		
	}
}
