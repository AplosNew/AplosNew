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

using Library.Service.Enums;
using Library.Service.Logs;
using Library.Model.Products;
using Library.Core;
using Library.Model.Accounts;
using Library.Service.Core;
using Syncfusion.DocIO.DLS;
using Library.Service.Helpers;
using System.IO;
using Syncfusion.DocIO;
using System.Collections.Specialized;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using System.Text.RegularExpressions;

#endregion Using

namespace Library.MaterialManagement.InventoryManagements
{
	public class GatePassService
	{
		private readonly SqlRepository _sqlRepository;

		#region Constructor
		public GatePassService()
		{
			_sqlRepository = new SqlRepository();
		}
		#endregion Constructor


		public IEnumerable<object> getGatePassRegister()
        {
            try
            {
				var sql = @"select gpd.Id as GatePassDetailId , gpd.GatePassMasterId , gpd.MaterialMasterId ,mm.UserName as Material , gpd.ArticleId, mma.StandardName as Article,
							gpd.FirstCharacteristicsId, cf.UserName as FirstCharacteristics, gpd.FirstCharacteristicsValueId, cvf.UserName as FirstCharacteristicsValue,
							gpd.SecondCharacteristicsId, cs.UserName as SecondCharacteristics, gpd.SecondCharacteristicsValueId, cvs.UserName as SecondCharacteristicsValue,
							gpd.ThirdCharacteristicsId, ct.UserName as ThirdCharacteristics, gpd.ThirdCharacteristicsValueId, cvt.UserName as ThirdCharacteristicsValue,
							gpd.MaterialDetail ,gpd.TransactionQty , gpd.TransactionUoMId , uom.Username as UOM, gpd.Remarks , gpd.IsReturnable , 
							format(gpd.ReturnableDate,'dd-MMM-yyyy') as GPDReturnableDate,gpd.IsMutilated , gpd.Rate, gpd.ChallanNo , gpd.ChallanNoDetailId , gpd.PurposeofGatePass ,
							gpd.ConsignmentNo , gpd.DriverName as GPDDriverName,
							--- Master Table
							gpm.CompanyGroupId, cg.UserName as CompanyGroup,gpm.PlantId , p.UserName as Plant, gpm.GatePassType , gpm.GatePassStatus,
							format(gpm.ReturnableDate,'dd-MMM-yyyy') as GPMReturnableDate,format(gpm.GatePassEntryDate,'dd-MMM-yyyy') as GatePassEntryDate , gpm.FromEmployeeId ,
							fromemp.EmployeeName as FromEmployee, gpm.Through, gpm.CourierName,gpm.RunnerEmployeeId, gpm.ToType, gpm.ToPartyCode , 
							pty.Username as ToParty, gpm.ToBuyerId, b.UserName as ToBuyer,gpm.ToPlantId, pl.Username as ToPlant, gpm.ToUnitId , u.UserName as ToUnit,
							gpm.ToDivisionId , div.Username as ToDivision,gpm.ToDepartment as ToDepartmentId, dept.UserName as ToDepartment,
							gpm.DepartmentEmployeeId, DeptEmp.EmployeeName as DepartmentEmployee, gpm.OtherCompanyName,gpm.PersonName,gpm.MobileNo,gpm.Address,gpm.Remarks as GPMRemarks,
							gpm.CheckedBy, CheckEmp.EmployeeName as CheckedByEmployee, gpm.CheckedByStatus , gpm.CheckedHoldRejectReason, gpm.ApprovedBy , ApprEmp.EmployeeName as ApprovedByEmployee,
							gpm.ApprovedHoldRejectReason, gpm.SenderSecurityEmployeeId , SenderSecEmp.EmployeeName as SenderSecurityEmployee , gpm.SenderSecurityApprovedStatus,
							gpm.ReceiverSecurityEmployeeId , ReceiverSecEmp.EmployeeName as ReceiverSecurityEmployee , gpm.ReceiverSecurityApprovedStatus , gpm.VendorBuyerOtherCompanyReceivedStatus,
							gpm.ChallanNo as GPMChallanNo , gpm.TransportAgentMobileNo, gpm.TransportAgentName , gpm.VehicleNo, gpm.GateOutStatus, gpm.GateRegisterType, gpm.ReceivedChallanNO,
							gpm.InvoiceNo, gpm.PurposeofGatePass as GPMPurposeOfGatePass, gpm.ConsignmentNo as GPMConsignmentNo, gpm.DriverName, gpm.NoOfPackages
							from trn.GatePassDetails gpd
							left join mst.MaterialMaster mm on mm.Id = gpd.MaterialMasterId
							left join mst.MaterialMasterArticle mma on mma.Id = gpd.ArticleId
							left join hkp.Characteristics cf on cf.Id = gpd.FirstCharacteristicsId
							left join hkp.CharacteristicsValue cvf on cvf.Id = gpd.FirstCharacteristicsValueId
							left join hkp.Characteristics cs on cs.Id = gpd.SecondCharacteristicsId
							left join hkp.CharacteristicsValue cvs on cvs.Id = gpd.SecondCharacteristicsValueId
							left join hkp.Characteristics ct on ct.Id = gpd.ThirdCharacteristicsId
							left join hkp.CharacteristicsValue cvt on cvt.Id = gpd.ThirdCharacteristicsValueId
							left join scs.UnitOfMeasurement uom on uom.Id = gpd.TransactionUoMId
							left join trn.GatePassMaster gpm on gpm.Id = gpd.GatePassMasterId
							left join org.CompanyGroup cg on cg.Id = gpm.CompanyGroupId
							left join org.Plant p on p.Id = gpm.PlantId
							left join dbo.EmployeeInformation FromEmp on fromemp.SystemId = gpm.FromEmployeeId
							left join dbo.EmployeeInformation DeptEmp on DeptEmp.SystemId = gpm.DepartmentEmployeeId
							left join dbo.EmployeeInformation CheckEmp on CheckEmp.SystemId = gpm.CheckedBy
							left join dbo.EmployeeInformation ApprEmp on ApprEmp.SystemId = gpm.ApprovedBy
							left join dbo.EmployeeInformation SenderSecEmp on SenderSecEmp.SystemId = gpm.SenderSecurityEmployeeId
							left join dbo.EmployeeInformation ReceiverSecEmp on ReceiverSecEmp.SystemId = gpm.ReceiverSecurityEmployeeId
							left join hkp.Party pty on pty.Id = gpm.ToPartyCode
							left join hkp.Buyer b on b.Id = gpm.ToBuyerId
							left join org.Plant pl on pl.Id = gpm.ToPlantId
							left join org.Unit u on u.Id = gpm.ToUnitId
							left join org.Division div on div.Id= gpm.ToDivisionId
							left join org.Department dept on dept.Id = gpm.ToDepartment";

				return _sqlRepository.GetDataCollection(sql);
            }
			catch(Exception e)
            {
				throw e;
            }
        }

