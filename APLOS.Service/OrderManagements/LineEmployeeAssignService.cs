#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.OrderManagements;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.ViewModel.OrderManagements;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.OrderManagements
{
	public class LineEmployeeAssignService : Service<LineEmployeeAssign>, ILineEmployeeAssignService
	{
		#region Constructor

		private readonly IRepositoryAsync<LineProductionBooking> _lineProductionBookingRepository;
		private readonly IRepositoryAsync<LineOperationBooking> _lineOperationBookingRepository;
		private readonly IRepositoryAsync<EmployeeInformation> _employeeInformationRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly ISqlRepository _sqlRepository;

		public LineEmployeeAssignService(
			IRepositoryAsync<LineEmployeeAssign> lineEmployeeAssignRepository
			, IRepositoryAsync<LineProductionBooking> lineProductionBookingRepository
			, IRepositoryAsync<LineOperationBooking> lineOperationBookingRepository
			, IRepositoryAsync<EmployeeInformation> employeeInformationRepository
			, IPKGeneratorService pkGeneratorService
			, IUnitOfWork unitOfWork
			, ISqlRepository sqlRepository
			) : base(lineEmployeeAssignRepository, unitOfWork, pkGeneratorService)
		{
			_lineProductionBookingRepository = lineProductionBookingRepository;
			_lineOperationBookingRepository = lineOperationBookingRepository;
			_employeeInformationRepository = employeeInformationRepository;
			_unitOfWork = unitOfWork;
			_sqlRepository = sqlRepository;
		}

		#endregion Constructor

		private string GetPK()
		{
			return GetAutoNumber(nameof(LineEmployeeAssign), PKGeneratorEnum.Auto, null, DateTime.Now);
		}

		public IEnumerable<object> QueryGraph(string date, string salesOrderName, string line, string shift)
		{
			//   var _sql = @"SELECT LE.Id,LE.EmployeeId,LE.ProductionQty,E.EmployeeName, A.SalesOrder, A.ProductionDate, A.Line, A.Fabrication, A.Style, A.TotalQty
			//                           ,B.Id AS LineOperationBookingId, B.Operation, B.Rate FROM  [MST].[LineEmployeeAssign] LE
			//                           JOIN [MST].[LineOperationBooking] AS B ON LE.LineOperationBookingId=B.Id
			//                           JOIN [MST].[LineProductionBooking] AS A ON B.LineProductionBookingId=A.Id
			//JOIN  EmployeeInformation AS E ON LE.EmployeeId=E.SystemId
			//                           WHERE A.ProductionDate=CAST('" + date + "' AS DATE) AND B.Operation='" + operationname + "' AND A.Line='" + line + "'";

			var _sql = @"SELECT LE.Id,LE.EmployeeId,LE.OperatorQty,LE.PlantId
                        ,CASE
                            WHEN
                              E.EmployeeId is null
                                THEN
                                TE.EmployeeName
                            ELSE
                                 E.EmployeeName
                          END AS EmployeeName
                        ,CASE
                            WHEN
                              E.EmployeeId is null
                                THEN
                                TE.EmployeeCode
                            ELSE
                                 E.EmployeeCode
                          END AS EmployeeCode
                        ,B.TempEmployeeId, B.OperationName, A.ProductionDate, A.Line, A.Fabrication, A.Style, A.TargetQuantity,A.ProductionQty
                        ,B.Id AS LineOperationBookingId,B.Target,B.Rate
                        FROM [MST].[LineOperationBooking] AS B
                        LEFT JOIN [MST].[LineEmployeeAssign] LE ON LE.LineOperationBookingId=B.Id
                        JOIN [MST].[LineProductionBooking] AS A ON B.LineProductionBookingId=A.Id
                        LEFT JOIN EmployeeInformation AS E ON LE.EmployeeId=E.SystemId
                        LEFT JOIN EmployeeInformation AS TE ON B.TempEmployeeId=TE.SystemId
                        WHERE A.ProductionDate=CAST('" + date + "' AS DATE) AND A.SalesOrder='" + salesOrderName + "' AND A.Line='" + line + "'AND A.ProductionShift='" + shift + "' AND B.OperationType='Machine'";
			return _sqlRepository.GetDataCollection(_sql);
		}

		public void InsertOrUpdateGraph(string date, string line, IEnumerable<LineEmployeeAssign> entities, IEnumerable<LineEmployeeAssign> tempEntities)
		{
			var flag = false;
			try
			{
				if (entities == null)
					throw new CustomException("Please insert line employee assign");
				_unitOfWork.BeginTransaction();
				flag = true;
				var pk = GetMaxNumber(nameof(LineEmployeeAssign), PKGeneratorEnum.Yearly, null, DateTime.Now);
				var lOpIds = tempEntities.Select(r => r.LineOperationBookingId).Distinct();
				var lOpData = _lineOperationBookingRepository.Query(t => lOpIds.Contains(t.Id)).Select().ToList();
				foreach (var item in entities)
				{
					var existEmpList = GetExistEmployeeId(date, line, item.EmployeeId, item.Id);
					if (existEmpList.Count() > 0)
					{
						var dic = existEmpList.First();
						var existLine = (Dictionary<string, object>)dic;
						throw new CustomException(_employeeInformationRepository.Find(item.EmployeeId).EmployeeName + " is already assigned in line " + existLine["Line"] + " and in operation " + existLine["OperationName"]);
					}
					if (string.IsNullOrEmpty(item.Id))
					{
						pk.MaxNumber++;
						item.Id = pk.MaxNumber.ToString();
						InsertGraph(item);
					}
					else
					{
						UpdateGraph(item);
					}
					//var t = tempEntities.First(r => r.LineOperationBookingId == item.LineOperationBookingId);
					//LineProductionBooking ln = new LineProductionBooking();
					//ln = _lineOperationBookingRepository.Query(r => r.LineOperationBookingId == t.LineOperationBookingId).Select().FirstOrDefault();
					//ln.TempEmployeeId = item.EmployeeId;
					//_lineOperationBookingRepository.Update(ln);
				}
				foreach (var item in tempEntities)
				{
					foreach (var item2 in lOpData)
					{
						if (item.LineOperationBookingId == item2.Id)
						{
							item2.TempEmployeeId = item.EmployeeId;
							item2.ModelState = ModelState.Modified;
							_lineOperationBookingRepository.Update(item2);
						}
					}
				}
				_unitOfWork.SaveChanges();
				flag = false;
				_unitOfWork.Commit();
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
				null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
			}
			finally
			{
				if (flag)
					_unitOfWork.Rollback();
			}
		}

		public void UpdateGraph(IEnumerable<LineProductionOperationBookingViewModel> entities)
		{
			var flag = false;
			try
			{
				if (entities.Count() == 0 && entities.IsNull()) return;
				_unitOfWork.BeginTransaction();
				flag = true;

				var opIds = new string[] { };
				opIds = entities.Select(t => t.LineOperationBookingId).Distinct().ToArray();
				CheckTotalOperatorQty(entities.Select(t => t.ProductionQty).FirstOrDefault(), entities, opIds);
				var changeEmpData = base.Query(t => opIds.Contains(t.LineOperationBookingId)).Select().ToList();
				foreach (var item in entities)
				{
					var lineEmp = changeEmpData.FirstOrDefault(t => t.Id == item.Id);
					if (lineEmp.IsNotNull())
					{
						lineEmp.OperatorQty = item.OperatorQty;
						base.UpdateGraph(lineEmp);
					}
				}
				_unitOfWork.SaveChanges();
				flag = false;
				_unitOfWork.Commit();
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
				null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
			}
			finally
			{
				if (flag)
					_unitOfWork.Rollback();
			}
		}

		public void DeleteGraph(string id)
		{
			var flag = false;
			try
			{
				_unitOfWork.BeginTransaction();
				flag = true;
				base.DeleteGraph(id);
				_unitOfWork.SaveChanges();
				flag = false;
				_unitOfWork.Commit();
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
			}
			finally
			{
				if (flag)
					_unitOfWork.Rollback();
			}
		}

		public IEnumerable<object> GetLineEmployeeDetail(string lineOperationBookingId)
		{
			try
			{
				var _sql = @"SELECT LE.Id, LE.EmployeeId, LE.LineOperationBookingId, LE.OperatorQty, A.SalesOrder, A.ProductionDate
	                        , A.Line, A.SalesOrder, A.Fabrication, A.Style, A.TotalQty
	                        , B.Id AS LineOperationBookingId, B.Operation, B.Rate
                            FROM [MST].[LineEmployeeAssign] as LE
                            JOIN [MST].[LineOperationBooking] AS B ON LE.LineOperationBookingId=B.Id
                            JOIN [MST].[LineProductionBooking] AS A ON B.LineProductionBookingId=A.Id
                            where le.LineOperationBookingId='" + lineOperationBookingId + "'";
				return _sqlRepository.GetDataCollection(_sql, null);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public IEnumerable<object> GetLineCbo(string date, string plantId)
		{
			try
			{
				var _sql = @"SELECT ROW_NUMBER()OVER (ORDER BY Line) AS [Value], Line AS [Text]
                            FROM (
                            SELECT DISTINCT A.Line FROM [MST].[LineProductionBooking] AS A
                            WHERE A.ProductionDate=CAST('" + date + "' AS DATE)) AS T";
				return _sqlRepository.GetDataCollection(_sql, null);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public IEnumerable<object> GetOperationCbo(string date, string linetext, string plantId)
		{
			try
			{
				var _sql = @"SELECT ROW_NUMBER()OVER (ORDER BY Operation) AS [Value], Operation AS [Text]
                            FROM (
                            SELECT DISTINCT A.Operation FROM [MST].[LineOperationBooking] AS A
                            JOIN [MST].[LineProductionBooking] AS B ON A.LineProductionBookingId=B.Id
                            WHERE B.ProductionDate=CAST('" + date + @"' AS DATE) AND B.Line='" + linetext + @"' AND B.PlantId='" + plantId + "' ) AS T";
				return _sqlRepository.GetDataCollection(_sql, null);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public IEnumerable<object> GetSalesOrderCbo(string date, string linetext, string plantId)
		{
			try
			{
				var _sql = @"SELECT ROW_NUMBER()OVER (ORDER BY SalesOrder) AS [Value], SalesOrder AS [Text]  FROM (
                             SELECT Distinct SalesOrder FROM MST.LineProductionBooking
                             WHERE ProductionDate=CAST('" + date + "' AS DATE) AND Line='" + linetext + "') AS T";
				return _sqlRepository.GetDataCollection(_sql, null);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public IEnumerable<object> GetShiftCbo(string date, string linetext, string salesorder, string plantId)
		{
			try
			{
				var _sql = @"SELECT ROW_NUMBER()OVER (ORDER BY ProductionShift) AS [Value], ProductionShift AS [Text]
                            FROM (SELECT ProductionShift FROM MST.LineProductionBooking
	                        WHERE ProductionDate=CAST('" + date + "' AS DATE) AND SalesOrder='" + salesorder + "' AND Line='" + linetext + "') AS T";
				return _sqlRepository.GetDataCollection(_sql, null);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public IEnumerable<object> GetSalesOrder(string date, string lineName, string operationName, string plantId)
		{
			try
			{
				var _sql = @"SELECT DISTINCT A.Id,B.SalesOrder, B.ProductionDate, B.Line, B.Fabrication,B.Style, B.TotalQty
							, A.Id AS LineOperationBookingId, A.Operation, A.Rate FROM [MST].[LineOperationBooking] AS A
                            JOIN [MST].[LineProductionBooking] AS B ON A.LineProductionBookingId=B.Id
                            WHERE B.ProductionDate=CAST('" + date + "' AS DATE) AND B.Line='" + lineName + "' AND A.Operation='" + operationName + "'";
				return _sqlRepository.GetDataCollection(_sql, null);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public IEnumerable<object> GetProduction(string date, string lineName, string salesOrderName, string plantId)
		{
			try
			{
				var _sql = @"SELECT DISTINCT A.Id,A.Operation, B.ProductionDate, B.Line, B.Fabrication,B.Style, B.TotalQty
							, A.Id AS LineOperationBookingId, A.Operation, A.Rate FROM [MST].[LineOperationBooking] AS A
                            JOIN [MST].[LineProductionBooking] AS B ON A.LineProductionBookingId=B.Id
                            WHERE B.ProductionDate=CAST('" + date + "' AS DATE) AND B.Line='" + lineName + "' AND B.SalesOrder='" + salesOrderName + "'";
				return _sqlRepository.GetDataCollection(_sql, null);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public IEnumerable<object> GetExistEmployeeId(string date, string linetext, string employeeId, string id)
		{
			try
			{
				var stext = string.Empty;
				if (!string.IsNullOrEmpty(id))
				{
					stext = "and LE.Id <> '" + id + "'";
				}
				var _sql = @"SELECT LE.Id,LE.EmployeeId,LE.OperatorQty,E.EmployeeName, A.SalesOrder, A.ProductionDate, A.Line, A.Fabrication, A.Style,A.ProductionShift,A.MaterialCode +' - '+ A.MaterialDesc Material, LE.OperatorQty
                                    ,B.Id AS LineOperationBookingId, B.OperationName, B.Rate FROM  [MST].[LineEmployeeAssign] LE
                                    JOIN [MST].[LineOperationBooking] AS B ON LE.LineOperationBookingId=B.Id
                                    JOIN [MST].[LineProductionBooking] AS A ON B.LineProductionBookingId=A.Id
									JOIN  EmployeeInformation AS E ON LE.EmployeeId=E.SystemId
						where A.ProductionDate=CAST('" + date + "' AS DATE) AND A.Line <>'" + linetext + "' AND LE.EmployeeId='" + employeeId + "' " + stext + "";
				return _sqlRepository.GetDataCollection(_sql, null);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		#region Entry from excel

		public void InsertLineProductionOperation(List<LineProductionOperationBookingViewModel> viewModel, DateTime toDate)
		{
			var flag = false;
			try
			{
				_unitOfWork.BeginTransaction();
				flag = true;
				var linePrdData = viewModel.GroupBy(t => new { t.ProductionDate, t.Line, t.ProductionShift, t.Fabrication, t.Style, t.SalesOrder }).Select(x => x.First());
				var lineOperationData = viewModel.GroupBy(t => new { t.ProductionDate, t.Line, t.ProductionShift, t.Fabrication, t.Style, t.SalesOrder, t.OperationName }).Select(x => x.First());

				//var allLine = linePrdData.Select(t => t.Line).Distinct().ToArray();
				//var allSO = linePrdData.Select(t => t.SalesOrder).Distinct().ToArray();

				var pk1 = GetMaxNumber(nameof(LineProductionBooking), PKGeneratorEnum.Auto, null, DateTime.Now);
				var pk2 = GetMaxNumber(nameof(LineOperationBooking), PKGeneratorEnum.Auto, null, DateTime.Now);

				var sqlBuilder = new System.Text.StringBuilder();
				var dateTime = DateTime.Now;
				var sql = @"SELECT A.Id, A.LineProductionBookingId, A.MachineType, A.OperationType, A.OperationName, A.[Target], A.Rate, A.TempEmployeeId
	                               , B.ProductionDate, B.Line, B.SalesOrder, B.ProductionShift
                    FROM [MST].[LineOperationBooking] AS A
                    JOIN [MST].[LineProductionBooking] AS B ON A.LineProductionBookingId=B.Id
                    WHERE A.TempEmployeeId<>'' AND B.ProductionDate>=CAST('" + toDate.AddDays(-2) + @"' AS DATE) AND B.ProductionDate<=CAST('" + toDate.AddDays(-1) + @"' AS DATE)
                    AND B.Line IN(" + ReturnStringArray(linePrdData.Select(t => t.Line).Distinct().ToArray()) + @")
                    AND B.SalesOrder IN(" + ReturnStringArray(linePrdData.Select(t => t.SalesOrder).Distinct().ToArray()) + @")
                    AND B.ProductionShift IN(" + ReturnStringArray(linePrdData.Select(t => t.ProductionShift).Distinct().ToArray()) + ") ORDER BY B.ProductionDate DESC";
				var preViousDataList = _sqlRepository.GetModelCollection<LineProductionOperationBookingViewModel>(sql);
				foreach (var item in linePrdData)
				{
					pk1.MaxNumber++;
					//sqlBuilder.Append(@"INSERT INTO [MST].[LineProductionBooking] VALUES('" + pk1.MaxNumber.ToString() + "', '" + item.CompanyGroupId + "', '" + item.CompanyId + "', '" + item.PlantName + "', '" + item.ProductionDate + "', '" + item.ProductionShift + "', '" + item.Line + "', '" + item.SalesOrder + "', '" + item.Fabrication + "', '" + item.Style + "', " + item.ProductionQty + ", 'TS', '" + dateTime + "', 'TS', NULL, NULL, NULL); ");
					sqlBuilder.Append(@"INSERT INTO [MST].[LineProductionBooking] VALUES('" + pk1.MaxNumber+ "',NULL, NULL,	'" + item.PlantName + @"'
                    , '" + item.ProductionDate + "','" + item.Line + "','" + item.ProductionShift + "', '" + item.SalesOrder + "', '" + item.Fabrication + "','" + item.Style + "','" + item.ProductionQty + @"'
                    , '" + item.CustomerCode + "','" + item.CustomerName + "','" + item.TotalManPower + "','" + item.PlanRunMC + "','" + item.ActualRunMC + @"'
                    , '" + item.ExtraMC + "','" + item.TrimCheckPress + "','" + item.SewingSMV + "','" + item.TotalSMV + "','" + item.MCMINAvailable + "','" + item.NonMCMINAvailable + @"'
                    , '" + item.TotalMINAvailable + "','" + item.ActualMINWorked + "','" + item.MCSAMProd + "','" + item.TotalSAMProd + "','" + item.MCEfficiency + @"'
                    , '" + item.OrderQty + "','" + item.TargetQuantity + "','" + item.MaterialCode + "','" + item.MaterialDesc + "','TS','" + dateTime + "','TS', NULL, NULL, NULL);");

					var operationList = lineOperationData.Where(t => t.ProductionDate == item.ProductionDate && t.Line == item.Line && t.Fabrication == item.Fabrication && t.Style == item.Style && t.SalesOrder == item.SalesOrder);

					foreach (var op in operationList)
					{
						var firstDayData = preViousDataList.FirstOrDefault(t => t.Line == item.Line && t.ProductionShift == item.ProductionShift && t.OperationName == op.OperationName);
						var secondDayData = preViousDataList.FirstOrDefault(t => t.Line == item.Line && t.ProductionShift == item.ProductionShift && t.OperationName == op.OperationName && t.Id != firstDayData.Id);

						var tempEmpId = "NULL";
						if (firstDayData != null && !string.IsNullOrEmpty(firstDayData.TempEmployeeId))
							tempEmpId = "'" + firstDayData.TempEmployeeId + "'";
						else if (firstDayData != null && !string.IsNullOrEmpty(secondDayData.TempEmployeeId))
							tempEmpId = "'" + secondDayData.TempEmployeeId + "'";

						pk2.MaxNumber++;
						//sqlBuilder.Append(@"INSERT INTO [MST].[LineOperationBooking] VALUES('" + pk2.MaxNumber.ToString() + "','" + pk1.MaxNumber.ToString() + "', '" + op.OperationName + "', '" + op.Rate + "', 'TS', '" + dateTime + "', 'TS', NULL, NULL, NULL);");
						sqlBuilder.Append(@"INSERT INTO [MST].[LineOperationBooking] VALUES('" + pk2.MaxNumber+ "','" + pk1.MaxNumber+ @"'
                            , '" + op.MachineType + "', '" + op.OperationType + "', '" + op.OperationName + "', " + op.Target + ", " + op.Rate + ", " + tempEmpId + ", 'TS', '" + dateTime + "', 'TS', NULL, NULL, NULL); ");
					}
				}
				_sqlRepository.ExecuteSqlCommand(sqlBuilder.ToString());
				_unitOfWork.SaveChanges();
				flag = false;
				_unitOfWork.Commit();
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
			}
			finally
			{
				if (flag)
					_unitOfWork.Rollback();
			}
		}

		#endregion Entry from excel

		#region report

		public IWorkbook GetEmployeeAssignReport(string companyGroupId, string companyId, string plantName, string reportName, string date, string line)
		{
			try
			{
				var excelEngine = new ExcelEngine();
				var report = new ReportUtility();
				var workbook = report.GetWorkbook(ref excelEngine, 1);
				var sheet1 = workbook.Worksheets[0];
				CreateSheetEmployeeAdvance(ref sheet1, report, reportName, "Sheet1", companyGroupId, companyId, plantName, date, line);
				workbook.Version = ExcelVersion.Excel2013;
				return workbook;
			}
			catch (Exception)
			{
				throw;
			}
		}

		private void CreateSheetEmployeeAdvance(ref IWorksheet sheet, ReportUtility report, string sheetHeader, string sheetName, string companyGroupId, string companyId, string plantName, string date, string line)
		{
			#region List data

			var dtGeneralVoucher = LineEmpData(date, line);

			if (dtGeneralVoucher.Count() == 0)
			{
				throw (new Exception("No Data Found !!!"));
			}

			var _col = 1;
			var _row = 6;
			var shet2EndxlsCol = _col;
			report.SetMasterHeaderText(ref sheet, _row, _col, "Production Date");
			sheet[report.GetColumnNameForXls(_col) + _row + ":" + report.GetColumnNameForXls(_col + 1) + _row].Merge();
			report.SetText(ref sheet, _row, _col + 2, date); _row++;
			//sheet[report.GetColumnNameForXls(_col3) + _row + ":" + report.GetColumnNameForXls(_col3 + 2) + _row].Merge();

			report.SetMasterHeaderText(ref sheet, _row, _col, "Line");
			sheet[report.GetColumnNameForXls(_col) + _row + ":" + report.GetColumnNameForXls(_col + 1) + _row].Merge();
			report.SetText(ref sheet, _row, _col + 2, line); _row++;
			//sheet[report.GetColumnNameForXls(_col3) + _row + ":" + report.GetColumnNameForXls(_col3 + 2) + _row].Merge();

			var _colData = 1;
			var _rowData = _row;
			var Row_Total_Start = _rowData + 1;
			report.SetHeaderText(ref sheet, _rowData, _colData, "EmployeeName", 28); _colData++;
			report.SetHeaderText(ref sheet, _rowData, _colData, "Line", 25); _colData++;
			report.SetHeaderText(ref sheet, _rowData, _colData, "SalesOrder", 28); _colData++;
			report.SetHeaderText(ref sheet, _rowData, _colData, "Shift", 20); _colData++;
			report.SetHeaderText(ref sheet, _rowData, _colData, "Fabrication", 32); _colData++;
			report.SetHeaderText(ref sheet, _rowData, _colData, "Style", 28); _colData++;
			report.SetHeaderText(ref sheet, _rowData, _colData, "Operation", 32); _colData++;
			report.SetHeaderText(ref sheet, _rowData, _colData, "Operator Qty", 20); _colData++;
			report.SetHeaderText(ref sheet, _rowData, _colData, "Rate", 20); _colData++;
			report.SetHeaderText(ref sheet, _rowData, _colData, "Amount", 20); _colData++;
			foreach (var item in dtGeneralVoucher)
			{
				var dic = (Dictionary<string, object>)item;
				_rowData++;
				report.SetText(ref sheet, _rowData, 1, dic["Employee"].ToString());
				report.SetText(ref sheet, _rowData, 2, dic["Line"].ToString());
				report.SetText(ref sheet, _rowData, 3, dic["SalesOrder"].ToString());
				report.SetText(ref sheet, _rowData, 4, dic["ProductionShift"].ToString());
				report.SetText(ref sheet, _rowData, 5, dic["Fabrication"].ToString());
				report.SetText(ref sheet, _rowData, 6, dic["Style"].ToString());
				report.SetText(ref sheet, _rowData, 7, dic["OperationName"].ToString());
				report.SetText(ref sheet, _rowData, 8, dic["OperatorQty"].ToString());
				report.SetText(ref sheet, _rowData, 9, dic["Rate"].ToString());
				report.SetFormula(ref sheet, _rowData, 10, "=" + report.GetColumnNameForXls(8) + (_rowData) + "*" + report.GetColumnNameForXls(9) + (_rowData), true);
			}//main

			#region sumCalc

			_rowData++;
			//var sumdrcrCol = 2;
			sheet.Range[report.GetColumnNameForXls(1) + _rowData + ":" + report.GetColumnNameForXls(7) + _rowData].Merge();
			report.SetText(ref sheet, _rowData, 1, "Total", true);
			var sumcCol = 7;
			sumcCol++;
			sheet.Range[_rowData, sumcCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumcCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumcCol) + (_rowData - 1) + ")";
			sheet.Range[_rowData, sumcCol].NumberFormat = report.NumberFormatDecimalTwo();
			sheet.Range[_rowData, sumcCol].CellStyle.Font.Bold = true;
			sheet.Range[_rowData, sumcCol].BorderAround(ExcelLineStyle.Hair);
			sumcCol++;
			sheet.Range[_rowData, sumcCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumcCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumcCol) + (_rowData - 1) + ")";
			sheet.Range[_rowData, sumcCol].NumberFormat = report.NumberFormatDecimalTwo();
			sheet.Range[_rowData, sumcCol].CellStyle.Font.Bold = true;
			sheet.Range[_rowData, sumcCol].BorderAround(ExcelLineStyle.Hair);

			#endregion sumCalc

			//#region InWord

			//vAmount = vAmount / 2;
			//var _amountValue = report.InWord(vAmount, _CurrencyId);//for Trn Currency

			//var _amount = report.InWord(_Total_Amount, plCurrencyId);//for Para Currency

			//_rowL += 1;

			//report.SetText(ref sheet, _rowL, _col, "In Word:", true);
			//if (_amountValue == _amount)
			//{
			//    _col = 2;
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].Text = _amountValue;
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].CellStyle.Font.Bold = true;
			//}
			//else
			//{
			//    _col = 2;
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].Text = _amountValue;
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].CellStyle.Font.Bold = true;

			//    _rowL += 1;
			//    _col = 2;
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].Text = _amount;
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].CellStyle.Font.Bold = true;
			//}

			//#endregion InWord

			//_rowL = _rowL + 6;

			//#region Signature

			//sheet.Range[_rowL, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
			//sheet.Range[_rowL, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
			//sheet.Range[_rowL, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
			//sheet.Range[_rowL, 7].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
			//sheet.Range[_rowL, 9].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

			//report.SetText(ref sheet, _rowL, 1, "Received By", true); _col += 1;
			//report.SetText(ref sheet, _rowL, 3, "Prepared By", true); _col += 1;
			//report.SetText(ref sheet, _rowL, 5, "Checked By", true); _col += 1;
			//report.SetText(ref sheet, _rowL, 7, "HOD (Finance)", true); _col += 1;
			//report.SetText(ref sheet, _rowL, 9, "CEO/Director", true); _col += 1;

			//#endregion Signature

			sheet.Name = sheetName;
			sheet.UsedRange.WrapText = true;
			sheet.UsedRange.CellStyle.Font.Size = 8;
			report.CompanyPlantHeader(ref sheet, 4, sheetHeader, companyId, plantName, null);
			report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);

			#endregion List data
		}

		public IEnumerable<object> LineEmpData(string date, string line)
		{
			var _sql = @"SELECT LE.Id,LE.EmployeeId,LE.OperatorQty,E.EmployeeCode +' - '+ E.EmployeeName Employee, A.SalesOrder, A.ProductionDate, A.Line, A.Fabrication, A.Style,A.ProductionShift,A.MaterialCode +' - '+ A.MaterialDesc Material, LE.OperatorQty OperatorQty,LE.OperatorQty*B.Rate Amount
                                    ,B.Id AS LineOperationBookingId, B.OperationName, B.Rate FROM  [MST].[LineEmployeeAssign] LE
                                    JOIN [MST].[LineOperationBooking] AS B ON LE.LineOperationBookingId=B.Id
                                    JOIN [MST].[LineProductionBooking] AS A ON B.LineProductionBookingId=A.Id
									JOIN  EmployeeInformation AS E ON LE.EmployeeId=E.SystemId
                                    WHERE A.ProductionDate=CAST('" + date + "' AS DATE)";
			return _sqlRepository.GetDataCollection(_sql);
		}

		public IWorkbook GetEmployeeReport(string companyGroupId, string companyId, string plantName, string reportName, string fromdate, string todate)
		{
			try
			{
				var excelEngine = new ExcelEngine();
				var report = new ReportUtility();
				var workbook = report.GetWorkbook(ref excelEngine, 1);
				var sheet1 = workbook.Worksheets[0];
				CreateSheetEmployeeInfo(ref sheet1, report, reportName, "Sheet1", companyGroupId, companyId, plantName, fromdate, todate);
				workbook.Version = ExcelVersion.Excel2013;
				return workbook;
			}
			catch (Exception)
			{
				throw;
			}
		}

		private void CreateSheetEmployeeInfo(ref IWorksheet sheet, ReportUtility report, string sheetHeader, string sheetName, string companyGroupId, string companyId, string plantName, string fromdate, string todate)
		{
			#region List data
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			var dtMainQuery = GetReportEmpData(fromdate, todate);
			var totalEmpRow = dtMainQuery.Select(r => new { r.EmployeeId, r.EmployeeName, r.BasicSalary }).ToList().Distinct();
			if (dtMainQuery.Count() == 0)
			{
				throw (new Exception("No Data Found !!!"));
			}


			var _dateCol = 1;
			var _dateRow = 6;

			report.SetMasterHeaderText(ref sheet, _dateRow, _dateCol, "From Date");
			sheet[report.GetColumnNameForXls(_dateCol) + _dateRow + ":" + report.GetColumnNameForXls(_dateCol) + _dateRow].Merge();
			report.SetText(ref sheet, _dateRow, _dateCol + 1, fromdate); //_dateRow++;
																		 //sheet[report.GetColumnNameForXls(_dateCol) + _dateRow + ":" + report.GetColumnNameForXls(_col3 + 2) + _dateRow].Merge();

			var _dateColR = 3;

			report.SetMasterHeaderText(ref sheet, _dateRow, _dateColR, "To Date");
			sheet[report.GetColumnNameForXls(_dateColR) + _dateRow + ":" + report.GetColumnNameForXls(_dateColR) + _dateRow].Merge();
			report.SetText(ref sheet, _dateRow, _dateColR + 1, todate); //_dateRow++;
																		//sheet[report.GetColumnNameForXls(_dateColR) + _dateRow + ":" + report.GetColumnNameForXls(_dateColR + 1) + _dateRow].Merge();


			var _colData = 1;
			var _rowData = 8;
			var Row_Total_Start = _rowData + 1;
			report.SetHeaderText(ref sheet, _rowData, _colData, "OPERATOR", 28); _colData++;
			var dateList = new List<LineProductionOperationBookingViewModel>();
			for (var dt = Convert.ToDateTime(fromdate); dt <= Convert.ToDateTime(todate); dt = dt.AddDays(1))
			{
				LineProductionOperationBookingViewModel l = new LineProductionOperationBookingViewModel();
				l.ProductionDate = dt;
				l.DefaultWeekOff = dtMainQuery.Where(r => r.PlantId == identity.PlantId).Select(r => r.DefaultWeekOff).FirstOrDefault();
				dateList.Add(l);
			}
			//var dateList = dtMainQuery.Select(r => new {r.ProductionDate,r.DefaultWeekOff }).Distinct().ToList();
			var weekCount = 0;
			foreach (var item in dateList)
			{
				report.SetHeaderText(ref sheet, _rowData, _colData, item.ProductionDate.ToShortDateString(), 10); _colData++;
				if (item.ProductionDate.DayOfWeek.ToString() == item.DefaultWeekOff)
				{
					weekCount++;
					report.SetHeaderText(ref sheet, _rowData, _colData, "Pcs RATE WAGES Earned", 10, ExcelKnownColors.Light_blue); _colData++;
					report.SetHeaderText(ref sheet, _rowData, _colData, "BASIC RATE MASTER", 10, ExcelKnownColors.Light_blue); _colData++;
					report.SetHeaderText(ref sheet, _rowData, _colData, "Pcs Rate Days", 10, ExcelKnownColors.Light_blue); _colData++;
					report.SetHeaderText(ref sheet, _rowData, _colData, "Minimum wage", 10, ExcelKnownColors.Light_blue); _colData++;
					report.SetHeaderText(ref sheet, _rowData, _colData, "INCENTIVE", 10, ExcelKnownColors.Light_blue); _colData++;
				}
			}
			if (weekCount < 1)
			{
				report.SetHeaderText(ref sheet, _rowData, _colData, "Pcs RATE WAGES Earned", 10, ExcelKnownColors.Light_blue); _colData++;
				report.SetHeaderText(ref sheet, _rowData, _colData, "BASIC RATE MASTER", 10, ExcelKnownColors.Light_blue); _colData++;
				report.SetHeaderText(ref sheet, _rowData, _colData, "Pcs Rate Days", 10, ExcelKnownColors.Light_blue); _colData++;
				report.SetHeaderText(ref sheet, _rowData, _colData, "Minimum wage", 10, ExcelKnownColors.Light_blue); _colData++;
				report.SetHeaderText(ref sheet, _rowData, _colData, "INCENTIVE", 10, ExcelKnownColors.Light_blue); _colData++;
			}
			foreach (var item in totalEmpRow)
			{
				_rowData++;
				var _colSetData = 1;
				report.SetText(ref sheet, _rowData, 1, item.EmployeeName); _colSetData++;
				//var empDateList = dtMainQuery.Where(r => r.EmployeeId == item.EmployeeId).Select(r => new { r.ProductionDate,r.DefaultWeekOff }).ToList();
				var dateColstart = _colSetData;
				var dateColend = 0;
				foreach (var ed in dateList)
				{
					var dateProduction = dtMainQuery.Where(r => r.EmployeeId == item.EmployeeId && r.ProductionDate == ed.ProductionDate).Select(r => r.Amount).FirstOrDefault();
					report.SetText(ref sheet, _rowData, _colSetData, Convert.ToInt32(dateProduction)); _colSetData++;
					dateColend = _colSetData;
					if (ed.ProductionDate.DayOfWeek.ToString() == ed.DefaultWeekOff)
					{
						report.SetFormula(ref sheet, _rowData, _colSetData, "=ROUNDUP(SUM(" + report.GetColumnNameForXls(dateColstart) + (_rowData) + ":" + report.GetColumnNameForXls(dateColend - 1) + (_rowData) + "),0)", true); _colSetData++;
						var pcsRateV = _colSetData - 1;
						report.SetText(ref sheet, _rowData, _colSetData, Convert.ToInt32(item.BasicSalary)); _colSetData++;
						var salaryV = _colSetData - 1;
						report.SetFormula(ref sheet, _rowData, _colSetData, "=COUNTIF(" + report.GetColumnNameForXls(dateColstart) + (_rowData) + ":" + report.GetColumnNameForXls(dateColend - 1) + (_rowData) + ",\"<>0\")", true); _colSetData++;
						var pcsV = _colSetData - 1;
						report.SetFormula(ref sheet, _rowData, _colSetData, "=ROUNDUP(((" + report.GetColumnNameForXls(salaryV) + (_rowData) + "/26)*" + report.GetColumnNameForXls(pcsV) + (_rowData) + "),0)", true); _colSetData++;
						var wageV = _colSetData - 1;
						report.SetFormula(ref sheet, _rowData, _colSetData, "=IF((" + report.GetColumnNameForXls(pcsRateV) + (_rowData) + "-" + report.GetColumnNameForXls(wageV) + (_rowData) + ")>0(" + report.GetColumnNameForXls(pcsRateV) + (_rowData) + "-" + report.GetColumnNameForXls(wageV) + (_rowData) + "),0)", true); _colSetData++;
						dateColstart = _colSetData;
					}
				}
				if (weekCount < 1)
				{
					report.SetFormula(ref sheet, _rowData, _colSetData, "=ROUNDUP(SUM(" + report.GetColumnNameForXls(dateColstart) + (_rowData) + ":" + report.GetColumnNameForXls(dateColend - 1) + (_rowData) + "),0)", true); _colSetData++;
					var pcsRateV = _colSetData - 1;
					report.SetText(ref sheet, _rowData, _colSetData, 100); _colSetData++;
					var salaryV = _colSetData - 1;
					report.SetFormula(ref sheet, _rowData, _colSetData, "=COUNTIF(" + report.GetColumnNameForXls(dateColstart) + (_rowData) + ":" + report.GetColumnNameForXls(dateColend - 1) + (_rowData) + ",\"<>0\")", true); _colSetData++;
					var pcsV = _colSetData - 1;
					report.SetFormula(ref sheet, _rowData, _colSetData, "=ROUNDUP(((" + report.GetColumnNameForXls(salaryV) + (_rowData) + "/26)*" + report.GetColumnNameForXls(pcsV) + (_rowData) + "),0)", true); _colSetData++;
					var wageV = _colSetData - 1;
					report.SetFormula(ref sheet, _rowData, _colSetData, "=IF((" + report.GetColumnNameForXls(pcsRateV) + (_rowData) + "-" + report.GetColumnNameForXls(wageV) + (_rowData) + ")>0(" + report.GetColumnNameForXls(pcsRateV) + (_rowData) + "-" + report.GetColumnNameForXls(wageV) + (_rowData) + "),0)", true); _colSetData++;
				}
			}//main

			//#region sumCalc

			//_rowData++;
			//var sumdrcrCol = 2;
			//sheet.Range[report.GetColumnNameForXls(1) + (_rowData) + ":" + report.GetColumnNameForXls(7) + _rowData].Merge();
			//report.SetText(ref sheet, _rowData, 1, "Total", true);
			//var sumcCol = 7;
			//sumcCol++;
			//sheet.Range[_rowData, sumcCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumcCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumcCol) + (_rowData - 1) + ")";
			//sheet.Range[_rowData, sumcCol].NumberFormat = report.NumberFormatDecimalTwo();
			//sheet.Range[_rowData, sumcCol].CellStyle.Font.Bold = true;
			//sheet.Range[_rowData, sumcCol].BorderAround(ExcelLineStyle.Hair);
			//sumcCol++;
			//sheet.Range[_rowData, sumcCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumcCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumcCol) + (_rowData - 1) + ")";
			//sheet.Range[_rowData, sumcCol].NumberFormat = report.NumberFormatDecimalTwo();
			//sheet.Range[_rowData, sumcCol].CellStyle.Font.Bold = true;
			//sheet.Range[_rowData, sumcCol].BorderAround(ExcelLineStyle.Hair);
			//#endregion sumCalc

			//#region InWord

			//vAmount = vAmount / 2;
			//var _amountValue = report.InWord(vAmount, _CurrencyId);//for Trn Currency

			//var _amount = report.InWord(_Total_Amount, plCurrencyId);//for Para Currency

			//_rowL += 1;

			//report.SetText(ref sheet, _rowL, _col, "In Word:", true);
			//if (_amountValue == _amount)
			//{
			//    _col = 2;
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].Text = _amountValue;
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].CellStyle.Font.Bold = true;
			//}
			//else
			//{
			//    _col = 2;
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].Text = _amountValue;
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].CellStyle.Font.Bold = true;

			//    _rowL += 1;
			//    _col = 2;
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].Text = _amount;
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
			//    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].CellStyle.Font.Bold = true;
			//}

			//#endregion InWord

			//_rowL = _rowL + 6;

			//#region Signature

			//sheet.Range[_rowL, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
			//sheet.Range[_rowL, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
			//sheet.Range[_rowL, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
			//sheet.Range[_rowL, 7].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
			//sheet.Range[_rowL, 9].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

			//report.SetText(ref sheet, _rowL, 1, "Received By", true); _col += 1;
			//report.SetText(ref sheet, _rowL, 3, "Prepared By", true); _col += 1;
			//report.SetText(ref sheet, _rowL, 5, "Checked By", true); _col += 1;
			//report.SetText(ref sheet, _rowL, 7, "HOD (Finance)", true); _col += 1;
			//report.SetText(ref sheet, _rowL, 9, "CEO/Director", true); _col += 1;

			//#endregion Signature

			sheet.Name = sheetName;
			sheet.UsedRange.WrapText = true;
			sheet.UsedRange.CellStyle.Font.Size = 8;
			report.CompanyPlantHeader(ref sheet, 4, sheetHeader, companyId, plantName, null);
			report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
			sheet.IsDisplayZeros = true;
			#endregion ----Minimized only for Work Purpose
		}

		public IEnumerable<LineProductionOperationBookingViewModel> GetReportEmpData(string fromDate, string toDate)
		{
			var _sql = @"SELECT distinct LE.EmployeeId,LE.PlantId,E.EmployeeName,A.ProductionDate,LE.OperatorQty,LE.OperatorQty*B.Rate Amount,PH.DefaultWeekOff,SC.DisbusmentAmount BasicSalary FROM  [MST].[LineEmployeeAssign] LE
                                    JOIN [MST].[LineOperationBooking] AS B ON LE.LineOperationBookingId=B.Id
                                    JOIN [MST].[LineProductionBooking] AS A ON B.LineProductionBookingId=A.Id
									JOIN  EmployeeInformation AS E ON LE.EmployeeId=E.SystemId
									LEFT join [dbo].[PlantWiseHRMSSetting] PH on LE.PlantId=PH.PlantID
									LEFT join SalaryProcChild SC ON LE.EmployeeId=SC.EmpInfoSystemID
									LEFT JOIN (select * from SalaryProcMaster where MonthNo=DATEPART(MONTH,'" + toDate + @"')) SM ON sc.SlrProcMstSystemID=SM.SystemID
									LEFT JOIN SalaryHead SH ON SC.SalaryHeadID=SH.SalaryHeadID AND HeadCategory='Basic'
                                    WHERE A.ProductionDate Between CAST('" + fromDate + "' AS DATE) AND CAST('" + toDate + "' AS DATE)";
			return _sqlRepository.GetModelCollection<LineProductionOperationBookingViewModel>(_sql);
		}

		#endregion report

		#region Production Booking

		public IEnumerable<object> GetProductionBookingListByDate(string date)
		{
			try
			{
				var _sql = @"SELECT A.Id, A.CompanyGroupId, A.CompanyId, A.PlantName, A.ProductionDate, A.Line, A.ProductionShift, A.SalesOrder, A.Fabrication
							, A.Style, A.ProductionQty, A.CustomerCode, A.CustomerName, A.TotalManPower, A.PlanRunMC, A.ActualRunMC, A.ExtraMC
							, A.TrimCheckPress, A.SewingSMV, A.TotalSMV, A.MCMINAvailable, A.NonMCMINAvailable, A.TotalMINAvailable, A.ActualMINWorked
							, A.MCSAMProd, A.TotalSAMProd, A.MCEfficiency, A.OrderQty, A.TargetQuantity, A.MaterialCode, A.MaterialDesc, A.NoApplicablePcsRate
							, TotalMachineOperation=(SELECT COUNT(*) FROM [MST].[LineOperationBooking] WHERE LineProductionBookingId=A.Id AND OperationType='MACHINE')
							, TotalMachine=(SELECT COUNT(E.Id) FROM [MST].[LineEmployeeAssign] AS E JOIN [MST].[LineOperationBooking] AS B 
									ON E.LineOperationBookingId=B.Id WHERE B.LineProductionBookingId=A.Id)
							, TotalEmpAttach=(SELECT COUNT(E.Id) FROM [MST].[LineEmployeeAssign] AS E JOIN [MST].[LineOperationBooking] AS B 
									ON E.LineOperationBookingId=B.Id WHERE B.LineProductionBookingId=A.Id AND E.EmployeeId<>'')
							, TotalPendingMachine=(SELECT COUNT(E.Id) FROM [MST].[LineEmployeeAssign] AS E JOIN [MST].[LineOperationBooking] AS B 
									ON E.LineOperationBookingId=B.Id WHERE B.LineProductionBookingId=A.Id AND E.EmployeeId IS NULL)
						FROM [MST].[LineProductionBooking] AS A
                        WHERE A.ProductionDate=CAST('" + date + "' AS DATE) ORDER BY A.Line, A.ProductionShift, A.SalesOrder";
				return _sqlRepository.GetDataCollection(_sql, null);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public IEnumerable<object> GetForEditPrdBooking(string date, string salesOrderName, string line, string shift)
		{
			var _sql = @"SELECT LE.Id, LE.LineOperationBookingId, B.LineProductionBookingId, LE.EmployeeId, LE.OperatorQty, E.EmployeeName, E.EmployeeCode
	                        , B.OperationName, A.Fabrication, A.Style, A.SalesOrder,A.Line, A.ProductionShift, A.ProductionQty
                        FROM [MST].[LineEmployeeAssign] LE
                        JOIN [MST].[LineOperationBooking] AS B ON LE.LineOperationBookingId=B.Id
                        JOIN [MST].[LineProductionBooking] AS A ON B.LineProductionBookingId=A.Id
                        LEFT JOIN EmployeeInformation AS E ON LE.EmployeeId=E.SystemId
                        WHERE LE.LineOperationBookingId IN(
		                        SELECT A.LineOperationBookingId
		                        FROM [MST].[LineEmployeeAssign] AS A
		                        JOIN [MST].[LineOperationBooking] AS B ON A.LineOperationBookingId=B.Id
		                        JOIN [MST].[LineProductionBooking] AS C ON B.LineProductionBookingId=C.Id
		                        WHERE C.ProductionDate=CAST('" + date + @"' AS DATE) AND B.MachineType<>'MANUAL' 
                                AND C.SalesOrder='" + salesOrderName + "' AND C.Line='" + line + "' AND C.ProductionShift='" + shift + @"' 
                                GROUP BY A.LineOperationBookingId  HAVING COUNT(B.OperationName)>1)";
			return _sqlRepository.GetDataCollection(_sql);
		}

		public void UpdateGraphLineProduction(string id, decimal prdQty, IEnumerable<LineProductionOperationBookingViewModel> entities)
		{
			var flag = false;
			try
			{
				if (entities == null)
					throw new CustomException("No data selected.");
				_unitOfWork.BeginTransaction();
				flag = true;

				var opIds = new string[] { };
				if (entities.Count() > 0 && entities.IsNotNull())
				{
					opIds = entities.Select(t => t.LineOperationBookingId).Distinct().ToArray();
					CheckTotalOperatorQty(prdQty, entities, opIds);
					var changeEmpData = base.Query(t => opIds.Contains(t.LineOperationBookingId)).Select().ToList();
					foreach (var item in entities)
					{
						var lineEmp = changeEmpData.FirstOrDefault(t => t.Id == item.Id);
						if (lineEmp.IsNotNull())
						{
							lineEmp.OperatorQty = item.OperatorQty;
							base.UpdateGraph(lineEmp);
						}
					}
				}

				var sql = @"SELECT LEA.* FROM [MST].[LineEmployeeAssign] AS LEA
                            JOIN [MST].[LineOperationBooking] AS LO ON LEA.LineOperationBookingId=LO.Id
                            JOIN [MST].[LineProductionBooking] AS LP ON LO.LineProductionBookingId=LP.Id
                            WHERE LP.Id='" + id + "' AND LEA.LineOperationBookingId NOT IN(" + ReturnStringArray(opIds) + ")";
				var empData = _sqlRepository.GetModelCollection<LineEmployeeAssign>(sql);

				foreach (var emp in empData)
				{
					emp.OperatorQty = Convert.ToInt32(prdQty);
					base.UpdateGraph(emp);
				}

				var lpData = _lineProductionBookingRepository.Find(id);
				lpData.ProductionQty = prdQty;
				_lineProductionBookingRepository.Update(lpData);

				_unitOfWork.SaveChanges();
				flag = false;
				_unitOfWork.Commit();
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
				null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
			}
			finally
			{
				if (flag)
					_unitOfWork.Rollback();
			}
		}

		private static void CheckTotalOperatorQty(decimal prdQty, IEnumerable<LineProductionOperationBookingViewModel> entities, string[] opIds)
		{
			foreach (var opId in opIds)
			{
				int totalPrdQty = entities.Where(t => t.LineOperationBookingId == opId).Select(t => t.OperatorQty).Sum();
				if (totalPrdQty != Convert.ToInt16(prdQty))
					throw new Exception(entities.FirstOrDefault(t => t.LineOperationBookingId == opId).OperationName + " qty does not match the production qty");
			}
		}

		public void UpdateNoApplicablePcsRate(string id)
		{
			try
			{
				var data = _lineProductionBookingRepository.Find(id);
				if (data.IsNotNull())
				{
					data.NoApplicablePcsRate = !data.NoApplicablePcsRate;
					_lineProductionBookingRepository.Update(data);
					_unitOfWork.SaveChanges();
				}
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
				null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
			}
		}
		#endregion Production Booking
	}
}