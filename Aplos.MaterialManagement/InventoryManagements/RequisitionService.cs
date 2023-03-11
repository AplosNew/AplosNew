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
	public class RequisitionService
	{
		private readonly SqlRepository _sqlRepository;

		#region Constructor
		public RequisitionService()
		{
			_sqlRepository = new SqlRepository();
		}
		#endregion Constructor

		public IEnumerable<object> GetFiscalYear(string formattedDate)
		{
			//  DateTime date = DateTime.Now;
			// string formattedDate = date.ToString("dd-MM-yyyy");
			try
			{
				var _sql = @"SELECT REPLACE(CONVERT(VARCHAR(11),StartDate, 113), ' ', '-') StartDate, REPLACE(CONVERT(VARCHAR(11),EndDate, 113), ' ', '-') EndDate  FROM scs.FiscalYear WHERE StartDate <='" + formattedDate + "' AND EndDate >='" + formattedDate + "'";
				return _sqlRepository.GetDataCollection(_sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public IEnumerable<object> LoadRequisitionMasterTotalEmpWise1(string MaterialMasterId, string startDate, string endDate, string empId)//convert(varchar, convert(money, x.POTotalAmount), 1)
		{
			

			string strSQL;
			//clsConnection objCon;
			try
			{			strSQL = @"select --sum(x.RequisitionId) RequisitionId,x.ReqEmpId,x.EmployeeName,sum(Round(x.ReqTotalAmount,2)) ReqTotalAmount,sum(Round(x.POTotalAmount,2)) POTotalAmount,sum(Round(x.GRNTOtalAmount,2)) GRNTOtalAmount 
									x.Code,sum(x.RequisitionId) RequisitionId,x.ReqEmpId,x.EmployeeName,convert(varchar, convert(money, sum(x.ReqTotalAmount)), 1) ReqTotalAmount,convert(varchar, convert(money, sum(x.POTotalAmount)), 1) POTotalAmount,sum(Round(x.GRNTOtalAmount,2)) GRNTOtalAmount 
                                        from (
                                        Select  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName, Count(MR.Id) RequisitionId, 0 ReqTotalAmount,0 POTotalAmount,0 GRNTOtalAmount 
                                        from trn.MaterialRequsitionMaster MR
										 LEFT JOIN(select Id,MaterialReqqusitionMasterId,CurrencyId ,Sum(TotalAmount) TotalAmount 
		                                        FROM trn.MaterialRequsitionDetails group by Id,MaterialReqqusitionMasterId,CurrencyId) MRD ON MRD.MaterialReqqusitionMasterId=MR.Id
                                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=MR.ReqEmpId
											left join scs.Currency C On MRD.CurrencyId=C.Id
                                        where MR.ReqEmpId='" + empId + "' and  MR.RequisitionDate between '" + startDate + "' AND '" + endDate + @"'
                                        Group by  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName

                                        UNION All
                                        Select  MRD.CurrencyId,C.Code, MR.ReqEmpId,EI.EmployeeName, 0 RequisitionId,Sum(CONVERT(NUMERIC(10,2),MRD.TotalAmount)) ReqTotalAmount,0 POTotalAmount,0 GRNTOtalAmount 
                                        from trn.MaterialRequsitionMaster MR
                                        LEFT JOIN(select Id,MaterialReqqusitionMasterId,CurrencyId ,Sum(TotalAmount) TotalAmount 
		                                        FROM trn.MaterialRequsitionDetails group by Id,MaterialReqqusitionMasterId,CurrencyId) MRD ON MRD.MaterialReqqusitionMasterId=MR.Id
                                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=MR.ReqEmpId
										left join scs.Currency C On MRD.CurrencyId=C.Id
                                        where MR.ReqEmpId='" + empId + "' and  MR.RequisitionDate between '" + startDate + "' AND '" + endDate + @"'
                                        Group by  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName

                                        UNION All
                                        Select  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName, 0 RequisitionId,0 ReqTotalAmount,sum(CONVERT(NUMERIC(10,2),BaseAmount)) POTotalAmount,0 GRNTOtalAmount 
                                        from trn.MaterialRequsitionMaster MR
                                        LEFT JOIN(select Id,MaterialReqqusitionMasterId,CurrencyId ,Sum(TotalAmount) TotalAmount 
                                        FROM trn.MaterialRequsitionDetails group by Id,MaterialReqqusitionMasterId,CurrencyId) MRD ON MRD.MaterialReqqusitionMasterId=MR.Id
                                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=MR.ReqEmpId                               
                                        left JOIN(select Id, RequisitionDetailId ,Sum(BaseAmount) BaseAmount from trn.PurchaseOrderDetail where RequisitionDetailId is not NULL  group by RequisitionDetailId,Id)PO ON PO.RequisitionDetailId=MRD.Id
                                        	left join scs.Currency C On MRD.CurrencyId=C.Id
                                        where MR.ReqEmpId='" + empId + "' and  MR.RequisitionDate between '" + startDate + "' AND '" + endDate + @"'
                                        Group by  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName
                                        )x
                                        Group By x.Code,x.EmployeeName,x.ReqEmpId";

				return _sqlRepository.GetDataCollection(strSQL);

			}
			catch (System.Exception ex)
			{
				throw (ex);
			}
			finally
			{

			}
		}

		public IEnumerable<object> RequisitionByEmpInFixsal(string startDate, string endDate)
		{
            if (startDate== "Invalid Date")
            {
				startDate = "";
			}
			if (endDate == "Invalid Date")
            {
				endDate = "";
			}
			string strSQL;
			//clsConnection objCon;
			try
			{
				strSQL = @"select Distinct EI.SystemId Value,EI.EmployeeName Text from trn.MaterialRequsitionMaster MRM
							LEFT JOIN dbo.EmployeeInformation EI On EI.SystemId=MRM.ReqEmpId
							where MRM.RequisitionDate between '" + startDate + "' AND '" + endDate + @"'";
				return _sqlRepository.GetDataCollection(strSQL);

			}
			catch (System.Exception ex)
			{
				throw (ex);
			}
			finally
			{

			}
		}
        public IEnumerable<object> LoadServiceRequisitionMasterTotalEmpWise1(string MaterialMasterId, string startDate, string endDate, string empId)//convert(varchar, convert(money, x.POTotalAmount), 1)
        {

            string strSQL;
            //clsConnection objCon;
            try
            {  
                strSQL = @"select x.Code,sum(x.RequisitionId) RequisitionId,x.ReqEmpId,x.EmployeeName,convert(varchar, convert(money, sum(x.ReqTotalAmount)), 1) ReqTotalAmount,convert(varchar, convert(money, sum(x.POTotalAmount)), 1) POTotalAmount,sum(Round(x.GRNTOtalAmount,2)) GRNTOtalAmount 
								from (
								Select  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName, Count(MR.Id) RequisitionId, 0 ReqTotalAmount,0 POTotalAmount,0 GRNTOtalAmount 
								from trn.ServiceRequsitionMaster MR
									LEFT JOIN(select Id,ServiceRequisitionMasterID,CurrencyId ,Sum(TotalServiceBooksCurrencyAmount) TotalAmount 
										FROM trn.ServiceRequsitionDetail group by Id,ServiceRequisitionMasterID,CurrencyId) MRD ON MRD.ServiceRequisitionMasterID=MR.Id
								LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=MR.ReqEmpId
									left join scs.Currency C On MRD.CurrencyId=C.Id
								where MR.ReqEmpId='"+empId+@"' and  MR.RequisitionDate between '"+startDate+@"' AND '"+endDate+ @"'
								Group by  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName

								UNION All
								Select  MRD.CurrencyId,C.Code, MR.ReqEmpId,EI.EmployeeName, 0 RequisitionId,Sum(CONVERT(NUMERIC(10,2),MRD.TotalAmount)) ReqTotalAmount,0 POTotalAmount,0 GRNTOtalAmount 
								from trn.ServiceRequsitionMaster MR
								LEFT JOIN(select Id,ServiceRequisitionMasterID,CurrencyId ,Sum(TotalServiceBooksCurrencyAmount) TotalAmount 
										FROM trn.ServiceRequsitionDetail group by Id,ServiceRequisitionMasterID,CurrencyId) MRD ON MRD.ServiceRequisitionMasterID=MR.Id
								LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=MR.ReqEmpId
								left join scs.Currency C On MRD.CurrencyId=C.Id
								where MR.ReqEmpId='" + empId + @"' and  MR.RequisitionDate between '" + startDate + @"' AND '" + endDate + @"'
								Group by  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName

								UNION All
								Select  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName, 0 RequisitionId,0 ReqTotalAmount,sum(CONVERT(NUMERIC(10,2),BaseAmount)) POTotalAmount,0 GRNTOtalAmount 
								from trn.ServiceRequsitionMaster MR
								LEFT JOIN(select Id,ServiceRequisitionMasterID,CurrencyId ,Sum(TotalServiceBooksCurrencyAmount) TotalAmount 
								FROM trn.ServiceRequsitionDetail group by Id,ServiceRequisitionMasterID,CurrencyId) MRD ON MRD.ServiceRequisitionMasterID=MR.Id
								LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=MR.ReqEmpId                               
								left JOIN(select Id, ServiceRequsitionDetailId ,Sum(Amount) BaseAmount from trn.ServicePODetail where ServiceRequsitionDetailId is not NULL  group by ServiceRequsitionDetailId,Id)PO ON PO.ServiceRequsitionDetailId=MRD.Id
									left join scs.Currency C On MRD.CurrencyId=C.Id
								where MR.ReqEmpId='" + empId + @"' and  MR.RequisitionDate between '" + startDate + @"' AND '" + endDate + @"'
								Group by  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName
								)x
								Group By x.Code,x.EmployeeName,x.ReqEmpId";

                return _sqlRepository.GetDataCollection(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }



		public IEnumerable<object> RequisitionByEmpInMonth(string MaterialMasterId, DateTime startDate, DateTime endDate, string empId)//convert(varchar, convert(money, x.POTotalAmount), 1)
		{
			string strSQL;
			
			try
			{
				strSQL = @"select --sum(x.RequisitionId) RequisitionId,x.ReqEmpId,x.EmployeeName,sum(Round(x.ReqTotalAmount,2)) ReqTotalAmount,sum(Round(x.POTotalAmount,2)) POTotalAmount,sum(Round(x.GRNTOtalAmount,2)) GRNTOtalAmount 
									x.Code,sum(x.RequisitionId) RequisitionId,x.ReqEmpId,x.EmployeeName,convert(varchar, convert(money, sum(x.ReqTotalAmount)), 1) ReqTotalAmount,convert(varchar, convert(money, sum(x.POTotalAmount)), 1) POTotalAmount,sum(Round(x.GRNTOtalAmount,2)) GRNTOtalAmount 
                                        from (
                                        Select  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName, Count(MR.Id) RequisitionId, 0 ReqTotalAmount,0 POTotalAmount,0 GRNTOtalAmount 
                                        from trn.MaterialRequsitionMaster MR
										 LEFT JOIN(select Id,MaterialReqqusitionMasterId,CurrencyId ,Sum(TotalAmount) TotalAmount 
		                                        FROM trn.MaterialRequsitionDetails group by Id,MaterialReqqusitionMasterId,CurrencyId) MRD ON MRD.MaterialReqqusitionMasterId=MR.Id
                                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=MR.ReqEmpId
											left join scs.Currency C On MRD.CurrencyId=C.Id
                                        where MR.ReqEmpId='" + empId + "' and  MR.RequisitionDate between '" + startDate + "' AND '" + endDate + @"'
                                        Group by  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName

                                        UNION All
                                        Select  MRD.CurrencyId,C.Code, MR.ReqEmpId,EI.EmployeeName, 0 RequisitionId,Sum(CONVERT(NUMERIC(10,2),MRD.TotalAmount)) ReqTotalAmount,0 POTotalAmount,0 GRNTOtalAmount 
                                        from trn.MaterialRequsitionMaster MR
                                        LEFT JOIN(select Id,MaterialReqqusitionMasterId,CurrencyId ,Sum(TotalAmount) TotalAmount 
		                                        FROM trn.MaterialRequsitionDetails group by Id,MaterialReqqusitionMasterId,CurrencyId) MRD ON MRD.MaterialReqqusitionMasterId=MR.Id
                                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=MR.ReqEmpId
										left join scs.Currency C On MRD.CurrencyId=C.Id
                                        where MR.ReqEmpId='" + empId + "' and  MR.RequisitionDate between '" + startDate + "' AND '" + endDate + @"'
                                        Group by  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName

                                        UNION All
                                        Select  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName, 0 RequisitionId,0 ReqTotalAmount,sum(CONVERT(NUMERIC(10,2),BaseAmount)) POTotalAmount,0 GRNTOtalAmount 
                                        from trn.MaterialRequsitionMaster MR
                                        LEFT JOIN(select Id,MaterialReqqusitionMasterId,CurrencyId ,Sum(TotalAmount) TotalAmount 
                                        FROM trn.MaterialRequsitionDetails group by Id,MaterialReqqusitionMasterId,CurrencyId) MRD ON MRD.MaterialReqqusitionMasterId=MR.Id
                                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=MR.ReqEmpId                               
                                        left JOIN(select Id, RequisitionDetailId ,Sum(BaseAmount) BaseAmount from trn.PurchaseOrderDetail where RequisitionDetailId is not NULL  group by RequisitionDetailId,Id)PO ON PO.RequisitionDetailId=MRD.Id
                                        	left join scs.Currency C On MRD.CurrencyId=C.Id
                                        where MR.ReqEmpId='" + empId + "' and  MR.RequisitionDate between '" + startDate + "' AND '" + endDate + @"'
                                        Group by  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName
                                        )x
                                        Group By x.Code,x.EmployeeName,x.ReqEmpId";

				return _sqlRepository.GetDataCollection(strSQL);

			}
			catch (System.Exception ex)
			{
				throw (ex);
			}
			finally
			{

			}
		}

		public IEnumerable<object> ServiceRequisitionByEmpInMonth(string MaterialMasterId, DateTime startDate, DateTime endDate, string empId)//convert(varchar, convert(money, x.POTotalAmount), 1)
		{
			string strSQL;

			try
			{
				//strSQL = @"select --sum(x.RequisitionId) RequisitionId,x.ReqEmpId,x.EmployeeName,sum(Round(x.ReqTotalAmount,2)) ReqTotalAmount,sum(Round(x.POTotalAmount,2)) POTotalAmount,sum(Round(x.GRNTOtalAmount,2)) GRNTOtalAmount 
				//					x.Code,sum(x.RequisitionId) RequisitionId,x.ReqEmpId,x.EmployeeName,convert(varchar, convert(money, sum(x.ReqTotalAmount)), 1) ReqTotalAmount,convert(varchar, convert(money, sum(x.POTotalAmount)), 1) POTotalAmount,sum(Round(x.GRNTOtalAmount,2)) GRNTOtalAmount 
				//                                    from (
				//                                    Select  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName, Count(MR.Id) RequisitionId, 0 ReqTotalAmount,0 POTotalAmount,0 GRNTOtalAmount 
				//                                    from trn.MaterialRequsitionMaster MR
				//						 LEFT JOIN(select Id,MaterialReqqusitionMasterId,CurrencyId ,Sum(TotalAmount) TotalAmount 
				//                                      FROM trn.MaterialRequsitionDetails group by Id,MaterialReqqusitionMasterId,CurrencyId) MRD ON MRD.MaterialReqqusitionMasterId=MR.Id
				//                                    LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=MR.ReqEmpId
				//							left join scs.Currency C On MRD.CurrencyId=C.Id
				//                                    where MR.ReqEmpId='" + empId + "' and  MR.RequisitionDate between '" + startDate + "' AND '" + endDate + @"'
				//                                    Group by  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName

				//                                    UNION All
				//                                    Select  MRD.CurrencyId,C.Code, MR.ReqEmpId,EI.EmployeeName, 0 RequisitionId,Sum(CONVERT(NUMERIC(10,2),MRD.TotalAmount)) ReqTotalAmount,0 POTotalAmount,0 GRNTOtalAmount 
				//                                    from trn.MaterialRequsitionMaster MR
				//                                    LEFT JOIN(select Id,MaterialReqqusitionMasterId,CurrencyId ,Sum(TotalAmount) TotalAmount 
				//                                      FROM trn.MaterialRequsitionDetails group by Id,MaterialReqqusitionMasterId,CurrencyId) MRD ON MRD.MaterialReqqusitionMasterId=MR.Id
				//                                    LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=MR.ReqEmpId
				//						left join scs.Currency C On MRD.CurrencyId=C.Id
				//                                    where MR.ReqEmpId='" + empId + "' and  MR.RequisitionDate between '" + startDate + "' AND '" + endDate + @"'
				//                                    Group by  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName

				//                                    UNION All
				//                                    Select  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName, 0 RequisitionId,0 ReqTotalAmount,sum(CONVERT(NUMERIC(10,2),BaseAmount)) POTotalAmount,0 GRNTOtalAmount 
				//                                    from trn.MaterialRequsitionMaster MR
				//                                    LEFT JOIN(select Id,MaterialReqqusitionMasterId,CurrencyId ,Sum(TotalAmount) TotalAmount 
				//                                    FROM trn.MaterialRequsitionDetails group by Id,MaterialReqqusitionMasterId,CurrencyId) MRD ON MRD.MaterialReqqusitionMasterId=MR.Id
				//                                    LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=MR.ReqEmpId                               
				//                                    left JOIN(select Id, RequisitionDetailId ,Sum(BaseAmount) BaseAmount from trn.PurchaseOrderDetail where RequisitionDetailId is not NULL  group by RequisitionDetailId,Id)PO ON PO.RequisitionDetailId=MRD.Id
				//                                    	left join scs.Currency C On MRD.CurrencyId=C.Id
				//                                    where MR.ReqEmpId='" + empId + "' and  MR.RequisitionDate between '" + startDate + "' AND '" + endDate + @"'
				//                                    Group by  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName
				//                                    )x
				//                                    Group By x.Code,x.EmployeeName,x.ReqEmpId";

				strSQL = @"select 
							x.Code,sum(x.RequisitionId) RequisitionId,x.ReqEmpId,x.EmployeeName,convert(varchar, convert(money, sum(x.ReqTotalAmount)), 1) ReqTotalAmount,convert(varchar, convert(money, sum(x.POTotalAmount)), 1) POTotalAmount,sum(Round(x.GRNTOtalAmount,2)) GRNTOtalAmount 
							from (
							Select  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName, Count(MR.Id) RequisitionId, 0 ReqTotalAmount,0 POTotalAmount,0 GRNTOtalAmount 
							from trn.ServiceRequsitionMaster MR
								LEFT JOIN(select Id,ServiceRequisitionMasterID,CurrencyId ,Sum(TotalServiceTranAmount) TotalAmount 
									FROM trn.ServiceRequsitionDetail group by Id,ServiceRequisitionMasterID,CurrencyId) MRD ON MRD.ServiceRequisitionMasterID=MR.Id
							LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=MR.ReqEmpId
								left join scs.Currency C On MRD.CurrencyId=C.Id
							where MR.ReqEmpId='" + empId + "' and  MR.RequisitionDate between '" + startDate + "' AND '" + endDate + @"'
							Group by  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName

							UNION All
							Select  MRD.CurrencyId,C.Code, MR.ReqEmpId,EI.EmployeeName, 0 RequisitionId,Sum(CONVERT(NUMERIC(10,2),MRD.TotalAmount)) ReqTotalAmount,0 POTotalAmount,0 GRNTOtalAmount 
							from trn.ServiceRequsitionMaster MR
							LEFT JOIN(select Id,ServiceRequisitionMasterID,CurrencyId ,Sum(TotalServiceTranAmount) TotalAmount 
									FROM trn.ServiceRequsitionDetail group by Id,ServiceRequisitionMasterID,CurrencyId) MRD ON MRD.ServiceRequisitionMasterID=MR.Id
							LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=MR.ReqEmpId
							left join scs.Currency C On MRD.CurrencyId=C.Id
							where MR.ReqEmpId='" + empId + "' and  MR.RequisitionDate between '" + startDate + "' AND '" + endDate + @"'
							Group by  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName

							UNION All
							Select  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName, 0 RequisitionId,0 ReqTotalAmount,sum(CONVERT(NUMERIC(10,2),BaseAmount)) POTotalAmount,0 GRNTOtalAmount 
							from trn.ServiceRequsitionMaster MR
							LEFT JOIN(select Id,ServiceRequisitionMasterID,CurrencyId ,Sum(TotalServiceTranAmount) TotalAmount 
							FROM trn.ServiceRequsitionDetail group by Id,ServiceRequisitionMasterID,CurrencyId) MRD ON MRD.ServiceRequisitionMasterID=MR.Id
							LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=MR.ReqEmpId                               
							left JOIN(select Id, ServiceRequsitionDetailId ,Sum(Amount) BaseAmount from trn.ServicePODetail where ServiceRequsitionDetailId is not NULL  group by ServiceRequsitionDetailId,Id)PO ON PO.ServiceRequsitionDetailId=MRD.Id
								left join scs.Currency C On MRD.CurrencyId=C.Id
							where MR.ReqEmpId='" + empId + "' and  MR.RequisitionDate between '" + startDate + "' AND '" + endDate + @"'
							Group by  MRD.CurrencyId,C.Code,MR.ReqEmpId,EI.EmployeeName
							)x
							Group By x.Code,x.EmployeeName,x.ReqEmpId";

				return _sqlRepository.GetDataCollection(strSQL);

			}
			catch (System.Exception ex)
			{
				throw (ex);
			}
			finally
			{

			}
		}
		public IEnumerable<object> GetAllReqdata(string ReqStatus)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				var sql = "";
				if (ReqStatus == "ForChecked")
				{
					sql = @"                   SELECT *
FROM ( Select 
	                        SRM.Id 
	                        ,REPLACE(CONVERT(CHAR(11), SRM.RequisitionDate, 106),' ','-') AS RequisitionDate ,REPLACE(CONVERT(CHAR(11), SRM.RequisitionDate, 106),' ','-') AS RequisitionDate1 
	                        ,SRM.RequisitionType
	                        ,SRM.RequirmentType
	                        ,E.UserName EntityName
							,E.Id EntityId
							--,EI.SystemId CheckedBy
                            ,SRM.ReasonWhyItIsNotPlanEarlier 
                            ,ei.SystemId CheckedBy
                            ,ei.EmployeeName AS CheckedByName
                            ,SRM.CheckedByStatus
	                        ,ei1.EmployeeName AS AuthorizedByName
		                    ,ei2.EmployeeName As EmployeeName
							,ei3.EmployeeName AS ResponsiblePersonName
                             ,SUM(SRD.TotalServiceTranAmount) TotalServiceTranAmount
							 ,SUM(SRD.TotalServiceBooksCurrencyAmount) TotalServiceBooksCurrencyAmount
                            
                          FROM [TRN].[ServiceRequsitionMaster] SRM 
                          Left Join (select ServiceRequisitionMasterID,sum(TotalServiceTranAmount) TotalServiceTranAmount, sum(TotalServiceBooksCurrencyAmount) TotalServiceBooksCurrencyAmount from [TRN].[ServiceRequsitionDetail] group by ServiceRequisitionMasterID)SRD On SRD.ServiceRequisitionMasterID = SRM.Id
                        Left Join org.Entity E on E.Id=SRM.EntityId
                        LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=SRM.CheckedBy
                        LEFT JOIN EmployeeInformation AS ei1 ON ei1.SystemId=SRM.AuthorizedBy
					   LEFT JOIN EmployeeInformation AS ei2 ON ei2.SystemId=SRM.QualityApprovalResponsiblePersonId
					   	 LEFT JOIN EmployeeInformation AS ei3 ON ei3.SystemId=SRM.NeedSpecialAppId
                        Where SRM.ReqEmpId='" + identity.EmployeeId + @"' 
						AND SRM.CheckedByStatus='For Checking' 
						AND SRM.AuthorizedByStatus IS NULL 
						group by SRM.Id,SRM.RequisitionDate,SRM.RequisitionType,SRM.RequirmentType,E.UserName,SRM.ReasonWhyItIsNotPlanEarlier,
						ei.EmployeeName ,ei1.EmployeeName,SRM.CheckedByStatus,E.Id,EI.SystemId ,ei1.EmployeeName 
		                ,ei2.EmployeeName ,SRM.CheckedBy
					,ei3.EmployeeName 
              UNION ALL
					     Select 
	                        SRM.Id 
	                        ,REPLACE(CONVERT(CHAR(11), SRM.RequisitionDate, 106),' ','-') AS RequisitionDate ,REPLACE(CONVERT(CHAR(11), SRM.RequisitionDate, 106),' ','-') AS RequisitionDate1 
	                        ,SRM.RequisitionType
	                        ,SRM.RequirmentType
	                        ,E.UserName EntityName
							,E.Id EntityId
							--,EI.SystemId CheckedBy
                            ,SRM.ReasonWhyItIsNotPlanEarlier 
                            ,ei.SystemId CheckedBy
                            ,ei.EmployeeName AS CheckedByName
                            ,SRM.CheckedByStatus
	                        ,ei1.EmployeeName AS AuthorizedByName
		                    ,ei2.EmployeeName As EmployeeName
							,ei3.EmployeeName AS ResponsiblePersonName
                             ,SUM(SRD.TotalServiceTranAmount) TotalServiceTranAmount
							 ,SUM(SRD.TotalServiceBooksCurrencyAmount) TotalServiceBooksCurrencyAmount
                            
                          FROM [TRN].[ServiceRequsitionMaster] SRM 
                          Left Join (select ServiceRequisitionMasterID,sum(TotalServiceTranAmount) TotalServiceTranAmount, sum(TotalServiceBooksCurrencyAmount) TotalServiceBooksCurrencyAmount from [TRN].[ServiceRequsitionDetail] group by ServiceRequisitionMasterID)SRD On SRD.ServiceRequisitionMasterID = SRM.Id
                        Left Join org.Entity E on E.Id=SRM.EntityId
                        LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=SRM.CheckedBy
                        LEFT JOIN EmployeeInformation AS ei1 ON ei1.SystemId=SRM.AuthorizedBy
					   LEFT JOIN EmployeeInformation AS ei2 ON ei2.SystemId=SRM.QualityApprovalResponsiblePersonId
					   	 LEFT JOIN EmployeeInformation AS ei3 ON ei3.SystemId=SRM.NeedSpecialAppId
                        Where SRM.ReqEmpId='" + identity.EmployeeId + @"' 
						AND SRM.CheckedByStatus IS NULL
						AND SRM.AuthorizedByStatus = 'For Approval'
						group by SRM.Id,SRM.RequisitionDate,SRM.RequisitionType,SRM.RequirmentType,E.UserName,SRM.ReasonWhyItIsNotPlanEarlier,
						ei.EmployeeName ,ei1.EmployeeName,SRM.CheckedByStatus,E.Id,EI.SystemId ,ei1.EmployeeName 
		                ,ei2.EmployeeName ,SRM.CheckedBy
					,ei3.EmployeeName 
               UNION ALL

					Select 
	                        SRM.Id 
	                        ,REPLACE(CONVERT(CHAR(11), SRM.RequisitionDate, 106),' ','-') AS RequisitionDate ,REPLACE(CONVERT(CHAR(11), SRM.RequisitionDate, 106),' ','-') AS RequisitionDate1 
	                        ,SRM.RequisitionType
	                        ,SRM.RequirmentType
	                        ,E.UserName EntityName
							,E.Id EntityId
							--,EI.SystemId CheckedBy
                            ,SRM.ReasonWhyItIsNotPlanEarlier 
                            ,ei.SystemId CheckedBy
                            ,ei.EmployeeName AS CheckedByName
                            ,SRM.CheckedByStatus
	                        ,ei1.EmployeeName AS AuthorizedByName
		                    ,ei2.EmployeeName As EmployeeName
							,ei3.EmployeeName AS ResponsiblePersonName
                             ,SUM(SRD.TotalServiceTranAmount) TotalServiceTranAmount
							 ,SUM(SRD.TotalServiceBooksCurrencyAmount) TotalServiceBooksCurrencyAmount
                            --,SRM.CheckedBy
                          FROM [TRN].[ServiceRequsitionMaster] SRM 
                          Left Join (select ServiceRequisitionMasterID,sum(TotalServiceTranAmount) TotalServiceTranAmount, sum(TotalServiceBooksCurrencyAmount) TotalServiceBooksCurrencyAmount from [TRN].[ServiceRequsitionDetail] group by ServiceRequisitionMasterID)SRD On SRD.ServiceRequisitionMasterID = SRM.Id
                        Left Join org.Entity E on E.Id=SRM.EntityId
                        LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=SRM.CheckedBy
                        LEFT JOIN EmployeeInformation AS ei1 ON ei1.SystemId=SRM.AuthorizedBy
					   LEFT JOIN EmployeeInformation AS ei2 ON ei2.SystemId=SRM.QualityApprovalResponsiblePersonId
					   	 LEFT JOIN EmployeeInformation AS ei3 ON ei3.SystemId=SRM.NeedSpecialAppId
                        Where SRM.ReqEmpId='" + identity.EmployeeId + @"' 
						AND SRM.CheckedByStatus IS NULL
						AND SRM.AuthorizedByStatus IS NULL 
                       AND SRM.Id not in( Select ServicePOMasterId from trn.ServicePODetail where ServicePOMasterId IS NOT NULL)
						group by SRM.Id,SRM.RequisitionDate,SRM.RequisitionType,SRM.RequirmentType,E.UserName,SRM.ReasonWhyItIsNotPlanEarlier,
						ei.EmployeeName ,ei1.EmployeeName,SRM.CheckedByStatus,E.Id,EI.SystemId ,ei1.EmployeeName 
		                ,ei2.EmployeeName ,SRM.CheckedBy
					,ei3.EmployeeName 


				)X
					 Order By RequisitionDate DESC";
				}
				else if (ReqStatus == "HoldReject")
				{
					sql = @"Select 
	                        SRM.Id 
	                        ,REPLACE(CONVERT(CHAR(11), SRM.RequisitionDate, 106),' ','-') AS RequisitionDate,REPLACE(CONVERT(CHAR(11), SRM.RequisitionDate, 106),' ','-') AS RequisitionDate1  
	                        ,SRM.RequisitionType
	                        ,SRM.RequirmentType
	                        ,E.UserName EntityName
							,E.Id EntityId
							--,EI.SystemId CheckedBy
                            ,SRM.ReasonWhyItIsNotPlanEarlier Reason
                             ,ei.SystemId CheckedBy
                            ,ei.EmployeeName AS CheckedByName
                            ,SRM.CheckedByStatus
	                        ,ei1.EmployeeName AS AuthorizedBy
		                    ,ei2.EmployeeName As EmployeeName
							,ei3.EmployeeName AS ResponsiblePersonName
                             ,SUM(SRD.TotalServiceTranAmount) TotalServiceTranAmount
							 ,SUM(SRD.TotalServiceBooksCurrencyAmount) TotalServiceBooksCurrencyAmount
                            ,SRM.CheckedBy
                        	,SRM.CheckedHoldRejectReason ReasonHR
                          FROM [TRN].[ServiceRequsitionMaster] SRM 
                          Left Join (select ServiceRequisitionMasterID,sum(TotalServiceTranAmount) TotalServiceTranAmount, sum(TotalServiceBooksCurrencyAmount) TotalServiceBooksCurrencyAmount from [TRN].[ServiceRequsitionDetail] group by ServiceRequisitionMasterID)SRD On SRD.ServiceRequisitionMasterID = SRM.Id
                        Left Join org.Entity E on E.Id=SRM.EntityId
                        LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=SRM.CheckedBy
                        LEFT JOIN EmployeeInformation AS ei1 ON ei1.SystemId=SRM.AuthorizedBy
					   LEFT JOIN EmployeeInformation AS ei2 ON ei2.SystemId=SRM.QualityApprovalResponsiblePersonId
					   	 LEFT JOIN EmployeeInformation AS ei3 ON ei3.SystemId=SRM.NeedSpecialAppId
                       Where SRM.ReqEmpId='" + identity.EmployeeId + @"'
                    AND CheckedByStatus = 'Hold' OR CheckedByStatus = 'Reject'
                    AND SRM.AuthorizedByStatus IS NULL
						group by SRM.Id,SRM.RequisitionDate,SRM.RequisitionType,SRM.RequirmentType,E.UserName,SRM.ReasonWhyItIsNotPlanEarlier,
						ei.EmployeeName ,ei1.EmployeeName,SRM.CheckedByStatus,E.Id,EI.SystemId ,ei1.EmployeeName 
		                ,ei2.EmployeeName ,SRM.CheckedBy,SRM.CheckedHoldRejectReason
					    ,ei3.EmployeeName  Order By SRM.RequisitionDate DESC
                        ";



				}
				else
				{
					sql = @"Select 
	                        SRM.Id 
	                        ,REPLACE(CONVERT(CHAR(11), SRM.RequisitionDate, 106),' ','-') AS RequisitionDate ,REPLACE(CONVERT(CHAR(11), SRM.RequisitionDate, 106),' ','-') AS RequisitionDate1 
	                        ,SRM.RequisitionType
	                        ,SRM.RequirmentType
	                        ,E.UserName EntityName
							,E.Id EntityId
							--,EI.SystemId CheckedBy
                            ,SRM.ReasonWhyItIsNotPlanEarlier Reason
                            ,ei.SystemId CheckedBy
                            ,ei.EmployeeName AS CheckedByName
                            ,SRM.CheckedByStatus
	                        ,ei1.EmployeeName AS AuthorizedBy
		                    ,ei2.EmployeeName As EmployeeName
							,ei3.EmployeeName AS ResponsiblePersonName
                             ,SUM(SRD.TotalServiceTranAmount) TotalServiceTranAmount
							 ,SUM(SRD.TotalServiceBooksCurrencyAmount) TotalServiceBooksCurrencyAmount
                            ,SRM.CheckedBy
                          FROM [TRN].[ServiceRequsitionMaster] SRM 
                          Left Join (select ServiceRequisitionMasterID,sum(TotalServiceTranAmount) TotalServiceTranAmount, sum(TotalServiceBooksCurrencyAmount) TotalServiceBooksCurrencyAmount from [TRN].[ServiceRequsitionDetail] group by ServiceRequisitionMasterID)SRD On SRD.ServiceRequisitionMasterID = SRM.Id
                        Left Join org.Entity E on E.Id=SRM.EntityId
                        LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=SRM.CheckedBy
                        LEFT JOIN EmployeeInformation AS ei1 ON ei1.SystemId=SRM.AuthorizedBy
					   LEFT JOIN EmployeeInformation AS ei2 ON ei2.SystemId=SRM.QualityApprovalResponsiblePersonId
					   	 LEFT JOIN EmployeeInformation AS ei3 ON ei3.SystemId=SRM.NeedSpecialAppId
                        Where SRM.ReqEmpId='" + identity.EmployeeId + @"'
                        AND SRM.CheckedByStatus='Checked' 
                        AND SRM.AuthorizedByStatus= 'For Approval'
						group by SRM.Id,SRM.RequisitionDate,SRM.RequisitionType,SRM.RequirmentType,E.UserName,SRM.ReasonWhyItIsNotPlanEarlier,
						ei.EmployeeName ,ei1.EmployeeName,SRM.CheckedByStatus,E.Id,EI.SystemId ,ei1.EmployeeName 
		                ,ei2.EmployeeName ,SRM.CheckedBy
					    ,ei3.EmployeeName  Order By SRM.RequisitionDate DESC
                        ";


				}


				return _sqlRepository.GetDataCollection(sql);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}

	}
}