		public DataTable getGatePassRegisterReport()
		{
			try
			{
				var sql = @"select gpd.Id as GatePassDetailId , gpd.GatePassMasterId , gpd.MaterialMasterId ,mm.UserName as Material , gpd.ArticleId, mma.StandardName as Article,
							gpd.FirstCharacteristicsId, cf.UserName as FirstCharacteristics, gpd.FirstCharacteristicsValueId, cvf.UserName as FirstCharacteristicsValue,
							gpd.SecondCharacteristicsId, cs.UserName as SecondCharacteristics, gpd.SecondCharacteristicsValueId, cvs.UserName as SecondCharacteristicsValue,
							gpd.ThirdCharacteristicsId, ct.UserName as ThirdCharacteristics, gpd.ThirdCharacteristicsValueId, cvt.UserName as ThirdCharacteristicsValue,
							gpd.MaterialDetail ,gpd.TransactionQty , gpd.TransactionUoMId , uom.Username as UOM, gpd.Remarks , gpd.IsReturnable , 
							format(gpd.ReturnableDate,'dd-MMM-yyyy') as GPDReturnableDate,gpd.IsMutilated , gpd.Rate, gpd.ChallanNo , gpd.ChallanNoDetailId , gpd.PurposeofGatePass ,
							gpd.ConsignmentNo , gpd.DriverName as GPDDriverName,
							--- Master Table
							gpm.CompanyGroupId, cg.UserName as CompanyGroup,gpm.PlantId , p.UserName as Plant, gpm.GatePassType , gpm.GatePassStatus,
							format(gpm.ReturnableDate,'dd-MMM-yyyy') as GPMReturnableDate,format(gpm.GatePassEntryDate,'dd-MMM-yyyy') as GatePassEntryDate , gpm.FromEmployeeId ,
							fromemp.EmployeeName as FromEmployee, gpm.Through, gpm.CourierName,gpm.RunnerEmployeeId, gpm.ToType, gpm.ToPartyCode , 
							pty.Username as ToParty, gpm.ToBuyerId, b.UserName as ToBuyer,gpm.ToPlantId, pl.Username as ToPlant, gpm.ToUnitId , u.UserName as ToUnit,
							gpm.ToDivisionId , div.Username as ToDivision,gpm.ToDepartment as ToDepartmentId, dept.UserName as ToDepartment,
							gpm.DepartmentEmployeeId, DeptEmp.EmployeeName as DepartmentEmployee, gpm.OtherCompanyName,gpm.PersonName,gpm.MobileNo,gpm.Address,gpm.Remarks as GPMRemarks,
							gpm.CheckedBy, CheckEmp.EmployeeName as CheckedByEmployee, gpm.CheckedByStatus , gpm.CheckedHoldRejectReason, gpm.ApprovedBy , ApprEmp.EmployeeName as ApprovedByEmployee,
							gpm.ApprovedHoldRejectReason, gpm.SenderSecurityEmployeeId , SenderSecEmp.EmployeeName as SenderSecurityEmployee , gpm.SenderSecurityApprovedStatus,
							gpm.ReceiverSecurityEmployeeId , ReceiverSecEmp.EmployeeName as ReceiverSecurityEmployee , gpm.ReceiverSecurityApprovedStatus , gpm.VendorBuyerOtherCompanyReceivedStatus,
							gpm.ChallanNo as GPMChallanNo , gpm.TransportAgentMobileNo, gpm.TransportAgentName , gpm.VehicleNo, gpm.GateOutStatus, gpm.GateRegisterType, gpm.ReceivedChallanNO,
							gpm.InvoiceNo, gpm.PurposeofGatePass as GPMPurposeOfGatePass, gpm.ConsignmentNo as GPMConsignmentNo, gpm.DriverName, gpm.NoOfPackages
							from trn.GatePassDetails gpd
							left join mst.MaterialMaster mm on mm.Id = gpd.MaterialMasterId
							left join mst.MaterialMasterArticle mma on mma.Id = gpd.ArticleId
							left join hkp.Characteristics cf on cf.Id = gpd.FirstCharacteristicsId
							left join hkp.CharacteristicsValue cvf on cvf.Id = gpd.FirstCharacteristicsValueId
							left join hkp.Characteristics cs on cs.Id = gpd.SecondCharacteristicsId
							left join hkp.CharacteristicsValue cvs on cvs.Id = gpd.SecondCharacteristicsValueId
							left join hkp.Characteristics ct on ct.Id = gpd.ThirdCharacteristicsId
							left join hkp.CharacteristicsValue cvt on cvt.Id = gpd.ThirdCharacteristicsValueId
							left join scs.UnitOfMeasurement uom on uom.Id = gpd.TransactionUoMId
							left join trn.GatePassMaster gpm on gpm.Id = gpd.GatePassMasterId
							left join org.CompanyGroup cg on cg.Id = gpm.CompanyGroupId
							left join org.Plant p on p.Id = gpm.PlantId
							left join dbo.EmployeeInformation FromEmp on fromemp.SystemId = gpm.FromEmployeeId
							left join dbo.EmployeeInformation DeptEmp on DeptEmp.SystemId = gpm.DepartmentEmployeeId
							left join dbo.EmployeeInformation CheckEmp on CheckEmp.SystemId = gpm.CheckedBy
							left join dbo.EmployeeInformation ApprEmp on ApprEmp.SystemId = gpm.ApprovedBy
							left join dbo.EmployeeInformation SenderSecEmp on SenderSecEmp.SystemId = gpm.SenderSecurityEmployeeId
							left join dbo.EmployeeInformation ReceiverSecEmp on ReceiverSecEmp.SystemId = gpm.ReceiverSecurityEmployeeId
							left join hkp.Party pty on pty.Id = gpm.ToPartyCode
							left join hkp.Buyer b on b.Id = gpm.ToBuyerId
							left join org.Plant pl on pl.Id = gpm.ToPlantId
							left join org.Unit u on u.Id = gpm.ToUnitId
							left join org.Division div on div.Id= gpm.ToDivisionId
							left join org.Department dept on dept.Id = gpm.ToDepartment";

				return _sqlRepository.GetDataTable(sql);
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		
	}
}
